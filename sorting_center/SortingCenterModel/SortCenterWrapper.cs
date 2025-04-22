using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    public class SortCenterWrapper : FastAbstractWrapper
    {
        private SortingCenterConfig sortConfig;

        public SortCenterWrapper(SortingCenterConfig sortConfig)
        {
            this.sortConfig = sortConfig;
            TimeSpan startPoint = TimeSpan.Zero;
            for(int i=0; i<sortConfig.sorceNumber;i++)
            {
                startPoint = AddEvent(startPoint, new CreateSourcePoint()); 
            }
        }
    }
}
