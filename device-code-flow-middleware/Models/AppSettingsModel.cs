namespace device_code_flow_middleware.Models
{
    public class AppSettingsModel
    {
        public string VerificationUri { get; set; }
        public string Scope { get; set; }
        public string ConnectionString { get; set; }
        public int ExpirationInSeconds { get; set; }
        public int IntervalInSeconds { get; set; }
    }
}