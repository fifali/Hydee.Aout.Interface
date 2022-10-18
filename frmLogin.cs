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
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
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
                Application.Exit();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmMain frm = new frmMain();

            frm.Show();

            this.Close();
        }
    }
}
