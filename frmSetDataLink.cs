using Hydee.Aout.Interface.DAO;
using MD5Encrypt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Hydee.Aout.Interface
{
    public partial class frmSetDataLink : Form
    {
        public frmSetDataLink()
        {
            InitializeComponent();
        }

        private void frmSetDataLink_Load(object sender, EventArgs e)
        {
            //DBConnection dbConn = new DBConnection();

            string strEnConn = clsGolbal.GetDLLAppKey("HydeeConn");
            string softver = "";

            try
            {
                softver = clsGolbal.GetDLLAppKey("SoftVer");
            }
            catch
            {
                softver = "1";
            }

            if (softver == "1")
            {
                rdoSql.Checked = true;
            }
            else if (softver == "3")
            {
                rdoodbc.Checked = true;
            }
            else
            {
                rdoOra.Checked = true;
            }

            if (!string.IsNullOrEmpty(strEnConn))
            {
                DeAndEncrypt crypt = new DeAndEncrypt();

                string strConn = crypt.Decrypt(strEnConn);

                if (softver == "1")
                {
                    string[] conn = strConn.Split(';');

                    if (conn.Length > 0)
                    {
                        for (int i = 0; i < conn.Length; i++)
                        {
                            if (conn[i].Length > 4)
                            {
                                switch (conn[i].Substring(0, 4))
                                {
                                    case "Data":
                                        txtServer.Text = conn[i].Substring(conn[i].IndexOf("=") + 1);
                                        break;
                                    case "Init":
                                        txtDataBase.Text = conn[i].Substring(conn[i].IndexOf("=") + 1);
                                        break;
                                    case "User":
                                        txtUserID.Text = conn[i].Substring(conn[i].IndexOf("=") + 1);
                                        break;
                                    case "Pass":
                                        txtPassWord.Text = conn[i].Substring(conn[i].IndexOf("=") + 1);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                else if (softver == "3")
                {
                    //Odbc
                    //DSN = XXX; UID = XXX; Pwd = XXX
                    strConn = strConn.Substring(strConn.IndexOf("DSN=") + 4);

                    txtServer.Text = strConn.Substring(0, strConn.IndexOf(";"));

                    txtDataBase.Text = txtServer.Text;

                    strConn = strConn.Substring(strConn.IndexOf("UID=") + 4);

                    txtUserID.Text = strConn.Substring(0, strConn.IndexOf(";"));

                    strConn = strConn.Substring(strConn.IndexOf("Pwd=") + 4);

                    int lenth = strConn.IndexOf(";");
                    if (lenth > 0)
                    {
                        txtPassWord.Text = strConn.Substring(0, strConn.IndexOf(";"));
                    }
                    else
                    {
                        txtPassWord.Text = strConn;
                    }
                }
                else
                {
                    //Oracle
                    //string strConn = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1) (PORT=1521)))(CONNECT_DATA=(SERVICE_NAME= orcl)));User Id=H2; Password=hydeesoft";

                    strConn = strConn.Substring(strConn.IndexOf("HOST=") + 5);

                    txtServer.Text = strConn.Substring(0, strConn.IndexOf(")"));

                    strConn = strConn.Substring(strConn.IndexOf("PORT=") + 5);

                    txtOraPort.Text = strConn.Substring(0, strConn.IndexOf(")"));

                    strConn = strConn.Substring(strConn.IndexOf("SERVICE_NAME=") + 13);

                    txtDataBase.Text = strConn.Substring(0, strConn.IndexOf(")"));

                    strConn = strConn.Substring(strConn.IndexOf("User Id=") + 8);

                    txtUserID.Text = strConn.Substring(0, strConn.IndexOf(";"));

                    strConn = strConn.Substring(strConn.IndexOf("Password=") + 9);

                    int lenth = strConn.IndexOf(";");
                    if (lenth > 0)
                    {
                        txtPassWord.Text = strConn.Substring(0, strConn.IndexOf(";"));
                    }
                    else
                    {
                        txtPassWord.Text = strConn;
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnSure_Click(object sender, EventArgs e)
        {
            try
            {
                btnSure.Enabled = false;
                string strServer = txtServer.Text.ToString().Trim();
                string strDataBase = txtDataBase.Text.ToString().Trim();
                string strUserID = txtUserID.Text.ToString().Trim();
                string strPassWord = txtPassWord.Text.ToString().Trim();

                if (strServer == "")
                {
                    MessageBox.Show("服务器地址不能为空！");

                    txtServer.Focus();
                }
                else if (strDataBase == "")
                {
                    MessageBox.Show("数据库名称不能为空！");

                    txtDataBase.Focus();
                }
                else if (strUserID == "")
                {
                    MessageBox.Show("数据库用户不能为空！");

                    txtUserID.Focus();
                }
                else if (strPassWord == "")
                {
                    MessageBox.Show("数据库密码不能为空！");

                    txtPassWord.Focus();
                }
                else
                {
                    string strConn = "";
                    string softver = "";

                    if (rdoSql.Checked)
                    {
                        softver = "1";
                        strConn = string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3}", strServer, strDataBase, strUserID, strPassWord);
                    }
                    else if(rdoodbc.Checked)
                    {
                        softver = "3";

                        //string strPort = txtOraPort.Text.ToString().Trim();

                        //if (strPort == "")
                        //{
                        //    MessageBox.Show("请输入端口号");
                        //    txtOraPort.Focus();
                        //    return;
                        //}

                        strConn = string.Format("DSN={0};UID={1};Pwd={2}", strServer, strUserID, strPassWord);
                    }
                    else
                    {
                        softver = "2";

                        string strPort = txtOraPort.Text.ToString().Trim();

                        if (strPort == "")
                        {
                            MessageBox.Show("请输入端口号");
                            txtOraPort.Focus();
                            return;
                        }

                        strConn = string.Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0}) (PORT={4})))(CONNECT_DATA=(SERVICE_NAME={1})));User Id={2}; Password={3}", strServer, strDataBase, strUserID, strPassWord, strPort);
                    }

                    DeAndEncrypt crypt = new DeAndEncrypt();

                    strConn = crypt.Encrypt(strConn);

                    clsGolbal.SetDllAppKey("HydeeConn", strConn);
                    clsGolbal.SetDllAppKey("SoftVer", softver);

                    MessageBox.Show("数据库服务器设置成功");

                    DialogResult = System.Windows.Forms.DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                btnSure.Enabled = true;
            }
        }

        private void rdoSql_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSql.Checked)
            {
                label6.Visible = false;
                txtOraPort.Visible = false;
            }
            else
            {
                label6.Visible = true;
                txtOraPort.Visible = true;
            }
        }
    }
}
