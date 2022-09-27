namespace BuildingRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using BuildingRegistry.Api.BackOffice.Abstractions.Exceptions;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Infrastructure;
    using BuildingRegistry.Building.Exceptions;
    using FluentAssertions;
    using Microsoft.Data.SqlClient;
    using Xunit;

    public sealed class LambdaHandlerRetryPolicyTests
    {
        [Theory]
        [InlineData(926)]
        [InlineData(4060)]
        [InlineData(40197)]
        [InlineData(40501)]
        [InlineData(40549)]
        [InlineData(40550)]
        [InlineData(40613)]
        [InlineData(49918)]
        [InlineData(49919)]
        [InlineData(49920)]
        [InlineData(4221)]
        [InlineData(615)]
        public async Task RetryOnSpecificSqlExceptionErrors(int errorNumber)
        {
            var maxRetryCount = 2;
            var retryCounter = -1; // First execution is not part of the retry count

            var sut = new LambdaHandlerRetryPolicy(maxRetryCount, 0);

            try
            {
                // Act
                await sut.Retry(() =>
                {
                    retryCounter++;
                    throw SqlExceptionMocker.NewSqlException(errorNumber);
                });
            }
            catch (Exception)
            {
                // ignored
            }

            // Assert
            retryCounter.Should().Be(maxRetryCount);
        }

        [Fact]
        public async Task DoesNotRetryOnAggregateNotFoundException()
        {
            await RetryDoesNotHappenOnException(() => throw new AggregateNotFoundException(string.Empty, typeof(string)));
        }

        [Fact]
        public async Task DoesNotRetryOnDomainException()
        {
            await RetryDoesNotHappenOnException(() => throw new BuildingUnitIsNotFoundException());
        }

        [Fact]
        public async Task DoesNotRetryOnIfMatchHeaderValueMismatchException()
        {
            await RetryDoesNotHappenOnException(() => throw new IfMatchHeaderValueMismatchException(null));
        }

        [Fact]
        public async Task DoesNotRetryOnSqlConnectionFailure()
        {
            await RetryDoesNotHappenOnException(() => new SqlConnection(@"Data Source=.;Database=GUARANTEED_TO_FAIL;Connection Timeout=1").Open());
        }

        private async Task RetryDoesNotHappenOnException(Action throwExceptionMethod)
        {
            var maxRetryCount = 2;
            var retryCounter = -1; // First execution is not part of the retry count

            var sut = new LambdaHandlerRetryPolicy(maxRetryCount, 0);

            try
            {
                // Act
                await sut.Retry(() =>
                {
                    retryCounter++;
                    throwExceptionMethod();
                    return Task.CompletedTask;
                });
            }
            catch (Exception)
            {
                // ignored
            }

            // Assert
            retryCounter.Should().Be(0);
        }
    }

    class SqlExceptionMocker
    {
        private static T Construct<T>(params object[] p)
        {
            var ctor = typeof(T)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(c => c.GetParameters().Length == p.Length);

            return (T)ctor.Invoke(p);
        }

        public static SqlException NewSqlException(int errorNumber)
        {
            var collection = Construct<SqlErrorCollection>();
            var error = Construct<SqlError>(errorNumber, (byte)2, (byte)3, "server name", "This is a Mock-SqlException", "proc", 100, new Exception());

            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(collection, new object[] { error });


            var e = typeof(SqlException)
                .GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.ExplicitThis, new[] { typeof(SqlErrorCollection), typeof(string) }, new ParameterModifier[] { })
                ?.Invoke(null, new object[] { collection, string.Empty }) as SqlException;

            return e!;
        }
    }
}

