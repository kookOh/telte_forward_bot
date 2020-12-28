using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tele_signal_foreward_bot.Common;

namespace tele_signal_foreward_bot.Config
{
    class startupConfig : BaseSingletone<startupConfig>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string configFileName = "settings.ini";
        public string env = "PRODUCT";

        public startupConfig( )
        {

        

        }
        public bool init(string[] args)
        {
            try
            {
                foreach (string arg in args)
                {
                    if (arg.StartsWith("--config="))
                    {
                        configFileName = getValue(arg);

                    }
                    else if (arg.StartsWith("--env="))
                    {
                        env = getValue(arg);
                    }
                    else if (arg.StartsWith(""))
                    {

                    }
                }

                return true;
            }
            catch (Exception)
            {
                usage();
                return false;
            }
         

            
        }

        string getValue(string __arg__)
        {
            try
            {
                return __arg__.Split('=').Last().ToLower();
            }
            catch (Exception)
            {
                throw;
            }
        }

        void usage()
        {
            Console.WriteLine($" ### Startup Setup Settings. ###");
            Console.WriteLine($" >>> Default Setup");
            Console.WriteLine($" --config=testconfig.ini            e.g (--config=testconfig.ini)");
            Console.WriteLine($" --env=dev                          e.g (--env=dev)");
            Console.WriteLine($" --");
            Console.WriteLine($"");
            Console.WriteLine($"");
            Console.WriteLine($"");
            Console.WriteLine($"");
            Console.WriteLine($"");

        }
    }
}
