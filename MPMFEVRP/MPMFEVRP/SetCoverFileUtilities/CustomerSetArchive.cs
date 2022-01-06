using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MPMFEVRP.SetCoverFileUtilities
{
    public class CustomerSetArchive
    {
        public static void SaveToFile(PartitionedCustomerSetList pcsl, string filename, EVvsGDV_ProblemModel theProblemModel)
        {
            StreamWriter sw = new StreamWriter(filename, false)
            {
                AutoFlush = true
            };//append not allowed
            sw.WriteLine(HeaderRow());
            foreach(CustomerSet cs in pcsl.ToCustomerSetList())
            {
                sw.WriteLine(CustomerSetRow(cs, theProblemModel));
            }
            sw.Close();
        }
        static string HeaderRow()
        {
            return "Customer Set\tVehicle (GDV) Specific Route Optimization Status";
        }
        static string CustomerSetRow(CustomerSet cs, EVvsGDV_ProblemModel theProblemModel)
        {
            if (cs.RouteOptimizationOutcome.GetRouteOptimizationStatus() == RouteOptimizationStatus.InfeasibleForBothGDVandEV)
                return CustomerSetToString(cs, theProblemModel) + "\t" + VehicleSpecificRouteOptimizationStatus.Infeasible.ToString();
            else
                return CustomerSetToString(cs, theProblemModel) + "\t" + cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV).Status.ToString();
        }
        static string CustomerSetToString(CustomerSet cs, EVvsGDV_ProblemModel theProblemModel)
        {
            if ((cs.RouteOptimizationOutcome == null) || (cs.RouteOptimizationOutcome.GetVehicleSpecificRouteOptimizationOutcome(Domains.ProblemDomain.VehicleCategories.GDV) == null))
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


        public static PartitionedCustomerSetList RecreateFromFile(string filename, EVvsGDV_ProblemModel theProblemModel)
        {
            Domains.ProblemDomain.Vehicle theGDV = theProblemModel.VRD.GetTheVehicleOfCategory(Domains.ProblemDomain.VehicleCategories.GDV);
            PartitionedCustomerSetList outcome = new PartitionedCustomerSetList();
            StreamReader sr = new StreamReader(filename);
            sr.ReadLine();//This is the header row, won't use for anything
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] entriesInLine = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                VehicleSpecificRouteOptimizationStatus vsrossss = (VehicleSpecificRouteOptimizationStatus)Enum.Parse(typeof(VehicleSpecificRouteOptimizationStatus), entriesInLine[1]);
                VehicleSpecificRoute vsr = new VehicleSpecificRoute(theProblemModel, theGDV);
                CustomerSet cs = new CustomerSet(RemoveSpacesAndConvertToList(entriesInLine[0]), theProblemModel, vsros: (VehicleSpecificRouteOptimizationStatus)Enum.Parse(typeof(VehicleSpecificRouteOptimizationStatus), entriesInLine[1]), vehicleSpecificRoute: vsr);
                outcome.Add(cs);
            }
            sr.Close();
            return outcome;
        }
        static List<string> RemoveSpacesAndConvertToList(string spaceSeparatedString)
        {
            return spaceSeparatedString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
        }
    }

}
