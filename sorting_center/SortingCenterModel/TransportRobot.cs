using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{

    public enum TransportRobotState
    {
        Waiting,           // Ждет
        Moving,            // Движется
        PickingUpFromDisassemblyChannel,         // Берет
        PlaceBoxIntoChannel,
        PickingUpFromChannel,// Выгружает
        PlaceBoxIntoPalletAssembly
    }

    public enum TransportRobotTask
    {
        Depaltize,           
        MoveBoxToLine,
        MoveBoxToPaletize,
        NoTask,
        MovedForFreeLine
    }


    public class TransportRobot : FastAbstractObject
    {
        public RobotNode currentNode;
        private SortCenterWrapper wrapper;
        public TransportRobotState CurrentState { get; private set; } = TransportRobotState.Waiting;
        public TransportRobotTask CurrentTask { get; private set; } = TransportRobotTask.Depaltize;
        public RobotNode movedTo { get; private set; }
        public Box currentBox = null; // Текущая коробка, которую робот перевозит
        public SourcePoint getSourcePoint = null;
        public TeleportLine teleportLine = null;
        public double speed = 1;

        public RobotNode targetNodeForMove = null;
        public int targetSkuForFree = -1;

        public CommandList commandList;


        public TimeSpan _endAt { get; private set; } = TimeSpan.Zero;

        private ConsumerPoint consumer;

        public TimeSpan _startAt { get; private set; }

        // Метод для изменения состояния
        public void ChangeState(TransportRobotState newState)
        {
            CurrentState = newState;
        }

        public void ChangeTask(TransportRobotTask newTask)
        {
            CurrentTask = newTask;
        }

        public TransportRobot(RobotNode node, SortCenterWrapper wrapper, TimeSpan createTime)
        {
            this.uid = wrapper.GetNewUid();
            this.currentNode = node;
            this.movedTo = node;
            this.wrapper = wrapper;
            node.Reserved = (true, uid);
            commandList = new CommandList(this);
            _endAt = createTime + TimeSpan.FromTicks(1);
            lastUpdated = createTime;
        }




        public override (TimeSpan, FastAbstractEvent) getNearestEvent()
        {
            if (CurrentState == TransportRobotState.Waiting)
            {
                // Если робот занят, то он ждет
                if (commandList.commands.Count > 0)
                {
                    var task = commandList.commands.First();
                    if (lastUpdated > task.Key)
                        return (lastUpdated, task.Ev);
                    else
                        return task;
                }
                if (_endAt<wrapper.updatedTime) 
                    _endAt = wrapper.updatedTime;
                return (_endAt, new TransportRobotEndTask(this));
            }
            else if (CurrentState == TransportRobotState.Moving)
            {
                return (_endAt, new TransportRobotEndMove(this));
            }
            else if (CurrentState == TransportRobotState.PickingUpFromDisassemblyChannel)
            {
                return (_endAt, new EndPickBoxFromDisassemblyChannel(this));
            }
            else if (CurrentState == TransportRobotState.PickingUpFromChannel)
            {
                return (_endAt, new EndPickingUpFromChannel(this));
            }
            else if (CurrentState == TransportRobotState.PlaceBoxIntoChannel)
            {
                return (_endAt, new EndPlaceBoxIntoChannel(this));
            }
            else if (CurrentState == TransportRobotState.PlaceBoxIntoPalletAssembly)
            {
                return (_endAt, new EndPlaceBoxIntoPalletAssembly(this));
            }
            return (TimeSpan.MaxValue, null);
        }

        public override void Update(TimeSpan timeSpan)
        {
            lastUpdated = timeSpan;
        }

        internal void AddCommandMove(RobotNode nextNode)
        {
            if (!nextNode.Reserved.isReserved | nextNode.Reserved.uid == uid)
            {
                commandList.AddCommand(new TransportRobotStartMoveTo(this, nextNode));
                wrapper.SetReservation(nextNode, uid, true);
            }
            else
            {
                _endAt = wrapper.updatedTime + TimeSpan.FromSeconds(1);
                var log = new EventLog(wrapper.updatedTime, _endAt, uid, "Wait", currentNode.Id, movedTo.Id, 0, "", "");
                wrapper.logs.Add(log);
            }
        }

        internal void AddCommandGetFromSource(SourcePoint source)
        {
            commandList.AddCommand(new StartPickBoxFromDisassemblyChannel(this, source));
        }


        internal void AddCommandDropToLine(TeleportLine line)
        {
            commandList.AddCommand(new StartPlaceBoxIntoChannel(this, line));
        }

        internal void AddCommandPickBoxFromChannel(TeleportLine line)
        {
            commandList.AddCommand(new StartPickBoxFromChannel(this, line));
        }

        internal void AddCommandPlaceBoxIntoPalletAssembly(ConsumerPoint consumer)
        {
            commandList.AddCommand(new StartPlaceBoxIntoPalletAssembly(this, consumer));
        }

        public void StartMoving(TimeSpan timeSpan, RobotNode nextNode)
        {
            commandList.commands = new List<(TimeSpan Key, FastAbstractEvent Ev)>();
            CurrentState = TransportRobotState.Moving;
            movedTo = nextNode;
            _startAt = timeSpan;
            _endAt = timeSpan + TimeSpan.FromSeconds(CalculateDistance(currentNode, nextNode) / wrapper.sortConfig.robotSpeedMs);
            var log = new EventLog(_startAt, _endAt, uid, "startMotion", currentNode.Id, movedTo.Id, 0, "", "");
            wrapper.logs.Add(log);
        }


        public void StartPickBoxFromDisassemblyChannel(TimeSpan timeSpan, SourcePoint sourcePoint)
        {
            commandList.commands = new List<(TimeSpan Key, FastAbstractEvent Ev)>();
            CurrentState = TransportRobotState.PickingUpFromDisassemblyChannel;
            _startAt = timeSpan;
            _endAt = timeSpan + TimeSpan.FromSeconds(wrapper.sortConfig.pickBoxFromDisassemblyChannelTime);
            getSourcePoint = sourcePoint;
            var log = new EventLog(_startAt, _endAt, uid, "start_movebox2bot", sourcePoint.dNode.Id, currentNode.Id, sourcePoint.currentSku, "", sourcePoint.dNode.Id);
            wrapper.logs_2.Add(log);
        }

        internal void StartPickBoxFromChannel(TimeSpan timeSpan, TeleportLine teleportLine)
        {
            commandList.commands = new List<(TimeSpan Key, FastAbstractEvent Ev)>();
            CurrentState = TransportRobotState.PickingUpFromChannel;
            _startAt = timeSpan;
            _endAt = timeSpan + TimeSpan.FromSeconds(wrapper.sortConfig.pickBoxFromChannelTime);
            this.teleportLine = teleportLine;
            var log = new EventLog(_startAt, _endAt, uid, "start_movebox2bot", teleportLine.endLine.Id, currentNode.Id, teleportLine.boxes.Peek().sku, "", "");
            wrapper.logs_2.Add(log);
        }

        internal void StartPlaceBoxIntoChannel(TimeSpan timeSpan, TeleportLine teleportLine)
        {
            commandList.commands = new List<(TimeSpan Key, FastAbstractEvent Ev)>();
            CurrentState = TransportRobotState.PlaceBoxIntoChannel;
            _startAt = timeSpan;
            _endAt = timeSpan + TimeSpan.FromSeconds(wrapper.sortConfig.placeBoxIntoChannelTime);
            this.teleportLine = teleportLine;
            var log = new EventLog(_startAt, _endAt, uid, "start_movebox2channel", currentNode.Id, teleportLine.startLine.Id, currentBox.sku, "", "");
            wrapper.logs_2.Add(log);
        }


        internal void StartPlaceBoxIntoPalletAssembly(TimeSpan timeSpan, ConsumerPoint consumer)
        {
            commandList.commands = new List<(TimeSpan Key, FastAbstractEvent Ev)>();
            CurrentState = TransportRobotState.PlaceBoxIntoPalletAssembly;
            _startAt = timeSpan;
            _endAt = timeSpan + TimeSpan.FromSeconds(wrapper.sortConfig.placeBoxIntoPalletAssemblyTime);
            this.consumer = consumer;
            var log = new EventLog(_startAt, _endAt, uid, "start_movebox2tr", currentNode.Id, consumer.pNode.Id, currentBox.sku, "", consumer.pNode.Id);
            wrapper.logs_2.Add(log);
        }

        internal void EndPlaceBoxIntoChannel(TimeSpan timeSpan)
        {
            _startAt = timeSpan;
            _endAt = timeSpan;
            teleportLine.boxes.Enqueue(currentBox);
            CurrentState = TransportRobotState.Waiting;
            CurrentTask = TransportRobotTask.Depaltize;
            var log = new EventLog(_startAt, _endAt, uid, "end_movebox2channel", currentNode.Id, teleportLine.startLine.Id, currentBox.sku, "", "");
            wrapper.logs_2.Add(log);
            teleportLine = null;
            currentBox = null;
        }


        public void EndMoving(TimeSpan timeSpan)
        {
            wrapper.WriteDebug($"Robot {uid} is coming from {currentNode.Id} to {movedTo.Id}");
            if (!wrapper.reverceReservation[currentNode].robotNodes.Contains(movedTo))
                wrapper.SetReservation(currentNode, "", false);
            currentNode = movedTo;
            _startAt = timeSpan;
            _endAt = timeSpan;
            CurrentState = TransportRobotState.Waiting;
            var log = new EventLog(_startAt, _endAt, uid, "finishMotion", currentNode.Id, movedTo.Id, 0, "", "");
            wrapper.logs.Add(log);
            wrapper.WriteDebug($"Robot {uid} is coming to {currentNode.x} {currentNode.y} ");
        }

        internal void EndPickBoxFromDisassemblyChannel(TimeSpan timeSpan)
        {
            _startAt = timeSpan;
            _endAt = timeSpan;
            Box box = new Box();
            box.sku = getSourcePoint.fifoQueue.Dequeue();
            currentBox = box;
            wrapper.skuCount[box.sku]++;
            CurrentState = TransportRobotState.Waiting;
            CurrentTask = TransportRobotTask.MoveBoxToLine;
            var log = new EventLog(_startAt, _endAt, uid, "end_movebox2bot", getSourcePoint.dNode.Id, currentNode.Id, getSourcePoint.currentSku, "", getSourcePoint.dNode.Id);
            wrapper.logs_2.Add(log);
            if (getSourcePoint.fifoQueue.Count == 0)
            {
                getSourcePoint.finishDepaletize(timeSpan);
            }
            getSourcePoint = null;
        }

        internal void EndPickingUpFromChannel(TimeSpan timeSpan)
        {
            _startAt = timeSpan;
            _endAt = timeSpan;
            currentBox = teleportLine.boxes.Dequeue();
            CurrentState = TransportRobotState.Waiting;
            if (CurrentTask != TransportRobotTask.MovedForFreeLine)
                CurrentTask = TransportRobotTask.MoveBoxToPaletize;
            var log = new EventLog(_startAt, _endAt, uid, "end_movebox2bot", teleportLine.endLine.Id, currentNode.Id, currentBox.sku, "", "");
            wrapper.logs_2.Add(log);
            teleportLine = null;
        }

        internal void EndPlaceBoxIntoPalletAssembly(TimeSpan timeSpan)
        {
            _startAt = timeSpan;
            _endAt = timeSpan;
            int sku = consumer.FifoQueue.Dequeue();
            wrapper.skuCount[sku]--;
            Console.WriteLine($"Put {sku} to {currentBox.sku} consumer.lenght {consumer.FifoQueue.Count} {wrapper.skuCount[sku]}");
            CurrentState = TransportRobotState.Waiting;
            CurrentTask = TransportRobotTask.NoTask;
            var log = new EventLog(_startAt, _endAt, uid, "end_movebox2tr", currentNode.Id, consumer.pNode.Id, currentBox.sku, "", consumer.pNode.Id);
            wrapper.logs_2.Add(log);
            if (consumer.FifoQueue.Count == 0)
            {
                log = new EventLog(_startAt, _endAt, consumer.uid, "finishPalletize", consumer.pNode.Id, consumer.pNode.Id, -1, "", consumer.pNode.Id);
                wrapper.logs_2.Add(log);
            }

            consumer = null;
            currentBox = null;
        }






        private static double CalculateDistance(RobotNode from, RobotNode to)
        {
            // Пример: Евклидово расстояние
            return Math.Sqrt(Math.Pow(from.x - to.x, 2) + Math.Pow(from.y - to.y, 2));
        }


    }
}
