﻿using System.Collections.Specialized;
using Castle;
using Castle.Config;
using FluentAssertions;
using Tests.SetUp;
using Xunit;

namespace Tests.Messages
{
    public class When_creating_request_context_for_Framework
    {
        [Theory, AutoFakeData]
        public void Should_get_client_id_from_castle_header_if_present(
            string castleHeaderValue,
            string cookieValue)
        {
            var headers = new NameValueCollection
            {
                ["X-Castle-Client-ID"] = castleHeaderValue
            };

            string GetCookie(string name) => name == "__cid" ? cookieValue : null;

            var result = Context.GetClientIdForFramework(headers, GetCookie);

            result.Should().Be(castleHeaderValue);
        }

        [Theory, AutoFakeData]
        public void Should_get_client_id_from_cookie_if_castle_header_not_present(
            string otherHeader,
            string otherHeaderValue,
            string cookieValue)
        {
            var headers = new NameValueCollection
            {
                [otherHeader] = otherHeaderValue
            };

            string GetCookie(string name) => name == "__cid" ? cookieValue : null;

            var result = Context.GetClientIdForFramework(headers, GetCookie);

            result.Should().Be(cookieValue);
        }

        [Theory, AutoFakeData]
        public void Should_use_empty_string_if_unable_to_get_client_id(
            string otherHeader,
            string otherHeaderValue,
            string otherCookie,
            string otherCookieValue)
        {
            var headers = new NameValueCollection
            {
                [otherHeader] = otherHeaderValue
            };

            string GetCookie(string name) => name == otherCookie ? otherCookieValue : null;

            var result = Context.GetClientIdForFramework(headers, GetCookie);

            result.Should().Be("");
        }

        [Theory, AutoFakeData]
        public void Should_get_ip_from_supplied_headers_in_order(
            CastleConfiguration cfg,
            string ipHeader,
            string ip,
            string secondaryIpHeader,
            string secondaryIp,
            string otherHeader,
            string otherHeaderValue,
            string httpContextIp)
        {
            var headers = new NameValueCollection
            {
                [ipHeader] = ip,
                [secondaryIpHeader] = secondaryIp,
                [otherHeader] = otherHeaderValue
            };

            var result = Context.GetIpForFramework(headers, new [] {  ipHeader, secondaryIpHeader }, () => httpContextIp, () => cfg);

            result.Should().Be(ip);
        }

        [Theory, AutoFakeData]
        public void Should_get_ip_from_second_header_if_first_is_not_found(
            CastleConfiguration cfg,
            string ipHeader,
            string secondaryIpHeader,
            string secondaryIp,
            string otherHeader,
            string otherHeaderValue,
            string httpContextIp)
        {
            var headers = new NameValueCollection
            {
                [secondaryIpHeader] = secondaryIp,
                [otherHeader] = otherHeaderValue
            };

            var result = Context.GetIpForFramework(headers, new[] { ipHeader, secondaryIpHeader }, () => httpContextIp, () => cfg);

            result.Should().Be(secondaryIp);
        }

        [Theory, AutoFakeData]
        public void Should_get_ip_from_httpcontext_if_no_header_supplied(
            CastleConfiguration cfg,
            string ipHeader,
            string ip,
            string otherHeader,
            string otherHeaderValue,
            string httpContextIp)
        {
            var headers = new NameValueCollection
            {
                [ipHeader] = ip,
                [otherHeader] = otherHeaderValue
            };

            var result = Context.GetIpForFramework(headers, null, () => httpContextIp, () => cfg);

            result.Should().Be(httpContextIp);
        }

        [Theory, AutoFakeData]
        public void Should_get_regular_ip(
            CastleConfiguration cfg,
            string ipHeader,
            string ip
        )
        {
            var headers = new NameValueCollection
            {
                [ipHeader] = ip,
            };

            var result = Context.GetIpForFramework(headers, null, () => ip, () => cfg);

            result.Should().Be(ip);
        }

        [Theory, AutoFakeData]
        public void Should_get_other_ip_header(CastleConfiguration cfg, string cfConnectiongIp)
        {
            var headers = new NameValueCollection
            {
                ["Cf-Connecting-Ip"] = cfConnectiongIp,
                ["X-Forwarded-For"] = "1.1.1.1, 1.2.2.2, 1.2.3.5"
            };

            var ipHeaders = new[] {"Cf-Connecting-Ip", "X-Forwarded-For"};

            var result = Context.GetIpForFramework(headers, ipHeaders, () => cfConnectiongIp, () => cfg);

            result.Should().Be(cfConnectiongIp);
        }

        [Theory, AutoFakeData]
        public void Should_get_first_available_with_all_trusted_proxies(CastleConfiguration cfg, string defaultIp)
        {
            var headers = new NameValueCollection
            {
                ["Remote-Addr"] = "127.0.0.1",
                ["X-Forwarded-For"] = "127.0.0.1,10.0.0.1,172.31.0.1,192.168.0.1"
            };

            var result = Context.GetIpForFramework(headers, null, () => defaultIp, () => cfg);
            result.Should().Be("127.0.0.1");
        }

        [Theory, AutoFakeData]
        public void Should_get_first_available_with_trust_proxy_chain(CastleConfiguration cfg, string defaultIp)
        {
            var headers = new NameValueCollection
            {
                ["Remote-Addr"] = "6.6.6.4",
                ["X-Forwarded-For"] = "6.6.6.6, 2.2.2.3, 6.6.6.5"
            };

            cfg.TrustProxyChain = true;

            var result = Context.GetIpForFramework(headers, null, () => defaultIp, () => cfg);
            result.Should().Be("6.6.6.6");
        }

        [Theory, AutoFakeData]
        public void Should_get_remote_addr_if_others_internal(CastleConfiguration cfg, string defaultIp)
        {
            var headers = new NameValueCollection
            {
                ["Remote-Addr"] = "6.5.4.3",
                ["X-Forwarded-For"] = "127.0.0.1,10.0.0.1,172.31.0.1,192.168.0.1"
            };

            var result = Context.GetIpForFramework(headers, null, () => defaultIp, () => cfg);
            result.Should().Be("6.5.4.3");
        }

        [Theory, AutoFakeData]
        public void Should_get_equivalent_to_trusted_proxy_depth_1(CastleConfiguration cfg, string defaultIp)
        {
            var headers = new NameValueCollection
            {
                ["Remote-Addr"] = "6.6.6.4",
                ["X-Forwarded-For"] = "6.6.6.6, 2.2.2.3, 6.6.6.5"
            };

            cfg.TrustedProxyDepth = 1;

            var result = Context.GetIpForFramework(headers, null, () => defaultIp, () => cfg);
            result.Should().Be("2.2.2.3");
        }

        [Theory, AutoFakeData]
        public void Should_get_equivalent_to_trusted_proxy_depth_2_ip_headers(CastleConfiguration cfg, string defaultIp)
        {
            var headers = new NameValueCollection
            {
                ["Remote-Addr"] = "6.6.6.4",
                ["X-Forwarded-For"] = "6.6.6.6, 2.2.2.3, 6.6.6.5, 6.6.6.7"
            };

            cfg.TrustedProxyDepth = 2;
            cfg.IpHeaders = new[] {"X-Forwarded-For", "Remote-Addr"};

            var result = Context.GetIpForFramework(headers, null, () => defaultIp, () => cfg);
            result.Should().Be("2.2.2.3");
        }
    }
}
