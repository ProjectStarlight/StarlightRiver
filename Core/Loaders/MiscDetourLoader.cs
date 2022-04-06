using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace StarlightRiver.Core.Loaders
{
	class MiscDetourLoader : IOrderedLoadable
    {
        public float Priority { get => 1.1f; }

        public void Load()
        {
            On.Terraria.Player.KeyDoubleTap += Player_KeyDoubleTap;
        }

        public void Unload()
        {
            On.Terraria.Player.KeyDoubleTap -= Player_KeyDoubleTap;
        }
        private static void Player_KeyDoubleTap(On.Terraria.Player.orig_KeyDoubleTap orig, Player self, int keyDir)
        {
            orig(self, keyDir);
            self.GetModPlayer<StarlightPlayer>().DoubleTapEffects(keyDir);
        }
    }
}
