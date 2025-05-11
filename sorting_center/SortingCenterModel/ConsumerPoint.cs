using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SortingCenterModel
{
    public class FifoQueueWrapper
    {
        private FileQueue fileQueue;
        private Queue<int> currentDataQueue = new Queue<int>(); // Текущая очередь данных из файла
        private SortCenterWrapper wrapper;
        private ConsumerPoint consumerPoint;
        FileData fileData = null;

        public FifoQueueWrapper(FileQueue fileQueue, SortCenterWrapper wrapper, ConsumerPoint consumerPoint)
        {
            this.fileQueue = fileQueue;
            this.wrapper = wrapper;
            this.consumerPoint = consumerPoint;
            LoadNextFileIfNeeded();
        }

        // Метод для получения первого элемента
        public int Peek()
        {
            LoadNextFileIfNeeded();
            if (currentDataQueue.Count > 0)
            {
                return currentDataQueue.Peek();
            }
            throw new InvalidOperationException("Очередь пуста.");
        }


        public int GetSkuForReserve()
        {
            var failedReserv = consumerPoint.reservedSku.FirstOrDefault(x => x.robot == null);
            if (failedReserv != null)
            {
                return failedReserv.sku;
            }

            return Peek(consumerPoint.reservedSku.Count);
        }

        public int Peek(int i)
        {
            if (i < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(i), "Индекс не может быть отрицательным.");
            }

            int remainingIndex = i; // Индекс для поиска в текущей очереди
            if (remainingIndex >= currentDataQueue.Count)
            {
                remainingIndex -= currentDataQueue.Count;
                FileData nextFileData = fileQueue.PeekFile();
                if (nextFileData == null)
                {
                    return -1;
                }
                return nextFileData.DataQueue.ElementAt(remainingIndex);
            }
            return currentDataQueue.ElementAt(remainingIndex);
        }



        // Метод для получения количества элементов
        public int Count
        {
            get
            {
                if (currentDataQueue.Count == 0)
                {
                    LoadNextFileIfNeeded();
                }
                return currentDataQueue.Count;
            }
        }

        // Метод для извлечения элемента из очереди
        public int Dequeue()
        {
            LoadNextFileIfNeeded();
            //Console.WriteLine(currentDataQueue.Count);
            if (currentDataQueue.Count > 0)
            {
                return currentDataQueue.Dequeue();
            }
            throw new InvalidOperationException("Очередь пуста.");
        }

        // Свойство для проверки, закончились ли все очереди
        public bool IsEmpty
        {
            get
            {
                LoadNextFileIfNeeded(); 
                return currentDataQueue.Count == 0 && !fileQueue.HasFiles();
            }
        }

        // Загрузка данных из следующего файла, если текущая очередь пуста
        private void LoadNextFileIfNeeded()
        {
            while (currentDataQueue.Count == 0 && fileQueue.HasFiles())
            {
                EventLog log;
                if (fileData != null)
                {
                    log = new EventLog(
                        wrapper.updatedTime,
                        wrapper.updatedTime,
                        consumerPoint.uid,
                        "finishPalletize",
                        $"Unloaded file {fileData.FileNumber}",
                        consumerPoint.pNode.Id,
                        fileData.DataQueue.Count,
                        "",
                        consumerPoint.uid
                    );
                    wrapper.logs_2.Add(log);
                }


                fileData = fileQueue.DequeueFile();
                Console.WriteLine($"Загружается файл: {fileData.FileNumber}");

                log = new EventLog(
                    wrapper.updatedTime,
                    wrapper.updatedTime,
                    consumerPoint.uid,
                    "startPalletize",
                    $"Loaded file {fileData.FileNumber}",
                    consumerPoint.pNode.Id,
                    fileData.DataQueue.Count,
                    "",
                    consumerPoint.uid
                );
                wrapper.logs_2.Add(log);


                while (fileData.DataQueue.Count > 0)
                {
                    int value = fileData.DataQueue.Dequeue();
                    currentDataQueue.Enqueue(value);
                }
            }
        }
    }


    public class ReservSku
    {
        public int sku;
        public TransportRobot? robot;
        public ReservSku(int sku, TransportRobot? robot)
        {
            this.sku = sku;
            this.robot = robot;
        }
    }

    public class ConsumerPoint : FastAbstractObject
    {
        public PalettizeNode pNode;
        private Queue<(int sku, TransportRobot? robot)> fifoQueue = new Queue<(int, TransportRobot?)>(); // Очередь FIFO
        private Random random = new Random(); // Генератор случайных чисел
        private SortCenterWrapper wrapper;
        private FileQueue fileQueue;
        public FifoQueueWrapper FifoQueue;
        
        public Queue<ReservSku> reservedSku = new Queue<ReservSku>(); // Очередь для резервирования SKU 
        
        internal void TaskIsImposibleForRobot(TransportRobot robot)
        {
            var reservedItem = reservedSku.First(x => x.robot == robot);
            reservedItem.robot = null;
        }

        internal void MakeReservation(int skuForPeek, TransportRobot waitingRobot)
        {
            if (waitingRobot.targetSkuForFree!=skuForPeek)
            {
                throw new Exception("waitingRobot.targetSkuForFree!=skuForPeek");
            }
            if (reservedSku.Any(x=>x.robot == waitingRobot))
            {
                throw new Exception("Add wrong robot!!!");
            }

            var failedReserv = reservedSku.FirstOrDefault(x => x.robot == null & x.sku == skuForPeek);
            if (failedReserv != null)
            {
                failedReserv.robot = waitingRobot;
                return;
            }
            else
            {
                reservedSku.Enqueue(new ReservSku(skuForPeek, waitingRobot));
            }
        }
        internal void FinishReservation(TransportRobot robot)
        {
            var reservedItem = reservedSku.First(x => x.robot == robot);
            reservedItem.robot = null;
        }

        public ConsumerPoint(PalettizeNode pNode, SortCenterWrapper wrapper, FileQueue fileQueue)
        {
            this.pNode = pNode;
            this.wrapper = wrapper;
            this.fileQueue = fileQueue;
            FifoQueue = new FifoQueueWrapper(fileQueue, wrapper, this);
        }



        // Метод для заполнения очереди случайными значениями
        public void FillQueue(int count)
        {
            //Dictionary<int, int> countSku = new Dictionary<int, int>();
            for (int i = 0; i < count; i++)
            {
                int randomValue = random.Next(0, wrapper.sortConfig.skuSize); // Случайное значение от 0 до 34
                fifoQueue.Enqueue((randomValue, null));
            }
            var log = new EventLog(wrapper.updatedTime, wrapper.updatedTime, uid, "startPalletize", pNode.Id, pNode.Id, -1, count.ToString(), uid);
            wrapper.logs_2.Add(log);
        }

        // Метод для извлечения элемента из очереди
        public (int, TransportRobot?)? Dequeue()
        {
            if (fifoQueue.Count > 0)
            {
                return fifoQueue.Dequeue();
            }
            return null; // Если очередь пуста, возвращаем null
        }

        // Метод для получения текущего состояния очереди
        public IEnumerable<(int, TransportRobot?)> GetQueueState()
        {
            return fifoQueue;
        }

        public override (TimeSpan, FastAbstractEvent) getNearestEvent()
        {
            return (TimeSpan.MaxValue, null);
        }

        public override void Update(TimeSpan timeSpan)
        {
            // Логика обновления, если требуется
        }


    }
}
