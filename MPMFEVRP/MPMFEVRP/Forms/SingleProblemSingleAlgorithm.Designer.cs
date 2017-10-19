namespace MPMFEVRP.Forms
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
            this.button_exportTravelDuration = new System.Windows.Forms.Button();
            this.button_exportEnergyConsmp = new System.Windows.Forms.Button();
            this.button_exportDistances = new System.Windows.Forms.Button();
            this.comboBox_TSPModel = new System.Windows.Forms.ComboBox();
            this.label_TSPModel = new System.Windows.Forms.Label();
            this.button_createProblemModel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_selectedFile = new System.Windows.Forms.Label();
            this.button_browseForFile = new System.Windows.Forms.Button();
            this.button_openDataManager = new System.Windows.Forms.Button();
            this.panel_problemCharacteristics = new System.Windows.Forms.Panel();
            this.comboBox_problemModels = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_problems = new System.Windows.Forms.ComboBox();
            this.button_viewProblem = new System.Windows.Forms.Button();
            this.groupBox_algorithms = new System.Windows.Forms.GroupBox();
            this.button_run = new System.Windows.Forms.Button();
            this.panel_parameters = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBox_algorithms = new System.Windows.Forms.ComboBox();
            this.BackgroundWorker_algorithmRunner = new System.ComponentModel.BackgroundWorker();
            this.button_viewSolution = new System.Windows.Forms.Button();
            this.groupBox_status = new System.Windows.Forms.GroupBox();
            this.button_extractLogInfo = new System.Windows.Forms.Button();
            this.button_showCharts = new System.Windows.Forms.Button();
            this.textBox_log = new System.Windows.Forms.TextBox();
            this.groupBox_problem.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox_algorithms.SuspendLayout();
            this.groupBox_status.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_problem
            // 
            this.groupBox_problem.Controls.Add(this.button_exportTravelDuration);
            this.groupBox_problem.Controls.Add(this.button_exportEnergyConsmp);
            this.groupBox_problem.Controls.Add(this.button_exportDistances);
            this.groupBox_problem.Controls.Add(this.comboBox_TSPModel);
            this.groupBox_problem.Controls.Add(this.label_TSPModel);
            this.groupBox_problem.Controls.Add(this.button_createProblemModel);
            this.groupBox_problem.Controls.Add(this.label1);
            this.groupBox_problem.Controls.Add(this.groupBox1);
            this.groupBox_problem.Controls.Add(this.panel_problemCharacteristics);
            this.groupBox_problem.Controls.Add(this.comboBox_problemModels);
            this.groupBox_problem.Controls.Add(this.label3);
            this.groupBox_problem.Controls.Add(this.label2);
            this.groupBox_problem.Controls.Add(this.comboBox_problems);
            this.groupBox_problem.Controls.Add(this.button_viewProblem);
            this.groupBox_problem.Location = new System.Drawing.Point(17, 16);
            this.groupBox_problem.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox_problem.Name = "groupBox_problem";
            this.groupBox_problem.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox_problem.Size = new System.Drawing.Size(595, 798);
            this.groupBox_problem.TabIndex = 0;
            this.groupBox_problem.TabStop = false;
            this.groupBox_problem.Text = "The \"Problem\" Side (problem, model, data, etc.)";
            // 
            // button_exportTravelDuration
            // 
            this.button_exportTravelDuration.Location = new System.Drawing.Point(431, 709);
            this.button_exportTravelDuration.Margin = new System.Windows.Forms.Padding(4);
            this.button_exportTravelDuration.Name = "button_exportTravelDuration";
            this.button_exportTravelDuration.Size = new System.Drawing.Size(149, 28);
            this.button_exportTravelDuration.TabIndex = 24;
            this.button_exportTravelDuration.Text = "Export Travel Times";
            this.button_exportTravelDuration.UseVisualStyleBackColor = true;
            this.button_exportTravelDuration.Click += new System.EventHandler(this.Button_exportTravelDuration_Click);
            // 
            // button_exportEnergyConsmp
            // 
            this.button_exportEnergyConsmp.Location = new System.Drawing.Point(192, 709);
            this.button_exportEnergyConsmp.Margin = new System.Windows.Forms.Padding(4);
            this.button_exportEnergyConsmp.Name = "button_exportEnergyConsmp";
            this.button_exportEnergyConsmp.Size = new System.Drawing.Size(204, 28);
            this.button_exportEnergyConsmp.TabIndex = 23;
            this.button_exportEnergyConsmp.Text = "Export Energy Consumption";
            this.button_exportEnergyConsmp.UseVisualStyleBackColor = true;
            this.button_exportEnergyConsmp.Click += new System.EventHandler(this.Button_exportEnergyConsmp_Click);
            // 
            // button_exportDistances
            // 
            this.button_exportDistances.Location = new System.Drawing.Point(8, 709);
            this.button_exportDistances.Margin = new System.Windows.Forms.Padding(4);
            this.button_exportDistances.Name = "button_exportDistances";
            this.button_exportDistances.Size = new System.Drawing.Size(149, 28);
            this.button_exportDistances.TabIndex = 22;
            this.button_exportDistances.Text = "Export Distances";
            this.button_exportDistances.UseVisualStyleBackColor = true;
            this.button_exportDistances.Click += new System.EventHandler(this.Button_exportDistances_Click);
            // 
            // comboBox_TSPModel
            // 
            this.comboBox_TSPModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_TSPModel.FormattingEnabled = true;
            this.comboBox_TSPModel.Location = new System.Drawing.Point(8, 658);
            this.comboBox_TSPModel.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_TSPModel.Name = "comboBox_TSPModel";
            this.comboBox_TSPModel.Size = new System.Drawing.Size(571, 24);
            this.comboBox_TSPModel.TabIndex = 21;
            this.comboBox_TSPModel.SelectedIndexChanged += new System.EventHandler(this.ComboBox_TSPModel_SelectedIndexChanged);
            // 
            // label_TSPModel
            // 
            this.label_TSPModel.AutoSize = true;
            this.label_TSPModel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_TSPModel.ForeColor = System.Drawing.Color.Black;
            this.label_TSPModel.Location = new System.Drawing.Point(77, 624);
            this.label_TSPModel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_TSPModel.Name = "label_TSPModel";
            this.label_TSPModel.Size = new System.Drawing.Size(188, 20);
            this.label_TSPModel.TabIndex = 20;
            this.label_TSPModel.Text = "TSP Model for Solver";
            // 
            // button_createProblemModel
            // 
            this.button_createProblemModel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_createProblemModel.Location = new System.Drawing.Point(304, 754);
            this.button_createProblemModel.Margin = new System.Windows.Forms.Padding(4);
            this.button_createProblemModel.Name = "button_createProblemModel";
            this.button_createProblemModel.Size = new System.Drawing.Size(283, 34);
            this.button_createProblemModel.TabIndex = 19;
            this.button_createProblemModel.Text = "Create Problem Model";
            this.button_createProblemModel.UseVisualStyleBackColor = true;
            this.button_createProblemModel.Click += new System.EventHandler(this.Button_createProblemModel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(77, 282);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(213, 20);
            this.label1.TabIndex = 18;
            this.label1.Text = "Problem Characteristics";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label_selectedFile);
            this.groupBox1.Controls.Add(this.button_browseForFile);
            this.groupBox1.Controls.Add(this.button_openDataManager);
            this.groupBox1.Location = new System.Drawing.Point(8, 124);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(579, 154);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Load From File";
            // 
            // label_selectedFile
            // 
            this.label_selectedFile.Location = new System.Drawing.Point(16, 65);
            this.label_selectedFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_selectedFile.Name = "label_selectedFile";
            this.label_selectedFile.Size = new System.Drawing.Size(555, 73);
            this.label_selectedFile.TabIndex = 4;
            this.label_selectedFile.Text = "No file selected yet!";
            // 
            // button_browseForFile
            // 
            this.button_browseForFile.Location = new System.Drawing.Point(16, 30);
            this.button_browseForFile.Margin = new System.Windows.Forms.Padding(4);
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
            this.button_openDataManager.Margin = new System.Windows.Forms.Padding(4);
            this.button_openDataManager.Name = "button_openDataManager";
            this.button_openDataManager.Size = new System.Drawing.Size(149, 28);
            this.button_openDataManager.TabIndex = 5;
            this.button_openDataManager.Text = "Open Data Manager";
            this.button_openDataManager.UseVisualStyleBackColor = true;
            this.button_openDataManager.Click += new System.EventHandler(this.Button_openDataManager_Click);
            // 
            // panel_problemCharacteristics
            // 
            this.panel_problemCharacteristics.Location = new System.Drawing.Point(8, 316);
            this.panel_problemCharacteristics.Margin = new System.Windows.Forms.Padding(4);
            this.panel_problemCharacteristics.Name = "panel_problemCharacteristics";
            this.panel_problemCharacteristics.Size = new System.Drawing.Size(579, 304);
            this.panel_problemCharacteristics.TabIndex = 17;
            // 
            // comboBox_problemModels
            // 
            this.comboBox_problemModels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_problemModels.FormattingEnabled = true;
            this.comboBox_problemModels.Location = new System.Drawing.Point(8, 92);
            this.comboBox_problemModels.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_problemModels.Name = "comboBox_problemModels";
            this.comboBox_problemModels.Size = new System.Drawing.Size(571, 24);
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
            this.comboBox_problems.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_problems.Name = "comboBox_problems";
            this.comboBox_problems.Size = new System.Drawing.Size(571, 24);
            this.comboBox_problems.TabIndex = 12;
            // 
            // button_viewProblem
            // 
            this.button_viewProblem.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_viewProblem.Location = new System.Drawing.Point(8, 754);
            this.button_viewProblem.Margin = new System.Windows.Forms.Padding(4);
            this.button_viewProblem.Name = "button_viewProblem";
            this.button_viewProblem.Size = new System.Drawing.Size(283, 34);
            this.button_viewProblem.TabIndex = 8;
            this.button_viewProblem.Text = "View Problem";
            this.button_viewProblem.UseVisualStyleBackColor = true;
            this.button_viewProblem.Click += new System.EventHandler(this.Button_viewProblem_Click);
            // 
            // groupBox_algorithms
            // 
            this.groupBox_algorithms.Controls.Add(this.button_run);
            this.groupBox_algorithms.Controls.Add(this.panel_parameters);
            this.groupBox_algorithms.Controls.Add(this.label9);
            this.groupBox_algorithms.Controls.Add(this.comboBox_algorithms);
            this.groupBox_algorithms.Location = new System.Drawing.Point(635, 16);
            this.groupBox_algorithms.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox_algorithms.Name = "groupBox_algorithms";
            this.groupBox_algorithms.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox_algorithms.Size = new System.Drawing.Size(467, 798);
            this.groupBox_algorithms.TabIndex = 1;
            this.groupBox_algorithms.TabStop = false;
            this.groupBox_algorithms.Text = "Algorithm";
            // 
            // button_run
            // 
            this.button_run.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_run.Location = new System.Drawing.Point(9, 754);
            this.button_run.Margin = new System.Windows.Forms.Padding(4);
            this.button_run.Name = "button_run";
            this.button_run.Size = new System.Drawing.Size(449, 34);
            this.button_run.TabIndex = 11;
            this.button_run.Text = "RUN";
            this.button_run.UseVisualStyleBackColor = true;
            this.button_run.Click += new System.EventHandler(this.Button_run_Click);
            // 
            // panel_parameters
            // 
            this.panel_parameters.Location = new System.Drawing.Point(9, 84);
            this.panel_parameters.Margin = new System.Windows.Forms.Padding(4);
            this.panel_parameters.Name = "panel_parameters";
            this.panel_parameters.Size = new System.Drawing.Size(449, 654);
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
            this.comboBox_algorithms.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_algorithms.Name = "comboBox_algorithms";
            this.comboBox_algorithms.Size = new System.Drawing.Size(449, 24);
            this.comboBox_algorithms.TabIndex = 11;
            // 
            // BackgroundWorker_algorithmRunner
            // 
            this.BackgroundWorker_algorithmRunner.WorkerReportsProgress = true;
            this.BackgroundWorker_algorithmRunner.WorkerSupportsCancellation = true;
            this.BackgroundWorker_algorithmRunner.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker_algorithmRunner_DoWork);
            this.BackgroundWorker_algorithmRunner.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWorker_algorithmRunner_ProgressChanged);
            this.BackgroundWorker_algorithmRunner.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker_algorithmRunner_RunWorkerCompleted);
            // 
            // button_viewSolution
            // 
            this.button_viewSolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_viewSolution.Location = new System.Drawing.Point(224, 754);
            this.button_viewSolution.Margin = new System.Windows.Forms.Padding(4);
            this.button_viewSolution.Name = "button_viewSolution";
            this.button_viewSolution.Size = new System.Drawing.Size(208, 34);
            this.button_viewSolution.TabIndex = 13;
            this.button_viewSolution.Text = "View Solution";
            this.button_viewSolution.UseVisualStyleBackColor = true;
            this.button_viewSolution.Click += new System.EventHandler(this.Button_viewSolution_Click);
            // 
            // groupBox_status
            // 
            this.groupBox_status.Controls.Add(this.button_extractLogInfo);
            this.groupBox_status.Controls.Add(this.button_showCharts);
            this.groupBox_status.Controls.Add(this.textBox_log);
            this.groupBox_status.Controls.Add(this.button_viewSolution);
            this.groupBox_status.Location = new System.Drawing.Point(1119, 16);
            this.groupBox_status.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox_status.Name = "groupBox_status";
            this.groupBox_status.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox_status.Size = new System.Drawing.Size(443, 798);
            this.groupBox_status.TabIndex = 2;
            this.groupBox_status.TabStop = false;
            this.groupBox_status.Text = "Status";
            // 
            // button_extractLogInfo
            // 
            this.button_extractLogInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_extractLogInfo.Location = new System.Drawing.Point(225, 628);
            this.button_extractLogInfo.Margin = new System.Windows.Forms.Padding(4);
            this.button_extractLogInfo.Name = "button_extractLogInfo";
            this.button_extractLogInfo.Size = new System.Drawing.Size(208, 34);
            this.button_extractLogInfo.TabIndex = 16;
            this.button_extractLogInfo.Text = "Extract Info From CPLEX Log";
            this.button_extractLogInfo.UseVisualStyleBackColor = true;
            this.button_extractLogInfo.Click += new System.EventHandler(this.ExtractLogInfo);
            // 
            // button_showCharts
            // 
            this.button_showCharts.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_showCharts.Location = new System.Drawing.Point(8, 754);
            this.button_showCharts.Margin = new System.Windows.Forms.Padding(4);
            this.button_showCharts.Name = "button_showCharts";
            this.button_showCharts.Size = new System.Drawing.Size(208, 34);
            this.button_showCharts.TabIndex = 15;
            this.button_showCharts.Text = "Show Charts";
            this.button_showCharts.UseVisualStyleBackColor = true;
            this.button_showCharts.Click += new System.EventHandler(this.Button_showCharts_Click);
            // 
            // textBox_log
            // 
            this.textBox_log.Location = new System.Drawing.Point(8, 25);
            this.textBox_log.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_log.Multiline = true;
            this.textBox_log.Name = "textBox_log";
            this.textBox_log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_log.Size = new System.Drawing.Size(425, 595);
            this.textBox_log.TabIndex = 14;
            // 
            // SingleProblemSingleAlgorithm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1567, 826);
            this.Controls.Add(this.groupBox_status);
            this.Controls.Add(this.groupBox_algorithms);
            this.Controls.Add(this.groupBox_problem);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4);
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
        private System.Windows.Forms.Panel panel_problemCharacteristics;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_createProblemModel;
        private System.Windows.Forms.ComboBox comboBox_TSPModel;
        private System.Windows.Forms.Label label_TSPModel;
        private System.Windows.Forms.Button button_exportTravelDuration;
        private System.Windows.Forms.Button button_exportEnergyConsmp;
        private System.Windows.Forms.Button button_exportDistances;
        private System.ComponentModel.BackgroundWorker BackgroundWorker_algorithmRunner;
        private System.Windows.Forms.Button button_showCharts;
        private System.Windows.Forms.Button button_extractLogInfo;
    }
}