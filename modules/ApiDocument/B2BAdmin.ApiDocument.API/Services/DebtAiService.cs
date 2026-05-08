using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace B2BAdmin.ApiDocument.API.Services
{
    public interface IDebtAiService
    {
        Task<DebtAiResult> AnalyzeAsync(DebtAiQueryRequest request, CancellationToken cancellationToken = default);
    }

    public class DebtAiService : IDebtAiService
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        private readonly IConfiguration _configuration;

        public DebtAiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<DebtAiResult> AnalyzeAsync(DebtAiQueryRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                throw new ArgumentException("Prompt is required.");
            }

            var apiKey = (_configuration["Gemini:ApiKey"] ?? _configuration["Gemini__ApiKey"] ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Gemini ApiKey is not configured.");
            }

            var endpointBase = (_configuration["Gemini:ApiEndpoint"] ?? _configuration["Gemini__ApiEndpoint"] ?? "https://generativelanguage.googleapis.com/v1beta/models").Trim().TrimEnd('/');
            var modelName = (_configuration["Gemini:ModelName"] ?? _configuration["Gemini__ModelName"] ?? "gemini-2.0-flash").Trim();
            var systemPrompt = (_configuration["Gemini:SystemPrompt"] ?? _configuration["Gemini__SystemPrompt"]
                ?? "You are a debt analysis assistant for an accounting portal. Answer in Vietnamese by default unless the user clearly asks for English. Use only the provided debt data. If the data is insufficient, say exactly what is missing. Keep the answer practical, precise, and grounded in the numbers.").Trim();
            var timeoutSeconds = ReadPositiveInt(_configuration["Gemini:TimeoutSeconds"] ?? _configuration["Gemini__TimeoutSeconds"], 45);

            var contextJson = JsonSerializer.Serialize(TrimContext(request.Context), JsonOptions);
            var historyText = BuildHistoryText(request.History);
            var combinedPrompt = new StringBuilder()
                .AppendLine(systemPrompt)
                .AppendLine()
                .AppendLine("Conversation history:")
                .AppendLine(historyText)
                .AppendLine()
                .AppendLine("Current user request:")
                .AppendLine(request.Prompt.Trim())
                .AppendLine()
                .AppendLine("Available debt context JSON:")
                .AppendLine(contextJson)
                .AppendLine()
                .AppendLine("Return JSON only. No markdown fences, no commentary outside JSON.")
                .AppendLine("Required JSON schema:")
                .AppendLine("{")
                .AppendLine("  \"summary\": \"short executive summary\",")
                .AppendLine("  \"findings\": [\"fact 1\", \"fact 2\"],")
                .AppendLine("  \"recommendations\": [\"action 1\", \"action 2\"],")
                .AppendLine("  \"relatedCustomers\": [\"331-VP - CONG TY ...\"],")
                .AppendLine("  \"scopeNotes\": [\"analysis scope note\"],")
                .AppendLine("  \"suggestedQuestions\": [\"follow-up 1\", \"follow-up 2\"],")
                .AppendLine("  \"answer\": \"full human-readable answer in Vietnamese\"")
                .AppendLine("}")
                .AppendLine()
                .AppendLine("Rules:")
                .AppendLine("1. Do not invent customers, amounts, or transactions.")
                .AppendLine("2. When possible, cite customer code/name and amount from the provided context.")
                .AppendLine("3. If a filter limits the scope, mention that clearly in scopeNotes.")
                .AppendLine("4. findings and recommendations should be concise bullet-ready strings.")
                .ToString();

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = combinedPrompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.15,
                    topP = 0.8,
                    maxOutputTokens = 1400,
                    responseMimeType = "application/json",
                }
            };

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            using var client = new HttpClient();
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpointBase}/{modelName}:generateContent?key={Uri.EscapeDataString(apiKey)}")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
            };

            using var response = await client.SendAsync(httpRequest, timeoutCts.Token);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Gemini request failed: {(int)response.StatusCode} {response.ReasonPhrase}. {responseBody}");
            }

            using var document = JsonDocument.Parse(responseBody);
            var answer = ExtractAnswer(document.RootElement);
            if (string.IsNullOrWhiteSpace(answer))
            {
                throw new InvalidOperationException("Gemini returned an empty answer.");
            }

            var result = ParseStructuredResult(answer.Trim());
            result.Provider = "gemini";
            result.Model = modelName;
            result.GeneratedAt = DateTime.UtcNow;
            return result;
        }

        private static int ReadPositiveInt(string raw, int fallback)
        {
            return int.TryParse(raw, out var value) && value > 0 ? value : fallback;
        }

        private static string BuildHistoryText(IList<DebtAiHistoryMessage> history)
        {
            if (history == null || history.Count == 0)
            {
                return "(no previous conversation)";
            }

            var lines = history
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Role) && !string.IsNullOrWhiteSpace(x.Content))
                .TakeLast(8)
                .Select(x => $"{x.Role.Trim()}: {x.Content.Trim()}");

            var text = string.Join(Environment.NewLine, lines);
            return string.IsNullOrWhiteSpace(text) ? "(no previous conversation)" : text;
        }

        private static DebtAiContext TrimContext(DebtAiContext context)
        {
            context ??= new DebtAiContext();
            context.DebtItems = (context.DebtItems ?? new List<DebtAiDebtItem>()).Take(20).ToList();
            context.Transactions = (context.Transactions ?? new List<DebtAiTransactionItem>()).Take(50).ToList();
            context.SelectedTransactions = (context.SelectedTransactions ?? new List<DebtAiTransactionItem>()).Take(50).ToList();
            return context;
        }

        private static string ExtractAnswer(JsonElement root)
        {
            if (!root.TryGetProperty("candidates", out var candidates) || candidates.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            foreach (var candidate in candidates.EnumerateArray())
            {
                if (!candidate.TryGetProperty("content", out var content)
                    || !content.TryGetProperty("parts", out var parts)
                    || parts.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var texts = parts.EnumerateArray()
                    .Where(x => x.TryGetProperty("text", out _))
                    .Select(x => x.GetProperty("text").GetString())
                    .Where(x => !string.IsNullOrWhiteSpace(x));

                var merged = string.Join(Environment.NewLine, texts);
                if (!string.IsNullOrWhiteSpace(merged))
                {
                    return merged;
                }
            }

            return string.Empty;
        }

        private static DebtAiResult ParseStructuredResult(string rawAnswer)
        {
            var jsonText = ExtractJsonObject(rawAnswer);
            if (!string.IsNullOrWhiteSpace(jsonText))
            {
                try
                {
                    using var document = JsonDocument.Parse(jsonText);
                    var root = document.RootElement;
                    var summary = GetString(root, "summary");
                    var findings = GetStringList(root, "findings");
                    var recommendations = GetStringList(root, "recommendations");
                    var relatedCustomers = GetStringList(root, "relatedCustomers");
                    var scopeNotes = GetStringList(root, "scopeNotes");
                    var suggestedQuestions = GetStringList(root, "suggestedQuestions");
                    var answer = GetString(root, "answer");

                    return new DebtAiResult
                    {
                        Summary = string.IsNullOrWhiteSpace(summary) ? FirstNonEmpty(findings.FirstOrDefault(), rawAnswer) : summary,
                        Findings = findings,
                        Recommendations = recommendations,
                        RelatedCustomers = relatedCustomers,
                        ScopeNotes = scopeNotes,
                        SuggestedQuestions = suggestedQuestions,
                        Answer = BuildAnswer(answer, summary, findings, recommendations, scopeNotes, rawAnswer),
                        RawText = rawAnswer,
                    };
                }
                catch
                {
                }
            }

            return new DebtAiResult
            {
                Summary = TakeFirstLine(rawAnswer),
                Findings = new List<string>(),
                Recommendations = new List<string>(),
                RelatedCustomers = new List<string>(),
                ScopeNotes = new List<string>(),
                SuggestedQuestions = new List<string>(),
                Answer = rawAnswer,
                RawText = rawAnswer,
            };
        }

        private static string ExtractJsonObject(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var start = text.IndexOf('{');
            if (start < 0)
            {
                return string.Empty;
            }

            var depth = 0;
            var inString = false;
            var escaped = false;
            for (var index = start; index < text.Length; index++)
            {
                var ch = text[index];
                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                    }
                    else if (ch == '\\')
                    {
                        escaped = true;
                    }
                    else if (ch == '"')
                    {
                        inString = false;
                    }
                    continue;
                }

                if (ch == '"')
                {
                    inString = true;
                    continue;
                }

                if (ch == '{')
                {
                    depth++;
                }
                else if (ch == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return text.Substring(start, index - start + 1);
                    }
                }
            }

            return string.Empty;
        }

        private static string GetString(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
                ? (value.GetString() ?? string.Empty).Trim()
                : string.Empty;
        }

        private static List<string> GetStringList(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
            {
                return new List<string>();
            }

            return value.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.String)
                .Select(x => (x.GetString() ?? string.Empty).Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string BuildAnswer(
            string answer,
            string summary,
            List<string> findings,
            List<string> recommendations,
            List<string> scopeNotes,
            string fallback)
        {
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer.Trim();
            }

            var builder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(summary))
            {
                builder.AppendLine(summary.Trim());
            }

            if (findings.Count > 0)
            {
                builder.AppendLine().AppendLine("Chi tiet:");
                foreach (var item in findings)
                {
                    builder.AppendLine($"- {item}");
                }
            }

            if (recommendations.Count > 0)
            {
                builder.AppendLine().AppendLine("De xuat:");
                foreach (var item in recommendations)
                {
                    builder.AppendLine($"- {item}");
                }
            }

            if (scopeNotes.Count > 0)
            {
                builder.AppendLine().AppendLine("Pham vi:");
                foreach (var item in scopeNotes)
                {
                    builder.AppendLine($"- {item}");
                }
            }

            var built = builder.ToString().Trim();
            return string.IsNullOrWhiteSpace(built) ? fallback : built;
        }

        private static string TakeFirstLine(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? text;
        }

        private static string FirstNonEmpty(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first) ? first : second;
        }
    }

    public class DebtAiResult
    {
        public string Provider { get; set; }
        public string Model { get; set; }
        public string Summary { get; set; }
        public List<string> Findings { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
        public List<string> RelatedCustomers { get; set; } = new List<string>();
        public List<string> ScopeNotes { get; set; } = new List<string>();
        public List<string> SuggestedQuestions { get; set; } = new List<string>();
        public string Answer { get; set; }
        public string RawText { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class DebtAiQueryRequest
    {
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }

        [JsonPropertyName("context")]
        public DebtAiContext Context { get; set; }

        [JsonPropertyName("history")]
        public IList<DebtAiHistoryMessage> History { get; set; } = new List<DebtAiHistoryMessage>();
    }

    public class DebtAiHistoryMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class DebtAiContext
    {
        [JsonPropertyName("activeTab")]
        public string ActiveTab { get; set; }

        [JsonPropertyName("overview")]
        public DebtAiOverview Overview { get; set; }

        [JsonPropertyName("filters")]
        public DebtAiFilterSummary Filters { get; set; }

        [JsonPropertyName("debtItems")]
        public List<DebtAiDebtItem> DebtItems { get; set; }

        [JsonPropertyName("transactions")]
        public List<DebtAiTransactionItem> Transactions { get; set; }

        [JsonPropertyName("selectedTransactions")]
        public List<DebtAiTransactionItem> SelectedTransactions { get; set; }
    }

    public class DebtAiOverview
    {
        [JsonPropertyName("totalReceivable")]
        public decimal TotalReceivable { get; set; }

        [JsonPropertyName("totalPayable")]
        public decimal TotalPayable { get; set; }

        [JsonPropertyName("netExposure")]
        public decimal NetExposure { get; set; }

        [JsonPropertyName("highRiskExposure")]
        public decimal HighRiskExposure { get; set; }

        [JsonPropertyName("customerCount")]
        public int CustomerCount { get; set; }
    }

    public class DebtAiFilterSummary
    {
        [JsonPropertyName("overviewSearch")]
        public string OverviewSearch { get; set; }

        [JsonPropertyName("overviewStatus")]
        public string OverviewStatus { get; set; }

        [JsonPropertyName("overviewRiskLevel")]
        public string OverviewRiskLevel { get; set; }

        [JsonPropertyName("overviewBalanceType")]
        public string OverviewBalanceType { get; set; }

        [JsonPropertyName("transactionSearch")]
        public string TransactionSearch { get; set; }

        [JsonPropertyName("transactionCustomerId")]
        public string TransactionCustomerId { get; set; }

        [JsonPropertyName("transactionType")]
        public string TransactionType { get; set; }

        [JsonPropertyName("cartLockedCustomerId")]
        public string CartLockedCustomerId { get; set; }
    }

    public class DebtAiDebtItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("riskLevel")]
        public string RiskLevel { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("debtAmount")]
        public decimal DebtAmount { get; set; }

        [JsonPropertyName("creditAmount")]
        public decimal CreditAmount { get; set; }

        [JsonPropertyName("netBalance")]
        public decimal NetBalance { get; set; }

        [JsonPropertyName("agingDays")]
        public int AgingDays { get; set; }

        [JsonPropertyName("agingBucket")]
        public string AgingBucket { get; set; }

        [JsonPropertyName("lastTransactionAt")]
        public string LastTransactionAt { get; set; }
    }

    public class DebtAiTransactionItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("customerId")]
        public string CustomerId { get; set; }

        [JsonPropertyName("customerCode")]
        public string CustomerCode { get; set; }

        [JsonPropertyName("customerName")]
        public string CustomerName { get; set; }

        [JsonPropertyName("transactionType")]
        public string TransactionType { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("transactionAt")]
        public string TransactionAt { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; }

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }
    }
}