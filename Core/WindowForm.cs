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
                if(Environment.OSVersion.Platform == PlatformID.Win32NT)
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
            GameForm.Icon = Texture2Icon(ModContent.GetTexture(GetModIcon()));
            GameForm.Text = GetWindowTitle();
        }

        private string GetModIcon()
        {
            if (DateChanges.AprilFirst)
                return "StarlightRiver/Assets/Misc/StarlightTroll";
            else if (DateChanges.Anniversary)
                return "StarlightRiver/Assets/Misc/StarlightCake";
            else if (DateChanges.Halloween)
                return "StarlightRiver/Assets/Misc/StarlightHalloween";
            else if (DateChanges.Christmas)
                return "StarlightRiver/Assets/Misc/StarlightChristmas";

            else if (DateChanges.StartupRandom16 == 32767)//1 in 32767
                return "StarlightRiver/Assets/Misc/StarlightOld";
            else if (DateChanges.StartupRandom16 < 4)//1 in 8191
                return "StarlightRiver/Assets/Misc/StarlightGalaxy";

            else if(DateChanges.StartupRandom8 == 0)//1 in 255
                return "StarlightRiver/Assets/Misc/StarlightStream";
            else if (DateChanges.StartupRandom8 > 247)//1 in 32
                return "StarlightRiver/Assets/Misc/StarlightTablet";

            else
                return "StarlightRiver/Assets/Misc/StarlightTree";
        }

        private string GetWindowTitle()
        {//TODO: change this to make some more common
            if (DateChanges.StartupRandom16 == 922)//1 in 32767
                return "Of course you listen to weezer";
            else if (DateChanges.StartupRandom16 == 629)//1 in 32767
                return "Wait, it's all Grubby?";
            else if (DateChanges.StartupRandom16 == 621)//1 in 32767
                return "Case of the vanishing cheese...";
            else if (DateChanges.StartupRandom16 == 1025)//1 in 32767
                return "Mucho gracias for dollaris";
            else if (DateChanges.StartupRandom16 == 905)//1 in 32767
                return "\"Huge breaking change\"";
            else if (DateChanges.StartupRandom16 == 906)//1 in 32767
                return "Revert \"Huge breaking change\"";
            else if (DateChanges.StartupRandom16 == 907)//1 in 32767
                return "Revert \"Revert \"Huge breaking change\"\"";
            else if (DateChanges.StartupRandom16 == 429)//1 in 32767
                return "Amoung pequeno";
            else if (DateChanges.StartupRandom16 == 1011)//1 in 32767
                return "Attention SLR gamers!";
            else if (DateChanges.StartupRandom16 == 401)//1 in 32767
                return "vro you literally deleted the actual general chat";
            else if (DateChanges.StartupRandom16 == 1110)//1 in 32767
                return "Idea: suffer";

            else
                return DefaultTitle;
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
