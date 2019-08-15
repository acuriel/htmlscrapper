using System;
using System.Collections.Generic;
using System.Text;

namespace HtmlScrapper.Common.Parsers
{
    public interface IParser<T>
    {
        T Parse(string content);
    }
}
