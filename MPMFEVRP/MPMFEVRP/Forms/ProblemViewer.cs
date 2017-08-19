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

namespace MPMFEVRP.Forms
{
    public partial class ProblemViewer : Form
    {
        IProblem theProblem;
        private float[] x;
        private float[] y;
        private int numNodes;
        public ProblemViewer(IProblem problem)
        {
            
            theProblem = problem;
            numNodes = theProblem.PDP.SRD.NumCustomers;
            x = new float[numNodes];
            for (int i = 0; i < numNodes - 1; i++)
            {
                x[i] = (float)(10 * theProblem.PDP.SRD.GetSiteByID(theProblem.PDP.SRD.GetSiteID(i)).X);
            }
            x[numNodes - 1] = x[0];
            y = new float[numNodes];
            for (int i = 0; i < numNodes - 1; i++)
            {
                y[i] = (float)(10 * theProblem.PDP.SRD.GetSiteByID(theProblem.PDP.SRD.GetSiteID(i)).Y);
            }
            y[numNodes - 1] = y[0];

            InitializeComponent();
            Panel_Paint(this, null);
            panel_problemViewer.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            Pen penG = new Pen(Color.Green, 3);
            Pen penGL = new Pen(Color.LightGreen, 3);
            Pen penGY = new Pen(Color.Aqua, 3);
            Pen penB = new Pen(Color.Black, 3);
            Pen penR = new Pen(Color.Red, 2);
            Pen penEV = new Pen(Color.Green, 2);
            Pen penDash = new Pen(Color.Yellow, 2) { DashPattern = new float[] { 5, 1.5f } };
            SolidBrush sb = new SolidBrush(Color.Black);
            Graphics g = panel_problemViewer.CreateGraphics();
            FontFamily ff = new FontFamily("Arial");
            System.Drawing.Font font = new System.Drawing.Font(ff, 10);
            g.DrawEllipse(penB, x[0], y[0], 20, 20);
            g.DrawString(0.ToString(), font, sb, x[0] + 3, y[0] + 3);
            for (int i = 1; i < x.Length - 4; i++)
            {
                g.DrawEllipse(penG, x[i], y[i], 20, 20);
                g.DrawString(i.ToString(), font, sb, x[i] + 3, y[i] + 3);
            }
            for (int i = x.Length - 4; i < x.Length - 1; i++)
            {
                g.DrawEllipse(penGY, x[i], y[i], 20, 20);
                g.DrawString(i.ToString(), font, sb, x[i] + 3, y[i] + 3);
              
            }
        }
        
    }

    public class ColoredTask : Task
    {
        public ColoredTask() : base() { }
        public Color Color { get; set; }
    }
}
