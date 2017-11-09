using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class IndividualESVisitDataPackage
    {
        string iD; public string ID { get { return iD; } }
        double stayDuration;
        int preprocessedESSiteIndex;
        bool used; public bool Used { get { return used; } }

        public IndividualESVisitDataPackage(string iD,double stayDuration,int preprocessedESSiteIndex = -1,bool used = false)
        {
            this.iD = iD;
            this.stayDuration = stayDuration;
            this.preprocessedESSiteIndex = preprocessedESSiteIndex;
            this.used = used;
        }

        public double GetVisitStayDurationToES(string ESID)
        {
            if (ESID != iD)
                throw new Exception("IndividualESVisitDataPackage.GetFirstUnprocessedVisitStayDurationToES invoked for the wrong IndividualESVisitDataPackage!");
            if(used)
                throw new Exception("IndividualESVisitDataPackage.GetFirstUnprocessedVisitStayDurationToES invoked for the an already used IndividualESVisitDataPackage!");
            used = true;
            return stayDuration;
        }
    }

    public class IndividualRouteESVisits : List<IndividualESVisitDataPackage>
    {
        public double GetFirstUnprocessedVisitStayDurationToES(string ESID)
        {
            IndividualESVisitDataPackage currentESVisit;
            for (int i=0;i<Count;i++)
            {
                currentESVisit = this[i];
                if (!currentESVisit.Used)
                    if (currentESVisit.ID == ESID)
                        return currentESVisit.GetVisitStayDurationToES(ESID);
            }
            throw new Exception("IndividualRouteESVisits.GetFirstUnprocessedVisitStayDurationToES invoked with the wrong ESID!");
        }
    }
}
