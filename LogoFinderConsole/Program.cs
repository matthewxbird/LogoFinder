using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using HtmlAgilityPack;

namespace LogoFinderConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            const string TARGET = "https://www.shell.co.uk/";

            HttpClient httpClient = new HttpClient();
            var uri = new Uri(TARGET);
            var page = httpClient.GetStreamAsync(uri).Result;

            HtmlDocument doc = new HtmlDocument
            {
                DisableServerSideCode = true
            };

            doc.Load(page, true);

            var possibleLogos = new List<string>();
            possibleLogos.AddRange(getMetaOgImage(doc));
            possibleLogos.AddRange(getAppleTouchIcons(doc));
            possibleLogos.AddRange(getAllAnchors(doc, TARGET));
            possibleLogos.AddRange(getAsIcon(doc));

            Console.WriteLine($"We found the following possible logos: {string.Join(',', possibleLogos)}");

            DownloadLogos(TARGET, possibleLogos);

            Console.WriteLine("Press the any key to finish");
            Console.Read();
        }

        private static void DownloadLogos(string target, IList<string> possibles)
        {
            DirectoryInfo dir = new DirectoryInfo("Logos");
            if (dir.Exists)
            {
                dir.Delete(true);
            }

            dir.Create();

            using (var client = new WebClient())
            {
                foreach (var possible in possibles)
                {
                    string downloadUri = string.Empty;
                    if (possible.StartsWith("https"))
                    {
                        downloadUri = possible;
                    }
                    else
                    {
                        downloadUri = target + possible;
                    }

                    var uri = new Uri(downloadUri);
                    var filename = uri.Segments.Last();
                    var writeLocation = Path.Combine(dir.FullName, filename);

                    Console.WriteLine($"Attempting to download: {uri}");
                    client.DownloadFile(uri, writeLocation);
                }
            }

            Console.WriteLine("Completed downloads!");
        }

        private static IList<string> getMetaOgImage(HtmlDocument doc)
        {
            HtmlNodeCollection metaNodes = doc.DocumentNode.SelectNodes("//head//meta");

            IList<string> possibilities = new List<string>();

            foreach (var metaNode in metaNodes)
            {
                foreach (var attribute in metaNode.Attributes)
                {
                    if (attribute.Name == "property" && attribute.Value == "og:image")
                    {
                        possibilities.Add(metaNode.Attributes.First(x => x.Name == "content").Value);
                    }
                }
            }

            return possibilities;
        }

        private static IList<string> getAppleTouchIcons(HtmlDocument doc)
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

            return possibles;
        }

        private static IList<string> getAllAnchors(HtmlDocument doc, string self)
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

            return possiblities;
        }

        public static IList<string> getAsIcon(HtmlDocument doc)
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

            return possibles;
        }
    }


}
