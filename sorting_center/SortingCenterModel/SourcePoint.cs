using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    class SourcePoint : FastAbstractObject
    {
        public override (TimeSpan, FastAbstractEvent) getNearestEvent()
        {
            throw new NotImplementedException();
        }

        public override void Update(TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }
    }
}
