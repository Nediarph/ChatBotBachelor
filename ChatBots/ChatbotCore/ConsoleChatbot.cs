using System;

namespace ChatbotCore
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
            if(cmdString.Contains("!test"))
                Console.WriteLine(newsSource.getNews());
        }

        public void Respond(string responseString)
        {
            //JSON.parse(responseString);
        }
    }
}