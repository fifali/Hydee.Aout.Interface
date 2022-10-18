namespace Hydee.Aout.Interface
{
    partial class frmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.btnTest = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lvSub = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rthText = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtAgentPass = new System.Windows.Forms.TextBox();
            this.txtAgentUser = new System.Windows.Forms.TextBox();
            this.lblAgent = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.btnAgent = new System.Windows.Forms.Button();
            this.txtReRunTime = new System.Windows.Forms.TextBox();
            this.chkAotoReRun = new System.Windows.Forms.CheckBox();
            this.chkAutoRun = new System.Windows.Forms.CheckBox();
            this.lblCount = new System.Windows.Forms.Label();
            this.btnAbout = new System.Windows.Forms.Button();
            this.chkTest = new System.Windows.Forms.CheckBox();
            this.btnDataBase = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.chbusexml = new System.Windows.Forms.CheckBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.numAgain = new System.Windows.Forms.NumericUpDown();
            this.dtEnd = new System.Windows.Forms.DateTimePicker();
            this.dtBegin = new System.Windows.Forms.DateTimePicker();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnTest3 = new System.Windows.Forms.Button();
            this.btnRunSelectSub = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.timLoop = new System.Windows.Forms.Timer(this.components);
            this.ninote = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmBack = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmExit = new System.Windows.Forms.ToolStripMenuItem();
            this.timTrigger = new System.Windows.Forms.Timer(this.components);
            this.timReRun = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAgain)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(8, 398);
            this.btnTest.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(121, 29);
            this.btnTest.TabIndex = 2;
            this.btnTest.Text = "刷新接口列表";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            this.splitContainer1.Panel1MinSize = 650;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel2MinSize = 100;
            this.splitContainer1.Size = new System.Drawing.Size(1268, 886);
            this.splitContainer1.SplitterDistance = 1151;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 3;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.splitContainer2);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(1151, 886);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(4, 22);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox3);
            this.splitContainer2.Panel1MinSize = 400;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox4);
            this.splitContainer2.Panel2MinSize = 100;
            this.splitContainer2.Size = new System.Drawing.Size(1143, 860);
            this.splitContainer2.SplitterDistance = 438;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lvSub);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox3.Size = new System.Drawing.Size(1143, 438);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "接口函数列表";
            // 
            // lvSub
            // 
            this.lvSub.AutoArrange = false;
            this.lvSub.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvSub.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.lvSub.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSub.FullRowSelect = true;
            this.lvSub.GridLines = true;
            this.lvSub.Location = new System.Drawing.Point(4, 22);
            this.lvSub.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lvSub.Name = "lvSub";
            this.lvSub.Size = new System.Drawing.Size(1135, 412);
            this.lvSub.TabIndex = 0;
            this.lvSub.UseCompatibleStateImageBehavior = false;
            this.lvSub.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "名称";
            this.columnHeader1.Width = 300;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "方法名";
            this.columnHeader2.Width = 120;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "类名称";
            this.columnHeader3.Width = 120;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "地址";
            this.columnHeader4.Width = 360;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "可自动执行";
            this.columnHeader5.Width = 110;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rthText);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(0, 0);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox4.Size = new System.Drawing.Size(1143, 417);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "信息";
            // 
            // rthText
            // 
            this.rthText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rthText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rthText.Location = new System.Drawing.Point(4, 22);
            this.rthText.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rthText.Name = "rthText";
            this.rthText.Size = new System.Drawing.Size(1135, 391);
            this.rthText.TabIndex = 0;
            this.rthText.Text = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtAgentPass);
            this.groupBox1.Controls.Add(this.txtAgentUser);
            this.groupBox1.Controls.Add(this.lblAgent);
            this.groupBox1.Controls.Add(this.txtPort);
            this.groupBox1.Controls.Add(this.txtIP);
            this.groupBox1.Controls.Add(this.btnAgent);
            this.groupBox1.Controls.Add(this.txtReRunTime);
            this.groupBox1.Controls.Add(this.chkAotoReRun);
            this.groupBox1.Controls.Add(this.chkAutoRun);
            this.groupBox1.Controls.Add(this.lblCount);
            this.groupBox1.Controls.Add(this.btnAbout);
            this.groupBox1.Controls.Add(this.chkTest);
            this.groupBox1.Controls.Add(this.btnDataBase);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.chbusexml);
            this.groupBox1.Controls.Add(this.btnExit);
            this.groupBox1.Controls.Add(this.btnSave);
            this.groupBox1.Controls.Add(this.numAgain);
            this.groupBox1.Controls.Add(this.dtEnd);
            this.groupBox1.Controls.Add(this.dtBegin);
            this.groupBox1.Controls.Add(this.btnRun);
            this.groupBox1.Controls.Add(this.btnTest3);
            this.groupBox1.Controls.Add(this.btnRunSelectSub);
            this.groupBox1.Controls.Add(this.btnTest);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(112, 886);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "配置";
            // 
            // txtAgentPass
            // 
            this.txtAgentPass.Location = new System.Drawing.Point(7, 595);
            this.txtAgentPass.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtAgentPass.Name = "txtAgentPass";
            this.txtAgentPass.PasswordChar = '*';
            this.txtAgentPass.Size = new System.Drawing.Size(132, 25);
            this.txtAgentPass.TabIndex = 26;
            this.toolTip1.SetToolTip(this.txtAgentPass, "代理用户密码");
            // 
            // txtAgentUser
            // 
            this.txtAgentUser.Location = new System.Drawing.Point(8, 560);
            this.txtAgentUser.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtAgentUser.Name = "txtAgentUser";
            this.txtAgentUser.Size = new System.Drawing.Size(132, 25);
            this.txtAgentUser.TabIndex = 25;
            this.toolTip1.SetToolTip(this.txtAgentUser, "代理用户名");
            // 
            // lblAgent
            // 
            this.lblAgent.AutoSize = true;
            this.lblAgent.Location = new System.Drawing.Point(17, 675);
            this.lblAgent.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAgent.Name = "lblAgent";
            this.lblAgent.Size = new System.Drawing.Size(0, 15);
            this.lblAgent.TabIndex = 24;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(7, 526);
            this.txtPort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(132, 25);
            this.txtPort.TabIndex = 23;
            this.toolTip1.SetToolTip(this.txtPort, "网络代理端口");
            // 
            // txtIP
            // 
            this.txtIP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIP.Location = new System.Drawing.Point(8, 490);
            this.txtIP.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(95, 25);
            this.txtIP.TabIndex = 22;
            this.toolTip1.SetToolTip(this.txtIP, "网络代理地址");
            // 
            // btnAgent
            // 
            this.btnAgent.Location = new System.Drawing.Point(11, 638);
            this.btnAgent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAgent.Name = "btnAgent";
            this.btnAgent.Size = new System.Drawing.Size(123, 29);
            this.btnAgent.TabIndex = 21;
            this.btnAgent.Text = "开启代理";
            this.btnAgent.UseVisualStyleBackColor = true;
            this.btnAgent.Visible = false;
            this.btnAgent.Click += new System.EventHandler(this.button2_Click);
            // 
            // txtReRunTime
            // 
            this.txtReRunTime.Location = new System.Drawing.Point(8, 121);
            this.txtReRunTime.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtReRunTime.Name = "txtReRunTime";
            this.txtReRunTime.Size = new System.Drawing.Size(117, 25);
            this.txtReRunTime.TabIndex = 19;
            this.toolTip1.SetToolTip(this.txtReRunTime, "自动重启时间间隔(分钟)，勾选自动开始后有效");
            // 
            // chkAotoReRun
            // 
            this.chkAotoReRun.AutoSize = true;
            this.chkAotoReRun.Location = new System.Drawing.Point(19, 94);
            this.chkAotoReRun.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkAotoReRun.Name = "chkAotoReRun";
            this.chkAotoReRun.Size = new System.Drawing.Size(89, 19);
            this.chkAotoReRun.TabIndex = 18;
            this.chkAotoReRun.Text = "自动重启";
            this.chkAotoReRun.UseVisualStyleBackColor = true;
            // 
            // chkAutoRun
            // 
            this.chkAutoRun.AutoSize = true;
            this.chkAutoRun.Location = new System.Drawing.Point(19, 66);
            this.chkAutoRun.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkAutoRun.Name = "chkAutoRun";
            this.chkAutoRun.Size = new System.Drawing.Size(89, 19);
            this.chkAutoRun.TabIndex = 17;
            this.chkAutoRun.Text = "自动开始";
            this.chkAutoRun.UseVisualStyleBackColor = true;
            this.chkAutoRun.CheckedChanged += new System.EventHandler(this.chkAutoRun_CheckedChanged);
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(15, 160);
            this.lblCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(0, 15);
            this.lblCount.TabIndex = 16;
            // 
            // btnAbout
            // 
            this.btnAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAbout.Location = new System.Drawing.Point(8, 812);
            this.btnAbout.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(119, 29);
            this.btnAbout.TabIndex = 15;
            this.btnAbout.Text = "关  于";
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // chkTest
            // 
            this.chkTest.AutoSize = true;
            this.chkTest.Location = new System.Drawing.Point(11, 370);
            this.chkTest.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkTest.Name = "chkTest";
            this.chkTest.Size = new System.Drawing.Size(119, 19);
            this.chkTest.TabIndex = 14;
            this.chkTest.Text = "日志记录报文";
            this.toolTip1.SetToolTip(this.chkTest, "记录接收到的报文信息，如果有发送的XML报文也记录");
            this.chkTest.UseVisualStyleBackColor = true;
            this.chkTest.CheckedChanged += new System.EventHandler(this.chkTest_CheckedChanged);
            // 
            // btnDataBase
            // 
            this.btnDataBase.Location = new System.Drawing.Point(5, 334);
            this.btnDataBase.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnDataBase.Name = "btnDataBase";
            this.btnDataBase.Size = new System.Drawing.Size(119, 29);
            this.btnDataBase.TabIndex = 13;
            this.btnDataBase.Text = "配置数据连接";
            this.btnDataBase.UseVisualStyleBackColor = true;
            this.btnDataBase.Click += new System.EventHandler(this.btnDataBase_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(11, 749);
            this.button3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(100, 29);
            this.button3.TabIndex = 12;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Visible = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // chbusexml
            // 
            this.chbusexml.AutoSize = true;
            this.chbusexml.Location = new System.Drawing.Point(8, 721);
            this.chbusexml.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chbusexml.Name = "chbusexml";
            this.chbusexml.Size = new System.Drawing.Size(119, 19);
            this.chbusexml.TabIndex = 11;
            this.chbusexml.Text = "使用信息报文";
            this.toolTip1.SetToolTip(this.chbusexml, "勾选此按钮后，会使用信息窗口中的报文进行解析回写数据");
            this.chbusexml.UseVisualStyleBackColor = true;
            this.chbusexml.Visible = false;
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExit.Location = new System.Drawing.Point(8, 849);
            this.btnExit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(119, 29);
            this.btnExit.TabIndex = 10;
            this.btnExit.Text = "退  出";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(5, 298);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(119, 29);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "保存配置";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // numAgain
            // 
            this.numAgain.Location = new System.Drawing.Point(19, 259);
            this.numAgain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numAgain.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numAgain.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numAgain.Name = "numAgain";
            this.numAgain.Size = new System.Drawing.Size(108, 25);
            this.numAgain.TabIndex = 8;
            this.toolTip1.SetToolTip(this.numAgain, "自动运行每轮时间间隔(秒)");
            this.numAgain.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // dtEnd
            // 
            this.dtEnd.CustomFormat = "HH:mm";
            this.dtEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtEnd.Location = new System.Drawing.Point(19, 225);
            this.dtEnd.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dtEnd.Name = "dtEnd";
            this.dtEnd.ShowUpDown = true;
            this.dtEnd.Size = new System.Drawing.Size(107, 25);
            this.dtEnd.TabIndex = 7;
            this.toolTip1.SetToolTip(this.dtEnd, "自动运行结束时间");
            this.dtEnd.Value = new System.DateTime(2017, 10, 18, 23, 59, 0, 0);
            // 
            // dtBegin
            // 
            this.dtBegin.CustomFormat = "HH:mm";
            this.dtBegin.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtBegin.Location = new System.Drawing.Point(19, 191);
            this.dtBegin.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dtBegin.Name = "dtBegin";
            this.dtBegin.ShowUpDown = true;
            this.dtBegin.Size = new System.Drawing.Size(107, 25);
            this.dtBegin.TabIndex = 6;
            this.toolTip1.SetToolTip(this.dtBegin, "自动运行开始时间");
            this.dtBegin.Value = new System.DateTime(2017, 10, 18, 0, 0, 0, 0);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(8, 25);
            this.btnRun.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(119, 29);
            this.btnRun.TabIndex = 5;
            this.btnRun.Text = "开  始";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnTest3
            // 
            this.btnTest3.Location = new System.Drawing.Point(9, 774);
            this.btnTest3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTest3.Name = "btnTest3";
            this.btnTest3.Size = new System.Drawing.Size(121, 29);
            this.btnTest3.TabIndex = 4;
            this.btnTest3.Text = "测试按钮3";
            this.btnTest3.UseVisualStyleBackColor = true;
            this.btnTest3.Visible = false;
            this.btnTest3.Click += new System.EventHandler(this.btnTest3_Click);
            // 
            // btnRunSelectSub
            // 
            this.btnRunSelectSub.Location = new System.Drawing.Point(7, 434);
            this.btnRunSelectSub.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnRunSelectSub.Name = "btnRunSelectSub";
            this.btnRunSelectSub.Size = new System.Drawing.Size(123, 29);
            this.btnRunSelectSub.TabIndex = 3;
            this.btnRunSelectSub.Text = "执行选中接口";
            this.btnRunSelectSub.UseVisualStyleBackColor = true;
            this.btnRunSelectSub.Click += new System.EventHandler(this.btnRunSelectSub_Click);
            // 
            // timLoop
            // 
            this.timLoop.Interval = 1000;
            this.timLoop.Tick += new System.EventHandler(this.timLoop_Tick);
            // 
            // ninote
            // 
            this.ninote.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.ninote.BalloonTipText = "我在这里呢";
            this.ninote.BalloonTipTitle = "提醒";
            this.ninote.ContextMenuStrip = this.contextMenuStrip1;
            this.ninote.Icon = ((System.Drawing.Icon)(resources.GetObject("ninote.Icon")));
            this.ninote.Text = "notifyIcon1";
            this.ninote.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ninote_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmBack,
            this.toolStripMenuItem1,
            this.tsmExit});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(109, 58);
            // 
            // tsmBack
            // 
            this.tsmBack.Name = "tsmBack";
            this.tsmBack.Size = new System.Drawing.Size(108, 24);
            this.tsmBack.Text = "恢复";
            this.tsmBack.Click += new System.EventHandler(this.tsmBack_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(105, 6);
            // 
            // tsmExit
            // 
            this.tsmExit.Name = "tsmExit";
            this.tsmExit.Size = new System.Drawing.Size(108, 24);
            this.tsmExit.Text = "退出";
            this.tsmExit.Click += new System.EventHandler(this.tsmExit_Click);
            // 
            // timTrigger
            // 
            this.timTrigger.Interval = 1000;
            this.timTrigger.Tick += new System.EventHandler(this.timTrigger_Tick);
            // 
            // timReRun
            // 
            this.timReRun.Interval = 1000;
            this.timReRun.Tick += new System.EventHandler(this.timReRun_Tick);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1268, 886);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "海典软件第三方WEB接口调用 V2.4.0";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAgain)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListView lvSub;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button btnRunSelectSub;
        private System.Windows.Forms.Button btnTest3;
        private System.Windows.Forms.RichTextBox rthText;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.DateTimePicker dtBegin;
        private System.Windows.Forms.DateTimePicker dtEnd;
        private System.Windows.Forms.NumericUpDown numAgain;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Timer timLoop;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.NotifyIcon ninote;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmBack;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem tsmExit;
        private System.Windows.Forms.CheckBox chbusexml;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button btnDataBase;
        private System.Windows.Forms.CheckBox chkTest;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Timer timTrigger;
        private System.Windows.Forms.CheckBox chkAutoRun;
        private System.Windows.Forms.CheckBox chkAotoReRun;
        private System.Windows.Forms.TextBox txtReRunTime;
        private System.Windows.Forms.Timer timReRun;
        private System.Windows.Forms.Button btnAgent;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblAgent;
        private System.Windows.Forms.TextBox txtAgentPass;
        private System.Windows.Forms.TextBox txtAgentUser;
    }
}

