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

namespace MPMFEVRP.Forms
{
    public partial class HybridTreeSearchAndSetPartitionCharts : Form, CustomerSetTreeSearchListener, UpperBoundListener
    {
        DateTime chartwideStartTime;
        public HybridTreeSearchAndSetPartitionCharts()
        {
            InitializeComponent();
        }
        delegate void OnChangeOfNumberOfUnexploredCustomerSetsCallback(int[] newNumberUnexplored);
        delegate void OnChangeOfNumbersOfUnexploredAndExploredCustomerSetsCallback(int[] newNumberUnexplored, int[] newNumberAll);
        delegate void OnUpperBoundUpdateCallBack(double newUpperBound);

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
                for (int l = newNumberUnexplored.Length-1; l > 0 ; l--)
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
                OnUpperBoundUpdateCallBack os = new OnUpperBoundUpdateCallBack(OnUpperBoundUpdate);
                this.Invoke(os, new object[] { newUpperBound });
            }
            else if (this.Visible)
            {
                DateTime now = DateTime.Now;
                this.AllCharts.Series["UpperBound"].Points.AddXY(Math.Round((now-chartwideStartTime).TotalSeconds,0), Math.Round(newUpperBound, 2));

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
            if (this.AllCharts.Series[seriesName].Points.Count > 2)
                this.AllCharts.Series[seriesName].Points[this.AllCharts.Series[seriesName].Points.Count - 2].Label = "";
            // Set the last label as value
            this.AllCharts.Series[seriesName].Points[this.AllCharts.Series[seriesName].Points.Count - 1].Label = value.ToString();
        }

        private void HybridTreeSearchAndSetPartitionCharts_Load(object sender, EventArgs e)
        {
            chartwideStartTime = DateTime.Now;
        }
    }
}
