using MtsConnect;
using System;
using System.Collections.Generic;
using NLog;
using System.Threading.Tasks;

namespace DataTrack.Data
{
    public class MtsConnect
    {
        public bool Connected { get; private set; }
        private static MtsTcpConnection _connection;
        private static Subscription _subscription;
        private Action<SubscriptionStateEventArgs> _callBack;
        private readonly int _reconnectTimeout;
        private readonly int _timeout;
        private readonly Logger logger;

        private static string _address;
        private static int _port;
        private string _identity;

        public MtsConnect(string identity, string addr, int port, int timeout, int reconnect) 
        {
            logger = LogManager.GetCurrentClassLogger();
            _address = addr;
            _port = port;
            _reconnectTimeout = reconnect;
            _timeout = timeout;
            _identity = identity;
        }

        public bool Connect ()
        {
            bool Res = false;
            try
            {
                _connection = MtsTcpConnection.Create(_address, _port);
                Res = true;
            }
            catch (Exception e)
            {
                logger.Error($"Ошибка при подключении к сервису MTSService {e.Message}");
                Connected = Res;
            }

            return Res;
        }

        private void SubscribeInternal()
        {
            // Устанавливаем подписки на изменение значения сигналов
            if (_subscription == null)
                return;

            _subscription.NewData += SubscriptionOnNewData;  // Подписка на изменение данных
            _subscription.OnError += SubscriptionOnError;    // Подписка на возникновение ошибки
        }

        private void SubscriptionOnNewData(object sender, SubscriptionStateEventArgs e) =>
    _callBack?.Invoke(e);

        private void SubscriptionOnError(object sender, SubscriptionErrorEventArgs e)
        {
            // Вызов функции обратного вызова и попытка переподключения при возникновении ошибки от сигнала
            logger.Error($"Ошибка при получении сигнала от датчика: {e.Message}");
            Connected = false;
            TryReconnect();
        }

        private void TryConnect()
        {
            if (!Connected)
            {
                try
                {
                    // Пытаемся создать подключение к MTSSercice
                    _connection = MtsTcpConnection.Create(_address, _port);
                    Connected = true;

                    // Поключение создано, устанавливаем подписки на изменение значений сигналов
                    SubscribeInternal();
                }
                catch (Exception e)
                {
                    // Если не удалось подключиться
                    Connected = false;
                    Console.WriteLine(e.Message);
                    logger.Error($"Ошибка при подключении к сервису MTSService: {e.Message}");

                    // Пытаемся переподключиться через указаное в настройках время
                    TryReconnect();
                }
            }
        }

        private void ResetConnection()
        {
            // Попытка сбросить предыдущее неудачное соединение
            _connection?.Dispose();
            if (_subscription == null)
                return;

            // Удаление предыдущих подписок на сигналы
            _subscription.OnError -= SubscriptionOnError;
            _subscription.NewData -= SubscriptionOnNewData;
        }

        private void TryReconnect()
        {
            // Попытка переподключения если прошло указанное в настройках время
            Task.Delay(TimeSpan.FromMilliseconds(_reconnectTimeout));

            // Сбрасываем предыдущую неудачную попытку подключения
            ResetConnection();

            // Пытаемся подключиться заново
            TryConnect();
        }

        public Task Subscribe(List<ushort> signalIds, Action<SubscriptionStateEventArgs> callBack)
        {
            _callBack = callBack;
            TryConnect();

            var subConfig = new SubscriptionConfig()
                .Identity(_identity)
                .Timeout(TimeSpan.FromMilliseconds(_timeout));
            foreach (var id in signalIds)
                subConfig.AddSignal(id);

            subConfig.AddAoI();
            _subscription = _connection.CreateNewSubscription(subConfig);
            SubscribeInternal();
            return Task.CompletedTask;
        }
    }
}