using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HtmlScrapper.Common
{
    /// <summary>
    /// A basic Node
    /// </summary>
    public class TagNode
    {
        private string fullText;
        private string text;

        /// <summary>
        /// TagNode constructor that receives a name and a parent
        /// </summary>
        /// <param name="name">The tag's name</param>
        /// <param name="parent">The tag's parent</param>
        public TagNode(string name, TagNode parent)
        {
            Name = name.ToLower();
            Attributes = new Dictionary<string, string>();
            Children = new List<TagNode>();
            Parent = parent;
        }

        /// <summary>
        /// TagNode's Name (word between '<' and '>')
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// TagNode's attributes (attrName="attrValue"). Ex. class, id, href, src, ...
        /// This attributes are saved in a dictionary where the attribute's name is the key 
        /// and the attribute's value is the assigned value to that key
        /// </summary>
        public virtual Dictionary<string,string> Attributes { get; private set; }
        /// <summary>
        /// The list of the children (contained tags)
        /// </summary>
        public virtual List<TagNode> Children { get; private set; }
        /// <summary>
        /// TagNode's parent. If it's null, the tag is a root node
        /// </summary>
        public TagNode Parent { get; set; }
        /// <summary>
        /// TagNode's siblings
        /// </summary>
        public IEnumerable<TagNode> Siblings 
            => Parent != null ? 
            Parent.Children.Where((t) => t != this) : 
            new List<TagNode>();

        /// <summary>
        /// Sibling after the current tag in the document
        /// </summary>
        public TagNode NextSibling => NextFullSiblings.First();
        /// <summary>
        /// Sibling before the current tag in the document
        /// </summary>
        public TagNode PrevSibling => PrevFullSiblings.First();
        /// <summary>
        /// All the siblings after the current tag in the document
        /// </summary>
        public IEnumerable<TagNode> NextFullSiblings => Siblings.SkipWhile(t => !t.Equals(this)).Skip(1);
        /// <summary>
        /// All the siblings before the current tag in the document
        /// </summary>
        public IEnumerable<TagNode> PrevFullSiblings => Siblings.Reverse().SkipWhile(t => !t.Equals(this)).Skip(1);
        /// <summary>
        /// All the decendents returned using a BFS search
        /// </summary>
        public IEnumerable<TagNode> Decendents => Breadth(this);
        /// <summary>
        /// All the ancestos from the current element to the root (BottomUp)
        /// </summary>
        public IEnumerable<TagNode> Ancestors => BottomUp(this);


        /// <summary>
        /// Gets the text directly isnide the current tag
        /// </summary>
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
        /// <summary>
        /// Get the full text inside the current tag.
        /// </summary>
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

        /// <summary>
        /// Search the first match to a tag's name from the current tag (BFS)
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public TagNode GetTag(string tag) 
            => Find(
                Breadth, 
                t => t.Name.Equals(tag, StringComparison.CurrentCultureIgnoreCase)
                ).First();

        /// <summary>
        /// BFS from a given tag
        /// </summary>
        /// <param name="arg">The tag to start searching from</param>
        /// <returns>An iterator with the results</returns>
        private static IEnumerable<TagNode> Breadth(TagNode arg)
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

        /// <summary>
        /// TopDown search from a given tag
        /// </summary>
        /// <param name="arg">The tag to start searching from</param>
        /// <returns>An iterator with the results</returns>
        public static IEnumerable<TagNode> TopDown(TagNode arg)
        {
            yield return arg;
            foreach (var child in arg.Children)
                foreach (var item in TopDown(child))
                    yield return item;
        }

        /// <summary>
        /// BottomUp search from a given tag
        /// </summary>
        /// <param name="arg">The tag to start searching from</param>
        /// <returns>An iterator with the results</returns>
        public static IEnumerable<TagNode> BottomUp(TagNode arg)
        {
            while(arg.Parent != null)
            {
                arg = arg.Parent;
                yield return arg;
            }
        }

        /// <summary>
        /// Gets all the tags starting from current tag
        /// </summary>
        /// <returns>All the tags</returns>
        public IEnumerable<TagNode> FindAll() => FindAll(_ => true);
        /// <summary>
        /// Gets all the tags fitting with a specific condition (BFS)
        /// </summary>
        /// <param name="filter">The condition to filter with</param>
        /// <returns>All the tags fitting with the given condition</returns>
        public IEnumerable<TagNode> FindAll(Func<TagNode, bool> filter) => Find(Breadth, filter);

        /// <summary>
        /// Gets all the tags, with an especific iteration order and a given filtering condition
        /// </summary>
        /// <param name="iter">A generator for the current tag</param>
        /// <param name="filter">The condition to filter with</param>
        /// <returns></returns>
        public IEnumerable<TagNode> Find
            (Func<TagNode, IEnumerable<TagNode>> iter, Func<TagNode, bool> filter)
            => iter(this).Skip(1).Where(filter);
        public IEnumerable<TagNode> FindParents(Func<TagNode, bool> filter) => Find(BottomUp, filter);

    }

    /// <summary>
    /// Extension class for TagNode
    /// </summary>
    public static class TagExtensions
    {
        /// <summary>
        /// Gets the element with a current tag name
        /// </summary>
        /// <param name="obj">Current TagNode</param>
        /// <param name="tagName">Tag Name to search for</param>
        /// <returns></returns>
        public static IEnumerable<TagNode> WithTag(this IEnumerable<TagNode> obj, string tagName)
            => obj.Where(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
        /// <summary>
        /// Gets the elements with at least one of the given tag names
        /// </summary>
        /// <param name="obj">Current TagNode</param>
        /// <param name="tagsNames">A list of tag names</param>
        /// <returns></returns>
        public static IEnumerable<TagNode> WithTag(this IEnumerable<TagNode> obj, params string[] tagsNames)
            => obj.Where(t => tagsNames.Contains(t.Name));

        /// <summary>
        /// Gets the elements with a specific attribute key
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="attrName">The attribute key (name) </param>
        /// <returns></returns>
        public static IEnumerable<TagNode> WithAttribute(this IEnumerable<TagNode> obj, string attrName)
            => obj.Where(t => t.Attributes.ContainsKey(attrName));
        /// <summary>
        /// Gets the elements with a specific attribute with a given value
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="attrName">The attribute's name</param>
        /// <param name="attrValue">The attribute's value</param>
        /// <returns></returns>
        public static IEnumerable<TagNode> WithAttribute(this IEnumerable<TagNode> obj, string attrName, string attrValue)
            => obj.Where(t => t.Attributes.ContainsKey(attrName) && t.Attributes[attrName] == attrValue);

        /// <summary>
        /// Gets the elements with a given class (class="className")
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="className">The class's name to search for</param>
        /// <returns></returns>
        public static IEnumerable<TagNode> WithClass(this IEnumerable<TagNode> obj, string className)
            => WithClass(obj, s => s.Split().Contains(className));
        /// <summary>
        /// Gets the elements with a class name following an especific condition
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="func">The condition that evaluates a class name</param>
        /// <returns></returns>
        public static IEnumerable<TagNode> WithClass(this IEnumerable<TagNode> obj, Func<string, bool> func)
            => obj.Where(t => t.Attributes.ContainsKey("class") && func(t.Attributes["class"]));
    }

    /// <summary>
    /// A node tha represents Text
    /// </summary>
    public class TextNode : TagNode
    {
        public TextNode(TagNode parent, string text) : base("text", parent)
        {
            Text = text;
        }
        public override string Text => base.Text;
    }
}
