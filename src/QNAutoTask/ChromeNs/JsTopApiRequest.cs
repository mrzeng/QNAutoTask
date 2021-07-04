using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QNAutoTask.ChromeNs
{
    public class JsTopApiRequest
    {
        public static string BuildRequest(long taskId, string cmd, Dictionary<string, string> pms)
        {
            var paramStr = string.Empty;
            if (pms != null && pms.Count > 0)
            {
                var idx = 0;
                var pmList = new List<string>();
                foreach (var key in pms.Keys)
                {
                    var val = pms[key];
                    pmList.Add(string.Format("'{0}': '{1}'", key, val));
                    idx++;
                }
                paramStr = pmList.Aggregate((s1, s2) => s1 + ", " + s2);
            }

            var sb = new StringBuilder();
            sb.Append("QN.top.invoke({"); // start
            sb.Append("method: 'post',");
            sb.AppendFormat("cmd: '{0}',", cmd);
            sb.AppendFormat("param: {{{0}}},", paramStr);
            sb.AppendFormat("success: function (res) {{console.log('ChromeWindowConsoleLog,'+JSON.stringify({{api_name:'{0}',task_command_id:'{1}',top_response: res}}));}},", cmd, taskId);
            sb.AppendFormat("error: function (res) {{console.log('ChromeWindowConsoleLog,'+JSON.stringify({{api_name:'{0}',task_command_id:'{1}',top_response: res}}));}}", cmd, taskId);
            sb.Append("});"); //end
            return sb.ToString();
        }

        public static string BuildQapRequest(long taskId, string cmd, Dictionary<string, string> pms = null,bool refillTid = false)
        {
            var paramStr = string.Empty;
            var pmList = new List<string>();
            if (pms != null && pms.Count > 0)
            {
                var idx = 0;
                foreach (var key in pms.Keys)
                {
                    var val = pms[key];
                    pmList.Add(string.Format("'{0}': '{1}'", key, val));
                    idx++;
                }
            }
            pmList.Add(string.Format("'method': '{0}'", cmd));
            paramStr = pmList.Aggregate((s1, s2) => s1 + ", " + s2);
            var sb = new StringBuilder();
            sb.Append("QN.top.invoke({"); // start
            sb.AppendFormat(" query: {{ {0} }} ", paramStr);
            sb.Append(" })");
            if (refillTid)
            {
                sb.AppendFormat(".then(res => {{ var fisrt_prop='';for(var prop in res){{fisrt_prop=prop;break}}if(res[fisrt_prop].total_results<1){{console.log('ChromeWindowConsoleLog,'+JSON.stringify({{api_name:'{0}',task_command_id:'{1}',top_response:res}}))}}else{{var promises=res[fisrt_prop].trades.trade.map(trade=>{{return new Promise(resolve=>{{QN.app.invoke({{api:'invokeMTopChannelService',query:{{method:'mtop.taobao.qianniu.airisland.user.get',param:{{fields:JSON.stringify(['user_id']),nick:trade.buyer_nick}},httpMethod:'post',version:'1.0',}}}}).then(buyer=>{{var buyerId=0;if(buyer.data.errorMessage!=''){{QN.app.invoke({{api:'invokeMTopChannelService',query:{{method:'mtop.taobao.seattle.qianniu.card.send.post',param:JSON.stringify({{sellerNick:'cntaobao'+trade.buyer_nick,buyerNick:'cntaobao'+trade.buyer_nick}}),httpMethod:'post',version:'1.0',}}}}).then(newbuyer=>{{console.log('newbuyerid:'+newbuyer.data.data.buyerId); buyerId=newbuyer.data.data.buyerId;trade.tid_str=trade.tid.toString().substring(0,trade.tid.toString().length-4)+buyerId.toString().substring(buyerId.toString().length-2)+buyerId.toString().substring(buyerId.toString().length-4,buyerId.toString().length-2);resolve(newbuyer)}})}}else{{console.log('buyerid:'+ buyer.data.data.userId);buyerId=buyer.data.data.userId;trade.tid_str=trade.tid.toString().substring(0,trade.tid.toString().length-4)+buyerId.toString().substring(buyerId.toString().length-2)+buyerId.toString().substring(buyerId.toString().length-4,buyerId.toString().length-2);resolve(buyer)}}}})}})}});Promise.all(promises).then(rt=>{{console.log('ChromeWindowConsoleLog,'+JSON.stringify({{api_name:'{0}',task_command_id:'{1}',top_response:res}}))}})}} }},error =>{{ console.log('ChromeWindowConsoleLog,'+JSON.stringify({{api_name:'{0}',task_command_id:'{1}',top_response: error}})); }}) ", cmd, taskId);
            }
            else
            {
                sb.AppendFormat(".then(res => {{ console.log('ChromeWindowConsoleLog,'+JSON.stringify({{api_name:'{0}',task_command_id:'{1}',top_response: res}})); }},error =>{{ console.log('ChromeWindowConsoleLog,'+JSON.stringify({{api_name:'{0}',task_command_id:'{1}',top_response: error}})); }}) ", cmd, taskId);
            }
            return sb.ToString();
        }
    }
}
