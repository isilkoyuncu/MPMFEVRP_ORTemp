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

namespace MPMFEVRP.Forms
{
    public partial class SingleProblemSingleAlgorithm : Form
    {
        IProblem theProblem;
        IProblemModel theProblemModel;
        IAlgorithm theAlgorithm;

        public SingleProblemSingleAlgorithm()
        {
            InitializeComponent();

            comboBox_problems.Items.AddRange(ProblemUtil.GetAllProblemNames().ToArray());
            comboBox_problems.SelectedIndexChanged += ComboBox_problems_SelectedIndexChanged;
            comboBox_problems.SelectedIndex = 0;

            comboBox_algorithms.Items.AddRange(AlgorithmUtil.GetAllAlgorithmNames().ToArray());
            comboBox_algorithms.SelectedIndexChanged += ComboBox_algorithms_SelectedIndexChanged;
            comboBox_algorithms.SelectedIndex = 0;

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

        }

        private void button_viewProblem_Click(object sender, EventArgs e)
        {
            if (theProblem != null)
            {
                MessageBox.Show("This part is currently under development. It will eventually link to the new Problem Viewer.");
                //TODO: Revisit here after developing the new problem viewer, and then uncomment the next line as well as eliminate the message box in the line above.
                //new ProblemViewer(theProblem).Show();
            }
            else
                MessageBox.Show("You should create a problem first!", "No problem!");
        }

        void Log(string message)
        {
            textBox_log.AppendText(DateTime.Now.ToString("HH:mm:ss tt") + ": " + message + "\n");
        }

        private void button_browseForFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.RestoreDirectory = true;
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                label_selectedFile.Text = dialog.FileName;
                try
                {
                    //String fileContents = File.ReadAllText(dialog.FileName);
                    theProblem = ProblemUtil.CreateProblemByFileName(dialog.FileName);
                    UpdateProblemLabels();
                    Log("Problem loaded from file.");
                }
                catch (Exception)
                {
                    MessageBox.Show("There is something wrong while parsing the file!", "File parse error!");
                }
            }
        }

        private void button_run_Click(object sender, EventArgs e)
        {
            if (theProblem == null)
            {
                MessageBox.Show("Please load a problem first!", "No problem!");
            }
            else
            {
                IProblemModel model = new DefaultProblemModel(theProblem);
                Log("Algorithm initializing.");
                theAlgorithm.Initialize(model);
                Log("Algorithm running.");
                theAlgorithm.Run();
                Log("Algorithm concluding.");
                theAlgorithm.Conclude();
                Log("Algorithm finished.");
            }
        }

        private void button_viewSolution_Click(object sender, EventArgs e)
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

        private void button_openDataManager_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This part is currently under development. It will eventually link to the Data Manager, which is a different project within this environment.");
            //TODO: Fix the following line to the other project, not just a form within this project, and then uncomment it as well as eliminate the message box in the line above.
            //new DataManager().Show();
        }

    }
}
