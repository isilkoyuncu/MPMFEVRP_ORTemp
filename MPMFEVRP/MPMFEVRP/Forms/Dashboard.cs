using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Instance_Generation.Forms;

namespace MPMFEVRP.Forms
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void Button_single_Click(object sender, EventArgs e)
        {
            new SingleProblemSingleAlgorithm().ShowDialog();
        }

        private void Button_multiple_Click(object sender, EventArgs e)
        {
            new MultipleProblemMultipleAlgorithm().ShowDialog();
        }

        private void Button_DataManager_Click(object sender, EventArgs e)
        {
            new TestInstanceGenerator().ShowDialog();
        }

        private void Button_TSRuns_Click(object sender, EventArgs e)
        {
            new TS_Runs().ShowDialog();
        }
    }
}
