using AbstractModel;

namespace SortingCenterModel
{
    internal class EndPlaceBoxIntoChannel : FastAbstractEvent
    {
        private TransportRobot transportRobot;

        public EndPlaceBoxIntoChannel(TransportRobot transportRobot)
        {
            this.transportRobot = transportRobot;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            transportRobot.EndPlaceBoxIntoChannel(timeSpan);
        }
    }
}