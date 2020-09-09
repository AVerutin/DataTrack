using Microsoft.Extensions.Configuration;

namespace DataTrack.Data
{
    public class DBConnectionOptions
    {
        /// <summary>
        /// Адрес сервера СУБД
        /// </summary>
        public string DBHost { get; }
        /// <summary>
        /// Порт для подключения к СУБД
        /// </summary>
        public ushort DBPort { get; }

        /// <summary>
        /// Имя базы данных
        /// </summary>
        public string DBName { get; }

        /// <summary>
        /// Имя схемы в базе данных
        /// </summary>
        public string DBSchema { get; }

        /// <summary>
        /// Имя пользователя СУБД
        /// </summary>
        public string DBUser { get; }

        /// <summary>
        /// Пароль для подключения к СУБД
        /// </summary>
        public string DBPass { get; }

        public DBConnectionOptions(IConfigurationRoot config)
        {
            DBHost = config.GetSection("PGSQL:DBHost").Value;
            DBPort = ushort.Parse(config.GetSection("PGSQL:DBPort").Value);
            DBName = config.GetSection("PGSQL:DBName").Value;
            DBUser = config.GetSection("PGSQL:DBUser").Value;
            DBPass = config.GetSection("PGSQL:DBPass").Value;
            DBSchema = config.GetSection("PGSQL:DBSchema").Value;
        }
    }
}