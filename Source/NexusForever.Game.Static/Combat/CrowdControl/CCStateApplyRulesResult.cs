namespace NexusForever.Game.Static.Combat.CrowdControl
{
    public enum CCStateApplyRulesResult
    {
        Ok,
        InvalidCCState,
        NoTargetSpecified,
        TargetImmune,
        TargetInfiniteInterruptArmor,
        TargetInterruptArmorReduced,
        TargetInterruptArmorBlocked,
        StackingDoesNotStack,
        StackingShorterDuration,
        DiminishingReturnsTriggerCap
    }
}
