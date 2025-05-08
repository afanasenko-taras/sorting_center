using AbstractModel;

namespace SortingCenterModel
{
    internal class TransportRobotEndMove : FastAbstractEvent
    {
        private TransportRobot transportRobot;

        public TransportRobotEndMove(TransportRobot transportRobot)
        {
            this.transportRobot = transportRobot;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            transportRobot.EndMoving(timeSpan);

        }
    }
}