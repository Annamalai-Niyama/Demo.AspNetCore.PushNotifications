using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace Demo.AspNetCore.PushNotification.Data
{
    public class UnitOfWork
    {
        private readonly IMongoDatabase _database = null;
        private readonly MongoClient _client = null;

        //public UnitOfWork(IOptions<> appSetting)
        //{
        //    _client = new MongoClient(appSetting.Value.ConnectionString);
        //    if (_client != null)
        //        _database = _client.GetDatabase(appSetting.Value.Database);
        //}
    }
}
