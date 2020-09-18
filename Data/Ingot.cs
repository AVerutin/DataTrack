using System;
using System.Collections.Generic;

namespace DataTrack.Data
{
    public class Ingot : IIngot
    {
        /// <summary>
        /// Уникальный иденттификатор единицы учета
        /// </summary>
        public ulong Uid { get; private set; }
        
        /// <summary>
        /// Идентификатор в таблице единиц учета в базе данных
        /// </summary>
        public long DbUId { get; private set; }
        
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
        public ushort Thread { get; set; }

        /// <summary>
        /// Родитель единицы учета
        /// </summary>
        public ulong Parent { get; private set; }

        /// <summary>
        /// Список дочерних единиц учета
        /// </summary>
        public List<ulong> Children { get; private set; }
        
        /// <summary>
        /// Список параметров единицы учета
        /// </summary>
        private readonly IngotParameters _parameters;

        public Ingot(ulong uid)
        {
            if (uid > 0)
            {
                Uid = uid;
                _parameters = new IngotParameters();
            }
            else
            {
                throw new Exception("Не указан уникальный идентификатор для создаваемой единицы учета");
            }
        }

        public Ingot(ulong uid, ulong parent)
        {
            if (uid > 0 && parent > 0)
            {
                Uid = uid;
                Parent = parent;
                _parameters = new IngotParameters();
            }
            else
            {
                throw new  ArgumentNullException("Не указан уникальный идентификатор для создаваемой единицы учета");
            }
        }

        public void AddParameter(int key, string value)
        {
            _parameters.AddStringParameter(key, value);
        }

        public void AddParameter(int key, double value)
        {
            _parameters.AddDoubleParameter(key, value);
        }

        public void SetDbId(long dbid)
        {
            if (dbid > 0)
            {
                DbUId = dbid;
            }
        }
    }
}