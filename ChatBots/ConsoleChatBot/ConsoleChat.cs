using ChatbotCore;
using System;

namespace ConsoleChatBot
{
    class ConsoleChat : IChat
    {
        public void HandleIncomingMessage(string message)
        {
            throw new System.NotImplementedException();
        }

        public void sendMessage(string message)
        {
           Console.WriteLine(message);
        }
    }
}
