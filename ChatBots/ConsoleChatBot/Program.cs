using System;
using ChatbotCore;

namespace ConsoleChatBot
{
    class Program
    {
        static void Main(string[] args)
        {
           INews newsAggre = new NewssourcePh();
           IChatbot chatbot = new ConsoleChatbot(newsAggre);

            while (true)
            {
                string msg = Console.ReadLine();
                chatbot.RecognizeCmd(msg);
            }

        }
    }
}