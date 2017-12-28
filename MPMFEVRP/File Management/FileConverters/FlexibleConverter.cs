using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Instance_Generation.Interfaces;
using Instance_Generation.FormSections;
using Instance_Generation.Other;
using Instance_Generation.Utility;

namespace Instance_Generation.FileConverters
{
    class FlexibleConverter
    {
        //Input related fields
        Experiment_RelatedData ExpData;
        CommonCoreData CCData;
        TypeGammaPrize_RelatedData TGPData;
        Vehicle_RelatedData VehData;
        IRawReader reader;

        //Intermediate fields, needed for calculations
        int numCustomers;
        int numNodes; public int NumberOfNodes { get { return numNodes; } }   //This is numCustomers+nES+1 (1 is for the depot)

        //Output related fields
        string[] nodeID; public string[] NodeID { get { return nodeID; } }
        string[] nodeType; public string[] NodeType { get { return nodeType; } }
        double[] x; public double[] X { get { return x; } }//[numNodes] X coordinate
        double[] y; public double[] Y { get { return y; } } //[numNodes] Y coordinate
        double[] demand; public double[] Demand { get { return demand; } }//[numNodes]
        double[] timeWindowStart; public double[] TimeWindowStart { get { return timeWindowStart; } }//[numNodes]
        double[] timeWindowEnd; public double[] TimeWindowEnd { get { return timeWindowEnd; } }//[numNodes]
        double[] customerServiceDuration; public double[] CustomerServiceDuration { get { return customerServiceDuration; } }//[numNodes] typically 30-60 (minutes)
        double[] gamma; public double[] Gamma { get { return gamma; } }//[numNodes] Charging rate (KHW/minute)
        double[,] prize; public double[,] Prize { get { return prize; } }//[2, numNodes] [vehicle - 0:EV, 1:GDV]
        double travelSpeed; public double TravelSpeed { get { return travelSpeed; } }// miles per minute
        double[,] distance; public double[,] Distance { get { return distance; } }
        bool useGeogPosition; public bool UseGeogPosition { get { return useGeogPosition; } }//is needed when calculating the distances

        //Random Instance Generator
        NewRandomInstanceGenerator RIG;

        //Constructors
        public FlexibleConverter() { }
        public FlexibleConverter(Experiment_RelatedData ExpData, CommonCoreData CCData, TypeGammaPrize_RelatedData TGPData, Vehicle_RelatedData VehData, IRawReader reader = null)
        {
            //Input related:
            this.ExpData = ExpData;
            this.CCData = CCData;
            this.TGPData = TGPData;
            this.VehData = VehData;
            if (reader == null)
                numCustomers = CCData.NCustomers;
            else
                numCustomers = reader.getNumCustomers();

            if (reader != null)
                this.reader = reader;

            //Intermediate
            numNodes = numCustomers + TGPData.NESS + 1;

            //RIG
            RIG = new NewRandomInstanceGenerator(ExpData.Seed);
        }

        //Main function
        public void Convert()
        {
            //The ID Column: if there is a file reader, it's just what's in the file; otherwise, it's created from sctratch
            if (reader == null)
                PopulateIDColumn();
            else
            {
                nodeID = reader.getIDColumn();
                if (nodeID == null)
                {
                    System.Windows.Forms.MessageBox.Show("reader.getIDColumn() returned null, so we have to create it from scratch!");//TODO After verifying the reader always populates the ID column correctly, delete this conditional code!
                    PopulateIDColumn();
                }
            }

            //X&Y Columns
            if (reader == null)
            {
                RIG.PopulateXYColumns(numNodes, CCData, out x, out y);
            }
            else
            {
                x = reader.getXorLongitudeColumn();
                y = reader.getYorLatitudeColumn();
            }

            //Demand Column
            if (reader == null)
            {
                PopulateDemands();
            }
            else
            {
                demand = reader.getDemandColumn();
            }

            //Time Window Matrix
            if (reader == null)
            {
                PopulateTimeWindows();
            }
            else
            {
                timeWindowStart = reader.getReadyTimeColumn();
                timeWindowEnd = reader.getDueDateColumn();
            }

            //Customer Service Duration Column
            if (reader == null)
            {
                RIG.PopulateServiceDurationColumn(numNodes, CCData, TGPData, out customerServiceDuration);
            }
            else
            {
                customerServiceDuration = reader.getServiceDurationColumn();
            }

            //Now we need to shuffle the rows
            if (reader != null)//otherwise we don't need the shuffling at all!
            {
                if (reader.needToShuffleCustomers())
                    RIG.ShuffleRows(reader.getNumESS(), numCustomers, nodeID, x, y, demand, timeWindowStart, timeWindowEnd, customerServiceDuration);
            }

            //The Type Column:
            PopulateTypeColumn();//TODO Test & compare populated type info with what was in the read file for several examples, if there is any mismatch, we'll have to re-consider how to convert the type info

            //Gamma Columns
            PopulateGammaColumn(); //Since this function depends on type column only, no need to seperate as from reader and from form.

            //Prize Matrix
            PopulatePrizeColumn();

            //Travel speed
            if (reader == null)
            {
                travelSpeed = CCData.TravelSpeed;
            }
            else
            {
                travelSpeed = reader.getTravelSpeed();
            }

            //Distance
            PopulateDistancesMatrix();

            //Removal of infeasible customers
            //RemoveInfeasibleCustomers(new List<CustomerRemovalCriteria>() { CustomerRemovalCriteria.CannotBeReachedWithAtMostOneESVisit, CustomerRemovalCriteria.DirectRouteExceedsWorkdayLength });
        }

        void PopulateIDColumn()
        {
            nodeID = new string[numNodes];
            int counter = 0;
            nodeID[counter++] = "D0";
            for (int i = 0; i < TGPData.NESS; i++)
                nodeID[counter++] = "E" + i.ToString();
            for (int i = 1; i <= numCustomers; i++)
                nodeID[counter++] = "C" + i.ToString();
        }
        void PopulateTypeColumn()
        {
            nodeType = new string[numNodes];
            int counter = 0;
            nodeType[counter] = "d";
            nodeType[++counter] = "e";
            nodeType[counter] += TGPData.SelectedDepotChargingLvl.ToString();

            int nESS_L3_temp = TGPData.NESS_L3 - (TGPData.SelectedDepotChargingLvl == ChargingLevels.L3 ? 1 : 0);
            int nESS_L2_temp = TGPData.NESS_L2 - (TGPData.SelectedDepotChargingLvl == ChargingLevels.L2 ? 1 : 0);
            for (int i = 1; i < TGPData.NESS; i++)
            {
                nodeType[++counter] = "e";
                if (i <= nESS_L3_temp)
                    nodeType[counter] += "L3";
                else
                {
                    if (i <= nESS_L3_temp + nESS_L2_temp)
                        nodeType[counter] += "L2";
                    else if (i <= TGPData.NESS)
                        nodeType[counter] += "L1";
                }
            }
            for (int i = 1; i <= numCustomers; i++)
            {
                nodeType[++counter] = "c";
                if (i <= TGPData.NEVPremPayCustomers)
                {
                    nodeType[counter] += "p";
                    if (i <= TGPData.NISS_L3)
                        nodeType[counter] += "iL3";
                    else
                    {
                        if (i <= TGPData.NISS_L3 + TGPData.NISS_L2)
                            nodeType[counter] += "iL2";
                        else if (i <= TGPData.NISS)
                            nodeType[counter] += "iL1";
                    }
                }
            }

        }
        void ManipulateTypeColumn(string[] nodeTypeFromReader)
        {
            nodeType = new string[numNodes];
            int counter = 0;
            nodeType[counter] = "d";
            nodeType[++counter] = "e";
            nodeType[counter] += TGPData.SelectedDepotChargingLvl.ToString();
            // TODO finish manipulatetype column method
        }
        void PopulateGammaColumn()
        {
            gamma = new double[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                if (nodeType[i].Contains("L3"))
                    gamma[i] = TGPData.L3kWhPerMinute;
                else if (nodeType[i].Contains("L2"))
                    gamma[i] = TGPData.L2kWhPerMinute;
                else if (nodeType[i].Contains("L1"))
                    gamma[i] = TGPData.L1kWhPerMinute;
            }
        }
        void PopulatePrizeColumn()
        {
            prize = new double[Enum.GetNames(typeof(VehicleCategories)).Length, numNodes];
            //Depot and all ES get 0:
            for (int v = 0; v < Enum.GetNames(typeof(VehicleCategories)).Length; v++)
                for (int i = 0; i <= TGPData.NESS; i++)
                {
                    prize[v, i] = 0.0;
                }

            //For each customer, we first calculate the base+trip price, and then multiply it for the EV
            for (int i = TGPData.NESS + 1; i <= TGPData.NESS + numCustomers; i++)
            {
                //Since vehicle categories are fixed (0:EV, 1:GDV), we handle them in quite a rigid manner:
                switch (TGPData.BasePricingPol)
                {
                    case BasePricingPolicy.Identical:
                        prize[1, i] = TGPData.BasePricingDollar;
                        break;
                    case BasePricingPolicy.ProportionalToServiceDuration:
                        prize[1, i] = TGPData.BasePricingDollar * customerServiceDuration[i];
                        break;
                }
                switch (TGPData.TripChargePol)
                {
                    case TripChargePolicy.None:
                        break;
                    case TripChargePolicy.TwoTier:
                        if ((Math.Abs(x[i] - x[0]) > 0.5 * (CCData.XMax - x[0])) || (Math.Abs(y[i] - y[0]) > 0.5 * (CCData.XMax - y[0])))
                        {
                            prize[1, i] += TGPData.TripChargeDollar;
                        }
                        break;
                    case TripChargePolicy.Individualized:
                        prize[1, i] += TGPData.TripChargeDollar * Calculators.EuclideanDistance(x[0], x[i], y[0], y[i]);
                        break;
                }

                if (i < TGPData.NESS + 1 + TGPData.NEVPremPayCustomers)
                    prize[0, i] = prize[1, i] * TGPData.EVPrizeCoefficient;
                else
                    prize[0, i] = prize[1, i];
            }
        }
        void PopulateDemands()
        {
            demand = new double[numNodes];
            for (int i = 0; i < numNodes; i++)
                demand[i] = 0.0;
        }
        void PopulateTimeWindows()
        {
            timeWindowStart = new double[numNodes];
            timeWindowEnd = new double[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                timeWindowStart[i] = 0.0;
                timeWindowEnd[i] = CCData.TMax;
            }
        }
        void PopulateDistancesMatrix()
        {
            distance = new double[numNodes - 1, numNodes - 1];
            if (reader != null)
            {
                useGeogPosition = reader.usesGeographicPositions();
                distance = reader.getDistanceMatrix();
            }
            else
            {
                useGeogPosition = false;
                distance = null;
            }
        }
        void RemoveInfeasibleCustomers(List<CustomerRemovalCriteria> removalCriteria)
        {
            List<string> customerIDs2Remove = new List<string>();
            //Identification
            //TODO: In the following feasibility checkers, distance calculation is hardcoded, make it flexible
            if (removalCriteria.Contains(CustomerRemovalCriteria.DirectRouteExceedsWorkdayLength))
                customerIDs2Remove.AddRange(InfeasibleCustomers_DirectRouteExceedsWorkdayLength());
            if (removalCriteria.Contains(CustomerRemovalCriteria.CannotBeReachedWithAtMostOneESVisit))
                customerIDs2Remove.AddRange(InfeasibleCustomers_CannotBeReachedWithAtMostOneESVisit());
            //Execution
            RemoveInfeasibleCustomers(customerIDs2Remove);
        }
        List<string> InfeasibleCustomers_DirectRouteExceedsWorkdayLength()
        {
            List<string> outcome = new List<string>();
            for (int c = 0; c < nodeID.Length; c++)
                if (nodeType[c].Contains("c"))
                    if (CustomerDirectRouteExceedsWorkdayLength(c))
                        outcome.Add(nodeID[c]);
            return outcome;
        }
        bool CustomerDirectRouteExceedsWorkdayLength(int c)
        {
            double workdayLength = timeWindowEnd[0];
            double minStay = 0.0;
            if (nodeType[c].Contains("c"))
            {
                minStay = customerServiceDuration[c];
            }
            if (distance == null)
            {
                //double distTo = Calculators.HaversineDistance(X[0], Y[0], X[c], Y[c]);
                //double distFrom = Calculators.HaversineDistance(X[c], Y[c], X[0], Y[0]);
                //double totalTravelTime = (distTo + distFrom) / travelSpeed;
                //if (totalTravelTime + minStay > workdayLength)
                //    System.Windows.Forms.MessageBox.Show(nodeID[c] + " is infeasible: distance to = " + distTo.ToString() + ", distance from = " + distFrom.ToString() + ", total travel time = " + totalTravelTime.ToString() + ", tour length = " + (totalTravelTime + minStay).ToString() + " > " + workdayLength.ToString());
                return ((Calculators.HaversineDistance(X[0], Y[0], X[c], Y[c]) + Calculators.HaversineDistance(X[c], Y[c], X[0], Y[0])) / travelSpeed + minStay > workdayLength);
            }
            else
                return ((distance[c, 0] + distance[0, c]) / travelSpeed + minStay > timeWindowEnd[0]);
        }
        List<string> InfeasibleCustomers_CannotBeReachedWithAtMostOneESVisit()
        {
            List<string> outcome = new List<string>();
            for (int c = 0; c < nodeID.Length; c++)
                if (nodeType[c].Contains("c"))
                    if (CustomerCannotBeReachedWithAtMostOneESVisit(c))
                        outcome.Add(nodeID[c]);
            return outcome;
        }
        bool CustomerCannotBeReachedWithAtMostOneESVisit(int c)
        {
            double EVDrivingRange = VehData.SelectedEV.BatteryCapacity / VehData.SelectedEV.ConsumptionRate;
            double workdayLength = timeWindowEnd[0];
            if (distance == null)
            {
                if ((Calculators.HaversineDistance(X[0], Y[0], X[c], Y[c]) + Calculators.HaversineDistance(X[c], Y[c], X[0], Y[0])) <= EVDrivingRange)
                    return false;
                for (int r = 0; r < nodeID.Length; r++)
                    if (nodeType[r].Contains("e"))
                        if (Calculators.HaversineDistance(X[0], Y[0], X[r], Y[r]) <= EVDrivingRange)
                            if (Calculators.HaversineDistance(X[r], Y[r], X[c], Y[c]) + Calculators.HaversineDistance(X[c], Y[c], X[0], Y[0]) <= EVDrivingRange)
                                if ((Calculators.HaversineDistance(X[0], Y[0], X[r], Y[r]) + Calculators.HaversineDistance(X[r], Y[r], X[c], Y[c]) + Calculators.HaversineDistance(X[c], Y[c], X[0], Y[0])) / travelSpeed + customerServiceDuration[c] + (1.0 / gamma[r]) <= workdayLength)
                                    return false;
            }
            else//distances are stored in the matrix
            {
                if ((distance[c, 0] + distance[0, c]) <= EVDrivingRange)
                    return false;
                for (int r = 0; r < nodeID.Length; r++)
                    if (nodeType[r].Contains("e"))
                        if (distance[0, r] <= EVDrivingRange)
                            if (distance[r, c] + distance[c, 0] <= EVDrivingRange)
                                if ((distance[0, r] + distance[r, c] + distance[c, 0]) / travelSpeed + customerServiceDuration[c] + (1.0 / gamma[r]) <= workdayLength)
                                    return false;
            }
            return true;
        }
        void RemoveInfeasibleCustomers(List<string> customerIDs2Remove)
        {
            customerIDs2Remove = customerIDs2Remove.Distinct().ToList();
            List<int> rowNumbers2Remove = new List<int>();
            foreach (string id in customerIDs2Remove)
                for (int i = 0; i < nodeID.Length; i++)
                    if (nodeID[i] == id)
                    {
                        rowNumbers2Remove.Add(i);
                        break;
                    }
            RemoveRows(rowNumbers2Remove);
        }
        void RemoveRows(List<int> rowNumbers)
        {
            if (rowNumbers == null)
                return;
            if (rowNumbers.Count == 0)
                return;
            //Initialization
            rowNumbers.Sort();
            int newNumCustomers = numCustomers - rowNumbers.Count;
            int newNumNodes = numNodes - rowNumbers.Count;
            int[] oldRowNumberOfNewRow = new int[newNumNodes];
            int newIndex = 0;
            for (int oldIndex = 0; oldIndex < numNodes; oldIndex++)
            {
                if (rowNumbers.Contains(oldIndex))
                    continue;
                //else:
                oldRowNumberOfNewRow[newIndex++] = oldIndex;
            }
            //Single-index data
            string[] newnodeID = new string[newNumNodes];
            string[] newnodeType = new string[newNumNodes];
            double[] newx = new double[newNumNodes];
            double[] newy = new double[newNumNodes];
            double[] newdemand = new double[newNumNodes];
            double[] newtimeWindowStart = new double[newNumNodes];
            double[] newtimeWindowEnd = new double[newNumNodes];
            double[] newcustomerServiceDuration = new double[newNumNodes];
            double[] newgamma = new double[newNumNodes];
            for (int i = 0; i < newNumNodes; i++)
            {
                newnodeID[i] = nodeID[oldRowNumberOfNewRow[i]];
                newnodeType[i] = nodeType[oldRowNumberOfNewRow[i]];
                newx[i] = x[oldRowNumberOfNewRow[i]];
                newy[i] = y[oldRowNumberOfNewRow[i]];
                newdemand[i] = demand[oldRowNumberOfNewRow[i]];
                newtimeWindowStart[i] = timeWindowStart[oldRowNumberOfNewRow[i]];
                newtimeWindowEnd[i] = timeWindowEnd[oldRowNumberOfNewRow[i]];
                newcustomerServiceDuration[i] = customerServiceDuration[oldRowNumberOfNewRow[i]];
                newgamma[i] = gamma[oldRowNumberOfNewRow[i]];
            }
            //price
            double[,] newprize = new double[2, newNumNodes];
            for (int v = 0; v < 2; v++)
                for (int i = 0; i < newNumNodes; i++)
                    newprize[v, i] = prize[v, oldRowNumberOfNewRow[i]];
            //distance
            double[,] newdistance = new double[newNumNodes, newNumNodes];
            if (distance != null)
                for (int i = 0; i < newNumNodes; i++)
                    for (int j = 0; j < newNumNodes; j++)
                        newdistance[i, j] = distance[oldRowNumberOfNewRow[i], oldRowNumberOfNewRow[j]];
            //replace old with new
            nodeID = newnodeID;
            nodeType = newnodeType;
            x = newx;
            y = newy;
            demand = newdemand;
            timeWindowStart = newtimeWindowStart;
            timeWindowEnd = newtimeWindowEnd;
            customerServiceDuration = newcustomerServiceDuration;
            gamma = newgamma;
            prize = newprize;
            if (distance != null)
                distance = newdistance;
            numCustomers = newNumCustomers;
            numNodes = newNumNodes;
        }
    }
}
