namespace Infirmary_Integrated
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
            this.timerDraw = new System.Windows.Forms.Timer(this.components);
            this.ecgTracing = new Infirmary_Integrated.Tracing();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // timerDraw
            // 
            this.timerDraw.Enabled = true;
            this.timerDraw.Tick += new System.EventHandler(this.onTick);
            // 
            // ecgTracing
            // 
            this.ecgTracing.Location = new System.Drawing.Point(12, 12);
            this.ecgTracing.Name = "ecgTracing";
            this.ecgTracing.Size = new System.Drawing.Size(735, 126);
            this.ecgTracing.TabIndex = 1;
            this.ecgTracing.Paint += new System.Windows.Forms.PaintEventHandler(this.ECGTracing_Paint);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Normal_Sinus",
            "Sinus_Tachycardia",
            "Sinus_Bradycardia",
            "Atrial_Flutter",
            "Atrial_Fibrillation",
            "Premature_Atrial_Contractions",
            "Supraventricular_Tachycardia",
            "AV_Block__1st_Degree",
            "AV_Block__Wenckebach",
            "AV_Block__Mobitz_II",
            "AV_Block__3rd_Degree",
            "Junctional",
            "Premature_Junctional_Contractions",
            "Block__Bundle_Branch",
            "Premature_Ventricular_Contractions",
            "Idioventricular",
            "Ventricular_Fibrillation",
            "Ventricular_Standstill",
            "Asystole"});
            this.comboBox1.Location = new System.Drawing.Point(492, 433);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(255, 21);
            this.comboBox1.TabIndex = 3;
            this.comboBox1.Text = "Normal_Sinus";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_TextUpdate);
            // 
            // Device_Monitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(759, 466);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.ecgTracing);
            this.Name = "Device_Monitor";
            this.Text = "Infirmary Integrated: Cardiac Monitor";
            this.ResumeLayout(false);

        }

        #endregion

        private Infirmary_Integrated.Tracing ecgTracing;
        private System.Windows.Forms.Timer timerDraw;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

