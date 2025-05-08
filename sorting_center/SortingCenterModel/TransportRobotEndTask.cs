using AbstractModel;

namespace SortingCenterModel
{
    internal class TransportRobotEndTask : FastAbstractEvent
    {
        private TransportRobot transportRobot;

        public TransportRobotEndTask(TransportRobot transportRobot)
        {
            this.transportRobot = transportRobot;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
  
        }
    }
}