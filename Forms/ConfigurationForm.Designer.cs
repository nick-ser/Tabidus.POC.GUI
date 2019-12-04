namespace Tabidus.POC.GUI.Forms
{
    partial class ConfigurationForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numServerPort = new System.Windows.Forms.NumericUpDown();
            this.txtServerKey = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnTestConfiguration = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtServerAddress = new System.Windows.Forms.TextBox();
            this.lblConnecting = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numServerPort)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(151, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Configuration";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Server port:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Server key:";
            // 
            // numServerPort
            // 
            this.numServerPort.Location = new System.Drawing.Point(100, 81);
            this.numServerPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numServerPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numServerPort.Name = "numServerPort";
            this.numServerPort.Size = new System.Drawing.Size(79, 20);
            this.numServerPort.TabIndex = 1;
            this.numServerPort.Value = new decimal(new int[] {
            9002,
            0,
            0,
            0});
            // 
            // txtServerKey
            // 
            this.txtServerKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServerKey.Location = new System.Drawing.Point(100, 122);
            this.txtServerKey.Name = "txtServerKey";
            this.txtServerKey.Size = new System.Drawing.Size(285, 20);
            this.txtServerKey.TabIndex = 2;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(310, 166);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnTestConfiguration
            // 
            this.btnTestConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTestConfiguration.Location = new System.Drawing.Point(190, 166);
            this.btnTestConfiguration.Name = "btnTestConfiguration";
            this.btnTestConfiguration.Size = new System.Drawing.Size(114, 23);
            this.btnTestConfiguration.TabIndex = 3;
            this.btnTestConfiguration.Text = "Test configuration";
            this.btnTestConfiguration.UseVisualStyleBackColor = true;
            this.btnTestConfiguration.Click += new System.EventHandler(this.btnTestConfiguration_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 47);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Server address:";
            // 
            // txtServerAddress
            // 
            this.txtServerAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServerAddress.Location = new System.Drawing.Point(100, 44);
            this.txtServerAddress.Name = "txtServerAddress";
            this.txtServerAddress.Size = new System.Drawing.Size(285, 20);
            this.txtServerAddress.TabIndex = 0;
            this.txtServerAddress.Text = "192.168.0.247";
            this.txtServerAddress.TextChanged += new System.EventHandler(this.txtServerAddress_TextChanged);
            // 
            // lblConnecting
            // 
            this.lblConnecting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblConnecting.AutoSize = true;
            this.lblConnecting.Location = new System.Drawing.Point(114, 171);
            this.lblConnecting.Name = "lblConnecting";
            this.lblConnecting.Size = new System.Drawing.Size(70, 13);
            this.lblConnecting.TabIndex = 6;
            this.lblConnecting.Text = "Please wait...";
            this.lblConnecting.Visible = false;
            // 
            // ConfigurationForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 204);
            this.Controls.Add(this.lblConnecting);
            this.Controls.Add(this.btnTestConfiguration);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtServerAddress);
            this.Controls.Add(this.txtServerKey);
            this.Controls.Add(this.numServerPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigurationForm";
            this.ShowIcon = false;
            this.Text = "Configuration Form";
            this.Load += new System.EventHandler(this.ConfigurationForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numServerPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numServerPort;
        private System.Windows.Forms.TextBox txtServerKey;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnTestConfiguration;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtServerAddress;
        private System.Windows.Forms.Label lblConnecting;
    }
}