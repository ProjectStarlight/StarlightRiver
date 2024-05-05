﻿using System;
using Terraria.GameContent;

namespace StarlightRiver.Core
{
	internal class ResourceReservationPlayer : ModPlayer
	{
		public int reservedLife;
		public int reservedMana;

		private int oldReservedLife; //so that checks will compare against old values aswell, prevents overdrafting your resources
		private int oldReservedMana;

		private int reservedLifeAnimation;
		private int reservedManaAnimation;

		public override void Load()
		{
			On_Main.GUIBarsDraw += DrawResourceOverlays;
		}

		public override void PostUpdate()
		{
			if (Player.statLife > Player.statLifeMax2 - reservedLifeAnimation)
				Player.statLife = Player.statLifeMax2 - reservedLifeAnimation;

			if (Player.statMana > Player.statManaMax2 - reservedManaAnimation)
				Player.statMana = Player.statManaMax2 - reservedManaAnimation;

			if (reservedLife > reservedLifeAnimation)
				reservedLifeAnimation++;

			if (reservedLife < reservedLifeAnimation)
				reservedLifeAnimation--;

			if (reservedMana > reservedManaAnimation)
				reservedManaAnimation++;

			if (reservedMana < reservedManaAnimation)
				reservedManaAnimation--;

			oldReservedLife = reservedLife;
			oldReservedMana = reservedMana;

			reservedLife = 0;
			reservedMana = 0;
		}

		public void ReserveLife(int amount)
		{
			reservedLife += amount;
		}

		public bool TryReserveLife(int amount)
		{
			if (reservedLife + amount >= Player.statLifeMax2 || oldReservedLife + amount >= Player.statLifeMax2) //cant have 0 health. That kills you.
				return false;

			return true;
		}

		public void ReserveMana(int amount)
		{
			reservedMana += amount;
		}

		public bool TryReserveMana(int amount)
		{
			if (reservedMana + amount > Player.statManaMax2 || oldReservedMana + amount > Player.statManaMax2) //you can have 0 mana... its just not an ideal situation
				return false;

			return true;
		}

		private void DrawResourceOverlays(On_Main.orig_GUIBarsDraw orig, Main self)
		{
			orig(self);

			DrawReservedLife();

			if (Main.ResourceSetsManager.ActiveSetKeyName == "Default")
				DrawReservedMana();

			if (Main.ResourceSetsManager.ActiveSetKeyName == "New")
				DrawReservedManaFancy();

			if (Main.ResourceSetsManager.ActiveSetKeyName == "NewWithText")
				DrawReservedManaFancy();

			if (Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBars")
				DrawReservedManaBars(48f);

			if (Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBarsWithText")
				DrawReservedManaBars(52.5f);

			if (Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBarsWithFullText")
				DrawReservedManaBars(50f);
		}

		private void DrawReservedLife()
		{
			Player player = Main.LocalPlayer;

			int vanillaHearts = Math.Min(20, player.statLifeMax / 20);
			float lifePerHeart = player.statLifeMax2 > vanillaHearts * 20 ? player.statLifeMax2 / (float)vanillaHearts : 20;
			int fullHeartsToDraw = Math.Min(vanillaHearts, (int)(player.GetModPlayer<ResourceReservationPlayer>().reservedLifeAnimation / lifePerHeart));

			for (int k = 0; k <= fullHeartsToDraw; k++)
			{
				Vector2 pos = Vector2.Zero;

				if (Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBars" || Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBarsWithText" || Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBarsWithFullText")
				{
					Texture2D texBar = Assets.GUI.ReservedBar.Value;
					float yOffset = 24f;

					if (Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBarsWithText")
						yOffset = 28f;
					else if (Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBarsWithFullText")
						yOffset = 26f;

					pos = new Vector2(Main.screenWidth - 60 - vanillaHearts * 12 + k * 12, yOffset);

					int width2 = 0;

					if (player.GetModPlayer<ResourceReservationPlayer>().reservedLifeAnimation >= (k + 1) * lifePerHeart)
						width2 = texBar.Width;
					else if (player.GetModPlayer<ResourceReservationPlayer>().reservedLifeAnimation > k * lifePerHeart)
						width2 = (int)(player.GetModPlayer<ResourceReservationPlayer>().reservedLifeAnimation % lifePerHeart / lifePerHeart * texBar.Width);

					var source2 = new Rectangle(0, 0, width2, texBar.Height);
					var target2 = new Rectangle((int)pos.X, (int)pos.Y, width2, texBar.Height);
					Main.spriteBatch.Draw(texBar, target2, source2, Color.White);

					if (k == fullHeartsToDraw && player.GetModPlayer<ResourceReservationPlayer>().reservedLifeAnimation > 0)
					{
						var targetLine = new Rectangle((int)pos.X + width2, (int)pos.Y, 2, texBar.Height);
						Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, targetLine, null, new Color(19, 16, 37));

						var targetLine2 = new Rectangle((int)pos.X + width2 - 2, (int)pos.Y, 2, texBar.Height);
						Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, targetLine2, null, new Color(119, 149, 139));
					}

					continue;
				}

				k += 20 - vanillaHearts;

				if (Main.ResourceSetsManager.ActiveSetKeyName == "Default")
				{
					pos = new Vector2(Main.screenWidth - 66 - k * 26, 58f);

					if (k >= 10)
						pos += new Vector2(260, -26);
				}
				else if (Main.ResourceSetsManager.ActiveSetKeyName == "New")
				{
					pos = new Vector2(Main.screenWidth - 76 - k * 24, 47f);

					if (k >= 10)
						pos += new Vector2(240, -28);
				}
				else if (Main.ResourceSetsManager.ActiveSetKeyName == "NewWithText")
				{
					pos = new Vector2(Main.screenWidth - 76 - k * 24, 52f);

					if (k >= 10)
						pos += new Vector2(240, -28);
				}

				k -= 20 - vanillaHearts;

				Texture2D tex = Assets.GUI.ReservedLife.Value;
				Texture2D texLine = Assets.GUI.ReservedLifeLine.Value;
				int width = 0;

				if (player.GetModPlayer<ResourceReservationPlayer>().reservedLifeAnimation >= (k + 1) * lifePerHeart)
					width = tex.Width;
				else if (player.GetModPlayer<ResourceReservationPlayer>().reservedLifeAnimation > k * lifePerHeart)
					width = (int)(player.GetModPlayer<ResourceReservationPlayer>().reservedLifeAnimation % lifePerHeart / lifePerHeart * tex.Width);

				if (width > 0 && k < 20)
				{
					var source = new Rectangle(22 - width, 0, width, tex.Height);
					var target = new Rectangle((int)pos.X + 22 - width, (int)pos.Y, width, tex.Height);
					var lineTarget = new Rectangle((int)pos.X + 24 - width - 2, (int)pos.Y, 2, tex.Height);
					var lineSource = new Rectangle(24 - width - 2, 0, 2, tex.Height);

					Main.spriteBatch.Draw(tex, target, source, Color.White);
					Main.spriteBatch.Draw(texLine, lineTarget, lineSource, Color.White);
				}
			}
		}

		private void DrawReservedMana()
		{
			Player player = Main.LocalPlayer;

			for (int i = 1; i < player.statManaMax2 / 20 + 1; i++) //iterate each mana star
			{
				int manaDrawn = i * 20; //the amount of mana drawn by this star and all before it

				float starHeight = MathHelper.Clamp((player.statMana - (i - 1) * 20) / 20f / 4f + 0.75f, 0.75f, 1); //height of the current star based on current mana

				if (player.statMana <= i * 20 && player.statMana >= (i - 1) * 20) //pulsing star for the "current" star
					starHeight += Main.cursorScale - 1;

				int reservedManaAmount = player.statManaMax2 - player.GetModPlayer<ResourceReservationPlayer>().reservedManaAnimation; //amount of mana to draw as rotten

				if (reservedManaAmount < manaDrawn)
				{
					if (manaDrawn - reservedManaAmount < 20)
					{
						Texture2D tex1 = Assets.GUI.ReservedMana.Value;
						var pos1 = new Vector2(Main.screenWidth - 25, 30 + TextureAssets.Mana.Height() / 2f + (TextureAssets.Mana.Height() - TextureAssets.Mana.Height() * starHeight) / 2f + 28 * (i - 1));

						int off = (int)(reservedManaAmount % 20 / 20f * tex1.Height);
						var source = new Rectangle(0, off, tex1.Width, tex1.Height - off);
						pos1.Y += off;

						Main.spriteBatch.Draw(tex1, pos1, source, Color.White, 0f, tex1.Size() / 2, 1, 0, 0);
						continue;
					}

					Texture2D tex = Assets.GUI.ReservedMana.Value;
					var pos = new Vector2(Main.screenWidth - 25, 30 + TextureAssets.Mana.Height() / 2f + (TextureAssets.Mana.Height() - TextureAssets.Mana.Height() * starHeight) / 2f + 28 * (i - 1));

					Main.spriteBatch.Draw(tex, pos, null, Color.White, 0f, tex.Size() / 2, 1, 0, 0);
				}
			}
		}

		private void DrawReservedManaFancy()
		{
			Player player = Main.LocalPlayer;

			for (int i = 1; i < player.statManaMax2 / 20 + 1; i++) //iterate each mana star
			{
				int manaDrawn = i * 20; //the amount of mana drawn by this star and all before it

				int reservedManaAmount = player.statManaMax2 - player.GetModPlayer<ResourceReservationPlayer>().reservedManaAnimation; //amount of mana to draw as rotten

				if (reservedManaAmount < manaDrawn)
				{
					if (manaDrawn - reservedManaAmount < 20)
					{
						Texture2D tex1 = Assets.GUI.ReservedMana.Value;
						var pos1 = new Vector2(Main.screenWidth - 25, 38 + 22 * (i - 1));

						int off = (int)(reservedManaAmount % 20 / 20f * tex1.Height);
						var source = new Rectangle(0, off, tex1.Width, tex1.Height - off);
						pos1.Y += off;

						Main.spriteBatch.Draw(tex1, pos1, source, Color.White, 0f, tex1.Size() / 2, 1, 0, 0);
						continue;
					}

					Texture2D tex = Assets.GUI.ReservedMana.Value;
					var pos = new Vector2(Main.screenWidth - 25, 38 + 22 * (i - 1));

					Main.spriteBatch.Draw(tex, pos, null, Color.White, 0f, tex.Size() / 2, 1, 0, 0);
				}
			}
		}

		private void DrawReservedManaBars(float yOffset)
		{
			Player player = Main.LocalPlayer;

			int vanillaStars = Math.Min(20, player.statManaMax2 / 20);
			int fullStarsToDraw = player.GetModPlayer<ResourceReservationPlayer>().reservedManaAnimation / 20;

			for (int k = 0; k <= fullStarsToDraw; k++)
			{
				Texture2D texBar = Assets.GUI.ReservedBar.Value;
				var pos = new Vector2(Main.screenWidth - 70 - vanillaStars * 12 + k * 12, yOffset);

				int width2 = 0;

				if (player.GetModPlayer<ResourceReservationPlayer>().reservedManaAnimation >= (k + 1) * 20)
					width2 = texBar.Width;
				else if (player.GetModPlayer<ResourceReservationPlayer>().reservedManaAnimation > k * 20)
					width2 = (int)(player.GetModPlayer<ResourceReservationPlayer>().reservedManaAnimation % 20 / 20f * texBar.Width);

				var source2 = new Rectangle(0, 0, width2, texBar.Height);
				var target2 = new Rectangle((int)pos.X, (int)pos.Y, width2, texBar.Height);
				Main.spriteBatch.Draw(texBar, target2, source2, Color.White);

				if (k == fullStarsToDraw && player.GetModPlayer<ResourceReservationPlayer>().reservedManaAnimation > 0)
				{
					var targetLine = new Rectangle((int)pos.X + width2, (int)pos.Y, 2, texBar.Height);
					Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, targetLine, null, new Color(19, 16, 37));

					var targetLine2 = new Rectangle((int)pos.X + width2 - 2, (int)pos.Y, 2, texBar.Height);
					Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, targetLine2, null, new Color(119, 149, 139));
				}
			}
		}
	}
}