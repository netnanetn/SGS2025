namespace WinFormsAppTest
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
            btnConnect = new Button();
            btnSend = new Button();
            txtDataLog = new TextBox();
            btnCoiHu = new Button();
            btnStopCoiHu = new Button();
            btnCoi = new Button();
            btnDenxanh = new Button();
            btnDendo = new Button();
            lbSensor1 = new Label();
            lbSensor2 = new Label();
            lbSensor3 = new Label();
            SuspendLayout();
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(56, 360);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(75, 23);
            btnConnect.TabIndex = 0;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(647, 40);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(75, 23);
            btnSend.TabIndex = 1;
            btnSend.Text = "get DI Data";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // txtDataLog
            // 
            txtDataLog.Location = new Point(56, 69);
            txtDataLog.Multiline = true;
            txtDataLog.Name = "txtDataLog";
            txtDataLog.Size = new Size(549, 275);
            txtDataLog.TabIndex = 2;
            // 
            // btnCoiHu
            // 
            btnCoiHu.Location = new Point(278, 360);
            btnCoiHu.Name = "btnCoiHu";
            btnCoiHu.Size = new Size(75, 23);
            btnCoiHu.TabIndex = 3;
            btnCoiHu.Text = "Coi Hu";
            btnCoiHu.UseVisualStyleBackColor = true;
            btnCoiHu.Click += btnCoiHu_Click;
            // 
            // btnStopCoiHu
            // 
            btnStopCoiHu.Location = new Point(359, 360);
            btnStopCoiHu.Name = "btnStopCoiHu";
            btnStopCoiHu.Size = new Size(75, 23);
            btnStopCoiHu.TabIndex = 4;
            btnStopCoiHu.Text = "tat Coi Hu";
            btnStopCoiHu.UseVisualStyleBackColor = true;
            btnStopCoiHu.Click += btnStopCoiHu_Click;
            // 
            // btnCoi
            // 
            btnCoi.Location = new Point(647, 171);
            btnCoi.Name = "btnCoi";
            btnCoi.Size = new Size(75, 23);
            btnCoi.TabIndex = 5;
            btnCoi.Text = "Coi";
            btnCoi.UseVisualStyleBackColor = true;
            btnCoi.Click += btnCoi_Click;
            // 
            // btnDenxanh
            // 
            btnDenxanh.Location = new Point(647, 230);
            btnDenxanh.Name = "btnDenxanh";
            btnDenxanh.Size = new Size(75, 23);
            btnDenxanh.TabIndex = 6;
            btnDenxanh.Text = "Đèn xanh";
            btnDenxanh.UseVisualStyleBackColor = true;
            btnDenxanh.Click += btnDenxanh_Click;
            // 
            // btnDendo
            // 
            btnDendo.Location = new Point(647, 282);
            btnDendo.Name = "btnDendo";
            btnDendo.Size = new Size(75, 23);
            btnDendo.TabIndex = 7;
            btnDendo.Text = "Đèn đỏ";
            btnDendo.UseVisualStyleBackColor = true;
            btnDendo.Click += btnDendo_Click;
            // 
            // lbSensor1
            // 
            lbSensor1.AutoSize = true;
            lbSensor1.Location = new Point(655, 81);
            lbSensor1.Name = "lbSensor1";
            lbSensor1.Size = new Size(67, 15);
            lbSensor1.TabIndex = 8;
            lbSensor1.Text = "Cảm biến 1";
            // 
            // lbSensor2
            // 
            lbSensor2.AutoSize = true;
            lbSensor2.Location = new Point(655, 116);
            lbSensor2.Name = "lbSensor2";
            lbSensor2.Size = new Size(67, 15);
            lbSensor2.TabIndex = 9;
            lbSensor2.Text = "Cảm biến 2";
            // 
            // lbSensor3
            // 
            lbSensor3.AutoSize = true;
            lbSensor3.Location = new Point(655, 143);
            lbSensor3.Name = "lbSensor3";
            lbSensor3.Size = new Size(67, 15);
            lbSensor3.TabIndex = 10;
            lbSensor3.Text = "Cảm biến 3";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lbSensor3);
            Controls.Add(lbSensor2);
            Controls.Add(lbSensor1);
            Controls.Add(btnDendo);
            Controls.Add(btnDenxanh);
            Controls.Add(btnCoi);
            Controls.Add(btnStopCoiHu);
            Controls.Add(btnCoiHu);
            Controls.Add(txtDataLog);
            Controls.Add(btnSend);
            Controls.Add(btnConnect);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnConnect;
        private Button btnSend;
        private TextBox txtDataLog;
        private Button btnCoiHu;
        private Button btnStopCoiHu;
        private Button btnCoi;
        private Button btnDenxanh;
        private Button btnDendo;
        private Label lbSensor1;
        private Label lbSensor2;
        private Label lbSensor3;
    }
}
