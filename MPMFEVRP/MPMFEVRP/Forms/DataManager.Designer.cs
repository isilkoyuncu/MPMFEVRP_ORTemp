namespace MPMFEVRP.Forms
{
    partial class DataManager
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textBox_dueDateUpperLimit = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_dueDateLowerLimit = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_processingTimeUpperLimit = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_processingTimeLowerLimit = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_numberOfJobs = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label_numberOfJobs = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.button_generateRandom = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.button_addJob = new System.Windows.Forms.Button();
            this.textBox_dueDate = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_processingTime = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_description = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.button_run = new System.Windows.Forms.Button();
            this.button_viewProblem = new System.Windows.Forms.Button();
            this.button_reset = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(13, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(290, 219);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button_generateRandom);
            this.tabPage1.Controls.Add(this.textBox_dueDateUpperLimit);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.textBox_dueDateLowerLimit);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.textBox_processingTimeUpperLimit);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.textBox_processingTimeLowerLimit);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.textBox_numberOfJobs);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(282, 193);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Random";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.textBox_description);
            this.tabPage2.Controls.Add(this.label11);
            this.tabPage2.Controls.Add(this.textBox_dueDate);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.textBox_processingTime);
            this.tabPage2.Controls.Add(this.label10);
            this.tabPage2.Controls.Add(this.button_addJob);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(282, 193);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Manual";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // textBox_dueDateUpperLimit
            // 
            this.textBox_dueDateUpperLimit.Location = new System.Drawing.Point(218, 85);
            this.textBox_dueDateUpperLimit.Name = "textBox_dueDateUpperLimit";
            this.textBox_dueDateUpperLimit.Size = new System.Drawing.Size(35, 20);
            this.textBox_dueDateUpperLimit.TabIndex = 19;
            this.textBox_dueDateUpperLimit.Text = "100";
            this.textBox_dueDateUpperLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(191, 88);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(25, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "and";
            // 
            // textBox_dueDateLowerLimit
            // 
            this.textBox_dueDateLowerLimit.Location = new System.Drawing.Point(154, 84);
            this.textBox_dueDateLowerLimit.Name = "textBox_dueDateLowerLimit";
            this.textBox_dueDateLowerLimit.Size = new System.Drawing.Size(32, 20);
            this.textBox_dueDateLowerLimit.TabIndex = 17;
            this.textBox_dueDateLowerLimit.Text = "25";
            this.textBox_dueDateLowerLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(22, 88);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(100, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Due dates between";
            // 
            // textBox_processingTimeUpperLimit
            // 
            this.textBox_processingTimeUpperLimit.Location = new System.Drawing.Point(218, 53);
            this.textBox_processingTimeUpperLimit.Name = "textBox_processingTimeUpperLimit";
            this.textBox_processingTimeUpperLimit.Size = new System.Drawing.Size(35, 20);
            this.textBox_processingTimeUpperLimit.TabIndex = 15;
            this.textBox_processingTimeUpperLimit.Text = "25";
            this.textBox_processingTimeUpperLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(191, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(25, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "and";
            // 
            // textBox_processingTimeLowerLimit
            // 
            this.textBox_processingTimeLowerLimit.Location = new System.Drawing.Point(154, 52);
            this.textBox_processingTimeLowerLimit.Name = "textBox_processingTimeLowerLimit";
            this.textBox_processingTimeLowerLimit.Size = new System.Drawing.Size(32, 20);
            this.textBox_processingTimeLowerLimit.TabIndex = 13;
            this.textBox_processingTimeLowerLimit.Text = "5";
            this.textBox_processingTimeLowerLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Processing times between";
            // 
            // textBox_numberOfJobs
            // 
            this.textBox_numberOfJobs.Location = new System.Drawing.Point(109, 23);
            this.textBox_numberOfJobs.Name = "textBox_numberOfJobs";
            this.textBox_numberOfJobs.Size = new System.Drawing.Size(148, 20);
            this.textBox_numberOfJobs.TabIndex = 11;
            this.textBox_numberOfJobs.Text = "25";
            this.textBox_numberOfJobs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Number of jobs";
            // 
            // label_numberOfJobs
            // 
            this.label_numberOfJobs.AutoSize = true;
            this.label_numberOfJobs.Location = new System.Drawing.Point(237, 282);
            this.label_numberOfJobs.Name = "label_numberOfJobs";
            this.label_numberOfJobs.Size = new System.Drawing.Size(13, 13);
            this.label_numberOfJobs.TabIndex = 10;
            this.label_numberOfJobs.Text = "0";
            this.label_numberOfJobs.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(48, 282);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(78, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "Number of jobs";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(90, 257);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(134, 16);
            this.label7.TabIndex = 8;
            this.label7.Text = "Problem Summary";
            // 
            // button_generateRandom
            // 
            this.button_generateRandom.Location = new System.Drawing.Point(7, 164);
            this.button_generateRandom.Name = "button_generateRandom";
            this.button_generateRandom.Size = new System.Drawing.Size(269, 23);
            this.button_generateRandom.TabIndex = 20;
            this.button_generateRandom.Text = "Generate Random";
            this.button_generateRandom.UseVisualStyleBackColor = true;
            this.button_generateRandom.Click += new System.EventHandler(this.button_generateRandom_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(282, 193);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Other File Format";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(88, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "This feature is coming...";
            // 
            // button_addJob
            // 
            this.button_addJob.Location = new System.Drawing.Point(6, 164);
            this.button_addJob.Name = "button_addJob";
            this.button_addJob.Size = new System.Drawing.Size(270, 23);
            this.button_addJob.TabIndex = 21;
            this.button_addJob.Text = "Add Job";
            this.button_addJob.UseVisualStyleBackColor = true;
            this.button_addJob.Click += new System.EventHandler(this.button_addJob_Click);
            // 
            // textBox_dueDate
            // 
            this.textBox_dueDate.Location = new System.Drawing.Point(176, 102);
            this.textBox_dueDate.Name = "textBox_dueDate";
            this.textBox_dueDate.Size = new System.Drawing.Size(67, 20);
            this.textBox_dueDate.TabIndex = 25;
            this.textBox_dueDate.Text = "25";
            this.textBox_dueDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(40, 106);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(51, 13);
            this.label9.TabIndex = 24;
            this.label9.Text = "Due date";
            // 
            // textBox_processingTime
            // 
            this.textBox_processingTime.Location = new System.Drawing.Point(176, 65);
            this.textBox_processingTime.Name = "textBox_processingTime";
            this.textBox_processingTime.Size = new System.Drawing.Size(67, 20);
            this.textBox_processingTime.TabIndex = 23;
            this.textBox_processingTime.Text = "5";
            this.textBox_processingTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(40, 69);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(81, 13);
            this.label10.TabIndex = 22;
            this.label10.Text = "Processing time";
            // 
            // textBox_description
            // 
            this.textBox_description.Location = new System.Drawing.Point(106, 27);
            this.textBox_description.Name = "textBox_description";
            this.textBox_description.Size = new System.Drawing.Size(137, 20);
            this.textBox_description.TabIndex = 27;
            this.textBox_description.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(40, 31);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(60, 13);
            this.label11.TabIndex = 26;
            this.label11.Text = "Description";
            // 
            // button_run
            // 
            this.button_run.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_run.Location = new System.Drawing.Point(12, 369);
            this.button_run.Name = "button_run";
            this.button_run.Size = new System.Drawing.Size(295, 31);
            this.button_run.TabIndex = 12;
            this.button_run.Text = "Export to File";
            this.button_run.UseVisualStyleBackColor = true;
            this.button_run.Click += new System.EventHandler(this.button_run_Click);
            // 
            // button_viewProblem
            // 
            this.button_viewProblem.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_viewProblem.Location = new System.Drawing.Point(12, 335);
            this.button_viewProblem.Name = "button_viewProblem";
            this.button_viewProblem.Size = new System.Drawing.Size(151, 28);
            this.button_viewProblem.TabIndex = 13;
            this.button_viewProblem.Text = "View Problem";
            this.button_viewProblem.UseVisualStyleBackColor = true;
            this.button_viewProblem.Click += new System.EventHandler(this.button_viewProblem_Click);
            // 
            // button_reset
            // 
            this.button_reset.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_reset.Location = new System.Drawing.Point(169, 335);
            this.button_reset.Name = "button_reset";
            this.button_reset.Size = new System.Drawing.Size(138, 28);
            this.button_reset.TabIndex = 14;
            this.button_reset.Text = "Reset";
            this.button_reset.UseVisualStyleBackColor = true;
            this.button_reset.Click += new System.EventHandler(this.button_reset_Click);
            // 
            // DataManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(319, 412);
            this.Controls.Add(this.button_reset);
            this.Controls.Add(this.button_viewProblem);
            this.Controls.Add(this.button_run);
            this.Controls.Add(this.label_numberOfJobs);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "DataManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DataManager";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox textBox_dueDateUpperLimit;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_dueDateLowerLimit;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_processingTimeUpperLimit;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_processingTimeLowerLimit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_numberOfJobs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_numberOfJobs;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button_generateRandom;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_addJob;
        private System.Windows.Forms.TextBox textBox_description;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBox_dueDate;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox_processingTime;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button_run;
        private System.Windows.Forms.Button button_viewProblem;
        private System.Windows.Forms.Button button_reset;
    }
}