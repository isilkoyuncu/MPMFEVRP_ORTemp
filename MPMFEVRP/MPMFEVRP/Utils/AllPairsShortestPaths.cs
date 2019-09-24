using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Utils
{
    /// <summary>
    /// This class aims to calculate all-pairs shortest distance matrix as well as the nodes visited in between
    /// </summary>
    public class AllPairsShortestPaths
    {
        string[] IDs;
        double[,] shortestDistance; public double[,] ShortestDistance { get { return shortestDistance; } }
        List<string>[,] shortestPaths; public List<string>[,] ShortestPaths { get { return shortestPaths; } }
        int verticesCount;
        public AllPairsShortestPaths()
        {
        }
        public AllPairsShortestPaths(double[,] distances, string[] IDs)
        {
            InitializeAndSolveAPSS(distances, IDs);
            ModifiedFloydWarshall();
        }
        public void InitializeAndSolveAPSS(double[,] distances, string[] IDs)
        {
            if (distances.GetLength(0) != IDs.Length)
                throw new Exception("Arrays of different lengths are not compatible to calculate the all pairs shortest paths.");

            verticesCount = distances.GetLength(0);
            shortestDistance = new double[verticesCount, verticesCount];
            shortestPaths = new List<string>[verticesCount, verticesCount];
            this.IDs = new string[verticesCount];

            for (int i = 0; i < verticesCount; ++i)
            {
                this.IDs[i] = IDs[i];
                for (int j = 0; j < verticesCount; ++j)
                {
                    shortestDistance[i, j] = distances[i, j];
                    shortestPaths[i, j] = new List<string>() { IDs[i], IDs[j] };
                }
            }
            ModifiedFloydWarshall();
        }
        public double[,] ModifyDistanceMatrix(double[,] distances, double Dmax)
        {
            double[,] outcome = distances;
            for (int i = 0; i < distances.GetLength(0); i++)
                for (int j = 0; j < distances.GetLength(1); j++)
                    if (outcome[i, j] > Dmax)
                        outcome[i, j] = double.MaxValue;
            return outcome;
        }
        /// <summary>
        /// Modified as follows...
        /// </summary>
        /// <param name="distances"></param>
        public void ModifiedFloydWarshall()
        {
            for (int k = 0; k < verticesCount; ++k)
            {
                for (int i = 0; i < verticesCount; ++i)
                {
                    for (int j = 0; j < verticesCount; ++j)
                    {
                        if (shortestDistance[i, k] + shortestDistance[k, j] < shortestDistance[i, j])
                        {
                            shortestDistance[i, j] = shortestDistance[i, k] + shortestDistance[k, j];
                            shortestPaths[i, j].RemoveRange(1, shortestPaths[i, j].Count - 1);
                            for (int l = 1; l < shortestPaths[i, k].Count - 1; l++)
                            {
                                shortestPaths[i, j].Add(shortestPaths[i, k][l]);
                            }
                            shortestPaths[i, j].AddRange(shortestPaths[k, j]);
                        }
                    }
                }
            }
        }
        public List<string>[] GetShortestPathsFrom(string ID)
        {
            List<string>[] outcome = new List<string>[verticesCount];
            for(int i=0; i<verticesCount; i++)
            {
                if (IDs[i] == ID)
                {
                    for (int j = 0; j < verticesCount; j++)
                    {
                        outcome[j] = shortestPaths[i, j];
                    }
                    break;
                }
            }
            return outcome;
        }
        public List<string> GetShortestPathBetween(string fromID, string toID)
        {
            List<string> outcome = new List<string>();
            for (int i = 0; i < verticesCount; i++)
            {
                if (IDs[i] == fromID)
                {
                    for (int j = 0; j < verticesCount; j++)
                    {
                        if (shortestPaths[i, j].Last() == toID)
                        {
                            outcome = shortestPaths[i, j];
                            break;
                        }
                    }
                    break;
                }
            }
            return outcome;
        }
    }
}
