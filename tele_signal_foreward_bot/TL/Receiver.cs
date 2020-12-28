using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using tele_signal_foreward_bot.Common;
using tele_signal_foreward_bot.Config;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

namespace tele_signal_foreward_bot.TL
{
   

    class Receiver : BaseSingletone<Receiver>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      
        
        
        public string ForwardChannelName ;
        public int ForwardChannelId;
        public long ForwardChannelAccessHash; 
        
        /// <summary>
        /// 메세지 ID를 통해서 마지막으로 읽어들인 메세지번호를 체킹한다. 
        /// </summary>
        Dictionary<int, int> MessageMap { get; set; } = new Dictionary<int, int>();

        Dictionary<int, string> MessageCheckMap { get; set; } = new Dictionary<int, string>();

        Authentication Auth { get; set; }
        TelegramClient Client { get; set; }

        public Receiver() {  }
        public void SetAuth(Authentication auth) 
        {
            this.Auth = auth;
            if(this.Auth.IsAuthenticated)
            {
                this.Client = this.Auth.Client;
            }

            ForwardChannelName = Configs.Instance.channelname;
            if( string.IsNullOrEmpty(ForwardChannelName))
            {
                log.Error("Forward channel name unset.");
                throw new Exception("Forwad channel name unset.");
            }
        }

        public void Init()
        {
            var chats = GetChannels().Result;

            // 마지막메세지를 전송하지 않기위해 메세지 아이디를 업데이트 함. 
            if (Configs.Instance.lastmessage_send == false)
            {
                foreach (var o in chats)
                {
                    if (o.Left)
                        continue;

                    if (!MessageMap.ContainsKey(o.Id))
                        MessageMap.Add(o.Id, 0);

                    var messages = GetMessages(o.Id, o.AccessHash.Value).Result;
                    foreach (var message in messages)
                    {
                        if (message.Id > MessageMap[o.Id])
                        {
                            MessageMap[o.Id] = message.Id;
                        }
                    }
                }
            }
            else
            {
                foreach (var o in chats)
                {
                    if (o.Left)
                        continue;

                    // TODO : 테스트가 끝나면 지워야함 
                    if (o.Title == "Team IU")
                    {
                        continue;
                    }


                    if (o.Title == ForwardChannelName)
                    {
                        ForwardChannelId = o.Id;
                        ForwardChannelAccessHash = o.AccessHash.Value;
                        continue;
                    }


                    if (!MessageMap.ContainsKey(o.Id))
                        MessageMap.Add(o.Id, 0);
                }

                foreach (var o in chats)
                {
                    if (o.Left)
                        continue;

                  /*  // TODO : 테스트가 끝나면 지워야함 
                    if (o.Title == "Team IU")
                    {
                        continue;
                    }*/


                    if (o.Title == ForwardChannelName)
                    {
                        ForwardChannelId = o.Id;
                        ForwardChannelAccessHash = o.AccessHash.Value;
                        continue;
                    }


                    if (!MessageMap.ContainsKey(o.Id))
                        MessageMap.Add(o.Id, 0);

                    var messages = GetMessages(o.Id, o.AccessHash.Value, 1).Result;
                    foreach (var message in messages)
                    {
                        Forwarder.Instance.Send(messages, MessageMap, o, MessageCheckMap).Wait();
                    }
                }

            }
        }

        bool isReadyForward = false;
        public void Run()
        {
            while (true)
            {
                try
                {
                    var chats = GetChannels().Result;

                    foreach (var o in chats)
                    {
                        Console.WriteLine($"ChannelId : {o.Id}, Title : {o.Title}, AccessHash : {o.AccessHash}, UserName : {o.Username}");
                        
                        if (o.Left)
                            continue;
                        /*// TODO : 테스트가 끝나면 지워야함 
                        if (o.Title == "Team IU" )
                        {
                            continue;
                        }*/
                        

                        if (o.Title == ForwardChannelName)
                        {
                            ForwardChannelId = o.Id;
                            ForwardChannelAccessHash = o.AccessHash.Value;
                            isReadyForward = true;
                            continue;
                        }

                        if (!MessageMap.ContainsKey(o.Id))
                            MessageMap.Add(o.Id, 0);


                        if (isReadyForward)
                        {
                            var messages = GetMessages(o.Id, o.AccessHash.Value, 10).Result;
                            Forwarder.Instance.Send(messages, MessageMap, o, MessageCheckMap).Wait();
                        }
                        

                    }

                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Message : {ex.Message} \nStackTrace : {ex.StackTrace}");
                    
                }

                Thread.Sleep(2000);

            }

        }
        async Task<TLDialogs> GetDialogs()
        {
            //get user dialogs
            var dialogs = (TLDialogs)await Client.GetUserDialogsAsync();
            return dialogs;
        }
        async Task<List<TLChannel>> GetChannels()
        {
            var dialogs = await GetDialogs();
            var chat = dialogs.Chats
                .OfType<TLChannel>().ToList();

            return chat;
        }

        async Task<List<TLMessage>> GetMessages(int id, long accHash, int count = 5)
        {
            var tlAbsMessages = (TLChannelMessages)await Client.GetHistoryAsync(new TLInputPeerChannel { ChannelId = id, AccessHash = accHash }, 0, 0, 0, count);
            var tlChatMessage = tlAbsMessages.Messages.OfType<TLMessage>().OrderBy(a => a.Date).ToList();
            return tlChatMessage;
        }
    }
}
