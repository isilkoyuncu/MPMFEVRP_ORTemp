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
using MPMFEVRP.Models;
using MPMFEVRP.Implementations.Algorithms;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Models.XCPlex;

namespace MPMFEVRP.Forms
{
    public partial class SingleProblemSingleAlgorithm : Form
    {
        IProblem theProblem;
        EVvsGDV_ProblemModel theProblemModel;
        IAlgorithm theAlgorithm;
        //ISolution theSolution;
        Type TSPModelType;
        HybridTreeSearchAndSetPartitionCharts charts;

        public SingleProblemSingleAlgorithm()
        {
            InitializeComponent();
            groupBox_algorithms.Enabled = false;

            comboBox_problems.Items.AddRange(ProblemUtil.GetAllProblemNames().ToArray());
            comboBox_problems.SelectedIndexChanged += ComboBox_problems_SelectedIndexChanged;
            comboBox_problems.SelectedIndex = 0;

            comboBox_algorithms.Items.AddRange(AlgorithmUtil.GetAllAlgorithmNames().ToArray());
            comboBox_algorithms.SelectedIndexChanged += ComboBox_algorithms_SelectedIndexChanged;
            comboBox_algorithms.SelectedIndex = 0;

            comboBox_TSPModel.Items.AddRange(XCPlexUtil.GetTSPModelNamesForSolver().ToArray());
            comboBox_TSPModel.SelectedIndexChanged += ComboBox_TSPModel_SelectedIndexChanged;
            comboBox_TSPModel.SelectedIndex = 0;

            Log("Program started.");
        }

        private void ComboBox_algorithms_SelectedIndexChanged(object sender, EventArgs e)
        {
            theAlgorithm = AlgorithmUtil.CreateAlgorithmByName(comboBox_algorithms.SelectedItem.ToString());
            ParamUtil.DrawParameters(panel_parameters, theAlgorithm.AlgorithmParameters.GetAllParameters());
        }

        private void ComboBox_problems_SelectedIndexChanged(object sender, EventArgs e)
        {
            theProblem = ProblemUtil.CreateProblemByName(comboBox_problems.SelectedItem.ToString());
            if (theProblem == null)
                MessageBox.Show("We just selected the problem, but it failed to create!");
            else
            {
                comboBox_problemModels.Items.Clear();
                comboBox_problemModels.Items.AddRange(ProblemModelUtil.GetCompatibleProblemModelNames(theProblem).ToArray());
                comboBox_problemModels.SelectedIndexChanged += ComboBox_problemModels_SelectedIndexChanged;
                comboBox_problemModels.SelectedIndex = 0;
                UpdateProblemLabels();
            }
        }

        private void ComboBox_problemModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            theProblemModel = ProblemModelUtil.CreateProblemModelByName(comboBox_problemModels.SelectedItem.ToString());
            if (theProblemModel == null)
                MessageBox.Show("We just selected the problem, but it failed to create!");
            else
                UpdateProblemLabels();
        }

        void UpdateProblemLabels()
        {
            // TODO this should not be empty
        }

        private void Button_viewProblem_Click(object sender, EventArgs e)
        {
            if (theProblem != null)
            {
                //MessageBox.Show("This part is currently under development. It will eventually link to the new Problem Viewer.");
                //TODO: Revisit here after developing the new problem viewer, and then uncomment the next line as well as eliminate the message box in the line above.
                new ProblemViewer(theProblem).Show();
            }
            else
                MessageBox.Show("You should create a problem first!", "No problem!");
        }

        void Log(string message)
        {
            textBox_log.AppendText(DateTime.Now.ToString("HH:mm:ss tt") + ": " + message + "\n");
        }

        private void Button_browseForFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                RestoreDirectory = true,
                Multiselect = false
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                label_selectedFile.Text = dialog.FileName;
                try
                {
                    theProblem = ProblemUtil.CreateProblemByFileName(theProblem.GetName(), label_selectedFile.Text);
                    ParamUtil.DrawParameters(panel_problemCharacteristics, theProblem.ProblemCharacteristics.GetAllParameters());
                    Log("Problem data loaded from file.");
                }
                catch (Exception)
                {
                    MessageBox.Show("There is something wrong while parsing the file!", "File parse error!");
                }
            }
        }
        private void Button_createProblemModel_Click(object sender, EventArgs e)
        {
            try
            {
                TSPModelType = XCPlexUtil.GetXCPlexModelTypeByName(comboBox_TSPModel.SelectedItem.ToString());
                theProblemModel = ProblemModelUtil.CreateProblemModelByProblem(theProblemModel.GetType(), theProblem, TSPModelType);
                UpdateProblemLabels();
                Log("Problem model is created.");
                groupBox_algorithms.Enabled = true;
            }
            catch (Exception)
            {
                MessageBox.Show("There is something wrong while loading problem from the whole data!", "Problem loading error!");
            }
        }
        private void Button_run_Click(object sender, EventArgs e)
        {
            
            if (theProblem == null)
            {
                MessageBox.Show("Please load a problem first!", "No problem!");
            }
            else
            {
                Log("Algorithm initializing.");
                theAlgorithm.Initialize(theProblemModel);
                theAlgorithm.setBackgroundWorker(BackgroundWorker_algorithmRunner);

                button_showCharts.Enabled = true;
                button_showCharts_Click(sender, e);//This assumes the "charts" is compatible with the algorithm

                Log("Algorithm running.");
                BackgroundWorker_algorithmRunner.RunWorkerAsync();
            }
        }
        private void BackgroundWorker_algorithmRunner_DoWork(object sender, DoWorkEventArgs e)
        {
            theAlgorithm.Run();
        }

        private void BackgroundWorker_algorithmRunner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Log("Algorithm is finished");
            Log("Algorithm concluding.");
            theAlgorithm.Conclude();
            Log("Algorithm finished.");
        }

        private void BackgroundWorker_algorithmRunner_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void Button_viewSolution_Click(object sender, EventArgs e)
        {
            if (theAlgorithm != null && theAlgorithm.Solution != null)
            {
                theAlgorithm.Solution.View(theProblem);
            }
            else
            {
                MessageBox.Show("Run the algorithm first by selecting a problem!", "No solution!");
            }
        }

        private void Button_openDataManager_Click(object sender, EventArgs e)
        {
            new TestInstanceGenerator().ShowDialog();
        }

        private void ComboBox_TSPModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            TSPModelType = XCPlexUtil.GetXCPlexModelTypeByName(comboBox_TSPModel.SelectedItem.ToString());
        }

        private void button_exportDistances_Click(object sender, EventArgs e)
        {
            theProblemModel.ExportDistancesAsTxt();
        }

        private void button_exportEnergyConsmp_Click(object sender, EventArgs e)
        {
            theProblemModel.ExportEnergyConsumpionAsTxt();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            theProblemModel.ExportTravelDurationAsTxt();
        }

        private void button_showCharts_Click(object sender, EventArgs e)
        {
            if (charts != null && charts.Visible)
            {
                charts.Focus();
            }
            else
            {
                charts = new HybridTreeSearchAndSetPartitionCharts();//TODO: Discuss with Huseyin: Is this a good implementation with not much coupling? 
                theAlgorithm.setListener(charts);
                charts.Show();
            }
        }
    }
}
