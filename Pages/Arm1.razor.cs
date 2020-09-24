using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
        // public Kernel DataKernel;
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

        private readonly Statuses[] _statuses = new Statuses[10];
        private Statuses _status = new Statuses();
        private readonly string[] _selected = new string[10];
        private bool _silosLoading;
        private int fromInput;
        private int toSilos;
        private string _telegaPos;
        private string[] _telegaPositions = new string[8];
        private string[] _conveyors = new string[4];
        private string _detailPosX;
        private string _detailPosY;
        private string _showed = "none";
        
        //TODO: Предусмотреть возможность отмены операции загрузки (разгрузки) с возвратом зарезервированного материала
        private CancellationTokenSource _cancelLoadInput;
        private CancellationTokenSource _cancelLoadSilos;
        private CancellationToken _tokenInput;
        private CancellationToken _tokenSilos;

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
            Kernel.Data.SetMaterials(_materials);
            Kernel.Data.SetInputTankers(_inputTankers);
            Kernel.Data.SetSiloses(_siloses);
            Kernel.Data.SetCurrentMaterialIndex(_currentMaterial);
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
                case 4038: await LoadInputTanker(0, _tokenInput); break; // Загрузить материал в 1 загрузочный бункер
                case 4039: await LoadInputTanker(1, _tokenInput); break; // Загрузить материал во 2 загрузочный бункер
                
                // case 4040: break;// Признак конца загрузки первого загрузочного бункера
                // case 4041: break; // Признак конца загрузки второго загрузочного бункера      
            }
        }

        // Получить список всем материалов
        private void GetMaterials()
        {
            _materials = _db.GetMaterials();
            Kernel.Data.SetMaterials(_materials);
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
            
            // Установить текущий материал в ядре
            Kernel.Data.SetCurrentMaterialIndex(_currentMaterial);
        }

        // Перместить счетчик материала предыдущего загруженного материала
        private void MovePrewMaterial()
        {
            if (_currentMaterial < 0)
            {
                _currentMaterial = 0;
            }
            else
            {
                _currentMaterial--;
            }
            
            // Установить текущий материал в ядре
            Kernel.Data.SetCurrentMaterialIndex(_currentMaterial);
        }

        // Начальная инициализация компонентов АРМ 1
        private async Task Initialize()
        {
            // Если в ядре системе нет загрузочных бункеров, то создадим их
            if (Kernel.Data.GetInputTankersCount() == 0)
            {
                // Создаем новые загрузочные бункера
                for (int i = 1; i < 3; i++)
                {
                    InputTanker tanker = new InputTanker(i);
                    Statuses status = new Statuses();
                    status.CurrentState = Statuses.Status.Off;
                    status.StatusIcon = "img/led/SquareGrey.png";
                    status.StatusMessage = "";
                    tanker.SetStatus(status);
                    _inputTankers.Add(tanker);
                }
                
                // Установить загрузочные бункера в ядре системы
                Kernel.Data.SetInputTankers(_inputTankers);
            }
            else
            {
                // Добавляем загрузочные бункера из ядра системы слежения
                for (int i = 0; i < Kernel.Data.GetInputTankersCount(); i++)
                {
                    _inputTankers.Add(Kernel.Data.GetInputTanker(i));
                }
            }

            // Если в ядре системы нет силосов, то создадим их
            if (Kernel.Data.GetSilosesCount() == 0)
            {
                // Создаем новые силоса
                for (int i = 1; i < 9; i++)
                {
                    Silos silos = new Silos(i);
                    Statuses status = new Statuses();
                    status.CurrentState = Statuses.Status.Off;
                    status.StatusIcon = "img/led/SmallGrey.png";
                    status.StatusMessage = "";
                    silos.SetStatus(status);
                    _siloses.Add(silos);
                }
                
                // Установить силосы в ядре системы 
                Kernel.Data.SetSiloses(_siloses);
            }
            else
            {
                // Добавляем силоса из ядра системы
                for (int i = 0; i < Kernel.Data.GetSilosesCount(); i++)
                {
                    _siloses.Add(Kernel.Data.GetSilos(i));
                }
            }
            
            // Если в ядре системы нет конвейеров, то создаим их
            // if (DataKernel.DataKernel.GetConveyorsCount() == 0)
            // {
            //     for (int i = 0; i < 5; i++)
            //     {
            //         Conveyor conveyor = new Conveyor(i, Conveyor.Types.Horizontal, 15, 1);
            //         _conveyors.Add(conveyor);
            //     }
            //     
            //     // Добавляем конвейеры в ядро системы
            //     DataKernel.DataKernel.SetConveyors(_conveyors);
            // }
            // else
            // {
            //     for (int i = 0; i < DataKernel.DataKernel.GetConveyorsCount(); i++)
            //     {
            //         _conveyors.Add(DataKernel.DataKernel.GetConveyor(i));
            //     }
            // }
            
            // Проверяем индекс текущего материала, и если он не установлен, то устанавливаем его
            int kernelMaterialIndex = Kernel.Data.GetCurrentMaterialIndex();
            if (kernelMaterialIndex == -1)
            {
                _currentMaterial = 0;
                Kernel.Data.SetCurrentMaterialIndex(_currentMaterial);
            }
            else
            {
                _currentMaterial = kernelMaterialIndex;
            }

            // Устанавливаем текущее состояние для конвейеров
            _conveyors[0] = "img/arm1/Elevator2Grey.png";
            _conveyors[1] = "img/arm1/Elevator3Grey.png";
            _conveyors[2] = "img/arm1/Elevator2Grey.png";
            _conveyors[3] = "img/arm1/TelegaGrey.png";

            // Устанавливаем текущее состояние для загрузочных бункеров
            // _status = new Statuses();
            // _status.StatusIcon = "img/led/SquareGrey.png";
            _statuses[0] = _inputTankers[0].GetStatus();
            _statuses[1] = _inputTankers[1].GetStatus();
            
            // Устанавливаем текущее состояние для силосов
            // _status = new Statuses();
            // _status.StatusIcon = "img/led/SmallGrey.png";
            _statuses[2] = _siloses[0].GetStatus();
            _statuses[3] = _siloses[1].GetStatus();
            _statuses[4] = _siloses[2].GetStatus();
            _statuses[5] = _siloses[3].GetStatus();
            _statuses[6] = _siloses[4].GetStatus();
            _statuses[7] = _siloses[5].GetStatus();
            _statuses[8] = _siloses[6].GetStatus();
            _statuses[9] = _siloses[7].GetStatus();

            _selected[0] = "img/led/LedGrey.png";
            _selected[1] = "img/led/LedGrey.png";
            _selected[2] = "img/led/LedGrey.png";
            _selected[3] = "img/led/LedGrey.png";
            _selected[4] = "img/led/LedGrey.png";
            _selected[5] = "img/led/LedGrey.png";
            _selected[6] = "img/led/LedGrey.png";
            _selected[7] = "img/led/LedGrey.png";

            _telegaPositions[0] = "670px";
            _telegaPositions[1] = "770px";
            _telegaPositions[2] = "870px";
            _telegaPositions[3] = "970px";
            _telegaPositions[4] = "770px";
            _telegaPositions[5] = "870px";
            _telegaPositions[6] = "970px";
            _telegaPositions[7] = "1070px";

            _silosLoading = false;
            await OnNotify("Готов");
            _cancelLoadInput = new CancellationTokenSource();
            _cancelLoadSilos = new CancellationTokenSource();
            _tokenInput = _cancelLoadInput.Token;
            _tokenSilos = _cancelLoadSilos.Token;
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
                    matCount = _siloses[0].GetLayersCount();
                    _loadedMaterial = _siloses[0].GetMaterials();
                    break;
                }
                case 4:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[1].GetLayersCount();
                    _loadedMaterial = _siloses[1].GetMaterials();
                    break;
                }
                case 5:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[2].GetLayersCount();
                    _loadedMaterial = _siloses[2].GetMaterials();
                    break;
                }
                case 6:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[3].GetLayersCount();
                    _loadedMaterial = _siloses[3].GetMaterials();
                    break;
                }
                case 7:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[4].GetLayersCount();
                    _loadedMaterial = _siloses[4].GetMaterials();
                    break;
                }
                case 8:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[5].GetLayersCount();
                    _loadedMaterial = _siloses[5].GetMaterials();
                    break;
                }
                case 9:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[6].GetLayersCount();
                    _loadedMaterial = _siloses[6].GetMaterials();
                    break;
                }
                case 10:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[7].GetLayersCount();
                    _loadedMaterial = _siloses[7].GetMaterials();
                    break;
                }
            }

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
            _showed = "none";
        }

        /// <summary>
        /// Обнуление загрузочного бункера
        /// </summary>
        /// <param name="number">Номер обнуляемого загрузочного бункера</param>
        private void ResetInputTank(int number)
        {
            _inputTankers[number].Reset();
            _status = new Statuses();
            _status.StatusIcon = "img/led/SquareGrey.png";
            _status.StatusMessage = "";
            _status.CurrentState = Statuses.Status.Off;
            _statuses[number] = _status;
            _logger.Warn($"Было произведено обнуление загрузочного бункера [{number}]!");
            Debug.WriteLine($"Было произведено обнуление загрузочного бункера [{number}]!");
            fromInput = 0;
        }

        /// <summary>
        /// Сбросить состояние ошибки загрузочного бункера
        /// </summary>
        /// <param name="number">Номер загрузочного бункера</param>
        private void ResetInputError(int number)
        {
            if(_inputTankers[number].GetCurrentState() == Statuses.Status.Error)
            {
                _status = new Statuses();
                _status.StatusIcon = "img/led/SquareGrey.png";
                _status.StatusMessage = "";
                _status.CurrentState = Statuses.Status.Off;
                _statuses[number] = _status;
                _inputTankers[number].SetStatus(_status);
                _logger.Warn($"Был произведен сброс загрузочного бункера [{number}]!");
                Debug.WriteLine($"Был произведен сброс загрузочного бункера [{number}]!");
                fromInput = 0;
            }
        }
        
        /// <summary>
        /// Обнуление силоса
        /// </summary>
        /// <param name="number">Номер обнуляемого силоса</param>
        private void ResetSilos(int number)
        {
            _status = new Statuses();
            _status.StatusIcon = "img/led/SmallGrey.png";
            _status.StatusMessage = "";
            _status.CurrentState = Statuses.Status.Off;
            _siloses[number].Reset();
            _siloses[number].SetStatus(_status);
            _statuses[number+2] =_status;
            _logger.Warn($"Было произведено обнуление загрузочного бункера [{number+1}]!");
            Debug.WriteLine($"Было произведено обнуление загрузочного бункера [{number+1}]!");
        }


        /// <summary>
        /// Сбросить состояние ошибки загрузочного бункера
        /// </summary>
        /// <param name="number">Номер загрузочного бункера</param>
        private void ResetSilosError(int number)
        {
            // Сбрасывать ошибку силоса только если он находится в состоянии ошибки
            if(_siloses[number].GetCurrentState() == Statuses.Status.Error)
            {
                _status = new Statuses();
                _status.StatusIcon = "img/led/SmallGrey.png";
                _status.StatusMessage = "";
                _status.CurrentState = Statuses.Status.Off;
                _statuses[number + 2] = _status;
                _siloses[number].SetStatus(_status);
                _logger.Warn($"Был произведен сброс силоса [{number + 1}]!");
                Debug.WriteLine($"Был произведен сброс силоса [{number + 1}]!");
            }
        }

        /// <summary>
        /// Загрузка материала в загрузочный бункер
        /// </summary>
        /// <param name="number">Номер загрузочного бункера</param>
        private async Task LoadInputTanker(int number, CancellationToken cancel)
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
            Statuses.Status status = _inputTankers[number].GetCurrentState();

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
                            _status = new Statuses();
                            _status.StatusMessage = "ОШИБКА";
                            _status.StatusIcon = "img/led/SquareRed.png";
                            _status.CurrentState = Statuses.Status.Error;
                            _inputTankers[number].SetStatus(_status);
                            _statuses[number] = _status;
                            _logger.Error(
                                $"Попытка загрузить материал {newName} в загрузочный бункер [1], содержащий материал {oldName}");
                            Debug.WriteLine(
                                $"Попытка загрузить материал {newName} в загрузочный бункер [1], содержащий материал {oldName}");
                            return;
                        }
                    }

                    // 5. Начинаем загрузку материала в загрузочный бункер        
                    MoveNextMaterial();    // Резервируем загружаемый материал для бункера
                    _status = new Statuses();
                    _status.StatusMessage = "ЗАГРУЗКА";
                    _status.StatusIcon = "img/led/SquareGreen.png";
                    _status.CurrentState = Statuses.Status.Loading;
                    _statuses[number] = _status;
                    _inputTankers[number].SetStatus(_status);

                    // Задержка и загрузка материала      
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    if (cancel.IsCancellationRequested)
                    {
                        _logger.Warn($"Отменена загрузка материала в силос {number}");
                        Debug.WriteLine($"Отменена загрузка материала в силос {number}");
                        MovePrewMaterial();
                        _inputTankers[number].Load(material);
                        _status = new Statuses();
                        _status.StatusMessage = "";
                        _status.StatusIcon = "img/led/SquareGrey.png";
                        _status.CurrentState = Statuses.Status.Off;
                        _statuses[number] = _status;
                        _inputTankers[number].SetStatus(_status);
                        
                        return;
                    }
                    
                    // Материал загружен
                    _inputTankers[number].Load(material);
                    _status = new Statuses();
                    _status.StatusMessage = "";
                    _status.StatusIcon = "img/led/SquareGrey.png";
                    _status.CurrentState = Statuses.Status.Off;
                    _statuses[number] = _status;
                    _inputTankers[number].SetStatus(_status);
                    

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
                    await StartLoadSilos(_tokenSilos);
                    break;
                }
            }
        }

        /// <summary>
        /// Начало загрузки материала в силос
        /// </summary>
        private async Task StartLoadSilos(CancellationToken cancel)
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
                Statuses.Status inputStatus = _inputTankers[inputNumber].GetCurrentState();
                Statuses.Status silosStatus = _siloses[silosNumber].GetCurrentState();
                
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
                                    _status = new Statuses();
                                    _status.StatusIcon = "img/led/SmallRed.png";
                                    _status.StatusMessage = "ОШИБКА";
                                    _status.CurrentState = Statuses.Status.Error;
                                    _siloses[silosNumber].SetStatus(_status);
                                    _statuses[silosNumber + 2] = _status;
                                    await OnNotify($"Загружаемый материал [{inputMaterial}] не соответствует материалу в силосе {silosNumber+1}");
                                    return;
                                }
                            }
                            
                            // Начинаем загрузку материала в силос из загрузочного бункера
                            _silosLoading = true;
                            _status = new Statuses();
                            _status.StatusIcon = "img/led/SquareYellow.png";
                            _status.StatusMessage = "ВЫГРУЗКА";
                            _status.CurrentState = Statuses.Status.Unloading;
                            _statuses[inputNumber] = _status;
                            _inputTankers[inputNumber].SetStatus(_status);
                            
                            _status = new Statuses();
                            _status.StatusIcon = "img/led/SmallGreen.png";
                            _status.StatusMessage = "ЗАГРУЗКА";
                            _status.CurrentState = Statuses.Status.Loading;
                            _statuses[silosNumber + 2] = _status;
                            _siloses[silosNumber].SetStatus(_status);
                            
                            _selected[silosNumber] = "img/led/LedGreen.png";
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
                            _status = new Statuses();
                            _status.StatusIcon = "img/led/SquareGrey.png";
                            _status.StatusMessage = "";
                            _status.CurrentState = Statuses.Status.Off;
                            _statuses[inputNumber] = _status;
                            _inputTankers[inputNumber].SetStatus(_status);
                            
                            _status = new Statuses();
                            _status.StatusIcon = "img/led/SmallGrey.png";
                            _status.StatusMessage = "";
                            _status.CurrentState = Statuses.Status.Off;
                            _statuses[silosNumber + 2] = _status;
                            _siloses[silosNumber].SetStatus(_status);
                            _selected[silosNumber] = "img/led/LedGrey.png";
                            
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
                            _status = new Statuses();
                            _status.StatusIcon = "img/led/SmallRed.png";
                            _status.StatusMessage = "ОШИБКА";
                            _status.CurrentState = Statuses.Status.Error;
                            _statuses[silosNumber + 2] = _status;
                            _siloses[silosNumber].SetStatus(_status);
                            await OnNotify($"Загрузочный бункер {fromInput+1} пуст!");
                        }
                    }
                    else
                    {
                        // Силос занят или ошибка силоса
                        Debug.WriteLine($"Силос {silosNumber+1} занят или в состоянии ошибки: [{silosStatus.ToString()}]!");
                        _logger.Error($"Силос {silosNumber+1} занят или в состоянии ошибки: [{silosStatus.ToString()}]!");

                        // Выдать ошибку силоса
                        _status = new Statuses();
                        _status.StatusIcon = "img/led/SmallRed.png";
                        _status.StatusMessage = "ОШИБКА";
                        _status.CurrentState = Statuses.Status.Error;
                        _statuses[silosNumber + 2] = _status;
                        _siloses[silosNumber].SetStatus(_status);
                        await OnNotify($"Силос {silosNumber+1} в состоянии: [{silosStatus.ToString()}]!");
                    }
                }
                else
                {
                    // Загрузочный бункер занят или ошибка загрузочного бункера
                    Debug.WriteLine($"Загрузочный бункер {fromInput+1} занят или в состоянии ошибки: [{inputStatus.ToString()}]!");
                    _logger.Error($"Загрузочный бункер {fromInput+1} занят или в состоянии ошибки: [{inputStatus.ToString()}]!");

                    // Выдать ошибку силоса
                    _status = new Statuses();
                    _status.StatusIcon = "img/led/SmallRed.png";
                    _status.StatusMessage = "ОШИБКА";
                    _status.CurrentState = Statuses.Status.Error;
                    _statuses[silosNumber + 2] = _status;
                    _siloses[silosNumber].SetStatus(_status);
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
                    await LoadInputTanker(number, _tokenInput);
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

        private void CancelLoadInput(CancellationTokenSource token)
        {
            token.Cancel();
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
                    await StartLoadSilos(_tokenSilos);
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
