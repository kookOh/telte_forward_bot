using SoftCircuits.IniFileParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tele_signal_foreward_bot.Common;

namespace tele_signal_foreward_bot.Config
{
    class Configs : BaseSingletone<Configs>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string channelname { get; set; }
        public bool lastmessage_send { get; set; }
        public List<string> keywords { get; set; } = new List<string>();
        public List<string> nonkeywords { get; set; } = new List<string>();
        
        public Dictionary<string, List<string>> channelKeywords = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> channelNonKeywords = new Dictionary<string, List<string>>();
        public Dictionary<string, FilteringEnum> channelFileter = new Dictionary<string, FilteringEnum>();
        public Configs()
        {
            Init(); 
        }
        public void Init()
        {
            var dir = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(dir, startupConfig.Instance.configFileName);
            IniFile file = new IniFile();
            file.Load(filePath);
            file.CommentCharacter = '#';

            foreach (var section in file.GetSections())
            {
                log.Debug($"Section [{section}]");
                foreach (var setting in file.GetSectionSettings(section))
                {
                    log.Debug($"Name [{setting.Name}] : Value [{setting.Value}]");
                    string key = setting.Name.ToLower();

                    if (key == "channelname")
                    {
                        channelname = setting.Value;
                    }
                    else if (key == "lastmessage_send")
                    {
                        if (setting.Value == "true")
                            lastmessage_send = true;
                        else
                            lastmessage_send = false; 
                    }
                    else if (key == "filter")
                    {
                        try
                        {
                            channelFileter.Add(section, (FilteringEnum)Convert.ToInt32(setting.Value));
                        }
                        catch (Exception)
                        {

                            log.Error($"Filter 값이 잘못되었습니다. section [{section}] ");
                            throw;
                        }

                    }
                    else if(key == "keyword")
                    {
                        if( !channelKeywords.ContainsKey(section))
                            channelKeywords.Add(section, new List<string>());
                        
                        channelKeywords[section].Clear();
                        var list = setting.Value.Split(',').ToList();
                        if (list.Count > 0)
                            channelKeywords[section].AddRange(list);
                    }
                    else if (key == "nonkeyword")
                    {
                        if (!channelNonKeywords.ContainsKey(section))
                            channelNonKeywords.Add(section, new List<string>());

                        channelNonKeywords[section].Clear();
                        var list = setting.Value.Split(',').ToList();
                        if (list.Count > 0)
                            channelNonKeywords[section].AddRange(list);


                    }

                }
            }

        }
    }
}
