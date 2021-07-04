using BotLib;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BotLib.Extensions;
using QNAutoTask.Automation.ChatDeskNs;
using QNAutoTask.Common;
using QNAutoTask.Automation.ChromeDebugger;
using System.Collections.Concurrent;
using Top.Api;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Top.Api.Util;
using Top.Api.Domain;
using Top.Api.Response;

namespace QNAutoTask.ChromeNs
{
    public class ChatRecordChrome : ChromeConnector, IChatRecordChrome
    {
        private HashSet<string> _chatRecordChromeTitle;
        private DateTime _preListeningTime;
        public event EventHandler<BuyerSwitchedEventArgs> EvBuyerSwitched;
        public event EventHandler<BuyerEventArgs> EvBuyerClosed;

        private ConcurrentDictionary<long, ManualResetEventSlim> _requestWaitHandles = new ConcurrentDictionary<long, ManualResetEventSlim>();
        private ConcurrentDictionary<long, TopResponse> _responses = new ConcurrentDictionary<long, TopResponse>();
        long _incrementCount = 0;

        public string PreBuyer
        {
            get;
            private set;
        }
        public string CurBuyer
        {
            get;
            private set;
        }


        public ChatRecordChrome(ChatDesk desk)
            : base(desk.Hwnd.Handle, "ocr_" + desk.Seller)
        {
            this._chatRecordChromeTitle = new HashSet<string>
			{
				"当前聊天窗口",
				"IMKIT.CLIENT.QIANNIU",
				"聊天窗口",
				"imkit.qianniu",
                "千牛聊天消息",
                "千牛消息聊天",
                "alires:///WebUI/chatnewmsg/recent.html"
			};
            this._preListeningTime = DateTime.Now;
            this.Timer.AddAction(FetchRecordLoop, 300, 300);
        }
        public bool GetHtml(out string html, int timeoutMs = 500)
        {
            html = "";
            bool result = false;
            if (this.WaitForChromeOk(timeoutMs))
            {
                result = true;
            }
            return result;
        }
        protected override void ClearStateValues()
        {
            base.ClearStateValues();
            this.CurBuyer = "";
            this.PreBuyer = "";
        }
        protected override ChromeOperator CreateChromeOperator(string chromeSessionInfoUrl)
        {
            var chromeOp = new ChromeOperator(chromeSessionInfoUrl, this._chatRecordChromeTitle, true);
            chromeOp.ClearChromeConsole();
            chromeOp.EvalForMessageListen();
            chromeOp.ListenChromeConsoleMessageAddedMessage(DealChromeConsoleMessage);
            return chromeOp;
        }

        private void DealChromeConsoleMessage(ConsoleMessage consoleMessage)
        {
            try
            {
                var text = consoleMessage.Text.Trim();
                TopResponse response = null;

                var t = this.GetType();
                var funcGetTopResponse = this.GetType().GetMethod("GetTopResponse", BindingFlags.NonPublic | BindingFlags.Instance);

                if (text.StartsWith("ChromeWindowConsoleLog,"))
                {
                    text = text.Substring("ChromeWindowConsoleLog,".Length);
                    var jObject = JObject.Parse(text);
                    var commandId = long.Parse(jObject["task_command_id"].ToString());
                    var apiName = jObject["api_name"].ToString();
                    var topResponse = jObject["top_response"].ToString();
                    var resT = GetTopResponseGenericMethodType(apiName);
                    var GeneriGetTopResponse = funcGetTopResponse.MakeGenericMethod(resT);
                    var res = GeneriGetTopResponse.Invoke(this, new object[] { topResponse });
                    if (res != null)
                    {
                        response = res as TopResponse;
                        HandleResponse(response, commandId);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public Type GetTopResponseGenericMethodType(string apiName)
        {
            //Type req = null;
            //foreach (var k in reqNs)
            //{
            //    var inst = k.Assembly.CreateInstance(k.FullName);
            //    var m = k.GetMethod("GetApiName");
            //    if (m == null) continue;
            //    var r = m.Invoke(inst, null);
            //    if (r != null && r.ToString() == apiName)
            //    {
            //        req = k;
            //        break;
            //    }
            //}
            //var responseGenericMethodType = req.BaseType.GenericTypeArguments[0].FullName;
            var ts = Assembly.Load("TopSdk");
            var reqNs = ts.DefinedTypes.Where(k => k.Namespace == "Top.Api.Request");
            var req = reqNs.FirstOrDefault(k => k.GetMethod("GetApiName") != null && k.GetMethod("GetApiName").Invoke(k.Assembly.CreateInstance(k.FullName), null).ToString() == apiName);
            return req.BaseType.GenericTypeArguments[0];
        }

        private TopResponse GetTopResponse<T>(string message) where T : TopResponse
        {
            return TopUtils.ParseResponse<T>(message);
        }

        private void HandleResponse(TopResponse response, long commandId)
        {
            if (null == response) return;
            ManualResetEventSlim requestMre;
            if (_requestWaitHandles.TryGetValue(commandId, out requestMre))
            {
                _responses.AddOrUpdate(commandId, id => response, (key, value) => response);
                requestMre.Set();
            }
            else
            {
                if (1 == _requestWaitHandles.Count)
                {
                    var requestId = _requestWaitHandles.Keys.First();
                    _requestWaitHandles.TryGetValue(requestId, out requestMre);
                    _responses.AddOrUpdate(requestId, id => response, (key, value) => response);
                    requestMre.Set();
                }
            }
        }

        private void FetchRecordLoop()
        {
            try
            {
                if (this.IsChromeOk)
                {
                    if ((DateTime.Now - this._preListeningTime).TotalSeconds > 5.0)
                    {
                        this._preListeningTime = DateTime.Now;
                        if (this.ChromOp != null)
                        {
                            ChromOp.EvalForMessageListen();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private void BuyerSwitched(string preBuyer, string curBuyer = null)
        {
            if (preBuyer != curBuyer)
            {
                this.PreBuyer = (curBuyer ?? this.CurBuyer);
                this.CurBuyer = preBuyer;
                if (this.EvBuyerSwitched != null)
                {
                    EvBuyerSwitched(this, new BuyerSwitchedEventArgs
                    {
                        CurBuyer = preBuyer,
                        PreBuyer = this.PreBuyer,
                        Connector = this
                    });
                }
            }
        }

        public void OpenChat(string uid)
        {
            var cmd = @"
                imsdk.invoke('application.openChat',{nick:'{uid}'})
                .then((data)=>{
                    console.log(data);
                });";
            var cmdFull = cmd.Replace("{uid}", uid);
            if (ChromOp != null)
            {
                ChromOp.Eval(cmdFull);
            }
        }

        public async Task<TopResponse> TimeGet()
        {
            var res = await RequestTopApi("taobao.time.get");
            return res;
        }

        public async Task<List<Trade>> TradesSoldIncrementGetForAllPage(DateTime dayModified)
        {
            var trades = new List<Trade>();
            var pageNo = 1;
            var totalPages = 0L;
            var pageSize = 100;
            do
            {
                var pms = new Dictionary<string, string>();
                pms["fields"] = "trd,buyer_nick";
                pms["start_modified"] = dayModified.ToString("yyyy-MM-dd");
                pms["end_modified"] = dayModified.ToString("yyyy-MM-dd") + " 23:59:59";
                pms["status"] = "WAIT_SELLER_SEND_GOODS";
                pms["page_no"] = pageNo.ToString();
                pms["page_size"] = pageSize.ToString();
                var res = (await RequestTopApi("taobao.trades.sold.increment.get", pms, true)) as TradesSoldGetResponse;
                totalPages = res.TotalResults % pageSize > 0 ? res.TotalResults / pageSize + 1 : res.TotalResults / pageSize;
                pageNo++;
                trades.AddRange(res.Trades);
            } while (pageNo <= totalPages);
            ChromOp.ClearChromeConsole();
            return trades;
        }

        public async Task<List<Trade>> TradesSoldGetForAllPage(DateTime startCreated, DateTime endCreated)
        {
            var trades = new List<Trade>();
            var pageNo = 1;
            var totalPages = 0L;
            var pageSize = 100;
            do
            {
                var pms = new Dictionary<string, string>();
                pms["fields"] = "delivery_time,buyer_alipay_no,step,modified,timeout_action_time,end_time,pay_time,consign_time,rate_status,seller_nick,shipping_type,cod_status,tid,status,end_time,buyer_nick,trade_from,credit_card_fee,buyer_rate,seller_rate,created,num,payment,pic_path,has_buyer_message,receiver_country,receiver_state,receiver_city,receiver_district,receiver_town,receiver_address,receiver_zip,receiver_name,receiver_mobile,receiver_phone,seller_flag,type,post_fee,has_yfx,yfx_fee,buyer_message,buyer_flag,buyer_memo,seller_memo,invoice_name,invoice_type,invoice_kind,promotion_details,alipay_no,buyerTaxNO,pbly,step_trade_status,step_paid_fee,send_time";
                pms["start_created"] = startCreated.ToString("yyyy-MM-dd");
                pms["end_created"] = endCreated.ToString("yyyy-MM-dd") + " 23:59:59";
                pms["page_no"] = pageNo.ToString();
                pms["page_size"] = pageSize.ToString();
                var res = (await RequestTopApi("taobao.trades.sold.get", pms, true)) as TradesSoldGetResponse;
                totalPages = res.TotalResults % pageSize > 0 ? res.TotalResults / pageSize + 1 : res.TotalResults / pageSize;
                pageNo++;
                trades.AddRange(res.Trades);
            } while (pageNo <= totalPages);
            ChromOp.ClearChromeConsole();
            return trades;
        }


        public async Task<TopResponse> ItemsOnsaleGet()
        {
            var pms = new Dictionary<string, string>();
            pms["fields"] = "approve_status,num_iid,title,nick,type,cid,pic_url,num,props,valid_thru,list_time,price,has_discount,has_invoice,has_warranty,has_showcase,modified,delist_time,postage_id,seller_cids,outer_id,sold_quantity";
            var res = await RequestTopApi("taobao.items.onsale.get", pms);
            return res;
        }

        public async Task<TopResponse> LogisticsDummySend(string tid)
        {
            var pms = new Dictionary<string, string>();
            pms["tid"] = tid.ToString();
            var res = await RequestTopApi("taobao.logistics.dummy.send", pms);
            return res;
        }

        public async Task<TopResponse> TradeMemoAdd(string tid, string memo)
        {
            var pms = new Dictionary<string, string>();
            pms["tid"] = tid.ToString();
            pms["memo"] = memo.ToString();
            var res = await RequestTopApi("taobao.trade.memo.add", pms);
            return res;
        }

        public void SendMsg(string uid, string msg)
        {
            var cmd = @"
                imsdk.invoke('intelligentservice.SendSmartTipMsg', {
                    userId: '{uid}',
                    smartTip: '{msg}',
                    smartTipPos: 1
                });";
            var cmdFull = cmd.Replace("{uid}", uid).Replace("{msg}", msg);
            var retVal = string.Empty;
            if (ChromOp != null)
            {
                ChromOp.Eval(cmdFull, out retVal);
            }
        }

        public Task<TopResponse> RequestTopApi(string cmd, Dictionary<string, string> pms = null, bool refillTid = false)
        {
            var taskId = Interlocked.Increment(ref _incrementCount);
            var requestJs = JsTopApiRequest.BuildQapRequest(taskId, cmd, pms, refillTid);
            var requestResetEvent = new ManualResetEventSlim(false);
            _requestWaitHandles.AddOrUpdate(taskId, requestResetEvent, (id, r) => requestResetEvent);
            return System.Threading.Tasks.Task.Run(() =>
            {
                if (ChromOp != null)
                {
                    ChromOp.Eval(requestJs);
                }
                requestResetEvent.Wait(3 * 60 * 1000);
                TopResponse response = null;
                _responses.TryRemove(taskId, out response);
                _requestWaitHandles.TryRemove(taskId, out requestResetEvent);
                return response;
            });
        }

    }

}
