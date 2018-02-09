using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Domains.ProblemDomain;
namespace MPMFEVRP.Utils
{
    public class IKTestsToDelete
    {
        bool areFilesTheSame; public bool AreFilesTheSame { get { return areFilesTheSame; } }
        bool isFeasible; public bool IsFeasible { get { return isFeasible; } }
        List<bool> isAllFeasible; public bool IsAllFeasible { get { return isFeasible; } }

        List<List<int>> routes; public List<List<int>> Routes { get { return routes; } }
        IProblem theProblem;
        Vehicle theEV;
        Site theDepot;
        int numSites;
        List<Site> ESs;
        List<Site> customersList;
        public IKTestsToDelete()
        {
            //areFilesTheSame = CheckIfTwoFilesAreTheSame();
            //isFeasible = CheckIfTheGivenSolutionIsFeasible();
            //routes = SubstractTheRouteFromSolution();
            isAllFeasible = CustomersCanBeReachedWithAtMostOneESVisit();
        }
        List<bool> CustomersCanBeReachedWithAtMostOneESVisit()
        {
            List<bool> tempisAllFeasible = new List<bool>();
            for (int i = 1; i <= 10; i++)
            {
                if(i==10)
                    theProblem = ProblemUtil.CreateProblemByFileName("EV vs GDV Minimum Cost VRP", "C:\\Users\\ikoyuncu\\Google Drive\\0-Research\\KoyuncuYavuzInstances\\Yavuz-Capar Test Instances\\KY 50c1i2e-U 80x80_1(1,1+0+0)_2(2+0+0)_Y24\\50c0i3e-U10-80x80_1(1,1+0+0)_2(2+0+0)_Y24.txt");
                else
                    theProblem = ProblemUtil.CreateProblemByFileName("EV vs GDV Minimum Cost VRP", "C:\\Users\\ikoyuncu\\Google Drive\\0-Research\\KoyuncuYavuzInstances\\Yavuz-Capar Test Instances\\KY 50c1i2e-U 80x80_1(1,1+0+0)_2(2+0+0)_Y24\\50c0i3e-U0" + i + "-80x80_1(1,1+0+0)_2(2+0+0)_Y24.txt");
                theEV = theProblem.PDP.VRD.GetTheVehicleOfCategory(VehicleCategories.EV);
                theDepot = theProblem.PDP.SRD.GetSingleDepotSite();
                numSites = theProblem.PDP.SRD.NumNodes;
                ESs = theProblem.PDP.SRD.GetSitesList(SiteTypes.ExternalStation);
                customersList = theProblem.PDP.SRD.GetSitesList(SiteTypes.Customer);
                foreach (Site c in customersList)
                    tempisAllFeasible.Add(CustomerCannotBeReachedWithAtMostOneESVisit(c.ID));

            }

            return tempisAllFeasible;
        }
        bool CustomerCannotBeReachedWithAtMostOneESVisit(String cID)
        {
            double EVDrivingRange = theEV.BatteryCapacity / theEV.ConsumptionRate;
            double workdayLength = theDepot.DueDate;
                if ((theProblem.PDP.SRD.GetDistance(cID, theDepot.ID) + theProblem.PDP.SRD.GetDistance(theDepot.ID, cID)) <= EVDrivingRange)
                    return false;
                foreach(Site es in ESs)
                        if (theProblem.PDP.SRD.GetDistance(theDepot.ID,es.ID) <= EVDrivingRange)
                            if (theProblem.PDP.SRD.GetDistance(es.ID,cID) + theProblem.PDP.SRD.GetDistance(cID,theDepot.ID) <= EVDrivingRange)
                                if ((theProblem.PDP.SRD.GetDistance(theDepot.ID, es.ID) + theProblem.PDP.SRD.GetDistance(es.ID, cID) + theProblem.PDP.SRD.GetDistance(cID, theDepot.ID)) / theProblem.PDP.CRD.TravelSpeed + theProblem.PDP.SRD.GetSiteByID(cID).ServiceDuration + (1.0 / theProblem.PDP.SRD.GetSiteByID(es.ID).RechargingRate) <= workdayLength)
                                    return false;
            return true;
        }
        bool CheckIfTwoFilesAreTheSame()
        {
            String directory = "C:/Users/ikoyuncu/Desktop/MPMFEVRP_ORTemp/MPMFEVRP/MPMFEVRP/bin/x64/Debug/";
            String[] linesA = File.ReadAllLines(Path.Combine(directory, "A.txt"));
            String[] linesB = File.ReadAllLines(Path.Combine(directory, "B.txt"));

            IEnumerable<String> onlyB = linesB.Except(linesA);

            IEnumerable<String> onlyA = linesA.Except(linesB);

            if (onlyB.Count() > 0 || onlyA.Count() > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        List<List<int>> SubstractTheRouteFromSolution()
        {
            String directory = "C:/Users/ikoyuncu/Desktop/MPMFEVRP_ORTemp/MPMFEVRP/MPMFEVRP/bin/x64/Debug/";
            String[] solutionFile = File.ReadAllLines(Path.Combine(directory, "KoyuncuYavuz_50c1i1e-U01-80x80_1(1,1+0+0)_1(1+0+0)_Y24 Outsource to CPLEX-NodeDuplicatingwoU Runtime Limit-1800.txt"));
            List<string> routeLines = new List<string>();
            int i;
            for (i = 0; i < solutionFile.Length; i++)
            {
                if (solutionFile[i].Contains("Route")) { i++; break; }
            }
            for (int j = i; j < solutionFile.Length; j++)
            { routeLines.Add(solutionFile[j]); }
            List<List<int>> routes = new List<List<int>>();
            for (int j = 0; j < routeLines.Count; j++)
            {
                if (routeLines[j] != "")
                {
                    List<int> route = new List<int>();
                    route.Add(0);
                    bool stop = false;
                    string routeLine = routeLines[j];
                    int s = 0;
                    int e = 0; int l = 0;
                    do
                    {
                        s = routeLine.IndexOf("C", e++);
                        e = routeLine.IndexOf("C", (s + 1));
                        l = (e - 1) - (s + 1);
                        if (l < 0)
                        {
                            e = routeLine.IndexOf("D", (s + 1));
                            l = (e - 1) - (s + 1);
                            string lele = routeLine.Substring(s + 1, l);
                            route.Add(int.Parse(lele));
                            route.Add(0);

                            stop = true;
                        }
                        else
                        {
                            string lele = routeLine.Substring(s + 1, l);
                            route.Add(int.Parse(lele));
                        }

                    } while (!stop);
                    routes.Add(route);
                }
            }
            return routes;
        }

    }
}
