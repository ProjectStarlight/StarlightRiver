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

		public virtual void HitBarrierEffects(Player Player) { }

		public virtual void LoseBarrierEffects(Player Player) { }

		public virtual void PreDrawEffects(SpriteBatch spriteBatch, Player Player) { }

		public virtual void PostDrawEffects(SpriteBatch spriteBatch, Player Player) { }

		public override bool CanRightClick() => true;

        public override void RightClick(Player Player)
        {
			ShieldPlayer mp = Player.GetModPlayer<ShieldPlayer>();

			Item prevBarrierItem = mp.barrierDyeItem;
			Player.GetModPlayer<ShieldPlayer>().barrierDyeItem = Item.Clone();
			Item.TurnToAir();
			mp.rechargeAnimation = 0;


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

		public override void PreDrawEffects(SpriteBatch spriteBatch, Player Player)
		{
			if (!CustomHooks.PlayerTarget.canUseTarget)
				return;

			var barrier = Player.GetModPlayer<ShieldPlayer>();

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			float opacity = barrier.rechargeAnimation;

			float sin = (float)Math.Sin(Main.GameUpdateCount / 10f);

			for (int k = 0; k < 8; k++)
			{
				Vector2 dir = Vector2.UnitX.RotatedBy(k / 8f * 6.28f) * (5.5f + sin * 3.2f);
				var color = new Color(100, 255, 255) * (opacity - sin * 0.1f) * 0.9f;

				spriteBatch.Draw(CustomHooks.PlayerTarget.Target, CustomHooks.PlayerTarget.getPlayerTargetPosition(Player.whoAmI) + dir, CustomHooks.PlayerTarget.getPlayerTargetSourceRectangle(Player.whoAmI), color);
			}

			spriteBatch.End();

			SamplerState samplerState = Main.DefaultSamplerState;

			if (Player.mount.Active)
				samplerState = Main.MountedSamplerState; //PORTTODO: Figure out what the fuck this is supposed to be

			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}
	}
}
