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
        int lambda = 1;
        string probShortName;
        string formulationName;
        string refuelingOpt;
        public TS_Runs()
        {
            InitializeComponent();
            problems = new BindingList<IProblem>();
            problemModels = new BindingList<EVvsGDV_ProblemModel>();
        }

        //Solve EMH - FF with NDF and ADF
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
        //Solve YC - FF with NDF and ADF
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
            CreateYCProblem();
            probShortName = "YC";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full);
            refuelingOpt = "FF";
            CreateProblemModels();
            Run();
        }
        //Solve EMH - VF with NDF and ADF
        private void Button_solve_EMH_NDF_VF_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.NodeDuplicatingwoU);
            formulationName = "NDF";
            CreateEMHProblem();
            probShortName = "EMH";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full);
            refuelingOpt = "VF";
            CreateProblemModels();
            Run();
        }
        private void Button_solve_EMH_ADF_VF_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.ArcDuplicatingwoU);
            formulationName = "ADF";
            CreateEMHProblem();
            probShortName = "EMH";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full);
            refuelingOpt = "VF";
            CreateProblemModels();
            Run();
        }
        //Solve EMH - VP with NDF and ADF
        private void Button_solve_EMH_NDF_VP_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.NodeDuplicatingwoU);
            formulationName = "NDF";
            CreateEMHProblem();
            probShortName = "EMH";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial);
            refuelingOpt = "VP";
            CreateProblemModels();
            Run();
        }
        private void Button_solve_EMH_ADF_VP_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.ArcDuplicatingwoU);
            formulationName = "ADF";
            CreateEMHProblem();
            probShortName = "EMH";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial);
            refuelingOpt = "VP";
            CreateProblemModels();
            Run();
        }
        //Solve YC - VF with NDF and ADF
        private void Button_solve_YC_NDF_VF_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.NodeDuplicatingwoU);
            formulationName = "NDF";
            CreateYCProblem();
            probShortName = "YC";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full);
            refuelingOpt = "VF";
            CreateProblemModels();
            Run();
        }
        private void Button_solve_YC_ADF_VF_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.ArcDuplicatingwoU);
            formulationName = "ADF";
            CreateYCProblem();
            probShortName = "YC";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full);
            refuelingOpt = "VF";
            CreateProblemModels();
            Run();
        }
        //Solve YC - VP with NDF and ADF
        private void Button_solve_YC_NDF_VP_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.NodeDuplicatingwoU);
            formulationName = "NDF";
            CreateYCProblem();
            probShortName = "YC";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial);
            refuelingOpt = "VP";
            CreateProblemModels();
            Run();
        }
        private void Button_solve_YC_ADF_VP_Click(object sender, EventArgs e)
        {
            CreateXcplexFormulation();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_XCPLEX_FORMULATION, XCPlex_Formulation.ArcDuplicatingwoU);
            formulationName = "ADF";
            CreateYCProblem();
            probShortName = "YC";
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_RECHARGING_ASSUMPTION, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial);
            refuelingOpt = "VP";
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
                Log("The formulation is initialized: " + formulationName);
                theAlgorithm.Run();
                Log("Started solving: " + probShortName+index.ToString());
                Log("************************************************");
                theAlgorithm.Conclude();
                theSolution = theAlgorithm.Solution;
                solutions.Add(theSolution);
                writer = new IndividualSolutionWriter(index, probShortName,formulationName,refuelingOpt,runTime, theAlgorithm.GetOutputSummary(), theSolution.GetOutputSummary(), theSolution.GetWritableSolution());
                writer.Write();
                theAlgorithm.Reset();
                Log("Finished solving: " + probShortName + index.ToString());
                Log("************************************************");
                index++;
            }
        }       
        void CreateXcplexFormulation()
        {
            writeOnFile = checkBox_cplexLog2File.Checked;
            theAlgorithm = new Outsource2Cplex();
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_EXPORT_LP_MODEL, false);
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_LOG_OUTPUT_TYPE, writeOnFile);
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_RELAXATION, XCPlexRelaxation.None);
            runTime = double.Parse(textBox_runTime.Text);
            theAlgorithm.AlgorithmParameters.UpdateParameter(Models.ParameterID.ALG_RUNTIME_SECONDS, runTime);
            Log("The algorithm parameters are set: " + theAlgorithm.GetName());
            Log("************************************************");
        }
        void CreateEMHProblem()
        {
            lambda = int.Parse(textBox_lambda.Text);
            theProblem = new EMH_Problem();
            AddProblems();
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_NUM_EV, 6);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_NUM_GDV, 0);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_LAMBDA, lambda);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_USE_EXACTLY_NUM_EV_AVAILABLE, false);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_USE_EXACTLY_NUM_GDV_AVAILABLE, false);
            Log("The problem parameters are set: " + theProblem.GetName());
            Log("************************************************");
            theProblemModel = new EMH_ProblemModel();
        }
        void CreateYCProblem()
        {
            lambda = int.Parse(textBox_lambda.Text);
            theProblem = new EVvsGDV_MinCost_VRP();
            AddProblems();
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_NUM_EV, 7);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_NUM_GDV, 7);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_LAMBDA, lambda);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_USE_EXACTLY_NUM_EV_AVAILABLE, false);
            theProblem.ProblemCharacteristics.UpdateParameter(Models.ParameterID.PRB_USE_EXACTLY_NUM_GDV_AVAILABLE, false);
            Log("The problem parameters are set: " + theProblem.GetName());
            Log("************************************************");
            theProblemModel = new EVvsGDV_MinCost_VRP_Model();
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
                        Log("The problem is read: " + theProblem.PDP.InputFileName);
                        problems.Add(theProblem);
                    }
                    Log("************************************************");
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
                    Log(probShortName + (i+1).ToString() + " is created.");
                    problemModels.Add(theProblemModel);
                }
                Log("************************************************");
            }
            catch (Exception)
            {
                MessageBox.Show("There is something wrong while loading problem from the whole data!", "Problem loading error!");
            }
        }
        void Log(string message)
        {
            textBox_log.AppendText(DateTime.Now.ToString("HH:mm:ss tt") + ": " + message + "\n");
            textBox_log.SelectionStart = textBox_log.GetFirstCharIndexOfCurrentLine();
            textBox_log.SelectionLength = 1;
            textBox_log.ScrollToCaret();
        }

        
    }
}
