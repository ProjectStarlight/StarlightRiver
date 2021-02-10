using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
    class TempleRune : SmartAccessory
    {
        public override string Texture => AssetDirectory.CaveTempleItem + Name;

        private int RuneTimer;
        public TempleRune() : base("Rune of Warding", "Periodically provides +5 Defense") { }
        public override void SafeSetDefaults()
        {
            item.rare = ItemRarityID.Blue;
        }
        public override void SafeUpdateEquip(Player player)
        {
            RuneTimer++;
            if (RuneTimer < 300)
            {
                Lighting.AddLight(player.Center, new Vector3(1, 0.5f, 0.2f) * 0.2f);
                player.statDefense += 5;
                for (float k = RuneTimer % 5 * 0.1f; k < 6.28f; k += 0.5f)
                {
                    Vector2 off = new Vector2((float)Math.Cos(k + RuneTimer / 100f) * player.width, (float)Math.Sin(k + RuneTimer / 100f) * player.height);
                    Dust d = Dust.NewDustPerfect(player.Center, DustType<Content.Dusts.PlayerFollowOrange>(), off);
                    d.customData = player.whoAmI;
                }
            }
            if (RuneTimer > 600) RuneTimer = 0;
        }
    }
}
