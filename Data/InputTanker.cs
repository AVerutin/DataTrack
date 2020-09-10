using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
namespace DataTrack.Data
{
    public class InputTanker
    {
        /// <summary>
        /// Уникальный идентификатор загрузочного бункера
        /// </summary>
        public int InputTankerId { get; private set; }

        /// <summary>
        /// Текущее состояние загрузочного бункера
        /// </summary>
        public Statuses.Status Status { get; private set; }
        public bool Selected { get; set; }

        /// <summary>
        /// Наименование загруженного в бункер материала
        /// </summary>
        public string Material { get; private set; }

        // Список материала, загруженного в загрузочный бункер
        private List<Material> Materials;
        
        private readonly Logger logger;
        private int LayersCount;
        private int TimeLoading;

        public InputTanker(int number)
        {
            if (number > 0)
            {
                logger = LogManager.GetCurrentClassLogger();
                InputTankerId = number;
                Status = Statuses.Status.Off;
                Materials = new List<Material>();
                LayersCount = 0;
                Material = "";
                TimeLoading = 0;
            }
            else
            {
                Status = Statuses.Status.Error;
                logger.Error($"Неверный ID загузочного бункера: [{number}]");
                throw new ArgumentException($"Неверный ID загузочного бункера: [{number}]");
            }
        }

        /// <summary>
        /// Установить статус загрузочного бункера
        /// </summary>
        /// <param name="status">Статус загрузочного бункера</param>
        public void SetStatus(Statuses.Status status)
        {
            Status = status;
        }
        
        /// <summary>
        /// Получить текущее состояние загрузочного бункера
        /// </summary>
        /// <returns>Текущее состояние загрузочного бункера</returns>
        public Statuses.Status GetStatus()
        {
            return Status;
        }

        /// <summary>
        /// Получить наименование материала, установленного для загрузочного бункера
        /// </summary>
        /// <returns>Наименование материала. установленного для загрузочного бункера</returns>
        public string GetMaterialName()
        {
            return Material;
        }

        /// <summary>
        /// Загрузить материал в загрузочный бункер
        /// </summary>
        /// <param name="material">Загружаемый материал</param>
        public void Load(Material material)
        {
            Status = Statuses.Status.Loading;

            if (material != null)
            {
                Material = material.Name;
                if (LayersCount > 0 && material.Name != Material)
                {
                    Status = Statuses.Status.Error;
                    // Если наименование загружаемого материала не соответствует наименованию ранее заруженного материала
                    logger.Error($"Попытка загрузить в загрузочный бункер {InputTankerId}, содержащий материал" +
                        $"{Materials[LayersCount - 1].Name}, новый  материал {material.Name}");
                    throw new InvalidCastException($"Невозможно загрузить в загрузочный бункер {InputTankerId}, " + 
                        $"содержащий материал {Material}, новый материал {material.Name}");
                }

                try
                {
                    // Загрузка бункера производится определенное время
                    logger.Info($"Начинается ожидание [{TimeLoading} сек] загрузки загрузочного бункера [{InputTankerId}]");
                    Debug.WriteLine($"Начинается ожидание [{TimeLoading} сек] загрузки загрузочного бункера [{InputTankerId}]");
                    var t = Task.Run(async delegate
                    {
                        await Task.Delay(TimeLoading * 1000);
                        Materials.Add(material);
                        LayersCount = Materials.Count;
                        Material = Materials[LayersCount - 1].Name;
                    });
                    t.Wait();
                    logger.Info($"Закончилось ожидание [{TimeLoading} сек] загрузки загрузочного бункера [{InputTankerId}]");
                    Debug.WriteLine($"Закончилось ожидание [{TimeLoading} сек] загрузки загрузочного бункера [{InputTankerId}]");
                }
                catch (Exception e)
                {
                    Status = Statuses.Status.Error;
                    logger.Error($"Ошибка при добавлении материала в загрузочного бункер [{InputTankerId}] : {e.Message}");
                    throw new NotSupportedException();
                }
            }

            Status = Statuses.Status.Off;
        }

        public void SetTimeLoading(int time)
        {
            if (time > 0)
            {
                TimeLoading = time;
            }
        }

        /// <summary>
        /// Выгрузить весь материал из загрузочного бункера
        /// </summary>
        /// <returns>Список выгруженного материала из загрузочного бункера</returns>
        public List<Material> Unload()
        {
            Status = Statuses.Status.Unloading;

            // Получаем количество слоев материала. Если материала нет, выдаем ошибку
            if (LayersCount == 0)
            {
                Status = Statuses.Status.Error;
                logger.Error($"Загрузочный бункер {InputTankerId} не содержит материал");
                throw new ArgumentNullException($"Загрузочный бункер {InputTankerId} не содержит материал");
            }

            List<Material> result = Materials;
            
            var t = Task.Run(async delegate
            {
                await Task.Delay(TimeLoading * 1000);
            });
            t.Wait();
            
            Reset();
            return result;
        }

        /// <summary>
        /// Получить количество слоев материала, загруженного в бункер
        /// </summary>
        /// <returns>Количество слоев материала, загруженного в бункер</returns>
        public int GetLayersCount()
        {
            return Materials.Count;
        }

        /// <summary>
        /// Получить список слоев материала, загруженного в бункер
        /// </summary>
        /// <returns>Список слоев материала, загруженного в бункер</returns>
        public List<Material> GetMaterials()
        {
            return Materials;
        }

        /// <summary>
        /// Сброс загрузочного бункера в исходное состояние
        /// </summary>
        public void Reset()
        {
            Status = Statuses.Status.Off;
            LayersCount = 0;
            Materials = new List<Material>();
            Material = "";
        }
    }
}