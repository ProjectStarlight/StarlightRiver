using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Misc
{
	internal class SandscriptTile : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Misc/" + Name;

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			ItemDrop = ItemType<Items.Misc.Sandscript>();
			DustType = DustID.Gold;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			if (Main.rand.Next(2) == 0)
				Dust.NewDustPerfect(new Vector2(i + Main.rand.NextFloat(), j + Main.rand.NextFloat()) * 16, 204, new Vector2(0, Main.rand.NextFloat(1, 1.6f)), 0, default, 0.5f);
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			RecipeSystem.LearnRecipie(GetInstance<Items.Misc.Sandscript>().Name);
		}
	}
}