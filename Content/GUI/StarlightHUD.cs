using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Loaders.UILoading;
using System;
using System.Collections.Generic;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class StarlightHUD : SmartUIState
	{
		/// <summary>
		/// The stamina bar
		/// </summary>
		private readonly StarlightBar starlightBar = new();

		/// <summary>
		/// The timer controlling the gain animation
		/// </summary>
		public static int gainAnimationTimer = 0;

		/// <summary>
		/// Exposes the position that shards should go to for their animation
		/// </summary>
		public static Vector2 shardTarget;

		// Fields related to the shard animation
		public static Vector2 shardStartPos;
		public static int shardTimer;
		public const int SHARD_TIMER_MAX = 120;
		public readonly ParticleSystem shardParticles = new("StarlightRiver/Assets/StarTexture", UpdateShardParticles, ParticleSystem.AnchorOptions.UI);

		/// <summary>
		/// If the gain animation is currently playing
		/// </summary>
		public static bool InGainAnimation => gainAnimationTimer > 0;

		public override bool Visible => Main.LocalPlayer.GetHandler().StaminaMax > 0;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			AddElement(starlightBar, -303, 1f, 110, 0f, 30, 0f, 0, 0f);
		}

		public static void StartShardAnimation(Vector2 start)
		{
			shardStartPos = start;
			shardTimer = SHARD_TIMER_MAX;
		}

		public void DrawShardAnimation(SpriteBatch spriteBatch)
		{
			shardTimer--;

			Player Player = Main.LocalPlayer;
			AbilityHandler mp = Player.GetHandler();

			Texture2D tex;

			if (mp.ShardCount % 3 == 1)
				tex = Assets.Abilities.Stamina1.Value;
			else if (mp.ShardCount % 3 == 2)
				tex = Assets.Abilities.Stamina2.Value;
			else
				tex = Assets.Abilities.Stamina3.Value;

			float prog = Eases.EaseQuadInOut(1f - shardTimer / (float)SHARD_TIMER_MAX);
			float scale = 1f + MathF.Sin(prog * 3.14f) * 0.5f;

			var pos = Vector2.Lerp(shardStartPos, shardTarget + tex.Size() / 2f, prog);
			pos.Y += MathF.Sin(prog * 3.14f) * Main.screenHeight / 6f;

			// Stars
			Texture2D star = Assets.StarTexture.Value;
			float sin = (float)Math.Sin(Main.GameUpdateCount * 0.05f);
			float sin2 = (float)Math.Sin(Main.GameUpdateCount * 0.05f + 2f);

			Vector2 drawPos = pos;

			float op = 1f - prog;

			Main.spriteBatch.Draw(star, drawPos, null, new Color(190, 255, 255, 0) * op, 0, star.Size() / 2f, 0.2f + sin * 0.05f, 0, 0);
			Main.spriteBatch.Draw(star, drawPos, null, new Color(190, 255, 255, 0) * op, 1.57f / 2f, star.Size() / 2f, 0.1f + sin2 * 0.05f, 0, 0);

			Main.spriteBatch.Draw(star, drawPos, null, new Color(0, 230, 255, 0) * op, 0, star.Size() / 2f, 0.25f + sin * 0.05f, 0, 0);
			Main.spriteBatch.Draw(star, drawPos, null, new Color(0, 160, 255, 0) * op, 1.57f / 2f, star.Size() / 2f, 0.2f + sin2 * 0.05f, 0, 0);

			Main.spriteBatch.Draw(star, drawPos, null, new Color(0, 10, 60, 0) * op, 0, star.Size() / 2f, 0.3f + sin * 0.05f, 0, 0);
			Main.spriteBatch.Draw(star, drawPos, null, new Color(0, 0, 60, 0) * op, 1.57f / 2f, star.Size() / 2f, 0.25f + sin2 * 0.05f, 0, 0);

			// Main tex
			spriteBatch.Draw(tex, pos, null, Color.White, 0, tex.Size() / 2f, scale, 0, 0);

			// Over glow
			Texture2D glow = Assets.Masks.GlowAlpha.Value;
			Color glowColor = new Color(0, 90, 120, 0) * op;
			Main.spriteBatch.Draw(glow, pos, glow.Frame(), glowColor * 0.3f, 0, glow.Size() / 2, 1, 0, 0);
			Main.spriteBatch.Draw(glow, pos, glow.Frame(), glowColor * 0.5f, 0, glow.Size() / 2, 0.6f, 0, 0);

			shardParticles.AddParticle(
				position: pos + new Vector2(Main.rand.Next(34), Main.rand.Next(34)) - Vector2.One * 20,
				velocity: Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.5f),
				rotation: 0,
				scale: Main.rand.NextFloat(0.1f),
				color: new Color(50, 100 + Main.rand.Next(100), 255, 0),
				timer: 45,
				storedPosition: default
			);

			if (shardTimer == 0)
			{
				SoundHelper.PlayPitched("Magic/HolyCastShort", 1, -0.5f);
				SoundHelper.PlayPitched("Impacts/StoneStrikeLight", 0.3f, -0.2f);
				SoundHelper.PlayPitched("Effects/Loot", 1f, -0.3f);

				shardParticles.AddParticle(
					position: pos,
					velocity: Vector2.Zero,
					rotation: 0,
					scale: 0.01f,
					color: new Color(150, 255, 255, 0),
					timer: 120,
					storedPosition: default
				);

				shardParticles.AddParticle(
					position: pos,
					velocity: Vector2.Zero,
					rotation: 1.57f / 2f,
					scale: 0.01f,
					color: new Color(100, 200, 255, 0),
					timer: 120,
					storedPosition: default
				);

				for (int k = 0; k < 20; k++)
				{
					shardParticles.AddParticle(
						position: pos + new Vector2(Main.rand.Next(-17, 17), Main.rand.Next(-17, 17)),
						velocity: Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.3f, 0.5f),
						rotation: 0,
						scale: Main.rand.NextFloat(0.1f, 0.2f),
						color: new Color(50, 100 + Main.rand.Next(100), 255, 0),
						timer: 90,
						storedPosition: default
					);
				}

				if (mp.ShardCount % 3 == 0)
					TextCard.Display(
						Language.GetTextValue("Mods.StarlightRiver.GUI.StarlightHUD.IncreaseMessage"),
						Language.GetTextValue("Mods.StarlightRiver.GUI.StarlightHUD.IncreaseTooltip"),
						240, 1f);
			}
		}

		private static void UpdateShardParticles(Particle particle)
		{
			particle.Timer--;

			if (particle.Timer > 110)
				particle.Scale += 0.1f * ((particle.Timer - 110) / 10f);

			if (particle.Timer < 110)
			{
				particle.Scale *= 0.97f;
				particle.Color *= 0.97f;
			}

			particle.Position += particle.Velocity;
		}

		/// <summary>
		/// We have additional rendering logic for an informative string on mouse-over here
		/// </summary>
		/// <param name="spriteBatch"></param>
		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			if (shardTimer > 0)
				DrawShardAnimation(spriteBatch);

			shardParticles.DrawParticles(spriteBatch);
			shardParticles.UpdateParticles();

			// We render some mouse-over text to display the numerical stamina amount if the mouse is hovered over the bar
			if (starlightBar.IsMouseHovering)
			{
				AbilityHandler mp = Main.LocalPlayer.GetHandler();
				double stamina = Math.Round(mp.Stamina, 1);
				double staminaMax = Math.Round(mp.StaminaMax, 1);
				string text = Language.GetText("Mods.StarlightRiver.GUI.StarlightHUD.Hover").Format(stamina, staminaMax);
				Vector2 pos = Main.MouseScreen + Vector2.One * 16;
				pos.X = Math.Min(Main.screenWidth - Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(text).X - 6, pos.X);
				Utils.DrawBorderString(spriteBatch, text, pos, Main.MouseTextColorReal);
			}

			Recalculate();
		}

		/// <summary>
		/// We handle updating the animation timer and re-positioning the bar here
		/// </summary>
		/// <param name="gameTime"></param>
		public override void SafeUpdate(GameTime gameTime)
		{
			Player Player = Main.LocalPlayer;
			AbilityHandler mp = Player.GetHandler();

			// Here we handle the timer for the gain animation if applicable
			if (gainAnimationTimer > 0)
				gainAnimationTimer--;

			// Here we handle adjusting the position of the bar based on various events that can move the vanilla GUI
			if (Main.ResourceSetsManager.ActiveSetKeyName.Contains("HorizontalBars")) // logic for if we are on a bar style
			{
				int width = 36 + 12 * (int)mp.StaminaMax;
				starlightBar.Left.Set(-306 + 258 - width, 1);
				starlightBar.Top.Set(110 - 44, 0);
				starlightBar.Height.Set(22, 0);
				starlightBar.Width.Set(width, 0);
			}
			else // logic for if we are on an icon style
			{
				if (Main.mapStyle != 1) // If the minimap is disabled
				{
					if (Main.playerInventory) // We need to shift over if the inventory is open due to the equipment panel on the right side
					{
						starlightBar.Left.Set(-220, 1);
						starlightBar.Top.Set(90, 0);
					}
					else
					{
						starlightBar.Left.Set(-70, 1);
						starlightBar.Top.Set(90, 0);
					}
				}
				else // If the minimap is enabled
				{
					starlightBar.Left.Set(-306, 1);
					starlightBar.Top.Set(110, 0);
				}

				// We set the height according to the amount of icons displayed here. Since it loops horizontally after 7, we cap it there
				float height = 30 * mp.StaminaMax;
				if (height > 30 * 7)
					height = 30 * 7;

				starlightBar.Height.Set(height, 0f);
			}
		}
	}

	/// <summary>
	/// This element acts as the actual stamina bar, for ease of re-positioning it as it can shift around alot according to other UI settings
	/// </summary>
	internal class StarlightBar : SmartUIElement
	{
		/// <summary>
		/// Allows overriding the fill texture of stamina with icon styles. Reset every frame.
		/// </summary>
		public static Texture2D overrideTexture = null;

		/// <summary>
		/// Allows overriding the container texture of specific containers with icon styles. Reset every frame.
		/// </summary>
		public static List<string> specialVesselTextures = [];

		/// <summary>
		/// Particle system used for the gain animation
		/// </summary>
		private readonly ParticleSystem gainAnimationParticles = new("StarlightRiver/Assets/GUI/Sparkle", UpdateGainParticles, ParticleSystem.AnchorOptions.UI);

		public readonly ParticleSystem sparkleParticles = new("StarlightRiver/Assets/GUI/Sparkle", UpdateSparkleParticles, ParticleSystem.AnchorOptions.UI);

		/// <summary>
		/// Draws this stamina bar as either the gain animation or standard depending on the animation state of the Stamina UIState
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw with</param>
		public override void DrawSelf(SpriteBatch spriteBatch)
		{
			var dimensions = GetDimensions().ToRectangle();

			if (StarlightHUD.InGainAnimation) // If we are in the gain animation, draw that
			{
				DrawGainAnimation(spriteBatch, dimensions, 240 - StarlightHUD.gainAnimationTimer);
			}
			else // Otherwise, proceed with standard drawing logic
			{
				if (Main.ResourceSetsManager.ActiveSetKeyName.Contains("HorizontalBars")) //logic for the horizontal bars UI style
					DrawBars(spriteBatch, dimensions);
				else //logic for icon UI
					DrawIcons(spriteBatch, dimensions);
			}

			// Draw particles
			sparkleParticles.UpdateParticles();
			sparkleParticles.DrawParticles(spriteBatch);
		}

		/// <summary>
		/// Draws the stamina GUI as a horizontal bar
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw with</param>
		/// <param name="dimensions">The dimensions of this UIElement</param>
		void DrawBars(SpriteBatch spriteBatch, Rectangle dimensions)
		{
			Player Player = Main.LocalPlayer;
			AbilityHandler mp = Player.GetHandler();

			Texture2D ornament = Assets.GUI.StaminaBarOrnament.Value;
			Texture2D empty = Assets.GUI.StaminaBarEmpty.Value;
			Texture2D fill = Assets.GUI.StaminaBarFill.Value;
			Texture2D edge = Assets.GUI.StaminaBarEdge.Value;

			Vector2 pos = dimensions.TopRight() + new Vector2(-56, 0);

			pos.X += 12;

			for (int k = 0; k < mp.StaminaMax; k++)
			{
				spriteBatch.Draw(empty, pos, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

				if (mp.Stamina - 1 >= k)
				{
					spriteBatch.Draw(fill, pos + new Vector2(0, 6), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
				}
				else if (mp.Stamina > k)
				{
					int width = (int)(fill.Width * (mp.Stamina % 1f));
					var target = new Rectangle((int)pos.X + (fill.Width - width), (int)pos.Y + 6, width, fill.Height);
					spriteBatch.Draw(fill, target, new Rectangle(fill.Width - width, 0, width, fill.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
				}

				pos.X -= 12;
			}

			pos.X += 6;
			spriteBatch.Draw(edge, pos, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

			pos = dimensions.TopRight() + new Vector2(-56, 0);
			spriteBatch.Draw(ornament, pos + new Vector2(0, -2), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

			// Draw shards
			int effectiveShardCount = StarlightHUD.shardTimer > 0 ? mp.ShardCount - 1 : mp.ShardCount;
			Texture2D shard1 = Assets.Abilities.Stamina1.Value;
			Texture2D shard2 = Assets.Abilities.Stamina2.Value;

			pos = dimensions.TopRight();
			StarlightHUD.shardTarget = pos;

			if (effectiveShardCount % 3 >= 1)
				spriteBatch.Draw(shard1, pos, shard1.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

			if (effectiveShardCount % 3 >= 2)
				spriteBatch.Draw(shard2, pos, shard2.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
		}

		/// <summary>
		/// Draws the stamina GUI as icons on the side of the map (or mana if map is off)
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw with</param>
		/// <param name="dimensions">The dimensions of this UIElement</param>
		void DrawIcons(SpriteBatch spriteBatch, Rectangle dimensions)
		{
			Player Player = Main.LocalPlayer;
			AbilityHandler mp = Player.GetHandler();

			//logic for other UI styles
			Texture2D emptyTex = Assets.GUI.StaminaEmpty.Value;
			Texture2D fillTex = overrideTexture is null ? Assets.GUI.Stamina.Value : overrideTexture;

			int row = 0;
			for (int k = 0; k <= mp.StaminaMax; k++)
			{
				if (k % 7 == 0 && k != 0)
					row++;

				Vector2 pos = row % 2 == 0 ? dimensions.TopLeft() + new Vector2(row * -18, k % 7 * 24) :
					dimensions.TopLeft() + new Vector2(row * -18, 12 + k % 7 * 24);

				int effectiveShardCount = StarlightHUD.shardTimer > 0 ? mp.ShardCount - 1 : mp.ShardCount;
				float effectiveStaminaMax = StarlightHUD.shardTimer > 0 ? (mp.ShardCount % 3 == 0 ? mp.StaminaMax - 1 : mp.StaminaMax) : mp.StaminaMax;

				if (k >= effectiveStaminaMax) //draws the incomplete vessel
				{
					Texture2D shard1 = Assets.Abilities.Stamina1.Value;
					Texture2D shard2 = Assets.Abilities.Stamina2.Value;

					StarlightHUD.shardTarget = pos;

					if (effectiveShardCount % 3 >= 1)
						spriteBatch.Draw(shard1, pos, shard1.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

					if (effectiveShardCount % 3 >= 2)
						spriteBatch.Draw(shard2, pos, shard2.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

					break;
				}

				Texture2D slotTex = emptyTex;

				// Changes slot textures if needed
				if (k >= mp.StaminaMax - specialVesselTextures.Count)
					slotTex = Request<Texture2D>(specialVesselTextures[(int)mp.StaminaMax - k - 1]).Value;

				spriteBatch.Draw(slotTex, pos, slotTex.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

				if (k < mp.Stamina - 1) // If on a filled stamina vessel
				{
					spriteBatch.Draw(fillTex, pos + Vector2.One * 4, Color.White);
				}
				else if (k <= mp.Stamina) // If on the last stamina vessel
				{
					float scale = mp.Stamina - k;
					spriteBatch.Draw(fillTex, pos + Vector2.One * 4 + fillTex.Size() / 2, fillTex.Frame(), Color.White, 0, fillTex.Size() / 2, scale, 0, 0);
				}
			}

			overrideTexture = null;
			specialVesselTextures.Clear();
		}

		/// <summary>
		/// Handles drawing the animation for gaining the ability to use stamina
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw with</param>
		/// <param name="dimensions">The dimensions of this UIElement</param>
		/// <param name="timer">The timer associated with the animation</param>
		private void DrawGainAnimation(SpriteBatch spriteBatch, Rectangle dimensions, int timer)
		{
			if (Main.ResourceSetsManager.ActiveSetKeyName.Contains("HorizontalBars"))
				DrawGainAnimationBar(spriteBatch, dimensions, timer);
			else
				DrawGainAnimationIcon(spriteBatch, dimensions, timer);

			gainAnimationParticles.UpdateParticles();
			gainAnimationParticles.DrawParticles(spriteBatch);
		}

		/// <summary>
		/// Animation for gain for bar styles
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw with</param>
		/// <param name="dimensions">The dimensions of this UIElement</param>
		/// <param name="timer">The timer associated with the animation</param>
		private void DrawGainAnimationBar(SpriteBatch spriteBatch, Rectangle dimensions, int timer)
		{
			Texture2D star = Assets.StarTexture.Value;
			Vector2 pos = dimensions.TopRight() + new Vector2(-18, 10);

			// Spawn particles
			if (timer < 64)
			{
				float prog = timer / 80f;

				if (Main.rand.NextFloat() < prog)
				{
					float rot = Main.rand.NextFloat(6.28f);

					gainAnimationParticles.AddParticle(
								position: pos + Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(80, 150) - Vector2.One * 20,
								velocity: Vector2.One.RotatedBy(rot + 1.57f) * 3.5f,
								rotation: 0,
								scale: Main.rand.NextFloat(0.5f, 1f),
								color: new Color(150, 220, 250),
								timer: 80,
								storedPosition: pos,
								alpha: 0
						);
				}
			}

			// Fade in
			if (timer > 40 && timer < 120)
			{
				float prog = (timer - 40) / 80f;
				float opacity = Helpers.Eases.BezierEase(prog);

				DrawGlowingBar(spriteBatch, dimensions, new Color(50, 120, 255) * opacity);

				Color starColor = new Color(150, 220, 255) * Helpers.Eases.SwoopEase(prog);
				starColor.A = 0;

				spriteBatch.Draw(star, pos, star.Frame(), starColor, 0, star.Size() / 2f, Helpers.Eases.SwoopEase(prog) * 0.5f, SpriteEffects.None, 0);
			}

			// Hold
			if (timer >= 120 && timer <= 140)
			{
				float colorProg = Helpers.Eases.BezierEase((timer - 120) / 20f);
				var color = Color.Lerp(new Color(50, 120, 255), Color.White, colorProg);

				DrawGlowingBar(spriteBatch, dimensions, color);

				Color starColor = Color.Lerp(new Color(150, 220, 255), Color.White, colorProg) * (1 - colorProg);
				starColor.A = 0;

				spriteBatch.Draw(star, pos, star.Frame(), starColor, 0, star.Size() / 2f, (1 + colorProg) * 0.5f, SpriteEffects.None, 0);
			}

			// Fade out with slot underneath
			if (timer > 140 && timer < 170)
			{
				float prog = (timer - 140) / 30f;
				float opacity = 1 - Helpers.Eases.BezierEase(prog);

				DrawBars(spriteBatch, dimensions);
				DrawGlowingBar(spriteBatch, dimensions, Color.White * opacity);
			}

			if (timer >= 90 && Main.rand.NextBool(9))
			{
				sparkleParticles.AddParticle(
						position: pos + new Vector2(Main.rand.Next(34), Main.rand.Next(34)) - Vector2.One * 20,
						velocity: Vector2.UnitY * Main.rand.NextFloat(0.2f),
						rotation: 0,
						scale: 0,
						color: new Color(255, 50, 50),
						timer: 90,
						storedPosition: new Vector2(Main.rand.NextFloat(0.4f, 0.7f), 0),
						frame: new Rectangle(0, 0, 15, 15)
					);
			}

			if (timer >= 170)
			{
				DrawBars(spriteBatch, dimensions);
			}

			if (timer == 239)
				gainAnimationParticles.ClearParticles();
		}

		/// <summary>
		/// Helper to draw the glowing stamina bar for the bar gain animation
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="dimensions"></param>
		/// <param name="color"></param>
		private void DrawGlowingBar(SpriteBatch spriteBatch, Rectangle dimensions, Color color)
		{
			Player Player = Main.LocalPlayer;
			AbilityHandler mp = Player.GetHandler();

			Texture2D ornament = Assets.GUI.StaminaBarOrnamentGlow.Value;
			Texture2D empty = Assets.GUI.StaminaBarGlow.Value;
			Texture2D edge = Assets.GUI.StaminaBarEdgeGlow.Value;

			Vector2 pos = dimensions.TopRight() + new Vector2(-56, 0);

			pos.X += 12;

			for (int k = 0; k < mp.StaminaMax; k++)
			{
				spriteBatch.Draw(empty, pos, null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
				pos.X -= 12;
			}

			pos.X += 6;
			spriteBatch.Draw(edge, pos, null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

			pos = dimensions.TopRight() + new Vector2(-56, 0);
			spriteBatch.Draw(ornament, pos + new Vector2(0, -2), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
		}

		/// <summary>
		/// Animation for gain for icon styles
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw with</param>
		/// <param name="dimensions">The dimensions of this UIElement</param>
		/// <param name="timer">The timer associated with the animation</param>
		private void DrawGainAnimationIcon(SpriteBatch spriteBatch, Rectangle dimensions, int timer)
		{
			Texture2D slotTex = Assets.GUI.StaminaEmpty.Value;
			Texture2D slotTexGlow = Assets.GUI.StaminaGlowNormal.Value;
			Texture2D star = Assets.StarTexture.Value;
			Vector2 pos = dimensions.TopLeft();

			// Spawn particles
			if (timer < 64)
			{
				float prog = timer / 80f;

				if (Main.rand.NextFloat() < prog)
				{
					float rot = Main.rand.NextFloat(6.28f);

					gainAnimationParticles.AddParticle(
								position: pos + Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(80, 150),
								velocity: Vector2.One.RotatedBy(rot + 1.57f) * 3.5f,
								rotation: 0,
								scale: Main.rand.NextFloat(0.5f, 1f),
								color: new Color(150, 220, 250),
								timer: 80,
								storedPosition: pos + Vector2.One * 12,
								alpha: 0
						);
				}
			}

			// Fade in
			if (timer > 40 && timer < 120)
			{
				float prog = (timer - 40) / 80f;
				float opacity = Helpers.Eases.BezierEase(prog);

				spriteBatch.Draw(slotTexGlow, pos + slotTexGlow.Size() / 2f, slotTexGlow.Frame(), new Color(50, 120, 255) * opacity, 0, slotTexGlow.Size() / 2f, opacity, SpriteEffects.None, 0);

				Color starColor = new Color(150, 220, 255) * Helpers.Eases.SwoopEase(prog);
				starColor.A = 0;

				spriteBatch.Draw(star, pos + slotTexGlow.Size() / 2f, star.Frame(), starColor, 0, star.Size() / 2f, Helpers.Eases.SwoopEase(prog) * 0.5f, SpriteEffects.None, 0);
			}

			// Hold
			if (timer >= 120 && timer <= 140)
			{
				float colorProg = Helpers.Eases.BezierEase((timer - 120) / 20f);
				var color = Color.Lerp(new Color(50, 120, 255), Color.White, colorProg);

				spriteBatch.Draw(slotTexGlow, pos, slotTexGlow.Frame(), color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

				Color starColor = Color.Lerp(new Color(150, 220, 255), Color.White, colorProg) * (1 - colorProg);
				starColor.A = 0;

				spriteBatch.Draw(star, pos + slotTexGlow.Size() / 2f, star.Frame(), starColor, 0, star.Size() / 2f, (1 + colorProg) * 0.5f, SpriteEffects.None, 0);
			}

			// Fade out with slot underneath
			if (timer > 140 && timer < 170)
			{
				float prog = (timer - 140) / 30f;
				float opacity = 1 - Helpers.Eases.BezierEase(prog);

				spriteBatch.Draw(slotTex, pos, slotTex.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
				spriteBatch.Draw(slotTexGlow, pos, slotTexGlow.Frame(), Color.White * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
			}

			if (timer >= 90 && Main.rand.NextBool(9))
			{
				sparkleParticles.AddParticle(
						position: pos + new Vector2(Main.rand.Next(34), Main.rand.Next(34)) - Vector2.One * 20,
						velocity: Vector2.UnitY * Main.rand.NextFloat(0.2f),
						rotation: 0,
						scale: 0,
						color: new Color(255, 50, 50),
						timer: 90,
						storedPosition: new Vector2(Main.rand.NextFloat(0.4f, 0.7f), 0),
						frame: new Rectangle(0, 0, 15, 15)
					);
			}

			if (timer >= 170)
			{
				spriteBatch.Draw(slotTex, pos, slotTex.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
			}

			if (timer == 239)
				gainAnimationParticles.ClearParticles();
		}

		/// <summary>
		/// Update method for the gain animation particles
		/// </summary>
		/// <param name="particle">The particle to process</param>
		private static void UpdateGainParticles(Particle particle)
		{
			particle.Timer--;

			if (Vector2.Distance(particle.Position, particle.StoredPosition) <= 48)
			{
				particle.Alpha *= 0.95f;
				particle.Velocity *= 0.92f;
				particle.Scale *= 0.92f;
			}
			else
			{
				particle.Alpha += 0.03f;
				particle.Velocity += Vector2.Normalize(particle.Position - particle.StoredPosition) * -0.34f;

				if (particle.Velocity.Length() > 4)
					particle.Velocity = Vector2.Normalize(particle.Velocity) * 4;
			}

			particle.Position += particle.Velocity;
		}

		/// <summary>
		/// Update method for the sparkle particles
		/// </summary>
		/// <param name="particle">The particle to process</param>
		private static void UpdateSparkleParticles(Particle particle)
		{
			particle.Timer--;

			particle.Scale = (float)Math.Sin(particle.Timer / 60f * 3.14f) * particle.StoredPosition.X;

			particle.Color = new Color(180, 255, (byte)(Math.Sin(particle.Timer / 60f * 3.14f) * 255)) * (float)Math.Sin(particle.Timer / 60f * 3.14f);
			particle.Position += particle.Velocity;

			particle.Rotation += 0.05f;
		}
	}
}