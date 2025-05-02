using SortingCenterAPI.Controllers;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace SortingCenterAPI.Models
{
    [JsonSerializable(type: typeof(GraphStateResponse))]
    public class GraphStateResponse
    {
        public List<GraphNode> Nodes { get; set; } = new List<GraphNode>();
        public List<GraphEdge> Edges { get; set; } = new List<GraphEdge>();
        public List<LineNodeResponse> LineNodes { get; set; } = new List<LineNodeResponse>();
        public List<LineNodeResponse> DepaletizeNodes { get; set; } = new List<LineNodeResponse>();
        public List<LineNodeResponse> PalettizeNodes { get; set; } = new List<LineNodeResponse>();


        public List<GraphEdge> GetEdges { get; set; } = new List<GraphEdge>();
        public List<GraphEdge> DropEdges { get; set; } = new List<GraphEdge>();
    }

    public class LineNodeResponse
    {
        public string Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
