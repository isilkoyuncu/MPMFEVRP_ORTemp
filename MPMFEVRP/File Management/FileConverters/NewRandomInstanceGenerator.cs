using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Instance_Generation.Other;
using Instance_Generation.FormSections;

namespace Instance_Generation.FileConverters
{
    public class NewRandomInstanceGenerator
    {
        Random rnd;

        public NewRandomInstanceGenerator(int seed)
        {
            rnd = new Random(seed);
        }

        public void PopulateXYColumns(int numNodes, CommonCoreData CCData, out double[] X, out double[] Y)
        {
            X = new double[numNodes];
            Y = new double[numNodes];
            if ((CCData.XMax % 2 != 0) || (CCData.YMax % 2 != 0))
                throw new Exception("Both XMax and YMax must be even numbers, fix input and try again!");
            switch (CCData.DepotLocation)
            {
                case DepotLocations.Center:
                    X[0] = CCData.XMax / 2;
                    Y[0] = CCData.YMax / 2;
                    break;
                case DepotLocations.LowerLeftCorner:
                    X[0] = 0;
                    Y[0] = 0;
                    break;
                case DepotLocations.Random:
                    rnd.NextDouble();
                    X[0] = CCData.XMax * (rnd.NextDouble());
                    Y[0] = CCData.YMax * (rnd.NextDouble());
                    break;
            }
            X[1] = X[0]; //E0: Duplicate of the depot
            Y[1] = Y[0];
            for (int i = 2; i < numNodes; i++)
            {
                X[i] = CCData.XMax * (rnd.NextDouble());
                Y[i] = CCData.YMax * (rnd.NextDouble());
            }
        }
        public void PopulateServiceDurationColumn(int numNodes, CommonCoreData CCData, TypeGammaPrize_RelatedData TGPData, out double[] CustomerServiceDuration)
        {
            CustomerServiceDuration = new double[numNodes];
            string userInput = CCData.ServiceDurationDistribution.ToString().Substring(1);
            char[] separator = new char[] { '_' };
            string[] userInputSeparated = userInput.Split(separator);
            int[] userInputParsed = new int[userInputSeparated.Length];
            for (int i = 0; i < userInputSeparated.Length; i++)
                userInputParsed[i] = int.Parse(userInputSeparated[i]);
            for (int j = 0; j <= TGPData.NESS; j++)
            {
                CustomerServiceDuration[j] = 0.0;
            }
            for (int j = TGPData.NESS + 1; j < numNodes; j++)
            {
                CustomerServiceDuration[j] = userInputParsed[rnd.Next(userInputParsed.Length)];
            }
        }

        public void ShuffleRows(int NESS, int NCustomers, string[] idColumn, double[] xColumn, double[] yColumn, double[] demandColumn, double[] readyTimeColumn, double[] dueDateColumn, double[] serviceDurColumn)
        {
            int nRows = NESS + NCustomers + 1;//nRows = nNodes = nESS+NCustomers+1: 0 for the depot, 1-NESS for the ESS, NESS+1,..,NESS+NCustomers for the customers
            double[] randomKey = new double[nRows];
            randomKey[0] = 0.0;
            randomKey[1] = 0.0;//This is necessary to exclude the ES replica of the depot from shuffling
            for (int e = 2; e <= NESS; e++)
                randomKey[e] = rnd.NextDouble();
            for (int c = NESS + 1; c < nRows; c++)
                randomKey[c] = 1.0 + rnd.NextDouble();
            //now comes the sorting
            Array.Sort(newCopyOfKey(randomKey), idColumn);
            Array.Sort(newCopyOfKey(randomKey), xColumn);
            Array.Sort(newCopyOfKey(randomKey), yColumn);
            Array.Sort(newCopyOfKey(randomKey), demandColumn);
            Array.Sort(newCopyOfKey(randomKey), readyTimeColumn);
            Array.Sort(newCopyOfKey(randomKey), dueDateColumn);
            Array.Sort(newCopyOfKey(randomKey), serviceDurColumn);
        }
        double[] newCopyOfKey(double[] randomKey)
        {
            double[] outcome = new double[randomKey.Length];
            for (int i = 0; i < randomKey.Length; i++)
                outcome[i] = randomKey[i];
            return outcome;
        }
    }
}
