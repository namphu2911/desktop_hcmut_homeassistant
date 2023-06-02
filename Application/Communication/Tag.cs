using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIHOMEASSIST.Application.Communication
{
    public class Tag
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public Tag(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
