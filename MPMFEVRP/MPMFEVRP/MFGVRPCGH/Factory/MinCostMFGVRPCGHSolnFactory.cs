using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.MFGVRPCGH.WorkersAndTools;
using MPMFEVRP.MFGVRPCGH.RawMaterial;
using MPMFEVRP.MFGVRPCGH.FinalProduct;

namespace MPMFEVRP.MFGVRPCGH.Factory
{
    public class MinCostMFGVRPCGHSolnFactory
    {
        protected MixedFleetGVRPMaterial theMaterial;
        protected MFGVRPMILP theToolVRPmodel;
        protected MFGVRPMILP theToolETSPmodel;
        protected MFGVRPMILP theToolTSPmodel;
        protected CGMatheuristicForMinCostMFGVRP theMachine;
        protected GVRPSolution theProduct;

    }
}
