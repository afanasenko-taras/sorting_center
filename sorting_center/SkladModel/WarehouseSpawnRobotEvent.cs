using AbstractModel;
using SkladModel;

namespace SkaldModel
{
    internal class WarehouseSpawnRobotEvent : FastAbstractEvent
    {
        private RobotSpawn spawn;
        private RobotConfig robotConfig;

        public WarehouseSpawnRobotEvent(RobotSpawn spawn, RobotConfig robotConfig)
        {
            this.spawn = spawn;
            this.robotConfig = robotConfig;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            WarehouseWrapper warehouseWrapper = wrapper as WarehouseWrapper;
            warehouseWrapper.logs.Add(new WarehouseEvent(timeSpan, WarehouseEventType.Create, WarehouseObjectType.Robot, spawn.position, spawn.uid));
            wrapper.addObject(new WarehouseRobot(spawn, timeSpan, warehouseWrapper, robotConfig, null));
        }
    }
}