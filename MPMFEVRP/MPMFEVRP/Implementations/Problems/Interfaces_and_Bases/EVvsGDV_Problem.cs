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
            AddProblemCharacteristics();
            base.PDP = new ProblemDataPackage(PDP);
            //The following are the problem characteristics. Each problem will have these fixed here, and they have nothing to do with data!

            //This code is extremely strict, for sake of simplicity!
            //First, we must be given exactly 2 vehicles
            if (PDP.VRD.VehicleArray.Length != 2)
                throw new ArgumentException("Reader had the wrong number of vehicle categories!");
            //Then, the first (0) must be an EV, and the other (1) must be a GDV!
            if ((PDP.VRD.VehicleArray[0].Category != VehicleCategories.EV) ||
                (PDP.VRD.VehicleArray[1].Category != VehicleCategories.GDV))
                throw new ArgumentException("Reader had the wrong composition or ordering of vehicle categories!");
        }

        void AddProblemCharacteristics()
        {
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_NUM_EV, "Available # of EVS", "3"));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_USE_EXACTLY_NUM_EV_AVAILABLE, "Use exactly available", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_NUM_GDV, "Available # of GDVs", "3"));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_USE_EXACTLY_NUM_GDV_AVAILABLE, "Use exactly available", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_RECHARGING_ASSUMPTION, "Recharging Assumption", new List<object>() { RechargingDurationAndAllowableDepartureStatusFromES.Fixed_Full, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Partial }, RechargingDurationAndAllowableDepartureStatusFromES.Variable_Full, UserInputObjectType.ComboBox));
        }

        public override string GetName() { return "EV vs GDV VRP"; }

        public override string ToString() { return "EV vs GDV Vehicle Routing Problem"; }
    }
}
