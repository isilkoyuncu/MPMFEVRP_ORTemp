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
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;

namespace MPMFEVRP.Forms
{
    public partial class TS_Runs : Form
    {
        BindingList<IProblem> problems;
        BindingList<EVvsGDV_ProblemModel> problemModels;
        BindingList<ISolution> solutions;

        IProblem theProblem;
        EVvsGDV_ProblemModel theProblemModel;
        Outsource2Cplex theAlgorithm;
        ISolution theSolution;
        IWriter writer;
        Type TSPModelType;

        public TS_Runs()
        {
            InitializeComponent();

            problems = new BindingList<IProblem>();
            problemModels = new BindingList<EVvsGDV_ProblemModel>();

        }

        private void Button_solveEMHwithNDF_FF_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.NodeDuplicatingwoU);

            CreateEMHProblem();
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full);

            CreateProblemModels();

            Run();

        }
        private void Button_solveEMHwithADF_FF_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.ArcDuplicatingwoU);

            CreateEMHProblem();
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full);

            CreateProblemModels();

            Run();

        }
        private void Button_solveYCwithNDF_FF_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.NodeDuplicatingwoU);

            CreateYCProblem();
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full);

            CreateProblemModels();

            Run();

        }
        private void Button_solveYCwithADF_FF_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.ArcDuplicatingwoU);

            CreateEMHProblem();
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full);

            CreateProblemModels();

            Run();
        }

        void Run()
        {
            solutions = new BindingList<ISolution>();
            foreach (var problemModel in problemModels)
            {
                theAlgorithm.Initialize(problemModel);
                theAlgorithm.Run();
                theAlgorithm.Conclude();
                theSolution = theAlgorithm.Solution;
                solutions.Add(theSolution);
                writer = new IndividualSolutionWriter(problemModel.InputFileName, theAlgorithm.GetOutputSummary(), theSolution.GetOutputSummary(), theSolution.GetWritableSolution());
                writer.Write();
                theAlgorithm.Reset();
            }
        }

        void CreateEMHProblem()
        {
            theProblem = new EMH_Problem();
            AddProblems();
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_NUM_EV, 6);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_NUM_GDV, 0);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_LAMBDA, 1);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_USE_EXACTLY_NUM_EV_AVAILABLE, false);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_USE_EXACTLY_NUM_GDV_AVAILABLE, false);
            theProblemModel = new EMH_ProblemModel();
        }
        void CreateYCProblem()
        {
            theProblem = new EVvsGDV_MinCost_VRP();
            AddProblems();
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_NUM_EV, 7);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_NUM_GDV, 7);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_LAMBDA, 1);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_USE_EXACTLY_NUM_EV_AVAILABLE, false);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_USE_EXACTLY_NUM_GDV_AVAILABLE, false);
        }
        void CreateXcplexFormulation()
        {
            theAlgorithm = new Outsource2Cplex();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_EXPORT_LP_MODEL, false);
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_LOG_OUTPUT_TYPE, false);
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_RELAXATION, XCPlexRelaxation.None);
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_RUNTIME_SECONDS, 30.0);
        }
        private void AddProblems()
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
                        problems.Add(theProblem);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("There is something wrong while parsing the file!", "File parse error!");
                }
            }

        }
        private void CreateProblemModels()
        {
            try
            {
                for (int i = 0; i < problems.Count; i++)
                {
                    problems[i].ProblemCharacteristics.UpdateParameters(theProblem.ProblemCharacteristics);
                    TSPModelType = typeof(Models.XCPlex.XCPlex_ArcDuplicatingFormulation_woU);
                    theProblemModel = ProblemModelUtil.CreateProblemModelByProblem(theProblemModel.GetType(), problems[i], TSPModelType);
                    problemModels.Add(theProblemModel);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("There is something wrong while loading problem from the whole data!", "Problem loading error!");
            }
        }
    }
}
