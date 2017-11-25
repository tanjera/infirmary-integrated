namespace II.Forms {
    partial class Patient_Vitals {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent () {
            this.comboRhythm = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numHR = new System.Windows.Forms.NumericUpDown();
            this.numSBP = new System.Windows.Forms.NumericUpDown();
            this.numDBP = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.numSpO2 = new System.Windows.Forms.NumericUpDown();
            this.buttonApply = new System.Windows.Forms.Button();
            this.checkNormalRange = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numHR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSBP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDBP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSpO2)).BeginInit();
            this.SuspendLayout();
            // 
            // comboRhythm
            // 
            this.comboRhythm.FormattingEnabled = true;
            this.comboRhythm.Location = new System.Drawing.Point(97, 91);
            this.comboRhythm.Name = "comboRhythm";
            this.comboRhythm.Size = new System.Drawing.Size(193, 21);
            this.comboRhythm.TabIndex = 4;
            this.comboRhythm.Text = "Normal_Sinus";
            this.comboRhythm.SelectedIndexChanged += new System.EventHandler(this.Vitals_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "HR";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "BP";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "SpO2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Rhythm";
            // 
            // numHR
            // 
            this.numHR.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numHR.Location = new System.Drawing.Point(97, 13);
            this.numHR.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numHR.Name = "numHR";
            this.numHR.Size = new System.Drawing.Size(67, 20);
            this.numHR.TabIndex = 7;
            this.numHR.ValueChanged += new System.EventHandler(this.Vitals_ValueChanged);
            // 
            // numSBP
            // 
            this.numSBP.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numSBP.Location = new System.Drawing.Point(97, 39);
            this.numSBP.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numSBP.Name = "numSBP";
            this.numSBP.Size = new System.Drawing.Size(67, 20);
            this.numSBP.TabIndex = 7;
            this.numSBP.ValueChanged += new System.EventHandler(this.Vitals_ValueChanged);
            // 
            // numDBP
            // 
            this.numDBP.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numDBP.Location = new System.Drawing.Point(207, 39);
            this.numDBP.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numDBP.Name = "numDBP";
            this.numDBP.Size = new System.Drawing.Size(67, 20);
            this.numDBP.TabIndex = 7;
            this.numDBP.ValueChanged += new System.EventHandler(this.Vitals_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(180, 41);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(12, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "/";
            // 
            // numSpO2
            // 
            this.numSpO2.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numSpO2.Location = new System.Drawing.Point(97, 65);
            this.numSpO2.Name = "numSpO2";
            this.numSpO2.Size = new System.Drawing.Size(67, 20);
            this.numSpO2.TabIndex = 7;
            this.numSpO2.ValueChanged += new System.EventHandler(this.Vitals_ValueChanged);
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(12, 118);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(276, 24);
            this.buttonApply.TabIndex = 8;
            this.buttonApply.Text = "Apply Changes";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // checkNormalRange
            // 
            this.checkNormalRange.AutoSize = true;
            this.checkNormalRange.Checked = true;
            this.checkNormalRange.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkNormalRange.Location = new System.Drawing.Point(167, 160);
            this.checkNormalRange.Name = "checkNormalRange";
            this.checkNormalRange.Size = new System.Drawing.Size(121, 17);
            this.checkNormalRange.TabIndex = 9;
            this.checkNormalRange.Text = "Use Normal Ranges";
            this.checkNormalRange.UseVisualStyleBackColor = true;
            // 
            // Patient_Vitals
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 184);
            this.Controls.Add(this.checkNormalRange);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.numDBP);
            this.Controls.Add(this.numSpO2);
            this.Controls.Add(this.numSBP);
            this.Controls.Add(this.numHR);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboRhythm);
            this.Name = "Patient_Vitals";
            this.Text = "Patient_Vitals";
            ((System.ComponentModel.ISupportInitialize)(this.numHR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSBP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDBP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSpO2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboRhythm;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numHR;
        private System.Windows.Forms.NumericUpDown numSBP;
        private System.Windows.Forms.NumericUpDown numDBP;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numSpO2;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.CheckBox checkNormalRange;
    }
}