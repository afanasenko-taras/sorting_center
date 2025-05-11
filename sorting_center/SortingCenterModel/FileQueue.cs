using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SortingCenterModel
{
    public class FileQueue
    {
        private Queue<FileData> fileQueue = new Queue<FileData>();
        private HashSet<int> uniqueNumbers = new HashSet<int>(); // Для хранения уникальных номеров

        public FileData PeekFile()
        {
            if (fileQueue.Count > 0)
            {
                return fileQueue.Peek(); // Метод Peek() доступен в стандартной очереди
            }
            return null;
        }

        // Загрузка файлов из папки
        public void LoadFiles(string folderPath)
        {
            var files = Directory.GetFiles(folderPath, "*.csv")
                .OrderBy(file => int.Parse(Path.GetFileNameWithoutExtension(file))) // Сортируем по номеру файла
                .ToList();

            foreach (var file in files)
            {
                int fileNumber = int.Parse(Path.GetFileNameWithoutExtension(file));
                var fileData = new FileData(fileNumber);

                // Читаем данные из файла
                using (var reader = new StreamReader(file))
                {
                    string header = reader.ReadLine(); // Пропускаем заголовок
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var columns = line.Split(',');
                        if (int.TryParse(columns[0], out int value))
                        {
                            fileData.DataQueue.Enqueue(value); // Добавляем данные в очередь
                            uniqueNumbers.Add(value); // Добавляем уникальное значение
                        }
                    }
                }

                fileQueue.Enqueue(fileData); // Добавляем файл в очередь
            }
        }

        // Получение следующего файла из очереди
        public FileData DequeueFile()
        {
            return fileQueue.Count > 0 ? fileQueue.Dequeue() : null;
        }

        // Проверка, есть ли файлы в очереди
        public bool HasFiles()
        {
            return fileQueue.Count > 0;
        }

        // Получение списка всех уникальных номеров
        public List<int> GetUniqueNumbers()
        {
            return uniqueNumbers.ToList();
        }

        // Получение количества уникальных номеров
        public int GetUniqueCount()
        {
            return uniqueNumbers.Count;
        }
    }

    // Класс для хранения данных из файла
    public class FileData
    {
        public int FileNumber { get; set; } // Номер файла
        public Queue<int> DataQueue { get; set; } = new Queue<int>(); // FIFO-очередь данных

        public FileData(int fileNumber)
        {
            FileNumber = fileNumber;
        }
    }
}