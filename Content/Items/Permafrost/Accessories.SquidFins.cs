using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.AuroraWaterSystem;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Permafrost
{
	public class SquidFins : SmartAccessory
	{
		public override string Texture => AssetDirectory.PermafrostItem + Name;

		public SquidFins() : base("Squid Fins", "Allows you to swim like a jellysquid") { }

		public override void Load()
		{
			StarlightPlayer.ModifyDrawInfoEvent += DrawSquidFins;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Expert;
			Item.expert = true;
			Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(0, 2));
		}

		public override void SafeUpdateEquip(Player player)
		{
			bool canSwim = player.grapCount <= 0 && player.wet && !player.mount.Active;
			player.GetModPlayer<SwimPlayer>().ShouldSwim = canSwim;
			player.GetModPlayer<SwimPlayer>().SwimSpeed = 1.33f + player.moveSpeed * 1.33f;
		}

		private void DrawSquidFins(ref PlayerDrawSet drawInfo)
		{
			Player player = drawInfo.drawPlayer;

			if (Equipped(player))
			{
				Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.PermafrostItem + "SquidFinsBack").Value;
				SpriteEffects effects = player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				var offset = new Vector2(-7 * player.direction, player.gfxOffY);

				drawInfo.DrawDataCache.Add(new DrawData(tex, player.Center + offset - Main.screenPosition, null, Lighting.GetColor((player.Center / 16).ToPoint()), 0, tex.Size() / 2, 1, effects, 0));
			}
		}
	}
}
