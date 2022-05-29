/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core.Loaders
{
    class WindowFormHandler : IOrderedLoadable
    {
        //We need this handler since if the file that holds the reference to windowForm is opened at all on a different platform it will attempt to load it and crash even if it never passes over the problem code
        //so we use this file to decide if we load window forms or not

        public float Priority => 1.3f;

        public WindowForm form = null;

        public void Load()
        {
            return; //TODO: Figure out what to replace this with since winforms is dead as a doornail

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
}*/
