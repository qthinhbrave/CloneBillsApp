using CloneBillsApp.Class.Constants;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloneBillsApp.Class.AppData
{
    public class clsTaskService
    {
        public string TaskName { get; set; }
        public string Description { get; set; }
        public short DaysInterval { get; set; }
        public DateTime StartBoundary { get; set; }

        public clsTaskService(string timeUpload)
        {
            TaskName = String.Format("{0}_{1}", clsCommon.APP_MUTEX_NAME, timeUpload.Replace(":", ""));
            Description = OptionKeys.TASK_DESCRIPTION;
            DaysInterval = 1;
            DateTime d = DateTime.Now;
            int hour = 0, minute = 0;
            string[] list = timeUpload.Split(':');
            if (list.Length == 2 )
            {
                hour = int.Parse(list[0]);
                minute = int.Parse(list[1]);
            }
            StartBoundary = new DateTime(d.Year, d.Month, d.Day, hour, minute, 0).AddDays(DaysInterval);
        }

    }
}
