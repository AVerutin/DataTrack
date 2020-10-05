using System;
using System.Collections.Generic;

namespace DataTrack.Data
{
    /* Класс "Единица Учета"
     * Список материала
     * Цель (куда предназаначен материал)
     * Список объектов, через которые прошли и последний, в котором находимся
     * Номер нити, на которой находмся
     * Текущие координаты относительно начала текущей нити
     * 
     */
    public class Ingot
    {
        /// <summary>
        /// Уникальный иденттификатор единицы учета
        /// </summary>
        public uint Uid { get; set; }
        
        /// <summary>
        /// Идентификатор в таблице единиц учета в базе данных
        /// </summary>
        public ulong DbUid { get; set; }
        
        /// <summary>
        /// Номер плавки
        /// </summary>
        public long PlavNo { get; set; }
        
        /// <summary>
        /// Цвет единицы учета
        /// </summary>
        public ConsoleColor Color { get; private set; }
        
        /// <summary>
        /// Координаты головы единицы учета 
        /// </summary>
        public Coords StartPos { get; set; }
        
        /// <summary>
        /// Координаты хвоста единицы учета
        /// </summary>
        public Coords FinishPos { get; set; }
        
        /// <summary>
        /// Координаты центра единицы учета
        /// </summary>
        public Coords CenterPos { get; set; }
        
        /// <summary>
        /// Номер нити, на которой находится единица учета
        /// </summary>
        public int Thread { get; set; }

        /// <summary>
        /// Родитель единицы учета
        /// </summary>
        public ulong Parent { get; private set; }

        /// <summary>
        /// Список дочерних единиц учета
        /// </summary>
        public List<ulong> Children { get; private set; }
        
        /// <summary>
        /// Параметры отображения единицы учета
        /// </summary>
        public IngotVisualParameters VisualParameters { get; set; }
        
        /// <summary>
        /// Список материалов, входящих в состав единицы учета
        /// </summary>
        private List<Material> _materials;

        /// <summary>
        /// Список параметров единицы учета
        /// </summary>
        private readonly IngotParameters _parameters;

        /// <summary>
        /// Время начала и конца движения единицы учета
        /// </summary>
        private DateTime _startTime;
        private DateTime _finishTime;
        private DateTime _accessTime;

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public Ingot()
        {
            Uid = 0;
            DbUid = 0;
            Thread = 0;
            Parent = 0;
            _parameters = new IngotParameters();
            StartPos = new Coords();
            FinishPos = new Coords();
            CenterPos = new Coords();
            Children = new List<ulong>();
            _startTime = DateTime.Now;
            _finishTime = new DateTime();
            _accessTime = new DateTime();
            int colorsCount = 13;
            Random rnd = new Random();
            Color = (ConsoleColor)rnd.Next(colorsCount)+1;
            VisualParameters = new IngotVisualParameters("img/colors/Empty.png");
            _materials = new List<Material>();
        }

        /// <summary>
        /// Конструктор класса единицы учета
        /// </summary>
        /// <param name="uid">Уникальный идентификатор единицы учета</param>
        /// <param name="dbuid">Уникальный идентификатор единицы учета в базе данных</param>
        /// <param name="thread">Номер нити</param>
        /// <param name="parent">Уникальный идентификатор родительской единицы учета</param>
        /// <exception cref="Exception">Не указан уникальный идентификатор единицы учета</exception>
        public Ingot(uint uid, ulong dbuid=0, int thread=0, ulong parent=0)
        {
            if (uid > 0)
            {
                Uid = uid;
                DbUid = dbuid;
                Thread = thread;
                Parent = parent;
                _parameters = new IngotParameters();
                StartPos = new Coords();
                FinishPos = new Coords();
                CenterPos = new Coords();
                Children = new List<ulong>();
                _materials = new List<Material>();
                _startTime = DateTime.Now;
                _finishTime = new DateTime();
                _accessTime = new DateTime();
                int colorsCount = 13;
                Random rnd = new Random();
                Color = (ConsoleColor)rnd.Next(colorsCount)+1;
                VisualParameters = new IngotVisualParameters("img/colors/Empty.png");
            }
            else
            {
                throw new Exception("Не указан уникальный идентификатор для создаваемой единицы учета");
            }
        }

        /// <summary>
        /// Добавить новый строковый параметр к единице учета
        /// </summary>
        /// <param name="key">Идентификатор параметра</param>
        /// <param name="value">Значение параметра</param>
        public void AddParameter(int key, string value)
        {
            _parameters.AddStringParameter(key, value);
        }

        /// <summary>
        /// Добавить новый значащий параметр для единицы учета
        /// </summary>
        /// <param name="key">Идентификатор параметра</param>
        /// <param name="value">Значение параметра</param>
        public void AddParameter(int key, double value)
        {
            _parameters.AddDoubleParameter(key, value);
        }

        /// <summary>
        /// Установить уникальный идентификатор в базе данных
        /// </summary>
        /// <param name="dbuid">Уникальный идентификатор</param>
        public void SetDbId(ulong dbuid)
        {
            if (dbuid > 0)
            {
                DbUid = dbuid;
            }
        }

        /// <summary>
        /// Добавить материал в единицу учета 
        /// </summary>
        /// <param name="material">Добавляемый материал</param>
        public void AddMaterial(Material material)
        {
            if (material != null)
            {
                _materials.Add(material);
            }
        }

        /// <summary>
        /// Добавить список материалов в единицу учета
        /// </summary>
        /// <param name="materials">Добавляемый материал</param>
        public void AddMaterials(List<Material> materials)
        {
            if (materials != null)
            {
                foreach (Material item in materials)
                {
                    _materials.Add(item);
                }
            }
        }

        /// <summary>
        /// Задать навчальное время для единицы учета
        /// </summary>
        /// <param name="time">Время зарождения единицы учета</param>
        public void SetStartTime(DateTime time) => _startTime = time;

        /// <summary>
        /// Задать конечное время для единицы учета
        /// </summary>
        /// <param name="time">Время уничтожения единицы учета</param>
        public void SetFinishTime(DateTime time) => _finishTime = time;

        /// <summary>
        /// Задать время последнего обращения к единице учета
        /// </summary>
        /// <param name="time">Время последнего обращения к единице учета</param>
        public void SetAccessTime(DateTime time) => _accessTime = time;

        /// <summary>
        /// Получить начальное время для единицы учета
        /// </summary>
        /// <returns>Время зарождения единицы учета</returns>
        public DateTime GetStartTime() => _startTime;

        /// <summary>
        /// Получить конечное время для единицы учета
        /// </summary>
        /// <returns>Время уничтожения единицы учета</returns>
        public DateTime GetFinishTime() => _finishTime;

        /// <summary>
        /// Получить время последнего обращения к единице учета
        /// </summary>
        /// <returns></returns>
        public DateTime GetAccessTime() => _accessTime;

        /// <summary>
        /// Получить наименование последнего загруженного материала
        /// </summary>
        /// <returns></returns>
        public string GetLastMaterialName() => _materials[_materials.Count].Name;

        /// <summary>
        /// Получить список материалов
        /// </summary>
        /// <returns></returns>
        public List<Material> GetMaterials() => _materials;

        /// <summary>
        /// Получить материал по его номеру
        /// </summary>
        /// <param name="num">Номер загруженного материала</param>
        /// <returns>Загруженный материал</returns>
        public Material GetMaterial(int num)
        {
            Material res = null;
            if (num > 0 && num < _materials.Count)
            {
                res = _materials[num - 1];
            }

            return res;
        }

        /// <summary>
        /// Получить количество слоев материала
        /// </summary>
        /// <returns>Количество слоев материала</returns>
        public int GetMaterialsCount() => _materials.Count;

    }
}