using System;
using System.Collections.Generic;

namespace DataTrack.Data
{
    public class ManualLoadWeights
    {
        /// <summary>
        /// Номер выбранного силоса
        /// </summary>
        public string SilosNumber
        {
            get => silosNumber.ToString();
            set => silosNumber = Int32.Parse(value);
        }

        private int silosNumber;

        /// <summary>
        /// Номер выбранного весового бункера
        /// </summary>
        public string WeightNumber
        {
            get => weightNumber.ToString();
            set => weightNumber = Int32.Parse(value);
        }

        private int weightNumber;

        /// <summary>
        /// Список силосов, доступных для загрузки весового бункера
        /// </summary>
        public Dictionary<string, string> Siloses;

        public ManualLoadWeights()
        {
            SilosNumber = "0";
            WeightNumber = "0";
            Siloses = new Dictionary<string, string>();
        }
    }
}