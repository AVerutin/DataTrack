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

        /// <summary>
        /// Вес загруженного в силос материала
        /// </summary>
        public double Weight { get; private set; }

        private List<Material> Materials;
        private readonly Logger logger;
        private int LayersCount;

        public Silos(int number)
        {
            if (number > 0)
            {
                logger = LogManager.GetCurrentClassLogger();
                SilosId = number;
                Status = Statuses.Status.Off;
                Materials = new List<Material>();
                LayersCount = 0;
                Material = "";
                Weight = 0;
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
        public void SetStatus(Statuses.Status status)
        {
            Status = status;
        }

        /// <summary>
        /// Выбор материала силоса
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
                logger.Error($"Установка пустого материала для силоса {SilosId}");
                throw new ArgumentNullException("Нельзя установить пустой материал для силоса!");
            }
        }

        /// <summary>
        /// Расчитать вес материала, загруженного в силос
        /// </summary>
        /// <returns>Рассчитанный вес материала в силосе</returns>
        private double GetWeight()
        {
            double Result = 0;

            if (Materials != null && LayersCount > 0)
            {
                for (int i = 0; i < LayersCount; i++)
                {
                    Result += Materials[i].getWeight();
                }
            }

            return Result;
        }

        /// <summary>
        /// Сброс силоса в исходное состояние
        /// </summary>
        public void Reset()
        {
            Status = Statuses.Status.Off;
            LayersCount = 0;
            Materials = new List<Material>();
            Material = "";
            Weight = 0;
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

            Materials = source.Unload();
            LayersCount = Materials.Count;
            Weight = GetWeight();
        }

        /// <summary>
        /// Полная разгрузка силоса
        /// </summary>
        /// <returns>Список разгруженного материала</returns>
        public List<Material> UnloadFull()
        {
            Status = Statuses.Status.Unloading;

            List<Material> Result = Materials;
            Materials = new List<Material>();
            LayersCount = Materials.Count;
            Status = Statuses.Status.Empty;
            Weight = 0;
            
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
                        unload.setMaterial(_material.ID, _material.Name, _material.PartNo, weight, _material.Volume);
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

            Weight = GetWeight();
            return unloaded;
        }
    }
}