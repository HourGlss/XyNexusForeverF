using NexusForever.Shared;

namespace NexusForever.Tests;

public class BuildInfoTests
{
    [Fact]
    public void WithMilestonePrefixesMessage()
    {
        Assert.Equal("XYF-1.1 Welcome", BuildInfo.WithMilestone("Welcome"));
    }

    [Fact]
    public void WithMilestoneUsesMilestoneForEmptyMessage()
    {
        Assert.Equal(BuildInfo.Milestone, BuildInfo.WithMilestone(""));
    }

    [Fact]
    public void WithMilestoneDoesNotDoublePrefix()
    {
        Assert.Equal("XYF-1.1 Welcome", BuildInfo.WithMilestone("XYF-1.1 Welcome"));
    }

    [Fact]
    public void WithMilestoneRequiresWholeTagMatch()
    {
        Assert.Equal("XYF-1.1 XYF-1.10 Welcome", BuildInfo.WithMilestone("XYF-1.10 Welcome"));
    }
}
