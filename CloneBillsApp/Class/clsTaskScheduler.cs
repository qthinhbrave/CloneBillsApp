using CloneBillsApp.Class.AppData;
using CloneBillsApp.Class.Constants;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CloneBillsApp.Class
{
    public class clsTaskScheduler
    {
        public static bool CreateTask(clsTaskService task)
        {
            using (TaskService ts = new TaskService())
            {
                // Create a new task definition and assign properties
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = task.Description;

                // Create a trigger that will fire the task at this time every other day
                td.Triggers.Add(new DailyTrigger { 
                    DaysInterval = task.DaysInterval,
                    StartBoundary = task.StartBoundary,
                });

                // Create an action
                td.Actions.Add(new ExecAction(OptionKeys.EXE_FILE, OptionKeys.EXE_ARGUMENT, Application.StartupPath));

                // Register the task in the root folder
                ts.RootFolder.RegisterTaskDefinition(task.TaskName, td);
            }
            return true;
        }

        public static bool DeleteTask(string taskName)
        {
            using (TaskService ts = new TaskService())
            {
                // Remove the task by name
                if (ts.GetTask(taskName) != null)
                {
                    ts.RootFolder.DeleteTask(taskName);
                }
            }
            return true;
        }
    }
}
