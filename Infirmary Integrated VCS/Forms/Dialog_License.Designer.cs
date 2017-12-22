namespace II.Forms
{
    partial class Dialog_License
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing)
        {
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
        private void InitializeComponent ()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Dialog_License));
            this.label1 = new System.Windows.Forms.Label();
            this.textLicense = new System.Windows.Forms.TextBox();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.buttonDecline = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(456, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "Please read and accept the following end-user license agreement to use Infirmary " +
    "Integrated:";
            // 
            // textLicense
            // 
            this.textLicense.Location = new System.Drawing.Point(15, 27);
            this.textLicense.Multiline = true;
            this.textLicense.Name = "textLicense";
            this.textLicense.ReadOnly = true;
            this.textLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textLicense.Size = new System.Drawing.Size(453, 221);
            this.textLicense.TabIndex = 1;
            this.textLicense.TabStop = false;
            this.textLicense.Text = resources.GetString("textLicense.Text");
            // 
            // buttonAccept
            // 
            this.buttonAccept.Location = new System.Drawing.Point(361, 254);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(107, 23);
            this.buttonAccept.TabIndex = 2;
            this.buttonAccept.Text = "Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.ButtonAccept_Click);
            // 
            // buttonDecline
            // 
            this.buttonDecline.Location = new System.Drawing.Point(15, 253);
            this.buttonDecline.Name = "buttonDecline";
            this.buttonDecline.Size = new System.Drawing.Size(107, 23);
            this.buttonDecline.TabIndex = 2;
            this.buttonDecline.Text = "Decline and Exit";
            this.buttonDecline.UseVisualStyleBackColor = true;
            this.buttonDecline.Click += new System.EventHandler(this.ButtonDecline_Click);
            // 
            // Dialog_License
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 288);
            this.Controls.Add(this.buttonDecline);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.textLicense);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Dialog_License";
            this.Text = "Infirmary Integrated: End-User License Agreement";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textLicense;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Button buttonDecline;
    }
}