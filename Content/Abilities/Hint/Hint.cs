using Terraria.GameInput;

namespace StarlightRiver.Content.Abilities.Hint
{
	internal class HintAbility : CooldownAbility, IOrderedLoadable
	{
		public static int effectTimer;
		public static string hintToDisplay;

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

			Vector2 pos = Main.MouseScreen;

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
			Main.NewText(hintToDisplay);
			Deactivate();
		}
	}
}
