using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIHOMEASSIST.Application.Communication
{
    public class MqttOptions
    {
        public int CommunicationTimeout { get; set; }
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public int KeepAliveInterval { get; set; }
    }
}
