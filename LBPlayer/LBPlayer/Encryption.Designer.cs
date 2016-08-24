namespace LBPlayer
{
    partial class Encryption
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
            this.components = new System.ComponentModel.Container();
            this.skinButton_OK = new CCWin.SkinControl.SkinButton();
            this.skinButton_Cancel = new CCWin.SkinControl.SkinButton();
            this.textBox_Password = new CCWin.SkinControl.SkinTextBox();
            this.skinLabel1 = new CCWin.SkinControl.SkinLabel();
            this.label_PassWordError = new CCWin.SkinControl.SkinLabel();
            this.SuspendLayout();
            // 
            // skinButton_OK
            // 
            this.skinButton_OK.BackColor = System.Drawing.Color.Transparent;
            this.skinButton_OK.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.skinButton_OK.DownBack = null;
            this.skinButton_OK.Location = new System.Drawing.Point(36, 194);
            this.skinButton_OK.MouseBack = null;
            this.skinButton_OK.Name = "skinButton_OK";
            this.skinButton_OK.NormlBack = null;
            this.skinButton_OK.Size = new System.Drawing.Size(75, 23);
            this.skinButton_OK.TabIndex = 0;
            this.skinButton_OK.Text = "确定";
            this.skinButton_OK.UseVisualStyleBackColor = false;
            this.skinButton_OK.Visible = false;
            this.skinButton_OK.Click += new System.EventHandler(this.skinButton_OK_Click);
            // 
            // skinButton_Cancel
            // 
            this.skinButton_Cancel.BackColor = System.Drawing.Color.Transparent;
            this.skinButton_Cancel.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.skinButton_Cancel.DownBack = null;
            this.skinButton_Cancel.Location = new System.Drawing.Point(193, 194);
            this.skinButton_Cancel.MouseBack = null;
            this.skinButton_Cancel.Name = "skinButton_Cancel";
            this.skinButton_Cancel.NormlBack = null;
            this.skinButton_Cancel.Size = new System.Drawing.Size(75, 23);
            this.skinButton_Cancel.TabIndex = 0;
            this.skinButton_Cancel.Text = "取消";
            this.skinButton_Cancel.UseVisualStyleBackColor = false;
            this.skinButton_Cancel.Visible = false;
            // 
            // textBox_Password
            // 
            this.textBox_Password.BackColor = System.Drawing.Color.Transparent;
            this.textBox_Password.DownBack = null;
            this.textBox_Password.Icon = null;
            this.textBox_Password.IconIsButton = false;
            this.textBox_Password.IconMouseState = CCWin.SkinClass.ControlState.Normal;
            this.textBox_Password.IsPasswordChat = '*';
            this.textBox_Password.IsSystemPasswordChar = false;
            this.textBox_Password.Lines = new string[0];
            this.textBox_Password.Location = new System.Drawing.Point(83, 76);
            this.textBox_Password.Margin = new System.Windows.Forms.Padding(0);
            this.textBox_Password.MaxLength = 32767;
            this.textBox_Password.MinimumSize = new System.Drawing.Size(28, 28);
            this.textBox_Password.MouseBack = null;
            this.textBox_Password.MouseState = CCWin.SkinClass.ControlState.Normal;
            this.textBox_Password.Multiline = false;
            this.textBox_Password.Name = "textBox_Password";
            this.textBox_Password.NormlBack = null;
            this.textBox_Password.Padding = new System.Windows.Forms.Padding(5);
            this.textBox_Password.ReadOnly = false;
            this.textBox_Password.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textBox_Password.Size = new System.Drawing.Size(185, 28);
            // 
            // 
            // 
            this.textBox_Password.SkinTxt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_Password.SkinTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_Password.SkinTxt.Font = new System.Drawing.Font("微软雅黑", 9.75F);
            this.textBox_Password.SkinTxt.Location = new System.Drawing.Point(5, 5);
            this.textBox_Password.SkinTxt.Name = "BaseText";
            this.textBox_Password.SkinTxt.PasswordChar = '*';
            this.textBox_Password.SkinTxt.Size = new System.Drawing.Size(175, 18);
            this.textBox_Password.SkinTxt.TabIndex = 0;
            this.textBox_Password.SkinTxt.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.textBox_Password.SkinTxt.WaterText = "";
            this.textBox_Password.TabIndex = 1;
            this.textBox_Password.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.textBox_Password.WaterColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.textBox_Password.WaterText = "";
            this.textBox_Password.WordWrap = true;
            // 
            // skinLabel1
            // 
            this.skinLabel1.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel1.BorderColor = System.Drawing.Color.White;
            this.skinLabel1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel1.Location = new System.Drawing.Point(11, 81);
            this.skinLabel1.Name = "skinLabel1";
            this.skinLabel1.Size = new System.Drawing.Size(69, 23);
            this.skinLabel1.TabIndex = 2;
            this.skinLabel1.Text = "密码";
            this.skinLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_PassWordError
            // 
            this.label_PassWordError.BackColor = System.Drawing.Color.Transparent;
            this.label_PassWordError.BorderColor = System.Drawing.Color.White;
            this.label_PassWordError.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_PassWordError.ForeColor = System.Drawing.Color.Red;
            this.label_PassWordError.Location = new System.Drawing.Point(80, 113);
            this.label_PassWordError.Name = "label_PassWordError";
            this.label_PassWordError.Size = new System.Drawing.Size(69, 23);
            this.label_PassWordError.TabIndex = 2;
            this.label_PassWordError.Text = "密码错误";
            this.label_PassWordError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_PassWordError.Visible = false;
            // 
            // Encryption
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 163);
            this.ControlBox = false;
            this.Controls.Add(this.label_PassWordError);
            this.Controls.Add(this.skinLabel1);
            this.Controls.Add(this.textBox_Password);
            this.Controls.Add(this.skinButton_Cancel);
            this.Controls.Add(this.skinButton_OK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Encryption";
            this.ShowBorder = false;
            this.ShowDrawIcon = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ShowSystemMenu = false;
            this.Text = "";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinButton skinButton_OK;
        private CCWin.SkinControl.SkinButton skinButton_Cancel;
        private CCWin.SkinControl.SkinTextBox textBox_Password;
        private CCWin.SkinControl.SkinLabel skinLabel1;
        private CCWin.SkinControl.SkinLabel label_PassWordError;
    }
}