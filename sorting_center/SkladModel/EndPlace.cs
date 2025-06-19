using AbstractModel;
using SkaldModel;

namespace SkladModel
{
    internal class EndPlace : FastAbstractEvent
    {
        private WarehouseRobot warehouseRobot;
        private string tableUid;

        public EndPlace(WarehouseRobot warehouseRobot, string tableUid)
        {
            this.warehouseRobot = warehouseRobot;
            this.tableUid = tableUid;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            WarehouseWrapper warehouseWrapper = wrapper as WarehouseWrapper;
            warehouseWrapper.logs.Add(new WarehouseEvent(timeSpan, WarehouseEventType.EndPlace, WarehouseObjectType.Robot, warehouseRobot, tableUid));
            warehouseWrapper.instructions.Add(new RMSInstruction(timeSpan, WarehouseEventType.EndPlace, WarehouseObjectType.Robot, warehouseRobot, tableUid));
            warehouseRobot.FinishLocalEvent();
        }
    }
}