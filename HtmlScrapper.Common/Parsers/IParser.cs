using System;
using System.Collections.Generic;
using System.Text;

namespace HtmlScrapper.Common.Parsers
{
    public interface IParser<T> where T:Document<T>
    {
        T Parse(string content);
    }
}
