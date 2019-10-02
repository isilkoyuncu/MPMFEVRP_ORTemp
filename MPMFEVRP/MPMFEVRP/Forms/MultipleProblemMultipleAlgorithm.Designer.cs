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
            this.panel_multi_problemCharacteristics = new System.Windows.Forms.Panel();
            this.button_multi_createProblemModel = new System.Windows.Forms.Button();
            this.comboBox_multi_TSPModel = new System.Windows.Forms.ComboBox();
            this.comboBox_multi_problemModels = new System.Windows.Forms.ComboBox();
            this.label_multi_TSPModel = new System.Windows.Forms.Label();
            this.label_problemModel = new System.Windows.Forms.Label();
            this.label_problem = new System.Windows.Forms.Label();
            this.comboBox_multi_problems = new System.Windows.Forms.ComboBox();
            this.button_openDataManager = new System.Windows.Forms.Button();
            this.button_addProblem = new System.Windows.Forms.Button();
            this.button_multi_viewProblem = new System.Windows.Forms.Button();
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
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel_multi_problemCharacteristics);
            this.groupBox1.Controls.Add(this.button_multi_createProblemModel);
            this.groupBox1.Controls.Add(this.comboBox_multi_TSPModel);
            this.groupBox1.Controls.Add(this.comboBox_multi_problemModels);
            this.groupBox1.Controls.Add(this.label_multi_TSPModel);
            this.groupBox1.Controls.Add(this.label_problemModel);
            this.groupBox1.Controls.Add(this.label_problem);
            this.groupBox1.Controls.Add(this.comboBox_multi_problems);
            this.groupBox1.Controls.Add(this.button_openDataManager);
            this.groupBox1.Controls.Add(this.button_addProblem);
            this.groupBox1.Controls.Add(this.button_multi_viewProblem);
            this.groupBox1.Controls.Add(this.linkLabel_deleteSelectedProblem);
            this.groupBox1.Controls.Add(this.listBox_problems);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(368, 752);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Problems";
            // 
            // panel_multi_problemCharacteristics
            // 
            this.panel_multi_problemCharacteristics.Location = new System.Drawing.Point(6, 463);
            this.panel_multi_problemCharacteristics.Margin = new System.Windows.Forms.Padding(2);
            this.panel_multi_problemCharacteristics.Name = "panel_multi_problemCharacteristics";
            this.panel_multi_problemCharacteristics.Size = new System.Drawing.Size(353, 201);
            this.panel_multi_problemCharacteristics.TabIndex = 25;
            // 
            // button_multi_createProblemModel
            // 
            this.button_multi_createProblemModel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_multi_createProblemModel.Location = new System.Drawing.Point(194, 711);
            this.button_multi_createProblemModel.Name = "button_multi_createProblemModel";
            this.button_multi_createProblemModel.Size = new System.Drawing.Size(165, 32);
            this.button_multi_createProblemModel.TabIndex = 24;
            this.button_multi_createProblemModel.Text = "Create Problem Model";
            this.button_multi_createProblemModel.UseVisualStyleBackColor = true;
            this.button_multi_createProblemModel.Click += new System.EventHandler(this.Button_multi_createProblemModel_Click);
            // 
            // comboBox_multi_TSPModel
            // 
            this.comboBox_multi_TSPModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_multi_TSPModel.FormattingEnabled = true;
            this.comboBox_multi_TSPModel.Location = new System.Drawing.Point(6, 687);
            this.comboBox_multi_TSPModel.Name = "comboBox_multi_TSPModel";
            this.comboBox_multi_TSPModel.Size = new System.Drawing.Size(354, 21);
            this.comboBox_multi_TSPModel.TabIndex = 23;
            this.comboBox_multi_TSPModel.SelectedIndexChanged += new System.EventHandler(this.ComboBox_multi_TSPModel_SelectedIndexChanged);
            // 
            // comboBox_multi_problemModels
            // 
            this.comboBox_multi_problemModels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_multi_problemModels.FormattingEnabled = true;
            this.comboBox_multi_problemModels.Location = new System.Drawing.Point(6, 72);
            this.comboBox_multi_problemModels.Name = "comboBox_multi_problemModels";
            this.comboBox_multi_problemModels.Size = new System.Drawing.Size(356, 21);
            this.comboBox_multi_problemModels.TabIndex = 18;
            // 
            // label_multi_TSPModel
            // 
            this.label_multi_TSPModel.AutoSize = true;
            this.label_multi_TSPModel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_multi_TSPModel.ForeColor = System.Drawing.Color.Black;
            this.label_multi_TSPModel.Location = new System.Drawing.Point(4, 666);
            this.label_multi_TSPModel.Name = "label_multi_TSPModel";
            this.label_multi_TSPModel.Size = new System.Drawing.Size(162, 17);
            this.label_multi_TSPModel.TabIndex = 22;
            this.label_multi_TSPModel.Text = "TSP Model for Solver";
            // 
            // label_problemModel
            // 
            this.label_problemModel.AutoSize = true;
            this.label_problemModel.Location = new System.Drawing.Point(6, 56);
            this.label_problemModel.Name = "label_problemModel";
            this.label_problemModel.Size = new System.Drawing.Size(77, 13);
            this.label_problemModel.TabIndex = 20;
            this.label_problemModel.Text = "Problem Model";
            // 
            // label_problem
            // 
            this.label_problem.AutoSize = true;
            this.label_problem.Location = new System.Drawing.Point(6, 15);
            this.label_problem.Name = "label_problem";
            this.label_problem.Size = new System.Drawing.Size(45, 13);
            this.label_problem.TabIndex = 19;
            this.label_problem.Text = "Problem";
            // 
            // comboBox_multi_problems
            // 
            this.comboBox_multi_problems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_multi_problems.FormattingEnabled = true;
            this.comboBox_multi_problems.Location = new System.Drawing.Point(6, 32);
            this.comboBox_multi_problems.Name = "comboBox_multi_problems";
            this.comboBox_multi_problems.Size = new System.Drawing.Size(356, 21);
            this.comboBox_multi_problems.TabIndex = 17;
            // 
            // button_openDataManager
            // 
            this.button_openDataManager.Location = new System.Drawing.Point(6, 98);
            this.button_openDataManager.Name = "button_openDataManager";
            this.button_openDataManager.Size = new System.Drawing.Size(180, 23);
            this.button_openDataManager.TabIndex = 6;
            this.button_openDataManager.Text = "Open Data Manager";
            this.button_openDataManager.UseVisualStyleBackColor = true;
            this.button_openDataManager.Click += new System.EventHandler(this.Button_openDataManager_Click);
            // 
            // button_addProblem
            // 
            this.button_addProblem.Location = new System.Drawing.Point(194, 98);
            this.button_addProblem.Name = "button_addProblem";
            this.button_addProblem.Size = new System.Drawing.Size(167, 23);
            this.button_addProblem.TabIndex = 5;
            this.button_addProblem.Text = "Load Problem from File";
            this.button_addProblem.UseVisualStyleBackColor = true;
            this.button_addProblem.Click += new System.EventHandler(this.Button_addProblem_Click);
            // 
            // button_multi_viewProblem
            // 
            this.button_multi_viewProblem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_multi_viewProblem.Location = new System.Drawing.Point(6, 711);
            this.button_multi_viewProblem.Name = "button_multi_viewProblem";
            this.button_multi_viewProblem.Size = new System.Drawing.Size(180, 32);
            this.button_multi_viewProblem.TabIndex = 5;
            this.button_multi_viewProblem.Text = "View Problem";
            this.button_multi_viewProblem.UseVisualStyleBackColor = true;
            this.button_multi_viewProblem.Click += new System.EventHandler(this.Button_viewProblem_Click);
            // 
            // linkLabel_deleteSelectedProblem
            // 
            this.linkLabel_deleteSelectedProblem.AutoSize = true;
            this.linkLabel_deleteSelectedProblem.Location = new System.Drawing.Point(280, 125);
            this.linkLabel_deleteSelectedProblem.Name = "linkLabel_deleteSelectedProblem";
            this.linkLabel_deleteSelectedProblem.Size = new System.Drawing.Size(83, 13);
            this.linkLabel_deleteSelectedProblem.TabIndex = 5;
            this.linkLabel_deleteSelectedProblem.TabStop = true;
            this.linkLabel_deleteSelectedProblem.Text = "Delete Selected";
            this.linkLabel_deleteSelectedProblem.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_deleteSelectedProblem_LinkClicked);
            // 
            // listBox_problems
            // 
            this.listBox_problems.FormattingEnabled = true;
            this.listBox_problems.Location = new System.Drawing.Point(6, 142);
            this.listBox_problems.Name = "listBox_problems";
            this.listBox_problems.Size = new System.Drawing.Size(356, 316);
            this.listBox_problems.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button_run);
            this.groupBox2.Controls.Add(this.linkLabel_deleteSelectedAlgorithm);
            this.groupBox2.Controls.Add(this.button_addAlgo);
            this.groupBox2.Controls.Add(this.listBox_algorithms);
            this.groupBox2.Controls.Add(this.comboBox_algorithms);
            this.groupBox2.Location = new System.Drawing.Point(386, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(292, 739);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Algorithms";
            // 
            // button_run
            // 
            this.button_run.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_run.Location = new System.Drawing.Point(7, 700);
            this.button_run.Name = "button_run";
            this.button_run.Size = new System.Drawing.Size(279, 32);
            this.button_run.TabIndex = 4;
            this.button_run.Text = "RUN";
            this.button_run.UseVisualStyleBackColor = true;
            this.button_run.Click += new System.EventHandler(this.Button_run_Click);
            // 
            // linkLabel_deleteSelectedAlgorithm
            // 
            this.linkLabel_deleteSelectedAlgorithm.AutoSize = true;
            this.linkLabel_deleteSelectedAlgorithm.Location = new System.Drawing.Point(204, 52);
            this.linkLabel_deleteSelectedAlgorithm.Name = "linkLabel_deleteSelectedAlgorithm";
            this.linkLabel_deleteSelectedAlgorithm.Size = new System.Drawing.Size(83, 13);
            this.linkLabel_deleteSelectedAlgorithm.TabIndex = 3;
            this.linkLabel_deleteSelectedAlgorithm.TabStop = true;
            this.linkLabel_deleteSelectedAlgorithm.Text = "Delete Selected";
            this.linkLabel_deleteSelectedAlgorithm.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_deleteSelected_LinkClicked);
            // 
            // button_addAlgo
            // 
            this.button_addAlgo.Location = new System.Drawing.Point(228, 18);
            this.button_addAlgo.Name = "button_addAlgo";
            this.button_addAlgo.Size = new System.Drawing.Size(58, 23);
            this.button_addAlgo.TabIndex = 2;
            this.button_addAlgo.Text = "Add";
            this.button_addAlgo.UseVisualStyleBackColor = true;
            this.button_addAlgo.Click += new System.EventHandler(this.Button_addAlgo_Click);
            // 
            // listBox_algorithms
            // 
            this.listBox_algorithms.FormattingEnabled = true;
            this.listBox_algorithms.Location = new System.Drawing.Point(7, 66);
            this.listBox_algorithms.Name = "listBox_algorithms";
            this.listBox_algorithms.Size = new System.Drawing.Size(279, 628);
            this.listBox_algorithms.TabIndex = 1;
            // 
            // comboBox_algorithms
            // 
            this.comboBox_algorithms.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_algorithms.FormattingEnabled = true;
            this.comboBox_algorithms.Location = new System.Drawing.Point(7, 19);
            this.comboBox_algorithms.Name = "comboBox_algorithms";
            this.comboBox_algorithms.Size = new System.Drawing.Size(214, 21);
            this.comboBox_algorithms.TabIndex = 0;
            // 
            // textBox_log
            // 
            this.textBox_log.Location = new System.Drawing.Point(7, 20);
            this.textBox_log.Multiline = true;
            this.textBox_log.Name = "textBox_log";
            this.textBox_log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_log.Size = new System.Drawing.Size(264, 674);
            this.textBox_log.TabIndex = 0;
            this.textBox_log.WordWrap = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button_report);
            this.groupBox3.Controls.Add(this.textBox_log);
            this.groupBox3.Location = new System.Drawing.Point(683, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(279, 739);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Status";
            // 
            // button_report
            // 
            this.button_report.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_report.Location = new System.Drawing.Point(7, 700);
            this.button_report.Name = "button_report";
            this.button_report.Size = new System.Drawing.Size(262, 32);
            this.button_report.TabIndex = 5;
            this.button_report.Text = "Report";
            this.button_report.UseVisualStyleBackColor = true;
            this.button_report.Click += new System.EventHandler(this.Button_report_click);
            // 
            // MultipleProblemMultipleAlgorithm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(965, 768);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
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
        private System.Windows.Forms.Button button_multi_viewProblem;
        private System.Windows.Forms.Button button_addProblem;
        private System.Windows.Forms.Button button_openDataManager;
        private System.Windows.Forms.ComboBox comboBox_multi_problemModels;
        private System.Windows.Forms.Label label_problemModel;
        private System.Windows.Forms.Label label_problem;
        private System.Windows.Forms.ComboBox comboBox_multi_problems;
        private System.Windows.Forms.ComboBox comboBox_multi_TSPModel;
        private System.Windows.Forms.Label label_multi_TSPModel;
        private System.Windows.Forms.Button button_multi_createProblemModel;
        private System.Windows.Forms.Panel panel_multi_problemCharacteristics;
    }
}