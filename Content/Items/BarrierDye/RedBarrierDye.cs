using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.Items.BarrierDye
{
	class RedBarrierDye : BarrierDye
	{
		public override string Texture => AssetDirectory.BarrierDyeItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Rose Tincture");
			Tooltip.SetDefault("Barrier FX turn a vibrant red");
		}

		public override void SetDefaults()
		{
			item.useTime = 10;
			item.useAnimation = 10;
			item.useStyle = 1;
		}

		public override bool UseItem(Player player)
		{
			var barrier = player.GetModPlayer<ShieldPlayer>();
			barrier.dye = this;

			return true;
		}

		public override void LoseBarrierEffects(Player player)
		{
			Main.PlaySound(Terraria.ID.SoundID.NPCDeath57, player.Center);

			for (int k = 0; k < 50; k++)
				Dust.NewDustPerfect(player.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2f), 0, new Color(100, 10, 10), Main.rand.NextFloat(0.5f));
		}

		public override void PreDrawEffects(SpriteBatch spriteBatch, Player player)
		{
			var barrier = player.GetModPlayer<ShieldPlayer>();

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive);

			float opacity = barrier.rechargeAnimation;

			float sin = (float)Math.Sin(Main.GameUpdateCount / 10f);

			for (int k = 0; k < 8; k++)
			{
				Vector2 dir = Vector2.UnitX.RotatedBy(k / 8f * 6.28f) * (8 + sin);
				var color = new Color(255, 50, 50) * (opacity - sin * 0.1f) * 0.8f;

				spriteBatch.Draw(CustomHooks.PlayerTarget.Target, Vector2.Zero + dir, color);
			}

			spriteBatch.End();
			spriteBatch.Begin();
		}
	}
}
