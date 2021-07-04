using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using QNAutoTask.Automation;
using QNAutoTask.Automation.ChatDeskNs;
using BotLib;
using BotLib.Db.Sqlite;
using BotLib.Extensions;
using BotLib.Misc;
using BotLib.Wpf.Extensions;
using QNAutoTask.Common;
using Top.Api.Util;
using Top.Api.Response;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections;
using Top.Api.Domain;

namespace QNAutoTask.AssistWindow
{
    public partial class WndAssist : Window
    {
        private NoReEnterTimer _timer;
        private NoReEnterTimer _timerSendGoods;

        public static WndAssist _inst;
        public static WndAssist Inst
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new WndAssist();
                }
                return _inst;
            }
        }

        public WndAssist()
        {
            InitializeComponent();
            try
            {
                try
                {
                    string text = "微软雅黑";
                    if (IsFontFamilyExist(text))
                    {
                        base.FontFamily = new System.Windows.Media.FontFamily(text);
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
                FontSize = (double)Params.Other.FontSize;
                this.xHideToAltTab();

                Closed += WndAssist_Closed;
                Loaded += WndAssist_Loaded;
                txtStartDate.SelectedDate = DateTime.Now.AddDays(-7);
                txtEndDate.SelectedDate = DateTime.Now;
                Show();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private void WndAssist_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= WndAssist_Loaded;
            Show();

            AppStartInitiator.Init();
            _timer = new NoReEnterTimer(LoopNewSeller, 1000, 200);
            _timerSendGoods = new NoReEnterTimer(VirtualTradeAutoSend,30 * 1000, 1000);
        }


        private void VirtualTradeAutoSend()
        {
            DispatcherEx.xInvoke( () =>
            {
                if (chkAutoSendGoods.IsChecked ?? false)
                {
                    ChatDesk.DeskSet.ToList().ForEach(async desk =>
                    {
                        var trds = await desk.TradesSoldIncrementGetForAllPage(DateTime.Now);
                        trds.ForEach( async trd=>{
                            if (!trd.TidStr.xIsNullOrEmptyOrSpace())
                            {
                                var sendRes = (await desk.LogisticsDummySend(trd.TidStr)) as LogisticsDummySendResponse ;
                                if (sendRes.Shipping != null && sendRes.Shipping.IsSuccess)
                                {
                                    Log.Info("发货成功:" + trd.TidStr);
                                }
                                else
                                {
                                    Log.Info("发货失败:" + trd.TidStr);
                                }
                            }
                        });
                    });
                }
            });
        }

        private void LoopNewSeller()
        {
            DispatcherEx.xInvoke(() =>
            {
                var qns = txtQns.Items.Cast<string>().ToList();
                var newQns = ChatDesk.DeskSet.ToList();
                var expts = newQns.Where(k => !qns.Any(j => k.Seller == j)).Select(k => k.Seller);
                txtQns.Items.xAddRange(expts);

            });
        }


        private void WakeUp()
        {
        }


        private void WndAssist_Closed(object sender, EventArgs e)
        {
        }

        private bool IsFontFamilyExist(string fontName)
        {
            bool f = false;
            try
            {
                using (Font font = new Font(fontName, 12f))
                {
                    f = (font.Name == fontName);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return f;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        public void BringTop()
        {
            try
            {
                if (IsLoaded)
                {
                    Topmost = true;
                    DispatcherEx.DoEvents();
                    Topmost = false;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        public static WndAssist GetTopWindow()
        {
            return WinApi.GetTopWindow<WndAssist>();
        }


        List<Trade> selectedTrades = new List<Trade>();
        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            var desk = ChatDesk.DeskSet.FirstOrDefault(k => k.Seller == txtQns.Text);
            if (desk == null)
            {
                MessageBox.Show("没有检测到千牛接待窗口");
                return;
            }
            if (!txtStartDate.SelectedDate.HasValue)
            {
                MessageBox.Show("请选择开始时间");
                return;
            }
            if (!txtEndDate.SelectedDate.HasValue)
            {
                MessageBox.Show("请选择结束时间");
                return;
            }
            try
            {
                ShowWaiting();
                var trades = await desk.TradesSoldGetForAllPage(txtStartDate.SelectedDate.Value, txtEndDate.SelectedDate.Value);
                dtCtrl.ItemsSource = trades;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            finally
            {
                CloseWaiting();
            }
        }

        private void CloseWaiting()
        {
            grdWaiting.Visibility = System.Windows.Visibility.Collapsed;
            dtCtrl.Visibility = System.Windows.Visibility.Visible;
            grdNoDataTip.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ShowWaiting()
        {
            grdWaiting.Visibility = System.Windows.Visibility.Visible;
            dtCtrl.Visibility = System.Windows.Visibility.Collapsed;
            grdNoDataTip.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ShowNoDataTip()
        {
            grdWaiting.Visibility = System.Windows.Visibility.Collapsed;
            dtCtrl.Visibility = System.Windows.Visibility.Collapsed;
            grdNoDataTip.Visibility = System.Windows.Visibility.Visible;
        }

        private void chkSelectTrade_Click(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            var tidStr = chk.Tag.ToString();
            if (tidStr.xIsNullOrEmptyOrSpace())
            {
                return;
            }
            var trds = dtCtrl.ItemsSource as List<Trade>;
            var trd = trds.FirstOrDefault(k => tidStr == k.TidStr);
            if (chk.IsChecked ?? false)
            {
                selectedTrades.Add(trd);
            }
            else
            {
                selectedTrades.Remove(trd);
            }
        }

        private void btnSendMsg_Click(object sender, RoutedEventArgs e)
        {
            var desk = ChatDesk.DeskSet.FirstOrDefault(k => k.Seller == txtQns.Text);
            if (desk == null)
            {
                MessageBox.Show("没有检测到千牛接待窗口");
                return;
            }

            if (selectedTrades.Count < 1)
            {
                MessageBox.Show("请选择要发送的订单");
                return;
            }

            if (chkSendTemplateMsg.IsChecked ?? false)
            {
                if (txtMsgTemplate.Text.xIsNullOrEmptyOrSpace())
                {
                    MessageBox.Show("请输入要模板消息内容");
                    return;
                }
                BatchSendTemplateMsg(desk);
            }
            else
            {
                if (txtMsg.Text.xIsNullOrEmptyOrSpace())
                {
                    MessageBox.Show("请输入要发送的内容");
                    return;
                }
                BatchSendNormalMsg(desk);
            }
            MessageBox.Show("发送完成!!");
        }

        private void BatchSendNormalMsg(ChatDesk desk)
        {
            selectedTrades.ForEach(k =>
            {
                desk.SendMsg("cntaobao" + k.BuyerNick, txtMsg.Text);
            });
        }

        private void BatchSendTemplateMsg(ChatDesk desk)
        {
            selectedTrades.ForEach(k =>
            {
                var msg = txtMsgTemplate.Text.Replace("{省}", k.ReceiverState)
                                    .Replace("{市}", k.ReceiverCity)
                                    .Replace("{区}", k.ReceiverDistrict)
                                    .Replace("{镇}", k.ReceiverTown)
                                    .Replace("{详细地址}", k.ReceiverAddress)
                                    .Replace("{电话}", k.ReceiverMobile)
                                    .Replace("{姓名}", k.ReceiverName);
                desk.SendMsg("cntaobao" + k.BuyerNick, msg);
            });
        }

        private void btnReward_Click(object sender, RoutedEventArgs e)
        {
            new WndReward().ShowDialog();
        }

    }

}
