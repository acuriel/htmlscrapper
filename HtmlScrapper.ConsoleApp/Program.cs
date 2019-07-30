using HtmlScrapper.Common;
using System;
using System.Diagnostics;

namespace HtmlScrapper.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            string path = @"D:\Projects\HTMLScrapper\test.htm";
            var scrapper = HtmlDocument.LoadFromPath(path);
            sp.Stop();
            Console.WriteLine(sp.ElapsedMilliseconds);
        }
    }
}
