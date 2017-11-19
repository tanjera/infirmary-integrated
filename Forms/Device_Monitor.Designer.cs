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
            // Device_Monitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(759, 466);
            this.Controls.Add(this.ecgTracing);
            this.Name = "Device_Monitor";
            this.Text = "Infirmary Integrated: Cardiac Monitor";
            this.ResumeLayout(false);

        }

        #endregion

        private Infirmary_Integrated.Tracing ecgTracing;
        private System.Windows.Forms.Timer timerDraw;
    }
}

