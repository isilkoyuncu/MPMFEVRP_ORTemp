using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class Job // TODO THIS IS GOING TO BE DELETED!!!!!!!!!!!! 
    {
        string id;
        public string ID { get { return id; } }

        int processingTime;
        public int ProcessingTime { get { return processingTime; } }

        int dueDate;
        public int DueDate { get { return dueDate; } }

        string description;
        public string Description { get { return description; } }

        public Job(int processingTime, int dueDate, string description)
        {
            this.processingTime = processingTime;
            this.dueDate = dueDate;
            this.description = description;
            this.id = Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return "JOB: pTime=" + processingTime + " dDate=" + dueDate + " ID='" + id + "'";
        }
    }
}
