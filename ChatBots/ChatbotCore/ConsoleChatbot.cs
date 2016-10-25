using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChatbotCore
{
    public class ConsoleChatbot : IChatbot
    {
        class dataObject
        {
            public string title { get; set; }
            public string url { get; set; }
        }
        private INews newsSource;
        public ConsoleChatbot(INews news)
        {
            newsSource = news;
        }
        public void RecognizeCmd(string cmdString)
        {
            if(cmdString.Contains("!news"))
                Respond(newsSource.getNews());
        }

        public void Respond(string responseString)
        {
            var responseObj = JsonConvert.DeserializeObject<JObject>(responseString);
            var resultsList = responseObj["results"].Children();

            int count = 0;
            foreach (var newsItem in resultsList)
            {

                Console.WriteLine(count+1 +".: " + newsItem["title"]);
                Console.WriteLine("Abstract: " + newsItem["abstract"]);
                Console.WriteLine("Read More: " + newsItem["url"]);
                count++;
                if (count == 5)
                    break;
            }
        }
    }
}