using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using System.Drawing;

namespace MPMFEVRP.Utils
{
    public class NetworkGraphicalUtil
    {
        public static SvgUnit nodeWidth = 40;
        public static SvgUnit marginWidth = 50;
        public static SvgUnit maxLongEdgeLength = 1000;//3000;
        public static SvgUnit maxShortEdgeLength = 1000;//2250;
        public static SvgUnit arcLength = 30;

    }

    public class SiteNetworkToSvgConverter
    {
        Size size;
        double svgMapScale;
        Dictionary<Site, SvgPoint> siteCentralPoints = new Dictionary<Site, SvgPoint>();

        public SiteNetworkToSvgConverter(Site[] sites)
        {
            double maxX = double.MinValue;
            double minX = double.MaxValue;
            double maxY = double.MinValue;
            double minY = double.MaxValue;
            foreach (Site s in sites)
            {
                maxX = Math.Max(s.X, maxX);
                minX = Math.Min(s.X, minX);
                maxY = Math.Max(s.Y, maxY);
                minY = Math.Min(s.Y, minY);
            }
            double mapWidth = maxX - minX;
            double mapHeight = maxY - minY;

            double mapAspectRatio = mapWidth / mapHeight;//x/y 
            svgMapScale //This is the ratio between (svgPoint1-svgPoint2)/(mapPoint1-mapPoint2)
                = (mapWidth >= mapHeight)
                ? Math.Min((NetworkGraphicalUtil.maxLongEdgeLength - 2 * NetworkGraphicalUtil.marginWidth) / mapWidth, (NetworkGraphicalUtil.maxShortEdgeLength - 2 * NetworkGraphicalUtil.marginWidth) / mapHeight)
                : Math.Min((NetworkGraphicalUtil.maxLongEdgeLength - 2 * NetworkGraphicalUtil.marginWidth) / mapHeight, (NetworkGraphicalUtil.maxShortEdgeLength - 2 * NetworkGraphicalUtil.marginWidth) / mapWidth);
            size = new Size((int)(svgMapScale * mapWidth + 2 * NetworkGraphicalUtil.marginWidth), (int)(svgMapScale * mapHeight + 2 * NetworkGraphicalUtil.marginWidth));

            foreach (Site s in sites)
                siteCentralPoints.Add(s, ConvertMapPointToSvgPoint(s.X, s.Y, minX, maxY));
        }
        SvgPoint ConvertMapPointToSvgPoint(double X, double Y, double minX, double maxY)//maxY because the maxY in the map corresponds to the top of the image, which is y=0
        {
            float[] output = new float[2];
            output[0] = (float)((X - minX) * svgMapScale) + (float)0.5*NetworkGraphicalUtil.marginWidth;
            output[1] = (float)(-1*(Y - maxY) * svgMapScale) + (float)0.5*NetworkGraphicalUtil.marginWidth;
            return new SvgPoint(output[0], output[1]);
        }

        public SvgDocument GetGridLayer()
        {
            SvgDocument outcome = new SvgDocument();
            SvgRectangle mapRegion = new SvgRectangle();
            SvgRectangle docRegion = new SvgRectangle();
            outcome.Children.Add(mapRegion);
            SvgUnit minX = 1000, minY = 1000, minXorY = 1000, maxX = 0, maxY = 0;
            foreach (SvgPoint sp in siteCentralPoints.Values)
            {
                if (minX > sp.X)
                    minX = sp.X;
                if (minY > sp.Y)
                    minY = sp.Y;
                if (maxX < sp.X)
                    maxX = sp.X;
                if (maxY < sp.Y)
                    maxY = sp.Y;
            }
            minXorY = Math.Min(minX, minY);
            mapRegion.X = minX;
            mapRegion.Y = minY;
            mapRegion.Width = (maxX - minX);
            mapRegion.Height = (maxY - minY);
            mapRegion.Stroke = new SvgColourServer(Color.Red);
            mapRegion.StrokeWidth = 1;
            mapRegion.Fill = new SvgColourServer(Color.Transparent);
            docRegion.X = 0;
            docRegion.Y = 0;
            docRegion.Width = maxX + minXorY;
            docRegion.Height = maxY + minXorY;
            docRegion.Stroke = new SvgColourServer(Color.Black);
            docRegion.StrokeWidth = 10;
            docRegion.Fill = new SvgColourServer(Color.Transparent);
            outcome.Children.Add(docRegion);
            return outcome;
        }

        public SvgDocument GetNodesLayer(List<SiteTypes> siteTypes, List<string> allVisitedSiteIDs = null)
        {
            SvgDocument outcome = new SvgDocument();
            foreach (Site s in siteCentralPoints.Keys)
            {
                if ((allVisitedSiteIDs == null) || (allVisitedSiteIDs.Contains(s.ID)))
                    outcome.Children.Add(SvgNode(s));
            }
            return outcome;
        }
        SvgElement SvgNode(Site site)
        {
            SvgPoint centerPoint = siteCentralPoints[site];
            switch (site.SiteType)
            {
                case SiteTypes.Depot:
                    SvgRectangle r = new SvgRectangle();
                    r.Width = NetworkGraphicalUtil.nodeWidth;
                    r.Height = NetworkGraphicalUtil.nodeWidth;
                    r.X = centerPoint.X - NetworkGraphicalUtil.nodeWidth / 2;
                    r.Y = centerPoint.Y - NetworkGraphicalUtil.nodeWidth / 2;
                    r.Stroke = new SvgColourServer(Color.Black);
                    r.StrokeWidth = 2;
                    r.Fill = new SvgColourServer(Color.Gray);
                    r.ID = site.ID;
                    return r;
                case SiteTypes.Customer:
                    SvgCircle c = new SvgCircle();
                    c.Radius = NetworkGraphicalUtil.nodeWidth / 2;
                    c.CenterX = centerPoint.X;
                    c.CenterY = centerPoint.Y;
                    c.Stroke = new SvgColourServer(Color.DarkGoldenrod);
                    c.StrokeWidth = 2;
                    c.Fill = new SvgColourServer(Color.Gold);
                    c.ID = site.ID;
                    return c;
                case SiteTypes.ExternalStation:
                    SvgPolygon t = new SvgPolygon();
                    SvgPoint bottom_right = Move(centerPoint, Math.PI * 30.0 / 180.0, (SvgUnit)0.5 * NetworkGraphicalUtil.nodeWidth);
                    SvgPoint bottom_left = Move(centerPoint, Math.PI * 150.0 / 180.0, (SvgUnit)0.5 * NetworkGraphicalUtil.nodeWidth);
                    SvgPointCollection pColl = new SvgPointCollection() {
                        bottom_right.X, bottom_right.Y,//bottom-right
                        bottom_left.X, bottom_left.Y,//bottom-left
                        centerPoint.X, centerPoint.Y - NetworkGraphicalUtil.nodeWidth / 2 //top
                    };
                    t.Points = pColl;
                    t.Stroke = new SvgColourServer(Color.DarkGreen);
                    t.StrokeWidth = 2;
                    t.Fill = new SvgColourServer(Color.SpringGreen);
                    t.ID = site.ID;
                    return t;
                default:
                    throw new Exception("SvgNode invoked for a SiteType that's not accounted for!");
            }
        }

        public SvgDocument GetArcsLayer(ISolution solution)//TODO: Make work on solutions of various types
        {
            List<Site> EVVisitSites = new List<Site>();
            List<Site> GDVVisitSites = new List<Site>();
            if (solution == null)
            //return null;
            {
                List<Site> allSites = siteCentralPoints.Keys.ToList();
                EVVisitSites = new List<Site>() { allSites[0], allSites[4], allSites[8], allSites[0] };
                GDVVisitSites = new List<Site>() { allSites[0], allSites[3], allSites[9], allSites[0] };
            }
            //TODO: Obtain EV and GDV points
            SvgDocument outcome = new SvgDocument();
            foreach (SvgElement arc in GetArcs(EVVisitSites, VehicleCategories.EV))
                outcome.Children.Add(arc);
            foreach (SvgElement arc in GetArcs(GDVVisitSites, VehicleCategories.GDV))
                AddElementAndAllDescendantsToDocument(outcome, arc);
            return outcome;
        }
        List<SvgElement> GetArcs(List<Site> visitSites, VehicleCategories vehicleCategory)
        {
            if (visitSites == null)
                return null;
            if (visitSites.Count < 2)
                return null;
            List<SvgElement> outcome = new List<SvgElement>();
            Site from = visitSites[0];
            for (int i = 1; i < visitSites.Count; i++)
            {
                Site to = visitSites[i];
                outcome.Add(SvgArc(from, to, vehicleCategory));
                from = to;
            }
            return outcome;
        }
        SvgElement SvgArc(string fromSiteID, string toSiteID, VehicleCategories vehicleCategory)
        {
            return SvgArc(GetSiteByID(fromSiteID),GetSiteByID(toSiteID),vehicleCategory);
        }
        Site GetSiteByID(string siteID)
        {
            foreach (Site s in siteCentralPoints.Keys)
                if (s.ID == siteID)
                    return s;
            return null;
        }
        SvgElement SvgArc(Site from, Site to, VehicleCategories vehicleCategory)
        {
            if ((from == null) || (to == null))
                return null;

            SvgPoint fromSvgPoint = siteCentralPoints[from];
            SvgPoint toSvgPoint = siteCentralPoints[to];
            double direction_radians = Math.Atan2(toSvgPoint.Y - fromSvgPoint.Y, toSvgPoint.X - fromSvgPoint.X);
            fromSvgPoint = GetIntersectionPoint(from, direction_radians);
            toSvgPoint = GetIntersectionPoint(to, direction_radians + Math.PI);

            return SvgArc(fromSvgPoint, toSvgPoint, vehicleCategory);
        }
        SvgElement SvgArc(SvgPoint from, SvgPoint to, VehicleCategories vehicleCategory)
        {
            Color arcColor = (vehicleCategory == VehicleCategories.EV) ? Color.Green : Color.Black;

            SvgLine outcome = new SvgLine();

            double direction_radians = Math.Atan2(to.Y - from.Y, to.X - from.X);

            SvgPoint startPoint = from;// Move(from, direction_radians, NetworkGraphicalUtil.nodeWidth / 2);
            outcome.StartX = startPoint.X;
            outcome.StartY = startPoint.Y;
            SvgPoint endPoint = to;// Move(to, direction_radians + Math.PI, NetworkGraphicalUtil.nodeWidth / 2);
            outcome.EndX = endPoint.X;
            outcome.EndY = endPoint.Y;
            outcome.StrokeWidth = 2;
            outcome.Stroke = new SvgColourServer(arcColor);

            outcome.Children.Add(Arrowhead(endPoint, direction_radians, arcColor));
            return outcome;
        }
        SvgPoint GetIntersectionPoint(string siteID, double direction_radians)
        {
            foreach (Site s in siteCentralPoints.Keys)
                if (s.ID == siteID)
                    return GetIntersectionPoint(s, direction_radians);
            return new SvgPoint();
        }
        SvgPoint GetIntersectionPoint(Site site, double direction_radians)
        {
            return GetIntersectionPoint(SvgNode(site), direction_radians);
        }
        SvgPoint GetIntersectionPoint(SvgElement element, double direction_radians)
        {
            if (element is SvgCircle)
                return GetIntersectionPoint((SvgCircle)element, direction_radians);
            if (element is SvgRectangle)
                return GetIntersectionPoint((SvgRectangle)element, direction_radians);
            if (element is SvgPolygon)
                return GetIntersectionPoint((SvgPolygon)element, direction_radians);
            else return new SvgPoint();
        }
        SvgPoint GetIntersectionPoint(SvgCircle circle, double direction_radians)
        {
            return Move(circle.Center, direction_radians, NetworkGraphicalUtil.nodeWidth / 2);
        }
        SvgPoint GetIntersectionPoint(SvgRectangle rectangle, double direction_radians)
        {
            SvgUnit x=0, y=0;
            int roundedMultiplierOf90 = (int)Math.Round(direction_radians / (0.5 * Math.PI));
            switch (roundedMultiplierOf90)
            {
                case 0:
                    x = rectangle.X + rectangle.Width;
                    y = rectangle.Y + (SvgUnit)0.5 * rectangle.Width + (SvgUnit)0.5 * rectangle.Width * (SvgUnit) Math.Tan(direction_radians);
                    break;
                case 1:
                    x = rectangle.X+ (SvgUnit)0.5 * rectangle.Width + (SvgUnit)0.5 * rectangle.Width / (SvgUnit)Math.Tan(direction_radians);
                    y = rectangle.Y + rectangle.Width;
                    break;
                case 2:
                    x = rectangle.X;
                    y = rectangle.Y + (SvgUnit)0.5 * rectangle.Width - (SvgUnit)0.5 * rectangle.Width * (SvgUnit)Math.Tan(direction_radians);
                    break;
                case 3:
                    x = rectangle.X + (SvgUnit)0.5 * rectangle.Width + (SvgUnit)0.5 * rectangle.Width / (SvgUnit)Math.Tan(direction_radians);
                    y = rectangle.Y;
                    break;
            }
            SvgPoint outcome = new SvgPoint();
            outcome.X = x;
            outcome.Y = y;
            return outcome;// Move(rectangle.Center, direction_radians, NetworkGraphicalUtil.nodeWidth / 2);
        }
        SvgPoint GetIntersectionPoint(SvgPolygon polygon, double direction_radians)
        {
            //This works only for triangle because that's the only kind of polygon we use in nodes
           // double remainderOf120 = (direction_radians - Math.PI * 90.0 / 180.0) % (Math.PI * 120.0 / 180.0);
            int roundedMultiplierOf120 = (int)Math.Round((direction_radians - Math.PI * 90.0 / 180.0) / (Math.PI * 120.0 / 180.0));
            double remainderOf120_v2 = direction_radians - Math.PI * 90.0 / 180.0 - roundedMultiplierOf120 * Math.PI * 120.0 / 180.0;
            double absAngle = Math.Abs(remainderOf120_v2);
            SvgUnit moveDistance = (NetworkGraphicalUtil.nodeWidth / (SvgUnit)4.0)/ (SvgUnit)Math.Cos(absAngle);
            SvgUnit centerX = (SvgUnit)(polygon.Points[0] + polygon.Points[2] + polygon.Points[4]) / (SvgUnit)3.0;
            SvgUnit centerY = (SvgUnit)(polygon.Points[1] + polygon.Points[3] + polygon.Points[5]) / (SvgUnit)3.0;
            SvgPoint reconstuctedCenter = new SvgPoint(centerX, centerY);
            return Move(reconstuctedCenter, direction_radians, moveDistance);
        }
        SvgElement Arrowhead(SvgPoint tip, double radians, Color color)
        {
            SvgPolygon outcome = new SvgPolygon();
            double angle = 60.0 / 180.0 * Math.PI;
            SvgPointCollection pColl = new SvgPointCollection() {
                        tip.X, tip.Y, //tip
                        tip.X + (SvgUnit)Math.Cos(radians+Math.PI+angle/2)*NetworkGraphicalUtil.arcLength, tip.Y + (SvgUnit)Math.Sin(radians+Math.PI+angle/2)*NetworkGraphicalUtil.arcLength,
                        tip.X + (SvgUnit)Math.Cos(radians+Math.PI-angle/2)*NetworkGraphicalUtil.arcLength, tip.Y + (SvgUnit)Math.Sin(radians+Math.PI-angle/2)*NetworkGraphicalUtil.arcLength
                    };
            outcome.Points = pColl;
            outcome.Stroke = new SvgColourServer(color);
            outcome.StrokeWidth = 2;
            outcome.Fill = new SvgColourServer(color);
            return outcome;
        }
        SvgPoint Move(SvgPoint point, double angle, SvgUnit length)
        {
            SvgPoint outcome = new SvgPoint();
            outcome.X = point.X + (SvgUnit)Math.Cos(angle) * length;
            outcome.Y = point.Y + (SvgUnit)Math.Sin(angle) * length;
            return outcome;
        }
        private void AddElementAndAllDescendantsToDocument(SvgDocument theDocument, SvgElement element)
        {
            theDocument.Children.Add(element);
            foreach (SvgElement child in element.Children)
                AddElementAndAllDescendantsToDocument(theDocument, child);
        }

        public SvgDocument GetAllLayersCombined(List<SiteTypes> siteTypes, bool showGrid, ISolution solution)
        {
            SvgDocument outcome = GetNodesLayer(siteTypes);
            if (showGrid)
                AddElementAndAllDescendantsToDocument(outcome, GetGridLayer());
            AddElementAndAllDescendantsToDocument(outcome, GetArcsLayer(solution));
            return outcome;
        }
    }
}
