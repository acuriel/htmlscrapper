﻿using System;
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

        public TagNode NextSibling => NextFullSiblings.First();
        public TagNode PrevSibling => PrevFullSiblings.First();
        public IEnumerable<TagNode> NextFullSiblings => Siblings.SkipWhile(t => !t.Equals(this)).Skip(1);
        public IEnumerable<TagNode> PrevFullSiblings => Siblings.Reverse().SkipWhile(t => !t.Equals(this)).Skip(1);


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
            => SearchTags(
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

        public IEnumerable<TagNode> SearchTags
            (Func<TagNode, IEnumerable<TagNode>> iter, Func<TagNode, bool> filter)
            => iter(this).Skip(1).Where(filter);

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
