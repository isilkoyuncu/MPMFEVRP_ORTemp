using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.SolutionDomain;

namespace MPMFEVRP.Utils
{
    /// <summary>
    /// At each iteration of the column generation framework, we record at least the following:
    /// Iteration number, total duration of the iteration, number of columns added to the explored set, number of negative reduced cost columns added to the master set, 
    /// total duration of the route optimization, duration of the relaxed set partitioning, the objective function value of the relaxed set partitioning model, 
    /// </summary>
    public class CGIterationStats
    {
        int iterationNo; //iteration number

        int numberOfColumnsExplored; //columns added to the explored set
        int numberOfNegRedCostColumnsAdded; //negative reduced cost columns added to the master set
        int numberOfPromisingCustomers; //Customers having >0 shadow prices

        int totalNumOfColumnsToSetCoverSoFar;
        int totalNumOfColumnsExploredSoFar;

        double relaxedSetPartitionTime; //relaxed set partitioning duration
        double customerSetGeneratorTotalTime;
        double routeOptimizationTotalTime; //total duration of the route optimization
        double iterationTotalTimeCalculated; //total duration of the iteration = relaxed set partitioning duration + total duration of the route optimization
        double iterationTotalTimeActual;

        double relaxedSetPartitionObjValue; //objective function value of the relaxed set partitioning model
        double avgNumberOfCustomersExplored;

        public CGIterationStats(
            int iterationNo,
            int numberOfColumnsExplored,
            int numberOfNegRedCostColumnsAdded,
            int numberOfPromisingCustomers,
            int totalNumOfColumnsToSetCoverSoFar,
            int totalNumOfColumnsExploredSoFar,
            double relaxedSetPartitionTime,
            double customerSetGeneratorTotalTime,
            double routeOptimizationTotalTime,
            double iterationTotalTimeCalculated,
            double iterationTotalTimeActual,
            double relaxedSetPartitionObjValue,
            double avgNumberOfCustomersExplored)
        {
            this.iterationNo = iterationNo;
            this.numberOfColumnsExplored = numberOfColumnsExplored;
            this.numberOfNegRedCostColumnsAdded = numberOfNegRedCostColumnsAdded;
            this.numberOfPromisingCustomers = numberOfPromisingCustomers;
            this.totalNumOfColumnsToSetCoverSoFar = totalNumOfColumnsToSetCoverSoFar;
            this.totalNumOfColumnsExploredSoFar = totalNumOfColumnsExploredSoFar;
            this.relaxedSetPartitionTime = relaxedSetPartitionTime;
            this.customerSetGeneratorTotalTime = customerSetGeneratorTotalTime;
            this.routeOptimizationTotalTime = routeOptimizationTotalTime;
            this.iterationTotalTimeCalculated = iterationTotalTimeCalculated;
            this.iterationTotalTimeActual = iterationTotalTimeActual;
            this.relaxedSetPartitionObjValue = relaxedSetPartitionObjValue;
            this.avgNumberOfCustomersExplored = avgNumberOfCustomersExplored;
        }

        public static string GetHeaderRow()
        {
            return "iterationNo\tnumberOfColumnsExplored\tnumberOfNegRedCostColumnsAdded\tnumberOfPromisingCustomers\ttotalNumOfColumnsToSetCoverSoFar\ttotalNumOfColumnsExploredSoFar\trelaxedSetPartitionTime\tcustomerSetGeneratorTotalTime\trouteOptimizationTotalTime\titerationTotalTimeCalculated\titerationTotalTimeActual\tavgNumberOfCustomersExplored\trelaxedSetPartitionObjValue";
        }

        public string GetDataRow()
        {
            return
               iterationNo.ToString() + "\t"+
             numberOfColumnsExplored.ToString() + "\t" +
             numberOfNegRedCostColumnsAdded.ToString() + "\t" +
             numberOfPromisingCustomers.ToString() + "\t" +
             totalNumOfColumnsToSetCoverSoFar.ToString() + "\t" +
             totalNumOfColumnsExploredSoFar.ToString() + "\t" +
             relaxedSetPartitionTime.ToString() + "\t" +
             customerSetGeneratorTotalTime.ToString() + "\t" +
             routeOptimizationTotalTime.ToString() + "\t" +
             iterationTotalTimeCalculated.ToString() + "\t" +
             iterationTotalTimeActual.ToString() + "\t" +
             avgNumberOfCustomersExplored.ToString() + "\t" +
             relaxedSetPartitionObjValue.ToString();
        }
    }
}
