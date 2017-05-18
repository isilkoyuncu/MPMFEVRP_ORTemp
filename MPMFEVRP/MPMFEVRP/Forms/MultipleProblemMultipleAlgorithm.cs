using MPMFEVRP.Implementations.ProblemModels;
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
using Instance_Generation.Forms;

namespace MPMFEVRP.Forms
{
    public partial class MultipleProblemMultipleAlgorithm : Form
    {
        BindingList<IAlgorithm> algorithms;
        BindingList<IProblem> problems;
        BindingList<IProblemModel> problemModels;

        IProblem theProblem;
        ProblemModelBase theProblemModel;
        IAlgorithm theAlgorithm;

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

        private void Button_addAlgo_Click(object sender, EventArgs e)
        {
            IAlgorithm algorithm = AlgorithmUtil.CreateAlgorithmByName(comboBox_algorithms.SelectedItem.ToString());
            algorithms.Add(algorithm);
            new AlgorithmViewer(algorithm).ShowDialog();
        }

        private void LinkLabel_deleteSelected_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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

        private void Button_report_click(object sender, EventArgs e)
        {
            MessageBox.Show("This feature is coming...", "Coming...");
        }

        private void Button_run_Click(object sender, EventArgs e)
        {
            foreach (var algorithm in algorithms)
            {
                foreach (var problem in problems)
                {
                    // TODO The following line is from the template, which simply assumes that each problem can be worked using onyl a single model, and there is exactly one problem (type) in nthe entire project.
                    // Now that we have enriched the project, the line below must be corrected to handle differnet problem models for different problems 
                    throw new NotImplementedException();
                    //IProblemModel model = new DefaultProblemModel(problem);

                    //algorithm.Initialize(model);
                    //Log("Algorithm " + algorithm.ToString() + " is initialized.");

                    //Log("Algorithm " + algorithm.ToString() + " started running.");
                    //algorithm.Run();

                    //Log("Algorithm " + algorithm.ToString() + " finished.");
                    //Log("================");
                    //// TODO algorithm statistics
                    ////log("Solution status: " + algorithm.Solution.Status);
                    ////log("Total cost: " + algorithm.Solution.TotalCost);
                    ////log("Run time: " + algorithm.AlgorithmStatistics.RunTimeMilliSeconds + " ms");

                    //Log("**************RESET************");
                    //algorithm.Reset();
                }
            }
        }

        private void LinkLabel_deleteSelectedProblem_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (listBox_problems.SelectedIndex != -1)
                problems.RemoveAt(listBox_problems.SelectedIndex);
        }

        private void Button_addProblem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                RestoreDirectory = true,
                Multiselect = false
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    theProblem = ProblemUtil.CreateProblemByFileName(dialog.FileName);
                    theProblemModel = ProblemModelUtil.CreateProblemModelByProblem(theProblem);
                    
                    problems.Add(theProblem);

                    problemModels = new BindingList<IProblemModel>();
                    problemModels.Add(theProblemModel);

                    Log("Problem loaded from file.");
                }
                catch (Exception)
                {
                    MessageBox.Show("There is something wrong while parsing the file!", "File parse error!");
                }
            }
        }

        private void Button_viewProblem_Click(object sender, EventArgs e)
        {
            if (problemModels != null)
            {
                MessageBox.Show("This part is currently under development. It will eventually link to the new Problem Viewer.");
                //TODO: Revisit here after developing the new problem viewer, and then uncomment the next line as well as eliminate the message box in the line above.
                //new ProblemViewer(theProblem).Show();
            }
            else
                MessageBox.Show("You should create a problem first!", "No problem!");
        }

        private void Button_openDataManager_Click(object sender, EventArgs e)
        {
            new TestInstanceGenerator().ShowDialog();
        }
    }
}
