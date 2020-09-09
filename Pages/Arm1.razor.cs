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
        private (ushort key, double value) lastNotification;

        private List<Material> _material = new List<Material>();
        private List<InputTanker> InputTankers = new List<InputTanker>();
        private List<Silos> Siloses = new List<Silos>();
        private List<Conveyor> Conveyors = new List<Conveyor>();

        private int currentMaterial = 0;
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private IConfigurationRoot _config;
        private Data.MtsConnect _mtsConnect;
        private List<ushort> _signals;
        private Dictionary<ushort, double> _data = new Dictionary<ushort, double>();
        private DBConnection db = new DBConnection();
        private ConfigMill _configMill = new ConfigMill();

        private string[] _statuses = new string[10];
        private string[] _selected = new string[8];
        private int silosSelected = 0;
        private string TelegaPos;
        private string detailPosX;
        private string detailPosY;
        private string showed = "none";

        // Обработка события загрузки страницы
        protected override async void OnInitialized()
        {
            // Добавления подписки на события уведомлений
            Notifier.Notify += OnNotify;

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
            GetMaterials();
            Initialize();

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

        // Событие при обновлении значения события
        private async Task OnNotify(ushort key, double value)
        {
            await InvokeAsync(() =>
            {
                lastNotification = (key, value);
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            Notifier.Notify -= OnNotify;
        }

        private async void NewData(SubscriptionStateEventArgs e)
        {
            SignalsState diff = e.Diff.Signals;
            if (diff != null)
            {
                foreach (var item in diff)
                {
                    _data[item.Key] = item.Value;
                    lastNotification.key = item.Key;
                    lastNotification.value = item.Value;
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
                /*
                         * При получении сигнала на загрузку силоса:
                         *     1. Получаем номер активного загрузочного бункера
                         *     2. Получаем следующий материал, который будет загружен в активном загрузочном бункере
                         *     3. Если силос пуст, или наименование материала совпадает:
                         *         - загружаем материал из активного загрузочного бункера
                         */
                case 4014: // Загрузка силоса 1
                {
                    Debug.Write("Начата загрузка силоса 1...");
                    _logger.Info("Начата загрузка силоса 1");
                    // Получаем номер активного загрузочного бункера и загруженный в него материал
                    int tanker;
                    Material _mat = GetNextMaterial();

                    if (InputTankers[0].GetStatus() == Statuses.Status.Selected)
                    {
                        tanker = 0;
                    }
                    else
                    {
                        tanker = 1;
                    }

                    InputTankers[tanker].Load(_mat);

                    // Если материал загрузочного бункера и силоса совпадает, или силос пуст
                    if (Siloses[0].Material == "" || _mat.Name == Siloses[0].Material)
                    {
                        // Производим загрузку материала в силос из активного загрузочного бункера
                        Siloses[0].Load(InputTankers[tanker]);
                        _statuses[2] = "img/arm1/led/SmallGreen.png";
                    }
                    
                    // Организация задержки на время загрузки силоса
                    // Вместо лямбды передать ссылку на метод завершения загрузки силоса
                    // var t = Task.Run(async delegate
                    // {
                    //     await Task.Delay(TimeSpan.FromSeconds(15));
                    //     return 42;
                    // });
                    // t.Wait();
                    Task.Delay(TimeSpan.FromSeconds(15));

                    Debug.WriteLine("OK");
                    _logger.Info($"В силос 1 загружен материал [{_mat.Name}]");
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
                case 4019:
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

                case 4038: // Признак активации первого загрузочного бункера
                {
                    /*
                                * Установить статус первого загрузочного бункера в состояния Selected
                                * Установить статус второго загрузочного бункера в состояние Deselected
                                */

                    InputTankers[0].SetStatus(Statuses.Status.Selected);
                    InputTankers[1].SetStatus(Statuses.Status.Deselected);
                    _logger.Info("Активирован загрузочный бункер 1");
                    Debug.WriteLine("Активирован загрузочный бункер 1");
                    break;
                }
                case 4039: // Признак загрузки второго загрузочного бункера
                {
                    /*
                                * Установить статус первого загрузочного бункера в состояния Selected
                                * Установить статус второго загрузочного бункера в состояние Deselected
                                */

                    InputTankers[0].SetStatus(Statuses.Status.Deselected);
                    InputTankers[1].SetStatus(Statuses.Status.Selected);
                    _logger.Info("Активирован загрузочный бункер 2");
                    Debug.WriteLine("Активирован загрузочный бункер 2");
                    break;
                }

                case 4040:
                    break; // Признак разгрузки первого загрузочного бункера
                case 4041:
                    break; // Признак разгрузки второго загрузочного бункера

            }
        }

        // Получить список всем материалов
        private void GetMaterials()
        {
            _material = db.GetMaterials();
        }

        // Получить следующий материал из списка всех материалов 
        private Material GetNextMaterial()
        {
            Material _mat = new Material();
            if (_material.Count == 0)
            {
                _mat = null;
            }
            else
            {
                if (currentMaterial >= _material.Count)
                {
                    currentMaterial = 0;
                }

                _mat = _material[currentMaterial++];
            }

            return _mat;
        }

        //
        private void Initialize()
        {
            InputTanker _tanker;
            Silos _silos;
            Conveyor _conveyor;

            // Добавляем загрузочные бункера
            for (int i = 1; i < 3; i++)
            {
                _tanker = new InputTanker(i);
                InputTankers.Add(_tanker);
            }

            for (int i = 1; i < 9; i++)
            {
                _silos = new Silos(i);
                Siloses.Add(_silos);
            }

            _conveyor = new Conveyor(1, Conveyor.Types.Horizontal, 5);
            Conveyors.Add(_conveyor);
            _conveyor = new Conveyor(2, Conveyor.Types.Vertical, 25);
            Conveyors.Add(_conveyor);
            _conveyor = new Conveyor(3, Conveyor.Types.Horizontal, 15);
            Conveyors.Add(_conveyor);

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
                    detailPosY = $"{e.ClientY + 20}px";
                    detailPosX = $"{e.ClientX + 10}px";
                    matCount = InputTankers[0].GetLayersCount();
                    _material = InputTankers[0].GetMaterials();
                    break;
                }
                case 2:
                {
                    detailPosY = $"{e.ClientY + 20}px";
                    detailPosX = $"{e.ClientX + 10}px";
                    matCount = InputTankers[1].GetLayersCount();
                    _material = InputTankers[1].GetMaterials();
                    break;
                }
                case 3:
                {
                    detailPosY = $"{e.ClientY + 20}px";
                    detailPosX = $"{e.ClientX + 10}px";
                    TelegaPos = "670px";
                    matCount = Siloses[0].GetLayersCount();
                    _material = Siloses[0].GetMaterials();
                    break;
                }
                case 4:
                {
                    detailPosY = $"{e.ClientY + 20}px";
                    detailPosX = $"{e.ClientX + 10}px";
                    TelegaPos = "770px";
                    matCount = Siloses[1].GetLayersCount();
                    _material = Siloses[1].GetMaterials();
                    break;
                }
                case 5:
                {
                    detailPosY = $"{e.ClientY + 20}px";
                    detailPosX = $"{e.ClientX + 10}px";
                    TelegaPos = "870px";
                    matCount = Siloses[2].GetLayersCount();
                    _material = Siloses[2].GetMaterials();
                    break;
                }
                case 6:
                {
                    detailPosY = $"{e.ClientY + 20}px";
                    detailPosX = $"{e.ClientX + 10}px";
                    TelegaPos = "970px";
                    matCount = Siloses[3].GetLayersCount();
                    _material = Siloses[3].GetMaterials();
                    break;
                }
                case 7:
                {
                    detailPosY = $"{e.ClientY + 20}px";
                    detailPosX = $"{e.ClientX + 10}px";
                    TelegaPos = "770px";
                    matCount = Siloses[4].GetLayersCount();
                    _material = Siloses[4].GetMaterials();
                    break;
                }
                case 8:
                {
                    detailPosY = $"{e.ClientY + 20}px";
                    detailPosX = $"{e.ClientX + 10}px";
                    TelegaPos = "870px";
                    matCount = Siloses[5].GetLayersCount();
                    _material = Siloses[5].GetMaterials();
                    break;
                }
                case 9:
                {
                    detailPosY = $"{e.ClientY + 20}px";
                    detailPosX = $"{e.ClientX + 10}px";
                    TelegaPos = "970px";
                    matCount = Siloses[6].GetLayersCount();
                    _material = Siloses[6].GetMaterials();
                    break;
                }
                case 10:
                {
                    detailPosY = $"{e.ClientY + 20}px";
                    detailPosX = $"{e.ClientX + 10}px";
                    TelegaPos = "1070px";
                    matCount = Siloses[7].GetLayersCount();
                    _material = Siloses[7].GetMaterials();
                    break;
                }
            }

            silosSelected = number;
            if (matCount > 0)
            {
                showed = "inherit";
            }
            else
            {
                showed = "none";
            }
        }

        private void HideMaterial(MouseEventArgs e, int number)
        {
            silosSelected = 0;
            showed = "none";
        }
    }
}
