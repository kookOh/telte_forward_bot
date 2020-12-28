using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Updates;
using TLSharp.Core;
using TLSharp.Core.Network;

namespace tele_signal_foreward_bot.TL
{
    class Example
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        const int apiId = 546613;
        const string apiHash = "d01286377172be46a4d422fde6bd7e1f";

        int sendChatId;
        long sendAccessHash;
        // 546613
        // d01286377172be46a4d422fde6bd7e1f

        List<string> notFwdKeyword = new List<string>();
        List<string> fwdKeyword = new List<string>();

        public Example()
        {
            test_getChannels();
            fwdKeyword.Add("TP");
            fwdKeyword.Add("stop-loss");
            fwdKeyword.Add("SL");
            fwdKeyword.Add("Entry Zone");
        }

        public async void test_getChannels()
        {
            var client = new TelegramClient(apiId, apiHash, dcIpVersion: DataCenterIPVersion.OnlyIPv4);
            //client.ConnectAsync();
            await client.ConnectAsync();
            var hash = await client.SendCodeRequestAsync("8201083989681");
            Console.WriteLine("Code: ");
            
            var code =  Console.ReadLine();
            var user = await client.MakeAuthAsync("8201083989681", hash, code);

            

            //get user dialogs
            var dialogs = (TLDialogs)await client.GetUserDialogsAsync();

           
            //find channel by title
            var chat = dialogs.Chats
                .OfType<TLChannel>().ToList();

            Dictionary<int, int> MessageMap = new Dictionary<int, int>();
            // TODO : Last MessageId Config파일로 설정해서 가져오기. 

          
            Console.WriteLine("### 채팅방 리스트 출력 ###");
            foreach (var o in chat)
            {
                Console.WriteLine($"ChannelId : {o.Id}, Title : {o.Title}, AccessHash : {o.AccessHash}, UserName : {o.Username}");
                if(o.Title == "Team IU Test")
                {
                    sendChatId = o.Id;
                    sendAccessHash = o.AccessHash.Value;
                }
            }

             
            while (true)
            {
                dialogs = (TLDialogs)await client.GetUserDialogsAsync();
                chat = dialogs.Chats
                .OfType<TLChannel>().ToList();

                foreach (var o in chat)
                {
                    Console.WriteLine($"ChannelId : {o.Id}, Title : {o.Title}, AccessHash : {o.AccessHash}, UserName : {o.Username}");
                    if (o.Left)
                        continue;

                    if (o.Title.Contains("Team IU"))
                        continue;

                    if(!MessageMap.ContainsKey(o.Id))
                        MessageMap.Add(o.Id, 0);

                    try
                    {
                        //var tlAbsMessages = await client.GetHistoryAsync(new TLInputPeerChat { ChatId = o.Id }, 0, -1, 50);
                        
                        var tlAbsMessages  = (TLChannelMessages)await client.GetHistoryAsync(new TLInputPeerChannel { ChannelId = o.Id, AccessHash = o.AccessHash.Value }, 0, 0, 0, 5);
                        var tlChatMessage = tlAbsMessages.Messages.OfType<TLMessage>().OrderBy(a=> a.Date);
                        
                        foreach (var tlAbsMessage in tlChatMessage)
                        {
                            var message = (TLMessage)tlAbsMessage;
                            var messageID = message.Id;
                            if(messageID > MessageMap[o.Id]) 
                            {
                                string msg = null;

                             
                             
                                if(message.Media != null)
                                {
                                    if ( message.Media is  TeleSharp.TL.TLMessageMediaPhoto )
                                    {
                                        // (((TLMessageMediaPhoto)message.Media).Photo as TLPhoto).
                                        var caption = ((TLMessageMediaPhoto)message.Media).Caption;
                                        if (!string.IsNullOrEmpty(caption))
                                        {
                                            Console.WriteLine($"photo msg received. [ {caption} ]");
                                            msg = caption;
                                        }
                                        
                                    }
                                }
                                else
                                {
                                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                    DateTime dt = dtDateTime.AddSeconds(message.Date);

                                    Console.WriteLine($" new msg received . [{o.Title}] from [{o.Username}]. msg [{ message.Message}] time [{dt:yyyy-MM-dd HH:mm:ss}]");
                                    msg = message.Message;
                                }

                                // 포워딩할 키워드에 포함되고, 포워딩하지 않을 키워드에 포함되지 않는경우.
                                // 포워딩할 키워드 필터링 후 메세지 전송 
                                if (isKeyword(msg) && ! isNonKeyword(msg))
                                {
                                    msg += $"\n\n FROM {o.Title}";
                                    await client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = sendChatId, AccessHash = sendAccessHash }, msg);
                                }

                                // message Id Update. 
                                MessageMap[o.Id] = messageID;
                                
                                
                            }
                            else
                            {
                                //Console.WriteLine($" no new messages > {message.Id}");
                            }
                        }

                    }catch (Exception ex)
                    {
                        Console.WriteLine($"ERR ! {ex.Message}\n Trace : {ex.StackTrace}");
                        continue;
                    }

                    Thread.Sleep(1000);
                }
            }







            


             
            //Console.WriteLine("success ");
        }

        bool isKeyword(string message)
        {
            if (message == null)
                return false;

            foreach (var str in fwdKeyword)
            {
                if (message.Contains(str))
                {
                    return true; 
                }
            }
            return false;
        }

        bool isNonKeyword(string message)
        {
            if (message == null)
                return false;

            foreach (var str in notFwdKeyword)
            {
                if (message.Contains(str))
                {
                    return true; 
                }
            }

            return false;
        }
    }
}
