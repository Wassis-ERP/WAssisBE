using System.Diagnostics;
using WAssis.Infra.CrossCutting.IoC;

namespace WAssis.Tests.Security;

public sealed class TelemetryPrivacyTests
{
    [Fact]
    public void ExportedSpanDoesNotContainPayloadOrSqlOrFullUrl()
    {
        using var activity = new Activity("SELECT private_data").Start();
        activity.SetTag("db.statement", "SELECT private_data");
        activity.SetTag("url.full", "https://example.invalid?token=sensitive");
        activity.SetTag("http.request.header.authorization", "sensitive");
        activity.SetTag("http.route", "/api/segurados/{id}");
        activity.SetStatus(ActivityStatusCode.Error, "sensitive exception");
        new SafeTelemetryProcessor().OnEnd(activity);
        Assert.Null(activity.GetTagItem("db.statement"));
        Assert.Null(activity.GetTagItem("url.full"));
        Assert.Null(activity.GetTagItem("http.request.header.authorization"));
        Assert.Null(activity.StatusDescription);
        Assert.Equal("HTTP /api/segurados/{id}", activity.DisplayName);
    }
}
