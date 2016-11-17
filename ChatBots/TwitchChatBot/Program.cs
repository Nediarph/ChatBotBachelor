using System;
using IrcDotNet;

namespace TwitchChatBot
{
    class Program
    {
        //Using the IrcDotNet IRC client library.
        //Code based on sample from here: https://github.com/IrcDotNet/IrcDotNet/blob/develop/samples/IrcDotNet.Samples.TwitchChat/Program.cs
        static void Main(string[] args)
        {
            var chatClient = new TwitchChatClient();
            chatClient.Connect();

            Console.ReadKey();


        }
    }
}
