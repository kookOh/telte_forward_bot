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
    class MsgForward : BaseSingletone<MsgForward>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Authentication Auth { get; set; }
        TelegramClient Client { get; set; }

        public MsgForward() { }
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
                if( messageId > MessageMap[channel.Id])
                {
                    string caption = GetCaption(message, channel);

                    if (message.Media != null)
                    {
                        if (message.Media is TeleSharp.TL.TLMessageMediaPhoto)
                        {
                            string fileName = SavePhoto(message.Media as TeleSharp.TL.TLMessageMediaPhoto);
                            
                            if(fileName != "")
                            {
                                var fileResult = GetFile(fileName).Result;
                                // 중복전송 방지
                                if ( caption.Contains(MessageCheckMap[channel.Id]) 
                                     || MessageCheckMap[channel.Id].Contains(caption))
                                {
                                    
                                }
                                else
                                {
                                    var updates = await Client.SendUploadedPhoto(new TLInputPeerChannel() { ChannelId = Receiver.Instance.ForwardChannelId, AccessHash = Receiver.Instance.ForwardChannelAccessHash },
                                    fileResult,
                                    caption);
                                }

                            }
                            else
                            {
                                await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = Receiver.Instance.ForwardChannelId, AccessHash = Receiver.Instance.ForwardChannelAccessHash }, caption);
                            }
                        }
                    }
                    else
                    {
                        await Client.SendMessageAsync(new TLInputPeerChannel() { ChannelId = Receiver.Instance.ForwardChannelId, AccessHash = Receiver.Instance.ForwardChannelAccessHash }, caption);
                    }

                    MessageMap[channel.Id] = messageId;
                    if (MessageCheckMap.ContainsKey(channel.Id))
                    {
                        MessageCheckMap[channel.Id] = caption;
                    }
                    else
                    {
                        MessageCheckMap.Add(channel.Id, caption);
                    }
                }


            }
                
            return Task.CompletedTask;
        }
        /// <summary>
        /// 텔레그램에 보낼 사진파일 준비단계. 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        async Task<TLAbsInputFile> GetFile(string fileName) 
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            var fileResult = /*(TLInputFile)*/await Client.UploadFile(fileName, new StreamReader(path));
            return fileResult; 
        }

        /// <summary>
        /// 텔레그램 메세지에 있는 사진 파일 저장. 
        /// </summary>
        /// <param name="photoMsg"></param>
        /// <returns></returns>
        string SavePhoto(TeleSharp.TL.TLMessageMediaPhoto photoMsg)
        {
            var photo = ((TLPhoto)photoMsg.Photo);
            var photoSize = photo.Sizes.OfType<TLPhotoSize>().OrderByDescending(m => m.Size).FirstOrDefault();
            TLFileLocation tf = (TLFileLocation)photoSize.Location;
            string fileName = $"{photo.Id}.jpg";
            string path = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            try
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($" getFile (size:{photoSize.Size}) : {ex.Message} \n {ex.StackTrace}");
                return "";
                
            }

            return fileName;
        }

        string GetCaption(TLMessage msg, TLChannel channel)
        {
            string caption = "";
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            DateTime dt = dtDateTime.AddSeconds(msg.Date);
            if (msg.Media != null)
            {
                caption = ((TLMessageMediaPhoto)msg.Media).Caption;
                
                if(!string.IsNullOrEmpty(caption))
                    caption += $"\n\n FROM {channel.Title}";
            }
            else
            {
                caption = msg.Message;
                caption += $"\n\n FROM {channel.Title}";
            }

            return caption;
        }
        void UpdateMessageMap(Dictionary<int, int> MessageMap, Dictionary<int, string> MessageCheckMap, string msg, int msgNum)
        {
            
        }

    }
}
