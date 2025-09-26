namespace ScaleKey
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnCreateKey = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            txtCustomer = new TextBox();
            txtHardwareId = new TextBox();
            txtDay = new TextBox();
            SuspendLayout();
            // 
            // btnCreateKey
            // 
            btnCreateKey.Location = new Point(167, 192);
            btnCreateKey.Name = "btnCreateKey";
            btnCreateKey.Size = new Size(298, 23);
            btnCreateKey.TabIndex = 0;
            btnCreateKey.Text = "CreateKey";
            btnCreateKey.UseVisualStyleBackColor = true;
            btnCreateKey.Click += btnCreateKey_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(90, 58);
            label1.Name = "label1";
            label1.Size = new Size(59, 15);
            label1.TabIndex = 1;
            label1.Text = "Customer";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(90, 97);
            label2.Name = "label2";
            label2.Size = new Size(71, 15);
            label2.TabIndex = 2;
            label2.Text = "HardwareId ";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(90, 137);
            label3.Name = "label3";
            label3.Size = new Size(57, 15);
            label3.TabIndex = 3;
            label3.Text = "DayAllow";
            // 
            // txtCustomer
            // 
            txtCustomer.Location = new Point(167, 55);
            txtCustomer.Name = "txtCustomer";
            txtCustomer.Size = new Size(298, 23);
            txtCustomer.TabIndex = 4;
            // 
            // txtHardwareId
            // 
            txtHardwareId.Location = new Point(167, 94);
            txtHardwareId.Name = "txtHardwareId";
            txtHardwareId.Size = new Size(298, 23);
            txtHardwareId.TabIndex = 5;
            // 
            // txtDay
            // 
            txtDay.Location = new Point(167, 137);
            txtDay.Name = "txtDay";
            txtDay.Size = new Size(298, 23);
            txtDay.TabIndex = 6;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(566, 450);
            Controls.Add(txtDay);
            Controls.Add(txtHardwareId);
            Controls.Add(txtCustomer);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnCreateKey);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnCreateKey;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox txtCustomer;
        private TextBox txtHardwareId;
        private TextBox txtDay;
    }
}
