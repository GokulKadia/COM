using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCMCOMServer;
using ConfigUI.Handlers;

namespace DebugApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WCMCOMServer.SettingsDetails[] array;
            IConfigUIValidator configValidator = new ConfigUIValidator();
            configValidator.GetParsedJSONPayLoad(File.ReadAllText("C:\\Users\\Sumeet_Dev_PC\\Desktop\\ConfigUI_JSON\\Citrix_RDP_RC.json"), out array);
        }
    }
}
