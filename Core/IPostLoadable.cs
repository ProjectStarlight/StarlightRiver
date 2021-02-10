using StarlightRiver.Core;

namespace StarlightRiver.Core
{
    interface IPostLoadable
    {
        void PostLoad();
        void PostLoadUnload();
    }
}
