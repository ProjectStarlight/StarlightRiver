using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Misc
{
	internal class SandscriptTile : ModTile
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Misc/" + Name;

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            drop = ItemType<Content.Items.Misc.Sandscript>();
            dustType = DustID.Gold;
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            if (Main.rand.Next(2) == 0) Dust.NewDustPerfect(new Vector2(i + Main.rand.NextFloat(), j + Main.rand.NextFloat()) * 16, 204, new Vector2(0, Main.rand.NextFloat(1, 1.6f)), 0, default, 0.5f);
        }

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
            StarlightWorld.LearnRecipie("SandScripts");
		}
	}
}