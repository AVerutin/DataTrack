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
        private readonly List<Silos> _siloses = new List<Silos>();

        private string _detailPosX;
        private string _detailPosY;

        // Позиция сталевоза:
        //    1. top: 680px; left: 1500px;
        //    2. top: 680px; left: 1180px;
        private string _stalevozPos1;
        private string _stalevozPos2;

        private string _showed = "none";

        protected override void OnInitialized()
        {
            Notifier.Notify += OnNotify;
            
            // Точка входа на страницу
            int kernelMaterialIndex = DataKernel.DataKernel.GetCurrentMaterialIndex();
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
                    // _loadedMaterial = _siloses[0].GetMaterials();
                    break;
                }
                case 2:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[1].GetLayersCount();
                    // _loadedMaterial = _siloses[1].GetMaterials();
                    break;
                }
                case 3:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[2].GetLayersCount();
                    // _loadedMaterial = _siloses[2].GetMaterials();
                    break;
                }
                case 4:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[3].GetLayersCount();
                    // _loadedMaterial = _siloses[3].GetMaterials();
                    break;
                }
                case 5:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[4].GetLayersCount();
                    // _loadedMaterial = _siloses[4].GetMaterials();
                    break;
                }
                case 6:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[5].GetLayersCount();
                    // _loadedMaterial = _siloses[5].GetMaterials();
                    break;
                }
                case 7:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[6].GetLayersCount();
                    // _loadedMaterial = _siloses[6].GetMaterials();
                    break;
                }
                case 8:
                {
                    _detailPosY = $"{e.ClientY + 20}px";
                    _detailPosX = $"{e.ClientX + 10}px";
                    matCount = _siloses[7].GetLayersCount();
                    // _loadedMaterial = _siloses[7].GetMaterials();
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

    }
}

