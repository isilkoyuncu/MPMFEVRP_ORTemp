using MPMFEVRP.Interfaces;
using MPMFEVRP.Utils;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using MPMFEVRP.Implementations.Solutions.Writers;
using MPMFEVRP.Implementations.Algorithms;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using System.Diagnostics;


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

        bool writeOnFile = true;
        double runTime = 3600.0;
        string probShortName;
        string formulationName;
        string refuelingOpt;
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
            formulationName = "NDF";
            CreateEMHProblem();
            probShortName = "EMH";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full);
            refuelingOpt = "FF";
            CreateProblemModels();
            Run();
        }
        private void Button_solveEMHwithADF_FF_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.ArcDuplicatingwoU);
            formulationName = "ADF";
            CreateEMHProblem();
            probShortName = "EMH";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full);
            refuelingOpt = "FF";
            CreateProblemModels();
            Run();
        }
        private void Button_solveYCwithNDF_FF_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.NodeDuplicatingwoU);
            formulationName = "NDF";
            CreateYCProblem();
            probShortName = "YC";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full);
            refuelingOpt = "FF";
            CreateProblemModels();
            Run();
        }
        private void Button_solveYCwithADF_FF_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.ArcDuplicatingwoU);
            formulationName = "ADF";
            CreateEMHProblem();
            probShortName = "YC";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full);
            refuelingOpt = "FF";
            CreateProblemModels();
            Run();
        }

        void Run()
        {
            solutions = new BindingList<ISolution>();
            int index = 1;
            foreach (var problemModel in problemModels)
            {
                theAlgorithm.Initialize(problemModel);
                Debug.WriteLine("The algorithm is initialized: " + theAlgorithm.GetName() + "\t" + DateTime.Now);
                theAlgorithm.Run();
                Debug.WriteLine("Started solving: " + problemModel.InputFileName +"\t"+ DateTime.Now);
                Debug.WriteLine("************************************************");
                theAlgorithm.Conclude();
                theSolution = theAlgorithm.Solution;
                solutions.Add(theSolution);
                writer = new IndividualSolutionWriter(index, probShortName,formulationName,refuelingOpt,runTime, theAlgorithm.GetOutputSummary(), theSolution.GetOutputSummary(), theSolution.GetWritableSolution());
                writer.Write();
                theAlgorithm.Reset();
                Debug.WriteLine("Finished solving: " + "\t" + DateTime.Now);
                Debug.WriteLine("************************************************");
                index++;
            }
        }       
        void CreateXcplexFormulation()
        {
            theAlgorithm = new Outsource2Cplex();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_EXPORT_LP_MODEL, false);
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_LOG_OUTPUT_TYPE, writeOnFile);
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_RELAXATION, XCPlexRelaxation.None);
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_RUNTIME_SECONDS, runTime);
            Debug.WriteLine("The algorithm parameters are set: " + theAlgorithm.GetName() + "\t" + DateTime.Now);
            Debug.WriteLine("************************************************");
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
            Debug.WriteLine("The problem parameters are set: " + theAlgorithm.GetName() + "\t" + DateTime.Now);
            Debug.WriteLine("************************************************");
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
                        Debug.WriteLine("The problem is created: " + theProblem.PDP.InputFileName + "\t" + DateTime.Now);
                        problems.Add(theProblem);
                    }
                    Debug.WriteLine("************************************************");
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
                    Debug.WriteLine("The problem model is created: " + theProblemModel.InputFileName + "\t" + DateTime.Now);
                    problemModels.Add(theProblemModel);
                }
                Debug.WriteLine("************************************************");
            }
            catch (Exception)
            {
                MessageBox.Show("There is something wrong while loading problem from the whole data!", "Problem loading error!");
            }
        }

        
    }
}
