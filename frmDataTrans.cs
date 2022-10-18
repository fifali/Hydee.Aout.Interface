using Hydee.Aout.Interface.DAO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Hydee.Aout.Interface
{
    public partial class frmDataTrans : Form
    {
        public string strSubName = "";
        public string subChiName = "";
        public int iSub = 0;
        public bool bSelf = false;
        public bool bPre = false;

        public List<clsInterfaceSet> InterFaceSet = new List<clsInterfaceSet>();

        public List<clsInterfaceReSet> InterFaceReSet = new List<clsInterfaceReSet>();

        public bool bTest = false;

        public string softvar = "";

        List<clsTableStru> listTable = new List<clsTableStru>();

        List<clsXmlStu> listXmlNode = new List<clsXmlStu>();

        DAOInterface dii = DAOFactory.CreateDAOInterface();
        /*
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
        */

        public frmDataTrans()
        {
            InitializeComponent();
        }

        public void DataTrans()
        {
            try
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":开始调用接口" + "\r\n");

                    rthText.ScrollToCaret();

                    CallDoEvents();
                }));

                
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
                        List<string[]> listArgs = new List<string[]>();
                        List<string> listXml = new List<string>();
                        listHeader.Clear();
                        tmp_url_para = "";
                        tmp_url = "";

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

                                            return;
                                        }

                                        tmp_parm_l = listXml[0];
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

                                        tmp_parm_l = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + listXml[0];
                                    }
                                }
                                args[i] = tmp_parm_l;

                                if (bTest)
                                {
                                    TraceHelper TraceHelper = new TraceHelper();
                                    TraceHelper.WriteLine(subChiName + ":发送的报文:\r\n" + tmp_parm_l);
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

                        //for (int i = 0; i < iInParaNum; i++)
                        //{
                        //    if (InterFaceSet[iSub].IPara[i].dataDllDetail == "1")
                        //    {
                        //        object[] ob = new object[InterFaceSet[iSub].IPara[i].dllSet.paraNum];

                        //        for (int iPa = 0; iPa < InterFaceSet[iSub].IPara[i].dllSet.paraNum; iPa++)
                        //        {
                        //            ob[iPa] = args[i];

                        //            if (!string.IsNullOrEmpty(InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue))
                        //            {
                        //                if (InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue.Contains("#H#"))
                        //                {
                        //                    string strTi = InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue.Replace("#H#", "");

                        //                    int iDot = strTi.IndexOf(".");
                        //                    //末尾标签的位置
                        //                    int iEnd = strTi.Length - 4;

                        //                    //提取从哪个数据表中取数据进行替换
                        //                    string strTab = strTi.Substring(3, iDot - 3);

                        //                    int iTab = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTab; });
                        //                    //提取对应数据表中的列名
                        //                    string strDataTitle = strTi.Substring(iDot + 1, iEnd - iDot);

                        //                    //如果数据表集合的长度符合指定的长度
                        //                    if (iTab >= 0)
                        //                    {
                        //                        //根据数据表、对应的行、列名对数据进行替换，整合成真正需要执行的SQL语句
                        //                        ob[iPa] = listTable[iTab].table.Rows[listTable[iTab].iRows][strDataTitle].ToString().Trim();
                        //                    }
                        //                }
                        //                else if (InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue.Contains("#args#"))
                        //                {
                        //                    if (InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue.Contains("->"))
                        //                    {
                        //                        string tNode = InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue;
                        //                        int iLast = Convert.ToInt32(tNode.Substring(tNode.LastIndexOf(":") + 1));

                        //                        int iM = tNode.IndexOf("->");
                        //                        int iN = tNode.IndexOf(":");
                        //                        int iBegin = Convert.ToInt32(tNode.Substring(iN + 1, iM - iN - 1));

                        //                        object[,] obb = new object[iLast - iBegin + 1, 2];

                        //                        for (int i_dll = iBegin; i_dll <= iLast; i_dll++)
                        //                        {
                        //                            obb[i_dll - iBegin, 0] = InterFaceSet[iSub].IPara[i_dll].ParaName;
                        //                            obb[i_dll - iBegin, 1] = args[i_dll];
                        //                        }

                        //                        ob[iPa] = obb;
                        //                    }
                        //                    else
                        //                    {
                        //                        string tNode = InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue;
                        //                        int iArgs = Convert.ToInt32(tNode.Substring(tNode.LastIndexOf(":") + 1));
                        //                        ob[iPa] = args[iArgs];
                        //                    }
                        //                }
                        //                else
                        //                {
                        //                    ob[iPa] = InterFaceSet[iSub].IPara[i].dllSet.dllPara[iPa].paravalue;
                        //                }
                        //            }
                        //            //ob[iPa] = tmp_value;
                        //        }

                        //        args[i] = Invoke_Dll(InterFaceSet[iSub].IPara[i].dllSet.dllName, InterFaceSet[iSub].IPara[i].dllSet.nameSpace, InterFaceSet[iSub].IPara[i].dllSet.className, InterFaceSet[iSub].IPara[i].dllSet.procName, ob);

                        //        ob = null;
                        //    }
                        //}

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
                                else
                                {
                                    param = (string)args[i];
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

                        if (bTest)
                        {
                            if (!string.IsNullOrEmpty(param))
                            {
                                TraceHelper TraceHelper = new TraceHelper();
                                TraceHelper.WriteLine(subChiName + ":发送的报文:\r\n" + param);
                            }
                        }

                        if (InterFaceSet[iSub].SubType == "http-get")
                        {
                            strResult = HttpGet(strAdd_tmp, contentType, encodtype, listHeader);
                        }
                        else if (InterFaceSet[iSub].SubType == "http-post")
                        {
                            strResult = HttpPost(strAdd_tmp, contentType, encodtype, param, listHeader);
                        }
                        else if (InterFaceSet[iSub].SubType == "webclient-post")  //webclient-post   webclient-get
                        {
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

                        //if (!string.IsNullOrEmpty(strResult))
                        //{
                        //    if (bTest)
                        //    {
                        //        TraceHelper.WriteLine(subChiName + ":接收到的数据:\r\n" + strResult);
                        //    }

                        //    if (InterFaceSet[iSub].recDataDatil == "1")
                        //    {
                        //        object[] ob = new object[InterFaceSet[iSub].dllSet.paraNum];

                        //        for (int iPa = 0; iPa < InterFaceSet[iSub].dllSet.paraNum; iPa++)
                        //        {
                        //            string tmp_value = strResult;

                        //            if (!string.IsNullOrEmpty(InterFaceSet[iSub].dllSet.dllPara[iPa].paravalue))
                        //            {
                        //                tmp_value = InterFaceSet[iSub].dllSet.dllPara[iPa].paravalue;
                        //            }

                        //            ob[iPa] = tmp_value;
                        //        }

                        //        strResult = (string)Invoke_Dll(InterFaceSet[iSub].dllSet.dllName, InterFaceSet[iSub].dllSet.nameSpace, InterFaceSet[iSub].dllSet.className, InterFaceSet[iSub].dllSet.procName, ob);

                        //        ob = null;
                        //    }

                        //    //JSON添加内容不为空，则认定返回的是json格式
                        //    //if (!string.IsNullOrEmpty(jsonAdd))
                        //    //{
                        //    //    //将json添加内容中的#HH#替换为接收到的数据，组合成可转换为XML的数据
                        //    //    strResult = jsonAdd.Replace("#HH#", strResult);

                        //    //    //Json转换为XML
                        //    //    strResult = JsonConvert.DeserializeXmlNode(strResult).OuterXml;
                        //    //}


                        //    //判断是否判断返回结果里面的成功标识
                        //    //if (InterFaceSet[iSub].chkReSign == "1")
                        //    //{
                        //    //    if (!JudRecSign(iSub, strResult, args, "sub"))
                        //    //    {
                        //    //        if (!string.IsNullOrEmpty(InterFaceSet[iSub].reErrorNode))
                        //    //        {
                        //    //            string strMessage = "";

                        //    //            strMessage = XmlHelper.GetValue(strResult, InterFaceSet[iSub].reErrorNode);

                        //    //            rthText.Invoke(new EventHandler(delegate
                        //    //            {
                        //    //                rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":返回结果--成功验证（失败）:" + strMessage + "\r\n");

                        //    //                rthText.ScrollToCaret();

                        //    //                CallDoEvents();
                        //    //            }));

                        //    //            //失败
                        //    //            TraceHelper.WriteLine(strMessage);
                        //    //        }
                        //    //        else
                        //    //        {
                        //    //            rthText.Invoke(new EventHandler(delegate
                        //    //            {
                        //    //                rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":返回结果--成功验证（失败）,不进行回写处理" + "\r\n");

                        //    //                rthText.ScrollToCaret();

                        //    //                CallDoEvents();
                        //    //            }));
                        //    //        }

                        //    //        //继续循环，跳过后面的结果处理过程
                        //    //        if (!bPre)
                        //    //        {
                        //    //            continue;
                        //    //        }
                        //    //        else
                        //    //        {
                        //    //            return;
                        //    //        }
                        //    //    }
                        //    //}

                        //    //判断是否有回写配置
                        //    //if (InterFaceSet[iSub].IReSet.Count > 0)
                        //    //{
                        //    //    InterFaceReSet.Clear();

                        //    //    InterFaceReSet.AddRange(InterFaceSet[iSub].IReSet);

                        //    //    List<clsInterfaceReSet> tmpList = InterFaceReSet.FindAll(delegate(clsInterfaceReSet p) { return string.IsNullOrEmpty(p.fathersql); });

                        //    //    List<string> listSql = new List<string>();

                        //    //    rthText.Invoke(new EventHandler(delegate
                        //    //    {
                        //    //        rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":开始整合回写SQL语句" + "\r\n");

                        //    //        rthText.ScrollToCaret();

                        //    //        CallDoEvents();
                        //    //    }));

                        //    //    //循环回写配置
                        //    //    for (int iReSet = 0; iReSet < tmpList.Count; iReSet++)
                        //    //    {
                        //    //        listSql.AddRange(CreateReWriteSql(strResult, tmpList[iReSet]));
                        //    //    }

                        //    //    rthText.Invoke(new EventHandler(delegate
                        //    //    {
                        //    //        rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":回写语句整合完成，开始执行SQL语句" + "\r\n");

                        //    //        rthText.ScrollToCaret();

                        //    //        CallDoEvents();
                        //    //    }));

                        //    //    if (listSql.Count > 0)
                        //    //    {
                        //    //        if (bTest)
                        //    //        {
                        //    //            for (int iii = 0; iii < listSql.Count; iii++)
                        //    //            {
                        //    //                TraceHelper.WriteLine(listSql[iii]);
                        //    //            }
                        //    //        }

                        //    //        dii.RunSql(listSql);

                        //    //        listSql.Clear();

                        //    //        listSql = null;
                        //    //    }

                        //    //    rthText.Invoke(new EventHandler(delegate
                        //    //    {
                        //    //        rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":SQL语句执行成功，回写完成，共执行 " + listSql.Count.ToString() + " 条SQL语句\r\n");

                        //    //        rthText.ScrollToCaret();

                        //    //        CallDoEvents();
                        //    //    }));
                        //    //}
                        //    //else
                        //    //{
                        //    //    rthText.Invoke(new EventHandler(delegate
                        //    //    {
                        //    //        rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":没有回写配置，接口执行完成" + "\r\n");

                        //    //        rthText.ScrollToCaret();

                        //    //        CallDoEvents();
                        //    //    }));
                        //    //}

                        //    rthText.Invoke(new EventHandler(delegate
                        //    {
                        //        rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":回写被注释掉了，执行完毕一次" + "\r\n");

                        //        rthText.ScrollToCaret();

                        //        CallDoEvents();
                        //    }));

                        //    //如果设置了接口执行成功后的后继调用接口，则直接调用对应的接口
                        //    //if (!string.IsNullOrEmpty(InterFaceSet[iSub].FollowIF))
                        //    //{
                        //    //    string[] fi = InterFaceSet[iSub].FollowIF.Split(',');

                        //    //    //0:chiname,1:subname
                        //    //    int iiSub = InterFaceSet.FindIndex(delegate (clsInterfaceSet p) { return p.SubName == fi[1] && p.ChiName == fi[0]; });

                        //    //    if (iiSub >= 0)
                        //    //    {
                        //    //        if (string.IsNullOrEmpty(InterFaceSet[iiSub].SubType) || InterFaceSet[iiSub].SubType == "webservice")
                        //    //        {
                        //    //            //WebService接口执行
                        //    //            RunWebServiceSub(fi[1], fi[0], iiSub, false, false);
                        //    //        }
                        //    //        else
                        //    //        {
                        //    //            RunHttpSub(fi[1], fi[0], iiSub, false, false);
                        //    //        }
                        //    //    }
                        //    //}

                        //    //判断接口是否需要自循环
                        //    //获取数据的接口，才需要进行设置判断逻辑
                        //    //发送数据的接口，在基表或者组合XML/Json的根/第一级节点对应的SQL语句查询不到数据时
                        //    //都会跳出本次，不进行自循环
                        //    //if (InterFaceSet[iSub].loopBySelf == "1")
                        //    //{
                        //    //    if (InterFaceSet[iSub].ILoopJudRec.Count > 0)
                        //    //    {
                        //    //        //如果有自循环判断逻辑设置，进行计算判断，通过后才进行自循环
                        //    //        if (JudRecSign(iSub, strResult, args, "loop"))
                        //    //        {
                        //    //            RunHttpSub(strSubName, subChiName, iSub, true, false);
                        //    //        }
                        //    //    }
                        //    //    else
                        //    //    {
                        //    //        //没有自循环判断逻辑设置，进行自循环
                        //    //        RunHttpSub(strSubName, subChiName, iSub, true, false);
                        //    //    }
                        //    //}

                        //    //if (bPre)
                        //    //{
                        //    //    bPreRunResult = true;
                        //    //}

                        //}
                        //else
                        //{
                        //    rthText.Invoke(new EventHandler(delegate
                        //    {
                        //        rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":返回数据为空值" + "\r\n");

                        //        rthText.ScrollToCaret();

                        //        CallDoEvents();
                        //    }));
                        //}
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
                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":接口执行发生异常\r\n" + ex.Message + "\r\n");

                    rthText.ScrollToCaret();

                    CallDoEvents();
                }));
            }
            catch (Exception ex)
            {
                rthText.Invoke(new EventHandler(delegate
                {
                    rthText.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-->" + subChiName + ":接口执行发生异常\r\n" + ex.Message + "\r\n");

                    rthText.ScrollToCaret();

                    CallDoEvents();
                }));
            }
        }

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

                            int iTab = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTab; });

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

                            if (drSelect[i]["isdate"].ToString().Trim() == "1" || drSelect[i]["dateformat"].ToString().Trim() == "")
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
                                int iTableT = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTableName; });

                                if (iTableT >= 0)
                                {
                                    //判断设定的取值列名在数据表中是否存在
                                    if (listTable[iTableT].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                    {
                                        //如果设定的取值列名存在，则将值添加至XML中

                                        string tmpColValue = "";

                                        if (listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()] != null)
                                        {
                                            tmpColValue = listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();
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
                                        else if (drSelect[i]["dateformat"].ToString().Trim() != "" && drSelect[i]["dateformat"].ToString().Trim() != "Num" && tmpColValue != "")
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

                            if (drSelect[i]["isdate"].ToString().Trim() == "1" || drSelect[i]["dateformat"].ToString().Trim() == "")
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

                            if (drSelect[i]["titleisloop"].ToString().Trim() == "1")
                            {
                                sBuilder.Append("[");

                                int iTable = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == drSelect[i]["looptable"].ToString().Trim(); });

                                if (iTable >= 0)
                                {
                                    //对数据进行循环
                                    for (int m = 0; m < listTable[iTable].table.Rows.Count; m++)
                                    {
                                        listTable[iTable].iRows = m;

                                        sBuilder.Append(CreateJson(dsXml, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet));

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
                        int iTableHave = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); });

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

                        if (drSelect[i]["isdate"].ToString().Trim() == "1" || drSelect[i]["dateformat"].ToString().Trim() == "")
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
                            int iTableT = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTableName; });

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
                                        tmpColValue = listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();
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
                                    else if (drSelect[i]["dateformat"].ToString().Trim() != "" && drSelect[i]["dateformat"].ToString().Trim() != "Num" && tmpColValue != "")
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

                        if (drSelect[i]["isdate"].ToString().Trim() == "1" || drSelect[i]["dateformat"].ToString().Trim() == "")
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

                        if (drSelect[i]["titleisloop"].ToString().Trim() == "1")
                        {
                            sbu.Append("[");

                            int iTable = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == drSelect[i]["looptable"].ToString().Trim(); });

                            if (iTable >= 0)
                            {
                                //对数据进行循环
                                for (int m = 0; m < listTable[iTable].table.Rows.Count; m++)
                                {
                                    listTable[iTable].iRows = m;

                                    sbu.Append(CreateJson(dsXml, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet));

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

        #region "整合XML"
        private List<string> CreateXmlPara(DataSet dsXml, DataSet dllSet, DataSet dllParaSet)
        {
            //DAOInterface din = DAOFactory.CreateDAOInterface();

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
                    cts.table = dii.GetDataSetBySql(strSql).Tables[0];

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

                sBuilder.Append("<" + xmlRoot + ">");

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
            //DAOInterface din = DAOFactory.CreateDAOInterface();

            try
            {
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
                        int iTableHave = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == drSelect[i]["tablename"].ToString().Trim(); });

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
                        int iTable = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == drSelect[i]["looptable"].ToString().Trim(); });

                        //对数据进行循环
                        for (int m = 0; m < listTable[iTable].table.Rows.Count; m++)
                        {
                            //赋值循环行数脚标
                            listTable[iTable].iRows = m;

                            //首先添加该标签的开始标签
                            sbu.Append("<" + drSelect[i]["xmltitle"].ToString().Trim() + ">");

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
                                    int iTableT = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTableName; });

                                    if (iTableT >= 0)
                                    {
                                        //判断设定的取值列名在数据表中是否存在
                                        if (listTable[iTableT].table.Columns.Contains(drSelect[i]["valtitle"].ToString().Trim()))
                                        {
                                            //如果设定的取值列名存在，则将值添加至XML中

                                            string tmpColValue = "";

                                            if (listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()] != null)
                                            {
                                                tmpColValue = listTable[iTableT].table.Rows[listTable[iTableT].iRows][drSelect[i]["valtitle"].ToString().Trim()].ToString();
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
                                            else if (drSelect[i]["dateformat"].ToString().Trim() != "" && drSelect[i]["dateformat"].ToString().Trim() != "Num" && tmpColValue != "")
                                            {
                                                tmpColValue = string.Format(drSelect[i]["dateformat"].ToString().Trim(), Convert.ToDouble(tmpColValue));
                                            }


                                            if (drSelect[i]["datadatilbydll"].ToString().Trim().ToUpper() == "1")
                                            {

                                                tmpColValue = ConvertValueByDll(tmpColValue, parentTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);

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
                            string xmlRootEnd = drSelect[i]["xmltitle"].ToString().Trim();
                            if (xmlRootEnd.Trim().Contains(" "))
                            {
                                xmlRootEnd = xmlRootEnd.Substring(0, xmlRootEnd.IndexOf(" "));
                            }

                            sbu.Append("</" + xmlRootEnd + ">");

                            //sbu.Append("</" + drSelect[i]["xmltitle"].ToString().Trim() + ">");
                        }
                    }
                    else
                    {
                        //此标签不需要循环
                        //不需要循环时，下层标签如果是取值的标签则直接使用第一行数据
                        //获取数据表的标签，因为是不需要循环的标签则值应该是使用父标签对应的表
                        //使用父标签以及XML设置结构，获取父标签下的所有子标签信息


                        //首先添加该标签的开始标签
                        sbu.Append("<" + drSelect[i]["xmltitle"].ToString().Trim() + ">");

                        //判断该标签下面是值还是XML
                        if (drSelect[i]["paratype"].ToString().Trim().ToUpper() == "V")
                        {
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
                                            tmpColValue = ConvertValueByDll(tmpColValue, parentTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
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
                                        xmlStaticVal = ConvertValueByDll(xmlStaticVal, parentTitle, drSelect[i]["xmltitle"].ToString().Trim(), dllSet, dllParaSet);
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
                        string xmlRootEnd = drSelect[i]["xmltitle"].ToString().Trim();
                        if (xmlRootEnd.Trim().Contains(" "))
                        {
                            xmlRootEnd = xmlRootEnd.Substring(0, xmlRootEnd.IndexOf(" "));
                        }

                        sbu.Append("</" + xmlRootEnd + ">");
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
        #endregion "整合XML"

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

                int iTableT = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == strTable; });

                return listTable[iTableT].table.Rows[listTable[iTableT].iRows][strCol].ToString().Trim();

            }
            catch
            {
                return "";
            }
        }
        #endregion "获取发送数据"

        #region "调用DLL处理数据"
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
        #endregion "调用DLL处理数据"

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
            catch (UserException ex)
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

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback += (object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error) => { return true; };

                System.Net.WebRequest myHttpWebRequest = System.Net.WebRequest.Create(url);
                myHttpWebRequest.Method = "get";

                if (header != null)
                {
                    for (int i = 0; i < header.Count; i++)
                    {
                        myHttpWebRequest.Headers.Add(header[i].HeaderName, header[i].HeaderValue);
                    }
                }

                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                string lcHtml = string.Empty;
                Encoding enc = Encoding.GetEncoding(en_coding);
                System.IO.Stream stream = response.GetResponseStream();
                System.IO.StreamReader streamReader = new System.IO.StreamReader(stream, enc);
                lcHtml = streamReader.ReadToEnd();
                return lcHtml;
            }
            catch (Exception ex)
            {
                string a = ex.Message;

                if (!string.IsNullOrEmpty(ex.InnerException.Message))
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

                System.Net.ServicePointManager.ServerCertificateValidationCallback += (object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors error) => { return true; };

                System.Net.WebRequest myHttpWebRequest = System.Net.WebRequest.Create(url);
                myHttpWebRequest.Method = "post";

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
                    myHttpWebRequest.ContentType = contenttype;
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
                System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                string lcHtml = string.Empty;
                Encoding enc = Encoding.GetEncoding(en_coding);
                System.IO.Stream stream = response.GetResponseStream();
                System.IO.StreamReader streamReader = new System.IO.StreamReader(stream, enc);
                lcHtml = streamReader.ReadToEnd();
                return lcHtml;

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
                else if (zip.ToUpper() == "DEFLATE")
                {
                    handler.AutomaticDecompression = DecompressionMethods.Deflate;
                }
                else
                {
                    handler.AutomaticDecompression = DecompressionMethods.None;
                }

                using (var http = new HttpClient(handler))
                {
                    var content = new FormUrlEncodedContent(dic);

                    var response = http.PostAsync(new Uri(url), content).Result;

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

        #region WebClientPost
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

                using (var http = new HttpClient(handler))
                {
                    var content = new FormUrlEncodedContent(dic);

                    var response = http.PostAsync(new Uri(url), content).Result;

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

        #region "判断返回的数据是否可以继续向下解析"
        /// <summary>
        /// 判断返回的数据是否可以继续向下解析
        /// </summary>
        /// <param name="iSub">接口函数脚标</param>
        /// <param name="strResult">接收到的数据</param>
        /// <returns>true:可以继续向下解析；false：不能继续向下解析，显示设定的提示信息标签的内容</returns>
        private bool JudRecSign(int iSub, string strResult, object[] args, string judType)
        {
            try
            {
                string strProm = "";

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
            catch (Exception ex)
            {
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
        private string ConformProm(string prom, string strResult, object[] args, string dataType)
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

                        int iXmlAt = listXmlNode.FindIndex(delegate (clsXmlStu p) { return p.titleName == loopNode; });

                        if (iXmlAt >= 0)
                        {
                            listXmlNode.RemoveAt(iXmlAt);
                        }

                        listXmlNode.Add(cxs);

                        iXmlAt = listXmlNode.FindIndex(delegate (clsXmlStu p) { return p.titleName == loopNode; });

                        //循环
                        for (int i = 0; i < xnl.Count; i++)
                        {
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
                                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey == "1"; });
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
                                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey == "1"; });
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

                                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey == "1"; });
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
                                        List<clsInterfaceReTableSet> listCanUpdate = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.canUpdate == "1"; });

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
                                #endregion "不需要判断数据是否在表中已经存在"}
                            }
                            #endregion "执行insert"
                            else
                            #region "执行update"
                            {
                                List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                List<clsInterfaceReTableSet> listRtN = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey != "1"; });

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
                            #endregion "执行update"
                        }
                    }
                    #endregion "循环依据为返回数据"
                    else
                    #region "循环依据为发送数据"
                    {
                        string tabName = reSet.looppoit;

                        int iTableT = listTable.FindIndex(delegate (clsTableStru p) { return p.titleName == tabName; });

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
                                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey == "1"; });
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
                                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey == "1"; });
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

                                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey == "1"; });
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
                                        List<clsInterfaceReTableSet> listCanUpdate = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.canUpdate == "1"; });

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
                                #endregion "不需要判断数据是否在表中已经存在"
                            }
                            #endregion "执行insert"
                            else
                            #region "执行update"
                            {
                                List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey == "1"; });
                                List<clsInterfaceReTableSet> listRtN = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey != "1"; });

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
                                List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey == "1"; });
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
                                listSql.Add(sb.ToString());
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
                                List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey == "1"; });
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

                                listSql.Add(sb.ToString());
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

                                List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey == "1"; });
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
                                List<clsInterfaceReTableSet> listCanUpdate = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.canUpdate == "1"; });

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

                            listSql.Add(sb.ToString());
                        }
                        #endregion "不需要判断数据是否在表中已经存在"
                    }
                    else
                    {
                        List<clsInterfaceReTableSet> listRt = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey == "1"; });
                        List<clsInterfaceReTableSet> listRtN = reSet.IReTabSet.FindAll(delegate (clsInterfaceReTableSet p) { return p.iskey != "1"; });

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
                }
                #endregion "数据不需要循环"

                return listSql;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
            finally
            {
                listSql.Clear();

                sb.Clear();
                sb2.Clear();
            }
        }
        #endregion "整合回写SQL语句"

        #region "处理数据1"
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
        #endregion "处理数据1"

        #region "获取接收数据"
        private string GetRevXmlValue(string strXml, string node)
        {
            try
            {
                string lastNode = node;

                string tmpNode = node;

                int iCount = Regex.Matches(tmpNode.Replace(".", "<A>"), @"<A>").Count;

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

                        int iWhere = listXmlNode.FindIndex(delegate (clsXmlStu p) { return p.titleName == tmpNode; });

                        if (iWhere >= 0)
                        {
                            return XmlHelper.GetValue(listXmlNode[iWhere].nodeList[listXmlNode[iWhere].iRows], node.Substring(tmpNode.Length + 1));
                        }
                        else
                        {
                            iCount = Regex.Matches(tmpNode.Replace(".", "<A>"), @"<A>").Count;
                        }
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

                    int iWhere = listXmlNode.FindIndex(delegate (clsXmlStu p) { return p.titleName == tmpNode; });

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

        private void frmDataTrans_Load(object sender, EventArgs e)
        {
            DataTrans();

            this.DialogResult = DialogResult.OK;
        }
    }
}
