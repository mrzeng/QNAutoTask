using BotLib;
using BotLib.Extensions;
using QNAutoTask.ControllerNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QNAutoTask
{
    public class AppStartInitiator
    {
        public static void Init()
        {
            AppStartInitiator.WaitForInit();
            AppStartInitiator.ClearTmpPathFiles();
            DeskScanner.LoopScan();
        }

        private static void ClearTmpPathFiles()
        {
            try
            {
                if (Directory.GetFiles(PathEx.TmpPath).Length > 0)
                {
                    DirectoryEx.DeleteC(PathEx.TmpPath, true);
                    Directory.CreateDirectory(PathEx.TmpPath);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
        private static void WaitForInit()
        {
            DateTime now = DateTime.Now;
            BatTime.WaitForInit(2000);
        }
    }
}
