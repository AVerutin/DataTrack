using System.Collections.Generic;
using System.Threading.Tasks;
using DataTrack.Data;
using DataTrack.Shared;

namespace DataTrack.Pages
{
    public partial class Arm3
    {
        private (string value, Task t) lastNotification;

        private List<Tanker> _tankers;
        private Tanker _tanker1;
        private Tanker _tanker2;
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            Notifier.Notify += OnNotify;
            
            // Точка входа на страницу
            _tankers = new List<Tanker>();
            _tanker1 = new Tanker();
            _tanker2 = new Tanker();
            Coords pos = new Coords();
            
            pos.PosX = 300;
            pos.PosY = 350;
            _tanker1.SetPostion(pos);
            _tankers.Add(_tanker1);
            pos = new Coords();
            pos.PosX = 500;
            pos.PosY = 350;
            _tanker2.SetPostion(pos);
            _tankers.Add(_tanker2);
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

        private async void MoveTankers()
        {
            Coords pos = _tankers[0].GetPosition();
            pos.PosX = pos.PosX + 75;
            _tankers[0].SetPostion(pos);
            _tankers[0].SetStatusMsg("MOVED");
            StateHasChanged();
            await OnNotify("Перемещение бункера");
        }
        
        
    }
}
