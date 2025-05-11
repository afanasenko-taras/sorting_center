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

        public void ManageRobots(TimeSpan timeSpan)
        {
            var movingForFreeSku = _wrapper.GetRobotByTask(TransportRobotTask.MovedForFreeLine);
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
                            robot.AddCommandMove(nextNode); //we move to the path to Depaletize Node
                        }
                    }
                    else
                    {
                        var consumer = (ConsumerPoint)_wrapper.GetFilteredObjects(obj => obj is ConsumerPoint)[0];
                        var skuForPeek = consumer.FifoQueue.Peek();
                        if (robot.currentBox == null)
                        {
                            if (skuForPeek == robot.targetSkuForFree) //for time moving nothing is changing
                            {
                                var line = _wrapper.teleportLinesList.Find(x => x.endLine == robot.currentNode && x.boxes.Any(y => y.sku == skuForPeek));
                                if (line != null)
                                {
                                    robot.AddCommandPickBoxFromChannel(line);
                                } else
                                {
                                    robot.ChangeTask(TransportRobotTask.NoTask);
                                    robot.ChangeState(TransportRobotState.Waiting);
                                }
                            } 
                            else
                            {
                                robot.ChangeTask(TransportRobotTask.NoTask);
                                robot.ChangeState(TransportRobotState.Waiting);
                            }
                        }
                        else {
                            if (robot.currentBox.sku != skuForPeek)
                            {
                                var lines_drop = FindLinesForDrop(robot.currentNode);
                                if (lines_drop[0].startLine == robot.currentNode)
                                {
                                    robot.AddCommandDropToLine(lines_drop[0]);
                                } 
                                else 
                                {
                                    robot.ChangeTask(TransportRobotTask.MoveBoxToLine);
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
                    if (waitingRobot.CurrentTask == TransportRobotTask.MovedForFreeLine)
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
                                var skuForPeek = consumer.FifoQueue.Peek();
                                var line = _wrapper.teleportLinesList.Find(x=>x.boxes.Count()>0 && x.boxes.Peek().sku == skuForPeek);
                                if (line != null)
                                {
                                    if (_wrapper.shortestPaths[waitingRobot.currentNode][line.endLine].path.Count > 1)
                                    {
                                        nextNode = _wrapper.shortestPaths[waitingRobot.currentNode][line.endLine].path[1];
                                        if (waitingRobot.currentNode.nextNodes.Contains(nextNode))
                                        {
                                            waitingRobot.AddCommandMove(nextNode); //we move to the path to Depaletize Node
                                        }
                                    }
                                    else
                                    {                                      
                                        waitingRobot.AddCommandPickBoxFromChannel(line);
                                    }
                                } else
                                {
                                    var lines = FindLinesWithSku(skuForPeek, waitingRobot.currentNode);
                                    waitingRobot.targetNodeForMove = lines[0].endLine;
                                    waitingRobot.targetSkuForFree = skuForPeek;
                                    waitingRobot.ChangeTask(TransportRobotTask.MovedForFreeLine);
                                    waitingRobot.ChangeState(TransportRobotState.Waiting);
                                    //Console.WriteLine($"Dig up {waitingRobot.uid} Send to Line {waitingRobot.targetNodeForMove.Id} sku {waitingRobot.targetSkuForFree}");
                                }
                            }
                        }
                        else //robot have box
                        {
                            if (waitingRobot.CurrentTask == TransportRobotTask.MoveBoxToLine) //and we need put this box in any line
                            {
                                while (waitingRobot.teleportLine == null)
                                {
                                    var teleportLine = _wrapper.teleportLinesList[rnd.Next(_wrapper.teleportLinesList.Count())];
                                    if (teleportLine.boxes.Count < _wrapper.sortConfig.subRowNumber)
                                    {
                                        waitingRobot.teleportLine = teleportLine;
                                    }
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
                                    if (waitingRobot.currentNode.nextNodes.Contains(nextNode))
                                    {
                                        waitingRobot.AddCommandMove(nextNode); //we move to the path to Depaletize Node
                                    }
                                }
                                else
                                {
                                    if (waitingRobot.currentBox.sku == consumer.FifoQueue.Peek())
                                    {
                                        waitingRobot.AddCommandPlaceBoxIntoPalletAssembly(consumer);
                                    } else
                                    {
                                        waitingRobot.ChangeTask(TransportRobotTask.MoveBoxToLine);
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