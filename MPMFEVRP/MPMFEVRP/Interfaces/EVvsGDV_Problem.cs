using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Models;


namespace MPMFEVRP.Interfaces
{
    public abstract class EVvsGDV_Problem:ProblemBase
    {
        public EVvsGDV_Problem()
        {
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_NUM_EV, "Available # of EVS", "0"));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_USE_EXACTLY_NUM_EV_AVAILABLE, "Use exactly available", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_NUM_GDV, "Available # of GDVs", "0"));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_USE_EXACTLY_NUM_GDV_AVAILABLE, "Use exactly available", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
        }

        public EVvsGDV_Problem(ProblemDataPackage PDP) 
        {
            base.PDP = new ProblemDataPackage(PDP);
            //The following are the problem characteristics. Each problem will have these fixed here, and they have nothing to do with data!
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_NUM_EV, "Available # of EVS", "0"));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_USE_EXACTLY_NUM_EV_AVAILABLE, "Use exactly available", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_NUM_GDV, "Available # of GDVs", "0"));
            problemCharacteristics.AddParameter(new InputOrOutputParameter(ParameterID.PRB_USE_EXACTLY_NUM_GDV_AVAILABLE, "Use exactly available", new List<object>() { true, false }, true, UserInputObjectType.CheckBox));

            //This code is extremely strict, for sake of simplicity!
            //First, we must be given exactly 2 vehicles
            if (PDP.VRD.VehicleArray.Length != 2)
                throw new ArgumentException("Reader had the wrong number of vehicle categories!");
            //Then, the first (0) must be an EV, and the other (1) must be a GDV!
            if ((PDP.VRD.VehicleArray[0].Category != VehicleCategories.EV) ||
                (PDP.VRD.VehicleArray[1].Category != VehicleCategories.GDV))
                throw new ArgumentException("Reader had the wrong composition or ordering of vehicle categories!");

            PDP.VRD.NumVehicles[0] = problemCharacteristics.GetParameter(ParameterID.PRB_NUM_EV).GetIntValue();
            PDP.VRD.NumVehicles[1] = problemCharacteristics.GetParameter(ParameterID.PRB_NUM_GDV).GetIntValue();
        }

        public override string GetName()
        {
            return "EV vs GDV VRP";
        }

        public override string ToString()
        {
            return PDP.InputFileName;
        }
    }
}
