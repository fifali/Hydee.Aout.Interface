namespace Hydee.Aout.Interface
{
    partial class frmSetDataLink
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSetDataLink));
            this.txtOraPort = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.rdoOra = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.rdoSql = new System.Windows.Forms.RadioButton();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSure = new System.Windows.Forms.Button();
            this.txtPassWord = new System.Windows.Forms.TextBox();
            this.txtUserID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDataBase = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.rdoodbc = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // txtOraPort
            // 
            this.txtOraPort.Location = new System.Drawing.Point(399, 75);
            this.txtOraPort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtOraPort.Name = "txtOraPort";
            this.txtOraPort.Size = new System.Drawing.Size(160, 25);
            this.txtOraPort.TabIndex = 37;
            this.txtOraPort.Text = "1521";
            this.txtOraPort.Visible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(288, 79);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(97, 15);
            this.label6.TabIndex = 36;
            this.label6.Text = "数据库端口：";
            this.label6.Visible = false;
            // 
            // rdoOra
            // 
            this.rdoOra.AutoSize = true;
            this.rdoOra.Location = new System.Drawing.Point(180, 109);
            this.rdoOra.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoOra.Name = "rdoOra";
            this.rdoOra.Size = new System.Drawing.Size(100, 19);
            this.rdoOra.TabIndex = 35;
            this.rdoOra.Text = "H2 Oracle";
            this.rdoOra.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 114);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 15);
            this.label5.TabIndex = 34;
            this.label5.Text = "软件版本：";
            // 
            // rdoSql
            // 
            this.rdoSql.AutoSize = true;
            this.rdoSql.Checked = true;
            this.rdoSql.Location = new System.Drawing.Point(119, 109);
            this.rdoSql.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rdoSql.Name = "rdoSql";
            this.rdoSql.Size = new System.Drawing.Size(44, 19);
            this.rdoSql.TabIndex = 33;
            this.rdoSql.TabStop = true;
            this.rdoSql.Text = "H1";
            this.rdoSql.UseVisualStyleBackColor = true;
            this.rdoSql.CheckedChanged += new System.EventHandler(this.rdoSql_CheckedChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(291, 149);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 29);
            this.btnCancel.TabIndex = 32;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSure
            // 
            this.btnSure.Location = new System.Drawing.Point(180, 149);
            this.btnSure.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSure.Name = "btnSure";
            this.btnSure.Size = new System.Drawing.Size(100, 29);
            this.btnSure.TabIndex = 31;
            this.btnSure.Text = "确定";
            this.btnSure.UseVisualStyleBackColor = true;
            this.btnSure.Click += new System.EventHandler(this.btnSure_Click);
            // 
            // txtPassWord
            // 
            this.txtPassWord.Location = new System.Drawing.Point(399, 41);
            this.txtPassWord.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtPassWord.Name = "txtPassWord";
            this.txtPassWord.PasswordChar = '#';
            this.txtPassWord.Size = new System.Drawing.Size(160, 25);
            this.txtPassWord.TabIndex = 30;
            this.txtPassWord.Text = "1";
            // 
            // txtUserID
            // 
            this.txtUserID.Location = new System.Drawing.Point(399, 8);
            this.txtUserID.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtUserID.Name = "txtUserID";
            this.txtUserID.Size = new System.Drawing.Size(160, 25);
            this.txtUserID.TabIndex = 29;
            this.txtUserID.Text = "sa";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 15);
            this.label1.TabIndex = 23;
            this.label1.Text = "服务器地址：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(288, 45);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 15);
            this.label4.TabIndex = 26;
            this.label4.Text = "数据库密码：";
            // 
            // txtDataBase
            // 
            this.txtDataBase.Location = new System.Drawing.Point(119, 41);
            this.txtDataBase.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtDataBase.Name = "txtDataBase";
            this.txtDataBase.Size = new System.Drawing.Size(160, 25);
            this.txtDataBase.TabIndex = 28;
            this.txtDataBase.Text = "hydee";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(288, 11);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 15);
            this.label3.TabIndex = 25;
            this.label3.Text = "数据库用户：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 42);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 15);
            this.label2.TabIndex = 24;
            this.label2.Text = "数据库名称：";
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(119, 8);
            this.txtServer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(160, 25);
            this.txtServer.TabIndex = 27;
            this.txtServer.Text = "192.168.1.1";
            // 
            // rdoodbc
            // 
            this.rdoodbc.AutoSize = true;
            this.rdoodbc.Location = new System.Drawing.Point(294, 109);
            this.rdoodbc.Margin = new System.Windows.Forms.Padding(4);
            this.rdoodbc.Name = "rdoodbc";
            this.rdoodbc.Size = new System.Drawing.Size(60, 19);
            this.rdoodbc.TabIndex = 38;
            this.rdoodbc.Text = "Odbc";
            this.rdoodbc.UseVisualStyleBackColor = true;
            // 
            // frmSetDataLink
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 191);
            this.Controls.Add(this.rdoodbc);
            this.Controls.Add(this.txtOraPort);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.rdoOra);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.rdoSql);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSure);
            this.Controls.Add(this.txtPassWord);
            this.Controls.Add(this.txtUserID);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtDataBase);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtServer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "frmSetDataLink";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "配置数据连接";
            this.Load += new System.EventHandler(this.frmSetDataLink_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtOraPort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton rdoOra;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton rdoSql;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSure;
        private System.Windows.Forms.TextBox txtPassWord;
        private System.Windows.Forms.TextBox txtUserID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtDataBase;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.RadioButton rdoodbc;
    }
}