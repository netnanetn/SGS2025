namespace WinFormsAppTestScale
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
            label1 = new Label();
            label2 = new Label();
            txtCOM = new TextBox();
            txtBaudrate = new TextBox();
            btnOpen = new Button();
            btnClose = new Button();
            label3 = new Label();
            lbWeight = new Label();
            rtxtLog = new RichTextBox();
            btnClear = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(22, 21);
            label1.Name = "label1";
            label1.Size = new Size(53, 15);
            label1.TabIndex = 0;
            label1.Text = "portcom";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(22, 88);
            label2.Name = "label2";
            label2.Size = new Size(54, 15);
            label2.TabIndex = 1;
            label2.Text = "baudrate";
            // 
            // txtCOM
            // 
            txtCOM.Location = new Point(22, 45);
            txtCOM.Name = "txtCOM";
            txtCOM.Size = new Size(190, 23);
            txtCOM.TabIndex = 2;
            txtCOM.Text = "COM2";
            // 
            // txtBaudrate
            // 
            txtBaudrate.Location = new Point(22, 106);
            txtBaudrate.Name = "txtBaudrate";
            txtBaudrate.Size = new Size(190, 23);
            txtBaudrate.TabIndex = 3;
            txtBaudrate.Text = "9600";
            // 
            // btnOpen
            // 
            btnOpen.Location = new Point(22, 158);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(190, 23);
            btnOpen.TabIndex = 4;
            btnOpen.Text = "Open";
            btnOpen.UseVisualStyleBackColor = true;
            btnOpen.Click += btnOpen_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(22, 187);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(190, 23);
            btnClose.TabIndex = 5;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(294, 21);
            label3.Name = "label3";
            label3.Size = new Size(45, 15);
            label3.TabIndex = 6;
            label3.Text = "Weight";
            // 
            // lbWeight
            // 
            lbWeight.AutoSize = true;
            lbWeight.Location = new Point(371, 21);
            lbWeight.Name = "lbWeight";
            lbWeight.Size = new Size(38, 15);
            lbWeight.TabIndex = 7;
            lbWeight.Text = "label4";
            // 
            // rtxtLog
            // 
            rtxtLog.Location = new Point(243, 45);
            rtxtLog.Name = "rtxtLog";
            rtxtLog.Size = new Size(533, 318);
            rtxtLog.TabIndex = 8;
            rtxtLog.Text = "";
            // 
            // btnClear
            // 
            btnClear.Location = new Point(243, 385);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(533, 23);
            btnClear.TabIndex = 9;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnClear);
            Controls.Add(rtxtLog);
            Controls.Add(lbWeight);
            Controls.Add(label3);
            Controls.Add(btnClose);
            Controls.Add(btnOpen);
            Controls.Add(txtBaudrate);
            Controls.Add(txtCOM);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox txtCOM;
        private TextBox txtBaudrate;
        private Button btnOpen;
        private Button btnClose;
        private Label label3;
        private Label lbWeight;
        private RichTextBox rtxtLog;
        private Button btnClear;
    }
}
