namespace DahuaUICamera
{
    partial class Form3
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
            cameraPreview1 = new PictureBox();
            StatusCamera1 = new Label();
            ((System.ComponentModel.ISupportInitialize)cameraPreview1).BeginInit();
            SuspendLayout();
            // 
            // cameraPreview1
            // 
            cameraPreview1.Location = new Point(138, 35);
            cameraPreview1.Name = "cameraPreview1";
            cameraPreview1.Size = new Size(482, 251);
            cameraPreview1.TabIndex = 0;
            cameraPreview1.TabStop = false;
            // 
            // StatusCamera1
            // 
            StatusCamera1.AutoSize = true;
            StatusCamera1.Location = new Point(334, 323);
            StatusCamera1.Name = "StatusCamera1";
            StatusCamera1.Size = new Size(38, 15);
            StatusCamera1.TabIndex = 1;
            StatusCamera1.Text = "label1";
            // 
            // Form3
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(StatusCamera1);
            Controls.Add(cameraPreview1);
            Name = "Form3";
            Text = "Form3";
            ((System.ComponentModel.ISupportInitialize)cameraPreview1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox cameraPreview1;
        private Label StatusCamera1;
    }
}