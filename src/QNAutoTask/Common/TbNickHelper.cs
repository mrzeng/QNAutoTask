using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QNAutoTask.Common
{
    public class TbNickHelper
    {
        public static string GetMainPart(string nick)
        {
            if (!string.IsNullOrEmpty(nick))
            {
                int idx = nick.IndexOf(':');
                if (idx > 0)
                {
                    nick = nick.Substring(0, idx);
                }
            }
            return nick;
        }
    }
}
