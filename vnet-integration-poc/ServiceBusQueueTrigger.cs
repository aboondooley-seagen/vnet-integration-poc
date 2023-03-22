using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace vnet_integration_poc
{
    public class ServiceBusQueueTrigger
    {
        [FunctionName("ServiceBusQueueTrigger")]
        public async Task Run([ServiceBusTrigger("queue", Connection = "SERVICEBUS_CONNECTION")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function beginning to process message: {myQueueItem}");

            string databaseName = "dbTest";
            string collectionName = "collectionTest";
            dynamic data = new System.Dynamic.ExpandoObject();
            data.id = Guid.NewGuid().ToString();
            data.databaseName = databaseName;
            data.collectionName = collectionName;
            data.name = myQueueItem;

            var connectionString = Environment.GetEnvironmentVariable("COSMOSDB_CONNECTION");

            try
            {
                using (CosmosClient client = new CosmosClient(connectionString))
                {
                    log.LogInformation($"C# ServiceBus queue trigger function connected to cosmos db");
                    var database = client.GetDatabase(databaseName);
                    var container = database.GetContainer(collectionName);

                    var response = await container.CreateItemAsync(data);
                    log.LogInformation($"C# ServiceBus queue trigger function added data to database: {data}");
                }
            }
            catch (Exception ex)
            {
                data.errorMessage = ex.Message;
            }


            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}
