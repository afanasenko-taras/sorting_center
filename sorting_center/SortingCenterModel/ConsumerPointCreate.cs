using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    class ConsumerPointCreate : FastAbstractEvent
    {
        private PalettizeNode pNode;

        public ConsumerPointCreate(PalettizeNode pNode)
        {
            this.pNode = pNode;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            wrapper.addObject(new ConsumerPoint(pNode));
            wrapper.WriteDebug($"New ConsumerPoint in {pNode.Id} create");
        }
    }
}
