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
        }
    }
}
