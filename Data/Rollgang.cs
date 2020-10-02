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
        /// Длина рольганга
        /// </summary>
        public double RollgangLength { get; set; }
        
        /// <summary>
        /// Координаты начала и конца рольганга
        /// </summary>
        public Coords StartPos { get; set; }
        public Coords FinishPos { get; set; }

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

        private double _deliveringTime;
        
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
        /// <param name="dbuid">Уникальный идентификатор рольганга в базе данных</param>
        /// <param name="currentSpeed">Скорость движения рольганга в м/с</param>
        /// <param name="length">Длина рольганга в метрах</param>
        /// <param name="thread">Номер нити</param>
        /// <exception cref="ArgumentNullException">Не указан уникальный идентификатор рольганга</exception>
        public Rollgang(int uid, long dbuid=0, double currentSpeed=0.25, double length=5, int thread=0)
        {
            if (uid > 0)
            {
                Uid = uid;
                DbUid = dbuid;
                CurrentSpeed = currentSpeed;
                RollgangLength = length;
                StartPos = new Coords();
                FinishPos = new Coords();
                Thread = thread;
                _ingotsList = new List<Ingot>();
                Direction = Directions.Forward;
                try
                {
                    _deliveringTime = RollgangLength / CurrentSpeed;
                }
                catch (DivideByZeroException)
                {
                    _deliveringTime = RollgangLength / 0.25;
                }
            }
            else
            {
                throw new ArgumentNullException("Не указан номер рольганга!");
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
        /// Получить количество единиц учета на рольганге
        /// </summary>
        /// <returns></returns>
        public int IngotsCount() => _ingotsList.Count;

        /// <summary>
        /// Получить единицу учета по ее номеру
        /// </summary>
        /// <param name="number">Номер единицы учета</param>
        /// <returns>Единица учета</returns>
        public Ingot GetIngot(int number)
        {
            Ingot result = null;
            if (number > 0 && number < IngotsCount())
            {
                result = _ingotsList[number - 1];
            }

            return result;
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
            
           Coords pos = new Coords();
           pos.PosX = 400;
           pos.PosY = 510;
            
            // Рассчитываем время движения единицы учета по рольгангу
           switch (Type)
            {
                case RollgangTypes.Horizontal:
                {
                    double realX = CurrentSpeed * movingTime.TotalSeconds;
                    double percentX = realX * 100 / RollgangLength;
                    int screenX = (int)Math.Round(440 * percentX / 100);
                    pos.PosX += screenX;
                    break;
                }
                case RollgangTypes.Vertical:
                {
                    double realY = CurrentSpeed * movingTime.TotalSeconds;
                    double percentY = realY * 100 / RollgangLength;
                    int screenY = (int) Math.Round(510 * percentY / 100);
                    pos.PosY += screenY;
                    break;
                }
            }
                
            return pos;
        }
        
        /// <summary>
        /// Доставить материал через рольганг
        /// </summary>
        /// <param name="ingot">Доставляемый материал</param>
        public async Task Delivering(Ingot ingot)
        {
            // Установка изображения единицы учета в соотвествии с типом рольганга
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
            while (movingTime.TotalSeconds < _deliveringTime)
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
            Delivered?.Invoke(ingot);
        }
    }
}