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
            this.ecgTracing = new System.Windows.Forms.Panel();
            this.ecgNumerics = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // ecgTracing
            // 
            this.ecgTracing.Location = new System.Drawing.Point(147, 12);
            this.ecgTracing.Name = "ecgTracing";
            this.ecgTracing.Size = new System.Drawing.Size(600, 126);
            this.ecgTracing.TabIndex = 1;
            this.ecgTracing.Paint += new System.Windows.Forms.PaintEventHandler(this.ecgTracing_Paint);
            // 
            // ecgNumerics
            // 
            this.ecgNumerics.Location = new System.Drawing.Point(12, 12);
            this.ecgNumerics.Name = "ecgNumerics";
            this.ecgNumerics.Size = new System.Drawing.Size(129, 126);
            this.ecgNumerics.TabIndex = 1;
            this.ecgNumerics.Paint += new System.Windows.Forms.PaintEventHandler(this.ecgNumerics_Paint);
            // 
            // Device_Monitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(759, 466);
            this.Controls.Add(this.ecgNumerics);
            this.Controls.Add(this.ecgTracing);
            this.Name = "Device_Monitor";
            this.Text = "Infirmary Integrated: Cardiac Monitor";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ecgTracing;
        private System.Windows.Forms.Panel ecgNumerics;
    }
}

