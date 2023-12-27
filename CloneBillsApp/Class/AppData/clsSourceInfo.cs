using CloneBillsApp.Class.Constants;
using CloneBillsApp.Class.VacsMapApp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneBillsApp.Class.AppData
{
    public class clsSourceInfo : clsOptionKeyBase
    {
        public string Path { get; private set; }

        public clsSourceInfo() 
        { 
            KEY_1 = OptionKeys.CONFIG;
            KEY_2 = OptionKeys.SOURCE_PATH;
            Path = clsOptionSetting.GetString_Value1(KEY_1, KEY_2);
        }

        public bool Save(string path)
        {
            if (clsOptionSetting.Save(KEY_1, KEY_2, path))
            {
                this.Path = path;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
