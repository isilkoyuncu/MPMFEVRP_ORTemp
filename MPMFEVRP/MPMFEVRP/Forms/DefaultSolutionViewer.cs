using Braincase.GanttChart;
using MPMFEVRP.Domains.ProblemDomain;
using MPMFEVRP.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MPMFEVRP.Forms
{
    public partial class DefaultSolutionViewer : Form
    {
        IProblem theProblem;
        ISolution theSolution;

        public DefaultSolutionViewer(IProblem problem, ISolution solution)
        {
            InitializeComponent();

            theProblem = problem;
            theSolution = solution;

            //Dictionary<string, Job> solutionSortedJobs = new Dictionary<string, Job>();

            //foreach (var job in problem.Jobs)
            //{
            //    solutionSortedJobs.Add(job.ID, job);
            //}

            var manager = new ProjectManager();

            Random r = new Random();
            int cumulTime = 0;
            int maxLateness = int.MinValue;
            int lateness = 0;
            //foreach (var jobID in theSolution.IDs)
            //{
            //    var task = new ColoredTask()
            //    {
            //        //Name = solutionSortedJobs[jobID].Description,
            //        Color = Color.FromArgb(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256)),
            //    };
            //    manager.Add(task);
            //    manager.SetStart(task, cumulTime);
            //    //manager.SetDuration(task, solutionSortedJobs[jobID].ProcessingTime);
            //    //manager.SetEnd(task, cumulTime += solutionSortedJobs[jobID].ProcessingTime);
            //    //lateness = cumulTime - solutionSortedJobs[jobID].DueDate;
            //    task.Name += " (late by " + lateness.ToString() + ")";
            //    if (maxLateness < lateness)
            //        maxLateness = lateness;
            //}
            var ghostSummaryTask = new ColoredTask()
            {
                Name = "Max. Lateness = " + maxLateness.ToString(),
                Color = Color.Black
            };
            manager.Add(ghostSummaryTask);
            manager.SetDuration(ghostSummaryTask, 0);
            manager.SetEnd(ghostSummaryTask, 0);

            var chart = new Chart();
            chart.Init(manager);
            chart.AllowTaskDragDrop = false;

            this.Controls.Add(chart);

            chart.PaintTask += (s, e) =>
            {
                ColoredTask ctask = e.Task as ColoredTask;
                if (ctask != null)
                {
                    var format = new TaskFormat();
                    format = e.Format;
                    format.BackFill = new SolidBrush(ctask.Color);
                    e.Format = format;
                }
            };
        }
    }
}
