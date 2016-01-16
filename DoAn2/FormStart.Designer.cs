namespace DoAn2
{
    partial class FormStart
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
            this.btn_PaintNetwork = new System.Windows.Forms.Button();
            this.btn_NetworkClient = new System.Windows.Forms.Button();
            this.btn_NetworkSerber = new System.Windows.Forms.Button();
            this.btn_About = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_PaintNetwork
            // 
            this.btn_PaintNetwork.BackColor = System.Drawing.Color.Transparent;
            this.btn_PaintNetwork.BackgroundImage = global::DoAn2.Properties.Resources.GO;
            this.btn_PaintNetwork.Font = new System.Drawing.Font("Mistral", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_PaintNetwork.ForeColor = System.Drawing.Color.Yellow;
            this.btn_PaintNetwork.Location = new System.Drawing.Point(565, 12);
            this.btn_PaintNetwork.Name = "btn_PaintNetwork";
            this.btn_PaintNetwork.Size = new System.Drawing.Size(166, 71);
            this.btn_PaintNetwork.TabIndex = 0;
            this.btn_PaintNetwork.Text = "PaintOffline";
            this.btn_PaintNetwork.UseVisualStyleBackColor = false;
            this.btn_PaintNetwork.Click += new System.EventHandler(this.btn_PaintNetwork_Click);
            // 
            // btn_NetworkClient
            // 
            this.btn_NetworkClient.Font = new System.Drawing.Font("Mistral", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_NetworkClient.ForeColor = System.Drawing.Color.Yellow;
            this.btn_NetworkClient.Image = global::DoAn2.Properties.Resources.GO;
            this.btn_NetworkClient.Location = new System.Drawing.Point(565, 89);
            this.btn_NetworkClient.Name = "btn_NetworkClient";
            this.btn_NetworkClient.Size = new System.Drawing.Size(166, 71);
            this.btn_NetworkClient.TabIndex = 1;
            this.btn_NetworkClient.Text = "Client";
            this.btn_NetworkClient.UseVisualStyleBackColor = true;
            this.btn_NetworkClient.Click += new System.EventHandler(this.btn_NetworkClient_Click);
            // 
            // btn_NetworkSerber
            // 
            this.btn_NetworkSerber.Font = new System.Drawing.Font("Mistral", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_NetworkSerber.ForeColor = System.Drawing.Color.Yellow;
            this.btn_NetworkSerber.Image = global::DoAn2.Properties.Resources.GO;
            this.btn_NetworkSerber.Location = new System.Drawing.Point(565, 166);
            this.btn_NetworkSerber.Name = "btn_NetworkSerber";
            this.btn_NetworkSerber.Size = new System.Drawing.Size(166, 71);
            this.btn_NetworkSerber.TabIndex = 2;
            this.btn_NetworkSerber.Text = "Server";
            this.btn_NetworkSerber.UseVisualStyleBackColor = true;
            this.btn_NetworkSerber.Click += new System.EventHandler(this.btn_NetworkSerber_Click);
            // 
            // btn_About
            // 
            this.btn_About.Font = new System.Drawing.Font("Mistral", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_About.ForeColor = System.Drawing.Color.Yellow;
            this.btn_About.Image = global::DoAn2.Properties.Resources.GO;
            this.btn_About.Location = new System.Drawing.Point(565, 255);
            this.btn_About.Name = "btn_About";
            this.btn_About.Size = new System.Drawing.Size(166, 69);
            this.btn_About.TabIndex = 3;
            this.btn_About.Text = "Exit";
            this.btn_About.UseVisualStyleBackColor = true;
            this.btn_About.Click += new System.EventHandler(this.btn_About_Click);
            // 
            // FormStart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::DoAn2.Properties.Resources.backround;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.btn_About);
            this.Controls.Add(this.btn_NetworkSerber);
            this.Controls.Add(this.btn_NetworkClient);
            this.Controls.Add(this.btn_PaintNetwork);
            this.Name = "FormStart";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormStart";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_PaintNetwork;
        private System.Windows.Forms.Button btn_NetworkClient;
        private System.Windows.Forms.Button btn_NetworkSerber;
        private System.Windows.Forms.Button btn_About;
    }
}