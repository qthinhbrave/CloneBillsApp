using CloneBillsApp.Class.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneBillsApp.Class.AppData
{
    public class clsLocalDestinationInfo :  clsOptionKeyBase
    {
        public bool IsActive { get; private set; }
        public string Path { get; private set; }

        public clsLocalDestinationInfo()
        {
            KEY_1 = OptionKeys.CONFIG;
            KEY_2 = OptionKeys.LOCAL_DES_CONFIG;
            IsActive = clsOptionSetting.GetBool_Value3(KEY_1, KEY_2);
            Path = clsOptionSetting.GetString_Value1(KEY_1, KEY_2);
        }

        public bool Save(bool isActive, string path)
        {
            if (clsOptionSetting.Save(KEY_1, KEY_2, path, "NULL", isActive ? "1" : "0"))
            {
                this.Path = path;
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
