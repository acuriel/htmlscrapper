using HtmlScrapper.Common.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HtmlScrapper.Common
{
    public class Document<T> where T:Document<T>
    {
        protected Document(string content, IParser<TagNode> parser)
        {
            Content = content;
            Parser = parser;
            RootNode = parser.Parse(content);
        }
        public TagNode RootNode { get; }
        public string Content { get; }
        public IParser<TagNode> Parser { get; }

        public virtual TagNode Scrap => RootNode;

        public static Document<T> LoadFromText(string content, IParser<TagNode> parser)
            => new Document<T>(content, parser);

        public static Document<T> LoadFromPath(string path, IParser<TagNode> parser)
            => LoadFromStream(new FileStream(path, FileMode.Open), parser);

        public static Document<T> LoadFromStream(Stream stream, IParser<TagNode> parser, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            string content;
            using (StreamReader reader = new StreamReader(stream, encoding))
            {
                content = reader.ReadToEnd();
            }
            return new Document<T>(content, parser);

        }
        public static async Task<Document<T>> LoadFromStreamAsync(Stream stream, IParser<TagNode> parser, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            string content;
            using (StreamReader reader = new StreamReader(stream, encoding))
            {
                content = await reader.ReadToEndAsync();
            }
            return new Document<T>(content, parser);
        }
        public static Document<T> LoadFromUrl(string url, IParser<TagNode> parser)
        {
            throw new NotImplementedException();
        }
    }

    public class HtmlDocument : Document<HtmlDocument>
    {
        protected HtmlDocument(string content, IParser<TagNode> parser, string docType)
            :base(content, parser)
        {
            DocType = docType;
        }
        public string DocType { get; }

        public static Document<HtmlDocument> LoadFromText(string content) 
            => LoadFromText(content, HtmlParser.GetInstance());

        public static Document<HtmlDocument> LoadFromPath(string path)
            => LoadFromPath(path, HtmlParser.GetInstance());
        public static Document<HtmlDocument> LoadFromStream(Stream stream, Encoding encoding = null) 
            => LoadFromStream(stream, HtmlParser.GetInstance(), encoding);
        public static async Task<Document<HtmlDocument>> LoadFromStreamAsync(Stream stream, Encoding encoding = null)
            => await LoadFromStreamAsync(stream, HtmlParser.GetInstance(), encoding);
        public static Document<HtmlDocument> LoadFromUrl(string url)
            => LoadFromUrl(url, HtmlParser.GetInstance());
    }
}
