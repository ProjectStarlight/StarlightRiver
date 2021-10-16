using Microsoft.Xna.Framework;
using Terraria;
using System.Windows.Forms;
using System.Drawing;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria.ModLoader;

namespace StarlightRiver.Core.Loaders
{
	class WindowForm : ILoadable
    {
        public static Form GameForm;

        public float Priority => 1.3f;

        const string DefaultTitle = "Starlight River DEMO";

        const int iconSize = 48; //16, 32, 48

        private void GameFormChanges()
        {
            GameForm.Icon = Texture2Icon(ModContent.GetTexture("StarlightRiver/Assets/Misc/StarlightTree"));
            GameForm.Text = DefaultTitle;//TODO: make random.
        }

        public void Load()
        {
            if (!Main.dedServ)
            {
                using (Control threadControl = new Control())
                {
                    //_threadControl.Enabled = true;
                    //_threadControl.Visible = true;
                    IntPtr ptr = threadControl.Handle;//this needs to be accessed for it to be generated
                    if (threadControl.IsHandleCreated)//if the icon doesn't show its its likely because this check failed
                        threadControl.Invoke((Action)(() => 
                        { GameForm = (Form)Control.FromHandle(Main.instance.Window.Handle); }));
                }

                InvokeOnForm(GameFormChanges);
            }
        }

        //public static Image Texture2Image(Texture2D texture)
        //{
        //    Image img;
        //    using (MemoryStream MS = new MemoryStream())
        //    {
        //        texture.SaveAsPng(MS, texture.Width, texture.Height);
        //        Go To the beginning of the stream.
        //       MS.Seek(0, SeekOrigin.Begin);
        //        Create the image based on the stream.
        //        img = Bitmap.FromStream(MS);
        //    }
        //    return img;
        //}
        public static Icon Texture2Icon(Texture2D texture)
        {
            Icon icon;
            using (MemoryStream MS = new MemoryStream())
            {
                texture.SaveAsPng(MS, iconSize, iconSize);
                //Go To the  beginning of the stream.
                MS.Seek(0, SeekOrigin.Begin);

                //Create the image based on the stream.
                icon = Icon.FromHandle(new Bitmap(MS).GetHicon());
            }
            return icon;
        }
        public static void InvokeOnForm(Action action)
        {
            if(GameForm != null)
                GameForm.Invoke(action);
        }
        public void Unload()
        {

        }
    }
}
