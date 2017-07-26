﻿namespace MPMFEVRP.Forms
{
    partial class SingleProblemSingleAlgorithm
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
            this.groupBox_problem = new System.Windows.Forms.GroupBox();
            this.comboBox_problemModels = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_problems = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_selectedFile = new System.Windows.Forms.Label();
            this.button_browseForFile = new System.Windows.Forms.Button();
            this.button_openDataManager = new System.Windows.Forms.Button();
            this.button_viewProblem = new System.Windows.Forms.Button();
            this.label_numberOfJobs = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox_algorithms = new System.Windows.Forms.GroupBox();
            this.button_run = new System.Windows.Forms.Button();
            this.panel_parameters = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBox_algorithms = new System.Windows.Forms.ComboBox();
            this.button_viewSolution = new System.Windows.Forms.Button();
            this.groupBox_status = new System.Windows.Forms.GroupBox();
            this.textBox_log = new System.Windows.Forms.TextBox();
            this.groupBox_problem.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox_algorithms.SuspendLayout();
            this.groupBox_status.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_problem
            // 
            this.groupBox_problem.Controls.Add(this.comboBox_problemModels);
            this.groupBox_problem.Controls.Add(this.label3);
            this.groupBox_problem.Controls.Add(this.label2);
            this.groupBox_problem.Controls.Add(this.comboBox_problems);
            this.groupBox_problem.Controls.Add(this.groupBox1);
            this.groupBox_problem.Controls.Add(this.button_viewProblem);
            this.groupBox_problem.Controls.Add(this.label_numberOfJobs);
            this.groupBox_problem.Controls.Add(this.label8);
            this.groupBox_problem.Controls.Add(this.label7);
            this.groupBox_problem.Location = new System.Drawing.Point(17, 16);
            this.groupBox_problem.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox_problem.Name = "groupBox_problem";
            this.groupBox_problem.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox_problem.Size = new System.Drawing.Size(364, 598);
            this.groupBox_problem.TabIndex = 0;
            this.groupBox_problem.TabStop = false;
            this.groupBox_problem.Text = "The \"Problem\" Side (problem, model, data, etc.)";
            // 
            // comboBox_problemModels
            // 
            this.comboBox_problemModels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_problemModels.FormattingEnabled = true;
            this.comboBox_problemModels.Location = new System.Drawing.Point(8, 92);
            this.comboBox_problemModels.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBox_problemModels.Name = "comboBox_problemModels";
            this.comboBox_problemModels.Size = new System.Drawing.Size(341, 24);
            this.comboBox_problemModels.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 73);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 17);
            this.label3.TabIndex = 16;
            this.label3.Text = "Problem Model";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 23);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 17);
            this.label2.TabIndex = 15;
            this.label2.Text = "Problem";
            // 
            // comboBox_problems
            // 
            this.comboBox_problems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_problems.FormattingEnabled = true;
            this.comboBox_problems.Location = new System.Drawing.Point(8, 43);
            this.comboBox_problems.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBox_problems.Name = "comboBox_problems";
            this.comboBox_problems.Size = new System.Drawing.Size(341, 24);
            this.comboBox_problems.TabIndex = 12;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_selectedFile);
            this.groupBox1.Controls.Add(this.button_browseForFile);
            this.groupBox1.Controls.Add(this.button_openDataManager);
            this.groupBox1.Location = new System.Drawing.Point(9, 132);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(341, 154);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Load From File";
            // 
            // label_selectedFile
            // 
            this.label_selectedFile.Location = new System.Drawing.Point(16, 65);
            this.label_selectedFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_selectedFile.Name = "label_selectedFile";
            this.label_selectedFile.Size = new System.Drawing.Size(305, 70);
            this.label_selectedFile.TabIndex = 4;
            this.label_selectedFile.Text = "No file selected yet!";
            // 
            // button_browseForFile
            // 
            this.button_browseForFile.Location = new System.Drawing.Point(16, 30);
            this.button_browseForFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_browseForFile.Name = "button_browseForFile";
            this.button_browseForFile.Size = new System.Drawing.Size(160, 28);
            this.button_browseForFile.TabIndex = 0;
            this.button_browseForFile.Text = "Browse for File";
            this.button_browseForFile.UseVisualStyleBackColor = true;
            this.button_browseForFile.Click += new System.EventHandler(this.Button_browseForFile_Click);
            // 
            // button_openDataManager
            // 
            this.button_openDataManager.Location = new System.Drawing.Point(184, 30);
            this.button_openDataManager.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_openDataManager.Name = "button_openDataManager";
            this.button_openDataManager.Size = new System.Drawing.Size(149, 28);
            this.button_openDataManager.TabIndex = 5;
            this.button_openDataManager.Text = "Open Data Manager";
            this.button_openDataManager.UseVisualStyleBackColor = true;
            this.button_openDataManager.Click += new System.EventHandler(this.Button_openDataManager_Click);
            // 
            // button_viewProblem
            // 
            this.button_viewProblem.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_viewProblem.Location = new System.Drawing.Point(11, 351);
            this.button_viewProblem.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_viewProblem.Name = "button_viewProblem";
            this.button_viewProblem.Size = new System.Drawing.Size(341, 34);
            this.button_viewProblem.TabIndex = 8;
            this.button_viewProblem.Text = "View Problem";
            this.button_viewProblem.UseVisualStyleBackColor = true;
            this.button_viewProblem.Click += new System.EventHandler(this.Button_viewProblem_Click);
            // 
            // label_numberOfJobs
            // 
            this.label_numberOfJobs.AutoSize = true;
            this.label_numberOfJobs.Location = new System.Drawing.Point(284, 320);
            this.label_numberOfJobs.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_numberOfJobs.Name = "label_numberOfJobs";
            this.label_numberOfJobs.Size = new System.Drawing.Size(18, 17);
            this.label_numberOfJobs.TabIndex = 7;
            this.label_numberOfJobs.Text = "N";
            this.label_numberOfJobs.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(32, 320);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(104, 17);
            this.label8.TabIndex = 6;
            this.label8.Text = "Number of jobs";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(88, 289);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(162, 20);
            this.label7.TabIndex = 5;
            this.label7.Text = "Problem Summary";
            // 
            // groupBox_algorithms
            // 
            this.groupBox_algorithms.Controls.Add(this.button_run);
            this.groupBox_algorithms.Controls.Add(this.panel_parameters);
            this.groupBox_algorithms.Controls.Add(this.label9);
            this.groupBox_algorithms.Controls.Add(this.comboBox_algorithms);
            this.groupBox_algorithms.Location = new System.Drawing.Point(391, 16);
            this.groupBox_algorithms.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox_algorithms.Name = "groupBox_algorithms";
            this.groupBox_algorithms.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox_algorithms.Size = new System.Drawing.Size(404, 598);
            this.groupBox_algorithms.TabIndex = 1;
            this.groupBox_algorithms.TabStop = false;
            this.groupBox_algorithms.Text = "Algorithm";
            // 
            // button_run
            // 
            this.button_run.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_run.Location = new System.Drawing.Point(9, 554);
            this.button_run.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_run.Name = "button_run";
            this.button_run.Size = new System.Drawing.Size(387, 34);
            this.button_run.TabIndex = 11;
            this.button_run.Text = "RUN";
            this.button_run.UseVisualStyleBackColor = true;
            this.button_run.Click += new System.EventHandler(this.Button_run_Click);
            // 
            // panel_parameters
            // 
            this.panel_parameters.Location = new System.Drawing.Point(9, 84);
            this.panel_parameters.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel_parameters.Name = "panel_parameters";
            this.panel_parameters.Size = new System.Drawing.Size(387, 449);
            this.panel_parameters.TabIndex = 12;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(17, 63);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(81, 17);
            this.label9.TabIndex = 11;
            this.label9.Text = "Parameters";
            // 
            // comboBox_algorithms
            // 
            this.comboBox_algorithms.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_algorithms.FormattingEnabled = true;
            this.comboBox_algorithms.Location = new System.Drawing.Point(8, 23);
            this.comboBox_algorithms.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBox_algorithms.Name = "comboBox_algorithms";
            this.comboBox_algorithms.Size = new System.Drawing.Size(387, 24);
            this.comboBox_algorithms.TabIndex = 11;
            // 
            // button_viewSolution
            // 
            this.button_viewSolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_viewSolution.Location = new System.Drawing.Point(8, 554);
            this.button_viewSolution.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_viewSolution.Name = "button_viewSolution";
            this.button_viewSolution.Size = new System.Drawing.Size(427, 34);
            this.button_viewSolution.TabIndex = 13;
            this.button_viewSolution.Text = "View Solution";
            this.button_viewSolution.UseVisualStyleBackColor = true;
            this.button_viewSolution.Click += new System.EventHandler(this.Button_viewSolution_Click);
            // 
            // groupBox_status
            // 
            this.groupBox_status.Controls.Add(this.textBox_log);
            this.groupBox_status.Controls.Add(this.button_viewSolution);
            this.groupBox_status.Location = new System.Drawing.Point(804, 16);
            this.groupBox_status.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox_status.Name = "groupBox_status";
            this.groupBox_status.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox_status.Size = new System.Drawing.Size(443, 598);
            this.groupBox_status.TabIndex = 2;
            this.groupBox_status.TabStop = false;
            this.groupBox_status.Text = "Status";
            // 
            // textBox_log
            // 
            this.textBox_log.Location = new System.Drawing.Point(8, 25);
            this.textBox_log.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_log.Multiline = true;
            this.textBox_log.Name = "textBox_log";
            this.textBox_log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_log.Size = new System.Drawing.Size(425, 521);
            this.textBox_log.TabIndex = 14;
            // 
            // SingleProblemSingleAlgorithm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1263, 629);
            this.Controls.Add(this.groupBox_status);
            this.Controls.Add(this.groupBox_algorithms);
            this.Controls.Add(this.groupBox_problem);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "SingleProblemSingleAlgorithm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SingleProblemSingleAlgorithm";
            this.groupBox_problem.ResumeLayout(false);
            this.groupBox_problem.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox_algorithms.ResumeLayout(false);
            this.groupBox_algorithms.PerformLayout();
            this.groupBox_status.ResumeLayout(false);
            this.groupBox_status.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_problem;
        private System.Windows.Forms.Button button_browseForFile;
        private System.Windows.Forms.Label label_selectedFile;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label_numberOfJobs;
        private System.Windows.Forms.Button button_viewProblem;
        private System.Windows.Forms.GroupBox groupBox_algorithms;
        private System.Windows.Forms.ComboBox comboBox_algorithms;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button_run;
        private System.Windows.Forms.Panel panel_parameters;
        private System.Windows.Forms.Button button_viewSolution;
        private System.Windows.Forms.GroupBox groupBox_status;
        private System.Windows.Forms.TextBox textBox_log;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_openDataManager;
        private System.Windows.Forms.ComboBox comboBox_problems;
        private System.Windows.Forms.ComboBox comboBox_problemModels;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
    }
}