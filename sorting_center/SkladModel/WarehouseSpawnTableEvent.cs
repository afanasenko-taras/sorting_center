using AbstractModel;
using SkladModel;
using System;

namespace SkaldModel
{
    internal class WarehouseSpawnTableEvent : FastAbstractEvent
    {
        private Spawn table;

        public WarehouseSpawnTableEvent(Spawn table)
        {
            this.table = table;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            WarehouseWrapper warehouseWrapper = wrapper as WarehouseWrapper;
            warehouseWrapper.logs.Add(new WarehouseEvent(timeSpan, WarehouseEventType.Create, WarehouseObjectType.Table, table.position, table.uid));
            wrapper.addObject(new WarehouseTable(table, timeSpan, wrapper as WarehouseWrapper));
        }
    }
}