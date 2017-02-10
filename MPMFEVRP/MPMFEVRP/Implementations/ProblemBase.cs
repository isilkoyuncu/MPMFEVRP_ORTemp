using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Implementations
{
    public abstract class ProblemBase : IProblem
    {
        List<Job> jobs;
        public List<Job> Jobs { get { return jobs; } }

        protected ObjectiveFunctionTypes objectiveFunctionType;
        public ObjectiveFunctionTypes ObjectiveFunctionType { get { return objectiveFunctionType; } }

        public ProblemBase()
        {
            jobs = new List<Job>();
        }

        public abstract string GetName();

        public override string ToString()
        {
            return "Data of " + jobs.Count + " jobs.";
        }

        public string CreateRawData()
        {
            StringBuilder raw = new StringBuilder();
            raw.AppendLine("// N = number of jobs : integer");
            raw.AppendLine(jobs.Count.ToString());
            raw.AppendLine("// P = processing times of jobs : integer array of N");
            raw.AppendLine(string.Join(" ", jobs.Select(x => x.ProcessingTime).ToArray()));
            raw.AppendLine("// D = due dates of jobs : integer array of N");
            raw.AppendLine(string.Join(" ", jobs.Select(x => x.DueDate).ToArray()));
            raw.AppendLine("// Desc = descriptions of jobs : string of N lines");
            foreach (var job in jobs)
            {
                raw.AppendLine(job.Description);
            }
            return raw.ToString();
        }
    }
}
