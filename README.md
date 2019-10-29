# HTML Scrapper for .NET

**NuGet:** [https://www.nuget.org/packages/WebTools.HtmlScrapper/](https://www.nuget.org/packages/WebTools.HtmlScrapper/)

## Introduction

There are incredible options for web scrapping like [BeautifulSoup4](https://pypi.org/project/beautifulsoup4/) in python, but I needed an easy to use and understand scrapper for .Net family. So, I started this simple scrapper, extensible and easy to understand.
Using a declarative syntax you'll be able to scrap information from HTML documents (extensible to others markup documents), from a file, url, and more.

## How to use

The first thing you need to do, is to load the content. A class called `HtmlDocument` contains some static methods for this:

```csharp
LoadFromText(string content);
LoadFromPath(string path);
LoadFromStream(Stream stream, Encoding encoding = null);
LoadFromStreamAsync(Stream stream, Encoding encoding =null);
LoadFromUrl(string url);
```

These method return a `Document` instance where you can use the `Scrap` for starting scrapping. 

```csharp
var rootNode = HtmlDocument.LoadFromUrl("https://www.foo.com").Scrap;
```

At this moment, the content was already loaded and parsed, so you can start to move through the document tree. The `Scrap` property returns te root node/tag (usually the `html` tag).

### TagNode class

The TagNode class represents a single tag in the document. It contains the following properties and methods that you could to extract info and moving through the document tree from a single Tag:

```csharp

//Properties
string Name
Dictionary<string,string> Attributes
List<TagNode> Children
TagNode Parent
IEnumerable<TagNode> Siblings
TagNode NextSibling // The following Tag in the same level
TagNode PrevSibling // The previous Tag in the same level
IEnumerable<TagNode> NextFullSiblings // The following Tags in the same level
IEnumerable<TagNode> PrevFullSiblings // The previous Tags in the same level
IEnumerable<TagNode> Descendants // uses BFS
IEnumerable<TagNode> Ancestors
string Text // The text in the current level (directly in the current Tag)
string GetFullText // The full text inside the current tag and its descendants

//Methods
IEnumerable<TagNode> FindAll() // returns all the tags starting in the current one
IEnumerable<TagNode> FindAll(Func<TagNode, bool> filter) // filter the tags with a current condition
IEnumerable<TagNode> Find(Func<TagNode, IEnumerable<TagNode>> iter, Func<TagNode, bool> filter) // filter the tags following a specific iterator and condition
IEnumerable<TagNode> FindAncestors(Func<TagNode, bool> filter) // the same as FindAll but going up in the tree
```

Also, you can use some `IEnumerable` extension methods provided to filter and move through the tree:

```csharp
// Filters elements by a Tag name
IEnumerable<TagNode> WithTag(this IEnumerable<TagNode> obj, string tagName)

// Filters current elements by a list of Tag names
IEnumerable<TagNode> WithTag(this IEnumerable<TagNode> obj, params string[] tagsNames)

// Filters elements keeping those that contains an specific attribute
IEnumerable<TagNode> WithAttribute(this IEnumerable<TagNode> obj, string attrName)

// Filters elements by an attribute and a value
IEnumerable<TagNode> WithAttribute(this IEnumerable<TagNode> obj, string attrName, string attrValue)

// Filters elements by a specific class
IEnumerable<TagNode> WithClass(this IEnumerable<TagNode> obj, string className)

// Filters elements using a condition that applies to the classes
IEnumerable<TagNode> WithClass(this IEnumerable<TagNode> obj, Func<string, bool> func)
```

Remember you can use any of the usual extension methods like, `Where`, `Any`, `All`, and more.

Also, you can chain all this methods when possible:

```csharp
var links = node.Descendants
                .WithTag("a")
                .WithClass(
                    (c)=>c.StartsWith("link-"))
                .Select((t) => t.Attributes["href"]);
```

## ToDo

There are a lot of pending functionality, and improvements I want to do to create a fluid use. Please, any idea or request, just contact me.
