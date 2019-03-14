﻿using System;
using System.Threading.Tasks;
using Castle.Actions;
using Castle.Config;
using Castle.Infrastructure;
using Castle.Infrastructure.Exceptions;
using Castle.Messages;
using Castle.Messages.Requests;
using Castle.Messages.Responses;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Tests.SetUp;
using Xunit;

namespace Tests
{
    public class When_authenticating
    {
        [Theory, AutoFakeData]
        public async Task Should_return_response_if_successful(
            JObject request,
            CastleConfiguration configuration,
            Verdict response,
            IInternalLogger logger)
        {
            Task<Verdict> Send(JObject req) => Task.FromResult(response);

            var result = await Authenticate.Execute(Send, request, configuration, logger);

            result.Should().Be(response);
        }

        [Theory, AutoFakeData]
        public async Task Should_return_failover_response_if_timeout(
            JObject request,
            string requestUri,
            CastleConfiguration configuration,
            IInternalLogger logger)
        {
            configuration.FailOverStrategy = ActionType.Allow;

            Task<Verdict> Send(JObject req) => throw new CastleTimeoutException(requestUri, configuration.Timeout);

            var result = await Authenticate.Execute(Send, request, configuration, logger);

            result.Failover.Should().Be(true);
            result.FailoverReason.Should().Be("timeout");
        }

        [Theory, AutoFakeData]
        public async Task Should_return_failover_response_if_any_exception(
            JObject request,
            Exception exception,
            CastleConfiguration configuration,
            IInternalLogger logger)
        {
            configuration.FailOverStrategy = ActionType.Allow;

            Task<Verdict> Send(JObject req) => throw exception;

            var result = await Authenticate.Execute(Send, request, configuration, logger);

            result.Failover.Should().Be(true);
            result.FailoverReason.Should().Be("server error");
        }

        [Theory, AutoFakeData]
        public async Task Should_log_failover_exception_as_warning(
            JObject request,
            Exception exception,
            CastleConfiguration configuration,
            IInternalLogger logger)
        {
            configuration.FailOverStrategy = ActionType.Allow;

            Task<Verdict> Send(JObject req) => throw exception;

            await Authenticate.Execute(Send, request, configuration, logger);

            logger.Received().Warn(Arg.Is<Func<string>>(x => x() == "Failover, " + exception));
        }

        [Theory, AutoFakeData]
        public async Task Should_throw_exception_if_failing_over_with_no_strategy(
            JObject request,
            Exception exception,
            CastleConfiguration configuration,
            IInternalLogger logger)
        {
            configuration.FailOverStrategy = ActionType.None;

            Task<Verdict> Send(JObject req) => throw exception;

            Func<Task> act = async () => await Authenticate.Execute(Send, request, configuration, logger);

            await act.Should().ThrowAsync<CastleExternalException>();
        }

        [Theory, AutoFakeData]
        public async Task Should_return_failover_response_if_do_not_track_is_set(
            JObject request,
            CastleConfiguration configuration,
            Verdict response,
            IInternalLogger logger)
        {
            configuration.DoNotTrack = true;
            configuration.FailOverStrategy = ActionType.Allow;

            Task<Verdict> Send(JObject req) => Task.FromResult(response);

            var result = await Authenticate.Execute(Send, request, configuration, logger);

            result.Failover.Should().Be(true);
            result.FailoverReason.Should().Be("do not track");
        }
    }
}
