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
using MPMFEVRP.Implementations.Solutions.Writers;
using MPMFEVRP.Implementations.Algorithms;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;


namespace MPMFEVRP.Forms
{
    public partial class MultipleProblemMultipleAlgorithm : Form
    {
        BindingList<IAlgorithm> algorithms;
        BindingList<IProblem> problems;
        BindingList<EVvsGDV_ProblemModel> problemModels;
        BindingList<ISolution> solutions; 

        IProblem theProblem;
        EVvsGDV_ProblemModel theProblemModel;
        IAlgorithm theAlgorithm;
        ISolution theSolution;
        IWriter writer;
        Type TSPModelType;

        List<string[]> solutionSummaryList;
        string[] solutionSummary;
        public MultipleProblemMultipleAlgorithm()
        {
            InitializeComponent();

            problems = new BindingList<IProblem>();
            listBox_problems.DataSource = problems;
            problemModels = new BindingList<EVvsGDV_ProblemModel>();

            algorithms = new BindingList<IAlgorithm>();
            listBox_algorithms.DataSource = algorithms;

            comboBox_multi_problems.Items.AddRange(ProblemUtil.GetAllProblemNames().ToArray());
            comboBox_multi_problems.SelectedIndexChanged += ComboBox_multi_problems_SelectedIndexChanged;
            comboBox_multi_problems.SelectedIndex = 0;

            listBox_algorithms.MouseDoubleClick += ListBox_algorithms_MouseDoubleClick;
            listBox_algorithms.MouseMove += ListBox_algorithms_MouseMove;
            comboBox_algorithms.Items.AddRange(AlgorithmUtil.GetAllAlgorithmNames().ToArray());
            comboBox_algorithms.SelectedIndex = 0;

            comboBox_multi_TSPModel.Items.AddRange(XCPlexUtil.GetTSPModelNamesForSolver().ToArray());
            comboBox_multi_TSPModel.SelectedIndexChanged += ComboBox_multi_TSPModel_SelectedIndexChanged;
            comboBox_multi_TSPModel.SelectedIndex = 0;
        }

        private void ComboBox_multi_problems_SelectedIndexChanged(object sender, EventArgs e)
        {
            theProblem = ProblemUtil.CreateProblemByName(comboBox_multi_problems.SelectedItem.ToString());
            ParamUtil.DrawParameters(panel_multi_problemCharacteristics, theProblem.ProblemCharacteristics.GetAllParameters());
            if (theProblem == null)
                MessageBox.Show("We just selected the problem, but it failed to create!");
            else
            {
                comboBox_multi_problemModels.Items.Clear();
                comboBox_multi_problemModels.Items.AddRange(ProblemModelUtil.GetCompatibleProblemModelNames(theProblem).ToArray());
                comboBox_multi_problemModels.SelectedIndexChanged += ComboBox_multi_problemModels_SelectedIndexChanged;
                comboBox_multi_problemModels.SelectedIndex = 0;
                UpdateProblemLabels();
            }
        }
        private void ComboBox_multi_problemModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            theProblemModel = ProblemModelUtil.CreateProblemModelByName(comboBox_multi_problemModels.SelectedItem.ToString());
            if (theProblemModel == null)
                MessageBox.Show("We just selected the problem, but it failed to create!");
            else
                UpdateProblemLabels();
        }
        void UpdateProblemLabels()
        {
            // TODO this should not be empty
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
            theAlgorithm = AlgorithmUtil.CreateAlgorithmByName(comboBox_algorithms.SelectedItem.ToString());
            algorithms.Add(theAlgorithm);
            new AlgorithmViewer(theAlgorithm).ShowDialog();
            listBox_algorithms.DataSource = null;
            listBox_algorithms.DataSource = algorithms;
            algorithms.ResetBindings();
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
            //MessageBox.Show("This feature is coming...", "Coming...");
            // Koyuncu Yavuz solution reader
            OpenFileDialog dialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                RestoreDirectory = true,
                Multiselect = true
            };
            solutionSummaryList = new List<string[]>();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    for (int i = 0; i < dialog.FileNames.Length; i++)
                    {
                        solutionSummary = SolutionUtil.ReadSolutionByFileName(dialog.FileNames[i]);
                        solutionSummaryList.Add(solutionSummary);
                    }

                    writer = new KoyuncuYavuzSummaryWriter(dialog.FileNames[0], solutionSummaryList);
                    writer.Write();
                    Log("Solution summary is written!");
                }
                catch (Exception)
                {
                    MessageBox.Show("There is something wrong while parsing the file!", "File parse error!");
                }
            }
            // Koyuncu Yavuz summary writer
        }

        private void Button_run_Click(object sender, EventArgs e)
        {
            solutions = new BindingList<ISolution>();
            foreach (var algorithm in algorithms)
            {
                foreach (var problemModel in problemModels)
                {
                    //TODO The following line is from the template, which simply assumes that each problem can be worked using onyl a single model, and there is exactly one problem(type) in nthe entire project.
                    //Now that we have enriched the project, the line below must be corrected to handle differnet problem models for different problems

                    if (problemModel == null)
                    {
                        MessageBox.Show("Please load a problem first!", "No problem!");
                    }
                    else
                    {
                        algorithm.Initialize(problemModel);
                        Log("Algorithm " + algorithm.ToString() + " is initialized.");

                        Log("Algorithm " + algorithm.ToString() + " started running.");
                        algorithm.Run();

                        Log("Algorithm " + algorithm.ToString() + " finished.");
                        Log("================");
                        algorithm.Conclude();
                        theSolution = algorithm.Solution;
                        solutions.Add(theSolution);
                        Log("Solution " + theSolution.ToString() + " started writing.");
                        writer = new IndividualSolutionWriter(problemModel.InputFileName, algorithm.GetOutputSummary(), theSolution.GetOutputSummary(), theSolution.GetWritableSolution());
                        writer.Write();
                        Log("**************RESET************");
                        algorithm.Reset();
                    }
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
                Multiselect = true
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    for (int i = 0; i < dialog.FileNames.Length; i++)
                    {
                        theProblem = ProblemUtil.CreateProblemByFileName(theProblem.GetName(), dialog.FileNames[i]);
                        theProblemModel = ProblemModelUtil.CreateProblemModelByProblem(theProblemModel.GetType(), theProblem, TSPModelType);

                        problems.Add(theProblem);
                        problemModels.Add(theProblemModel);

                        Log("Problem loaded from file "+theProblem.PDP.InputFileName);
                    }


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
                //TODO here select the problem from listbox first! theProblem is the last problem selected from computer if you want to see any problem from the list box then change this code
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

        private void ComboBox_multi_TSPModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            TSPModelType = XCPlexUtil.GetXCPlexModelTypeByName(comboBox_multi_TSPModel.SelectedItem.ToString());
        }

        private void Button_multi_createProblemModel_Click(object sender, EventArgs e)
        {
            try
            {
                TSPModelType = XCPlexUtil.GetXCPlexModelTypeByName(comboBox_multi_TSPModel.SelectedItem.ToString());
                theProblemModel = ProblemModelUtil.CreateProblemModelByProblem(theProblemModel.GetType(), theProblem, TSPModelType);
                UpdateProblemLabels();
                Log("Problem model is created.");
                comboBox_algorithms.Enabled = true;
            }
            catch (Exception)
            {
                MessageBox.Show("There is something wrong while loading problem from the whole data!", "Problem loading error!");
            }
        }
    }
}
