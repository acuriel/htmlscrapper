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
            Attributes = new Dictionary<string, string>();
            Children = new List<TagNode>();
            Parent = parent;
        }

        public string Name { get; set; }
        public virtual Dictionary<string,string> Attributes { get; private set; }
        public virtual List<TagNode> Children { get; private set; }
        public TagNode Parent { get; set; }
        public IEnumerable<TagNode> Siblings 
            => Parent != null ? 
            Parent.Children.Where((t) => t != this) : 
            new List<TagNode>();

        public TagNode NextSibling => NextFullSiblings.First();
        public TagNode PrevSibling => PrevFullSiblings.First();
        public IEnumerable<TagNode> NextFullSiblings => Siblings.SkipWhile(t => !t.Equals(this)).Skip(1);
        public IEnumerable<TagNode> PrevFullSiblings => Siblings.Reverse().SkipWhile(t => !t.Equals(this)).Skip(1);
        public IEnumerable<TagNode> NextFullElements => Siblings.SkipWhile(t => !t.Equals(this)).Skip(1);
        public IEnumerable<TagNode> PrevFullElements => Siblings.SkipWhile(t => !t.Equals(this)).Skip(1);



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

        public string GetFullText
        {
            get
            {
                if (fullText == null)
                {
                    StringBuilder textBuilder = new StringBuilder(Text);
                    foreach (var item in TopDown(this))
                        if (item.Text.Length > 0)
                            textBuilder.AppendLine(item.Text);
                    fullText = textBuilder.ToString();
                }
                return fullText;
            }
        }

        public override string ToString() => Name;

        public TagNode GetTag(string tag) 
            => FindAll(
                Wide, 
                t => t.Name.Equals(tag, StringComparison.CurrentCultureIgnoreCase)
                ).First();

        private static IEnumerable<TagNode> Wide(TagNode arg)
        {
            Queue<TagNode> queue = new Queue<TagNode>();
            queue.Enqueue(arg);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                current.Children.ForEach(t => queue.Enqueue(t));
                yield return current;
            }
        }

        public static IEnumerable<TagNode> TopDown(TagNode arg)
        {
            yield return arg;
            foreach (var child in arg.Children)
                foreach (var item in TopDown(child))
                    yield return item;
        }
        public IEnumerable<TagNode> FindAll() => FindAll(_ => true);
        public IEnumerable<TagNode> FindAll(string tagName) 
            => FindAll(Wide, t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
        public IEnumerable<TagNode> FindAll(string[] tagsNames) 
            => FindAll(Wide, t => tagsNames.Contains(t.Name));
        public IEnumerable<TagNode> FindAll(Func<TagNode, bool> filter) => FindAll(Wide, filter);
        public IEnumerable<TagNode> FindAll
            (Func<TagNode, IEnumerable<TagNode>> iter, Func<TagNode, bool> filter)
            => iter(this).Skip(1).Where(filter);
        public IEnumerable<TagNode> FindParents(Func<TagNode, bool> filter) => FindAll(Wide, filter);

    }
    public static class TagExtensions
    {
        public static IEnumerable<TagNode> WithAttribute(this IEnumerable<TagNode> obj, string attrName)
            => obj.Where(t => t.Attributes.ContainsKey(attrName));
        public static IEnumerable<TagNode> WithAttribute(this IEnumerable<TagNode> obj, string attrName, string attrValue)
            => obj.Where(t => t.Attributes.ContainsKey(attrName) && t.Attributes[attrName] == attrValue);
        public static IEnumerable<TagNode> WithClass(this IEnumerable<TagNode> obj, string className)
            => WithClass(obj, s => s.Split().Contains(className));
        public static IEnumerable<TagNode> WithClass(this IEnumerable<TagNode> obj, Func<string, bool> func)
            => obj.Where(t => t.Attributes.ContainsKey("class") && func(t.Attributes["class"]));
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
