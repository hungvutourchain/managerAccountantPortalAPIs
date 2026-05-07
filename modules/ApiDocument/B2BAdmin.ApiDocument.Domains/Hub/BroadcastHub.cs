using System;
using System.Threading.Tasks;

namespace B2BAdmin.realtime.Domains.Hubs
{
    public interface IHubClient
    {
        Task BroadcastMessage(string IdUserTo, string IdUserSend);
    }
}

