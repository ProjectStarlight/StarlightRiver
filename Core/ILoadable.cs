using StarlightRiver.Core;

namespace StarlightRiver.Core
{
    interface ILoadable
    {
        void Load();
        void Unload();
        float Priority { get; }
    }
}
