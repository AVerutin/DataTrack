using System.Collections.Generic;

namespace DataTrack.Data
{
    public class Material
    {
        // Свойства класса Материал
        #region
        public long ID { get; private set; }            // Уникальный идентификационный номер
        public string Name { get; private set; }        // Наименование материала
        public int PartNo { get; private set; }         // Номер партии
        public double Weight { get; private set; }      // Вес партии материала 
        public double Volume { get; private set; }      // Объем материала в партии 
        
        private readonly Dictionary<string, double> Chemicals;    // Химический состав материала
        #endregion

        // Конструктор по умолчанию
        public Material()
        {
            ID = 0;
            Name = "";
            PartNo = 0;
            Weight = 0;
            Volume = 0;
            Chemicals = new Dictionary<string, double>();
        }

        /// <summary>
        /// Добавление элемента к химическому составу материала
        /// </summary>
        /// <param name="element">Наименование химического элемента</param>
        /// <param name="volume">Содержание химического элемента в составе материала</param>
        public void AddElement(string element, double volume)
        {
            if (element.Trim() != "" && volume > 0)
            {
                Chemicals.Add(element, volume);
            }
        }

        /// <summary>
        /// Удаление элемента из химического состава материала
        /// </summary>
        /// <param name="element">Наименование удаляемого элемента из химического состава материала</param>
        public void RemoveElement (string element)
        {
            if(element.Trim() != "")
            {
                // Ищем, имеется ли такой элемент в химическом составе материала
                if (Chemicals.ContainsKey(element))
                {
                    Chemicals.Remove(element);
                }
            }
        }

        /// <summary>
        /// Получить химический состав материала
        /// </summary>
        /// <returns>Список элементов, входящий в химический состав материала, и доля их содержания в материале</returns>
        public Dictionary<string, double> GetChemicals()
        {
            return Chemicals;
        }

        /// <summary>
        /// Изменить долю содержания элемента в составе материала
        /// </summary>
        /// <param name="element">Наименование элемента, содержание которого требуется изменить</param>
        /// <param name="volume">Значение содержания элемента в составе материала</param>
        /// <returns>Результат выполнения операции (true - успешно, false - ошибка)</returns>
        public bool ChangeElementVolume(string element, double volume)
        {
            bool Result = false;

            // Если передан пустой элемент или неправильное значение доли содержания материала,
            // то возвращаем false
            if (element.Trim() == "" || volume <= 0)
            {
                return Result;
            }

            // Проверяем, есть ли такой элемент в составе материала
            if (Chemicals.ContainsKey(element))
            {
                Chemicals[element] = volume;
                Result = true;
            }

            return Result;
        }

        /// <summary>
        /// Конструктор для создания материала
        /// </summary>
        /// <param name="id">Уникальный идентификатор материала</param>
        /// <param name="name">Наименование материала</param>
        /// <param name="partno">Номер партии</param>
        /// <param name="weight">Вес материала</param>
        /// <param name="volume">Объем материала</param>
        public Material(long id=0, string name = "", int partno = 0, double weight=0.0, double volume = 0.0)
        {
            ID = id;
            Name = name;
            PartNo = partno;
            Weight = weight;
            Volume = volume;
        }

        /// <summary>
        /// Установить параметры материала
        /// </summary>
        /// <param name="id">Уникальный идентификатор материала</param>
        /// <param name="name">Наименование материала</param>
        /// <param name="partno">Номер партии</param>
        /// <param name="weight">Вес материала</param>
        /// <param name="weight">Объем материала</param>
        public void setMaterial(long id, string name, int partno, double weight, double volume = 0.0)
        {
            ID = id;
            Name = name;
            PartNo = partno;
            Weight = weight;
            Volume = volume;
        }

        /// <summary>
        /// Получить наименование материала
        /// </summary>
        /// <returns>Наименование материала</returns>
        public string getName()
        {
            return Name;
        }

        /// <summary>
        /// Установить наименование материала
        /// </summary>
        /// <param name="name">Наименование материала</param>
        public void setName(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Получить номер партии материала
        /// </summary>
        /// <returns></returns>
        public int getPartNo()
        {
            return PartNo;
        }

       /// <summary>
       /// Установить номер партии материала
       /// </summary>
       /// <param name="partno">Номер партии материала</param>
        public void setPartNo(int partno)
        {
            PartNo = partno;
        }

        /// <summary>
        /// Получить вес материала
        /// </summary>
        /// <returns>Вес материала</returns>
        public double getWeight()
        {
            return Weight;
        }

        /// <summary>
        /// Установить вес материала
        /// </summary>
        /// <param name="weight"></param>
        public void setWeight(double weight)
        {
            Weight = weight;
        }

        /// <summary>
        /// Получить уникальный идентификатор материала
        /// </summary>
        /// <returns>Уникальный идентификатор материала</returns>
        public long getId()
        {
            return ID;
        }

        /// <summary>
        /// Задать уникальный идентификатор материала
        /// </summary>
        /// <param name="id">Уникальный идентификатор материала</param>
        public void setId(long id)
        {
            ID = id;
        }

        /// <summary>
        /// Получить объем материала
        /// </summary>
        /// <returns></returns>
        public double getVolume()
        {
            return Volume;
        }

        /// <summary>
        /// Установить объем материала
        /// </summary>
        /// <param name="volume"></param>
        public void setVolume(double volume)
        {
            Volume = volume;
        }

    }
}