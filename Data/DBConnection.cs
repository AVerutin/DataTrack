using System;
using NLog;
using Npgsql;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Collections.Generic;

namespace DataTrack.Data
{
    public class DBConnection
    {
        private readonly NpgsqlConnection Connection;
        private readonly DBConnectionOptions options;
        private readonly string ConnectionString;
        private NpgsqlCommand SQLCommand;
        private NpgsqlDataReader SQLData;
        private string DBSchema;
        private readonly IConfigurationRoot config;
        private readonly Logger logger;

        /// <summary>
        /// Конструктор создания подключения к базе данных
        /// </summary>
        /// <param name="options">Параметры подключения к базе данных</param>
        public DBConnection()
        {
            // Читаем параметры подключения к СУБД PostgreSQL
            logger = LogManager.GetCurrentClassLogger();
            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            options = new DBConnectionOptions(config);
            // string db_host = config.GetSection("PGSQL:DBHost").Value;
            // string db_port = config.GetSection("PGSQL:DBPort").Value;
            // string db_name = config.GetSection("PGSQL:DBName").Value;
            // string db_schema = config.GetSection("PGSQL:DBSchema").Value;
            // string db_user = config.GetSection("PGSQL:DBUser").Value;
            // string db_pass = config.GetSection("PGSQL:DBPass").Value;

            ConnectionString =
                $"Server={options.DBHost};Username={options.DBUser};Database={options.DBName};Port={options.DBPort};Password={options.DBPass}"; //";SSLMode=Prefer";

            try
            {
                Connection = new NpgsqlConnection(ConnectionString);
            }
            catch (Exception e)
            {
                logger.Error($"Не удалось подключиться к БД [{e.Message}]");
                throw new DataException($"Ошибка при подключении к базе данных: [{e.Message}]");
            }

            SQLCommand = null;
            SQLData = null;
            DBSchema = options.DBSchema;
        }

        /// <summary>
        /// Добавление наименование материала в справочник
        /// </summary>
        /// <param name="name">Наименование материала</param>
        /// <returns>Номер ID добавленной записи, или -1 в случае ошибки</returns>
        public int AddMaterialToCollection(string name)
        {
            int Result = -1;
            if (name == "" || Connection == null)
            {
                return Result;
            }

            string sql = $"INSERT INTO {DBSchema}._material(name) VALUES (\'{name}\') RETURNING id;";
            SQLCommand = new NpgsqlCommand(sql, Connection);

            try
            {
                Connection.Open();
                Result = Int32.Parse(SQLCommand.ExecuteScalar().ToString()); //Выполняем нашу команду.
                Connection.Close();
            }
            catch (Exception e)
            {
                logger.Error($"Ошибка при добавлении материала [{name}] в базу данных: {e.Message}");
            }

            return Result;
        }

        /// <summary>
        /// Добавить новый материал в таблицу БД
        /// </summary>
        /// <param name="material">Экземпляр объекта Material</param>
        /// <returns>Результат добавления материала в таблицу (true - успех)</returns>
        public bool AddMaterial(Material material)
        {
            bool Result;

            if (Connection != null)
            {
                string name = material.getName();
                int partno = material.getPartNo();
                double weight = material.getWeight();
                string w = weight.ToString();
                w = w.Replace(',', '.');
                double volume = material.getVolume();
                string v = volume.ToString();
                v = v.Replace(',', '.');

                string query = string.Format(
                    "INSERT INTO {0}._materials (name, partno, weight, volume) VALUES ('{1}', {2}, {3}, {4});",
                    DBSchema, name, partno, w, v);

                SQLCommand = new NpgsqlCommand(query, Connection);

                try
                {
                    Connection.Open();
                    SQLCommand.ExecuteNonQuery();
                    Connection.Close();
                    Result = true;
                }
                catch (Exception e)
                {
                    logger.Error("Не удалось добавить новый материал {0}: {1}", material.getName(), e.Message);
                    Result = false;
                }
            }
            else
            {
                Result = false;
            }

            return Result;
        }


        public bool WriteData(string query)
        {
            bool Result = false;

            try
            {
                NpgsqlCommand command = new NpgsqlCommand(query, Connection);
                Connection.Open();
                command.ExecuteNonQuery();
                Connection.Close();
                Result = true;
            }
            catch (Exception e)
            {
                logger.Error($"Не удалось записать данные в базу данных: [{query}] = {e.Message}");
            }

            return Result;
        }

        /// <summary>
        /// Получить таблицу из БД по переданному SQL-запросу
        /// </summary>
        /// <param name="query">SQL-запрос к БД</param>
        /// <returns>Массив object[] представленный набором строк таблицы</returns>
        private object[] getData(string query)
        {
            object[] _table = null;

            if (Connection != null)
            {
                NpgsqlCommand command = new NpgsqlCommand(query, Connection)
                {
                    CommandTimeout = 20
                };
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(command);
                DataSet ds = new DataSet();
                da.Fill(ds, $"{DBSchema}._materials");

                // Перебор таблиц из результирующего набора
                foreach (DataTable table in ds.Tables)
                {
                    _table = new object[table.Rows.Count];
                    int __row = 0;

                    // Перебор строк в таблице
                    foreach (DataRow row in table.Rows)
                    {
                        var cells = row.ItemArray;
                        _table[__row++] = cells;
                    }
                }
            }

            return _table;
        }

        /// <summary>
        /// Получить материал из таблицы БД
        /// </summary>
        /// <returns>Экземпляр класса Material</returns>
        public List<Material> GetMaterials(/*string material*/)
        {
            List<Material> Result = new List<Material>();

            if (Connection != null)
            {
                // Полчение прихода и расхода материалов

                string query = "SELECT " +
                   "pqca.materials.id as material_id, " +                    // 0    
                   "pqca.invoices.id as invoices_id, " +                     // 1
                   "pqca.materials.material as material, " +                 // 2
                   "pqca.invoices.partno as partno, " +                      // 3
                   "COALESCE(pqca.entry.volume, 0) as entry_volume, " +      // 4
                   "COALESCE(pqca.entry.weight, 0) as entry_weight, " +      // 5
                   "COALESCE(pqca.sale.volume, 0) as sale_volume, " +        // 6
                   "COALESCE(pqca.sale.weight, 0) as sale_weight " +         // 7
                   "FROM " +
                   "pqca.materials LEFT JOIN pqca.invoices ON pqca.materials.id = pqca.invoices.material " +
                   "LEFT JOIN pqca.entry ON pqca.invoices.id = pqca.entry.invoice " +
                   "LEFT JOIN pqca.sale ON pqca.invoices.id = pqca.sale.invoice " +
                   "ORDER BY " +
                   "material ASC, " +
                   "partno ASC;";

                // string query = $"SELECT * FROM {DBShema}.tmp_materials ORDER BY name, partno ASC;";

                SQLCommand = new NpgsqlCommand(query, Connection);
                Connection.Open();
                SQLData = SQLCommand.ExecuteReader();

                while (SQLData.Read())
                {
                    Material layer = new Material();
                    List<Chemical> chemicals;

                    long material_id = SQLData.GetInt64(0);
                    long invoice_id = SQLData.GetInt64(1);
                    string material_name = SQLData.GetString(2);
                    int partno = SQLData.GetInt32(3);
                    double entry_volume = SQLData.GetDouble(4);
                    double entry_weight = SQLData.GetDouble(5);
                    double sale_volume = SQLData.GetDouble(6);
                    double sale_weight = SQLData.GetDouble(7);

                    double mat_weight = entry_weight - sale_weight;
                    double mat_volume = entry_volume - sale_volume;

                    if(mat_weight > 0)
                    {
                        chemicals = GetChemicals(material_name);
                        layer.setMaterial(material_id, invoice_id, material_name, partno, mat_weight, mat_volume);
                        layer.AddChemicals(chemicals);
                        Result.Add(layer);
                    }
                }

                Connection.Close();
            }
            else
            {
                Result = null;
            }

            return Result;
        }


        public List<Chemical> GetChemicals(string material_name)
        {
            List<Chemical> chemicals = new List<Chemical>();

            // Запрос на получение химического состава материала по его наименованию
            string query = "SELECT e.id as id, e.name as element, e.sign as sign, c.volume as volume ";
            query += "FROM pqca.materials m, pqca.elements e, pqca.composition c ";
            query += $"WHERE c.material = m.id AND c.element = e.id AND m.material = '{material_name}' "; 
            query += "ORDER BY element;";

            NpgsqlConnection _connection = new NpgsqlConnection(ConnectionString);
            NpgsqlCommand _command = new NpgsqlCommand(query, _connection);
            NpgsqlDataReader _sqlReader;
            
            _connection.Open();
            _sqlReader = _command.ExecuteReader();
            while (_sqlReader.Read())
            {
                long id = _sqlReader.GetInt64(0);
                string name = _sqlReader.GetString(1);
                string sign = _sqlReader.GetString(2);
                double vol = _sqlReader.GetDouble(3);
                Chemical chemical = new Chemical(id, name, sign, vol);
                chemicals.Add(chemical);
            }
            _connection.Close();

            return chemicals;
        }
    }
}