using System;
using System.Text;
using ChatbotCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleChatBot
{
    public class ConsoleChatbot : IChatbot
    {
        private INews newsSource;
        public ConsoleChatbot(INews news)
        {
            newsSource = news;
        }
        public void RecognizeCmd(string cmdString)
        {
            if(cmdString.Contains("!news"))
                Respond(CreateResponse(newsSource.getNews()));
        }

        public string CreateResponse(string newsString)
        {
            var responseObj = JsonConvert.DeserializeObject<JObject>(newsString);
            var resultsList = responseObj["results"].Children();
            var responseBuilder = new StringBuilder();

            int count = 1;
            foreach (var newsItem in resultsList)
            {
                responseBuilder.Append(count + ".: " + newsItem["title"] + Environment.NewLine);
                responseBuilder.Append("Abstract: " + newsItem["abstract"] + Environment.NewLine);
                responseBuilder.Append("Read More: " + newsItem["url"] + Environment.NewLine);
                count++;
                if (count > 5)
                    break;
            }

            return responseBuilder.ToString();
        }

        public void Respond(string responseString)
        {
            Console.WriteLine(responseString);
        }
    }
}