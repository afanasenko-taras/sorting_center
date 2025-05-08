using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    public class SourcePoint : FastAbstractObject
    {
        private SortCenterWrapper sortCenterWrapper;
        public Queue<int> fifoQueue = new Queue<int> ();
        public readonly DepaletizeNode dNode;
        public int currentSku = -1;


        public void FillQueue(int count, int sku)
        {
            currentSku = sku;
            for (int i = 0; i < count; i++)
            {
                fifoQueue.Enqueue(sku);
            }
            var log = new EventLog(sortCenterWrapper.updatedTime, sortCenterWrapper.updatedTime, uid, "startDepalletize", dNode.Id, dNode.Id, sku, count.ToString(), uid);
            sortCenterWrapper.logs_2.Add(log);
        }


        public SourcePoint(SortCenterWrapper sortCenterWrapper, DepaletizeNode dNode)
        {
            this.sortCenterWrapper = sortCenterWrapper;
            this.dNode = dNode;
        }

        public override (TimeSpan, FastAbstractEvent) getNearestEvent()
        {
            return (TimeSpan.MaxValue, null);
        }

        public override void Update(TimeSpan timeSpan)
        {

        }

        internal void finishDepaletize(TimeSpan timeSpan)
        {
            var log = new EventLog(timeSpan, timeSpan, uid, "finishDepalletize", dNode.Id, dNode.Id, currentSku, "0", uid);
            sortCenterWrapper.logs_2.Add(log);
        }
    }
}
