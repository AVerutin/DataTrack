using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Наименование загруженного в бункер материала
        /// </summary>
        public string Material { get; private set; }

        /// <summary>
        /// Вес загруженного в бункер материала
        /// </summary>
        public double Weight { get; private set; }

        private List<Material> Materials;
        private readonly Logger logger;
        private int LayersCount;

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
                Weight = 0;
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
        /// Выбор материала загрузочного бункера
        /// </summary>
        /// <param name="material"></param>
        public void SetMaterial(string material)
        {
            if (material != "")
            {
                Material = material;
            }
            else
            {
                Status = Statuses.Status.Error;
                logger.Error($"Установка пустого материала для загрузочного бункера {InputTankerId}");
                throw new ArgumentNullException("Нельзя установить пустой материал для загрузочного бункера!");
            }
        }

        /// <summary>
        /// Получить наименование материала, установленного для загрузочного бункера
        /// </summary>
        /// <returns>Наименование материала. укстановленного для загрузочного бункера</returns>
        public string GetMaterial()
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
                    Materials.Add(material);
                    LayersCount = Materials.Count;
                    Material = Materials[LayersCount - 1].Name;
                    Weight = GetWeight();
                }
                catch (Exception e)
                {
                    Status = Statuses.Status.Error;
                    logger.Error($"Ошибка при добавлении материала в загрузочного бункер [{InputTankerId}] : {e.Message}");
                    throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Получить суммарный вес всех загруженных материалов в загрузочный бункер
        /// </summary>
        /// <returns>Суммарный вес загруженного материала</returns>
        public double GetWeight()
        {
            double Result = 0;

            if (Materials != null && LayersCount > 0)
            {
                for (int i=0; i<LayersCount; i++)
                {
                    Result += Materials[i].getWeight();
                }
            }

            return Result;
        }

        /// <summary>
        /// Выгрузить из загрузочного бункера требуемый вес материала
        /// </summary>
        /// <param name="weight">Вес выгружаемого материала</param>
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

            Weight = 0;
            Status = Statuses.Status.Empty;
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
            Weight = 0;
        }
    }
}