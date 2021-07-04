using BotLib.BaseClass;
using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotLib.Extensions;
using BotLib;
using QNAutoTask.Automation.ChatDeskNs.Automators;
using QNAutoTask.Common;
using QNAutoTask.Automation;
using QNAutoTask.AssistWindow;
using System.Threading;
using BotLib.Wpf.Extensions;
using QNAutoTask.Automation.ChatDeskNs;

namespace QNAutoTask.ControllerNs
{
    public class DeskScanner : Disposable
    {
        private const int ScanIntervalMs = 1000;
        private static NoReEnterTimer _timer;
        private static bool _hadDetectSellerEver;
        private static bool _hadTipNoSellerEver;

        static DeskScanner()
        {
            _hadDetectSellerEver = false;
            _hadTipNoSellerEver = false;
        }

        public static void LoopScan()
        {
            _timer = new NoReEnterTimer(Loop, 1000, 0);
        }

        private static void Loop()
        {
            try
            {
                HashSet<string> closed;
                var selers = GetOpenedSellers(out closed);
                if (!selers.xIsNullOrEmpty())
                {
                    var nicks = selers.Keys.ToList();

                    nicks.ForEach(nick => {
                        CreateDesk(nick, 2000);
                    });
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private static void CreateDesk(string nick, int delayMs)
        {
            if (delayMs > 0)
            {
                Thread.Sleep(delayMs);
            }
            var loginedSeller = QnHelper.Detected.GetSellerFromCache(nick);
            string arg;
            var desk = ChatDesk.Create(loginedSeller, nick, out arg);
            if (desk != null)
            {
                DispatcherEx.xInvoke(() =>
                {
                    WndAssist.Inst.Show();
                });
            }
        }

        private static Dictionary<string, LoginedSeller> GetOpenedSellers(out HashSet<string> closed)
        {
            var sellers = QnAccountFinderFactory.Finder.GetLoginedSellers();
            var newSellers = QnHelper.Detected.Update(sellers, out closed);
            return newSellers;
        }

        protected override void CleanUp_Managed_Resources()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }

}
