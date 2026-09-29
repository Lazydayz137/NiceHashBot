namespace NHB3
{
    partial class Home
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Home));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newOrderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editSelectedOrderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ordersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.balanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.botToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arbitrageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arbitrageDashboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.multiAlgorithmMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.equihashMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.autoPilotOffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoPilotONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelMetrics = new System.Windows.Forms.Panel();
            this.panelBotStatus = new System.Windows.Forms.Panel();
            this.lblBotStatus = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.panelTotalSpeed = new System.Windows.Forms.Panel();
            this.lblTotalSpeed = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panelActiveRigs = new System.Windows.Forms.Panel();
            this.lblActiveRigs = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panelTotalOrders = new System.Windows.Forms.Panel();
            this.lblTotalOrders = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panelBalance = new System.Windows.Forms.Panel();
            this.lblBalance = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panelMetrics.SuspendLayout();
            this.panelBotStatus.SuspendLayout();
            this.panelTotalSpeed.SuspendLayout();
            this.panelActiveRigs.SuspendLayout();
            this.panelTotalOrders.SuspendLayout();
            this.panelBalance.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            //
            // menuStrip1
            //
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshToolStripMenuItem,
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1406, 36);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            //
            // refreshToolStripMenuItem
            //
            this.refreshToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newOrderToolStripMenuItem,
            this.editSelectedOrderToolStripMenuItem,
            this.ordersToolStripMenuItem,
            this.balanceToolStripMenuItem});
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(87, 32);
            this.refreshToolStripMenuItem.Text = "Actions";
            //
            // newOrderToolStripMenuItem
            //
            this.newOrderToolStripMenuItem.Name = "newOrderToolStripMenuItem";
            this.newOrderToolStripMenuItem.Size = new System.Drawing.Size(261, 34);
            this.newOrderToolStripMenuItem.Text = "New order";
            this.newOrderToolStripMenuItem.Click += new System.EventHandler(this.newOrderToolStripMenuItem_Click);
            //
            // editSelectedOrderToolStripMenuItem
            //
            this.editSelectedOrderToolStripMenuItem.Name = "editSelectedOrderToolStripMenuItem";
            this.editSelectedOrderToolStripMenuItem.Size = new System.Drawing.Size(261, 34);
            this.editSelectedOrderToolStripMenuItem.Text = "Edit selected order";
            this.editSelectedOrderToolStripMenuItem.Click += new System.EventHandler(this.editSelectedOrderToolStripMenuItem_Click);
            //
            // ordersToolStripMenuItem
            //
            this.ordersToolStripMenuItem.Name = "ordersToolStripMenuItem";
            this.ordersToolStripMenuItem.Size = new System.Drawing.Size(261, 34);
            this.ordersToolStripMenuItem.Text = "Refresh orders";
            this.ordersToolStripMenuItem.Click += new System.EventHandler(this.ordersToolStripMenuItem_Click);
            //
            // balanceToolStripMenuItem
            //
            this.balanceToolStripMenuItem.Name = "balanceToolStripMenuItem";
            this.balanceToolStripMenuItem.Size = new System.Drawing.Size(261, 34);
            this.balanceToolStripMenuItem.Text = "Refresh balance";
            this.balanceToolStripMenuItem.Click += new System.EventHandler(this.balanceToolStripMenuItem_Click);
            //
            // toolStripMenuItem1
            //
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.botToolStripMenuItem,
            this.arbitrageToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(92, 32);
            this.toolStripMenuItem1.Text = "Settings";
            //
            // toolStripMenuItem2
            //
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(180, 34);
            this.toolStripMenuItem2.Text = "API";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.api_Click);
            //
            // toolStripMenuItem3
            //
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(180, 34);
            this.toolStripMenuItem3.Text = "Pools";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.pools_Click);
            //
            // botToolStripMenuItem
            //
            this.botToolStripMenuItem.Name = "botToolStripMenuItem";
            this.botToolStripMenuItem.Size = new System.Drawing.Size(180, 34);
            this.botToolStripMenuItem.Text = "Bot";
            this.botToolStripMenuItem.Click += new System.EventHandler(this.botToolStripMenuItem_Click);
            //
            // arbitrageToolStripMenuItem
            //
            this.arbitrageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.arbitrageDashboardToolStripMenuItem,
            this.multiAlgorithmMonitorToolStripMenuItem,
            this.equihashMonitorToolStripMenuItem});
            this.arbitrageToolStripMenuItem.Name = "arbitrageToolStripMenuItem";
            this.arbitrageToolStripMenuItem.Size = new System.Drawing.Size(180, 34);
            this.arbitrageToolStripMenuItem.Text = "Arbitrage 💎";
            //
            // arbitrageDashboardToolStripMenuItem
            //
            this.arbitrageDashboardToolStripMenuItem.Name = "arbitrageDashboardToolStripMenuItem";
            this.arbitrageDashboardToolStripMenuItem.Size = new System.Drawing.Size(300, 34);
            this.arbitrageDashboardToolStripMenuItem.Text = "📊 Dashboard";
            this.arbitrageDashboardToolStripMenuItem.Click += new System.EventHandler(this.arbitrageDashboardToolStripMenuItem_Click);
            //
            // multiAlgorithmMonitorToolStripMenuItem
            //
            this.multiAlgorithmMonitorToolStripMenuItem.Name = "multiAlgorithmMonitorToolStripMenuItem";
            this.multiAlgorithmMonitorToolStripMenuItem.Size = new System.Drawing.Size(300, 34);
            this.multiAlgorithmMonitorToolStripMenuItem.Text = "⚡ Multi-Algorithm Monitor";
            this.multiAlgorithmMonitorToolStripMenuItem.Click += new System.EventHandler(this.multiAlgorithmMonitorToolStripMenuItem_Click);
            //
            // equihashMonitorToolStripMenuItem
            //
            this.equihashMonitorToolStripMenuItem.Name = "equihashMonitorToolStripMenuItem";
            this.equihashMonitorToolStripMenuItem.Size = new System.Drawing.Size(300, 34);
            this.equihashMonitorToolStripMenuItem.Text = "💎 Equihash Monitor";
            this.equihashMonitorToolStripMenuItem.Click += new System.EventHandler(this.equihashMonitorToolStripMenuItem_Click);
            //
            // statusStrip1
            //
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSplitButton1,
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 693);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1406, 32);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            //
            // toolStripSplitButton1
            //
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoPilotOffToolStripMenuItem,
            this.autoPilotONToolStripMenuItem});
            this.toolStripSplitButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton1.Image")));
            this.toolStripSplitButton1.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(37, 29);
            this.toolStripSplitButton1.Text = "toolStripSplitButton1";
            //
            // autoPilotOffToolStripMenuItem
            //
            this.autoPilotOffToolStripMenuItem.Name = "autoPilotOffToolStripMenuItem";
            this.autoPilotOffToolStripMenuItem.Size = new System.Drawing.Size(172, 34);
            this.autoPilotOffToolStripMenuItem.Text = "Bot Off";
            this.autoPilotOffToolStripMenuItem.Click += new System.EventHandler(this.autoPilotOffToolStripMenuItem_Click);
            //
            // autoPilotONToolStripMenuItem
            //
            this.autoPilotONToolStripMenuItem.Name = "autoPilotONToolStripMenuItem";
            this.autoPilotONToolStripMenuItem.Size = new System.Drawing.Size(172, 34);
            this.autoPilotONToolStripMenuItem.Text = "Bot On";
            this.autoPilotONToolStripMenuItem.Click += new System.EventHandler(this.autoPilotONToolStripMenuItem_Click);
            //
            // toolStripStatusLabel1
            //
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(100, 25);
            this.toolStripStatusLabel1.Text = "Bot: Stopped";
            //
            // toolStripStatusLabel2
            //
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(75, 25);
            this.toolStripStatusLabel2.Text = "Balance:";
            //
            // panelMetrics
            //
            this.panelMetrics.Controls.Add(this.panelBotStatus);
            this.panelMetrics.Controls.Add(this.panelTotalSpeed);
            this.panelMetrics.Controls.Add(this.panelActiveRigs);
            this.panelMetrics.Controls.Add(this.panelTotalOrders);
            this.panelMetrics.Controls.Add(this.panelBalance);
            this.panelMetrics.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMetrics.Location = new System.Drawing.Point(0, 36);
            this.panelMetrics.Name = "panelMetrics";
            this.panelMetrics.Padding = new System.Windows.Forms.Padding(15, 15, 15, 10);
            this.panelMetrics.Size = new System.Drawing.Size(1406, 120);
            this.panelMetrics.TabIndex = 2;
            //
            // panelBotStatus
            //
            this.panelBotStatus.BackColor = System.Drawing.Color.White;
            this.panelBotStatus.Controls.Add(this.lblBotStatus);
            this.panelBotStatus.Controls.Add(this.label8);
            this.panelBotStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelBotStatus.Location = new System.Drawing.Point(1115, 15);
            this.panelBotStatus.Name = "panelBotStatus";
            this.panelBotStatus.Size = new System.Drawing.Size(260, 95);
            this.panelBotStatus.TabIndex = 4;
            //
            // lblBotStatus
            //
            this.lblBotStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblBotStatus.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblBotStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.lblBotStatus.Location = new System.Drawing.Point(0, 45);
            this.lblBotStatus.Name = "lblBotStatus";
            this.lblBotStatus.Size = new System.Drawing.Size(260, 50);
            this.lblBotStatus.TabIndex = 1;
            this.lblBotStatus.Text = "Stopped";
            this.lblBotStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label8
            //
            this.label8.Dock = System.Windows.Forms.DockStyle.Top;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.label8.Location = new System.Drawing.Point(0, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(260, 30);
            this.label8.TabIndex = 0;
            this.label8.Text = "BOT STATUS";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // panelTotalSpeed
            //
            this.panelTotalSpeed.BackColor = System.Drawing.Color.White;
            this.panelTotalSpeed.Controls.Add(this.lblTotalSpeed);
            this.panelTotalSpeed.Controls.Add(this.label6);
            this.panelTotalSpeed.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelTotalSpeed.Location = new System.Drawing.Point(840, 15);
            this.panelTotalSpeed.Margin = new System.Windows.Forms.Padding(5);
            this.panelTotalSpeed.Name = "panelTotalSpeed";
            this.panelTotalSpeed.Size = new System.Drawing.Size(275, 95);
            this.panelTotalSpeed.TabIndex = 3;
            //
            // lblTotalSpeed
            //
            this.lblTotalSpeed.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblTotalSpeed.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTotalSpeed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.lblTotalSpeed.Location = new System.Drawing.Point(0, 45);
            this.lblTotalSpeed.Name = "lblTotalSpeed";
            this.lblTotalSpeed.Size = new System.Drawing.Size(275, 50);
            this.lblTotalSpeed.TabIndex = 1;
            this.lblTotalSpeed.Text = "0.00";
            this.lblTotalSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label6
            //
            this.label6.Dock = System.Windows.Forms.DockStyle.Top;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.label6.Location = new System.Drawing.Point(0, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(275, 30);
            this.label6.TabIndex = 0;
            this.label6.Text = "TOTAL SPEED";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // panelActiveRigs
            //
            this.panelActiveRigs.BackColor = System.Drawing.Color.White;
            this.panelActiveRigs.Controls.Add(this.lblActiveRigs);
            this.panelActiveRigs.Controls.Add(this.label4);
            this.panelActiveRigs.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelActiveRigs.Location = new System.Drawing.Point(565, 15);
            this.panelActiveRigs.Margin = new System.Windows.Forms.Padding(5);
            this.panelActiveRigs.Name = "panelActiveRigs";
            this.panelActiveRigs.Size = new System.Drawing.Size(275, 95);
            this.panelActiveRigs.TabIndex = 2;
            //
            // lblActiveRigs
            //
            this.lblActiveRigs.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblActiveRigs.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblActiveRigs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
            this.lblActiveRigs.Location = new System.Drawing.Point(0, 45);
            this.lblActiveRigs.Name = "lblActiveRigs";
            this.lblActiveRigs.Size = new System.Drawing.Size(275, 50);
            this.lblActiveRigs.TabIndex = 1;
            this.lblActiveRigs.Text = "0";
            this.lblActiveRigs.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label4
            //
            this.label4.Dock = System.Windows.Forms.DockStyle.Top;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(275, 30);
            this.label4.TabIndex = 0;
            this.label4.Text = "ACTIVE RIGS";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // panelTotalOrders
            //
            this.panelTotalOrders.BackColor = System.Drawing.Color.White;
            this.panelTotalOrders.Controls.Add(this.lblTotalOrders);
            this.panelTotalOrders.Controls.Add(this.label2);
            this.panelTotalOrders.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelTotalOrders.Location = new System.Drawing.Point(290, 15);
            this.panelTotalOrders.Margin = new System.Windows.Forms.Padding(5);
            this.panelTotalOrders.Name = "panelTotalOrders";
            this.panelTotalOrders.Size = new System.Drawing.Size(275, 95);
            this.panelTotalOrders.TabIndex = 1;
            //
            // lblTotalOrders
            //
            this.lblTotalOrders.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblTotalOrders.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTotalOrders.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(156)))), ((int)(((byte)(18)))));
            this.lblTotalOrders.Location = new System.Drawing.Point(0, 45);
            this.lblTotalOrders.Name = "lblTotalOrders";
            this.lblTotalOrders.Size = new System.Drawing.Size(275, 50);
            this.lblTotalOrders.TabIndex = 1;
            this.lblTotalOrders.Text = "0";
            this.lblTotalOrders.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label2
            //
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(275, 30);
            this.label2.TabIndex = 0;
            this.label2.Text = "TOTAL ORDERS";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // panelBalance
            //
            this.panelBalance.BackColor = System.Drawing.Color.White;
            this.panelBalance.Controls.Add(this.lblBalance);
            this.panelBalance.Controls.Add(this.label1);
            this.panelBalance.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelBalance.Location = new System.Drawing.Point(15, 15);
            this.panelBalance.Margin = new System.Windows.Forms.Padding(5);
            this.panelBalance.Name = "panelBalance";
            this.panelBalance.Size = new System.Drawing.Size(275, 95);
            this.panelBalance.TabIndex = 0;
            //
            // lblBalance
            //
            this.lblBalance.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblBalance.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblBalance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.lblBalance.Location = new System.Drawing.Point(0, 45);
            this.lblBalance.Name = "lblBalance";
            this.lblBalance.Size = new System.Drawing.Size(275, 50);
            this.lblBalance.TabIndex = 1;
            this.lblBalance.Text = "0.00000000";
            this.lblBalance.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // label1
            //
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(275, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = "BALANCE";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // dataGridView1
            //
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(15, 166);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(15, 10, 15, 10);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.RowTemplate.Height = 30;
            this.dataGridView1.Size = new System.Drawing.Size(1376, 515);
            this.dataGridView1.TabIndex = 3;
            //
            // Home
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1406, 725);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panelMetrics);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Home";
            this.Text = "NHB3 - Modern NiceHash Bot";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelMetrics.ResumeLayout(false);
            this.panelBotStatus.ResumeLayout(false);
            this.panelTotalSpeed.ResumeLayout(false);
            this.panelActiveRigs.ResumeLayout(false);
            this.panelTotalOrders.ResumeLayout(false);
            this.panelBalance.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem balanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ordersToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ToolStripMenuItem newOrderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editSelectedOrderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripMenuItem autoPilotOffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoPilotONToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem botToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arbitrageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arbitrageDashboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem multiAlgorithmMonitorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem equihashMonitorToolStripMenuItem;
        private System.Windows.Forms.Panel panelMetrics;
        private System.Windows.Forms.Panel panelBalance;
        private System.Windows.Forms.Label lblBalance;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelTotalOrders;
        private System.Windows.Forms.Label lblTotalOrders;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelActiveRigs;
        private System.Windows.Forms.Label lblActiveRigs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panelTotalSpeed;
        private System.Windows.Forms.Label lblTotalSpeed;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panelBotStatus;
        private System.Windows.Forms.Label lblBotStatus;
        private System.Windows.Forms.Label label8;
    }
}

