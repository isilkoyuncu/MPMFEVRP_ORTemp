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
            System.Windows.Forms.DataVisualization.Charting.LineAnnotation lineAnnotation1 = new System.Windows.Forms.DataVisualization.Charting.LineAnnotation();
            System.Windows.Forms.DataVisualization.Charting.LineAnnotation lineAnnotation2 = new System.Windows.Forms.DataVisualization.Charting.LineAnnotation();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.Title title2 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.Title title3 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.AllCharts = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.AllCharts)).BeginInit();
            this.SuspendLayout();
            // 
            // AllCharts
            // 
            lineAnnotation1.Height = 100D;
            lineAnnotation1.Name = "LineAnnotation1";
            lineAnnotation1.Width = 0D;
            lineAnnotation1.X = 50D;
            lineAnnotation1.Y = 0D;
            lineAnnotation2.Height = 0D;
            lineAnnotation2.Name = "LineAnnotation2";
            lineAnnotation2.Width = 50D;
            lineAnnotation2.X = 50D;
            lineAnnotation2.Y = 49.75D;
            this.AllCharts.Annotations.Add(lineAnnotation1);
            this.AllCharts.Annotations.Add(lineAnnotation2);
            chartArea1.AxisX.IsMarginVisible = false;
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisX.MajorTickMark.Enabled = false;
            chartArea1.AxisX.Title = "Level (Number of customers in a set)";
            chartArea1.AxisY.MajorGrid.Enabled = false;
            chartArea1.AxisY.MajorTickMark.Enabled = false;
            chartArea1.AxisY.Title = "Number of customer sets at level";
            chartArea1.Name = "HorizontalBarArea";
            chartArea1.Position.Auto = false;
            chartArea1.Position.Height = 93F;
            chartArea1.Position.Width = 50F;
            chartArea1.Position.Y = 2F;
            chartArea2.AxisX.MajorGrid.Enabled = false;
            chartArea2.AxisX.MajorTickMark.Enabled = false;
            chartArea2.AxisX.Minimum = 0D;
            chartArea2.AxisX.Title = "Elapsed time (seconds)";
            chartArea2.AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            chartArea2.AxisX2.LabelStyle.Enabled = false;
            chartArea2.AxisX2.MajorGrid.Enabled = false;
            chartArea2.AxisX2.MajorTickMark.Enabled = false;
            chartArea2.AxisX2.MajorTickMark.TickMarkStyle = System.Windows.Forms.DataVisualization.Charting.TickMarkStyle.None;
            chartArea2.AxisY.MajorGrid.Enabled = false;
            chartArea2.AxisY.MajorTickMark.Enabled = false;
            chartArea2.AxisY.Title = "OFV";
            chartArea2.Name = "TimeSeriesArea";
            chartArea2.Position.Auto = false;
            chartArea2.Position.Height = 43F;
            chartArea2.Position.Width = 50F;
            chartArea2.Position.X = 50F;
            chartArea2.Position.Y = 2F;
            chartArea3.Name = "PieChartArea";
            chartArea3.Position.Auto = false;
            chartArea3.Position.Height = 41F;
            chartArea3.Position.Width = 50F;
            chartArea3.Position.X = 50F;
            chartArea3.Position.Y = 52F;
            this.AllCharts.ChartAreas.Add(chartArea1);
            this.AllCharts.ChartAreas.Add(chartArea2);
            this.AllCharts.ChartAreas.Add(chartArea3);
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.DockedToChartArea = "HorizontalBarArea";
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.IsDockedInsideChartArea = false;
            legend1.Name = "Legend_HorizontalBarChart";
            legend2.Alignment = System.Drawing.StringAlignment.Center;
            legend2.DockedToChartArea = "TimeSeriesArea";
            legend2.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend2.IsDockedInsideChartArea = false;
            legend2.Name = "Legend_TimeSeriesChart";
            legend3.Alignment = System.Drawing.StringAlignment.Center;
            legend3.DockedToChartArea = "PieChartArea";
            legend3.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend3.IsDockedInsideChartArea = false;
            legend3.Name = "Legend_PieChart";
            this.AllCharts.Legends.Add(legend1);
            this.AllCharts.Legends.Add(legend2);
            this.AllCharts.Legends.Add(legend3);
            this.AllCharts.Location = new System.Drawing.Point(12, 12);
            this.AllCharts.Name = "AllCharts";
            series1.ChartArea = "HorizontalBarArea";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedBar;
            series1.Color = System.Drawing.Color.FromArgb(((int)(((byte)(163)))), ((int)(((byte)(38)))), ((int)(((byte)(56)))));
            series1.Legend = "Legend_HorizontalBarChart";
            series1.Name = "Unexplored";
            series2.ChartArea = "HorizontalBarArea";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedBar;
            series2.Color = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(181)))), ((int)(((byte)(20)))));
            series2.Legend = "Legend_HorizontalBarChart";
            series2.Name = "Explored";
            series3.ChartArea = "TimeSeriesArea";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Color = System.Drawing.Color.FromArgb(((int)(((byte)(163)))), ((int)(((byte)(38)))), ((int)(((byte)(56)))));
            series3.Legend = "Legend_TimeSeriesChart";
            series3.Name = "UpperBound";
            series4.ChartArea = "PieChartArea";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series4.IsValueShownAsLabel = true;
            series4.LabelFormat = "0.0";
            series4.Legend = "Legend_PieChart";
            series4.Name = "TimeSpent";
            this.AllCharts.Series.Add(series1);
            this.AllCharts.Series.Add(series2);
            this.AllCharts.Series.Add(series3);
            this.AllCharts.Series.Add(series4);
            this.AllCharts.Size = new System.Drawing.Size(800, 600);
            this.AllCharts.TabIndex = 0;
            this.AllCharts.Text = "chart1";
            title1.DockedToChartArea = "HorizontalBarArea";
            title1.IsDockedInsideChartArea = false;
            title1.Name = "NumbersPerLevel";
            title1.Text = "Numbers per Level";
            title2.DockedToChartArea = "TimeSeriesArea";
            title2.IsDockedInsideChartArea = false;
            title2.Name = "Bounds";
            title2.Text = "Bounds";
            title3.DockedToChartArea = "PieChartArea";
            title3.IsDockedInsideChartArea = false;
            title3.Name = "TimeSpent";
            title3.Text = "Time spent";
            this.AllCharts.Titles.Add(title1);
            this.AllCharts.Titles.Add(title2);
            this.AllCharts.Titles.Add(title3);
            // 
            // HybridTreeSearchAndSetPartitionCharts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 621);
            this.Controls.Add(this.AllCharts);
            this.Name = "HybridTreeSearchAndSetPartitionCharts";
            this.Text = "HybridTreeSearchAndSetPartitionCharts";
            this.Load += new System.EventHandler(this.HybridTreeSearchAndSetPartitionCharts_Load);
            ((System.ComponentModel.ISupportInitialize)(this.AllCharts)).EndInit();
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart AllCharts;
    }
}