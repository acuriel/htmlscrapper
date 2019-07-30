using System;
using System.Collections.Generic;
using System.Linq;
using HtmlScrapper.Common.Parsers;

namespace HtmlScrapper.Common
{
    public class Scrapper<T> where T:Document<T>
    {
        public Scrapper(string content, IParser<T> parser)
        {
            Content = content;
            Parser = parser;
            Document = Parser.Parse(Content);
            Tags = new List<TagNode> { Document.RootNode };
        }

        public Document<T> Document { get; }
        public string Content { get; }
        public IEnumerable<TagNode> Tags { get; private set; }
        public IEnumerable<string> Text(bool deep = true) => Tags.Select(t => deep ? t.GetFullText() : t.Text);
        //FIX: when a tag is descendant of another tag in the list (duplicated text)
        private IParser<T> Parser { get; set; }

        public Scrapper<T> WithTag(string name)
        {
            Tags = SearchTag(
                Tags, 
                t => t.Name == name
                );
            return this;
        }
        public static IEnumerable<TagNode> SearchTag(IEnumerable<TagNode> tags, Func<TagNode, bool> filter) 
            => tags.Concat(tags.SelectMany(t => t.Children)).Where(filter);

        public Scrapper<T> WithAttribute(string key, string value=null)
        {
            Tags = SearchTag(
                Tags, 
                t => t.Attributes.Any(
                    a => a.Name == key && 
                    (value == null || a.Value == value)
                    )
                );
            return this;
        } 
    }
}
