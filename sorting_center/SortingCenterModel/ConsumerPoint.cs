using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public FifoQueueWrapper(FileQueue fileQueue, SortCenterWrapper wrapper)
        {
            this.fileQueue = fileQueue;
            this.wrapper = wrapper;
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
            Console.WriteLine(currentDataQueue.Count);
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
                var fileData = fileQueue.DequeueFile();
                Console.WriteLine($"Загружается файл: {fileData.FileNumber}");

                var log = new EventLog(
                    wrapper.updatedTime,
                    wrapper.updatedTime,
                    "FileQueue",
                    $"Loaded file {fileData.FileNumber}",
                    "FileQueue",
                    "FileQueue",
                    -1,
                    fileData.FileNumber.ToString(),
                    "FileQueue"
                );
                wrapper.logs.Add(log);

                while (fileData.DataQueue.Count > 0)
                {
                    int value = fileData.DataQueue.Dequeue();
                    currentDataQueue.Enqueue(value);
                }
            }
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

        public ConsumerPoint(PalettizeNode pNode, SortCenterWrapper wrapper, FileQueue fileQueue)
        {
            this.pNode = pNode;
            this.wrapper = wrapper;
            this.fileQueue = fileQueue;
            FifoQueue = new FifoQueueWrapper(fileQueue, wrapper);
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
