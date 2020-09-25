﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
        /// Уникальный идентификатор силоса в базе данных
        /// </summary>
        public long SilosDbId { get; private set; }

        /// <summary>
        /// Текущее состояние силоса
        /// </summary>
        public Statuses Status { get; private set; }

        /// <summary>
        /// Наименование загруженного в силос материала
        /// </summary>
        public string Material { get; private set; }

        // Список материала, загруженного в силос
        private List<Material> _materials;
        
        // Время загрузки силоса
        private int _timeLoading;
        private readonly Logger _logger;
        private int _layersCount;
        
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

        public Silos(int number, int thread=0, long dbid=0)
        {
            if (number > 0)
            {
                _logger = LogManager.GetCurrentClassLogger();
                SilosId = number;
                SilosDbId = dbid;
                Statuses status = new Statuses();
                status.CurrentState = Statuses.Status.Off;
                Status = status;
                _materials = new List<Material>();
                _layersCount = 0;
                Material = "";
                Thread = thread;
                StartPos = new Coords();
                FinishPos = new Coords();
                _timeLoading = 0;
            }
            else
            {
                // Status = Statuses.Status.Error;
                _logger.Error($"Номер создаваемого силоса не может быть равен {number}");
                throw new ArgumentNullException($"Номер создаваемого силоса не может быть равен {number}");
            }
        }

        /// <summary>
        /// Установить статус силоса
        /// </summary>
        /// <param name="status">Статус силоса</param>
        public void SetStatus(Statuses status) => Status = status;             
        
        /// <summary>
        /// Получить статус силоса
        /// </summary>
        /// <returns>Статус силоса</returns>
        public Statuses GetStatus() => Status;

        /// <summary>
        /// Установить текущее состояние силоса
        /// </summary>
        /// <param name="state">Текущее состояние силоса</param>
        public void SetCurrentState(Statuses.Status state) => Status.CurrentState = state;
        
        /// <summary>
        /// Получить текущее состояние силоса
        /// </summary>
        /// <returns>Текущее состояние силоса</returns>
        public Statuses.Status GetCurrentState() => Status.CurrentState;
        

        /// <summary>
        /// Сброс силоса в исходное состояние
        /// </summary>
        public void Reset()
        {
            Status.CurrentState = Statuses.Status.Off;
            _layersCount = 0;
            _materials = new List<Material>();
            Material = "";
        }

        /// <summary>
        /// Загрузить материал в силос из загрузочного бункера
        /// </summary>
        /// <param name="source">Загрузочный бункер, из которого принимается материал</param>
        public void Load(InputTanker source)
        {
            if (source == null)
            {
                _logger.Error($"Не указан загрузочный бункер при загрузке материала в силос {SilosId}");
                throw new ArgumentNullException($"Не указан загрузочный бункер при загрузке материала в силос {SilosId}");
            }

            if (Material == "")
            {
                _logger.Info($"В силос {SilosId} загружается материал {source.Material}");
                Material = source.Material;
            }

            if (source.Material != Material)
            {
                _logger.Error($"Загрузка в силос {SilosId}, содержащего материал {Material} новый материал {source.Material}");
                throw new ArgumentException($"Силос {SilosId} ожидает материал {Material} вместо {source.Material}");
            }

            // Добавляем материал из загрузочного бункера к слоям материала, уже имеющимся в силосе
            List<Material> materials = source.Unload();
            foreach (Material material in materials)
            {
                _materials.Add(material);
            }
            _layersCount = _materials.Count;
        }

        public void SetTimeLoading(int time)
        {
            if (time > 0)
            {
                _timeLoading = time;
            }
        }

        /// <summary>
        /// Получить количество слоев материала, загруженного в силос
        /// </summary>
        /// <returns>Количество слоев материала, загруженного в силос</returns>
        public int GetLayersCount()
        {
            return _materials.Count;
        }

        /// <summary>
        /// Получить список слоев материала, загруженного в силос
        /// </summary>
        /// <returns>Список слоев материала, загруженного в силос</returns>
        public List<Material> GetMaterials()
        {
            return _materials;
        }

        /// <summary>
        /// Полная разгрузка силоса
        /// </summary>
        /// <returns>Список разгруженного материала</returns>
        public List<Material> Unload()
        {
            List<Material> Result = _materials;
            _materials = new List<Material>();
            _layersCount = _materials.Count;
            Material = "";
            
            return Result;
        }

        /// <summary>
        /// Выгрузить требуемый вес материала из силоса
        /// </summary>
        /// <param name="weight">Вес выгружаемого материала</param>
        /// <returns>Список выгруженного материала из силоса</returns>
        public List<Material> Unload(double weight)
        {
            // Получаем количество слоев материала. Если материала нет, выдаем ошибку
            if (_layersCount == 0)
            {
                _logger.Error($"Силос {SilosId} не содержит материал, невозможно выгрузить {weight} кг");
                throw new ArgumentOutOfRangeException($"Силос {SilosId} не содержит материал, невозможно выгрузить {weight} кг");
            }

            List<Material> unloaded = new List<Material>();
            if (weight > 0)
            {
                // Известен вес выгружаемого материала, начинаем выгружать

                // Пока остались слои материала и количество списываемого материала больше нуля
                while (_materials.Count > 0 && weight > 0)
                {
                    List<Material> materials = new List<Material>();
                    Material material = _materials[0]; // получаем первый слой материала

                    // Если вес материала в слое больше веса списываемого материала,
                    // то вес списываемого материала устанавливаем в ноль, а вес слоя уменьшаем на вес списываемого материала
                    if (material.Weight > weight)
                    {
                        // Находим вес оставшегося материала на слое
                        material.setWeight(material.Weight - weight);

                        // Добавляем в выгруженную часть слоя в список выгруженного материала
                        Material unload = new Material();
                        unload.setMaterial(material.ID, material.Invoice, material.Name, material.PartNo, weight, material.Volume);
                        unload.setWeight(weight);
                        unloaded.Add(unload);
                        weight = 0;
                    }
                    else
                    {
                        // Если вес материала в слое меньше веса списываемого материала,
                        // то удаляем полностью слой и уменьшаем вес списываемого материала на вес, который был в слое
                        
                        if (material.Weight < weight)
                        {
                            weight -= material.Weight;
                            unloaded.Add(_materials[0]);
                            for (int i = 1; i < _materials.Count; i++)
                            {
                                materials.Add(_materials[i]);
                            }
                            _materials = materials;
                            _layersCount = _materials.Count;
                        }
                        else
                        {
                            // Если вес материала в слое равен весу списываемого материала,
                            // то удаляем слой и устнавливаем вес списываемого материала равным нулю
                            if ( Math.Abs(material.Weight - weight) <= 0.001)
                            {
                                // этот код не выпоняется никогда
                                weight = 0;
                                unloaded.Add(_materials[0]);
                                for (int i = 1; i < _materials.Count; i++)
                                {
                                    _materials.Add(_materials[i]);
                                }
                                _materials = materials;
                                _layersCount = _materials.Count;
                            }
                        }
                    }
                }

                // Если вес списываемого материала больше нуля, а количество слоев равно нулю, 
                // то выдаем сообщение, что материал уже закончился, выгружать больше нечего!
                if (_materials.Count == 0 && weight > 0)
                {
                    _logger.Error($"Материал в силосе {SilosId} закончился. Не хватило {weight} кг");
                }
            }
            else
            {
                _logger.Warn($"Не указан вес выгружаемого материала из силоса {SilosId}");
                throw new ArgumentNullException($"Не указан вес выгружаемого материала из силоса {SilosId}");
            }

            if(_layersCount == 0)
            {
                _materials = new List<Material>();
                Material = "";
            }
            
            return unloaded;
        }
    }
}