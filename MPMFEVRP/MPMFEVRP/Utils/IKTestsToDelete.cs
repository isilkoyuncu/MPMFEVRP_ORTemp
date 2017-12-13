using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MPMFEVRP.Utils
{
    public class IKTestsToDelete
    {
        bool areFilesTheSame; public bool AreFilesTheSame { get { return areFilesTheSame; } }
        public IKTestsToDelete()
        {
            areFilesTheSame = CheckIfTwoFilesAreTheSame();
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
        
    }
}
