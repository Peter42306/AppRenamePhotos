using Microsoft.Graph.Models;
using System;
using System.Drawing;
using System.IO;




namespace AppRenamePhotos
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Укажите путь к папке с фотографиями
            string folderPath = @"D:\MY HOME ВИДЕО\Test for rename";

            // Получить все файлы с расширением .jpg, .jpeg, .png,
            //string[] fileExtensions = { "*.JPG" };
            //string[] fileExtensions = { "*.MOV" };
            string[] fileExtensions = { "*.mp4" };

            foreach (var extension in fileExtensions)
            {
                // Directory.GetFiles возвращает все файлы, соответствующие шаблону поиска в указанной папке
                foreach (var filePath in Directory.GetFiles(folderPath, extension))
                {
                    RenameVideoFilesMP4ForDateTimeFromMetadata(filePath);
                }
            }
        }

        static void RenamePhotosFilesForDateTimeFromMetadata(string filePath)
        {
            try
            {
                using (var image = new Bitmap(filePath))
                {
                    // Получаем коллекцию всех метаданных изображения
                    var properties = image.PropertyItems;

                    // Находим нужный нам тег EXIF для даты и времени съёмки
                    const int ExifDTOriginalTag = 0x9003; // Это тег для DateTimeOriginal

                    foreach (var prop in properties)
                    {
                        if (prop.Id == ExifDTOriginalTag)
                        {
                            // Получаем значение тега как строку
                            string dateTaken = System.Text.Encoding.UTF8.GetString(prop.Value).Trim('\0');

                            // Парсим значение в DateTime
                            DateTime dateTimeTaken = DateTime.ParseExact(dateTaken, "yyyy:MM:dd HH:mm:ss", null);

                            // Форматируем дату и время
                            string dateTimeString = dateTimeTaken.ToString("yyyyMMdd_HHmmss");

                            // Получаем текущее имя, директорию и расширение файла
                            string currentFileName = Path.GetFileName(filePath);
                            string directory = Path.GetDirectoryName(filePath);
                            string extension = Path.GetExtension(filePath);

                            // Сформируем новое имя файла
                            string newFileName = $"IMG_{dateTimeString}_{currentFileName}";

                            // Получаем полный путь к новому файлу
                            string newFilePath = Path.Combine(directory, newFileName);

                            // Закрываем изображение
                            image.Dispose();

                            // Переименуем файл
                            File.Move(filePath, newFilePath);

                            Console.WriteLine($"File renamed from {currentFileName} to {newFileName}");

                            return; // Выходим после первого найденного тега
                        }
                    }

                    // Если не найден тег DateTimeOriginal в метаданных
                    Console.WriteLine($"No DateTimeOriginal tag found in {filePath}");

                    // Закрываем изображение
                    image.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            }
        }       

        static void RenamePhotoFilesForCurrentDate(string filePath)
        {
            // Получить дату и время в формате YYYYMMDD_HHMMSS
            string dateTimeString = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Получить текущее имя, директорию и расширение файла
            string currentFileName = Path.GetFileName(filePath);
            string directory = Path.GetDirectoryName(filePath);
            string extension = Path.GetExtension(filePath);            


            // Сформировать новое имя файла
            string newFileName = $"IMG_{dateTimeString}_{currentFileName}";

            // Получить полный путь к новому файлу
            string newFilePath = Path.Combine(directory, newFileName);

            // Переименовать файл
            File.Move(filePath, newFilePath);

        }

        static void RenameVideoFilesMP4ForDateTimeFromMetadata(string filePath)
        {
            try
            {
                // Открываем файл для чтения метаданных
                using (var fileStream = File.OpenRead(filePath))
                {
                    // Попытаемся прочитать метаданные напрямую из файла
                    string creationTime = GetVideoCreationTimeMP4(filePath);

                    if (!string.IsNullOrEmpty(creationTime))
                    {
                        // Попробуем распарсить полученную строку
                        if (DateTime.TryParseExact(creationTime, "yyyyMMdd_HHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime dateTimeTaken))
                        {
                            // Форматируем дату и время
                            string dateTimeString = dateTimeTaken.ToString("yyyyMMdd_HHmmss");

                            // Получаем текущее имя, директорию и расширение файла
                            string currentFileName = Path.GetFileName(filePath);
                            string directory = Path.GetDirectoryName(filePath);
                            string extension = Path.GetExtension(filePath);

                            // Сформируем новое имя файла
                            string newFileName = $"VIDEO_{dateTimeString}_{currentFileName}";

                            // Получаем полный путь к новому файлу
                            string newFilePath = Path.Combine(directory, newFileName);

                            // Переименуем файл
                            File.Move(filePath, newFilePath);

                            Console.WriteLine($"File renamed from {currentFileName} to {newFileName}");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse creation time '{creationTime}' for file {filePath}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No creation time found in metadata for {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            }
        }

        static string GetVideoCreationTimeMP4(string filePath)
        {
            // Пример для MP4 файлов
            if (Path.GetExtension(filePath).Equals(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fs.Seek(0x18, SeekOrigin.Begin);
                        var buffer = new byte[4];
                        fs.Read(buffer, 0, 4);
                        int secondsSince1904 = (buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3];
                        DateTime date = new DateTime(1904, 1, 1).AddSeconds(secondsSince1904);
                        return date.ToString("yyyyMMdd_HHmmss");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error extracting creation time from MP4 file {filePath}: {ex.Message}");
                }
            }

            // Для других форматов видео файлов реализация будет различаться
            // В этом примере реализован только для MP4 файлов
            // Для других форматов может потребоваться другой способ извлечения метаданных
            // или использование специализированных библиотек, если доступны

            return null;
        }
    }
}
