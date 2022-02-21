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
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.BarrierDye
{
	public abstract class BarrierDye : ModItem
	{
		public virtual float RechargeAnimationRate => 0.05f;

		public virtual void HitBarrierEffects(Player player) { }

		public virtual void LoseBarrierEffects(Player player) { }

		public virtual void PreDrawEffects(SpriteBatch spriteBatch, Player player) { }

		public virtual void PostDrawEffects(SpriteBatch spriteBatch, Player player) { }

		public override bool CanRightClick() => true;

        public override void RightClick(Player player)
        {
			ShieldPlayer mp = player.GetModPlayer<ShieldPlayer>();

			Item prevBarrierItem = mp.barrierDyeItem;
			player.GetModPlayer<ShieldPlayer>().barrierDyeItem = item.Clone();
			item.TurnToAir();
			mp.rechargeAnimation = 0;


			Main.EquipPageSelected = 2;

			if (prevBarrierItem.type != ModContent.ItemType<BaseBarrierDye>())
				Main.LocalPlayer.GetItem(Main.myPlayer, prevBarrierItem.Clone());
        }
    }

	class BaseBarrierDye : BarrierDye
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void UpdateInventory(Player player)
		{
			if (Main.mouseItem == item)
				item.TurnToAir();
		}

		public override void LoseBarrierEffects(Player player)
		{
			Main.PlaySound(Terraria.ID.SoundID.NPCDeath57, player.Center);

			for (int k = 0; k < 50; k++)
				Dust.NewDustPerfect(player.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4f), 0, new Color(20, 100, 110), Main.rand.NextFloat(0.5f));
		}

		public override void PreDrawEffects(SpriteBatch spriteBatch, Player player)
		{
			if (!CustomHooks.PlayerTarget.canUseTarget)
				return;

			var barrier = player.GetModPlayer<ShieldPlayer>();

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			float opacity = barrier.rechargeAnimation;

			float sin = (float)Math.Sin(Main.GameUpdateCount / 10f);

			for (int k = 0; k < 8; k++)
			{
				Vector2 dir = Vector2.UnitX.RotatedBy(k / 8f * 6.28f) * (5.5f + sin * 1.6f);
				var color = new Color(100, 255, 255) * (opacity - sin * 0.1f) * 0.9f;

				spriteBatch.Draw(CustomHooks.PlayerTarget.Target, CustomHooks.PlayerTarget.getPlayerTargetPosition(player.whoAmI) + dir, CustomHooks.PlayerTarget.getPlayerTargetSourceRectangle(player.whoAmI), color);
			}

			spriteBatch.End();

			SamplerState samplerState = Main.DefaultSamplerState;

			if (player.mount.Active)
				samplerState = Main.MountedSamplerState;

			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}
	}
}
