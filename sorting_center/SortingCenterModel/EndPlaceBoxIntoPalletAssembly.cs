using AbstractModel;

namespace SortingCenterModel
{
    internal class EndPlaceBoxIntoPalletAssembly : FastAbstractEvent
    {
        private TransportRobot transportRobot;

        public EndPlaceBoxIntoPalletAssembly(TransportRobot transportRobot)
        {
            this.transportRobot = transportRobot;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            transportRobot.EndPlaceBoxIntoPalletAssembly(timeSpan);
        }
    }
}