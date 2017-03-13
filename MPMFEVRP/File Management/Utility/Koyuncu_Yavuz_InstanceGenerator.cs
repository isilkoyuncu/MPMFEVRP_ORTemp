using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Instance_Generation.Other;

namespace Instance_Generation.Utility
{
    class Koyuncu_Yavuz_InstanceGenerator
    {
        //Site-related information (sites: depot, customer, ES)
        int numCustomers;    //Number of Customers, not all of which must be served
        string customerDistribution;
        int nEVPremPayCustomers;
        int nISS, nISS_L1, nISS_L2, nISS_L3;
        int numES,  nESS_L1, nESS_L2, nESS_L3;  //# of ES, which includes the depot. If this is 0, intra-day charging is not allowed even at the depot. 
        int numNodes;   //This is numCustomers+nES+1 (1 is for the depot)
        string[] nodeID;
        string[] nodeType;
        int[] X;//[numNodes] X coordinate
        int[] Y;//[numNodes] Y coordinate
        double[] Demand;//[numNodes]
        double[] TimeWindowStart;//[numNodes]
        double[] TimeWindowEnd;//[numNodes]
        double[] CustomerServiceDuration;//[numNodes] typically 30-60 (minutes)
        double[] Gamma;//[numNodes] Charging rate (KHW/minute)
        double[,] Prize;//[2, numNodes] [vehicle - 0:EV, 1:GDV]

        //Vehicle-related information
        int numVehicleTypes;    //For our instances we ecpect this to be 2 (1 EV, 1 GDV) at all times, but for sake of future expendability, we define flexibly so one may have, say, 5 EV and 3 GDV types to compare
        VehicleCategories[] vehCategory; //[numVehicleTypes] Basically, this is to tell which vehicle is an EV, which is a GDV
        int[] D_Max;   //[numVehicleTypes] Driving range, which is infinite for GDVs
        double[] FixedCost;// [numVehicleTypes] $/day
        double[] VariableCostPerMile;   //[numVehicleTypes]  $
        Vehicle selectedEV;
        Vehicle selectedGDV;

        //Overall (not site- or vehicle-related information)
        double T_Max;    //Leghts of the workday, minutes
        double travelSpeed;

        public Koyuncu_Yavuz_InstanceGenerator()
        {

        }

        public void GenerateKoyuncuYavuzCustomers(int seed,
            int nCustomers,
            string customerDistribution,
            int XMax, int YMax,
            DepotLocations DepotLocation,
            ServiceDurationDistributions ServiceDurationDistribution)
        {
            PopulateIDColumn();
            Random rnd = new Random(seed);
            numCustomers = nCustomers;
            if (customerDistribution != "U")
                throw new Exception("Generator can't generate anything other than 'U' customer distribution, somehow we called it with wrong input!");
            this.customerDistribution = customerDistribution;
            numNodes = numCustomers + numES + 1;
            PopulateXYColumns(rnd, DepotLocation, XMax, YMax);
            PopulateServiceDurationColumn(rnd, ServiceDurationDistribution);
        }

        public void AddISS_ESS_info(
            int nISS_L1, int nISS_L2, int nISS_L3,
            int nESS, int nESS_L1, int nESS_L2, int nESS_L3,
            ChargingLevels selectedDepotChargingLvl
            )
        {
            nISS = nISS_L1 + nISS_L2 + nISS_L3;
            this.nISS_L1 = nISS_L1;
            this.nISS_L2 = nISS_L2;
            this.nISS_L3 = nISS_L3;
            numES = nESS;//The first one is depot, don't forget!
            this.nESS_L1 = nESS_L1;
            this.nESS_L2 = nESS_L2;
            this.nESS_L3 = nESS_L3;
        }

        public void AddPricingPolicy(
            int XMax, int YMax,
            int nEVPremPayCustomers,
            BasePricingPolicy basePricingPol, double basePricingDollar,
            TripChargePolicy tripChargePol, double tripChargeDollar,
            double EVPrizeCoefficient
            )
        {
            PopulatePrizeColumn(XMax, YMax, basePricingPol, basePricingDollar, tripChargePol, tripChargeDollar, EVPrizeCoefficient);
        }

        public void AddGDV(int seed,
            Vehicle selectedEV,
            Vehicle selectedGDV
            )
        {
            this.selectedEV = selectedEV;
            this.selectedGDV = selectedGDV;
        }
        public void Generate(int seed, 
            int nCustomers, 
            string customerDistribution, 
            int nEVPremPayCustomers, 
            int nISS_L1, int nISS_L2, int nISS_L3, 
            int nESS, int nESS_L1, int nESS_L2, int nESS_L3, 
            double TMax, 
            double travelSpeed, 
            int XMax, int YMax, 
            DepotLocations DepotLocation, 
            ServiceDurationDistributions ServiceDurationDistribution, 
            Vehicle selectedEV, 
            Vehicle selectedGDV, 
            ChargingLevels selectedDepotChargingLvl,
            BasePricingPolicy basePricingPol, double basePricingDollar,
            TripChargePolicy tripChargePol, double tripChargeDollar,
            double EVPrizeCoefficient
            )
        {
            this.travelSpeed = travelSpeed;
            this.T_Max = TMax;
            //Variable definitions are complete, now converting all that input into outputable information
            PopulateTypeColumn(selectedDepotChargingLvl);
            PopulateGammaColumn();
            PopulateDemands();
            PopulateTimeWindows();
        }
        void PopulateIDColumn()
        {
            nodeID = new string[numNodes];
            int counter = 0;
            nodeID[counter++] = "D0";
            for (int i = 0; i < numES; i++)
                nodeID[counter++] = "E" + i.ToString();
            for (int i = 1; i <= numCustomers; i++)
                nodeID[counter++] = "C" + i.ToString();
        }
        void PopulateTypeColumn(ChargingLevels selectedDepotChargingLvl)
        {
            nodeType = new string[numNodes];
            int counter = 0;
            nodeType[counter] = "d";
            nodeType[++counter] = "e";
            nodeType[counter] += selectedDepotChargingLvl.ToString();

            int nESS_L3_temp = nESS_L3 - (selectedDepotChargingLvl == ChargingLevels.L3 ? 1 : 0);
            int nESS_L2_temp = nESS_L2 - (selectedDepotChargingLvl == ChargingLevels.L2 ? 1 : 0);
            for (int i = 1; i < numES; i++)
            {
                nodeType[++counter] = "e";
                if (i <= nESS_L3_temp)
                    nodeType[counter] += "L3";
                else
                {
                    if (i <= nESS_L3_temp + nESS_L2_temp)
                        nodeType[counter] += "L2";
                    else if (i <= numES)
                        nodeType[counter] += "L1";
                }
            }
            for (int i = 1; i <= numCustomers; i++)
            {
                nodeType[++counter] = "c";
                if (i <= nEVPremPayCustomers)
                {
                    nodeType[counter] += "p";
                    if (i <= nISS_L3)
                        nodeType[counter] += "iL3";
                    else
                    {
                        if (i <= nISS_L3 + nISS_L2)
                            nodeType[counter] += "iL2";
                        else if(i <= nISS)
                            nodeType[counter] += "iL1";
                    }
                }
            }
            
        }
        void PopulateXYColumns(Random rnd, DepotLocations DepotLocation, int XMax, int YMax)
        {
            X = new int[numNodes];
            Y = new int[numNodes];
            if ((XMax % 2 != 0) || (YMax % 2 != 0))
                throw new Exception("Both XMax and YMax must be even numbers, fix input and try again!");
            switch (DepotLocation)
            {
                case DepotLocations.Center:
                    X[0] = XMax/2;
                    Y[0] = YMax/2;
                    break;
                case DepotLocations.LowerLeftCorner:
                    X[0] = 0;
                    Y[0] = 0;
                    break;
                case DepotLocations.Random:
                    X[0] = rnd.Next(0, XMax);
                    Y[0] = rnd.Next(0, YMax);
                    break;
            }
            X[1] = X[0]; //E0: Duplicate of the depot
            Y[1] = Y[0];
            for(int i=2; i<numNodes; i++)
            {
                X[i] = rnd.Next(0, XMax);
                Y[i] = rnd.Next(0, YMax);
            }
        }
        void PopulateGammaColumn()
        {
            Gamma = new double[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                if(nodeType[i].Contains("L3"))
                    Gamma[i] = ChargingStation.RechargingRate(ChargingLevels.L3);
                else if (nodeType[i].Contains("L2"))
                    Gamma[i] = ChargingStation.RechargingRate(ChargingLevels.L2);
                else if (nodeType[i].Contains("L1"))
                    Gamma[i] = ChargingStation.RechargingRate(ChargingLevels.L1);
            }
        }
        void PopulateServiceDurationColumn(Random rnd, ServiceDurationDistributions ServiceDurationDistribution)
        {
            CustomerServiceDuration = new double[numNodes];
            string userInput = ServiceDurationDistribution.ToString().Substring(1);
            char[] separator = new char[] { '_' };
            string[] userInputSeparated = userInput.Split(separator);
            int[] userInputParsed = new int[userInputSeparated.Length];
            for (int i = 0; i < userInputSeparated.Length; i++)
                userInputParsed[i] = int.Parse(userInputSeparated[i]);
            for (int j = 0; j <= numES; j++)
            {
                CustomerServiceDuration[j] = 0.0;
            }
            for (int j = numES + 1; j < numNodes; j++)
            {
                CustomerServiceDuration[j] = userInputParsed[rnd.Next(userInputParsed.Length)];
            }
        }
        void PopulatePrizeColumn(
            int XMax, int YMax,
            BasePricingPolicy basePricingPol, double basePricingDollar,
            TripChargePolicy tripChargePol, double tripChargeDollar,
            double EVPrizeCoefficient)
        {
            Prize = new double[Enum.GetNames(typeof(VehicleCategories)).Length, numNodes];
            //Depot and all ES get 0:
            for (int v = 0; v < Enum.GetNames(typeof(VehicleCategories)).Length; v++)
                for (int i = 0; i <= numES;i++ )
                {
                    Prize[v, i] = 0.0;
                }

            //For each customer, we first calculate the base+trip price, and then multiply it for the EV
            for (int i = 1; i <= numCustomers; i++)
            {
                //Since vehicle categories are fixed (0:EV, 1:GDV), we handle them in quite a rigid manner:
                switch (basePricingPol)
                {
                    case BasePricingPolicy.Identical:
                        Prize[1, i] = basePricingDollar;
                        break;
                    case BasePricingPolicy.ProportionalToServiceDuration:
                        Prize[1, i] = basePricingDollar * CustomerServiceDuration[i];
                        break;
                }
                switch (tripChargePol)
                {
                    case TripChargePolicy.None:
                        break;
                    case TripChargePolicy.TwoTier:
                        if ((Math.Abs(X[i] - X[0]) > 0.5 * (XMax - X[0])) || (Math.Abs(Y[i] - Y[0]) > 0.5 * (YMax - Y[0])))
                        {
                            Prize[1, i] += tripChargeDollar;
                        }
                        break;
                    case TripChargePolicy.Individualized:
                        Prize[1, i] += tripChargeDollar * Calculators.EuclideanDistance(X[0], X[i], Y[0], Y[i]);
                        break;
                }
                if (i <= nEVPremPayCustomers)
                    Prize[0, i] = Prize[1, i] * EVPrizeCoefficient;
                else
                    Prize[0, i] = Prize[1, i];
            }
        }
        void PopulateDemands()
        {
            Demand = new double[numNodes];
            for (int i = 0; i < numNodes; i++)
                Demand[i] = 0.0;
        }
        void PopulateTimeWindows()
        {
            TimeWindowStart = new double[numNodes];
            TimeWindowEnd = new double[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                TimeWindowStart[i] = 0.0;
                TimeWindowEnd[i] = T_Max;
            }
        }
        public void SaveToFile(string filename)
        {
            filename += ".txt";
            System.IO.StreamWriter sw = new System.IO.StreamWriter(filename);
            sw.WriteLine("StringID\tType\tx\ty\tdemand\tReadyTime\tDueDate\tServiceDuration\tRechargingRate\tEVPrize\tGDVPrize");
            for (int i = 0; i < numNodes; i++)
            {
                sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}", nodeID[i], nodeType[i], X[i], Y[i], Demand[i], TimeWindowStart[i], TimeWindowEnd[i], CustomerServiceDuration[i], Gamma[i], Prize[0, i], Prize[1, i]);
            }
            sw.WriteLine();
            sw.WriteLine(CombineAndTabSeparateStringArray(ChargingStation.getHeaderRow()));
            foreach (ChargingLevels cl in Enum.GetValues(typeof(ChargingLevels)))
                sw.WriteLine(CombineAndTabSeparateStringArray(ChargingStation.getIndividualRow(cl)));
            sw.WriteLine();
            sw.WriteLine(CombineAndTabSeparateStringArray(Vehicle.getHeaderRow()));
            sw.WriteLine(CombineAndTabSeparateStringArray(selectedEV.getIndividualRow()));
            sw.WriteLine(CombineAndTabSeparateStringArray(selectedGDV.getIndividualRow()));
            sw.WriteLine();
            sw.WriteLine("Average Velocity\t{0}", travelSpeed);
            sw.WriteLine();
            sw.Flush();
            sw.Close();
        }

        string CombineAndTabSeparateStringArray(string[] inputStrArray)
        {
            string output = "";
            for (int i = 0; i < inputStrArray.Length; i++)
                output += inputStrArray[i] + "\t";
            return output;
        }
    }
}
