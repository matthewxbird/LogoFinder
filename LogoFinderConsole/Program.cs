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
            string TARGET = "URLHERE";

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
            IList<HtmlNode> nodes = GetNodesRecursively(doc.DocumentNode);
            var possibleLogos = new List<string>();
            possibleLogos.AddRange(getMetaOgImage(nodes));
            possibleLogos.AddRange(getAppleTouchIcons(nodes));
            possibleLogos.AddRange(getAllAnchors(nodes, TARGET));
            possibleLogos.AddRange(getAsIcon(nodes));
            possibleLogos.AddRange(getAllKeyWords(nodes, "logo"));

            //possibleLogos.AddRange(getAllFromAnyStyleSheets(TARGET, nodes, "background-image", "logo"));

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
                    filename = filename.Replace("/", "");
                    if (!filename.Contains("."))
                    {
                        filename += "_assumed.png";
                    }

                    var writeLocation = Path.Combine(dir.FullName, filename);

                    Console.WriteLine($"Attempting to download: {uri}");
                    try
                    {
                        client.DownloadFile(uri, writeLocation);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something didn't work: " + e.Message);
                    }
                }
            }

            Console.WriteLine("Completed downloads!");
        }

        private static IList<string> getMetaOgImage(IList<HtmlNode> nodes)
        {
            IList<string> possibilities = new List<string>();

            foreach (var node in nodes)
            {
                foreach (var attribute in node.Attributes)
                {
                    if (attribute.Name == "property" && attribute.Value == "og:image")
                    {
                        possibilities.Add(node.Attributes.First(x => x.Name == "content").Value);
                    }
                }
            }

            return possibilities;
        }

        private static IList<string> getAppleTouchIcons(IList<HtmlNode> nodes)
        {
            IList<string> possibles = new List<string>();

            foreach (var node in nodes)
            {
                foreach (var attribute in node.Attributes)
                {
                    if (attribute.Name == "rel" && attribute.Value == "apple-touch-icon")
                    {
                        var link = node.Attributes.First(x => x.Name == "href").Value;
                        possibles.Add(link);
                    }
                }
            }

            return possibles;
        }

        private static IList<string> getAllAnchors(IList<HtmlNode> nodes, string self)
        {
            IList<string> possiblities = new List<string>();

            foreach (var node in nodes)
            {
                foreach (var attribute in node.Attributes)
                {
                    if (attribute.Name == "href" && (attribute.Value == "/" || attribute.Value == self))
                    {
                        foreach (var possibleImageNode in node.ChildNodes)
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

        public static IList<string> getAsIcon(IList<HtmlNode> nodes)
        {
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

        public static IList<string> getAllKeyWords(IList<HtmlNode> nodes, string keyword)
        {
            IList<string> possibles = new List<string>();
            foreach (var node in nodes)
            {
                foreach (var attribute in node.Attributes)
                {
                    if (attribute.Value.Contains('.') && attribute.Value.Contains(keyword, StringComparison.InvariantCultureIgnoreCase))
                    {
                        possibles.Add(attribute.Value);
                    }
                }
            }

            return possibles;
        }

        //public static IList<string> getAllFromAnyStyleSheets(string target, IList<HtmlNode> nodes, string property, string keyword)
        //{
        //    var links = nodes.Where(x => x.Name == "link");
        //    var stylesheetUris = new List<string>();

        //    foreach (var link in links)
        //    {
        //        foreach (var attribute in link.Attributes)
        //        {
        //            if (attribute.Name == "rel" && attribute.Value == "stylesheet")
        //            {
        //                stylesheetUris.Add(link.Attributes.First(x => x.Name == "href").Value);
        //            }
        //        }
        //    }

        //    HttpClient httpClient = new HttpClient();

        //    httpClient.DefaultRequestHeaders.Add("User-Agent", spoofedAgent);
        //    httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
        //    httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");
        //    httpClient.DefaultRequestHeaders.Add("referer", "https://www.google.co.uk");

        //    IList<string> downloadedStylesheet = new List<string>();

        //    foreach (var stylesheet in stylesheetUris)
        //    {
        //        var uri = new Uri(target + stylesheet);
        //        var page = httpClient.GetStringAsync(uri).Result;
        //        downloadedStylesheet.Add(page);
        //    }

        //    Console.WriteLine("test");

        //    return null;
        //}


        private static IList<HtmlNode> GetNodesRecursively(HtmlNode node)
        {
            List<HtmlNode> nodes = new List<HtmlNode>
            {
                node
            };

            foreach (HtmlNode child in node.ChildNodes)
            {
                nodes.AddRange(GetNodesRecursively(child));
            }

            return nodes;
        }
    }


}
