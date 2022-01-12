using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core.Loaders
{
    class WindowFormHandler : ILoadable
    {
        //We need this handler since if the file that holds the reference to windowForm is opened at all on a different platform it will attempt to load it and crash even if it never passes over the problem code
        //so we use this file to decide if we load window forms or not

        public float Priority => 1.3f;

        public WindowForm form = null;

        public void Load()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                form = new WindowForm();
                form.Load();
            }
        }

        public void Unload()
        {
            
            if (form != null)
            {
                form.Unload();
            }
        }
    }
}
