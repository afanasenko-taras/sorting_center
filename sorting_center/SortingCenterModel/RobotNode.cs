using System.Data.Common;

namespace SortingCenterModel
{
    public class Reservation()
    {
        public List<RobotNode> robotNodes = new List<RobotNode>();
    }
    public class RobotNode : NodeBase
    {
        public List<RobotNode> nextNodes { get; set; } = new List<RobotNode>();
        public List<LineNode> dropNodes { get; set; } = new List<LineNode>();
        public List<LineNode> getNodes { get; set; } = new List<LineNode>();
        public List<DepaletizeNode> depaletizeNodes { get; set; } = new List<DepaletizeNode>();
        public List<PalettizeNode> paletizeNodes { get; set; } = new List<PalettizeNode>();
        public (bool isReserved, string uid) Reserved { get; set; } = (false, "");


        public bool isDistanceCalculated { get; set; } = false;

        public RobotNode(int row, int col, int column, int line, int lineNumber)
            : base(row, col, col, row * (lineNumber + 1), $"Node_{row}_{column}_{line}_{col}")
        {
        }

        public RobotNode(int row, int col, int lineNumber)
            : base(row, col, col, row * (lineNumber + 1), $"Node_{row}_{col}")
        {
        }

        public RobotNode(int row, int col, int lineNumber, string nodeName)
            : base(row, col, col, row* (lineNumber + 1), nodeName)
        {
        }

        public RobotNode(int row, int col, int x, int y, string id) : base(row, col, x, y, id)
        {
        }

        internal void AddLinkedNode(RobotNode robotNode)
        {
            nextNodes.Add(robotNode);
        }
    }
}
