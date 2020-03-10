using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MPMFEVRP.SupplementaryInterfaces.Listeners;
using System.Windows.Forms.DataVisualization.Charting;

namespace MPMFEVRP.Forms
{
    public partial class ClusterBasedSetCoverCharts : Form, CustomerSetTreeSearchListener, UpperBoundListener, TimeSpentAccountListener
    {
        DateTime chartwideStartTime;
        public ClusterBasedSetCoverCharts()
        {
            InitializeComponent();
        }
        delegate void OnChangeOfNumberOfUnexploredCustomerSetsCallback(int[] newNumberUnexplored);
        delegate void OnChangeOfNumbersOfUnexploredAndExploredCustomerSetsCallback(int[] newNumberUnexplored, int[] newNumberAll);
        delegate void OnUpperBoundUpdateCallback(double newUpperBound);
        delegate void OnChangeOfTimeSpentAccountCallback(Dictionary<string, double> newTimeSpentAccount);

        public void OnChangeOfNumberOfUnexploredCustomerSets(int newNumberUnexplored)
        {
            throw new System.NotImplementedException();
        }

        public void OnChangeOfNumberOfUnexploredCustomerSets(int[] newNumberUnexplored)
        {
            if (this.InvokeRequired)
            {
                OnChangeOfNumberOfUnexploredCustomerSetsCallback os = new OnChangeOfNumberOfUnexploredCustomerSetsCallback(OnChangeOfNumberOfUnexploredCustomerSets);
                this.Invoke(os, new object[] { newNumberUnexplored });
            }
            else if (this.Visible)
            {
                AllCharts.Series["Unexplored"].Points.Clear();
                for (int l = newNumberUnexplored.Length - 1; l > 0; l--)
                    AllCharts.Series["Unexplored"].Points.AddXY(l.ToString(), newNumberUnexplored[l]);
                AllCharts.Update();
            }
        }

        public void OnChangeOfNumberOfAllCustomerSets(int newNumberAll)
        {
            throw new System.NotImplementedException();
        }

        public void OnChangeOfNumberOfAllCustomerSets(int[] newNumberAll)
        {
            throw new System.NotImplementedException();
        }

        public void OnChangeOfNumbersOfUnexploredAndExploredCustomerSets(int newNumberUnexplored, int newNumberAll)
        {
            throw new NotImplementedException();
        }

        public void OnChangeOfNumbersOfUnexploredAndExploredCustomerSets(int[] newNumberUnexplored, int[] newNumberAll)
        {
            if (this.InvokeRequired)
            {
                OnChangeOfNumbersOfUnexploredAndExploredCustomerSetsCallback os = new OnChangeOfNumbersOfUnexploredAndExploredCustomerSetsCallback(OnChangeOfNumbersOfUnexploredAndExploredCustomerSets);
                this.Invoke(os, new object[] { newNumberUnexplored, newNumberAll });
            }
            else if (this.Visible)
            {
                AllCharts.Series["Unexplored"].Points.Clear();
                AllCharts.Series["Explored"].Points.Clear();
                for (int l = newNumberUnexplored.Length - 1; l > 0; l--)
                {
                    AllCharts.Series["Unexplored"].Points.AddXY(l.ToString(), newNumberUnexplored[l]);
                    AllCharts.Series["Explored"].Points.AddXY(l.ToString(), newNumberAll[l] - newNumberUnexplored[l]);
                }
                AllCharts.Update();
            }
        }

        public void OnUpperBoundUpdate(double newUpperBound)
        {
            if (this.InvokeRequired)
            {
                OnUpperBoundUpdateCallback os = new OnUpperBoundUpdateCallback(OnUpperBoundUpdate);
                this.Invoke(os, new object[] { newUpperBound });
            }
            else if (this.Visible)
            {
                DateTime now = DateTime.Now;
                this.AllCharts.Series["UpperBound"].Points.AddXY(Math.Round((now - chartwideStartTime).TotalSeconds, 0), Math.Round(newUpperBound, 2));
                if (AllCharts.Series["UpperBound"].Points.Count == 1)
                {
                    double maximum = Math.Round(newUpperBound, 2);
                    AllCharts.ChartAreas["TimeSeriesArea"].AxisY.Maximum = maximum;
                    AllCharts.ChartAreas["TimeSeriesArea"].AxisY.Interval = (maximum / 4.0);
                }
                SetLastLabelAsValue("UpperBound", Math.Round(newUpperBound, 2));
            }

        }
        private void SetLastLabelAsValue(int seriesIndex, object value)
        {
            // Reset previous label
            if (this.AllCharts.Series[seriesIndex].Points.Count > 2)
                this.AllCharts.Series[seriesIndex].Points[this.AllCharts.Series[seriesIndex].Points.Count - 2].Label = "";
            // Set the last label as value
            this.AllCharts.Series[seriesIndex].Points[this.AllCharts.Series[seriesIndex].Points.Count - 1].Label = value.ToString();
        }
        private void SetLastLabelAsValue(string seriesName, object value)
        {
            // Reset previous label
            if (this.AllCharts.Series[seriesName].Points.Count > 1)//If you want to hold on to the label of the very first UB, change the rhs of this condition to 2
                this.AllCharts.Series[seriesName].Points[this.AllCharts.Series[seriesName].Points.Count - 2].Label = "";
            // Set the last label as value
            this.AllCharts.Series[seriesName].Points[this.AllCharts.Series[seriesName].Points.Count - 1].Label = value.ToString();
        }

        public void OnChangeOfTimeSpentAccount(Dictionary<string, double> newTimeSpentAccount)
        {
            if (this.InvokeRequired)
            {
                OnChangeOfTimeSpentAccountCallback os = new OnChangeOfTimeSpentAccountCallback(OnChangeOfTimeSpentAccount);
                this.Invoke(os, new object[] { newTimeSpentAccount });
            }
            else if (this.Visible)
            {
                foreach (DataPoint dp in AllCharts.Series["TimeSpent"].Points)
                {
                    string extractedKey = dp.AxisLabel;

                    if (newTimeSpentAccount.ContainsKey(extractedKey))
                    {
                        dp.SetValueY(newTimeSpentAccount[extractedKey]);
                        newTimeSpentAccount.Remove(extractedKey);
                    }
                }
                foreach (string key in newTimeSpentAccount.Keys)
                {
                    AllCharts.Series["TimeSpent"].Points.AddXY(key, newTimeSpentAccount[key]);
                }
            }
        }

        private void ClusterBasedSetCoverCharts_Load(object sender, EventArgs e)
        {
            chartwideStartTime = DateTime.Now;
        }
    }
}
