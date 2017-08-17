using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MPMFEVRP.Utils
{
    class StringOperations
    {
        public static string CombineAndTabSeparateArray(object[] inputStrArray)
        {
            if ((inputStrArray == null) || (inputStrArray.Length == 0))
                throw new Exception("CombineAndTabSeparateArray invoked with nothing to combine!");
            string output = inputStrArray[0].ToString();
            for (int i = 1; i < inputStrArray.Length; i++)
                output += "\t" + inputStrArray[i].ToString();
            return output;
        }
        public static string CombineAndSpaceSeparateArray(object[] inputStrArray)
        {
            if ((inputStrArray == null) || (inputStrArray.Length == 0))
                throw new Exception("CombineAndSpaceSeparateArray invoked with nothing to combine!");
            string output = inputStrArray[0].ToString(); ;
            for (int i = 1; i < inputStrArray.Length; i++)
                output += " " + inputStrArray[i].ToString();
            return output;
        }

        public static string CombineAndTabSeparateMatrix(double[,] inputStrMatrix)
        {
            string output = "";
            for (int i = 0; i < inputStrMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < inputStrMatrix.GetLength(1); j++)
                    output += inputStrMatrix[i, j].ToString() + "\t";
                output += "\n";
            }
            return output;
        }
        public static string[] SeparateFullFileName(string fullFileName)
        {
            int filenameStart = -1, filenameEnd = -1;//These are the positions of the first and last characters in the core file name
            bool startFound = false, endFound = false;
            char characterSought = '.';
            char[] characterArray = fullFileName.ToCharArray();
            for (int i = characterArray.Length - 1; !startFound; i--)
                if (characterArray[i] == characterSought)
                {
                    if (endFound)
                    {
                        filenameStart = i + 1;
                        startFound = true;
                    }
                    else
                    {
                        filenameEnd = i - 1;
                        endFound = true;
                        characterSought = '\\';
                    }
                }
            string sourceDirectory = fullFileName.Substring(0,filenameStart);
            string file_name = fullFileName.Substring(filenameStart, filenameEnd - filenameStart + 1);
            string file_extension = fullFileName.Substring(filenameEnd + 1);
            return new string[] { sourceDirectory, file_name, file_extension };
        }
        public static string CombineFullFileName(string file_name, string file_extension, string sourceDirectory = "")
        {
            return sourceDirectory + file_name + file_extension;
        }

        public static void CompareTwoCustomerSetArchive(string filesdirectory)
        {
            String directory = filesdirectory; //TODO give a directory
            String[] linesA = File.ReadAllLines(Path.Combine(directory, "fileA.txt"));
            String[] linesB = File.ReadAllLines(Path.Combine(directory, "fileB.txt"));

            IEnumerable<String> onlyB = linesB.Except(linesA);

            IEnumerable<String> onlyA = linesA.Except(linesB);

            if (onlyB.Count() > 0 || onlyA.Count() > 0)
            {
                Console.WriteLine("Two files are different.");
            }
            else
            {
                Console.WriteLine("Two files are same.");
            }
        }

    }
}
