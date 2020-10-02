using System;
using System.Collections.Generic;
using System.Diagnostics;
using NLog;

namespace DataTrack.Data
{
    public class WeightTanker
    {
        /// <summary>
        /// Уникальный идентификатор весового бункера
        /// </summary>
        public int WeightTankerId { get; private set; }
        
        /// <summary>
        /// Наименование бункера
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Уникальный идентификатор весового бункера в базе данных
        /// </summary>
        public long WeightTankerDbId { get; private set; }

        /// <summary>
        /// Совокупный вес материала, загруженного в весовой бункер
        /// </summary>
        public double Weight
        {
            get => GetWeight();
            private set => value = GetWeight();
        }
        
        /// <summary>
        /// Максимально допустимый совокупный вес загруженного материала
        /// </summary>
        public double MaxWeight { get; private set; }
        
        /// <summary>
        /// Текущее состояние весового бункера
        /// </summary>
        public Statuses Status { get; private set; }
        
        /// <summary>
        /// Номер нити (линии производства)
        /// </summary>
        public int Thread { get; private set; }
        
        /// <summary>
        /// Начальная координата на линии производства
        /// </summary>
        public Coords StartPos { get; set; }
        
        /// <summary>
        /// Конечная координата на линии производства
        /// </summary>
        public Coords FinishPos { get; set; }

        // Логирование
        private Logger _logger = LogManager.GetCurrentClassLogger();
        
        // Материал, загруженный в весовой бьункер
        private List<Material> _materials;
        private Ingot _ingot;

        private int _timeLoading;

        public WeightTanker(int id, string name = "", double maxWeight = 3500, int thread = 0, long dbid = 0)
        {
            if (id>0)
            {
                WeightTankerId = id;
                MaxWeight = maxWeight;
                Thread = thread;
                Name = name;
                WeightTankerDbId = dbid;
                Statuses status = new Statuses();
                status.CurrentState = Statuses.Status.Off; 
                Status = status;
                StartPos = new Coords();
                FinishPos = new Coords();
                _materials = new List<Material>();
                _ingot = new Ingot();
                Weight = GetWeight();
                _timeLoading = 0;
            }
            else
            {
                _logger.Error($"Некорректный номер весового бункера [{id}]. Номер должен быть больше нуля");
                throw new ArgumentNullException($"Некорректный номер весового бункера [{id}]. Номер должен быть больше нуля");
            }
        }

        /// <summary>
        /// Получить суммарный вес материала, загруженного в весовой бункер
        /// </summary>
        /// <returns>Суммарный вес материала, загруженного в весовой бункер</returns>
        public double GetWeight()
        {
            double weight = 0;
            foreach (Material material in _materials)
            {
                weight += material.getWeight();
            }

            return weight;
        }

        /// <summary>
        /// Добавить материал к уже имеющимся слоям
        /// </summary>
        /// <param name="material">Добавлемый материал</param>
        /// <returns>Результат выполнения операции добавления материала</returns>
        public bool AddMaterial(Material material)
        {
            bool result = false; 
            
            if(material!=null)
            {
                _materials.Add(material);
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Добавить слои материала к уже загруженному материалу в весовой бункер
        /// </summary>
        /// <param name="materials">Список добавляемого материала</param>
        /// <returns>Результат выполнения операции добавления материала</returns>
        public bool AddMaterials(List<Material> materials)
        {
            bool result = false;
            if (materials != null && materials.Count > 0)
            {
                foreach (Material material in materials)
                {
                    AddMaterial(material);
                }
            }

            return result;
        }

        /// <summary>
        /// Загрузка определенного веса материала из указанного силоса
        /// </summary>
        /// <param name="silos">Силос, из которого будет произведена загрузка материала</param>
        /// <param name="weight">Вес материала, который требуется загрузить</param>
        public bool LoadMaterial(Silos silos, double weight)
        {
            bool result = false;
            List<Material> loaded = new List<Material>();
            // Загружаем из силоса требуемый вес материала
            try
            {
                loaded = silos.Unload(weight);
                AddMaterials(loaded);
                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return result;
        }
        
        /// <summary>
        /// Загрузка материала в приемочный бункер из весового
        /// </summary>
        /// <param name="tanker">Весовой бункер, из которого будет загружен материал</param>
        /// <param name="weight">Вес загружаемого материала</param>
        /// <returns></returns>
        public void LoadMaterial(WeightTanker tanker, double weight)
        {
            // Загружаем из силоса требуемый вес материала
            try
            {
                List<Material> loaded = new List<Material>();
                loaded = tanker.Unload(weight);
                AddMaterials(loaded);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Разгрузка материала из весового бункера
        /// </summary>
        /// <param name="weight">Вес разгружаемого материала</param>
        /// <returns>Список разгруженного материала</returns>
        public List<Material> Unload(double weight)
        {
            // Получаем количество слоев материала. Если материала нет, выдаем ошибку
            if (GetLayersCount() == 0)
            {
                _logger.Error($"Бункер {WeightTankerId} не содержит материал, невозможно выгрузить {weight} кг");
                throw new ArgumentOutOfRangeException($"Бункер {WeightTankerId} не содержит материал, невозможно выгрузить {weight} кг");
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
                            }
                        }
                    }
                }

                // Если вес списываемого материала больше нуля, а количество слоев равно нулю, 
                // то выдаем сообщение, что материал уже закончился, выгружать больше нечего!
                if (_materials.Count == 0 && weight > 0)
                {
                    _logger.Error($"Материал в бункере{WeightTankerId} закончился. Не хватило {weight} кг");
                }
            }
            else
            {
                _logger.Warn($"Не указан вес выгружаемого материала из бункера {WeightTankerId}");
                throw new ArgumentNullException($"Не указан вес выгружаемого материала из бункера {WeightTankerId}");
            }

            if(GetLayersCount() == 0)
            {
                _materials = new List<Material>();
            }
            
            return unloaded;
        }

        /// <summary>
        /// Выгрузить весь материал из бункера
        /// </summary>
        /// <returns>Список выгруженного материала из бункера</returns>
        public List<Material> Unload()
        {
            List<Material> Result = _materials;
            _materials = new List<Material>();
            
            return Result;
        }

        /// <summary>
        /// Загрузка всего материала из указанного силоса
        /// </summary>
        /// <param name="silos">Силос, из которого будет произведена загрузка материала</param>
        /// <returns></returns>
        public void LoadMaterial(Silos silos)
        {
            if (silos == null)
            {
                _logger.Error("Не указан силос для загрузки материала");
                throw new ArgumentNullException("Не указан силос для загрузки материала");
            }

            // Силос передан
            // проверяем наличие материала в силосе
            if (silos.GetLayersCount() == 0)
            {
                _logger.Error($"Силос {silos.SilosId} не содержит материал!");
                throw new ArgumentNullException($"Силос {silos.SilosId} не содержит материал!");
            }

            // проверякм текущее состояние силоса
            if (silos.GetCurrentState() != Statuses.Status.Off)
            {
                _logger.Error($"Силос {silos.SilosId} не готов к разгрузке [{silos.Status.ToString()}]");
                throw new ArgumentException($"Силос {silos.SilosId} не готов к разгрузке [{silos.Status.ToString()}]");
            }
            
            List<Material> materials = silos.Unload();
            foreach (Material material in materials)
            {
                _materials.Add(material);
            }
        }

        /// <summary>
        /// Получить количество слоев материала, загруженного в весовой бункер
        /// </summary>
        /// <returns>Количество слоев материала, загруженного в весовой бункер</returns>
        public int GetLayersCount()
        {
            return _materials.Count;
        }

        /// <summary>
        /// Получить список материала, загруженного в весовой бункер
        /// </summary>
        /// <returns>Список материала, загруженного в весовой бункер</returns>
        public List<Material> GetMaterials()
        {
            return _materials;
        }

        /// <summary>
        /// Получить материал по его номеру в списке
        /// </summary>
        /// <param name="num">Номера материала в сиписке</param>
        /// <returns>Материал, загруженный в весовой бункер</returns>
        public Material GetMaterial(int num)
        {
            Material result = null;
            if (num <= _materials.Count)
            {
                result = _materials[num - 1];
            }
            else
            {
                _logger.Error(
                    $"Весовой бункер {WeightTankerId + 1} не содержит слой материала{num}. Всего загружено {GetLayersCount()} слоев материала");
                throw new ArgumentOutOfRangeException(
                    $"Весовой бункер {WeightTankerId + 1} не содержит слой материала{num}. Всего загружено {GetLayersCount()} слоев материала");
            }
            
            return result;
        }

        /// <summary>
        /// Сброс состояния весового бункера до начального состояния
        /// </summary>
        public void Reset()
        {
            Statuses status = new Statuses();
            status.CurrentState = Statuses.Status.Off; 
            Status = status;
            _materials = new List<Material>();
            Weight = GetWeight();
        }

        /// <summary>
        /// Установить текущее состояние весового бункера
        /// </summary>
        /// <param name="status">Текущее состояние весового бункера</param>
        public void SetStatus(Statuses status) => Status = status;

        /// <summary>
        /// Получить текущее состояние весового бункера
        /// </summary>
        /// <returns></returns>
        public Statuses GetStatus() => Status;

        /// <summary>
        /// Установить текущее состояние весового бункера
        /// </summary>
        /// <param name="state">Текущее состояние весового бункера</param>
        public void SetCurrentState(Statuses.Status state) => Status.CurrentState = state;

        /// <summary>
        /// Получить текущее состояние весового бункера
        /// </summary>
        /// <returns>Текущее состояние весового бункера</returns>
        public Statuses.Status GetCurrentState() => Status.CurrentState;

        /// <summary>
        /// Установить время загрузки материала в весовой бункер в секундах 
        /// </summary>
        /// <param name="time">Время загрузки материала в весовой бункер</param>
        public void SetTimeLoading(int time)
        {
            if (time > 0)
            {
                _timeLoading = time;
            }
            else
            {
                _logger.Error($"Время загрузки весового бункера [{time}] должно быть больше нуля");
                throw new ArgumentNullException($"Время загрузки весового бункера [{time}] должно быть больше нуля");
            }
        }

        /// <summary>
        /// Получить вес материала, который можно загрузить в бункер
        /// </summary>
        /// <returns>Свободный вес бункера</returns>
        public double GetAvailibleWeight() => MaxWeight - GetWeight();
    }
}