using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;

namespace LogoFinderConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            const string TARGET = "https://www.tesco.com/";

            HttpClient httpClient = new HttpClient();

            var pageStream = httpClient.GetStreamAsync(TARGET);

            HtmlDocument doc = new HtmlDocument();
            doc.Load(pageStream.Result);

            Console.WriteLine($"Found: {getMetaOgImage(doc)}");
            Console.WriteLine($"Found: {getAppleTouchIcons(doc)}");
            Console.WriteLine($"Found: {getAllAnchors(doc, TARGET)}");

            Console.Read();
        }

        private static string getMetaOgImage(HtmlDocument doc)
        {
            HtmlNodeCollection metaNodes = doc.DocumentNode.SelectNodes("//head//meta");

            foreach (var metaNode in metaNodes)
            {
                foreach (var attribute in metaNode.Attributes)
                {
                    if (attribute.Name == "property" && attribute.Value == "og:image")
                    {
                        return metaNode.Attributes.First(x => x.Name == "content").Value;
                    }
                }
            }

            return null;
        }

        private static string getAppleTouchIcons(HtmlDocument doc)
        {
            IList<HtmlNode> linkNodes = doc.DocumentNode.Descendants("link").ToList();

            HtmlNode lastFound = null;

            foreach (var linkNode in linkNodes)
            {
                foreach (var attribute in linkNode.Attributes)
                {
                    if (attribute.Name == "rel" && attribute.Value == "apple-touch-icon")
                    {
                        lastFound = linkNode;
                    }
                }
            }

            if (lastFound != null)
            {
                return lastFound.Attributes.First(x => x.Name == "href").Value;
            }

            return null;
        }

        private static string getAllAnchors(HtmlDocument doc, string self)
        {
            IList<HtmlNode> anchorNodes = doc.DocumentNode.Descendants("a").ToList();

            IList<string> possiblities = new List<string>();

            foreach (var anchorNode in anchorNodes)
            {
                foreach (var attribute in anchorNode.Attributes)
                {
                    if (attribute.Name == "href" && (attribute.Value == "/" || attribute.Value == self.TrimEnd('/')))
                    {
                        foreach (var possibleImageNode in anchorNode.ChildNodes)
                        {
                            if (possibleImageNode.Name == "img")
                            {
                                possiblities.Add(possibleImageNode.Attributes.First(x => x.Name == "src").Value);
                            }
                        }
                    }
                }
            }

            if (possiblities.Count == 0) return null;

            return string.Join(',', possiblities);
        }
    }


}
