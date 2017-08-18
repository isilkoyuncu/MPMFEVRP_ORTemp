namespace MPMFEVRP.Forms
{
    partial class AlgorithmViewer
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
            this.label_name = new System.Windows.Forms.Label();
            this.panel_params = new System.Windows.Forms.Panel();
            this.button_Close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_name
            // 
            this.label_name.AutoSize = true;
            this.label_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_name.Location = new System.Drawing.Point(13, 13);
            this.label_name.Name = "label_name";
            this.label_name.Size = new System.Drawing.Size(47, 15);
            this.label_name.TabIndex = 0;
            this.label_name.Text = "label1";
            // 
            // panel_params
            // 
            this.panel_params.Location = new System.Drawing.Point(13, 39);
            this.panel_params.Name = "panel_params";
            this.panel_params.Size = new System.Drawing.Size(257, 323);
            this.panel_params.TabIndex = 1;
            // 
            // button_Close
            // 
            this.button_Close.Location = new System.Drawing.Point(13, 368);
            this.button_Close.Name = "button_Close";
            this.button_Close.Size = new System.Drawing.Size(257, 23);
            this.button_Close.TabIndex = 2;
            this.button_Close.Text = "Close";
            this.button_Close.UseVisualStyleBackColor = true;
            this.button_Close.Click += new System.EventHandler(this.Button_Close_Click);
            // 
            // AlgorithmViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 403);
            this.Controls.Add(this.button_Close);
            this.Controls.Add(this.panel_params);
            this.Controls.Add(this.label_name);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "AlgorithmViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AlgorithmViewer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_name;
        private System.Windows.Forms.Panel panel_params;
        private System.Windows.Forms.Button button_Close;
    }
}