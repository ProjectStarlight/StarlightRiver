using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using Terraria.GameInput;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Abilities.Faewhip
{
	public class Whip : Ability
	{
		public override Asset<Texture2D> Texture => Assets.Abilities.Faeflame;
		public override Asset<Texture2D> PreviewTexture => Assets.Abilities.FaeflamePreview;
		public override Asset<Texture2D> PreviewTextureOff => Assets.Abilities.FaeflamePreviewOff;

		public override float ActivationCostDefault => 0.15f;
		public override Color Color => new(255, 247, 126);

		public Trail trail;
		public Trail glowTrail;
		public Vector2[] trailPoints = new Vector2[100];
		public SplineHelper.SplineData spline = new();

		public Vector2 tipsPosition; //where the "tip" of the whip is in the world
		public bool attached; //if the whip is attached to anything
		public bool endRooted; //if the endpoint is "rooted" to a certain location and cant be moved

		public float length;
		public float tipVelocity;

		public Vector2 extraVelocity;
		public float targetRot;

		public float endScale;

		public NPC attachedNPC; //if the whip is attached to an NPC, what is it attached to?
		public IFaeWhippable attachedWhippable; //if the whip is attached to an entity with custom whip behavior

		public override void Reset()
		{

		}

		public override void OnActivate()
		{
			trail = null;
			glowTrail = null;
			endScale = 0;

			endRooted = false;

			Player.mount.Dismount(Player);
			spline.StartPoint = Vector2.Zero;

			targetRot = (Main.MouseWorld - Player.Center).ToRotation();
			tipsPosition = Player.Center;
			tipVelocity = 2;

			SoundHelper.PlayPitched("Magic/WaterWoosh", 1, 1.5f + Main.rand.NextFloat(-0.1f, 0.1f), Player.Center);
			SoundHelper.PlayPitched("Magic/FrostHit", 1, 1.5f + Main.rand.NextFloat(-0.1f, 0.1f), Player.Center);
		}

		public void AttachEffects()
		{
			if (endRooted)
			{
				SoundHelper.PlayPitched("JellyBounce", 0.5f, 0.2f, Player.Center);
				SoundHelper.PlayPitched("Magic/FrostHit", 1, 2.6f, Player.Center);

				for (int i = 0; i < 50; i++)
				{
					Vector2 pos = tipsPosition + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4);
					Vector2 vel = Vector2.UnitX.RotatedByRandom(6.28f) * Main.rand.NextFloat(1, 20);

					Dust.NewDustPerfect(pos, DustType<Dusts.PixelatedImpactLineDust>(), vel, 1, new Color(0, 150, 255, 0), 0.15f);
				}
			}
			else
			{
				SoundHelper.PlayPitched("JellyBounce", 1f, -0.5f, Player.Center);

				for (int i = 0; i < 50; i++)
				{
					float rot = Main.rand.NextFloat(6.28f);
					float dist = Main.rand.NextFloat(120);
					Vector2 pos = tipsPosition + Vector2.UnitX.RotatedBy(rot) * dist;
					Vector2 vel = Vector2.UnitX.RotatedBy(rot) * -dist / 8f;

					Dust.NewDustPerfect(pos, DustType<Dusts.PixelatedImpactLineDust>(), vel, 1, new Color(255, 50, 75, 0), 0.1f);
				}
			}
		}

		public override void UpdateActive()
		{
			bool control = StarlightRiver.Instance.AbilityKeys.Get<Whip>().Current;

			if (!control || Player.GetHandler().Stamina <= 0)
			{
				attached = false;
				attachedNPC = null;

				attachedWhippable?.OnRelease(this);
				attachedWhippable = null;

				Deactivate();

				extraVelocity = Main.MouseScreen;
				return;
			}

			Player.GetHandler().Stamina -= 0.0025f;

			if (spline.StartPoint == Vector2.Zero)
			{
				spline.MidPoint = Vector2.Lerp(Player.Center, tipsPosition, 0.5f);
			}

			spline.StartPoint = Player.Center;
			spline.MidPoint += (Vector2.Lerp(Player.Center, tipsPosition, 0.5f) - spline.MidPoint) * 0.075f;
			spline.EndPoint = tipsPosition;

			if (!attached)
			{
				float dist = Vector2.Distance(Player.Center, tipsPosition);

				for (int k = 0; k < 8; k++)
				{
					if (dist < 700)
						tipsPosition += Vector2.UnitX.RotatedBy(targetRot) * tipVelocity;

					//Check VS NPC interactions
					for (int i = 0; i < Main.maxNPCs; i++)
					{
						NPC npc = Main.npc[i];

						//First check the special case of custom whip interaction implementation
						if (npc.ModNPC is IFaeWhippable)
						{
							if ((npc.ModNPC as IFaeWhippable).IsWhipColliding(tipsPosition))
							{
								attachedWhippable = npc.ModNPC as IFaeWhippable;
								attachedWhippable.OnAttach(this);
								attached = true;

								//If the object still wants the regular NPC binding to run
								if (attachedWhippable.NormalNPCInteraction())
								{
									attachedNPC = Main.npc[i];

									if (attachedNPC.knockBackResist == 0)
										endRooted = true;
								}

								AttachEffects();
							}

							return;
						}

						//Next check the normal case for NPCs (hitbox colission)
						if (npc.active && npc.Hitbox.Contains(tipsPosition.ToPoint()))
						{
							attachedNPC = Main.npc[i];
							attached = true;

							if (attachedNPC.knockBackResist == 0)
								endRooted = true;

							AttachEffects();

							return;
						}
					}

					//Check VS Projectile interactions
					for (int i = 0; i < Main.maxProjectiles; i++)
					{
						//we only want to handle special cases for projectiles
						var whippable = Main.projectile[i].ModProjectile as IFaeWhippable;

						if (whippable != null && whippable.IsWhipColliding(tipsPosition))
						{
							attachedWhippable = whippable;
							attachedWhippable.OnAttach(this);
							attached = true;

							AttachEffects();
						}
					}

					// Check VS dummy interactions
					foreach (Dummy dummy in DummySystem.dummies)
					{
						var whippable = dummy as IFaeWhippable;

						if (whippable != null && whippable.IsWhipColliding(tipsPosition))
						{
							attachedWhippable = whippable;
							attachedWhippable.OnAttach(this);
							attached = true;

							AttachEffects();
						}
					}

					//Check VS Tile interactions
					Tile tile = Framing.GetTileSafely((int)tipsPosition.X / 16, (int)tipsPosition.Y / 16);

					if (tile.HasTile && Main.tileSolid[tile.TileType]) //debug
					{
						endRooted = true;
						attached = true;

						AttachEffects();

						return;
					}
				}

				if (tipVelocity < 16)
					tipVelocity++;

				if (dist > 700)
					Deactivate();

				length = dist - 80;
				if (length < 100)
					length = 100;
			}
			else
			{
				if (endScale < 1.5f)
					endScale += 0.1f;

				if (attachedWhippable != null)
				{
					attachedWhippable.UpdateWhileWhipped(this);

					if (attachedWhippable.DetachCondition())
					{
						attachedWhippable.OnRelease(this);
						attachedWhippable = null;
						attached = false;
						Deactivate();
					}

					return;
				}

				if (endRooted)
				{
					if (attachedNPC != null && attachedNPC.active)
						tipsPosition = attachedNPC.Center;

					Player.velocity -= extraVelocity;

					Player.velocity.Y -= 0.43f;

					Player.velocity += (Main.MouseWorld - tipsPosition) * -(0.05f - Eases.BezierEase(Player.velocity.Length() / 24f) * 0.025f);

					if (Player.velocity.Length() > 18)
						Player.velocity = Vector2.Normalize(Player.velocity) * 17.99f;

					Player.velocity *= 0.92f;

					Vector2 pullPoint = tipsPosition + Vector2.Normalize(Player.Center - tipsPosition) * length;
					Player.velocity += (pullPoint - Player.Center) * 0.06f;
					extraVelocity = (pullPoint - Player.Center) * 0.05f;
				}
				else
				{
					if (attachedNPC is null || !attachedNPC.active)
					{
						attached = false;
						attachedNPC = null;
						Deactivate();
						return;
					}

					tipsPosition = attachedNPC.Center;

					Vector2 targetPoint = Player.Center + Vector2.UnitX.RotatedBy((Main.MouseWorld - Player.Center).ToRotation()) * Math.Min(700, Vector2.Distance(Player.Center, Main.MouseWorld));
					attachedNPC.velocity += (targetPoint - attachedNPC.Center) * 0.1f;

					if (attachedNPC.velocity.Length() > 18)
						attachedNPC.velocity = Vector2.Normalize(attachedNPC.velocity) * 17.99f;

					attachedNPC.velocity *= 0.92f;

					//attachedNPC.velocity += (attachedNPC.Center - Player.Center) * -0.05f;
				}
			}

			for (int k = 0; k < 100; k++) //dust
			{
				Vector2 pos = SplineHelper.PointOnSpline(k / 100f, spline);

				if (k > 0 && Main.rand.NextBool(80))
					Dust.NewDustPerfect(pos, DustType<Dusts.PixelatedEmber>(), Vector2.UnitY * Main.rand.NextFloat(-2f, -1f), 1, trailColor(new Vector2(k/100f, 0)), Main.rand.NextFloat(0.2f));
			}
		}

		public Color trailColor(Vector2 prog)
		{
			Color stillColor = new Color(0, 150, 255, 0);
			Color moveColor = new Color(255, 50, 75, 0);
			Color noodleColor = new Color(255, 255, 150, 0);

			Color startColor = endRooted ? moveColor : stillColor;
			Color endColor = endRooted ? stillColor : moveColor;

			if (prog.X < 0.5)
				return Color.Lerp(startColor, noodleColor, prog.X / 0.5f);
			if (prog.X > 0.5)
				return Color.Lerp(noodleColor, endColor, (prog.X - 0.5f) / 0.5f);

			return noodleColor;
		}

		public override void DrawActiveEffects(SpriteBatch spriteBatch)
		{
			if (!Active || !PlayerTargetSystem.canUseTarget)
				return;

			if (trail is null || trail.IsDisposed)
				trail = new Trail(Main.graphics.GraphicsDevice, 100, new NoTip(), n => 10 + (int)(4 * Math.Sin(n * 3.14f)), n => trailColor(n) * 0.25f);

			if (glowTrail is null || glowTrail.IsDisposed)
				glowTrail = new Trail(Main.graphics.GraphicsDevice, 100, new NoTip(), n => 18 + n * 0, n => trailColor(n) * 0.03f);

			trail.Positions = trailPoints;
			glowTrail.Positions = trailPoints;

			for (int k = 0; k < 100; k++)
			{
				Vector2 pos = SplineHelper.PointOnSpline(k / 100f, spline);
				trailPoints[k] = pos;
			}

			Effect effect = ShaderLoader.GetShader("WhipAbility").Value;

			if (spline.StartPoint != Vector2.Zero && effect != null)
			{
				ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
				{
					Texture2D tex0 = Assets.BlurryTrail.Value;
					Texture2D tex1 = Assets.ShadowTrail.Value;

					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Matrix.Identity;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["time"].SetValue(Main.GameUpdateCount * -0.025f);
					effect.Parameters["repeats"].SetValue(4f);
					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["sampleTexture"].SetValue(tex0);

					trail?.Render(effect);

					effect.Parameters["repeats"].SetValue(2f);
					effect.Parameters["sampleTexture"].SetValue(tex1);

					glowTrail?.Render(effect);

				});
			}

			if (attached)
			{
				Color stillColor = new Color(50, 150, 255, 0);
				Color moveColor = new Color(255, 50, 75, 0);

				Color endColor = endRooted ? stillColor : moveColor;

				Texture2D endTex = endRooted ? Assets.Abilities.WhipEndRoot.Value : Assets.Abilities.WhipEndGrab.Value;
				Texture2D endGlow = Assets.Masks.GlowSoftAlpha.Value;

				spriteBatch.Draw(endTex, tipsPosition - Main.screenPosition, null, endColor, Main.GameUpdateCount * 0.1f, endTex.Size() / 2, endScale * 0.75f, 0, 0);
				spriteBatch.Draw(endGlow, tipsPosition - Main.screenPosition, null, endColor, 0, endGlow.Size() / 2, endScale * (endRooted ? 0.5f : 1f), 0, 0);
			}
		}

		public override void OnExit()
		{
			for (int k = 0; k < 40; k++) //dust
			{
				Vector2 pos = SplineHelper.PointOnSpline(k / 40f, spline);
				Vector2 next = SplineHelper.PointOnSpline((k + 1) / 40f, spline);

				if (k > 0)
					Dust.NewDustPerfect(pos, DustType<Dusts.PixelatedImpactLineDust>(), Vector2.Normalize(next - pos).RotatedByRandom(0.5f) * Main.rand.NextFloat(10f), 1, trailColor(new Vector2(k / 40f, 0)), 0.1f);
			}
		}

		public override bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys)
		{
			return abilityKeys.Get<Whip>().JustPressed;
		}
	}
}