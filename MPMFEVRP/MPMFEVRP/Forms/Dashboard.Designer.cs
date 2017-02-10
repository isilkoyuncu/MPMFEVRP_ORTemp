namespace MPMFEVRP.Forms
{
    partial class Dashboard
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
            this.Button_single = new System.Windows.Forms.Button();
            this.Button_multiple = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Button_single
            // 
            this.Button_single.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Button_single.Image = global::MPMFEVRP.Properties.Resources.single;
            this.Button_single.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Button_single.Location = new System.Drawing.Point(12, 110);
            this.Button_single.Name = "Button_single";
            this.Button_single.Padding = new System.Windows.Forms.Padding(35, 0, 0, 0);
            this.Button_single.Size = new System.Drawing.Size(260, 92);
            this.Button_single.TabIndex = 0;
            this.Button_single.Text = "Single Instance\r\nby Single Algorithm";
            this.Button_single.UseVisualStyleBackColor = true;
            this.Button_single.Click += new System.EventHandler(this.Button_single_Click);
            // 
            // Button_multiple
            // 
            this.Button_multiple.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Button_multiple.Image = global::MPMFEVRP.Properties.Resources.multi;
            this.Button_multiple.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Button_multiple.Location = new System.Drawing.Point(12, 208);
            this.Button_multiple.Name = "Button_multiple";
            this.Button_multiple.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.Button_multiple.Size = new System.Drawing.Size(260, 92);
            this.Button_multiple.TabIndex = 1;
            this.Button_multiple.Text = "Multiple Instances\r\nby Multiple Algorithms";
            this.Button_multiple.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Button_multiple.UseVisualStyleBackColor = true;
            this.Button_multiple.Click += new System.EventHandler(this.Button_multiple_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Image = global::MPMFEVRP.Properties.Resources.data;
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Padding = new System.Windows.Forms.Padding(35, 0, 0, 0);
            this.button1.Size = new System.Drawing.Size(260, 92);
            this.button1.TabIndex = 2;
            this.button1.Text = "Data Manager\r\n";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Dashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 312);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Button_multiple);
            this.Controls.Add(this.Button_single);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "Dashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dashboard";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Button_single;
        private System.Windows.Forms.Button Button_multiple;
        private System.Windows.Forms.Button button1;
    }
}