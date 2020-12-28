using OpenTl.ClientApi;
using OpenTl.Schema;
using OpenTl.Schema.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tele_signal_foreward_bot.OpenTL
{
    class Example
    {
        protected int apiId = 546613;
        protected string apiHash = "d01286377172be46a4d422fde6bd7e1f";
        string phone = "8201093960641";
        public string ForwardChannelName;
        public int ForwardChannelId;
        public long ForwardChannelAccessHash;
        public Example()
        {
            Test1();
           
        }

         async void Test1()
        {
            var settings = new FactorySettings
            {
                AppHash = apiHash,
                AppId = apiId,
                ServerAddress = "149.154.167.50",
                ServerPort = 443,
                SessionTag = $"session", // by defaut
                ServerPublicKey = "-----BEGIN RSA PUBLIC KEY-----\n" +
                "MIIBCgKCAQEAwVACPi9w23mF3tBkdZz+zwrzKOaaQdr01vAbU4E1pvkfj4sqDsm6\n" +
                "lyDONS789sVoD/xCS9Y0hkkC3gtL1tSfTlgCMOOul9lcixlEKzwKENj1Yz/s7daS\n" +
                "an9tqw3bfUV/nqgbhGX81v/+7RFAEd+RwFnK7a+XYl9sluzHRyVVaTTveB2GazTw\n" +
                "Efzk2DWgkBluml8OREmvfraX3bkHZJTKX4EQSjBbbdJ2ZXIsRrYOXfaA+xayEGB+\n" +
                "8hdlLmAjbCVfaigxX0CDqWeR1yFL9kwd9P0NsZRPsmoqVwMbMu7mStFai6aIhc3n\n" +
                "Slv8kg9qv1m6XHVQY3PnEw+QQtqSIXklHwIDAQAB\n" +
                "-----END RSA PUBLIC KEY-----"
                ,
                Properties = new ApplicationProperties()
                {
                    AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(), // You can leave as in the example
                    DeviceModel = "PC", // You can leave as in the example
                    LangCode = "ko", // You can leave as in the example
                    LangPack = "tdesktop", // You can leave as in the example
                    SystemLangCode = "ko", // You can leave as in the example
                    SystemVersion = "Win 10 Pro" // You can leave as in the example
                }

                /*
                 -----BEGIN RSA PUBLIC KEY-----
                MIIBCgKCAQEAwVACPi9w23mF3tBkdZz+zwrzKOaaQdr01vAbU4E1pvkfj4sqDsm6
                lyDONS789sVoD/xCS9Y0hkkC3gtL1tSfTlgCMOOul9lcixlEKzwKENj1Yz/s7daS
                an9tqw3bfUV/nqgbhGX81v/+7RFAEd+RwFnK7a+XYl9sluzHRyVVaTTveB2GazTw
                Efzk2DWgkBluml8OREmvfraX3bkHZJTKX4EQSjBbbdJ2ZXIsRrYOXfaA+xayEGB+
                8hdlLmAjbCVfaigxX0CDqWeR1yFL9kwd9P0NsZRPsmoqVwMbMu7mStFai6aIhc3n
                Slv8kg9qv1m6XHVQY3PnEw+QQtqSIXklHwIDAQAB
                -----END RSA PUBLIC KEY-----
                 */
            };
            
            var clientApi = await ClientFactory.BuildClientAsync(settings).ConfigureAwait(false);
            //clientApi.KeepAliveConnection();
            
            // If the user is not authenticated
            if (!clientApi.AuthService.CurrentUserId.HasValue)
            {
                Console.WriteLine("Request Code : ");
                var sentCode = await clientApi.AuthService.SendCodeAsync(phone); // .ConfigureAwait(false);
                Console.WriteLine("Code : ");
                var code = "63914";//Console.ReadLine();
                TUser user = await clientApi.AuthService.SignInAsync(phone, sentCode, code).ConfigureAwait(false);

                //await clientApi.UpdatesService.AutoReceiveUpdates 

                clientApi.UpdatesService.AutoReceiveUpdates += UpdatesService_AutoReceiveUpdates;
                var dialogs = await clientApi.MessagesService.GetUserDialogsAsync() as TDialogs;
                var chats = dialogs.Chats.OfType<TChannel>().ToList();
                
                foreach(var o in chats)
                {
                    if (o.Title == "kook test")
                    {
                        ForwardChannelId = o.Id;
                        ForwardChannelName = o.Title;
                        ForwardChannelAccessHash = o.AccessHash;
                        break;
                    }
                }

                foreach(var o in chats)
                {
                    if(o.Title.Contains("Rose Challenge") )
                    {
                        var messages = await clientApi.MessagesService.GetHistoryAsync(new TInputPeerChannel { AccessHash = o.AccessHash, ChannelId = o.Id }, 0, 0, 10) as TChannelMessages;
                        var chatMsg = messages.Messages.OfType<TMessage>().OrderBy(m => m.Date).ToList();

                        foreach(var msg in chatMsg)
                        {
                            if ( msg.Media != null )
                            {
                                if ( msg.Media is TMessageMediaPhoto )
                                {
                                    var mediaPhto = msg.Media as TMessageMediaPhoto;
                                    var photo = mediaPhto.Photo as TPhoto;
                                    //(photo.Photo as TPhoto).
                                    var from = new TInputPeerChannel { ChannelId = o.Id, AccessHash = o.AccessHash };
                                    var to = new TInputPeerChannel { ChannelId = ForwardChannelId, AccessHash = ForwardChannelAccessHash };
                                    List<int> msgIds = new List<int>();
                                    msgIds.Add(msg.Id);
                                    var forwardResult = await clientApi.MessagesService.ForwardMessagesAsync(from, to, msgIds, false, false);

                                    TPhotoSize photosize = photo.Sizes.OfType<TPhotoSize>().OrderByDescending(m => m.Size).FirstOrDefault();
                                    string photoName = $"{photo.Id}.jpg";
                                    // Be sure to do this in a seperate task, else it will wait indefinately because you cannot process messages and sending requests at the same thread
                                    await Task.Run(() =>
                                    {
                                        byte[] fileBytes = clientApi.FileService.DownloadFullFileAsync(new TInputFileLocation
                                        {
                                            FileReference = (photosize.Location as TFileLocation).FileReference,
                                            VolumeId = photosize.Location.VolumeId,
                                            LocalId = photosize.Location.LocalId,
                                            Secret = photosize.Location.Secret
                                        }, CancellationToken.None).Result;

                                        File.WriteAllBytes(photoName, fileBytes);
                                    });

                                    IInputFile inputFile;
                                    using (FileStream fileStream = File.Open(photoName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                    {
                                        inputFile = clientApi.FileService.UploadFileAsync(photoName, fileStream).Result;
                                    }
                                    var result = clientApi.MessagesService.SendMediaAsync(to, new TInputMediaUploadedPhoto { File = inputFile }, "Test PHOTO").Result;

                                }
                            }
                        }



                    }
                }

            }
        }

        private void UpdatesService_AutoReceiveUpdates(IUpdates update)
        {
            switch (update)
            {
                case TUpdates updates:
                    var channels = updates.Chats.OfType<TChannel>().ToList();
                    
                    break;
                case TUpdatesCombined updatesCombined:
                    break;
                case TUpdateShort updateShort:
                    break;
                case TUpdateShortChatMessage updateShortChatMessage:
                    break;
                case TUpdateShortMessage updateShortMessage:
                    break;
                case TUpdateShortSentMessage updateShortSentMessage:
                    break;
                case TUpdatesTooLong updatesTooLong:
                    
                    break;
                default:
                    
                    break;
            }
        }
    }
}
