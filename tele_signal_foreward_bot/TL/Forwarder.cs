using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using tele_signal_foreward_bot.Common;
using TeleSharp.TL;
using TeleSharp.TL.Upload;
using TLSharp.Core;
using TLSharp.Core.Utils;

namespace tele_signal_foreward_bot.TL
{
    class Forwarder : BaseSingletone<Forwarder>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        Authentication Auth { get; set; }
        TelegramClient Client { get; set; }

        public Forwarder() { }
        public void SetAuth(Authentication auth)
        {
            this.Auth = auth;
            if (this.Auth.IsAuthenticated)
            {
                this.Client = this.Auth.Client;
            }

        }
        
        public async Task<Task> Send(List<TLMessage> messages, Dictionary<int, int> MessageMap, TLChannel channel, Dictionary<int, string> MessageCheckMap)
        {
            var config = Config.Configs.Instance;
            foreach (var message in messages)
            {
                var messageId = message.Id;
                
                if(messageId > MessageMap[channel.Id])
                {
                    string msg = "";
                    
                    
                    
                    if(message.Media != null)
                    {
                        if (message.Media is TeleSharp.TL.TLMessageMediaPhoto)
                        {
                            var photoMsg = message.Media as TeleSharp.TL.TLMessageMediaPhoto;
                            var photo = ((TLPhoto)photoMsg.Photo);
                            
                            var photoSize = photo.Sizes.OfType<TLPhotoSize>().OrderByDescending(m => m.Size).FirstOrDefault();
                            TLFileLocation tf = (TLFileLocation)photoSize.Location;
                            string fileName = $"{photo.Id}.jpg";
                            string path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                            try
                            {

                                /*
                                 var mb = 1048576;
                                var upperLimit = (int)Math.Pow(2, Math.Ceiling(Math.Log(fileInfo.Size, 2))) * 4;
                                var limit = Math.Min(mb, upperLimit);

                                var currentOffset = 0;
                                using (var fs = File.OpenWrite(filename))
                                {
                                    while (currentOffset < fileInfo.Size)
                                    {
                                        file = m_telegramClient.GetFile(fileLocation, limit, currentOffset).ConfigureAwait(false).GetAwaiter().GetResult();
                                        fs.Write(file.Bytes, currentOffset, file.Bytes.Length);
                                        currentOffset += file.Bytes.Length;
                                    }

                                    fs.Close();
}
                                 */

                                var mb = 1048576;
                                var upperLimit = (int)Math.Pow(2, Math.Ceiling(Math.Log(photoSize.Size, 2))) * 4;
                                var limit = Math.Min(mb, upperLimit);
                                var currentOffset = 0;

                                using (var fs = File.OpenWrite(path))
                                {
                                    while (currentOffset < photoSize.Size)
                                    {
                                        TLFile file = Client.GetFile(new TLInputFileLocation { LocalId = tf.LocalId, Secret = tf.Secret, VolumeId = tf.VolumeId }, limit, currentOffset).ConfigureAwait(false).GetAwaiter().GetResult();
                                        fs.Write(file.Bytes, currentOffset, file.Bytes.Length);
                                        currentOffset += file.Bytes.Length;
                                    }

                                    fs.Close();
                                }
                                
                                /*var resFile2 = await Client.GetFile(tf as TLInputFileLocation, Math.Min(1024 * 256, photoSize.Size));
                                var resFile = await Client.GetFile(new TLInputFileLocation { LocalId = tf.LocalId, Secret = tf.Secret, VolumeId = tf.VolumeId }, Math.Min(1024 * 256, photoSize.Size));
                                File.WriteAllBytes(path, resFile.Bytes);*/
                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine($" getFile (size:{photoSize.Size}) : {ex.Message} \n {ex.StackTrace}");
                                continue;
                            }






                            //var resFile = await Client.GetFile(new TLInputFileLocation { LocalId = fl.LocalId, Secret = fl.Secret, VolumeId = fl.VolumeId }, photoSize.Size);

                            /*ar cachedPhotos = photo.Sizes.OfType<TLPhotoCachedSize>().ToList();
                            foreach(var o in cachedPhotos)
                            {
                                Console.WriteLine($"");
                            }*/
                            // (((TLMessageMediaPhoto)message.Media).Photo as TLPhoto).
                            var caption = ((TLMessageMediaPhoto)message.Media).Caption;
                            
                            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                            DateTime dt = dtDateTime.AddSeconds(message.Date);

                            if (!string.IsNullOrEmpty(caption))
                            {
                                Console.WriteLine($"photo msg received. [ {caption} ]  msg_time [{dt:yyyy-MM-dd HH:mm:ss}]");
                                msg = caption;
                            }
                            else
                            {
                                Console.WriteLine($"photo msg received but caption empty. msg_time [{dt:yyyy-MM-dd HH:mm:ss}]");
                                
                            }
                            
                            var fileResult = /*(TLInputFile)*/await Client.UploadFile(fileName, new StreamReader(path));
                            Console.WriteLine($"{Receiver.Instance.ForwardChannelId} // {Receiver.Instance.ForwardChannelAccessHash}");
                            var updates = await Client.SendUploadedPhoto(new TLInputPeerChannel() { ChannelId = Receiver.Instance.ForwardChannelId, AccessHash = Receiver.Instance.ForwardChannelAccessHash },
                                fileResult,
                                msg);

                            /*var tlUpdate = updates as TLUpdateChannel;

                            if(updates is TLUpdateNewMessage _msg)
                            {
                                if ( _msg.Message is TLMessage tlmsg)
                                {
                                    if( tlmsg.Media != null)
                                    {
                                        Console.WriteLine($"{tlmsg.Id}");
                                    }
                                }
                            }*/


                        }



                    }
                    else
                    {
                        DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                        DateTime dt = dtDateTime.AddSeconds(message.Date);

                        Console.WriteLine($" new msg received . [{channel.Title}] from [{channel.Username}]. msg [{ message.Message}] msg_time [{dt:yyyy-MM-dd HH:mm:ss}]");
                        msg = message.Message;
                        if (!string.IsNullOrEmpty(msg))
                        {
                            msg += $"\n\n FROM {channel.Title}";
                            await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = Receiver.Instance.ForwardChannelId, AccessHash = Receiver.Instance.ForwardChannelAccessHash }, msg);
                            Console.WriteLine($"Message Send Success From [{channel.Title}]. \n Message >>>> {msg}");
                        }

                        /*if (channel.Title.ToLower().Contains("rose premium"))
                        {
                            msg += $"\n\n FROM Rose";
                            await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = Receiver.Instance.ForwardChannelId, AccessHash = Receiver.Instance.ForwardChannelAccessHash }, msg);
                            Console.WriteLine($"Message Send Success From [{channel.Title}]. \n Message >>>> {msg}");
                        }
                        else
                        {
                            // 필터링
                            foreach (var str in config.channelKeywords["common"])
                            {
                                if (msg != null && msg.Contains(str))
                                {
                                    msg += $"\n\n FROM {channel.Title}";
                                    await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = Receiver.Instance.ForwardChannelId, AccessHash = Receiver.Instance.ForwardChannelAccessHash }, msg);
                                    Console.WriteLine($"Message Send Success From [{channel.Title}]. \n Message >>>> {msg}");
                                }
                            }

                        }*/

                    }

                    
                    
                    // 메세지 번호 업데이트
                    MessageMap[channel.Id] = messageId;
                    if (MessageCheckMap.ContainsKey(channel.Id))
                    {
                        MessageCheckMap[channel.Id] = msg;
                    }
                    else
                    {
                        MessageCheckMap.Add(channel.Id, msg);
                    }

                    if (string.IsNullOrEmpty(msg))
                    {
                        continue;
                    }


                  

                    

                }

            }

            return Task.CompletedTask;
        }
    }
}
