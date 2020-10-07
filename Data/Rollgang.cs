using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataTrack.Pages;

namespace DataTrack.Data
{
    public class Rollgang
    {
        /// <summary>
        /// Уникальный идентификатор рольганга
        /// </summary>
        public int Uid { get; private set; }
        
        /// <summary>
        /// Уникальный идентификатор рольганга в базе данных
        /// </summary>
        public long DbUid { get; private set; }
        
        /// <summary>
        /// Текущая скорость движения рольганга
        /// </summary>
        public double CurrentSpeed { get; set; }
        
        /// <summary>
        /// Координаты начала и конца рольганга
        /// </summary>
        public Coords ScreenStartPos { get; set; }
        public Coords ScreenFinishPos { get; set; }
        public Coords RealStartPos { get; set; }
        public Coords RealFinishPos { get; set; }

        /// <summary>
        /// Номер нити, на которой расположен рольганг 
        /// </summary>
        public int Thread { get; set; }

        /// <summary>
        /// Направление движения рольганга
        /// </summary>
        public Directions Direction { get; set; }
        
        /// <summary>
        /// Тип рольганга - горизонтальный, или вертикальный
        /// </summary>
        public RollgangTypes Type { get; set; }
        
        
        /// <summary>
        /// Список единиц учета, находящихся на рольганге
        /// </summary>
        private readonly List<Ingot> _ingotsList;

        /// <summary>
        /// Метод, вызывающийся при завершении доставки материала через рольганг 
        /// </summary>
        /// <param name="ingot">Материал, который был доставлен рольгангом</param>
        public delegate void OnDelivered(Ingot ingot);

        /// <summary>
        /// Событие, вызывающееся при завершении доставки материала
        /// </summary>
        public event OnDelivered Delivered;

        /// <summary>
        /// Метод, вызывающийся при изменении координат единицы учета
        /// </summary>
        /// <param name="ingot">Единица учета</param>
        public delegate void OnMoved(Ingot ingot);

        /// <summary>
        /// Вызов метода для отображения перемещения единицы учета
        /// </summary>
        public event OnMoved Moved;


        /// <summary>
        /// Конструктор класса рольганга
        /// </summary>
        /// <param name="uid">Уникальный идентификатор рольганга</param>
        /// <param name="type">Тип рольганга</param>
        /// <param name="dbuid">Уникальный идентификатор рольганга в базе данных</param>
        /// <param name="currentSpeed">Скорость движения рольганга в м/с</param>
        /// <param name="thread">Номер нити</param>
        /// <exception cref="ArgumentNullException">Не указан уникальный идентификатор рольганга</exception>
        public Rollgang(int uid, RollgangTypes type, long dbuid=0, double currentSpeed=0.25, int thread=0)
        {
            if (uid > 0)
            {
                Uid = uid;
                DbUid = dbuid;
                CurrentSpeed = currentSpeed;
                ScreenStartPos = new Coords();
                ScreenFinishPos = new Coords();
                RealStartPos = new Coords();
                RealFinishPos = new Coords();
                Thread = thread;
                _ingotsList = new List<Ingot>();
                Direction = Directions.Forward;
                Type = type;
            }
            else
            {
                throw new ArgumentNullException("Не указан номер рольганга!");
            }
        }

        /// <summary>
        /// Установить координаты рольганга
        /// </summary>
        /// <param name="screenStart">Координаты головы рольганга на экране</param>
        /// <param name="screenFinish">Координаты хвоста рольганга на экране</param>
        /// <param name="realStart">Координаты головы рольганга физические</param>
        /// <param name="realFinish">Координаты хвоста рольганга физические</param>
        public void SetPosition(Coords screenStart, Coords screenFinish, Coords realStart, Coords realFinish)
        {
            if (screenStart != null && screenFinish != null && realStart != null && realFinish != null)
            {
                ScreenStartPos = screenStart;
                ScreenFinishPos = screenFinish;
                RealStartPos = realStart;
                RealFinishPos = realFinish;
            }
        }

        /// <summary>
        /// Добавить единицу учета на рольганг
        /// </summary>
        /// <param name="ingot">Единица учета</param>
        public void Push(Ingot ingot)
        {
            if (ingot != null)
            {
                // DateTime current = DateTime.Now;
                // ingot.SetStartTime(current);
                _ingotsList.Add(ingot);
            }
        }

        /// <summary>
        /// Получить единицу учета с рольганга
        /// </summary>
        /// <returns></returns>
        public Ingot Pop()
        {
            // List<Ingot> _ingots = new List<Ingot>();
            Ingot first = _ingotsList[0];
            _ingotsList.Remove(first);
            // DateTime current = DateTime.Now;
            // first.SetFinishTime(current);

            return first;
        }

        /// <summary>
        /// Получить единицу учета по ее номеру
        /// </summary>
        /// <param name="number">Номер единицы учета</param>
        /// <returns>Единица учета</returns>
        public Ingot GetIngot(int number)
        {
            Ingot result = null;
            if (number > 0 && number <= GetIngotsCount())
            {
                result = _ingotsList[number - 1];
            }

            return result;
        }

        /// <summary>
        /// Получить рассчетное время доставки единицы учета рольгангом 
        /// </summary>
        /// <returns>Рассчетное время доставки единицы учета</returns>
        public double GetDeliveringTime()
        {
            double deliveringTime;
            double rollgangLength = 0;
            
            switch (Type)
            {
                case RollgangTypes.Horizontal:
                {
                    rollgangLength = RealFinishPos.PosX - RealStartPos.PosX;
                    break;
                }
                case RollgangTypes.Vertical:
                {
                    rollgangLength = RealFinishPos.PosY - RealStartPos.PosY;
                    break;
                }
            }

            deliveringTime = rollgangLength / CurrentSpeed;
            return deliveringTime;
        }

        /// <summary>
        /// Получить координаты единицы учета по ее номеру
        /// </summary>
        /// <param name="number">Номер единицы учета</param>
        /// <returns>Текущие координаты единицы учета</returns>
        public Coords GetIngotPosition(TimeSpan movingTime)
        {
            // Расчитать текущие координаты единицы учета исходя из времени движения единицы учета,
            // скорости движения и длины рольганга.
            Coords newPos = new Coords();
            newPos.PosX = ScreenStartPos.PosX;
            newPos.PosY = ScreenStartPos.PosY;

            // Рассчитываем время движения единицы учета по рольгангу
            switch (Type)
            {
                case RollgangTypes.Horizontal:
                {
                    double rollgangLength = RealFinishPos.PosX - RealStartPos.PosX;
                    double realX = CurrentSpeed * movingTime.TotalSeconds;
                    double percentX = realX * 100 / rollgangLength;
                    int screenX = (int) Math.Round((ScreenFinishPos.PosX - ScreenStartPos.PosX) * percentX / 100);
                    newPos.PosX += screenX;
                    break;
                }
                case RollgangTypes.Vertical:
                {
                    double rollgangLength = RealFinishPos.PosY - RealStartPos.PosY;
                    double realY = CurrentSpeed * movingTime.TotalSeconds;
                    double percentY = realY * 100 / rollgangLength;
                    int screenY = (int) Math.Abs(Math.Round((ScreenFinishPos.PosY - ScreenStartPos.PosY) * percentY / 100));
                    newPos.PosY -= screenY;
                    break;
                }
            }
                
            return newPos;
        }

        /// <summary>
        /// Добаввить единицу учета в список
        /// </summary>
        public void AddIngot(Ingot ingot)
        {
            if (ingot != null)
            {
                _ingotsList.Add(ingot);
            }
        }

        /// <summary>
        /// Получить количество единиц учета на рольганге
        /// </summary>
        /// <returns>Количество единиц учета</returns>
        public int GetIngotsCount() => _ingotsList.Count;

        /// <summary>
        /// Удалить единицу учета с рольганга
        /// </summary>
        /// <param name="ingot"></param>
        /// <returns>Удаленная с рольганга единица учета</returns>
        public Ingot RemoveIngot(Ingot ingot)
        {
            Ingot res = null;
            for (int i = 0; i < _ingotsList.Count; i++)
            {
                if (_ingotsList[i].Uid == ingot.Uid)
                {
                    res = _ingotsList[i];
                    _ingotsList.RemoveAt(i);
                }
            }

            return res;
        }

        /// <summary>
        /// Удалить все единицы учета с рольганга
        /// </summary>
        public void ClearIngots()
        {
            _ingotsList.Clear();
        }

        /// <summary>
        /// Доставить материал через рольганг
        /// </summary>
        /// <param name="ingot">Доставляемый материал</param>
        public async Task Delivering(Ingot ingot)
        {
            // Установка изображения единицы учета в соотвествии с типом рольганга
            AddIngot(ingot);
            string color;
            if (Type == RollgangTypes.Horizontal)
            {
                color = "img/colors/";
            }
            else
            {
                color = "img/colors/v";
            }

            color += ingot.Color + ".png";
            ingot.VisualParameters.FileName = color;

            // Расчитываем время движения единицы учета по рольгангу
            // _deliveringTime - время полного перемещения единицы учета по рольгангу
            DateTime startDelivering = ingot.GetAccessTime();
            if (startDelivering == default)
            {
                startDelivering = DateTime.Now;
                ingot.SetAccessTime(startDelivering);
            }

            // Расчитать время, прошедшее с начала транспортировки материала
            TimeSpan movingTime = DateTime.Now - startDelivering;
            
            // Пока время перемещения не достигнуто
            double deliveringTime = GetDeliveringTime();
            while (movingTime.TotalSeconds < deliveringTime)
            {
                // Расчитываем новые координаты для единицы учета и выводим ее позицию
                Coords newCoords = GetIngotPosition(movingTime);
                ingot.VisualParameters.XPos = newCoords.PosX + "px";
                ingot.VisualParameters.YPos = newCoords.PosY + "px";
                
                // Ждем одну секунду
                await Task.Delay(TimeSpan.FromSeconds(1));
                movingTime = DateTime.Now - startDelivering;
                Moved?.Invoke(ingot);
            }
            
            // Сообщаем о завершении доставки материала
            ingot.SetAccessTime(default);
            RemoveIngot(ingot);
            Delivered?.Invoke(ingot);
        }
    }
}