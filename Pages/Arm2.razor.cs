using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DataTrack.Data;
using Microsoft.AspNetCore.Components.Web;
using NLog;

namespace DataTrack.Pages
{
    public partial class Arm2
    {
        private (string value, Task t) _lastNotification;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private List<Silos> _siloses = new List<Silos>();
        private List<WeightTanker> _weightTankers = new List<WeightTanker>();
        private List<WeightTanker> _receiverTankers = new List<WeightTanker>();
        private List<Material> _loadedMaterial = new List<Material>();
        private List<Ingot> _ingots = new List<Ingot>();
        private List<IngotVisualParameters> _ingotsVisualParameters = new List<IngotVisualParameters>();
        // private IngotVisualParameters _ingotsVisualParameters;
        // private List<IngotVisualParameters> _visualParameters = new List<IngotVisualParameters>();
        private List<Rollgang> _rollgangs = new List<Rollgang>();
        private string[] _diviatorsDirection = new string[4];
        private string[] _conveyors = new string[6];

        private string _detailPosX;
        private string _detailPosY;
        private string _detailCaption;

        // Позиция сталевоза:
        //    1. top: 680px; left: 1500px;
        //    2. top: 680px; left: 1180px;
        private string _stalevozPos;
        // private string _stalevozPos1;
        // private string _stalevozPos2;
        private Statuses _status;

        private string _showed = "none";
        
        // Класс, заполняемый формой ручной загрузки весовых бункеров
        private readonly ManualLoadWeights _manualLoadWeights = new ManualLoadWeights();
        private readonly ManualLoadReceiver _manualLoadReceiver = new ManualLoadReceiver();
        private Dictionary<string, string> _avalibleSiloses = new Dictionary<string, string>();
        private bool[] _weightLoading = new bool[3];
        private bool[] _receiverLoading = new bool[4];
        private int _target; // 0 - ДСП, 1- УПК, 2 - Сторона УПК, 3 - Сторона ДСП

        protected override void OnInitialized()
        {
            // Точка входа на страницу
            Initialize();
        }
        
        private async Task OnNotify(string value)
        {
            await InvokeAsync(() =>
            {
                _lastNotification.value = value;
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            Kernel.Data.SetWeightTankers(_weightTankers);
            Kernel.Data.SetReceiverTankers(_receiverTankers);
            Kernel.Data.SetSiloses(_siloses);
            Kernel.Data.SetIngots(_ingots);
            Kernel.Data.SetRollgangs(_rollgangs);

            // Удаление рольганга 1
            foreach (Rollgang rollgang in _rollgangs)
            {
                rollgang.Moved -= IngotMoved;
                rollgang.Delivered -= IngotDelivered;
            }

            Notifier.Notify -= OnNotify;
        }

        /// <summary>
        /// Первоначальная настройка и установка значений по-умолчанию
        /// </summary>
        private async void Initialize()
        {
            Notifier.Notify += OnNotify;
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
                    silos.Name = $"Силос [{i}]";
                    _siloses.Add(silos);
                }
                
                // Установить силосы в ядре системы 
                Kernel.Data.SetSiloses(_siloses);
            }
            else
            {
                // Добавляем силоса из ядра системы
                _siloses = Kernel.Data.GetSiloses();
            }
            
            // Если в ядре нет рольгангов, то создадим их
            if (Kernel.Data.GetRollgangsCount() == 0)
            {
                // Рольганг 1
                Rollgang rollgang = new Rollgang(1, RollgangTypes.Horizontal, 1, 1.05F, 1);
                rollgang.SetPosition(new Coords(400, 510), new Coords(840, 510), new Coords(200, 350),
                    new Coords(225, 410));
                rollgang.Moved += IngotMoved;
                rollgang.Delivered += IngotDelivered;
                _rollgangs.Add(rollgang);
                
                // Рольганг 2
                rollgang = new Rollgang(2, RollgangTypes.Horizontal, 2, 0.7F, 1);
                rollgang.SetPosition(new Coords(750, 550), new Coords(960, 550), new Coords(305, 350),
                    new Coords(320, 410));
                rollgang.Moved += IngotMoved;
                rollgang.Delivered += IngotDelivered;
                _rollgangs.Add(rollgang);
                
                // Рольганг 3
                rollgang = new Rollgang(3, RollgangTypes.Vertical, 3, 0.85F, 1);
                rollgang.SetPosition(new Coords(1040, 480), new Coords(1040, 50), new Coords(300, 350),
                    new Coords(315, 395));
                rollgang.Moved += IngotMoved;
                rollgang.Delivered += IngotDelivered;
                _rollgangs.Add(rollgang);
                
                // Рольганг 4
                rollgang = new Rollgang(4, RollgangTypes.Horizontal, 4, 0.75F, 1);
                rollgang.SetPosition(new Coords(1080, 20), new Coords(1090, 20), new Coords(300, 350),
                    new Coords(315, 410));
                rollgang.Moved += IngotMoved;
                rollgang.Delivered += IngotDelivered;
                _rollgangs.Add(rollgang);
                
                // Рольганг 5
                rollgang = new Rollgang(5, RollgangTypes.Horizontal, 5, 1.0F, 1);
                rollgang.SetPosition(new Coords(1150, 70), new Coords(1400, 70), new Coords(300, 350),
                    new Coords(325, 410));
                rollgang.Moved += IngotMoved;
                rollgang.Delivered += IngotDelivered;
                _rollgangs.Add(rollgang);
                
                // Рольганг 6
                rollgang = new Rollgang(6, RollgangTypes.Horizontal, 6, 0.75F, 1);
                rollgang.SetPosition(new Coords(1350, 350), new Coords(1280, 350), new Coords(305, 350),
                    new Coords(325, 410));
                rollgang.Moved += IngotMoved;
                rollgang.Delivered += IngotDelivered;
                _rollgangs.Add(rollgang);
            }
            else
            {
                _rollgangs = Kernel.Data.GetRollgangs();

                foreach (Rollgang rollgang in _rollgangs)
                {
                    rollgang.Moved += IngotMoved;
                    rollgang.Delivered += IngotDelivered;
                }
            }

            if (Kernel.Data.GetWeightTankersCount() == 0)
            {
                // Создаем новые весовые бункера
                for (int i = 1; i < 4; i++)
                {
                    WeightTanker tanker = new WeightTanker(i, $"Весовой бункер {i}");
                    Statuses status = new Statuses();
                    status.CurrentState = Statuses.Status.Off;
                    status.StatusIcon = "img/led/MotorGrey.png";
                    status.StatusMessage = "";
                    tanker.SetStatus(status);
                    _weightTankers.Add(tanker);
                }

                for (int i = 1; i < 5; i++)
                {
                    string[] name =
                    {
                        "", 
                        "Приемочный бункер [ДСП]", 
                        "Приемочный бункер [УПК]", 
                        "Приемочный бункер [Сторона УПК]",
                        "Приемочный бункер [Сторона ДСП]"
                    };
                    WeightTanker tanker = new WeightTanker(i, name[i]);
                    Statuses status = new Statuses();
                    status.CurrentState = Statuses.Status.Off;
                    status.StatusIcon = "img/led/w1LedGrey.png";
                    status.StatusMessage = "";
                    tanker.SetStatus(status);
                    _receiverTankers.Add(tanker);
                }

                // Устанавливаем весовые и приемочные бункера в ядре системы
                Kernel.Data.SetWeightTankers(_weightTankers);
                Kernel.Data.SetReceiverTankers(_receiverTankers);
            }
            else
            {
                // Добавляем весовые и приемочные бункера из ядра системы
                    _weightTankers = Kernel.Data.GetWeightTankers();
                    _receiverTankers = Kernel.Data.GetReceiverTankers();
            }

            if (Kernel.Data.GetIngotsCount() == 0)
            {
                if (_ingots.Count > 0)
                {
                    Kernel.Data.SetIngots(_ingots);
                }
            }
            else
            {
                _ingots = Kernel.Data.GetIngots();
            }
            
            // Инициализация единиц учета для отображения на странице
            for (int i = 0; i < 18; i++)
            {
                IngotVisualParameters visualParameters = new IngotVisualParameters("img/colors/Empty.png");
                visualParameters.XPos = "0px";
                visualParameters.YPos = "0px";
                _ingotsVisualParameters.Add(visualParameters);
            }
            
            // Определение начальных координат для отображения единиц учета
            // Рольганг 1
            _ingotsVisualParameters[0].XPos = "400px";
            _ingotsVisualParameters[0].YPos = "510px";
            _ingotsVisualParameters[1].XPos = "400x";
            _ingotsVisualParameters[1].YPos = "510px";
            _ingotsVisualParameters[2].XPos = "400px";
            _ingotsVisualParameters[2].YPos = "510px";
            // Рольганг 2
            _ingotsVisualParameters[3].XPos = "750px";
            _ingotsVisualParameters[3].YPos = "550px";
            _ingotsVisualParameters[4].XPos = "750px";
            _ingotsVisualParameters[4].YPos = "550px";
            _ingotsVisualParameters[5].XPos = "750px";
            _ingotsVisualParameters[5].YPos = "550px";
            // Рольганг 3
            _ingotsVisualParameters[6].FileName = "img/colors/vEmpty.png";
            _ingotsVisualParameters[6].XPos = "1040px";
            _ingotsVisualParameters[6].YPos = "480px";
            _ingotsVisualParameters[7].FileName = "img/colors/vEmpty.png";
            _ingotsVisualParameters[7].XPos = "1040px";
            _ingotsVisualParameters[7].YPos = "480px";
            _ingotsVisualParameters[8].FileName = "img/colors/vEmpty.png";
            _ingotsVisualParameters[8].XPos = "1040px";
            _ingotsVisualParameters[8].YPos = "480px";
            // Рольганг 4
            _ingotsVisualParameters[9].XPos = "1080px";
            _ingotsVisualParameters[9].YPos = "20px";
            _ingotsVisualParameters[10].XPos = "1080px";
            _ingotsVisualParameters[10].YPos = "20px";
            _ingotsVisualParameters[11].XPos = "1080px";
            _ingotsVisualParameters[11].YPos = "20px";
            // Рольганг 5
            _ingotsVisualParameters[12].XPos = "1150px";
            _ingotsVisualParameters[12].YPos = "70px";
            _ingotsVisualParameters[13].XPos = "1150px";
            _ingotsVisualParameters[13].YPos = "70px";
            _ingotsVisualParameters[14].XPos = "1150px";
            _ingotsVisualParameters[14].YPos = "70px";
            // Рольганг 6
            _ingotsVisualParameters[15].XPos = "1350px";
            _ingotsVisualParameters[15].YPos = "350px";
            _ingotsVisualParameters[16].XPos = "1350px";
            _ingotsVisualParameters[16].YPos = "350px";
            _ingotsVisualParameters[17].XPos = "1350px";
            _ingotsVisualParameters[17].YPos = "350px";

            _manualLoadWeights.WeightNumber = "0";
            _manualLoadWeights.SilosNumber = "0";
            _avalibleSiloses.Add("0", "Силос 1");
            _avalibleSiloses.Add("1", "Силос 2");
            _avalibleSiloses.Add("2", "Силос 3");
            _diviatorsDirection[0] = "img/arm2/diviator_right.png";
            _diviatorsDirection[1] = "img/arm2/diviator_right.png";
            _diviatorsDirection[2] = "img/arm2/diviator_right.png";
            _diviatorsDirection[3] = "img/arm2/diviator_down.png";
            _conveyors[0] = "img/arm2/conveyor_long.png";
            _conveyors[1] = "img/arm2/conveyor_vertical_doublelong.png";
            _conveyors[2] = "img/arm2/RolgangVeryLongGrey.png";
            _conveyors[3] = "img/arm2/conveyor_short.png";
            _conveyors[4] = "img/arm2/RolgangLongGrey.png";
            _conveyors[5] = "img/arm2/conveyor_mid.png";
            
            _stalevozPos = "1180px;";
            _detailCaption = "";
            
            // Признак загрузки весовых бункера
            _weightLoading[0] = false;
            _weightLoading[1] = false;
            _weightLoading[2] = false;
            
            // Признак загрузки приемочных бункера
            _receiverLoading[0] = false;
            _receiverLoading[1] = false;
            _receiverLoading[2] = false;
            _receiverLoading[3] = false;

            await OnNotify("Готов");
        }
        
        private void ShowMaterial(MouseEventArgs e, int number)
        {
            int matCount = 0;
            
            // Определяем тип объекта, который вызвал метод:
            // number<10 - силос, number>10 - весовой бункер
            if (number < 10)
            {
                matCount = _siloses[number - 1].GetLayersCount();
                _loadedMaterial = _siloses[number - 1].GetMaterials();
                _detailCaption = _siloses[number - 1].Name;
            }
            else if (number < 13)
            {
                matCount = _weightTankers[number - 10].GetLayersCount();
                _loadedMaterial = _weightTankers[number - 10].GetMaterials();
                _detailCaption = _weightTankers[number - 10].Name;
            }
            else if (number < 17)
            {
                matCount = _receiverTankers[number - 13].GetLayersCount();
                _loadedMaterial = _receiverTankers[number - 13].GetMaterials();
                _detailCaption = _receiverTankers[number - 13].Name;
            }
            else if (number < 35)
            {
                uint ingotUid = _ingotsVisualParameters[number - 17].IngotUid;
                if (ingotUid > 0)
                {
                    Ingot ingot = GetIngotByUid(ingotUid);
                    matCount = ingot.GetMaterialsCount();

                    if (matCount > 0)
                    {
                        _loadedMaterial = ingot.GetMaterials();
                        _detailCaption = "Плавка №" + ingot.PlavNo;
                    }
                }
            }

            _detailPosY = $"{e.ClientY + 20}px";
            _detailPosX = $"{e.ClientX + 10}px";

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
        /// Обновление списка доступных для загрузки силосов
        /// </summary>
        /// <param name="value">Номер выбранного весового бункера</param>
        private void GetSilosesList(string value)
        {
            _avalibleSiloses = new Dictionary<string, string>();
            _manualLoadWeights.WeightNumber = value;
            switch (value)
            {
                case "0":
                {
                    _avalibleSiloses.Add("0", "Силос 1");
                    _avalibleSiloses.Add("1", "Силос 2");
                    _avalibleSiloses.Add("2", "Силос 3");
                    _manualLoadWeights.SilosNumber = "0";
                    break;
                }
                case "1":
                {
                    _avalibleSiloses.Add("3", "Силос 4");
                    _avalibleSiloses.Add("4", "Силос 5");
                    _avalibleSiloses.Add("5", "Силос 6");
                    _manualLoadWeights.SilosNumber = "3";
                    break;
                }
                case "2":
                {
                    _avalibleSiloses.Add("6", "Силос 7");
                    _avalibleSiloses.Add("7", "Силос 8");
                    _manualLoadWeights.SilosNumber = "6";
                    break;
                }
            }
            StateHasChanged();
        }

        /// <summary>
        /// Установить номер выбранного силоса
        /// </summary>
        /// <param name="value"></param>
        private void SetSilosNumber(string value)
        {
            _manualLoadWeights.SilosNumber = value;
        }

        /// <summary>
        /// Задержка на указанное время секунд
        /// </summary>
        /// <param name="time">Задержка в секундах</param>
        /// <returns></returns>
        private async Task Waiting(int time)
        {
            await Task.Delay(TimeSpan.FromSeconds(time));
            StateHasChanged();
        }

        /// <summary>
        /// Установить дивиаторы в исходное состояние
        /// </summary>
        private void ClearTarget()
        {
            _conveyors[0] = "img/arm2/conveyor_long.png";
            _conveyors[1] = "img/arm2/conveyor_vertical_doublelong.png";
            _conveyors[2] = "img/arm2/RolgangVeryLongGrey.png";
            _conveyors[3] = "img/arm2/conveyor_short.png";
            _conveyors[4] = "img/arm2/RolgangLongGrey.png";
            _conveyors[5] = "img/arm2/conveyor_mid.png";
            _diviatorsDirection[0] = "img/arm2/diviator_right.png";
            _diviatorsDirection[1] = "img/arm2/diviator_right.png";
            _diviatorsDirection[2] = "img/arm2/diviator_right.png";
            _diviatorsDirection[3] = "img/arm2/diviator_down.png";
            StateHasChanged();
        }

        /// <summary>
        /// Проверка признака загрузки приемочного бункера
        /// </summary>
        /// <returns>Признак загрузки приемочного бункера</returns>
        private bool IsReceiverLoading()
        {
            bool res = false;
            foreach (bool tanker in _receiverLoading)
            {
                if (tanker)
                {
                    res = true;
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Проверка признака загрузки весового бункера
        /// </summary>
        /// <returns>Признак загрузки весового бункера</returns>
        private bool IsWeightLoading()
        {
            bool res = false;
            foreach (bool tanker in _weightLoading)
            {
                if (tanker)
                {
                    res = true;
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Установить дивиаторы на цель
        /// </summary>
        private async void SetTarget()
        {
            string target = "";
            switch (_target)
            {
                case 0: target = "ДСП"; break;
                case 1: target = "УПК"; break;
                case 2: target = "Сторона УПК"; break;
                case 3: target = "Сторона ДСП"; break;
            }
            
            if (!IsReceiverLoading())
            {
                string value = _manualLoadReceiver.ReceiverTanker;

                // Устанавливаем направления дивиаторов на цель
                switch (value)
                {
                    case "0":
                    {
                        // ДСП
                        _stalevozPos = "1500px;";
                        _diviatorsDirection[0] = "img/arm2/diviator_right.png";
                        _target = 0;
                        break;
                    }
                    case "1":
                    {
                        // УПК
                        _stalevozPos = "1180px;";
                        _diviatorsDirection[0] = "img/arm2/diviator_left.png";
                        _diviatorsDirection[1] = "img/arm2/diviator_left.png";
                        _target = 1;
                        break;
                    }
                    case "2":
                    {
                        // Сторона УПК
                        _stalevozPos = "1500px;";
                        _diviatorsDirection[0] = "img/arm2/diviator_left.png";
                        _diviatorsDirection[1] = "img/arm2/diviator_right.png";
                        _diviatorsDirection[2] = "img/arm2/diviator_left.png";
                        _target = 2;
                        break;
                    }
                    case "3":
                    {
                        // Сторона ДСП
                        _stalevozPos = "1500px;";
                        _diviatorsDirection[0] = "img/arm2/diviator_left.png";
                        _diviatorsDirection[1] = "img/arm2/diviator_right.png";
                        _diviatorsDirection[2] = "img/arm2/diviator_right.png";
                        _target = 3;
                        break;
                    }
                }
            }
            else
            {
                _logger.Error($"Нельзя сменить приемочный бункер в процессе его загрузки!");
                Debug.WriteLine($"Нельзя сменить приемочный бункер в процессе его загрузки!");
                await OnNotify($"Дождитесь завершения загрузки приемочного бункера [{target}]");
            }
        }

        /// <summary>
        /// Начало загрузки материала в весовой бункер
        /// </summary>
        private async void LoadWeight()
        {
            // Номер весового бункера, принимающего материал
            if (/* !_weightLoading && !_receiverLoading */ true) // \\>>>
            {
                // Не производится загрузка весового или приемочного бункера
                int weightTanker = Int32.Parse(_manualLoadWeights.WeightNumber);

                // Номер силоса, отдающего материал
                int silos = Int32.Parse(_manualLoadWeights.SilosNumber);

                // Проверяем наличие весового бункера
                if (_weightTankers[weightTanker] != null)
                {
                    // Если весовой бункер готов к приему материала
                    if (_weightTankers[weightTanker].GetStatus().CurrentState == Statuses.Status.Off)
                    {
                        // Проверяем наличие силоса
                        if (_siloses[silos] != null)
                        {
                            // Если силос не готов к разгрузке
                            if (_siloses[silos].GetStatus().CurrentState == Statuses.Status.Off)
                            {
                                // Проверяем наличие материала в силосе, из которого будет загрузка материала
                                if (_siloses[silos].GetLayersCount() > 0)
                                {
                                    // Проверяем наличие свободного места в весовом бункере
                                    double placed = _weightTankers[weightTanker].GetWeight();
                                    double fulled = _weightTankers[weightTanker].MaxWeight;
                                    if (fulled > placed)
                                    {
                                        // Загружем либо весь материал из силоса, либо количество материала, до полного наполнения весового буненра
                                        // Устанавливаем флаг загрузки весового бункера
                                        // _weightLoading = true; \\>>>
                                        await OnNotify(
                                            $"Начата загрузка весового бункера {weightTanker + 1} из силоса {silos + 1}");
                                        _status = new Statuses();
                                        _status.StatusIcon = "img/led/MotorGreen.png";
                                        _status.StatusMessage = "ЗАГРУЗКА";
                                        _status.CurrentState = Statuses.Status.Loading;
                                        _weightTankers[weightTanker].SetStatus(_status);

                                        _status = new Statuses();
                                        _status.StatusIcon = "img/led/SmallYellow.png";
                                        _status.StatusMessage = "ВЫГРУЗКА";
                                        _status.CurrentState = Statuses.Status.Unloading;
                                        _siloses[silos].SetStatus(_status);

                                        // Задержка на время загрузки 10 секунд
                                        await Waiting(10);
                                        bool result = _weightTankers[weightTanker]
                                            .LoadMaterial(_siloses[silos], fulled - placed);

                                        // Сброс теккущего состояния силоса и зарузочного бункера
                                        Statuses stateSilos = new Statuses();
                                        Statuses stateWeight = new Statuses();
                                        if (result)
                                        {
                                            stateSilos.CurrentState = Statuses.Status.Off;
                                            stateSilos.StatusIcon = "img/led/SmallGrey.png";
                                            stateSilos.StatusMessage = "";

                                            stateWeight.CurrentState = Statuses.Status.Off;
                                            stateWeight.StatusIcon = "img/led/MotorGrey.png";
                                            stateWeight.StatusMessage = "";
                                            
                                            await OnNotify($"Загрузка весового бункера {weightTanker + 1} завершена");
                                        }
                                        else
                                        {
                                            stateSilos.CurrentState = Statuses.Status.Error;
                                            stateSilos.StatusIcon = "img/led/SmallRed.png";
                                            stateSilos.StatusMessage = "ОШИБКА";

                                            stateWeight.CurrentState = Statuses.Status.Error;
                                            stateWeight.StatusIcon = "img/led/MotorRed.png";
                                            stateWeight.StatusMessage = "ОШИБКА";

                                            await OnNotify($"Ошибка при загрузке весового бункера {weightTanker + 1}");
                                        }

                                        _siloses[silos].SetStatus(stateSilos);
                                        _weightTankers[weightTanker].SetStatus(stateWeight);
                                        // _weightLoading = false; \\>>>

                                        StateHasChanged();

                                    }
                                    else
                                    {
                                        _logger.Error($"В весовом бункере {weightTanker + 1} нет места!");
                                        Debug.WriteLine($"В весовом бункере {weightTanker + 1} нет места!");
                                        await OnNotify($"В весовом бункере {weightTanker + 1} нет места!");
                                    }
                                }
                                else
                                {
                                    _logger.Error($"Силос {silos + 1} пуст!");
                                    Debug.WriteLine($"Силос {silos + 1} пуст!");
                                    await OnNotify($"Силос {silos + 1} пуст!");
                                }
                            }
                            else
                            {
                                _logger.Error(
                                    $"Силос {silos + 1} находится в состоянии {_siloses[silos].Status.CurrentState.ToString()}");
                                Debug.WriteLine(
                                    $"Силос {silos + 1} находится в состоянии {_siloses[silos].Status.CurrentState.ToString()}");
                                await OnNotify(
                                    $"Силос {silos + 1} находится в состоянии {_siloses[silos].Status.CurrentState.ToString()}");
                            }
                        }
                        else
                        {
                            _logger.Error($"Силос {silos + 1} не существует!");
                            Debug.WriteLine($"Силос {silos + 1} не существует!");
                            await OnNotify($"Силос {silos + 1} не существует!");
                        }
                    }
                    else
                    {
                        _logger.Error(
                            $"Весовой бункер {weightTanker + 1} находится в состоянии {_weightTankers[weightTanker].Status.CurrentState.ToString()}");
                        Debug.WriteLine(
                            $"Весовой бункер {weightTanker + 1} находится в состоянии {_weightTankers[weightTanker].Status.CurrentState.ToString()}");
                        await OnNotify(
                            $"Весовой бункер {weightTanker + 1} находится в состоянии {_weightTankers[weightTanker].Status.CurrentState.ToString()}");
                    }
                }
                else
                {
                    _logger.Error($"Весовой бункер {weightTanker + 1} не существует!");
                    Debug.WriteLine($"Весовой бункер {weightTanker + 1} не существует!");
                    await OnNotify($"Весовой бункер {weightTanker + 1} не существует!");
                }
            }
            // else
            // {
            //     // if (_weightLoading)
            //     // {
            //     //     // Производится загрузка весового бункера
            //     //     _logger.Error("Одновременная загрузка нескольких весовых бунеров недопустима!");
            //     //     Debug.WriteLine("Одновременная загрузка нескольких весовых бунеров недопустима!");
            //     //     await OnNotify("Одновременная загрузка нескольких весовых бунеров недопустима!");
            //     // }
            //
            //     // if (_receiverLoading)
            //     // {
            //     //     // Производится загрузка приемочного бункера
            //     //     _logger.Error("Необходимо дождаться окончания загрузки приемочного бункера");
            //     //     Debug.WriteLine("Необходимо дождаться окончания загрузки приемочного бункера");
            //     //     await OnNotify("Необходимо дождаться окончания загрузки приемочного бункера");
            //     //     
            //     // }
            // }
        }
        
        /// <summary>
        /// Начало загрузки заданного количества материала в весовой бункер
        /// </summary>
        private void LoadWeight(double weight)
        {
            // Номер весового бункера, принимающего материал
            int weightTanker = Int32.Parse(_manualLoadWeights.WeightNumber);
            
            // Номер силоса, отдающего материал
            int silos = Int32.Parse(_manualLoadWeights.SilosNumber);
            
            // Проверяем наличие весового бункера в ядре системы
            // Проверяем наличие силоса в ядре системы
            // Проверяем наличие материала в силосе, из которого будет загрузка материала
            // Проверяем наличие свободного места в весовом бункере
            // Расчитываем вес загружаемого материала (если максимальный вес бункера минус вес загруженного материала
            //     меньше веса загружаемого материала, то уменьшить вес загружаемого материала, чтобы он не превышал
            //     максимальный вес бункера
            // Загружем либо весь материал из силоса, либо количество материала, до полного наполнения весового буненра
        }

        /// <summary>
        /// Получить единицу учета по ее UID
        /// </summary>
        /// <param name="uid">UID единицы учета</param>
        /// <returns>Единица учета</returns>
        private Ingot GetIngotByUid(uint uid)
        {
            Ingot res = null;
            foreach (Ingot ingot in _ingots)
            {
                if (ingot.Uid == uid)
                {
                    res = ingot;
                    break;
                }
            }
            return res;
        }

        /// <summary>
        /// Получить следующий унркальный идентификатор единицы учета
        /// </summary>
        /// <returns></returns>
        private uint GetNextIngotId()
        {
            uint next = 0;
            foreach (Ingot ingot in _ingots)
            {
                if (ingot.Uid > next)
                {
                    next = ingot.Uid;
                }
            }

            if (next == uint.MaxValue)
            {
                next = 1;
            }
            else
            {
                next++;
            }

            return next;
        }

        private async Task PlaceOnRolgang(int number, Ingot ingot)
        {
            Rollgang rollgang = _rollgangs[number - 1];
            int ingotNum = FindNextIngotNumber(number);
            if (ingotNum >= 0)
            {
                // Есть свободная ячейка для отображения
                _ingotsVisualParameters[ingotNum].IngotUid = ingot.Uid;
                await rollgang.Delivering(ingot); 
            }
        }
        
        /// <summary>
        /// Тестирование модуля сопровождения единицы учета
        /// </summary>
        private async Task DeliverIngots(Ingot ingot)
        {
            // Определяем назначение доставки единицы учета
            string targetNum = _manualLoadReceiver.ReceiverTanker;
            string targetMsg = "";
            bool useRollgang6 = false;
            Selection[] diviators = new Selection[3];
            for (int i = 0; i < 3; i++)
            {
                diviators[i] = new Selection();
            }

            switch (targetNum)
            {
                case "0" :
                {
                    targetMsg = "ДСП";
                    diviators[0].Activate = true;
                    diviators[0].Selected = "img/arm2/diviator_right_green.png";
                    diviators[0].Deselected = "img/arm2/diviator_right.png";
                    break;
                }
                case "1" :
                {
                    diviators[0].Activate = true;
                    diviators[0].Selected = "img/arm2/diviator_left_green.png";
                    diviators[0].Deselected = "img/arm2/diviator_left.png";
                    diviators[1].Activate = true;
                    diviators[1].Selected = "img/arm2/diviator_left_green.png";
                    diviators[1].Deselected = "img/arm2/diviator_left.png";
                    useRollgang6 = true;
                    targetMsg = "УПК";
                    break;
                }
                case "2" :
                {
                    diviators[0].Activate = true;
                    diviators[0].Selected = "img/arm2/diviator_left_green.png";
                    diviators[0].Deselected = "img/arm2/diviator_left.png";
                    diviators[1].Activate = true;
                    diviators[1].Selected = "img/arm2/diviator_right_green.png";
                    diviators[1].Deselected = "img/arm2/diviator_right.png";
                    diviators[2].Activate = true;
                    diviators[2].Selected = "img/arm2/diviator_left_green.png";
                    diviators[2].Deselected = "img/arm2/diviator_left.png";
                    targetMsg = "Сторона УПК";
                    break;
                }
                case "3" :
                {
                    diviators[0].Activate = true;
                    diviators[0].Selected = "img/arm2/diviator_left_green.png";
                    diviators[0].Deselected = "img/arm2/diviator_left.png";
                    diviators[1].Activate = true;
                    diviators[1].Selected = "img/arm2/diviator_right_green.png";
                    diviators[1].Deselected = "img/arm2/diviator_right.png";
                    diviators[2].Activate = true;
                    diviators[2].Selected = "img/arm2/diviator_right_green.png";
                    diviators[2].Deselected = "img/arm2/diviator_right.png";
                    targetMsg = "Сторона ДСП";
                    break;
                }
            }
            
            await OnNotify($"[{ingot.GetStartTime()}] Начата доставка единицы учета №{ingot.Uid} к {targetMsg}");
            Console.ForegroundColor = ingot.Color;
            Console.WriteLine($"[{ingot.Uid}] {ingot.GetStartTime()} => {ingot.GetFinishTime()} ({ingot.Color.ToString()})");

            await PlaceOnRolgang(1, ingot);
            await PlaceOnRolgang(2, ingot);
            await PlaceOnRolgang(3, ingot);
            await PlaceOnRolgang(4, ingot);
            await PlaceOnRolgang(5, ingot);

            if (diviators[0].Activate)
            {
                // Выделение цветом дивиатора 1
                _diviatorsDirection[0] = diviators[0].Selected;
                await Waiting(3);
                _diviatorsDirection[0] = diviators[0].Deselected;
            }
            if (diviators[1].Activate)
            {
                // Выделение цветом дивиатора 2
                _diviatorsDirection[1] = diviators[1].Selected;
                await Waiting(3);
                _diviatorsDirection[1] = diviators[1].Deselected;
            }
            if (diviators[2].Activate)
            {
                // Выделение цветом дивиатора 3
                _diviatorsDirection[2] = diviators[2].Selected;
                await Waiting(3);
                _diviatorsDirection[2] = diviators[2].Deselected;
            }

            if (useRollgang6)
            {
                await PlaceOnRolgang(6, ingot);
            }

            await OnNotify($"Единица учета №{ingot.Uid} была доставлена");
        }

        /// <summary>
        /// Получить номер следующей свободной ячейки для отображения единицы учета
        /// </summary>
        /// <param name="rollgangNumer">Номер рольганга</param>
        /// <returns>Номер ячейки для отображения кдиницы учета</returns>
        private int FindNextIngotNumber(int rollgangNumer)
        {
            int band = rollgangNumer * 3 - 3;
            int res = -1;

            if (_ingotsVisualParameters[band].IngotUid == 0)
            {
                res = band;
            }
            else if (_ingotsVisualParameters[band + 1].IngotUid == 0)
            {
                res = band + 1;
            }
            else if (_ingotsVisualParameters[band + 2].IngotUid == 0)
            {
                res = band + 2;
            }

            return res;
        }

        /// <summary>
        /// Получить номер свободной единицы учета
        /// </summary>
        /// <returns>Номер свободной единицы учета</returns>
        private int FindIngotById(uint ingotUid)
        {
            int res = -1;
            for (int i = 0; i < _ingotsVisualParameters.Count; i++)
            {
                if (_ingotsVisualParameters[i].IngotUid == ingotUid)
                {
                    res = i;
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Перемещение единицы учета по рольгангу
        /// </summary>
        /// <param name="ingot">Единица учета</param>
        private void IngotMoved(Ingot ingot)
        {
            int ingotNum = FindIngotById(ingot.Uid);
            _ingotsVisualParameters[ingotNum].XPos = ingot.VisualParameters.XPos;
            _ingotsVisualParameters[ingotNum].YPos = ingot.VisualParameters.YPos;
            _ingotsVisualParameters[ingotNum].FileName = ingot.VisualParameters.FileName;
            _ingotsVisualParameters[ingotNum].ZIndex = "1";
            StateHasChanged();
        }
        
        /// <summary>
        /// Доставка единицы учета по рольгангу завершена
        /// </summary>
        /// <param name="ingot">Доставленная единица учета</param>
        private void IngotDelivered(Ingot ingot)
        {
            int ingotNum = FindIngotById(ingot.Uid);
            _ingotsVisualParameters[ingotNum].FileName = "img/colors/Empty.png";
            _ingotsVisualParameters[ingotNum].XPos = "400px";
            _ingotsVisualParameters[ingotNum].YPos = "510px";
            _ingotsVisualParameters[ingotNum].IngotUid = 0;
            _ingotsVisualParameters[ingotNum].ZIndex = "-1";
            // await OnNotify($"Материал №{ingot.Uid} был доставлен");
            StateHasChanged();
        }

        /// <summary>
        /// Загрузка материала в приемочный бункер
        /// </summary>
        private async void LoadReceiver()
        {
            if (/* !_weightLoading && !_receiverLoading */ true)
            {
                int weightTanker = Int32.Parse(_manualLoadReceiver.WeightTanker); // Номер весового бункера
                int receiverTanker = Int32.Parse(_manualLoadReceiver.ReceiverTanker); // Номер приемочного бункера
                double weight = _manualLoadReceiver.Weight; // Вес загружаемого материала
                double availible; // Свободное место в приемочном бункере
                string message = "";

                if (weight > 0)
                {
                    // Определить наличие приемочного бункера в ядре системы
                    if (_receiverTankers[receiverTanker] != null)
                    {
                        // Определить наличие весового бункера в ядре системы
                        if (_weightTankers[weightTanker] != null)
                        {
                            // Определить статус приемочного бункера
                            if (_weightTankers[weightTanker].GetCurrentState() == Statuses.Status.Off)
                            {
                                // Определить статус весового бункера и направление уже производящейся загрузки
                                // if (_receiverTankers[receiverTanker].GetCurrentState() == Statuses.Status.Off)
                                if ((!IsReceiverLoading() && _receiverTankers[receiverTanker].GetCurrentState() ==
                                        Statuses.Status.Off) || _receiverTankers[receiverTanker].GetCurrentState() ==
                                    Statuses.Status.Loading)
                                {
                                    availible = _receiverTankers[receiverTanker].GetAvailibleWeight();
                                    // Определить свободное место в приемочном бункере
                                    if (availible > 0)
                                    {
                                        if (weight > availible)
                                        {
                                            weight = availible;
                                        }

                                        // Определить наличие материала в весовом бункера
                                        if (_weightTankers[weightTanker].GetLayersCount() > 0)
                                        {
                                            // Устанавливаем текущие состояния для весового и приемочного бункера
                                            SetTarget();
                                            _receiverLoading[receiverTanker] = true;
                                            await OnNotify(
                                                $"Начата загрузка приемочного бункера {receiverTanker + 1} из загрузочного бункера {weightTanker + 1}");

                                            // Весовой бункер
                                            Statuses state = new Statuses();
                                            state.CurrentState = Statuses.Status.Unloading;
                                            state.StatusMessage = "ВЫГРУЗКА";
                                            state.StatusIcon = "img/led/MotorYellow.png";
                                            _weightTankers[weightTanker].SetStatus(state);

                                            // Приемочный бункер
                                            state = new Statuses();
                                            state.CurrentState = Statuses.Status.Loading;
                                            state.StatusMessage = "ЗАГРУЗКА";
                                            state.StatusIcon = "img/led/w1LedGreen.png";
                                            _receiverTankers[receiverTanker].SetStatus(state);

                                            // Создание единицы учета
                                            uint uid = GetNextIngotId(); // ищем следующий номер UID единицы учета
                                            Ingot ingot = new Ingot(uid);
                                            List<Material> materials = _weightTankers[weightTanker].Unload(weight);
                                            ingot.AddMaterials(materials);
                                            _ingots.Add(ingot);

                                            // Перемещение единицы учета по рольгангам
                                            //TODO: Посмотреть, почему обрывается загрузка приемочного бункера при обновлении страницы
                                            await DeliverIngots(ingot);

                                            // Загружаем требуемый вес материала, либо вес до максимальной загрузки приемочного бункера
                                            _receiverTankers[receiverTanker].SetIngot(ingot);

                                            // Снимаем текущие состояния весового и приемочного бункеров

                                            // Весовой бункер
                                            state = new Statuses();
                                            state.CurrentState = Statuses.Status.Off;
                                            state.StatusMessage = "";
                                            state.StatusIcon = "img/led/MotorGrey.png";
                                            _weightTankers[weightTanker].SetStatus(state);

                                            // Приемочный бункер
                                            state = new Statuses();
                                            state.CurrentState = Statuses.Status.Off;
                                            state.StatusMessage = "";
                                            state.StatusIcon = "img/led/w1LedGrey.png";
                                            _receiverTankers[receiverTanker].SetStatus(state);
                                            await OnNotify($"Приемочный бункер {receiverTanker + 1} загружен");
                                            _receiverLoading[receiverTanker] = false;
                                        }
                                        else
                                        {
                                            _logger.Error($"{_weightTankers[weightTanker].Name} пуст!");
                                            Debug.WriteLine($"{_weightTankers[weightTanker].Name} пуст!");
                                            await OnNotify($"{_weightTankers[weightTanker].Name} пуст!");
                                        }
                                    }
                                    else
                                    {
                                        _logger.Error($"{_receiverTankers[receiverTanker].Name} заполнен!");
                                        Debug.WriteLine($"{_receiverTankers[receiverTanker].Name} заполнен!");
                                        await OnNotify($"{_receiverTankers[receiverTanker].Name} заполнен!");
                                    }
                                }
                                else
                                {
                                    if (!IsReceiverLoading() && _receiverTankers[receiverTanker].GetCurrentState() ==
                                        Statuses.Status.Off)
                                    {
                                        message =
                                            $"{_receiverTankers[receiverTanker].Name} не готов к приему материала!";
                                    }
                                    else
                                    {
                                        message = "Дождитесь завершения операции загрузки приемочного бункера!";
                                    }
                                    _logger.Error(message);
                                    Debug.WriteLine(message);
                                    await OnNotify(message);
                                }
                            }
                            else
                            {
                                switch (_weightTankers[weightTanker].GetCurrentState())
                                {
                                    case Statuses.Status.Error:
                                    {
                                        message = $"{_weightTankers[weightTanker].Name} в состоянии ошибки!";
                                        break;
                                    }
                                    case Statuses.Status.Unloading:
                                    {
                                        message = $"Дождитесь окончания разгрузки {_weightTankers[weightTanker].Name}";
                                        break;
                                    }
                                    case Statuses.Status.Loading:
                                    {
                                        message = $"Дождитесь окончания загрузки {_weightTankers[weightTanker].Name}";
                                        break;
                                    }
                                }

                                _logger.Error(message);
                                Debug.WriteLine(message);
                                await OnNotify(message);
                            }
                        }
                        else
                        {
                            _logger.Error($"Весовой бункер {weightTanker + 1} не определен");
                            Debug.WriteLine($"Весовой бункер {weightTanker + 1} не определен");
                            await OnNotify($"Весовой бункер {weightTanker + 1} не определен");
                        }
                    }
                    else
                    {
                        _logger.Error($"Приемочный бункер {receiverTanker + 1} не определен");
                        Debug.WriteLine($"Приемочный бункер {receiverTanker + 1} не определен");
                        await OnNotify($"Приемочный бункер {receiverTanker + 1} не определен");
                    }
                }
                else
                {
                    _logger.Error($"Вес загружаемого материала не может составлять {weight} кг!");
                    Debug.WriteLine($"Вес загружаемого материала не может составлять {weight} кг!");
                    await OnNotify($"Вес загружаемого материала не может составлять {weight} кг!");
                }
            }
            // else
            // {
            //     // if (_weightLoading)
            //     // {
            //     //     // Производится загрузка весового бункера
            //     //     _logger.Error("Необходимо дождаться окончания загрузки весового бункера!");
            //     //     Debug.WriteLine("Необходимо дождаться окончания загрузки весового бункера!");
            //     //     await OnNotify("Необходимо дождаться окончания загрузки весового бункера!");
            //     // }
            //
            //     // if (_receiverLoading)
            //     // {
            //     //     // Производится загрузка приемочного бункера
            //     //     _logger.Error("Одновременная загрузка нескольких приемочных бунеров недопустима!");
            //     //     Debug.WriteLine("Одновременная загрузка нескольких приемочных бунеров недопустима!");
            //     //     await OnNotify("Одновременная загрузка нескольких приемочных бунеров недопустима!");
            //     // }
            // }
        }

        /// <summary>
        /// Сбросить состояние ошибки для бункера
        /// </summary>
        private async void ResetErrorTanker(int tankerType)
        {
            switch (tankerType)
            {
                case 1:
                {
                    // Весовой бункер
                    int tankerNumber = Int32.Parse(_manualLoadWeights.WeightNumber);
                    if(_weightTankers[tankerNumber].Status.CurrentState == Statuses.Status.Error)
                    {
                        Statuses state = new Statuses();
                        state.CurrentState = Statuses.Status.Off;
                        state.StatusMessage = "";
                        state.StatusIcon = "img/led/MotorGrey.png";
                        _weightTankers[tankerNumber].SetStatus(state);
                        _logger.Warn($"Был произведен сброс [{_weightTankers[tankerNumber].Name}]!");
                        Debug.WriteLine($"Был произведен сброс [{_weightTankers[tankerNumber].Name}]!");
                        await OnNotify($"Был произведен сброс [{_weightTankers[tankerNumber].Name}]!");
                    }                    
                    break;
                }
                case 2:
                {
                    // Приемлчный бункер
                    int tankerNumber = Int32.Parse(_manualLoadReceiver.ReceiverTanker);
                    if(_receiverTankers[tankerNumber].Status.CurrentState == Statuses.Status.Error)
                    {
                        Statuses state = new Statuses();
                        state.CurrentState = Statuses.Status.Off;
                        state.StatusMessage = "";
                        state.StatusIcon = "img/led/w1LedGrey.png";
                        _receiverTankers[tankerNumber].SetStatus(state);
                        _logger.Warn($"Был произведен сброс [{_receiverTankers[tankerNumber].Name}]!");
                        Debug.WriteLine($"Был произведен сброс [{_receiverTankers[tankerNumber].Name}]!");
                        await OnNotify($"Был произведен сброс [{_receiverTankers[tankerNumber].Name}]!");
                    }    
                    break;
                }
            }
        }

        /// <summary>
        /// Обнулить бункер 
        /// </summary>
        private async void ResetTanker(int tankerType)
        {
            switch (tankerType)
            {
                case 1:
                {
                    // Весовой бункер
                    int tankerNumber = Int32.Parse(_manualLoadWeights.WeightNumber);
                    Statuses state = new Statuses();
                    state.CurrentState = Statuses.Status.Off;
                    state.StatusMessage = "";
                    state.StatusIcon = "img/led/MotorGrey.png";
                    _weightTankers[tankerNumber].Reset();
                    _weightTankers[tankerNumber].SetStatus(state);
                    _logger.Warn($"Был произведен сброс [{_weightTankers[tankerNumber].Name}]!");
                    Debug.WriteLine($"Был произведен сброс [{_weightTankers[tankerNumber].Name}]!");
                    await OnNotify($"Был произведен сброс [{_weightTankers[tankerNumber].Name}]!");
                    break;
                }
                case 2:
                {
                    // Приемлчный бункер
                    int tankerNumber = Int32.Parse(_manualLoadReceiver.ReceiverTanker);
                    Statuses state = new Statuses();
                    state.CurrentState = Statuses.Status.Off;
                    state.StatusMessage = "";
                    state.StatusIcon = "img/led/w1LedGrey.png";
                    _receiverTankers[tankerNumber].Reset();
                    _receiverTankers[tankerNumber].SetStatus(state);
                    _logger.Warn($"Был произведен сброс [{_receiverTankers[tankerNumber].Name}]!");
                    Debug.WriteLine($"Был произведен сброс [{_receiverTankers[tankerNumber].Name}]!");
                    await OnNotify($"Был произведен сброс [{_receiverTankers[tankerNumber].Name}]!");
                    break;
                }
            }
        }
    }
}

