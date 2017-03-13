using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Instance_Generation.Other;


namespace Instance_Generation.FormSections
{
    public class Vehicle_RelatedData
    {
        Vehicle selectedEV;
        Vehicle selectedGDV;
        public Vehicle SelectedEV { get { return selectedEV; } }
        public Vehicle SelectedGDV { get { return selectedGDV; } }

        public Vehicle_RelatedData(
            Vehicle selectedEV,
            Vehicle selectedGDV)
        {
            this.selectedEV = selectedEV;
            this.selectedGDV = selectedGDV;
        }
    }
}
