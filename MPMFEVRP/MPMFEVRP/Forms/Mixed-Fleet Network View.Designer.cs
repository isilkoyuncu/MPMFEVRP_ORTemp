namespace MPMFEVRP.Forms
{
    partial class Mixed_Fleet_Network_View
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
            this.panel_Base = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel_Base
            // 
            this.panel_Base.BackColor = System.Drawing.Color.White;
            this.panel_Base.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel_Base.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel_Base.Location = new System.Drawing.Point(13, 13);
            this.panel_Base.Name = "panel_Base";
            this.panel_Base.Padding = new System.Windows.Forms.Padding(0, 0, 4, 4);
            this.panel_Base.Size = new System.Drawing.Size(500, 500);
            this.panel_Base.TabIndex = 0;
            this.panel_Base.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_Base_Paint);
            // 
            // Mixed_Fleet_Network_View
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(825, 540);
            this.Controls.Add(this.panel_Base);
            this.Name = "Mixed_Fleet_Network_View";
            this.Text = "Mixed_Fleet_Network_View";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_Base;
        private System.Drawing.Graphics graphics_Base;

    }
}