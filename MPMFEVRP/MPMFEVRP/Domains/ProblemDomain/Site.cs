﻿using System;

namespace MPMFEVRP.Domains.ProblemDomain
{
    public class Site
    {
        //This is that new "Site" class, everything we need to know regarding a particular (depot, customer, refueling, etc.) site will be retrieved from within this class
        string id;
        string type;
        SiteTypes siteType;
        ESTypes eSType;
        double x;
        double y;
        double demand;
        double readyTime;
        double dueDate;
        double serviceDuration;
        double rechargingRate;
        double refuelingCostPerKWH;
        double[] prize;

        public string ID { get { return id; } }
        public string Type { get { return type; } }
        public SiteTypes SiteType { get { return siteType; } }
        public ESTypes ESType { get { return eSType; } }
        public double X { get { return x; } }
        public double Y { get { return y; } }
        public double Demand { get { return demand; } }
        public double ReadyTime { get { return readyTime; } }
        public double DueDate { get { return dueDate; } }
        public double ServiceDuration { get { return serviceDuration; } }
        public double RechargingRate { get { return rechargingRate; } }
        public double RefuelingCostPerKWH { get { return refuelingCostPerKWH; } }
        public double[] Prize { get { return prize; } }

        public Site() { }

        public Site(string id, string type, double x, double y, double demand, double readyTime, double dueDate, double serviceDuration, double rechargingRate, double refuelingCostPerKWH, double[] prize)
        {
            this.id = id;
            this.type = type;
            siteType = ConvertStringTypeToSiteTypes(type);
            this.x = x;
            this.y = y;
            this.demand = demand;
            this.readyTime = readyTime;
            this.dueDate = dueDate;
            this.serviceDuration = serviceDuration;
            this.refuelingCostPerKWH = refuelingCostPerKWH;
            this.rechargingRate = rechargingRate;
            this.prize = (double[])prize.Clone();
        }

        public Site(Site twinSite)
        {
            id = twinSite.id;
            type = twinSite.type;
            siteType = twinSite.siteType;
            eSType = twinSite.eSType;
            x = twinSite.x;
            y = twinSite.y;
            demand = twinSite.demand;
            readyTime = twinSite.readyTime;
            dueDate = twinSite.dueDate;
            serviceDuration = twinSite.serviceDuration;
            rechargingRate = twinSite.rechargingRate;
            refuelingCostPerKWH = twinSite.refuelingCostPerKWH;
            prize = (double[])twinSite.prize.Clone();
        }

        SiteTypes ConvertStringTypeToSiteTypes(string strSiteType)
        {
            switch (strSiteType.Substring(0, 1))
            {
                case "c":
                    return SiteTypes.Customer;
                case "d":
                    return SiteTypes.Depot;
                case "e":
                    return SiteTypes.ExternalStation;
                default:
                    throw new Exception("Site type incompatible!");
            }
        }
        
        public double GetPrize(VehicleCategories vehCategory)
        {
            if(vehCategory == VehicleCategories.EV)
            {
                return prize[0];
            }
            else
            {
                return prize[1];
            }
        }

    }
}
