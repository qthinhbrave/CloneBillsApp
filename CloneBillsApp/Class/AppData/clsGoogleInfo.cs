using CloneBillsApp.Class.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneBillsApp.Class.AppData
{
    public class clsGoogleInfo : clsOptionKeyBase
    {
        public bool IsActive { get;  set; }
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }
        public clsGoogleInfo()
        {
            KEY_1 = OptionKeys.CONFIG;
            KEY_2 = OptionKeys.GOOGLE_CONFIG;
            IsActive = clsOptionSetting.GetBool_Value3(KEY_1, KEY_2);
            ClientId = clsOptionSetting.GetString_Value1(KEY_1, KEY_2);
            ClientSecret = clsOptionSetting.GetString_Value2(KEY_1, KEY_2);
        }

        public override string ToString()
        {
            return String.Format("Google drive ({0})", clsCommon.APP_NAME);
        }

        public bool Save(bool isActive, string folderID, string jsonKey)
        {
            if (clsOptionSetting.Save(KEY_1, KEY_2, folderID, jsonKey, isActive ? "1" : "0"))
            {
                this.ClientId = folderID;
                this.ClientSecret = jsonKey;
                this.IsActive = isActive;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
