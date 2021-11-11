using Microsoft.Xna.Framework;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricBossBarrier : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            On.Terraria.Main.Update += UpdateColission; //TODO: Find a better/cleaner way to do this
            return true;
        }

        private void UpdateColission(On.Terraria.Main.orig_Update orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);

            if (Main.gameMenu)
                return;

            Main.tileSolid[TileType<VitricBossBarrier>()] = Main.npc.Any(n => n.active && n.type == NPCType<VitricBoss>());
        }

		public override void SetDefaults()
        {
            TileID.Sets.DrawsWalls[Type] = true;
            Main.tileBlockLight[Type] = false;
            minPick = 999;
        }
    }
}