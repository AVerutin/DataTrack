using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DataTrack.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using MtsConnect;
using NLog;

namespace DataTrack.Pages
{
    public partial class Arm1
    {
        private (string value, Task t) _lastNotification;

        // Список материалов, полученный из базы данных
        private List<Material> _materials = new List<Material>();
        
        // Список материала, загруженного в силос (для визуализации на странице)
        private List<Material> _loadedMaterial = new List<Material>(); 
        
        private readonly List<InputTanker> _inputTankers = new List<InputTanker>();
        private readonly List<Silos> _siloses = new List<Silos>();
        // private readonly List<Conveyor> _conveyors = new List<Conveyor>();

        private int _currentMaterial;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IConfigurationRoot _config;
        private Data.MtsConnect _mtsConnect;
        private List<ushort> _signals;
        private readonly DBConnection _db = new DBConnection();
        private readonly ConfigMill _configMill = new ConfigMill();

        private readonly string[] _statuses = new string[10];
        private readonly string[] _selected = new string[10];
        private readonly string[] _loadStatuses = new string[10];
        private int _silosSelected;
        private string _telegaPos;
        private string[] _telegaPositions = new string[8];
        private string[] _conveyors = new string[4];
        private string _detailPosX;
        private string _detailPosY;
        private string _showed = "none";

        // private long num = 0;
        private readonly ManualLoadMaterial _manualLoadMaterial = new ManualLoadMaterial();
        private readonly ManualLoadSilos _manualLoadSilos = new ManualLoadSilos();

        // Обработка события загрузки страницы
        protected override async void OnInitialized()
        {
            // Добавления подписки на события уведомлений
            Notifier.Notify += OnNotify;

            GetMaterials();
            Initialize();
            // await ConnectToMts(); // Подключение к сервису MTS Service
        }

        // Событие при обновлении значения события
        private async Task OnNotify(string value)
        {
            await InvokeAsync(() =>
            {
                _lastNotification.value = value;
            });
            StateHasChanged();
        }

        public void Dispose()
        {
            Notifier.Notify -= OnNotify;
        }

        /// <summary>
        /// Подключение к слубже MTS Service
        /// </summary>
        private async Task ConnectToMts()
        {
            // Получение параметров подключения сервису МТС
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            string mtsIP = _config.GetSection("Mts:Ip").Value;
            int mtsPort = Int32.Parse(_config.GetSection("Mts:Port").Value);
            int mtsTimeout = Int32.Parse(_config.GetSection("Mts:Timeout").Value);
            int mtsReconnect = Int32.Parse(_config.GetSection("Mts:ReconnectTimeout").Value);
            
            // Получение списка сигналов для подписки
            _signals = _configMill.GetSignals();
            
            // Подключение к сервису МТС
            try
            {
                _mtsConnect = new Data.MtsConnect("ARM-1", mtsIP, mtsPort, mtsTimeout, mtsReconnect);
                await _mtsConnect.Subscribe(_signals, NewData);
            }
            catch (Exception e)
            {
                _logger.Error($"Ошибка при подключении к сервису МТС: [{e.Message}]");
            }

            _logger.Info("Подключились MTSService как АРМ1");

        }

        private async void NewData(SubscriptionStateEventArgs e)
        {
            SignalsState diff = e.Diff.Signals;
            if (diff != null)
            {
                foreach (var item in diff)
                {
                    string message = $"[{item.Key}] = {item.Value}";

                    await InvokeAsync(async () =>
                    {
                        await OnNotify(message);
                        StateHasChanged();
                    });

                    CheckSignal(item.Key, item.Value);
                }
            }
        }

        private void CheckSignal(ushort signal, double value)
        {
            switch (signal)
            {
                case 4014: LoadSilos(0, value); break; // Загрузка силоса 1
                case 4015: LoadSilos(1, value); break; // Загрузка силоса 2
                case 4016: LoadSilos(2, value); break; // Загрузка силоса 3
                case 4017: LoadSilos(3, value); break; // Загрузка силоса 4
                case 4018: LoadSilos(4, value); break; // Загрузка силоса 5
                case 4019: LoadSilos(5, value); break; // Загрузка силоса 6
                case 4020: LoadSilos(6, value); break; // Загрузка силоса 7
                case 4021: LoadSilos(7, value); break; // Загрузка силоса 8

                // case 4022: break; // Высыпать весовой бункер 1
                // case 4023: break; // Высыпать весовой бункер 2
                // case 4024: break; // Высыпать весовой бункер 3
                    
                // case 4025: break; // Вес в бункере 1
                // case 4026: break; // Вес в бункере 2
                // case 4027: break; // Вес в бункере 3
                    
                // case 4028: break; // Целевое направление материала  
                // case 4029: break; // Температура плавки      

                case 4038: StartLoadInputTanker(0); break; // Загрузить материал в 1 загрузочный бункер
                case 4039: StartLoadInputTanker(1); break; // Загрузить материал во 2 загрузочный бункер

                case 4040: // Признак конца загрузки первого загрузочного бункера
                {
                    /*
                    * Установить статус первого загрузочного бункера в состояния Selected
                    * Установить статус второго загрузочного бункера в состояние Deselected
                    */
                    FinishLoadInputTanker(0);
                    _inputTankers[0].Selected = true;
                    _inputTankers[1].Selected = false;
                    _logger.Info("Выбран загрузочный бункер 1");
                    Debug.WriteLine("Выбран загрузочный бункер 1");
                    break;
                } 
                case 4041: // Признак конца загрузки второго загрузочного бункера
                {
                    /*
                    * Установить статус первого загрузочного бункера в состояния Selected
                    * Установить статус второго загрузочного бункера в состояние Deselected
                    */
                    FinishLoadInputTanker(1);
                    _inputTankers[0].Selected = false;
                    _inputTankers[1].Selected = true;
                    _logger.Info("Выбран загрузочный бункер 2");
                    Debug.WriteLine("Выбран загрузочный бункер 2");
                    break;
                } 

            }
        }

        // Получить список всем материалов
        private void GetMaterials()
        {
            _materials = _db.GetMaterials();
        }

        // Получить следующий материал из списка всех материалов 
        private Material GetNextMaterial()
        {
            return _materials[_currentMaterial];
        }
        
        // Переместить счетчик следующего загружаемого материала 
        private void MoveNextMaterial()
        {
            if (_currentMaterial >= _materials.Count)
            {
                _currentMaterial = 0;
            }
            else
            {
                _currentMaterial++;
            }
        }

        // Начальная инициализация компонентов АРМ 1
        private void Initialize()
        {
            InputTanker _tanker;
            Silos _silos;
            // Conveyor _conveyor;

            // Добавляем загрузочные бункера
            for (int i = 1; i < 3; i++)
            {
                _tanker = new InputTanker(i);
                // Время загрузки материала в загрузочный бункер производится 10 секунд
                _tanker.SetTimeLoading(0);
                _inputTankers.Add(_tanker);
            }

            for (int i = 1; i < 9; i++)
            {
                _silos = new Silos(i);
                _siloses.Add(_silos);
            }

            /*
            _conveyor = new Conveyor(1, Conveyor.Types.Horizontal, 5);
            _conveyors.Add(_conveyor);
            _conveyor = new Conveyor(2, Conveyor.Types.Vertical, 25);
            _conveyors.Add(_conveyor);
            _conveyor = new Conveyor(3, Conveyor.Types.Horizontal, 15);
            _conveyors.Add(_conveyor);
            */

            _conveyors[0] = "img/arm1/Elevator2Grey.png";
            _conveyors[1] = "img/arm1/Elevator3Grey.png";
            _conveyors[2] = "img/arm1/Elevator2Grey.png";
            _conveyors[3] = "img/arm1/TelegaGrey.png";

            _statuses[0] = "img/arm1/led/SquareGrey.png";
            _statuses[1] = "img/arm1/led/SquareGrey.png";
            _statuses[2] = "img/arm1/led/SmallGrey.png";
            _statuses[3] = "img/arm1/led/SmallGrey.png";
            _statuses[4] = "img/arm1/led/SmallGrey.png";
            _statuses[5] = "img/arm1/led/SmallGrey.png";
            _statuses[6] = "img/arm1/led/SmallGrey.png";
            _statuses[7] = "img/arm1/led/SmallGrey.png";
            _statuses[8] = "img/arm1/led/SmallGrey.png";
            _statuses[9] = "img/arm1/led/SmallGrey.png";

            _selected[0] = "img/arm1/led/LedGrey.png";
            _selected[1] = "img/arm1/led/LedGrey.png";
            _selected[2] = "img/arm1/led/LedGrey.png";
            _selected[3] = "img/arm1/led/LedGrey.png";
            _selected[4] = "img/arm1/led/LedGrey.png";
            _selected[5] = "img/arm1/led/LedGrey.png";
            _selected[6] = "img/arm1/led/LedGrey.png";
            _selected[7] = "img/arm1/led/LedGrey.png";

            _telegaPositions[0] = "670px";
            _telegaPositions[1] = "770px";
            _telegaPositions[2] = "870px";
            _telegaPositions[3] = "970px";
            _telegaPositions[4] = "770px";
            _telegaPositions[5] = "870px";
            _telegaPositions[6] = "970px";
            _telegaPositions[7] = "1070px";
        }

        private void ShowMaterial(MouseEventArgs e, int number)
        {
            int matCount = 0;
            switch (number)
            {
                case 1:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _inputTankers[0].GetLayersCount();
                    _loadedMaterial = _inputTankers[0].GetMaterials();
                    break;
                }
                case 2:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _inputTankers[1].GetLayersCount();
                    _loadedMaterial = _inputTankers[1].GetMaterials();
                    break;
                }
                case 3:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    // _telegaPos = _telegaPositions[0];
                    matCount = _siloses[0].GetLayersCount();
                    _loadedMaterial = _siloses[0].GetMaterials();
                    break;
                }
                case 4:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    // _telegaPos = _telegaPositions[1];
                    matCount = _siloses[1].GetLayersCount();
                    _loadedMaterial = _siloses[1].GetMaterials();
                    break;
                }
                case 5:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    // _telegaPos = _telegaPositions[2];
                    matCount = _siloses[2].GetLayersCount();
                    _loadedMaterial = _siloses[2].GetMaterials();
                    break;
                }
                case 6:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    // _telegaPos = _telegaPositions[3];
                    matCount = _siloses[3].GetLayersCount();
                    _loadedMaterial = _siloses[3].GetMaterials();
                    break;
                }
                case 7:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    // _telegaPos = _telegaPositions[4];
                    matCount = _siloses[4].GetLayersCount();
                    _loadedMaterial = _siloses[4].GetMaterials();
                    break;
                }
                case 8:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    // _telegaPos = _telegaPositions[5];
                    matCount = _siloses[5].GetLayersCount();
                    _loadedMaterial = _siloses[5].GetMaterials();
                    break;
                }
                case 9:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    // _telegaPos = _telegaPositions[6];
                    matCount = _siloses[6].GetLayersCount();
                    _loadedMaterial = _siloses[6].GetMaterials();
                    break;
                }
                case 10:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    // _telegaPos = _telegaPositions[7];
                    matCount = _siloses[7].GetLayersCount();
                    _loadedMaterial = _siloses[7].GetMaterials();
                    break;
                }
            }

            _silosSelected = number;
            if (matCount > 0)
            {
                _showed = "inherit";
            }
            else
            {
                _showed = "none";
            }
        }

        private void HideMaterial()
        {
            _silosSelected = 0;
            _showed = "none";
        }

        private void ResetInputTank(int number)
        {
            _inputTankers[number].Reset();
            _statuses[number] = "/img/arm1/led/SquareGrey.png";
            _loadStatuses[number] = "";
            _logger.Warn($"Был произведен сброс загрузочного бункера [{number}]!");
            Debug.WriteLine($"Был произведен сброс загрузочного бункера [{number}]!");
        }

        /// <summary>
        /// Завершение загрузки материала в загрузочный бункер
        /// </summary>
        /// <param name="number">Номер загрузочного бункера</param>
        private void FinishLoadInputTanker(int number)
        {
            _loadStatuses[number] = "";
            _inputTankers[number].SetStatus(Statuses.Status.Off);
            _statuses[number] = "img/arm1/led/SquareGrey.png";
        }

        /// <summary>
        /// Загрузка материала в загрузочный бункер
        /// </summary>
        /// <param name="number">Номер загрузочного бункера, в который следует загрузить материал</param>
        /// <param name="value">Значение сигнала от MTS Service</param>
        private void StartLoadInputTanker(int number)
        {
            /*
             * 1. Получить следующий загружаемый материал из списка
             * 2. Если бункер не содержит материал, то загрузить полученный материал и установить задержку на время загрузки бункера
             * 3. Если бункер содержит материал, то проверить наименование загружаемого материала. Если оно совпадает
             *     наименовавнием загруженного материала, то загрузить материал, иначе выдать ошибку
             */
            
            // 1. Получаем следующий загружаемый материал
            int tanker = number;
            Material _material = GetNextMaterial();
            int _layers = _inputTankers[tanker].GetLayersCount();
            Statuses.Status _status = _inputTankers[tanker].GetStatus();
            
            
            // Если загрузочный бункер простаивает и нет ошибки
            if (_status == Statuses.Status.Off)
            {
                // 2. Проверяем, содержит ли загрузочный бункер материал
                if (_layers > 0)
                {
                    string oldName = _inputTankers[tanker].GetMaterialName();
                    string newName = _material.Name;
                    
                    // 3. Проверяем соотвествие загруженного и загружаемого материала
                    if (oldName == newName)
                    {
                        // a) Начинаем загрузку материала в загрузочный бункер №1
                        _inputTankers[tanker].SetStatus(Statuses.Status.Loading);
                        _loadStatuses[tanker] = "ЗАГРУЗКА";
                        _statuses[tanker] = "img/arm1/led/SquareGreen.png";
                        _inputTankers[tanker].Load(_material);
                        MoveNextMaterial();

                        // Нижеследующий код вынесен в обработчик события завершения загрузки бункера
                        // _inputTankers[tanker].SetStatus(Statuses.Status.Off);
                        // _statuses[tanker] = "img/arm1/led/SquareGrey.png";
                    }
                    else
                    {
                        // b) Загружаемый материал не совпадает с загруженным, ошибка
                        _inputTankers[tanker].SetStatus(Statuses.Status.Error);
                        _loadStatuses[tanker] = "ОШИБКА";
                        _statuses[tanker] = "img/arm1/led/SquareRed.png";
                        _logger.Error(
                            $"Попытка загрузить материал {newName} в загрузочный бункер [1], содержащий материал {oldName}");
                        Debug.WriteLine(
                            $"Попытка загрузить материал {newName} в загрузочный бункер [1], содержащий материал {oldName}");
                    }
                }
                else
                {
                    // Загрузочный бункер пуст, загружаем материал
                    _inputTankers[tanker].SetStatus(Statuses.Status.Loading);
                    _loadStatuses[tanker] = "ЗАГРУЗКА";
                    _statuses[tanker] = "img/arm1/led/SquareGreen.png";
                    _inputTankers[tanker].Load(_material);
                    MoveNextMaterial();

                    // Нижеследующий код вынесен в обработчик события завершения загрузки бункера
                    // _inputTankers[tanker].SetStatus(Statuses.Status.Off);
                    // _statuses[tanker] = "img/arm1/led/SquareGrey.png";
                }
            }
            else
            {
                switch (_status)
                {
                    case Statuses.Status.Error:
                    {
                        // Возникла ошибка при предыдущей попытке загрузки бункера
                        _logger.Error($"Загрузочный бункер 1 находится в состоянии ошибки");
                        Debug.WriteLine($"Загрузочный бункер 1 находится в состоянии ошибки");
                        break;
                    }
                    case Statuses.Status.On:
                    {
                        _logger.Error($"Загрузочный бункер 1 находится в состоянии загрузки! Необходимо дождаться окончания процесса загрузки материала");
                        Debug.WriteLine($"Загрузочный бункер 1 находится в состоянии загрузки! Необходимо дождаться окончания процесса загрузки материала");
                        break;
                    }
                    case Statuses.Status.Loading:
                    {
                        _logger.Error($"В загрузочный бункер {number} уже производится загрузка материала");
                        Debug.WriteLine($"В загрузочный бункер {number} уже производится загрузка материала");
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Управление загрузкой силосов
        /// </summary>
        /// <param name="number">Номер силоса для управления</param>
        /// <param name="value">1 - загрузка силоса начата, 0 - загрузка силоса завершена</param>
        private void LoadSilos(int number, double value)
        {
            switch (value)
            {
                case 1: StartLoadSilos(number); break;
                case 0: FinishLoadSilos(number); break;
            }
        }

        /// <summary>
        /// Начало загрузки материала в силос
        /// </summary>
        /// <param name="silosNumber">Номер силоса для загрузки материала</param>
        private void StartLoadSilos(int silosNumber)
        {
            Debug.WriteLine($"Начата загрузка силоса {silosNumber + 1}");
            _logger.Info($"Начата загрузка силоса {silosNumber + 1}");
            
            /* Алгоритм загрузки силоса
            1. Опрделить текущее состояние силоса:
               Если силос в состоянии ошибки или занят другой операцией, то выход из метода
            2. Установить текущее состояние силоса "Загрузка" 
            3. Определить номер активного загрузочного бункера, из которого будем забирать материал
            4. Определить текущее состояние выбранного загрузочного бункера:
               Если в состоянии ошибки, или занят другой операцией, то выход из метода
            5. Получить наименование материала, загруженного в выбранный загрузочный бункер:
            6. Определить, есть ли в силосе загруженный материал:
               Если да, то сравнить наименование материалов, загруженных в силос и загрузочный бункер
               Если материал в загрузочном бункере не совпадает, установить статус ошибки для силоса и прекратить загрузку
            7. Забрать материал из загрузочного бункера и добавить к слоям материала силоса
            8. Установить текущее состояние силоса "Ожидание"
            */

            Statuses.Status status = _siloses[silosNumber].GetStatus();
            if (status != Statuses.Status.Off)
            {
                _logger.Error($"Силос {silosNumber + 1} занят или находится в состоянии ошибки, загрузка материала невозможна");
                Debug.WriteLine($"Силос {silosNumber + 1} занят или находится в состоянии ошибки, загрузка материала невозможна");
            }
            else
            {
                // Силос готов к приему материала из загрузочного бункера
                _siloses[silosNumber].SetStatus(Statuses.Status.Loading);
                _statuses[silosNumber + 2] = "img/arm1/led/SmallGreen.png";
                _loadStatuses[silosNumber + 2] = "ЗАГРУЗКА";

                int selectedInput = -1;
                if (_inputTankers[0].Selected)
                {
                    selectedInput = 0;
                } else
                {
                    if (_inputTankers[1].Selected)
                    {
                        selectedInput = 1;
                    }
                }
                
                if (selectedInput == -1)
                { 
                    _logger.Error($"Нет выбранного загрузочного бункера для забора материала");
                    Debug.WriteLine($"Нет выбранного загрузочного бункера для забора материала");
                }
                else
                {
                    // Один из загрузочных бункеров выбран
                    if (silosNumber < 4)
                    {
                        _conveyors[3] = "img/arm1/TelegaGreenLeft.png"; 
                    }
                    else
                    {
                        _conveyors[3] = "img/arm1/TelegaGreen.png"; 
                    }
                    
                    _selected[silosNumber] = "img/arm1/led/LedGreen.png";
                    _conveyors[0] = "img/arm1/Elevator2Green.png";
                    _conveyors[1] = "img/arm1/Elevator3Green.png";
                    _conveyors[2] = "img/arm1/Elevator2Green.png";
                    _telegaPos = _telegaPositions[silosNumber];
                }

            }

            // Получаем номер активного загрузочного бункера и загруженный в него материал
            // int tanker;
            //
            // if (InputTankers[0].Selected)
            // {
            //     tanker = 0;
            // }
            // else
            // {
            //     tanker = 1;
            // }

            // Если материал загрузочного бункера и силоса совпадает, или силос пуст
            // if (Siloses[0].Material == "" || _mat.Name == Siloses[0].Material)
            // {
            //     // Производим загрузку материала в силос из активного загрузочного бункера
            //     Siloses[0].Load(InputTankers[tanker]);
            //     _statuses[2] = "img/arm1/led/SmallGreen.png";
            // }

            // Организация задержки на время загрузки силоса
            // var t = Task.Run(async delegate
            // {
            //     await Task.Delay(TimeSpan.FromSeconds(15));
            // });
            // t.Wait();
            // Task.Delay(TimeSpan.FromSeconds(15));

            // Debug.WriteLine("OK");
            // _logger.Info($"В силос 1 загружен материал [{_mat.Name}]");

        }

        /// <summary>
        /// Окончание загрузки материала в силос
        /// </summary>
        /// <param name="silosNumber">Номер силоса</param>
        private void FinishLoadSilos(int silosNumber)
        {
            // Окончание загрузки силоса
            Debug.WriteLine($"Завершена загрузка силоса {silosNumber}");
            _logger.Info($"Завершена загрузка силоса {silosNumber}");

            _loadStatuses[silosNumber + 2] = "";
            _siloses[silosNumber].SetStatus(Statuses.Status.Off);
            _statuses[silosNumber + 2] = "img/arm1/led/SmallGrey.png";
            _selected[silosNumber] = "img/arm1/led/LedGrey.png"; 
            _conveyors[0] = "img/arm1/Elevator2Grey.png";
            _conveyors[1] = "img/arm1/Elevator3Grey.png";
            _conveyors[2] = "img/arm1/Elevator2Grey.png";
            _conveyors[3] = "img/arm1/TelegaGrey.png"; 

            if (_inputTankers[0].Selected)
            {
                _inputTankers[0].Selected = false;
                _statuses[0] = "img/arm1/led/SquareGrey.png";
                _inputTankers[0].SetStatus(Statuses.Status.Off);
                _loadStatuses[0] = "";
            }                                   
            else
            {
                _inputTankers[1].Selected = false;
                _statuses[1] = "img/arm1/led/SquareGrey.png";
                _inputTankers[1].SetStatus(Statuses.Status.Off);
                _loadStatuses[1] = "";
            }
        }

        private async void Test()
        {
            int number = Int32.Parse(_manualLoadMaterial.BunkerId);
            StartLoadInputTanker(number);
            
            // Организация задержки на время загрузки силоса
            await Task.Delay(15000);
            FinishLoadInputTanker(number);
            await OnNotify($"Загрузка бункера №{number + 1} завершена");
        }
        
        private async void Test1()
        {
            int input = Int32.Parse(_manualLoadSilos.InputId);
            int silos = Int32.Parse(_manualLoadSilos.SilosId);

            // Активируем загрузочный бункер, из которого будем забирать материал
            switch (input)
            {
                case 0: // Загрузочный бункер 1 
                {
                    _inputTankers[0].Selected = true;
                    _inputTankers[0].SetStatus(Statuses.Status.Unloading);
                    _statuses[0] = "img/arm1/led/SquareYellow.png";
                    _loadStatuses[0] = "ВЫГРУЗКА";
                    _inputTankers[1].Selected = false;
                    break;
                }
                case 1: // Загрузочный бункер 2
                {
                    _inputTankers[0].Selected = false;
                    _inputTankers[1].Selected = true;
                    _inputTankers[1].SetStatus(Statuses.Status.Unloading);
                    _loadStatuses[1] = "ВЫГРУЗКА";
                    _statuses[1] = "img/arm1/led/SquareYellow.png";
                    break;
                }
            }
            
            // Начало загрузки материала в силос из загрузочного бункера
            StartLoadSilos(silos);
            
            // Организация задержки на время загрузки силоса
            await Task.Delay(15000);
            
            // Окончание загрузки материала в силос из загрузочного бункера
            FinishLoadSilos(silos);
            await OnNotify($"Загрузка силоса №{silos + 1} завершена");
        }

    }
}
