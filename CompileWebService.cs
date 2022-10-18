using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services.Description;
using System.Xml;
using Hydee.Aout.Interface.DAO;

namespace Hydee.Aout.Interface
{
    public class CompileWebService
    {
        public CompilerResults CreateCRByUrl(string strUrl, ref string strClassName)
        {
            try
            {
                string @namespace = "Hydee.Aout.Interface";

                //string classname = "";
                if ((strClassName == null) || (strClassName == ""))
                {
                    strClassName = GetWsClassName(strUrl);
                }

                //使用 Stream 对象，创建和格式化 WSDL 文档
                //WebClient wc = new WebClient();
                //Stream stream = wc.OpenRead(url + "?WSDL");
                //ServiceDescription sd = ServiceDescription.Read(stream);

                //使用 XmlTextReader 对象，创建和格式化 WSDL 文档
                XmlTextReader reader = new XmlTextReader(strUrl + "?wsdl");
                ServiceDescription sd = ServiceDescription.Read(reader);

                ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
                sdi.AddServiceDescription(sd, "", "");
                CodeNamespace cn = new CodeNamespace(@namespace);

                //生成客户端代理类代码
                CodeCompileUnit ccu = new CodeCompileUnit();
                ccu.Namespaces.Add(cn);
                sdi.Import(cn, ccu);
                CSharpCodeProvider csc = new CSharpCodeProvider();
                ICodeCompiler icc = csc.CreateCompiler();

                //设定编译参数
                CompilerParameters cplist = new CompilerParameters();
                cplist.GenerateExecutable = false;
                cplist.GenerateInMemory = true;
                cplist.ReferencedAssemblies.Add("System.dll");
                cplist.ReferencedAssemblies.Add("System.XML.dll");
                cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                cplist.ReferencedAssemblies.Add("System.Data.dll");

                CompilerResults cr = icc.CompileAssemblyFromDom(cplist, ccu);
                if (true == cr.Errors.HasErrors)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (System.CodeDom.Compiler.CompilerError ce in cr.Errors)
                    {
                        sb.Append(ce.ToString());
                        sb.Append(System.Environment.NewLine);
                    }
                    throw new Exception(sb.ToString());
                }
                else
                {
                    
                    return cr;
                }
            }
            catch(Exception ex)
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

        private string GetWsClassName(string wsUrl)
        {
            string[] parts = wsUrl.Split('/');
            string[] pps = parts[parts.Length - 1].Split('.');

            return pps[0];
        }
    }
}
