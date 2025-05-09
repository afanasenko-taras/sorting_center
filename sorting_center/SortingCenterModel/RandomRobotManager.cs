using SortingCenterModel;
using System.ComponentModel.DataAnnotations;

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

        public void ManageRobots(TimeSpan timeSpan)
        {
            var waitingRobots = _wrapper.GetWaitingRobot();
            if (waitingRobots.Count() > 0)
            {
                foreach (var waitingRobot in waitingRobots)
                {
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
                                var skuForPeek = consumer.fifoQueue.Peek().sku;
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
                                    Console.WriteLine("need dig here");
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
                                var nodeDrop = _wrapper.robotNodes[consumer.pNode.join_col][consumer.pNode.join_row];
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
                                    waitingRobot.AddCommandPlaceBoxIntoPalletAssembly(consumer);
                                }
                            }
                        }
                    }

                    if (waitingRobot.commandList.commands.Count() == 0)
                    {
                        waitingRobot.AddCommandMove(waitingRobot.currentNode.nextNodes[0]);
                    }
                }
            }
        }
    }
}