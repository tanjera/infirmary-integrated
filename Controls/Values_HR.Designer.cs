namespace Infirmary_Integrated.Controls {
    partial class Values_HR {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose ();
            }
            base.Dispose (disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent () {
            this.labelType = new System.Windows.Forms.Label();
            this.labelHR = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelType
            // 
            this.labelType.AutoSize = true;
            this.labelType.ForeColor = System.Drawing.SystemColors.Control;
            this.labelType.Location = new System.Drawing.Point(3, 0);
            this.labelType.Name = "labelType";
            this.labelType.Size = new System.Drawing.Size(0, 13);
            this.labelType.TabIndex = 0;
            // 
            // labelHR
            // 
            this.labelHR.AutoSize = true;
            this.labelHR.Font = new System.Drawing.Font("Microsoft Sans Serif", 50F, System.Drawing.FontStyle.Bold);
            this.labelHR.ForeColor = System.Drawing.SystemColors.Control;
            this.labelHR.Location = new System.Drawing.Point(16, 21);
            this.labelHR.Name = "labelHR";
            this.labelHR.Size = new System.Drawing.Size(0, 76);
            this.labelHR.TabIndex = 0;
            this.labelHR.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Values_HR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.labelHR);
            this.Controls.Add(this.labelType);
            this.Name = "Values_HR";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelType;
        private System.Windows.Forms.Label labelHR;
    }
}
