using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPMFEVRP.Utils
{
    public class FlexibleStringMultiDoubleTuple
    {
        string id; public string ID { get { return id; }set { id = value; } }
        double item1; public double Item1 { get { return item1; }}
        double item2; public double Item2 { get { return item2; }}
        double item3; public double Item3 { get { return item3; } }

        public FlexibleStringMultiDoubleTuple(string id, double item1, double item2)
        {
            this.id = id;
            this.item1 = item1;
            this.item2 = item2;
        }
        public FlexibleStringMultiDoubleTuple(string id, double item1, double item2, double item3)
        {
            this.id = id;
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
        }
        public FlexibleStringMultiDoubleTuple(string id)
        {
            this.id = id;
        }
        public FlexibleStringMultiDoubleTuple(FlexibleStringMultiDoubleTuple twinSiteAuxBounds)
        {
            id = twinSiteAuxBounds.id;
            item1 = twinSiteAuxBounds.item1;
            item2 = twinSiteAuxBounds.item2;
            item3 = twinSiteAuxBounds.item3;
        }
        public void SetItem1(double item1)
        {
            this.item1 = item1;
        }
        public void SetItem2(double item2)
        {
            this.item2 = item2;
        }
        public void SetItem3(double item3)
        {
            this.item3 = item3;
        }
    }
}
