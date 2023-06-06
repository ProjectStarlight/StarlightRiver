using ReLogic.Graphics;
using System;
using Terraria.GameContent;
using Terraria.GameInput;

namespace StarlightRiver.Content.Abilities.Hint
{
	internal class HintAbility : CooldownAbility, IOrderedLoadable
	{
		public static int effectTimer;
		public static string hintToDisplay;

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
				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Abilities/HintCursor").Value;
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
			hintToDisplay = "Nothing interesting here...";

			Vector2 pos = Main.MouseWorld;

			for (int k = 0; k < Main.maxNPCs; k++)
			{
				if (Main.npc[k].Hitbox.Contains(pos.ToPoint()))
				{
					NPC npc = Main.npc[k];

					if (npc.ModNPC is IHintable hintable)
					{
						hintToDisplay = hintable.GetHint();
						return;
					}
				}
			}

			for (int k = 0; k < Main.maxProjectiles; k++)
			{
				if (Main.projectile[k].Hitbox.Contains(pos.ToPoint()))
				{
					Projectile proj = Main.projectile[k];

					if (proj.ModProjectile is IHintable hintable)
					{
						hintToDisplay = hintable.GetHint();
						return;
					}
				}
			}

			Tile tile = Framing.GetTileSafely((int)pos.X / 16, (int)pos.Y / 16);
			ModTile modTile = ModContent.GetModTile(tile.TileType);

			if (modTile is IHintable hintableT)
			{
				hintToDisplay = hintableT.GetHint();
				return;
			}
		}

		public override void UpdateActive()
		{
			int i = Projectile.NewProjectile(null, Main.MouseWorld + Vector2.UnitY * -32, Vector2.Zero, ModContent.ProjectileType<HintText>(), 0, 0, Main.myPlayer);
			var proj = Main.projectile[i].ModProjectile as HintText;

			if (proj != null)
				proj.text = hintToDisplay;

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
		}

		public override void AI()
		{
			Timer++;

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

		public override bool PreDraw(ref Color lightColor)
		{
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
