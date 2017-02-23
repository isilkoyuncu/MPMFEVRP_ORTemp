using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class ProblemFramework
    {
        string inputFileName;   //For record only
        public string InputFileName { get { return inputFileName; } }
        SiteRelatedData srd;
        public SiteRelatedData SRD { get { return srd; } }
        VehicleRelatedData vrd;
        public VehicleRelatedData VRD { get { return vrd; } }
        ContextRelatedData crd;
        public ContextRelatedData CRD { get { return crd; } }
        public ProblemFramework() { }
        public ProblemFramework(string inputFileName, SiteRelatedData srd, VehicleRelatedData vrd, ContextRelatedData crd)
        {
            this.inputFileName = inputFileName;
            this.srd = srd;
            this.vrd = vrd;
            this.crd = crd;
        }
    }
}
