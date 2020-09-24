namespace DataTrack.Data
{
    public class Statuses
    {
        /// <summary>
        /// Перечисление доступных состояний
        /// </summary>
        public enum Status { On, Off, Error, Full, Empty, Loading, Unloading, Selected, Deselected, Delivering, Delivered }

        public Status CurrentState;

        /// <summary>
        /// Изображение, обозначающее текущее состояние
        /// </summary>
        public string StatusIcon;

        /// <summary>
        /// Текстовое описание текущего состояния
        /// </summary>
        public string StatusMessage;

        public Statuses()
        {
            StatusIcon = "";
            StatusMessage = "";
            CurrentState = Status.Off;
        }
    }
}