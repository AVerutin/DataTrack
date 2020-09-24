using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
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
        /// Количество слоев материала, загруженного в весовой бункер
        /// </summary>
        public int LayersCount { get; private set; }
        
        /// <summary>
        /// Начальная координата на линии производства
        /// </summary>
        public Coords StartPos { get; set; }
        
        /// <summary>
        /// Конечная координата на линии производства
        /// </summary>
        public Coords FinishPos { get; set; }

        // Логирование
        private Logger _logger;
        
        // Материал, загруженный в весовой бьункер
        private List<Material> _materials;

        private int _timeLoading;

        public WeightTanker(int id, double maxWeight = 3500, int thread = 0, long dbid = 0)
        {
            if (id>0)
            {
                WeightTankerId = id;
                MaxWeight = maxWeight;
                Thread = thread;
                WeightTankerDbId = dbid;
                Statuses status = new Statuses();
                status.CurrentState = Statuses.Status.Off; 
                Status = status;
                StartPos = new Coords();
                FinishPos = new Coords();
                _materials = new List<Material>();
                LayersCount = _materials.Count;
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
        /// <returns></returns>
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
        /// Загрузка определенного веса материала из указанного силоса
        /// </summary>
        /// <param name="silos">Силос, из которого будет произведена загрузка материала</param>
        /// <param name="weight">Вес материала, который требуется загрузить</param>
        public bool LoadMaterial(Silos silos, double weight)
        {
            bool result = false;

            return result;
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
                    $"Весовой бункер {WeightTankerId + 1} не содержит слой материала{num}. Всего загружено {LayersCount} слоев материала");
                throw new ArgumentOutOfRangeException(
                    $"Весовой бункер {WeightTankerId + 1} не содержит слой материала{num}. Всего загружено {LayersCount} слоев материала");
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
            LayersCount = _materials.Count;
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
    }
}