using ConfigUI.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WCMCOMServer
{
    [Guid("9A249DC6-17A0-4EEF-BDC3-B792A2F14F15")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IConfigUIValidator
    {
        bool dtcConfigMgr_Validate_Interface(string filePath);

        void GetParsedJSONPayLoad(string payLoadDetails, out SettingsDetails[] array);
    }

    [Guid("5180B6D6-C6A4-486E-B27B-A81D6815F340")]
    [ClassInterface(ClassInterfaceType.None)]
    public class ConfigUIValidator : IConfigUIValidator
    {
        public bool dtcConfigMgr_Validate_Interface([MarshalAs(UnmanagedType.BStr)] string filePath)
        {
            return SupportValidator.IsConfigUISupported() ? SupportValidator.dtcConfigMgr_Validate(filePath) : false;
        }

        public void GetParsedJSONPayLoad([MarshalAs(UnmanagedType.BStr)] string payLoadDetails, [Out] out SettingsDetails[] array)
        {
            var sd_count = SupportValidator.getPayload(payLoadDetails).Count();
            array = new SettingsDetails[sd_count];
            var list_sd = SupportValidator.getPayload(payLoadDetails);
            for (int i = 0; i < sd_count; i++)
            {
                array[i].configKey = list_sd.ElementAt(i).Item1;
                array[i].configPayload = list_sd.ElementAt(i).Item2.Trim();
            }
        }
    }

    [ComVisible(true)]
    [Guid("4D4142B2-4CF6-4DF2-928E-DE047C818192")]
    [StructLayout(LayoutKind.Sequential)]
    public struct SettingsDetails
    {
        [MarshalAs(UnmanagedType.BStr)]
        public string configKey;
        [MarshalAs(UnmanagedType.BStr)]
        public string configPayload;
    }
}
