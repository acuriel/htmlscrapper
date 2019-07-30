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
        public TagNode RootNode { get; set; }

        public static Scrapper<T> LoadFromText(string content, IParser<T> parser) 
            => new Scrapper<T>(content, parser);

        public static Scrapper<T> LoadFromPath(string path, IParser<T> parser)
            => LoadFromStream(new FileStream(path, FileMode.Open), parser);

        public static Scrapper<T> LoadFromStream(Stream stream, IParser<T> parser, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            Scrapper<T> result;
            using (StreamReader reader = new StreamReader(stream, encoding))
            {
                string content = reader.ReadToEnd();
                result = new Scrapper<T>(content, parser);
            }
            return result;
        }
        public static async Task<Scrapper<T>> LoadFromStreamAsync(Stream stream, IParser<T> parser, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            Scrapper<T> result;
            using (StreamReader reader = new StreamReader(stream, encoding))
            {
                string content = await reader.ReadToEndAsync();
                result = new Scrapper<T>(content, parser);
            }
            return result;
        }
        public static Scrapper<T> LoadFromUrl(string url, IParser<T> parser)
        {
            throw new NotImplementedException();
        }
    }

    public class HtmlDocument : Document<HtmlDocument>
    {
        public string DocType { get; set; }

        public static Scrapper<HtmlDocument> LoadFromText(string content) 
            => LoadFromText(content, HtmlParser.GetInstance());

        public static Scrapper<HtmlDocument> LoadFromPath(string path)
            => LoadFromPath(path, HtmlParser.GetInstance());
        public static Scrapper<HtmlDocument> LoadFromStream(Stream stream, Encoding encoding = null) 
            => LoadFromStream(stream, HtmlParser.GetInstance(), encoding);
        public static async Task<Scrapper<HtmlDocument>> LoadFromStreamAsync(Stream stream, Encoding encoding = null)
            => await LoadFromStreamAsync(stream, HtmlParser.GetInstance(), encoding);
        public static Scrapper<HtmlDocument> LoadFromUrl(string url)
            => LoadFromUrl(url, HtmlParser.GetInstance());
    }
}
