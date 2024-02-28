using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ConfigUI.Handlers
{
    public class Configurations
    {
        public AgentSettingsEx AgentSettings { get; set; }

        private List<ConfigSetting> m_configSettings = new List<ConfigSetting>();

        //WING - 1703 - Added for handling SelectedProfiles in Shell
        //public string IsSelectedProfile { get; set; } 
        public List<ConfigSetting> configSettings
        {
            get { return m_configSettings; }
            set { m_configSettings = value; }
        }

        //-----------------------------------------------------------
        // Same as ($) above.
        //-----------------------------------------------------------
        public List<ConfigSetting> itemValue
        {
            get { return m_configSettings; }
            set { m_configSettings = value; }
        }
        //--------------------------------------(Newtonsoft craft)---------------------------------------      
    }
    public class ElementJson
    {
        public ElementJson(string itemKey, Object itemValue, string parentCcm, string valueType)
        {
            this.itemKey = itemKey;
            this.itemValue = itemValue;
            this.valueType = valueType;
            this.parentCcm = parentCcm;
        }
        public string itemKey { get; set; }
        public Object itemValue { get; set; }
        public string valueType { get; set; }                                   // We need it during loading. When it is JSON, itemValue is an array.
        public string parentCcm { get; set; }

        public bool ShouldSerializeparentCcm()                                  // A custom function of Newtonsoft ShouldSerialize[valueType]
        {                                                                       // to suppress serialize of valueType. WOW.
            return false;
        }
    }
    public class ConfigSetting
    {
        public ConfigSetting(string configName)
        {
            this.configName = configName;
            this.configItems = new List<ElementJson>();
        }

        private string m_configName = "";
        public string configName
        {
            get { return m_configName; }
            set { m_configName = value; }
        }

        private List<ElementJson> m_configItems = new List<ElementJson>();
        public List<ElementJson> configItems
        {
            get { return m_configItems; }
            set { m_configItems = value; }
        }

        //-----------------------------------------------------------($)
        // Code below has a subtle need. It allows us to write two
        // different monikers in SUSE vs. WTOS
        // The ShouldSerialize[param] of Newtonsoft is exploited.
        //-----------------------------------------------------------
        // itemKey/itemValue in place of the configName/configItems
        //-----------------------------------------------------------
        public string itemKey
        {
            get { return m_configName; }
            set { m_configName = value; }
        }
        public List<ElementJson> itemValue
        {
            get { return m_configItems; }
            set { m_configItems = value; }
        }
        //--------------------------------------(Newtonsoft craft)---

    }
    public class AgentSettingsEx
    {
        public AgentSettingsEx()
        {
            Reset = "No";
            EnableBalloonTips = "Yes";
            ImportOnBoot = "Yes";
            UseWdm = "No";
            Protocol = "FTP";
            Port = "21";
            RemoteUsername = "anonymous";

            DHCPProtocolTag = "183";
            DHCPServerTag = "195";
            DHCPPathTag = "196";
            DHCPSUserNameTag = "184";
            DHCPPasswordTag = "185";
        }

        public String Reset { get; set; }
        public String EnableBalloonTips { get; set; }
        public String ImportOnBoot { get; set; }
        public String UseWdm { get; set; }

        public String Protocol { get; set; }
        public String RemoteIP { get; set; }   // Use URI Datatype?
        public String Port { get; set; }
        public String RemotePath { get; set; }

        public String RemoteUsername { get; set; }
        public SecureString RemotePassword { get; set; }

        public String DHCPProtocolTag { get; set; }
        public String DHCPServerTag { get; set; }
        public String DHCPPathTag { get; set; }
        public String DHCPSUserNameTag { get; set; }
        public String DHCPPasswordTag { get; set; }
    }
}
