﻿using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.IO;
using Terraria.UI;

namespace StarlightRiver.Content.CustomHooks
{
	class CharacterSelectAddons : HookGroup
	{
		public static ParticleSystem sparkles;

		//TODO: Create a reflection cache for this, or implement a global reflection caching utility
		private readonly FieldInfo _PlayerPanel = typeof(UICharacterListItem).GetField("_playerPanel", BindingFlags.NonPublic | BindingFlags.Instance);
		private readonly FieldInfo _Player = typeof(UICharacter).GetField("_player", BindingFlags.NonPublic | BindingFlags.Instance);
		private readonly FieldInfo Elements = typeof(UIElement).GetField("Elements", BindingFlags.NonPublic | BindingFlags.Instance);

		//Relies on other mods not doing anything visual here to work properly, but there is very little actual danger here, its just some extra draw calls in the UI element.
		public override void Load()
		{
			if (Main.dedServ)
				return;

			Terraria.GameContent.UI.Elements.On_UICharacterListItem.DrawSelf += DrawSpecialCharacter;
			Terraria.GameContent.UI.Elements.On_UICharacterListItem.ctor += ShiftTextOver;

			sparkles = new ParticleSystem(AssetDirectory.GUI + "Sparkle", updateSparkles);
		}

		public override void Unload()
		{
			sparkles ??= null;
		}

		private static void updateSparkles(Particle particle)
		{
			particle.Timer--;

			particle.Scale = (float)Math.Sin(particle.Timer / 60f * 3.14f) * particle.StoredPosition.X;

			particle.Color = new Color(255, 230, (byte)(Math.Sin(particle.Timer / 60f * 3.14f) * 200)) * (float)Math.Sin(particle.Timer / 60f * 3.14f);
			particle.Position += particle.Velocity;

			particle.Rotation += 0.05f;
		}

		private void ShiftTextOver(Terraria.GameContent.UI.Elements.On_UICharacterListItem.orig_ctor orig, UICharacterListItem self, PlayerFileData data, int snapPointIndex)
		{
			orig(self, data, snapPointIndex);

			FieldInfo info = typeof(UICharacterListItem).GetField("_buttonLabel", BindingFlags.Instance | BindingFlags.NonPublic);
			var text = info.GetValue(self) as UIText;

			text.Left.Set(190, 0);

			var character = (UICharacter)_PlayerPanel.GetValue(self);
			var Player = (Player)_Player.GetValue(character);
			MedalPlayer mp3 = Player.GetModPlayer<MedalPlayer>();

			if (mp3.Player == Player && mp3.medals.Count > 0) //expand only if medals are earned
			{
				self.Height.Set(152, 0);

				var elements = (List<UIElement>)Elements.GetValue(self);
				foreach (UIElement e in elements.Where(n => n.VAlign == 1))
					e.Top.Set(-56, 0);
			}
		}

		private void DrawSpecialCharacter(Terraria.GameContent.UI.Elements.On_UICharacterListItem.orig_DrawSelf orig, UICharacterListItem self, SpriteBatch spriteBatch)
		{
			orig(self, spriteBatch);
			var origin = new Vector2(self.GetDimensions().X, self.GetDimensions().Y);

			//hooray double reflection
			var character = (UICharacter)_PlayerPanel.GetValue(self);

			var Player = (Player)_Player.GetValue(character);
			AbilityHandler mp = Player.GetHandler();
			MedalPlayer mp3 = Player.GetModPlayer<MedalPlayer>();

			if (mp == null || mp3 == null)
				return;

			for (int k = 0; k < Player.armor.Length; k++)
			{
				if (Player.armor[k].ModItem != null && Player.armor[k].ModItem.Mod.Name == StarlightRiver.Instance.Name)
					Player.GrantPrefixBenefits(Player.armor[k]);
			}

			float PlayerStamina = mp.StaminaMaxDefault;

			var box = new Rectangle((int)(origin + new Vector2(110, 66)).X, (int)(origin + new Vector2(86, 66)).Y, 80, 25);
			var box2 = new Rectangle((int)(origin + new Vector2(196, 66)).X, (int)(origin + new Vector2(86, 66)).Y, 104, 25);

			spriteBatch.Draw(ModContent.Request<Texture2D>("StarlightRiver/Assets/GUI/box").Value, box, Color.White); //Stamina box

			if (mp.AnyUnlocked)//Draw stamina if any unlocked
			{
				spriteBatch.Draw(ModContent.Request<Texture2D>("StarlightRiver/Assets/GUI/Stamina").Value, origin + new Vector2(115, 68), Color.White);
				Utils.DrawBorderString(spriteBatch, PlayerStamina + " SP", origin + new Vector2(142, 68), Color.White);
			}
			else//Myserious if locked
			{
				spriteBatch.Draw(ModContent.Request<Texture2D>("StarlightRiver/Assets/GUI/Stamina3").Value, origin + new Vector2(115, 68), Color.White);
				Utils.DrawBorderString(spriteBatch, "???", origin + new Vector2(142, 68), Color.White);
			}

			Ability[] abilities = Ability.GetAbilityInstances();

			//Draw ability Icons
			for (int k = 0; k < abilities.Length; k++)
			{
				Ability ability = abilities[abilities.Length - 1 - k];
				string texture = Player.GetHandler().Unlocked(ability.GetType())
					? ability.PreviewTexture
					: ability.PreviewTextureOff;
				spriteBatch.Draw(ModContent.Request<Texture2D>(texture).Value, origin + new Vector2(542 - k * 32, 64), Color.White);
			}

			if (Player.statLifeMax > 400) //why vanilla dosent do this I dont know
			{
				spriteBatch.Draw(TextureAssets.Heart2.Value, origin + new Vector2(80, 37), Color.White);
			}

			if (Player.GetModPlayer<BarrierPlayer>().maxBarrier > 0)
			{
				Texture2D barrierTex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ShieldHeartOver").Value;
				Texture2D barrierTex2 = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ShieldHeart").Value;
				Texture2D barrierTex3 = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ShieldHeartLine").Value;

				Vector2 pos = origin + new Vector2(80, 37);
				int width = barrierTex.Width / 2;
				var source = new Rectangle(0, 0, width, barrierTex.Height);
				var target = new Rectangle((int)pos.X, (int)pos.Y, width, barrierTex.Height);
				var lineTarget = new Rectangle((int)pos.X + width - 2, (int)pos.Y, 2, barrierTex.Height);
				var lineSource = new Rectangle(width - 2, 0, 2, barrierTex.Height);

				spriteBatch.Draw(barrierTex2, target, source, Color.White * 0.25f);
				spriteBatch.Draw(barrierTex, target, source, Color.White);
				spriteBatch.Draw(barrierTex3, lineTarget, lineSource, Color.White);
			}

			if (mp3.medals.Count > 0) //draw medals if any are earned
			{
				var boxMedals = new Rectangle((int)(origin.X + 10), (int)(origin.Y + 96), (int)self.GetDimensions().Width - 20, 50);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, boxMedals, new Color(43, 56, 101)); //Medal box

				for (int k = 0; k < mp3.medals.Count; k++) //Draw all medals
				{
					Medal medal = mp3.medals[k];
					Texture2D tex = mp3.GetMedalTexture(medal.name);
					var frame = new Rectangle(36 * (medal.difficulty + 1), 0, 34, 46);
					Vector2 pos = origin + new Vector2(14 + k * 42, 98);

					spriteBatch.Draw(tex, pos, frame, Color.White);

					if (medal.difficulty == 1 && Main.rand.NextBool(10))
						sparkles.AddParticle(new Particle(pos + new Vector2(Main.rand.Next(34), 10 + Main.rand.Next(36)), Vector2.UnitY * Main.rand.NextFloat(0.2f), 0, 0, new Color(255, 230, 0), 90, new Vector2(Main.rand.NextFloat(0.4f, 0.7f), 0), new Rectangle(0, 0, 15, 15)));

					if (medal.difficulty == 2 && Main.rand.NextBool(8))
						sparkles.AddParticle(new Particle(pos + new Vector2(Main.rand.Next(34), 10 + Main.rand.Next(36)), Vector2.UnitY * Main.rand.NextFloat(0.2f), 0, 0, new Color(255, 50, 50), 90, new Vector2(Main.rand.NextFloat(0.4f, 0.7f), 0), new Rectangle(0, 0, 15, 15)));

					var rectangle = new Rectangle((int)origin.X + 14 + k * 42, (int)origin.Y + 98, 34, 46);

					if (rectangle.Contains(Main.MouseScreen.ToPoint()))
					{
						string message = medal.ToString();

						if (medal.difficulty == 2 && Main.time % 4800 > 2400)
							message = "Deaths: " + mp3.GetDeaths(medal.name);

						Utils.DrawBorderString(spriteBatch, message, origin + new Vector2(306, 70), Color.White);
					}
				}
			}

			if ( //Player is "complete"
				Player.statLifeMax == 500 &&
				Player.statManaMax == 200 &&
				PlayerStamina == 5 &&
				!abilities.Any(n => !Player.GetHandler().Unlocked(n.GetType()))
				)
			{
				Texture2D borderTex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "GoldBorder").Value;
				spriteBatch.Draw(borderTex, origin, Color.White);

				if (Main.rand.NextBool(3))
				{
					if (Main.rand.Next(4) > 0)
						sparkles.AddParticle(new Particle(origin + new Vector2(Main.rand.Next(borderTex.Width), Main.rand.NextBool() ? 2 : borderTex.Height - 2), Vector2.Zero, 0, 0, new Color(255, 230, 0), 60, new Vector2(Main.rand.NextFloat(0.4f, 0.7f), 0), new Rectangle(0, 0, 15, 15)));
					else
						sparkles.AddParticle(new Particle(origin + new Vector2(Main.rand.NextBool() ? 2 : borderTex.Width - 2, Main.rand.Next(borderTex.Height)), Vector2.Zero, 0, 0, new Color(255, 230, 0), 60, new Vector2(Main.rand.NextFloat(0.4f, 0.7f), 0), new Rectangle(0, 0, 15, 15)));
				}
			}

			sparkles.DrawParticles(spriteBatch);

			Player.ResetEffects();
		}
	}
}