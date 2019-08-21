using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using CsvHelper;

namespace LogoFinderConsole
{
    static class GarysJankThing
    {
        public static IList<Merchant> Load(string csvLocation)
        {
            List<Merchant> mechantList = new List<Merchant>();
            using (var reader = new StreamReader(csvLocation))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.HeaderValidated = null;
                csv.Configuration.MissingFieldFound = null;
                mechantList.AddRange(csv.GetRecords<Merchant>().ToList());
            }

            foreach (var merchant in mechantList)
            {
                merchant.Name = RemoveSpecialCharacters(merchant.Name);
            }

            return mechantList;
        }

        public static string RunSetOfTests(string name)
        {
            string found = RunTest(name, false, true);
            if (found != null)
            {
                Console.WriteLine($"{found} found! Adding to list");
                return found;
            }

            found = RunTest(name, false, false);
            if (found != null)
            {
                Console.WriteLine($"{found} found! Adding to list");
                return found;
            }

            found = RunTest(name, true, true);
            if (found != null)
            {
                Console.WriteLine($"{found} found! Adding to list");
                return found;
            }

            found = RunTest(name, true, false);
            if (found != null)
            {
                Console.WriteLine($"{found} found! Adding to list");
                return found;
            }

            Console.WriteLine($"Website for {name} NOT found! gg");
            return null;
        }


        public static string RunTest(string name, bool com, bool https)
        {
            string urlToTest = BasicTrimJob(name, com, https);

            if (DoesWebsiteExist(urlToTest))
                return urlToTest;

            return null;
        }

        public static string BasicTrimJob(string name, bool com, bool https)
        {
            string url = "";

            if (https)
                url = "https://www.";
            else
                url = "http://www.";

            name = name.ToLower().Trim().Replace(" ", "");
            name = RemoveSpecialCharacters(name);

            if (com)
                return url + name + ".com";
            else
                return url + name + ".co.uk";
        }

        public static bool DoesWebsiteExist(string url)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(2);
            try
            {
                HttpResponseMessage something =
                    httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;

                return something.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
