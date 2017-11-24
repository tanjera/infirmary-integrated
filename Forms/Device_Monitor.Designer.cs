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
            this.timerTracing = new System.Windows.Forms.Timer(this.components);
            this.menuMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_NewPatient = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItem_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.patientToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_EditPatient = new System.Windows.Forms.ToolStripMenuItem();
            this.deviceOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_About = new System.Windows.Forms.ToolStripMenuItem();
            this.timerVitals = new System.Windows.Forms.Timer(this.components);
            this.bpValues = new II.Controls.Values_BP();
            this.spO2Values = new II.Controls.Values_HR();
            this.ecgValues = new II.Controls.Values_HR();
            this.ecgTracing = new II.Controls.Tracing();
            this.tracing1 = new II.Controls.Tracing();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.menuMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // timerTracing
            // 
            this.timerTracing.Enabled = true;
            this.timerTracing.Tick += new System.EventHandler(this.onTick_Tracing);
            // 
            // menuMain
            // 
            this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.patientToolStripMenuItem,
            this.deviceOptionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuMain.Location = new System.Drawing.Point(0, 0);
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(759, 24);
            this.menuMain.TabIndex = 4;
            this.menuMain.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_NewPatient,
            this.toolStripSeparator1,
            this.menuItem_Exit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // menuItem_NewPatient
            // 
            this.menuItem_NewPatient.Name = "menuItem_NewPatient";
            this.menuItem_NewPatient.Size = new System.Drawing.Size(138, 22);
            this.menuItem_NewPatient.Text = "New Patient";
            this.menuItem_NewPatient.Click += new System.EventHandler(this.menuItem_NewPatient_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(135, 6);
            // 
            // menuItem_Exit
            // 
            this.menuItem_Exit.Name = "menuItem_Exit";
            this.menuItem_Exit.Size = new System.Drawing.Size(138, 22);
            this.menuItem_Exit.Text = "Exit";
            this.menuItem_Exit.Click += new System.EventHandler(this.menuItem_Exit_Click);
            // 
            // patientToolStripMenuItem
            // 
            this.patientToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_EditPatient});
            this.patientToolStripMenuItem.Name = "patientToolStripMenuItem";
            this.patientToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.patientToolStripMenuItem.Text = "Patient";
            // 
            // menuItem_EditPatient
            // 
            this.menuItem_EditPatient.Name = "menuItem_EditPatient";
            this.menuItem_EditPatient.Size = new System.Drawing.Size(134, 22);
            this.menuItem_EditPatient.Text = "Edit Patient";
            this.menuItem_EditPatient.Click += new System.EventHandler(this.menuItem_EditPatient_Click);
            // 
            // deviceOptionsToolStripMenuItem
            // 
            this.deviceOptionsToolStripMenuItem.Name = "deviceOptionsToolStripMenuItem";
            this.deviceOptionsToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.deviceOptionsToolStripMenuItem.Text = "Device";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_About});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // menuItem_About
            // 
            this.menuItem_About.Name = "menuItem_About";
            this.menuItem_About.Size = new System.Drawing.Size(215, 22);
            this.menuItem_About.Text = "About Infirmary Integrated";
            this.menuItem_About.Click += new System.EventHandler(this.menuItem_About_Click);
            // 
            // timerVitals
            // 
            this.timerVitals.Enabled = true;
            this.timerVitals.Tick += new System.EventHandler(this.onTick_Vitals);
            // 
            // bpValues
            // 
            this.bpValues.BackColor = System.Drawing.Color.Black;
            this.bpValues.Location = new System.Drawing.Point(12, 324);
            this.bpValues.Name = "bpValues";
            this.bpValues.Size = new System.Drawing.Size(132, 126);
            this.bpValues.TabIndex = 5;
            // 
            // spO2Values
            // 
            this.spO2Values.BackColor = System.Drawing.Color.Black;
            this.spO2Values.Location = new System.Drawing.Point(12, 192);
            this.spO2Values.Name = "spO2Values";
            this.spO2Values.Size = new System.Drawing.Size(132, 126);
            this.spO2Values.TabIndex = 5;
            // 
            // ecgValues
            // 
            this.ecgValues.BackColor = System.Drawing.Color.Black;
            this.ecgValues.Location = new System.Drawing.Point(12, 60);
            this.ecgValues.Name = "ecgValues";
            this.ecgValues.Size = new System.Drawing.Size(132, 126);
            this.ecgValues.TabIndex = 5;
            // 
            // ecgTracing
            // 
            this.ecgTracing.Location = new System.Drawing.Point(150, 60);
            this.ecgTracing.Name = "ecgTracing";
            this.ecgTracing.Size = new System.Drawing.Size(597, 126);
            this.ecgTracing.TabIndex = 1;
            this.ecgTracing.Paint += new System.Windows.Forms.PaintEventHandler(this.ECGTracing_Paint);
            // 
            // tracing1
            // 
            this.tracing1.Location = new System.Drawing.Point(150, 233);
            this.tracing1.Name = "tracing1";
            this.tracing1.Size = new System.Drawing.Size(597, 126);
            this.tracing1.TabIndex = 1;
            this.tracing1.Paint += new System.Windows.Forms.PaintEventHandler(this.tracing1_Paint);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(504, 394);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            11,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown1.TabIndex = 6;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // Device_Monitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(759, 466);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.bpValues);
            this.Controls.Add(this.spO2Values);
            this.Controls.Add(this.ecgValues);
            this.Controls.Add(this.tracing1);
            this.Controls.Add(this.ecgTracing);
            this.Controls.Add(this.menuMain);
            this.MainMenuStrip = this.menuMain;
            this.Name = "Device_Monitor";
            this.Text = "Infirmary Integrated: Cardiac Monitor";
            this.menuMain.ResumeLayout(false);
            this.menuMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private II.Controls.Tracing ecgTracing;
        private System.Windows.Forms.Timer timerTracing;
        private System.Windows.Forms.MenuStrip menuMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItem_NewPatient;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuItem_Exit;
        private System.Windows.Forms.ToolStripMenuItem patientToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItem_EditPatient;
        private II.Controls.Values_HR ecgValues;
        private System.Windows.Forms.Timer timerVitals;
        private Controls.Values_HR spO2Values;
        private Controls.Values_BP bpValues;
        private System.Windows.Forms.ToolStripMenuItem deviceOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItem_About;
        private Controls.Tracing tracing1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
    }
}

