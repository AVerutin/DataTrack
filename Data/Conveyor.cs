using System;
using System.Collections.Generic;
using NLog;
using System.Threading.Tasks;


namespace DataTrack.Data
{
    public class Conveyor
    {
        /// <summary>
        /// Коллекция типов конвейера
        /// </summary>
        public enum Types { Horizontal, Vertical };

        /// <summary>
        /// Коллекция направлений перемещения конвейера
        /// </summary>
        public enum Directions { Stopped, Forward, Backward };

        /// <summary>
        /// Уникальный идентификатор конвейера
        /// </summary>
        public readonly int ConveyorId;

        /// <summary>
        /// Тип конвейера
        /// </summary>
        public readonly Types Type;

        /// <summary>
        /// Текущее состояние конвейера
        /// </summary>
        public Statuses.Status Status { get; private set; }

        /// <summary>
        /// Направление движения конвейера
        /// </summary>
        public Directions Direction { get; private set; }

        /// <summary>
        /// Скорость перемещения материала по конвейеру
        /// </summary>
        public double Speed { get; private set; }

        /// <summary>
        /// Длина ленты койнвейера
        /// </summary>
        public double Length { get; private set; }

        /// <summary>
        /// Количество материала на конвейере
        /// </summary>
        public int MaterialCount { get; private set; }

        /// <summary>
        /// Метод, который будет вызван при окончании доставки материала
        /// </summary>
        /// <param name="material"></param>
        public delegate void Delivered(List<Material> material);  

        private readonly Logger logger;
        Dictionary<ushort, List<Material>> Material;              // Материал и его UID на конвейере

        /// <summary>
        /// Конструктор для создания нового конвейера
        /// </summary>
        /// <param name="number">Уникальный идентификатор конвейера</param>
        /// <param name="type">Тип конвейера</param>
        /// <param name="length">Длина конвейера в метрах</param>
        public Conveyor(int number, Types type, double length)
        {
            logger = LogManager.GetCurrentClassLogger();
            ConveyorId = number;
            Type = type;
            Direction = Directions.Stopped;
            Material = new Dictionary<ushort, List<Material>>();
            Speed = 0;
            Status = Statuses.Status.Off;
            MaterialCount = 0;
            Length = length;
        }

        /// <summary>
        /// Сброс текущего состояния конвейера
        /// </summary>
        public void Reset()
        {
            Direction = Directions.Stopped;
            Material = new Dictionary<ushort, List<Material>>();
            Speed = 0;
            Status = Statuses.Status.Off;
            MaterialCount = 0;
        }

        /// <summary>
        /// Управление направлением перемещения конвейера
        /// </summary>
        /// <param name="direction">Направление перемещения конвейера</param>
        public void SetDirection(Directions direction)
        {
            Direction = direction;
        }

        /// <summary>
        /// Установка скорости перемещения материала по конвейеру
        /// </summary>
        /// <param name="speed"></param>
        public void SetSpeed(double speed)
        {
            if (speed > 0)
            {
                Speed = speed;
            }
            else
            {
                logger.Error($"Скорость перемещения материала по конвейеру {ConveyorId} должна быть больше нуля");
                throw new ArgumentNullException($"Скорость перемещения материала по конвейеру {ConveyorId} должна быть больше нуля");
            }
        }

        /// <summary>
        /// Установка текущего состояния конвейера
        /// </summary>
        /// <param name="status"></param>
        public void SetStatus(Statuses.Status status)
        {
            Status = status;
        }

        /// <summary>
        /// Добавление материала на конвейер для транспортировки
        /// </summary>
        /// <param name="material"></param>
        async public void Deliver(List<Material> material, Delivered ondelivered)
        {
            Status = Statuses.Status.Delivering;
            if (material != null)
            {
                // Расчитать время, необходимое на доставку материала до конца конвейера по формуле T=S/V
                // Запусить метод с задержкой на указанное время и передать ему материал, который требуется доставить
                // и внешний метод обратного вызова, который требуется вызвать при завершении доставки материала

                /* Добавление материала на конвейер:
                 *      - Найти следующий свободный UID для материала на ленте
                 *      - Добавить в словарь материал и его UID
                 *      - вызвать метод доставки передав ему UID материала на ленте 
                 */

                ushort uid = GetNextUID();
                Material.Add(uid, material);
                MaterialCount = Material.Count;

                await OnDelivered(uid, ondelivered);
            }
            else
            {
                logger.Error($"Материал для загрузки на конвейер {ConveyorId} не может быть NULL");
                throw new ArgumentNullException($"Материал для загрузки на конвейер {ConveyorId} не может быть NULL");
            }
        }

        /// <summary>
        /// Получение следующего UID для материала на конвейере
        /// </summary>
        /// <returns></returns>
        private ushort GetNextUID()
        {
            ushort uid = 1;

            // Если на ленте есть материал
            if (MaterialCount > 0)
            {
                foreach (KeyValuePair<ushort, List<Material>> material in Material)
                {
                    // Находим материал с наибольшим UID
                    if (material.Key > uid)
                    {
                        uid = material.Key;
                    }
                }

                // Если UID достиг максимального значения своего типа данных, то начинаем нумерацию сначала
                if (uid == ushort.MaxValue)
                {
                    uid = 1;
                }
                else
                {
                    uid++;
                }
            }

            return uid;
        }


        /// <summary>
        /// Метод, осуществляет доставку материала до конца конвейера и вызывает внешний метод при завершении доставки
        /// </summary>
        private async Task OnDelivered(ushort uid, Delivered delivered)
        {
            if (Speed == 0)
            {
                logger.Error($"Не установлена скорость перемещения для конвейера {ConveyorId}");
                throw new DivideByZeroException($"Не установлена скорость перемещения для конвейера {ConveyorId}");
            }

            List<Material> material = new List<Material>();
            foreach(KeyValuePair<ushort, List<Material>> _material in Material)
            {
                if (_material.Key == uid)
                {
                    material = _material.Value;
                    break;
                }
            }

            double delay = (Math.Round(Length / Speed, 0)) * 1000;
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
            delivered?.Invoke(material);

            Material.Remove(uid);
            MaterialCount = Material.Count;
            Status = Statuses.Status.Delivered;
        }
    }
}