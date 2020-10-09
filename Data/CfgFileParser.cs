using System.Collections.Generic;
using System.IO;

namespace DataTrack.Data
{
    public class CfgFileParser
    {
        private const string CfgFileName = @"c:\mts\Config\RollingMillConfig.txt";
        private readonly List<CfgFileObjects> _objects = new List<CfgFileObjects>();
        private readonly int[] _objectsCount = {0, 0, 0, 0, 0, 0, 0, 0};
        private readonly string[] _objectsNames = new string[8];
        private readonly CfgFileObjectTypes[] _objectsTypes = new CfgFileObjectTypes[8];
        private int _currentObjectNumber;
        private string _line; 
        private CfgFileObjects _currObject = new CfgFileObjects();

        /// <summary>
        /// Получить список объектов конфигурационного файла
        /// </summary>
        /// <returns>Список объектов конфигурационного файла</returns>
        public List<CfgFileObjects> GetObjects()
        {
            Parse();
            return _objects;
        }
        
        /// <summary>
        /// Разбор конфигурационного файла на объекты
        /// </summary>
        private void Parse()
        {
            using (StreamReader reader = new StreamReader(CfgFileName, System.Text.Encoding.Default))
            {
                while ((_line = reader.ReadLine()) != null)
                {
                    switch (_line.Trim())
                    {
                        case "":
                            continue;
                        case "(":
                            _currObject = new CfgFileObjects();
                            PushObject();
                            break;
                        case ")":
                            _currObject.Name = _objectsNames[_currentObjectNumber];
                            _currObject.Type = _objectsTypes[_currentObjectNumber];
                            _objects.Add(_currObject);
                            PopObject();
                            break;
                    }

                    if (_line.Trim().StartsWith("//"))
                    {
                        continue;
                    }

                    if (!_line.Contains("="))
                    {
                        switch (_line.Trim().ToUpper())
                        {
                            case "ОБЩИЕПАРАМЕТРЫСТАНА":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.MillConfig;
                                break;
                            case "СИГНАЛ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.Signal;
                                break;
                            case "НИТЬ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.Thread;
                                break;
                            case "ДАТЧИК":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.Sensor;
                                break;
                            case "РОЛЬГАНГ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.Rollgang;
                                break;
                            case "ПОДПИСКИ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.Subscription;
                                break;
                            case "БЛОКДАННЫХ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.DataBlock;
                                break;
                            case "МЕТКА":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.Label;
                                break;
                            case "УПОР":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.Stopper;
                                break;
                            case "АГРЕГАТЛИНЕЙНОГОПЕРЕМЕЩЕНИЯ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.LinearMoving;
                                break;
                            case "АГРЕГАТШАГОВОГОПЕРЕМЕЩЕНИЯ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.StepperMoving;
                                break;
                            case "УДАЛЕНИЕЗАСТРЯВШИХ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.Deleter;
                                break;
                            case "КЛЕТЬ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.Cage;
                                break;
                            case "ТЕХУЗЕЛ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.TechnicalUnit;
                                break;
                            case "АГРЕГАТ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.Agregate;
                                break;
                            case "ПАРАМЕТРЫЕУ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.IngotParams;
                                break;
                            case "ПОДКЛЮЧЕНИЕ":
                                _objectsNames[_currentObjectNumber] = _line.Trim();
                                _objectsTypes[_currentObjectNumber] = CfgFileObjectTypes.Connection;
                                break;
                        }
                    }
                    else
                    {
                        string[] par = _line.Split("=");
                        _currObject.Parameters[par[0]] = par[1];
                    }
                }
            }
        }

        /// <summary>
        /// Получить количество объектов
        /// </summary>
        /// <returns></returns>
        private int GetCurrentObjectNumber()
        {
            int res = 0;
            for (int i=0; i<_objectsCount.Length;i++)
            {
                if (_objectsCount[i] == 1)
                    res = i;
            }

            return res;
        }

        /// <summary>
        /// Добавить объект
        /// </summary>
        /// <returns>Количество объектов</returns>
        private void PushObject()
        {
            for (int i = 0; i < _objectsCount.Length; i++)
            {
                if (_objectsCount[i] == 0)
                {
                    _objectsCount[i] = 1;
                    break;
                }
            }

            _currentObjectNumber = GetCurrentObjectNumber();
        }

        /// <summary>
        /// Удалить объект
        /// </summary>
        /// <returns>Количество объектов</returns>
        private void PopObject()
        {
            for (int i = _objectsCount.Length - 1; i >= 0; i--)
            {
                if (_objectsCount[i] == 1)
                {
                    _objectsCount[i] = 0;
                    break;
                }
            }

            _currentObjectNumber = GetCurrentObjectNumber();
            if (_currentObjectNumber < 0)
            {
                _currentObjectNumber = 0;
            }
        }

    }
}