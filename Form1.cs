using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Specialized;
using System.Threading;

namespace WinForms_HDD_Activity
{
    public partial class Form1 : Form
    {
        NotifyIcon hddLedIcon;
        Icon activeIcon;
        Icon idleIcon;
        Thread hddLedWorker;
        #region Hide Form
        //Gathering HDD telemetry 
        public Form1()
        {
            InitializeComponent();
            //Load Icons from files into objects
            
            ///Remake the files as ico files in gimp:
            ///16 x 16
            activeIcon = new Icon("hdd_active.ico");
            idleIcon = new Icon("hdd_idle.ico");
            //Create and use Icons
            hddLedIcon = new NotifyIcon();
            hddLedIcon.Icon = idleIcon;
            hddLedIcon.Visible = true;

            //Create all context menu items 
            MenuItem programMenu = new MenuItem("HDD Activity App");
            MenuItem quitMenu = new MenuItem("Quit");
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(programMenu);
            contextMenu.MenuItems.Add(quitMenu);
            hddLedIcon.ContextMenu = contextMenu;

            //End Application Session
            quitMenu.Click += QuitMenu_Click;

            //Hide form / hide in notification tray
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            //Start Worker Thread
            hddLedWorker = new Thread(new ThreadStart(HddActivityThread));
            hddLedWorker.Start();
        }

        //Close Application
        private void QuitMenu_Click(object sender, EventArgs e)
        {
            hddLedWorker.Abort();
            hddLedIcon.Dispose();
            this.Close();
        }
        #endregion
        #region Threading
        //This is the thread that pulls HDD Activity
        public void HddActivityThread()
        {
        ManagementClass driveDataClass = new ManagementClass("Win32_PerFormattedDate_PerfDisk_PhysicalDisk");
            try
            {
                //Main Loop
                ///Query WME Class Objects
                ///Compare the size of the bits
                ///If it is above 0 then the drive is busy
                ///Otherwise it is not in use
                while(true)
                {
                    //Connect to drive performnance instace
                    ManagementObjectCollection driveDataClassCollection = driveDataClass.GetInstances();//Management Exception Unhandled
                    foreach(ManagementObject obj in driveDataClassCollection)
                    {
                        if(obj["Name"].ToString() == "_Total")
                        {
                            //Returns 64bit Int - Convert this for process
                            if(Convert.ToUInt32(obj["DiskBytesPersec"]) > 0)
                            {
                                //Show Busy Icon
                                hddLedIcon.Icon = activeIcon;
                            }
                            else
                            {
                                //Show Idle Icon
                                hddLedIcon.Icon = idleIcon;
                            }
                        }
                    }
                    //WBEMTest Class
                    //Testting Thread MessageBox.Show("This is a test");
                    Thread.Sleep(100);
                }
            }
            catch(ThreadAbortException tae)
            {
                //Thread is Aborted
                driveDataClass.Dispose();
            }
        }
        #endregion
    }
}