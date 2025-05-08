using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    class TransportRobotSpawnPoint : FastAbstractObject
    {
        public readonly RobotSpawnNode spawn;
        private int number;
        public TimeSpan readyTime;

        public TransportRobotSpawnPoint(RobotSpawnNode spawn, int number, TimeSpan timeSpan)
        {
            this.spawn = spawn;
            this.number = number;
            this.readyTime = timeSpan;
        }

        public void RobotSpawned()
        {
            number -= 1;
        }

        public override (TimeSpan, FastAbstractEvent) getNearestEvent()
        {
            if (number > 0)
                return (readyTime + TimeSpan.FromSeconds(1), new TransportRobotCreate(this));
            else
                return (TimeSpan.MaxValue, null);
        }

        public override void Update(TimeSpan timeSpan)
        {
            return;
        }
    }
}
