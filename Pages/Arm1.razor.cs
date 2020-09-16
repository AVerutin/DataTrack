using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DataTrack.Data;
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
        // private int _silosSelected;
        private bool _silosLoading;
        private int fromInput;
        private int toSilos;
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
            await Initialize();
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

        private async void CheckSignal(ushort signal, double value)
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

                // case 4038: StartLoadInputTanker(0); break; // Загрузить материал в 1 загрузочный бункер
                // case 4039: StartLoadInputTanker(1); break; // Загрузить материал во 2 загрузочный бункер
                case 4038: await LoadInputTanker(0); break; // Загрузить материал в 1 загрузочный бункер
                case 4039: await LoadInputTanker(1); break; // Загрузить материал во 2 загрузочный бункер
                
                // case 4040: break;// Признак конца загрузки первого загрузочного бункера
                // case 4041: break; // Признак конца загрузки второго загрузочного бункера      
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
        private async Task Initialize()
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

            // _silosSelected = -1;
            _silosLoading = false;
            await OnNotify("Готов");
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

            // _silosSelected = number;
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
            // _silosSelected = 0;
            _showed = "none";
        }

        /// <summary>
        /// Обнуление загрузочного бункера
        /// </summary>
        /// <param name="number">Номер обнуляемого загрузочного бункера</param>
        private void ResetInputTank(int number)
        {
            _inputTankers[number].Reset();
            _statuses[number] = "/img/arm1/led/SquareGrey.png";
            _loadStatuses[number] = "";
            _logger.Warn($"Было произведено обнуление загрузочного бункера [{number}]!");
            Debug.WriteLine($"Было произведено обнуление загрузочного бункера [{number}]!");
            fromInput = 0;
            // _inputSelected = -1;
        }

        /// <summary>
        /// Сбросить состояние ошибки загрузочного бункера
        /// </summary>
        /// <param name="number">Номер загрузочного бункера</param>
        private void ResetInputError(int number)
        {
            _statuses[number] = "/img/arm1/led/SquareGrey.png";
            _loadStatuses[number] = "";
            _inputTankers[number].SetStatus(Statuses.Status.Off);
            _logger.Warn($"Был произведен сброс загрузочного бункера [{number}]!");
            Debug.WriteLine($"Был произведен сброс загрузочного бункера [{number}]!");
            fromInput = 0;
            // _inputSelected = -1;
        }
        
        /// <summary>
        /// Обнуление силоса
        /// </summary>
        /// <param name="number">Номер обнуляемого силоса</param>
        private void ResetSilos(int number)
        {
            _siloses[number].Reset();
            _statuses[number+2] = "/img/arm1/led/SmallGrey.png";
            _loadStatuses[number+2] = "";
            _logger.Warn($"Было произведено обнуление загрузочного бункера [{number+1}]!");
            Debug.WriteLine($"Было произведено обнуление загрузочного бункера [{number+1}]!");
            // fromInput = 0;
            // _inputSelected = -1;
        }


        /// <summary>
        /// Сбросить состояние ошибки загрузочного бункера
        /// </summary>
        /// <param name="number">Номер загрузочного бункера</param>
        private void ResetSilosError(int number)
        {
            // Сбрасывать ошибку силоса только если он находится в состоянии ошибки
            if(_siloses[number].Status == Statuses.Status.Error)
            {
                _statuses[number + 2] = "/img/arm1/led/SmallGrey.png";
                _loadStatuses[number + 2] = "";
                _siloses[number].SetStatus(Statuses.Status.Off);
                _logger.Warn($"Был произведен сброс силоса [{number + 1}]!");
                Debug.WriteLine($"Был произведен сброс силоса [{number + 1}]!");
                // fromInput = 0;
                // _inputSelected = -1;

            }
        }

        /// <summary>
        /// Загрузка материала в загрузочный бункер
        /// </summary>
        /// <param name="number">Номер загрузочного бункера</param>
        private async Task LoadInputTanker(int number)
        {
            /*
             * 1. Проверяем, не производится ли уже загрузка бункера
             * 2. Проверяем, готов ли бункер к загрузке материала
             * 3. Проверяем, имеется ли в бункере уже загруженный материал
             * 4. Проверяем, соответствует ли загружаемый материал уже загруженному
             * 5. Производим загрузку материала в загрузочный бункер
             */

            // 1. Проверяем, осуществляется ли уже загрузка какого-либо материала в загрузочный бункер
            bool loadingStarted = false;

            // 2. Получаем текущее состояние загрузочного бункера
            Statuses.Status status = _inputTankers[number].Status;

            if (status == Statuses.Status.Loading)
            {
                loadingStarted = true;
            }

            // Загрузка материала в загрузочный бункер не производится
            if (!loadingStarted)
            {
                // Если загрузочный бункер простаивает и нет ошибки
                if (status == Statuses.Status.Off)
                {
                    int layers = _inputTankers[number].GetLayersCount();
                    Material material = GetNextMaterial();

                    // 3. Проверяем, содержит ли загрузочный бункер материал
                    if (layers > 0)
                    {
                        string oldName = _inputTankers[number].GetMaterialName();
                        string newName = material.Name;

                        // 4. Проверяем соотвествие загруженного и загружаемого материала
                        if (oldName != newName)
                        {
                            // Загружаемый материал не совпадает с загруженным, ошибка нет прерывания загрузки !!!
                            status = Statuses.Status.Error;
                            _inputTankers[number].SetStatus(status);
                            _loadStatuses[number] = "ОШИБКА";
                            _statuses[number] = "img/arm1/led/SquareRed.png";
                            _logger.Error(
                                $"Попытка загрузить материал {newName} в загрузочный бункер [1], содержащий материал {oldName}");
                            Debug.WriteLine(
                                $"Попытка загрузить материал {newName} в загрузочный бункер [1], содержащий материал {oldName}");
                            return;
                        }
                    }

                    // 5. Начинаем загрузку материала в загрузочный бункер        
                    MoveNextMaterial();    // Резервируем загружаемый материал для бункера
                    _inputTankers[number].SetStatus(Statuses.Status.Loading);
                    _loadStatuses[number] = "ЗАГРУЗКА";
                    _statuses[number] = "img/arm1/led/SquareGreen.png";

                    // Задержка и загрузка материала      
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    _inputTankers[number].Load(material);
                    
                    // Материал загружен
                    _loadStatuses[number] = "";
                    _inputTankers[number].SetStatus(Statuses.Status.Off);
                    _statuses[number] = "img/arm1/led/SquareGrey.png";

                    await OnNotify($"В загрузочный бункер №{number + 1} загружен материал [{material.Name}]");
                }
                else
                {
                    // Бункер в состоянии ошибки или занят
                    switch (status)
                    {
                        case Statuses.Status.Error:
                        {
                            // Возникла ошибка при предыдущей попытке загрузки бункера
                            _logger.Error($"Загрузочный бункер {number+1} находится в состоянии ошибки");
                            Debug.WriteLine($"Загрузочный бункер {number+1} находится в состоянии ошибки");
                            break;
                        }
                        case Statuses.Status.On:
                        {
                            // Бункер занят выполнением других операций
                            _logger.Error(
                                $"Загрузочный бункер {number+1} занят! Необходимо дождаться окончания процесса загрузки материала");
                            Debug.WriteLine(
                                $"Загрузочный бункер {number+1} занят! Необходимо дождаться окончания процесса загрузки материала");
                            break;
                        }
                        case Statuses.Status.Unloading:
                        {
                            // Производится разгрузка материала из бункера
                            _logger.Error(
                                $"Производится разгрузка бункера {number+1}! Необходимо дождаться окончания процесса разгрузки материала");
                            Debug.WriteLine(
                                $"Производится разгрузка бункера {number+1}! Необходимо дождаться окончания процесса разгрузки материала");

                            break;
                        }
                    }
                }
            }
            else
            {
                // Уже прозводится загрузка данного загрузочного бункера
                _logger.Error($"В загрузочный бункер {number + 1} уже производится загрузка материала");
                Debug.WriteLine($"В загрузочный бункер {number + 1} уже производится загрузка материала");
            }
        }


        /// <summary>
        /// Управление загрузкой силосов
        /// </summary>
        /// <param name="number">Номер силоса для управления</param>
        /// <param name="value">1 - загрузка силоса начата, 0 - загрузка силоса завершена</param>
        private async void LoadSilos(int number, double value)
        {
            switch (value)
            {
                case 1:
                {
                    await StartLoadSilos();
                    break;
                }
            }
        }

        /// <summary>
        /// Начало загрузки материала в силос
        /// </summary>
        private async Task StartLoadSilos()
        {
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
            
            // 1. Проверяем, не производится ли в данный момент загрузка силоса
            int silosNumber;
            int inputNumber;
            
            if (!_silosLoading)
            {
                silosNumber = toSilos;        // Номер загружаемого силоса
                inputNumber = fromInput;      // Номер разгружаемого бункера

                Debug.WriteLine($"Подготовка к загрузке силоса {silosNumber + 1}");
                _logger.Info($"Подготовка к загрузке силоса {silosNumber + 1}");
                
                // 1. Получаем текущее состояние загрузочного бункера, из которого будем загружать материал
                Statuses.Status inputStatus = _inputTankers[inputNumber].GetStatus();
                Statuses.Status silosStatus = _siloses[silosNumber].GetStatus();
                
                // Проверяем состояние загрузочного бункера (не занят и не ошибка)
                if (inputStatus == Statuses.Status.Off)
                {
                    // Загрузочный бункер готов к разгрузке материала
                    // Проверяем состояние силоса
                    if (silosStatus == Statuses.Status.Off)
                    {
                        // Силос готов к приему материала                
                        // Проверяем наличие материала в выбранном загрузочном бункере
                        if (_inputTankers[inputNumber].GetLayersCount() > 0)
                        {
                            // Получаем наименование материала, загруженного в загрузочный бункер
                            string inputMaterial = _inputTankers[inputNumber].Material;
                            
                            // Проверяем наличие материала в выбранном силосе
                            if (_siloses[silosNumber].GetLayersCount() > 0)
                            {
                                // Силос содержит материал, проверим на соответствие с загружаемым
                                if (_siloses[silosNumber].Material != inputMaterial)
                                {
                                    // Материал в загрузочном бункере отличается от материала в силосе
                                    Debug.WriteLine(
                                        $"Загрузочный бункер {fromInput + 1} содержит материал [{inputMaterial}] вместо ожидаемого [{_siloses[silosNumber].Material}]!");
                                    _logger.Error(
                                        $"Загрузочный бункер {fromInput + 1} содержит материал [{inputMaterial}] вместо ожидаемого [{_siloses[silosNumber].Material}]!");
                                    
                                    // Выдать ошибку силоса
                                    _siloses[silosNumber].SetStatus(Statuses.Status.Error);
                                    _statuses[silosNumber + 2] = "img/arm1/led/SmallRed.png";
                                    _loadStatuses[silosNumber + 2] = "ОШИБКА";
                                    await OnNotify($"Загружаемый материал [{inputMaterial}] не соответствует материалу в силосе {silosNumber+1}");
                                    return;
                                }
                            }
                            
                            // Начинаем загрузку материала в силос из загрузочного бункера
                            _silosLoading = true;
                            _inputTankers[inputNumber].SetStatus(Statuses.Status.Unloading);
                            _statuses[inputNumber] = "img/arm1/led/SquareYellow.png";
                            _loadStatuses[inputNumber] = "ВЫГРУЗКА";
                            _siloses[silosNumber].SetStatus(Statuses.Status.Loading);
                            _statuses[silosNumber + 2] = "img/arm1/led/SmallGreen.png";
                            _loadStatuses[silosNumber + 2] = "ЗАГРУЗКА";
                            _selected[silosNumber] = "img/arm1/led/LedGreen.png";
                            _conveyors[0] = "img/arm1/Elevator2Green.png";
                            _conveyors[1] = "img/arm1/Elevator3Green.png";
                            _conveyors[2] = "img/arm1/Elevator2Green.png";
                            _telegaPos = _telegaPositions[silosNumber];
                            if (silosNumber < 4)
                            {
                                _conveyors[3] = "img/arm1/TelegaGreenLeft.png";
                            }
                            else
                            {
                                _conveyors[3] = "img/arm1/TelegaGreen.png";
                            }

                            // Ошидание завершения загрузки
                            await Task.Delay(TimeSpan.FromSeconds(10));
                            _siloses[silosNumber].Load(_inputTankers[inputNumber]);
                            
                            // Завершаем загрузку материала в силос из загрузочного бункера
                            _inputTankers[inputNumber].SetStatus(Statuses.Status.Off);
                            _statuses[inputNumber] = "img/arm1/led/SquareGrey.png";
                            _loadStatuses[inputNumber] = "";
                            _siloses[silosNumber].SetStatus(Statuses.Status.Off);
                            _statuses[silosNumber + 2] = "img/arm1/led/SmallGrey.png";
                            _loadStatuses[silosNumber + 2] = "";
                            _selected[silosNumber] = "img/arm1/led/LedGrey.png";
                            _conveyors[0] = "img/arm1/Elevator2Grey.png";
                            _conveyors[1] = "img/arm1/Elevator3Grey.png";
                            _conveyors[2] = "img/arm1/Elevator2Grey.png";
                            _conveyors[3] = "img/arm1/TelegaGrey.png";
                            toSilos = 0;
                            fromInput = 0;
                            _silosLoading = false;
                            await OnNotify($"Загрузка силоса {silosNumber+1} завершена");
                        }
                        else
                        {
                            // Выбранный загрузочный бункер пуст!
                            Debug.WriteLine($"Загрузочный бункер {fromInput+1} пуст!");
                            _logger.Error($"Загрузочный бункер {fromInput+1} пуст!");
                            
                            // Выдать ошибку силоса
                            _siloses[silosNumber].SetStatus(Statuses.Status.Error);
                            _statuses[silosNumber + 2] = "img/arm1/led/SmallRed.png";
                            _loadStatuses[silosNumber + 2] = "ОШИБКА";
                            await OnNotify($"Загрузочный бункер {fromInput+1} пуст!");
                        }
                    }
                    else
                    {
                        // Силос занят или ошибка силоса
                        Debug.WriteLine($"Силос {silosNumber+1} занят или в состоянии ошибки: [{silosStatus.ToString()}]!");
                        _logger.Error($"Силос {silosNumber+1} занят или в состоянии ошибки: [{silosStatus.ToString()}]!");

                        // Выдать ошибку силоса
                        _siloses[silosNumber].SetStatus(Statuses.Status.Error);
                        _statuses[silosNumber + 2] = "img/arm1/led/SmallRed.png";
                        _loadStatuses[silosNumber + 2] = "ОШИБКА";
                        await OnNotify($"Силос {silosNumber+1} в состоянии: [{silosStatus.ToString()}]!");
                    }
                }
                else
                {
                    // Загрузочный бункер занят или ошибка загрузочного бункера
                    Debug.WriteLine($"Загрузочный бункер {fromInput+1} занят или в состоянии ошибки: [{inputStatus.ToString()}]!");
                    _logger.Error($"Загрузочный бункер {fromInput+1} занят или в состоянии ошибки: [{inputStatus.ToString()}]!");

                    // Выдать ошибку силоса
                    _siloses[silosNumber].SetStatus(Statuses.Status.Error);
                    _statuses[silosNumber + 2] = "img/arm1/led/SmallRed.png";
                    _loadStatuses[silosNumber + 2] = "ОШИБКА";
                    await OnNotify($"Загрузочный бункер {fromInput+1} в состоянии: [{inputStatus.ToString()}]!");
                }
            }
            else
            {
                // Загрузка нескольких силосов одновременно невозможна
                Debug.WriteLine("Загрузка нескольких силосов одновременно невозможна!");
                _logger.Error("Загрузка нескольких силосов одновременно невозможна!");
                await OnNotify("Загрузка нескольких силосов одновременно невозможна!");
            }
        }


        /// <summary>
        /// Ручная загрузка материала в выбранный загрузочный бункер
        /// </summary>
        private async void Test(int buttonId)
        {
            int number = Int32.Parse(_manualLoadMaterial.BunkerId);
            
            // buttonId = 1 - Загрузка
            // buttonId = 2 - Сброс
            // buttonId = 3 - Обнуление

            switch (buttonId)
            {
                case 1:
                {
                    await LoadInputTanker(number);
                    break;
                }
                case 2:
                {
                    ResetInputError(number);
                    break;
                }
                case 3:
                {
                    ResetInputTank(number);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Ручная загрузка материала в силос из выбранного загрузочного бункера
        /// </summary>
        private async void Test1(int buttonId)
        {
            //FIXME: Добавить возможность отмены загрузки силоса, пока его загрузка еще не завершена с возвратом материала из резерва
            int input = Int32.Parse(_manualLoadSilos.InputId);
            int silos = Int32.Parse(_manualLoadSilos.SilosId);

            fromInput = input;
            toSilos = silos;
            
            // buttonId = 1 - Загрузка
            // buttonId = 2 - Сброс
            // buttonId = 3 - Обнуление

            switch (buttonId)
            {
                case 1:
                {
                    await StartLoadSilos();
                    break;
                }
                case 2:
                {
                    ResetSilosError(silos);
                    break;
                }
                case 3:
                {
                    ResetSilos(silos);
                    break;
                }
            }
        }
    }
}
