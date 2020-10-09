namespace DataTrack.Data
{
    public class IngotVisualParameters
    {
        /// <summary>
        /// UID единицы учета
        /// </summary>
        public uint IngotUid { get; set; }
        
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
        
        /// <summary>
        /// Положение слоя в пачке
        /// </summary>
        public string ZIndex { get; set; }

        public IngotVisualParameters(string filename)
        {
            FileName = filename;
            XPos = "0px";
            YPos = "0px";
            ZIndex = "-1";
        }
    }
}