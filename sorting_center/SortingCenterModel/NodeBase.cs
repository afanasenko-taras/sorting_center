namespace SortingCenterModel
{
    public abstract class NodeBase
    {
        public int x { get; set; }
        public int y { get; set; }
        public int row { get; set; }
        public int col { get; set; }
        public string Id { get; set; }

        protected NodeBase(int row, int col, int x, int y, string id)
        {
            this.row = row;
            this.col = col;
            this.x = x;
            this.y = y;
            this.Id = id;
        }
    }
}
