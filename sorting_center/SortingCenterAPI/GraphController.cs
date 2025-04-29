using Microsoft.AspNetCore.Mvc;
using SortingCenterAPI.Models;
using SortingCenterModel;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SortingCenterAPI.Controllers
{

    [JsonSerializable(type: typeof(GraphNode))]
    public class GraphNode
    {
        public string Id { get; set; }
        public int X { get; set; } // Добавлено
        public int Y { get; set; } // Добавлено
    }

    [JsonSerializable(type: typeof(GraphEdge))]
    public class GraphEdge
    {
        public string Source { get; set; }
        public string Target { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class GraphController : ControllerBase
    {
        private readonly SortCenterWrapper _sortCenter;

        public GraphController()
        {
            // Создаём объект SortCenterWrapper с конфигурацией
            var config = new SortingCenterConfig
            {
                rowNumber = 5,
                columnNumber = 5,
                lineNumber = 10
            };
            _sortCenter = new SortCenterWrapper(config);
        }

        [HttpGet("state")]
        public IActionResult GetGraphState()
        {
            var response = new GraphStateResponse();

            foreach (var row in _sortCenter.robotNodes.Keys)
            {
                foreach (var col in _sortCenter.robotNodes[row].Keys)
                {
                    var node = _sortCenter.robotNodes[row][col];
                    response.Nodes.Add(new GraphNode
                    {
                        Id = node.Id,
                        X = node.x, // Передача координаты X
                        Y = node.y  // Передача координаты Y
                    });

                    foreach (var linkedNode in _sortCenter.robotNodes[row][col].nextNodes)
                    {
                        response.Edges.Add(new GraphEdge
                        {
                            Source = node.Id,
                            Target = linkedNode.Id
                        });
                    }
                }
            }

            foreach (var row in _sortCenter.lineNodes.Keys)
            {
                foreach (var x in _sortCenter.lineNodes[row].Keys)
                {
                    foreach (var row2 in _sortCenter.lineNodes[row][x].Keys)
                    {
                        var node = _sortCenter.lineNodes[row][x][row2];
                        response.LineNodes.Add(new LineNodeResponse
                        {
                            Id = node.Id,
                            X = node.x,
                            Y = node.y
                        });
                    }
                }
            }



            return Ok(response);
        }
    }
}
