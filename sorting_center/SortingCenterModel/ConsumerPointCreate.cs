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
        private FileQueue fileQueue;

        public ConsumerPointCreate(PalettizeNode pNode, FileQueue fileQueue)
        {
            this.pNode = pNode;
            this.fileQueue = fileQueue;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            wrapper.addObject(new ConsumerPoint(pNode, wrapper as SortCenterWrapper, fileQueue));
            wrapper.WriteDebug($"New ConsumerPoint in {pNode.Id} create");
        }
    }
}
