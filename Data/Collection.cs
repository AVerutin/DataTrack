using System.Collections.Generic;

namespace DataTrack.Data
{
    public class Collection<T>
    {
        private List<T> _collection;

        public Collection()
        {
            _collection = new List<T>();
        }

        /// <summary>
        /// Добавить элемент в коллекцию
        /// </summary>
        /// <param name="item">Добавляемый элемент в коллекцию</param>
        public void AddItem(T item)
        {
            if (item!=null)
            {
                _collection.Add(item);
            }
        }

        /// <summary>
        /// Получить коллекцию элементов
        /// </summary>
        /// <returns>Коллекция элементов</returns>
        public List<T> GetItems()
        {
            return _collection;
        }

        /// <summary>
        /// Установить ранее сформированную коллекцию
        /// </summary>
        /// <param name="items">Сформированная коллкция элементов</param>
        public void SetItems(List<T> items)
        {
            if (items != null)
            {
                _collection = items;
            }
        }

        /// <summary>
        /// Получить количество элементов в коллекции
        /// </summary>
        /// <returns></returns>
        public int GetItemsCount()
        {
            return _collection.Count;
        }

        /// <summary>
        /// Получить элемент коллекции по его номеру
        /// </summary>
        /// <param name="num">Номер элемента в коллекции</param>
        /// <returns>Элемент из коллекции по его номеру</returns>
        public T GetItem(int num)
        {
            T result = default;
            if (_collection != null && num >= 0 && num < _collection.Count)
            {
                result = _collection[num];
            }

            return result;
        }

        /// <summary>
        /// Удалить элемент из коллекции по его номеру
        /// </summary>
        /// <param name="num">Номер удаляемого элемента коллекции</param>
        /// <returns>Результат выполнения операции удаления</returns>
        public bool RemoveItem(int num)
        {
            bool result = false;
            if (_collection.Count > 0 && num >= 0 && num < _collection.Count)
            {
                _collection.RemoveAt(num);
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Очистить коллекцию
        /// </summary>
        public void ClearCollection()
        {
            _collection.Clear();
        }
    }
}