using Braincase.GanttChart;
using MPMFEVRP.Interfaces;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MPMFEVRP.Implementations.Algorithms;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;
using MPMFEVRP.Implementations.ProblemModels;
using MPMFEVRP.Implementations.ProblemModels.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Implementations.Solutions;
using MPMFEVRP.Implementations.Solutions.Interfaces_and_Bases;
using MPMFEVRP.Domains.ProblemDomain;

namespace MPMFEVRP.Forms
{
    public partial class ProblemViewer : Form
    {
        IProblem theProblem;
        double mapAspectRatio;//y/x 
        double mapWidth, mapHeight;
        double minX, maxX, minY, maxY;
        double panelMapScale;//This is the ratio between (panelPint1-panelPoint2)/(mapPoint1-mapPoint2)
        Graphics g;

        //List<Site> allSites;

        public ProblemViewer(IProblem problem)
        {
            theProblem = problem;
            //allSites = theProblem.PDP.SRD.GetAllSitesArray().ToList();
            InitializeComponent();
            InitializePanelAndFormSizes();
            g = panel_problemViewer.CreateGraphics();

            DrawBorderAndGridlines(panel_problemViewer);
            DrawAllSites(panel_problemViewer, theProblem.PDP.SRD.GetAllSitesArray(), 20);
        }
        void InitializePanelAndFormSizes()
        {
            double currentWidth = panel_problemViewer.Width;
            double currentHeight = panel_problemViewer.Height;
            ProcessTheProblemMap();
            double currentAspectRatio = currentHeight / currentWidth;
            if (currentAspectRatio > mapAspectRatio)
            {
                panel_problemViewer.Height = (int)Math.Round(currentWidth * mapAspectRatio);
            }
            else if(currentAspectRatio < mapAspectRatio)
            {
                panel_problemViewer.Width = (int)Math.Round(currentHeight / mapAspectRatio);
            }
            panelMapScale = (double)panel_problemViewer.Width / mapWidth;
            panel_problemViewer.Height = panel_problemViewer.Height + 30;
            panel_problemViewer.Width = panel_problemViewer.Width + 30;
        }
        void ProcessTheProblemMap()// y/x ratio
        {
            if (theProblem == null)
                throw new Exception("TheProblem has not been initialized! CalculateAspectRatio invoked prematurely!");
            if (theProblem.PDP.SRD.GetAllSitesArray().Length == 0)
                throw new Exception("TheProblem does not contain any sites!");
            minX = double.MaxValue;
            maxX = double.MinValue;
            minY = double.MaxValue;
            maxY = double.MinValue;
            foreach(Site s in theProblem.PDP.SRD.GetAllSitesArray())
            {
                if (minX > s.X)
                    minX = s.X;
                if (maxX < s.X)
                    maxX = s.X;
                if (minY > s.Y)
                    minY = s.Y;
                if (maxY < s.Y)
                    maxY = s.Y;
            }
            mapWidth = maxX - minX;
            mapHeight = maxY - minY;
            if (mapWidth == 0)//TODO: Revisit this after the margins are defined, we may have a nice solution using the margins!
                mapAspectRatio = double.MaxValue;
            else
            mapAspectRatio=(mapHeight / mapWidth);
        }

        void DrawBorderAndGridlines(Panel panel)
        {
            this.Paint += (o, e) =>
            {
                Pen p = new Pen(Color.Black, 5);
                g.DrawLine(p, 0, 0, 0, panel.Height);
                g.DrawLine(p, 0, 0, 0, panel.Width);
                g.DrawLine(p, 0, 0, 0, panel.Height);

            };
        }
        void DrawAllSites(Panel panel, Site[] sites, float size)
        {
            foreach(Site site in sites)
            {
                DrawSiteAsNodeOnPanel(panel, site, size);
            }
        }
        void DrawSiteAsNodeOnPanel(Panel panel, Site site, float size)
        {

            Pen p = new Pen(Color.Black,3);
            float[] location = ConvertSiteLocationToPointOnPanel(site);
            switch (site.SiteType)
            {
                case SiteTypes.Depot:
                    p.Color = Color.Black;
                    this.Paint += (o, e) =>
                    {
                        g.DrawRectangle(p, location[0], location[1], size, size);
                    };
                    break;
                case SiteTypes.Customer:
                    p.Color = Color.Red;
                    this.Paint += (o, e) =>
                    {
                        g.DrawEllipse(p, location[0], location[1], size, size);
                    };
                    break;
                case SiteTypes.ExternalStation:
                    p.Color = Color.Purple;
                    this.Paint += (o, e) =>
                    {
                        g.DrawRectangle(p, location[0], location[1], size, size);
                    };
                    break;
                    //throw new NotImplementedException();
            }
        }
        float[] ConvertSiteLocationToPointOnPanel(Site site)//[0]:X, [1]:Y
        {
            float[] output = new float[2];
            output[0] = (float)((site.X - minX) * panelMapScale);
            output[1] = (float)((site.Y - minY) * panelMapScale);
            return output; 
        }








        //private float[] x;
        //private float[] y;
        //private int numNodes;

        //        public ProblemViewer(IProblem problem)
        //        {

        //            theProblem = problem;
        //            numNodes = theProblem.PDP.SRD.NumCustomers;
        //            x = new float[numNodes];
        //            for (int i = 0; i < numNodes - 1; i++)
        //            {
        //                x[i] = (float)(10 * theProblem.PDP.SRD.GetSiteByID(theProblem.PDP.SRD.GetSiteID(i)).X);
        //            }
        //            x[numNodes - 1] = x[0];
        //            y = new float[numNodes];
        //            for (int i = 0; i < numNodes - 1; i++)
        //            {
        //                y[i] = (float)(10 * theProblem.PDP.SRD.GetSiteByID(theProblem.PDP.SRD.GetSiteID(i)).Y);
        //            }
        //            y[numNodes - 1] = y[0];

        //            InitializeComponent();

        //            trying();
        ////            Panel_Paint(this, null);
        //            panel_problemViewer.Show();
        //        }
        void GiveRedBackground()
        {
            panel_problemViewer.BackColor = Color.Red;
        }
        void GiveWhiteBackground()
        {
            panel_problemViewer.BackColor = Color.White;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            //Pen penG = new Pen(Color.Green, 3);
            //Pen penGL = new Pen(Color.LightGreen, 3);
            //Pen penGY = new Pen(Color.Aqua, 3);
            //Pen penB = new Pen(Color.Black, 3);
            //Pen penR = new Pen(Color.Red, 2);
            //Pen penEV = new Pen(Color.Green, 2);
            //Pen penDash = new Pen(Color.Yellow, 2) { DashPattern = new float[] { 5, 1.5f } };
            //SolidBrush sb = new SolidBrush(Color.Black);
            //Graphics g = panel_problemViewer.CreateGraphics();
            //FontFamily ff = new FontFamily("Arial");
            //System.Drawing.Font font = new System.Drawing.Font(ff, 10);
            //g.DrawEllipse(penB, x[0], y[0], 20, 20);
            //g.DrawString(0.ToString(), font, sb, x[0] + 3, y[0] + 3);
            //for (int i = 1; i < x.Length - 4; i++)
            //{
            //    g.DrawEllipse(penG, x[i], y[i], 20, 20);
            //    g.DrawString(i.ToString(), font, sb, x[i] + 3, y[i] + 3);
            //}
            //for (int i = x.Length - 4; i < x.Length - 1; i++)
            //{
            //    g.DrawEllipse(penGY, x[i], y[i], 20, 20);
            //    g.DrawString(i.ToString(), font, sb, x[i] + 3, y[i] + 3);
              
            //}
        }
        
    }

    class DrawableNode
    {

    }

}
