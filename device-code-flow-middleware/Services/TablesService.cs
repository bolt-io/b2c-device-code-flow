// For more infomation, see https://docs.microsoft.com/en-us/azure/cosmos-db/tutorial-develop-table-dotnet

using System;
using System.Threading.Tasks;
using device_code_flow_middleware.Models;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;

namespace device_code_flow_middleware
{
    public class TablesService
    {
        readonly string _ConnectionString = "";

        public TablesService(string connectionString)
        {
            _ConnectionString = connectionString;
        }

        private CloudStorageAccount CreateStorageAccountFromConnectionString()
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(this._ConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }

        private async Task<CloudTable> CreateTableAsync(string tableName)
        {

            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString();

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            Console.WriteLine("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", tableName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", tableName);
            }

            Console.WriteLine();
            return table;
        }
        public async Task<TraceEntity> TraceHttpAsync(string method, string path)
        {
            try
            {
                // Create or reference an existing table
                CloudTable table = await CreateTableAsync("Trace");

                // Add new entity
                TraceEntity device = new TraceEntity(method, path);

                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(device);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                TraceEntity insertedtrace = result.Result as TraceEntity;

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure Cosmos DB
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return insertedtrace;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public async Task<DeviceEntity> AddAuthRequestAsync(string device_code, string user_code)
        {
            try
            {
                // Create or reference an existing table
                CloudTable table = await CreateTableAsync("Devices");

                // Add new entity
                DeviceEntity device = new DeviceEntity(device_code, user_code);

                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(device);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                DeviceEntity insertedDevice = result.Result as DeviceEntity;

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure Cosmos DB
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return insertedDevice;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }
        public async Task<DeviceEntity> UpdateAccessTokenAsync(string user_code, string access_token)
        {
            try
            {
                // Create or reference an existing table
                CloudTable table = await CreateTableAsync("Devices");

                TableOperation retrieveOperation = TableOperation.Retrieve<DeviceEntity>(DeviceEntity.PartitionKeyName, user_code);
                TableResult result = await table.ExecuteAsync(retrieveOperation);

                DeviceEntity device = result.Result as DeviceEntity;
                if (device != null)
                {
                    device.AccessToken = access_token;
                }

                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(device);

                // Execute the operation.
                TableResult result1 = await table.ExecuteAsync(insertOrMergeOperation);
                DeviceEntity insertedDevice = result.Result as DeviceEntity;

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure CosmoS DB 
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of Retrieve Operation: " + result.RequestCharge);
                }

                return insertedDevice;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public async Task<DeviceEntity> GetEntityByUserCodeAsync(string user_code)
        {
            try
            {
                // Create or reference an existing table
                CloudTable table = await CreateTableAsync("Devices");

                TableOperation retrieveOperation = TableOperation.Retrieve<DeviceEntity>(DeviceEntity.PartitionKeyName, user_code);
                TableResult result = await table.ExecuteAsync(retrieveOperation);

                DeviceEntity device = result.Result as DeviceEntity;

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure CosmoS DB 
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of Retrieve Operation: " + result.RequestCharge);
                }

                return device;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public async Task DeleteEntityByUserCodeAsync(DeviceEntity deviceEntity)
        {
            try
            {
                // Create or reference an existing table
                CloudTable table = await CreateTableAsync("Devices");

                TableOperation deleteOperation = TableOperation.Delete(deviceEntity);
                TableResult result = await table.ExecuteAsync(deleteOperation);

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure CosmoS DB 
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of Retrieve Operation: " + result.RequestCharge);
                }
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }
    }
}