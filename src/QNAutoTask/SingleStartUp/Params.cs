using BotLib;
using BotLib.Db.Sqlite;
using BotLib.Extensions;
using BotLib.Misc;
using QNAutoTask.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QNAutoTask
{
    public class Params
    {
        public const int Version = 80205;

        public static string VersionStr;

        public static string TestVersionDesc;
        public const bool UseHook = false;

        public static bool TestParamDefault;

        public static string AppSecret;
        private const bool _isUseLocalTestServer = false;

        public static string HelpRoot;

        public const int KeepInstalledVersionsCount = 3;

        public const string AppName = "千牛";

        private static bool? _isDevoloperClient;

        public const int CtlGoodsListGoodsPerPageMin = 3;

        public const int CtlGoodsListGoodsPerPageMax = 20;

        public const int CtlGoodsListGoodsPerPageDefault = 4;

        public const int MaxQACountForChatRecordManager = 2000;

        public const int MaxSynableQuestionAnswersCount = 2000;

        public const int MaxAddQaCountForQuestionAndAnswersCiteTableManager = 30000;

        public const int MaxSynableQuestionTimeoutDays = 10;

        public const bool UseChaJianMode = false;

        public static int BottomPannelAnswerCount;

        public static bool RulePatternMatchStrict;

        private static string _pcGuid;

        private static string _instanceGuid;

        public static readonly DateTime AppStartTime;

        public static bool IsAppClosing;

        public static bool ForceActiveIME
        {
            get
            {
                return PersistentParams.GetParam("ForceActiveIME", true);
            }
            set
            {
                PersistentParams.TrySaveParam("ForceActiveIME", value);
            }
        }

        public static bool SetIsFirstLogin(string nick)
        {
            bool param2Key;
            if (param2Key = PersistentParams.GetParam2Key("IsFirstLogin", nick, true))
            {
                PersistentParams.TrySaveParam2Key("IsFirstLogin", nick, false);
            }
            return param2Key;
        }


        public static bool IsFirstLogin(string nick)
        {
            bool param2Key;
            if (param2Key = PersistentParams.GetParam2Key("IsFirstLogin", nick, true))
            {
                PersistentParams.TrySaveParam2Key("IsFirstLogin", nick, false);
            }
            return param2Key;
        }

        public static bool IsDevoloperClient
        {
            get
            {
                if (_isDevoloperClient == null)
                {
                    _isDevoloperClient = File.Exists("DevoloperClientMark.txt");
                    var isDevoloperClient = _isDevoloperClient;
                    if (!isDevoloperClient.GetValueOrDefault() & isDevoloperClient != null)
                    {
                        string filenameUnderAppDataDir = PathEx.GetFilenameUnderAppDataDir("DevoloperClientMark.txt");
                        _isDevoloperClient = new bool?(File.Exists(filenameUnderAppDataDir));
                    }
                }
                return _isDevoloperClient.Value;
            }
        }

        public static bool IsShowGoodsKnowledgeWhenBuyerTalkIt
        {
            get
            {
                return PersistentParams.GetParam("IsShowGoodsKnowledgeWhenBuyerTalkIt", true);
            }
            set
            {
                PersistentParams.TrySaveParam("IsShowGoodsKnowledgeWhenBuyerTalkIt", value);
            }
        }

        public static void SaveLatestCheckDbDeleteTime(string dbAccount)
        {
            PersistentParams.TrySaveParam2Key("LatestCheckDbDeleteTime", dbAccount, BatTime.Now);
        }

        public static DateTime GetLatestCheckDbDeleteTime(string dbAccount)
        {
            return PersistentParams.GetParam2Key("LatestCheckDbDeleteTime", dbAccount, DateTime.MinValue);
        }

        public static DateTime GetLatestSynOkTime(string dbAccount)
        {
            return PersistentParams.GetParam2Key("LatestSynOkTime", dbAccount, DateTime.MinValue);
        }

        public static void SetLatestSynOkTime(string dbAccount)
        {
            PersistentParams.TrySaveParam2Key("LatestSynOkTime", dbAccount, BatTime.Now);
        }

        public static string SystemInfo
        {
            get
            {
                return string.Format("{4} {0},千牛版本={1}，PcId={2},{3}", new object[]
				{
					VersionStr,
					QnHelper.QnVersion,
					PcId,
					ComputerInfo.SysInfoForLog,
					"软件"
				});
            }
        }

        public static bool TestParam
        {
            get
            {
                return PersistentParams.GetParam("TestParam", TestParamDefault);
            }
            set
            {
                PersistentParams.TrySaveParam("TestParam", true);
            }
        }

        public static string PcId
        {
            get
            {
                if (_pcGuid == null)
                {
                    _pcGuid = ComputerInfo.GetCpuID();
                }
                return _pcGuid;
            }
        }

        public static string InstanceGuid
        {
            get
            {
                if (_instanceGuid == null)
                {
                    string text = PersistentParams.GetParam("InstanceGuid", "");
                    string param = PersistentParams.GetParam("PcId4InstanceGuid", "");
                    if (string.IsNullOrEmpty(text) || param != PcId)
                    {
                        text = StringEx.xGenGuidB64Str();
                        PersistentParams.TrySaveParam("InstanceGuid", text);
                        PersistentParams.TrySaveParam("PcId4InstanceGuid", PcId);
                    }
                    _instanceGuid = text;
                }
                return _instanceGuid;
            }
        }


        public static bool IsAppStartMoreThan10Second
        {
            get
            {
                return (DateTime.Now - AppStartTime).TotalSeconds > 10.0;
            }
        }

        public static bool IsAppStartMoreThan20Second
        {
            get
            {
                return (DateTime.Now - AppStartTime).TotalSeconds > 20.0;
            }
        }

        public static bool HadUniformShortcutCode
        {
            get
            {
                return PersistentParams.GetParam("HadUniformShortcutCode", false);
            }
            set
            {
                PersistentParams.TrySaveParam("HadUniformShortcutCode", value);
            }
        }

        public static bool NeedClearDb
        {
            get
            {
                return PersistentParams.GetParam("NeedClearDb", false);
            }
            set
            {
                PersistentParams.TrySaveParam("NeedClearDb", value);
            }
        }

        static Params()
        {
            TestVersionDesc = "";
            TestParamDefault = false;
            AppSecret = "asfda";
            HelpRoot = "https://github.com/renchengxiaofeixia";
            _isDevoloperClient = null;
            BottomPannelAnswerCount = 5;
            RulePatternMatchStrict = true;
            AppStartTime = DateTime.Now;
            IsAppClosing = false;
        }

        public static void SetProcessPath(string processName, string processPath)
        {
            string key = GetProcessPathKey(processName);
            PersistentParams.TrySaveParam(key, processPath);
        }

        private static string GetProcessPathKey(string processName)
        {
            return "ProcessPath#" + processName;
        }

        public static string GetProcessPath(string processName)
        {
            string key = GetProcessPathKey(processName);
            return PersistentParams.GetParam(key, "");
        }

        public class Other
        {
            public static int FontSize
            {
                get
                {
                    int num = PersistentParams.GetParam("Other.FontSize", 12);
                    if (num < 12)
                    {
                        num = 12;
                        Other.FontSize = 12;
                    }
                    else if (num > 14)
                    {
                        num = 14;
                        Other.FontSize = 14;
                    }
                    return num;
                }
                set
                {
                    PersistentParams.TrySaveParam("Other.FontSize", value);
                }
            }

            public const int FontSizeDefault = 12;
        }

        public static int BottomPanelAnswerCount { get; set; }

        public static bool NeedTipReSynDataOk
        {
            get
            {
                return PersistentParams.GetParam("NeedTipReSynDataOk", false);
            }
            set
            {
                PersistentParams.TrySaveParam("NeedTipReSynDataOk", value);
            }
        }

    }

}
