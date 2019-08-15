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
        private static string spoofedAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.100 Safari/537.36";

        static void Main(string[] args)
        {
            string TARGET = "https://www.blacks.co.uk/";

            TARGET = TARGET.TrimEnd('/');

            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("User-Agent", spoofedAgent);
            httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
            httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");
            httpClient.DefaultRequestHeaders.Add("referer", "https://www.google.co.uk");

            var uri = new Uri(TARGET);
            var page = httpClient.GetStreamAsync(uri).Result;

            HtmlDocument doc = new HtmlDocument();

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
                client.Headers[HttpRequestHeader.UserAgent] = spoofedAgent;
                client.Headers[HttpRequestHeader.Referer] = "https://www.google.co.uk";
                client.Headers["pragma"] = "no-cache";
                client.Headers[HttpRequestHeader.CacheControl] = "no-cache";

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
                    if (!filename.Contains("."))
                    {
                        filename += "-assumed-.png";
                    }

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
