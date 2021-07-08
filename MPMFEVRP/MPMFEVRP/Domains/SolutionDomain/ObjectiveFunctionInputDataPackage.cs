﻿using MPMFEVRP.Domains.ProblemDomain;
using System.Collections.Generic;

namespace MPMFEVRP.Domains.SolutionDomain
{
    public class ObjectiveFunctionInputDataPackage
    {
        //This class is written to serve as a one-stop-shop for everything OF-related
        //Recall that this project is limited to only two vehicle categories: GDV and EV, and all GDVs and EVs are homogeneous within their respective category
        //Each solution must be able to provide this package so the problem model can calculate the objective function for the solution properly
        //Each solution component also must provide the package, however, the solution that owns the components is responsible of aggregating the smaller packages into something directly usable

        int numberOfCustomersServed_EV;
        int numberOfCustomersServed_GDV;
        public int GetNumberOfCustomersServed(VehicleCategories vehicleCategory) { return (vehicleCategory == VehicleCategories.EV ? numberOfCustomersServed_EV : numberOfCustomersServed_GDV); }
        public int GetTotalNumberOfCustomersServed() { return (numberOfCustomersServed_EV + numberOfCustomersServed_GDV); }

        double prizeCollected_EV;
        double prizeCollected_GDV;
        public double GetPrizeCollected(VehicleCategories vehicleCategory) { return (vehicleCategory == VehicleCategories.EV ? prizeCollected_EV : prizeCollected_GDV); }
        public double GetTotalPrizeCollected() { return (prizeCollected_EV + prizeCollected_GDV); }

        int numberOfVehiclesUsed_EV;
        int numberOfVehiclesUsed_GDV;
        public int GetNumberOfVehiclesUsed(VehicleCategories vehicleCategory) { return (vehicleCategory == VehicleCategories.EV ? numberOfVehiclesUsed_EV : numberOfVehiclesUsed_GDV); }
        public int GetTotalNumberOfVehiclesUsed() { return (numberOfVehiclesUsed_EV + numberOfVehiclesUsed_GDV); }

        double VMT_EV;
        double VMT_GDV;
        public double GetVMT(VehicleCategories vehicleCategory) { return (vehicleCategory == VehicleCategories.EV ? VMT_EV : VMT_GDV); }
        public double GetTotalVMT() { return (VMT_EV + VMT_GDV); }

        public ObjectiveFunctionInputDataPackage() { }//Do-nothing: all numbers are 0 by default
        public ObjectiveFunctionInputDataPackage(VehicleCategories vehicleCategory, int numberOfCustomersServed, double prizeCollected, int numberOfVehiclesUsed, double VMT)
        {
            //This constructor is for a vehicle-specific solution component (or aggregation thereof)
            if (vehicleCategory == VehicleCategories.EV)
            {
                numberOfCustomersServed_EV = numberOfCustomersServed;
                prizeCollected_EV = prizeCollected;
                numberOfVehiclesUsed_EV = numberOfVehiclesUsed;
                VMT_EV = VMT;
            }
            else
            {
                numberOfCustomersServed_GDV = numberOfCustomersServed;
                prizeCollected_GDV = prizeCollected;
                numberOfVehiclesUsed_GDV = numberOfVehiclesUsed;
                VMT_GDV = VMT;
            }
        }
        public ObjectiveFunctionInputDataPackage(int numberOfCustomersServed_EV, int numberOfCustomersServed_GDV, double prizeCollected_EV, double prizeCollected_GDV, int numberOfVehiclesUsed_EV, int numberOfVehiclesUsed_GDV, double VMT_EV, double VMT_GDV)
        {
            //This is the complete constructor
            this.numberOfCustomersServed_EV = numberOfCustomersServed_EV;
            this.numberOfCustomersServed_GDV = numberOfCustomersServed_GDV;
            this.prizeCollected_EV = prizeCollected_EV;
            this.prizeCollected_GDV = prizeCollected_GDV;
            this.numberOfVehiclesUsed_EV = numberOfVehiclesUsed_EV;
            this.numberOfVehiclesUsed_GDV = numberOfVehiclesUsed_GDV;
            this.VMT_EV = VMT_EV;
            this.VMT_GDV = VMT_GDV;
        }
        //Think carefully before adding other constructors. The constructors defined above had no coupling with the problem, solution, etc. 
        public ObjectiveFunctionInputDataPackage(ObjectiveFunctionInputDataPackage ofdp_ev, ObjectiveFunctionInputDataPackage ofdp_gdv)
        {
            numberOfCustomersServed_EV = ofdp_ev.GetNumberOfCustomersServed(VehicleCategories.EV);
            numberOfCustomersServed_GDV = ofdp_gdv.GetNumberOfCustomersServed(VehicleCategories.GDV);
            prizeCollected_EV = ofdp_ev.GetPrizeCollected(VehicleCategories.EV);
            prizeCollected_GDV = ofdp_gdv.GetPrizeCollected(VehicleCategories.GDV);
            numberOfVehiclesUsed_EV = ofdp_ev.GetNumberOfVehiclesUsed(VehicleCategories.EV);
            numberOfVehiclesUsed_GDV = ofdp_gdv.GetNumberOfVehiclesUsed(VehicleCategories.GDV);
            VMT_EV = ofdp_ev.GetVMT(VehicleCategories.EV);
            VMT_GDV = ofdp_gdv.GetVMT(VehicleCategories.GDV);
        }

        public ObjectiveFunctionInputDataPackage(ObjectiveFunctionInputDataPackage twin_OFIDP)
        {
            numberOfCustomersServed_EV = twin_OFIDP.GetNumberOfCustomersServed(VehicleCategories.EV);
            numberOfCustomersServed_GDV = twin_OFIDP.GetNumberOfCustomersServed(VehicleCategories.GDV);
            prizeCollected_EV = twin_OFIDP.GetPrizeCollected(VehicleCategories.EV);
            prizeCollected_GDV = twin_OFIDP.GetPrizeCollected(VehicleCategories.GDV);
            numberOfVehiclesUsed_EV = twin_OFIDP.GetNumberOfVehiclesUsed(VehicleCategories.EV);
            numberOfVehiclesUsed_GDV = twin_OFIDP.GetNumberOfVehiclesUsed(VehicleCategories.GDV);
            VMT_EV = twin_OFIDP.GetVMT(VehicleCategories.EV);
            VMT_GDV = twin_OFIDP.GetVMT(VehicleCategories.GDV);
        }

        public void Add(ObjectiveFunctionInputDataPackage theOtherOFIDP)
        {
            numberOfCustomersServed_EV += theOtherOFIDP.numberOfCustomersServed_EV;
            numberOfCustomersServed_GDV += theOtherOFIDP.numberOfCustomersServed_GDV;
            prizeCollected_EV += theOtherOFIDP.prizeCollected_EV;
            prizeCollected_GDV += theOtherOFIDP.prizeCollected_GDV;
            numberOfVehiclesUsed_EV += theOtherOFIDP.numberOfVehiclesUsed_EV;
            numberOfVehiclesUsed_GDV += theOtherOFIDP.numberOfVehiclesUsed_GDV;
            VMT_EV += theOtherOFIDP.VMT_EV;
            VMT_GDV += theOtherOFIDP.VMT_GDV;
        }
        public void Add(ObjectiveFunctionInputDataPackage theOtherOFIDP, VehicleCategories vehicleCategory)
        {
            if(vehicleCategory== VehicleCategories.EV)
            {
                numberOfCustomersServed_EV += theOtherOFIDP.numberOfCustomersServed_EV;
                prizeCollected_EV += theOtherOFIDP.prizeCollected_EV;
                numberOfVehiclesUsed_EV += theOtherOFIDP.numberOfVehiclesUsed_EV;
                VMT_EV += theOtherOFIDP.VMT_EV;
            }
            else//GDV
            {
                numberOfCustomersServed_GDV += theOtherOFIDP.numberOfCustomersServed_GDV;
                prizeCollected_GDV += theOtherOFIDP.prizeCollected_GDV;
                numberOfVehiclesUsed_GDV += theOtherOFIDP.numberOfVehiclesUsed_GDV;
                VMT_GDV += theOtherOFIDP.VMT_GDV;
            }
        }

        public static ObjectiveFunctionInputDataPackage AggregateByMerge(List<ObjectiveFunctionInputDataPackage> IndividualOFIDPs_EV = null, List<ObjectiveFunctionInputDataPackage> IndividualOFIDPs_GDV = null)
        {
            if ((IndividualOFIDPs_EV == null) && (IndividualOFIDPs_GDV == null))//Return an empty one because the individual inputs are not provided:
                return new ObjectiveFunctionInputDataPackage();
            //Initialization:
            int numberOfCustomersServed_EV=0;
            int numberOfCustomersServed_GDV=0;
            double prizeCollected_EV=0.0;
            double prizeCollected_GDV=0.0;
            int numberOfVehiclesUsed_EV=0;
            int numberOfVehiclesUsed_GDV=0;
            double VMT_EV=0.0;
            double VMT_GDV=0.0;
            double cplexObjective_EV = 0.0;
            double cplexObjective_GDV = 0.0;
            //Aggregation:
            if (IndividualOFIDPs_EV != null)
            {
                foreach (ObjectiveFunctionInputDataPackage OFIDP in IndividualOFIDPs_EV)
                {
                    numberOfCustomersServed_EV += OFIDP.numberOfCustomersServed_EV;
                    prizeCollected_EV += OFIDP.prizeCollected_EV;
                    numberOfVehiclesUsed_EV += OFIDP.numberOfVehiclesUsed_EV;
                    VMT_EV += OFIDP.VMT_EV;
                }
            }
            if (IndividualOFIDPs_GDV != null)
            {
                foreach (ObjectiveFunctionInputDataPackage OFIDP in IndividualOFIDPs_GDV)
                {
                    numberOfCustomersServed_GDV += OFIDP.numberOfCustomersServed_GDV;
                    prizeCollected_GDV += OFIDP.prizeCollected_GDV;
                    numberOfVehiclesUsed_GDV += OFIDP.numberOfVehiclesUsed_GDV;
                    VMT_GDV += OFIDP.VMT_GDV;
                }
            }
            //Return:
            return new ObjectiveFunctionInputDataPackage(numberOfCustomersServed_EV, numberOfCustomersServed_GDV, prizeCollected_EV, prizeCollected_GDV, numberOfVehiclesUsed_EV, numberOfVehiclesUsed_GDV, VMT_EV, VMT_GDV);
        }
        public static ObjectiveFunctionInputDataPackage AggregateByAddition(List<ObjectiveFunctionInputDataPackage> IndividualOFIDPs)
        {
            if (IndividualOFIDPs == null)//Return an empty one because the individual inputs are not provided:
                return new ObjectiveFunctionInputDataPackage();
            //Initialization:
            int numberOfCustomersServed_EV = 0;
            int numberOfCustomersServed_GDV = 0;
            double prizeCollected_EV = 0.0;
            double prizeCollected_GDV = 0.0;
            int numberOfVehiclesUsed_EV = 0;
            int numberOfVehiclesUsed_GDV = 0;
            double VMT_EV = 0.0;
            double VMT_GDV = 0.0;
            double cplexObjective_EV = 0.0;
            double cplexObjective_GDV = 0.0;
            //Aggregation:
            foreach (ObjectiveFunctionInputDataPackage OFIDP in IndividualOFIDPs)
            {
                numberOfCustomersServed_EV += OFIDP.numberOfCustomersServed_EV;
                numberOfCustomersServed_GDV += OFIDP.numberOfCustomersServed_GDV;
                prizeCollected_EV += OFIDP.prizeCollected_EV;
                prizeCollected_GDV += OFIDP.prizeCollected_GDV;
                numberOfVehiclesUsed_EV += OFIDP.numberOfVehiclesUsed_EV;
                numberOfVehiclesUsed_GDV += OFIDP.numberOfVehiclesUsed_GDV;
                VMT_EV += OFIDP.VMT_EV;
                VMT_GDV += OFIDP.VMT_GDV;
            }
            //Return:
            return new ObjectiveFunctionInputDataPackage(numberOfCustomersServed_EV, numberOfCustomersServed_GDV, prizeCollected_EV, prizeCollected_GDV, numberOfVehiclesUsed_EV, numberOfVehiclesUsed_GDV, VMT_EV, VMT_GDV);
        }
    }
}
