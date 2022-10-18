using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Hydee.Aout.Interface.DAO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Xml.Serialization;
using System.Net.Http;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Hydee.Aout.Interface
{
    public partial class frmMain : Form
    {
        #region "变量"
        bool bFirst = false;

        string ExeName = "";

        bool bComplete = false;

        bool bTriggerResult = false;

        bool bTriggerFirst = false;

        bool bSingle = false;

        bool bCheckConnAtOpen = false;

        int iReRunTime = 1440; //默认24小时重启一次

        List<clsTableStru> listTable = new List<clsTableStru>();

        List<clsXmlStu> listXmlNode = new List<clsXmlStu>();

        string softvar = "1";

        string autoRun = "0";

        string strAgentIP = "";
        string strAgentPort = "";
        string strAgentUser = "";
        string strAgentPass = "";

        string strWebEx = "0";

        bool bRun = false;

        bool bTriggerRun = false;

        int iWaitTime = 0;

        int iTriggerWait = 0;

        int iCount = 0;

        int iTriggerCount = 0;

        bool bRuNow = false;

        bool bTriggerNow = false;

        Thread threadRun;

        Thread threadSingle;

        Thread thrBinSub;

        Thread threadTrigger;

        bool bCanExit = false;

        List<clsInterfaceSet> InterFaceSet = new List<clsInterfaceSet>();

        List<clsInterfaceReSet> InterFaceReSet = new List<clsInterfaceReSet>();

        bool bTest = false;

        bool bPreRunResult = true;

        DAOInterface dii = DAOFactory.CreateDAOInterface();

        #endregion "变量"

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        //public struct INTERNET_PROXY_INFO
        //{
        //    public int dwAccessType;
        //    public string lpszProxy;
        //    public string lpszProxyBypass;
        //}

        //[DllImport("urlmon.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        //public static extern int UrlMkSetSessionOption(int dwOption, ref INTERNET_PROXY_INFO pBuffer, int dwBufferLength, int dwReserved);
        //[DllImport("urlmon.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        //public static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);
        //public const int S_OK = 0;
        //public const int S_NULL = 0;
        //public const int INTERNET_OPEN_TYPE_PROXY = 3;
        //public const int INTERNET_OPTION_PROXY = 38;
        //public const int INTERNET_OPTION_PROXY_USERNAME = 43;
        //public const int INTERNET_OPTION_PROXY_PASSWORD = 44;

        // 置网络代理        
        //private bool SetUserProxy(string address, string username, string password)
        //{
        //    username = username ?? string.Empty;
        //    password = password ?? string.Empty;
        //    var sProxy = new INTERNET_PROXY_INFO()
        //    {
        //        lpszProxy = address,
        //        dwAccessType = INTERNET_OPEN_TYPE_PROXY
        //    };
        //    UrlMkSetSessionOption(INTERNET_OPTION_PROXY_USERNAME, username, username.Length, S_NULL);
        //    UrlMkSetSessionOption(INTERNET_OPTION_PROXY_PASSWORD, password, password.Length, S_NULL);
        //    return UrlMkSetSessionOption(INTERNET_OPTION_PROXY, ref sProxy, 12, S_NULL) == S_OK;
        //}

        #region "窗体加载"
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                ReadAppConfig();

                string strExeName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

                if (!string.IsNullOrEmpty(ExeName))
                {
                    strExeName = ExeName;
                }

                this.Text = strExeName + "-V" + AssemblyVersion;
                
                if (bCheckConnAtOpen)
                {
                    if (!string.IsNullOrEmpty(clsGolbal.GetDLLAppKey("HydeeConn")))
                    {
                        DAOInterface di = DAOFactory.CreateDAOInterface();

                        try
                        {
                            if (!di.TestConn())
                            {
                                MessageBox.Show("数据库连接不上，请检查数据库连接配置或者网络");

                                return;
                            }
                        }
                        catch(Exception ex)
                        {
                            string strConn = di.GetDBConn();

                            MessageBox.Show("数据库连接不上，请检查数据库连接配置或者网络\r\n" + ex.Message + "\r\n" + strConn);
                            return;
                        }

                        //di.CreateTempTable();
                    }
                    else
                    {
                        MessageBox.Show("请先配置数据库连接");

                        return;
                    }
                }

                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText("加载接口信息\r\n");

                    rthText.ScrollToCaret();
                }));

                thrBinSub = new Thread(new ThreadStart(GetAndBindSubList));

                thrBinSub.Start();

                bTest = chkTest.Checked;

                //bTriggerRun = true;

                //threadTrigger = new Thread(new ThreadStart(RunTriggerSub));

                //threadTrigger.Start();

                //timTrigger.Enabled = true;

                //bTriggerFirst = true;

            }
            catch (Exception ex)
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText("\r\n" + ex.Message + "\r\n");

                    rthText.ScrollToCaret();
                }));
            }
        }
        #endregion "窗体加载"

        #region "窗体关闭相关"
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!bCanExit)
            {
                e.Cancel = true;    //取消"关闭窗口"事件
                this.WindowState = FormWindowState.Minimized;    //使关闭时窗口向右下角缩小的效
                ninote.Text = this.Text.ToString();
                ninote.Visible = true;
                ninote.ShowBalloonTip(1000, "提示", "软件将继续运行，双击图标恢复", ToolTipIcon.Info);
                this.Hide();
                return;
            }

            if (ninote.Visible)
            {
                ninote.Dispose();
            }
        }

        private void ninote_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Maximized;
            ninote.Visible = false;
        }

        private void tsmExit_Click(object sender, EventArgs e)
        {
            if (bTriggerNow)
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    bTriggerRun = false;

                    rthText.AppendText("\r\n正在触发执行接口，请稍后\r\n");

                    rthText.ScrollToCaret();
                }));

                return;
            }
            else
            {
                //threadTrigger.Abort();
                bTriggerRun = false;
            }

            bCanExit = true;

            Application.Exit();
        }

        private void tsmBack_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Maximized;
            ninote.Visible = false;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (bTriggerNow)
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    bTriggerRun = false;

                    rthText.AppendText("\r\n正在触发执行接口，请稍后\r\n");

                    rthText.ScrollToCaret();
                }));

                return;
            }
            else
            {
                //threadTrigger.Abort();
                bTriggerRun = false;
            }

            bCanExit = true;

            Application.Exit();
        }
        #endregion "窗体关闭相关"

        #region "读取配置"
        private void ReadAppConfig()
        {
            try
            {
                dtBegin.Value = Convert.ToDateTime(clsGolbal.GetDLLAppKey("beginTime"));
                dtEnd.Value = Convert.ToDateTime(clsGolbal.GetDLLAppKey("endTime"));
                autoRun = clsGolbal.GetDLLAppKey("autorun");

                ExeName = clsGolbal.GetDLLAppKey("exename");

                txtIP.Text = clsGolbal.GetDLLAppKey("agentIp");
                strAgentIP = txtIP.Text.ToString().Trim();

                txtPort.Text = clsGolbal.GetDLLAppKey("agentPort");
                strAgentPort = txtPort.Text.ToString().Trim();

                txtAgentUser.Text = clsGolbal.GetDLLAppKey("agentUser");
                strAgentUser = txtAgentUser.Text.ToString().Trim();

                strWebEx = clsGolbal.GetDLLAppKey("webExDetailType");

                string strTmpPass = clsGolbal.GetDLLAppKey("agentPass");

                if (!string.IsNullOrEmpty(strTmpPass))
                {
                    MD5Encrypt.DeAndEncrypt de = new MD5Encrypt.DeAndEncrypt();

                    strTmpPass = de.Decrypt(strTmpPass);
                }

                txtAgentPass.Text = strTmpPass;
                strAgentPass = txtAgentPass.Text.ToString().Trim();

                string autorerun = clsGolbal.GetDLLAppKey("autorerun");
                string autoreruntime = clsGolbal.GetDLLAppKey("autoreruntime");
                string takenotes = clsGolbal.GetDLLAppKey("takenotes");
                string checkconnatopen = clsGolbal.GetDLLAppKey("checkconnatopen");

                if (checkconnatopen == "1")
                {
                    bCheckConnAtOpen = true;
                }

                if (takenotes == "1")
                {
                    chkTest.Checked = true;
                }
                else
                {
                    chkTest.Checked = false;
                }

                if (autoRun == "1")
                {
                    chkAutoRun.Checked = true;
                }
                else
                {
                    chkAutoRun.Checked = false;
                }

                if (autorerun == "1")
                {
                    chkAotoReRun.Checked = true;
                }
                else
                {
                    chkAotoReRun.Checked = false;
                }


                txtReRunTime.Text = autoreruntime;

                iReRunTime = Convert.ToInt32(autoreruntime);

                decimal dagain = Convert.ToDecimal(clsGolbal.GetDLLAppKey("intervalTime"));

                if (dagain < 30)
                {
                    dagain = 30;
                }

                numAgain.Value = dagain;

                iWaitTime = Convert.ToInt32(numAgain.Value);

                iTriggerWait = Convert.ToInt32(clsGolbal.GetDLLAppKey("triggerTimes"));

                softvar = clsGolbal.GetDLLAppKey("SoftVer");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion "读取配置"

        #region "保存"
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string strBegin = dtBegin.Value.ToString("HH:mm");

                string strEnd = dtEnd.Value.ToString("HH:mm");

                int iAgain = Convert.ToInt32(numAgain.Value);

                string strAutoRun = "0";

                if (chkAutoRun.Checked)
                {
                    strAutoRun = "1";
                }

                string autorerun = "0";

                if (chkAotoReRun.Checked)
                {
                    autorerun = "1";
                }

                string takenotes = "0";

                if (chkTest.Checked)
                {
                    takenotes = "1";
                }

                string autoreruntime = txtReRunTime.Text.ToString();

                iReRunTime = Convert.ToInt32(autoreruntime);

                if (iAgain < 30)
                {
                    iAgain = 30;
                }

                strAgentIP = txtIP.Text.ToString().Trim();
                strAgentPort = txtPort.Text.ToString().Trim();

                strAgentUser = txtAgentUser.Text.ToString().Trim();
                strAgentPass = txtAgentPass.Text.ToString().Trim();

                string strTmpPass = "";

                if (!string.IsNullOrEmpty(strAgentPass))
                {
                    MD5Encrypt.DeAndEncrypt de = new MD5Encrypt.DeAndEncrypt();

                    strTmpPass = de.Encrypt(strAgentPass);
                }

                clsGolbal.SetDllAppKey("beginTime", strBegin);
                clsGolbal.SetDllAppKey("endTime", strEnd);
                clsGolbal.SetDllAppKey("intervalTime", iAgain.ToString());
                clsGolbal.SetDllAppKey("autorun", strAutoRun);

                clsGolbal.SetDllAppKey("autorerun", autorerun);

                clsGolbal.SetDllAppKey("autoreruntime", autoreruntime);

                clsGolbal.SetDllAppKey("agentIp", strAgentIP);
                clsGolbal.SetDllAppKey("agentPort", strAgentPort);
                clsGolbal.SetDllAppKey("agentUser", strAgentUser);
                clsGolbal.SetDllAppKey("agentPass", strTmpPass);
                clsGolbal.SetDllAppKey("takenotes", takenotes);

                ReadAppConfig();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion "保存"

        #region "读取接口函数列表，并绑定到界面控件"
        private void GetAndBindSubList()
        {
            try
            {
                DAOAccess da = new DAOAccess();

                using (DataSet ds = da.GetInterfaceSub())
                {
                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {

                        lvSub.Items.Clear();
                        InterFaceSet.Clear();

                        DataTable dt = ds.Tables[0];

                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            //string[] strItem = { dt.Rows[i]["chiname"].ToString().Trim(), dt.Rows[i]["subname"].ToString().Trim(), dt.Rows[i]["classname"].ToString().Trim(), dt.Rows[i]["webaddres"].ToString().Trim(), dt.Rows[i]["inparanum"].ToString().Trim(), dt.Rows[i]["outparanum"].ToString().Trim(), dt.Rows[i]["rewsql"].ToString().Trim(), dt.Rows[i]["basesql"].ToString().Trim(), dt.Rows[i]["btablename"].ToString().Trim() };

                            //lvSub.Items.Insert(lvSub.Items.Count, new ListViewItem(strItem));

                            clsInterfaceSet ISet = new clsInterfaceSet();

                            ISet.SubName = dt.Rows[i]["subname"].ToString().Trim();
                            ISet.ChiName = dt.Rows[i]["chiname"].ToString().Trim();
                            ISet.SubType = dt.Rows[i]["subtype"].ToString().Trim();
                            ISet.ClassName = dt.Rows[i]["classname"].ToString().Trim();
                            ISet.InParaNum = Convert.ToInt32(dt.Rows[i]["inparanum"]);
                            //ISet.ReWriteSql = dt.Rows[i]["rewsql"].ToString().Trim();
                            ISet.baseSql = dt.Rows[i]["basesql"].ToString().Trim();
                            ISet.bTableName = dt.Rows[i]["btablename"].ToString().Trim();
                            ISet.strAdd = dt.Rows[i]["webaddres"].ToString().Trim();
                            ISet.chkReSign = dt.Rows[i]["chkReSign"].ToString().Trim();
                            ISet.reTrueValue = dt.Rows[i]["reTrueValue"].ToString().Trim();
                            ISet.reSignNode = dt.Rows[i]["reSignNode"].ToString().Trim();
                            ISet.reErrorNode = dt.Rows[i]["reErrorNode"].ToString().Trim();
                            ISet.recDataDatil = dt.Rows[i]["recdatadatil"].ToString().Trim();
                            ISet.jsonAdd = dt.Rows[i]["jsonadd"].ToString().Trim();
                            ISet.encodType = dt.Rows[i]["encodtype"].ToString().Trim();
                            ISet.contentType = dt.Rows[i]["contenttype"].ToString().Trim();
                            ISet.reObject = dt.Rows[i]["reobject"].ToString().Trim();
                            ISet.expDll = dt.Rows[i]["expdll"].ToString().Trim();
                            ISet.loopBySelf = dt.Rows[i]["loopbyself"].ToString().Trim();
                            ISet.FollowIF = dt.Rows[i]["followif"].ToString().Trim();
                            ISet.PreInter = dt.Rows[i]["preInter"].ToString().Trim();

                            string tmp_autorun = dt.Rows[i]["autorun"].ToString().Trim();

                            if (string.IsNullOrEmpty(tmp_autorun) || tmp_autorun == "0")
                            {
                                tmp_autorun = "否";
                            }
                            else
                            {
                                tmp_autorun = "是";
                            }

                            ISet.autoRun = tmp_autorun;

                            if (ISet.recDataDatil == "1" || ISet.expDll == "1")
                            {
                                DataSet dsDllSet = da.GetDllSet(ISet.SubName, ISet.ChiName, "WEBRECEIVE", "", "");

                                DataSet dsDllParaSet = da.GetDllParaSet(ISet.SubName, ISet.ChiName, "WEBRECEIVE", "", "");

                                clsInterfaceDllSet ciDllSet = new clsInterfaceDllSet();

                                ciDllSet.dllName = dsDllSet.Tables[0].Rows[0]["dllname"].ToString().Trim();
                                ciDllSet.nameSpace = dsDllSet.Tables[0].Rows[0]["dllnamespe"].ToString().Trim();
                                ciDllSet.className = dsDllSet.Tables[0].Rows[0]["dllclassname"].ToString().Trim();
                                ciDllSet.procName = dsDllSet.Tables[0].Rows[0]["dllprocname"].ToString().Trim();
                                ciDllSet.paraNum = Convert.ToInt32(dsDllSet.Tables[0].Rows[0]["dllparanum"]);
                                ciDllSet.dllPara = new List<clsInterfaceDllParaSet>();

                                for (int idp = 0; idp < dsDllParaSet.Tables[0].Rows.Count; idp++)
                                {
                                    clsInterfaceDllParaSet ciDPSet = new clsInterfaceDllParaSet();

                                    ciDPSet.paraname = dsDllParaSet.Tables[0].Rows[idp]["dllparaname"].ToString().Trim();
                                    ciDPSet.paravalue = dsDllParaSet.Tables[0].Rows[idp]["dllparavalue"].ToString().Trim();

                                    ciDllSet.dllPara.Add(ciDPSet);
                                }

                                ISet.dllSet = ciDllSet;
                            }

                            if (ISet.chkReSign == "1")
                            {
                                DataSet dsJudRec = da.GetSubJudRec(ISet.SubName, ISet.ChiName, "sub");

                                ISet.IJudRec = new List<clsInterfaceJudRec>();

                                for (int iJud = 0; iJud < dsJudRec.Tables[0].Rows.Count; iJud++)
                                {
                                    clsInterfaceJudRec ciJud = new clsInterfaceJudRec();

                                    ciJud.sort = dsJudRec.Tables[0].Rows[iJud]["sort"].ToString().Trim();
                                    ciJud.leftbra = dsJudRec.Tables[0].Rows[iJud]["leftbra"].ToString().Trim();
                                    ciJud.titname = dsJudRec.Tables[0].Rows[iJud]["titname"].ToString().Trim();
                                    ciJud.condition = dsJudRec.Tables[0].Rows[iJud]["condition"].ToString().Trim();
                                    ciJud.titvalues = dsJudRec.Tables[0].Rows[iJud]["titvalues"].ToString().Trim();
                                    ciJud.rightbra = dsJudRec.Tables[0].Rows[iJud]["rightbra"].ToString().Trim();
                                    ciJud.datatype = dsJudRec.Tables[0].Rows[iJud]["datatype"].ToString().Trim();
                                    ciJud.linktype = dsJudRec.Tables[0].Rows[iJud]["linktype"].ToString().Trim();

                                    ISet.IJudRec.Add(ciJud);
                                }
                            }

                            if (ISet.loopBySelf == "1")
                            {
                                DataSet dsJudRec = da.GetSubJudRec(ISet.SubName, ISet.ChiName, "loop");

                                ISet.ILoopJudRec = new List<clsInterfaceJudRec>();

                                for (int iJud = 0; iJud < dsJudRec.Tables[0].Rows.Count; iJud++)
                                {
                                    clsInterfaceJudRec ciJud = new clsInterfaceJudRec();

                                    ciJud.sort = dsJudRec.Tables[0].Rows[iJud]["sort"].ToString().Trim();
                                    ciJud.leftbra = dsJudRec.Tables[0].Rows[iJud]["leftbra"].ToString().Trim();
                                    ciJud.titname = dsJudRec.Tables[0].Rows[iJud]["titname"].ToString().Trim();
                                    ciJud.condition = dsJudRec.Tables[0].Rows[iJud]["condition"].ToString().Trim();
                                    ciJud.titvalues = dsJudRec.Tables[0].Rows[iJud]["titvalues"].ToString().Trim();
                                    ciJud.rightbra = dsJudRec.Tables[0].Rows[iJud]["rightbra"].ToString().Trim();
                                    ciJud.datatype = dsJudRec.Tables[0].Rows[iJud]["datatype"].ToString().Trim();
                                    ciJud.linktype = dsJudRec.Tables[0].Rows[iJud]["linktype"].ToString().Trim();

                                    ISet.ILoopJudRec.Add(ciJud);
                                }
                            }

                            ISet.IPara = new List<clsInterfacePara>();

                            DataSet dsPara = da.GetSubPara(ISet.SubName, ISet.ChiName);

                            if (dsPara != null && dsPara.Tables.Count > 0 && dsPara.Tables[0].Rows.Count > 0)
                            {
                                for (int j = 0; j < dsPara.Tables[0].Rows.Count; j++)
                                {
                                    clsInterfacePara cp = new clsInterfacePara();

                                    cp.ParaName = dsPara.Tables[0].Rows[j]["paraname"].ToString().Trim();
                                    cp.isXml = dsPara.Tables[0].Rows[j]["isxml"].ToString().Trim();
                                    cp.getValSql = dsPara.Tables[0].Rows[j]["getvalsql"].ToString().Trim();
                                    cp.valTitle = dsPara.Tables[0].Rows[j]["valtitle"].ToString().Trim();
                                    cp.paraStaticVal = dsPara.Tables[0].Rows[j]["parastaticval"].ToString().Trim();
                                    cp.valByBase = dsPara.Tables[0].Rows[j]["valbybase"].ToString().Trim();
                                    cp.dataDllDetail = dsPara.Tables[0].Rows[j]["datadlldetail"].ToString().Trim();
                                    cp.isJson = dsPara.Tables[0].Rows[j]["isjson"].ToString().Trim();
                                    cp.paType = dsPara.Tables[0].Rows[j]["patype"].ToString().Trim();
                                    cp.dataType = dsPara.Tables[0].Rows[j]["datatype"].ToString().Trim();
                                    cp.withxml = dsPara.Tables[0].Rows[j]["withxml"].ToString().Trim();

                                    if (cp.isXml == "1")
                                    {
                                        cp.xml = da.GetXmlSet(ISet.SubName, ISet.ChiName, cp.ParaName);

                                        cp.paraDll = da.GetDllSet(ISet.SubName, ISet.ChiName, cp.ParaName);

                                        cp.paraPDll = da.GetDllParaSet(ISet.SubName, ISet.ChiName, cp.ParaName);
                                    }

                                    if (cp.dataDllDetail == "1")
                                    {
                                        DataSet dsDllSet = da.GetDllSet(ISet.SubName, ISet.ChiName, cp.ParaName, "", "");

                                        DataSet dsDllParaSet = da.GetDllParaSet(ISet.SubName, ISet.ChiName, cp.ParaName, "", "");

                                        if (dsDllSet == null || dsDllSet.Tables.Count <= 0 || dsDllSet.Tables[0].Rows.Count <= 0
                                           || dsDllParaSet == null || dsDllParaSet.Tables.Count <= 0 || dsDllParaSet.Tables[0].Rows.Count <= 0)
                                        {
                                            throw new UserException(ISet.ChiName + ":" + cp.ParaName + " dll配置缺失");
                                        }

                                        clsInterfaceDllSet ciDllSet = new clsInterfaceDllSet();

                                        ciDllSet.dllName = dsDllSet.Tables[0].Rows[0]["dllname"].ToString().Trim();
                                        ciDllSet.nameSpace = dsDllSet.Tables[0].Rows[0]["dllnamespe"].ToString().Trim();
                                        ciDllSet.className = dsDllSet.Tables[0].Rows[0]["dllclassname"].ToString().Trim();
                                        ciDllSet.procName = dsDllSet.Tables[0].Rows[0]["dllprocname"].ToString().Trim();
                                        ciDllSet.paraNum = Convert.ToInt32(dsDllSet.Tables[0].Rows[0]["dllparanum"]);
                                        ciDllSet.dllPara = new List<clsInterfaceDllParaSet>();

                                        for (int idp = 0; idp < dsDllParaSet.Tables[0].Rows.Count; idp++)
                                        {
                                            clsInterfaceDllParaSet ciDPSet = new clsInterfaceDllParaSet();

                                            ciDPSet.paraname = dsDllParaSet.Tables[0].Rows[idp]["dllparaname"].ToString().Trim();
                                            ciDPSet.paravalue = dsDllParaSet.Tables[0].Rows[idp]["dllparavalue"].ToString().Trim();

                                            ciDllSet.dllPara.Add(ciDPSet);
                                        }

                                        cp.dllSet = ciDllSet;
                                    }

                                    ISet.IPara.Add(cp);
                                }
                            }

                            ISet.IReSet = new List<clsInterfaceReSet>();

                            DataSet dsReSet = da.GetReset(ISet.SubName, ISet.ChiName);

                            if (dsReSet != null && dsReSet.Tables.Count > 0 && dsReSet.Tables[0].Rows.Count > 0)
                            {
                                for (int m = 0; m < dsReSet.Tables[0].Rows.Count; m++)
                                {
                                    clsInterfaceReSet cReSet = new clsInterfaceReSet();

                                    cReSet.sqlname = dsReSet.Tables[0].Rows[m]["sqlname"].ToString().Trim();

                                    cReSet.isloop = dsReSet.Tables[0].Rows[m]["isloop"].ToString().Trim();

                                    cReSet.loopwith = dsReSet.Tables[0].Rows[m]["loopwith"].ToString().Trim();

                                    cReSet.runtype = dsReSet.Tables[0].Rows[m]["runtype"].ToString().Trim();

                                    cReSet.ifhave = dsReSet.Tables[0].Rows[m]["ifhave"].ToString().Trim();

                                    cReSet.reorupdate = dsReSet.Tables[0].Rows[m]["reorupdate"].ToString().Trim();

                                    cReSet.tablename = dsReSet.Tables[0].Rows[m]["tablename"].ToString().Trim();

                                    cReSet.fathersql = dsReSet.Tables[0].Rows[m]["fathersql"].ToString().Trim();

                                    cReSet.looppoit = dsReSet.Tables[0].Rows[m]["looppoit"].ToString().Trim();

                                    cReSet.isDesc = dsReSet.Tables[0].Rows[m]["isDesc"].ToString().Trim();

                                    cReSet.IReTabSet = new List<clsInterfaceReTableSet>();

                                    DataSet dsReTabSet = da.GetReTableSet(ISet.SubName, ISet.ChiName, cReSet.sqlname);

                                    if (dsReTabSet != null && dsReTabSet.Tables.Count > 0 && dsReTabSet.Tables[0].Rows.Count > 0)
                                    {
                                        for (int n = 0; n < dsReTabSet.Tables[0].Rows.Count; n++)
                                        {
                                            clsInterfaceReTableSet cReTabSet = new clsInterfaceReTableSet();

                                            cReTabSet.colname = dsReTabSet.Tables[0].Rows[n]["colname"].ToString().Trim();

                                            cReTabSet.iskey = dsReTabSet.Tables[0].Rows[n]["iskey"].ToString().Trim();

                                            cReTabSet.databy = dsReTabSet.Tables[0].Rows[n]["databy"].ToString().Trim();

                                            cReTabSet.valuefrom = dsReTabSet.Tables[0].Rows[n]["valuefrom"].ToString().Trim();

                                            cReTabSet.staticvalue = dsReTabSet.Tables[0].Rows[n]["staticvalue"].ToString().Trim();

                                            cReTabSet.isDate = dsReTabSet.Tables[0].Rows[n]["isDate"].ToString().Trim();

                                            cReTabSet.dateType = dsReTabSet.Tables[0].Rows[n]["dateType"].ToString().Trim();

                                            cReTabSet.canUpdate = dsReTabSet.Tables[0].Rows[n]["canupdate"].ToString().Trim();

                                            cReSet.IReTabSet.Add(cReTabSet);
                                        }
                                    }

                                    ISet.IReSet.Add(cReSet);
                                }
                            }


                            InterFaceSet.Add(ISet);

                            lvSub.Invoke(new EventHandler(delegate
                            {
                                string[] strItem = { dt.Rows[i]["chiname"].ToString().Trim(), dt.Rows[i]["subname"].ToString().Trim(), dt.Rows[i]["classname"].ToString().Trim(), dt.Rows[i]["webaddres"].ToString().Trim(), tmp_autorun };

                                //, dt.Rows[i]["inparanum"].ToString().Trim(), dt.Rows[i]["outparanum"].ToString().Trim(), dt.Rows[i]["rewsql"].ToString().Trim(), dt.Rows[i]["basesql"].ToString().Trim(), dt.Rows[i]["btablename"].ToString().Trim()
                                lvSub.Items.Insert(lvSub.Items.Count, new ListViewItem(strItem));

                                //Thread.Sleep(100);

                            }));
                        }
                    }
                }

                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText("接口信息加载完成\r\n");

                    rthText.ScrollToCaret();

                }));

                bComplete = true;
                bTriggerNow = false;
                bTriggerFirst = true;

                if (autoRun == "1")
                {
                    btnRun.Invoke(new EventHandler(delegate
                    {
                        //string strIp = txtIP.Text.ToString();

                        //if (!string.IsNullOrEmpty(strIp))
                        //{
                        //    btnAgent.PerformClick();
                        //}

                        btnRun.PerformClick();
                    }));
                }
            }
            catch (UserException ex)
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(ex.Message);

                    rthText.ScrollToCaret();
                }));
            }
            catch (Exception ex)
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(ex.Message);

                    rthText.ScrollToCaret();
                }));
            }
        }
        #endregion "读取函数列表，并绑定到界面控件"

        #region "自动运行，执行接口函数"
        private void ExecInterface()
        {
            //DAOInterface di = DAOFactory.CreateDAOInterface();

            //string strStep = "";
            string strSubName = "";
            string subChiName = "";

            while (bRun)
            {
                try
                {
                    if ((!bRuNow && (iCount >= iWaitTime)) || bFirst)
                    {
                        bFirst = false;
                        bRuNow = true;

                        iCount = 0;

                        lblCount.Invoke(new EventHandler(delegate
                        {
                            lblCount.Text = "0";
                        }));

                        rthText.Invoke(new EventHandler(delegate
                        {
                            rthText.Text = "";
                        }));

                        if (!bCheckConnAtOpen)
                        {
                            if (!string.IsNullOrEmpty(clsGolbal.GetDLLAppKey("HydeeConn")))
                            {
                                DAOInterface di = DAOFactory.CreateDAOInterface();

                                try
                                {
                                    if (!di.TestConn())
                                    {
                                        rthText.Invoke(new EventHandler(delegate
                                        {
                                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->:数据库连接不上，请检查数据库连接配置或者网络" + "\r\n");

                                            rthText.ScrollToCaret();

                                            CallDoEvents();
                                        }));

                                        Thread.Sleep(1000);

                                        GC.Collect();

                                        continue;
                                        //MessageBox.Show("数据库连接不上，请检查数据库连接配置或者网络");

                                        //return;
                                    }
                                }
                                catch
                                {
                                    rthText.Invoke(new EventHandler(delegate
                                    {
                                        rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->:数据库连接不上，请检查数据库连接配置或者网络" + "\r\n");

                                        rthText.ScrollToCaret();

                                        CallDoEvents();
                                    }));

                                    Thread.Sleep(1000);

                                    GC.Collect();

                                    continue;
                                    //MessageBox.Show("数据库连接不上，请检查数据库连接配置或者网络");
                                    //return;
                                }

                                //di.CreateTempTable();
                            }
                            else
                            {
                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->:请先配置数据库连接" + "\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));

                                //Thread.Sleep(1000);

                                GC.Collect();

                                return;
                                //MessageBox.Show("请先配置数据库连接");
                            }
                        }
                        //strStep = "开始循环接口";
                        for (int il = 0; il < lvSub.Items.Count; il++)
                        {
                            lvSub.Invoke(new EventHandler(delegate
                            {
                                lvSub.Items[il].BackColor = Color.LightBlue;

                                if (il > 0)
                                {
                                    lvSub.Items[il - 1].BackColor = Color.White;
                                }


                                //strStep = "开始循环接口:" + il.ToString();

                                //读取接口函数名称
                                strSubName = lvSub.Items[il].SubItems[1].Text.ToString().Trim();

                                subChiName = lvSub.Items[il].SubItems[0].Text.ToString().Trim();
                            }));

                            int iSub = InterFaceSet.FindIndex(delegate(clsInterfaceSet p) { return p.SubName == strSubName && p.ChiName == subChiName; });

                            if (InterFaceSet[iSub].autoRun == "否")
                            {
                                Thread.Sleep(1000);
                                CallDoEvents();
                                continue;
                            }

                            //frmDataTrans frm = new frmDataTrans();

                            //frm.strSubName = strSubName;
                            //frm.subChiName = subChiName;
                            //frm.iSub = iSub;
                            //frm.bTest = false;
                            //frm.softvar = softvar;

                            //frm.InterFaceSet = InterFaceSet;
                            //frm.InterFaceReSet = InterFaceReSet;

                            //frm.ShowDialog();

                            ////frm.DataTrans();

                            ////frm.Close();

                            //frm.Dispose();
                            string subType = InterFaceSet[iSub].SubType;

                            if (string.IsNullOrEmpty(subType) || subType == "webservice")
                            {
                                //WebService接口执行
                                RunWebServiceSub(strSubName, subChiName, iSub, false, false);
                            }
                            else
                            {
                                RunHttpSub(strSubName, subChiName, iSub, false, false);
                            }

                            Thread.Sleep(500);
                        }

                        lvSub.Invoke(new EventHandler(delegate
                        {
                            lvSub.Items[lvSub.Items.Count - 1].BackColor = Color.White;

                        }));

                        bRuNow = false;

                        btnRun.Invoke(new EventHandler(delegate
                        {
                            if (btnRun.Text.ToString().Trim() == "正在停止")
                            {
                                btnRun.Enabled = true;

                                btnRun.Text = "点击停止";
                            }
                        }));
                    }

                }
                catch (Exception ex)
                {
                    rthText.Invoke(new EventHandler(delegate
                    {
                        rthText.AppendText(ex.Message);

                        rthText.ScrollToCaret();
                    }));

                    bRuNow = false;
                    iCount = 0;
                }

                Thread.Sleep(1000);

                GC.Collect();

                //SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        #endregion "自动运行，执行接口函数"

        #region "非自动运行的情况下，执行选中函数的按钮"
        private void btnRunSelectSub_Click(object sender, EventArgs e)
        {
            try
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    btnRunSelectSub.Enabled = false;
                    btnTest.Enabled = false;
                    if (!chbusexml.Checked)
                    {
                        rthText.Text = "";

                        rthText.ScrollToCaret();
                    }
                }));

                threadSingle = new Thread(new ThreadStart(RunSingleInterface));

                threadSingle.Start();
            }
            catch (Exception ex)
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    btnTest.Enabled = true;
                    btnRunSelectSub.Enabled = true;

                    rthText.AppendText(ex.Message + "\r\n");

                    rthText.ScrollToCaret();
                }));
            }
        }

        private void RunSingleInterface()
        {
            bSingle = true;

            try
            {
                DAOInterface di = DAOFactory.CreateDAOInterface();

                lvSub.Invoke(new EventHandler(delegate
                {
                    for (int il = 0; il < lvSub.SelectedItems.Count; il++)
                    {
                        string strSubName = lvSub.SelectedItems[il].SubItems[1].Text.ToString().Trim();//lvSub.Items[il].SubItems[1].Text.ToString().Trim();

                        string subChiName = lvSub.SelectedItems[il].SubItems[0].Text.ToString().Trim();

                        int iSub = InterFaceSet.FindIndex(delegate(clsInterfaceSet p) { return p.SubName == strSubName && p.ChiName == subChiName; });

                        string subType = InterFaceSet[iSub].SubType;

                        if (string.IsNullOrEmpty(subType) || subType == "webservice")
                        {
                            //WebService接口执行
                            RunWebServiceSub(strSubName, subChiName, iSub, false, false);
                        }
                        else
                        {
                            RunHttpSub(strSubName, subChiName, iSub, false, false);
                        }

                    }

                    btnRunSelectSub.Enabled = true;
                    btnTest.Enabled = true;
                }));
            }
            catch (Exception ex)
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    btnTest.Enabled = true;
                    btnRunSelectSub.Enabled = true;

                    rthText.AppendText(ex.Message + "\r\n");

                    rthText.ScrollToCaret();
                }));
            }
            finally
            {
                threadSingle.Abort();
            }

            bSingle = false;
        }

        #endregion "非自动运行的情况下，执行选中函数的按钮"

        #region "整合回写SQL语句"
        /// <summary>
        /// 将接口接收数据整合成SQL语句集合
        /// </summary>
        /// <param name="strXml">接收到的XML字符串</param>
        /// <param name="reSet">语句配置</param>
        /// <returns>SQL语句集合</returns>
        private List<string> CreateReWriteSql(string strXml, clsInterfaceReSet reSet)
        {
            List<string> listSql = new List<string>();

            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            try
            {
                #region "数据需要循环"
                //语句是否循环
                if (reSet.isloop == "1")
                {
                    #region "循环依据为返回数据"
                    if (reSet.loopwith == "返回数据")
                    {
                        //如果语句需要循环，则通过配置的循环节点，获取XML Node List
                        string loopNode = reSet.looppoit;

                        //XmlNodeList xnl = XmlHelper.GetNodeList(strXml, loopNode);
                        XmlNodeList xnl = GetRevXmlNodes(strXml, loopNode);

                        clsXmlStu cxs = new clsXmlStu();

                        cxs.nodeList = xnl;
                        cxs.iRows = 0;
                        cxs.titleName = loopNode;

                        //listXmlNode.Add(cxs);

                        int iXmlAt = listXmlNode.FindIndex(delegate(clsXmlStu p) { return p.titleName == loopNode; });

                        if (iXmlAt >= 0)
                        {
                            listXmlNode.RemoveAt(iXmlAt);
                        }

                        listXmlNode.Add(cxs);

                        iXmlAt = listXmlNode.FindIndex(delegate(clsXmlStu p) { return p.titleName == loopNode; });

                        //循环
                        for (int i = 0; i < xnl.Count; i++)
                        {
                            iXmlAt = listXmlNode.FindIndex(delegate (clsXmlStu p) { return p.titleName == loopNode; });
                            listXmlNode[iXmlAt].iRows = i;

                            #region "执行insert"
                            if (reSet.runtype.ToUpper() == "INSERT")
                            {
                                #region "判断数据是否在表中已经存在（根据设置的主键）"
                                if (reSet.ifhave == "1")
                                {
                                    #region "如果数据已经存在则跳过"
                                    if (reSet.reorupdate == "跳过")
                                    {
                                        sb = new StringBuilder();
                                        sb2 = new StringBuilder();

                                        if (softvar == "2")
                                        {
                                            sb.Append("declare v_count number; begin ");
                                            sb.Append("select count(1) into v_count from ");
                                        }
                                        else
                                        {
                                            sb.Append("if not exists(select 1 from ");
                                        }
                                        sb.Append(reSet.tablename + " ");
                                        //List<clsTableStru> iTableHave = listTable.FindAll(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); }); //.FindIndex(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); });
                                        //查找所有主键标签
                                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                        sb.Append("where ");

                                        for (int m = 0; m < listRt.Count; m++)
                                        {
                                            sb.Append(listRt[m].colname);
                                            sb.Append("=");
                                            //TODO:这里应该添加判断字段类型，字符、数字、日期，以判断是否需要添加引号，或者将值内容进行日期时间的类型转换

                                            sb.Append(DetailSB(strXml, listRt[m], softvar, i));

                                            if (m < listRt.Count - 1)
                                            {
                                                sb.Append(" and ");
                                            }
                                        }

                                        if (softvar == "2")
                                        {
                                            sb.Append("; ");

                                            sb.Append("if v_count <=0 then ");
                                        }
                                        else
                                        {
                                            sb.Append(")");

                                            sb.Append(" begin ");
                                        }
                                        sb.Append("insert into ");
                                        sb.Append(reSet.tablename);
                                        sb.Append("(");

                                        for (int m = 0; m < reSet.IReTabSet.Count; m++)
                                        {
                                            sb.Append(reSet.IReTabSet[m].colname);

                                            if (m < reSet.IReTabSet.Count - 1)
                                            {
                                                sb.Append(",");
                                            }

                                            sb2.Append(DetailSB(strXml, reSet.IReTabSet[m], softvar, i));

                                            if (m < reSet.IReTabSet.Count - 1)
                                            {
                                                sb2.Append(",");
                                            }
                                        }

                                        sb.Append(")");
                                        sb.Append(" values");
                                        sb.Append("(");

                                        sb.Append(sb2);

                                        sb.Append(")");

                                        if (softvar == "2")
                                        {
                                            sb.Append("; end if; end;");
                                        }
                                        else
                                        {
                                            sb.Append(" end");
                                        }

                                        //listSql.Add(sb.ToString());

                                        #region "子语句整合"
                                        //判断是否有子语句
                                        List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return p.fathersql == reSet.sqlname; });

                                        if (tmpList.Count > 0)
                                        {
                                            bool bInserted = false;

                                            for (int iC = 0; iC < tmpList.Count; iC++)
                                            {
                                                List<string> tmpSql = CreateReWriteSql(strXml, tmpList[iC]);

                                                if (tmpList[iC].isDesc == "0" && !bInserted)
                                                {
                                                    listSql.Add(sb.ToString());

                                                    bInserted = true;
                                                }

                                                listSql.AddRange(tmpSql);
                                            }

                                            if (!bInserted)
                                            {
                                                listSql.Add(sb.ToString());

                                                bInserted = true;
                                            }
                                        }
                                        else
                                        {
                                            listSql.Add(sb.ToString());
                                        }
                                        #endregion "子语句整合"
                                    }
                                    #endregion "如果数据已经存在则跳过"
                                    else if (reSet.reorupdate == "删插")
                                    #region "如果数据已经存在则先删除，再插入"
                                    {
                                        sb = new StringBuilder();
                                        sb2 = new StringBuilder();

                                        StringBuilder sb3 = new StringBuilder();

                                        if (softvar == "2")
                                        {
                                            sb.Append("declare v_count number; begin ");
                                            sb.Append("select count(1) into v_count from ");
                                        }
                                        else
                                        {
                                            sb.Append("if exists(select 1 from ");
                                        }

                                        sb3.Append(reSet.tablename + " ");
                                        //List<clsTableStru> iTableHave = listTable.FindAll(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); }); //.FindIndex(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); });
                                        //查找所有主键标签
                                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                        sb3.Append("where ");

                                        for (int m = 0; m < listRt.Count; m++)
                                        {
                                            sb3.Append(listRt[m].colname);
                                            sb3.Append("=");
                                            //TODO:这里应该添加判断字段类型，字符、数字、日期，以判断是否需要添加引号，或者将值内容进行日期时间的类型转换
                                            //sb3.Append("'");
                                            sb3.Append(DetailSB(strXml, listRt[m], softvar, i));

                                            if (m < listRt.Count - 1)
                                            {
                                                sb3.Append(" and ");
                                            }
                                        }

                                        sb.Append(sb3);

                                        if (softvar == "2")
                                        {
                                            sb.Append("; ");
                                            sb.Append("if v_count>0 then ");
                                        }
                                        else
                                        {
                                            sb.Append(")");

                                            sb.Append(" begin ");
                                        }
                                        sb.Append("delete from ");

                                        sb.Append(sb3);

                                        if (softvar == "2")
                                        {
                                            sb.Append("; end if; ");
                                        }
                                        else
                                        {
                                            sb.Append(" end");
                                        }

                                        sb.Append(" insert into ");
                                        sb.Append(reSet.tablename);
                                        sb.Append("(");

                                        for (int m = 0; m < reSet.IReTabSet.Count; m++)
                                        {
                                            sb.Append(reSet.IReTabSet[m].colname);

                                            if (m < reSet.IReTabSet.Count - 1)
                                            {
                                                sb.Append(",");
                                            }

                                            sb2.Append(DetailSB(strXml, reSet.IReTabSet[m], softvar, i));
                                            //sb2.Append("'");

                                            if (m < reSet.IReTabSet.Count - 1)
                                            {
                                                sb2.Append(",");
                                            }
                                        }

                                        sb.Append(")");
                                        sb.Append(" values");
                                        sb.Append("(");

                                        sb.Append(sb2);

                                        sb.Append(")");

                                        if (softvar == "2")
                                        {
                                            sb.Append("; end;");
                                        }

                                        //listSql.Add(sb.ToString());

                                        #region "子语句整合"
                                        //判断是否有子语句
                                        List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return p.fathersql == reSet.sqlname; });

                                        if (tmpList.Count > 0)
                                        {
                                            bool bInserted = false;

                                            for (int iC = 0; iC < tmpList.Count; iC++)
                                            {
                                                List<string> tmpSql = CreateReWriteSql(strXml, tmpList[iC]);

                                                if (tmpList[iC].isDesc == "0" && !bInserted)
                                                {
                                                    listSql.Add(sb.ToString());

                                                    bInserted = true;
                                                }

                                                listSql.AddRange(tmpSql);
                                            }

                                            if (!bInserted)
                                            {
                                                listSql.Add(sb.ToString());

                                                bInserted = true;
                                            }
                                        }
                                        else
                                        {
                                            listSql.Add(sb.ToString());
                                        }
                                        #endregion "子语句整合"
                                    }
                                    #endregion "如果数据已经存在则先删除，再插入"
                                    else
                                    #region "根据主键，修改设置的可修改字段"
                                    {
                                        sb = new StringBuilder();
                                        sb2 = new StringBuilder();

                                        StringBuilder sb3 = new StringBuilder();

                                        if (softvar == "2")
                                        {
                                            sb.Append("declare v_count number; begin ");
                                            sb.Append("select count(1) into v_count from ");
                                        }
                                        else
                                        {
                                            sb.Append("if exists(select 1 from ");
                                        }

                                        sb.Append(reSet.tablename + " ");

                                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                        sb3.Append(" where ");

                                        #region "循环主键字段，整合where条件   sb3"
                                        for (int m = 0; m < listRt.Count; m++)
                                        {
                                            sb3.Append(listRt[m].colname);
                                            sb3.Append("=");

                                            sb3.Append(DetailSB(strXml, listRt[m], softvar, i));

                                            if (m < listRt.Count - 1)
                                            {
                                                sb3.Append(" and ");
                                            }
                                        }
                                        #endregion "循环主键字段，整合where条件"

                                        sb.Append(sb3);

                                        if (softvar == "2")
                                        {
                                            sb.Append("; ");
                                            sb.Append("if v_count>0 then ");
                                        }
                                        else
                                        {
                                            sb.Append(")");

                                            sb.Append(" begin ");
                                        }

                                        sb.Append("update ");
                                        sb.Append(reSet.tablename);
                                        sb.Append(" set ");

                                        #region "获取并整合SET部分内容"
                                        List<clsInterfaceReTableSet> listCanUpdate = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.canUpdate == "1"; });

                                        for (int ic = 0; ic < listCanUpdate.Count; ic++)
                                        {
                                            sb.Append(listCanUpdate[ic].colname);
                                            sb.Append("=");

                                            sb.Append(DetailSB(strXml, listCanUpdate[ic], softvar, i));

                                            if (ic < listCanUpdate.Count - 1)
                                            {
                                                sb.Append(",");
                                            }

                                        }

                                        #endregion "获取并整合SET部分内容"

                                        //添加where部分内容
                                        sb.Append(sb3);

                                        if (softvar == "2")
                                        {
                                            sb.Append("; else ");
                                        }
                                        else
                                        {
                                            sb.Append(" end else begin ");
                                        }

                                        sb.Append(" insert into ");
                                        sb.Append(reSet.tablename);
                                        sb.Append("(");

                                        for (int m = 0; m < reSet.IReTabSet.Count; m++)
                                        {
                                            sb.Append(reSet.IReTabSet[m].colname);

                                            if (m < reSet.IReTabSet.Count - 1)
                                            {
                                                sb.Append(",");
                                            }

                                            sb2.Append(DetailSB(strXml, reSet.IReTabSet[m], softvar, i));

                                            if (m < reSet.IReTabSet.Count - 1)
                                            {
                                                sb2.Append(",");
                                            }
                                        }

                                        sb.Append(")");
                                        sb.Append(" values");
                                        sb.Append("(");

                                        sb.Append(sb2);

                                        sb.Append(")");

                                        if (softvar == "2")
                                        {
                                            sb.Append("; end if; end;");
                                        }
                                        else
                                        {
                                            sb.Append(" end");
                                        }

                                        #region "子语句整合"
                                        //判断是否有子语句
                                        List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return p.fathersql == reSet.sqlname; });

                                        if (tmpList.Count > 0)
                                        {
                                            bool bInserted = false;

                                            for (int iC = 0; iC < tmpList.Count; iC++)
                                            {
                                                List<string> tmpSql = CreateReWriteSql(strXml, tmpList[iC]);

                                                if (tmpList[iC].isDesc == "0" && !bInserted)
                                                {
                                                    listSql.Add(sb.ToString());

                                                    bInserted = true;
                                                }

                                                listSql.AddRange(tmpSql);
                                            }

                                            if (!bInserted)
                                            {
                                                listSql.Add(sb.ToString());

                                                bInserted = true;
                                            }
                                        }
                                        else
                                        {
                                            listSql.Add(sb.ToString());
                                        }
                                        #endregion "子语句整合"
                                    }
                                    #endregion "根据主键，修改设置的可修改字段"
                                }
                                #endregion "判断数据是否在表中已经存在（根据设置的主键）"
                                else
                                #region "不需要判断数据是否在表中已经存在"
                                {
                                    sb = new StringBuilder();
                                    sb2 = new StringBuilder();


                                    sb.Append("insert into ");
                                    sb.Append(reSet.tablename);
                                    sb.Append("(");

                                    for (int m = 0; m < reSet.IReTabSet.Count; m++)
                                    {
                                        sb.Append(reSet.IReTabSet[m].colname);

                                        if (m < reSet.IReTabSet.Count - 1)
                                        {
                                            sb.Append(",");
                                        }

                                        sb2.Append(DetailSB(strXml, reSet.IReTabSet[m], softvar, i));

                                        if (m < reSet.IReTabSet.Count - 1)
                                        {
                                            sb2.Append(",");
                                        }
                                    }

                                    sb.Append(")");
                                    sb.Append(" values");
                                    sb.Append("(");

                                    sb.Append(sb2);

                                    sb.Append(")");

                                    //20180627 单语句去掉最后的分号
                                    //if (softvar == "2")
                                    //{
                                    //    sb.Append(";");
                                    //}

                                    //listSql.Add(sb.ToString());

                                    #region "子语句整合"
                                    //判断是否有子语句
                                    List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return p.fathersql == reSet.sqlname; });

                                    if (tmpList.Count > 0)
                                    {
                                        bool bInserted = false;

                                        for (int iC = 0; iC < tmpList.Count; iC++)
                                        {
                                            List<string> tmpSql = CreateReWriteSql(strXml, tmpList[iC]);

                                            if (tmpList[iC].isDesc == "0" && !bInserted)
                                            {
                                                listSql.Add(sb.ToString());

                                                bInserted = true;
                                            }

                                            listSql.AddRange(tmpSql);
                                        }

                                        if (!bInserted)
                                        {
                                            listSql.Add(sb.ToString());

                                            bInserted = true;
                                        }
                                    }
                                    else
                                    {
                                        listSql.Add(sb.ToString());
                                    }
                                    #endregion "子语句整合"
                                }
                                #endregion "不需要判断数据是否在表中已经存在"}
                            }
                            #endregion "执行insert"
                            else
                            #region "执行update"
                            {
                                List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                List<clsInterfaceReTableSet> listRtN = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey != "1"; });

                                sb = new StringBuilder();
                                sb2 = new StringBuilder();

                                sb.Append("update ");
                                sb.Append(reSet.tablename);
                                sb.Append(" set ");

                                for (int m = 0; m < listRtN.Count; m++)
                                {
                                    sb.Append(listRtN[m].colname);
                                    sb.Append("=");
                                    //sb.Append("'");
                                    sb.Append(DetailSB(strXml, listRtN[m], softvar, i));

                                    if (m < listRtN.Count - 1)
                                    {
                                        sb.Append(",");
                                    }
                                }

                                sb.Append(" where ");

                                for (int m = 0; m < listRt.Count; m++)
                                {
                                    sb.Append(listRt[m].colname);
                                    sb.Append("=");
                                    //sb.Append("'");

                                    sb.Append(DetailSB(strXml, listRt[m], softvar, i));

                                    if (m < listRt.Count - 1)
                                    {
                                        sb.Append(" and ");
                                    }
                                }

                                //20180627 单语句去掉最后的分号
                                //if (softvar == "2")
                                //{
                                //    sb.Append(";");
                                //}

                                //listSql.Add(sb.ToString());

                                #region "子语句整合"
                                //判断是否有子语句
                                List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return p.fathersql == reSet.sqlname; });

                                if (tmpList.Count > 0)
                                {
                                    bool bInserted = false;

                                    for (int iC = 0; iC < tmpList.Count; iC++)
                                    {
                                        List<string> tmpSql = CreateReWriteSql(strXml, tmpList[iC]);

                                        if (tmpList[iC].isDesc == "0" && !bInserted)
                                        {
                                            listSql.Add(sb.ToString());

                                            bInserted = true;
                                        }

                                        listSql.AddRange(tmpSql);
                                    }

                                    if (!bInserted)
                                    {
                                        listSql.Add(sb.ToString());

                                        bInserted = true;
                                    }
                                }
                                else
                                {
                                    listSql.Add(sb.ToString());
                                }
                                #endregion "子语句整合"
                            }
                            #endregion "执行update"
                        }
                    }
                    #endregion "循环依据为返回数据"
                    else
                    #region "循环依据为发送数据"
                    {
                        string tabName = reSet.looppoit;

                        int iTableT = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == tabName; });

                        listTable[iTableT].iRows = 0;

                        for (int i = 0; i < listTable[iTableT].table.Rows.Count; i++)
                        {
                            listTable[iTableT].iRows = i;

                            #region "执行insert"
                            if (reSet.runtype.ToUpper() == "INSERT")
                            {
                                #region "判断数据是否在表中已经存在（根据设置的主键）"
                                if (reSet.ifhave == "1")
                                {
                                    #region "如果数据已经存在则跳过"
                                    if (reSet.reorupdate == "跳过")
                                    {
                                        sb = new StringBuilder();
                                        sb2 = new StringBuilder();

                                        if (softvar == "2")
                                        {
                                            sb.Append("declare v_count number; begin ");
                                            sb.Append("select count(1) into v_count from ");
                                        }
                                        else
                                        {
                                            sb.Append("if not exists(select 1 from ");
                                        }
                                        sb.Append(reSet.tablename + " ");
                                        //List<clsTableStru> iTableHave = listTable.FindAll(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); }); //.FindIndex(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); });
                                        //查找所有主键标签
                                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                        sb.Append("where ");

                                        for (int m = 0; m < listRt.Count; m++)
                                        {
                                            sb.Append(listRt[m].colname);
                                            sb.Append("=");
                                            //TODO:这里应该添加判断字段类型，字符、数字、日期，以判断是否需要添加引号，或者将值内容进行日期时间的类型转换
                                            sb.Append(DetailSB(strXml, listRt[m], softvar, i));

                                            if (m < listRt.Count - 1)
                                            {
                                                sb.Append(" and ");
                                            }
                                        }

                                        if (softvar == "2")
                                        {
                                            sb.Append("; ");

                                            sb.Append("if v_count <=0 then ");
                                        }
                                        else
                                        {
                                            sb.Append(")");

                                            sb.Append(" begin ");
                                        }
                                        sb.Append("insert into ");
                                        sb.Append(reSet.tablename);
                                        sb.Append("(");

                                        for (int m = 0; m < reSet.IReTabSet.Count; m++)
                                        {
                                            sb.Append(reSet.IReTabSet[m].colname);

                                            if (m < reSet.IReTabSet.Count - 1)
                                            {
                                                sb.Append(",");
                                            }

                                            sb2.Append(DetailSB(strXml, reSet.IReTabSet[m], softvar, i));

                                            if (m < reSet.IReTabSet.Count - 1)
                                            {
                                                sb2.Append(",");
                                            }
                                        }

                                        sb.Append(")");
                                        sb.Append(" values");
                                        sb.Append("(");

                                        sb.Append(sb2);

                                        sb.Append(")");

                                        if (softvar == "2")
                                        {
                                            sb.Append("; end if; end;");
                                        }
                                        else
                                        {
                                            sb.Append(" end");
                                        }
                                        //listSql.Add(sb.ToString());

                                        #region "子语句整合"
                                        //判断是否有子语句
                                        List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return p.fathersql == reSet.sqlname; });

                                        if (tmpList.Count > 0)
                                        {
                                            bool bInserted = false;

                                            for (int iC = 0; iC < tmpList.Count; iC++)
                                            {
                                                List<string> tmpSql = CreateReWriteSql(strXml, tmpList[iC]);

                                                if (tmpList[iC].isDesc == "0" && !bInserted)
                                                {
                                                    listSql.Add(sb.ToString());

                                                    bInserted = true;
                                                }

                                                listSql.AddRange(tmpSql);
                                            }

                                            if (!bInserted)
                                            {
                                                listSql.Add(sb.ToString());

                                                bInserted = true;
                                            }
                                        }
                                        else
                                        {
                                            listSql.Add(sb.ToString());
                                        }
                                        #endregion "子语句整合"
                                    }
                                    #endregion "如果数据已经存在则跳过"
                                    else if (reSet.reorupdate == "删插")
                                    #region "如果数据已经存在则先删除，再插入"
                                    {
                                        sb = new StringBuilder();
                                        sb2 = new StringBuilder();

                                        StringBuilder sb3 = new StringBuilder();

                                        if (softvar == "2")
                                        {
                                            sb.Append("declare v_count number; begin ");
                                            sb.Append("select count(1) into v_count from ");
                                        }
                                        else
                                        {
                                            sb.Append("if exists(select 1 from ");
                                        }

                                        sb3.Append(reSet.tablename + " ");
                                        //List<clsTableStru> iTableHave = listTable.FindAll(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); }); //.FindIndex(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); });
                                        //查找所有主键标签
                                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                        sb3.Append("where ");

                                        for (int m = 0; m < listRt.Count; m++)
                                        {
                                            sb3.Append(listRt[m].colname);
                                            sb3.Append("=");
                                            //TODO:这里应该添加判断字段类型，字符、数字、日期，以判断是否需要添加引号，或者将值内容进行日期时间的类型转换
                                            //sb3.Append("'");
                                            sb.Append(DetailSB(strXml, listRt[m], softvar, i));

                                            if (m < listRt.Count - 1)
                                            {
                                                sb3.Append(" and ");
                                            }
                                        }
                                        sb.Append(sb3);

                                        if (softvar == "2")
                                        {
                                            sb.Append("; ");
                                            sb.Append("if v_count>0 then ");
                                        }
                                        else
                                        {
                                            sb.Append(")");

                                            sb.Append(" begin ");
                                        }
                                        sb.Append("delete from ");

                                        sb.Append(sb3);

                                        if (softvar == "2")
                                        {
                                            sb.Append("; end if; ");
                                        }
                                        else
                                        {
                                            sb.Append(" end");
                                        }

                                        sb.Append(" insert into ");
                                        sb.Append(reSet.tablename);
                                        sb.Append("(");

                                        for (int m = 0; m < reSet.IReTabSet.Count; m++)
                                        {
                                            sb.Append(reSet.IReTabSet[m].colname);

                                            if (m < reSet.IReTabSet.Count - 1)
                                            {
                                                sb.Append(",");
                                            }

                                            sb2.Append(DetailSB(strXml, reSet.IReTabSet[m], softvar, i));
                                            //sb2.Append("'");

                                            if (m < reSet.IReTabSet.Count - 1)
                                            {
                                                sb2.Append(",");
                                            }
                                        }

                                        sb.Append(")");
                                        sb.Append(" values");
                                        sb.Append("(");

                                        sb.Append(sb2);

                                        sb.Append(")");

                                        if (softvar == "2")
                                        {
                                            sb.Append("; end;");
                                        }

                                        //listSql.Add(sb.ToString());
                                        #region "子语句整合"
                                        //判断是否有子语句
                                        List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return p.fathersql == reSet.sqlname; });

                                        if (tmpList.Count > 0)
                                        {
                                            bool bInserted = false;

                                            for (int iC = 0; iC < tmpList.Count; iC++)
                                            {
                                                List<string> tmpSql = CreateReWriteSql(strXml, tmpList[iC]);

                                                if (tmpList[iC].isDesc == "0" && !bInserted)
                                                {
                                                    listSql.Add(sb.ToString());

                                                    bInserted = true;
                                                }

                                                listSql.AddRange(tmpSql);
                                            }

                                            if (!bInserted)
                                            {
                                                listSql.Add(sb.ToString());

                                                bInserted = true;
                                            }
                                        }
                                        else
                                        {
                                            listSql.Add(sb.ToString());
                                        }
                                        #endregion "子语句整合"
                                    }
                                    #endregion "如果数据已经存在则先删除，再插入"
                                    else
                                    #region "根据主键，修改设置的可修改字段"
                                    {
                                        sb = new StringBuilder();
                                        sb2 = new StringBuilder();

                                        StringBuilder sb3 = new StringBuilder();

                                        if (softvar == "2")
                                        {
                                            sb.Append("declare v_count number; begin ");
                                            sb.Append("select count(1) into v_count from ");
                                        }
                                        else
                                        {
                                            sb.Append("if exists(select 1 from ");
                                        }

                                        sb.Append(reSet.tablename + " ");

                                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                        sb3.Append(" where ");

                                        #region "循环主键字段，整合where条件   sb3"
                                        for (int m = 0; m < listRt.Count; m++)
                                        {
                                            sb3.Append(listRt[m].colname);
                                            sb3.Append("=");

                                            sb3.Append(DetailSB(strXml, listRt[m], softvar, i));

                                            if (m < listRt.Count - 1)
                                            {
                                                sb3.Append(" and ");
                                            }
                                        }
                                        #endregion "循环主键字段，整合where条件"

                                        sb.Append(sb3);

                                        if (softvar == "2")
                                        {
                                            sb.Append("; ");
                                            sb.Append("if v_count>0 then ");
                                        }
                                        else
                                        {
                                            sb.Append(")");

                                            sb.Append(" begin ");
                                        }

                                        sb.Append("update ");
                                        sb.Append(reSet.tablename);
                                        sb.Append(" set ");

                                        #region "获取并整合SET部分内容"
                                        List<clsInterfaceReTableSet> listCanUpdate = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.canUpdate == "1"; });

                                        for (int ic = 0; ic < listCanUpdate.Count; ic++)
                                        {
                                            sb.Append(listCanUpdate[ic].colname);
                                            sb.Append("=");

                                            sb.Append(DetailSB(strXml, listCanUpdate[ic], softvar, i));

                                            if (ic < listCanUpdate.Count - 1)
                                            {
                                                sb.Append(",");
                                            }

                                        }

                                        #endregion "获取并整合SET部分内容"

                                        //添加where部分内容
                                        sb.Append(sb3);

                                        if (softvar == "2")
                                        {
                                            sb.Append("; else ");
                                        }
                                        else
                                        {
                                            sb.Append(" end else begin ");
                                        }

                                        sb.Append(" insert into ");
                                        sb.Append(reSet.tablename);
                                        sb.Append("(");

                                        for (int m = 0; m < reSet.IReTabSet.Count; m++)
                                        {
                                            sb.Append(reSet.IReTabSet[m].colname);

                                            if (m < reSet.IReTabSet.Count - 1)
                                            {
                                                sb.Append(",");
                                            }

                                            //sb2.Append("'");
                                            sb2.Append(DetailSB(strXml, reSet.IReTabSet[m], softvar, i));

                                            if (m < reSet.IReTabSet.Count - 1)
                                            {
                                                sb2.Append(",");
                                            }
                                        }

                                        sb.Append(")");
                                        sb.Append(" values");
                                        sb.Append("(");

                                        sb.Append(sb2);

                                        sb.Append(")");

                                        if (softvar == "2")
                                        {
                                            sb.Append("; end if; end;");
                                        }
                                        else
                                        {
                                            sb.Append(" end");
                                        }

                                        #region "子语句整合"
                                        //判断是否有子语句
                                        List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return p.fathersql == reSet.sqlname; });

                                        if (tmpList.Count > 0)
                                        {
                                            bool bInserted = false;

                                            for (int iC = 0; iC < tmpList.Count; iC++)
                                            {
                                                List<string> tmpSql = CreateReWriteSql(strXml, tmpList[iC]);

                                                if (tmpList[iC].isDesc == "0" && !bInserted)
                                                {
                                                    listSql.Add(sb.ToString());

                                                    bInserted = true;
                                                }

                                                listSql.AddRange(tmpSql);
                                            }

                                            if (!bInserted)
                                            {
                                                listSql.Add(sb.ToString());

                                                bInserted = true;
                                            }
                                        }
                                        else
                                        {
                                            listSql.Add(sb.ToString());
                                        }
                                        #endregion "子语句整合"
                                    }
                                    #endregion "根据主键，修改设置的可修改字段"
                                }
                                #endregion "判断数据是否在表中已经存在（根据设置的主键）"
                                else
                                #region "不需要判断数据是否在表中已经存在"
                                {
                                    sb = new StringBuilder();
                                    sb2 = new StringBuilder();


                                    sb.Append("insert into ");
                                    sb.Append(reSet.tablename);
                                    sb.Append("(");

                                    for (int m = 0; m < reSet.IReTabSet.Count; m++)
                                    {
                                        sb.Append(reSet.IReTabSet[m].colname);

                                        if (m < reSet.IReTabSet.Count - 1)
                                        {
                                            sb.Append(",");
                                        }

                                        //sb2.Append("'");
                                        sb2.Append(DetailSB(strXml, reSet.IReTabSet[m], softvar, i));

                                        if (m < reSet.IReTabSet.Count - 1)
                                        {
                                            sb2.Append(",");
                                        }
                                    }

                                    sb.Append(")");
                                    sb.Append(" values");
                                    sb.Append("(");

                                    sb.Append(sb2);

                                    sb.Append(")");

                                    //20180627 单语句去掉最后的分号
                                    //if (softvar == "2")
                                    //{
                                    //    sb.Append(";");
                                    //}

                                    //listSql.Add(sb.ToString());

                                    #region "子语句整合"
                                    //判断是否有子语句
                                    List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return p.fathersql == reSet.sqlname; });

                                    if (tmpList.Count > 0)
                                    {
                                        bool bInserted = false;

                                        for (int iC = 0; iC < tmpList.Count; iC++)
                                        {
                                            List<string> tmpSql = CreateReWriteSql(strXml, tmpList[iC]);

                                            if (tmpList[iC].isDesc == "0" && !bInserted)
                                            {
                                                listSql.Add(sb.ToString());

                                                bInserted = true;
                                            }

                                            listSql.AddRange(tmpSql);
                                        }

                                        if (!bInserted)
                                        {
                                            listSql.Add(sb.ToString());

                                            bInserted = true;
                                        }
                                    }
                                    else
                                    {
                                        listSql.Add(sb.ToString());
                                    }
                                    #endregion "子语句整合"
                                }
                                #endregion "不需要判断数据是否在表中已经存在"
                            }
                            #endregion "执行insert"
                            else
                            #region "执行update"
                            {
                                List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                List<clsInterfaceReTableSet> listRtN = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey != "1"; });

                                sb = new StringBuilder();
                                sb2 = new StringBuilder();

                                sb.Append("update ");
                                sb.Append(reSet.tablename);
                                sb.Append(" set ");

                                for (int m = 0; m < listRtN.Count; m++)
                                {
                                    sb.Append(listRtN[m].colname);
                                    sb.Append("=");
                                    //sb.Append("'");
                                    sb.Append(DetailSB(strXml, listRtN[m], softvar, i));

                                    if (m < listRtN.Count - 1)
                                    {
                                        sb.Append(",");
                                    }
                                }

                                sb.Append(" where ");

                                for (int m = 0; m < listRt.Count; m++)
                                {
                                    sb.Append(listRt[m].colname);
                                    sb.Append("=");
                                    //sb.Append("'");
                                    sb.Append(DetailSB(strXml, listRt[m], softvar, i));

                                    if (m < listRt.Count - 1)
                                    {
                                        sb.Append(" and ");
                                    }
                                }

                                //20180627 单语句去掉最后的分号
                                //if (softvar == "2")
                                //{
                                //    sb.Append(";");
                                //}

                                //listSql.Add(sb.ToString());

                                #region "子语句整合"
                                //判断是否有子语句
                                List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return p.fathersql == reSet.sqlname; });

                                if (tmpList.Count > 0)
                                {
                                    bool bInserted = false;

                                    for (int iC = 0; iC < tmpList.Count; iC++)
                                    {
                                        List<string> tmpSql = CreateReWriteSql(strXml, tmpList[iC]);

                                        if (tmpList[iC].isDesc == "0" && !bInserted)
                                        {
                                            listSql.Add(sb.ToString());

                                            bInserted = true;
                                        }

                                        listSql.AddRange(tmpSql);
                                    }

                                    if (!bInserted)
                                    {
                                        listSql.Add(sb.ToString());

                                        bInserted = true;
                                    }
                                }
                                else
                                {
                                    listSql.Add(sb.ToString());
                                }
                                #endregion "子语句整合"
                            }
                            #endregion "执行update"
                        }
                    }
                    #endregion "循环依据为发送数据"

                }
                #endregion "数据需要循环"
                else
                #region "数据不需要循环"
                {
                    if (reSet.runtype.ToUpper() == "INSERT")
                    {
                        #region "判断数据是否在表中已经存在（根据设置的主键）"
                        if (reSet.ifhave == "1")
                        {
                            if (reSet.reorupdate == "跳过")
                            #region "如果数据已经存在则跳过"
                            {
                                sb = new StringBuilder();
                                sb2 = new StringBuilder();

                                if (softvar == "2")
                                {
                                    sb.Append("declare v_count number; begin ");
                                    sb.Append("select count(1) into v_count from ");
                                }
                                else
                                {
                                    sb.Append("if not exists(select 1 from ");
                                }
                                sb.Append(reSet.tablename + " ");
                                //List<clsTableStru> iTableHave = listTable.FindAll(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); }); //.FindIndex(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); });
                                //查找所有主键标签
                                List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                sb.Append("where ");

                                for (int m = 0; m < listRt.Count; m++)
                                {
                                    sb.Append(listRt[m].colname);
                                    sb.Append("=");
                                    //TODO:这里应该添加判断字段类型，字符、数字、日期，以判断是否需要添加引号，或者将值内容进行日期时间的类型转换
                                    sb.Append(DetailSB(strXml, listRt[m], softvar, 0));

                                    if (m < listRt.Count - 1)
                                    {
                                        sb.Append(" and ");
                                    }
                                }

                                if (softvar == "2")
                                {
                                    sb.Append("; ");

                                    sb.Append("if v_count <=0 then ");
                                }
                                else
                                {
                                    sb.Append(")");

                                    sb.Append(" begin ");
                                }
                                sb.Append("insert into ");
                                sb.Append(reSet.tablename);
                                sb.Append("(");

                                for (int m = 0; m < reSet.IReTabSet.Count; m++)
                                {
                                    sb.Append(reSet.IReTabSet[m].colname);

                                    if (m < reSet.IReTabSet.Count - 1)
                                    {
                                        sb.Append(",");
                                    }

                                    sb2.Append(DetailSB(strXml, reSet.IReTabSet[m], softvar, 0));

                                    if (m < reSet.IReTabSet.Count - 1)
                                    {
                                        sb2.Append(",");
                                    }
                                }

                                sb.Append(")");
                                sb.Append(" values");
                                sb.Append("(");

                                sb.Append(sb2);

                                sb.Append(")");

                                if (softvar == "2")
                                {
                                    sb.Append("; end if; end;");
                                }
                                else
                                {
                                    sb.Append(" end");
                                }
                                //listSql.Add(sb.ToString());
                            }
                            #endregion "如果数据已经存在则跳过"
                            else if (reSet.reorupdate == "删插")
                            #region "如果数据已经存在则先删除，再插入"
                            {
                                sb = new StringBuilder();
                                sb2 = new StringBuilder();

                                StringBuilder sb3 = new StringBuilder();

                                if (softvar == "2")
                                {
                                    sb.Append("declare v_count number; begin ");
                                    sb.Append("select count(1) into v_count from ");
                                }
                                else
                                {
                                    sb.Append("if exists(select 1 from ");
                                }

                                sb3.Append(reSet.tablename + " ");
                                //List<clsTableStru> iTableHave = listTable.FindAll(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); }); //.FindIndex(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); });
                                //查找所有主键标签
                                List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                sb3.Append("where ");

                                for (int m = 0; m < listRt.Count; m++)
                                {
                                    sb3.Append(listRt[m].colname);
                                    sb3.Append("=");
                                    //TODO:这里应该添加判断字段类型，字符、数字、日期，以判断是否需要添加引号，或者将值内容进行日期时间的类型转换
                                    sb3.Append(DetailSB(strXml, listRt[m], softvar, 0));

                                    if (m < listRt.Count - 1)
                                    {
                                        sb3.Append(" and ");
                                    }
                                }
                                sb.Append(sb3);

                                if (softvar == "2")
                                {
                                    sb.Append("; ");
                                    sb.Append("if v_count>0 then ");
                                }
                                else
                                {
                                    sb.Append(")");

                                    sb.Append(" begin ");
                                }
                                sb.Append("delete from ");

                                sb.Append(sb3);

                                if (softvar == "2")
                                {
                                    sb.Append("; end if; ");
                                }
                                else
                                {
                                    sb.Append(" end");
                                }

                                sb.Append(" insert into ");
                                sb.Append(reSet.tablename);
                                sb.Append("(");

                                for (int m = 0; m < reSet.IReTabSet.Count; m++)
                                {
                                    sb.Append(reSet.IReTabSet[m].colname);

                                    if (m < reSet.IReTabSet.Count - 1)
                                    {
                                        sb.Append(",");
                                    }

                                    sb2.Append(DetailSB(strXml, reSet.IReTabSet[m], softvar, 0));

                                    if (m < reSet.IReTabSet.Count - 1)
                                    {
                                        sb2.Append(",");
                                    }
                                }

                                sb.Append(")");
                                sb.Append(" values");
                                sb.Append("(");

                                sb.Append(sb2);

                                sb.Append(")");

                                if (softvar == "2")
                                {
                                    sb.Append("; end;");
                                }

                                //listSql.Add(sb.ToString());
                            }
                            #endregion "如果数据已经存在则先删除，再插入"
                            else
                            #region "根据主键，修改设置的可修改字段"
                            {
                                sb = new StringBuilder();
                                sb2 = new StringBuilder();

                                StringBuilder sb3 = new StringBuilder();

                                if (softvar == "2")
                                {
                                    sb.Append("declare v_count number; begin ");
                                    sb.Append("select count(1) into v_count from ");
                                }
                                else
                                {
                                    sb.Append("if exists(select 1 from ");
                                }

                                sb.Append(reSet.tablename + " ");

                                List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                sb3.Append(" where ");

                                #region "循环主键字段，整合where条件   sb3"
                                for (int m = 0; m < listRt.Count; m++)
                                {
                                    sb3.Append(listRt[m].colname);
                                    sb3.Append("=");

                                    sb3.Append(DetailSB(strXml, listRt[m], softvar, 0));

                                    if (m < listRt.Count - 1)
                                    {
                                        sb3.Append(" and ");
                                    }
                                }
                                #endregion "循环主键字段，整合where条件"

                                sb.Append(sb3);

                                if (softvar == "2")
                                {
                                    sb.Append("; ");
                                    sb.Append("if v_count>0 then ");
                                }
                                else
                                {
                                    sb.Append(")");

                                    sb.Append(" begin ");
                                }

                                sb.Append("update ");
                                sb.Append(reSet.tablename);
                                sb.Append(" set ");

                                #region "获取并整合SET部分内容"
                                List<clsInterfaceReTableSet> listCanUpdate = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.canUpdate == "1"; });

                                for (int ic = 0; ic < listCanUpdate.Count; ic++)
                                {
                                    sb.Append(listCanUpdate[ic].colname);
                                    sb.Append("=");

                                    sb.Append(DetailSB(strXml, listCanUpdate[ic], softvar, 0));

                                    if (ic < listCanUpdate.Count - 1)
                                    {
                                        sb.Append(",");
                                    }

                                }

                                #endregion "获取并整合SET部分内容"

                                //添加where部分内容
                                sb.Append(sb3);

                                if (softvar == "2")
                                {
                                    sb.Append("; else ");
                                }
                                else
                                {
                                    sb.Append(" end else begin ");
                                }

                                sb.Append(" insert into ");
                                sb.Append(reSet.tablename);
                                sb.Append("(");

                                for (int m = 0; m < reSet.IReTabSet.Count; m++)
                                {
                                    sb.Append(reSet.IReTabSet[m].colname);

                                    if (m < reSet.IReTabSet.Count - 1)
                                    {
                                        sb.Append(",");
                                    }

                                    sb2.Append(DetailSB(strXml, reSet.IReTabSet[m], softvar, 0));
                                    //sb2.Append("'");

                                    if (m < reSet.IReTabSet.Count - 1)
                                    {
                                        sb2.Append(",");
                                    }
                                }

                                sb.Append(")");
                                sb.Append(" values");
                                sb.Append("(");

                                sb.Append(sb2);

                                sb.Append(")");

                                if (softvar == "2")
                                {
                                    sb.Append("; end if; end;");
                                }
                                else
                                {
                                    sb.Append(" end");
                                }

                                #region "子语句整合"
                                //判断是否有子语句
                                List<clsInterfaceReSet> tmpListSon = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return p.fathersql == reSet.sqlname; });

                                if (tmpListSon.Count > 0)
                                {
                                    bool bInserted = false;

                                    for (int iC = 0; iC < tmpListSon.Count; iC++)
                                    {
                                        List<string> tmpSql = CreateReWriteSql(strXml, tmpListSon[iC]);

                                        if (tmpListSon[iC].isDesc == "0" && !bInserted)
                                        {
                                            listSql.Add(sb.ToString());

                                            bInserted = true;
                                        }

                                        listSql.AddRange(tmpSql);
                                    }

                                    if (!bInserted)
                                    {
                                        listSql.Add(sb.ToString());

                                        bInserted = true;
                                    }
                                }
                                else
                                {
                                    listSql.Add(sb.ToString());
                                }
                                #endregion "子语句整合"
                            }
                            #endregion "根据主键，修改设置的可修改字段"
                        }
                        #endregion "判断数据是否在表中已经存在（根据设置的主键）"
                        else
                        #region "不需要判断数据是否在表中已经存在"
                        {
                            sb = new StringBuilder();
                            sb2 = new StringBuilder();

                            sb.Append("insert into ");
                            sb.Append(reSet.tablename);
                            sb.Append("(");

                            for (int m = 0; m < reSet.IReTabSet.Count; m++)
                            {
                                sb.Append(reSet.IReTabSet[m].colname);

                                if (m < reSet.IReTabSet.Count - 1)
                                {
                                    sb.Append(",");
                                }

                                sb2.Append(DetailSB(strXml, reSet.IReTabSet[m], softvar, 0));

                                if (m < reSet.IReTabSet.Count - 1)
                                {
                                    sb2.Append(",");
                                }
                            }

                            sb.Append(")");
                            sb.Append(" values");
                            sb.Append("(");

                            sb.Append(sb2);

                            sb.Append(")");

                            //20180627 单语句去掉最后的分号
                            //if (softvar == "2")
                            //{
                            //    sb.Append(";");
                            //}

                            //listSql.Add(sb.ToString());
                        }
                        #endregion "不需要判断数据是否在表中已经存在"
                    }
                    else
                    {
                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey == "1"; });
                        List<clsInterfaceReTableSet> listRtN = reSet.IReTabSet.FindAll(delegate(clsInterfaceReTableSet p) { return p.iskey != "1"; });

                        sb = new StringBuilder();
                        sb2 = new StringBuilder();

                        sb.Append("update ");
                        sb.Append(reSet.tablename);
                        sb.Append(" set ");

                        for (int m = 0; m < listRtN.Count; m++)
                        {
                            sb.Append(listRtN[m].colname);
                            sb.Append("=");
                            //sb.Append("'");
                            sb.Append(DetailSB(strXml, listRtN[m], softvar, 0));

                            if (m < listRtN.Count - 1)
                            {
                                sb.Append(",");
                            }
                        }

                        if (listRt.Count > 0)
                        {
                            sb.Append(" where ");
                        }

                        for (int m = 0; m < listRt.Count; m++)
                        {
                            sb.Append(listRt[m].colname);
                            sb.Append("=");
                            //sb.Append("'");
                            sb.Append(DetailSB(strXml, listRt[m], softvar, 0));

                            if (m < listRt.Count - 1)
                            {
                                sb.Append(" and ");
                            }
                        }

                        //20180627 单语句去掉最后的分号
                        //if (softvar == "2")
                        //{
                        //    sb.Append(";");
                        //}

                        //listSql.Add(sb.ToString());
                        
                    }

                    #region "子语句整合"
                    //判断是否有子语句
                    List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate (clsInterfaceReSet p) { return p.fathersql == reSet.sqlname; });

                    if (tmpList.Count > 0)
                    {
                        bool bInserted = false;

                        for (int iC = 0; iC < tmpList.Count; iC++)
                        {
                            List<string> tmpSql = CreateReWriteSql(strXml, tmpList[iC]);

                            if (tmpList[iC].isDesc == "0" && !bInserted)
                            {
                                listSql.Add(sb.ToString());

                                bInserted = true;
                            }

                            listSql.AddRange(tmpSql);
                        }

                        if (!bInserted)
                        {
                            listSql.Add(sb.ToString());

                            bInserted = true;
                        }
                    }
                    else
                    {
                        listSql.Add(sb.ToString());
                    }
                    #endregion "子语句整合"
                }
                #endregion "数据不需要循环"

                return listSql;
            }
            catch(Exception ex)
            {
                return new List<string>();
            }
            finally
            {
                //listSql.Clear();

                sb.Clear();
                sb2.Clear();
            }
        }
        #endregion "整合回写SQL语句"

        #region "获取发送数据"
        /// <summary>
        /// 获取发送数据
        /// </summary>
        /// <param name="tableAndCol">表名.字段名</param>
        /// <param name="iRows">无用了</param>
        /// <returns>值</returns>
        private string GetSendValue(string tableAndCol, int iRows)
        {
            try
            {
                string[] nodes = tableAndCol.Split('.');

                string strTable = nodes[0];
                string strCol = nodes[1];

                int iTableT = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == strTable; });

                return listTable[iTableT].table.Rows[listTable[iTableT].iRows][strCol].ToString().Trim();

            }
            catch
            {
                return "";
            }
        }
        #endregion "获取发送数据"

        #region "获取接收数据"
        private string GetRevXmlValue(string strXml, string node)
        {
            try
            {
                string lastNode = node;

                string tmpNode = node;

                int iCount = Regex.Matches(tmpNode.Replace(".", "<A>"), @"<A>").Count;
                int iWhere = -1;

                if (iCount > 0)
                {
                    lastNode = tmpNode.Substring(tmpNode.LastIndexOf(".") + 1, tmpNode.Length - tmpNode.LastIndexOf(".") - 1);
                }

                if (lastNode.ToUpper() == "ALL" || lastNode.ToUpper() == "COUNT()")
                {
                    return XmlHelper.GetValue(strXml, node);
                }
                else
                {
                    while (iCount > 1)
                    {
                        tmpNode = tmpNode.Substring(0, tmpNode.LastIndexOf("."));

                        iWhere = listXmlNode.FindIndex(delegate (clsXmlStu p) { return p.titleName == tmpNode; });

                        if (iWhere >= 0)
                        {
                            return XmlHelper.GetValue(listXmlNode[iWhere].nodeList[listXmlNode[iWhere].iRows], node.Substring(tmpNode.Length + 1));
                        }
                        else
                        {
                            iCount = Regex.Matches(tmpNode.Replace(".", "<A>"), @"<A>").Count;
                        }
                    }

                    iWhere = listXmlNode.FindIndex(delegate (clsXmlStu p) { return p.titleName == node; });

                    if (iWhere >= 0)
                    {
                        return listXmlNode[iWhere].nodeList[listXmlNode[iWhere].iRows].InnerText.ToString();
                    }

                    return XmlHelper.GetValue(strXml, node);
                }
            }
            catch
            {
                return "";
            }
        }
        #endregion "获取接收数据"

        #region "获取接收数据"
        private XmlNodeList GetRevXmlNodes(string strXml, string node)
        {
            try
            {
                string tmpNode = node;

                int iCount = Regex.Matches(tmpNode.Replace(".", "<A>"), @"<A>").Count;

                while (iCount > 1)
                {
                    tmpNode = tmpNode.Substring(0, tmpNode.LastIndexOf("."));

                    int iWhere = listXmlNode.FindIndex(delegate(clsXmlStu p) { return p.titleName == tmpNode; });

                    if (iWhere >= 0)
                    {
                        return XmlHelper.GetNodeList(listXmlNode[iWhere].nodeList[listXmlNode[iWhere].iRows], node.Substring(tmpNode.Length + 1));
                    }
                    else
                    {
                        iCount = Regex.Matches(tmpNode.Replace(".", "<A>"), @"<A>").Count;
                    }
                }

                return XmlHelper.GetNodeList(strXml, node);
            }
            catch
            {
                return null;
            }
        }
        #endregion "获取接收数据"

        #region "整合XML"
        private List<string> CreateXmlPara(DataSet dsXml, DataSet dllSet, DataSet dllParaSet)
        {
            DAOInterface din = DAOFactory.CreateDAOInterface();

            List<string> listXml = new List<string>();

            try
            {
                //201710112 修改为自动循环形式的XML整合,可以允许存在层循环

                //首先查找出根标签，根标签必须存在并且仅能有一个
                string strSearch = "";

                //使用标签层级为1作为条件，过滤数据，查找到根标签节点标识
                strSearch = "xmltitlelv=1";

                DataRow[] drSelect = dsXml.Tables[0].Select(strSearch, "titlesort");

                string xmlRoot = drSelect[0]["xmltitle"].ToString().Trim();

                //判断根标签有没有对应的获取数据的SQL语句，如果有，则执行SQL获取数据集
                if (!string.IsNullOrEmpty(drSelect[0]["getvaluesql"].ToString().Trim()))
                {
                    string strSql = drSelect[0]["getvaluesql"].ToString().Trim();

                    //整合SQL语句，替换其中的变量符为数据
                    strSql = ConformSql(strSql);

                    clsTableStru cts = new clsTableStru();
                    cts.titleName = drSelect[0]["tablename"].ToString().Trim(); ;
                    cts.iRows = 0;
                    cts.table = din.GetDataSetBySql(strSql).Tables[0];

                    int iTableHave = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == drSelect[0]["tablename"].ToString().Trim(); });

                    if (iTableHave >= 0)
                    {
                        //如果存在，则删除掉该数据表实体
                        //listTable.RemoveAt(iTableHave);
                        listTable[iTableHave] = cts;
                    }
                    else
                    {
                        listTable.Add(cts);
                    }

                    //根标签对应的SQL语句，如果没有查询到数据，则直接返回空List，本次不需要调用接口
                    if (cts.table == null || cts.table.Rows.Count == 0)
                    {
                        return new List<string>();
                    }
                }

                //判断根标签是否需要循环，如果根标签需要循环则代表一次获取数据多次调用
                //如果根标签不需要循环，则一次仅读取第一条数据进行发送


                //20180523  修改，根节点循环无效化
                //if (drSelect[0]["titleisloop"].ToString().Trim() == "1")
                //{
                //    for (int i = 0; i < listTable[0].table.Rows.Count; i++)
                //    {
                //        listTable[0].iRows = i;

                //        StringBuilder sBuilder = new StringBuilder();

                //        sBuilder.Append("<" + xmlRoot + ">");

                //        sBuilder.Append(CreateXml(dsXml, xmlRoot));

                //        sBuilder.Append("</" + xmlRoot + ">");

                //        listXml.Add(sBuilder.ToString());
                //    }
                //}
                //else
                //{
                StringBuilder sBuilder = new StringBuilder();

                string strXmlAttri = CreateXmlTitle(dsXml, xmlRoot, dllSet, dllParaSet);

                sBuilder.Append("<" + strXmlAttri + ">");

                sBuilder.Append(CreateXml(dsXml, xmlRoot, dllSet, dllParaSet));

                string xmlRootEnd = xmlRoot;
                if (xmlRootEnd.Trim().Contains(" "))
                {
                    xmlRootEnd = xmlRootEnd.Substring(0, xmlRootEnd.IndexOf(" "));
                }

                sBuilder.Append("</" + xmlRootEnd + ">");

                listXml.Add(sBuilder.ToString());
                //}
                //listXmlPara.Add("<" + xmlRoot + ">" + sBuilder.ToString() + "</" + xmlRoot + ">");

                //return sBuilder.ToString();

                return listXml;
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }

        private StringBuilder CreateXml(DataSet dsXml, string parentTitle, DataSet dllSet, DataSet dllParaSet)
        {
            StringBuilder sbu = new StringBuilder();
            DAOInterface din = DAOFactory.CreateDAOInterface();

            try
            {
                //使用父标签以及XML设置结构，获取父标签下的所有子标签信息
                string strSearch = "partitle='" + parentTitle + "' and xmltitlelv>0";

                DataRow[] drSelect = dsXml.Tables[0].Select(strSearch, "titlesort");

                for (int i = 0; i < drSelect.Length; i++)
                {
                    if (!string.IsNullOrEmpty(drSelect[i]["getvaluesql"].ToString().Trim()))
                    {
                        //如果标签对应的获取数据的SQL语句不为空，则执行语句获取数据集并将数据表添加至集合中

                        //取得标签对应的SQL语句
                        string strSql = drSelect[i]["getvaluesql"].ToString().Trim();

                        //整合SQL语句，替换其中的变量符为数据
                        strSql = ConformSql(strSql);

                        //实例化数据表实体类
                        clsTableStru cts = new clsTableStru();

                        //给数据表对应的标签进行赋值,使用配置的表别名
                        cts.titleName = drSelect[i]["tablename"].ToString().Trim();

                        //初始化该数据表循环到的行的值为0
                        cts.iRows = 0;

                        //执行SQL语句获取数据，并且将数据赋值给数据表
                        cts.table = din.GetDataSetBySql(strSql).Tables[0];

                        //查找在数据表集合中，是否存在相同标签的数据表
                        int iTableHave = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); });

                        if (iTableHave >= 0)
                        {
                            //如果存在，则删除掉该数据表实体
                            //listTable.RemoveAt(iTableHave);
                            listTable[iTableHave] = cts;
                        }
                        else
                        {
                            //将数据表实体插入到数据集合中
                            listTable.Add(cts);
                        }
                    }


                    //判断该标签是否需要循环
                    if (drSelect[i]["titleisloop"].ToString().Trim() == "1")
                    {
                        //此标签需要循环
                        //如果标签需要循环，肯定是要按照数据来进行循环，则该标签需要指定对应循环数据的数据表
                        //获取该标签循环数据对应的数据表在数据集合中的位置
                        int iTable = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == drSelect[i]["looptable"].ToString().Trim(); });

                        //对数据进行循环
                        for (int m = 0; m < listTable[iTable].table.Rows.Count; m++)
                        {
                            string strTitle = drSelect[i]["xmltitle"].ToString().Trim();

                            //赋值循环行数脚标
                            listTable[iTable].iRows = m;

                            //首先添加该标签的开始标签
                            string strXmlAttri = CreateXmlTitle(dsXml, strTitle, dllSet, dllParaSet);

                            sbu.Append("<" + strXmlAttri + ">");

                            //判断该标签下面是值还是XML
                            if (drSelect[i]["paratype"].ToString().Trim().ToUpper() == "V")
                            {
                                string xmlStaticVal = drSelect[i]["xmlstaticval"].ToString().Trim();

                                if (string.IsNullOrEmpty(xmlStaticVal))
                                {
                                    //标签下面对应的是数据表值
                                    //获取标签需要取值的数据表名
                                    string strTableName = drSelect[i]["valtable"].ToString().Trim();

                                    //查找数据表的位置
                                    int iTableT = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == strTableName; });

                                    if (iTableT >= 0)
                                    {
                                        //判断设定的取值列名在数据表中是否存在
                                        if (listTable[iTableT].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                        {
                                            //如果设定的取值列名存在，则将值添加至XML中

                                            string tmpColValue = "";

                                            if (listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()] != null)
                                            {
                                                if (drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                                {
                                                    tmpColValue = Convert.ToBase64String(((byte[])listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()]));

                                                    string strDataLink = drSelect[i]["dateformat"].ToString().Trim();

                                                    if (strDataLink.ToLower() != "base64")
                                                    {
                                                        strDataLink = strDataLink.Substring(7);

                                                        tmpColValue = strDataLink + tmpColValue;
                                                    }
                                                }
                                                else
                                                {
                                                    tmpColValue = listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();
                                                }
                                                //tmpColValue = listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();
                                            }

                                            if (drSelect[i]["canempty"].ToString().Trim() == "1" && drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() != "1")
                                            {
                                                if (string.IsNullOrEmpty(tmpColValue))
                                                {
                                                    throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                                }
                                            }

                                            //判断该列是否为日期格式
                                            if (drSelect[i]["isdate"].ToString().Trim() == "1")
                                            {
                                                string strFormat = "yyyy-MM-dd HH:mm:ss";

                                                if (drSelect[i]["dateformat"].ToString().Trim() != "")
                                                {
                                                    strFormat = drSelect[i]["dateformat"].ToString().Trim();
                                                }

                                                if (tmpColValue != "")
                                                {
                                                    tmpColValue = Convert.ToDateTime(listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()]).ToString(strFormat);
                                                    //sbu.Append(listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim());
                                                }
                                            }
                                            else if(drSelect[i]["dateformat"].ToString().Trim() != "" && drSelect[i]["dateformat"].ToString().Trim() != "Num" && tmpColValue != "" && !drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                            {
                                                tmpColValue = string.Format(drSelect[i]["dateformat"].ToString().Trim(), Convert.ToDouble(tmpColValue));
                                            }

                                            
                                            if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                            {

                                                tmpColValue = ConvertValueByDll(tmpColValue, strTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                                
                                            }

                                            if (tmpColValue != "")
                                            {
                                                tmpColValue = tmpColValue.Replace("&", "&amp;");
                                                tmpColValue = tmpColValue.Replace("<", "&lt;");
                                                tmpColValue = tmpColValue.Replace(">", "&gt;");
                                                tmpColValue = tmpColValue.Replace("\"", "&quot;");
                                                tmpColValue = tmpColValue.Replace("'", "&#39;");
                                            }

                                            if (drSelect[i]["canempty"].ToString().Trim() == "1")
                                            {
                                                if (string.IsNullOrEmpty(tmpColValue))
                                                {
                                                    throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                                }
                                            }

                                            sbu.Append(tmpColValue);
                                        }
                                    }
                                }
                                else
                                {
                                    if (xmlStaticVal == "####" && !string.IsNullOrEmpty(drSelect[i]["valtable"].ToString().Trim()) && drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                    {
                                        string strTableName = drSelect[i]["valtable"].ToString().Trim();

                                        //查找数据表的位置
                                        int iTableT = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTableName; });

                                        if (iTableT >= 0)
                                        {
                                            //判断设定的取值列名在数据表中是否存在
                                            if (listTable[iTableT].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                            {
                                                List<string> sVal = new List<string>();

                                                for (int iRows = 0; iRows < listTable[iTableT].table.Rows.Count; iRows++)
                                                {
                                                    string strTmpValue = listTable[iTableT].table.Rows[iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();

                                                    sVal.Add(strTmpValue);
                                                }

                                                if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                                {
                                                    xmlStaticVal = ConvertValueByDll(sVal, strTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                                }
                                            }
                                            else
                                            {
                                                throw new UserException(drSelect[i]["xmltitle"].ToString().Trim() + ":取值字段设置有错误，未找到对应的字段");
                                            }
                                        }
                                        else
                                        {
                                            throw new UserException(drSelect[i]["xmltitle"].ToString().Trim() + ":取值表名设置错误-未找到数据表");
                                        }
                                    }
                                    else
                                    {
                                        if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                        {
                                            xmlStaticVal = ConvertValueByDll(xmlStaticVal, strTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                        }
                                    }

                                    if (xmlStaticVal != "")
                                    {
                                        xmlStaticVal = xmlStaticVal.Replace("&", "&amp;");
                                        xmlStaticVal = xmlStaticVal.Replace("<", "&lt;");
                                        xmlStaticVal = xmlStaticVal.Replace(">", "&gt;");
                                        xmlStaticVal = xmlStaticVal.Replace("\"", "&quot;");
                                        xmlStaticVal = xmlStaticVal.Replace("'", "&#39;");
                                    }

                                    if (drSelect[i]["canempty"].ToString().Trim() == "1")
                                    {
                                        if (string.IsNullOrEmpty(xmlStaticVal))
                                        {
                                            throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                        }
                                    }

                                    sbu.Append(xmlStaticVal);
                                }
                            }
                            else
                            {
                                //标签下面对应的是XML结构，则该标签为下层标签的父标签
                                //函数递归调用，获取下层标签的XML结构信息
                                StringBuilder sbChild = CreateXml(dsXml, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);

                                sbu.Append(sbChild);
                            }

                            //添加结尾标签
                            //string xmlRootEnd = drSelect[i]["xmltitle"].ToString().Trim();
                            //if (xmlRootEnd.Trim().Contains(" "))
                            //{
                            //    xmlRootEnd = xmlRootEnd.Substring(0, xmlRootEnd.IndexOf(" "));
                            //}

                            sbu.Append("</" + strTitle + ">");
                            
                            //sbu.Append("</" + drSelect[i]["xmltitle"].ToString().Trim() + ">");
                        }
                    }
                    else
                    {
                        //此标签不需要循环
                        //不需要循环时，下层标签如果是取值的标签则直接使用第一行数据
                        //获取数据表的标签，因为是不需要循环的标签则值应该是使用父标签对应的表
                        //使用父标签以及XML设置结构，获取父标签下的所有子标签信息

                        string strTitle = drSelect[i]["xmltitle"].ToString().Trim();
                        //首先添加该标签的开始标签
                        string strXmlAttri = CreateXmlTitle(dsXml, strTitle, dllSet, dllParaSet);

                        sbu.Append("<" + strXmlAttri + ">");

                        //判断该标签下面是值还是XML
                        if (drSelect[i]["paratype"].ToString().Trim().ToUpper() == "V")
                        {
                            string xmlStaticVal = drSelect[i]["xmlstaticval"].ToString().Trim();

                            if (string.IsNullOrEmpty(xmlStaticVal))
                            {
                                string strTableName = drSelect[i]["valtable"].ToString().Trim();

                                int iTable = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == strTableName; });
                                //标签下面对应的是数据表值
                                //判断设定的取值列名在数据表中是否存在

                                if (iTable >= 0)
                                {
                                    string tmpColValue = "";

                                    if (listTable[iTable].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                    {
                                        if (listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()] != null)
                                        {
                                            if (drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                            {
                                                tmpColValue = Convert.ToBase64String(((byte[])listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()]));

                                                string strDataLink = drSelect[i]["dateformat"].ToString().Trim();

                                                if (strDataLink.ToLower() != "base64")
                                                {
                                                    strDataLink = strDataLink.Substring(7);

                                                    tmpColValue = strDataLink + tmpColValue;
                                                }
                                            }
                                            else
                                            {
                                                tmpColValue = listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim();
                                            }
                                            
                                        }

                                        if (drSelect[i]["canempty"].ToString().Trim() == "1" && drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() != "1")
                                        {
                                            if (string.IsNullOrEmpty(tmpColValue))
                                            {
                                                throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                            }
                                        }

                                        if (drSelect[i]["isdate"].ToString().Trim() == "1")
                                        {
                                            string strFormat = "yyyy-MM-dd HH:mm:ss";

                                            if (drSelect[i]["dateformat"].ToString().Trim() != "")
                                            {
                                                strFormat = drSelect[i]["dateformat"].ToString().Trim();
                                            }

                                            if (tmpColValue != "")
                                            {
                                                tmpColValue = Convert.ToDateTime(listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()]).ToString(strFormat);
                                                //sbu.Append(listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim());
                                            }
                                        }
                                        else if (drSelect[i]["dateformat"].ToString().Trim() != "" && drSelect[i]["dateformat"].ToString().Trim() != "Num" && tmpColValue != "" && !drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                        {
                                            tmpColValue = string.Format(drSelect[i]["dateformat"].ToString().Trim(), Convert.ToDouble(tmpColValue));
                                        }

                                        if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                        {
                                            tmpColValue = ConvertValueByDll(tmpColValue, strTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                        }


                                        if (tmpColValue != "")
                                        {
                                            tmpColValue = tmpColValue.Replace("&", "&amp;");
                                            tmpColValue = tmpColValue.Replace("<", "&lt;");
                                            tmpColValue = tmpColValue.Replace(">", "&gt;");
                                            tmpColValue = tmpColValue.Replace("\"", "&quot;");
                                            tmpColValue = tmpColValue.Replace("'", "&#39;");
                                        }

                                        if (drSelect[i]["canempty"].ToString().Trim() == "1")
                                        {
                                            if (string.IsNullOrEmpty(tmpColValue))
                                            {
                                                throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                            }
                                        }

                                        //如果设定的取值列名存在，则将值添加至XML中
                                        sbu.Append(tmpColValue);
                                    }
                                }
                            }
                            else
                            {
                                if (xmlStaticVal == "####" && !string.IsNullOrEmpty(drSelect[i]["valtable"].ToString().Trim()) && drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                {
                                    string strTableName = drSelect[i]["valtable"].ToString().Trim();

                                    //查找数据表的位置
                                    int iTableT = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTableName; });

                                    if (iTableT >= 0)
                                    {
                                        //判断设定的取值列名在数据表中是否存在
                                        if (listTable[iTableT].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                        {
                                            List<string> sVal = new List<string>();

                                            for (int iRows = 0; iRows < listTable[iTableT].table.Rows.Count; iRows++)
                                            {
                                                string strTmpValue = listTable[iTableT].table.Rows[iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim();

                                                sVal.Add(strTmpValue);
                                            }

                                            if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                            {
                                                xmlStaticVal = ConvertValueByDll(sVal, parentTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                            }
                                        }
                                        else
                                        {
                                            throw new UserException(drSelect[i]["xmltitle"].ToString().Trim() + ":取值字段设置有错误，未找到对应的字段");
                                        }
                                    }
                                    else
                                    {
                                        throw new UserException(drSelect[i]["xmltitle"].ToString().Trim() + ":取值表名设置错误-未找到数据表");
                                    }
                                }
                                else
                                {
                                    if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                    {
                                        xmlStaticVal = ConvertValueByDll(xmlStaticVal, strTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                    }
                                }

                                if (xmlStaticVal != "")
                                {
                                    xmlStaticVal = xmlStaticVal.Replace("&", "&amp;");
                                    xmlStaticVal = xmlStaticVal.Replace("<", "&lt;");
                                    xmlStaticVal = xmlStaticVal.Replace(">", "&gt;");
                                    xmlStaticVal = xmlStaticVal.Replace("\"", "&quot;");
                                    xmlStaticVal = xmlStaticVal.Replace("'", "&#39;");
                                }

                                if (drSelect[i]["canempty"].ToString().Trim() == "1")
                                {
                                    if (string.IsNullOrEmpty(xmlStaticVal))
                                    {
                                        throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                    }
                                }

                                sbu.Append(xmlStaticVal);
                            }
                        }
                        else
                        {
                            //标签下面对应的是XML结构，则该标签为下层标签的父标签
                            //函数递归调用，获取下层标签的XML结构信息
                            StringBuilder sbChild = CreateXml(dsXml, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);

                            sbu.Append(sbChild);
                        }

                        //添加结尾标签
                        //string xmlRootEnd = drSelect[i]["xmltitle"].ToString().Trim();
                        //if (xmlRootEnd.Trim().Contains(" "))
                        //{
                        //    xmlRootEnd = xmlRootEnd.Substring(0, xmlRootEnd.IndexOf(" "));
                        //}

                        sbu.Append("</" + strTitle + ">");
                        //sbu.Append("</" + drSelect[i]["xmltitle"].ToString().Trim() + ">");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }

            return sbu;
        }

        private string CreateXmlTitle(DataSet dsXml, string strXmlTitle, DataSet dllSet, DataSet dllParaSet)
        {
            try
            {
                string strSearch = "partitle='" + strXmlTitle + "' and xmltitlelv<0";

                DataRow[] drSelect = dsXml.Tables[0].Select(strSearch, "titlesort");

                if (drSelect.Length > 0)
                {
                    string strTitleAttri = strXmlTitle;

                    for (int i = 0; i < drSelect.Length; i++)
                    {
                        string strTitle = drSelect[i]["xmltitle"].ToString();
                        strTitleAttri = strTitleAttri + " ";

                        strTitleAttri = strTitleAttri + " " + strTitle + "=\"";

                        //限定标签属性只能是值，不能是XML结构
                        string xmlStaticVal = drSelect[i]["xmlstaticval"].ToString().Trim();

                        if (string.IsNullOrEmpty(xmlStaticVal))
                        {
                            string strTableName = drSelect[i]["valtable"].ToString().Trim();

                            int iTable = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTableName; });
                            //标签下面对应的是数据表值
                            //判断设定的取值列名在数据表中是否存在

                            if (iTable >= 0)
                            {
                                string tmpColValue = "";

                                if (listTable[iTable].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                {
                                    if (listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()] != null)
                                    {
                                        tmpColValue = listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim();
                                    }

                                    if (drSelect[i]["canempty"].ToString().Trim() == "1" && drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() != "1")
                                    {
                                        if (string.IsNullOrEmpty(tmpColValue))
                                        {
                                            throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                        }
                                    }

                                    if (drSelect[i]["isdate"].ToString().Trim() == "1")
                                    {
                                        string strFormat = "yyyy-MM-dd HH:mm:ss";

                                        if (drSelect[i]["dateformat"].ToString().Trim() != "")
                                        {
                                            strFormat = drSelect[i]["dateformat"].ToString().Trim();
                                        }

                                        if (tmpColValue != "")
                                        {
                                            tmpColValue = Convert.ToDateTime(listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()]).ToString(strFormat);
                                            //sbu.Append(listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim());
                                        }
                                    }
                                    else if (drSelect[i]["dateformat"].ToString().Trim() != "" && drSelect[i]["dateformat"].ToString().Trim() != "Num" && tmpColValue != "")
                                    {
                                        tmpColValue = string.Format(drSelect[i]["dateformat"].ToString().Trim(), Convert.ToDouble(tmpColValue));
                                    }

                                    if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                    {
                                        tmpColValue = ConvertValueByDll(tmpColValue, strTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                    }


                                    if (tmpColValue != "")
                                    {
                                        tmpColValue = tmpColValue.Replace("&", "&amp;");
                                        tmpColValue = tmpColValue.Replace("<", "&lt;");
                                        tmpColValue = tmpColValue.Replace(">", "&gt;");
                                        tmpColValue = tmpColValue.Replace("\"", "&quot;");
                                        tmpColValue = tmpColValue.Replace("'", "&#39;");
                                    }

                                    if (drSelect[i]["canempty"].ToString().Trim() == "1")
                                    {
                                        if (string.IsNullOrEmpty(tmpColValue))
                                        {
                                            throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                        }
                                    }

                                    //如果设定的取值列名存在，则将值添加至XML中
                                    //sbu.Append(tmpColValue);
                                    strTitleAttri = strTitleAttri + tmpColValue;
                                }
                            }
                        }
                        else
                        {
                            if (xmlStaticVal == "####" && !string.IsNullOrEmpty(drSelect[i]["valtable"].ToString().Trim()) && drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                            {
                                string strTableName = drSelect[i]["valtable"].ToString().Trim();

                                //查找数据表的位置
                                int iTableT = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTableName; });

                                if (iTableT >= 0)
                                {
                                    //判断设定的取值列名在数据表中是否存在
                                    if (listTable[iTableT].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                    {
                                        List<string> sVal = new List<string>();

                                        for (int iRows = 0; iRows < listTable[iTableT].table.Rows.Count; iRows++)
                                        {
                                            string strTmpValue = listTable[iTableT].table.Rows[iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim();

                                            sVal.Add(strTmpValue);
                                        }

                                        if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                        {
                                            xmlStaticVal = ConvertValueByDll(sVal, strTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                        }
                                    }
                                    else
                                    {
                                        throw new UserException(drSelect[i]["xmltitle"].ToString().Trim() + ":取值字段设置有错误，未找到对应的字段");
                                    }
                                }
                                else
                                {
                                    throw new UserException(drSelect[i]["xmltitle"].ToString().Trim() + ":取值表名设置错误-未找到数据表");
                                }
                            }
                            else
                            {
                                if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                {
                                    xmlStaticVal = ConvertValueByDll(xmlStaticVal, strTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                }
                            }

                            if (xmlStaticVal != "")
                            {
                                xmlStaticVal = xmlStaticVal.Replace("&", "&amp;");
                                xmlStaticVal = xmlStaticVal.Replace("<", "&lt;");
                                xmlStaticVal = xmlStaticVal.Replace(">", "&gt;");
                                xmlStaticVal = xmlStaticVal.Replace("\"", "&quot;");
                                xmlStaticVal = xmlStaticVal.Replace("'", "&#39;");
                            }

                            if (drSelect[i]["canempty"].ToString().Trim() == "1")
                            {
                                if (string.IsNullOrEmpty(xmlStaticVal))
                                {
                                    throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                }
                            }

                            //sbu.Append(xmlStaticVal);
                            strTitleAttri = strTitleAttri + xmlStaticVal;
                        }

                        strTitleAttri = strTitleAttri + "\"";
                    }

                    return strTitleAttri;
                }
                else
                {
                    return strXmlTitle;
                }
            }
            catch (Exception ex)
            {
                throw new UserException("整合XML标签属性时发生异常：" + ex.Message);
            }
        }
        public string ConvertValueByDll(string value, string parTitle, string title, DataSet dllSet, DataSet dllParaSet)
        {
            try
            {
                string strSearch = "xmltitle='" + title + "' and xmlpartitle='" + parTitle + "'";

                DataRow[] drDllSelect = dllSet.Tables[0].Select(strSearch);

                DataRow[] drDllParaSelect = dllParaSet.Tables[0].Select(strSearch, "dllparasort");

                int iParaNum = Convert.ToInt32(drDllSelect[0]["dllparanum"]);

                object[] ob = new object[iParaNum];

                for (int iPa = 0; iPa < iParaNum; iPa++)
                {
                    string tmp_value = value;

                    if (!string.IsNullOrEmpty(drDllParaSelect[iPa]["dllparavalue"].ToString()))
                    {
                        string paraValue = drDllParaSelect[iPa]["dllparavalue"].ToString();
                        if (paraValue.Contains("#H#"))
                        {
                            string strTi = paraValue.Replace("#H#", "");

                            int iDot = strTi.IndexOf(".");
                            //末尾标签的位置
                            int iEnd = strTi.Length;

                            //提取从哪个数据表中取数据进行替换
                            string strTab = strTi.Substring(0, iDot);

                            int iTab = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == strTab; });
                            //提取对应数据表中的列名
                            string strDataTitle = strTi.Substring(iDot + 1, iEnd - iDot - 1);

                            //如果数据表集合的长度符合指定的长度
                            if (iTab >= 0)
                            {
                                //根据数据表、对应的行、列名对数据进行替换，整合成真正需要执行的SQL语句
                                tmp_value = listTable[iTab].table.Rows[listTable[iTab].iRows][strDataTitle].ToString().Trim();
                            }
                        }
                        else
                        {
                            tmp_value = drDllParaSelect[iPa]["dllparavalue"].ToString();
                        }
                    }

                    ob[iPa] = tmp_value;
                }

                string strResult = (string)Invoke_Dll(drDllSelect[0]["dllname"].ToString().Trim(), drDllSelect[0]["dllnamespe"].ToString().Trim(), drDllSelect[0]["dllclassname"].ToString().Trim(), drDllSelect[0]["dllprocname"].ToString().Trim(), ob);

                return strResult;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string ConvertValueByDll(List<string> value, string parTitle, string title, DataSet dllSet, DataSet dllParaSet)
        {
            try
            {
                string strSearch = "xmltitle='" + title + "' and xmlpartitle='" + parTitle + "'";

                DataRow[] drDllSelect = dllSet.Tables[0].Select(strSearch);

                DataRow[] drDllParaSelect = dllParaSet.Tables[0].Select(strSearch, "dllparasort");

                int iParaNum = Convert.ToInt32(drDllSelect[0]["dllparanum"]);

                object[] ob = new object[iParaNum];

                for (int iPa = 0; iPa < iParaNum; iPa++)
                {
                    object tmp_value = value;

                    if (!string.IsNullOrEmpty(drDllParaSelect[iPa]["dllparavalue"].ToString()))
                    {
                        string paraValue = drDllParaSelect[iPa]["dllparavalue"].ToString();
                        if (paraValue.Contains("#H#"))
                        {
                            string strTi = paraValue.Replace("#H#", "");

                            int iDot = strTi.IndexOf(".");
                            //末尾标签的位置
                            int iEnd = strTi.Length;

                            //提取从哪个数据表中取数据进行替换
                            string strTab = strTi.Substring(0, iDot);

                            int iTab = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTab; });
                            //提取对应数据表中的列名
                            string strDataTitle = strTi.Substring(iDot + 1, iEnd - iDot - 1);

                            //如果数据表集合的长度符合指定的长度
                            if (iTab >= 0)
                            {
                                //根据数据表、对应的行、列名对数据进行替换，整合成真正需要执行的SQL语句
                                tmp_value = listTable[iTab].table.Rows[listTable[iTab].iRows][strDataTitle].ToString().Trim();
                            }
                        }
                        else
                        {
                            tmp_value = drDllParaSelect[iPa]["dllparavalue"].ToString();
                        }
                    }

                    ob[iPa] = tmp_value;
                }

                string strResult = (string)Invoke_Dll(drDllSelect[0]["dllname"].ToString().Trim(), drDllSelect[0]["dllnamespe"].ToString().Trim(), drDllSelect[0]["dllclassname"].ToString().Trim(), drDllSelect[0]["dllprocname"].ToString().Trim(), ob);

                return strResult;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion "整合XML"

        #region "整合SQL语句(发送数据时，将语句中的变量进行赋值)"
        /// <summary>
        /// 整合需要执行的SQL语句
        /// </summary>
        /// <param name="strSql">整合前的SQL语句</param>
        /// <param name="iRows"></param>
        /// <returns></returns>
        private string ConformSql(string strSql)
        {
            try
            {
                string strFlag = "#H#";
                List<string> listReplace = new List<string>();

                //查找替换标签在SQL语句中的位置
                int[] listFlag = Regex.Matches(strSql, strFlag).OfType<Match>().Select(t => t.Index).ToArray();

                if (listFlag.Length > 0)
                {
                    if ((listFlag.Length % 2) == 0)
                    {
                        //将所有需要替换的数据提取出来
                        for (int i = 0; i < listFlag.Length; i = i + 2)
                        {
                            string strTmp = strSql.Substring(listFlag[i], listFlag[i + 1] - listFlag[i] + 3);

                            listReplace.Add(strTmp);
                        }

                        //对需要替换的内容进行值的替换
                        for (int m = 0; m < listReplace.Count; m++)
                        {
                            //标签中.的位置
                            int iDot = listReplace[m].IndexOf(".");
                            //末尾标签的位置
                            int iEnd = listReplace[m].Length - 4;

                            //提取从哪个数据表中取数据进行替换
                            string strTab = listReplace[m].Substring(3, iDot - 3);

                            int iTab = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == strTab; });

                            if (iTab < 0)
                            {
                                throw new UserException(strTab + "表不存在，请检查SQL语句设置,表名区分大小写");
                            }
                            //提取对应数据表中的列名
                            string strDataTitle = listReplace[m].Substring(iDot + 1, iEnd - iDot);

                            //如果数据表集合的长度符合指定的长度
                            if (listTable.Count >= iTab + 1)
                            {
                                //根据数据表、对应的行、列名对数据进行替换，整合成真正需要执行的SQL语句
                                strSql = strSql.Replace(listReplace[m], listTable[iTab].table.Rows[listTable[iTab].iRows][strDataTitle].ToString().Trim());
                            }
                            else
                            {
                                throw new UserException("XML格式配置错误，整合SQL语句时找不到对应的替换数据表");
                            }
                        }

                    }
                    else
                    {
                        throw new UserException("读取数据语句配置错误，替换标签未成对出现");
                    }
                }
                //else
                //{
                //    //不存在需要替换值的内容，直接原内容返回
                //}

                return strSql;
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }
        #endregion "整合SQL语句(发送数据时，将语句中的变量进行赋值)"

        #region "时钟"
        
        DateTime dtime = DateTime.Now;

        private void timLoop_Tick(object sender, EventArgs e)
        {
            try
            {

                //已经启动运行并且时间在开始和结束之间
                if (bRun && !bRuNow && DateTime.Parse(dtime.ToString("HH:mm")) < DateTime.Parse(dtEnd.Value.ToString("HH:mm")) && DateTime.Parse(dtime.ToString("HH:mm")) > DateTime.Parse(dtBegin.Value.ToString("HH:mm")))
                {
                    //计数器+1
                    iCount = iCount + 1;

                    lblCount.Invoke(new EventHandler(delegate
                    {
                        lblCount.Text = iCount.ToString();
                    }));
                }
            }
            catch (Exception ex)
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(ex.Message);

                    rthText.ScrollToCaret();
                }));
            }
        }
        #endregion "时钟"

        #region "开始--停止  按钮"
        private void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                string strButtonText = btnRun.Text.ToString().Trim();

                if (strButtonText == "开  始")
                {
                    btnRun.Invoke(new EventHandler(delegate
                    {
                        btnRun.Text = "停  止";
                        btnSave.Enabled = false;

                        bFirst = true;

                        dtBegin.Enabled = false;
                        dtEnd.Enabled = false;
                        numAgain.Enabled = false;

                        btnExit.Enabled = false;
                        tsmExit.Enabled = false;

                        btnTest.Enabled = false;
                        btnRunSelectSub.Enabled = false;

                        if (chkAutoRun.Checked && chkAotoReRun.Checked && !string.IsNullOrEmpty(txtReRunTime.Text.ToString()))
                        {
                            timReRun.Enabled = true;
                        }
                    }));

                    bRun = true;

                    threadRun = new Thread(new ThreadStart(ExecInterface));

                    threadRun.Start();

                    timLoop.Enabled = true;
                }
                else if (strButtonText == "点击停止")
                {
                    bRun = false;

                    threadRun.Abort();

                    btnRun.Invoke(new EventHandler(delegate
                    {
                        btnRun.Text = "开  始";

                        btnSave.Enabled = true;

                        dtBegin.Enabled = true;
                        dtEnd.Enabled = true;
                        numAgain.Enabled = true;

                        btnExit.Enabled = true;
                        tsmExit.Enabled = true;

                        btnTest.Enabled = true;
                        btnRunSelectSub.Enabled = true;
                    }));
                }
                else
                {
                    timLoop.Enabled = false;

                    iCount = 0;

                    btnRun.Invoke(new EventHandler(delegate
                    {
                        btnRun.Enabled = false;
                        btnRun.Text = "正在停止";
                    }));

                    if (!bRuNow)
                    {
                        btnRun.Invoke(new EventHandler(delegate
                        {
                            if (btnRun.Text.ToString().Trim() == "正在停止")
                            {
                                btnRun.Enabled = true;

                                btnRun.Text = "点击停止";
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(ex.Message + "\r\n");

                    rthText.ScrollToCaret();
                }));
            }
        }
        #endregion "开始--停止  按钮"

        #region "动态执行DLL方法"
        //添加LoadDll方法：
        Assembly MyAssembly;

        private byte[] LoadDll(string lpFileName)
        {

            Assembly NowAssembly = Assembly.GetEntryAssembly();

            Stream fs = null;

            try
            {// 尝试读取资源中的 DLL

                fs = NowAssembly.GetManifestResourceStream(NowAssembly.GetName().Name + "." + lpFileName);

            }
            finally
            {// 如果资源没有所需的 DLL ，就查看硬盘上有没有，有的话就读取

                if (fs == null && !File.Exists(lpFileName))
                {
                    throw (new Exception(" 找不到文件 :" + lpFileName));
                }
                else if (fs == null && File.Exists(lpFileName))
                {

                    FileStream Fs = new FileStream(lpFileName, FileMode.Open);

                    fs = (Stream)Fs;

                }
            }

            byte[] buffer = new byte[(int)fs.Length];

            fs.Read(buffer, 0, buffer.Length);

            fs.Close();
            fs.Dispose();
            return buffer; // 以 byte[] 返回读到的 DLL
        }

        //添加UnLoadDll方法来卸载DLL：
        public void UnLoadDll()
        {
            // 使 MyAssembly 指空
            
            MyAssembly = null;
        }

        /// <summary>
        /// 动态执行动态链接库（DLL类库）的方法，以object返回结果
        /// </summary>
        /// <param name="lpFileName">类库包含名称的全地址</param>
        /// <param name="Namespace">方法对应的命名空间</param>
        /// <param name="ClassName">方法对应的类名</param>
        /// <param name="lpProcName">方法名称</param>
        /// <param name="ObjArray_Parameter">参数</param>
        /// <returns></returns>
        public object Invoke_Dll(string lpFileName, string Namespace, string ClassName, string lpProcName, object[] ObjArray_Parameter)
        {
            try
            {
                string basePath = System.AppDomain.CurrentDomain.BaseDirectory;

                lpFileName = basePath + lpFileName;

                // 判断 MyAssembly 是否为空或 MyAssembly 的命名空间不等于要调用方法的命名空间，如果条件为真，就用 Assembly.Load 加载所需 DLL 作为程序集 
                if (MyAssembly == null || MyAssembly.GetName().Name != Namespace)
                {
                    MyAssembly = Assembly.Load(LoadDll(lpFileName));
                }

                Type[] type = MyAssembly.GetTypes();

                foreach (Type t in type)
                {
                    if (t.Namespace == Namespace && t.Name == ClassName)
                    {
                        MethodInfo m = t.GetMethod(lpProcName);
                        if (m != null)
                        {// 调用并返回 
                            object o = Activator.CreateInstance(t);
                            return m.Invoke(o, ObjArray_Parameter);
                        }
                        else
                        {
                            throw new Exception("装载出错!");
                        }
                    }
                }

                throw new Exception("未找到配置的类库方法，请检查配置详情（命名空间、类名、方法名是否正确）!");
            }
            catch(UserException ex)
            {
                string strMessage = "";

                if (ex.Message != null)
                {
                    strMessage = ex.Message;
                }

                if (ex.InnerException != null)
                {
                    strMessage = strMessage + "|>--<|" + ex.InnerException;
                }

                throw new Exception(strMessage);
            }
            catch (Exception ex)
            {
                string strMessage = "";

                if (ex.Message != null)
                {
                    strMessage = ex.Message;
                }

                if (ex.InnerException != null)
                {
                    strMessage = strMessage + "|>--<|" + ex.InnerException;
                }

                throw new Exception(strMessage);
            }
            finally
            {
                if (MyAssembly != null)
                {
                    UnLoadDll();
                }
            }
        }
        #endregion "动态执行DLL方法"

        #region "判断返回的数据是否可以继续向下解析"
        /// <summary>
        /// 判断返回的数据是否可以继续向下解析
        /// </summary>
        /// <param name="iSub">接口函数脚标</param>
        /// <param name="strResult">接收到的数据</param>
        /// <returns>true:可以继续向下解析；false：不能继续向下解析，显示设定的提示信息标签的内容</returns>
        private bool JudRecSign(int iSub, string strResult, object[] args,string judType)
        {
            string strProm = "";

            try
            {
                //string strProm = "";

                strProm = "if(";

                List<clsInterfaceJudRec> IJud = new List<clsInterfaceJudRec>();//InterFaceSet[iSub].IJudRec;

                if (judType == "sub")
                {
                    IJud = InterFaceSet[iSub].IJudRec;
                }
                else
                {
                    IJud = InterFaceSet[iSub].ILoopJudRec;
                }

                for (int i = 0; i < IJud.Count; i++)
                {
                    strProm = strProm + IJud[i].leftbra;

                    //if (IJud[i].datatype == "字符")
                    //{
                    //    strProm = strProm + "\"";
                    //}

                    //if (IJud[i].titname.Contains("#args#"))
                    //{
                    //    string tNode = IJud[i].titname;
                    //    int iArgs = Convert.ToInt32(tNode.Substring(tNode.LastIndexOf(":") + 1));

                    //    strProm = strProm + args[iArgs].ToString();
                    //}
                    //else
                    //{
                    //    strProm = strProm + XmlHelper.GetValue(strResult, IJud[i].titname);
                    //}

                    //if (IJud[i].datatype == "字符")
                    //{
                    //    strProm = strProm + "\"";
                    //}

                    strProm = strProm + ConformProm(IJud[i].titname, strResult, args, IJud[i].datatype);

                    if (IJud[i].condition == "包含")
                    {
                        strProm = strProm + ".Contains(";
                    }
                    else
                    {
                        strProm = strProm + IJud[i].condition;
                    }

                    //if (IJud[i].datatype == "字符")
                    //{
                    //    strProm = strProm + "\"";
                    //}

                    //strProm = strProm + IJud[i].titvalues;

                    //if (IJud[i].datatype == "字符")
                    //{
                    //    strProm = strProm + "\"";
                    //}

                    strProm = strProm + ConformProm(IJud[i].titvalues, strResult, args, IJud[i].datatype);

                    if (IJud[i].condition == "包含")
                    {
                        strProm = strProm + ")";
                    }


                    strProm = strProm + IJud[i].rightbra;

                    if (i < IJud.Count - 1)
                    {
                        if (IJud[i].linktype == "并且")
                        {
                            strProm = strProm + " && ";
                        }
                        else
                        {
                            strProm = strProm + " || ";
                        }
                    }

                }

                strProm = strProm.Replace("\r", "").Replace("\n", "");

                strProm = strProm + "){return \"1\";}else{return \"0\";}";

                if (bTest)
                {
                    TraceHelper TraceHelper = new TraceHelper();
                    TraceHelper.WriteLine("成功逻辑判断代码：" + strProm);
                }

                Evaluator ee = new Evaluator(typeof(string), strProm, "RecJud");

                string strR = ee.EvaluateString("RecJud");
                
                if (strR == "1")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                TraceHelper TraceHelper = new TraceHelper();
                TraceHelper.WriteLine("成功逻辑判断出现异常：" + strProm + "\r\n" + ex.Message);
                return false;
            }
        }
        #endregion "判断返回的数据是否可以继续向下解析"

        #region "整合判断逻辑"
        /// <summary>
        /// 整合需要执行的SQL语句
        /// </summary>
        /// <param name="strSql">整合前的SQL语句</param>
        /// <param name="iRows"></param>
        /// <returns></returns>
        private string ConformProm(string prom,string strResult, object[] args,string dataType)
        {
            try
            {
                string strFlag = "#H#";
                List<string> listReplace = new List<string>();

                //查找替换标签在SQL语句中的位置
                int[] listFlag = Regex.Matches(prom, strFlag).OfType<Match>().Select(t => t.Index).ToArray();

                if (listFlag.Length > 0)
                {
                    if ((listFlag.Length % 2) == 0)
                    {
                        //将所有需要替换的数据提取出来
                        for (int i = 0; i < listFlag.Length; i = i + 2)
                        {
                            string strTmp = prom.Substring(listFlag[i], listFlag[i + 1] - listFlag[i] + 3);

                            listReplace.Add(strTmp);
                        }

                        //对需要替换的内容进行值的替换
                        for (int m = 0; m < listReplace.Count; m++)
                        {
                            string strNode = listReplace[m].Replace("#H#", "");

                            if (strNode.Contains("#args#"))
                            {
                                string tNode = strNode;
                                int iArgs = Convert.ToInt32(tNode.Substring(tNode.LastIndexOf(":") + 1));

                                string strValue = args[iArgs].ToString();

                                if (dataType == "字符")
                                {
                                    strValue = "\"" + strValue + "\"";
                                }

                                prom = prom.Replace(listReplace[m], strValue);
                            }
                            else
                            {
                                string strValue = XmlHelper.GetValue(strResult, strNode);

                                //strValue = strValue.Replace("\"", "\\\"");

                                if (dataType == "字符")
                                {
                                    strValue = "\"" + strValue + "\"";
                                }

                                prom = prom.Replace(listReplace[m], strValue);
                            }                            
                        }

                    }
                    else
                    {
                        throw new UserException("读取数据语句配置错误，替换标签未成对出现");
                    }
                }

                return prom;
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }
        #endregion "整合判断逻辑"

        #region "执行WebService接口"
        /// <summary>
        /// 执行WebService接口函数
        /// </summary>
        /// <param name="strSubName">函数名称</param>
        /// <param name="subChiName">中文名称</param>
        /// <param name="iSub">接口位置</param>
        /// <param name="bSelf">是否为自调用</param>
        /// <param name="bPre">是否为前置调用</param>

        private void RunWebServiceSub(string strSubName, string subChiName, int iSub, bool bSelf, bool bPre)
        {
            DAOInterface di = DAOFactory.CreateDAOInterface();

            
            try
            {
                string strTmpTmp = "";
                if (chbusexml.Checked)
                {
                    strTmpTmp = rthText.Text.ToString().Trim();
                }

                bTriggerResult = true;

                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":准备开始执行接口" + "\r\n");

                    rthText.ScrollToCaret();

                    CallDoEvents();
                }));

                for (int ic = 0; ic < listTable.Count; ic++)
                {
                    listTable[ic].table.Dispose();
                }

                listTable.Clear();
                listXmlNode.Clear();

                string strAdd = "";
                string strClassName = "";
                int iInParaNum = 0;
                //string strReWriteSql = "";
                string baseSql = "";
                string bTableName = "";

                //int iSub = InterFaceSet.FindIndex(delegate(clsInterfaceSet p) { return p.SubName == strSubName && p.ChiName == subChiName; });

                if (iSub >= 0)
                {

                    //如果设置了前置执行调用的接口，先调用该接口
                    if (!string.IsNullOrEmpty(InterFaceSet[iSub].PreInter))
                    {
                        string[] fi = InterFaceSet[iSub].PreInter.Split(',');

                        //0:chiname,1:subname
                        int iiSub = InterFaceSet.FindIndex(delegate (clsInterfaceSet p) { return p.SubName == fi[1] && p.ChiName == fi[0]; });

                        if (iiSub >= 0)
                        {
                            bPreRunResult = false;

                            if (string.IsNullOrEmpty(InterFaceSet[iiSub].SubType) || InterFaceSet[iiSub].SubType == "webservice")
                            {
                                //WebService接口执行
                                RunWebServiceSub(fi[1], fi[0], iiSub, false, true);
                            }
                            else
                            {
                                RunHttpSub(fi[1], fi[0], iiSub, false, true);
                            }

                            if (!bPreRunResult)
                            {
                                return;
                            }
                        }
                    }

                    strAdd = InterFaceSet[iSub].strAdd;
                    strClassName = InterFaceSet[iSub].ClassName;
                    iInParaNum = InterFaceSet[iSub].InParaNum;
                    //strReWriteSql = InterFaceSet[iSub].ReWriteSql;
                    baseSql = InterFaceSet[iSub].baseSql;
                    bTableName = InterFaceSet[iSub].bTableName;

                    //判断接口地址是否采用SQL语句在数据库中进行查询的方式
                    if (strAdd.Length > 4 && strAdd.Substring(0, 4).ToUpper() == "SQL:")
                    {
                        string strSql = strAdd.Substring(4);

                        DataSet ds = di.GetDataSetBySql(strSql);

                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            strAdd = ds.Tables[0].Rows[0][0].ToString().Trim();
                        }
                        else
                        {
                            rthText.Invoke(new EventHandler(delegate
                            {
                                rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":获取接口地址失败" + "\r\n");

                                rthText.ScrollToCaret();

                                CallDoEvents();
                            }));

                            return;
                        }

                        ds.Dispose();
                    }
                }
                else
                {
                    return;
                }

                object[] args = new object[iInParaNum];

                int iLoopNum = 1;

                if (InterFaceSet[iSub].IPara != null && InterFaceSet[iSub].IPara.Count > 0 && InterFaceSet[iSub].IPara.Count == iInParaNum)
                {

                    if (!string.IsNullOrEmpty(baseSql))
                    {
                        DataSet dsBase = di.GetDataSetBySql(baseSql);

                        if (dsBase != null && dsBase.Tables.Count > 0 && dsBase.Tables[0].Rows.Count > 0)
                        {
                            clsTableStru cts = new clsTableStru();
                            cts.titleName = bTableName;
                            cts.iRows = 0;
                            cts.table = dsBase.Tables[0];

                            listTable.Add(cts);

                            iLoopNum = dsBase.Tables[0].Rows.Count;
                        }
                        else
                        {
                            rthText.Invoke(new EventHandler(delegate
                            {
                                rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":未查询到可用基础数据" + "\r\n");

                                rthText.ScrollToCaret();

                                CallDoEvents();
                            }));

                            return;

                            //throw new UserException("接口：" + strSubName + "获取基础数据失败");
                        }

                        dsBase.Dispose();
                    }
                    ////////////当前仅考虑所有参数中只有一个是XML结构的情况

                    for (int iLoop = 0; iLoop < iLoopNum; iLoop++)
                    {
                        List<string[]> listArgs = new List<string[]>();
                        List<string> listXml = new List<string>();
                        //int iXml = 0;

                        //20180523  根节点循环无效化后，如果有基表数据的话，在外层进行循环，对接口进行调用
                        //之前的逻辑可能会造成多层循环，数据重复发送，几何级多次无必要的接口调用
                        if (listTable.Count > 0)
                        {
                            listTable[0].iRows = iLoop;
                        }

                        //整合参数
                        for (int i = 0; i < iInParaNum; i++)
                        {
                            string isXml = InterFaceSet[iSub].IPara[i].isXml.ToString().Trim();
                            string getValSql = InterFaceSet[iSub].IPara[i].getValSql;//getvalsql
                            string valTitle = InterFaceSet[iSub].IPara[i].valTitle;
                            string paraStaticVal = InterFaceSet[iSub].IPara[i].paraStaticVal;//allorloop
                            string valByBase = InterFaceSet[iSub].IPara[i].valByBase;
                            string paraName = InterFaceSet[iSub].IPara[i].ParaName;

                            getValSql = ConformSql(getValSql);

                            if (isXml == "1")
                            {
                                //iXml = i;
                                //获取XML结构并且整合成XML字符串返回
                                DataSet dsXml = InterFaceSet[iSub].IPara[i].xml;//da.GetXmlSet(strSubName, ds.Tables[0].Rows[i]["paraname"].ToString().Trim());

                                //
                                string tmp_parm = "";

                                if (InterFaceSet[iSub].IPara[i].isJson == "1")
                                {
                                    listXml = CreateJsonPara(dsXml, InterFaceSet[iSub].IPara[i].paraDll, InterFaceSet[iSub].IPara[i].paraPDll);

                                    //没有整合的xml信息，直接返回，本次不需要进行调用
                                    if (listXml == null || listXml.Count <= 0)
                                    {
                                        rthText.Invoke(new EventHandler(delegate
                                        {
                                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":没有需要上传的数据" + "\r\n");

                                            rthText.ScrollToCaret();

                                            CallDoEvents();
                                        }));

                                        return;
                                    }

                                    tmp_parm = listXml[0];
                                }
                                else
                                {
                                    listXml = CreateXmlPara(dsXml, InterFaceSet[iSub].IPara[i].paraDll, InterFaceSet[iSub].IPara[i].paraPDll);

                                    //没有整合的xml信息，直接返回，本次不需要进行调用
                                    if (listXml == null || listXml.Count <= 0)
                                    {
                                        rthText.Invoke(new EventHandler(delegate
                                        {
                                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":没有需要上传的数据" + "\r\n");

                                            rthText.ScrollToCaret();

                                            CallDoEvents();
                                        }));

                                        return;
                                    }

                                    tmp_parm = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + listXml[0];
                                }

                                args[i] = tmp_parm;

                                if (bTest)
                                {
                                    TraceHelper TraceHelper = new TraceHelper();
                                    TraceHelper.WriteLine(subChiName + ":发送的报文:\r\n" + tmp_parm);
                                }

                                dsXml.Dispose();
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(paraStaticVal))
                                {
                                    //有静态值，静态值优先级最高
                                    args[i] = paraStaticVal;
                                }
                                else if (valByBase == "1")
                                {
                                    //从基表读取数据
                                    args[i] = listTable[0].table.Rows[iLoop][valTitle].ToString().Trim();
                                }
                                else
                                {
                                    //执行提供的SQL语句并且根据列标题获取第一行语句进行赋值
                                    if (!string.IsNullOrEmpty(getValSql))
                                    {
                                        DataSet dsPara = di.GetDataSetBySql(getValSql);

                                        if (dsPara != null && dsPara.Tables.Count > 0 && dsPara.Tables[0].Rows.Count > 0)
                                        {
                                            args[i] = dsPara.Tables[0].Rows[0][valTitle].ToString().Trim();
                                        }
                                        else
                                        {
                                            rthText.Invoke(new EventHandler(delegate
                                            {
                                                rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":参数：" + InterFaceSet[iSub].IPara[i].ParaName + "取值失败,请检查设定的SQL语句" + "\r\n");

                                                rthText.ScrollToCaret();

                                                CallDoEvents();
                                            }));

                                            return;
                                            //throw new UserException("获取参数值失败:" + strSubName + ":" + InterFaceSet[iSub].IPara[i].ParaName);
                                        }

                                        dsPara.Dispose();
                                    }
                                    else
                                    {
                                        args[i] = "";
                                    }
                                }

                                if ((string)args[i] != "")
                                {
                                    if (InterFaceSet[iSub].IPara[i].dataType == "日期")
                                    {
                                        args[i] = Convert.ToDateTime(args[i].ToString());
                                    }
                                    else if (InterFaceSet[iSub].IPara[i].dataType == "整数")
                                    {
                                        args[i] = Convert.ToInt32(args[i].ToString());
                                    }
                                    else if (InterFaceSet[iSub].IPara[i].dataType == "复数")
                                    {
                                        args[i] = Convert.ToDouble(args[i].ToString());
                                    }
                                }

                            }
                        }

                        //for (int m = 0; m < listXml.Count; m++)
                        //{
                        ////讲XML参数值填写进参数数组中
                        //args[iXml] = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + listXml[m];
                        for (int i = 0; i < iInParaNum; i++)
                        {
                            if (InterFaceSet[iSub].IPara[i].dataDllDetail == "1")
                            {
                                object[] ob = new object[InterFaceSet[iSub].IPara[i].dllSet.paraNum];

                                for (int iPa = 0; iPa < InterFaceSet[iSub].IPara[i].dllSet.paraNum; iPa++)
                                {
                                    ob[iPa] = args[i];

                                    if (!string.IsNullOrEmpty(InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue))
                                    {
                                        if (InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue.Contains("#H#"))
                                        {
                                            string strTi = InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue.Replace("#H#", "");

                                            int iDot = strTi.IndexOf(".");
                                            //末尾标签的位置
                                            int iEnd = strTi.Length - 4;

                                            //提取从哪个数据表中取数据进行替换
                                            string strTab = strTi.Substring(3, iDot - 3);

                                            int iTab = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTab; });
                                            //提取对应数据表中的列名
                                            string strDataTitle = strTi.Substring(iDot + 1, iEnd - iDot);

                                            //如果数据表集合的长度符合指定的长度
                                            if (iTab >= 0)
                                            {
                                                //根据数据表、对应的行、列名对数据进行替换，整合成真正需要执行的SQL语句
                                                ob[iPa] = listTable[iTab].table.Rows[listTable[iTab].iRows][strDataTitle].ToString().Trim();
                                            }
                                        }
                                        else if (InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue.Contains("#args#"))
                                        {
                                            if (InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue.Contains("->"))
                                            {
                                                string tNode = InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue;
                                                int iLast = Convert.ToInt32(tNode.Substring(tNode.LastIndexOf(":") + 1));

                                                int iM = tNode.IndexOf("->");
                                                int iN = tNode.IndexOf(":");
                                                int iBegin = Convert.ToInt32(tNode.Substring(iN + 1, iM - iN - 1));

                                                object[,] obb = new object[iLast - iBegin + 1, 2];

                                                for (int i_dll = iBegin; i_dll <= iLast; i_dll++)
                                                {
                                                    obb[i_dll - iBegin, 0] = InterFaceSet[iSub].IPara[i_dll].ParaName;
                                                    obb[i_dll - iBegin, 1] = args[i_dll];
                                                }

                                                ob[iPa] = obb;
                                            }
                                            else
                                            {
                                                string tNode = InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue;
                                                int iArgs = Convert.ToInt32(tNode.Substring(tNode.LastIndexOf(":") + 1));
                                                ob[iPa] = args[iArgs];
                                            }
                                        }
                                        else
                                        {
                                            ob[iPa] = InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue;
                                        }
                                    }
                                    //ob[iPa] = tmp_value;
                                }

                                args[i] = Invoke_Dll(InterFaceSet[iSub].IPara[i].dllSet.dllName, InterFaceSet[iSub].IPara[i].dllSet.nameSpace, InterFaceSet[iSub].IPara[i].dllSet.className, InterFaceSet[iSub].IPara[i].dllSet.procName, ob);
                            }
                        }

                        object strResult_tmp = "";

                        string strResult = "";

                        rthText.Invoke(new EventHandler(delegate
                        {
                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":参数整合完成，开始执行WEB接口" + "\r\n");

                            rthText.ScrollToCaret();

                            CallDoEvents();
                        }));

                        if (InterFaceSet[iSub].expDll == "1")
                        {
                            object[] ob = new object[InterFaceSet[iSub].dllSet.paraNum];

                            ob[0] = strAdd;

                            for (int iPa = 1; iPa < InterFaceSet[iSub].dllSet.paraNum; iPa++)
                            {
                                if (InterFaceSet[iSub].dllSet.dllPara[iPa].paraname.ToLower() == "#h#agentip#h#")
                                {
                                    ob[iPa] = strAgentIP;
                                }
                                else if (InterFaceSet[iSub].dllSet.dllPara[iPa].paraname.ToLower() == "#h#agentport#h#")
                                {
                                    ob[iPa] = strAgentPort;
                                }
                                else if (InterFaceSet[iSub].dllSet.dllPara[iPa].paraname.ToLower() == "#h#agentuser#h#")
                                {
                                    ob[iPa] = strAgentUser;
                                }
                                else if (InterFaceSet[iSub].dllSet.dllPara[iPa].paraname.ToLower() == "#h#agentpass#h#")
                                {
                                    ob[iPa] = strAgentPass;
                                }
                                else
                                {
                                    ob[iPa] = args[iPa - 1];
                                }
                            }

                            if (chbusexml.Checked)
                            {
                                strResult_tmp = strTmpTmp;
                            }
                            else
                            {
                                if (bTest)
                                {
                                    string tmpPara = "";

                                    for (int i = 0; i < ob.Length; i++)
                                    {
                                        tmpPara = tmpPara + ob[i].ToString() + ">--||--<";
                                    }

                                    TraceHelper TraceHelper = new TraceHelper();
                                    TraceHelper.WriteLine(subChiName + ":发送的报文:\r\n" + tmpPara);
                                }
                                strResult_tmp = Invoke_Dll(InterFaceSet[iSub].dllSet.dllName, InterFaceSet[iSub].dllSet.nameSpace, InterFaceSet[iSub].dllSet.className, InterFaceSet[iSub].dllSet.procName, ob);
                            }
                        }
                        else
                        {
                            if (chbusexml.Checked)
                            {
                                strResult_tmp = strTmpTmp;
                            }
                            else
                            {
                                //整合函数参数完毕，调用webservice方法
                                strResult_tmp = RunWebService(strAdd, strClassName, strSubName, args, InterFaceSet[iSub].reObject);
                            }
                        }
                        rthText.Invoke(new EventHandler(delegate
                        {
                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":接收到接口返回数据" + "\r\n");

                            rthText.ScrollToCaret();

                            CallDoEvents();
                        }));

                        if (strResult_tmp != null && !string.IsNullOrEmpty(strResult_tmp.ToString()))
                        {

                            if (InterFaceSet[iSub].recDataDatil == "1" && !chbusexml.Checked)
                            {
                                object[] ob = new object[InterFaceSet[iSub].dllSet.paraNum];

                                for (int iPa = 0; iPa < InterFaceSet[iSub].dllSet.paraNum; iPa++)
                                {
                                    object tmp_value = strResult_tmp;

                                    if (!string.IsNullOrEmpty(InterFaceSet[iSub].dllSet.dllPara[iPa].paravalue))
                                    {
                                        tmp_value = InterFaceSet[iSub].dllSet.dllPara[iPa].paravalue;
                                    }

                                    ob[iPa] = tmp_value;
                                }

                                strResult = (string)Invoke_Dll(InterFaceSet[iSub].dllSet.dllName, InterFaceSet[iSub].dllSet.nameSpace, InterFaceSet[iSub].dllSet.className, InterFaceSet[iSub].dllSet.procName, ob);
                            }
                            else
                            {
                                strResult = strResult_tmp.ToString();
                            }

                            if (bTest)
                            {
                                TraceHelper TraceHelper = new TraceHelper();
                                TraceHelper.WriteLine(subChiName + ":接收到的报文:\r\n" + strResult);
                            }

                            string jsonAdd = InterFaceSet[iSub].jsonAdd;

                            //JSON添加内容不为空，则认定返回的是json格式
                            if (!string.IsNullOrEmpty(jsonAdd))
                            {
                                //将json添加内容中的#HH#替换为接收到的数据，组合成可转换为XML的数据
                                jsonAdd = jsonAdd.Replace("#HH#", strResult);

                                //Json转换为XML
                                strResult = JsonConvert.DeserializeXmlNode(jsonAdd).OuterXml;
                            }

                            //判断是否判断返回结果里面的成功标识
                            if (InterFaceSet[iSub].chkReSign == "1")
                            {
                                if (!JudRecSign(iSub, strResult, args, "sub"))
                                {
                                    if (!string.IsNullOrEmpty(InterFaceSet[iSub].reErrorNode))
                                    {
                                        string strMessage = "";

                                        if (InterFaceSet[iSub].reErrorNode.Contains("#args#"))
                                        {
                                            string errNode = InterFaceSet[iSub].reErrorNode;
                                            int iArgs = Convert.ToInt32(errNode.Substring(errNode.LastIndexOf(":") + 1));

                                            strMessage = args[iArgs].ToString();
                                        }
                                        else
                                        {
                                            strMessage = XmlHelper.GetValue(strResult, InterFaceSet[iSub].reErrorNode);
                                        }

                                        rthText.Invoke(new EventHandler(delegate
                                        {
                                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":返回结果--成功验证（失败）:" + strMessage + "\r\n");

                                            rthText.ScrollToCaret();

                                            CallDoEvents();
                                        }));

                                        //失败
                                        TraceHelper TraceHelper = new TraceHelper();
                                        TraceHelper.WriteLine(strMessage);
                                    }
                                    else
                                    {
                                        rthText.Invoke(new EventHandler(delegate
                                        {
                                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":返回结果--成功验证（失败）,不进行回写处理\r\n");

                                            rthText.ScrollToCaret();

                                            CallDoEvents();
                                        }));
                                    }

                                    //继续循环，跳过后面的结果处理过程
                                    if (!bPre)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }

                            //判断是否有回写配置
                            if (InterFaceSet[iSub].IReSet.Count > 0)
                            {
                                InterFaceReSet.Clear();

                                InterFaceReSet.AddRange(InterFaceSet[iSub].IReSet);

                                List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate (clsInterfaceReSet p) { return string.IsNullOrEmpty(p.fathersql); });

                                List<string> listSql = new List<string>();

                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":开始整合回写SQL语句\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));

                                //循环回写配置
                                for (int iReSet = 0; iReSet < tmpList.Count; iReSet++)
                                {
                                    listSql.AddRange(CreateReWriteSql(strResult, tmpList[iReSet]));
                                }

                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":SQL语句整合完成，准备执行\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));

                                if (listSql.Count > 0)
                                {
                                    if (bTest)
                                    {
                                        for (int iii = 0; iii < listSql.Count; iii++)
                                        {
                                            TraceHelper TraceHelper = new TraceHelper();
                                            TraceHelper.WriteLine(listSql[iii]);
                                        }
                                    }

                                    di.RunSql(listSql);
                                }

                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":SQL语句执行成功，回写完成，共执行 " + listSql.Count.ToString() + " 条SQL语句\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));

                            }
                            else
                            {
                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":没有回写配置，执行成功\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));
                            }

                            //如果设置了接口执行成功后的后继调用接口，则直接调用对应的接口
                            if (!string.IsNullOrEmpty(InterFaceSet[iSub].FollowIF))
                            {
                                string[] fi = InterFaceSet[iSub].FollowIF.Split(',');

                                //0:chiname,1:subname
                                int iiSub = InterFaceSet.FindIndex(delegate (clsInterfaceSet p) { return p.SubName == fi[1] && p.ChiName == fi[0]; });

                                if (iiSub >= 0)
                                {
                                    if (string.IsNullOrEmpty(InterFaceSet[iiSub].SubType) || InterFaceSet[iiSub].SubType == "webservice")
                                    {
                                        //WebService接口执行
                                        RunWebServiceSub(fi[1], fi[0], iiSub, false, false);
                                    }
                                    else
                                    {
                                        RunHttpSub(fi[1], fi[0], iiSub, false, false);
                                    }
                                }
                            }

                            //判断接口是否需要自循环
                            //获取数据的接口，才需要进行设置判断逻辑
                            //发送数据的接口，在基表或者组合XML/Json的根/第一级节点对应的SQL语句查询不到数据时
                            //都会跳出本次，不进行自循环
                            if (InterFaceSet[iSub].loopBySelf == "1")
                            {
                                if (InterFaceSet[iSub].ILoopJudRec.Count > 0)
                                {
                                    //如果有自循环判断逻辑设置，进行计算判断，通过后才进行自循环
                                    if (JudRecSign(iSub, strResult, args, "loop"))
                                    {
                                        RunWebServiceSub(strSubName, subChiName, iSub, true, false);
                                    }
                                }
                                else
                                {
                                    //没有自循环判断逻辑设置，进行自循环
                                    RunWebServiceSub(strSubName, subChiName, iSub, true, false);
                                }
                            }

                            if (bPre)
                            {
                                bPreRunResult = true;
                            }

                        }
                        else
                        {
                            if (InterFaceSet[iSub].reErrorNode.Contains("#args#"))
                            {
                                string errNode = InterFaceSet[iSub].reErrorNode;
                                int iArgs = Convert.ToInt32(errNode.Substring(errNode.LastIndexOf(":") + 1));

                                string strMessage = args[iArgs].ToString();

                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":错误信息" + strMessage + "\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));
                            }
                            else
                            {
                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":返回数据为空\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));
                            }
                        }
                    }

                    if (!bSelf)
                    {
                        rthText.Invoke(new EventHandler(delegate
                        {
                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":接口执行结束\r\n");

                            rthText.ScrollToCaret();

                            CallDoEvents();
                        }));
                    }
                }
                else
                {
                    rthText.Invoke(new EventHandler(delegate
                    {
                        rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":参数配置错误，数量与方法内指定的不一致\r\n");

                        rthText.ScrollToCaret();

                        CallDoEvents();
                    }));
                }
            }
            catch (UserException ex)
            {
                bTriggerResult = false;

                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":接口执行发生异常\r\n" + ex.Message + "\r\n");

                    rthText.ScrollToCaret();

                    CallDoEvents();
                }));
            }
            catch (Exception ex)
            {
                bTriggerResult = false;

                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":接口执行发生异常\r\n" + ex.Message + "\r\n");

                    rthText.ScrollToCaret();

                    CallDoEvents();
                }));
            }
        }
        #endregion "执行WebService接口"

        #region "执行Http接口"
        /// <summary>
        /// 执行Http接口函数
        /// </summary>
        /// <param name="strSubName">函数名称</param>
        /// <param name="subChiName">中文名称</param>
        /// <param name="iSub">接口在接口列表的位置</param>
        /// <param name="bSelf">是否自循环调用</param>
        /// <param name="bPre">是否前置调用</param>
        private void RunHttpSub(string strSubName, string subChiName, int iSub, bool bSelf,bool bPre)
        {
            //DAOInterface di = DAOFactory.CreateDAOInterface();

            try
            {
                bTriggerResult = true;

                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":开始调用接口" + "\r\n");

                    rthText.ScrollToCaret();

                    CallDoEvents();
                }));

                for (int ic = 0; ic < listTable.Count; ic++)
                {
                    listTable[ic].table.Dispose();
                }

                listTable.Clear();
                listXmlNode.Clear();

                string strAdd = "";
                string strClassName = "";
                int iInParaNum = 0;
                //string strReWriteSql = "";
                string baseSql = "";
                string bTableName = "";

                string tmp_url = "";
                string tmp_url_para = "";
                string tmp_parm = "";
                string param = "";

                string jsonAdd = "";
                string encodtype = "";
                string contentType = "";

                List<clsHttpHeader> listHeader = new List<clsHttpHeader>();


                //int iSub = InterFaceSet.FindIndex(delegate(clsInterfaceSet p) { return p.SubName == strSubName && p.ChiName == subChiName; });

                if (iSub >= 0)
                {
                    strAdd = InterFaceSet[iSub].strAdd;
                    strClassName = InterFaceSet[iSub].ClassName;
                    iInParaNum = InterFaceSet[iSub].InParaNum;
                    //strReWriteSql = InterFaceSet[iSub].ReWriteSql;
                    baseSql = InterFaceSet[iSub].baseSql;
                    bTableName = InterFaceSet[iSub].bTableName;

                    jsonAdd = InterFaceSet[iSub].jsonAdd;
                    encodtype = InterFaceSet[iSub].encodType;
                    contentType = InterFaceSet[iSub].contentType;

                    //判断接口地址是否采用SQL语句在数据库中进行查询的方式
                    if (strAdd.Length > 4 && strAdd.Substring(0, 4).ToUpper() == "SQL:")
                    {
                        string strSql = strAdd.Substring(4);

                        using (DataSet ds = dii.GetDataSetBySql(strSql))
                        {
                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                strAdd = ds.Tables[0].Rows[0][0].ToString().Trim();
                            }
                            else
                            {
                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":获取接口地址失败" + "\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));

                                return;
                            }
                        }
                        //ds.Dispose();
                    }

                }
                else
                {
                    return;
                }

                object[] args = new object[iInParaNum];

                //string[] args = new string[iInParaNum];

                int iLoopNum = 1;

                if (InterFaceSet[iSub].IPara != null && InterFaceSet[iSub].IPara.Count >= 0 && InterFaceSet[iSub].IPara.Count == iInParaNum)
                {

                    if (!string.IsNullOrEmpty(baseSql))
                    {
                        using (DataSet dsBase = dii.GetDataSetBySql(baseSql))
                        {
                            if (dsBase != null && dsBase.Tables.Count > 0 && dsBase.Tables[0].Rows.Count > 0)
                            {
                                clsTableStru cts = new clsTableStru();
                                cts.titleName = bTableName;
                                cts.iRows = 0;
                                cts.table = dsBase.Tables[0];

                                listTable.Add(cts);

                                iLoopNum = dsBase.Tables[0].Rows.Count;
                            }
                            else
                            {
                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":未查询到可用基础数据" + "\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));

                                return;

                                //throw new UserException("接口：" + strSubName + "获取基础数据失败");
                            }
                        }
                    }
                    //当前仅考虑所有参数中只有一个是XML结构的情况

                    for (int iLoop = 0; iLoop < iLoopNum; iLoop++)
                    {
                        bool bNext = false;

                        List<string[]> listArgs = new List<string[]>();
                        List<string> listXml = new List<string>();
                        listHeader.Clear();
                        tmp_url_para = "";
                        tmp_url = "";
                        param = "";

                        string strAdd_tmp = strAdd;
                        //int iXml = 0;

                        //20180523  根节点循环无效化后，如果有基表数据的话，在外层进行循环，对接口进行调用
                        //之前的逻辑可能会造成多层循环，数据重复发送，几何级多次无必要的接口调用
                        //listTable[0].iRows = iLoop;
                        if (listTable.Count > 0)
                        {
                            listTable[0].iRows = iLoop;
                        }
                        
                        for (int i = 0; i < iInParaNum; i++)
                        {
                            string isXml = InterFaceSet[iSub].IPara[i].isXml.ToString().Trim();
                            string getValSql = InterFaceSet[iSub].IPara[i].getValSql;//getvalsql
                            string valTitle = InterFaceSet[iSub].IPara[i].valTitle;
                            string paraStaticVal = InterFaceSet[iSub].IPara[i].paraStaticVal;//allorloop
                            string valByBase = InterFaceSet[iSub].IPara[i].valByBase;
                            string paraName = InterFaceSet[iSub].IPara[i].ParaName;

                            getValSql = ConformSql(getValSql);

                            if (isXml == "1")
                            {
                                string tmp_parm_l = "";
                                //iXml = i;
                                //获取XML结构并且整合成XML字符串返回
                                using (DataSet dsXml = InterFaceSet[iSub].IPara[i].xml)//da.GetXmlSet(strSubName, ds.Tables[0].Rows[i]["paraname"].ToString().Trim());
                                {
                                    if (InterFaceSet[iSub].IPara[i].isJson == "1")
                                    {
                                        listXml = CreateJsonPara(dsXml, InterFaceSet[iSub].IPara[i].paraDll, InterFaceSet[iSub].IPara[i].paraPDll);

                                        //没有整合的xml信息，直接返回，本次不需要进行调用
                                        if (listXml == null || listXml.Count <= 0)
                                        {
                                            rthText.Invoke(new EventHandler(delegate
                                            {
                                                rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":没有需要上传的数据" + "\r\n");

                                                rthText.ScrollToCaret();

                                                CallDoEvents();
                                            }));

                                            bNext = true;

                                            break;

                                        }

                                        
                                        if (!string.IsNullOrEmpty(InterFaceSet[iSub].IPara[i].paraStaticVal))
                                        {
                                            tmp_parm_l = InterFaceSet[iSub].IPara[i].paraStaticVal + listXml[0];
                                        }
                                        else
                                        {
                                            tmp_parm_l = listXml[0];
                                        }
                                    }
                                    else
                                    {
                                        listXml = CreateXmlPara(dsXml, InterFaceSet[iSub].IPara[i].paraDll, InterFaceSet[iSub].IPara[i].paraPDll);

                                        //没有整合的xml信息，直接返回，本次不需要进行调用
                                        if (listXml == null || listXml.Count <= 0)
                                        {
                                            rthText.Invoke(new EventHandler(delegate
                                            {
                                                rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":没有需要上传的数据" + "\r\n");

                                                rthText.ScrollToCaret();

                                                CallDoEvents();
                                            }));

                                            return;
                                        }

                                        if (InterFaceSet[iSub].IPara[i].withxml == "1")
                                        {
                                            tmp_parm_l = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + listXml[0];
                                        }
                                        else
                                        {
                                            tmp_parm_l = listXml[0];
                                        }

                                    }
                                }
                                args[i] = tmp_parm_l;

                                if (bTest)
                                {
                                    TraceHelper TraceHelper = new TraceHelper();
                                    TraceHelper.WriteLine(subChiName + ":" + InterFaceSet[iSub].IPara[i].ParaName + ":整合的报文:\r\n" + tmp_parm_l);
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(paraStaticVal))
                                {
                                    //有静态值，静态值优先级最高
                                    args[i] = paraStaticVal;
                                }
                                else if (valByBase == "1")
                                {
                                    //从基表读取数据
                                    args[i] = listTable[0].table.Rows[iLoop][valTitle].ToString().Trim();
                                }
                                else
                                {
                                    //执行提供的SQL语句并且根据列标题获取第一行语句进行赋值
                                    if (!string.IsNullOrEmpty(getValSql))
                                    {
                                        using (DataSet dsPara = dii.GetDataSetBySql(getValSql))
                                        {
                                            if (dsPara != null && dsPara.Tables.Count > 0 && dsPara.Tables[0].Rows.Count > 0)
                                            {
                                                args[i] = dsPara.Tables[0].Rows[0][valTitle].ToString().Trim();
                                            }
                                            else
                                            {
                                                rthText.Invoke(new EventHandler(delegate
                                                {
                                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":参数：" + InterFaceSet[iSub].IPara[i].ParaName + "取值失败,请检查设定的SQL语句" + "\r\n");

                                                    rthText.ScrollToCaret();

                                                    CallDoEvents();
                                                }));

                                                return;

                                                //throw new UserException("获取参数值失败:" + strSubName + ":" + InterFaceSet[iSub].IPara[i].ParaName);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        args[i] = "";
                                    }
                                }

                                if ((string)args[i] != "")
                                {
                                    if (InterFaceSet[iSub].IPara[i].dataType == "日期")
                                    {
                                        args[i] = Convert.ToDateTime(args[i].ToString());
                                    }
                                    else if (InterFaceSet[iSub].IPara[i].dataType == "整数")
                                    {
                                        args[i] = Convert.ToInt32(args[i].ToString());
                                    }
                                    else if (InterFaceSet[iSub].IPara[i].dataType == "复数")
                                    {
                                        args[i] = Convert.ToDouble(args[i].ToString());
                                    }
                                }

                            }
                        }

                        if (bNext)
                        {
                            continue;
                        }

                        for (int i = 0; i < iInParaNum; i++)
                        {
                            if (InterFaceSet[iSub].IPara[i].dataDllDetail == "1")
                            {
                                object[] ob = new object[InterFaceSet[iSub].IPara[i].dllSet.paraNum];

                                for (int iPa = 0; iPa < InterFaceSet[iSub].IPara[i].dllSet.paraNum; iPa++)
                                {
                                    ob[iPa] = args[i];

                                    if (!string.IsNullOrEmpty(InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue))
                                    {
                                        if (InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue.Contains("#H#"))
                                        {
                                            string strTi = InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue.Replace("#H#", "");

                                            int iDot = strTi.IndexOf(".");
                                            //末尾标签的位置
                                            int iEnd = strTi.Length - 1;

                                            //提取从哪个数据表中取数据进行替换
                                            string strTab = strTi.Substring(0, iDot);

                                            int iTab = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == strTab; });
                                            //提取对应数据表中的列名
                                            string strDataTitle = strTi.Substring(iDot + 1, iEnd - iDot);

                                            //如果数据表集合的长度符合指定的长度
                                            if (iTab>=0)
                                            {
                                                //根据数据表、对应的行、列名对数据进行替换，整合成真正需要执行的SQL语句
                                                ob[iPa] = listTable[iTab].table.Rows[listTable[iTab].iRows][strDataTitle].ToString().Trim();
                                            }
                                        }
                                        else if (InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue.Contains("#args#"))
                                        {
                                            if (InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue.Contains("->"))
                                            {
                                                string tNode = InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue;
                                                int iLast = Convert.ToInt32(tNode.Substring(tNode.LastIndexOf(":") + 1));

                                                int iM = tNode.IndexOf("->");
                                                int iN = tNode.IndexOf(":");
                                                int iBegin = Convert.ToInt32(tNode.Substring(iN + 1, iM - iN - 1));

                                                object[,] obb = new object[iLast - iBegin + 1, 2];

                                                for (int i_dll = iBegin; i_dll <= iLast; i_dll++)
                                                {
                                                    obb[i_dll - iBegin, 0] = InterFaceSet[iSub].IPara[i_dll].ParaName;
                                                    obb[i_dll - iBegin, 1] = args[i_dll];
                                                }

                                                ob[iPa] = obb;
                                            }
                                            else
                                            {
                                                string tNode = InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue;
                                                int iArgs = Convert.ToInt32(tNode.Substring(tNode.LastIndexOf(":") + 1));
                                                ob[iPa] = args[iArgs];
                                            }
                                        }
                                        else
                                        {
                                            ob[iPa] = InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue;
                                        }
                                    }
                                    //ob[iPa] = tmp_value;
                                }

                                args[i] = Invoke_Dll(InterFaceSet[iSub].IPara[i].dllSet.dllName, InterFaceSet[iSub].IPara[i].dllSet.nameSpace, InterFaceSet[iSub].IPara[i].dllSet.className, InterFaceSet[iSub].IPara[i].dllSet.procName, ob);

                                ob = null;
                            }
                        }
                        
                        var dic = new Dictionary<string, string>();

                        if (InterFaceSet[iSub].SubType == "http-get" || InterFaceSet[iSub].SubType == "http-post")
                        {
                            tmp_url = string.Empty;
                            tmp_url_para = string.Empty;

                            for (int i = 0; i < iInParaNum; i++)
                            {
                                if (InterFaceSet[iSub].IPara[i].paType == "地址")
                                {
                                    if (!string.IsNullOrEmpty(tmp_url))
                                    {
                                        tmp_url = tmp_url + "/";
                                    }

                                    tmp_url = tmp_url + (string)args[i];
                                }
                                else if (InterFaceSet[iSub].IPara[i].paType == "地参")
                                {
                                    if (!string.IsNullOrEmpty(tmp_url_para))
                                    {
                                        tmp_url_para = tmp_url_para + "&";
                                    }

                                    tmp_url_para = tmp_url_para + InterFaceSet[iSub].IPara[i].ParaName + "=" + (string)args[i];
                                }
                                else if (InterFaceSet[iSub].IPara[i].paType == "header")
                                {
                                    clsHttpHeader cHeader = new clsHttpHeader();

                                    cHeader.HeaderName = InterFaceSet[iSub].IPara[i].ParaName;
                                    cHeader.HeaderValue = (string)args[i];

                                    listHeader.Add(cHeader);
                                }
                                else if (InterFaceSet[iSub].IPara[i].paType == "content")
                                {
                                    if (!string.IsNullOrEmpty(param))
                                    {
                                        param = param + "&";
                                    }

                                    param = param + (string)args[i];
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(param))
                                    {
                                        param = param + "&";
                                    }

                                    param = param + InterFaceSet[iSub].IPara[i].ParaName + "=" + (string)args[i];
                                }
                            }

                            if (!string.IsNullOrEmpty(tmp_url))
                            {
                                strAdd_tmp = strAdd_tmp + tmp_url;
                            }

                            if (!string.IsNullOrEmpty(tmp_url_para))
                            {
                                strAdd_tmp = strAdd_tmp + "?" + tmp_url_para;

                            }
                        }
                        else
                        {
                            for (int i = 0; i < iInParaNum; i++)
                            {
                                if (InterFaceSet[iSub].IPara[i].paType == "地参")
                                {
                                    if (!string.IsNullOrEmpty(tmp_url_para))
                                    {
                                        tmp_url_para = tmp_url_para + "&";
                                    }

                                    tmp_url_para = tmp_url_para + InterFaceSet[iSub].IPara[i].ParaName + "=" + (string)args[i];
                                }
                                else
                                {
                                    dic.Add(InterFaceSet[iSub].IPara[i].ParaName, (string)args[i]);

                                    if (chkTest.Checked)
                                    {
                                        TraceHelper TraceHelper = new TraceHelper();
                                        TraceHelper.WriteLine(InterFaceSet[iSub].IPara[i].ParaName + " : " + (string)args[i]);
                                    }
                                }
                            }
                            
                            if (!string.IsNullOrEmpty(tmp_url_para))
                            {
                                strAdd_tmp = strAdd_tmp + "?" + tmp_url_para;
                            }
                        }

                        rthText.Invoke(new EventHandler(delegate
                        {
                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":参数整合完毕，WEB接口调用" + "\r\n");

                            rthText.ScrollToCaret();

                            CallDoEvents();
                        }));

                        //整合函数参数完毕，调用webservice方法
                        string strResult = "";//RunWebServiceSub(strAdd, strClassName, strSubName, args);

                        //if (bTest)
                        //{
                        //    if (!string.IsNullOrEmpty(param))
                        //    {
                        //        TraceHelper.WriteLine(subChiName + ":发送的报文:\r\n" + param);
                        //    }

                        //    //for(int iii=0;iii<)
                        //}

                        if (InterFaceSet[iSub].SubType == "http-get")
                        {
                            //if (bTest)
                            //{
                            //    TraceHelper.WriteLine(subChiName + ":地址:" + strAdd_tmp);

                            //    for (int iii = 0; iii < listHeader.Count; iii++)
                            //    {
                            //        TraceHelper.WriteLine(subChiName + ":Header：" + listHeader[iii].HeaderName + "<--:-->" + listHeader[iii].HeaderValue);
                            //    }
                            //}

                            strResult = HttpGet(strAdd_tmp, contentType, encodtype, listHeader);
                        }
                        else if (InterFaceSet[iSub].SubType == "http-post")
                        {
                            strResult = HttpPost(strAdd_tmp, contentType, encodtype, param, listHeader);
                        }
                        else if (InterFaceSet[iSub].SubType == "webclient-post")  //webclient-post   webclient-get
                        {
                            if (chkTest.Checked)
                            {
                                TraceHelper TraceHelper = new TraceHelper();
                                TraceHelper.WriteLine("url : " + strAdd_tmp);
                            }

                            strResult = WebClientPost(strAdd_tmp, dic, contentType);
                        }
                        else
                        {
                            strResult = WebClientGet(strAdd_tmp, contentType);
                        }

                        rthText.Invoke(new EventHandler(delegate
                        {
                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":接收到接口返回数据" + "\r\n");

                            rthText.ScrollToCaret();

                            CallDoEvents();
                        }));

                        if (!string.IsNullOrEmpty(strResult))
                        {
                            if (bTest)
                            {
                                TraceHelper TraceHelper = new TraceHelper();
                                TraceHelper.WriteLine(subChiName + ":接收到的数据:\r\n" + strResult);
                            }

                            if (InterFaceSet[iSub].recDataDatil == "1")
                            {
                                object[] ob = new object[InterFaceSet[iSub].dllSet.paraNum];

                                for (int iPa = 0; iPa < InterFaceSet[iSub].dllSet.paraNum; iPa++)
                                {
                                    string tmp_value = strResult;

                                    if (!string.IsNullOrEmpty(InterFaceSet[iSub].dllSet.dllPara[iPa].paravalue))
                                    {
                                        tmp_value = InterFaceSet[iSub].dllSet.dllPara[iPa].paravalue;
                                    }

                                    ob[iPa] = tmp_value;
                                }

                                strResult = (string)Invoke_Dll(InterFaceSet[iSub].dllSet.dllName, InterFaceSet[iSub].dllSet.nameSpace, InterFaceSet[iSub].dllSet.className, InterFaceSet[iSub].dllSet.procName, ob);
                                
                                ob = null;
                            }

                            //JSON添加内容不为空，则认定返回的是json格式
                            if (!string.IsNullOrEmpty(jsonAdd))
                            {
                                if (jsonAdd.Contains("SoapDataSet"))
                                {
                                    //对方返回的是以Soap报文的形式返回的DataSet

                                    string strSoapDataSet = XmlHelper.GetValue(strResult, jsonAdd.Substring(12));

                                    strSoapDataSet = "<DataSet>" + strSoapDataSet + "</DataSet>";

                                    DataSet dsSoap = DESerializer<DataSet>(strSoapDataSet);

                                    if (dsSoap != null && dsSoap.Tables != null && dsSoap.Tables.Count > 0)
                                    {
                                        for (int iSoap = 0; iSoap < dsSoap.Tables.Count; iSoap++)
                                        {
                                            clsTableStru cts = new clsTableStru();
                                            string soapTableName = "SoapTable" + (iSoap + 1).ToString(); 
                                            cts.titleName = soapTableName;
                                            cts.iRows = 0;
                                            cts.table = dii.GetDataSetBySql(strSql).Tables[0];

                                            int iTableHave = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == soapTableName; });

                                            if (iTableHave >= 0)
                                            {
                                                //如果存在，则删除掉该数据表实体
                                                //listTable.RemoveAt(iTableHave);
                                                listTable[iTableHave] = cts;
                                            }
                                            else
                                            {
                                                listTable.Add(cts);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //将json添加内容中的#HH#替换为接收到的数据，组合成可转换为XML的数据
                                    strResult = jsonAdd.Replace("#HH#", strResult);

                                    //Json转换为XML
                                    strResult = JsonConvert.DeserializeXmlNode(strResult).OuterXml;
                                }
                            }

                            
                            //判断是否判断返回结果里面的成功标识
                            if (InterFaceSet[iSub].chkReSign == "1")
                            {
                                if (!JudRecSign(iSub, strResult, args, "sub"))
                                {
                                    if (!string.IsNullOrEmpty(InterFaceSet[iSub].reErrorNode))
                                    {
                                        string strMessage = "";

                                        strMessage = XmlHelper.GetValue(strResult, InterFaceSet[iSub].reErrorNode);

                                        rthText.Invoke(new EventHandler(delegate
                                        {
                                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":返回结果--成功验证（失败）:" + strMessage + "\r\n");

                                            rthText.ScrollToCaret();

                                            CallDoEvents();
                                        }));

                                        //失败
                                        TraceHelper TraceHelper = new TraceHelper();
                                        TraceHelper.WriteLine(strMessage);
                                    }
                                    else
                                    {
                                        rthText.Invoke(new EventHandler(delegate
                                        {
                                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":返回结果--成功验证（失败）,不进行回写处理" + "\r\n");

                                            rthText.ScrollToCaret();

                                            CallDoEvents();
                                        }));
                                    }

                                    //继续循环，跳过后面的结果处理过程
                                    if (!bPre)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            }

                            //判断是否有回写配置
                            if (InterFaceSet[iSub].IReSet.Count > 0)
                            {
                                InterFaceReSet.Clear();

                                InterFaceReSet.AddRange(InterFaceSet[iSub].IReSet);

                                List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return string.IsNullOrEmpty(p.fathersql); });

                                List<string> listSql = new List<string>();

                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":开始整合回写SQL语句" + "\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));

                                //循环回写配置
                                for (int iReSet = 0; iReSet < tmpList.Count; iReSet++)
                                {
                                    listSql.AddRange(CreateReWriteSql(strResult, tmpList[iReSet]));
                                }

                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":回写语句整合完成，开始执行SQL语句" + "\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));

                                if (listSql.Count > 0)
                                {
                                    if (bTest)
                                    {
                                        for (int iii = 0; iii < listSql.Count; iii++)
                                        {
                                            TraceHelper TraceHelper = new TraceHelper();
                                            TraceHelper.WriteLine(listSql[iii]);
                                        }
                                    }

                                    dii.RunSql(listSql);

                                    //listSql.Clear();

                                    //listSql = null;
                                }

                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":SQL语句执行成功，回写完成，共执行 " + listSql.Count.ToString() + " 条SQL语句\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));
                            }
                            else
                            {
                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":没有回写配置，接口执行完成" + "\r\n");

                                    rthText.ScrollToCaret();

                                    CallDoEvents();
                                }));
                            }
                            
                            //rthText.Invoke(new EventHandler(delegate
                            //{
                            //    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":回写被注释掉了，执行完毕一次" + "\r\n");

                            //    rthText.ScrollToCaret();

                            //    CallDoEvents();
                            //}));

                            //如果设置了接口执行成功后的后继调用接口，则直接调用对应的接口
                            if (!string.IsNullOrEmpty(InterFaceSet[iSub].FollowIF))
                            {
                                string[] fi = InterFaceSet[iSub].FollowIF.Split(',');

                                //0:chiname,1:subname
                                int iiSub = InterFaceSet.FindIndex(delegate(clsInterfaceSet p) { return p.SubName == fi[1] && p.ChiName == fi[0]; });

                                if (iiSub >= 0)
                                {
                                    if (string.IsNullOrEmpty(InterFaceSet[iiSub].SubType) || InterFaceSet[iiSub].SubType == "webservice")
                                    {
                                        //WebService接口执行
                                        RunWebServiceSub(fi[1], fi[0], iiSub, false, false);
                                    }
                                    else
                                    {
                                        RunHttpSub(fi[1], fi[0], iiSub, false, false);
                                    }
                                }
                            }

                            //判断接口是否需要自循环
                            //获取数据的接口，才需要进行设置判断逻辑
                            //发送数据的接口，在基表或者组合XML/Json的根/第一级节点对应的SQL语句查询不到数据时
                            //都会跳出本次，不进行自循环
                            if (InterFaceSet[iSub].loopBySelf == "1")
                            {
                                if (InterFaceSet[iSub].ILoopJudRec.Count > 0)
                                {
                                    //如果有自循环判断逻辑设置，进行计算判断，通过后才进行自循环
                                    if (JudRecSign(iSub, strResult, args, "loop"))
                                    {
                                        RunHttpSub(strSubName, subChiName, iSub, true, false);
                                    }
                                }
                                else
                                {
                                    //没有自循环判断逻辑设置，进行自循环
                                    RunHttpSub(strSubName, subChiName, iSub, true, false);
                                }
                            }

                            if (bPre)
                            {
                                bPreRunResult = true;
                            }

                        }
                        else
                        {
                            rthText.Invoke(new EventHandler(delegate
                            {
                                rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":返回数据为空值" + "\r\n");

                                rthText.ScrollToCaret();

                                CallDoEvents();
                            }));
                        }
                        //}

                        strResult = null;
                    }
                    
                    if (!bSelf)
                    {
                        rthText.Invoke(new EventHandler(delegate
                        {
                            rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":接口执行结束\r\n");

                            rthText.ScrollToCaret();

                            CallDoEvents();
                        }));
                    }

                }
                else
                {
                    rthText.Invoke(new EventHandler(delegate
                    {
                        rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":参数配置错误，数量与方法内指定的不一致" + "\r\n");

                        rthText.ScrollToCaret();

                        CallDoEvents();
                    }));
                }
            }
            catch (UserException ex)
            {
                bTriggerResult = false;

                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":接口执行发生异常\r\n" + ex.Message + "\r\n");

                    rthText.ScrollToCaret();

                    CallDoEvents();
                }));
            }
            catch (Exception ex)
            {
                bTriggerResult = false;

                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":接口执行发生异常\r\n" + ex.Message + "\r\n");

                    rthText.ScrollToCaret();

                    CallDoEvents();
                }));
            }

            Thread.Sleep(100);
        }
        #endregion "执行Http接口"

        #region "WEBSERVICE SUB"
        /// <summary>
        /// 执行WEBSERVICE函数
        /// </summary>
        /// <param name="strAdd">地址（添加?wsdl后，可以获取wsdl判断的地址部分）</param>
        /// <param name="strClassName">类名</param>
        /// <param name="strSubName">函数名</param>
        /// <param name="args">参数组</param>
        /// <returns>返回值</returns>
        private object RunWebService(string strAdd, string strClassName, string strSubName, object[] args, string reObject)
        {
            try
            {
                CompileWebService cw = new CompileWebService();

                string classname = "";

                CompilerResults cr = cw.CreateCRByUrl(strAdd, ref classname);

                if (string.IsNullOrEmpty(strClassName))
                {
                    strClassName = classname;
                }

                RunWebServiceMethod rw = new RunWebServiceMethod();
                
                object or = rw.RunMethod(cr, strClassName, strSubName, args);
                
                if (reObject == "1")
                {
                    string typeName = or.ToString();

                    if (typeName.IndexOf(".") > 1)
                    {
                        typeName = typeName.Substring(typeName.LastIndexOf(".") + 1);
                    }

                    Type t = rw.GetType(cr, typeName);
                    
                    return Serializer(t, or);
                }
                else
                {
                    return or;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message != null)
                {
                    throw new UserException(ex.Message + ":" + ex.InnerException.Message);
                }
                else
                {
                    throw new UserException(ex.Message);
                }
            }
        }
        #endregion "WEBSERVICE SUB"

        #region "将接收到的实体对象数据，反序列化成XML字符串"
        public string Serializer(Type type, object obj)
        {
            MemoryStream Stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(type);

            try
            {
                //序列化对象
                xml.Serialize(Stream, obj);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            Stream.Position = 0;
            StreamReader sr = new StreamReader(Stream);
            string str = sr.ReadToEnd();

            sr.Dispose();
            Stream.Dispose();

            return str;
        }
        #endregion "将接收到的实体对象数据，反序列化成XML字符串"

        public object Deserialize(Type type, string xml)
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes = new XmlSerializer(type);
                    return xmldes.Deserialize(sr);
                }
            }
            catch (Exception e)
            {

                return null;
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }

        #region "HTTP GET"
        /// <summary>
        /// HTTP GET 方法调用
        /// </summary>
        /// <param name="url">全地址，如果附带url参数的，需要先组合完参数</param>
        /// <param name="contenttype">WebRequest contenttype</param>
        /// <param name="en_coding">字符集</param>
        /// <param name="header">header</param>
        /// <returns>结果信息</returns>
        private string HttpGet(string url, string contenttype, string en_coding, List<clsHttpHeader> header)
        {
            System.Net.HttpWebResponse response;
            try
            {
                if (bTest)
                {
                    TraceHelper TraceHelper = new TraceHelper();
                    TraceHelper.WriteLine("HTTP调用地址：" + url);
                    //TraceHelper.WriteLine("HTTP调用数据：" + param);

                    for (int i = 0; i < header.Count; i++)
                    {
                        TraceHelper.WriteLine("HTTP调用Header<" + i.ToString() + ">|name：" + header[i].HeaderName + "|value：" + header[i].HeaderValue);
                    }
                }

                //System.Net.ServicePointManager.ServerCertificateValidationCallback += (object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error) => { return true; };

                HttpWebRequest myHttpWebRequest = null;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    myHttpWebRequest = WebRequest.Create(url) as HttpWebRequest;
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    myHttpWebRequest.ProtocolVersion = HttpVersion.Version11;
                    // 这里设置了协议类型。
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;// SecurityProtocolType.Tls1.2; 
                    myHttpWebRequest.KeepAlive = false;
                    ServicePointManager.CheckCertificateRevocationList = true;
                    ServicePointManager.DefaultConnectionLimit = 100;
                    ServicePointManager.Expect100Continue = false;
                }
                else
                {
                    myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                }


                //System.Net.WebRequest myHttpWebRequest = System.Net.WebRequest.Create(url);

                if (!string.IsNullOrEmpty(strAgentIP))
                {
                    WebProxy p = new WebProxy(strAgentIP, Convert.ToInt32(strAgentPort));

                    if (!string.IsNullOrEmpty(strAgentUser))
                    {
                        ICredentials jxCredt = new NetworkCredential(strAgentUser, strAgentPass);

                        p.Credentials = jxCredt;
                    }

                    myHttpWebRequest.Proxy = p;
                }

                myHttpWebRequest.Method = "get";
                myHttpWebRequest.ContentType = contenttype;

                myHttpWebRequest.Referer = null;
                myHttpWebRequest.AllowAutoRedirect = true;
                myHttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                myHttpWebRequest.Accept = "*/*";

                if (header != null)
                {
                    for (int i = 0; i < header.Count; i++)
                    {
                        myHttpWebRequest.Headers.Add(header[i].HeaderName, header[i].HeaderValue);
                    }
                }

                response = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                string lcHtml = string.Empty;
                Encoding enc = Encoding.GetEncoding(en_coding);
                System.IO.Stream stream = response.GetResponseStream();
                System.IO.StreamReader streamReader = new System.IO.StreamReader(stream, enc);
                lcHtml = streamReader.ReadToEnd();
                return lcHtml;
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;

                if (response != null)
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    string result = sr.ReadToEnd();

                    if (strWebEx == "1")
                    {
                        return result;
                    }
                    else
                    {
                        throw new UserException(result);
                    }
                }
                else
                {
                    throw new UserException(ex.Message);
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message;

                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    a = a + "\r\n" + ex.InnerException.Message;
                }

                throw new UserException(a);
            }
        }
        #endregion "HTTP GET"

        #region "HTTP POST"
        /// <summary>
        /// HTTP POST 方法调用
        /// </summary>
        /// <param name="url">全地址，如果附带url参数的，需要先组合完参数</param>
        /// <param name="contenttype">WebRequest contenttype</param>
        /// <param name="en_coding">字符集</param>
        /// <param name="param">数据</param>
        /// <param name="header">header</param>
        /// <returns>结果信息</returns>
        public string HttpPost(string url, string contenttype, string en_coding, string param, List<clsHttpHeader> header)
        {
            System.Net.HttpWebResponse response;

            try
            {
                if (bTest)
                {
                    TraceHelper TraceHelper = new TraceHelper();
                    TraceHelper.WriteLine("HTTP调用地址：" + url);
                    TraceHelper.WriteLine("HTTP调用数据：" + param);

                    for (int i = 0; i < header.Count; i++)
                    {
                        TraceHelper.WriteLine("HTTP调用Header<" + i.ToString() + ">|name：" + header[i].HeaderName + "|value：" + header[i].HeaderValue);
                    }
                }

                //System.Net.ServicePointManager.ServerCertificateValidationCallback += (object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error) => { return true; };

                HttpWebRequest myHttpWebRequest = null;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    myHttpWebRequest = WebRequest.Create(url) as HttpWebRequest;
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    myHttpWebRequest.ProtocolVersion = HttpVersion.Version11;
                    // 这里设置了协议类型。
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;// SecurityProtocolType.Tls1.2; 
                    myHttpWebRequest.KeepAlive = false;
                    ServicePointManager.CheckCertificateRevocationList = true;
                    ServicePointManager.DefaultConnectionLimit = 100;
                    ServicePointManager.Expect100Continue = false;
                }
                else
                {
                    myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                }


                if (!string.IsNullOrEmpty(strAgentIP))
                {
                    WebProxy p = new WebProxy(strAgentIP, Convert.ToInt32(strAgentPort));

                    if (!string.IsNullOrEmpty(strAgentUser))
                    {
                        ICredentials jxCredt = new NetworkCredential(strAgentUser, strAgentPass);

                        p.Credentials = jxCredt;
                    }

                    myHttpWebRequest.Proxy = p;
                }

                myHttpWebRequest.Method = "post";
                myHttpWebRequest.ContentType = contenttype;
                myHttpWebRequest.Referer = null;
                myHttpWebRequest.AllowAutoRedirect = true;
                myHttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                myHttpWebRequest.Accept = "*/*";


                Encoding encoding = Encoding.GetEncoding(en_coding);

                //UTF8Encoding encoding = new UTF8Encoding();

                if (header != null && header.Count > 0)
                {
                    for (int i = 0; i < header.Count; i++)
                    {
                        myHttpWebRequest.Headers.Add(header[i].HeaderName, header[i].HeaderValue);
                    }
                }

                if (!string.IsNullOrEmpty(param))
                {
                    byte[] byte1 = encoding.GetBytes(param);
                    myHttpWebRequest.ContentLength = byte1.Length;

                    System.IO.Stream newStream = myHttpWebRequest.GetRequestStream();
                    newStream.Write(byte1, 0, byte1.Length);
                    newStream.Close();
                }
                else
                {
                    myHttpWebRequest.ContentLength = 0;
                }

                //myHttpWebRequest.Headers.Add("Authorization", "Bearer e10adc3949ba59abbe56e057f20f883e"); 
                response = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                string lcHtml = string.Empty;
                Encoding enc = Encoding.GetEncoding(en_coding);
                System.IO.Stream stream = response.GetResponseStream();
                System.IO.StreamReader streamReader = new System.IO.StreamReader(stream, enc);
                lcHtml = streamReader.ReadToEnd();
                return lcHtml;

            }
             catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;

                if (response != null)
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(),Encoding.UTF8);
                    string result = sr.ReadToEnd();

                    if (strWebEx == "1")
                    {
                        return result;
                    }
                    else
                    {
                        throw new UserException(result);
                    }
                }
                else
                {
                    throw new UserException(ex.Message);
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message;

                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    a = a + "\r\n" + ex.InnerException.Message;
                }

                throw new UserException(a);
            }
        }
        #endregion "HTTP POST"
        
        #region WebClientPost
        /// <summary>
        /// WebClientPost
        /// </summary>
        /// <param name="url">函数对应的地址</param>
        /// <param name="dic">参数</param>
        /// <param name="zip">压缩算法 GZIP DEFLATE</param>
        /// <returns></returns>
        public string WebClientPost(string url, Dictionary<string, string> dic, string zip)
        {
            try
            {
                var handler = new HttpClientHandler();

                if (zip.ToUpper() == "GZIP")
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip;
                }
                else if(zip.ToUpper() == "DEFLATE")
                {
                    handler.AutomaticDecompression = DecompressionMethods.Deflate;
                }
                else
                {
                    handler.AutomaticDecompression = DecompressionMethods.None;
                }

                if (!string.IsNullOrEmpty(strAgentIP))
                {
                    WebProxy p = new WebProxy(strAgentIP, Convert.ToInt32(strAgentPort));

                    if (!string.IsNullOrEmpty(strAgentUser))
                    {
                        ICredentials jxCredt = new NetworkCredential(strAgentUser, strAgentPass);

                        p.Credentials = jxCredt;
                    }

                    handler.Proxy = p;
                }

                using (var http = new HttpClient(handler))
                {
                    var content = new FormUrlEncodedContent(dic);
                    
                    var response = http.PostAsync(new Uri(url), content).Result;


                    response.EnsureSuccessStatusCode();

                    return response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (WebException ex)
            {
                System.Net.HttpWebResponse response = (HttpWebResponse)ex.Response;

                if (response != null)
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream(),Encoding.UTF8);
                    string result = sr.ReadToEnd();

                    throw new UserException(result);
                }
                else
                {
                    throw new UserException(ex.Message);
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message;

                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    a = a + "\r\n" + ex.InnerException.Message;

                    if (ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message))
                    {
                        a = a + "\r\n" + ex.InnerException.InnerException.Message;
                    }
                }
                

                throw new UserException(a);
            }
        }
        #endregion

        #region WebClientGet
        /// <summary>
        /// WebClientPost
        /// </summary>
        /// <param name="url">函数对应的地址</param>
        /// <param name="dic">参数</param>
        /// <param name="zip">压缩算法 GZIP DEFLATE</param>
        /// <returns></returns>
        public string WebClientGet(string url, string zip)
        {
            try
            {
                var handler = new HttpClientHandler();

                if (zip.ToUpper() == "GZIP")
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip;
                }
                else if (zip.ToUpper() == "DEFLATE")
                {
                    handler.AutomaticDecompression = DecompressionMethods.Deflate;
                }
                else
                {
                    handler.AutomaticDecompression = DecompressionMethods.None;
                }

                if (!string.IsNullOrEmpty(strAgentIP))
                {
                    WebProxy p = new WebProxy(strAgentIP, Convert.ToInt32(strAgentPort));

                    if (!string.IsNullOrEmpty(strAgentUser))
                    {
                        ICredentials jxCredt = new NetworkCredential(strAgentUser, strAgentPass);

                        p.Credentials = jxCredt;
                    }

                    handler.Proxy = p;
                }

                using (var http = new HttpClient(handler))
                {
                    var response = http.GetAsync(new Uri(url)).Result;

                    response.EnsureSuccessStatusCode();

                    return response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }
        #endregion

        #region post url
        /// <summary>
        /// post url
        /// </summary>
        public string PostUrl(string url, Dictionary<string, string> dic)
        {
            try
            {
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };

                if (!string.IsNullOrEmpty(strAgentIP))
                {
                    WebProxy p = new WebProxy(strAgentIP, Convert.ToInt32(strAgentPort));

                    if (!string.IsNullOrEmpty(strAgentUser))
                    {
                        ICredentials jxCredt = new NetworkCredential(strAgentUser, strAgentPass);

                        p.Credentials = jxCredt;
                    }

                    handler.Proxy = p;
                }

                using (var http = new HttpClient(handler))
                {
                    var content = new FormUrlEncodedContent(dic);

                    var response = http.PostAsync(new Uri(url), content).Result;

                    response.EnsureSuccessStatusCode();

                    return response.Content.ReadAsStringAsync().Result;
                }
            }
            catch(Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }
        #endregion

        private StringBuilder DetailSB(string strXml, clsInterfaceReTableSet crt, string softvare, int i)
        {
            StringBuilder sb = new StringBuilder();

            //判断设定的静态值是否有内容
            string tmpValue = "";
            string strB = "";
            string strE = "";
            if (string.IsNullOrEmpty(crt.staticvalue))
            {
                string strFrom = crt.valuefrom;
                string strNode = "";
                if (strFrom.Contains("#H#"))
                {
                    int iB = strFrom.IndexOf("#H#");

                    int iE = strFrom.LastIndexOf("#H#");

                    string strTmp = strFrom.Substring(iB, iE - iB + 3);

                    strNode = strTmp.Replace("#H#", "");

                    strB = strFrom.Substring(0, iB);
                    strE = strFrom.Substring(iE + 3);
                }
                else
                {
                    strNode = strFrom;
                }

                //没有设置静态值
                if (crt.databy == "接收数据")
                {
                    //从接收的XML数据中取值
                    //string strNode = crt.valuefrom;

                    tmpValue = GetRevXmlValue(strXml, strNode).Replace("'", "''");

                }
                else
                {
                    //从发送的数据表中取值
                    tmpValue = GetSendValue(strNode, 0).Replace("'", "''");
                }
            }
            else
            {
                if (crt.staticvalue.ToUpper() == "#ROWID#")
                {
                    tmpValue = (i + 1).ToString();
                }
                else
                {
                    //设置了静态值，则使用静态值作为字段的内容进行使用
                    tmpValue = crt.staticvalue;
                }
            }

            sb.Append(strB);

            if (crt.isDate == "字符")
            {
                sb.Append("'");
            }
            else if (crt.isDate == "日期" && !string.IsNullOrEmpty(tmpValue))
            {
                if (softvar == "2")
                {
                    sb.Append("to_date('");
                }
                else
                {
                    sb.Append("'");
                }
            }

            if (string.IsNullOrEmpty(tmpValue))
            {
                if (crt.isDate == "数值")
                {
                    sb.Append("0");
                }
                else if (crt.isDate == "日期")
                {
                    if (softvar == "2")
                    {
                        sb.Append("sysdate");
                    }
                    else
                    {
                        sb.Append("getdate()");
                    }
                }
            }
            else
            {
                sb.Append(tmpValue);
            }

            if (crt.isDate == "字符")
            {
                sb.Append("'");
            }
            else if (crt.isDate == "日期" && !string.IsNullOrEmpty(tmpValue))
            {
                if (softvar == "2")
                {
                    sb.Append("','");
                    sb.Append(crt.dateType);
                    sb.Append("')");
                }
                else
                {
                    sb.Append("'");
                }
            }

            sb.Append(strE);

            return sb;
        }

        #region "一堆测试按钮"
        
        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                GetAndBindSubList();
                //DAOAccess da = new DAOAccess();

                //DataSet ds = da.GetInterfaceSub();

                //if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                //{
                //    lvSub.Items.Clear();

                //    InterFaceSet.Clear();

                //    DataTable dt = ds.Tables[0];

                //    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                //    {
                //        string[] strItem = { dt.Rows[i]["chiname"].ToString().Trim(), dt.Rows[i]["subname"].ToString().Trim(), dt.Rows[i]["classname"].ToString().Trim(), dt.Rows[i]["webaddres"].ToString().Trim(), dt.Rows[i]["inparanum"].ToString().Trim(), dt.Rows[i]["outparanum"].ToString().Trim(), dt.Rows[i]["rewsql"].ToString().Trim() };

                //        lvSub.Items.Insert(lvSub.Items.Count, new ListViewItem(strItem));

                //        clsInterfaceSet ISet = new clsInterfaceSet();

                //    }
                //}
            }
            catch (UserException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnTest3_Click(object sender, EventArgs e)
        {
            try
            {
                string strXml = rthText.Text.ToString();

                string strNode = "PurchasePlanResult.PurchasePlans.PurchasePlan";


                XmlNodeList xnl = XmlHelper.GetNodeList(strXml, strNode);

                string value = XmlHelper.GetValue(xnl[0], "DeliveryAddress");

                MessageBox.Show(value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DAOInterface di = DAOFactory.CreateDAOInterface();
            string strResult = rthText.Text.ToString(); //RunWebServiceSub(strAdd, strClassName, strSubName, args);
            string strSubName = lvSub.SelectedItems[0].SubItems[1].Text.ToString().Trim();
            int iSub = InterFaceSet.FindIndex(delegate(clsInterfaceSet p) { return p.SubName == strSubName; });

            if (InterFaceSet[iSub].chkReSign == "1")
            {
                //需要判断是否成功
                //读取对应成功标识节点的内容
                //string reSign = XmlHelper.GetValue(strResult, InterFaceSet[iSub].reSignNode);

                ////判断结果标识节点的内容是否等于配置的成功的值
                //if (reSign == InterFaceSet[iSub].reTrueValue)
                //{
                //    //成功
                //    //可以处理数据
                //}
                if (JudRecSign(iSub, strResult, null,"sub"))
                { }
                else
                {
                    //失败
                    TraceHelper TraceHelper = new TraceHelper();
                    TraceHelper.WriteLine(XmlHelper.GetValue(strResult, InterFaceSet[iSub].reErrorNode));

                    //继续循环，跳过后面的结果处理过程
                    //continue;
                    return;
                }
            }

            //判断是否有回写配置
            if (InterFaceSet[iSub].IReSet.Count > 0)
            {
                InterFaceReSet.Clear();

                InterFaceReSet.AddRange(InterFaceSet[iSub].IReSet);

                List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return string.IsNullOrEmpty(p.fathersql); });

                List<string> listSql = new List<string>();
                //循环回写配置
                for (int iReSet = 0; iReSet < tmpList.Count; iReSet++)
                {
                    listSql.AddRange(CreateReWriteSql(strResult, tmpList[iReSet]));
                }

                if (listSql.Count > 0)
                {
                    di.RunSql(listSql);
                }

                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText("数据回写成功:" + strSubName + "\r\n");

                    rthText.ScrollToCaret();
                }));

            }
        }
        #endregion "一堆测试按钮

        #region "最最最最最初版整合XML（弃用）"
        private List<string> CreateXmlPara1(DataSet dsXml, DataSet dsValue, string strSql)
        {
            List<string> listXmlPara = new List<string>();
            DAOInterface din = DAOFactory.CreateDAOInterface();

            try
            {
                //20171011 暂定XML格式只存在两层，也就是一次上传主从表结构数据
                //其中可能存在主表数据循环连带明细表数据循环的形式
                //XML结构必有根节点标签  1层标签
                //主表数据如果循环，则必有一层外层标签，2层标签
                //主表数据各节点有标签，3层标签
                //XML主表数据其中有一个标签代表明细表数据，明细表数据如果循环，需要有一层外层标签，4层标签
                //明细表数据每个字段又有一个标签， 5层标签
                //所以XML结构设置里面可能存在5层标签设置

                //先整合第一级标签,而且第一级标签有切只能有一个
                string strSearch = "";

                //使用标签层级为1作为条件，过滤数据，查找到根标签节点标识
                strSearch = "xmltitlelv=1";

                DataRow[] drSelect = dsXml.Tables[0].Select(strSearch, "xmltitlelv");

                string xmlRoot = drSelect[0]["xmltitle"].ToString().Trim();

                StringBuilder sBuilder = new StringBuilder();

                //查找父标签为根标签的所有标签
                strSearch = "partitle=" + xmlRoot;

                for (int iv = 0; iv < dsValue.Tables[0].Rows.Count; iv++)
                {
                    DataRow[] drSelectS = dsXml.Tables[0].Select(strSearch, "xmltitlelv");

                    for (int i = 0; i < drSelectS.Length; i++)
                    {
                        //循环所有根节点下层标签信息

                        //如果标签是值形式，则直接整合标签填写上值内容
                        if (drSelectS[i]["paratype"].ToString().Trim().ToUpper() == "V")
                        {
                            sBuilder.Append("<" + drSelectS[i]["xmltitle"].ToString().Trim() + ">");
                            if (dsValue.Tables[0].Columns.Contains(drSelectS[i]["valtitle"].ToString().Trim()))
                            {
                                sBuilder.Append(dsValue.Tables[0].Rows[iv][drSelectS[i]["valtitle"].ToString().Trim()].ToString().Trim());
                            }
                            sBuilder.Append("</" + drSelectS[i]["xmltitle"].ToString().Trim() + ">");
                        }
                        //否则，该标签下面的值结构应该是XML结构，获取对应以该标签为父标签节点的所有子标签
                        {
                            strSearch = "partitle=" + drSelectS[i]["xmltitle"].ToString().Trim();
                            DataRow[] drSelectT = dsXml.Tables[0].Select(strSearch, "xmltitlelv");

                            //先添加头标签
                            sBuilder.Append("<" + drSelectS[i]["xmltitle"].ToString().Trim() + ">");

                            //在此方法中，XML结构为主表数据多条数据整合一个XML结构
                            //循环子标签
                            for (int m = 0; m < drSelectT.Length; m++)
                            {
                                //如果标签是值形式，则直接整合标签填写上值内容
                                if (drSelectT[m]["paratype"].ToString().Trim().ToUpper() == "V")
                                {
                                    //在此方法中，XML结构为主表数据多条数据整合一个XML结构
                                    //所以该层标签，即第三层标签对应的值，仍然应该为主表数据的值
                                    sBuilder.Append("<" + drSelectT[m]["xmltitle"].ToString().Trim() + ">");

                                    if (dsValue.Tables[0].Columns.Contains(drSelectT[m]["valtitle"].ToString().Trim()))
                                    {
                                        sBuilder.Append(dsValue.Tables[0].Rows[iv][drSelectT[m]["valtitle"].ToString().Trim()].ToString().Trim());
                                    }

                                    sBuilder.Append("</" + drSelectT[m]["xmltitle"].ToString().Trim() + ">");
                                }
                                //否则，该标签下面的值结构应该是XML结构，获取对应以该标签为父标签节点的所有子标签
                                {
                                    sBuilder.Append("<" + drSelectT[m]["xmltitle"].ToString().Trim() + ">");

                                    //进入到第四层标签，首先获取明细表数据
                                    if (!string.IsNullOrEmpty(strSql))
                                    {
                                        //替换SQL语句中的动态值
                                        for (int iCol = 0; iCol < dsValue.Tables[0].Columns.Count; iCol++)
                                        {
                                            if (strSql.IndexOf("$" + dsValue.Tables[0].Columns[iCol].ColumnName.Trim()) > 0)
                                            {
                                                strSql = strSql.Replace("$" + dsValue.Tables[0].Columns[iCol].ColumnName.Trim(), dsValue.Tables[0].Rows[iv][dsValue.Tables[0].Columns[iCol].ColumnName.Trim()].ToString().Trim());
                                            }
                                        }

                                        using (DataSet ds = din.GetDataSetBySql(strSql))
                                        {
                                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                            {
                                                //获取第四层标签
                                                strSearch = "partitle=" + drSelectT[m]["xmltitle"].ToString().Trim();
                                                DataRow[] drSelectFo = dsXml.Tables[0].Select(strSearch, "xmltitlelv");

                                                strSearch = "partitle=" + drSelectFo[0]["xmltitle"].ToString().Trim();
                                                DataRow[] drSelectFi = dsXml.Tables[0].Select(strSearch, "xmltitlelv");

                                                //再仅考虑主表明细表只有两个数据表的情况下
                                                //在后续扩展中，可以考虑将对应层下为XML结构的标签获取数据的语句配置到标签处
                                                //明细数据必然需要循环，第四层标签只会有一个，所以第四层标签获取到之后，不需要再进行循环

                                                for (int iDetail = 0; iDetail < ds.Tables[0].Rows.Count; iDetail++)
                                                {
                                                    sBuilder.Append("<" + drSelectFo[0]["xmltitle"].ToString().Trim() + ">");

                                                    for (int iFi = 0; iFi < drSelectFi.Length; iFi++)
                                                    {
                                                        sBuilder.Append("<" + drSelectFi[iFi]["xmltitle"].ToString().Trim() + ">");

                                                        if (ds.Tables[0].Columns.Contains(drSelectFi[iFi]["valtitle"].ToString().Trim()))
                                                        {
                                                            sBuilder.Append(ds.Tables[0].Rows[iDetail][drSelectFi[iFi]["valtitle"].ToString().Trim()].ToString().Trim());
                                                        }

                                                        sBuilder.Append("<" + drSelectFi[iFi]["xmltitle"].ToString().Trim() + ">");
                                                    }

                                                    sBuilder.Append("</" + drSelectFo[0]["xmltitle"].ToString().Trim() + ">");
                                                }
                                            }
                                        }
                                    }

                                    sBuilder.Append("</" + drSelectT[m]["xmltitle"].ToString().Trim() + ">");
                                }
                            }
                            //添加标签尾
                            sBuilder.Append("</" + drSelectS[i]["xmltitle"].ToString().Trim() + ">");
                        }
                    }
                }

                listXmlPara.Add("<" + xmlRoot + ">" + sBuilder.ToString() + "</" + xmlRoot + ">");
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }

            return listXmlPara;
        }

        private List<string> CreateXmlParaLoop1(DataSet dsXml, DataSet dsValue, string strSql)
        {
            List<string> listXmlPara = new List<string>();
            DAOInterface din = DAOFactory.CreateDAOInterface();

            try
            {
                //20171011 暂定XML格式只存在两层，也就是一次上传主从表结构数据
                //其中可能存在主表数据循环连带明细表数据循环的形式
                //XML结构必有根节点标签  1层标签
                //主表数据如果循环，则必有一层外层标签，2层标签
                //主表数据各节点有标签，3层标签
                //XML主表数据其中有一个标签代表明细表数据，明细表数据如果循环，需要有一层外层标签，4层标签
                //明细表数据每个字段又有一个标签， 5层标签
                //所以XML结构设置里面可能存在5层标签设置

                //先整合第一级标签,而且第一级标签有切只能有一个


                //在此方法中，是标注了主表数据单条循环进行上传，所以先循环主表数据，然后每条主表数据以及对应的明细数据整合成1个XML字符串
                for (int iv = 0; iv < dsValue.Tables[0].Rows.Count; iv++)
                {
                    string strSearch = "";

                    //使用标签层级为1作为条件，过滤数据，查找到根标签节点标识
                    strSearch = "xmltitlelv=1";

                    DataRow[] drSelect = dsXml.Tables[0].Select(strSearch, "xmltitlelv");

                    string xmlRoot = drSelect[0]["xmltitle"].ToString().Trim();

                    StringBuilder sBuilder = new StringBuilder();

                    //查找父标签为根标签的所有标签
                    strSearch = "partitle='" + xmlRoot + "'";


                    DataRow[] drSelectS = dsXml.Tables[0].Select(strSearch, "xmltitlelv");

                    for (int i = 0; i < drSelectS.Length; i++)
                    {
                        //循环所有根节点下层标签信息

                        //如果标签是值形式，则直接整合标签填写上值内容
                        if (drSelectS[i]["paratype"].ToString().Trim().ToUpper() == "V")
                        {
                            sBuilder.Append("<" + drSelectS[i]["xmltitle"].ToString().Trim() + ">");
                            if (dsValue.Tables[0].Columns.Contains(drSelectS[i]["valtitle"].ToString().Trim()))
                            {
                                sBuilder.Append(dsValue.Tables[0].Rows[iv][drSelectS[i]["valtitle"].ToString().Trim()].ToString().Trim());
                            }
                            sBuilder.Append("</" + drSelectS[i]["xmltitle"].ToString().Trim() + ">");
                        }
                        //否则，该标签下面的值结构应该是XML结构，获取对应以该标签为父标签节点的所有子标签
                        else
                        {
                            strSearch = "partitle='" + drSelectS[i]["xmltitle"].ToString().Trim() + "'";
                            DataRow[] drSelectT = dsXml.Tables[0].Select(strSearch, "xmltitlelv");

                            //先添加头标签
                            sBuilder.Append("<" + drSelectS[i]["xmltitle"].ToString().Trim() + ">");

                            //在此方法中，XML结构为主表数据多条数据整合一个XML结构
                            //循环子标签
                            for (int m = 0; m < drSelectT.Length; m++)
                            {
                                //如果标签是值形式，则直接整合标签填写上值内容
                                if (drSelectT[m]["paratype"].ToString().Trim().ToUpper() == "V")
                                {
                                    //在此方法中，XML结构为主表数据多条数据整合一个XML结构
                                    //所以该层标签，即第三层标签对应的值，仍然应该为主表数据的值
                                    sBuilder.Append("<" + drSelectT[m]["xmltitle"].ToString().Trim() + ">");

                                    if (dsValue.Tables[0].Columns.Contains(drSelectT[m]["valtitle"].ToString().Trim()))
                                    {
                                        sBuilder.Append(dsValue.Tables[0].Rows[iv][drSelectT[m]["valtitle"].ToString().Trim()].ToString().Trim());
                                    }

                                    sBuilder.Append("</" + drSelectT[m]["xmltitle"].ToString().Trim() + ">");
                                }
                                //否则，该标签下面的值结构应该是XML结构，获取对应以该标签为父标签节点的所有子标签
                                {
                                    sBuilder.Append("<" + drSelectT[m]["xmltitle"].ToString().Trim() + ">");

                                    //进入到第四层标签，首先获取明细表数据
                                    if (!string.IsNullOrEmpty(strSql))
                                    {
                                        //替换SQL语句中的动态值
                                        for (int iCol = 0; iCol < dsValue.Tables[0].Columns.Count; iCol++)
                                        {
                                            if (strSql.IndexOf("$" + dsValue.Tables[0].Columns[iCol].ColumnName.Trim()) > 0)
                                            {
                                                strSql = strSql.Replace("$" + dsValue.Tables[0].Columns[iCol].ColumnName.Trim(), dsValue.Tables[0].Rows[iv][dsValue.Tables[0].Columns[iCol].ColumnName.Trim()].ToString().Trim());
                                            }
                                        }

                                        using (DataSet ds = din.GetDataSetBySql(strSql))
                                        {
                                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                            {
                                                //获取第四层标签
                                                strSearch = "partitle='" + drSelectT[m]["xmltitle"].ToString().Trim() + "'";
                                                DataRow[] drSelectFo = dsXml.Tables[0].Select(strSearch, "xmltitlelv");

                                                strSearch = "partitle='" + drSelectFo[0]["xmltitle"].ToString().Trim() + "'";
                                                DataRow[] drSelectFi = dsXml.Tables[0].Select(strSearch, "xmltitlelv");

                                                //再仅考虑主表明细表只有两个数据表的情况下
                                                //在后续扩展中，可以考虑将对应层下为XML结构的标签获取数据的语句配置到标签处
                                                //明细数据必然需要循环，第四层标签只会有一个，所以第四层标签获取到之后，不需要再进行循环

                                                for (int iDetail = 0; iDetail < ds.Tables[0].Rows.Count; iDetail++)
                                                {
                                                    sBuilder.Append("<" + drSelectFo[0]["xmltitle"].ToString().Trim() + ">");

                                                    for (int iFi = 0; iFi < drSelectFi.Length; iFi++)
                                                    {
                                                        sBuilder.Append("<" + drSelectFi[iFi]["xmltitle"].ToString().Trim() + ">");

                                                        if (ds.Tables[0].Columns.Contains(drSelectFi[iFi]["valtitle"].ToString().Trim()))
                                                        {
                                                            sBuilder.Append(ds.Tables[0].Rows[iDetail][drSelectFi[iFi]["valtitle"].ToString().Trim()].ToString().Trim());
                                                        }

                                                        sBuilder.Append("<" + drSelectFi[iFi]["xmltitle"].ToString().Trim() + ">");
                                                    }

                                                    sBuilder.Append("</" + drSelectFo[0]["xmltitle"].ToString().Trim() + ">");
                                                }
                                            }
                                        }
                                    }

                                    sBuilder.Append("</" + drSelectT[m]["xmltitle"].ToString().Trim() + ">");
                                }
                            }
                            //添加标签尾
                            sBuilder.Append("</" + drSelectS[i]["xmltitle"].ToString().Trim() + ">");
                        }
                    }

                    listXmlPara.Add("<" + xmlRoot + ">" + sBuilder.ToString() + "</" + xmlRoot + ">");
                }

            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }

            return listXmlPara;
        }
        #endregion "最最最最最初版整合XML（弃用）"

        private void btnDataBase_Click(object sender, EventArgs e)
        {
            frmSetDataLink frm = new frmSetDataLink();

            DialogResult dr = frm.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                //bCanExit = true;

                //ninote.Visible = false;

                //Application.Exit();

                //System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                
                MessageBox.Show("配置完成，请重启软件");
               
            }
        }

        private void chkTest_CheckedChanged(object sender, EventArgs e)
        {
            bTest = chkTest.Checked;
        }

        #region "整合Json"
        private List<string> CreateJsonPara(DataSet dsXml, DataSet dllSet, DataSet dllParaSet)
        {
            //DAOInterface din = DAOFactory.CreateDAOInterface();

            List<string> listXml = new List<string>();

            try
            {
                StringBuilder sBuilder = new StringBuilder();

                string strSearch = "";

                //使用标签层级为1作为条件，过滤数据，第一级标签
                strSearch = "xmltitlelv=1";

                DataRow[] drSelect = dsXml.Tables[0].Select(strSearch, "titlesort");

                if (drSelect.Length > 0)
                {
                    if (drSelect[0]["xmltitle"].ToString().Trim() == "NoIn" && drSelect[0]["injson"].ToString().Trim() != "1")
                    { }
                    else
                    {
                        //json 以{开始，}结束
                        sBuilder.Append("{");
                    }

                    //循环第一级标签
                    for (int i = 0; i < drSelect.Length; i++)
                    {
                        //判断根标签有没有对应的获取数据的SQL语句，如果有，则执行SQL获取数据集
                        if (!string.IsNullOrEmpty(drSelect[i]["getvaluesql"].ToString().Trim()))
                        {
                            string strSql = drSelect[i]["getvaluesql"].ToString().Trim();

                            //整合SQL语句，替换其中的变量符为数据
                            strSql = ConformSql(strSql);

                            clsTableStru cts = new clsTableStru();
                            cts.titleName = drSelect[i]["tablename"].ToString().Trim();
                            cts.iRows = 0;
                            cts.table = dii.GetDataSetBySql(strSql).Tables[0];

                            int iTableHave = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); });

                            if (iTableHave >= 0)
                            {
                                //如果存在，则删除掉该数据表实体
                                //listTable.RemoveAt(iTableHave);
                                listTable[iTableHave] = cts;
                            }
                            else
                            {
                                listTable.Add(cts);
                            }

                            //第一级，任何一个标签对应的SQL语句，如果没有查询到数据，都认为是没有数据需要上传，跳出
                            if (cts.table == null || cts.table.Rows.Count == 0)
                            {
                                return new List<string>();
                            }
                        }
                        //injson
                        //暂定认为，如果json标签直接是值的话，不会循环
                        if (drSelect[i]["paratype"].ToString().Trim().ToUpper() == "V")
                        {
                            //如果标签对应的是值，不包含标签的命令无效化
                            //if (drSelect[i]["xmltitle"].ToString().Trim() == "1")
                            //{
                            if (drSelect[i]["sortitle"] != null && drSelect[i]["sortitle"].ToString().Trim() != "")
                            {
                                string tmpTitle = GetSendValue(drSelect[i]["sortitle"].ToString().Trim(), 0);

                                sBuilder.Append("\"" + tmpTitle + "\":");
                            }
                            else
                            {
                                sBuilder.Append("\"" + drSelect[i]["xmltitle"].ToString().Trim() + "\":");
                            }

                            if (drSelect[i]["isdate"].ToString().Trim() == "1" || drSelect[i]["dateformat"].ToString().Trim() == "" || drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                            {
                                sBuilder.Append("\"");
                            }
                            //sBuilder.Append("\"" + drSelect[i]["xmltitle"].ToString().Trim() + "\":\"");
                            //}

                            string xmlStaticVal = drSelect[i]["xmlstaticval"].ToString().Trim();

                            if (string.IsNullOrEmpty(xmlStaticVal))
                            {
                                //标签下面对应的是数据表值
                                //获取标签需要取值的数据表名
                                string strTableName = drSelect[i]["valtable"].ToString().Trim();

                                //查找数据表的位置
                                int iTableT = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == strTableName; });

                                if (iTableT >= 0)
                                {
                                    //判断设定的取值列名在数据表中是否存在
                                    if (listTable[iTableT].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                    {
                                        //如果设定的取值列名存在，则将值添加至XML中

                                        string tmpColValue = "";

                                        if (listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()] != null)
                                        {
                                            if (drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                            {
                                                tmpColValue = Convert.ToBase64String(((byte[])listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()]));

                                                string strDataLink = drSelect[i]["dateformat"].ToString().Trim();

                                                if (strDataLink.ToLower() != "base64")
                                                {
                                                    strDataLink = strDataLink.Substring(7);

                                                    tmpColValue = strDataLink + tmpColValue;
                                                }
                                            }
                                            else
                                            {
                                                tmpColValue = listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();
                                            }
                                        }
                                        //判断该列是否为日期格式
                                        if (drSelect[i]["isdate"].ToString().Trim() == "1")
                                        {
                                            string strFormat = "yyyy-MM-dd HH:mm:ss";

                                            if (drSelect[i]["dateformat"].ToString().Trim() != "")
                                            {
                                                strFormat = drSelect[i]["dateformat"].ToString().Trim();
                                            }

                                            if (tmpColValue != "")
                                            {
                                                tmpColValue = Convert.ToDateTime(listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()]).ToString(strFormat);
                                                //sbu.Append(listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim());
                                            }
                                        }
                                        else if (drSelect[i]["dateformat"].ToString().Trim() != "" && drSelect[i]["dateformat"].ToString().Trim() != "Num" && tmpColValue != "" && !drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                        {
                                            tmpColValue = string.Format(drSelect[i]["dateformat"].ToString().Trim(), Convert.ToDouble(tmpColValue));
                                        }

                                        if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                        {
                                            tmpColValue = ConvertValueByDll(tmpColValue, "", drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                        }

                                        if (drSelect[i]["canempty"].ToString().Trim() == "1")
                                        {
                                            if (string.IsNullOrEmpty(tmpColValue))
                                            {
                                                throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                            }
                                        }

                                        sBuilder.Append(tmpColValue);

                                    }
                                }
                            }
                            else
                            {
                                if (xmlStaticVal == "####" && !string.IsNullOrEmpty(drSelect[i]["valtable"].ToString().Trim()) && drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                {
                                    string strTableName = drSelect[i]["valtable"].ToString().Trim();

                                    //查找数据表的位置
                                    int iTableT = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTableName; });

                                    if (iTableT >= 0)
                                    {
                                        //判断设定的取值列名在数据表中是否存在
                                        if (listTable[iTableT].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                        {
                                            List<string> sVal = new List<string>();

                                            for (int iRows = 0; iRows < listTable[iTableT].table.Rows.Count; iRows++)
                                            {
                                                string strTmpValue = listTable[iTableT].table.Rows[iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();

                                                sVal.Add(strTmpValue);
                                            }

                                            if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                            {
                                                xmlStaticVal = ConvertValueByDll(sVal, "", drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                            }

                                            sVal.Clear();
                                            sVal = null;
                                        }
                                        else
                                        {
                                            throw new UserException(drSelect[i]["xmltitle"].ToString().Trim() + ":取值字段设置有错误，未找到对应的字段");
                                        }
                                    }
                                    else
                                    {
                                        throw new UserException(drSelect[i]["xmltitle"].ToString().Trim() + ":取值表名设置错误-未找到数据表");
                                    }
                                }
                                else
                                {
                                    if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                    {
                                        xmlStaticVal = ConvertValueByDll(xmlStaticVal, "", drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                    }
                                }

                                if (drSelect[i]["canempty"].ToString().Trim() == "1")
                                {
                                    if (string.IsNullOrEmpty(xmlStaticVal))
                                    {
                                        throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                    }
                                }

                                sBuilder.Append(xmlStaticVal);
                            }

                            if (drSelect[i]["isdate"].ToString().Trim() == "1" || drSelect[i]["dateformat"].ToString().Trim() == "" || drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                            {
                                sBuilder.Append("\"");
                            }
                        }
                        else
                        {
                            if (drSelect[i]["injson"].ToString().Trim() == "1")
                            {
                                if (drSelect[i]["sortitle"] != null && drSelect[i]["sortitle"].ToString().Trim() != "")
                                {
                                    string tmpTitle = GetSendValue(drSelect[i]["sortitle"].ToString().Trim(), 0);

                                    sBuilder.Append("\"" + tmpTitle + "\":");
                                }
                                else
                                {
                                    sBuilder.Append("\"" + drSelect[i]["xmltitle"].ToString().Trim() + "\":");
                                }
                                //sBuilder.Append("\"" + drSelect[i]["xmltitle"].ToString().Trim() + "\":");
                            }

                            string xmlStaticVal = drSelect[i]["xmlstaticval"].ToString().Trim();

                            if (!string.IsNullOrEmpty(xmlStaticVal))
                            {
                                sBuilder.Append("'");
                            }

                            if (drSelect[i]["titleisloop"].ToString().Trim() == "1")
                            {
                                sBuilder.Append("[");

                                string strSearchK = "partitle='" + drSelect[i]["xmltitle"].ToString().Trim() + "'";

                                DataRow[] drSelectK = dsXml.Tables[0].Select(strSearchK, "titlesort");

                                string strTableName = drSelect[i]["valtable"].ToString().Trim();

                                int iTable = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == drSelect[i]["looptable"].ToString().Trim(); });

                                if (iTable >= 0)
                                {
                                    //对数据进行循环
                                    for (int m = 0; m < listTable[iTable].table.Rows.Count; m++)
                                    {
                                        listTable[iTable].iRows = m;

                                        if (drSelectK.Length <= 0 && !string.IsNullOrEmpty(strTableName) && strTableName == drSelect[i]["looptable"].ToString().Trim())
                                        {
                                            if (drSelect[i]["isdate"].ToString().Trim() == "1" || drSelect[i]["dateformat"].ToString().Trim() == "" || drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                            {
                                                sBuilder.Append("\"");
                                            }
                                            //判断设定的取值列名在数据表中是否存在
                                            if (listTable[iTable].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                            {
                                                //如果设定的取值列名存在，则将值添加至XML中
                                                string tmpColValue = "";

                                                if (listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()] != null)
                                                {
                                                    //tmpColValue = listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim();
                                                    if (drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                                    {
                                                        tmpColValue = Convert.ToBase64String(((byte[])listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()]));

                                                        string strDataLink = drSelect[i]["dateformat"].ToString().Trim();

                                                        if (strDataLink.ToLower() != "base64")
                                                        {
                                                            strDataLink = strDataLink.Substring(7);

                                                            tmpColValue = strDataLink + tmpColValue;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        tmpColValue = listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();
                                                    }
                                                    
                                                }
                                                //判断该列是否为日期格式
                                                if (drSelect[i]["isdate"].ToString().Trim() == "1")
                                                {
                                                    string strFormat = "yyyy-MM-dd HH:mm:ss";

                                                    if (drSelect[i]["dateformat"].ToString().Trim() != "")
                                                    {
                                                        strFormat = drSelect[i]["dateformat"].ToString().Trim();
                                                    }

                                                    if (tmpColValue != "")
                                                    {
                                                        tmpColValue = Convert.ToDateTime(listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()]).ToString(strFormat);
                                                        //sbu.Append(listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim());
                                                    }
                                                }
                                                else if (drSelect[i]["dateformat"].ToString().Trim() != "" && drSelect[i]["dateformat"].ToString().Trim() != "Num" && tmpColValue != "" && !drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                                {
                                                    tmpColValue = string.Format(drSelect[i]["dateformat"].ToString().Trim(), Convert.ToDouble(tmpColValue));
                                                }

                                                if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                                {
                                                    tmpColValue = ConvertValueByDll(tmpColValue, "", drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                                }

                                                if (drSelect[i]["canempty"].ToString().Trim() == "1")
                                                {
                                                    if (string.IsNullOrEmpty(xmlStaticVal))
                                                    {
                                                        throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                                    }
                                                }

                                                sBuilder.Append(tmpColValue);

                                                if (drSelect[i]["isdate"].ToString().Trim() == "1" || drSelect[i]["dateformat"].ToString().Trim() == "" || drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                                {
                                                    sBuilder.Append("\"");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            sBuilder.Append(CreateJson(dsXml, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet));
                                            
                                        }

                                        if (m < listTable[iTable].table.Rows.Count - 1)
                                        {
                                            sBuilder.Append(",");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                sBuilder.Append(CreateJson(dsXml, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet));
                            }

                            if (drSelect[i]["titleisloop"].ToString().Trim() == "1")
                            {
                                sBuilder.Append("]");
                            }

                            if (!string.IsNullOrEmpty(xmlStaticVal))
                            {
                                sBuilder.Append("'");
                            }
                        }

                        if (i < drSelect.Length - 1)
                        {
                            sBuilder.Append(",");
                        }
                    }

                    if (drSelect[0]["xmltitle"].ToString().Trim() == "NoIn" && drSelect[0]["injson"].ToString().Trim() != "1")
                    { }
                    else
                    {
                        //json 以{开始，}结束
                        sBuilder.Append("}");
                    }
                    

                    listXml.Add(sBuilder.ToString());
                }

                return listXml;
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }

        private StringBuilder CreateJson(DataSet dsXml, string parentTitle, DataSet dllSet, DataSet dllParaSet)
        {
            StringBuilder sbu = new StringBuilder();
            
            //DAOInterface din = DAOFactory.CreateDAOInterface();

            try
            {
                sbu.Append("{");
                //使用父标签以及XML设置结构，获取父标签下的所有子标签信息
                string strSearch = "partitle='" + parentTitle + "'";

                DataRow[] drSelect = dsXml.Tables[0].Select(strSearch, "titlesort");

                for (int i = 0; i < drSelect.Length; i++)
                {
                    if (!string.IsNullOrEmpty(drSelect[i]["getvaluesql"].ToString().Trim()))
                    {
                        //如果标签对应的获取数据的SQL语句不为空，则执行语句获取数据集并将数据表添加至集合中

                        //取得标签对应的SQL语句
                        string strSql = drSelect[i]["getvaluesql"].ToString().Trim();

                        //整合SQL语句，替换其中的变量符为数据
                        strSql = ConformSql(strSql);

                        //实例化数据表实体类
                        clsTableStru cts = new clsTableStru();

                        //给数据表对应的标签进行赋值,使用配置的表别名
                        cts.titleName = drSelect[i]["tablename"].ToString().Trim();

                        //初始化该数据表循环到的行的值为0
                        cts.iRows = 0;

                        //执行SQL语句获取数据，并且将数据赋值给数据表
                        cts.table = dii.GetDataSetBySql(strSql).Tables[0];

                        //查找在数据表集合中，是否存在相同标签的数据表
                        int iTableHave = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); });

                        if (iTableHave >= 0)
                        {
                            //如果存在，则删除掉该数据表实体
                            //listTable.RemoveAt(iTableHave);
                            listTable[iTableHave] = cts;
                        }
                        else
                        {
                            //将数据表实体插入到数据集合中
                            listTable.Add(cts);
                        }
                    }

                    //暂定认为，如果json标签直接是值的话，不会循环
                    if (drSelect[i]["paratype"].ToString().Trim().ToUpper() == "V")
                    {
                        //如果标签对应的是值，不包含标签的命令无效化
                        //if (drSelect[i]["xmltitle"].ToString().Trim() == "1")
                        //{
                        //sbu.Append("\"" + drSelect[i]["xmltitle"].ToString().Trim() + "\":\"");
                        //}
                        if (drSelect[i]["sortitle"] != null && drSelect[i]["sortitle"].ToString().Trim() != "")
                        {
                            string tmpTitle = GetSendValue(drSelect[i]["sortitle"].ToString().Trim(), 0);

                            sbu.Append("\"" + tmpTitle + "\":");
                        }
                        else
                        {
                            sbu.Append("\"" + drSelect[i]["xmltitle"].ToString().Trim() + "\":");
                        }

                        if (drSelect[i]["isdate"].ToString().Trim() == "1" || drSelect[i]["dateformat"].ToString().Trim() == "" || drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                        {
                            sbu.Append("\"");
                        }

                        string xmlStaticVal = drSelect[i]["xmlstaticval"].ToString().Trim();

                        if (string.IsNullOrEmpty(xmlStaticVal))
                        {
                            //标签下面对应的是数据表值
                            //获取标签需要取值的数据表名
                            string strTableName = drSelect[i]["valtable"].ToString().Trim();

                            //查找数据表的位置
                            int iTableT = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == strTableName; });

                            if (iTableT >= 0)
                            {
                                //判断设定的取值列名在数据表中是否存在
                                if (listTable[iTableT].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                {
                                    //如果设定的取值列名存在，则将值添加至XML中

                                    string tmpColValue = "";

                                    if (listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()] != null)
                                    {
                                        //tmpColValue = listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim();
                                        if (drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                        {
                                            tmpColValue = Convert.ToBase64String(((byte[])listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()]));

                                            string strDataLink = drSelect[i]["dateformat"].ToString().Trim();

                                            if (strDataLink.ToLower() != "base64")
                                            {
                                                strDataLink = strDataLink.Substring(7);

                                                tmpColValue = strDataLink + tmpColValue;
                                            }
                                        }
                                        else
                                        {
                                            tmpColValue = listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();
                                        }
                                        //tmpColValue = listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();
                                    }
                                    //判断该列是否为日期格式
                                    if (drSelect[i]["isdate"].ToString().Trim() == "1")
                                    {
                                        string strFormat = "yyyy-MM-dd HH:mm:ss";

                                        if (drSelect[i]["dateformat"].ToString().Trim() != "")
                                        {
                                            strFormat = drSelect[i]["dateformat"].ToString().Trim();
                                        }

                                        if (tmpColValue != "")
                                        {
                                            tmpColValue = Convert.ToDateTime(listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()]).ToString(strFormat);
                                            //sbu.Append(listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim());
                                        }
                                    }
                                    else if (drSelect[i]["dateformat"].ToString().Trim() != "" && drSelect[i]["dateformat"].ToString().Trim() != "Num" && tmpColValue != "" && !drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                    {
                                        tmpColValue = string.Format(drSelect[i]["dateformat"].ToString().Trim(), Convert.ToDouble(tmpColValue));
                                    }

                                    if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                    {
                                        tmpColValue = ConvertValueByDll(tmpColValue, parentTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                    }

                                    if (drSelect[i]["canempty"].ToString().Trim() == "1")
                                    {
                                        if (string.IsNullOrEmpty(xmlStaticVal))
                                        {
                                            throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                        }
                                    }

                                    sbu.Append(tmpColValue);

                                }
                            }
                        }
                        else
                        {
                            if (xmlStaticVal == "####" && !string.IsNullOrEmpty(drSelect[i]["valtable"].ToString().Trim()) && drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                            {
                                string strTableName = drSelect[i]["valtable"].ToString().Trim();

                                //查找数据表的位置
                                int iTableT = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTableName; });

                                if (iTableT >= 0)
                                {
                                    //判断设定的取值列名在数据表中是否存在
                                    if (listTable[iTableT].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                    {
                                        List<string> sVal = new List<string>();

                                        for (int iRows = 0; iRows < listTable[iTableT].table.Rows.Count; iRows++)
                                        {
                                            string strTmpValue = listTable[iTableT].table.Rows[iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();

                                            sVal.Add(strTmpValue);
                                        }

                                        if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                        {
                                            xmlStaticVal = ConvertValueByDll(sVal, parentTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                        }
                                    }
                                    else
                                    {
                                        throw new UserException(drSelect[i]["xmltitle"].ToString().Trim() + ":取值字段设置有错误，未找到对应的字段");
                                    }
                                }
                                else
                                {
                                    throw new UserException(drSelect[i]["xmltitle"].ToString().Trim() + ":取值表名设置错误-未找到数据表");
                                }
                            }
                            else
                            {
                                if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                {
                                    xmlStaticVal = ConvertValueByDll(xmlStaticVal, parentTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                }
                            }

                            if (drSelect[i]["canempty"].ToString().Trim() == "1")
                            {
                                if (string.IsNullOrEmpty(xmlStaticVal))
                                {
                                    throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                }
                            }

                            sbu.Append(xmlStaticVal);
                        }

                        if (drSelect[i]["isdate"].ToString().Trim() == "1" || drSelect[i]["dateformat"].ToString().Trim() == "" || drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                        {
                            sbu.Append("\"");
                        }
                    }
                    else
                    {
                        if (drSelect[i]["injson"].ToString().Trim() == "1")
                        {
                            if (drSelect[i]["sortitle"] != null && drSelect[i]["sortitle"].ToString().Trim() != "")
                            {
                                string tmpTitle = GetSendValue(drSelect[i]["sortitle"].ToString().Trim(), 0);

                                sbu.Append("\"" + tmpTitle + "\":");
                            }
                            else
                            {
                                sbu.Append("\"" + drSelect[i]["xmltitle"].ToString().Trim() + "\":");
                            }
                            //sbu.Append("\"" + drSelect[i]["xmltitle"].ToString().Trim() + "\":");
                        }

                        string xmlStaticVal = drSelect[i]["xmlstaticval"].ToString().Trim();

                        if (!string.IsNullOrEmpty(xmlStaticVal))
                        {
                            sbu.Append("'");
                        }

                        if (drSelect[i]["titleisloop"].ToString().Trim() == "1")
                        {
                            sbu.Append("[");

                            string strSearchK = "partitle='" + drSelect[i]["xmltitle"].ToString().Trim() + "'";

                            DataRow[] drSelectK = dsXml.Tables[0].Select(strSearchK, "titlesort");

                            string strTableName = drSelect[i]["valtable"].ToString().Trim();

                            int iTable = listTable.FindIndex(delegate(clsTableStru p) { return p.titleName == drSelect[i]["looptable"].ToString().Trim(); });

                            if (iTable >= 0)
                            {
                                //对数据进行循环
                                for (int m = 0; m < listTable[iTable].table.Rows.Count; m++)
                                {
                                    listTable[iTable].iRows = m;

                                    if (drSelectK.Length <= 0 && !string.IsNullOrEmpty(strTableName) && strTableName == drSelect[i]["looptable"].ToString().Trim())
                                    {
                                        if (drSelect[i]["isdate"].ToString().Trim() == "1" || drSelect[i]["dateformat"].ToString().Trim() == "" || drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                        {
                                            sbu.Append("\"");
                                        }

                                        //判断设定的取值列名在数据表中是否存在
                                        if (listTable[iTable].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                        {
                                            //如果设定的取值列名存在，则将值添加至XML中
                                            string tmpColValue = "";

                                            if (listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()] != null)
                                            {
                                                //tmpColValue = listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim();
                                                if (drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                                {
                                                    tmpColValue = Convert.ToBase64String(((byte[])listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()]));

                                                    string strDataLink = drSelect[i]["dateformat"].ToString().Trim();

                                                    if (strDataLink.ToLower() != "base64")
                                                    {
                                                        strDataLink = strDataLink.Substring(7);

                                                        tmpColValue = strDataLink + tmpColValue;
                                                    }
                                                }
                                                else
                                                {
                                                    tmpColValue = listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();
                                                }
                                                
                                            }
                                            //判断该列是否为日期格式
                                            if (drSelect[i]["isdate"].ToString().Trim() == "1")
                                            {
                                                string strFormat = "yyyy-MM-dd HH:mm:ss";

                                                if (drSelect[i]["dateformat"].ToString().Trim() != "")
                                                {
                                                    strFormat = drSelect[i]["dateformat"].ToString().Trim();
                                                }

                                                if (tmpColValue != "")
                                                {
                                                    tmpColValue = Convert.ToDateTime(listTable[iTable].table.Rows[listTable[iTable].iRows][drSelect[i]["valtitle"].ToString().Trim()]).ToString(strFormat);
                                                    //sbu.Append(listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString().Trim());
                                                }
                                            }
                                            else if (drSelect[i]["dateformat"].ToString().Trim() != "" && drSelect[i]["dateformat"].ToString().Trim() != "Num" && tmpColValue != "" && !drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                            {
                                                tmpColValue = string.Format(drSelect[i]["dateformat"].ToString().Trim(), Convert.ToDouble(tmpColValue));
                                            }

                                            if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                            {
                                                tmpColValue = ConvertValueByDll(tmpColValue, parentTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
                                            }

                                            if (drSelect[i]["canempty"].ToString().Trim() == "1")
                                            {
                                                if (string.IsNullOrEmpty(xmlStaticVal))
                                                {
                                                    throw new Exception(drSelect[i]["xmltitle"].ToString().Trim() + ":不能为空");
                                                }
                                            }

                                            sbu.Append(tmpColValue);

                                            if (drSelect[i]["isdate"].ToString().Trim() == "1" || drSelect[i]["dateformat"].ToString().Trim() == "" || drSelect[i]["dateformat"].ToString().ToLower().Trim().Contains("base64"))
                                            {
                                                sbu.Append("\"");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        sbu.Append(CreateJson(dsXml, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet));
                                        
                                    }

                                    if (m < listTable[iTable].table.Rows.Count - 1)
                                    {
                                        sbu.Append(",");
                                    }

                                }
                            }
                        }
                        else
                        {
                            sbu.Append(CreateJson(dsXml, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet));
                        }

                        if (drSelect[i]["titleisloop"].ToString().Trim() == "1")
                        {
                            sbu.Append("]");
                        }

                        if (!string.IsNullOrEmpty(xmlStaticVal))
                        {
                            sbu.Append("'");
                        }
                    }

                    if (i < drSelect.Length - 1)
                    {
                        sbu.Append(",");
                    }
                }

                sbu.Append("}");
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }

            return sbu;
        }
        #endregion "整合Json"

        #region "改善响应"
        private System.DateTime dLatesAction = DateTime.Now;

        /// <summary>
        /// 改善相应
        /// </summary>
        private void CallDoEvents()
        {
            TimeSpan tSpan = DateTime.Now.Subtract(dLatesAction);
            if (tSpan.TotalSeconds >= 0.2)
            {
                Application.DoEvents();
                dLatesAction = DateTime.Now;
            }
        }
        #endregion

        #region "获取软件版本信息"
        /// <summary>
        /// 获取版本信息
        /// </summary>
        public string AssemblyVersion
        {
            get
            {
                string ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                return ver.Substring(0, ver.LastIndexOf('.'));
            }
        }
        #endregion

        private T DESerializer<T>(string strXML) where T : class
        {
            try
            {
                using (StringReader sr = new StringReader(strXML))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(sr) as T;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            frmAbout frm = new frmAbout();

            frm.ShowDialog();
        }

        //读取数据库，触发接口表信息
        string strSql = "SELECT distinct subname,chiname,MIN(submittime) FROM temp_autointer_trigger_sub WHERE status=0 GROUP BY subname,chiname ORDER BY MIN(submittime)";

        private void RunTriggerSub()
        {
            while (bTriggerRun)
            {
                try
                {
                    if ((!bTriggerNow && (iTriggerCount >= iTriggerWait)) || bTriggerFirst)
                    {
                        bTriggerNow = true;
                        bTriggerFirst = false;
                        iTriggerCount = 0;
                        
                        using (DataSet ds = dii.GetDataSetBySql(strSql))
                        {
                            //自动运行在间歇期，没有在手动执行接口，并且有需要触发运行的接口
                            if (!bRuNow && !bSingle && ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                btnRun.Invoke(new EventHandler(delegate
                                {
                                    if (btnRun.Text.ToString() == "停  止")
                                    {
                                        timLoop.Enabled = false;
                                    }
                                }));

                                btnRunSelectSub.Invoke(new EventHandler(delegate
                                {
                                    btnRunSelectSub.Enabled = false;
                                }));

                                //如果有需要触发运行的上传项目
                                rthText.Invoke(new EventHandler(delegate
                                {
                                    rthText.Text = "";
                                    rthText.AppendText("\r\n触发接口执行\r\n");
                                }));

                                //接口列表全部反选显示为未被选中
                                for (int i = 0; i < lvSub.Items.Count; i++)
                                {
                                    lvSub.Invoke(new EventHandler(delegate
                                    {
                                        lvSub.Items[i].BackColor = Color.White;
                                    }));
                                }

                                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                {

                                    lvSub.Invoke(new EventHandler(delegate
                                    {
                                        string strSubName = ds.Tables[0].Rows[i]["subname"].ToString().Trim();//lvSub.Items[il].SubItems[1].Text.ToString().Trim();

                                        string subChiName = ds.Tables[0].Rows[i]["chiname"].ToString().Trim();

                                        int iSub = InterFaceSet.FindIndex(delegate (clsInterfaceSet p) { return p.SubName == strSubName && p.ChiName == subChiName; });

                                        if (iSub >= 0)
                                        {
                                            lvSub.Items[iSub].BackColor = Color.LightBlue;

                                            lvSub.Items[iSub].Selected = true;
                                        }
                                        else
                                        {
                                            dii.ReWiTempTable(strSubName, subChiName, "2");
                                        }
                                    }));
                                }

                                lvSub.Invoke(new EventHandler(delegate
                                {
                                    for (int il = 0; il < lvSub.SelectedItems.Count; il++)
                                    {
                                        string strSubName = lvSub.SelectedItems[il].SubItems[1].Text.ToString().Trim();//lvSub.Items[il].SubItems[1].Text.ToString().Trim();

                                        string subChiName = lvSub.SelectedItems[il].SubItems[0].Text.ToString().Trim();

                                        int iSub = InterFaceSet.FindIndex(delegate (clsInterfaceSet p) { return p.SubName == strSubName && p.ChiName == subChiName; });

                                        string subType = InterFaceSet[iSub].SubType;

                                        if (string.IsNullOrEmpty(subType) || subType == "webservice")
                                        {
                                                //WebService接口执行
                                                RunWebServiceSub(strSubName, subChiName, iSub, false, false);
                                        }
                                        else
                                        {
                                            RunHttpSub(strSubName, subChiName, iSub, false, false);
                                        }

                                        if (bTriggerResult)
                                        {
                                            dii.ReWiTempTable(strSubName, subChiName, "1");
                                        }

                                        lvSub.Items[iSub].BackColor = Color.White;

                                        lvSub.Items[iSub].Selected = false;

                                            //strSql = "update temp_autointer_trigger_sub set status=1,comptime= WHERE status=0 GROUP BY subname,chiname ORDER BY MIN(submittime)";
                                        }
                                }));

                                btnRunSelectSub.Invoke(new EventHandler(delegate
                                {
                                    btnRunSelectSub.Enabled = true;
                                }));
                            }
                        }
                        
                        btnRun.Invoke(new EventHandler(delegate
                        {
                            if (btnRun.Text.ToString() == "停  止")
                            {
                                timLoop.Enabled = true;
                            }
                        }));

                        bTriggerNow = false;
                    }
                }
                catch (Exception ex)
                {
                    rthText.Invoke(new EventHandler(delegate
                    {
                        rthText.AppendText(ex.Message);

                        rthText.ScrollToCaret();
                    }));

                    btnRun.Invoke(new EventHandler(delegate
                    {
                        if (btnRun.Text.ToString() == "停  止")
                        {
                            timLoop.Enabled = true;
                        }
                    }));

                    bTriggerNow = false;
                    iTriggerCount = 0;
                }

                if (!bTriggerRun)
                {
                    threadTrigger.Abort();
                    bCanExit = true;
                    Application.Exit();
                }
            }
        }

        private void timTrigger_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!bTriggerNow && bComplete)
                {
                    //计数器+1
                    iTriggerCount = iTriggerCount + 1;
                }

            }
            catch (Exception ex)
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(ex.Message);

                    rthText.ScrollToCaret();
                }));
            }
        }

        int iReRunWait = 0;

        bool bAutoExit = true;
        private void timReRun_Tick(object sender, EventArgs e)
        {
            iReRunWait++;

            if (iReRunWait > iReRunTime * 60 && bAutoExit)
            {
                btnRun.Invoke(new EventHandler(delegate
                {
                    bTriggerRun = false;

                    string strButtonText = btnRun.Text.ToString().Trim();

                    if (strButtonText == "停  止")
                    {
                        btnRun.PerformClick();
                    }
                    else if (strButtonText == "点击停止")//
                    {
                        btnRun.PerformClick();
                    }
                    else if (strButtonText == "开  始")
                    {
                        bAutoExit = false;

                        bCanExit = true;

                        Application.Exit();

                        System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    }

                }));

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //string strIp = txtIP.Text.ToString().Trim();

            //string strPort = txtPort.Text.ToString().Trim();

            //string strUser = txtAgentUser.Text.ToString().Trim();

            //string strPass = txtAgentPass.Text.ToString().Trim();

            //string strUrl = strIp;

            //if (string.IsNullOrEmpty(strIp))
            //{
            //    //MessageBox.Show("请输入代理地址");
            //    return;
            //}

            //if (!string.IsNullOrEmpty(strPort))
            //{
            //    strUrl = strUrl + ":" + strPort;
            //}

            //if (SetUserProxy(strUrl,strUser,strPass))
            //{
            //    lblAgent.Text = "代理设置成功";
            //}
            //else
            //{
            //    lblAgent.Text = "代理设置失败";
            //}

            //if (Proxy.SetIEProxy(strUrl))
            //{
            //    lblAgent.Text = "代理设置成功";
            //}
            //else
            //{
            //    lblAgent.Text = "代理设置失败";
            //}
        }

        private void chkAutoRun_CheckedChanged(object sender, EventArgs e)
        {

        }

        //软件加载时，遍历所有开启的接口，将webservice接口先反射出来
        //数据库操作类，在软件加载完成后，直接实例化
        //所有用到DataSet的地方，都使用using或则使用后销毁
    }
}
