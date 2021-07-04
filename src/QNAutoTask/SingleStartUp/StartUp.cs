using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using BotLib;
using BotLib.Extensions;
using QNAutoTask.AssistWindow;

namespace QNAutoTask.SingleStartUp
{
	public class StartUp
	{
		[STAThread]
		public static void Main(string[] args)
		{
			try
			{
                bool createdNew;
                using (new Mutex(true, "QNAutoTask", out createdNew))
				{
                    if (createdNew)
					{
                        KillProcess();
						AppLife.OnStart();
						App app = new App();
                        app.InitializeComponent();
                        app.Run(WndAssist.Inst);
					}
					else
					{
                        MessageBox.Show("软件已经运行了！", Params.AppName);
						Application app = Application.Current;
						if (app != null)
						{
							app.Shutdown();
						}
					}
				}
			}
			catch (Exception ex)
			{
                MessageBox.Show("OnStartup:" + ex.Message, Params.AppName);
				Log.Exception(ex);
			}
		}

        private static void KillProcess()
		{
            var processes = Process.GetProcessesByName("QNAutoTask");
			var curProcess = Process.GetCurrentProcess();
			foreach (var p in processes.xSafeForEach())
			{
                if (p.Id != curProcess.Id)
				{
					p.Kill();
				}
			}
		}

	}
}
