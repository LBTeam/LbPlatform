namespace LBPlayer
{
    partial class ToolTipWindow
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
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.skinLabel_tip = new CCWin.SkinControl.SkinLabel();
            this.SuspendLayout();
            // 
            // skinLabel_tip
            // 
            this.skinLabel_tip.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel_tip.BorderColor = System.Drawing.Color.White;
            this.skinLabel_tip.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel_tip.ForeColor = System.Drawing.Color.Red;
            this.skinLabel_tip.Location = new System.Drawing.Point(0, 5);
            this.skinLabel_tip.Name = "skinLabel_tip";
            this.skinLabel_tip.Size = new System.Drawing.Size(273, 26);
            this.skinLabel_tip.TabIndex = 0;
            this.skinLabel_tip.Text = "您的鼠标键盘已经锁定按ESC解锁！";
            this.skinLabel_tip.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ToolTipWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 39);
            this.ControlBox = false;
            this.Controls.Add(this.skinLabel_tip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ToolTipWindow";
            this.ShowBorder = false;
            this.ShowDrawIcon = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ShowSystemMenu = false;
            this.Text = "";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private CCWin.SkinControl.SkinLabel skinLabel_tip;
    }
}