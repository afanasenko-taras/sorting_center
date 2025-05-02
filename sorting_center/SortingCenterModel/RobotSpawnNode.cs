namespace SortingCenterModel
{
    public class RobotSpawnNode : RobotNode
    {
        public int join_row = 0;
        public int join_col = 0;

        public RobotSpawnNode(int row, int col, int join_row, int join_col, int lineNumber) : base(row, col, lineNumber)
        {
            this.y = row;
            this.join_row = join_row;
            this.join_col = join_col;
        }

        public RobotSpawnNode(int row, int col, int column, int line, int lineNumber, int join_row, int join_col) : base(row, col, column, line, lineNumber)
        {
            this.join_row = join_row;
            this.join_col = join_col;
        }
    }
}

