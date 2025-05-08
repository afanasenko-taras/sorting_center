using AbstractModel;
using System.ComponentModel.DataAnnotations;

namespace SortingCenterModel
{
    internal class StartPlaceBoxIntoChannel : FastAbstractEvent
    {
        private TransportRobot transportRobot;
        private TeleportLine teleportLine;

        public StartPlaceBoxIntoChannel(TransportRobot transportRobot, TeleportLine teleportLine)
        {
            this.transportRobot = transportRobot;
            this.teleportLine = teleportLine;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            if (transportRobot.currentNode != teleportLine.startLine)
                throw new Exception("transportRobot.currentNode != teleportLine.startLine");

            transportRobot.StartPlaceBoxIntoChannel(timeSpan, teleportLine);
        }
    }
}