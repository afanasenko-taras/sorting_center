using AbstractModel;
using SkaldModel;

namespace SkladModel
{
    internal class ChangeAccelerationRotation : FastAbstractEvent
    {
        private WarehouseRobot warehouseRobot;
        private double accelerationDeg; // Угловое ускорение в градусах/с²

        public ChangeAccelerationRotation(WarehouseRobot warehouseRobot, double accelerationDeg)
        {
            this.warehouseRobot = warehouseRobot;
            this.accelerationDeg = accelerationDeg;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            warehouseRobot.SetAccelerationRotation(accelerationDeg);

            WarehouseWrapper warehouseWrapper = wrapper as WarehouseWrapper;
            warehouseWrapper.logs.Add(new WarehouseEvent(timeSpan, WarehouseEventType.ChangeAccelerationRotation, WarehouseObjectType.Robot, warehouseRobot, ""));
            warehouseWrapper.instructions.Add(new RMSInstruction(timeSpan, WarehouseEventType.ChangeAccelerationRotation, WarehouseObjectType.Robot, warehouseRobot, ""));


            warehouseRobot.FinishLocalEvent();
        }
    }
}