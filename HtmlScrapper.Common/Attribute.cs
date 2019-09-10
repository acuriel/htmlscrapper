using System;
using System.Collections.Generic;
using System.Text;

namespace HtmlScrapper.Common
{
    /// <summary>
    /// A tag's attribute
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// Build an attribute with the specific name and value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public Attribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Attribute's name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Attribute's value
        /// </summary>
        public string Value { get; set; }

        public override string ToString() => $"{Name}=\"{Value}\"";

        
    }

}
