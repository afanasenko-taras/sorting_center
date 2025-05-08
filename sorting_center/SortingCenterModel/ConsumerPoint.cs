using AbstractModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingCenterModel
{
    class ConsumerPoint : FastAbstractObject
    {
        private PalettizeNode pNode;
        private Queue<(int sku, TransportRobot? robot)> fifoQueue = new Queue<(int, TransportRobot?)>(); // Очередь FIFO
        private Random random = new Random(); // Генератор случайных чисел

        public ConsumerPoint(PalettizeNode pNode)
        {
            this.pNode = pNode;
            FillQueue(1000);
        }

        // Метод для заполнения очереди случайными значениями
        public void FillQueue(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int randomValue = random.Next(0, 35); // Случайное значение от 0 до 34
                fifoQueue.Enqueue((randomValue, null));
            }
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
