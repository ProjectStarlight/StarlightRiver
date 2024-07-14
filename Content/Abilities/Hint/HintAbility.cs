﻿using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;

namespace StarlightRiver.Content.Abilities.Hint
{
	internal class HintAbility : CooldownAbility, IOrderedLoadable
	{
		public static int effectTimer;
		public static string hintToDisplay;
		public static bool defaultHint;

		public override string Name => "Starsight";
		public override string Tooltip => "Pull a strand of meaning from the memory of the world, allowing you to reveal secrets, hidden knowledge, and messages left by ancient scholars. NEWBLOCK " +
			"Most things can be investigated - treasures, lore, and enemy weaknesses all lie in plain sight to those who can see with the eyes of a star.";

		public override float ActivationCostDefault => 0.25f;
		public override string Texture => "StarlightRiver/Assets/Abilities/Hint";
		public override Color Color => new(68, 76, 220);

		public override int CooldownMax => 60;

		public float Priority => 1f;

		public override bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys)
		{
			return abilityKeys.Get<HintAbility>().JustPressed;
		}

		public void Load()
		{
			On_Main.DrawCursor += Test;
			On_Main.DrawThickCursor += Test2;
		}

		public void Unload()
		{
			On_Main.DrawCursor -= Test;
			On_Main.DrawThickCursor -= Test2;
		}

		private void Test(On_Main.orig_DrawCursor orig, Vector2 bonus, bool smart)
		{
			if (effectTimer > 0)
			{
				Texture2D tex = Assets.Abilities.HintCursor.Value;
				int frame = (int)(effectTimer / 20f * 9);
				var source = new Rectangle(0, frame * 30, 50, 30);
				Main.spriteBatch.Draw(tex, Main.MouseScreen + Vector2.One * 8, source, Color.White, 0, new Vector2(25, 15), 1, 0, 0);

				effectTimer--;
			}
			else
			{
				orig(bonus, smart);
			}
		}

		private Vector2 Test2(On_Main.orig_DrawThickCursor orig, bool smart)
		{
			if (effectTimer > 0)
				return Vector2.Zero;
			else
				return orig(smart);
		}

		public override void OnActivate()
		{
			effectTimer = 20;

			hintToDisplay = null;
			defaultHint = false;

			Vector2 pos = Main.MouseWorld;

			// Check NPCs
			for (int k = 0; k < Main.maxNPCs; k++)
			{
				//inflate their hitbox to make it easier to hit them
				Rectangle box = Main.npc[k].Hitbox;
				box.Inflate(10, 10);

				if (box.Contains(pos.ToPoint()))
				{
					NPC npc = Main.npc[k];
					hintToDisplay ??= HintLoader.GetNPCEntry(npc);
				}
			}

			// Check Projectiles
			for (int k = 0; k < Main.maxProjectiles; k++)
			{
				Rectangle box = Main.projectile[k].Hitbox;
				box.Inflate(10, 10);

				if (box.Contains(pos.ToPoint()))
				{
					Projectile proj = Main.projectile[k];
					hintToDisplay ??= HintLoader.GetProjectileEntry(proj);
				}
			}

			// Check tiles
			Tile tile = Framing.GetTileSafely((int)pos.X / 16, (int)pos.Y / 16);

			if (tile.HasTile)
				hintToDisplay ??= HintLoader.GetTileEntry(tile);

			// Fallback to air hint
			if (string.IsNullOrEmpty(hintToDisplay))
			{
				if (HintLoader.hints.AirHints.ContainsKey(Player.GetModPlayer<HintPlayer>().AirHintState))
					hintToDisplay = HintLoader.hints.AirHints[Player.GetModPlayer<HintPlayer>().AirHintState];
				else
					hintToDisplay = HintLoader.hints.AirHints["Default"];

				defaultHint = true;
			}
		}

		/// <summary>
		/// Actions that should be taken when a hint is taken. Returns if the hint should be shown or not
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="defaultHint"></param>
		/// <returns></returns>
		public virtual bool OnHint(Vector2 pos, bool defaultHint)
		{
			return true;
		}

		public override void UpdateActive()
		{
			if (OnHint(Main.MouseWorld, defaultHint))
			{
				int i = Projectile.NewProjectile(Player.GetSource_FromThis(), Main.MouseWorld + Vector2.UnitY * -32, Vector2.Zero, ModContent.ProjectileType<HintText>(), 0, 0, Main.myPlayer);
				var proj = Main.projectile[i].ModProjectile as HintText;

				if (proj != null)
					proj.text = hintToDisplay;
			}

			Deactivate();
		}
	}

	internal class HintText : ModProjectile
	{
		public string text;
		public bool follow;

		public override string Texture => AssetDirectory.Invisible;

		public ref float Timer => ref Projectile.ai[0];

		public override void SetStaticDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.timeLeft = 2;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Timer++;

			if (text is null)
				return;

			if (Timer < text.Length * 6f)
				Projectile.timeLeft = 2;

			if (Timer > text.Length * 2f)
				Projectile.velocity.Y = -0.25f;

			if (follow && !Main.dedServ)
				Projectile.Center = Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 64);
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			return false;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			overWiresUI.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (text is null)
				return false;

			int end = Math.Min((int)Timer, text.Length);
			string toDraw = Helpers.Helper.WrapString(text[..end], 400, FontAssets.ItemStack.Value, Projectile.scale);

			float opacity = 1f;
			int full = text.Length * 3;

			if (Timer > full)
				opacity = 1 - (Timer - text.Length * 3f) / (text.Length * 2f);

			DynamicSpriteFont font = FontAssets.MouseText.Value;

			for (int k = 0; k < 4; k++)
			{
				Vector2 off = Vector2.One.RotatedBy(Main.GameUpdateCount * 0.08f + k / 4f * 6.28f) * 2f * Projectile.scale;
				Main.spriteBatch.DrawString(font, toDraw, Projectile.Center + off - Main.screenPosition, new Color(30, 170, 220) * 0.5f * opacity, 0, font.MeasureString(toDraw) / 2f, Projectile.scale, 0, 0);
			}

			Main.spriteBatch.DrawString(font, toDraw, Projectile.Center - Main.screenPosition, Color.White * opacity, 0, font.MeasureString(toDraw) / 2f, Projectile.scale, 0, 0);

			return false;
		}
	}
}