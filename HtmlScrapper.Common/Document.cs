using HtmlScrapper.Common.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HtmlScrapper.Common
{

    /// <summary>
    /// A basic Document
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Document<T> where T:Document<T>
    {
        /// <summary>
        /// Build a Document object from a given content using an especific Parser
        /// </summary>
        /// <param name="content">The document content</param>
        /// <param name="parser">The parser to be used</param>
        protected Document(string content, IParser<TagNode> parser)
        {
            Content = content;
            Parser = parser;
            RootTag = parser.Parse(content);
        }
        /// <summary>
        /// The RootTag extracted from the docuemnt
        /// </summary>
        public TagNode RootTag { get; }
        /// <summary>
        /// The given content
        /// </summary>
        public string Content { get; }
        /// <summary>
        /// The given Parser
        /// </summary>
        public IParser<TagNode> Parser { get; }

        /// <summary>
        /// Start Scrapping the document
        /// </summary>
        public virtual TagNode Scrap => RootTag;

        /// <summary>
        /// Load Document from text
        /// </summary>
        /// <param name="content">The text content</param>
        /// <param name="parser">The used parser</param>
        /// <returns></returns>
        public static Document<T> LoadFromText(string content, IParser<TagNode> parser)
            => new Document<T>(content, parser);

        /// <summary>
        /// Load document from an especific path
        /// </summary>
        /// <param name="path">The path where the content must be extracted</param>
        /// <param name="parser">The used parser</param>
        /// <returns></returns>
        public static Document<T> LoadFromPath(string path, IParser<TagNode> parser)
            => LoadFromStream(new FileStream(path, FileMode.Open), parser);

        /// <summary>
        /// Load Document from a Stream
        /// </summary>
        /// <param name="stream">The stream object</param>
        /// <param name="parser">The used parser</param>
        /// <param name="encoding">The encoding to be used for reading the stream</param>
        /// <returns></returns>
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
        /// <summary>
        /// Async load Document from a Stream
        /// </summary>
        /// <param name="stream">The stream object</param>
        /// <param name="parser">The used parser</param>
        /// <param name="encoding">The encoding to be used for reading the stream</param>
        /// <returns></returns>
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
        /// <summary>
        /// Load Document from a given url
        /// </summary>
        /// <param name="url">The url to extract content</param>
        /// <param name="parser">The used parser</param>
        /// <returns></returns>
        public static Document<T> LoadFromUrl(string url, IParser<TagNode> parser)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("Wrong URL or not connection to Internet");

            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream =
                response.CharacterSet == null
                ? new StreamReader(receiveStream)
                : new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

            string content = readStream.ReadToEnd();

            response.Close();
            readStream.Close();

            return new Document<T>(content, parser);
        }
    }

    /// <summary>
    /// An HTML document
    /// </summary>
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
