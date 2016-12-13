using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using ChatbotCore;
using IrcDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TwitchChatBot
{

    //Code from https://github.com/IrcDotNet/IrcDotNet/blob/develop/samples/IrcDotNet.Samples.TwitchChat/Program.cs
    public class TwitchChatClient : IChatbot
    {
        private string username = "nediarph";
        private string password = "oauth:4rf96d7d5aqfcye59qqswf9yr3m9b7";
        private string server = "irc.chat.twitch.tv";
        private string port = "6667";

        private TwitchIrcClient client;

        private INews newsSource;

        private ObservableCollection<string> msgQueue;
        private List<string> cmdList;


        public TwitchChatClient(INews news)
        {
            client = new TwitchIrcClient();
            newsSource = news;
            client.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
            client.Disconnected += IrcClient_Disconnected;
            client.Registered += IrcClient_Registered;
            
            msgQueue = new ObservableCollection<string>();
            cmdList = new List<string>();
            cmdList.Add("!news");

            msgQueue.CollectionChanged += delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    var tmp = msgQueue.First();
                    msgQueue.RemoveAt(0);

                    RecognizeCmd(tmp);
                }
            };
            
        }

        public void SendMessage(string msg)
        {
            client.SendRawMessage(msg);
        }

        public void Connect()
        {
            using (var registeredEvent = new ManualResetEventSlim(false))
            {
                using (var connectedEvent = new ManualResetEventSlim(false))
                {
                    client.Connected += (sender2, e2) => connectedEvent.Set();
                    client.Registered += (sender2, e2) => registeredEvent.Set();
                    client.Connect(server, false,
                        new IrcUserRegistrationInfo()
                        {
                            NickName = username,
                            Password = password,
                            UserName = username
                        });
                    if (!connectedEvent.Wait(10000))
                    {
                        Console.WriteLine("Connection to: "+ server  + " timed out.");
                        return;
                    }
                }
                Console.WriteLine("Now connected to: " + server);
                if (!registeredEvent.Wait(10000))
                {
                    Console.WriteLine("Could not register to: " + server);
                }

                client.Channels.Join("#nediarph");
            }
        }

        public bool IsConnected()
        {
            return client.IsConnected;
        }

        public bool IsRegistered()
        {
            return client.IsRegistered;
        }

        ~TwitchChatClient()
        {
            client.Disconnect();
        }

        #region Eventhandlers

        private void IrcClient_Disconnected(object sender, EventArgs e)
        {
           
        }

        private void IrcClient_Registered(object sender, EventArgs e)
        {

            client.LocalUser.NoticeReceived += IrcClient_LocalUser_NoticeReceived;
            client.LocalUser.MessageReceived += IrcClient_LocalUser_MessageReceived;
            client.LocalUser.JoinedChannel += IrcClient_LocalUser_JoinedChannel;
            client.LocalUser.LeftChannel += IrcClient_LocalUser_LeftChannel;
        }


        private void IrcClient_LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
        {
            var localUser = (IrcLocalUser)sender;

            e.Channel.UserJoined -= IrcClient_Channel_UserJoined;
            e.Channel.UserLeft -= IrcClient_Channel_UserLeft;
            e.Channel.MessageReceived -= IrcClient_Channel_MessageReceived;
            e.Channel.NoticeReceived -= IrcClient_Channel_NoticeReceived;

            Console.WriteLine("You left the channel {0}.", e.Channel.Name);
        }

        private void IrcClient_LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            var localUser = (IrcLocalUser)sender;

            e.Channel.UserJoined += IrcClient_Channel_UserJoined;
            e.Channel.UserLeft += IrcClient_Channel_UserLeft;
            e.Channel.MessageReceived += IrcClient_Channel_MessageReceived;
            e.Channel.NoticeReceived += IrcClient_Channel_NoticeReceived;

            Console.WriteLine("You joined the channel {0}.", e.Channel.Name);
        }


        private void IrcClient_Channel_UserLeft(object sender, IrcChannelUserEventArgs e)
        {
            var channel = (IrcChannel)sender;
            Console.WriteLine("[{0}] User {1} left the channel.", channel.Name, e.ChannelUser.User.NickName);
        }

        private void IrcClient_Channel_UserJoined(object sender, IrcChannelUserEventArgs e)
        {
            var channel = (IrcChannel)sender;
            Console.WriteLine("[{0}] User {1} joined the channel.", channel.Name, e.ChannelUser.User.NickName);
        }

        private void IrcClient_Channel_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            var channel = (IrcChannel)sender;

            Console.WriteLine("[{0}] Notice: {1}.", channel.Name, e.Text);
        }


        //Crux of the use: This is event that gets fired when a message is received from the channel.
        private void IrcClient_Channel_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            var channel = (IrcChannel)sender;
            if (e.Source is IrcUser)
            {
                // Read message.
                foreach (var cmd in cmdList)
                {
                    if (e.Text.Contains(cmd))
                        msgQueue.Add(e.Text);
                        
                }
                Console.WriteLine("[{0}]({1}): {2}", channel.Name, e.Source.Name, e.Text);
            }
            else
            {
                Console.WriteLine("[{0}]({1}) Message: {2}.", channel.Name, e.Source.Name, e.Text);
            }
        }
        
        private void IrcClient_LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            var localUser = (IrcLocalUser)sender;

            if (e.Source is IrcUser)
            {
                // Read message.
                Console.WriteLine("({0}): {1}.", e.Source.Name, e.Text);
            }
            else
            {
                Console.WriteLine("({0}) Message: {1}.", e.Source.Name, e.Text);
            }
        }
        private void IrcClient_LocalUser_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            var localUser = (IrcLocalUser)sender;
            Console.WriteLine("Notice: {0}.", e.Text);
        }
        #endregion

        public void RecognizeCmd(string cmdString)
        {
            if (cmdString.Contains("!news"))
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
            SendMessage(responseString);
        }
    }
}
