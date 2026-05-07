using Microsoft.AspNetCore.SignalR;

namespace B2BAdmin.realtime.Domains.Hubs
{
    public class HubRetrieveTour : Hub
    {
    }
    
    public class RetrieveProgress
    {
        public string BookingId { get; set; } = "";
        
        public string? ServiceName { get; set; }
        
        public bool IsCompleted { get; set; }
    }
}

