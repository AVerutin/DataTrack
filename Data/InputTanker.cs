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

        public bool Selected { get; set; }

        /// <summary>
        /// Наименование загруженного в бункер материала
        /// </summary>
        public string Material { get; private set; }

        // Список материала, загруженного в загрузочный бункер
        private List<Material> _materials;
        // private List<Ingot> _ingots;
        
        // Текущее состояние загрузочного бункера
        public Statuses Status { get; private set; }
        // private Statuses.Status _status;

        private readonly Logger _logger;
        private int _layersCount;
        private int _timeLoading;
        
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

        public InputTanker(int number, int thread=0)
        {
            if (number > 0)
            {
                _logger = LogManager.GetCurrentClassLogger();
                InputTankerId = number;
                Statuses status = new Statuses();
                status.CurrentState = Statuses.Status.Off;
                Status = status;
                _materials = new List<Material>();
                // _ingots = new List<Ingot>();
                _layersCount = 0;
                Material = "";
                _timeLoading = 0;
                Thread = thread;
                StartPos = new Coords();
                FinishPos = new Coords();
            }
            else
            {
                _logger.Error($"Неверный ID загузочного бункера: [{number}]");
                throw new ArgumentException($"Неверный ID загузочного бункера: [{number}]");
            }
        }

        /// <summary>
        /// Установить статус загрузочного бункера
        /// </summary>
        /// <param name="status">Статус загрузочного бункера</param>
        public void SetStatus(Statuses status) => Status = status;

        /// <summary>
        /// Получить статус загрузочного бункера
        /// </summary>
        /// <returns>Статус загрузочного бункера</returns>
        public Statuses GetStatus() => Status;

        /// <summary>
        /// Установить текущее состояние загрузочного бункера 
        /// </summary>
        /// <param name="state">Текущее состояние загрузочного бункера</param>
        public void SetCurrentState(Statuses.Status state) => Status.CurrentState = state;

        /// <summary>
        /// Получить текущее состояние загрузочного бункера
        /// </summary>
        /// <returns>Текущее состояние загрузочного бункера</returns>
        public Statuses.Status GetCurrentState() => Status.CurrentState;

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
            // _status = Statuses.Status.Loading;

            if (material != null)
            {
                Material = material.Name;
                if (_layersCount > 0 && material.Name != Material)
                {
                    // _status = Statuses.Status.Error;
                    // Если наименование загружаемого материала не соответствует наименованию ранее заруженного материала
                    _logger.Error($"Попытка загрузить в загрузочный бункер {InputTankerId}, содержащий материал" +
                        $"{_materials[_layersCount-1].Name}, новый  материал {material.Name}");
                    throw new InvalidCastException($"Невозможно загрузить в загрузочный бункер {InputTankerId}, " + 
                        $"содержащий материал {Material}, новый материал {material.Name}");
                }

                try
                {
                    // Загрузка бункера производится определенное время
                    _logger.Info($"Начинается ожидание [{_timeLoading} сек] загрузки загрузочного бункера [{InputTankerId}]");
                    Debug.WriteLine($"Начинается ожидание [{_timeLoading} сек] загрузки загрузочного бункера [{InputTankerId}]");
                    var t = Task.Run(async delegate
                    {
                        await Task.Delay(TimeSpan.FromSeconds(_timeLoading));
                        // Ingot ingot = new Ingot(_ingots[_ingots.Count].Uid+1);
                        // ingot.AddMaterial(material);
                        _materials.Add(material);
                        // _ingots.Add(ingot);
                        // _layersCount = _ingots.Count;
                        // Material = _ingots[_layersCount - 1].GetLastMaterialName();
                        _layersCount = _materials.Count;
                        Material = _materials[_layersCount - 1].Name;
                    });
                    t.Wait();
                    _logger.Info($"Закончилось ожидание [{_timeLoading} сек] загрузки загрузочного бункера [{InputTankerId}]");
                    Debug.WriteLine($"Закончилось ожидание [{_timeLoading} сек] загрузки загрузочного бункера [{InputTankerId}]");
                }
                catch (Exception e)
                {
                    //  _status = Statuses.Status.Error;
                    _logger.Error($"Ошибка при добавлении материала в загрузочного бункер [{InputTankerId}] : {e.Message}");
                    throw new NotSupportedException();
                }
            }

            // _status = Statuses.Status.Off;
        }

        public void SetTimeLoading(int time)
        {
            if (time > 0)
            {
                _timeLoading = time;
            }
        }

        /// <summary>
        /// Выгрузить весь материал из загрузочного бункера
        /// </summary>
        /// <returns>Список выгруженного материала из загрузочного бункера</returns>
        public List<Material> Unload()
        {
            // _status = Statuses.Status.Unloading;

            // Получаем количество слоев материала. Если материала нет, выдаем ошибку
            if (_layersCount == 0)
            {
                // _status = Statuses.Status.Error;
                _logger.Error($"Загрузочный бункер {InputTankerId} не содержит материал");
                throw new ArgumentNullException($"Загрузочный бункер {InputTankerId} не содержит материал");
            }

            List<Material> result = _materials;
            
            var t = Task.Run(async delegate
            {
                await Task.Delay(_timeLoading * 1000);
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
            return _materials.Count;
        }

        /// <summary>
        /// Получить список слоев материала, загруженного в бункер
        /// </summary>
        /// <returns>Список слоев материала, загруженного в бункер</returns>
        public List<Material> GetMaterials()
        {
            return _materials;
        }

        /// <summary>
        /// Сброс загрузочного бункера в исходное состояние
        /// </summary>
        public void Reset()
        {
            Statuses status = new Statuses();
            status.CurrentState = Statuses.Status.Off; 
            Status = status;
            _layersCount = 0;
            _materials = new List<Material>();
            Material = "";
        }
    }
}