using System;
using System.Collections.Generic;
using System.Text;

namespace HtmlScrapper.Common
{
    public class Attribute
    {
        public Attribute(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString() => $"{Name}=\"{Value}\"";

        
    }

}
