
using System;

namespace Dalamud.Updater
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.buttonCheckForUpdate = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.buttonCheckRuntime = new System.Windows.Forms.Button();
            this.comboBoxFFXIV = new System.Windows.Forms.ComboBox();
            this.buttonInject = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.checkBoxAcce = new System.Windows.Forms.CheckBox();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.checkBoxAutoInject = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonCheckForUpdate
            // 
            this.buttonCheckForUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCheckForUpdate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCheckForUpdate.Location = new System.Drawing.Point(12, 93);
            this.buttonCheckForUpdate.Name = "buttonCheckForUpdate";
            this.buttonCheckForUpdate.Size = new System.Drawing.Size(196, 40);
            this.buttonCheckForUpdate.TabIndex = 0;
            this.buttonCheckForUpdate.Text = "检查更新";
            this.buttonCheckForUpdate.UseVisualStyleBackColor = true;
            this.buttonCheckForUpdate.Click += new System.EventHandler(this.ButtonCheckForUpdate_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVersion.Location = new System.Drawing.Point(9, 23);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(119, 15);
            this.labelVersion.TabIndex = 1;
            this.labelVersion.Text = "当前版本 : Unknown";
            // 
            // buttonCheckRuntime
            // 
            this.buttonCheckRuntime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCheckRuntime.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCheckRuntime.Location = new System.Drawing.Point(12, 47);
            this.buttonCheckRuntime.Name = "buttonCheckRuntime";
            this.buttonCheckRuntime.Size = new System.Drawing.Size(196, 40);
            this.buttonCheckRuntime.TabIndex = 0;
            this.buttonCheckRuntime.Text = "下载运行库";
            this.buttonCheckRuntime.UseVisualStyleBackColor = true;
            this.buttonCheckRuntime.Click += new System.EventHandler(this.ButtonCheckRuntime_Click);
            // 
            // comboBoxFFXIV
            // 
            this.comboBoxFFXIV.Cursor = System.Windows.Forms.Cursors.Default;
            this.comboBoxFFXIV.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFFXIV.FormattingEnabled = true;
            this.comboBoxFFXIV.ImeMode = System.Windows.Forms.ImeMode.On;
            this.comboBoxFFXIV.Location = new System.Drawing.Point(12, 179);
            this.comboBoxFFXIV.Name = "comboBoxFFXIV";
            this.comboBoxFFXIV.Size = new System.Drawing.Size(196, 23);
            this.comboBoxFFXIV.TabIndex = 2;
            this.comboBoxFFXIV.Click += new System.EventHandler(this.comboBoxFFXIV_Clicked);
            // 
            // buttonInject
            // 
            this.buttonInject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInject.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonInject.Location = new System.Drawing.Point(12, 208);
            this.buttonInject.Name = "buttonInject";
            this.buttonInject.Size = new System.Drawing.Size(196, 89);
            this.buttonInject.TabIndex = 0;
            this.buttonInject.Text = "注入灵魂";
            this.buttonInject.UseVisualStyleBackColor = true;
            this.buttonInject.Click += new System.EventHandler(this.ButtonInject_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(161, 303);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(64, 15);
            this.linkLabel1.TabIndex = 3;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "加入QQ群";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // checkBoxAcce
            // 
            this.checkBoxAcce.AutoSize = true;
            this.checkBoxAcce.Location = new System.Drawing.Point(12, 154);
            this.checkBoxAcce.Name = "checkBoxAcce";
            this.checkBoxAcce.Size = new System.Drawing.Size(78, 19);
            this.checkBoxAcce.TabIndex = 4;
            this.checkBoxAcce.Text = "国际加速";
            this.checkBoxAcce.UseVisualStyleBackColor = true;
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(-2, 303);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(59, 15);
            this.linkLabel2.TabIndex = 3;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "投喂小獭";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // checkBoxAutoInject
            // 
            this.checkBoxAutoInject.AutoSize = true;
            this.checkBoxAutoInject.Location = new System.Drawing.Point(130, 154);
            this.checkBoxAutoInject.Name = "checkBoxAutoInject";
            this.checkBoxAutoInject.Size = new System.Drawing.Size(78, 19);
            this.checkBoxAutoInject.TabIndex = 4;
            this.checkBoxAutoInject.Text = "自动注入";
            this.checkBoxAutoInject.UseVisualStyleBackColor = true;
            this.checkBoxAutoInject.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(220, 315);
            this.Controls.Add(this.checkBoxAutoInject);
            this.Controls.Add(this.checkBoxAcce);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.comboBoxFFXIV);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.buttonInject);
            this.Controls.Add(this.buttonCheckRuntime);
            this.Controls.Add(this.buttonCheckForUpdate);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMain";
            this.Text = "卫月更新器";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Disposed += new System.EventHandler(this.FormMain_Disposed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCheckForUpdate;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Button buttonCheckRuntime;
        private System.Windows.Forms.ComboBox comboBoxFFXIV;
        private System.Windows.Forms.Button buttonInject;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox checkBoxAcce;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.CheckBox checkBoxAutoInject;
    }
}

