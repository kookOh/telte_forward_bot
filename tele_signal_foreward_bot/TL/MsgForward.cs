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
            return Task.CompletedTask;
        }


        void SavePhoto(TeleSharp.TL.TLMessageMediaPhoto photoMSg)
        {

        }

        void UpdateMessageMap(Dictionary<int, int> MessageMap, Dictionary<int, string> MessageCheckMap, string msg, int msgNum)
        {
            // 비트상승 > 알트하방 > 비트하방 > 알트더하방 > 비트 반등 > 알트롱  > 비트저항 > 알트조정 (알트매수타임)
            // 비트 하방 > 알트 상방 > 비트 상방 > 알트더 상방 > 비트 저항 > 알트숏  > 비트상승 > 알트 롱(알트 숏타임) 
        }

    }
}
