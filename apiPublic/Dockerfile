
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


# Install native dependencies required by PDFtoImage (PDFium) and SkiaSharp
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    libc6-dev \
    libfontconfig1 \
    libfreetype6 \
    libx11-6 \
    libxext6 \
    libxrender1 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN cp apiPublic/nuget.config ./nuget.config
RUN dotnet restore "apiPublic/apiPublic.csproj"
WORKDIR "/src/apiPublic"
RUN dotnet build "apiPublic.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "apiPublic.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "apiPublic.dll"]