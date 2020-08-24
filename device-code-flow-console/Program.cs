using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace device_code_flow_console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            Console.WriteLine("Press any key to exit");
        }

        private static async Task RunAsync()
        {
            // SampleConfiguration config = SampleConfiguration.ReadFromJsonFile("appsettings.json");
            // var appConfig = config.PublicClientApplicationOptions;

            var msalAppOptions = new PublicClientApplicationOptions();

            // Azure AD integration
            // string[] Scopes = new string[] { "https://graph.microsoft.com/.default" };
            // msalAppOptions.ClientId = "9658d0b0-168a-4693-8474-0b3c5c09ec56";
            // msalAppOptions.TenantId = "starflower.onmicrosoft.com";
            // msalAppOptions.AzureCloudInstance = AzureCloudInstance.AzurePublic;

            // AD-FS integration
            // string[] Scopes = new string[] { "profile" };
            // msalAppOptions.ClientId = "2b464e5e-ee58-4e38-8611-17b7406b6217";
            // msalAppOptions.Instance = "https://cosmos.irisflower.pro/adfs/";
            // msalAppOptions.AzureCloudInstance = AzureCloudInstance.None;

            // Custom middleware integration
            string[] Scopes = new string[] { "https://irisflower.onmicrosoft.com/api1/read" };
            msalAppOptions.ClientId = "2b464e5e-ee58-4e38-8611-17b7406b6217";
            msalAppOptions.Instance = "https://device-code.azurewebsites.net/";
            msalAppOptions.AzureCloudInstance = AzureCloudInstance.None;

            // Logs
            msalAppOptions.EnablePiiLogging = true;
            msalAppOptions.LogLevel = LogLevel.Verbose;
            msalAppOptions.IsDefaultPlatformLoggingEnabled = true;
            msalAppOptions.ClientName = "AzureADB2C";

            var app = PublicClientApplicationBuilder.CreateWithApplicationOptions(msalAppOptions)
                                                    // .WithLogging(MyLoggingMethod, LogLevel.Info,
                                                    //         enablePiiLogging: true,
                                                    //         enableDefaultPlatformLogging: true)
                                                    .Build();

            var tokenAcquisitionHelper = new PublicAppUsingDeviceCodeFlow(app);


            AuthenticationResult authenticationResult = await tokenAcquisitionHelper.AcquireATokenFromCacheOrDeviceCodeFlowAsync(Scopes);
            if (authenticationResult != null)
            {
                Console.WriteLine(authenticationResult.AccessToken);
            }
            else
            {
                Console.WriteLine("Error");
            }
        }

        static void MyLoggingMethod(LogLevel level, string message, bool containsPii)
        {
            Console.WriteLine($"MSAL {level} {containsPii} {message}");
        }
    }
}
