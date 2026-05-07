using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPlugin
{
    public class SentryConfig
    {
        public bool Enable { get; set; }
        public string Dsn { get; set; }
        public float SampleRate { get; set; }
        public string Environment { get; set; }
        public string Source { get; set; }

        //"enable": false, // enable sentry
        //"dsn": "https://8f27b44f7ac74050bb392da4325f734c@sentry.bigin.top/11",
        //"sampleRate": 0.2,
        //"environment": "dev",
        //"source": "notification-service"
    }
}
