using AbstractModel;

namespace SortingCenterModel
{
    internal class EndPickingUpFromChannel : FastAbstractEvent
    {
        private TransportRobot transportRobot;

        public EndPickingUpFromChannel(TransportRobot transportRobot)
        {
            this.transportRobot = transportRobot;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            transportRobot.EndPickingUpFromChannel(timeSpan);
        }
    }
}