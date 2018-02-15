using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class TotalRouteMeasures
    {
        double totalTravelTime = 0.0; public double TotalTravelTime { get { return totalTravelTime; } } //Note that this travel time includes the service durations and ES visits if any
        double totalTravelDistance = 0.0; public double TotalTravelDistance { get { return totalTravelDistance; } }
        List<SiteWithAuxiliaryVariables> allOriginalSWAVs, customers;
        EVvsGDV_ProblemModel theProblemModel;

        public TotalRouteMeasures(EVvsGDV_ProblemModel theProblemModel, List<SiteWithAuxiliaryVariables> allOriginalSWAVs, List<SiteWithAuxiliaryVariables> customers, int numRoutes, int numESVisits)
        {
            this.theProblemModel = theProblemModel;
            this.allOriginalSWAVs = allOriginalSWAVs;
            this.customers = customers;
            UpdateTotalTravelTime(numRoutes, numESVisits);
            UpdateTotalTravelDistance(numRoutes, numESVisits);
        }
        public void UpdateTotalTravelTime(int numRoutes, int numESVisits)
        {
            int numCustomers = theProblemModel.SRD.NumCustomers;
            string theDepotID = theProblemModel.SRD.GetSingleDepotID();

            double[] minDurationsFrom = new double[numCustomers];
            string[] idsOfminDurationFrom = new string[numCustomers];
            double[] minDurationsTo = new double[numCustomers];
            string[] idsOfminDurationTo = new string[numCustomers];
            double[] durationsFromDepot = new double[numCustomers];
            string[] idsOfdurationFromDepot = new string[numCustomers];

            for (int i = 0; i < customers.Count; i++)
            {
                double minDuration = double.MaxValue;
                string minDurationID = "";
                for (int j = 0; j < allOriginalSWAVs.Count; j++)
                    if (allOriginalSWAVs[j].SiteType != SiteTypes.ExternalStation
                        &&
                        customers[i].ID != allOriginalSWAVs[j].ID)
                        if (theProblemModel.SRD.GetTravelTime(customers[i].ID, allOriginalSWAVs[j].ID) < minDuration)
                        {
                            minDuration = theProblemModel.SRD.GetTravelTime(customers[i].ID, allOriginalSWAVs[j].ID);
                            minDurationID = allOriginalSWAVs[j].ID;
                        }
                minDurationsFrom[i] = minDuration;
                idsOfminDurationFrom[i] = minDurationID;
                minDuration = double.MaxValue;
                minDurationID = "";
                for (int k = 0; k < allOriginalSWAVs.Count; k++)
                    if (allOriginalSWAVs[k].SiteType == SiteTypes.Customer
                        &&
                        customers[i].ID != allOriginalSWAVs[k].ID &&
                        allOriginalSWAVs[k].ID != idsOfminDurationFrom[i]
                        ||
                        allOriginalSWAVs[k].SiteType == SiteTypes.Depot)
                        if (theProblemModel.SRD.GetTravelTime(allOriginalSWAVs[k].ID, customers[i].ID) < minDuration)
                        {
                            minDuration = theProblemModel.SRD.GetTravelTime(allOriginalSWAVs[k].ID, customers[i].ID);
                            minDurationID = allOriginalSWAVs[k].ID;
                        }
                minDurationsTo[i] = minDuration;
                idsOfminDurationTo[i] = minDurationID;
            }

            for (int j = 0; j < customers.Count; j++)
            {
                durationsFromDepot[j] = theProblemModel.SRD.GetTravelTime(theDepotID, customers[j].ID);
                idsOfdurationFromDepot[j] = customers[j].ID;
            }
            Array.Sort(durationsFromDepot, idsOfdurationFromDepot);

            List<string> singleCustVisitFromDepot = new List<string>();
            List<string> visitFromDepot = new List<string>();

        
                List<double> travelDurationFromDepot = new List<double>();
                List<double> travelDurationToDepot = new List<double>();
                double durationFromToDepot = 0.0;
                double minCustomersTravelDuration = 0.0;
                double avgMinTotalTravelDuration = 0.0;
                double totalServiceDuration = 0.0;
                double totalDuration = 0.0;

                int nArcsSelectedByDepot = 0;
                int indexMinDurationFromDepot = 0;
                while (nArcsSelectedByDepot < 2 * numRoutes)
                {
                    int count = 0;
                    int index = 0;
                    for (int j = 0; j < customers.Count; j++)
                        if (customers[j].ID == idsOfdurationFromDepot[indexMinDurationFromDepot])
                        {
                            index = j;
                            break;
                        }
                    if (idsOfminDurationFrom[index] == theDepotID &&
                        idsOfminDurationTo[index] == theDepotID)
                    {
                        count = 2;
                        singleCustVisitFromDepot.Add(idsOfdurationFromDepot[indexMinDurationFromDepot]);
                    }
                    else
                    {
                        count = 1;
                        visitFromDepot.Add(idsOfdurationFromDepot[indexMinDurationFromDepot]);
                    }
                    durationFromToDepot = durationFromToDepot + count * durationsFromDepot[indexMinDurationFromDepot];
                    indexMinDurationFromDepot++;
                    nArcsSelectedByDepot = nArcsSelectedByDepot + count;
                }

                for (int i = 0; i < numCustomers; i++)
                    minCustomersTravelDuration = minCustomersTravelDuration + minDurationsFrom[i] + minDurationsTo[i];

                //TODO I need to add ES visit durations as well. Depending on how many ES visits we made we need to add the shortest durations and the as for the time it should be 


                avgMinTotalTravelDuration = (minCustomersTravelDuration + durationFromToDepot) / 2.0;

                foreach (Site s in customers)
                    totalServiceDuration = totalServiceDuration + s.ServiceDuration;

                totalDuration = avgMinTotalTravelDuration + totalServiceDuration;
            }
        public void UpdateTotalTravelDistance(int numRoutes, int numESVisits)
        {
        }
    }
}
