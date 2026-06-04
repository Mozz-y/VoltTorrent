using FluentAssertions;
using Xunit;

namespace VoltTorrent.Storage.Tests;

public sealed class ArchitectureSmokeTests
{
    [Fact]
    public void TestProject_IsConfigured()
    {
        typeof(ArchitectureSmokeTests).Assembly.GetName().Name.Should().Be("VoltTorrent.Storage.Tests");
    }
}
