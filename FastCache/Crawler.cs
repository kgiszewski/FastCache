// Be sure to include these namespaces
using System.Net;
using System.IO;
using System;
using System.Collections.Generic;
using System.Xml;

using HtmlAgilityPack;

namespace FastCache
{
    public class Crawler
    {
        private List<string> urlList = new List<string>();
        private string _rootDomain;

        public void Crawl(string rootDomain){
            _rootDomain = rootDomain;

            XmlDocument config = new XmlDocument();
            AddGetPageLinks(_rootDomain);
        }

        private void AddGetPageLinks(string page){
            WebRequest request = WebRequest.Create(page);
            WebResponse response = request.GetResponse();

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseData = responseReader.ReadToEnd();

            HtmlDocument document = new HtmlDocument();

            try
            {
                document.LoadHtml(responseData);

                foreach (HtmlNode link in document.DocumentNode.SelectNodes("//a"))
                {
                    if (link != null)
                    {
                        string href=link.Attributes["href"].Value;

                        string extension = Path.GetExtension(href);

                        if (!String.IsNullOrEmpty(extension) && extension.Contains("."))
                        {
                            extension = extension.Substring(1);
                        }

                        if (
                            !href.StartsWith("http") &&
                            !href.StartsWith("#") &&
                            !urlList.Contains(href) &&
                            href != "" &&
                            !href.Contains("javascript")
                           )
                        {
                            urlList.Add(href);
                            Console.WriteLine("Crawling=>" + href);
                            AddGetPageLinks(_rootDomain + href);
                        }
                        else
                        {
                            Console.WriteLine("Rejected=>"+href);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Rejected=>" + e.Message);
            }
        }                
    }
}
            
        
