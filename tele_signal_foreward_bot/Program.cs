using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using tele_signal_foreward_bot.Config;
using tele_signal_foreward_bot.TL;

namespace tele_signal_foreward_bot
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {

            /*Authentication auth = new Authentication();
            var authResult  =  auth.Auth().Result;
            return;*/
            if (!startupConfig.Instance.init(args))
                return;

            /*try
            {
                OpenTL.Example example = new OpenTL.Example();
                string a = "";
                while (a != "exit")
                {
                    Thread.Sleep(30000);
                }
            }
            catch (Exception)
            {

                throw;
            }
            */


            //Example ex = new Example();
            var config = Configs.Instance;
            Authentication auth = new Authentication();
            try
            {
                bool authProcess = auth.Auth().Result;



                Thread thr;
                if (auth.IsAuthenticated)
                {

                    Forwarder.Instance.SetAuth(auth);
                    Receiver.Instance.SetAuth(auth);
                    //receiver.Init();
                    Receiver.Instance.Run();
                    /*thr = new Thread(receiver.Run);
                    thr.Start();*/

                }
                else
                {
                    log.Error("Not Authenticated ");
                    return;
                }


                string a = "";
                while (a != "exit")
                {
                    Thread.Sleep(30000);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }


       

        


    }
}
