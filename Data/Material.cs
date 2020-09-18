using System.Collections.Generic;

namespace DataTrack.Data
{
    public class Material
    {
        // Свойства класса Материал
        #region
        public long ID { get; private set; }            // Уникальный идентификационный номер
        public long Invoice { get; private set; }       // Номер инвойса для идентификации номера партии материала 
        public string Name { get; private set; }        // Наименование материала
        public int PartNo { get; private set; }         // Номер партии
        public double Weight { get; private set; }      // Вес партии материала 
        public double Volume { get; private set; }      // Объем материала в партии 
        
        private List<Chemical> Chemicals;    // Химический состав материала
        #endregion

        // Конструктор по умолчанию
        public Material()
        {
            ID = 0;
            Invoice = 0;
            Name = "";
            PartNo = 0;
            Weight = 0;
            Volume = 0;
            Chemicals = new List<Chemical>();
        }

        /// <summary>
        /// Конструктор для создания материала
        /// </summary>
        /// <param name="id">Уникальный идентификатор материала</param>
        /// <param name="invoice">Номер инвойса для идентификации партии материала</param>
        /// <param name="name">Наименование материала</param>
        /// <param name="partno">Номер партии</param>
        /// <param name="weight">Вес материала</param>
        /// <param name="volume">Объем материала</param>
        public Material(long id, long invoice, string name, int partno, double weight, double volume)
        {
            ID = id;
            Invoice = invoice;
            Name = name;
            PartNo = partno;
            Weight = weight;
            Volume = volume;
        }

        /// <summary>
        /// Добавление элемента к химическому составу материала
        /// </summary>
        /// <param name="element">Наименование химического элемента</param>
        /// <param name="volume">Содержание химического элемента в составе материала</param>
        public void AddChemicals(List<Chemical> chemicals)
        {
            if (chemicals != null)
            {
                Chemicals = chemicals;
            }
        }

        /// <summary>
        /// Получить химический состав материала
        /// </summary>
        /// <returns>Список элементов, входящий в химический состав материала, и доля их содержания в материале</returns>
        public List<Chemical> GetChemicals()
        {
            return Chemicals;
        }

        /// <summary>
        /// Установить параметры материала
        /// </summary>
        /// <param name="id">Уникальный идентификатор материала</param>
        /// <param name="name">Наименование материала</param>
        /// <param name="partno">Номер партии</param>
        /// <param name="weight">Вес материала</param>
        /// <param name="weight">Объем материала</param>
        public void setMaterial(long id, long invoice, string name, int partno, double weight, double volume = 0.0)
        {
            ID = id;
            Invoice = invoice;
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

        public override string ToString()
        {
            string result = Name + "[" + PartNo +", " + Weight + "]";
            return result;
        }
    }
}