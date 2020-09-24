using System;
using DataTrack.Data;
using Microsoft.AspNetCore.Components;

namespace DataTrack.Shared
{
    public partial class Tanker
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }
        
        [Parameter]
        public string Top
        {
            get => _top + "px";
            set => _top = Int32.Parse(value);
        }
        
        [Parameter]
        public string Left
        {
            get => _left + "px";
            set => _left = Int32.Parse(value);
        }

        private int _top;
        private int _left;
        private string topMargin;
        private string leftMargin;
        private string leftMarginStatus;
        private string topMarginStatus;
        
        string srcImage = "img/arm2/weights_long.png";
        private string statusMsg = "Ready";
        private string statusImg = "img/led/w1LedGreen.png";

        public Tanker()
        {
            _top = 0;
            _left = 0;
            topMargin = _top + "px";
            leftMargin = _left + "px";
            topMarginStatus = _top + 67 + "px";
            leftMarginStatus = _left + 38 + "px";
        }

        /// <summary>
        /// Задать позицию компонента
        /// </summary>
        /// <param name="position">Координаты компонента</param>
        public void SetPostion(Coords position)
        {
            if (position != null)
            {
                _left = (int) position.PosX;
                _top = (int) position.PosY;
            }
        }

        /// <summary>
        /// Получить текущие координаты бункера
        /// </summary>
        /// <returns>Текущие координаты бункера</returns>
        public Coords GetPosition()
        {
            Coords coords = new Coords();
            coords.PosX = _left;
            coords.PosY = _top;

            return coords;
        }

        /// <summary>
        /// Установить сообщение о текущем состоянии бункера
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        public void SetStatusMsg(string message)
        {
            if (message.Trim() != "")
            {
                statusMsg = message;
                StateHasChanged();
            }
        }
    }
}