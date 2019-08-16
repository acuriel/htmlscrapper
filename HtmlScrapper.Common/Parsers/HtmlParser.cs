using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HtmlScrapper.Common.Parsers
{
    public class HtmlParser : IParser<TagNode>
    {
        private static HtmlParser instance = new HtmlParser();
        public static HtmlParser GetInstance() => instance;

        private const char TAG_START_SYMBOL = '<';
        private const char TAG_END_SYMBOL = '>';
        private const char TAG_CLOSE_SYMBOL = '/';
        private const char ATTR_ASSIGN_SYMBOL = '=';
        private const string ATTR_ENCL_SYMBOLs = "'\"";
        private const char ATTR_SEP_SYMBOL = ' ';

        private const string DUMMY_SYMBOLS = "\n\r\t ";

        private void MoveUntilNextSymbol(string ignoreSymbols = DUMMY_SYMBOLS)
            => GetWhile(c => ignoreSymbols.Contains(c));

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
        public TagNode Parse(string text)
        {
            pos = 0;
            content = text;
            stack = new Stack<TagNode>();
            string docType;

            while (pos < content.Length) //execute until end of content
            {
                MoveUntilNextSymbol();
                char currentChar = content[pos];
                if (content[pos] == TAG_START_SYMBOL)
                {
                    if (content[pos + 1] == '!')
                    {
                        if (content[pos + 2] == '-')
                            JumpComment();
                        else
                             docType = GetDocType();
                    }
                    else
                        ParseTag();
                }
                else
                    AddText();
            }

            return rootNode;
        }

        private string GetDocType()
        {
            GetWhile(c => c != ATTR_SEP_SYMBOL);
            pos++;
            string docType = GetWhile(c => c != TAG_END_SYMBOL);
            pos++;
            return docType;
        }

        private void AddText()
        {
            TagNode tag = stack.Peek();
            string text;
            if (tag.Name == "script" || tag.Name == "style")
               text = ForceTagClose();
            else
                text = GetWhile(c => c != TAG_START_SYMBOL);
            text = text.Trim(DUMMY_SYMBOLS.ToCharArray());
            if(text.Length > 0)
                tag.Children.Add(new TextNode(tag, text.Trim()));
        }

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

        private void JumpComment()
        {
            while (!(content[pos] == TAG_END_SYMBOL && content[pos - 1] == '-' && content[pos - 2] == '-'))
                pos++;
            pos++;
        }

        private void ParseTag()
        {
            bool closeTag = false;

            if (closeTag = content[++pos] == TAG_CLOSE_SYMBOL)
                pos++;

            //get tag name
            string name = GetWhile(c => c != ATTR_SEP_SYMBOL && c!=TAG_END_SYMBOL);

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

        private void CloseTag(string name)
        {
            TagNode cursor = stack.Pop();
            while (cursor.Name != name)
            {
                cursor.Parent.Children.AddRange(cursor.Children);
                cursor.Children.ForEach(t => t.Parent = cursor.Parent);
                cursor.Children.Clear();
                cursor = stack.Pop();
            }
        }

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

                    tag.Attributes.Add(new Attribute(attrName, attrValue));
                    pos++;
                    //FIX: empty attr like 'checked'
                }
                char temp = content[pos];
            }
        }
    }
}
