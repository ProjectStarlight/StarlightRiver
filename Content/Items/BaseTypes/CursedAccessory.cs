﻿using StarlightRiver.Content.Prefixes.Accessory.Cursed;
using StarlightRiver.Core.Loaders;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Utilities;

namespace StarlightRiver.Content.Items.BaseTypes
{
	public abstract class CursedAccessory : SmartAccessory
	{
		public static readonly Color CurseColor = new(25, 17, 49);

		private static float tooltipProgress;

		public Vector2 drawpos = Vector2.Zero;
		public bool GoingBoom = false;
		private int boomTimer = 0;

		protected CursedAccessory() : base("Unnamed Cursed Accessory", "You forgot to give this a display name dingus!") { }

		public override int ChoosePrefix(UnifiedRandom rand)
		{
			List<int> pool = CursedPrefixPool.GetCursedPrefixes();
			return pool[rand.Next(pool.Count)];
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			if (GoingBoom)
			{
				Texture2D tex = TextureAssets.Item[Item.type].Value;
				position += Vector2.One.RotatedByRandom(6.28f) * boomTimer / 60;

				spriteBatch.Draw(tex, position, frame, Color.White, 0, origin, scale, 0, 0);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);

				spriteBatch.Draw(tex, position, frame, Color.White * (boomTimer / 30f), 0, origin, scale, 0, 0);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);

				return false;
			}

			return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, ItemColor, origin, scale);
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			Vector2 pos = position - origin * scale + new Vector2(0, 10) + Main.rand.NextVector2Circular(16, 16) + frame.Size() / 2f * scale;

			if (!GoingBoom)
				CursedAccessoryParticleManager.CursedSystem.AddParticle(pos, new Vector2(0, -0.4f), 0, 0.5f, CurseColor, 60, Vector2.Zero);

			drawpos = pos;

			if (GoingBoom && boomTimer < 60)
			{
				float rot = Main.rand.NextFloat(6.28f);
				CursedAccessoryParticleManager.CursedSystem.AddParticle(pos + Vector2.One.RotatedBy(rot) * 30, -Vector2.One.RotatedBy(rot) * 1.5f, 0, 0.6f * boomTimer / 100f, Color.White, 20, Vector2.Zero);
			}
		}

		public override void OnEquip(Player player, Item item)
		{
			if (Main.playerInventory)
			{
				SoundEngine.PlaySound(SoundID.NPCHit55);
				SoundEngine.PlaySound(SoundID.Item123);

				for (int k = 0; k <= 50; k++)
					CursedAccessoryParticleManager.CursedSystem.AddParticle(drawpos, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.75f), 0, 1, CurseColor, 60, Vector2.Zero);
			}
		}

		public override void UpdateInventory(Player Player)
		{
			if (!(Main.HoverItem.ModItem is CursedAccessory))
				tooltipProgress = 0;
		}

		public virtual void SafeUpdateAccessory(Player Player, bool hideVisual) { }

		public sealed override void UpdateAccessory(Player Player, bool hideVisual)
		{
			SafeUpdateAccessory(Player, hideVisual);

			if (!(Main.HoverItem.ModItem is CursedAccessory))
				tooltipProgress = 0;

			if (GoingBoom)
				boomTimer++;

			if (boomTimer == 1)
				SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/MysticCast"));

			if (boomTimer >= 85)
			{
				Texture2D tex = TextureAssets.Item[Item.type].Value;

				Item.TurnToAir();
				SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/Shadow2"));

				for (int k = 0; k <= 70; k++)
				{
					float distance = Main.rand.NextFloat(4.25f);

					CursedAccessoryParticleManager.CursedSystem.AddParticle(drawpos, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4.25f, 4.75f), 1, 1.2f, CurseColor * 0.6f, 60, Vector2.Zero);
					CursedAccessoryParticleManager.CursedSystem.AddParticle(drawpos, Vector2.One.RotatedByRandom(6.28f) * distance, 1, 0.8f, Color.Lerp(new Color(200, 60, 250), CurseColor * 0.8f, distance / 4.25f), 60, Vector2.Zero);
				}

				CursedAccessoryParticleManager.ShardsSystem.SetTexture(tex);

				for (int x = 0; x < 5; x++)
				{
					for (int y = 0; y < 5; y++)
					{
						CursedAccessoryParticleManager.ShardsSystem.AddParticle(
								drawpos + new Vector2(x * tex.Width / 5f, y * tex.Height / 5f),
								Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(1, 2.55f),
								0,
								1.25f,
								Color.White,
								120,
								Vector2.Zero,
								new Rectangle(x * tex.Width / 5, y * tex.Height / 5, tex.Width / 5, tex.Height / 5)
								);
					}
				}
			}
		}

		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
		{
			if (line.Mod == "Terraria" && line.Name == "ItemName")
			{
				Effect effect = ShaderLoader.GetShader("CursedTooltip").Value;

				if (effect is null)
					return true;

				Texture2D tex = Assets.Masks.Glow.Value;

				effect.Parameters["speed"].SetValue(1);
				effect.Parameters["power"].SetValue(0.011f * tooltipProgress);
				effect.Parameters["uTime"].SetValue(Main.GameUpdateCount / 10f);

				int measure = (int)(line.Font.MeasureString(line.Text).X * 1.1f);
				int offset = (int)(Math.Sin(Main.GameUpdateCount / 25f) * 5);
				var target = new Rectangle(line.X + measure / 2, line.Y + 10, (int)(measure * 1.5f) + offset, 34 + offset);
				Main.spriteBatch.Draw(tex, target, new Rectangle(4, 4, tex.Width - 4, tex.Height - 4), Color.Black * (0.675f * tooltipProgress + (float)Math.Sin(Main.GameUpdateCount / 25f) * -0.1f), 0, (tex.Size() - Vector2.One * 8) / 2, 0, 0);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, effect, Main.UIScaleMatrix);

				Utils.DrawBorderString(Main.spriteBatch, line.Text, new Vector2(line.X, line.Y), Color.Lerp(Color.White, new Color(180, 100, 225), tooltipProgress), 1.1f);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

				if (tooltipProgress < 1)
					tooltipProgress += 0.05f;

				return false;
			}

			return base.PreDrawTooltipLine(line, ref yOffset);
		}

		public sealed override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			SafeModifyTooltips(tooltips);
			tooltips.Add(new(Mod, "CursedAccessoryLine", "{{Cursed}} item"));
		}

		public virtual void SafeModifyTooltips(List<TooltipLine> tooltips) { }
	}

	class CursedAccessoryParticleManager : ModSystem
	{
		public static ParticleSystem CursedSystem;
		public static ParticleSystem ShardsSystem;

		public override void Load()
		{
			CursedSystem = new ParticleSystem("StarlightRiver/Assets/Masks/GlowAlpha", UpdateCursedBody, ParticleSystem.AnchorOptions.UI);
			ShardsSystem = new ParticleSystem("StarlightRiver/Assets/GUI/charm", UpdateShardsBody, ParticleSystem.AnchorOptions.UI);
		}

		public override void PostUpdateEverything()
		{
			CursedSystem?.UpdateParticles();
			ShardsSystem?.UpdateParticles();
		}

		private static void UpdateShardsBody(Particle particle)
		{
			particle.Color = Color.White;
			particle.Rotation += particle.Velocity.X * 0.1f;
			particle.Position += particle.Velocity;
			particle.Velocity.Y += 0.2f;
			particle.Timer--;
		}

		private static void UpdateCursedBody(Particle particle)
		{
			if (particle.Rotation == 1)
				particle.Velocity *= 0.92f;

			particle.Color.A = 0;
			particle.Alpha = particle.Timer * 0.053f - 0.00088f * (float)Math.Pow(particle.Timer, 2);
			particle.Scale *= 0.97f;
			particle.Position += particle.Velocity;
			particle.Timer--;
		}
	}
}