namespace BuildingRegistry.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using FluentAssertions;
    using Xunit;
    public class InfrastructureCommandsTests
    {
        private readonly IEnumerable<Type> _commandTypes;

        public InfrastructureCommandsTests()
        {
            // Attempt to auto-discover the domain assembly using a type called "DomainAssemblyMarker".
            // If this class is not present, these tests will fail.
            var domainAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => InfrastructureEventsTests.GetAssemblyTypesSafe(a).Any(t => t?.Name == "DomainAssemblyMarker"));
            if (domainAssembly == null)
            {
                _commandTypes = Enumerable.Empty<Type>();
                return;
            }

            bool IsEventNamespace(Type t) => t.Namespace.EndsWith("Commands");
            bool IsNotCompilerGenerated(MemberInfo t) => Attribute.GetCustomAttribute(t, typeof(CompilerGeneratedAttribute)) == null;

            _commandTypes = domainAssembly
                .GetTypes()
                .Where(t => t.IsClass && t.Namespace != null && IsEventNamespace(t) && IsNotCompilerGenerated(t));
        }

        [Fact]
        public void HasNoDuplicateNamespace()
        {
            var eventNames = new List<string>();

            foreach (var eventType in _commandTypes)
            {
                var @namespace = eventType
                    .GetField("Namespace", BindingFlags.Static|BindingFlags.NonPublic)
                    ?.GetValue(null)
                    ?.ToString();

                @namespace.Should().NotBeNullOrWhiteSpace();
                eventNames.Contains(@namespace!).Should().BeFalse($"Duplicate event name {@namespace}");
                eventNames.Add(@namespace!);
            }
        }
    }
}
