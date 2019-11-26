using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Lib.Net.Http.WebPush;
using Demo.AspNetCore.PushNotifications.Services.Abstractions;
using System;

namespace Demo.AspNetCore.PushNotifications.Services
{
    internal class PushNotificationsDequeuer : IHostedService
    {
        private readonly IPushSubscriptionStoreAccessorProvider _subscriptionStoreAccessorProvider;
        private readonly IPushNotificationsQueue _messagesQueue;
        private readonly IPushNotificationService _notificationService;
        private readonly CancellationTokenSource _stopTokenSource = new CancellationTokenSource();

        private Task _dequeueMessagesTask;

        public PushNotificationsDequeuer(IPushNotificationsQueue messagesQueue, IPushSubscriptionStoreAccessorProvider subscriptionStoreAccessorProvider, IPushNotificationService notificationService)
        {
            _subscriptionStoreAccessorProvider = subscriptionStoreAccessorProvider;
            _messagesQueue = messagesQueue;
            _notificationService = notificationService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _dequeueMessagesTask = Task.Run(DequeueMessagesAsync);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stopTokenSource.Cancel();

            return Task.WhenAny(_dequeueMessagesTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    Console.WriteLine("Notification Service Started");
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        await DequeueMessagesAsync();
        //    }
        //}

        private async Task DequeueMessagesAsync()
        {
            while (!_stopTokenSource.IsCancellationRequested)
            {
                PushMessage message = await _messagesQueue.DequeueAsync(_stopTokenSource.Token);

                if (!_stopTokenSource.IsCancellationRequested)
                {
                    using (IPushSubscriptionStoreAccessor subscriptionStoreAccessor = _subscriptionStoreAccessorProvider.GetPushSubscriptionStoreAccessor())
                    {
                        await subscriptionStoreAccessor.PushSubscriptionStore.ForEachSubscriptionAsync((PushSubscription subscription) =>
                        {
                            // Fire-and-forget 
                            _notificationService.SendNotificationAsync(subscription, message, _stopTokenSource.Token);
                        }, _stopTokenSource.Token);
                    }

                }
            }

        }
    }
}
