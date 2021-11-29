using Microsoft.Xna.Framework;
using Terraria;
using System.Windows.Forms;
using System.Drawing;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria.ModLoader;
using System.Reflection;

namespace StarlightRiver.Core
{
	class DateChanges : ILoadable
    {
        public static bool AprilFirst = false;
        public static bool Anniversary = false;
        public static bool Halloween = false;
        public static bool Christmas = false;
        public static bool NewYears = false;

        public static bool AnySpecialEvent = false;


        //random on startup
        public static int StartupRandom16 = 0;//32767
        public static int StartupRandom8 = 0;//255

        //randomized each real life day
        public static int DateRandom16 = 0;//32767
        public static int DateRandom8 = 0;//255


        public float Priority => 0.5f;

        public void Load()
        {
            AprilFirst = (DateTime.Now.Month == 4 && DateTime.Now.Day == 1);

            Anniversary = (DateTime.Now.Month == 8 && DateTime.Now.Day == 28);

            Halloween = (DateTime.Now.Month == 10 && DateTime.Now.Day == 31);

            Christmas = (DateTime.Now.Month == 12 && DateTime.Now.Day == 25);

            NewYears = (DateTime.Now.Month == 12 && DateTime.Now.Day == 31);

            StartupRandom16 = Main.rand.Next(32768);

            StartupRandom8 = Main.rand.Next(256);

            int dateSeed = DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.Year;

            DateRandom16 = new Random(dateSeed).Next(32768);

            DateRandom8 = new Random(dateSeed).Next(256);

            AnySpecialEvent = AprilFirst || Anniversary || Halloween || Christmas || NewYears;
        }

        public void Unload()
        {
            
        }
    }
}
