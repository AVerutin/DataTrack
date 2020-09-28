using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DataTrack.Data;
using Microsoft.AspNetCore.Components.Web;
using NLog;
using NLog.Targets;

namespace DataTrack.Pages
{
    public partial class Arm2
    {
        private (string value, Task t) _lastNotification;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private List<Silos> _siloses = new List<Silos>();
        private List<WeightTanker> _weights = new List<WeightTanker>();
        private List<Material> _loadedMaterial = new List<Material>();
        private string[] _diviatorsDirection = new string[4];

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

            _manualLoadWeights.WeightNumber = "0";
            _manualLoadWeights.SilosNumber = "0";
            _avalibleSiloses.Add("0", "Силос 1");
            _avalibleSiloses.Add("1", "Силос 2");
            _avalibleSiloses.Add("2", "Силос 3");
            _diviatorsDirection[0] = "img/arm2/diviator_right.png";
            _diviatorsDirection[1] = "img/arm2/diviator_right.png";
            _diviatorsDirection[2] = "img/arm2/diviator_right.png";
            _diviatorsDirection[3] = "img/arm2/diviator_down.png";
            _detailCaption = "";
            
            // Сбрасываем флаги загрузки весового и приемочного бункеров
            _weightLoading = false;
            _receiverLoading = false;
            await OnNotify("Готов");
        }
        
        private void ShowMaterial(MouseEventArgs e, int number)
        {
            int matCount;
            
            // Определяем тип объекта, который вызвал метод:
            // number<10 - силос, number>10 - весовой бункер
            if (number < 10)
            {
                matCount = _siloses[number - 1].GetLayersCount();
                _loadedMaterial = _siloses[number - 1].GetMaterials();
                _detailCaption = "";
            }
            else
            {
                matCount = _weights[number - 10].GetLayersCount();
                _loadedMaterial = _weights[number - 10].GetMaterials();
                _detailCaption = _weights[number - 10].Name;
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
        /// Установить дивиаторы на цель
        /// </summary>
        private void SetTarget()
        {
            //TODO: Добавить выделение цветом с задержкой через каждые 5 сек для промежуточных целей (конвейеров,
            // дивиаторов) (с одного снял выделение, на следующий поставил 
           string value = _manualLoadReceiver.ReceiverTanker;
           
           switch (value)
            {
                case "0":
                {
                    // Направление - ДСП
                    _diviatorsDirection[0] = "img/arm2/diviator_Right.png";
                    _stalevozPos = "1500px;";
                    break;
                }
                case "1":
                {
                    // Направление - УПК
                    _diviatorsDirection[0] = "img/arm2/diviator_Left.png";
                    _diviatorsDirection[1] = "img/arm2/diviator_Left.png";
                    _stalevozPos = "1180px;";
                    break;
                }
                case "2":
                {
                    // Направление - Сторона УПК
                    _diviatorsDirection[0] = "img/arm2/diviator_Left.png";
                    _diviatorsDirection[1] = "img/arm2/diviator_Right.png";
                    _diviatorsDirection[2] = "img/arm2/diviator_Left.png";
                    _stalevozPos = "1500px;";
                    break;
                }
                case "3":
                {
                    // Направление - Сторона ДСП
                    _diviatorsDirection[0] = "img/arm2/diviator_Left.png";
                    _diviatorsDirection[1] = "img/arm2/diviator_Right.png";
                    _diviatorsDirection[2] = "img/arm2/diviator_Right.png";
                    _stalevozPos = "1500px;";
                    break;
                }
            }
        }

        /// <summary>
        /// Начало загрузки материала в весовой бункер
        /// </summary>
        private async void LoadWeight()
        {
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

                                        // Задержка на время загрузки 50 секунд
                                        await Task.Delay(TimeSpan.FromSeconds(50));
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

        private async void LoadReceiver()
        {
            if (!_weightLoading && !_receiverLoading)
            {
                int weightTanker = Int32.Parse(_manualLoadReceiver.WeightTanker); // Номер весового бункера
                int receiverTanker = Int32.Parse(_manualLoadReceiver.ReceiverTanker) + 3; // Номер приемочного бункера
                double weight = _manualLoadReceiver.Weight; // Вес загружаемого материала
                double availible; // Свободное место в приемочном бункере

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
                                        SetTarget();
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

                                        // Задержка на время загрузки 50 секунд
                                        await Task.Delay(TimeSpan.FromSeconds(50));

                                        // Загружаем требуемый вес материала, либо вес до максимальной загрузки приемочного бункера
                                        _weights[receiverTanker].LoadMaterial(_weights[weightTanker], weight);

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
    }
}

