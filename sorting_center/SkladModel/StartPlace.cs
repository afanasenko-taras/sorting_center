using AbstractModel;
using SkaldModel;

namespace SkladModel
{
    internal class StartPlace : FastAbstractEvent
    {
        private WarehouseRobot warehouseRobot;
        private string tableUid;

        public StartPlace(WarehouseRobot warehouseRobot, string tableUid)
        {
            this.warehouseRobot = warehouseRobot;
            this.tableUid = tableUid;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            WarehouseWrapper warehouseWrapper = wrapper as WarehouseWrapper;
            warehouseWrapper.logs.Add(new WarehouseEvent(timeSpan, WarehouseEventType.StartPlace, WarehouseObjectType.Robot, warehouseRobot, tableUid));
            warehouseWrapper.instructions.Add(new RMSInstruction(timeSpan, WarehouseEventType.StartPlace, WarehouseObjectType.Robot, warehouseRobot, tableUid));
            warehouseRobot.FinishLocalEvent();
        }
    }
}