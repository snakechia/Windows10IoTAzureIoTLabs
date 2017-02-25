using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseHAT
{
    public class SensorData
    {
        public string deviceId { get; set; }
        public double temperature { get; set; }
        public double pressure { get; set; }
        public double humidity { get; set; }
        public DateTime createdAt { get; set; }
    }

    
}
