using System;
using System.Linq;

namespace MPMFEVRP.Utils
{
    public class RawDataParser
    {
        String data;

        int numberOfJobs;
        public int NumberOfJobs { get { return numberOfJobs; } }

        int[] dueDates;
        public int[] DuesDates { get { return dueDates; } }

        int[] processingTimes;
        public int[] ProcessingTimes { get { return processingTimes; } }

        string[] descriptions;
        public string[] Descriptions { get { return descriptions; } }

        public RawDataParser(String data)
        {
            this.data = data;
        }

        public void Parse()
        {
            String[] lines = data.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // first line: comment

            // second line: N
            numberOfJobs = int.Parse(lines[1]);

            // third line: comment

            // fourth line: processing times for each job
            processingTimes = lines[3].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();

            // fifth line: comment

            // sixth line: due dates for each job
            dueDates = lines[5].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();

            // if there is a seventh line, then there are descriptions
            if (lines.Length > 6)
            {
                descriptions = lines.Skip(7).Select(x => x.Trim()).ToArray();
            }
            else
            {
                descriptions = Enumerable.Range(0, numberOfJobs).Select(x => "Job " + x.ToString()).ToArray();
            }
        }

    }
}
