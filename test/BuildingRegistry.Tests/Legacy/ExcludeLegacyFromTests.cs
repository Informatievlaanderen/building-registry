// Apply the custom framework at the assembly level
[assembly: Xunit.TestFramework("BuildingRegistry.Tests.Legacy.ExcludeLegacyTests", "BuildingRegistry.Tests")]

namespace BuildingRegistry.Tests.Legacy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    // Custom Xunit Test Framework
    public class ExcludeLegacyTests : XunitTestFramework
    {
        public ExcludeLegacyTests(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            return new ExcludeLegacyTestsExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }
    }

    // Custom Executor
    public class ExcludeLegacyTestsExecutor : XunitTestFrameworkExecutor
    {
        public ExcludeLegacyTestsExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
        }

        protected override void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            var filteredTestCases = testCases.Where(tc => !tc.TestMethod.TestClass.Class.Name.Contains(".Legacy."));
            base.RunTestCases(filteredTestCases, executionMessageSink, executionOptions);
        }
    }
}
