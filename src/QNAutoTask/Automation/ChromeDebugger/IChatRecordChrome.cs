using QNAutoTask.ChromeNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QNAutoTask.Automation.ChromeDebugger
{
    public interface IChatRecordChrome
    {
        event EventHandler<BuyerSwitchedEventArgs> EvBuyerSwitched;
        event EventHandler<BuyerEventArgs> EvBuyerClosed;
        event EventHandler<ChromeAdapterEventArgs> EvChromeDetached;
        event EventHandler<ChromeAdapterEventArgs> EvChromeConnected;
        string PreBuyer { get; }
        string CurBuyer { get; }
        bool IsChromeOk { get; }
        void Dispose();
    }
}
