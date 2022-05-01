using System;
using IrcDotNet;
using System.Threading;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using System.Net;
using Microsoft.Speech.Recognition.SrgsGrammar;
using System.IO;

namespace Miku
{
    class Bot
    {

        private static TwitchIrcClient ircClient;
        private static String ChannelToConnect = "#yohelloyukinon";
        private static bool isRunning = true;
        private static SpeechSynthesizer ss = new SpeechSynthesizer();

        static void Main(string[] args)
        {
            ss.Volume = 100;// от 0 до 100
            ss.Rate = 0;//от -10 до 10
            ss.SetOutputToDefaultAudioDevice();

            Console.Title = "Miku";
            Console.SetWindowSize(120, 40);
            Console.SetBufferSize(120, 400);

            Connect();
            JoinChannel();
            VoiceRecognition();
            Run();

        }



        private static void Run()
        {
            // Read commands from stdin until bot terminates.
            while (isRunning)
            {
                string s = Console.ReadLine();
                if (s.Length == 0)
                    continue;
                switch (s)
                {
                    case "!end":
                        ircClient.SendRawMessage("CAP END");
                        break;
                    case "!tags":
                        ircClient.SendRawMessage("CAP REQ :twitch.tv/tags");
                        break;
                    case "!list":
                        ircClient.SendRawMessage("CAP LIST");
                        break;
                    case "!n":
                        ircClient.SendRawMessage("NAMES #yohelloyukinon");
                        break;
                    case "!j":
                        ircClient.SendRawMessage("JOIN #lirik");
                        break;
                    case "!c":
                        System.Collections.Generic.IEnumerable<IrcUser> icu = ircClient.Users;
                        Console.WriteLine("Пользователей в чате: " + ircClient.Users.Count + "\nСписок:");
                        foreach (IrcUser i in icu)
                        {
                            Console.Write(i.NickName + " " + i.AwayMessage);
                        }
                        Console.WriteLine();
                        break;
                    case "!w":
                        string json = "";
                        using (WebClient wc = new WebClient())
                        {
                            json = wc.DownloadString("https://tmi.twitch.tv/group/user/yohelloyukinon/chatters");
                        }
                        string chatters = "chatter_count";
                        int startIndex = json.IndexOf(chatters) + chatters.Length + 3;
                        int endIndex = json.IndexOf(",", startIndex);
                        int length = endIndex - startIndex;
                        string count = json.Substring(startIndex, length);
                        Console.WriteLine(count);
                        break;
                    case "!u":
                        Console.WriteLine("!!!!!" + ircClient.Channels[0].Users.Count);
                        //Console.WriteLine("ID: " + ircClient.Channels[0].Users[1].);
                        break;
                    case "!e":
                        Stop();
                        break;
                    default:
                        break;
                }
            
            } 
        }

       
        public static void Connect()
        {
            ircClient = new TwitchIrcClient
            {
                FloodPreventer = new IrcStandardFloodPreventer(10, 100)
            };

            // register events
            ircClient.Connected += ircClient_Connected;
            ircClient.ConnectFailed += ircClient_ConnectFailed;
            ircClient.Disconnected += ircClient_Disconnected;
            ircClient.Registered += ircClient_Registered;
            ircClient.RawMessageReceived += ircClient_RawMessageReceived;
            ircClient.RawMessageSent += ircClient_RawMessageSent;
            
            //ircClient.Channel.MessageReceived += ircClient_Channel_MessageReceived;
            

            IrcRegistrationInfo iri = new IrcUserRegistrationInfo()
            {
                UserName = "yohelloyukinon",
                NickName = "yohelloyukinon",
                Password = "oauth:62gpjlwlyripeq5ssyybsz3o7g0n8x"
            };

            ircClient.Connect("irc.chat.twitch.tv", 6667, false, iri);
            Thread.Sleep(1000);
            
        }

        private static void JoinChannel()
        {
            ircClient.SendRawMessage("CAP REQ :twitch.tv/membership");
            //Thread.Sleep(1000);
            //ircClient.SendRawMessage("CAP REQ :twitch.tv/tags");
            
            Thread.Sleep(1000);
            ircClient.Channels.Join(ChannelToConnect);
            //Thread.Sleep(1000);
            

            //ircClient.LocalUser.JoinedChannel+= ircClient_UsersListReceived;
            //ircClient.ChannelListReceived += ircClient_ChannelUserCount;
        }

        private static void Stop()
        {
            var serverName = "Unknown";
            if (ircClient != null)
            {
                serverName = ircClient.ServerName;
                ircClient.Disconnect();
                ircClient.Quit();
                ircClient.Dispose();
            }
            Console.Out.WriteLine("Disconnected from '{0}'.", serverName);
            isRunning = false;
            Console.Beep();
        }

        private static void VoiceRecognition()
        {/*
            SpeechSynthesizer ss = new SpeechSynthesizer();
            ss.Volume = 100;// от 0 до 100
            ss.Rate = 0;//от -10 до 10
            ss.SetOutputToDefaultAudioDevice();

            //ss.SelectVoice("Microsoft Server Speech Text to Speech Voice (ja-JP, Haruka)");
            ss.SpeakAsync("дай модерку айди 1 5 6 4 9 8 3");

            SpeechRecognitionEngine sr = new SpeechRecognitionEngine();
            sr.SetInputToDefaultAudioDevice();//микрофон
            */
            GrammarBuilder grammarBuilder = new GrammarBuilder();
            grammarBuilder.Append(new Choices("мику, покажи время", "мику, выход", "дай модерку айди 1 5 6 4 9 8 3"));//добавляем используемые фразы
            Grammar gr1 = new Grammar(grammarBuilder);
            //sr.UnloadAllGrammars();
            //sr.LoadGrammar(gr);
            
            SpeechRecognitionEngine sr = new SpeechRecognitionEngine();
            sr.SetInputToDefaultAudioDevice();//микрофон
            string grammarPath = @"D:\Проги\Programs\Miku\Miku\Miku\Resources\";
            //Компилируем наше грамматическое правило в файл Маршруты.cfg
            FileStream fs = new FileStream(grammarPath + "Маршруты.cfg", FileMode.Create);
            SrgsGrammarCompiler.Compile(grammarPath + "Маршруты.grxml", (Stream)fs);
            fs.Close();

            Grammar gr = new Grammar(grammarPath + "Маршруты.cfg", "тест");
            sr.UnloadAllGrammars();
            //Загружаем скомпилированный файл грамматики
            sr.LoadGrammar(gr);
            //Thread.Sleep(2000);
            //sr.LoadGrammar(gr1);

            sr.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);//событие речь распознана
            sr.RecognizeAsync(RecognizeMode.Multiple);//начинаем распознование
        }

        


        

        private static void SendMessage(string message)
        {
            if (ircClient != null)
            {
                ircClient.LocalUser.SendMessage(ChannelToConnect, message);
                //ircClient.SendRawMessage(message);
            }
        }




        #region events

        private static void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            
            Console.WriteLine("Recognized phrase: " + e.Result.Text);
            ss.SpeakAsync(e.Result.Text);

            if (e.Result.Text.Contains("Мику"))
            {
                switch (e.Result.Text.Substring(5))
                {
                    case "дай модерку айди 1 5 6 4 9 8 3":
                        SendMessage(" тест");
                        
                        break;
                    case "покажи время":
                        SendMessage("Сейчас " + DateTime.Now.ToShortTimeString() + ".");
                        break;
                    case "выход":
                        Stop();
                        break;
                    default:
                        break;
                }
            }
            //Распознанная фраза
            string recoString = e.Result.Text;
            //Имя команды
            string cmdName = e.Result.Semantics["action"].Value.ToString();
            //Точка А маршрута
            string thing = e.Result.Semantics["thing"].Value.ToString();
            //Точка Б маршрута
            string preposition = e.Result.Semantics["preposition"].Value.ToString();
            string game = e.Result.Semantics["game"].Value.ToString();
            //Показываем сообщение
            //Console.WriteLine($"Маршрут от точки А: {pointA} до точки Б: {pointB}");
            SendMessage(cmdName + " " + thing + " " + preposition + " " + game);
            string json = "body: { channel: { status, Battlefield 3 } }";
            using (WebClient wc = new WebClient())
            {
                
                wc.Headers.Set(System.Net.HttpRequestHeader.Accept, "application/vnd.twitchtv.v5+json");
                wc.Headers.Set(System.Net.HttpRequestHeader.ContentType, "application/json");
                wc.Headers.Set(System.Net.HttpRequestHeader.Authorization, "OAuth 62gpjlwlyripeq5ssyybsz3o7g0n8x");//, Client-ID:e3qp2yazpapigqfm29ydrk6q2ef40h");
                wc.UploadString("https://api.twitch.tv/api/channels/yohelloyukinon/access_token?client_id=e3qp2yazpapigqfm29ydrk6q2ef40h", "PUT", json); //98331196

                //var request = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/channels/");
                //request.bod

            }

            }

        private static void ircClient_Connected(object sender, EventArgs e)
        {
            if (e != null)
                Console.WriteLine("Connected.");
        }

        private static void ircClient_ConnectFailed(object sender, IrcErrorEventArgs e)
        {
            if (e != null)
                Console.WriteLine("Connect failed: " + e.Error.Message);
        }

        private static void ircClient_Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected.");
        }

        private static void ircClient_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        { 
            if (e != null)
            {
               Console.WriteLine(e.RawContent);
                if (e.RawContent.Contains("user-id"))
                {
                    //Console.WriteLine("user-id: ----------------------------------------");
                    //ircClient.SendRawMessage("CAP LIST :twitch.tv/tags");
                }
            }
        }


        // NOT triggered
        private static void ircClient_LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            var localUser = (IrcLocalUser)sender;

            if (e.Source is IrcUser)
            {
                // Read message.
                Console.WriteLine("({0}): {1}", e.Source.Name, e.Text);
            }
            else
            {
                Console.WriteLine("({0}) Message: {1}", e.Source.Name, e.Text);
            }
            //Console.WriteLine("local user message received" + e.Text);
        }

        //triggered
        private static void ircClient_Channel_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            var channel = (IrcChannel)sender;
            if (e.Source is IrcUser)
            {
                // Read message.
                Console.WriteLine("[{0}]({1}): {2}", channel.Name, e.Source.Name, e.Text);
            }
            else
            {
                Console.WriteLine("[{0}]({1}) Message: {2}", channel.Name, e.Source.Name, e.Text);
            }


            if(e != null)
                {
                if (e.Text.Contains("!tog"))
                    SendMessage("http://www.webtoons.com/en/fantasy/tower-of-god/list?title_no=95");

                if (e.Text.Contains("!music"))
                    SendMessage("https://www.youtube.com/playlist?list=PLd-myY-TJBJd3yv-CDwowV9TngjIE4ntL");

                if (e.Text.Contains("!ll"))
                {
                    SendMessage("/me L\\n");
                    SendMessage("/me O");
                    SendMessage("/me V");
                    SendMessage("/me E");
                    SendMessage("/me \u2063 \\n");
                    SendMessage("/me L");
                    SendMessage("/me I");
                    SendMessage("/me V");
                    SendMessage("/me E");
                    SendMessage("/me !");
                }
            }
            /*
            System.Collections.Generic.IEnumerable<IrcUser> icu = ircClient.Users;
            int count = ircClient.Users.Count;
            
            Console.WriteLine("Пользователей в чате: " + count + "\nСписок:");
            foreach (IrcUser i in icu)
            {
                Console.Write(i.NickName + " "+ i.ServerName);
            }
            Console.WriteLine();
            */
        }

        // NOT triggered
        private static void ircClientLocalUser_MessageSent(object sender, IrcMessageEventArgs e)
        {
            Console.WriteLine("local user message sent" + e.Text);
        }

        //triggered
        private static void ircClient_RawMessageSent(object sender, IrcRawMessageEventArgs e)
        {

            //Console.WriteLine("raw message sent: " + e.RawContent.ToString());
            
        }

        //triggered
        private static void ircClient_Registered(object sender, EventArgs e)
        {
            Console.WriteLine("Registered.");


            ircClient.LocalUser.MessageReceived += ircClient_LocalUser_MessageReceived;
            ircClient.LocalUser.MessageSent += ircClientLocalUser_MessageSent;
            ircClient.LocalUser.JoinedChannel += ircClient_LocalUser_JoinedChannel;
            Console.WriteLine("Nickname: " + ircClient.LocalUser.NickName);

            ircClient.LocalUser.NoticeReceived += ircClient_LocalUser_NoticeReceived;
            ircClient.LocalUser.LeftChannel += IrcClient_LocalUser_LeftChannel;
        }

        //triggered
        private static void ircClient_LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            var localUser = (IrcLocalUser)sender;

            //e.Channel.UserJoined += IrcClient_Channel_UserJoined;
            //e.Channel.UserLeft += IrcClient_Channel_UserLeft;
            e.Channel.MessageReceived += ircClient_Channel_MessageReceived;
            e.Channel.NoticeReceived += IrcClient_Channel_NoticeReceived;
            e.Channel.UsersListReceived += ircClient_UsersListReceived;

            Console.WriteLine("You joined the channel {0}.", e.Channel.Name);

            System.Collections.Generic.IEnumerable<IrcUser> icu = ircClient.Users;
            int count = ircClient.Users.Count;
            Console.WriteLine("Пользователей в чате: " + count + "\nСписок:");
            foreach (IrcUser i in icu)
            {
                Console.Write(i.NickName + " ");
            }
        }


            

        private static void ircClient_UsersListReceived(object sender, EventArgs e)
        {
            //var localUser = (IrcLocalUser)sender;

            //Console.WriteLine("list received" );
            //Console.WriteLine("id of moobot:" + ircClient.Users[0].ServerName);
            //Console.WriteLine("!!!!!" + ircClient.Channels[1].Users.Count);
        }


        private static void IrcClient_Channel_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            Console.WriteLine("Notice: " + e.Text);
        }

        private static void IrcClient_LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
        {

        }

        private static void ircClient_LocalUser_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            Console.WriteLine("Local user Notice: " + e.Text);
        }
        
        #endregion
    }
}
