﻿namespace DataTrack.Data
{
    public class IngotVisualParameters
    {
        /// <summary>
        /// Имя графического файла
        /// </summary>
        public string FileName { get; set; }
        
        /// <summary>
        /// Положение от левого края страницы
        /// </summary>
        public string XPos { get; set; }
        
        /// <summary>
        /// Положение от верхнего края страницы
        /// </summary>
        public string YPos { get; set; }

        public IngotVisualParameters(string filename)
        {
            FileName = filename;
            XPos = "0px";
            YPos = "0px";
        }
    }
}