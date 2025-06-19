using AbstractModel;
using SkaldModel;
using System;

namespace SkladModel
{
    internal class ChangeAcceleration : FastAbstractEvent
    {
        private WarehouseRobot warehouseRobot;
        private double acceleration;

        public ChangeAcceleration(WarehouseRobot warehouseRobot, double acceleration)
        {
            this.warehouseRobot = warehouseRobot;
            this.acceleration = acceleration;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            warehouseRobot.SetAcceleration(acceleration);

            WarehouseWrapper warehouseWrapper = wrapper as WarehouseWrapper;
            warehouseWrapper.logs.Add(new WarehouseEvent(timeSpan, WarehouseEventType.ChangeAcceleration, WarehouseObjectType.Robot, warehouseRobot, ""));
            warehouseWrapper.instructions.Add(new RMSInstruction(timeSpan, WarehouseEventType.ChangeAcceleration, WarehouseObjectType.Robot, warehouseRobot, ""));
            warehouseRobot.FinishLocalEvent();
        }
    }
}