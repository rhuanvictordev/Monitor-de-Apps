using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MonitorDeApps.Model
{
    public class Programa
    {
        public string Name { get; set; }
        public string ExePath { get; set; }
        public string UpdaterExePath { get; set; }
        public int CheckIntervalSeconds { get; set; }
        public string ApplicationType { get; set; }
        public int MaxProcessQuantity { get; set; }

        [JsonIgnore]
        public DateTime VerifiedAt { get; set; }

        public Programa()
        {
            
        }

        public Programa(string name, string exePath, string updaterExePath, int checkInterval, string applicationType, int maxProcessQuantity)
        {
            this.Name = name;
            this.ExePath = exePath;
            this.UpdaterExePath = updaterExePath;
            this.CheckIntervalSeconds = checkInterval;
            ApplicationType = applicationType;
            MaxProcessQuantity = maxProcessQuantity;
        }

    }
}
