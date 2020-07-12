// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading.Tasks;

namespace System.ServiceProcess.Tests
{
    internal sealed class TestServiceProvider
    {
        // To view tracing, use DbgView from systinternals.com;
        // run it elevated, check "Capture>Global Win32" and "Capture>Win32",
        // and filter to just messages beginning with "##"
        internal const bool DebugTracing = false;

        private const int readTimeout = 60000;

        private static readonly Lazy<bool> s_runningWithElevatedPrivileges = new Lazy<bool>(
            () => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator));

        private NamedPipeClientStream _client;

        public static bool RunningWithElevatedPrivileges
        {
            get { return s_runningWithElevatedPrivileges.Value; }
        }

        public NamedPipeClientStream Client
        {
            get
            {
                if (_client == null)
                {
                    DebugTrace("TestServiceProvider: Creating client stream");
                    _client = new NamedPipeClientStream(".", TestServiceName, PipeDirection.In);
                }
                return _client;
            }
            set
            {
                if (value == null)
                {
                    DebugTrace("TestServiceProvider: Disposing client stream");
                    _client.Dispose();
                    _client = null;
                }
            }

        }

        public readonly string TestServiceAssembly = typeof(TestService).Assembly.Location;
        public readonly string TestMachineName;
        public readonly TimeSpan ControlTimeout;
        public readonly string TestServiceName;
        public readonly string TestServiceDisplayName;

        private readonly TestServiceProvider _dependentServices;
        public TestServiceProvider()
        {
            TestMachineName = ".";
            ControlTimeout = TimeSpan.FromSeconds(120);
            TestServiceName = Guid.NewGuid().ToString();
            TestServiceDisplayName = "Test Service " + TestServiceName;

            _dependentServices = new TestServiceProvider(TestServiceName + ".Dependent");

            // Create the service
            CreateTestServices();
        }

        public TestServiceProvider(string serviceName)
        {
            TestMachineName = ".";
            ControlTimeout = TimeSpan.FromSeconds(120);
            TestServiceName = serviceName;
            TestServiceDisplayName = "Test Service " + TestServiceName;
            
            // Create the service
            CreateTestServices();
        }

        public async Task<byte> ReadPipeAsync()
        {
            Task readTask;
            byte[] received = new byte[] { 0 };
            readTask = Client.ReadAsync(received, 0, 1);
            await readTask.TimeoutAfter(readTimeout).ConfigureAwait(false);
            return received[0];
        }

        public byte GetByte() => ReadPipeAsync().Result;

        private void CreateTestServices()
        {
            TestServiceInstaller testServiceInstaller = new TestServiceInstaller();

            testServiceInstaller.ServiceName = TestServiceName;
            testServiceInstaller.DisplayName = TestServiceDisplayName;
            testServiceInstaller.Description = "__Dummy Test Service__";
            testServiceInstaller.Username = null;

            if (_dependentServices != null)
            {
                testServiceInstaller.ServicesDependedOn = new string[] { _dependentServices.TestServiceName };
            }

            var comparer = PlatformDetection.IsNetFramework ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal; // .NET Framework upper cases the name
            string processName = Process.GetCurrentProcess().MainModule.FileName;
            string entryPointName = typeof(TestService).Assembly.Location;
            string arguments = TestServiceName;

            // if process and entry point aren't the same then we are running hosted so pass
            // in the entrypoint as the first argument
            if (!PlatformDetection.IsNetFramework)
            {
                arguments = $"\"{entryPointName}\" {arguments}";
            }
            else
            {
                processName = entryPointName;
            }

            testServiceInstaller.ServiceCommandLine = $"\"{processName}\" {arguments}";

            testServiceInstaller.Install();
        }

        public void DeleteTestServices()
        {
            try
            {
                if (_client != null)
                {
                    DebugTrace("TestServiceProvider: Disposing client stream in Dispose()");
                    _client.Dispose();
                    _client = null;
                }

                TestServiceInstaller testServiceInstaller = new TestServiceInstaller();
                testServiceInstaller.ServiceName = TestServiceName;
                testServiceInstaller.RemoveService();
            }
            finally
            {
                // Lets be sure to try and clean up dependenct services even if something goes
                // wrong with the full removal of the other service.
                if (_dependentServices != null)
                {
                    _dependentServices.DeleteTestServices();
                }
            }
        }

        internal static void DebugTrace(string message)
        {
            if (DebugTracing)
            {
#pragma warning disable CS0162 // unreachable code
                Debug.WriteLine("## " + message);
#pragma warning restore
            }
        }
    }
}
