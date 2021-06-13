using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BasicExtension;
using SearchConnpassLineBot.Models;

namespace SearchConnpassLineBot.Parsers
{
    public class ConnpassParser
    {
        public ConnpassParser()
        {
            _httpClient = new HttpClient();
        }

        public List<ConnpassEventModel> Parse(DateTime dateTime, List<string> keywords)
        {
            string baseUrl = CreateBaseUrl(dateTime, keywords);
            List<ConnpassEventModel> connpassEventModels = new List<ConnpassEventModel>();
            ParseConnpassEventModels(baseUrl, 1, ref connpassEventModels);

            return connpassEventModels;
        }

        private string CreateBaseUrl(DateTime dateTime, List<string> keywords)
        {
            StringBuilder sb = new StringBuilder("https://connpass.com/search/?page={0}");

            if (keywords.Count > 0)
            {
                string searchQuery = keywords.Select(t => HttpUtility.UrlEncode(t)).ToStringJoin("+");
                sb.Append($"&q={searchQuery}");
            }

            string strDateTime = HttpUtility.UrlEncode(dateTime.ToString("yyyy/MM/dd"));
            sb.Append($"&start_from={strDateTime}&start_to={strDateTime}");

            return sb.ToString();
        }

        private void ParseConnpassEventModels(string baseUrl, int page,
            ref List<ConnpassEventModel> connpassEventModels)
        {
            string url = string.Format(baseUrl, page);
            HttpResponseMessage httpResponseMessage = _httpClient.GetAsync(url).Result;
            string html = httpResponseMessage.Content.ReadAsStringAsync().Result;
            IHtmlDocument htmlDocument = new HtmlParser().ParseDocument(html);

            IHtmlCollection<IElement> elements = htmlDocument.GetElementsByClassName("event_list");
            foreach (Element element in elements)
            {
                ConnpassEventModel ConnpassEventModel = ParseConnpassEventModel(element);
                connpassEventModels.Add(ConnpassEventModel);
            }

            int eventSumCount = ParseEventSumCount(htmlDocument);
            if (connpassEventModels.Count < eventSumCount)
            {
                ParseConnpassEventModels(baseUrl, page + 1, ref connpassEventModels);
            }
        }

        private int ParseEventSumCount(IHtmlDocument htmlDocument)
        {
            string value = htmlDocument.GetElementsByClassName("main_h2").First()
                .InnerHtml.Replace(",", "");
            string strEventSumCount = Regex.Replace(value, @"[^0-9]", "");

            return int.Parse(strEventSumCount);
        }

        private ConnpassEventModel ParseConnpassEventModel(Element element)
        {
            ConnpassEventModel connpassEventModel = new ConnpassEventModel();

            IElement scheduleElement = element.GetElementsByClassName("event_schedule_area").First();

            connpassEventModel.StartedAt = (DateTime)scheduleElement.GetElementsByClassName("dtstart").First()
                .GetElementsByTagName("span").First().GetAttribute("title")
                .Replace("Z", "").ToDateTime("yyyy-MM-ddTHH:mm:ss").Value.AddHours(9);

            connpassEventModel.EndedAt = (DateTime)scheduleElement.GetElementsByClassName("dtend").First()
                .GetElementsByTagName("span").First().GetAttribute("title")
                .Replace("Z", "").ToDateTime("yyyy-MM-ddTHH:mm:ss").Value.AddHours(9);

            IElement titleElement = element.GetElementsByClassName("event_detail_area").First()
                .GetElementsByClassName("event_inner").First()
                .GetElementsByClassName("event_title").First()
                .GetElementsByTagName("a").First();

            connpassEventModel.Title = titleElement.InnerHtml;
            connpassEventModel.Url = titleElement.GetAttribute("href");

            return connpassEventModel;
        }

        private readonly HttpClient _httpClient;
    }
}
