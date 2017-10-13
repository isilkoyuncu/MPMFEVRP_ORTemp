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
    public partial class HybridTreeSearchAndSetPartitionCharts : Form, CustomerSetTreeSearchListener
    {
        public HybridTreeSearchAndSetPartitionCharts()
        {
            InitializeComponent();
        }
        delegate void OnChangeOfNumberOfUnexploredCustomerSetsCallback(int[] newNumberUnexplored);

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
                BarChart.Series[0].Points.Clear();
                for (int l = newNumberUnexplored.Length-1; l >= 0 ; l--)
                    BarChart.Series[0].Points.AddXY(l.ToString(), newNumberUnexplored[l]);
                //if (newNumberUnexplored.Length > BarChart.Series[0].Points.Count)
                //{
                //    for (int l = BarChart.Series[0].Points.Count; l < newNumberUnexplored.Length; l++)
                //        BarChart.Series[0].Points.AddXY(l.ToString(), 0);
                //}
                //for (int l = 0; l < newNumberUnexplored.Length; l++)
                //    BarChart.Series[0].Points[l].SetValueY(newNumberUnexplored[l]);
                BarChart.Update();
                //this.Refresh();
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

    }
}
