using System;
using ChatbotCore;
using IrcDotNet;

namespace TwitchChatBot
{
    class Program
    {
        //Using the IrcDotNet IRC client library.
        //Code based on sample from here: https://github.com/IrcDotNet/IrcDotNet/blob/develop/samples/IrcDotNet.Samples.TwitchChat/Program.cs
        static void Main(string[] args)
        {
            var nytimes = new NYTimes();
            var chatClient = new TwitchChatClient(nytimes);
            chatClient.Connect();

            Console.ReadKey();


        }
    }
}
