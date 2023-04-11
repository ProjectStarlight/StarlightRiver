using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Physics;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Misc
{
	internal class LootWraith : ModNPC
	{
		private const int NUM_SEGMENTS = 20;

		private int xFrame = 0;

		private bool takenKnockback = false;
		private int chargeCounter = 0;
		private Vector2 chargeVel = Vector2.Zero;

		public bool enraged = false;
		public int xTile = 0;
		public int yTile = 0;

		private readonly List<Vector2> oldPos = new();

		private int screechTimer = 31;

		private float chargeupCounter = 0f;

		public VerletChain chain;

		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		private Player Target => Main.player[NPC.target];

		private Vector2 ChainStart => new Vector2(xTile + 1, yTile + 1) * 16;

		public override string Texture => AssetDirectory.MiscNPC + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Loot Wraith");
			Main.npcFrameCount[NPC.type] = 1;
		}

		public override void Load()
		{
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.MiscNPC + "LootWraith_Chain");
		}

		public override void SetDefaults()
		{
			NPC.width = 62;
			NPC.height = 46;
			NPC.damage = 0;
			NPC.defense = 0;
			NPC.lifeMax = 50;
			NPC.value = 100f;
			NPC.knockBackResist = 1f;
			NPC.HitSound = SoundID.NPCHit54;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.noGravity = true;
			NPC.noTileCollide= true;
			NPC.dontCountMe = true;
			NPC.dontTakeDamage = true;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
				new FlavorTextBestiaryInfoElement("The tortured souls of adventurers obsessed with treasure, forever cursed in death to protect the only thing they cared about in life")
			});
		}

		public override bool NeedSaving()
		{
			return !enraged;
		}

		public override void LoadData(TagCompound tag)
		{
			xTile = tag.GetInt("xTile");
			yTile = tag.GetInt("yTile");
			NPC.Center = ChainStart + new Vector2(0,1);
			chain = new VerletChain(NUM_SEGMENTS, true, ChainStart, 5, false)
			{
				forceGravity = new Vector2(0, 0.1f),
				simStartOffset = 0,
				useEndPoint = true,
				endPoint = NPC.Center
			};
		}

		public override void SaveData(TagCompound tag)
		{
			tag["xTile"] = xTile;
			tag["yTile"] = yTile;
		}

		public override void AI()
		{
			Lighting.AddLight(NPC.Center, Color.Cyan.ToVector3() * 0.5f);
			NPC.TargetClosest(true);
			if (!enraged)
			{
				screechTimer++;

				if (screechTimer < 30)
					CameraSystem.shake++;

				if (screechTimer > 300 && Target.Distance(NPC.Center) < 60 && chargeupCounter == 0)
				{
					SoundEngine.PlaySound(SoundID.DD2_BetsyScream, NPC.Center);
					//CameraSystem.shake += 7;

					Vector2 screamPos = NPC.Center + new Vector2(12 * NPC.spriteDirection, -8);
					DistortionPointHandler.AddPoint(screamPos, 1, 0.5f,
					(intensity, ticksPassed) => 1 + MathF.Sin(ticksPassed * 3.14f / 15f) - ticksPassed / 30f,
					(progress, ticksPassed) => 1 - ticksPassed / 15f,
					(progress, intensity, ticksPassed) => ticksPassed <= 15);

					for (int i = 0; i < 14; i++)
					{
						Vector2 dir = Main.rand.NextVector2CircularEdge(1, 1);
						Dust.NewDustPerfect(screamPos + dir * 25, ModContent.DustType<Dusts.GlowLineFast>(), dir * Main.rand.NextFloat(10), 0, Color.Cyan, 1);
					}

					Target.velocity -= Target.DirectionTo(NPC.Center) * 15;
					screechTimer = 0;
				}

				if (NPC.Distance(Target.Center - NPC.DirectionTo(Target.Center) * 60) > 10)
					NPC.velocity += NPC.DirectionTo(Target.Center - NPC.DirectionTo(Target.Center) * 60) * 0.2f;

				if (NPC.velocity.Length() > 10)
				{
					NPC.velocity.Normalize();
					NPC.velocity *= 10;
				}

				NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(ChainStart), NPC.Distance(ChainStart) * 0.001f);

				Tile tile = Framing.GetTileSafely(xTile, yTile);
				Chest chest = Main.chest.Where(n => n != null && n.x == xTile && n.y == yTile).FirstOrDefault();
				if (ChainStart.Distance(Target.Center) < 500 && (!tile.HasTile || chest.frame > 0 || chargeupCounter > 0))
				{
					chargeupCounter += 0.01f;
					if (chargeupCounter >= 1)
					{
						if (NPC.velocity == Vector2.Zero)
						{
							SoundEngine.PlaySound(SoundID.DeerclopsScream with { Pitch = 0.6f }, NPC.Center);
						}

						xFrame = 1;
						NPC.rotation = 0;
						NPC.velocity.Y = -8;
						if (NPC.Distance(ChainStart) > 100)
						{
							Helper.PlayPitched("Impacts/GlassExplodeShort", 1, Main.rand.NextFloat(0.1f, 0.3f), NPC.Center);
							chargeupCounter = 0;
							NPC.damage = 25;
							NPC.dontTakeDamage = false;
							chain.useEndPoint = false;
							enraged = true;
							NPC.rotation = 0;
							CameraSystem.shake += 6;

							for (int i = 0; i < 14; i++)
							{
								Vector2 dir = Main.rand.NextVector2CircularEdge(1, 1);
								Dust.NewDustPerfect(NPC.Center + dir * 25, ModContent.DustType<Dusts.GlowLineFast>(), dir * Main.rand.NextFloat(10), 0, Color.Cyan, 1);
							}

							ChainGores();
						}
					}
					else
					{
						Vector2 dustDir = Main.rand.NextVector2CircularEdge(1, 1);
						Dust.NewDustPerfect(NPC.Center + dustDir * 15, ModContent.DustType<Dusts.Glow>(), dustDir * Main.rand.NextFloat(5), 0, Color.Cyan, chargeupCounter * 0.5f);
						NPC.rotation = NPC.rotation + Main.rand.NextFloat(chargeupCounter * -0.1f, chargeupCounter * 0.1f);
						NPC.velocity = Vector2.Zero;
					}
				}
			}
			else
			{
				xFrame = 1;

				if (++chargeCounter % 150 == 0)
				{
					takenKnockback = false;
					chargeVel = NPC.DirectionTo(Target.Center) * 15;
				}

				chargeVel *= 0.96f;

				if (chargeVel.Length() > 1 && !takenKnockback)
				{
					NPC.velocity = chargeVel;
				}
				else
				{
					Vector2 dir = Main.rand.NextVector2CircularEdge(1, 1);
					Dust.NewDustPerfect(NPC.Center + new Vector2(5 * NPC.spriteDirection, -6) - dir * 25, ModContent.DustType<GlowLineFast>(), dir * 4, 0, Color.MediumPurple, MathHelper.Min((chargeCounter % 150 - 50) / 100f, 0.7f));
					NPC.velocity *= 0.96f;
				}
			}

			if (xFrame == 1)
			{
				oldPos.Add(NPC.Center);
				if (oldPos.Count > 10)
					oldPos.RemoveAt(0);
			}

			if (chain is null || NPC.DistanceSQ(Target.Center) > 4000000 || enraged)
				return;

			UpdateChain();

			if (!Main.dedServ)
			{
				ManageCache();
				ManageTrail();
			}
		}

		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			if (hit.Knockback > 0)
				takenKnockback = true;
		}

		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (hit.Knockback > 0)
				takenKnockback = true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = Request<Texture2D>(Texture).Value;

			SpriteEffects effects = SpriteEffects.None;
			var origin = new Vector2(NPC.width / 2, NPC.height / 2);

			if (NPC.spriteDirection != 1)
				effects = SpriteEffects.FlipHorizontally;

			if (!enraged)
				DrawChain();

			var slopeOffset = new Vector2(0, NPC.gfxOffY);

			for (int i = 0; i < 2; i++)
				Main.spriteBatch.Draw(texture, slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);

			Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;

			for (int i = 0; i < oldPos.Count; i++)
			{
				float opacity = i / (float)oldPos.Count;
				Main.spriteBatch.Draw(glowTex, slopeOffset + oldPos[i]  - screenPos, NPC.frame, Color.White * opacity * 0.75f, NPC.rotation, origin, NPC.scale * opacity, effects, 0f);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}

		public override void OnKill()
		{
			if (!enraged)
				ChainGores();

			for (int i = 0; i < 3; i++)
			{
				Gore.NewGore(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), GoreID.Smoke1, Main.rand.NextFloat(0.5f,2));
				Gore.NewGore(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), GoreID.Smoke2, Main.rand.NextFloat(0.5f, 2));
				Gore.NewGore(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(2, 2), GoreID.Smoke3, Main.rand.NextFloat(0.5f, 2));
			}
		}

		public override void FindFrame(int frameHeight)
		{
			int frameWidth = NPC.width;
			NPC.frame = new Rectangle(frameWidth * xFrame, 0, frameWidth, frameHeight);
		}

		private void UpdateChain()
		{
			if (!enraged)
				chain.endPoint = ChainStart;
			else
				chain.endPoint = GetChainPoints()[NUM_SEGMENTS - 1];

			chain.startPoint = NPC.Center;
			chain.UpdateChain();
		}

		private void ManageCache()
		{
			cache = new List<Vector2>();

			float pointLength = TotalLength(GetChainPoints()) / NUM_SEGMENTS;

			float pointCounter = 0;

			int presision = 30; //This normalizes length between points so it doesnt squash super weirdly on certain parts
			int iMax = NUM_SEGMENTS - 1;
			if (enraged)
				iMax--;
			for (int i = 0; i < iMax; i++)
			{
				for (int j = 0; j < presision; j++)
				{
					pointCounter += (chain.ropeSegments[i].posNow - chain.ropeSegments[i + 1].posNow).Length() / presision;

					while (pointCounter > pointLength)
					{
						float lerper = j / (float)presision;
						cache.Add(Vector2.Lerp(chain.ropeSegments[i].posNow, chain.ropeSegments[i + 1].posNow, lerper));
						pointCounter -= pointLength;
					}
				}
			}

			while (cache.Count < NUM_SEGMENTS)
			{
				cache.Add(chain.ropeSegments[iMax].posNow);
			}

			while (cache.Count > NUM_SEGMENTS)
			{
				cache.RemoveAt(cache.Count - 1);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, NUM_SEGMENTS, new TriangularTip(1), factor => 7, factor => Lighting.GetColor((int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)) * MathF.Sqrt(1 - factor.X));

			List<Vector2> positions = cache;
			trail.NextPosition = NPC.Center;

			trail.Positions = positions.ToArray();

			trail2 ??= new Trail(Main.instance.GraphicsDevice, NUM_SEGMENTS, new TriangularTip(1), factor => 7, factor => Color.White * chargeupCounter * MathF.Sqrt(1 - factor.X));

			trail2.NextPosition = NPC.Center;

			trail2.Positions = positions.ToArray();
		}

		private void DrawChain()
		{
			if (trail == null || trail == default)
				return;

			Main.spriteBatch.End();
			Effect effect = Terraria.Graphics.Effects.Filters.Scene["RepeatingChain"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture + "_Chain").Value);
			effect.Parameters["flip"].SetValue(false);
			effect.Parameters["alpha"].SetValue(1);

			List<Vector2> points;

			if (cache == null)
				points = GetChainPoints();
			else
				points = trail.Positions.ToList();

			effect.Parameters["repeats"].SetValue(TotalLength(points) / 20f);

			BlendState oldState = Main.graphics.GraphicsDevice.BlendState;
			Main.graphics.GraphicsDevice.BlendState = BlendState.Additive;
			trail?.Render(effect);
			Main.graphics.GraphicsDevice.BlendState = oldState;

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture + "_Chain_White").Value);

			//trail2?.Render(effect);

			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		private List<Vector2> GetChainPoints()
		{
			var points = new List<Vector2>();

			foreach (RopeSegment ropeSegment in chain.ropeSegments)
				points.Add(ropeSegment.posNow);

			return points;
		}

		private float TotalLength(List<Vector2> points)
		{
			float ret = 0;

			for (int i = 1; i < points.Count; i++)
			{
				ret += (points[i] - points[i - 1]).Length();
			}

			return ret;
		}

		private void ChainGores()
		{
			foreach (RopeSegment segment in chain.ropeSegments)
			{
				if (Main.rand.NextBool(2))
					Gore.NewGoreDirect(NPC.GetSource_Death(), segment.posNow, Main.rand.NextVector2Circular(1, 1), Mod.Find<ModGore>("LootWraith_Chain").Type);
			}
		}
	}
}