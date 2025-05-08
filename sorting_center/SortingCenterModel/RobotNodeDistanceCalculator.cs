using System;
using System.Collections.Generic;
using System.Linq;

namespace SortingCenterModel
{
    public static class RobotNodeDistanceCalculator
    {
        public static Dictionary<RobotNode, Dictionary<RobotNode, (double distance, List<RobotNode> path)>> CalculateAllPairsShortestPaths(List<RobotNode> robotNodes)
        {
            var result = new Dictionary<RobotNode, Dictionary<RobotNode, (double distance, List<RobotNode> path)>>();

            foreach (var startNode in robotNodes)
            {
                result[startNode] = CalculateShortestPathsFromNode(startNode, robotNodes);
            }

            return result;
        }

        private static Dictionary<RobotNode, (double distance, List<RobotNode> path)> CalculateShortestPathsFromNode(RobotNode startNode, List<RobotNode> allNodes)
        {
            var distances = allNodes.ToDictionary(node => node, node => double.MaxValue);
            var previousNodes = new Dictionary<RobotNode, RobotNode>();
            var priorityQueue = new PriorityQueue<RobotNode, double>();
            var visited = new HashSet<RobotNode>();

            distances[startNode] = 0;
            priorityQueue.Enqueue(startNode, 0);

            while (priorityQueue.Count > 0)
            {
                var currentNode = priorityQueue.Dequeue();

                if (visited.Contains(currentNode))
                    continue;

                visited.Add(currentNode);

                foreach (var neighbor in currentNode.nextNodes)
                {
                    if (visited.Contains(neighbor))
                        continue;

                    double tentativeDistance = distances[currentNode] + CalculateDistance(currentNode, neighbor);

                    if (tentativeDistance < distances[neighbor])
                    {
                        distances[neighbor] = tentativeDistance;
                        previousNodes[neighbor] = currentNode;
                        priorityQueue.Enqueue(neighbor, tentativeDistance);
                    }
                }
            }

            // Восстановление путей
            var result = new Dictionary<RobotNode, (double distance, List<RobotNode> path)>();
            foreach (var node in allNodes)
            {
                var path = ReconstructPath(previousNodes, startNode, node);
                result[node] = (distances[node], path);
            }

            return result;
        }

        private static List<RobotNode> ReconstructPath(Dictionary<RobotNode, RobotNode> previousNodes, RobotNode startNode, RobotNode endNode)
        {
            var path = new List<RobotNode>();
            for (var at = endNode; at != null; at = previousNodes.GetValueOrDefault(at))
            {
                path.Add(at);
            }
            path.Reverse();

            // Если путь не начинается с начальной ноды, значит пути нет
            if (path.FirstOrDefault() != startNode)
                return new List<RobotNode>();

            return path;
        }

        private static double CalculateDistance(RobotNode from, RobotNode to)
        {
            // Пример: Евклидово расстояние
            return Math.Sqrt(Math.Pow(from.x - to.x, 2) + Math.Pow(from.y - to.y, 2));
        }
    }
}

