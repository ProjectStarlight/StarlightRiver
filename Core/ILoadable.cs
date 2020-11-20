using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core
{
    interface ILoadable
    {
        void Load();
        void Unload();
    }
}
