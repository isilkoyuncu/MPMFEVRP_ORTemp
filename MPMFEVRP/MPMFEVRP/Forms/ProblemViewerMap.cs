using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Forms;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Domains.ProblemDomain;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Drawing;

namespace MPMFEVRP.Forms
{
    public partial class ProblemViewerMap : Form
    {

        IProblem theProblem;
        List<Site> allSites;
        List<GMarkerGoogleType> markerList;
        List<SiteMarker> allSiteMarkers;

        public ProblemViewerMap(IProblem problem)
        {
            theProblem = problem;
            allSites = theProblem.PDP.SRD.GetAllSitesArray().ToList();
            allSiteMarkers = new List<SiteMarker>();
            markerList = new List<GMarkerGoogleType>();
            markerList.Add(GMarkerGoogleType.blue);
            markerList.Add(GMarkerGoogleType.red);
            markerList.Add(GMarkerGoogleType.purple);
            InitializeComponent();
            MapSites();
        }
        public void MapSites()
        {
            GMapOverlay markersOverlay = new GMapOverlay("markers");
            GMapOverlay linesOverlay = new GMapOverlay("lines");
            gMapControl1.Overlays.Clear();
            gMapControl1.Overlays.Add(markersOverlay);
            gMapControl1.Overlays.Add(linesOverlay);
            Site depot = allSites.Where(x => x.SiteType == SiteTypes.Depot).ToList().First();
            PointLatLng depotLoc = new PointLatLng(depot.Y, depot.X);
            for(int i = allSites.Count - 1; i >=0; i--)
            {
                Site s = allSites[i];
                PointLatLng siteLoc = new PointLatLng(s.Y, s.X);
                GMarkerGoogle thisMarker;
                if(s.SiteType == SiteTypes.Depot)
                {
                    thisMarker = new GMarkerGoogle(siteLoc, markerList[0]);
                }
                else if(s.SiteType == SiteTypes.Customer)
                {
                    thisMarker = new GMarkerGoogle(siteLoc, markerList[1]);
                    GMapRoute thisRoute = new GMapRoute(new List<PointLatLng>() { depotLoc, siteLoc }, "aaa");
                    thisRoute.Stroke.Color = Color.Turquoise;
                    linesOverlay.Routes.Add(thisRoute);
                }
                else
                {
                    thisMarker = new GMarkerGoogle(siteLoc, markerList[2]);
                }
                allSiteMarkers.Add(new SiteMarker(s, thisMarker));
                markersOverlay.Markers.Add(thisMarker);
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            gMapControl1.OnMarkerClick += new GMap.NET.WindowsForms.MarkerClick(GMapControl1_OnMarkerClick);
            gMapControl1.SetPositionByKeywords("United States");
        }
        void GMapControl1_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            Site selSite = allSiteMarkers.Where(x => x.Marker == (GMarkerGoogle)item).ToList<SiteMarker>().First().Site;
            listBox1.Items.Clear();
            listBox1.Items.Add("Node ID is : " + selSite.ID.ToString());
            listBox1.Items.Add("Recharging rate is : " + selSite.RechargingRate.ToString());
            listBox1.Items.Add("EV Prize is : " + selSite.Prize[0].ToString());
            listBox1.Items.Add("GDV Prize is : " + selSite.Prize[1].ToString());
        }
    }
    public class SiteMarker
    {
        public Site Site { get; set; }
        public GMarkerGoogle Marker { get; set; }
        public SiteMarker(Site s, GMarkerGoogle m)
        {
            Site = s;
            Marker = m;
        }
    }
}