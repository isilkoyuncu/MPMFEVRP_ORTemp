using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Models;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.SupplementaryInterfaces.Listeners;
using System;
using System.Collections.Generic;
using MPMFEVRP.Domains.ProblemDomain;
using System.Linq;
using MPMFEVRP.Utils;
using System.IO;
using System.Diagnostics;

namespace MPMFEVRP.Implementations.Algorithms
{
    /// <summary>
    /// This algorithm solves a mixed-fleet EVRP with VP policy for the AFV refueling.
    /// The objective is to minimize the total cost.
    /// The total cost consists of the refueling costs of EVs at ESs and refueling costs of both EVs and CVs at the depot at the end of the day.
    /// Basic assumptions follow most of the literature: 
    /// VP policy for refueling, multiple ES visits are allowed, no time windows, no customer demands, (this is a service personnel routing problem), 
    /// it is cheaper to refuel at the depot at the end of the day since the electricity price goes down at the end of the day, 
    /// all ESs in the network are assumed to be public refueling stations, there is no subscription cost and they are not in-network (out-of-network price per kWh),
    /// there are limited number of EVs and CVs (but enough to cover the whole demand), daily activation cost (rent if you will) of an EV is higher than that of CV.
    /// </summary>
    public class CG_Matheuristic_MixedFleetEVRP_VP : AlgorithmBase
    {
        string folder { get; set; }

        public CG_Matheuristic_MixedFleetEVRP_VP() 
        {
            AddSpecializedParameters();
        }
        public CG_Matheuristic_MixedFleetEVRP_VP(double timeLimit, double terminationCondition, string folder) 
        {
            this.folder = folder;
        }
        public override void AddSpecializedParameters()
        {
            //throw new NotImplementedException();
        }
        public override void SpecializedInitialize(EVvsGDV_ProblemModel theProblemModel)
        {
            throw new NotImplementedException();
        }
        public override void SpecializedRun()
        {
            throw new NotImplementedException();
        }
        public override void SpecializedConclude()
        {
            throw new NotImplementedException();
        }
        public override string[] GetOutputSummary()
        {
            throw new NotImplementedException();
        }      
        public override void SpecializedReset()
        {
            throw new NotImplementedException();
        }   
        public override bool setListener(IListener listener)
        {
            throw new NotImplementedException();
        }
        public override string GetName()
        {
            return "CG Matheuristic for Mixed-Fleet EVRP with VP";
        }
    }
}
