﻿using System;
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
        private List<WeightTanker> _weights = new List<WeightTanker>();
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
        private bool _weightLoading;
        private bool _receiverLoading;

        protected override void OnInitialized()
        {
            Notifier.Notify += OnNotify;
            
            // Точка входа на страницу
            // Проверить наличие силосов и прочих узлов в ядре системе!
            Initialize();
            _siloses = Kernel.Data.GetSiloses();
            // int kernelMaterialIndex = Kernel.Data.GetCurrentMaterialIndex();
            _stalevozPos = "1180px;";
            // _stalevozPos2 = "top: 680px; left: 1500px;";
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
            Notifier.Notify -= OnNotify;
            Kernel.Data.SetWeightTankers(_weights);
            Kernel.Data.SetSiloses(_siloses);
            Kernel.Data.SetIngots(_ingots);
            Kernel.Data.SetRollgangs(_rollgangs);

            // Удаление рольганга 1
            foreach (Rollgang rollgang in _rollgangs)
            {
                rollgang.Moved -= IngotMoved;
                rollgang.Delivered -= IngotDelivered;
            }
        }

        /// <summary>
        /// Первоначальная настройка и установка значений по-умолчанию
        /// </summary>
        private async void Initialize()
        {
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
                    _weights.Add(tanker);
                }

                for (int i = 4; i < 8; i++)
                {
                    string[] name = {"", "", "", "", "ДСП", "УПК", "Сторона УПК", "Сторона ДСП"};
                    WeightTanker tanker = new WeightTanker(i, name[i]);
                    Statuses status = new Statuses();
                    status.CurrentState = Statuses.Status.Off;
                    status.StatusIcon = "img/led/w1LedGrey.png";
                    status.StatusMessage = "";
                    tanker.SetStatus(status);
                    _weights.Add(tanker);
                }

                // Устанавливаем весовые бункера в ядре системы
                Kernel.Data.SetWeightTankers(_weights);
            }
            else
            {
                // Добавляем весовые бункера из ядра системы
                    _weights = Kernel.Data.GetWeightTankers();
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
            
            _detailCaption = "";
            
            // Сбрасываем флаги загрузки весового и приемочного бункеров
            _weightLoading = false;
            _receiverLoading = false;
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
                _detailCaption = "";
            }
            else if (number < 17)
            {
                matCount = _weights[number - 10].GetLayersCount();
                _loadedMaterial = _weights[number - 10].GetMaterials();
                _detailCaption = _weights[number - 10].Name;
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
        /// Установить дивиаторы на цель
        /// </summary>
        private void SetTarget()
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
                   break;
               }
               case "1":
               {
                   // УПК
                   _stalevozPos = "1180px;";
                   _diviatorsDirection[0] = "img/arm2/diviator_left.png";
                   _diviatorsDirection[1] = "img/arm2/diviator_left.png";
                   break;
               }
               case "2":
               {
                   // Сторона УПК
                   _stalevozPos = "1500px;";
                   _diviatorsDirection[0] = "img/arm2/diviator_Left.png";
                   _diviatorsDirection[1] = "img/arm2/diviator_Right.png";
                   _diviatorsDirection[2] = "img/arm2/diviator_Left.png";
                   break;
               }
               case "3":
               {
                   // Сторона ДСП
                   _stalevozPos = "1500px;";
                   _diviatorsDirection[0] = "img/arm2/diviator_Left.png";
                   _diviatorsDirection[1] = "img/arm2/diviator_Right.png";
                   _diviatorsDirection[2] = "img/arm2/diviator_Right.png";
                   break;
               }
           }
        }

        /// <summary>
        /// Начало загрузки материала в весовой бункер
        /// </summary>
        private async void LoadWeight()
        {
            //TODO: Добавить одновременную загрузку нескольких весовых бункеров
            // Номер весового бункера, принимающего материал
            if (!_weightLoading && !_receiverLoading)
            {
                // Не производится загрузка весового или приемочного бункера
                int weightTanker = Int32.Parse(_manualLoadWeights.WeightNumber);

                // Номер силоса, отдающего материал
                int silos = Int32.Parse(_manualLoadWeights.SilosNumber);

                // Проверяем наличие весового бункера
                if (_weights[weightTanker] != null)
                {
                    // Если весовой бункер готов к приему материала
                    if (_weights[weightTanker].GetStatus().CurrentState == Statuses.Status.Off)
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
                                    double placed = _weights[weightTanker].GetWeight();
                                    double fulled = _weights[weightTanker].MaxWeight;
                                    if (fulled > placed)
                                    {
                                        // Загружем либо весь материал из силоса, либо количество материала, до полного наполнения весового буненра
                                        // Устанавливаем флаг загрузки весового бункера
                                        _weightLoading = true;
                                        await OnNotify(
                                            $"Начата загрузка весового бункера {weightTanker + 1} из силоса {silos + 1}");
                                        _status = new Statuses();
                                        _status.StatusIcon = "img/led/MotorGreen.png";
                                        _status.StatusMessage = "ЗАГРУЗКА";
                                        _weights[weightTanker].SetStatus(_status);

                                        _status = new Statuses();
                                        _status.StatusIcon = "img/led/SmallYellow.png";
                                        _status.StatusMessage = "ВЫГРУЗКА";
                                        _siloses[silos].SetStatus(_status);

                                        // Задержка на время загрузки 10 секунд
                                        await Waiting(10);
                                        bool result = _weights[weightTanker]
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
                                        _weights[weightTanker].SetStatus(stateWeight);
                                        _weightLoading = false;

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
                            $"Весовой бункер {weightTanker + 1} находится в состоянии {_weights[weightTanker].Status.CurrentState.ToString()}");
                        Debug.WriteLine(
                            $"Весовой бункер {weightTanker + 1} находится в состоянии {_weights[weightTanker].Status.CurrentState.ToString()}");
                        await OnNotify(
                            $"Весовой бункер {weightTanker + 1} находится в состоянии {_weights[weightTanker].Status.CurrentState.ToString()}");
                    }
                }
                else
                {
                    _logger.Error($"Весовой бункер {weightTanker + 1} не существует!");
                    Debug.WriteLine($"Весовой бункер {weightTanker + 1} не существует!");
                    await OnNotify($"Весовой бункер {weightTanker + 1} не существует!");
                }
            }
            else
            {
                if (_weightLoading)
                {
                    // Производится загрузка весового бункера
                    _logger.Error("Одновременная загрузка нескольких весовых бунеров недопустима!");
                    Debug.WriteLine("Одновременная загрузка нескольких весовых бунеров недопустима!");
                    await OnNotify("Одновременная загрузка нескольких весовых бунеров недопустима!");
                }

                if (_receiverLoading)
                {
                    // Производится загрузка приемочного бункера
                    _logger.Error("Необходимо дождаться окончания загрузки приемочного бункера");
                    Debug.WriteLine("Необходимо дождаться окончания загрузки приемочного бункера");
                    await OnNotify("Необходимо дождаться окончания загрузки приемочного бункера");
                    
                }
            }
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
            switch (targetNum)
            {
                case "0" : targetMsg = "ДСП"; break;
                case "1" : targetMsg = "УПК"; break;
                case "2" : targetMsg = "Сторона УПК"; break;
                case "3" : targetMsg = "Сторона ДСП"; break;
            }
            
            await OnNotify($"[{ingot.GetStartTime()}] Начата доставка единицы учета №{ingot.Uid} к {targetMsg}");
            Console.ForegroundColor = ingot.Color;
            Console.WriteLine($"[{ingot.Uid}] {ingot.GetStartTime()} => {ingot.GetFinishTime()} ({ingot.Color.ToString()})");
            

            await PlaceOnRolgang(1, ingot);
            await PlaceOnRolgang(2, ingot);
            await PlaceOnRolgang(3, ingot);
            await PlaceOnRolgang(4, ingot);
            await PlaceOnRolgang(5, ingot);
            
            //TODO: Добавить определение назначения материала и задействовать рольганг 6 для УПК
            //TODO: Добавить выделение цветом дивиатора при подходе к нему материала, организовать задержку 2 секунды и снять выделение дивиатора
            
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
            if (!_weightLoading && !_receiverLoading)
            {
                int weightTanker = Int32.Parse(_manualLoadReceiver.WeightTanker); // Номер весового бункера
                int receiverTanker = Int32.Parse(_manualLoadReceiver.ReceiverTanker) + 3; // Номер приемочного бункера
                double weight = _manualLoadReceiver.Weight; // Вес загружаемого материала
                double availible; // Свободное место в приемочном бункере

                if (weight > 0)
                {
                    // Определить наличие приемочного бункера в ядре системы
                    if (_weights[receiverTanker] != null)
                    {
                        // Определить наличие весового бункера в ядре системы
                        if (_weights[weightTanker] != null)
                        {
                            // Определить статус приемочного бункера
                            if (_weights[weightTanker].GetCurrentState() == Statuses.Status.Off)
                            {
                                // Определить статус весового бункера
                                if (_weights[receiverTanker].GetCurrentState() == Statuses.Status.Off)
                                {
                                    availible = _weights[receiverTanker].GetAvailibleWeight();
                                    // Определить свободное место в приемочном бункере
                                    if (availible > 0)
                                    {
                                        if (weight > availible)
                                        {
                                            weight = availible;
                                        }

                                        // Определить наличие материала в весовом бункера
                                        if (_weights[weightTanker].GetLayersCount() > 0)
                                        {
                                            // Устанавливаем текущие состояния для весового и приемочного бункера
                                            _receiverLoading = true;
                                            await OnNotify(
                                                $"Начата загрузка приемочного бункера {receiverTanker + 1} из загрузочного бункера {weightTanker + 1}");

                                            // Весовой бункер
                                            Statuses state = new Statuses();
                                            state.CurrentState = Statuses.Status.Unloading;
                                            state.StatusMessage = "ВЫГРУЗКА";
                                            state.StatusIcon = "img/led/MotorYellow.png";
                                            _weights[weightTanker].SetStatus(state);

                                            // Приемочный бункер
                                            state = new Statuses();
                                            state.CurrentState = Statuses.Status.Loading;
                                            state.StatusMessage = "ЗАГРУЗКА";
                                            state.StatusIcon = "img/led/w1LedGreen.png";
                                            _weights[receiverTanker].SetStatus(state);

                                            // Создание единицы учета
                                            uint uid = GetNextIngotId(); // ищем следующий номер UID единицы учета
                                            Ingot ingot = new Ingot(uid);
                                            List<Material> materials = _weights[weightTanker].Unload(weight);
                                            ingot.AddMaterials(materials);
                                            _ingots.Add(ingot);

                                            // Перемещение единицы учета по рольгангам
                                            //TODO: Посмотреть, почему обрывается загрузка приемочного бункера при обновлении страницы
                                            SetTarget();
                                            await DeliverIngots(ingot);

                                            // Загружаем требуемый вес материала, либо вес до максимальной загрузки приемочного бункера
                                            _weights[receiverTanker].SetIngot(ingot);

                                            // Снимаем текущие состояния весового и приемочного бункеров

                                            // Весовой бункер
                                            state = new Statuses();
                                            state.CurrentState = Statuses.Status.Off;
                                            state.StatusMessage = "";
                                            state.StatusIcon = "img/led/MotorGrey.png";
                                            _weights[weightTanker].SetStatus(state);

                                            // Приемочный бункер
                                            state = new Statuses();
                                            state.CurrentState = Statuses.Status.Off;
                                            state.StatusMessage = "";
                                            state.StatusIcon = "img/led/w1LedGrey.png";
                                            _weights[receiverTanker].SetStatus(state);
                                            await OnNotify($"Приемочный бункер {receiverTanker + 1} загружен");
                                            _receiverLoading = false;
                                        }
                                        else
                                        {
                                            _logger.Error($"Весовой бункер {weightTanker + 1} пуст!");
                                            Debug.WriteLine($"Весовой бункер {weightTanker + 1} пуст!");
                                            await OnNotify($"Весовой бункер {weightTanker + 1} пуст!");
                                        }
                                    }
                                    else
                                    {
                                        _logger.Error($"Приемочный бункер {receiverTanker + 1} заполнен!");
                                        Debug.WriteLine($"Приемочный бункер {receiverTanker + 1} заполнен!");
                                        await OnNotify($"Приемочный бункер {receiverTanker + 1} заполнен!");
                                    }
                                }
                                else
                                {
                                    _logger.Error(
                                        $"Весовой бункер {weightTanker + 1} находится в состоянии {_weights[weightTanker].GetCurrentState()}");
                                    Debug.WriteLine(
                                        $"Весовой бункер {weightTanker + 1} находится в состоянии {_weights[weightTanker].GetCurrentState()}");
                                    await OnNotify(
                                        $"Весовой бункер {weightTanker + 1} находится в состоянии {_weights[weightTanker].GetCurrentState()}");
                                }
                            }
                            else
                            {
                                _logger.Error(
                                    $"Приемочный бункер {receiverTanker + 1} находится в состоянии {_weights[receiverTanker].GetCurrentState()}");
                                Debug.WriteLine(
                                    $"Приемочный бункер {receiverTanker + 1} находится в состоянии {_weights[receiverTanker].GetCurrentState()}");
                                await OnNotify(
                                    $"Приемочный бункер {receiverTanker + 1} находится в состоянии {_weights[receiverTanker].GetCurrentState()}");
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
            else
            {
                if (_weightLoading)
                {
                    // Производится загрузка весового бункера
                    _logger.Error("Необходимо дождаться окончания загрузки весового бункера!");
                    Debug.WriteLine("Необходимо дождаться окончания загрузки весового бункера!");
                    await OnNotify("Необходимо дождаться окончания загрузки весового бункера!");
                }

                if (_receiverLoading)
                {
                    // Производится загрузка приемочного бункера
                    _logger.Error("Одновременная загрузка нескольких приемочных бунеров недопустима!");
                    Debug.WriteLine("Одновременная загрузка нескольких приемочных бунеров недопустима!");
                    await OnNotify("Одновременная загрузка нескольких приемочных бунеров недопустима!");
                }
            }
        }

        private void ResetErrorTanker()
        {
            //TODO: Добавить сброс ошибок для весовых и приемочных бункеров
        }

        private void ResetTanker()
        {
            //TODO: Добавить обнуление весовых и приемочных бункеров
        }
    }
}

