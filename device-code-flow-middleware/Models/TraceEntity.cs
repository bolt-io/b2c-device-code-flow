using System;
using Microsoft.Azure.Cosmos.Table;

namespace device_code_flow_middleware.Models
{
    public class TraceEntity : TableEntity
    {
        public TraceEntity()
        {
        }

        public TraceEntity(string method, string path)
        {
            PartitionKey = "HTTP";
            RowKey = Guid.NewGuid().ToString();

            this.Method = method;
            this.Path = path;
        }

        public string Method { get; set; }
        public string Path { get; set; }
    }
}