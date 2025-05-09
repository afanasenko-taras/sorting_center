using AbstractModel;

namespace SortingCenterModel
{
    internal class StartPickBoxFromChannel : FastAbstractEvent
    {
        private TransportRobot transportRobot;
        private TeleportLine teleportLine;

        public StartPickBoxFromChannel(TransportRobot transportRobot, TeleportLine teleportLine)
        {
            this.transportRobot = transportRobot;
            this.teleportLine = teleportLine;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            if (transportRobot.currentNode != teleportLine.endLine)
                throw new Exception("transportRobot.currentNode != teleportLine.endLine");

            transportRobot.StartPickBoxFromChannel(timeSpan, teleportLine);
        }
    }
}