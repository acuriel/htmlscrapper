using System;
using System.Collections.Generic;
using System.Text;

namespace HtmlScrapper.Common.Parsers
{
    /// <summary>
    /// Represents a Parser
    /// </summary>
    /// <typeparam name="T">The Type of element that must be returned</typeparam>
    public interface IParser<T>
    {
        /// <summary>
        /// Parses the given content and returns the result
        /// </summary>
        /// <param name="content">The content to be parsed</param>
        /// <returns></returns>
        T Parse(string content);
    }
}
