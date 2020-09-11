using System;
using System.Collections.Generic;
using NLog;

namespace DataTrack.Data
{
    public class Silos
    {
        /// <summary>
        /// Уникальный идентификатор силоса
        /// </summary>
        public readonly int SilosId;

        /// <summary>
        /// Текущее состояние силоса
        /// </summary>
        public Statuses.Status Status { get; private set; }

        /// <summary>
        /// Наименование загруженного в силос материала
        /// </summary>
        public string Material { get; private set; }

        // Список материала, загруженного в силос
        private List<Material> Materials;
        
        // Время загрузки силоса
        private int TimeLoading;
        private readonly Logger logger;
        private int LayersCount;
        
        /// <summary>
        /// Номер нити (Линия производства)
        /// </summary>
        public int Thread { get; set; }
        
        /// <summary>
        /// Начальная координата относительно начала линии производства
        /// </summary>
        public Coords StartPos { get; set; }
        
        /// <summary>
        /// Конечная координата относительно начала линии производства
        /// </summary>
        public Coords FinishPos { get; set; }

        public Silos(int number, int thread=0)
        {
            if (number > 0)
            {
                logger = LogManager.GetCurrentClassLogger();
                SilosId = number;
                Status = Statuses.Status.Off;
                Materials = new List<Material>();
                LayersCount = 0;
                Material = "";
                Thread = thread;
                StartPos = new Coords();
                FinishPos = new Coords();
            }
            else
            {
                Status = Statuses.Status.Error;
                logger.Error($"Номер создаваемого силоса не может быть равен {number}");
                throw new ArgumentException($"Номер создаваемого силоса не может быть равен {number}");
            }
        }

        /// <summary>
        /// Установить статус силоса
        /// </summary>
        /// <param name="status">Статус силоса</param>
        public void SetStatus(Statuses.Status status) => Status = status;             
        public Statuses.Status GetStatus() => Status;

        /// <summary>
        /// Сброс силоса в исходное состояние
        /// </summary>
        public void Reset()
        {
            Status = Statuses.Status.Off;
            LayersCount = 0;
            Materials = new List<Material>();
            Material = "";
        }

        /// <summary>
        /// Загрузить материал в силос из загрузочного бункера
        /// </summary>
        /// <param name="source">Загрузочный бункер, из которого принимается материал</param>
        public void Load(InputTanker source)
        {
            Status = Statuses.Status.Loading;

            if (source == null)
            {
                Status = Statuses.Status.Error;
                logger.Error($"Не указан загрузочный бункер при загрузке материала в силос {SilosId}");
                throw new ArgumentNullException($"Не указан загрузочный бункер при загрузке материала в силос {SilosId}");
            }

            if (Material == "")
            {
                logger.Info($"В силос {SilosId} загружается материал {source.Material}");
                Material = source.Material;
            }

            if (source.Material != Material)
            {
                Status = Statuses.Status.Error;
                logger.Error($"Загрузка в силос {SilosId}, содержащего материал {Material} новый материал {source.Material}");
                throw new ArgumentException($"Силос {SilosId} ожидает материал {Material} вместо {source.Material}");
            }

            // Добавляем материал из загрузочного бункера к слоям материала, уже имеющимся в силосе
            List<Material> materials = source.Unload();
            foreach (Material material in materials)
            {
                Materials.Add(material);
            }
            LayersCount = Materials.Count;
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
        /// Получить количество слоев материала, загруженного в силос
        /// </summary>
        /// <returns>Количество слоев материала, загруженного в силос</returns>
        public int GetLayersCount()
        {
            return Materials.Count;
        }

        /// <summary>
        /// Получить список слоев материала, загруженного в силос
        /// </summary>
        /// <returns>Список слоев материала, загруженного в силос</returns>
        public List<Material> GetMaterials()
        {
            return Materials;
        }

        /// <summary>
        /// Полная разгрузка силоса
        /// </summary>
        /// <returns>Список разгруженного материала</returns>
        public List<Material> Unload()
        {
            Status = Statuses.Status.Unloading;

            List<Material> Result = Materials;
            Materials = new List<Material>();
            LayersCount = Materials.Count;
            Material = "";
            Status = Statuses.Status.Off;
            
            return Result;
        }

        /// <summary>
        /// Выгрузить требуемый вес материала из силоса
        /// </summary>
        /// <param name="weight">Вес выгружаемого материала</param>
        /// <returns>Список выгруженного материала из силоса</returns>
        public List<Material> Unload(double weight)
        {
            Status = Statuses.Status.Unloading;

            // Получаем количество слоев материала. Если материала нет, выдаем ошибку
            if (LayersCount == 0)
            {
                Status = Statuses.Status.Error;
                logger.Error($"Силос {SilosId} не содержит материал, невозможно выгрузить {weight} кг");
                throw new ArgumentOutOfRangeException($"Силос {SilosId} не содержит материал, невозможно выгрузить {weight} кг");
            }

            List<Material> unloaded = new List<Material>();
            if (weight > 0)
            {
                // Известен вес выгружаемого материала, начинаем выгружать

                // Пока остались слои материала и количество списываемого материала больше нуля
                while (Materials.Count > 0 && weight > 0)
                {
                    List<Material> _materials = new List<Material>();
                    Material _material = Materials[0]; // получаем первый слой материала

                    // Если вес материала в слое больше веса списываемого материала,
                    // то вес списываемого материала устанавливаем в ноль, а вес слоя уменьшаем на вес списываемого материала
                    if (_material.Weight > weight)
                    {
                        // Находим вес оставшегося материала на слое
                        _material.setWeight(_material.Weight - weight);
                        Materials[0] = _material;

                        // Добавляем в выгруженную часть слоя в список выгруженного материала
                        Material unload = new Material();
                        unload.setMaterial(_material.ID, _material.Invoice, _material.Name, _material.PartNo, weight, _material.Volume);
                        unload.setWeight(weight);
                        unloaded.Add(unload);
                        weight = 0;
                    }
                    else
                    {
                        // Если вес материала в слое меньше веса списываемого материала,
                        // то удаляем полностью слой и уменьшаем вес списываемого материала на вес, который был в слое
                        if (_material.Weight < weight)
                        {
                            weight -= _material.Weight;
                            unloaded.Add(Materials[0]);
                            for (int i = 1; i < Materials.Count; i++)
                            {
                                _materials.Add(Materials[i]);
                            }
                            Materials = _materials;
                            LayersCount--;
                        }
                        else
                        {
                            // Если вес материала в слое равен весу списываемого материала,
                            // то удаляем слой и устнавливаем вес списываемого материала равным нулю
                            if (_material.Weight == weight)
                            {
                                weight = 0;
                                unloaded.Add(Materials[0]);
                                for (int i = 1; i < Materials.Count; i++)
                                {
                                    _materials.Add(Materials[i]);
                                }
                                Materials = _materials;
                                LayersCount--;
                            }
                        }
                    }
                }

                // Если вес списываемого материала больше нуля, а количество слоев равно нулю, 
                // то выдаем сообщение, что материал уже закончился, списывать больше нечего!
                if (Materials.Count == 0 && weight > 0)
                {
                    Status = Statuses.Status.Error;
                    logger.Error($"Материал в силосе {SilosId} закончился. Не хватило {weight} кг");
                    throw new ArgumentOutOfRangeException($"Материал в силосе {SilosId} закончился. Не хватило {weight} кг");
                }
            }
            else
            {
                Status = Statuses.Status.Error;
                logger.Warn($"Не указан вес выгружаемого материала из силоса {SilosId}");
                throw new ArgumentNullException($"Не указан вес выгружаемого материала из силоса {SilosId}");
            }

            Materials = new List<Material>();
            Material = "";
            LayersCount = 0;
            
            return unloaded;
        }
    }
}