using HtmlScrapper.Common;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace HtmlScrapper.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            string path = @"D:\Projects\HTMLScrapper\test.htm";
            string url = "https://www.crummy.com/software/BeautifulSoup/bs4/doc/";
            //var node = HtmlDocument.LoadFromPath(path).Scrap;
            Console.WriteLine("Loading document");
            var node = HtmlDocument.LoadFromUrl(url).Scrap;
            
            Console.WriteLine("Done!");
            sp.Stop();
            Console.WriteLine(sp.ElapsedMilliseconds);
        }
    }
}
