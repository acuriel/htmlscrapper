using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HtmlScrapper.Common.Parsers
{
    public class HtmlParser : IParser<TagNode>
    {
        private static HtmlParser instance = new HtmlParser();

        /// <summary>
        /// Singleton method
        /// </summary>
        /// <returns>The parser instance</returns>
        public static HtmlParser GetInstance() => instance;

        //Basic symbols
        private const char TAG_START_SYMBOL = '<';
        private const char TAG_END_SYMBOL = '>';
        private const char TAG_CLOSE_SYMBOL = '/';
        private const char ATTR_ASSIGN_SYMBOL = '=';
        private const string ATTR_ENCL_SYMBOLs = "'\"";
        private const char ATTR_SEP_SYMBOL = ' ';

        private const string DUMMY_SYMBOLS = "\n\r\t ";

        /// <summary>
        /// Move the cursor until the next ocurrence of a given symbols
        /// </summary>
        /// <param name="symbols">The given symbols</param>
        private void MoveUntilNextSymbol(string symbols = DUMMY_SYMBOLS)
            => GetWhile(c => symbols.Contains(c));

        /// <summary>
        /// Get the content while a condition is happening
        /// </summary>
        /// <param name="condition">The condition to be checked</param>
        /// <returns></returns>
        private string GetWhile(Func<char, bool> condition)
        {
            string result = "";
            while (condition(content[pos]))
                result += content[pos++];
            return result;
        }

        private int pos;
        private string content;
        Stack<TagNode> stack;
        TagNode rootNode;

        /// <summary>
        /// Parse the given content and return the root node
        /// </summary>
        /// <param name="text">The content to be parsed</param>
        /// <returns></returns>
        public TagNode Parse(string text)
        {
            pos = 0;
            content = text;
            stack = new Stack<TagNode>();
            string docType;

            while (pos < content.Length) //execute until end of content
            {
                MoveUntilNextSymbol(); //ignore dummy symbols
                char currentChar = content[pos];
                if (content[pos] == TAG_START_SYMBOL) //a tag is starting
                {
                    if (content[pos + 1] == '!')//a comment or the doctype
                    {
                        if (content[pos + 2] == '-') //ignore if comment
                            JumpComment();
                        else
                             docType = GetDocType(); //extract doctype
                    }
                    else
                        ParseTag(); //parse current tag
                }
                else
                    AddText(); //if it'sn not a tag, it's flat text
            }

            return rootNode;
        }

        /// <summary>
        /// Parse the DocType
        /// </summary>
        /// <returns>The extracted doctype</returns>
        private string GetDocType()
        {
            GetWhile(c => c != ATTR_SEP_SYMBOL);
            pos++;
            string docType = GetWhile(c => c != TAG_END_SYMBOL);
            pos++;
            return docType;
        }

        /// <summary>
        /// Extract text and ignore styles and scripts
        /// </summary>
        private void AddText()
        {
            TagNode tag = stack.Peek();
            string text;
            if (tag.Name == "script" || tag.Name == "style")
               text = ForceTagClose(); //ignore scripts and styles
            else
                text = GetWhile(c => c != TAG_START_SYMBOL);

            text = text.Trim(DUMMY_SYMBOLS.ToCharArray());
            if(text.Length > 0)
                tag.Children.Add(new TextNode(tag, text.Trim()));
        }

        /// <summary>
        /// Set the cursor at the endo of the current tag
        /// </summary>
        /// <returns>The ignored text</returns>
        private string ForceTagClose()
        {
            string text = GetWhile(c => c!=TAG_START_SYMBOL);
            while (content[pos + 1] != TAG_CLOSE_SYMBOL)
            {
                text += content[pos++];
                text += GetWhile(c => c != TAG_START_SYMBOL);
            }
            return text;
        }

        /// <summary>
        /// Ignores a comment
        /// </summary>
        private void JumpComment()
        {
            while (!(content[pos] == TAG_END_SYMBOL && content[pos - 1] == '-' && content[pos - 2] == '-'))
                pos++;
            pos++;
        }

        /// <summary>
        /// Parses the curent tag
        /// </summary>
        private void ParseTag()
        {
            bool closeTag = false;

            //if it's a self closed tag </aaa>
            if (closeTag = content[++pos] == TAG_CLOSE_SYMBOL)
                pos++;

            //get tag name
            string name = GetWhile(c => c != ATTR_SEP_SYMBOL && c!=TAG_END_SYMBOL);

            //finish closing the tag or opened
            if (closeTag)
            {
                CloseTag(name);
                pos++;
            }
            else
                OpenTag(name);
        }

        private void OpenTag(string name)
        {
            TagNode tag = new TagNode(name, stack.Count == 0 ? null : stack.Peek());

            //set the html tag as Root Tag
            if (tag.Parent == null && name == "html")
                rootNode = tag;

            //add as child of parent
            if (tag.Parent != null)
                tag.Parent.Children.Add(tag);

            //get attributes
            if(content[pos] != TAG_END_SYMBOL)
                GetAttributes(tag);

            stack.Push(tag);

            if (content[pos] == TAG_END_SYMBOL)
                pos++;
            else if (content[pos] == TAG_CLOSE_SYMBOL && content[pos + 1] == TAG_END_SYMBOL)
            {
                CloseTag(name);
                pos += 2;
            }
        }

        /// <summary>
        /// Closes the current tag
        /// </summary>
        /// <param name="name"></param>
        private void CloseTag(string name)
        {
            TagNode cursor = stack.Pop();
            //close all the tags in the stack until find the correct one
            //fix all the sub-tree (non-closed tags)
            while (cursor.Name != name)
            {
                cursor.Parent.Children.AddRange(cursor.Children);
                cursor.Children.ForEach(t => t.Parent = cursor.Parent);
                cursor.Children.Clear();
                cursor = stack.Pop();
            }
        }

        /// <summary>
        /// Extract the attributes from a given tag
        /// </summary>
        /// <param name="tag">The tag</param>
        private void GetAttributes(TagNode tag)
        {
            bool tagEnd = false;
            while (!tagEnd)
            {
                MoveUntilNextSymbol();

                //check if is end of tag
                tagEnd = content[pos] == TAG_CLOSE_SYMBOL || content[pos] == TAG_END_SYMBOL;

                if (!tagEnd)
                {
                    //get attribute name
                    string attrName = GetWhile(c => c != ATTR_ASSIGN_SYMBOL && !DUMMY_SYMBOLS.Contains(c));
                    MoveUntilNextSymbol(); //find = symbol
                    pos++;
                    MoveUntilNextSymbol(); //find " or ' symbol

                    char openingAttrSymbol = content[pos++];
                    string attrValue = GetWhile(c => c != openingAttrSymbol);

                    tag.Attributes.Add(attrName, attrValue);
                    pos++;
                    //FIX: empty attr like 'checked'
                }
                char temp = content[pos];
            }
        }
    }
}
