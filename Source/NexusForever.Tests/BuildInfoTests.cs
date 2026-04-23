using NexusForever.Shared;

namespace NexusForever.Tests;

public class BuildInfoTests
{
    [Fact]
    public void WithMilestonePrefixesMessage()
    {
        Assert.Equal("XYF-1.3 Welcome", BuildInfo.WithMilestone("Welcome"));
    }

    [Fact]
    public void WithMilestoneUsesMilestoneForEmptyMessage()
    {
        Assert.Equal(BuildInfo.Milestone, BuildInfo.WithMilestone(""));
    }

    [Fact]
    public void WithMilestoneDoesNotDoublePrefix()
    {
        Assert.Equal("XYF-1.3 Welcome", BuildInfo.WithMilestone("XYF-1.3 Welcome"));
    }

    [Fact]
    public void WithMilestoneRequiresWholeTagMatch()
    {
        Assert.Equal("XYF-1.3 XYF-1.20 Welcome", BuildInfo.WithMilestone("XYF-1.20 Welcome"));
    }
}
