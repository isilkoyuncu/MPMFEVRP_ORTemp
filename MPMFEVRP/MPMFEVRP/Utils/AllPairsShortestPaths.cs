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
        double[,] shortestDistance; public double[,] ShortestDistance { get { return shortestDistance; } }
        List<int>[,] shortestPaths; public List<int>[,] ShortestPaths { get { return shortestPaths; } }
        int verticesCount;

        public AllPairsShortestPaths(double[,] distances)
        {
            verticesCount = distances.GetLength(0);
            shortestDistance = new double[verticesCount, verticesCount];
            shortestPaths = new List<int>[verticesCount, verticesCount];
        }

        /// <summary>
        /// Modified as follows...
        /// </summary>
        /// <param name="distances"></param>
        public void ModifiedFloydWarshall(double[,] distances)
        {
            for (int i = 0; i < verticesCount; ++i)
                for (int j = 0; j < verticesCount; ++j)
                    shortestDistance[i, j] = distances[i, j];

            for (int i = 0; i < verticesCount; ++i)
                for (int j = 0; j < verticesCount; ++j)
                    shortestPaths[i, j] = new List<int>() { i, j };

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
    }
}
