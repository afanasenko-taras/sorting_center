using SortingCenterModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SortingCenterModel
{
    public class RandomRobotManager
    {
        private readonly SortCenterWrapper _wrapper;
        private Random rnd = new Random();

        public RandomRobotManager(SortCenterWrapper wrapper)
        {
            _wrapper = wrapper;
        }

        private List<TeleportLine> FindLinesWithSku(int targetSku, RobotNode currentNode)
        {
            return _wrapper.teleportLinesList
                .Where(line =>
                    line.boxes.Any(b => b.sku == targetSku) &&
                    _wrapper.shortestPaths.ContainsKey(currentNode) &&
                    _wrapper.shortestPaths[currentNode].ContainsKey(line.endLine) // Путь к endLine для забора
                )
                .Select(line =>
                {
                    var boxesList = line.boxes.ToList();
                    var targetIndex = boxesList.FindIndex(b => b.sku == targetSku);
                    return new
                    {
                        Line = line,
                        Depth = targetIndex, // Глубина целевого SKU (0 = верхний)
                        Distance = _wrapper.shortestPaths[currentNode][line.endLine].distance
                    };
                })
                .OrderBy(x => x.Depth) // Ближе к началу очереди -> меньше перемещений
                .ThenBy(x => x.Distance)
                .Select(x => x.Line)
                .ToList();
        }

        private List<TeleportLine> FindLinesForDrop(RobotNode currentNode)
        {
            return _wrapper.teleportLinesList
                .Where(line =>
                    line.boxes.Count<10 &&
                    _wrapper.shortestPaths.ContainsKey(currentNode) &&
                    _wrapper.shortestPaths[currentNode].ContainsKey(line.startLine) // Путь к endLine для забора
                )
                .Select(line =>
                {
                    return new
                    {
                        Line = line,
                        Distance = _wrapper.shortestPaths[currentNode][line.startLine].distance
                    };
                })
                .OrderBy(x => x.Distance) // Ближе к началу очереди -> меньше перемещений
                .Select(x => x.Line)
                .ToList();
        }


        private List<TeleportLine> FindLinesForDropMECGP(RobotNode currentNode)
        {
            return _wrapper.teleportLinesList
                .Where(line =>
                    line.boxes.Count < 10 &&
                    _wrapper.shortestPaths.ContainsKey(currentNode) &&
                    _wrapper.shortestPaths[currentNode].ContainsKey(line.startLine) // Путь к endLine для забора
                )
                .Select(line =>
                {
                    return new
                    {
                        Line = line,
                        Count = line.boxes.Count,
                        Distance = _wrapper.shortestPaths[currentNode][line.startLine].distance
                    };
                })
                .OrderBy(x => x.Count) // Ближе к началу очереди -> меньше перемещений
                .ThenBy(x => x.Distance)
                .Select(x => x.Line)
                .ToList();
        }

        private List<TeleportLine> FindLinesForDropMFCGP(RobotNode currentNode)
        {
            return _wrapper.teleportLinesList
                .Where(line =>
                    line.boxes.Count < 10 &&
                    _wrapper.shortestPaths.ContainsKey(currentNode) &&
                    _wrapper.shortestPaths[currentNode].ContainsKey(line.startLine) // Путь к endLine для забора
                )
                .Select(line =>
                {
                    return new
                    {
                        Line = line,
                        Count = line.boxes.Count,
                        Distance = _wrapper.shortestPaths[currentNode][line.startLine].distance
                    };
                })
                .OrderByDescending(x => x.Count) // Ближе к началу очереди -> больше перемещений
                .ThenBy(x => x.Distance)
                .Select(x => x.Line)
                .ToList();
        }

        public void ManageRobots(TimeSpan timeSpan)
        {
            var movingForFreeSku = _wrapper.GetRobotByTask(TransportRobotTask.TakeBoxFromLine);
            if (movingForFreeSku.Count() > 0)
            {
                foreach(var robot in movingForFreeSku)
                {
                    if (robot.CurrentState != TransportRobotState.Waiting)
                        continue;
                    if (robot.commandList.commands.Count > 0)
                        continue;

                    if (_wrapper.shortestPaths[robot.currentNode][robot.targetNodeForMove].path.Count > 1)
                    {
                        var nextNode = _wrapper.shortestPaths[robot.currentNode][robot.targetNodeForMove].path[1];
                        if (robot.currentNode.nextNodes.Contains(nextNode))
                        {
                            robot.AddCommandMove(nextNode); //we move to the path targetNodeForMove
                        }
                    }
                    else
                    {
                        var consumer = (ConsumerPoint)_wrapper.GetFilteredObjects(obj => obj is ConsumerPoint)[0];
                        if (robot.currentBox == null)
                        {
                            var line = _wrapper.teleportLinesList.Find(x => x.endLine == robot.currentNode && x.boxes.Any(y => y.sku == robot.targetSkuForFree));
                            if (line != null)
                            {
                                robot.AddCommandPickBoxFromChannel(line);
                            }
                            else
                            {
                                var lines = FindLinesWithSku(robot.targetSkuForFree, robot.currentNode);
                                robot.targetNodeForMove = lines[0].endLine;
                                robot.ChangeState(TransportRobotState.Waiting);
                            }
                        }
                        else {
                            if (robot.currentBox.sku != robot.targetSkuForFree)
                            {
                                var lines_drop = FindLinesForDrop(robot.currentNode);
                                if (lines_drop[0].startLine == robot.currentNode)
                                {
                                    robot.AddCommandDropToLine(lines_drop[0]);
                                } 
                                else 
                                {
                                    robot.ChangeTask(TransportRobotTask.MoveBoxToLine);
                                    robot.consumerPoint.FinishReservation(robot);
                                    robot.consumerPoint = null;
                                    robot.targetSkuForFree = -1;
                                    robot.teleportLine = lines_drop[0];
                                }
                            } 
                            else
                            {
                                robot.ChangeTask(TransportRobotTask.MoveBoxToPaletize);
                            }
                        }
                    }
                }
            }



            var waitingRobots = _wrapper.GetWaitingRobot();
            if (waitingRobots.Count() > 0)
            {
                foreach (var waitingRobot in waitingRobots)
                {
                    if (waitingRobot.CurrentTask == TransportRobotTask.TakeBoxFromLine)
                        continue;
                    if (waitingRobot.commandList.commands.Count() == 0)
                    {
                        if (waitingRobot.currentBox == null) { 
                            RobotNode nextNode = null;
                            if (_wrapper.allSourcePoint.Any(x => x.fifoQueue.Count > 0)) //we try depaletize in this route
                            {
                                foreach (var source in _wrapper.allSourcePoint)
                                {

                                    var nodeGet = _wrapper.robotNodes[source.dNode.join_row][source.dNode.join_col];
                                    if (_wrapper.shortestPaths[waitingRobot.currentNode][nodeGet].path.Count > 1)
                                    {
                                        nextNode = _wrapper.shortestPaths[waitingRobot.currentNode][nodeGet].path[1];
                                        if (waitingRobot.currentNode.nextNodes.Contains(nextNode))
                                        {
                                            waitingRobot.AddCommandMove(nextNode); //we move to the path to Depaletize Node
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        waitingRobot.AddCommandGetFromSource(source); //we in Depaletize Node 
                                        break;
                                    }
                                } 
                            } else // we have too start Paletize
                            {
                                var consumer = (ConsumerPoint)_wrapper.GetFilteredObjects(obj => obj is ConsumerPoint)[0];
                                var skuForPeek = consumer.FifoQueue.GetSkuForReserve();
                                if (skuForPeek == -1)
                                {
                                    waitingRobot.AddCommandMove(waitingRobot.currentNode.nextNodes[0]);
                                    continue;
                                }
                                var lines = FindLinesWithSku(skuForPeek, waitingRobot.currentNode);
                                waitingRobot.targetNodeForMove = lines[0].endLine;
                                waitingRobot.targetSkuForFree = skuForPeek;
                                waitingRobot.consumerPoint = consumer;
                                consumer.MakeReservation(skuForPeek, waitingRobot);
                                waitingRobot.ChangeTask(TransportRobotTask.TakeBoxFromLine);
                                waitingRobot.ChangeState(TransportRobotState.Waiting);
                            }
                        }
                        else //robot have box
                        {
                            if (waitingRobot.CurrentTask == TransportRobotTask.MoveBoxToLine) //and we need put this box in any line
                            {
                                if (waitingRobot.teleportLine == null)
                                {
                                    waitingRobot.teleportLine = FindLinesForDropMFCGP(waitingRobot.currentNode)[0];
                                }
                                if (_wrapper.shortestPaths[waitingRobot.currentNode][waitingRobot.teleportLine.startLine].path.Count > 1)
                                {
                                    var nextNode = _wrapper.shortestPaths[waitingRobot.currentNode][waitingRobot.teleportLine.startLine].path[1];
                                    if (waitingRobot.currentNode.nextNodes.Contains(nextNode))
                                    {
                                        waitingRobot.AddCommandMove(nextNode);
                                    }
                                }
                                else
                                {
                                    waitingRobot.AddCommandDropToLine(waitingRobot.teleportLine);
                                }
                            }
                            else if (waitingRobot.CurrentTask == TransportRobotTask.MoveBoxToPaletize)
                            {
                                var consumer = (ConsumerPoint)_wrapper.GetFilteredObjects(obj => obj is ConsumerPoint)[0];
                                var nodeDrop = _wrapper.robotNodes[consumer.pNode.join_row][consumer.pNode.join_col];
                                
                                if (_wrapper.shortestPaths[waitingRobot.currentNode][nodeDrop].path.Count > 1)
                                {
                                    var nextNode = _wrapper.shortestPaths[waitingRobot.currentNode][nodeDrop].path[1];
                                    var position = (waitingRobot.consumerPoint.reservedSku
                                            .Select((item, index) => new { item, index })
                                            .FirstOrDefault(x => x.item.robot == waitingRobot)?.index).Value;
                                    if (position > 6)
                                    {
                                        var row = _wrapper.robotNodes[Math.Max(0, 12 - position)];
                                        var col = rnd.Next(row.Keys.Count);
                                        var AnyWhereNode = row[row.Keys.ElementAt(col)];
                                        if (AnyWhereNode != waitingRobot.currentNode)
                                            nextNode = _wrapper.shortestPaths[waitingRobot.currentNode][AnyWhereNode].path[1];
                                    }
                                    if (waitingRobot.currentNode.nextNodes.Contains(nextNode))
                                    {
                                        waitingRobot.AddCommandMove(nextNode); //we move to the path to Depaletize Node
                                    }
                                }
                                else
                                {
                                    if (waitingRobot.currentBox.sku == consumer.FifoQueue.Peek())
                                    {
                                        if (consumer.reservedSku.Peek().robot == waitingRobot)
                                        {
                                            waitingRobot.AddCommandPlaceBoxIntoPalletAssembly(consumer);
                                        }
                                        else
                                        {
                                            waitingRobot.AddCommandMove(waitingRobot.currentNode.nextNodes[0]); //TODO DROP AND CHANGE queue
                                        }
                                    } else
                                    {
                                        waitingRobot.AddCommandMove(waitingRobot.currentNode.nextNodes[0]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}