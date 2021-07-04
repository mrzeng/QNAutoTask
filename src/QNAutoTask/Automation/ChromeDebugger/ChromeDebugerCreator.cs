using QNAutoTask.ChromeNs;
using QNAutoTask.Automation.ChatDeskNs;
using QNAutoTask.Automation.ChromeDebugger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QNAutoTask.Automation.ChromeDebugger
{
    public class ChromeDebugerCreator
    {
        public static IChatRecordChrome Create(ChatDesk desk)
        {
            IChatRecordChrome chrome = null;
            if (ChromeOperator.Connectable(desk.Hwnd.Handle))
            {
                chrome = new ChatRecordChrome(desk);
            }
            //else
            //{
            //    chrome = new ChatRecordChromeV2(desk);
            //}
            return chrome;
        }
    }
}
