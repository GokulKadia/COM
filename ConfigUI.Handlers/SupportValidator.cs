using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ConfigUI.Handlers
{
    public static class SupportValidator
    {
        public static string regKVerPath = "SYSTEM\\CurrentControlSet\\Control\\WNT";
        public static string regConfigSupported = "Software\\WNT";
        //This function is checking the if Device Supported the ConfigUI or Not
        public static bool IsConfigUISupported()
        {
            bool IsCnfgUISupported = false;
            try
            {
                RegistryKey reg_Kver = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                               RegistryView.Registry64).OpenSubKey(regKVerPath, true);
                RegistryKey reg_ConfigUI = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                    RegistryView.Registry64).OpenSubKey(regConfigSupported, true);

                IsCnfgUISupported = reg_Kver?.GetValue("Kver")?.ToString() == "-1" ? true : reg_ConfigUI?.GetValue("IsConfigUISupported")?.ToString() == "1" ? true : false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return IsCnfgUISupported;
        }
        private static List<string> getPayloadParamKeys(string Payload)
        {
            JObject o = JObject.Parse(Payload);
            JToken jt = o.SelectToken("$.configSettings.parameters");
            return jt?.Select(x => x.ToString()).ToList();
        }
        public static string getPayloadParamKeys(string Payload, string param)
        {
            JObject o = JObject.Parse(Payload);
            return o.Value<string>(param) ?? string.Empty.ToString();
        }
        private static List<Dictionary<string, string>> getPayloadParamKeys(string Payload, string param, string App)
        {
            List<Dictionary<string, string>> AppValue = new List<Dictionary<string, string>>();
            List<string> ss = null;
            JObject o = JObject.Parse(Payload);
            JToken jtValue;
            JToken jt = o.SelectToken("$.configSettings.parameters." + param + ".values"); //rcShellSettings_application           
            if (jt != null)
            {
                foreach (var v in jt.ToList())
                {
                    jtValue = JObject.Parse(v.ToString()).SelectToken("$.parameters");
                    ss = jtValue.Select(x => x.ToString()).ToList();
                    var something = ss.Select(x =>
                    {
                        var itemArr = x.Split(new[] { ':' }, 2);
                        var keyKVP = itemArr[0];
                        keyKVP = keyKVP.Substring(1, keyKVP.Length - 2);
                        var valKVP = itemArr[1].Split(new[] { ':' }, 2)[1].Split(new[] { ',' }, 2)[0].Trim().Replace("\n", "").Replace("\r", "");
                        valKVP = valKVP.Substring(1, valKVP.Length - 2);
                        return new { keyKVP, valKVP };
                    }).ToDictionary(x => x.keyKVP, x => x.valKVP);
                    AppValue.Add(something);
                }
            }
            return AppValue;
        }
        public static Dictionary<string, string> Common(string JsonPay, string SetName, string param)
        {
            Dictionary<string, string> setValue = null;
            switch (SetName)
            {
                case "Settings":
                    setValue = getPayloadParamKeys(JsonPay).Select(x =>
                    {
                        var itemArr = x.Split(new[] { ':' }, 2);
                        var keyKVP = itemArr[0];
                        keyKVP = keyKVP.Substring(1, keyKVP.Length - 2);
                        var valKVP = itemArr[1]; return new { keyKVP, valKVP };
                    }).ToDictionary(x => x.keyKVP, x => x.valKVP);
                    break;                
                default:
                    break;
            }
            return setValue;
        }

        //DistinctName = getConfigName(JsonPayload);
        //Configurations conf = new Configurations();
        //conf.AgentSettings = null;
        //conf.configSettings = new List<ConfigSetting>();
        //conf.itemValue = conf.configSettings;

        //Dictionary<string, string> internalList = null;
        //foreach (var c in DistinctName)
        //{
        //    internalList = setValue.Where(x => x.Key.StartsWith(c))
        //     .ToDictionary(x => x.Key.Replace(c + "_", ""), x =>
        //     {
        //         var val = "";
        //         if (x.Key.Replace(c + "_", "") != "application")
        //         {
        //             return x.Value;
        //         }
        //         else
        //         {
        //             val = x.Value;
        //             return val;
        //         }
        //     });
        //    conf.configSettings.Add(
        //          new ConfigSetting(c)
        //          {
        //              configName = c.ToString(),
        //              itemKey = c.ToString(),
        //              configItems = internalList.Select(x =>
        //                  new ElementJson(x.Key, x.Value, c, "")
        //              ).ToList()
        //          }
        //          );
        //}
        public static Dictionary<string, string> JsonParser(string JsonPayload, string settingName)
        {
            Dictionary<string, string> setValue = null;
            List<string> DistinctName = null;
            switch (settingName)
            {
                case "BIOSConfig":
                    setValue = Common(JsonPayload, "Settings", "").
                     Where(x => x.Key.StartsWith("BIOSConfig"))
                        ?.Where(x => x.Key.StartsWith($"BIOSConfig{getDevicePID()}"))
                    .ToDictionary(x => x.Key.Split(new[] { '_' }, 2)[1].Replace("___", "-").Replace("__", " "), x =>
                    {
                        var val = x.Value.Split(':')[1];
                        return val.Substring(val.IndexOf('"') + 1, val.LastIndexOf('"') - (val.IndexOf('"') + 1)).Replace("yes", "2").Replace("no", "1"); ;
                    });
                    break;
                case "VNCSettings":
                case "DeployementSettings":
                case "deviceSettings":
                case "DomainSettings":
                    setValue = Common(JsonPayload, "Settings", "").
                    Where(x => x.Key.StartsWith(settingName))
                    .ToDictionary(x => x.Key.Replace(settingName + "_", ""), x =>
                    {
                        var val = x.Value.Split(':')[1];
                        return val.Substring(val.IndexOf('"') + 1, val.LastIndexOf('"') - (val.IndexOf('"') + 1));
                    });
                    break;
                case "WirelessSettings":
                    setValue = setValue = Common(JsonPayload, "Settings", "").
                    Where(x => x.Key.StartsWith(settingName))
                    .ToDictionary(x => x.Key.Replace(settingName + "_", ""), x =>
                    {
                        var val = x.Value.Split(':')[1];
                        return val.Substring(val.IndexOf('"') + 1, val.LastIndexOf('"') - (val.IndexOf('"') + 1));
                    });
                    List<string> ss = null;
                    Dictionary<string, string> sval = null;
                    JObject o = JObject.Parse(JsonPayload);
                    JToken jt = o.SelectToken("$.configSettings.parameters.WirelessSettings_wesWifiProfileName.files"); //rcShellSettings_application           
                    if (jt != null)
                    {
                        foreach (var v in jt.ToList())
                        {
                            //jtValue = JObject.Parse(v.ToString()).SelectToken("$.files");
                            ss = v.Select(x => x.ToString()).ToList();
                            sval = ss.Select(x =>
                            {
                                var itemArr = x.Split(new[] { ':' }, 2);
                                var keyKVP = itemArr[0].Substring(itemArr[0].IndexOf('"') + 1, itemArr[0].LastIndexOf('"') - (itemArr[0].IndexOf('"') + 1));
                                var valKVP = itemArr[1].Substring(itemArr[1].IndexOf('"') + 1, itemArr[1].LastIndexOf('"') - (itemArr[1].IndexOf('"'))).Replace("\"", ""); return new { keyKVP, valKVP };
                            }).ToDictionary(x => x.keyKVP, x => x.valKVP);
                        }
                    }
                    setValue.Remove("wesWifiProfileName");
                    sval?.Where(x => x.Key == "name" || x.Key == "checksum" || x.Key == "checksumWithHashVersion2")
                        .ToDictionary(c => c.Key.Replace("name", "wesWifiProfileName"), c => c.Value).ToList().ForEach(x => setValue.Add(x.Key, x.Value));
                    break;

                case "rcShellSettings":
                case "rcCitrixSettings":
                case "rcVMWareSettings":
                case "rcBrowserSettings":
                    string strApp = "[" + "\n";
                    setValue = Common(JsonPayload, "Settings", "").
                    Where(x => x.Key.StartsWith("rc"))
                    .ToDictionary(x => x.Key, x =>
                    {
                        var val = "";
                        switch (x.Key.Substring(0, x.Key.IndexOf('_')))
                        {
                            case "rcShellSettings":
                            case "rcCitrixSettings":
                            case "rcVMWareSettings":
                            case "rcBrowserSettings":
                                if (!x.Key.EndsWith("_applications") && !x.Key.EndsWith("rcBrowserSettings_url") && !x.Key.EndsWith("_contentUrlRule") && !x.Key.EndsWith("_IEFavorite"))
                                {
                                    val = x.Value.Split(':')[1];
                                    val = val.Substring(val.IndexOf('"') + 1, val.LastIndexOf('"') - (val.IndexOf('"') + 1)).Replace("yes", "true").Replace("no", "false");
                                }
                                else if (x.Key.Contains("rcBrowserSettings_url"))
                                {
                                    val = x.Value.Split(new[] { ':' }, 2)[1];
                                    val = val.Substring(val.IndexOf('"') + 1, val.LastIndexOf('"') - (val.IndexOf('"') + 1)).Replace("yes", "true").Replace("no", "false");
                                }
                                else if (x.Key.EndsWith("_contentUrlRule"))
                                {
                                    var value = getPayloadParamKeys(JsonPayload, "rcCitrixSettings_contentUrlRule", "");
                                    foreach (var v in value.ToList())
                                    {
                                        strApp += "[" + v.GetString() + "]," + "\n";
                                    }
                                    strApp = strApp.Remove(strApp.LastIndexOf(','));
                                    strApp = strApp.Insert(strApp.Length, "\n" + "]");
                                    val = strApp;
                                }
                                else if (x.Key.EndsWith("_IEFavorite"))
                                {
                                    var value = getPayloadParamKeys(JsonPayload, "rcBrowserSettings_IEFavorite", "");
                                    foreach (var v in value.ToList())
                                    {
                                        strApp += "[" + v.GetString() + "]," + "\n";
                                    }
                                    strApp = strApp.Remove(strApp.LastIndexOf(','));
                                    strApp = strApp.Insert(strApp.Length, "\n" + "]");
                                    val = strApp;
                                }
                                else
                                {
                                    var value = getPayloadParamKeys(JsonPayload, "rcShellSettings_applications", "");
                                    foreach (var v in value.ToList())
                                    {
                                        strApp += "[" + v.GetString() + "]," + "\n";
                                    }
                                    strApp = strApp.Remove(strApp.LastIndexOf(','));
                                    strApp = strApp.Insert(strApp.Length, "\n" + "]");
                                    val = strApp;
                                }
                                break;
                            default:
                                break;
                        }
                        return val;
                    });
                    break;
                case "otherSettings":
                    string strShare = "[" + "\n";
                    setValue = Common(JsonPayload, "Settings", "").
                  Where(x => x.Key.StartsWith("ot"))
                  .ToDictionary(x => x.Key, x =>
                  {
                      var val = "";
                      if (!x.Key.EndsWith("_someShareDrives"))
                      {
                          val = x.Value.Split(':')[1];
                          val = val.Substring(val.IndexOf('"') + 1, val.LastIndexOf('"') - (val.IndexOf('"') + 1)).Replace("yes", "true").Replace("no", "false");
                      }
                      else
                      {
                          var value = getPayloadParamKeys(JsonPayload, "otherSettings_someShareDrives", "");
                          foreach (var v in value.ToList())
                          {
                              strShare += "[" + v.GetString() + "]," + "\n";
                          }
                          strShare = strShare.Remove(strShare.LastIndexOf(','));
                          strShare = strShare.Insert(strShare.Length, "\n" + "]");
                          val = strShare;
                      }

                      return val;
                  });
                    break;
                default:
                    break;
            }
            return setValue;
        }
        public static Dictionary<string, string> RDPDictionary()
        {
            Dictionary<string, string> val = new Dictionary<string, string>();
            val.Add("connectionName", "");
            val.Add("autoConnect", "");
            val.Add("hostName", "");
            val.Add("ssoRDP", "");
            val.Add("userName", "");
            val.Add("password", "");
            val.Add("domainName", "");
            val.Add("reconnect", "");
            val.Add("useRDGateway", "");
            val.Add("redirectClipboard", "");
            val.Add("redirectComPorts", "");
            val.Add("redirectDirectX", "");
            val.Add("redirectDrives", "");
            val.Add("redirectPOSDevices", "");
            val.Add("redirectPrinters", "");
            val.Add("redirectSmartCards", "");
            val.Add("remoteFXUSBredirect", "");
            val.Add("enableUSBredirectionLater", "");
            val.Add("enablePlugNPlayRedirection", "");
            val.Add("fullScreen", "");
            val.Add("displayConnectionBar", "");
            val.Add("useMultiMon", "");
            val.Add("screenColorDepthRDP", "");
            val.Add("audioMode", "");
            val.Add("audioCaptureMode", "");
            val.Add("applyWindowsKeys", "");
            val.Add("startProgramOnConnection", "");
            val.Add("promptForCredentials", "");
            val.Add("negotiateSecurityLayer", "");
            val.Add("compression", "");
            val.Add("videoPlaybackMode", "");
            val.Add("enableWorkspaceReconnect", "");
            val.Add("connSpeedToOptimizeExperience", "");
            val.Add("rdpDisableWallpaper", "");
            val.Add("rdpDisableThemes", "");
            val.Add("rdpEnableFontSmoothing", "");
            val.Add("bitmapCachePersistEnable", "");
            val.Add("allowDesktopComposition", "");
            val.Add("disableCursorsetting", "");
            val.Add("disableFullWindowDrag", "");
            val.Add("rdpDisableAnimation", "");
            val.Add("useRedirectionServerName", "");
            val.Add("rdpAuthenticationLevel", "");
            return val;
        }
        public static List<Dictionary<string, string>> rdpJsonParser(string JsonPayload)
        {
            var valuerdp = getPayloadParamKeys(JsonPayload, "rcRDPSettings_RDPSettings", "");
            var dict = RDPDictionary();
            var someDict = valuerdp.Select(x =>
            {
                var tempDict = new Dictionary<string, string>();
                tempDict = dict.Select(y => x.ContainsKey(y.Key)
                ? new KeyValuePair<string, string>(y.Key, x[y.Key]) : y)
                .ToDictionary(z => z.Key, z => z.Value);
                return tempDict;
            }).ToList();
            return someDict;
        }
        public static List<string> getConfigName(string JsonPay)
        {
            List<string> ss = new System.Collections.Generic.List<string>();
            ss = getPayloadParamKeys(JsonPay).Select(
            x =>
            {
                var itemArr = x.Split(new[] { ':' }, 2);
                var keyKVP = itemArr[0];
                keyKVP = keyKVP.Substring(1, keyKVP.IndexOf('_') - 1);
                return keyKVP;
            }).Distinct().ToList();
            return ss;
        }
        private static string getDevicePID()
        {
            RegistryKey readPID = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                   RegistryView.Registry64).OpenSubKey(regKVerPath, true);
            return readPID.GetValue("PID").ToString().Substring(0, 3)=="527"? readPID.GetValue("PID").ToString().Substring(0, 3).Replace("527","528"): readPID.GetValue("PID").ToString().Substring(0, 3);
        }
        public static List<ValueTuple<string, string>> getPayload(string JsonPay)
        {
            string strValues;
            List<string> dictName = getConfigName(JsonPay);
            List<ValueTuple<string, string>> setPolicy = new List<ValueTuple<string, string>>();
            Dictionary<string, string> setValue = new Dictionary<string, string>();
            foreach (var item in dictName)
            {
                strValues = "[" + "\n";
                switch (item)
                {
                    case "otherSettings":
                        setValue = JsonParser(JsonPay, "otherSettings").Where(x => x.Key.Contains("otherSettings")).ToDictionary(c => c.Key.Replace("otherSettings_", ""), c => c.Value);
                        strValues += setValue.GetString().Replace(":\"[", ":[").Replace("]\",", "],") + "]";
                        setPolicy.Add((item, strValues));
                        break;
                    case "rcCitrixSettings":
                        setValue = JsonParser(JsonPay, "rcCitrixSettings").Where(x => x.Key.Contains("rcCitrixSettings")).ToDictionary(c => c.Key.Replace("rcCitrixSettings_", ""), c => c.Value);
                        strValues += setValue.GetString().Replace(":\"[", ":[").Replace("]\",", "],") + "]";
                        setPolicy.Add((item, strValues));
                        break;
                    case "rcShellSettings":
                        setValue = JsonParser(JsonPay, "rcShellSettings").Where(x => x.Key.Contains("rcShellSettings")).ToDictionary(c => c.Key.Replace("rcShellSettings_", ""), c => c.Value);
                        strValues += setValue.GetString().Replace(":\"[", ":[").Replace("]\",", "],") + "]";
                        setPolicy.Add((item, strValues));
                        break;
                    case "rcVMWareSettings":
                        setValue = JsonParser(JsonPay, "rcVMWareSettings").Where(x => x.Key.Contains("rcVMWareSettings")).ToDictionary(c => c.Key.Replace("rcVMWareSettings_", ""), c => c.Value);
                        strValues += setValue.GetString().Replace(":\"[", ":[").Replace("]\",", "],") + "]";
                        setPolicy.Add((item, strValues));
                        break;
                    case "rcBrowserSettings":
                        setValue = JsonParser(JsonPay, "rcBrowserSettings").Where(x => x.Key.Contains("rcBrowserSettings")).ToDictionary(c => c.Key.Replace("rcBrowserSettings_", ""), c => c.Value);
                        strValues += setValue.GetString().Replace(":\"[", ":[").Replace("]\",", "],") + "]";
                        setPolicy.Add((item, strValues));
                        break;
                    case "rcRDPSettings":
                        List<Dictionary<string, string>> setV = rdpJsonParser(JsonPay);
                        //strValues += setValue.GetString().Replace(":\"[", ":[").Replace("]\",", "],").Replace("rcRDPSettings_", "") + "]";
                        foreach(var v in setV)
                        {
                            strValues += v.GetString().Replace(":\"[", ":[").Replace("]\",", "],").Replace("rcRDPSettings_", "") + "]";
                            setPolicy.Add((item, strValues));
                        }
                        //setPolicy.Add((item, strValues));
                        break;
                    default:
                        break;
                }
            }
            return setPolicy;
        }
        public static bool dtcConfigMgr_Validate(string filePath)
        {
            if (getPayloadParamKeys(File.ReadAllText(filePath))?.Any() != true)
                return false; // invalid config
            else
                return true; // valid config 
        }
    }

    public struct SettingsDetails
    {
        public string configKey;
        public string configPayload;
    }

    //added for handle Application Parameters
    public static class Extensions
    {
        public static string GetString<K, V>(this IDictionary<K, V> dict)
        {
            var items = from kvp in dict
                        select "\n" + "{" + "\n" + "\"itemKey\":" + "\"" + kvp.Key + "\"" + "," + "\n"
                        + "\"itemValue\":" + "\"" + kvp.Value + "\"" + "," + "\n" + "\"itemValueExtra\":null"
                        + "," + "\n" + "\"valueType\":null" + "\n" + "}";
            return string.Join(", ", items);
        }
    }
}
