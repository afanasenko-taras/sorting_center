using AbstractModel;

namespace SortingCenterModel
{
    internal class EndPickBoxFromDisassemblyChannel : FastAbstractEvent
    {
        private TransportRobot transportRobot;

        public EndPickBoxFromDisassemblyChannel(TransportRobot transportRobot)
        {
            this.transportRobot = transportRobot;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            transportRobot.EndPickBoxFromDisassemblyChannel(timeSpan);
        }
    }
}