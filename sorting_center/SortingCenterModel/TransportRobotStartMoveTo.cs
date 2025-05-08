using AbstractModel;

namespace SortingCenterModel
{
    internal class TransportRobotStartMoveTo : FastAbstractEvent
    {
        private TransportRobot transportRobot;
        private RobotNode nextNode;

        public TransportRobotStartMoveTo(TransportRobot transportRobot, RobotNode nextNode)
        {
            this.transportRobot = transportRobot;
            this.nextNode = nextNode;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            transportRobot.StartMoving(timeSpan, nextNode);
        }
    }
}