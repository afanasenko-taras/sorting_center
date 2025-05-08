using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    class TransportRobotSpawnPointsCreate : FastAbstractEvent
    {
        private int number;

        public TransportRobotSpawnPointsCreate(int number)
        {
            this.number = number;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            SortCenterWrapper sortCenterWrapper = wrapper as SortCenterWrapper;
            foreach (var spawn in sortCenterWrapper.robotSpawnNodes)
            {
                wrapper.addObject(new TransportRobotSpawnPoint(spawn, number, timeSpan));
                sortCenterWrapper.WriteDebug($"TransportRobotSpawnPoint {spawn.Id} created");
            }
            
        }
    }
}
