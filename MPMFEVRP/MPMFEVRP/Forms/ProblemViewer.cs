using Braincase.GanttChart;
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
    public partial class ProblemViewer : Form
    {
        IProblem theProblem;

        public ProblemViewer(IProblem problem)
        {
            InitializeComponent();

            theProblem = problem;

            var manager = new ProjectManager();

            Random r = new Random();
            //foreach (var job in theProblem.Jobs)
            //{
            //    var task = new ColoredTask()
            //    {
            //        Name = job.Description,
            //        Color = Color.FromArgb(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256))
            //    };
            //    manager.Add(task);
            //    manager.SetEnd(task, job.DueDate);
            //    manager.SetDuration(task, job.ProcessingTime);
            //    manager.SetStart(task, job.DueDate - job.ProcessingTime);
            //}

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

    public class ColoredTask : Task
    {
        public ColoredTask() : base() { }
        public Color Color { get; set; }
    }
}
