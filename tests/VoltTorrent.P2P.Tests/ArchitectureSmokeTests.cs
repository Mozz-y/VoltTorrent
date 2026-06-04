using FluentAssertions;
using Xunit;

namespace VoltTorrent.P2P.Tests;

public sealed class ArchitectureSmokeTests
{
    [Fact]
    public void TestProject_IsConfigured()
    {
        typeof(ArchitectureSmokeTests).Assembly.GetName().Name.Should().Be("VoltTorrent.P2P.Tests");
    }
}
