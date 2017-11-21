namespace II.Controls {
    partial class Values_BP {
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
            this.labelSBP = new System.Windows.Forms.Label();
            this.labelDBP = new System.Windows.Forms.Label();
            this.labelMAP = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelType
            // 
            this.labelType.AutoSize = true;
            this.labelType.ForeColor = System.Drawing.SystemColors.Control;
            this.labelType.Location = new System.Drawing.Point(3, 0);
            this.labelType.Name = "labelType";
            this.labelType.Size = new System.Drawing.Size(29, 13);
            this.labelType.TabIndex = 0;
            this.labelType.Text = "label";
            // 
            // labelSBP
            // 
            this.labelSBP.AutoSize = true;
            this.labelSBP.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSBP.ForeColor = System.Drawing.SystemColors.Control;
            this.labelSBP.Location = new System.Drawing.Point(13, 29);
            this.labelSBP.Name = "labelSBP";
            this.labelSBP.Size = new System.Drawing.Size(63, 25);
            this.labelSBP.TabIndex = 0;
            this.labelSBP.Text = "label";
            // 
            // labelDBP
            // 
            this.labelDBP.AutoSize = true;
            this.labelDBP.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDBP.ForeColor = System.Drawing.SystemColors.Control;
            this.labelDBP.Location = new System.Drawing.Point(13, 54);
            this.labelDBP.Name = "labelDBP";
            this.labelDBP.Size = new System.Drawing.Size(63, 25);
            this.labelDBP.TabIndex = 0;
            this.labelDBP.Text = "label";
            // 
            // labelMAP
            // 
            this.labelMAP.AutoSize = true;
            this.labelMAP.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMAP.ForeColor = System.Drawing.SystemColors.Control;
            this.labelMAP.Location = new System.Drawing.Point(13, 79);
            this.labelMAP.Name = "labelMAP";
            this.labelMAP.Size = new System.Drawing.Size(63, 25);
            this.labelMAP.TabIndex = 0;
            this.labelMAP.Text = "label";
            // 
            // Values_BP
            // 
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.labelMAP);
            this.Controls.Add(this.labelDBP);
            this.Controls.Add(this.labelSBP);
            this.Controls.Add(this.labelType);
            this.Name = "Values_BP";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelType;
        private System.Windows.Forms.Label labelSBP;
        private System.Windows.Forms.Label labelDBP;
        private System.Windows.Forms.Label labelMAP;
    }
}
