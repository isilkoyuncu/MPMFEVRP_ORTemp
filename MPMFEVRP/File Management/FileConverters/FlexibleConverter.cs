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
            for (int i = TGPData.NESS+1; i <= TGPData.NESS+numCustomers; i++)
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
    }
}
