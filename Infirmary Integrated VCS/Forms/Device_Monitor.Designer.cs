namespace II.Forms
{
    partial class Device_Monitor
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Device_Monitor));
            this.timerTracing = new System.Windows.Forms.Timer(this.components);
            this.menuMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_PauseDevice = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.setRowAmountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
            this.fontSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mediumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.largeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extraLargeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_ColorScheme = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_Fullscreen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.patientToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_NewPatient = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_EditPatient = new System.Windows.Forms.ToolStripMenuItem();
            this.timerVitals = new System.Windows.Forms.Timer(this.components);
            this.layoutTracings = new System.Windows.Forms.TableLayoutPanel();
            this.layoutSplit = new System.Windows.Forms.SplitContainer();
            this.layoutNumerics = new System.Windows.Forms.TableLayoutPanel();
            this.menuMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutSplit)).BeginInit();
            this.layoutSplit.Panel1.SuspendLayout();
            this.layoutSplit.Panel2.SuspendLayout();
            this.layoutSplit.SuspendLayout();
            this.SuspendLayout();
            // 
            // timerTracing
            // 
            this.timerTracing.Enabled = true;
            this.timerTracing.Tick += new System.EventHandler(this.OnTick_Tracing);
            // 
            // menuMain
            // 
            this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.patientToolStripMenuItem});
            this.menuMain.Location = new System.Drawing.Point(0, 0);
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(759, 24);
            this.menuMain.TabIndex = 4;
            this.menuMain.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_PauseDevice,
            this.toolStripSeparator2,
            this.setRowAmountToolStripMenuItem,
            this.toolStripMenuItem1,
            this.fontSizeToolStripMenuItem,
            this.menuItem_ColorScheme,
            this.menuItem_Fullscreen,
            this.toolStripSeparator1,
            this.closeToolStripMenuItem,
            this.menuItem_Exit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.fileToolStripMenuItem.Text = "&Device";
            // 
            // menuItem_PauseDevice
            // 
            this.menuItem_PauseDevice.Name = "menuItem_PauseDevice";
            this.menuItem_PauseDevice.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.menuItem_PauseDevice.Size = new System.Drawing.Size(193, 22);
            this.menuItem_PauseDevice.Text = "Pa&use Device";
            this.menuItem_PauseDevice.Click += new System.EventHandler(this.MenuTogglePause_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(190, 6);
            // 
            // setRowAmountToolStripMenuItem
            // 
            this.setRowAmountToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5,
            this.toolStripMenuItem6});
            this.setRowAmountToolStripMenuItem.Name = "setRowAmountToolStripMenuItem";
            this.setRowAmountToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.setRowAmountToolStripMenuItem.Text = "&Numeric Row Amount";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(80, 22);
            this.toolStripMenuItem2.Text = "&1";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.MenuNumericRowCount_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(80, 22);
            this.toolStripMenuItem3.Text = "&2";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.MenuNumericRowCount_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(80, 22);
            this.toolStripMenuItem4.Text = "&3";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.MenuNumericRowCount_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(80, 22);
            this.toolStripMenuItem5.Text = "&4";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.MenuNumericRowCount_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(80, 22);
            this.toolStripMenuItem6.Text = "&5";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.MenuNumericRowCount_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem7,
            this.toolStripMenuItem8,
            this.toolStripMenuItem9,
            this.toolStripMenuItem10,
            this.toolStripMenuItem11});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(193, 22);
            this.toolStripMenuItem1.Text = "&Tracing Row Amount";
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(80, 22);
            this.toolStripMenuItem7.Text = "&1";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.MenuTracingRowCount_Click);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(80, 22);
            this.toolStripMenuItem8.Text = "&2";
            this.toolStripMenuItem8.Click += new System.EventHandler(this.MenuTracingRowCount_Click);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new System.Drawing.Size(80, 22);
            this.toolStripMenuItem9.Text = "&3";
            this.toolStripMenuItem9.Click += new System.EventHandler(this.MenuTracingRowCount_Click);
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(80, 22);
            this.toolStripMenuItem10.Text = "&4";
            this.toolStripMenuItem10.Click += new System.EventHandler(this.MenuTracingRowCount_Click);
            // 
            // toolStripMenuItem11
            // 
            this.toolStripMenuItem11.Name = "toolStripMenuItem11";
            this.toolStripMenuItem11.Size = new System.Drawing.Size(80, 22);
            this.toolStripMenuItem11.Text = "&5";
            this.toolStripMenuItem11.Click += new System.EventHandler(this.MenuTracingRowCount_Click);
            // 
            // fontSizeToolStripMenuItem
            // 
            this.fontSizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.smallToolStripMenuItem,
            this.mediumToolStripMenuItem,
            this.largeToolStripMenuItem,
            this.extraLargeToolStripMenuItem});
            this.fontSizeToolStripMenuItem.Name = "fontSizeToolStripMenuItem";
            this.fontSizeToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.fontSizeToolStripMenuItem.Text = "Font Size";
            // 
            // smallToolStripMenuItem
            // 
            this.smallToolStripMenuItem.Name = "smallToolStripMenuItem";
            this.smallToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.smallToolStripMenuItem.Text = "Small";
            this.smallToolStripMenuItem.Click += new System.EventHandler(this.MenuFontSize_Click);
            // 
            // mediumToolStripMenuItem
            // 
            this.mediumToolStripMenuItem.Name = "mediumToolStripMenuItem";
            this.mediumToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.mediumToolStripMenuItem.Text = "Medium";
            this.mediumToolStripMenuItem.Click += new System.EventHandler(this.MenuFontSize_Click);
            // 
            // largeToolStripMenuItem
            // 
            this.largeToolStripMenuItem.Name = "largeToolStripMenuItem";
            this.largeToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.largeToolStripMenuItem.Text = "Large";
            this.largeToolStripMenuItem.Click += new System.EventHandler(this.MenuFontSize_Click);
            // 
            // extraLargeToolStripMenuItem
            // 
            this.extraLargeToolStripMenuItem.Name = "extraLargeToolStripMenuItem";
            this.extraLargeToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.extraLargeToolStripMenuItem.Text = "Extra Large";
            this.extraLargeToolStripMenuItem.Click += new System.EventHandler(this.MenuFontSize_Click);
            // 
            // menuItem_ColorScheme
            // 
            this.menuItem_ColorScheme.Name = "menuItem_ColorScheme";
            this.menuItem_ColorScheme.Size = new System.Drawing.Size(193, 22);
            this.menuItem_ColorScheme.Text = "Color Scheme";
            // 
            // menuItem_Fullscreen
            // 
            this.menuItem_Fullscreen.Name = "menuItem_Fullscreen";
            this.menuItem_Fullscreen.ShortcutKeys = System.Windows.Forms.Keys.F11;
            this.menuItem_Fullscreen.Size = new System.Drawing.Size(193, 22);
            this.menuItem_Fullscreen.Text = "Toggle &Fullscreen";
            this.menuItem_Fullscreen.Click += new System.EventHandler(this.MenuFullscreen_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(190, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F4)));
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.closeToolStripMenuItem.Text = "&Close Device";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.MenuClose_Click);
            // 
            // menuItem_Exit
            // 
            this.menuItem_Exit.Name = "menuItem_Exit";
            this.menuItem_Exit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.menuItem_Exit.Size = new System.Drawing.Size(193, 22);
            this.menuItem_Exit.Text = "E&xit Infirmary";
            this.menuItem_Exit.Click += new System.EventHandler(this.MenuExit_Click);
            // 
            // patientToolStripMenuItem
            // 
            this.patientToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_NewPatient,
            this.menuItem_EditPatient});
            this.patientToolStripMenuItem.Name = "patientToolStripMenuItem";
            this.patientToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.patientToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.patientToolStripMenuItem.Text = "&Patient";
            // 
            // menuItem_NewPatient
            // 
            this.menuItem_NewPatient.Name = "menuItem_NewPatient";
            this.menuItem_NewPatient.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.menuItem_NewPatient.Size = new System.Drawing.Size(181, 22);
            this.menuItem_NewPatient.Text = "&New Patient";
            this.menuItem_NewPatient.Click += new System.EventHandler(this.MenuNewPatient_Click);
            // 
            // menuItem_EditPatient
            // 
            this.menuItem_EditPatient.Name = "menuItem_EditPatient";
            this.menuItem_EditPatient.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.menuItem_EditPatient.Size = new System.Drawing.Size(181, 22);
            this.menuItem_EditPatient.Text = "&Edit Patient";
            this.menuItem_EditPatient.Click += new System.EventHandler(this.MenuEditPatient_Click);
            // 
            // timerVitals
            // 
            this.timerVitals.Enabled = true;
            this.timerVitals.Tick += new System.EventHandler(this.OnTick_Vitals);
            // 
            // layoutTracings
            // 
            this.layoutTracings.AutoSize = true;
            this.layoutTracings.BackColor = System.Drawing.Color.Black;
            this.layoutTracings.ColumnCount = 1;
            this.layoutTracings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.layoutTracings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.layoutTracings.Location = new System.Drawing.Point(0, 0);
            this.layoutTracings.Name = "layoutTracings";
            this.layoutTracings.Padding = new System.Windows.Forms.Padding(10, 11, 10, 11);
            this.layoutTracings.RowCount = 1;
            this.layoutTracings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutTracings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 480F));
            this.layoutTracings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 480F));
            this.layoutTracings.Size = new System.Drawing.Size(605, 502);
            this.layoutTracings.TabIndex = 6;
            // 
            // layoutSplit
            // 
            this.layoutSplit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(10)))));
            this.layoutSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutSplit.Location = new System.Drawing.Point(0, 24);
            this.layoutSplit.Name = "layoutSplit";
            // 
            // layoutSplit.Panel1
            // 
            this.layoutSplit.Panel1.Controls.Add(this.layoutNumerics);
            // 
            // layoutSplit.Panel2
            // 
            this.layoutSplit.Panel2.Controls.Add(this.layoutTracings);
            this.layoutSplit.Size = new System.Drawing.Size(759, 478);
            this.layoutSplit.SplitterDistance = 150;
            this.layoutSplit.TabIndex = 7;
            // 
            // layoutNumerics
            // 
            this.layoutNumerics.AutoSize = true;
            this.layoutNumerics.BackColor = System.Drawing.Color.Black;
            this.layoutNumerics.ColumnCount = 1;
            this.layoutNumerics.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.layoutNumerics.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.layoutNumerics.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutNumerics.Location = new System.Drawing.Point(0, 0);
            this.layoutNumerics.Name = "layoutNumerics";
            this.layoutNumerics.Padding = new System.Windows.Forms.Padding(10, 11, 10, 11);
            this.layoutNumerics.RowCount = 1;
            this.layoutNumerics.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutNumerics.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 480F));
            this.layoutNumerics.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 480F));
            this.layoutNumerics.Size = new System.Drawing.Size(150, 478);
            this.layoutNumerics.TabIndex = 7;
            // 
            // Device_Monitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(759, 502);
            this.Controls.Add(this.layoutSplit);
            this.Controls.Add(this.menuMain);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuMain;
            this.Name = "Device_Monitor";
            this.Text = "Infirmary Integrated: Cardiac Monitor";
            this.SizeChanged += new System.EventHandler(this.OnFormResize);
            this.menuMain.ResumeLayout(false);
            this.menuMain.PerformLayout();
            this.layoutSplit.Panel1.ResumeLayout(false);
            this.layoutSplit.Panel1.PerformLayout();
            this.layoutSplit.Panel2.ResumeLayout(false);
            this.layoutSplit.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutSplit)).EndInit();
            this.layoutSplit.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timerTracing;
        private System.Windows.Forms.MenuStrip menuMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuItem_Exit;
        private System.Windows.Forms.ToolStripMenuItem patientToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItem_EditPatient;
        private System.Windows.Forms.Timer timerVitals;
        private System.Windows.Forms.TableLayoutPanel layoutTracings;
        private System.Windows.Forms.ToolStripMenuItem menuItem_PauseDevice;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem setRowAmountToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem menuItem_Fullscreen;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItem_NewPatient;
        private System.Windows.Forms.ToolStripMenuItem menuItem_ColorScheme;
        private System.Windows.Forms.SplitContainer layoutSplit;
        private System.Windows.Forms.TableLayoutPanel layoutNumerics;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem9;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem10;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem11;
        private System.Windows.Forms.ToolStripMenuItem fontSizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mediumToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem largeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extraLargeToolStripMenuItem;
    }
}

