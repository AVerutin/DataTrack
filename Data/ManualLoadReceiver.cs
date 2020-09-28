using System;

namespace DataTrack.Data
{
    public class ManualLoadReceiver
    {
        /// <summary>
        /// Номер весового бункера
        /// </summary>
        public string WeightTanker
        {
            get => _weighter.ToString();
            set => _weighter = Int32.Parse(value);
        }
        
        /// <summary>
        /// Номер приемочного бункера
        /// </summary>
        public string ReceiverTanker
        {
            get => _reciever.ToString();
            set => _reciever = Int32.Parse(value);
        }

        /// <summary>
        /// Вес загружаемого материала
        /// </summary>
        public double Weight
        {
            get => _weight;
            set => _weight = value;
        }
        // public string Weight
        // {
        //     get => _weight.ToString();
        //     set => _weight = double.Parse(value);
        // }

        private int _weighter;
        private int _reciever;
        private double _weight;

        public ManualLoadReceiver()
        {
            WeightTanker = "0";
            ReceiverTanker = "0";
            Weight = 0;
        }
    }
}