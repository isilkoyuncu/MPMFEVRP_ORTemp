using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Interfaces;

namespace MPMFEVRP.Implementations.Solutions.Writers
{
    public class IndividualSolutionWriter : IWriter
    {
        string filename;
        string solutionStatus;
        double actualRunTime;
        double relaxedSolnObj_LB;
        double bestSoln_UB;
        double optimalityGap;

        //Solution organized in columns
        List<List<int>> routes;
        List<string> vehicles;
        List<double> prizeCollected;
        List<double> costIncurred;
        List<double> profit;
        List<double> timeToReturnDepot;

        System.IO.StreamWriter sw;

        public IndividualSolutionWriter()
        {
            //Empty constructor
        }
        public IndividualSolutionWriter(string filename,
        string solutionStatus,
        double actualRunTime,
        double relaxedSolnObj_LB,
        double bestSoln_UB,
        double optimalityGap,

        List<List<int>> routes,
        List<string> vehicles,
        List<double> prizeCollected,
        List<double> costIncurred,
        List<double> profit,
        List<double> timeToReturnDepot)
        {
            //Take all input
            this.filename=filename;
            this.solutionStatus=solutionStatus;
            this.actualRunTime= actualRunTime;
            this.relaxedSolnObj_LB= relaxedSolnObj_LB;
            this.bestSoln_UB= bestSoln_UB;
            this.optimalityGap= optimalityGap;

            this.routes=routes;
            this.vehicles= vehicles;
            this.prizeCollected= prizeCollected;
            this.costIncurred= costIncurred;
            this.profit= profit;
            this.timeToReturnDepot= timeToReturnDepot;

            //TODO Make sure everything is passed into this constructor and used appropriately
            //verify input
            //Verify();
            
            //Process
            sw = new System.IO.StreamWriter(this.filename);
        }
        void Verify()
        {
            throw new NotImplementedException();
        }
            public void Write()
        {
            WriteStatistics();
            WriteSolution();
            sw.Flush();
            sw.Close();
        }
        private void WriteStatistics()
        {
            sw.WriteLine("Instance Name\t{0}", filename);
            sw.WriteLine("Solution Status\t{0}", solutionStatus);
            sw.WriteLine("Relaxed Soln Obj(LB)\t{0}", relaxedSolnObj_LB);
            sw.WriteLine("Best Soln(UB)\t{0}", bestSoln_UB);
            sw.WriteLine("Optimality Gap\t{0}", optimalityGap);
            sw.WriteLine();
        }
        private void WriteSolution()
        {
            sw.WriteLine("Route\tVehicle\tPrizeCollected\tCostIncurred\tProfit\tTimeToReturnDepot");
            for (int i = 0; i < routes.Count; i++)
            {
                sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", routes[i], vehicles[i], prizeCollected[i], costIncurred[i], profit[i], timeToReturnDepot[i]);
            }
            sw.WriteLine();
        }

    }
}
