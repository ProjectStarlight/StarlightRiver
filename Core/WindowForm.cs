using Microsoft.Xna.Framework;
using Terraria;
using System.Windows.Forms;
using System.Drawing;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria.ModLoader;
using System.Reflection;

namespace StarlightRiver.Core.Loaders
{
	class WindowForm : ILoadable
    {
        public static Form GameForm;

        public float Priority => 1.3f;

        const string DefaultTitle = "Starlight River Test Build";

        const int iconSize = 48; //16, 32, 48

        public void Load()
        {
            if (!Main.dedServ)
            {
                using (Control threadControl = new Control())
                {
                    IntPtr ptr = threadControl.Handle;//this needs to be accessed for it to be generated
                    if (threadControl.IsHandleCreated)//if the icon doesn't show its its likely because this check failed
                        threadControl.Invoke((Action)(() => 
                        { GameForm = (Form)Control.FromHandle(Main.instance.Window.Handle); }));
                }

                TryFormChanges();
            }
        }

        private Func<Form, bool> IsFormActive;

        private void TryFormChanges()//check if the window is active
        {
            PropertyInfo IsActiveInfo = typeof(Form).GetProperty("Active", BindingFlags.Instance | BindingFlags.NonPublic);//grab 'Active' property info (Unsure why this is private)
            IsFormActive = (Func<Form, bool>)Delegate.CreateDelegate( typeof(Func<Form, bool>), IsActiveInfo.GetGetMethod(true));//create delegate for property getter

            if (!IsFormActive(GameForm))//if inactive detour Main.DoUpdate and wait for it to become active
                On.Terraria.Main.DoUpdate += WaitForWindowAvailible;
            else
                InvokeOnForm(WindowFormChanges);//if active change icon
        }

        private void WaitForWindowAvailible(On.Terraria.Main.orig_DoUpdate orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);
            if (IsFormActive(GameForm))//if form becomes active change icon and un-detour Main.DoUpdate
            {
                InvokeOnForm(WindowFormChanges);
                On.Terraria.Main.DoUpdate -= WaitForWindowAvailible;
            }
        }

        private void WindowFormChanges()//generates a icon from a png and changes the window text
        {
            GameForm.Icon = Texture2Icon(ModContent.GetTexture("StarlightRiver/Assets/Misc/StarlightTree"));
            GameForm.Text = DefaultTitle;//TODO: make random.
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
        {//this seems to break if the window is minimized in fullscreen... or might be setting it breaking
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
            On.Terraria.Main.DoUpdate -= WaitForWindowAvailible;
        }
    }
}
