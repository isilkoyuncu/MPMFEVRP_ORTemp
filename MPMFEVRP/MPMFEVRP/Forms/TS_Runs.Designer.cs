namespace MPMFEVRP.Forms
{
    partial class TS_Runs
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
            this.label4 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.button_solveEMHwithNDF = new System.Windows.Forms.Button();
            this.button_solveYCwithNDF = new System.Windows.Forms.Button();
            this.button_solveEMHwithADF = new System.Windows.Forms.Button();
            this.button_solveYCwithADF = new System.Windows.Forms.Button();
            this.textBox_runTime = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBox_cplexLog2File = new System.Windows.Forms.CheckBox();
            this.textBox_log = new System.Windows.Forms.TextBox();
            this.label_lambda = new System.Windows.Forms.Label();
            this.textBox_lambda = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(167, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "1- Full Blown Comparison";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(51, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(650, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "In this experiment, we will read EMH and YC instances and then solve them with AD" +
    "F and NDF models.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(95, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(557, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Min VMT, Number of EVs = 6, Number of GDVs = 0, Lambda = 1, CPU Time = 3600 sec";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(69, 73);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(321, 17);
            this.label4.TabIndex = 3;
            this.label4.Text = "Create Problem Model of EMH with Min Total VMT";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(69, 133);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(308, 17);
            this.label9.TabIndex = 7;
            this.label9.Text = "Create Problem Model of YC with Min Total Cost";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(95, 162);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(556, 17);
            this.label10.TabIndex = 6;
            this.label10.Text = "Min Cost, Number of EVs = 7, Number of GDVs = 7, Lambda = 1, CPU Time = 3600 sec\r" +
    "\n";
            // 
            // button_solveEMHwithNDF
            // 
            this.button_solveEMHwithNDF.Location = new System.Drawing.Point(691, 86);
            this.button_solveEMHwithNDF.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_solveEMHwithNDF.Name = "button_solveEMHwithNDF";
            this.button_solveEMHwithNDF.Size = new System.Drawing.Size(175, 28);
            this.button_solveEMHwithNDF.TabIndex = 8;
            this.button_solveEMHwithNDF.Text = "Solve EMH with NDF";
            this.button_solveEMHwithNDF.UseVisualStyleBackColor = true;
            this.button_solveEMHwithNDF.Click += new System.EventHandler(this.Button_solveEMHwithNDF_FF_Click);
            // 
            // button_solveYCwithNDF
            // 
            this.button_solveYCwithNDF.Location = new System.Drawing.Point(691, 150);
            this.button_solveYCwithNDF.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_solveYCwithNDF.Name = "button_solveYCwithNDF";
            this.button_solveYCwithNDF.Size = new System.Drawing.Size(175, 28);
            this.button_solveYCwithNDF.TabIndex = 9;
            this.button_solveYCwithNDF.Text = "Solve YC with NDF";
            this.button_solveYCwithNDF.UseVisualStyleBackColor = true;
            this.button_solveYCwithNDF.Click += new System.EventHandler(this.Button_solveYCwithNDF_FF_Click);
            // 
            // button_solveEMHwithADF
            // 
            this.button_solveEMHwithADF.Location = new System.Drawing.Point(927, 86);
            this.button_solveEMHwithADF.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_solveEMHwithADF.Name = "button_solveEMHwithADF";
            this.button_solveEMHwithADF.Size = new System.Drawing.Size(175, 28);
            this.button_solveEMHwithADF.TabIndex = 10;
            this.button_solveEMHwithADF.Text = "Solve EMH with ADF";
            this.button_solveEMHwithADF.UseVisualStyleBackColor = true;
            this.button_solveEMHwithADF.Click += new System.EventHandler(this.Button_solveEMHwithADF_FF_Click);
            // 
            // button_solveYCwithADF
            // 
            this.button_solveYCwithADF.Location = new System.Drawing.Point(927, 150);
            this.button_solveYCwithADF.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_solveYCwithADF.Name = "button_solveYCwithADF";
            this.button_solveYCwithADF.Size = new System.Drawing.Size(175, 28);
            this.button_solveYCwithADF.TabIndex = 11;
            this.button_solveYCwithADF.Text = "Solve YC with NDF";
            this.button_solveYCwithADF.UseVisualStyleBackColor = true;
            this.button_solveYCwithADF.Click += new System.EventHandler(this.Button_solveYCwithADF_FF_Click);
            // 
            // textBox_runTime
            // 
            this.textBox_runTime.Location = new System.Drawing.Point(1145, 43);
            this.textBox_runTime.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_runTime.Name = "textBox_runTime";
            this.textBox_runTime.Size = new System.Drawing.Size(104, 22);
            this.textBox_runTime.TabIndex = 12;
            this.textBox_runTime.Text = "3600";
            this.textBox_runTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1163, 25);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 17);
            this.label5.TabIndex = 13;
            this.label5.Text = "Run Time";
            // 
            // checkBox_cplexLog2File
            // 
            this.checkBox_cplexLog2File.AutoSize = true;
            this.checkBox_cplexLog2File.Checked = true;
            this.checkBox_cplexLog2File.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_cplexLog2File.Location = new System.Drawing.Point(1268, 46);
            this.checkBox_cplexLog2File.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox_cplexLog2File.Name = "checkBox_cplexLog2File";
            this.checkBox_cplexLog2File.Size = new System.Drawing.Size(130, 21);
            this.checkBox_cplexLog2File.TabIndex = 14;
            this.checkBox_cplexLog2File.Text = "Cplex Log 2 File";
            this.checkBox_cplexLog2File.UseVisualStyleBackColor = true;
            // 
            // textBox_log
            // 
            this.textBox_log.Location = new System.Drawing.Point(1140, 86);
            this.textBox_log.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_log.Multiline = true;
            this.textBox_log.Name = "textBox_log";
            this.textBox_log.Size = new System.Drawing.Size(374, 514);
            this.textBox_log.TabIndex = 15;
            // 
            // label_lambda
            // 
            this.label_lambda.AutoSize = true;
            this.label_lambda.Location = new System.Drawing.Point(1428, 25);
            this.label_lambda.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_lambda.Name = "label_lambda";
            this.label_lambda.Size = new System.Drawing.Size(59, 17);
            this.label_lambda.TabIndex = 17;
            this.label_lambda.Text = "Lambda";
            // 
            // textBox_lambda
            // 
            this.textBox_lambda.Location = new System.Drawing.Point(1410, 43);
            this.textBox_lambda.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_lambda.Name = "textBox_lambda";
            this.textBox_lambda.Size = new System.Drawing.Size(104, 22);
            this.textBox_lambda.TabIndex = 16;
            this.textBox_lambda.Text = "1";
            this.textBox_lambda.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TS_Runs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1538, 615);
            this.Controls.Add(this.label_lambda);
            this.Controls.Add(this.textBox_lambda);
            this.Controls.Add(this.textBox_log);
            this.Controls.Add(this.checkBox_cplexLog2File);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_runTime);
            this.Controls.Add(this.button_solveYCwithADF);
            this.Controls.Add(this.button_solveEMHwithADF);
            this.Controls.Add(this.button_solveYCwithNDF);
            this.Controls.Add(this.button_solveEMHwithNDF);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "TS_Runs";
            this.Text = "TS_Runs";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button_solveEMHwithNDF;
        private System.Windows.Forms.Button button_solveYCwithNDF;
        private System.Windows.Forms.Button button_solveEMHwithADF;
        private System.Windows.Forms.Button button_solveYCwithADF;
        private System.Windows.Forms.TextBox textBox_runTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBox_cplexLog2File;
        private System.Windows.Forms.TextBox textBox_log;
        private System.Windows.Forms.Label label_lambda;
        private System.Windows.Forms.TextBox textBox_lambda;
    }
}