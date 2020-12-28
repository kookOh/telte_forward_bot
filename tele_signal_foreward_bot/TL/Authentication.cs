using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLSharp.Core;

namespace tele_signal_foreward_bot.TL
{
    class Authentication
    {
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected int apiId = 546613;
        protected string apiHash = "d01286377172be46a4d422fde6bd7e1f";
        string phone = "8201093960641";
        public TelegramClient Client { get; set; }
        public bool IsAuthenticated { get; set; }
        public Authentication()
        {

        }

        public async Task<bool> Auth()
        {
            var client = new TelegramClient(apiId, apiHash, dcIpVersion: DataCenterIPVersion.OnlyIPv4);
            await client.ConnectAsync();
            var hash = await client.SendCodeRequestAsync(phone);
            Console.WriteLine("Code: ");
            var code = Console.ReadLine();
            var user = await client.MakeAuthAsync(phone, hash, code);
            this.Client = client;
            
            this.IsAuthenticated = true;
            return true;

        }
    
    }
}
