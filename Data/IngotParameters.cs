using System.Collections.Generic;

namespace DataTrack.Data
{
    public class IngotParameters
    {
        private Dictionary<int, double> _params1;
        private Dictionary<int, string> _params2;

        public IngotParameters()
        {
            _params1 = new Dictionary<int, double>();
            _params2 = new Dictionary<int, string>();
        }

        /// <summary>
        /// Получить список строковых параметров едины учета
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, string> GetStringParameters()
        {
            return _params2;
        }

        /// <summary>
        /// Получить список значащих параметров единицы учета
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, double> GetDoubleParameters()
        {
            return _params1;
        }

        /// <summary>
        /// Добавить строковый параметр
        /// </summary>
        /// <param name="key">Идентификатор параметра</param>
        /// <param name="value">Значение параметра</param>
        /// <returns>Список строковых параметров с добавленным новым параметром</returns>
        public Dictionary<int, string> AddStringParameter(int key, string value)
        {
            if (key > 0 && value.Trim() != "")
            {
                _params2.Add(key, value);
            }

            return _params2;
        }

        /// <summary>
        /// Добавить значащий параметр
        /// </summary>
        /// <param name="key">Идентификатор параметра</param>
        /// <param name="value">Значение параметра</param>
        /// <returns></returns>
        public Dictionary<int, double> AddDoubleParameter(int key, double value)
        {
            if (key > 0 && value > 0)
            {
                _params1.Add(key, value);
            }

            return _params1;
        }

        /// <summary>
        /// Получить общее количество параметров единицы учета
        /// </summary>
        /// <returns>Количество параметров единицы учета</returns>
        public int GetParametersCount()
        {
            int result = 0;
            result += _params1.Count;
            result += _params2.Count;

            return result;
        }

        /// <summary>
        /// Удалить строковый параметр по его идентификатору
        /// </summary>
        /// <param name="key">Идентификатор параметра</param>
        /// <returns>Результат выполнения операции удаления</returns>
        public bool RemoveStringParameter(int key)
        {
            bool result = false;

            if (_params2.ContainsKey(key))
            {
                result = _params2.Remove(key);
            }

            return result;
        }

        /// <summary>
        /// Удалить значащий параметр по его идентификатору
        /// </summary>
        /// <param name="key">Идентификатор значащего параметра</param>
        /// <returns>Результат операции удаления</returns>
        public bool RemoveDoubleParameter(int key)
        {
            bool result = false;

            if (_params1.ContainsKey(key))
            {
                result = _params1.Remove(key);
            }

            return result;
        }

        /// <summary>
        /// Удалить все параметры единицы учета
        /// </summary>
        public void RemoveAllParameters()
        {
            _params1.Clear();
            _params2.Clear();
        }

        /// <summary>
        /// Поиск параметров по их идентификатору
        /// </summary>
        /// <param name="key">Идентификатор параметра</param>
        /// <returns>Параметры единицы учета</returns>
        public IngotParameters FindParameterById(int key)
        {
            IngotParameters result = new IngotParameters();
            if (_params1.ContainsKey(key))
            {
                result.AddDoubleParameter(key, _params1[key]);
            }

            if (_params2.ContainsKey(key))
            {
                result.AddStringParameter(key, _params2[key]);
            }
            
            return result;
        }

        /// <summary>
        /// Определить наличие параметра единицы учета по его идентификатору
        /// </summary>
        /// <param name="key">Идентификатор параметра</param>
        /// <returns>Признак наличия параметра в списке</returns>
        public bool Exists(int key)
        {
            bool result = _params1.ContainsKey(key) || _params2.ContainsKey(key);

            return result;
        }

        /// <summary>
        /// Установить новые параметры единицы учета 
        /// </summary>
        /// <param name="parameters">Новые параметра единицы учета</param>
        public void SetParameters(IngotParameters parameters)
        {
            _params1 = parameters.GetDoubleParameters();
            _params2 = parameters.GetStringParameters();
        }
    }
}