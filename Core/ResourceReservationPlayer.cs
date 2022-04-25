using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using StarlightRiver.Content.Items.Gravedigger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace StarlightRiver.Core
{
	internal class ResourceReservationPlayer : ModPlayer
	{
		public int ReservedLife;
		public int ReservedMana;

		private int oldReservedLife; //so that checks will compare against old values aswell, prevents overdrafting your resources
		private int oldReservedMana;

		private int ReservedLifeAnimation;
		private int ReservedManaAnimation;

		public override void Load()
		{
			On.Terraria.Main.DrawInterface_25_ResourceBars += DrawResourceOverlays;
		}

		public override void PostUpdate()
		{
			if(Player.statLife > Player.statLifeMax2 - ReservedLifeAnimation)
				Player.statLife = Player.statLifeMax2 - ReservedLifeAnimation;

			if (Player.statMana > Player.statManaMax2 - ReservedManaAnimation)
				Player.statMana = Player.statManaMax2 - ReservedManaAnimation;

			if (ReservedLife > ReservedLifeAnimation)
				ReservedLifeAnimation++;

			if (ReservedLife < ReservedLifeAnimation)
				ReservedLifeAnimation--;

			if (ReservedMana > ReservedManaAnimation)
				ReservedManaAnimation++;

			if (ReservedMana < ReservedManaAnimation)
				ReservedManaAnimation--;

			oldReservedLife = ReservedLife;
			oldReservedMana = ReservedMana;

			ReservedLife = 0;
			ReservedMana = 0;
		}

		public void ReserveLife(int amount)
		{
			ReservedLife += amount;
		}

		public bool TryReserveLife(int amount)
		{
			if (ReservedLife + amount >= Player.statLifeMax2 || oldReservedLife + amount >= Player.statLifeMax2) //cant have 0 health. That kills you.
				return false;

			return true;
		}

		public void ReserveMana(int amount)
		{
			ReservedMana += amount;
		}

		public bool TryReserveMana(int amount)
		{
			if (ReservedMana + amount > Player.statManaMax2 || oldReservedMana + amount > Player.statManaMax2) //you can have 0 mana... its just not an ideal situation
				return false;

			return true;
		}

		private void DrawResourceOverlays(On.Terraria.Main.orig_DrawInterface_25_ResourceBars orig, Main self)
		{
			orig(self);

			DrawReservedLife();

			if (Main.ResourceSetsManager.ActiveSetKeyName == "Default")
				DrawReservedMana();

			if (Main.ResourceSetsManager.ActiveSetKeyName == "New")
				//DrawReservedManaFancy();
				return;
		}

		private void DrawReservedLife()
		{
			Player Player = Main.LocalPlayer;

			int vanillaHearts = Math.Min(20, Player.statLifeMax / 20);
			int fullHeartsToDraw = Math.Min(vanillaHearts, Player.GetModPlayer<ResourceReservationPlayer>().ReservedLifeAnimation / 20);
			float lifePerHeart = Player.GetModPlayer<ResourceReservationPlayer>().ReservedLife > vanillaHearts * 20 ? Player.GetModPlayer<ResourceReservationPlayer>().ReservedLife / (float)vanillaHearts : 20;

			for (int k = 0; k <= fullHeartsToDraw; k++)
			{
				Vector2 pos = Vector2.Zero;

				if (Main.ResourceSetsManager.ActiveSetKeyName == "Default")
				{
					pos = new Vector2(Main.screenWidth - 300 + k * 26, 32f);
					if (k >= 10)
						pos += new Vector2(-260, 26);
				}
				else if (Main.ResourceSetsManager.ActiveSetKeyName == "New")
				{
					pos = new Vector2(Main.screenWidth - 292 + k * 24, 19f);
					if (k >= 10)
						pos += new Vector2(-240, 28);
				}

				var tex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ReservedLife").Value;
				var texLine = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ReservedLifeLine").Value;
				int width = 0;

				if (Player.GetModPlayer<ResourceReservationPlayer>().ReservedLifeAnimation >= (k + 1) * lifePerHeart)
					width = tex.Width;
				else if (Player.GetModPlayer<ResourceReservationPlayer>().ReservedLifeAnimation > k * lifePerHeart)
					width = (int)((Player.GetModPlayer<ResourceReservationPlayer>().ReservedLifeAnimation % lifePerHeart) / lifePerHeart * tex.Width);

				if (width > 0 && k < 20)
				{
					var source = new Rectangle(0, 0, width, tex.Height);
					var target = new Rectangle((int)pos.X, (int)pos.Y, width, tex.Height);
					var lineTarget = new Rectangle((int)pos.X + width - 2, (int)pos.Y, 2, tex.Height);
					var lineSource = new Rectangle(width - 2, 0, 2, tex.Height);

					Main.spriteBatch.Draw(tex, target, source, Color.White * 0.25f);
					Main.spriteBatch.Draw(texLine, lineTarget, lineSource, Color.White);
				}
			}
		}

		private void DrawReservedMana()
		{
			Player Player = Main.LocalPlayer;

			for (int i = 1; i < Player.statManaMax2 / 20 + 1; i++) //iterate each mana star
			{
				int manaDrawn = i * 20; //the amount of mana drawn by this star and all before it

				float starHeight = MathHelper.Clamp(((Player.statMana - (i - 1) * 20) / 20f) / 4f + 0.75f, 0.75f, 1); //height of the current star based on current mana

				if (Player.statMana <= i * 20 && Player.statMana >= (i - 1) * 20) //pulsing star for the "current" star
					starHeight += Main.cursorScale - 1;

				var reservedManaAmount = Player.statManaMax2 - Player.GetModPlayer<ResourceReservationPlayer>().ReservedManaAnimation; //amount of mana to draw as rotten

				if (reservedManaAmount < manaDrawn)
				{
					if (manaDrawn - reservedManaAmount < 20)
					{
						var tex1 = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ReservedMana").Value;
						var pos1 = new Vector2(Main.screenWidth - 25, (30 + TextureAssets.Mana.Height() / 2f) + (TextureAssets.Mana.Height() - TextureAssets.Mana.Height() * starHeight) / 2f + (28 * (i - 1)));

						int off = (int)(reservedManaAmount % 20 / 20f * tex1.Height);
						var source = new Rectangle(0, off, tex1.Width, tex1.Height - off);
						pos1.Y += off;

						Main.spriteBatch.Draw(tex1, pos1, source, Color.White, 0f, tex1.Size() / 2, starHeight, 0, 0);
						continue;
					}

					var tex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ReservedMana").Value;
					var pos = new Vector2(Main.screenWidth - 25, (30 + TextureAssets.Mana.Height() / 2f) + (TextureAssets.Mana.Height() - TextureAssets.Mana.Height() * starHeight) / 2f + (28 * (i - 1)));

					Main.spriteBatch.Draw(tex, pos, null, Color.White, 0f, tex.Size() / 2, starHeight, 0, 0);
				}
			}
		}
	}
}
