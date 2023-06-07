using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAssistantGUI.Application.Communication
{
    public class CommandMessage
    {
        public string Name { get; set; }
        public object? Value { get; set; }
        public CommandMessage(string name, object? value)
        {
            Name = name;
            Value = value;
        }
    }
}
