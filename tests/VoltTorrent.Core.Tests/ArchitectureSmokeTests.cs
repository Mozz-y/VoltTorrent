using FluentAssertions;
using Xunit;

namespace VoltTorrent.Core.Tests;

public sealed class ArchitectureSmokeTests
{
    [Fact]
    public void TestProject_IsConfigured()
    {
        typeof(ArchitectureSmokeTests).Assembly.GetName().Name.Should().Be("VoltTorrent.Core.Tests");
    }
}
