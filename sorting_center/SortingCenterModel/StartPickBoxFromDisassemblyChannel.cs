using AbstractModel;

namespace SortingCenterModel
{
    internal class StartPickBoxFromDisassemblyChannel : FastAbstractEvent
    {
        private TransportRobot transportRobot;
        private SourcePoint source;

        public StartPickBoxFromDisassemblyChannel(TransportRobot transportRobot, SourcePoint source)
        {
            this.transportRobot = transportRobot;
            this.source = source;
        }

        public override void runEvent(FastAbstractWrapper wrapper, TimeSpan timeSpan)
        {
            if (!transportRobot.currentNode.getNodes.Contains(source.dNode))
                throw new Exception("!transportRobot.currentNode.depaletizeNodes.Contains(source.dNode)");
            transportRobot.StartPickBoxFromDisassemblyChannel(timeSpan, source);
        }
    }
}