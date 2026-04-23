using NexusForever.Game.Abstract.Achievement;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Main.Questing.CrimsonIsle;
using NexusForever.Script.Main.Questing.NorthernWilds;
using NexusForever.Script.Main.Questing.Tutorial;

namespace NexusForever.Tests;

public class QuestBranchScriptTests
{
    [Fact]
    public void PoweringDownQuestScriptCompletesHiddenObjective()
    {
        var script = new PoweringDownQuestScript();

        uint updatedObjectiveId = 0u;
        uint updatedProgress = 0u;
        IQuest owner = TestProxy.Create<IQuest>(
            ("get_State", QuestState.Accepted),
            ("ObjectiveUpdate", (Action<uint, uint>)((objectiveId, progress) =>
            {
                updatedObjectiveId = objectiveId;
                updatedProgress = progress;
            })));

        script.OnLoad(owner);

        IQuestObjective objective = TestProxy.Create<IQuestObjective>(
            ("get_ObjectiveInfo", TestProxy.Create<IQuestObjectiveInfo>(("get_Id", 8229u))),
            ("IsComplete", true));

        script.OnObjectiveUpdate(objective);

        Assert.Equal(12870u, updatedObjectiveId);
        Assert.Equal(1u, updatedProgress);
    }

    [Fact]
    public void TacticalDemolitionsQuestScriptCompletesHiddenObjective()
    {
        var script = new TacticalDemolitionsQuestScript();

        uint updatedObjectiveId = 0u;
        IQuest owner = TestProxy.Create<IQuest>(
            ("get_State", QuestState.Accepted),
            ("ObjectiveUpdate", (Action<uint, uint>)((objectiveId, _) => updatedObjectiveId = objectiveId)));

        script.OnLoad(owner);

        IQuestObjective objective = TestProxy.Create<IQuestObjective>(
            ("get_ObjectiveInfo", TestProxy.Create<IQuestObjectiveInfo>(("get_Id", 8268u))),
            ("IsComplete", true));

        script.OnObjectiveUpdate(objective);

        Assert.Equal(15918u, updatedObjectiveId);
    }

    [Fact]
    public void LearningToShopQuestScriptCompletesWhenRelevantTitleArrives()
    {
        var script = new LearningToShopQuestScript();

        uint updatedObjectiveId = 0u;
        IPlayer player = TestProxy.Create<IPlayer>(
            ("get_Inventory", TestProxy.Create<IInventory>(("HasItem", (Func<uint, bool>)(_ => false)))),
            ("get_TitleManager", TestProxy.Create<ITitleManager>(("HasTitle", (Func<ushort, bool>)(_ => true)))));
        IQuest owner = TestProxy.Create<IQuest>(
            ("get_State", QuestState.Accepted),
            ("GetOwner", player),
            ("ObjectiveUpdate", (Action<uint, uint>)((objectiveId, _) => updatedObjectiveId = objectiveId)));

        script.OnLoad(owner);
        script.OnTitleAdded(400);

        Assert.Equal(21267u, updatedObjectiveId);
    }

    [Fact]
    public void MegatechWarbotEntityScriptAwardsQuestCreditToKiller()
    {
        var script = new MegatechWarbotEntityScript();

        uint updatedObjectiveId = 0u;
        ushort grantedAchievementId = 0;

        IQuestManager questManager = TestProxy.Create<IQuestManager>(
            ("GetQuestState", (Func<ushort, QuestState?>)(_ => QuestState.Accepted)),
            ("ObjectiveUpdate", (Action<uint, uint>)((objectiveId, _) => updatedObjectiveId = objectiveId)));
        ICharacterAchievementManager achievementManager = TestProxy.Create<ICharacterAchievementManager>(
            ("GrantAchievement", (Action<ushort>)(achievementId => grantedAchievementId = achievementId)));
        IPlayer killer = TestProxy.Create<IPlayer>(
            ("get_QuestManager", questManager),
            ("get_AchievementManager", achievementManager));

        script.OnDeath(killer);

        Assert.Equal(8249u, updatedObjectiveId);
        Assert.Equal((ushort)1730, grantedAchievementId);
    }

    [Fact]
    public void LoftiteCrystalEntityScriptGrantsQuestProgressAndDespawns()
    {
        var script = new LoftiteCrystalEntityScript();

        float rangeCheck = 0f;
        bool removed = false;
        IWorldEntity owner = TestProxy.Create<IWorldEntity>(
            ("SetInRangeCheck", (Action<float>)(range => rangeCheck = range)),
            ("RemoveFromMap", (Action<OnRemoveDelegate>)(_ => removed = true)));

        script.OnLoad(owner);

        uint updatedObjectiveId = 0u;
        IQuestManager questManager = TestProxy.Create<IQuestManager>(
            ("GetQuestState", (Func<ushort, QuestState?>)(_ => QuestState.Accepted)),
            ("ObjectiveUpdate", (Action<uint, uint>)((objectiveId, _) => updatedObjectiveId = objectiveId)));
        IPlayer player = TestProxy.Create<IPlayer>(("get_QuestManager", questManager));

        script.OnEnterRange(player);

        Assert.Equal(5f, rangeCheck);
        Assert.Equal(4485u, updatedObjectiveId);
        Assert.True(removed);
    }
}
