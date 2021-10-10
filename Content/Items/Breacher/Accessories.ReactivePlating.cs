using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.CustomHooks;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
			item.width = 30;
			item.height = 28;
			item.rare = 3;
			item.value = Item.buyPrice(0, 4, 0, 0);
			item.defense = 4;
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			ArmorPlatingPlayer modPlayer = player.GetModPlayer<ArmorPlatingPlayer>();
			modPlayer.active = true;
			if (modPlayer.Shield)
				player.endurance += 0.3f;
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
				flickerTime = 0;

			if (damageCounter >= 200)
			{
				damageCounter = 0;
				shieldTimer = 150;
			}
		}

		public override void OnHitByNPC(NPC npc, int damage, bool crit)
		{
			if (active && !Shield)
				damageCounter += 100;
		}

		public override void OnHitByProjectile(Projectile proj, int damage, bool crit)
		{
			if (active && !Shield)
				damageCounter += 100;
		}
	}
	public class ReactivePlatingHelper : ILoadable
	{

		public float Priority { get => 1.05f; }

		public void Load()
		{
			if (Main.dedServ)
				return;

			On.Terraria.Main.DrawPlayer += Main_DrawPlayer;
		}

		public void Unload()
		{

		}

		private static void Main_DrawPlayer(On.Terraria.Main.orig_DrawPlayer orig, Main self, Player drawPlayer, Vector2 Position, float rotation, Vector2 rotationOrigin, float shadow)
		{
			ArmorPlatingPlayer modPlayer = drawPlayer.GetModPlayer<ArmorPlatingPlayer>();
			orig(self, drawPlayer, Position, rotation, rotationOrigin, shadow);
			if (modPlayer.Shield && drawPlayer == Main.LocalPlayer)
				DrawPlayerTarget(drawPlayer, Position, rotation, rotationOrigin, shadow, modPlayer.flickerTime, modPlayer.shieldTimer);
		}

		private static void DrawPlayerTarget(Player drawPlayer, Vector2 Position, float rotation, Vector2 rotationOrigin, float shadow, int flickerTime, int shieldTimer)
        {
			GraphicsDevice gD = Main.graphics.GraphicsDevice;
			SpriteBatch spriteBatch = Main.spriteBatch;
			RenderTarget2D target = PlayerTarget.Target;

			if (Main.dedServ || spriteBatch == null || gD == null || target == null)
				return;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null);

			Effect effect = Filters.Scene["BreacherScan"].GetShader().Shader;
			effect.Parameters["uImageSize0"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
			effect.Parameters["alpha"].SetValue((float)Math.Pow((float)shieldTimer / 150f, 0.25f));

			if (flickerTime > 0 && flickerTime < 16)
			{
				float flickerTime2 = (float)(flickerTime / 20f);
				float whiteness = 1.5f - (((flickerTime2 * flickerTime2) / 2) + (2f * flickerTime2));
				effect.Parameters["whiteness"].SetValue(whiteness);
			}
			else
				effect.Parameters["whiteness"].SetValue(0);

			Color color = Color.Cyan;
			effect.Parameters["red"].SetValue(color.ToVector4());
			color.A = 230;
			effect.Parameters["red2"].SetValue(color.ToVector4());

			effect.CurrentTechnique.Passes[0].Apply();
			spriteBatch.Draw(target, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}
}