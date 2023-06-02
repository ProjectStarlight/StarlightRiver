using StarlightRiver.Core.Systems.BarrierSystem;
using System;

namespace StarlightRiver.Content.Items.BarrierDye
{
	public abstract class BarrierDye : ModItem
	{
		public virtual float RechargeAnimationRate => 0.05f;

		public virtual void HitBarrierEffects(Player Player) { }

		public virtual void LoseBarrierEffects(Player Player) { }

		public virtual void PreDrawEffects(SpriteBatch spriteBatch, Player Player) { }

		public virtual void PostDrawEffects(SpriteBatch spriteBatch, Player Player) { }

		public override bool CanRightClick()
		{
			return true;
		}

		public override void RightClick(Player Player)
		{
			BarrierPlayer mp = Player.GetModPlayer<BarrierPlayer>();

			Item prevBarrierItem = mp.barrierDyeItem;
			Player.GetModPlayer<BarrierPlayer>().barrierDyeItem = Item.Clone();
			Item.TurnToAir();
			mp.rechargeAnimationTimer = 0;

			Main.EquipPageSelected = 2;

			if (prevBarrierItem.type != ModContent.ItemType<BaseBarrierDye>())
				Main.LocalPlayer.GetItem(Main.myPlayer, prevBarrierItem.Clone(), GetItemSettings.ItemCreatedFromItemUsage);
		}
	}

	class BaseBarrierDye : BarrierDye
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void UpdateInventory(Player Player)
		{
			if (Main.mouseItem == Item)
				Item.TurnToAir();
		}

		public override void LoseBarrierEffects(Player Player)
		{
			Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.NPCDeath57, Player.Center);

			for (int k = 0; k < 50; k++)
				Dust.NewDustPerfect(Player.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4f), 0, new Color(20, 100, 110), Main.rand.NextFloat(0.5f));
		}

		public override void PreDrawEffects(SpriteBatch spriteBatch, Player player)
		{
			if (!CustomHooks.PlayerTarget.canUseTarget)
				return;

			BarrierPlayer barrier = player.GetModPlayer<BarrierPlayer>();

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			float opacity = barrier.rechargeAnimationTimer;

			float sin = (float)Math.Sin(Main.GameUpdateCount / 10f);

			for (int k = 0; k < 8; k++)
			{
				Vector2 dir = Vector2.UnitX.RotatedBy(k / 8f * 6.28f) * (5.5f + sin * 3.2f);
				Color color = new Color(100, 255, 255) * (opacity - sin * 0.1f) * 0.9f;

				if (Main.LocalPlayer.gravDir == -1f)
					spriteBatch.Draw(CustomHooks.PlayerTarget.Target, CustomHooks.PlayerTarget.getPlayerTargetPosition(player.whoAmI) + dir, CustomHooks.PlayerTarget.getPlayerTargetSourceRectangle(player.whoAmI), color, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
				else
					spriteBatch.Draw(CustomHooks.PlayerTarget.Target, CustomHooks.PlayerTarget.getPlayerTargetPosition(player.whoAmI) + dir, CustomHooks.PlayerTarget.getPlayerTargetSourceRectangle(player.whoAmI), color);
			}

			spriteBatch.End();

			SamplerState samplerState = Main.DefaultSamplerState;

			if (player.mount.Active)
				samplerState = Terraria.Graphics.Renderers.LegacyPlayerRenderer.MountedSamplerState;

			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}
	}
}