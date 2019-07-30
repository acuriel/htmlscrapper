using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HtmlScrapper.Common
{
    public class TagNode
    {
        private string fullText;
        private string text;
        public TagNode(string name, TagNode parent)
        {
            Name = name.ToLower();
            Attributes = new List<Attribute>();
            Children = new List<TagNode>();
            Parent = parent;
        }

        public string Name { get; set; }
        public virtual List<Attribute> Attributes { get; private set; }
        public virtual List<TagNode> Children { get; private set; }
        public TagNode Parent { get; set; }
        public IEnumerable<TagNode> Siblings 
            => Parent != null ? 
            Parent.Children.Where((t) => t != this) : 
            new List<TagNode>();
        public virtual string Text
        {
            get
            {
                if (text == null)
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (var item in Children.Where(t => t.Name == "text"))
                        builder.AppendLine(item.Text);
                    text = builder.ToString();
                }
                return text;
            }
            protected set {
                text = value;
            }
        }

        public string GetFullText()
        {
            if (fullText == null)
            {
                StringBuilder textBuilder = new StringBuilder(Text);
                Children.ForEach(t => textBuilder.AppendLine(t.Text));
                fullText = textBuilder.ToString();
            }
            return fullText;
        }

        public override string ToString() => Name;
    }

    public class TextNode : TagNode
    {
        public TextNode(TagNode parent, string text) : base("text", parent)
        {
            Text = text;
        }
        public override string Text => base.Text;
    }
}
