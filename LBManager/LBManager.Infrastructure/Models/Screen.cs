using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    public class Screen
    {
        public Screen()
        {

        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("size_x")]
        public int Width { get; set; }

        [JsonProperty("size_y")]
        public int Height { get; set; }

        [JsonProperty("resolu_x")]
        public int PixelsWidth { get; set; }

        [JsonProperty("resolu_y")]
        public int PixelsHeight { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("operate")]
        public string Operate { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
