using System;
using System.Threading;
using IrcDotNet;

namespace TwitchChatBot
{
    public class TwitchChatClient
    {
        private string username = "nediarph";
        private string password = "oauth:4rf96d7d5aqfcye59qqswf9yr3m9b7";
        private string server = "irc.chat.twitch.tv";
        private string port = "6667";

        private TwitchIrcClient client;



        public TwitchChatClient()
        {
            client = new TwitchIrcClient();
            client.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
            client.Disconnected += IrcClient_Disconnected;
            client.Registered += IrcClient_Registered;

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

        ~TwitchChatClient()
        {
            client.Disconnect();
        }

        #region Eventhandlers


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

        private void IrcClient_Channel_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            var channel = (IrcChannel)sender;
            if (e.Source is IrcUser)
            {
                // Read message.
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


    }
}
