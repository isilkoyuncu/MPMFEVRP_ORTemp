using System.Windows.Forms;
using MPMFEVRP.SupplementaryInterfaces.Listeners;

namespace MPMFEVRP.Forms
{
    public partial class HybridTreeSearchAndSetPartitionCharts
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.AllCharts = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.AllCharts)).BeginInit();
            this.SuspendLayout();
            // 
            // AllCharts
            // 
            chartArea1.Name = "HorizontalBarArea";
            chartArea1.Position.Auto = false;
            chartArea1.Position.Height = 81.26013F;
            chartArea1.Position.Width = 45F;
            chartArea1.Position.X = 3F;
            chartArea1.Position.Y = 3F;
            chartArea2.Name = "TimeSeriesArea";
            chartArea2.Position.Auto = false;
            chartArea2.Position.Height = 35F;
            chartArea2.Position.Width = 45F;
            chartArea2.Position.X = 48F;
            chartArea2.Position.Y = 3F;
            this.AllCharts.ChartAreas.Add(chartArea1);
            this.AllCharts.ChartAreas.Add(chartArea2);
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Name = "Legend1";
            this.AllCharts.Legends.Add(legend1);
            this.AllCharts.Location = new System.Drawing.Point(12, 12);
            this.AllCharts.Name = "AllCharts";
            series1.ChartArea = "HorizontalBarArea";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
            series1.Legend = "Legend1";
            series1.Name = "Unexplored";
            series2.ChartArea = "TimeSeriesArea";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Legend = "Legend1";
            series2.Name = "UpperBound";
            this.AllCharts.Series.Add(series1);
            this.AllCharts.Series.Add(series2);
            this.AllCharts.Size = new System.Drawing.Size(779, 556);
            this.AllCharts.TabIndex = 0;
            this.AllCharts.Text = "chart1";
            title1.Name = "NumbersPerLevel";
            title1.Text = "Numbers per Level";
            this.AllCharts.Titles.Add(title1);
            // 
            // HybridTreeSearchAndSetPartitionCharts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 580);
            this.Controls.Add(this.AllCharts);
            this.Name = "HybridTreeSearchAndSetPartitionCharts";
            this.Text = "HybridTreeSearchAndSetPartitionCharts";
            ((System.ComponentModel.ISupportInitialize)(this.AllCharts)).EndInit();
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart AllCharts;
    }
}