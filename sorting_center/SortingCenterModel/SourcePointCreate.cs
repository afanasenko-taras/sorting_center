using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    class SourcePointCreate : FastAbstractEvent
    {
        private DepaletizeNode dNode;

        public SourcePointCreate(DepaletizeNode dNode)
        {
            this.dNode = dNode;
        }


        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            var sp = new SourcePoint(wrapper as SortCenterWrapper, dNode);
            wrapper.addObject(sp);
            ((SortCenterWrapper)wrapper).allSourcePoint.Add(sp);
            wrapper.WriteDebug($"New SourcePoint in {dNode.Id} create");
        }
    }
}
