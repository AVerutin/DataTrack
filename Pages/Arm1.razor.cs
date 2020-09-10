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
        private (ushort key, double value) _lastNotification;

        // Список материалов, полученный из базы данных
        private List<Material> _materials = new List<Material>();
        
        // Список материала, загруженного в силос (для визуализации на странице)
        private List<Material> _loadedMaterial = new List<Material>(); 
        
        private readonly List<InputTanker> _inputTankers = new List<InputTanker>();
        private readonly List<Silos> _siloses = new List<Silos>();
        private readonly List<Conveyor> _conveyors = new List<Conveyor>();

        private int _currentMaterial;
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private IConfigurationRoot _config;
        private Data.MtsConnect _mtsConnect;
        private List<ushort> _signals;
        private readonly DBConnection _db = new DBConnection();
        private readonly ConfigMill _configMill = new ConfigMill();

        private readonly string[] _statuses = new string[10];
        private readonly string[] _selected = new string[8];
        private int _silosSelected;
        private string _telegaPos;
        private string _detailPosX;
        private string _detailPosY;
        private string _showed = "none";
        private readonly string[] _loadStatus = new string[2];


        // private long num = 0;
        private ManualWork manual = new ManualWork();

        // Обработка события загрузки страницы
        protected override async void OnInitialized()
        {
            // Добавления подписки на события уведомлений
            Notifier.Notify += OnNotify;

            GetMaterials();
            Initialize();
            await ConnectToMts(); // Подключение к сервису MTS Service
        }

        // Событие при обновлении значения события
        private async Task OnNotify(ushort key, double value)
        {
            await InvokeAsync(() =>
            {
                _lastNotification = (key, value);
                StateHasChanged();
            });
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
                    _lastNotification.key = item.Key;
                    _lastNotification.value = item.Value;
                    // Debug.WriteLine($"[{item.Key}] = {item.Value}");

                    await InvokeAsync(async () =>
                    {
                        await OnNotify(item.Key, item.Value);
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
                case 4014: // Загрузка силоса 1
                {
                    // Debug.Write("Начата загрузка силоса 1...");
                    // _logger.Info("Начата загрузка силоса 1");
                    // // Получаем номер активного загрузочного бункера и загруженный в него материал
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
                    break;
                }
                case 4015: // Загрузка силоса 2
                {
                    break;
                }
                case 4016: // Загрузка силоса 3
                {
                    break;
                }
                case 4017: // Загрузка силоса 4
                {
                    break;
                }
                case 4018: // Загрузка силоса 5
                {
                    break;
                }
                case 4019: // Загрузка силоса 6
                {
                    break;
                }
                case 4020: // Загрузка силоса 7
                {
                    break;
                }
                case 4021: // Загрузка силоса 8
                {
                    break;
                }

                case 4022:
                    break; // Высыпать весовой бункер 1
                case 4023:
                    break; // Высыпать весовой бункер 2
                case 4024:
                    break; // Высыпать весовой бункер 3
                case 4025:
                    break; // Вес в бункере 1
                case 4026:
                    break; // Вес в бункере 2
                case 4027:
                    break; // Вес в бункере 3
                case 4028:
                    break; // Целевое направление материала
                case 4029:
                    break; // Температура плавки

                // Загрузить материал в первый загрузочный бункер
                case 4038: StartLoadInputTanker(0); break; 
                // Загрузить материал во второй загрузочный бункер
                case 4039:  StartLoadInputTanker(1); break; 

                case 4040:    // Признак конца загрузки первого загрузочного бункера
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
                case 4041:    // Признак конца загрузки второго загрузочного бункера
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
            Conveyor _conveyor;

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

            _conveyor = new Conveyor(1, Conveyor.Types.Horizontal, 5);
            _conveyors.Add(_conveyor);
            _conveyor = new Conveyor(2, Conveyor.Types.Vertical, 25);
            _conveyors.Add(_conveyor);
            _conveyor = new Conveyor(3, Conveyor.Types.Horizontal, 15);
            _conveyors.Add(_conveyor);

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
                    _telegaPos = "670px";
                    matCount = _siloses[0].GetLayersCount();
                    _loadedMaterial = _siloses[0].GetMaterials();
                    break;
                }
                case 4:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    _telegaPos = "770px";
                    matCount = _siloses[1].GetLayersCount();
                    _loadedMaterial = _siloses[1].GetMaterials();
                    break;
                }
                case 5:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    _telegaPos = "870px";
                    matCount = _siloses[2].GetLayersCount();
                    _loadedMaterial = _siloses[2].GetMaterials();
                    break;
                }
                case 6:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    _telegaPos = "970px";
                    matCount = _siloses[3].GetLayersCount();
                    _loadedMaterial = _siloses[3].GetMaterials();
                    break;
                }
                case 7:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    _telegaPos = "770px";
                    matCount = _siloses[4].GetLayersCount();
                    _loadedMaterial = _siloses[4].GetMaterials();
                    break;
                }
                case 8:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    _telegaPos = "870px";
                    matCount = _siloses[5].GetLayersCount();
                    _loadedMaterial = _siloses[5].GetMaterials();
                    break;
                }
                case 9:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    _telegaPos = "970px";
                    matCount = _siloses[6].GetLayersCount();
                    _loadedMaterial = _siloses[6].GetMaterials();
                    break;
                }
                case 10:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    _telegaPos = "1070px";
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
            _loadStatus[number] = "";
            _logger.Warn($"Был произведен сброс загрузочного бункера [{number}]!");
            Debug.WriteLine($"Был произведен сброс загрузочного бункера [{number}]!");
        }

        /// <summary>
        /// Завершение загрузки материала в загрузочный бункер
        /// </summary>
        /// <param name="number">Номер загрузочного бункера</param>
        private void FinishLoadInputTanker(int number)
        {
            _loadStatus[number] = "";
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
            int tanker = number;

            /*
             * 1. Получить следующий загружаемый материал из списка
             * 2. Если бункер не содержит материал, то загрузить полученный материал и установить задержку на время загрузки бункера
             * 3. Если бункер содержит материал, то проверить наименование загружаемого материала. Если оно совпадает
             *     наименовавнием загруженного материала, то загрузить материал, иначе выдать ошибку
             */
            
            // 1. Получаем следующий загружаемый материал
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
                        _inputTankers[tanker].SetStatus(Statuses.Status.On);
                        _loadStatus[tanker] = "Загрузка";
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
                        _loadStatus[tanker] = "Ошибка";
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
                    _inputTankers[tanker].SetStatus(Statuses.Status.On);
                    _loadStatus[tanker] = "Загрузка";
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
                }
            }
        }

        private void Test()
        {
            int number = Int32.Parse(manual.BunkerID);
            StartLoadInputTanker(number);
        }
    }
}
