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

        // Внедрение зависимости через конструктор
        public GraphController(SortCenterWrapper sortCenter)
        {
            _sortCenter = sortCenter;
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


                    foreach (var linkedNode in _sortCenter.robotNodes[row][col].getNodes)
                    {
                        response.GetEdges.Add(new GraphEdge
                        {
                            Source = node.Id,
                            Target = linkedNode.Id
                        });
                    }

                    foreach (var linkedNode in _sortCenter.robotNodes[row][col].dropNodes)
                    {
                        response.DropEdges.Add(new GraphEdge
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

            foreach(var depaletizeNode in _sortCenter.depaletizeNodes)
            {
                response.DepaletizeNodes.Add(new LineNodeResponse
                {
                    Id = depaletizeNode.Id,
                    X = depaletizeNode.x,
                    Y = depaletizeNode.y
                });
            }

            foreach (var paletizeNode in _sortCenter.palettizeNodes)
            {
                response.PalettizeNodes.Add(new LineNodeResponse
                {
                    Id = paletizeNode.Id,
                    X = paletizeNode.x,
                    Y = paletizeNode.y
                });
            }


            foreach (var robotSpawnNode in _sortCenter.robotSpawnNodes)
            {
                response.Nodes.Add(new GraphNode
                {
                    Id = robotSpawnNode.Id,
                    X = robotSpawnNode.x,
                    Y = robotSpawnNode.y
                });
            }


            return Ok(response);
        }
    }
}
