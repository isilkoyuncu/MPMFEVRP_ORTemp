using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Models;
using System;
using System.Collections.Generic;


namespace MPMFEVRP.Implementations.Problems.Interfaces_and_Bases
{
    public abstract class EVvsGDV_Problem : ProblemBase
    {
        protected CustomerCoverageConstraint_EachCustomerMustBeCovered coverConstraintType;
        public CustomerCoverageConstraint_EachCustomerMustBeCovered CoverConstraintType { get { return coverConstraintType; } }

        public EVvsGDV_Problem() { }

        public EVvsGDV_Problem(ProblemDataPackage PDP)
        {
            AddProblemCharacteristics(); //Add the problem characteristics. Each problem will have these fixed here, and they have nothing to do with data!

            base.PDP = new ProblemDataPackage(PDP);
        }

        void AddProblemCharacteristics()
        {
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_NUM_EV, "Available # of EVS", "7"));
            //problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_USE_EXACTLY_NUM_EV_AVAILABLE, "Use exactly available", new List<object>() { true, false }, false, UserInputObjectType.CheckBox));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_NUM_GDV, "Available # of GDVs", "0"));
            //problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_USE_EXACTLY_NUM_GDV_AVAILABLE, "Use exactly available", new List<object>() { true, false }, false, UserInputObjectType.CheckBox));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_RECHARGING_ASSUMPTION, "Recharging Assumption", new List<object>() { RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial }, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial, UserInputObjectType.ComboBox));
            //problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_LAMBDA, "Max # of ES Visit", "1"));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_CREATETSPSOLVERS, "Create TSP Solvers", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_CREATEEXPLOITINGTSPSOLVER, "Create Exploiting TSP Solver", new List<object>() { true, false }, false, UserInputObjectType.CheckBox));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_CREATEPLAINTSPSOLVER, "Create Plain TSP Solver", new List<object>() { true, false }, false, UserInputObjectType.CheckBox));
        }

        public override string GetName() { return "EV vs GDV VRP"; }

        public override string ToString() { return "EV vs GDV Vehicle Routing Problem"; }
    }
}
