using System;
using Microsoft.Azure.Cosmos.Table;

namespace device_code_flow_middleware.Models
{
    public class DeviceEntity : TableEntity
    {
        public const string PartitionKeyName = "Device";
        public DeviceEntity()
        {
        }

        public DeviceEntity(string device_code, string user_code)
        {
            PartitionKey = PartitionKeyName;
            RowKey = user_code;

            this.DeviceCode = device_code;
        }

        public string DeviceCode { get; set; }
        public string AccessToken { get; set; }
    }
}