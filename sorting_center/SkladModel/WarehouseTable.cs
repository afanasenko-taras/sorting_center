using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkladModel
{
    class WarehouseTable : FastAbstractObject
    {
        private Spawn table;
        private TimeSpan timeSpan;
        WarehouseWrapper wrapper;

        public WarehouseTable(Spawn table, TimeSpan timeSpan, WarehouseWrapper wrapper)
        {
            this.table = table;
            this.timeSpan = timeSpan;
            this.wrapper = wrapper;
            this.uid = table.uid;
        }

        public override (TimeSpan, FastAbstractEvent) getNearestEvent()
        {
            return (TimeSpan.MaxValue, null);
        }

        public override void Update(TimeSpan timeSpan)
        {

        }
    }
}
