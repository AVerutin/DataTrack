namespace DataTrack.Data
{
    public enum CfgFileObjectTypes
    {
        MillConfig,     // Общие параметры стана
        Connection,     // Параметры подключения
        Signal,         // Сигнал 
        Subscription,   // Подписка
        DataBlock,      // Блок данных
        Thread,         // Нить
        Rollgang,       // Рольганг
        Label,          // Метка
        Sensor,         // Датчик
        Stopper,        // Упор
        LinearMoving,   // Агрегат линейного перемещения 
        StepperMoving,  // Агрегат шагающего перемещения
        Deleter,        // Удаление застравших
        Cage,           // Клеть
        TechnicalUnit,  // Техузел
        Agregate,       // Агрегат
        IngotParams,    // Параметры ЕУ
        Default         // Не определено
    }
}