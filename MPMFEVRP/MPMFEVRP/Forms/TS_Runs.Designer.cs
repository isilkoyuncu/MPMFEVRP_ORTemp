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
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.button_solve_EMH_NDF_VF = new System.Windows.Forms.Button();
            this.button_solve_EMH_ADF_VF = new System.Windows.Forms.Button();
            this.button_solve_EMH_ADF_VP = new System.Windows.Forms.Button();
            this.button_solve_EMH_NDF_VP = new System.Windows.Forms.Button();
            this.button_solve_YC_ADF_VP = new System.Windows.Forms.Button();
            this.button_solve_YC_NDF_VP = new System.Windows.Forms.Button();
            this.button_solve_YC_ADF_VF = new System.Windows.Forms.Button();
            this.button_solve_YC_NDF_VF = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "1- Full Blown Comparison";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(38, 42);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(493, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "In this experiment, we will read EMH and YC instances and then solve them with AD" +
    "F and NDF models.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(71, 80);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(355, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Min VMT, Number of EVs = 6, Number of GDVs = 0, Refueling Policy = FF";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(52, 59);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(245, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Create Problem Model of EMH with Min Total VMT";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(52, 108);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(233, 13);
            this.label9.TabIndex = 7;
            this.label9.Text = "Create Problem Model of YC with Min Total Cost";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(71, 132);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(353, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "Min Cost, Number of EVs = 7, Number of GDVs = 7, Refueling Policy = FF";
            // 
            // button_solveEMHwithNDF
            // 
            this.button_solveEMHwithNDF.Location = new System.Drawing.Point(496, 70);
            this.button_solveEMHwithNDF.Name = "button_solveEMHwithNDF";
            this.button_solveEMHwithNDF.Size = new System.Drawing.Size(153, 23);
            this.button_solveEMHwithNDF.TabIndex = 8;
            this.button_solveEMHwithNDF.Text = "Solve EMH - FF with NDF";
            this.button_solveEMHwithNDF.UseVisualStyleBackColor = true;
            this.button_solveEMHwithNDF.Click += new System.EventHandler(this.Button_solveEMHwithNDF_FF_Click);
            // 
            // button_solveYCwithNDF
            // 
            this.button_solveYCwithNDF.Location = new System.Drawing.Point(496, 122);
            this.button_solveYCwithNDF.Name = "button_solveYCwithNDF";
            this.button_solveYCwithNDF.Size = new System.Drawing.Size(153, 23);
            this.button_solveYCwithNDF.TabIndex = 9;
            this.button_solveYCwithNDF.Text = "Solve YC - FF with NDF";
            this.button_solveYCwithNDF.UseVisualStyleBackColor = true;
            this.button_solveYCwithNDF.Click += new System.EventHandler(this.Button_solveYCwithNDF_FF_Click);
            // 
            // button_solveEMHwithADF
            // 
            this.button_solveEMHwithADF.Location = new System.Drawing.Point(668, 70);
            this.button_solveEMHwithADF.Name = "button_solveEMHwithADF";
            this.button_solveEMHwithADF.Size = new System.Drawing.Size(158, 23);
            this.button_solveEMHwithADF.TabIndex = 10;
            this.button_solveEMHwithADF.Text = "Solve EMH - FF with ADF";
            this.button_solveEMHwithADF.UseVisualStyleBackColor = true;
            this.button_solveEMHwithADF.Click += new System.EventHandler(this.Button_solveEMHwithADF_FF_Click);
            // 
            // button_solveYCwithADF
            // 
            this.button_solveYCwithADF.Location = new System.Drawing.Point(668, 122);
            this.button_solveYCwithADF.Name = "button_solveYCwithADF";
            this.button_solveYCwithADF.Size = new System.Drawing.Size(158, 23);
            this.button_solveYCwithADF.TabIndex = 11;
            this.button_solveYCwithADF.Text = "Solve YC - FF with ADF";
            this.button_solveYCwithADF.UseVisualStyleBackColor = true;
            this.button_solveYCwithADF.Click += new System.EventHandler(this.Button_solveYCwithADF_FF_Click);
            // 
            // textBox_runTime
            // 
            this.textBox_runTime.Location = new System.Drawing.Point(859, 35);
            this.textBox_runTime.Name = "textBox_runTime";
            this.textBox_runTime.Size = new System.Drawing.Size(79, 20);
            this.textBox_runTime.TabIndex = 12;
            this.textBox_runTime.Text = "3600";
            this.textBox_runTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(872, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Run Time";
            // 
            // checkBox_cplexLog2File
            // 
            this.checkBox_cplexLog2File.AutoSize = true;
            this.checkBox_cplexLog2File.Checked = true;
            this.checkBox_cplexLog2File.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_cplexLog2File.Location = new System.Drawing.Point(951, 37);
            this.checkBox_cplexLog2File.Name = "checkBox_cplexLog2File";
            this.checkBox_cplexLog2File.Size = new System.Drawing.Size(101, 17);
            this.checkBox_cplexLog2File.TabIndex = 14;
            this.checkBox_cplexLog2File.Text = "Cplex Log 2 File";
            this.checkBox_cplexLog2File.UseVisualStyleBackColor = true;
            // 
            // textBox_log
            // 
            this.textBox_log.Location = new System.Drawing.Point(855, 70);
            this.textBox_log.Multiline = true;
            this.textBox_log.Name = "textBox_log";
            this.textBox_log.Size = new System.Drawing.Size(282, 418);
            this.textBox_log.TabIndex = 15;
            // 
            // label_lambda
            // 
            this.label_lambda.AutoSize = true;
            this.label_lambda.Location = new System.Drawing.Point(1071, 20);
            this.label_lambda.Name = "label_lambda";
            this.label_lambda.Size = new System.Drawing.Size(45, 13);
            this.label_lambda.TabIndex = 17;
            this.label_lambda.Text = "Lambda";
            // 
            // textBox_lambda
            // 
            this.textBox_lambda.Location = new System.Drawing.Point(1058, 35);
            this.textBox_lambda.Name = "textBox_lambda";
            this.textBox_lambda.Size = new System.Drawing.Size(79, 20);
            this.textBox_lambda.TabIndex = 16;
            this.textBox_lambda.Text = "1";
            this.textBox_lambda.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(52, 309);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(233, 13);
            this.label6.TabIndex = 23;
            this.label6.Text = "Create Problem Model of YC with Min Total Cost";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(52, 212);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(245, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "Create Problem Model of EMH with Min Total VMT";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(71, 233);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(154, 13);
            this.label11.TabIndex = 20;
            this.label11.Text = "Min VMT, Refueling Policy= VF";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(38, 195);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(493, 13);
            this.label12.TabIndex = 19;
            this.label12.Text = "In this experiment, we will read EMH and YC instances and then solve them with AD" +
    "F and NDF models.";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(17, 173);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(103, 13);
            this.label13.TabIndex = 18;
            this.label13.Text = "2- Refueling Policies";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(71, 266);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(155, 13);
            this.label14.TabIndex = 24;
            this.label14.Text = "Min VMT, Refueling Policy= VP";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(71, 367);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(153, 13);
            this.label7.TabIndex = 26;
            this.label7.Text = "Min Cost, Refueling Policy= VP";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(71, 334);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(152, 13);
            this.label15.TabIndex = 25;
            this.label15.Text = "Min Cost, Refueling Policy= VF";
            // 
            // button_solve_EMH_NDF_VF
            // 
            this.button_solve_EMH_NDF_VF.Location = new System.Drawing.Point(496, 222);
            this.button_solve_EMH_NDF_VF.Name = "button_solve_EMH_NDF_VF";
            this.button_solve_EMH_NDF_VF.Size = new System.Drawing.Size(153, 23);
            this.button_solve_EMH_NDF_VF.TabIndex = 27;
            this.button_solve_EMH_NDF_VF.Text = "Solve EMH - VF with NDF";
            this.button_solve_EMH_NDF_VF.UseVisualStyleBackColor = true;
            this.button_solve_EMH_NDF_VF.Click += new System.EventHandler(this.Button_solve_EMH_NDF_VF_Click);
            // 
            // button_solve_EMH_ADF_VF
            // 
            this.button_solve_EMH_ADF_VF.Location = new System.Drawing.Point(668, 221);
            this.button_solve_EMH_ADF_VF.Name = "button_solve_EMH_ADF_VF";
            this.button_solve_EMH_ADF_VF.Size = new System.Drawing.Size(158, 23);
            this.button_solve_EMH_ADF_VF.TabIndex = 28;
            this.button_solve_EMH_ADF_VF.Text = "Solve EMH - VF with ADF";
            this.button_solve_EMH_ADF_VF.UseVisualStyleBackColor = true;
            this.button_solve_EMH_ADF_VF.Click += new System.EventHandler(this.Button_solve_EMH_ADF_VF_Click);
            // 
            // button_solve_EMH_ADF_VP
            // 
            this.button_solve_EMH_ADF_VP.Location = new System.Drawing.Point(668, 260);
            this.button_solve_EMH_ADF_VP.Name = "button_solve_EMH_ADF_VP";
            this.button_solve_EMH_ADF_VP.Size = new System.Drawing.Size(158, 23);
            this.button_solve_EMH_ADF_VP.TabIndex = 30;
            this.button_solve_EMH_ADF_VP.Text = "Solve EMH - VP with ADF";
            this.button_solve_EMH_ADF_VP.UseVisualStyleBackColor = true;
            this.button_solve_EMH_ADF_VP.Click += new System.EventHandler(this.Button_solve_EMH_ADF_VP_Click);
            // 
            // button_solve_EMH_NDF_VP
            // 
            this.button_solve_EMH_NDF_VP.Location = new System.Drawing.Point(496, 261);
            this.button_solve_EMH_NDF_VP.Name = "button_solve_EMH_NDF_VP";
            this.button_solve_EMH_NDF_VP.Size = new System.Drawing.Size(153, 23);
            this.button_solve_EMH_NDF_VP.TabIndex = 29;
            this.button_solve_EMH_NDF_VP.Text = "Solve EMH - VP with NDF";
            this.button_solve_EMH_NDF_VP.UseVisualStyleBackColor = true;
            this.button_solve_EMH_NDF_VP.Click += new System.EventHandler(this.Button_solve_EMH_NDF_VP_Click);
            // 
            // button_solve_YC_ADF_VP
            // 
            this.button_solve_YC_ADF_VP.Location = new System.Drawing.Point(668, 361);
            this.button_solve_YC_ADF_VP.Name = "button_solve_YC_ADF_VP";
            this.button_solve_YC_ADF_VP.Size = new System.Drawing.Size(158, 23);
            this.button_solve_YC_ADF_VP.TabIndex = 34;
            this.button_solve_YC_ADF_VP.Text = "Solve YC - VP with ADF";
            this.button_solve_YC_ADF_VP.UseVisualStyleBackColor = true;
            this.button_solve_YC_ADF_VP.Click += new System.EventHandler(this.Button_solve_YC_ADF_VP_Click);
            // 
            // button_solve_YC_NDF_VP
            // 
            this.button_solve_YC_NDF_VP.Location = new System.Drawing.Point(496, 362);
            this.button_solve_YC_NDF_VP.Name = "button_solve_YC_NDF_VP";
            this.button_solve_YC_NDF_VP.Size = new System.Drawing.Size(153, 23);
            this.button_solve_YC_NDF_VP.TabIndex = 33;
            this.button_solve_YC_NDF_VP.Text = "Solve YC - VP with NDF";
            this.button_solve_YC_NDF_VP.UseVisualStyleBackColor = true;
            this.button_solve_YC_NDF_VP.Click += new System.EventHandler(this.Button_solve_YC_NDF_VP_Click);
            // 
            // button_solve_YC_ADF_VF
            // 
            this.button_solve_YC_ADF_VF.Location = new System.Drawing.Point(668, 322);
            this.button_solve_YC_ADF_VF.Name = "button_solve_YC_ADF_VF";
            this.button_solve_YC_ADF_VF.Size = new System.Drawing.Size(158, 23);
            this.button_solve_YC_ADF_VF.TabIndex = 32;
            this.button_solve_YC_ADF_VF.Text = "Solve YC - VF with ADF";
            this.button_solve_YC_ADF_VF.UseVisualStyleBackColor = true;
            this.button_solve_YC_ADF_VF.Click += new System.EventHandler(this.Button_solve_YC_ADF_VF_Click);
            // 
            // button_solve_YC_NDF_VF
            // 
            this.button_solve_YC_NDF_VF.Location = new System.Drawing.Point(496, 323);
            this.button_solve_YC_NDF_VF.Name = "button_solve_YC_NDF_VF";
            this.button_solve_YC_NDF_VF.Size = new System.Drawing.Size(153, 23);
            this.button_solve_YC_NDF_VF.TabIndex = 31;
            this.button_solve_YC_NDF_VF.Text = "Solve YC - VF with NDF";
            this.button_solve_YC_NDF_VF.UseVisualStyleBackColor = true;
            this.button_solve_YC_NDF_VF.Click += new System.EventHandler(this.Button_solve_YC_NDF_VF_Click);
            // 
            // TS_Runs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1154, 500);
            this.Controls.Add(this.button_solve_YC_ADF_VP);
            this.Controls.Add(this.button_solve_YC_NDF_VP);
            this.Controls.Add(this.button_solve_YC_ADF_VF);
            this.Controls.Add(this.button_solve_YC_NDF_VF);
            this.Controls.Add(this.button_solve_EMH_ADF_VP);
            this.Controls.Add(this.button_solve_EMH_NDF_VP);
            this.Controls.Add(this.button_solve_EMH_ADF_VF);
            this.Controls.Add(this.button_solve_EMH_NDF_VF);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label13);
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
            this.Margin = new System.Windows.Forms.Padding(2);
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
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button button_solve_EMH_NDF_VF;
        private System.Windows.Forms.Button button_solve_EMH_ADF_VF;
        private System.Windows.Forms.Button button_solve_EMH_ADF_VP;
        private System.Windows.Forms.Button button_solve_EMH_NDF_VP;
        private System.Windows.Forms.Button button_solve_YC_ADF_VP;
        private System.Windows.Forms.Button button_solve_YC_NDF_VP;
        private System.Windows.Forms.Button button_solve_YC_ADF_VF;
        private System.Windows.Forms.Button button_solve_YC_NDF_VF;
    }
}