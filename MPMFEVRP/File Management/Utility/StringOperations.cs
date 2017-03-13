using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instance_Generation.Utility
{
    public class StringOperations
    {
        public static string CombineAndTabSeparateArray(object[] inputStrArray)
        {
            string output = "";
            for (int i = 0; i < inputStrArray.Length; i++)
                output += inputStrArray[i].ToString() + "\t";
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

    }
}
