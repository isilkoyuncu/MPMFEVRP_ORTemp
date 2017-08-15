using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Domains.SolutionDomain;
using MPMFEVRP.Models.XCPlex;
using MPMFEVRP.Models;
using MPMFEVRP.Domains.AlgorithmDomain;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Interfaces;

namespace MPMFEVRP.Interfaces
{
    public abstract class ProblemModelBase : IProblemModel
    {
        protected string inputFileName; // This is not for reading but just for record keeping and reporting 
        public string InputFileName { get { return inputFileName; } set { inputFileName=value; } }

        protected ObjectiveFunctionTypes objectiveFunctionType;
        public ObjectiveFunctionTypes ObjectiveFunctionType { get { return objectiveFunctionType; } }

        protected InputOrOutputParameterSet problemCharacteristics;
        public InputOrOutputParameterSet ProblemCharacteristics { get { return problemCharacteristics; } }

        protected CustomerCoverageConstraint_EachCustomerMustBeCovered coverConstraintType;
        public CustomerCoverageConstraint_EachCustomerMustBeCovered CoverConstraintType { get { return coverConstraintType; }set { coverConstraintType = value; } }

        protected RechargingDurationAndAllowableDepartureStatusFromES rechargingDuration_status;
        public RechargingDurationAndAllowableDepartureStatusFromES RechargingDuration_status { get { return rechargingDuration_status; } set { rechargingDuration_status = value; } }

        protected ProblemDataPackage pdp;
        public SiteRelatedData SRD { get { return pdp.SRD; } }
        public VehicleRelatedData VRD { get { return pdp.VRD; } }
        public ContextRelatedData CRD { get { return pdp.CRD; } }

        protected int[] numVehicles; public int[] NumVehicles { get { return numVehicles; } set { numVehicles = value; } }

        protected bool archiveAllCustomerSets; public bool ArchiveAllCustomerSets { get { return archiveAllCustomerSets; } }
        protected CustomerSetList customerSetArchive; public CustomerSetList CustomerSetArchive { get { return customerSetArchive; } }

        public List<string> GetAllCustomerIDs()
        {
            List<string> outcome = new List<string>();
            foreach (Site s in SRD.SiteArray)
                if (s.SiteType == SiteTypes.Customer)
                    outcome.Add(s.ID);
            return outcome;
        }
        protected AssignedRoute ExtractTheSingleRouteFromSolution(RouteBasedSolution ncs)
        {
            if (ncs.Routes.Count != 1)
            {
                //This is a problem!
                System.Windows.Forms.MessageBox.Show("Single vehicle optimization resulted in none or multiple AssignedRoute in a Solution!");
                throw new Exception("Single vehicle optimization resulted in none or multiple AssignedRoute in a Solution!");
            }
            return ncs.Routes[0];
        }

        public abstract string GetName();
        public abstract string GetDescription();
        public abstract string GetNameOfProblemOfModel();

        protected List<Type> compatibleSolutions;
        public List<Type> GetCompatibleSolutions() { return compatibleSolutions; }

        public abstract RouteOptimizationOutcome OptimizeForSingleVehicle(CustomerSet CS);

        public abstract VehicleSpecificRouteOptimizationOutcome OptimizeRoute(CustomerSet CS, Vehicle vehicle, VehicleSpecificRoute GDVOptimalRoute = null);
        public abstract RouteOptimizationOutcome OptimizeRoute(CustomerSet CS, List<Vehicle> vehicles);

        public abstract ISolution GetRandomSolution(int seed, Type SolutionType);
        public abstract bool CheckFeasibilityOfSolution(ISolution solution);
        public abstract double CalculateObjectiveFunctionValue(ISolution solution);
        public abstract bool CompareTwoSolutions(ISolution solution1, ISolution solution2);

        protected bool IsSolutionTypeCompatible(Type solutionType)
        {
            return compatibleSolutions.Contains(solutionType);
        }
        
    }
}
