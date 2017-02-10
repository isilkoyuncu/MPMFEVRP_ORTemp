﻿using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MPMFEVRP.Forms
{
    public partial class MultipleProblemMultipleAlgorithm : Form
    {
        BindingList<IAlgorithm> algorithms;
        BindingList<IProblem> problems;

        public MultipleProblemMultipleAlgorithm()
        {
            InitializeComponent();

            problems = new BindingList<IProblem>();
            listBox_problems.DataSource = problems;

            algorithms = new BindingList<IAlgorithm>();
            listBox_algorithms.DataSource = algorithms;

            listBox_algorithms.MouseDoubleClick += ListBox_algorithms_MouseDoubleClick;
            listBox_algorithms.MouseMove += ListBox_algorithms_MouseMove;
            comboBox_algorithms.Items.AddRange(AlgorithmUtil.GetAllAlgorithmNames().ToArray());
            comboBox_algorithms.SelectedIndex = 0;
        }

        private void ListBox_algorithms_MouseMove(object sender, MouseEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            int index = lb.IndexFromPoint(e.Location);

            if (index >= 0 && index < lb.Items.Count)
            {
                string toolTipString = lb.Items[index].ToString();

                if (toolTip1.GetToolTip(lb) != toolTipString)
                    toolTip1.SetToolTip(lb, toolTipString);
            }
            else
                toolTip1.Hide(lb);
        }

        private void ListBox_algorithms_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox_algorithms.SelectedIndex != -1)
                new AlgorithmViewer(algorithms[listBox_algorithms.SelectedIndex]).ShowDialog();
        }

        private void button_addAlgo_Click(object sender, EventArgs e)
        {
            IAlgorithm algorithm = AlgorithmUtil.CreateAlgorithmByName(comboBox_algorithms.SelectedItem.ToString());
            algorithms.Add(algorithm);
            new AlgorithmViewer(algorithm).ShowDialog();
        }

        private void linkLabel_deleteSelected_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (listBox_algorithms.SelectedIndex != -1)
                algorithms.RemoveAt(listBox_algorithms.SelectedIndex);
        }

        void Log(string message)
        {
            textBox_log.AppendText(DateTime.Now.ToString("HH:mm:ss tt") + ": " + message + "\n");
            textBox_log.SelectionStart = textBox_log.GetFirstCharIndexOfCurrentLine();
            textBox_log.SelectionLength = 1;
            textBox_log.ScrollToCaret();
        }

        private void button_report_click(object sender, EventArgs e)
        {
            MessageBox.Show("This feature is coming...", "Coming...");
        }

        private void button_run_Click(object sender, EventArgs e)
        {
            foreach (var algorithm in algorithms)
            {
                foreach (var problem in problems)
                {
                    IProblemModel model = new DefaultProblemModel(problem);

                    algorithm.Initialize(model);
                    Log("Algorithm " + algorithm.ToString() + " is initialized.");

                    Log("Algorithm " + algorithm.ToString() + " started running.");
                    algorithm.Run();

                    Log("Algorithm " + algorithm.ToString() + " finished.");
                    Log("================");
                    // TODO algorithm statistics
                    //log("Solution status: " + algorithm.Solution.Status);
                    //log("Total cost: " + algorithm.Solution.TotalCost);
                    //log("Run time: " + algorithm.AlgorithmStatistics.RunTimeMilliSeconds + " ms");

                    Log("**************RESET************");
                    algorithm.Reset();
                }
            }
        }

        private void linkLabel_deleteSelectedProblem_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (listBox_problems.SelectedIndex != -1)
                problems.RemoveAt(listBox_problems.SelectedIndex);
        }

        private void button_addProblem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.RestoreDirectory = true;
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    String fileContents = File.ReadAllText(dialog.FileName);
                    IProblem theProblem = ProblemUtil.CreateProblemByRawData(fileContents);
                    problems.Add(theProblem);
                    Log("Problem loaded from file.");
                }
                catch (Exception)
                {
                    MessageBox.Show("There is something wrong while parsing the file!", "File parse error!");
                }
            }
        }

        private void button_viewProblem_Click(object sender, EventArgs e)
        {
            if (listBox_problems.SelectedIndex != -1)
                new ProblemViewer((IProblem)listBox_problems.SelectedItem).ShowDialog();
        }

        private void button_openDataManager_Click(object sender, EventArgs e)
        {
            new DataManager().Show();
        }
    }
}
