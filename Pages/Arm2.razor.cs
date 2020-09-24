using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataTrack.Data;
using Microsoft.AspNetCore.Components.Web;

namespace DataTrack.Pages
{
    public partial class Arm2
    {
        private (string value, Task t) lastNotification;
        private List<Silos> _siloses = new List<Silos>();
        private List<WeightTanker> _weights = new List<WeightTanker>();
        private List<Material> _loadedMaterial = new List<Material>();

        private string _detailPosX;
        private string _detailPosY;

        // Позиция сталевоза:
        //    1. top: 680px; left: 1500px;
        //    2. top: 680px; left: 1180px;
        private string _stalevozPos1;
        private string _stalevozPos2;
        private readonly Statuses[] _statuses = new Statuses[15];
        private Statuses _status;

        private string _showed = "none";
        
        // Класс, заполняемый формой ручной загрузки весовых бункеров
        private readonly ManualLoadWeights _manualLoadWeights = new ManualLoadWeights();
        private Dictionary<string, string> _avalibleSiloses = new Dictionary<string, string>();

        protected override void OnInitialized()
        {
            Notifier.Notify += OnNotify;
            
            // Точка входа на страницу
            // Проверить наличие силосов и прочих узлов в ядре системе!
            Initialize();
            _siloses = Kernel.Data.GetSiloses();
            // int kernelMaterialIndex = Kernel.Data.GetCurrentMaterialIndex();
            _stalevozPos1 = "top: 680px; left: 1180px;";
            _stalevozPos2 = "top: 680px; left: 1500px;";
        }
        
        private async Task OnNotify(string value)
        {
            await InvokeAsync(() =>
            {
                lastNotification.value = value;
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
                    WeightTanker tanker = new WeightTanker(i);
                    Statuses status = new Statuses();
                    status.CurrentState = Statuses.Status.Off;
                    status.StatusIcon = "img/led/MotorGrey.png";
                    status.StatusMessage = "";
                    tanker.SetStatus(status);
                    _weights.Add(tanker);
                }

                for (int i = 4; i < 8; i++)
                {
                    WeightTanker tanker = new WeightTanker(i);
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
            
            // Установка индиатора текущего состояния для весовых бункеров
            _statuses[0] = _weights[0].GetStatus();
            _statuses[1] = _weights[1].GetStatus();
            _statuses[2] = _weights[2].GetStatus();

            _statuses[3] = _weights[3].GetStatus();
            _statuses[4] = _weights[4].GetStatus();
            _statuses[5] = _weights[5].GetStatus();
            _statuses[6] = _weights[6].GetStatus();
            
            // Установка индикаторов текущего состояния для силосов
            _statuses[7] = _siloses[0].GetStatus();
            _statuses[8] = _siloses[1].GetStatus();
            _statuses[9] = _siloses[2].GetStatus();
            _statuses[10] = _siloses[3].GetStatus();
            _statuses[11] = _siloses[4].GetStatus();
            _statuses[12] = _siloses[5].GetStatus();
            _statuses[13] = _siloses[6].GetStatus();
            _statuses[14] = _siloses[7].GetStatus();

            _manualLoadWeights.WeightNumber = "0";
            _manualLoadWeights.SilosNumber = "0";
            _avalibleSiloses.Add("0", "Силос 1");
            _avalibleSiloses.Add("1", "Силос 2");
            _avalibleSiloses.Add("2", "Силос 3");
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
                    matCount = _siloses[0].GetLayersCount();
                    _loadedMaterial = _siloses[0].GetMaterials();
                    break;
                }
                case 2:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[1].GetLayersCount();
                    _loadedMaterial = _siloses[1].GetMaterials();
                    break;
                }
                case 3:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[2].GetLayersCount();
                    _loadedMaterial = _siloses[2].GetMaterials();
                    break;
                }
                case 4:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[3].GetLayersCount();
                    _loadedMaterial = _siloses[3].GetMaterials();
                    break;
                }
                case 5:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[4].GetLayersCount();
                    _loadedMaterial = _siloses[4].GetMaterials();
                    break;
                }
                case 6:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[5].GetLayersCount();
                    _loadedMaterial = _siloses[5].GetMaterials();
                    break;
                }
                case 7:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[6].GetLayersCount();
                    _loadedMaterial = _siloses[6].GetMaterials();
                    break;
                }
                case 8:
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
        /// Начало загрузки материала в весовой бункер
        /// </summary>
        private void LoadWeight()
        {
            // Номер весового бункера, принимающего материал
            int weightTanker = Int32.Parse(_manualLoadWeights.WeightNumber);
            
            // Номер силоса, отдающего материал
            int silos = Int32.Parse(_manualLoadWeights.SilosNumber);

            _status = new Statuses();
            _status.StatusIcon = "img/led/MotorGreen.png";
            _status.StatusMessage = "WORK";
            _statuses[weightTanker] = _status;
            _weights[weightTanker].SetStatus(_status);
            
            _status = new Statuses();
            _status.StatusIcon = "img/led/SmallYellow.png";
            _status.StatusMessage = "UNLOAD";
            _statuses[silos+7] = _status;
            _siloses[silos].SetStatus(_status);

            // Проверяем наличие весового бункера в ядре системы
            // Проверяем наличие силоса в ядре системы
            // Проверяем наличие материала в силосе, из которого будет загрузка материала
            // Проверяем наличие свободного места в весовом бункере
            // Загружем либо весь материал из силоса, либо количество материала, до полного наполнения весового буненра
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
    }
}

