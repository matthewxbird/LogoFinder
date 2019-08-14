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
            const string TARGET = "https://www.myringgo.co.uk/";

            HttpClient httpClient = new HttpClient();
            var uri = new Uri(TARGET);
            var page = httpClient.GetStreamAsync(uri).Result;

            HtmlDocument doc = new HtmlDocument
            {
                DisableServerSideCode = true
            };

            doc.Load(page, true);

            Console.WriteLine($"Found: {getMetaOgImage(doc)}");
            Console.WriteLine($"Found: {getAppleTouchIcons(doc)}");
            Console.WriteLine($"Found: {getAllAnchors(doc, TARGET)}");
            Console.WriteLine($"Found: {getAsIcon(doc)}");

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

            IList<string> possibles = new List<string>();

            foreach (var linkNode in linkNodes)
            {
                foreach (var attribute in linkNode.Attributes)
                {
                    if (attribute.Name == "rel" && attribute.Value == "apple-touch-icon")
                    {
                        var link = linkNode.Attributes.First(x => x.Name == "href").Value;
                        possibles.Add(link);
                    }
                }
            }

            return string.Join(",", possibles);
        }

        private static string getAllAnchors(HtmlDocument doc, string self)
        {
            IList<HtmlNode> anchorNodes = doc.DocumentNode.Descendants("a").ToList();

            IList<string> possiblities = new List<string>();

            foreach (var anchorNode in anchorNodes)
            {
                foreach (var attribute in anchorNode.Attributes)
                {
                    if (attribute.Name == "href" && (attribute.Value == "/" || attribute.Value == self))
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

        public static string getAsIcon(HtmlDocument doc)
        {
            IList<HtmlNode> nodes = doc.DocumentNode.SelectNodes("//head/link");
            IList<string> possibles = new List<string>();
            foreach (var node in nodes)
            {
                foreach (var attribute in node.Attributes)
                {
                    if (attribute.Name == "rel" && attribute.Value == "icon")
                    {
                        var link = node.Attributes.First(x => x.Name == "href").Value;
                        possibles.Add(link);
                    }
                }
            }

            return string.Join(",", possibles);
        }
    }


}
