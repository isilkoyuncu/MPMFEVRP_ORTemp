namespace EVvsGDVFrontierAnalyzer
{
    partial class Form1
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
            this.button_GenerateAllGDVFeasibleCustomerSets = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_GenerateAllGDVFeasibleCustomerSets
            // 
            this.button_GenerateAllGDVFeasibleCustomerSets.Location = new System.Drawing.Point(14, 15);
            this.button_GenerateAllGDVFeasibleCustomerSets.Name = "button_GenerateAllGDVFeasibleCustomerSets";
            this.button_GenerateAllGDVFeasibleCustomerSets.Size = new System.Drawing.Size(212, 23);
            this.button_GenerateAllGDVFeasibleCustomerSets.TabIndex = 0;
            this.button_GenerateAllGDVFeasibleCustomerSets.Text = "Generate All GDV-Feasible Customer Sets";
            this.button_GenerateAllGDVFeasibleCustomerSets.UseVisualStyleBackColor = true;
            this.button_GenerateAllGDVFeasibleCustomerSets.Click += new System.EventHandler(this.button_GenerateAllGDVFeasibleCustomerSets_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.button_GenerateAllGDVFeasibleCustomerSets);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_GenerateAllGDVFeasibleCustomerSets;
    }
}

