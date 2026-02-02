using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	/// <summary>
	/// Base class for all combat shrine variants. Contains shared combat logic, state machine, rendering, and networking.
	/// Variants override SpawnWave() to define their enemy compositions.
	/// </summary>
	abstract class CombatShrineBase : ShrineDummy
	{
		public List<NPC> minions = new();

		protected int waveTime = 0;
		public float Windup => Math.Min(1, timer / 120f);

		public abstract int MaxWaves { get; }

		public override int ArenaOffsetX => -25;
		public override int ArenaSizeX => 51;
		public override int ArenaOffsetY => -19;
		public override int ArenaSizeY => 26;

		public override abstract int ShrineTileWidth { get; }
		public override abstract int ShrineTileHeight { get; }

		protected CombatShrineBase(int tileType, int width, int height) : base(tileType, width, height) { }

		/// <summary>
		/// Spawns enemy wave based on current state. Must be overridden by variants.
		/// </summary>
		protected abstract void SpawnWave();

		/// <summary>
		/// Spawns rewards on shrine completion. Can be overridden for variant-specific loot.
		/// </summary>
		protected virtual void SpawnReward()
		{
			Item.NewItem(GetSource_FromAI(), Hitbox, ModContent.ItemType<DullBlade>());
			ShrineUtils.SimulateGoldChest(this, false);
			ShrineUtils.SimulateWoodenChest(this);
		}

		public override void Update()
		{
			if (state == SHRINE_STATE_DEFEATED)//dont run anything if this is defeated
				return;

			//this check never succeeds since the tile does not spawn dummys on the 3rd frame
			if (Parent.TileFrameX >= 6 * 18)//check file frame for this being defeated
			{
				state = SHRINE_STATE_DEFEATED;
				return;//return here so defeated shrines never run the below code even when spawning a new dummy
			}

			// Protection is active in all states except defeated
			ProtectionWorld.AddRegionBySource(new Point16(ParentX, ParentY), ArenaTile);

			bool anyPlayerInRange = false;

			foreach (Player player in Main.player)
			{
				bool thisPlayerInRange = player.active && !player.DeadOrGhost && ArenaPlayer.Intersects(player.Hitbox);

				if (thisPlayerInRange && state != SHRINE_STATE_IDLE)
					player.GetModPlayer<ShrinePlayer>().CombatShrineActive = true;

				anyPlayerInRange = anyPlayerInRange || thisPlayerInRange;
			}

			if (state == SHRINE_STATE_IDLE && Parent.TileFrameX >= 3 * 18)//if idle and frame isnt default (happens when entity is despawned while active)
			{
				SetFrame(0);
				return;
			}

			if (state != SHRINE_STATE_IDLE)//this does not need a defeated check because of the above one
			{

				Dust.NewDustPerfect(Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(2), 0, new Color(255, 40 + Main.rand.Next(50), 75) * Windup, 0.2f);

				if (Main.rand.NextBool(2))
				{
					Dust.NewDustPerfect(Center + new Vector2(-25 * 16 - 8 + 32, 24 + Main.rand.Next(-40, 40)), ModContent.DustType<Dusts.Glow>(), Vector2.UnitX * -Main.rand.NextFloat(2), 0, new Color(255, 40 + Main.rand.Next(50), 75) * Windup, 0.35f);
					Dust.NewDustPerfect(Center + new Vector2(24 * 16, 24 + Main.rand.Next(-40, 40)), ModContent.DustType<Dusts.Glow>(), Vector2.UnitX * Main.rand.NextFloat(2), 0, new Color(255, 40 + Main.rand.Next(50), 75) * Windup, 0.35f);
				}

				if (state == SHRINE_STATE_FAILED || !anyPlayerInRange) //"fail" conditions, no living Players in radius or already failing
				{
					state = SHRINE_STATE_FAILED;

					if (timer > 128)
					{
						netUpdate = true;
						timer = 128;
					}

					timer--;

					if (timer <= 0)
					{
						state = SHRINE_STATE_IDLE;
						waveTime = 0;

						foreach (NPC NPC in minions)
							NPC.active = false;

						minions.Clear();
						// Don't remove protection - it persists until defeated
					}

					return;
				}

				timer++;

				if (state == MaxWaves + 2)
				{
					if (timer - waveTime >= 128)// --- !  WIN CONDITION  ! ---
					{
						for (int k = 0; k < 30; k++)
							Dust.NewDustPerfect(Center + new Vector2(0, -32), ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, new Color(255, 100, 100), 0.6f);

						SpawnReward();
						state = SHRINE_STATE_DEFEATED;

						timer = 0;
						waveTime = 0;
						SetFrame(2);
						ProtectionWorld.RemoveRegionBySource(new Point16(ParentX, ParentY));
					}

					return;
				}

				// iterate over minions and remove from the list any that are not given name shadow or active to avoid other npcs replacing them and still counting
				minions.RemoveAll(n => !n.active || n.GivenName != "Shadow");

				if (!minions.Any(n => n.active) && timer - waveTime > 181) //advance the wave
				{
					SpawnWave();
					waveTime = (int)timer;
					state++;
				}
			}
			//else//renable this if there are issues with protection being left on
			//{
			//	ProtectionWorld.RemoveRegionBySource(new Point16(ParentX, ParentY));
			//}
		}

		protected void SpawnNPC(Vector2 pos, int type, int dustAmount, float hpOverride = -1, float damageOverride = -1, float defenseOverride = -1, float scale = 1)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return; //don't spawn this on mp clients

			int i = Projectile.NewProjectile(new EntitySource_WorldEvent(), pos, Vector2.Zero, ModContent.ProjectileType<SpawnEgg>(), 0, 0, Owner: -1, type, scale);
			(Main.projectile[i].ModProjectile as SpawnEgg).parent = this;
			(Main.projectile[i].ModProjectile as SpawnEgg).hpOverride = hpOverride;
			(Main.projectile[i].ModProjectile as SpawnEgg).damageOverride = damageOverride;
			(Main.projectile[i].ModProjectile as SpawnEgg).defenseOverride = defenseOverride;
			(Main.projectile[i].ModProjectile as SpawnEgg).DustCount = dustAmount;
		}

		public override void PostDraw(Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Effect effect = ShaderLoader.GetShader("Whitewash").Value;

			if (effect != null)
			{
				effect.Parameters["outlineColor"].SetValue(new Vector4(0.75f, 0.04f, 0.04f, 1f));

				spriteBatch.End();
				spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);

				for (int k = 0; k < minions.Count; k++)
				{
					NPC target = minions[k];

					if (!target.active)
						continue;

					if (Main.rand.NextBool(2))
						Dust.NewDustPerfect(target.position + new Vector2(Main.rand.Next(target.width), Main.rand.Next(target.height)), ModContent.DustType<Dusts.Shadow>(), new Vector2(0, -Main.rand.NextFloat()), 0, Color.Black, Main.rand.NextFloat());

					spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center + Vector2.UnitX * 2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
					spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center + Vector2.UnitX * -2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
					spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center + Vector2.UnitY * 2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
					spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center + Vector2.UnitY * -2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				}
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointWrap, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

			for (int k = 0; k < minions.Count; k++)
			{
				NPC target = minions[k];

				if (!target.active)
					continue;

				spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center - Main.screenPosition, target.frame, Color.Black, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			}

			DrawGlows(spriteBatch);

			spriteBatch.End();
			spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
		}

		public void DrawGlows(SpriteBatch spriteBatch)
		{
			if (state != SHRINE_STATE_IDLE && state != SHRINE_STATE_DEFEATED)
			{
				Texture2D tex = Assets.Tiles.Moonstone.GlowSmall.Value;
				var origin = new Vector2(tex.Width / 2, tex.Height);
				spriteBatch.Draw(tex, Center - Main.screenPosition + new Vector2(0, 60), default, GetBeamColor(StarlightWorld.visualTimer), 0, origin, 3.5f, 0, 0);
				spriteBatch.Draw(tex, Center - Main.screenPosition + new Vector2(10, 60), default, GetBeamColor(StarlightWorld.visualTimer + 2) * 0.8f, 0, origin, 2.5f, 0, 0);
				spriteBatch.Draw(tex, Center - Main.screenPosition + new Vector2(-10, 60), default, GetBeamColor(StarlightWorld.visualTimer + 4) * 0.8f, 0, origin, 3.2f, 0, 0);

				float rad = -32;

				if (state >= MaxWaves + 2)
					rad += Eases.BezierEase((timer - waveTime) / 128f) * 32;

				for (int k = 0; k < Math.Min(state - 2, MaxWaves - 1); k++)
				{
					Texture2D tex2 = Assets.Masks.GlowSoftAlpha.Value;
					spriteBatch.Draw(tex, Center - Main.screenPosition + new Vector2(0, -44) + Vector2.UnitX.RotatedBy(k / (float)(MaxWaves - 2) * 3.14f) * rad, default, new Color(255, 100, 100, 0), 0, tex.Size() / 2, 0.3f, 0, 0);
					spriteBatch.Draw(tex2, Center - Main.screenPosition + new Vector2(0, -32) + Vector2.UnitX.RotatedBy(k / (float)(MaxWaves - 2) * 3.14f) * rad, default, new Color(255, 100, 100, 0), 0, tex2.Size() / 2, 0.3f, 0, 0);
					spriteBatch.Draw(tex2, Center - Main.screenPosition + new Vector2(0, -32) + Vector2.UnitX.RotatedBy(k / (float)(MaxWaves - 2) * 3.14f) * rad, default, new Color(255, 255, 255, 0), 0, tex2.Size() / 2, 0.1f, 0, 0);
				}

				Texture2D barrier = Assets.MotionTrail.Value;
				var sourceRect = new Rectangle(0, (int)(Main.GameUpdateCount * 0.4f), barrier.Width, barrier.Height);
				var sourceRect2 = new Rectangle(0, (int)(Main.GameUpdateCount * -0.73f), barrier.Width, barrier.Height);

				var targetRect = new Rectangle((int)(Center.X - Main.screenPosition.X) - 25 * 16 - 10, (int)(Center.Y - Main.screenPosition.Y) - 16, 32, 80);
				spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(255, 100, 100, 0) * 0.6f * Windup);
				spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(255, 50, 50, 0) * 0.5f * Windup);
				targetRect.Inflate(-15, 0);
				targetRect.Offset(15, 0);
				spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(255, 255, 255, 0) * Windup);

				targetRect = new Rectangle((int)(Center.X - Main.screenPosition.X) + 24 * 16 - 6, (int)(Center.Y - Main.screenPosition.Y) - 16, 32, 80);
				spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(255, 100, 100, 0) * 0.6f * Windup, 0, default, SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(255, 50, 50, 0) * 0.5f * Windup, 0, default, SpriteEffects.FlipHorizontally, 0);
				targetRect.Inflate(-15, 0);
				targetRect.Offset(-15, 0);
				spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(255, 255, 255, 0) * Windup);
			}
		}

		private Color GetBeamColor(float time)
		{
			float sin = 0.5f + (float)Math.Sin(time * 2 + 1) * 0.5f;
			float sin2 = 0.5f + (float)Math.Sin(time) * 0.5f;
			return new Color(255, (int)(50 * sin), 0, 0) * sin2 * Windup;
		}

		public override void SafeSendExtraAI(BinaryWriter writer)
		{
			writer.Write(timer);
			writer.Write(state);
		}

		public override void SafeReceiveExtraAI(BinaryReader reader)
		{
			timer = reader.ReadSingle();
			state = reader.ReadSingle();
		}
	}
}
