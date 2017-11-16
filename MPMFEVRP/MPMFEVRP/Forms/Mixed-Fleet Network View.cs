using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.Problems.Interfaces_and_Bases;
using MPMFEVRP.Utils;
using Svg;

namespace MPMFEVRP.Forms
{
    public partial class Mixed_Fleet_Network_View : Form
    {
        IProblem theProblem;
        private List<SvgDocument> layers = new List<SvgDocument>();
        SiteNetworkToSvgConverter svgSource;

        public Mixed_Fleet_Network_View(IProblem Problem)
        {
            theProblem = Problem;
            InitializeComponent(); InitializeAdditionalComponents();
             svgSource = new SiteNetworkToSvgConverter(theProblem.PDP.SRD.GetAllSitesArray());
            //SvgDocument layer_Grid = svgSource.GetGridLayer();
            //layers.Add(layer_Grid);
            //SvgDocument layer_Nodes = svgSource.GetNodesLayer(new List<SiteTypes>() { SiteTypes.Depot, SiteTypes.Customer, SiteTypes.ExternalStation });
            //layers.Add(layer_Nodes);
            //SvgDocument layer_Arcs = svgSource.GetArcsLayer(null);
            //layers.Add(layer_Arcs);
            SvgDocument completeMap = svgSource.GetAllLayersCombined(new List<SiteTypes>() { SiteTypes.Depot, SiteTypes.Customer, SiteTypes.ExternalStation }, false, null);
            layers.Add(completeMap);
        }
        private void InitializeAdditionalComponents()
        {
            graphics_Base = panel_Base.CreateGraphics();
        }

        private void panel_Base_Paint(object sender, PaintEventArgs e)
        {
            foreach (SvgDocument layer in layers)
            {
                Bitmap bmp2Draw = layer.Draw();
                bmp2Draw.Save("Image"+layers.IndexOf(layer).ToString()+".png");

                int bmpWidth = bmp2Draw.Width;
                int bmpHeight = bmp2Draw.Height;
                double xScale = (double)(panel_Base.Width-panel_Base.Padding.Left-panel_Base.Padding.Right) / (double)bmpWidth;
                double yScale = (double)(panel_Base.Height-panel_Base.Padding.Top-panel_Base.Padding.Bottom) / (double)bmpHeight;
                double scale = Math.Min(xScale, yScale);
                Size size = new Size((int)Math.Ceiling(scale * bmpWidth), (int)Math.Ceiling(scale * bmpHeight));
                Point startPoint = new Point(panel_Base.Padding.Left, panel_Base.Padding.Top);//(panel_Base.Padding.Left, panel_Base.Padding.Top)
                if (xScale > scale)
                {
                    startPoint.X += (int)Math.Floor((panel_Base.Width - panel_Base.Padding.Left - panel_Base.Padding.Right - scale * bmpWidth) / 2);
                }
                if (yScale > scale)
                {
                    startPoint.Y += (int)Math.Floor((panel_Base.Height - panel_Base.Padding.Top - panel_Base.Padding.Bottom - scale * bmpHeight) / 2);
                }
                Rectangle rectangle2DrawOn = new Rectangle(startPoint, size);
                graphics_Base.DrawImage(bmp2Draw, rectangle2DrawOn, 0, 0, bmp2Draw.Width+1, bmp2Draw.Height+1, GraphicsUnit.Pixel);
            }
        }
    }
}
