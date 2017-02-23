using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Implementations.Problems;
using MPMFEVRP.Interfaces;
using MPMFEVRP.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MPMFEVRP.Forms
{
    public partial class DataManager : Form
    {
        IProblem theProblem;

        public DataManager()
        {
            InitializeComponent();
        }

        private void button_generateRandom_Click(object sender, EventArgs e)
        {
            int numberOfJobs = int.Parse(textBox_numberOfJobs.Text),
                dueDateLowerLimit = int.Parse(textBox_dueDateLowerLimit.Text),
                dueDateUpperLimit = int.Parse(textBox_dueDateUpperLimit.Text),
                processingTimeLowerLimit = int.Parse(textBox_processingTimeLowerLimit.Text),
                processingTimeUpperLimit = int.Parse(textBox_processingTimeUpperLimit.Text);
            theProblem = ProblemUtil.CreateRandomProblem(numberOfJobs, dueDateLowerLimit, dueDateUpperLimit, processingTimeLowerLimit, processingTimeUpperLimit);
            UpdateProblemLabels();
        }

        void UpdateProblemLabels()
        {
            //label_numberOfJobs.Text = theProblem.Jobs.Count.ToString();
            throw new NotImplementedException();
        }

        private void button_addJob_Click(object sender, EventArgs e)
        {
            if (theProblem == null)
                theProblem = new DefaultProblem();
            int dueDate = int.Parse(textBox_dueDate.Text),
                processingTime = int.Parse(textBox_processingTime.Text);
            string description = textBox_description.Text;
            //theProblem.Jobs.Add(new Job(processingTime, dueDate, description)); //TODO needs to be DELETED !!!!!
            UpdateProblemLabels();
        }

        private void button_viewProblem_Click(object sender, EventArgs e)
        {
            if (theProblem != null)
                new ProblemViewer(theProblem).ShowDialog();
        }

        private void button_reset_Click(object sender, EventArgs e)
        {
            theProblem = new DefaultProblem();
            UpdateProblemLabels();
        }

        private void button_run_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "TXT Files|*.txt";
            saveFile.Title = "Save raw data as TXT file";
            saveFile.ShowDialog();

            if (saveFile.FileName != "")
            {
                File.WriteAllText(saveFile.FileName, theProblem.CreateRawData());
                var result = MessageBox.Show("Raw data saved to " + saveFile.FileName + "." + Environment.NewLine + "Do you want to open it?", "Raw data saved", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(saveFile.FileName);
                }
            }
        }
    }
}
