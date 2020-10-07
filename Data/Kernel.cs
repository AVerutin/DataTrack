using System.Collections.Generic;
using NpgsqlTypes;

namespace DataTrack.Data
{
    public class Kernel
    {
        private readonly Collection<Material> _materials;
        private readonly Collection<InputTanker> _inputTankers;
        private readonly Collection<Silos> _siloses;
        private readonly Collection<Conveyor> _conveyors;
        private readonly Collection<WeightTanker> _weightTankers;
        private readonly Collection<WeightTanker> _receiverTankers;
        private readonly Collection<Ingot> _ingots;
        private readonly Collection<Rollgang> _rollgangs;
        private readonly IngotParameters _parameters;
        private int _currentMaterialIndex;
        

        public Kernel()
        {
            _materials = new Collection<Material>();
            _inputTankers = new Collection<InputTanker>();
            _siloses = new Collection<Silos>();
            _conveyors = new Collection<Conveyor>();
            _weightTankers = new Collection<WeightTanker>();
            _receiverTankers = new Collection<WeightTanker>();
            _ingots = new Collection<Ingot>();
            _rollgangs = new Collection<Rollgang>();
            _parameters = new IngotParameters();
            _currentMaterialIndex = -1;
        }

        #region Добавление элементов в колекции

        /// <summary>
        /// Добавить рольганг
        /// </summary>
        /// <param name="rollgang">Рольганг</param>
        public void AddRollgang(Rollgang rollgang)
        {
            if (rollgang != null)
            {
                _rollgangs.AddItem(rollgang);
            }
        }
        
        /// <summary>
        /// Добавить единицу учета
        /// </summary>
        /// <param name="ingot">Единица учета</param>
        public void AddIngot(Ingot ingot)
        {
            if (ingot != null)
            {
                _ingots.AddItem(ingot);
            }
        }
        
        /// <summary>
        /// Добавить новый материал
        /// </summary>
        /// <param name="material">Добавляемый материал</param>
        public void AddMaterial(Material material)
        {
            if (material != null)
            {
                _materials.AddItem(material);
            }
        }
        
        /// <summary>
        /// Добавить новый силос 
        /// </summary>
        /// <param name="silos">Силос, добавляемый в коллекцию</param>
        public void AddSilos(Silos silos)
        {
            _siloses.AddItem(silos);
        }
        
        /// <summary>
        /// Добавить конвейер в список
        /// </summary>
        /// <param name="conveyor"></param>
        public void AddConveyor(Conveyor conveyor)
        {
            _conveyors.AddItem(conveyor);
        }
        
        /// <summary>
        /// Добавить новый весовой бункер
        /// </summary>
        /// <param name="tanker">Весовой бункер</param>
        public void AddWeightTanker(WeightTanker tanker)
        {
            _weightTankers.AddItem(tanker);
        }
        
        /// <summary>
        /// Добавить новый загрузочный бункер
        /// </summary>
        /// <param name="tanker">Загрузочный бункер</param>
        public void AddInputTanker(InputTanker tanker)
        {
            _inputTankers.AddItem(tanker);
        }
        
        /// <summary>
        /// Добавить новый приемочный бункер
        /// </summary>
        /// <param name="tanker">Приемочный бункер</param>
        public void AddReceiverTanker(WeightTanker tanker)
        {
            _receiverTankers.AddItem(tanker);
        } 
        
        /// <summary>
        /// Добавление строкового параметра для единицы учета
        /// </summary>
        /// <param name="key">Идентификатор параметра</param>
        /// <param name="value">Значение параметра</param>
        public void AddStringParameter(int key, string value)
        {
            if (key>0 && value.Trim()!="")
            {
                _parameters.AddStringParameter(key, value);
            }
        }
        
        /// <summary>
        /// Добавить значащий параметр для единицы учета
        /// </summary>
        /// <param name="key">Идентификатор параметра</param>
        /// <param name="value">Значение параметра</param>
        public void AddDoubleParameter(int key, double value)
        {
            if (key > 0 && value > 0)
            {
                _parameters.AddDoubleParameter(key, value);
            }
        }
        
        #endregion

        #region Получение элементов из коллекции
        
        /// <summary>
        /// Получить все параметры единицы учета
        /// </summary>
        /// <returns>Список всех параметров единицы учета</returns>
        public IngotParameters GetAllParameters()
        {
            return _parameters;
        }

        /// <summary>
        /// Получить общее количество параметров в списке
        /// </summary>
        /// <returns>Количество параметров в списке</returns>
        public int GetParametersCount()
        {
            return _parameters.GetParametersCount();
        }

        /// <summary>
        /// Получить параметр единицы учета по его индентификатору
        /// </summary>
        /// <param name="id">Идентификатор параметра</param>
        /// <returns>Параметр единицы учета</returns>
        public IngotParameters GetParameterById(int id)
        {
            IngotParameters result = new IngotParameters();
            var strings = _parameters.GetStringParameters();
            var doubles = _parameters.GetDoubleParameters();
            
            foreach (var item in strings)
            {
                if (item.Key == id)
                {
                    result.AddStringParameter(item.Key, item.Value);
                }
            }

            foreach (var item in doubles)
            {
                if (item.Key == id)
                {
                    result.AddDoubleParameter(item.Key, item.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Получить количество конвейеров в списке
        /// </summary>
        /// <returns>Количество имеющихся конвейеров</returns>
        public int GetConveyorsCount()
        {
            return _conveyors.GetItemsCount();
        }

        /// <summary>
        /// Получить конвейер из списка по его номеру
        /// </summary>
        /// <param name="num">Номер конвейера в списке</param>
        /// <returns>Конвейер из списка по его номеруц</returns>
        public Conveyor GetConveyor(int num)
        {
            return _conveyors.GetItem(num);
        }

        /// <summary>
        /// Получить список конвейеров
        /// </summary>
        /// <returns>Список имеющихся конвейеров</returns>
        public List<Conveyor> GetConveyors()
        {
            return _conveyors.GetItems();
        }
        
        /// <summary>
        /// Получить единицу учета по ее номеру
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public Ingot GetIngot(int num)
        {
            Ingot res = null;

            if (num > 0 && num < _ingots.GetItemsCount())
            {
                res = _ingots.GetItem(num);
            }
            return res;
        }

        /// <summary>
        /// Получить количество едниц учета
        /// </summary>
        /// <returns>Количество единиц учета</returns>
        public int GetIngotsCount() => _ingots.GetItemsCount();

        /// <summary>
        /// Получить список единиц учета
        /// </summary>
        /// <returns></returns>
        public List<Ingot> GetIngots() => _ingots.GetItems();
        
        /// <summary>
        /// Получить список рольгагнов
        /// </summary>
        /// <returns>Список рольгангов</returns>
        public List<Rollgang> GetRollgangs() => _rollgangs.GetItems();

        /// <summary>
        /// Получить количество рольгангов
        /// </summary>
        /// <returns>Количество рольгангов</returns>
        public int GetRollgangsCount() => _rollgangs.GetItemsCount();

        /// <summary>
        /// Получить рольганг по его номеру
        /// </summary>
        /// <param name="number">Номер рольганга</param>
        /// <returns>Рольганг</returns>
        public Rollgang GetRollgang(int number)
        {
            Rollgang res = null;
            if (number > 0 && number < GetRollgangsCount())
            {
                res = _rollgangs.GetItem(number);
            }

            return res;
        }

        /// <summary>
        /// Получить номер последнее добавленной единицы учета
        /// </summary>
        /// <returns></returns>
        public uint GetLastKnownId()
        {
            uint res = 0;
            foreach (Ingot ingot in _ingots.GetItems())
            {
                if (ingot.Uid > res)
                {
                    res = ingot.Uid + 1;
                }
            }
            
            return res;
        }
        
        /// <summary>
        /// Получить список всех материалов
        /// </summary>
        /// <returns>Список материалов</returns>
        public List<Material> GetMaterials()
        {
            return _materials.GetItems();
        }

        /// <summary>
        /// Получить количество материалов в списке
        /// </summary>
        /// <returns>Количество материалов в списке</returns>
        public int GetMaterialsCount()
        {
            return _materials.GetItemsCount();
        }

        /// <summary>
        /// Получить материал из списка по его номеру
        /// </summary>
        /// <param name="num">Номер материала</param>
        /// <returns>Материал из списка</returns>
        public Material GetMaterial(int num)
        {
            return _materials.GetItem(num);
        }
        
        /// <summary>
        /// Получить индекс текущего материала в списке
        /// </summary>
        /// <returns>Индекс текущего материала в списке</returns>
        public int GetCurrentMaterialIndex()
        {
            return _currentMaterialIndex;
        }
        
        /// <summary>
        /// Получить список загрузочных бункеров
        /// </summary>
        /// <returns>Список загрузочных бункеров</returns>
        public List<InputTanker> GetInputTankers()
        {
            return _inputTankers.GetItems();
        }

        /// <summary>
        /// Получить количество загрузочных бункеров
        /// </summary>
        /// <returns>Количество загрузочных бункеров</returns>
        public int GetInputTankersCount()
        {
            return _inputTankers.GetItemsCount();
        }

        /// <summary>
        /// Получить загрузочный бункер по его номеру
        /// </summary>
        /// <param name="num">Номер загрузочного бункера</param>
        /// <returns>Загрузочный бункер</returns>
        public InputTanker GetInputTanker(int num)
        {
            return _inputTankers.GetItem(num);
        }

        /// <summary>
        /// Получить список всех силосов
        /// </summary>
        /// <returns></returns>
        public List<Silos> GetSiloses()
        {
            return _siloses.GetItems();
        }

        /// <summary>
        /// Получить количество силосов
        /// </summary>
        /// <returns></returns>
        public int GetSilosesCount()
        {
            return _siloses.GetItemsCount();
        }

        /// <summary>
        /// Получить силос по его номеру
        /// </summary>
        /// <param name="num">Номер силоса</param>
        /// <returns>Силос из списка</returns>
        public Silos GetSilos(int num)
        {
            return _siloses.GetItem(num);
        }

        /// <summary>
        /// Получить список всех весовых бункеров
        /// </summary>
        /// <returns>Список весовых бункеров</returns>
        public List<WeightTanker> GetWeightTankers()
        {
            return _weightTankers.GetItems();
        }

        /// <summary>
        /// Получить количество весовых бункеров в списке
        /// </summary>
        /// <returns>Количество весовых бункеров в списке</returns>
        public int GetWeightTankersCount()
        {
            return _weightTankers.GetItemsCount();
        }

        /// <summary>
        /// Получить весовой бункер из списка по его номеру
        /// </summary>
        /// <param name="num">Номер весового бункера</param>
        /// <returns>Весовой бункер из списка</returns>
        public WeightTanker GetWeightTanker(int num)
        {
            return _weightTankers.GetItem(num);
        }
        
        /// <summary>
        /// Получить список всех приемочных бункеров
        /// </summary>
        /// <returns>Список приемочных бункеров</returns>
        public List<WeightTanker> GetReceiverTankers()
        {
            return _receiverTankers.GetItems();
        }

        /// <summary>
        /// Получить количество приемочных бункеров в списке
        /// </summary>
        /// <returns>Количество приемочных бункеров в списке</returns>
        public int GetReceiverTankersCount()
        {
            return _receiverTankers.GetItemsCount();
        }

        /// <summary>
        /// Получить приемочный бункер из списка по его номеру
        /// </summary>
        /// <param name="num">Номер приемочного бункера</param>
        /// <returns>Приемочный бункер из списка</returns>
        public WeightTanker GetReceiverTanker(int num)
        {
            return _receiverTankers.GetItem(num);
        }


        #endregion

        #region Установка элементов в коллекции

        /// <summary>
        /// Установить список рольгангов
        /// </summary>
        /// <param name="rollgangs">Список рольгангов</param>
        public void SetRollgangs(List<Rollgang> rollgangs)
        {
            if (rollgangs != null)
            {
                _rollgangs.SetItems(rollgangs);
            }
        }
        
        /// <summary>
        /// Установить подготовленный список единиц учета
        /// </summary>
        /// <param name="ingots">Список единиц учета</param>
        public void SetIngots(List<Ingot> ingots)
        {
            if (ingots != null)
            {
                _ingots.SetItems(ingots);
            }
        }
        
        /// <summary>
        /// Установить подготовленный список материала
        /// </summary>
        /// <param name="materials">Список материала</param>
        public void SetMaterials(List<Material> materials)
        {
            _materials.SetItems(materials);
        }
        
        /// <summary>
        /// Установить список загрузочных бункеров
        /// </summary>
        /// <param name="tankers">Список загрузочных буркеров</param>
        public void SetInputTankers(List<InputTanker> tankers)
        {
            _inputTankers.SetItems(tankers);
        }
        
        /// <summary>
        /// Установить список силосов
        /// </summary>
        /// <param name="siloses">Список силосов</param>
        public void SetSiloses(List<Silos> siloses)
        {
            _siloses.SetItems(siloses);
        }
        
        /// <summary>
        /// Установить список конвейеров
        /// </summary>
        /// <param name="conveyors">Список конвейеров</param>
        public void SetConveyors(List<Conveyor> conveyors)
        {
            _conveyors.SetItems(conveyors);
        }
        
        /// <summary>
        /// Установить список параметров для единицы учета
        /// </summary>
        /// <param name="parameters"></param>
        public void SetParameters(IngotParameters parameters)
        {
            _parameters.SetParameters(parameters);
        }

        /// <summary>
        /// Установить индекс текущего материала в списке
        /// </summary>
        /// <param name="index">Индекс материала</param>
        public void SetCurrentMaterialIndex(int index)
        {
            if (index >= 0 && (index < _materials.GetItemsCount() || _materials.GetItemsCount() == 0))
            {
                _currentMaterialIndex = index;
            }
        }

        /// <summary>
        /// Установить подготовленный список весовых бункеров
        /// </summary>
        /// <param name="tankers">Список весовых бункеров</param>
        public void SetWeightTankers(List<WeightTanker> tankers)
        {
            _weightTankers.SetItems(tankers);
        }
        
        /// <summary>
        /// Установить подготовленный список приемочных бункеров
        /// </summary>
        /// <param name="tankers">Список приемочных бункеров</param>
        public void SetReceiverTankers(List<WeightTanker> tankers)
        {
            _receiverTankers.SetItems(tankers);
        }
        
        #endregion

        #region Удаление элементов из коллекции

        /// <summary>
        /// Удалить рольганг по его номеру
        /// </summary>
        /// <param name="number">Номер рольганга</param>
        /// <returns>Результат удаления</returns>
        public bool RemoveRollgang(int number)
        {
            bool res = false;
            if (number > 0 && number < GetRollgangsCount())
            {
                res = _rollgangs.RemoveItem(number);
            }

            return res;
        }

        /// <summary>
        /// Удалить единицу учета по ее номеру
        /// </summary>
        /// <param name="num"></param>
        public void RemoveIngot(int num)
        {
            if(num>0 && num<_ingots.GetItemsCount())
            {
                _ingots.RemoveItem(num);
            }
        }
        
        /// <summary>
        /// Удаление материала из списка по его номеру
        /// </summary>
        /// <param name="num">Номер удаляемого материала</param>
        /// <returns>Результат удаления материала</returns>
        public bool RemoveMaterial(int num)
        {
            return _materials.RemoveItem(num);
        }
        
        /// <summary>
        /// Удалить загрузочный бункер по его номеру
        /// </summary>
        /// <param name="num">Номер удаляемого загрузочного бункера</param>
        /// <returns>Результат выполнения операции удаления загрузочного бункера</returns>
        public bool RemoveInputTanker(int num)
        {
            return _inputTankers.RemoveItem(num);
        }
        
        /// <summary>
        /// Удаление силоса по его номеру
        /// </summary>
        /// <param name="num">Номер удаляемого силоса</param>
        /// <returns>Результат выполнения операции удаления</returns>
        public bool RemoveSilos(int num)
        {
            return _siloses.RemoveItem(num);
        }
        
        /// <summary>
        /// Удалить конвейер из списка по его номеру
        /// </summary>
        /// <param name="num">Номер удаляемого конвейера</param>
        /// <returns>Результат выполнения операции удаления</returns>
        public bool RemoveConveyor(int num)
        {
            return _conveyors.RemoveItem(num);
        }
        
        /// <summary>
        /// Удалить параметр по его идентификатору
        /// </summary>
        /// <param name="id">Идентификатор параметра</param>
        /// <returns>Результат выполнения операции удаления</returns>
        public bool RemoveParameterById(int id)
        {
            bool result = false;
            if(_parameters.Exists(id))
            {
                result = _parameters.RemoveStringParameter(id) || _parameters.RemoveDoubleParameter(id); 
            }

            return result;
        }
        
        /// <summary>
        /// Удаление весового бункера из списка по его номеру
        /// </summary>
        /// <param name="num">Номер удаляемого весового бункера</param>
        /// <returns>Результат удаления весового бункера</returns>
        public bool RemoveWeightTanker(int num)
        {
            return _weightTankers.RemoveItem(num);
        }
        
        /// <summary>
        /// Удаление приемочного бункера из списка по его номеру
        /// </summary>
        /// <param name="num">Номер удаляемого приемочного бункера</param>
        /// <returns>Результат удаления приемочного бункера</returns>
        public bool RemoveReceiverTanker(int num)
        {
            return _receiverTankers.RemoveItem(num);
        }

        #endregion

        #region Удаление всех элементов их коллекции

        /// <summary>
        /// Очистить список рольгангов
        /// </summary>
        public void ClearRollgangs()
        {
            _rollgangs.ClearCollection();
        }
        
        /// <summary>
        /// Удалить все единицы учета
        /// </summary>
        public void ClearIngots()
        {
            _ingots.ClearCollection();
        }
        
        /// <summary>
        /// Удаление всех материалов из списка
        /// </summary>
        public void ClearMaterials()
        {
            _materials.ClearCollection();
        }
        
        /// <summary>
        /// Удалить все загрузочные бункеры
        /// </summary>
        public void ClearInputTankers()
        {
            _inputTankers.ClearCollection();
        }
        
        /// <summary>
        /// Удалить все силосы
        /// </summary>
        public void ClearSiloses()
        {
            _siloses.ClearCollection();
        }
        
        /// <summary>
        /// Удалить все конвейеры
        /// </summary>
        public void ClearConveyors()
        {
            _conveyors.ClearCollection();
        }
        
        /// <summary>
        /// Удалить все параметры единицы учета
        /// </summary>
        public void ClearParameters()
        {
            _parameters.RemoveAllParameters();
        }
        
        /// <summary>
        /// Удаление всех весовых бункеров из списка
        /// </summary>
        public void ClearWeightTankers()
        {
            _weightTankers.ClearCollection();
        }
        
        /// <summary>
        /// Удаление всех приемочных бункеров из списка
        /// </summary>
        public void ClearReceiverTankers()
        {
            _receiverTankers.ClearCollection();
        }
        
        #endregion

    }
}