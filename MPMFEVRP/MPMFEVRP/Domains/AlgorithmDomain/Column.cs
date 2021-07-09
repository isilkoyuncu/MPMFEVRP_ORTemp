using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;

namespace MPMFEVRP.Domains.AlgorithmDomain
{
    public class Column
    {
        /// <summary>
        /// 
        /// </summary>
        public CustomerSet optimizedCS;
        string id; public string ID { get { return id; } }        
        int iterationNo=-1; public int IterationNo { get { return iterationNo; } }
        int countOnlyExtended=0; public int CountOnlyExtended { get { return countOnlyExtended; } set { countOnlyExtended = value; } }
        int countExtendAndOptimized=0; public int CountExtendAndOptimized { get { return countExtendAndOptimized; } set { countExtendAndOptimized = value; } }
        bool afvPartOftheRelaxedSolution=false; public bool AFVpartOftheRelaxedSolution { get { return afvPartOftheRelaxedSolution; } }
        bool gdvPartOftheRelaxedSolution = false; public bool GDVpartOftheRelaxedSolution { get { return gdvPartOftheRelaxedSolution; } }
        bool afvPartOftheIntegerSolution = false; public bool AFVpartOftheIntegerSolution { get { return afvPartOftheIntegerSolution; } }
        bool gdvPartOftheIntegerSolution =false; public bool GDVpartOftheIntegerSolution { get { return gdvPartOftheIntegerSolution; } }
        AlgorithmSolutionStatus afvSolutionStatus; public AlgorithmSolutionStatus AFVSolutionStatus { get { return afvSolutionStatus; } }
        AlgorithmSolutionStatus gdvSolutionStatus; public AlgorithmSolutionStatus GDVSolutionStatus { get { return gdvSolutionStatus; } }
        public double AFVvmt;
        public double GDVvmt;
        public double AFVprofit;
        public double GDVprofit;
        public double AFVfuelCost;
        public double GDVfuelCost;

        public Column(CustomerSet cs, int iterationNo, EVvsGDV_ProblemModel theProblemModel)
        {
            optimizedCS = new CustomerSet(cs,copyROO:true);
            id = optimizedCS.CustomerSetID;
            AFVvmt = optimizedCS.GetVMT(VehicleCategories.EV);
            GDVvmt = optimizedCS.GetVMT(VehicleCategories.EV);
            AFVfuelCost = optimizedCS.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.EV).FuelCost;
            GDVfuelCost = optimizedCS.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(VehicleCategories.GDV).FuelCost;
            AFVprofit = optimizedCS.OFIDP.GetPrizeCollected(VehicleCategories.EV) - AFVfuelCost - theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.EV).FixedCost;
            GDVprofit = optimizedCS.OFIDP.GetPrizeCollected(VehicleCategories.EV) - GDVfuelCost - theProblemModel.VRD.GetTheVehicleOfCategory(VehicleCategories.GDV).FixedCost;

            this.iterationNo = iterationNo;
        }
        public void UpdatePostOptimizationStatistics(AlgorithmSolutionStatus afvSolutionStatus = AlgorithmSolutionStatus.NotYetSolved, AlgorithmSolutionStatus gdvSolutionStatus = AlgorithmSolutionStatus.NotYetSolved, bool afvPartOftheRelaxedSolution =false, bool gdvPartOftheRelaxedSolution = false, bool afvPartOftheIntegerSolution = false, bool gdvPartOftheIntegerSolution = false)
        {
            countExtendAndOptimized++;
            this.afvSolutionStatus = afvSolutionStatus;
            this.gdvSolutionStatus = gdvSolutionStatus;
            this.afvPartOftheRelaxedSolution = afvPartOftheRelaxedSolution;
            this.gdvPartOftheRelaxedSolution = gdvPartOftheRelaxedSolution;
            this.afvPartOftheIntegerSolution = afvPartOftheIntegerSolution;
            this.gdvPartOftheIntegerSolution = gdvPartOftheIntegerSolution;
        }
        public void UpdatePostExtendStatistics()
        {
            countOnlyExtended++;
        }
    }
}
