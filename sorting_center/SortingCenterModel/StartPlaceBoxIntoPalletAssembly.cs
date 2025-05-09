using AbstractModel;

namespace SortingCenterModel
{
    internal class StartPlaceBoxIntoPalletAssembly : FastAbstractEvent
    {
        private TransportRobot transportRobot;
        private ConsumerPoint consumer;

        public StartPlaceBoxIntoPalletAssembly(TransportRobot transportRobot, ConsumerPoint consumer)
        {
            this.transportRobot = transportRobot;
            this.consumer = consumer;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            transportRobot.StartPlaceBoxIntoPalletAssembly(timeSpan, consumer);
        }
    }
}