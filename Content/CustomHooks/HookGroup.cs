using StarlightRiver.Core;

namespace StarlightRiver.Content.CustomHooks
{
    public class HookGroup : ILoadable
    {
        public virtual SafetyLevel Safety => SafetyLevel.OhGodOhFuck;

        public virtual float Priority { get => 1f; }

        public virtual void Load() { }

        public virtual void Unload() { }
    }

    /// <summary>
    /// Rules:
    /// 1. IL should never be anything lower than Questionable
    /// 2. When in doubt, pick the higher rating (more dangerous)
    /// 3. If possible, leave a short comment explaining the rating of the hook group
    /// </summary>
    public enum SafetyLevel
    {
        Safe = 0,
        Questionable = 1,
        Fragile = 2,
        OhGodOhFuck = 3
    }
}
