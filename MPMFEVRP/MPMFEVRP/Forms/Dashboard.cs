using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            MessageBox.Show("This part is currently under development. It will eventually link to the Data Manager, which is a different project within this environment.");
            //TODO: Fix the following line to the other project, not just a form within this project, and then uncomment it as well as eliminate the message box in the line above.
            //new DataManager().Show();
        }
    }
}
