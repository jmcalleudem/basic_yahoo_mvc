using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace basic_yahoo_mvc.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            Console.WriteLine("Hola");
            return View();
        }
        public IActionResult Best_crypto()
        {
            List<List<string>> crypto_data = scrape_data();
            List<List<string>> toview_data = new List<List<string>>();

            foreach(var crypto in crypto_data)
            {
                if (crypto[2].StartsWith("+"))
                {
                    toview_data.Add(crypto);
                }
            }

            //enviando lista resultante
            ViewData["cryptos"] = toview_data;

            return View();
        }
        public IActionResult Worst_crypto()
        {
            List<List<string>> crypto_data = scrape_data();
            //Con LINQ
            List<List<string>> toview_data = crypto_data.Where(row => row[2].StartsWith("-")).ToList();

            ViewData["cryptos"] = toview_data;
            return View();
        }
        List<List<string>> scrape_data()
        {
            //Accediendo a url
            async Task<string> call_url(string fullUrl)
            {
                HttpClient client = new HttpClient();
                var response = await client.GetStringAsync(fullUrl);
                return response;
            }

            //parseando datos
            List<List<string>> parse_html(string html)
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                //XPath
                //var book_names = htmlDoc.DocumentNode.SelectNodes("//a[@class='pollAnswer__bookLink']");

                //LINQ
                var parsed_data = htmlDoc.DocumentNode.Descendants("tr")
                    .Where(node => node.GetAttributeValue("class", "").Contains("simpTblRow")).
                    ToList();

                List<List<string>> crypto_data = new List<List<string>>();
                foreach (HtmlNode crypto in parsed_data)
                {
                    string crypto_name = crypto.SelectNodes("td")
                        .Where(node => node.GetAttributeValue("aria-label", "")
                        .Contains("Name"))
                        .ToList()[0].InnerText;

                    string crypto_price = crypto.SelectNodes("td")
                        .Where(node => node.GetAttributeValue("aria-label", "")
                        .Contains("Price (Intraday)"))
                        .ToList()[0].FirstChild.InnerText;

                    string crypto_change = crypto.SelectNodes("td")
                        .Where(node => node.GetAttributeValue("aria-label", "")
                        .Contains("Change"))
                        .ToList()[0].FirstChild.FirstChild.InnerText;

                    string crypto_changep = crypto.SelectNodes("td")
                        .Where(node => node.GetAttributeValue("aria-label", "")
                        .Contains("% Change"))
                        .ToList()[0].FirstChild.FirstChild.InnerText;

                    crypto_data.Add(new List<string>() { crypto_name, crypto_price, crypto_change, crypto_changep });
                }

                return crypto_data;
            }

            string url = "https://finance.yahoo.com/crypto/?offset=0&count=100";
            var response = call_url(url).Result;
            List<List<string>> data = parse_html(response);
            return data;
        }
    }
}
