namespace MPMFEVRP.Forms
{
    partial class ProblemViewer
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
            this.panel_problemViewer = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel_problemViewer
            // 
            this.panel_problemViewer.BackColor = System.Drawing.Color.White;
            this.panel_problemViewer.Location = new System.Drawing.Point(8, 9);
            this.panel_problemViewer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel_problemViewer.Name = "panel_problemViewer";
            this.panel_problemViewer.Size = new System.Drawing.Size(270, 246);
            this.panel_problemViewer.TabIndex = 0;
            this.panel_problemViewer.Paint += new System.Windows.Forms.PaintEventHandler(this.Panel_Paint);
            // 
            // ProblemViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.panel_problemViewer);
            this.Name = "ProblemViewer";
            this.Text = "ProblemViewer";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_problemViewer;
    }
}