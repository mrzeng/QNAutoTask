using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QNAutoTask.Automation
{
    public class HwndInfo
    {
        public string Description { get; set; }
        public int Handle { get; set; }
        
        public HwndInfo(int handle, string description)
        {
            this.Handle = handle;
            this.Description = description;
        }
    }
}
