using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Hydee.Aout.Interface
{
    public static class XmlHelper
    {
        /// <summary>
        /// 获取单个节点内的值时使用
        /// </summary>
        /// <param name="doc">加载后的xml</param>
        /// <param name="strNode">节点名称</param>
        /// <returns>节点内容，如果该节点下包含多个节点而不是直接对应值，则会将下面所有节点的值串联到一起返回</returns>
        public static string GetValueByNode(XmlDocument doc,string strNode)
        {
            try
            {
                return doc.SelectSingleNode(strNode).InnerText.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取单个节点内的值时使用
        /// </summary>
        /// <param name="doc">加载后的xml</param>
        /// <param name="strNode">节点名称</param>
        /// <returns>节点内容，如果该节点下包含多个节点而不是直接对应值，则会将下面所有节点的值串联到一起返回</returns>
        public static string GetValueByNode(string strXml, string strNode)
        {
            try
            {
                return GetXmlDoc(strXml).SelectSingleNode(strNode).InnerText.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取节点下面的XML格式内容（对应上面获取节点内的值）
        /// </summary>
        /// <param name="doc">加载后的xml</param>
        /// <param name="strNode">节点名称</param>
        /// <returns>返回节点下xml格式的字符串</returns>
        public static string GetXmlByNode(XmlDocument doc, string strNode)
        {
            try
            {
                return doc.SelectSingleNode(strNode).InnerXml.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取节点下面的XML格式内容（对应上面获取节点内的值）
        /// </summary>
        /// <param name="doc">加载后的xml</param>
        /// <param name="strNode">节点名称</param>
        /// <returns>返回节点下xml格式的字符串</returns>
        public static string GetXmlByNode(string strXml, string strNode)
        {
            try
            {
                return GetXmlDoc(strXml).SelectSingleNode(strNode).InnerXml.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取某节点下的所有节点内容（在一个节点下的内容是重复循环的明细数据时，使用该方法可以方便循环）
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="strNode"></param>
        /// <returns></returns>
        public static XmlNode GetXmlNodeByNode(XmlDocument doc, string strNode)
        {
            try
            {
                return doc.SelectSingleNode(strNode);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取某节点下的所有节点内容（在一个节点下的内容是重复循环的明细数据时，使用该方法可以方便循环）
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="strNode"></param>
        /// <returns></returns>
        public static XmlNode GetXmlNodeByNode(string strXml, string strNode)
        {
            try
            {
                XmlDocument xd = GetXmlDoc(strXml);
                int iLoop = xd.ChildNodes.Count;

                for (int i = 0; i < iLoop; i++)
                {
                    if (xd.ChildNodes[i].Name.ToUpper() == strNode.ToUpper())
                    {
                        return xd.ChildNodes[i];                    
                    }
                }

                return null;
                //return xmlNode.ChildNodes;
                //return GetXmlDoc(strXml).SelectSingleNode(strNode);
                //return GetXmlDoc(strXml).FirstChild
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static XmlNodeList GetXmlNodeByNodes(XmlNode xmlNode, string strNode)
        {
            try
            {
                //XmlNodeList xnl = xmlNode.ChildNodes;
                
                int iLoop = xmlNode.ChildNodes.Count;

                for (int i = 0; i < iLoop; i++)
                {
                    if (xmlNode.ChildNodes[i].Name.ToUpper() != strNode.ToUpper())
                    {
                        //xnl.
                        //xnl.Item(i).RemoveAll();
                        xmlNode.RemoveChild(xmlNode.ChildNodes[i]);
                        i--;
                        iLoop--;
                    }
                }

                return xmlNode.ChildNodes;
                //xmlNode.ChildNodes[
                //return xmlNode.SelectNodes(strNode);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static XmlNode GetXmlNodeByNode(XmlNode xmlNode, string strNode)
        {
            try
            {
                return xmlNode.SelectSingleNode(strNode);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GetValueNodeByNode(XmlNode xmlNode, string strNode)
        {
            try
            {
                //XmlNode xml = xmlNode.SelectSingleNode(strNode);
                string mmm = "";

                for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
                {
                    if (xmlNode.ChildNodes[i].Name.ToUpper() == strNode.ToUpper())
                    {
                        mmm = xmlNode.ChildNodes[i].InnerXml;

                        //if (string.IsNullOrEmpty(mmm))
                        //{
                        //    mmm = xmlNode.ChildNodes[i].InnerXml;
                        //}
                    }
                }
                //string mmm = xml.InnerText.ToString();

                return mmm;
                //return xmlNode.SelectSingleNode(strNode).InnerText.ToString();

                
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 加载XML字符串
        /// </summary>
        /// <param name="strXml"></param>
        /// <returns></returns>
        public static XmlDocument GetXmlDoc(string strXml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(strXml);
            
            return doc;
        }

        /// <summary>
        /// 通过XML字符串（包含根标签的整个XML）以及节点获取对应的值
        /// </summary>
        /// <param name="strXml">xml字符串</param>
        /// <param name="node">节点标签：格式--->node1.node2.node3</param>
        /// <returns>节点标签对应的值</returns>
        public static string GetValue(string strXml, string node)
        {
            try
            {
                if (node == "ALL")
                {
                    return strXml;
                }

                string[] nodes = node.Split('.');

                XmlNode xn = XmlHelper.GetXmlNodeByNode(strXml, nodes[0]);

                XmlNodeList xnl;

                if (nodes[nodes.Length - 1].ToUpper() == "COUNT()")
                {
                    XmlNodeList nl = XmlHelper.GetNodeList(strXml, node.Substring(0, node.LastIndexOf(".")));

                    return nl.Count.ToString();
                }
                else
                {
                    for (int i = 1; i < nodes.Length - 1; i++)
                    {
                        xnl = GetXmlNodeByNodes(xn, nodes[i]);

                        xn = xnl[0];
                    }

                    if (xn == null)
                    {
                        return "";
                    }
                    else
                    {
                        return GetValueNodeByNode(xn, nodes[nodes.Length - 1]);
                    }
                }
            }
            catch
            {
                return "";
            }
        }
        
        /// <summary>
        /// 通过XML字符串（包含根标签的整个XML）以及节点获取对应的节点集合
        /// </summary>
        /// <param name="strXml">xml字符串</param>
        /// <param name="node">节点标签：格式--->node1.node2.node3</param>
        /// <returns>节点集合</returns>
        public static XmlNodeList GetNodeList(string strXml, string node)
        {
            try
            {
                string[] nodes = node.Split('.');

                XmlNode xn = XmlHelper.GetXmlNodeByNode(strXml, nodes[0]);

                XmlNodeList xnl = null;

                for (int i = 1; i < nodes.Length; i++)
                {
                    xnl = GetXmlNodeByNodes(xn, nodes[i]);

                    xn = xnl[0];
                }

                return xnl;
                //return GetValueNodeByNode(xn, nodes[nodes.Length - 1]);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 通过提供的一个XML节点，以及对应的节点标签，获取标签对应的值
        /// </summary>
        /// <param name="xnlNode">XML节点信息(一个XML结构的内容)</param>
        /// <param name="node">节点标签：格式--->node1.node2.node3</param>
        /// <returns>标签对应的值</returns>
        public static string GetValue(XmlNode xnlNode, string node)
        {
            try
            {
                string[] nodes = node.Split('.');

                XmlNodeList xnl;

                for (int i = 0; i < nodes.Length - 1; i++)
                {
                    xnl = GetXmlNodeByNodes(xnlNode, nodes[i]);

                    xnlNode = xnl[0];
                }

                return GetValueNodeByNode(xnlNode, nodes[nodes.Length - 1]);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根据XML节点（）以及对应标签获取节点集
        /// </summary>
        /// <param name="xnlNode"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static XmlNodeList GetNodeList(XmlNode xnlNode, string node)
        {
            try
            {
                string[] nodes = node.Split('.');

                XmlNodeList xnl;

                for (int i = 1; i < nodes.Length - 1; i++)
                {
                    xnl = GetXmlNodeByNodes(xnlNode, nodes[i]);

                    xnlNode = xnl[0];
                }

                return xnlNode.SelectNodes(nodes[nodes.Length - 1]);
                //return GetValueNodeByNode(xn, nodes[nodes.Length - 1]);
            }
            catch
            {
                return null;
            }
        }

    }
}
