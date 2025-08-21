namespace DahuaUICamera
{
    partial class Form2
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
            btnLoadData = new Button();
            listBox1 = new ListBox();
            btnAddAccount = new Button();
            SuspendLayout();
            // 
            // btnLoadData
            // 
            btnLoadData.Location = new Point(12, 157);
            btnLoadData.Name = "btnLoadData";
            btnLoadData.Size = new Size(103, 23);
            btnLoadData.TabIndex = 0;
            btnLoadData.Text = "getAccount";
            btnLoadData.UseVisualStyleBackColor = true;
            btnLoadData.Click += btnLoadData_Click;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(12, 12);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(776, 139);
            listBox1.TabIndex = 1;
            // 
            // btnAddAccount
            // 
            btnAddAccount.Location = new Point(144, 157);
            btnAddAccount.Name = "btnAddAccount";
            btnAddAccount.Size = new Size(103, 23);
            btnAddAccount.TabIndex = 2;
            btnAddAccount.Text = "addAccount";
            btnAddAccount.UseVisualStyleBackColor = true;
            btnAddAccount.Click += btnAddAccount_Click;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnAddAccount);
            Controls.Add(listBox1);
            Controls.Add(btnLoadData);
            Name = "Form2";
            Text = "Form2";
            ResumeLayout(false);
        }

        #endregion

        private Button btnLoadData;
        private ListBox listBox1;
        private Button btnAddAccount;
    }
}