﻿using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Core.Loaders;
using System;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Breacher
{
	[AutoloadEquip(EquipType.HandsOn)]
	public class ReactivePlating : ModItem
	{
		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Reactive Plating");
			Tooltip.SetDefault("Gain brief damage resistance after taking several hits\n'The shielding activates, but only after... repeated triggers.'");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 28;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(0, 4, 0, 0);
			Item.defense = 4;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player Player, bool hideVisual)
		{
			ArmorPlatingPlayer modPlayer = Player.GetModPlayer<ArmorPlatingPlayer>();
			modPlayer.active = true;
			if (modPlayer.Shield)
				Player.endurance += 0.3f;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<SpaceEvent.Astroscrap>(), 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class ArmorPlatingPlayer : ModPlayer
	{
		public bool active = false;

		private int damageCounter;

		public int shieldTimer = 0;
		public int flickerTime = 0;

		public bool Shield => shieldTimer > 0;

		public override void ResetEffects()
		{
			active = false;

			if (damageCounter > 0)
				damageCounter--;

			if (shieldTimer > 0)
			{
				shieldTimer--;
				flickerTime++;
			}
			else
			{
				flickerTime = 0;
			}

			if (damageCounter >= 200)
			{
				damageCounter = 0;
				shieldTimer = 200;
			}
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (active && !Shield)
				damageCounter += 170;
		}
	}

	public class ReactivePlatingHelper : IOrderedLoadable
	{
		public float Priority => 1.05f;

		public void Load()
		{
			if (Main.dedServ)
				return;

			StarlightPlayer.PostDrawEvent += DrawOverlay;
		}

		private void DrawOverlay(Player player, SpriteBatch spriteBatch)
		{
			ArmorPlatingPlayer modPlayer = player.GetModPlayer<ArmorPlatingPlayer>();

			if (modPlayer.Shield)
				DrawPlayerTarget(modPlayer.flickerTime, modPlayer.shieldTimer, player);
		}

		public void Unload() { }

		private static void DrawPlayerTarget(int flickerTime, int shieldTimer, Player drawPlayer)
		{
			if (!PlayerTarget.canUseTarget)
				return;

			GraphicsDevice gD = Main.graphics.GraphicsDevice;
			SpriteBatch spriteBatch = Main.spriteBatch;
			Texture2D target = PlayerTarget.Target;

			if (Main.dedServ || spriteBatch == null || gD == null || target == null)
				return;

			Effect effect = ShaderLoader.GetShader("BreacherScan").Value;

			if (effect != null)
			{
				effect.Parameters["uImageSize0"].SetValue(new Vector2(PlayerTarget.sheetSquareX, PlayerTarget.sheetSquareY));
				effect.Parameters["alpha"].SetValue((float)Math.Pow(shieldTimer / 200f, 0.25f));

				spriteBatch.End();
				spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.ZoomMatrix);

				if (flickerTime > 0 && flickerTime < 16)
				{
					float flickerTime2 = (float)(flickerTime / 20f);
					float whiteness = 1.5f - (flickerTime2 * flickerTime2 / 2 + 2f * flickerTime2);
					effect.Parameters["whiteness"].SetValue(whiteness);
				}
				else
				{
					effect.Parameters["whiteness"].SetValue(0);
				}

				Color color = Color.Cyan;
				effect.Parameters["red"].SetValue(color.ToVector4());
				color.A = 230;
				effect.Parameters["red2"].SetValue(color.ToVector4());

				effect.CurrentTechnique.Passes[0].Apply();
				spriteBatch.Draw(target, PlayerTarget.getPlayerTargetPosition(drawPlayer.whoAmI), PlayerTarget.getPlayerTargetSourceRectangle(drawPlayer.whoAmI), Color.White);

				spriteBatch.End();
				spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}
	}
}