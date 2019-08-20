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
        public static IList<Merchant> Main(string csvLocation)
        {
            IList<Merchant> listOfFoundMerchants = new List<Merchant>();

            using (var reader = new StreamReader(csvLocation))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.HeaderValidated = null;
                csv.Configuration.MissingFieldFound = null;
                List<Merchant> merchants = csv.GetRecords<Merchant>().ToList();
                int totalMerchants = merchants.Count;

                foreach (Merchant merchant in merchants)
                {
                    string httpscouk = RunTest(merchant.Name, false, true);
                    if (httpscouk != null)
                    {
                        listOfFoundMerchants.Add(new Merchant(merchant.Name, httpscouk));
                        Console.WriteLine($"{httpscouk} found! Adding to list");
                        continue;
                    }

                    string httpcouk = RunTest(merchant.Name, false, false);
                    if (httpcouk != null)
                    {
                        listOfFoundMerchants.Add(new Merchant(merchant.Name, httpcouk));
                        Console.WriteLine($"{httpcouk} found! Adding to list");
                        continue;
                    }

                    string httpscom = RunTest(merchant.Name, true, true);
                    if (httpscom != null)
                    {
                        listOfFoundMerchants.Add(new Merchant(merchant.Name, httpscom));
                        Console.WriteLine($"{httpscom} found! Adding to list");
                        continue;
                    }

                    string httpcom = RunTest(merchant.Name, true, false);
                    if (httpcom != null)
                    {
                        listOfFoundMerchants.Add(new Merchant(merchant.Name, httpcom));
                        Console.WriteLine($"{httpcom} found! Adding to list");
                        continue;
                    }

                    Console.WriteLine($"Website for {merchant.Name} NOT found! gg");
                }

                Console.WriteLine(
                    $"JANK complete - Found {listOfFoundMerchants.Count} out of {totalMerchants}");
            }

            return listOfFoundMerchants;
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
            httpClient.Timeout = TimeSpan.FromSeconds(5);
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
