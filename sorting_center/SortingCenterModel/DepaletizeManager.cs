using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    public class DepaletizeManager
    {
        private SortCenterWrapper wrapper;


        public DepaletizeManager(SortCenterWrapper wrapper)
        {
            this.wrapper = wrapper;
        }

        public void ManageDepaletize()
        {
  


            foreach(SourcePoint sourcePoint in wrapper.allSourcePoint)
            {
                int paleteSize = wrapper.sortConfig.palleteSize;
                if (sourcePoint.fifoQueue.Count == 0)
                {
                    (int minSku, int countSku) =  wrapper.GetMinSkuCount();
                    if (countSku <= wrapper.sortConfig.minSku)
                    {
                        sourcePoint.FillQueue(paleteSize, minSku);
                        wrapper.WriteDebug($"{sourcePoint.dNode.Id} add {paleteSize} sku {minSku}");
                    }
                }
            }
        }
    }
}
