namespace SortingCenterModel
{
    public class LineNode : NodeBase
    {
        public int line { get; set; }
        public int row2 { get; set; }

        public LineNode(int row, int col, int line, int row2, int num_sub_row, int x)
            : base(row, col, x, (row - 1) * (num_sub_row + 1) + row2 + 1,
                 $"LineNode_{row}_{col}_{line}_{x}_{row2}")
        {
            this.line = line;
            this.row2 = row2;
        }
    }
}
