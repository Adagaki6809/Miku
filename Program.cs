using IrcDotNet;
using Newtonsoft.Json;
using System.Speech.Synthesis; 
using System.Speech.Recognition;
using System.Text;

namespace Miku
{
    class Bot
    {
        private static readonly HttpClient client = new HttpClient();
        private static TwitchIrcClient ircClient = new TwitchIrcClient
        {
            FloodPreventer = new IrcStandardFloodPreventer(10, 100)
        };
        private static bool isRunning = true;
        private static SpeechSynthesizer ss = new SpeechSynthesizer();
        private static string[] info = File.ReadAllLines($"{Directory.GetCurrentDirectory()}/twitch.txt");
        private static string host = info[0], channelToConnect = $"#{info[1]}", musicLink = info[4];
        static async Task Main(string[] args)
        {
            ss.Volume = 70; // от 0 до 100
            ss.Rate = 0; //от -10 до 10
            ss.SetOutputToDefaultAudioDevice();
            ss.SelectVoice("Microsoft Irina Desktop");
            Console.Title = "Miku";
            Console.SetWindowSize(120, 40);
            Console.SetBufferSize(120, 400);
            Connect();
            JoinChannel();
            //VoiceRecognition();
            await Run();
        }
        public static void Connect()
        {
            ircClient.Connected += ircClient_Connected;
            ircClient.ConnectFailed += ircClient_ConnectFailed;
            ircClient.Disconnected += ircClient_Disconnected;
            ircClient.Registered += ircClient_Registered;
            ircClient.RawMessageReceived += ircClient_RawMessageReceived;
            ircClient.RawMessageSent += ircClient_RawMessageSent;
            IrcRegistrationInfo iri = new IrcUserRegistrationInfo()
            {
                UserName = info[1],
                NickName = info[2],
                Password = info[3]
            };
            ircClient.Connect("irc.twitch.tv", 6667, false, iri);
            Thread.Sleep(3000);
        }

        private static void JoinChannel()
        {
            ircClient.Channels.Join(channelToConnect);
            Thread.Sleep(2000);
        }
        private static async Task Run()
        {
            while (isRunning)
            {
                string command = Console.ReadLine() ?? "";
                switch (command)
                {
                    case "!c":
                        GetChatters();
                        break;
                    case "!w":
                        string json = await client.GetStringAsync($"https://tmi.twitch.tv/group/user/{ircClient.LocalUser.UserName}/chatters");
                        dynamic? data = JsonConvert.DeserializeObject(json);
                        Console.WriteLine($"Пользователей в чате: {data?.chatter_count ?? -1}");
                        Console.WriteLine(String.Join(", ", ircClient.Users));
                        break;
                    case "!e":
                        Stop();
                        break;
                    default:
                        break;
                }
            } 
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
            Console.Out.WriteLine($"Отключено от {serverName}.");
            isRunning = false;
            Console.Beep();
        }

        private static void SendMessage(string message)
        {
            if (ircClient != null)
            {
                ircClient.LocalUser.SendMessage(channelToConnect, message);
            }
        }

        private static void GetChatters()
        {
            Console.WriteLine($"Пользователей в чате: {ircClient.Users.Count}");
            Console.WriteLine(String.Join(", ", ircClient.Users));
        }

        private static void VoiceRecognition()
        {
            /*
            SpeechRecognitionEngine sr = new SpeechRecognitionEngine();
            sr.SetInputToDefaultAudioDevice();//микрофон
            
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
            */
        }

        #region events
        private static void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            /*
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
            */
        }

        private static void ircClient_Connected(object? sender, EventArgs e)
        {
            Console.WriteLine("[Событие]Connected> Подключено");
        }

        private static void ircClient_ConnectFailed(object? sender, IrcErrorEventArgs e)
        {
            Console.WriteLine($"[Событие]ConnectFailed> Подключение не удалось: {e?.Error.Message}");
        }

        private static void ircClient_Disconnected(object? sender, EventArgs e)
        {
            Console.WriteLine("[Событие]Disconnected> Отключено.");
        }

        private static void ircClient_RawMessageReceived(object? sender, IrcRawMessageEventArgs e)
        { 
            //Console.WriteLine($"[Событие]RawMessageReceived> Сырой вид: {e.RawContent}");
        }

        // NOT triggered
        private static void ircClient_LocalUser_MessageReceived(object? sender, IrcMessageEventArgs e)
        {
            Console.WriteLine($"[Событие]LocalUser_MessageReceived> {e.Source.Name}: {e.Text}");
        }
        //triggered
        private static void ircClient_Channel_MessageReceived(object? sender, IrcMessageEventArgs e)
        {
            Console.WriteLine($"[Событие]Channel_MessageReceived> {e.Source.Name}: {e.Text}");
            if(e != null)
            {
                if (e.Text.Contains("!tog"))
                    SendMessage("http://www.webtoons.com/en/fantasy/tower-of-god/list?title_no=95");

                if (e.Text.Contains("!music"))
                    SendMessage(musicLink);

                if (e.Text.Contains("!ll"))
                {
                    SendMessage("/me L");
                    SendMessage("/me O");
                    SendMessage("/me V");
                    SendMessage("/me E");
                    SendMessage("/me L");
                    SendMessage("/me I");
                    SendMessage("/me V");
                    SendMessage("/me E");
                    SendMessage("/me !");
                }
                if (e.Source.Name != info[1])
                {
                    ss.SpeakAsync(e.Text);
                }
                var data = new Dictionary<string, string>
                {
                    { "u", e.Source.Name },
                    { "m", e.Text }
                };
                var jsonData = JsonConvert.SerializeObject(data);
                var contentData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                client.PostAsync($"{host}/chat", contentData);
            }
        }

        // NOT triggered
        private static void ircClient_LocalUser_MessageSent(object? sender, IrcMessageEventArgs e)
        {
            Console.WriteLine($"[Событие]LocalUser_MessageSent> {e.Text}");
        }

        //triggered
        private static void ircClient_RawMessageSent(object? sender, IrcRawMessageEventArgs e)
        {
            //Console.WriteLine($"[Событие]RawMessageSent> Сырой вид: {e.RawContent}");
        }

        //triggered
        private static void ircClient_Registered(object? sender, EventArgs e)
        {
            Console.WriteLine("[Событие]Registered> Зарегистрирован.");
            Console.WriteLine($"Никнейм локального пользователя: {ircClient.LocalUser.NickName}");
            ircClient.LocalUser.MessageReceived += ircClient_LocalUser_MessageReceived;
            ircClient.LocalUser.MessageSent += ircClient_LocalUser_MessageSent;
            ircClient.LocalUser.JoinedChannel += ircClient_LocalUser_JoinedChannel;
            ircClient.LocalUser.NoticeReceived += ircClient_LocalUser_NoticeReceived;
            ircClient.LocalUser.LeftChannel += IrcClient_LocalUser_LeftChannel;
        }

        //triggered
        private static void ircClient_LocalUser_JoinedChannel(object? sender, IrcChannelEventArgs e)
        {
            Console.WriteLine($"[Событие]LocalUser_JoinedChannel> {e.Channel}");
            Console.WriteLine($"Вы присоединились к каналу {e.Channel.Name}.");
            GetChatters();
            e.Channel.MessageReceived += ircClient_Channel_MessageReceived;
            e.Channel.NoticeReceived += IrcClient_Channel_NoticeReceived;
            e.Channel.UsersListReceived += ircClient_UsersListReceived;
        }
        private static void ircClient_UsersListReceived(object? sender, EventArgs e)
        {
            Console.WriteLine($"[Событие]UsersListReceived> {e}");
        }
        private static void IrcClient_Channel_NoticeReceived(object? sender, IrcMessageEventArgs e)
        {
            Console.WriteLine($"[Событие]]Channel_NoticeReceived> {e.Text}");
        }
        private static void IrcClient_LocalUser_LeftChannel(object? sender, IrcChannelEventArgs e)
        {
            Console.WriteLine($"[Событие]LocalUser_LeftChannel> {e.Channel}");
        }
        private static void ircClient_LocalUser_NoticeReceived(object? sender, IrcMessageEventArgs e)
        {
            Console.WriteLine($"[Событие]LocalUser_NoticeReceived> {e.Text}");
        }
        #endregion
    }
}
