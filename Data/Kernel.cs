using System.Collections.Generic;

namespace DataTrack.Data
{
    public class Kernel
    {
        private readonly Collection<Material> _materials;
        private readonly Collection<InputTanker> _inputTankers;
        private readonly Collection<Silos> _siloses;
        private readonly Collection<Conveyor> _conveyors;

        public Kernel()
        {
            _materials = new Collection<Material>();
            _inputTankers = new Collection<InputTanker>();
            _siloses = new Collection<Silos>();
            _conveyors = new Collection<Conveyor>();
        }

        /// <summary>
        /// Добавить новый материал
        /// </summary>
        /// <param name="material">Добавляемый материал</param>
        public void AddMaterial(Material material)
        {
            _materials.AddItem(material);
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
        /// Удаление материала из списка по его номеру
        /// </summary>
        /// <param name="num">Номер удаляемого материала</param>
        /// <returns>Результат удаления материала</returns>
        public bool RemoveMaterial(int num)
        {
            return _materials.RemoveItem(num);
        }

        /// <summary>
        /// Удаление всех материалов из списка
        /// </summary>
        public void ClearMaterials()
        {
            _materials.ClearCollection();
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
        /// Установить список загрузочных бункеров
        /// </summary>
        /// <param name="tankers">Список загрузочных буркеров</param>
        public void SetInputTankers(List<InputTanker> tankers)
        {
            _inputTankers.SetItems(tankers);
        }

        /// <summary>
        /// Получить список загрузочных бункеров
        /// </summary>
        /// <returns>Список загрузочных бункеров</returns>
        public List<InputTanker> GetInputTankers()
        {
            return _inputTankers.GetItems();
        }

        public int GetInputTankersCount()
        {
            return _inputTankers.GetItemsCount();
        }

        public InputTanker GetInputTanker(int num)
        {
            return _inputTankers.GetItem(num);
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
        /// Удалить все загрузочные бункеры
        /// </summary>
        public void ClearInputTankers()
        {
            _inputTankers.ClearCollection();
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
        /// Установить список силосов
        /// </summary>
        /// <param name="siloses">Список силосов</param>
        public void SetSiloses(List<Silos> siloses)
        {
            _siloses.SetItems(siloses);
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
        /// Удаление силоса по его номеру
        /// </summary>
        /// <param name="num">Номер удаляемого силоса</param>
        /// <returns>Результат выполнения операции удаления</returns>
        public bool RemoveSilos(int num)
        {
            return _siloses.RemoveItem(num);
        }

        /// <summary>
        /// Удалить все силосы
        /// </summary>
        public void ClearSiloses()
        {
            _siloses.ClearCollection();
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
        /// Установить список конвейеров
        /// </summary>
        /// <param name="conveyors">Список конвейеров</param>
        public void SetConveyors(List<Conveyor> conveyors)
        {
            _conveyors.SetItems(conveyors);
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
        /// Удалить конвейер из списка по его номеру
        /// </summary>
        /// <param name="num">Номер удаляемого конвейера</param>
        /// <returns>Результат выполнения операции удаления</returns>
        public bool RemoveConveyor(int num)
        {
            return _conveyors.RemoveItem(num);
        }

        /// <summary>
        /// Удалить все конвейеры
        /// </summary>
        public void ClearConveyors()
        {
            _conveyors.ClearCollection();
        }
    }
}