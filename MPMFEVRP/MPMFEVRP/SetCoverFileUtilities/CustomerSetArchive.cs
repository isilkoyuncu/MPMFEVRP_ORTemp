using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.SolutionDomain;
using System.IO;

namespace MPMFEVRP.SetCoverFileUtilities
{
    public class CustomerSetArchive
    {
        public static void SaveToFile(PartitionedCustomerSetList pcsl, string filename, MPMFEVRP.Interfaces.ProblemModelBase problemModel)
        {
            StreamWriter sw = new StreamWriter(filename, false);//append not allowed
            sw.AutoFlush = true;
            sw.WriteLine(HeaderRow());
            foreach(CustomerSet cs in pcsl.ToCustomerSetList())
            {
                sw.WriteLine(CustomerSetRow(cs, problemModel));
            }
            sw.Close();
        }
        static string HeaderRow()
        {
            return "Customer Set\tVehicle (GDV) Specific Route Optimization Status";
        }
        static string CustomerSetRow(CustomerSet cs, MPMFEVRP.Interfaces.ProblemModelBase problemModel)
        {
            return CustomerSetToString(cs, problemModel) + "\t" + cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV).Status.ToString();
        }
        static string CustomerSetToString(CustomerSet cs, MPMFEVRP.Interfaces.ProblemModelBase problemModel)
        {
            if (cs.RouteOptimizationOutcome == null)
                return SeparateBySpace(cs.Customers);
            //If here, cs.RouteOptimizationOutcome != null
            if (cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV).Status!= VehicleSpecificRouteOptimizationStatus.Optimized)
                return SeparateBySpace(cs.Customers);
            //if here, cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV).Status = VehicleSpecificRouteOptimizationStatus.Optimized
            List<string> listOfCustIDs = cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV).VSOptimizedRoute.ListOfVisitedNonDepotSiteIDs;
            return SeparateBySpace(listOfCustIDs);
        }
        static string SeparateBySpace(List<string> theList)
        {
            if ((theList == null) || (theList.Count == 0))
                return "";
            string outcome = theList[0];
            for (int i = 1; i < theList.Count; i++)
                outcome += " " + theList[i];
            return outcome;
        }


        public static PartitionedCustomerSetList RecreateFromFile(string filename, MPMFEVRP.Interfaces.ProblemModelBase problemModel)
        {
            PartitionedCustomerSetList outcome = new PartitionedCustomerSetList();
            StreamReader sr = new StreamReader(filename);
            sr.ReadLine();//This is the header row, won't use for anything
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] entriesInLine = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                VehicleSpecificRouteOptimizationStatus vsrossss = (VehicleSpecificRouteOptimizationStatus)Enum.Parse(typeof(VehicleSpecificRouteOptimizationStatus), entriesInLine[1]);
                CustomerSet cs = new CustomerSet(problemModel, RemoveSpacesAndConvertToList(entriesInLine[0]), vsros: (VehicleSpecificRouteOptimizationStatus)Enum.Parse(typeof(VehicleSpecificRouteOptimizationStatus), entriesInLine[1]));
                outcome.Add(cs);
            }
            sr.Close();
            return outcome;
        }
        static List<string> RemoveSpacesAndConvertToList(string spaceSeparatedString)
        {
            List<string> outcome = spaceSeparatedString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            return outcome;
            return spaceSeparatedString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
        }
    }

}
