using MPMFEVRP.Interfaces;
using MPMFEVRP.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MPMFEVRP.Implementations.Algorithms.Interfaces_and_Bases;

namespace MPMFEVRP.Forms
{
    public partial class AlgorithmViewer : Form
    {
        IAlgorithm theAlgorithm;

        public AlgorithmViewer(IAlgorithm anAlgorithm)
        {
            InitializeComponent();
            theAlgorithm = anAlgorithm;

            label_name.Text = theAlgorithm.GetName();
            ParamUtil.DrawParameters(panel_params, theAlgorithm.AlgorithmParameters.GetAllParameters());
        }

        private void Button_Close_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
