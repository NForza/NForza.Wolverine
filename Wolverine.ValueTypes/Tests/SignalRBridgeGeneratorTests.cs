using System.Linq;
using Shouldly;
using Xunit;

namespace NForza.Wolverine.ValueTypes.Tests;

public class SignalRBridgeGeneratorTests
{
    [Fact]
    public void WolverineHub_EmitsBaseClass()
    {
        var source = @"
namespace TestApp;
public class Dummy { }
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        var wolverineHub = generated.FirstOrDefault(s => s.Contains("class WolverineHub"));
        wolverineHub.ShouldNotBeNull();
        wolverineHub.ShouldContain("internal abstract class WolverineHub");
        wolverineHub.ShouldContain("void Broadcast<T>()");
        wolverineHub.ShouldContain("void Emit<T>");
        wolverineHub.ShouldContain("void UsePath(string path)");
    }

    [Fact]
    public void Broadcast_GeneratesSignalRHub()
    {
        var source = @"
using NForza.Wolverine;

namespace Microsoft.AspNetCore.SignalR { public class Hub { } }

namespace TestApp
{
    public record IssueCreated(string Title);

    public class TestHub : WolverineHub
    {
        public TestHub()
        {
            UsePath(""/hub/test"");
            Broadcast<IssueCreated>();
        }
    }
}
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        var hub = generated.FirstOrDefault(s => s.Contains("class TestHubSignalR"));
        hub.ShouldNotBeNull();
        hub.ShouldContain(": Hub;");
    }

    [Fact]
    public void Broadcast_GeneratesBridgeWithHandleMethod()
    {
        var source = @"
using NForza.Wolverine;

namespace Microsoft.AspNetCore.SignalR { public class Hub { } }

namespace TestApp
{
    public record IssueCreated(string Title);
    public record IssueAssigned(string Assignee);

    public class TestHub : WolverineHub
    {
        public TestHub()
        {
            UsePath(""/hub/test"");
            Broadcast<IssueCreated>();
            Broadcast<IssueAssigned>();
        }
    }
}
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        var bridge = generated.FirstOrDefault(s => s.Contains("class TestHubSignalRBridge"));
        bridge.ShouldNotBeNull();
        bridge.ShouldContain("Handle(global::TestApp.IssueCreated @event)");
        bridge.ShouldContain("Handle(global::TestApp.IssueAssigned @event)");
        bridge.ShouldContain("Clients.All.SendAsync");
    }

    [Fact]
    public void Emit_GeneratesBridgeWithGroupSending()
    {
        var source = @"
using NForza.Wolverine;

namespace Microsoft.AspNetCore.SignalR { public class Hub { } }

namespace TestApp
{
    public record IssueAssigned(string IssueId, string Assignee);

    public class TestHub : WolverineHub
    {
        public TestHub()
        {
            UsePath(""/hub/test"");
            Emit<IssueAssigned>(e => e.IssueId);
        }
    }
}
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        var bridge = generated.FirstOrDefault(s => s.Contains("class TestHubSignalRBridge"));
        bridge.ShouldNotBeNull();
        bridge.ShouldContain("Handle(global::TestApp.IssueAssigned @event)");
        bridge.ShouldContain("Clients.Group(@event.IssueId.ToString())");
    }

    [Fact]
    public void GeneratesMapWolverineHubsExtension()
    {
        var source = @"
using NForza.Wolverine;

namespace Microsoft.AspNetCore.SignalR { public class Hub { } }
namespace Microsoft.AspNetCore.Builder { public class WebApplication { } }

namespace TestApp
{
    public record IssueCreated(string Title);

    public class TestHub : WolverineHub
    {
        public TestHub()
        {
            UsePath(""/hub/test"");
            Broadcast<IssueCreated>();
        }
    }
}
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        var registration = generated.FirstOrDefault(s => s.Contains("MapWolverineHubs"));
        registration.ShouldNotBeNull();
        registration.ShouldContain("MapHub<global::TestApp.TestHubSignalR>(\"/hub/test\")");
    }

    [Fact]
    public void MixedBroadcastAndEmit_GeneratesCorrectBridge()
    {
        var source = @"
using NForza.Wolverine;

namespace Microsoft.AspNetCore.SignalR { public class Hub { } }

namespace TestApp
{
    public record IssueCreated(string Title);
    public record IssueAssigned(string IssueId, string Assignee);

    public class TestHub : WolverineHub
    {
        public TestHub()
        {
            UsePath(""/hub/test"");
            Broadcast<IssueCreated>();
            Emit<IssueAssigned>(e => e.IssueId);
        }
    }
}
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        var bridge = generated.FirstOrDefault(s => s.Contains("class TestHubSignalRBridge"));
        bridge.ShouldNotBeNull();
        bridge.ShouldContain("Clients.All.SendAsync(\"IssueCreated\"");
        bridge.ShouldContain("Clients.Group(@event.IssueId.ToString()).SendAsync(\"IssueAssigned\"");
    }

    [Fact]
    public void NoSignalR_DoesNotGenerateBridge()
    {
        // No Microsoft.AspNetCore.SignalR.Hub stub → generator should skip
        var source = @"
using NForza.Wolverine;

namespace TestApp
{
    public record IssueCreated(string Title);

    public class TestHub : WolverineHub
    {
        public TestHub()
        {
            UsePath(""/hub/test"");
            Broadcast<IssueCreated>();
        }
    }
}
";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        var bridge = generated.FirstOrDefault(s => s.Contains("class TestHubSignalRBridge"));
        bridge.ShouldBeNull();
    }
}
