using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class Site
    {
        public Site()
        {
            Attachments = new List<string>();
        }

        [JsonIgnore] public string Id { get; set; }

        [JsonIgnore] public bool DeleteAwaiting { get; set; }

        public string Name { get; set; }
        public List<string> Attachments { get; set; }
    }
}