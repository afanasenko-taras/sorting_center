using AbstractModel;

namespace SkladModel
{
    internal class WaitEvent : FastAbstractEvent
    {
        private WarehouseRobot warehouseRobot;
        public WaitEvent(WarehouseRobot warehouseRobot)
        {
            this.warehouseRobot = warehouseRobot;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            warehouseRobot.FinishLocalEvent();
        }
    }
}