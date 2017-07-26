namespace MPMFEVRP.Forms
{
    partial class MultipleProblemMultipleAlgorithm
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
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_openDataManager = new System.Windows.Forms.Button();
            this.button_addProblem = new System.Windows.Forms.Button();
            this.button_viewProblem = new System.Windows.Forms.Button();
            this.linkLabel_deleteSelectedProblem = new System.Windows.Forms.LinkLabel();
            this.listBox_problems = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button_run = new System.Windows.Forms.Button();
            this.linkLabel_deleteSelectedAlgorithm = new System.Windows.Forms.LinkLabel();
            this.button_addAlgo = new System.Windows.Forms.Button();
            this.listBox_algorithms = new System.Windows.Forms.ListBox();
            this.comboBox_algorithms = new System.Windows.Forms.ComboBox();
            this.textBox_log = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button_report = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.comboBox_multi_problemModels = new System.Windows.Forms.ComboBox();
            this.label_problemModel = new System.Windows.Forms.Label();
            this.label_problem = new System.Windows.Forms.Label();
            this.comboBox_multi_problems = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBox_multi_problemModels);
            this.groupBox1.Controls.Add(this.label_problemModel);
            this.groupBox1.Controls.Add(this.label_problem);
            this.groupBox1.Controls.Add(this.comboBox_multi_problems);
            this.groupBox1.Controls.Add(this.button_openDataManager);
            this.groupBox1.Controls.Add(this.button_addProblem);
            this.groupBox1.Controls.Add(this.button_viewProblem);
            this.groupBox1.Controls.Add(this.linkLabel_deleteSelectedProblem);
            this.groupBox1.Controls.Add(this.listBox_problems);
            this.groupBox1.Location = new System.Drawing.Point(16, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(257, 614);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Problems";
            // 
            // button_openDataManager
            // 
            this.button_openDataManager.Location = new System.Drawing.Point(9, 156);
            this.button_openDataManager.Margin = new System.Windows.Forms.Padding(4);
            this.button_openDataManager.Name = "button_openDataManager";
            this.button_openDataManager.Size = new System.Drawing.Size(240, 28);
            this.button_openDataManager.TabIndex = 6;
            this.button_openDataManager.Text = "Open Data Manager";
            this.button_openDataManager.UseVisualStyleBackColor = true;
            this.button_openDataManager.Click += new System.EventHandler(this.Button_openDataManager_Click);
            // 
            // button_addProblem
            // 
            this.button_addProblem.Location = new System.Drawing.Point(8, 120);
            this.button_addProblem.Margin = new System.Windows.Forms.Padding(4);
            this.button_addProblem.Name = "button_addProblem";
            this.button_addProblem.Size = new System.Drawing.Size(240, 28);
            this.button_addProblem.TabIndex = 5;
            this.button_addProblem.Text = "Load Problem from File";
            this.button_addProblem.UseVisualStyleBackColor = true;
            this.button_addProblem.Click += new System.EventHandler(this.Button_addProblem_Click);
            // 
            // button_viewProblem
            // 
            this.button_viewProblem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_viewProblem.Location = new System.Drawing.Point(9, 567);
            this.button_viewProblem.Margin = new System.Windows.Forms.Padding(4);
            this.button_viewProblem.Name = "button_viewProblem";
            this.button_viewProblem.Size = new System.Drawing.Size(240, 39);
            this.button_viewProblem.TabIndex = 5;
            this.button_viewProblem.Text = "View Problem";
            this.button_viewProblem.UseVisualStyleBackColor = true;
            this.button_viewProblem.Click += new System.EventHandler(this.Button_viewProblem_Click);
            // 
            // linkLabel_deleteSelectedProblem
            // 
            this.linkLabel_deleteSelectedProblem.AutoSize = true;
            this.linkLabel_deleteSelectedProblem.Location = new System.Drawing.Point(140, 188);
            this.linkLabel_deleteSelectedProblem.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel_deleteSelectedProblem.Name = "linkLabel_deleteSelectedProblem";
            this.linkLabel_deleteSelectedProblem.Size = new System.Drawing.Size(108, 17);
            this.linkLabel_deleteSelectedProblem.TabIndex = 5;
            this.linkLabel_deleteSelectedProblem.TabStop = true;
            this.linkLabel_deleteSelectedProblem.Text = "Delete Selected";
            this.linkLabel_deleteSelectedProblem.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_deleteSelectedProblem_LinkClicked);
            // 
            // listBox_problems
            // 
            this.listBox_problems.FormattingEnabled = true;
            this.listBox_problems.ItemHeight = 16;
            this.listBox_problems.Location = new System.Drawing.Point(9, 219);
            this.listBox_problems.Margin = new System.Windows.Forms.Padding(4);
            this.listBox_problems.Name = "listBox_problems";
            this.listBox_problems.Size = new System.Drawing.Size(239, 340);
            this.listBox_problems.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button_run);
            this.groupBox2.Controls.Add(this.linkLabel_deleteSelectedAlgorithm);
            this.groupBox2.Controls.Add(this.button_addAlgo);
            this.groupBox2.Controls.Add(this.listBox_algorithms);
            this.groupBox2.Controls.Add(this.comboBox_algorithms);
            this.groupBox2.Location = new System.Drawing.Point(283, 16);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(389, 613);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Algorithms";
            // 
            // button_run
            // 
            this.button_run.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_run.Location = new System.Drawing.Point(9, 566);
            this.button_run.Margin = new System.Windows.Forms.Padding(4);
            this.button_run.Name = "button_run";
            this.button_run.Size = new System.Drawing.Size(372, 39);
            this.button_run.TabIndex = 4;
            this.button_run.Text = "RUN";
            this.button_run.UseVisualStyleBackColor = true;
            this.button_run.Click += new System.EventHandler(this.Button_run_Click);
            // 
            // linkLabel_deleteSelectedAlgorithm
            // 
            this.linkLabel_deleteSelectedAlgorithm.AutoSize = true;
            this.linkLabel_deleteSelectedAlgorithm.Location = new System.Drawing.Point(272, 55);
            this.linkLabel_deleteSelectedAlgorithm.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel_deleteSelectedAlgorithm.Name = "linkLabel_deleteSelectedAlgorithm";
            this.linkLabel_deleteSelectedAlgorithm.Size = new System.Drawing.Size(108, 17);
            this.linkLabel_deleteSelectedAlgorithm.TabIndex = 3;
            this.linkLabel_deleteSelectedAlgorithm.TabStop = true;
            this.linkLabel_deleteSelectedAlgorithm.Text = "Delete Selected";
            this.linkLabel_deleteSelectedAlgorithm.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_deleteSelected_LinkClicked);
            // 
            // button_addAlgo
            // 
            this.button_addAlgo.Location = new System.Drawing.Point(304, 22);
            this.button_addAlgo.Margin = new System.Windows.Forms.Padding(4);
            this.button_addAlgo.Name = "button_addAlgo";
            this.button_addAlgo.Size = new System.Drawing.Size(77, 28);
            this.button_addAlgo.TabIndex = 2;
            this.button_addAlgo.Text = "Add";
            this.button_addAlgo.UseVisualStyleBackColor = true;
            this.button_addAlgo.Click += new System.EventHandler(this.Button_addAlgo_Click);
            // 
            // listBox_algorithms
            // 
            this.listBox_algorithms.FormattingEnabled = true;
            this.listBox_algorithms.ItemHeight = 16;
            this.listBox_algorithms.Location = new System.Drawing.Point(9, 74);
            this.listBox_algorithms.Margin = new System.Windows.Forms.Padding(4);
            this.listBox_algorithms.Name = "listBox_algorithms";
            this.listBox_algorithms.Size = new System.Drawing.Size(371, 484);
            this.listBox_algorithms.TabIndex = 1;
            // 
            // comboBox_algorithms
            // 
            this.comboBox_algorithms.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_algorithms.FormattingEnabled = true;
            this.comboBox_algorithms.Location = new System.Drawing.Point(9, 23);
            this.comboBox_algorithms.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_algorithms.Name = "comboBox_algorithms";
            this.comboBox_algorithms.Size = new System.Drawing.Size(284, 24);
            this.comboBox_algorithms.TabIndex = 0;
            // 
            // textBox_log
            // 
            this.textBox_log.Location = new System.Drawing.Point(9, 25);
            this.textBox_log.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_log.Multiline = true;
            this.textBox_log.Name = "textBox_log";
            this.textBox_log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_log.Size = new System.Drawing.Size(508, 533);
            this.textBox_log.TabIndex = 0;
            this.textBox_log.WordWrap = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button_report);
            this.groupBox3.Controls.Add(this.textBox_log);
            this.groupBox3.Location = new System.Drawing.Point(680, 16);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(527, 613);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Status";
            // 
            // button_report
            // 
            this.button_report.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_report.Location = new System.Drawing.Point(9, 565);
            this.button_report.Margin = new System.Windows.Forms.Padding(4);
            this.button_report.Name = "button_report";
            this.button_report.Size = new System.Drawing.Size(509, 39);
            this.button_report.TabIndex = 5;
            this.button_report.Text = "Report";
            this.button_report.UseVisualStyleBackColor = true;
            this.button_report.Click += new System.EventHandler(this.Button_report_click);
            // 
            // comboBox_multi_problemModels
            // 
            this.comboBox_multi_problemModels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_multi_problemModels.FormattingEnabled = true;
            this.comboBox_multi_problemModels.Location = new System.Drawing.Point(8, 88);
            this.comboBox_multi_problemModels.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_multi_problemModels.Name = "comboBox_multi_problemModels";
            this.comboBox_multi_problemModels.Size = new System.Drawing.Size(240, 24);
            this.comboBox_multi_problemModels.TabIndex = 18;
            // 
            // label_problemModel
            // 
            this.label_problemModel.AutoSize = true;
            this.label_problemModel.Location = new System.Drawing.Point(8, 69);
            this.label_problemModel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_problemModel.Name = "label_problemModel";
            this.label_problemModel.Size = new System.Drawing.Size(102, 17);
            this.label_problemModel.TabIndex = 20;
            this.label_problemModel.Text = "Problem Model";
            // 
            // label_problem
            // 
            this.label_problem.AutoSize = true;
            this.label_problem.Location = new System.Drawing.Point(8, 19);
            this.label_problem.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_problem.Name = "label_problem";
            this.label_problem.Size = new System.Drawing.Size(60, 17);
            this.label_problem.TabIndex = 19;
            this.label_problem.Text = "Problem";
            // 
            // comboBox_multi_problems
            // 
            this.comboBox_multi_problems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_multi_problems.FormattingEnabled = true;
            this.comboBox_multi_problems.Location = new System.Drawing.Point(8, 39);
            this.comboBox_multi_problems.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_multi_problems.Name = "comboBox_multi_problems";
            this.comboBox_multi_problems.Size = new System.Drawing.Size(240, 24);
            this.comboBox_multi_problems.TabIndex = 17;
            // 
            // MultipleProblemMultipleAlgorithm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1223, 644);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "MultipleProblemMultipleAlgorithm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Multiple Instances Using the same Model, by Multiple Algorithm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button_addAlgo;
        private System.Windows.Forms.ListBox listBox_algorithms;
        private System.Windows.Forms.ComboBox comboBox_algorithms;
        private System.Windows.Forms.LinkLabel linkLabel_deleteSelectedAlgorithm;
        private System.Windows.Forms.TextBox textBox_log;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button_run;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button button_report;
        private System.Windows.Forms.ListBox listBox_problems;
        private System.Windows.Forms.LinkLabel linkLabel_deleteSelectedProblem;
        private System.Windows.Forms.Button button_viewProblem;
        private System.Windows.Forms.Button button_addProblem;
        private System.Windows.Forms.Button button_openDataManager;
        private System.Windows.Forms.ComboBox comboBox_multi_problemModels;
        private System.Windows.Forms.Label label_problemModel;
        private System.Windows.Forms.Label label_problem;
        private System.Windows.Forms.ComboBox comboBox_multi_problems;
    }
}