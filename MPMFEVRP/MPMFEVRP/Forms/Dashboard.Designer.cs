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
            this.button_DataManager = new System.Windows.Forms.Button();
            this.Button_TSRuns = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Button_single
            // 
            this.Button_single.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Button_single.Image = global::MPMFEVRP.Properties.Resources.single;
            this.Button_single.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Button_single.Location = new System.Drawing.Point(16, 135);
            this.Button_single.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Button_single.Name = "Button_single";
            this.Button_single.Padding = new System.Windows.Forms.Padding(47, 0, 0, 0);
            this.Button_single.Size = new System.Drawing.Size(347, 113);
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
            this.Button_multiple.Location = new System.Drawing.Point(16, 256);
            this.Button_multiple.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Button_multiple.Name = "Button_multiple";
            this.Button_multiple.Padding = new System.Windows.Forms.Padding(27, 0, 0, 0);
            this.Button_multiple.Size = new System.Drawing.Size(347, 113);
            this.Button_multiple.TabIndex = 1;
            this.Button_multiple.Text = "Multiple Instances\r\nby Multiple Algorithms";
            this.Button_multiple.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Button_multiple.UseVisualStyleBackColor = true;
            this.Button_multiple.Click += new System.EventHandler(this.Button_multiple_Click);
            // 
            // button_DataManager
            // 
            this.button_DataManager.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_DataManager.Image = global::MPMFEVRP.Properties.Resources.data;
            this.button_DataManager.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button_DataManager.Location = new System.Drawing.Point(16, 15);
            this.button_DataManager.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_DataManager.Name = "button_DataManager";
            this.button_DataManager.Padding = new System.Windows.Forms.Padding(47, 0, 0, 0);
            this.button_DataManager.Size = new System.Drawing.Size(347, 113);
            this.button_DataManager.TabIndex = 2;
            this.button_DataManager.Text = "Data Manager\r\n";
            this.button_DataManager.UseVisualStyleBackColor = true;
            this.button_DataManager.Click += new System.EventHandler(this.Button_DataManager_Click);
            // 
            // Button_TSRuns
            // 
            this.Button_TSRuns.Location = new System.Drawing.Point(31, 430);
            this.Button_TSRuns.Name = "Button_TSRuns";
            this.Button_TSRuns.Size = new System.Drawing.Size(331, 43);
            this.Button_TSRuns.TabIndex = 3;
            this.Button_TSRuns.Text = "Transportation Science Paper Runs";
            this.Button_TSRuns.UseVisualStyleBackColor = true;
            this.Button_TSRuns.Click += new System.EventHandler(this.Button_TSRuns_Click);
            // 
            // Dashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 556);
            this.Controls.Add(this.Button_TSRuns);
            this.Controls.Add(this.button_DataManager);
            this.Controls.Add(this.Button_multiple);
            this.Controls.Add(this.Button_single);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "Dashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dashboard";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Button_single;
        private System.Windows.Forms.Button Button_multiple;
        private System.Windows.Forms.Button button_DataManager;
        private System.Windows.Forms.Button Button_TSRuns;
    }
}