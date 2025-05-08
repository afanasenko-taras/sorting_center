using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    class TransportRobotCreate : FastAbstractEvent
    {
        private TransportRobotSpawnPoint transportRobotSpawnPoint;

        public TransportRobotCreate(TransportRobotSpawnPoint transportRobotSpawnPoint)
        {
            this.transportRobotSpawnPoint = transportRobotSpawnPoint;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            if (transportRobotSpawnPoint.spawn.Reserved.isReserved)
            {
                transportRobotSpawnPoint.readyTime = timeSpan;
                wrapper.WriteDebug($"{timeSpan} Point for new transportRobot is busy {transportRobotSpawnPoint.spawn.Id}");
            }
            else
            {
                var robot = new TransportRobot(transportRobotSpawnPoint.spawn, (SortCenterWrapper)wrapper, timeSpan);
                wrapper.addObject(robot);
                ((SortCenterWrapper)wrapper).robots.Add(robot);
                transportRobotSpawnPoint.RobotSpawned();
                wrapper.WriteDebug($"{timeSpan} Create new robot {robot.uid}");

            }
        }
    }
}
