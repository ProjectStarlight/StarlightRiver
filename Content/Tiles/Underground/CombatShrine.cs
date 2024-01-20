using NetEasy;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	class CombatShrine : ShrineTile
	{

		public const int COMBAT_SHRINE_TILE_WIDTH = 3;
		public const int COMBAT_SHRINE_TILE_HEIGHT = 6;
		public override int DummyType => DummySystem.DummyType<CombatShrineDummy>();

		public override string Texture => "StarlightRiver/Assets/Tiles/Underground/CombatShrine";

		public override int ShrineTileWidth => COMBAT_SHRINE_TILE_WIDTH;

		public override int ShrineTileHeight => COMBAT_SHRINE_TILE_HEIGHT;

		public override string GetHint()
		{
			return "A shrine - to which deity, you do not know, though it wields a blade. The statue's eyes seem to follow you, and strange runes dance across its pedestal.";
		}
	}

	[SLRDebug]
	class CombatShrineItem : QuickTileItem
	{
		public CombatShrineItem() : base("Combat shrine placer", "{{Debug}} item", "CombatShrine") { }
	}

	class CombatShrineDummy : ShrineDummy, IDrawAdditive
	{
		public List<NPC> minions = new();

		public int maxWaves = 6;
		private int waveTime = 0;
		public float Windup => Math.Min(1, timer / 120f);

		public override int ArenaOffsetX => -25;
		public override int ArenaSizeX => 51;
		public override int ArenaOffsetY => -19;
		public override int ArenaSizeY => 26;

		public override int ShrineTileWidth => CombatShrine.COMBAT_SHRINE_TILE_WIDTH;
		public override int ShrineTileHeight => CombatShrine.COMBAT_SHRINE_TILE_HEIGHT;

		public CombatShrineDummy() : base(ModContent.TileType<CombatShrine>(), CombatShrine.COMBAT_SHRINE_TILE_WIDTH * 16, CombatShrine.COMBAT_SHRINE_TILE_HEIGHT * 16) { }

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
				ProtectionWorld.AddRegionBySource(new Point16(ParentX, ParentY), ArenaTile);//stop calling this and call RemoveRegionBySource() when shrine is completed

				StarlightRiver.Instance.useIntenseMusic = true;
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
						ProtectionWorld.RemoveRegionBySource(new Point16(ParentX, ParentY));
					}

					return;
				}

				timer++;

				if (state == maxWaves + 2)
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

		private void SpawnWave()
		{
			for (int k = 0; k < 20; k++)
				Dust.NewDustPerfect(Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(5), 0, new Color(255, Main.rand.Next(50), 0), 0.5f);

			if (state == 1)
			{
				SpawnNPC(Center + new Vector2(130, 50), NPCID.RedSlime, 20);
				SpawnNPC(Center + new Vector2(-130, 50), NPCID.RedSlime, 20);
				SpawnNPC(Center + new Vector2(267, -40), NPCID.RedSlime, 20);
				SpawnNPC(Center + new Vector2(-267, -40), NPCID.RedSlime, 20);
			}

			if (state == 2)
			{
				SpawnNPC(Center + new Vector2(110, 50), NPCID.RedSlime, 20);
				SpawnNPC(Center + new Vector2(-110, 50), NPCID.RedSlime, 20);
				SpawnNPC(Center + new Vector2(240, 40), NPCID.Skeleton, 20);
				SpawnNPC(Center + new Vector2(-240, 40), NPCID.Skeleton, 20);
				SpawnNPC(Center + new Vector2(0, -150), NPCID.CaveBat, 20);
			}

			if (state == 3)
			{
				SpawnNPC(Center + new Vector2(130, 40), NPCID.GreekSkeleton, 20);
				SpawnNPC(Center + new Vector2(-130, 40), NPCID.GreekSkeleton, 20);
				SpawnNPC(Center + new Vector2(140, -140), NPCID.CaveBat, 20);
				SpawnNPC(Center + new Vector2(-140, -140), NPCID.CaveBat, 20);
			}

			if (state == 4)
			{
				SpawnNPC(Center + new Vector2(130, 50), NPCID.Skeleton, 20);
				SpawnNPC(Center + new Vector2(-130, 50), NPCID.Skeleton, 20);

				SpawnNPC(Center + new Vector2(267, -40), NPCID.GreekSkeleton, 20);
				SpawnNPC(Center + new Vector2(-267, -40), NPCID.GreekSkeleton, 20);

				SpawnNPC(Center + new Vector2(70, -140), NPCID.CaveBat, 20);
				SpawnNPC(Center + new Vector2(-70, -140), NPCID.CaveBat, 20);
			}

			if (state == 5)
			{
				SpawnNPC(Center + new Vector2(130, 50), NPCID.GreekSkeleton, 20);
				SpawnNPC(Center + new Vector2(-130, 50), NPCID.GreekSkeleton, 20);

				SpawnNPC(Center + new Vector2(120, -160), NPCID.CaveBat, 20);
				SpawnNPC(Center + new Vector2(-120, -160), NPCID.CaveBat, 20);

				SpawnNPC(Center + new Vector2(220, -110), NPCID.CaveBat, 20);
				SpawnNPC(Center + new Vector2(-220, -110), NPCID.CaveBat, 20);
			}

			if (state == 6)
			{
				SpawnNPC(Center + new Vector2(130, 50), NPCID.Skeleton, 20);
				SpawnNPC(Center + new Vector2(-130, 50), NPCID.Skeleton, 20);

				SpawnNPC(Center + new Vector2(267, -50), NPCID.Skeleton, 20);
				SpawnNPC(Center + new Vector2(-267, -50), NPCID.Skeleton, 20);

				SpawnNPC(Center + new Vector2(0, -170), NPCID.Demon, 40, hpOverride: 2f, scale: 1.5f);
			}
		}

		private void SpawnNPC(Vector2 pos, int type, int dustAmount, float hpOverride = -1, float damageOverride = -1, float defenseOverride = -1, float scale = 1)
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

		private void SpawnReward()
		{
			Item.NewItem(GetSource_FromAI(), Hitbox, ModContent.ItemType<DullBlade>());
			ShrineUtils.SimulateGoldChest(this, false);//todo: maybe this should be a relic on no-hit
			ShrineUtils.SimulateWoodenChest(this);
		}

		public override void PostDraw(Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			for (int k = 0; k < minions.Count; k++)
			{
				NPC target = minions[k];

				if (!target.active)
					continue;

				if (Main.rand.NextBool(2))
					Dust.NewDustPerfect(target.position + new Vector2(Main.rand.Next(target.width), Main.rand.Next(target.height)), ModContent.DustType<Dusts.Shadow>(), new Vector2(0, -Main.rand.NextFloat()), 0, Color.Black, Main.rand.NextFloat());

				Effect effect = Terraria.Graphics.Effects.Filters.Scene["Whitewash"].GetShader().Shader;

				spriteBatch.End();
				spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

				spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center + Vector2.UnitX * 2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center + Vector2.UnitX * -2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center + Vector2.UnitY * 2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center + Vector2.UnitY * -2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

				spriteBatch.Draw(TextureAssets.Npc[target.type].Value, target.Center - Main.screenPosition, target.frame, Color.Black, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (state != SHRINE_STATE_IDLE && state != SHRINE_STATE_DEFEATED)
			{
				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall").Value;
				var origin = new Vector2(tex.Width / 2, tex.Height);
				spriteBatch.Draw(tex, Center - Main.screenPosition + new Vector2(0, 60), default, GetBeamColor(StarlightWorld.visualTimer), 0, origin, 3.5f, 0, 0);
				spriteBatch.Draw(tex, Center - Main.screenPosition + new Vector2(10, 60), default, GetBeamColor(StarlightWorld.visualTimer + 2) * 0.8f, 0, origin, 2.5f, 0, 0);
				spriteBatch.Draw(tex, Center - Main.screenPosition + new Vector2(-10, 60), default, GetBeamColor(StarlightWorld.visualTimer + 4) * 0.8f, 0, origin, 3.2f, 0, 0);

				float rad = -32;

				if (state >= maxWaves + 2)
					rad += Helpers.Helper.BezierEase((timer - waveTime) / 128f) * 32;

				for (int k = 0; k < Math.Min(state - 2, maxWaves - 1); k++)
				{
					Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
					spriteBatch.Draw(tex, Center - Main.screenPosition + new Vector2(0, -44) + Vector2.UnitX.RotatedBy(k / (float)(maxWaves - 2) * 3.14f) * rad, default, new Color(255, 100, 100), 0, tex.Size() / 2, 0.3f, 0, 0);
					spriteBatch.Draw(tex2, Center - Main.screenPosition + new Vector2(0, -32) + Vector2.UnitX.RotatedBy(k / (float)(maxWaves - 2) * 3.14f) * rad, default, new Color(255, 100, 100), 0, tex2.Size() / 2, 0.3f, 0, 0);
					spriteBatch.Draw(tex2, Center - Main.screenPosition + new Vector2(0, -32) + Vector2.UnitX.RotatedBy(k / (float)(maxWaves - 2) * 3.14f) * rad, default, Color.White, 0, tex2.Size() / 2, 0.1f, 0, 0);
				}

				Texture2D barrier = ModContent.Request<Texture2D>("StarlightRiver/Assets/MotionTrail").Value;
				var sourceRect = new Rectangle(0, (int)(Main.GameUpdateCount * 0.4f), barrier.Width, barrier.Height);
				var sourceRect2 = new Rectangle(0, (int)(Main.GameUpdateCount * -0.73f), barrier.Width, barrier.Height);

				var targetRect = new Rectangle((int)(Center.X - Main.screenPosition.X) - 25 * 16 - 10, (int)(Center.Y - Main.screenPosition.Y) - 16, 32, 80);
				spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(255, 100, 100) * 0.6f * Windup);
				spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(255, 50, 50) * 0.5f * Windup);
				targetRect.Inflate(-15, 0);
				targetRect.Offset(15, 0);
				spriteBatch.Draw(barrier, targetRect, sourceRect2, Color.White * Windup);

				targetRect = new Rectangle((int)(Center.X - Main.screenPosition.X) + 24 * 16 - 6, (int)(Center.Y - Main.screenPosition.Y) - 16, 32, 80);
				spriteBatch.Draw(barrier, targetRect, sourceRect, new Color(255, 100, 100) * 0.6f * Windup, 0, default, SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(barrier, targetRect, sourceRect2, new Color(255, 50, 50) * 0.5f * Windup, 0, default, SpriteEffects.FlipHorizontally, 0);
				targetRect.Inflate(-15, 0);
				targetRect.Offset(-15, 0);
				spriteBatch.Draw(barrier, targetRect, sourceRect2, Color.White * Windup);
			}
		}

		private Color GetBeamColor(float time)
		{
			float sin = 0.5f + (float)Math.Sin(time * 2 + 1) * 0.5f;
			float sin2 = 0.5f + (float)Math.Sin(time) * 0.5f;
			return new Color(255, (int)(50 * sin), 0) * sin2 * Windup;
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

	class SpawnEgg : ModProjectile, IDrawAdditive
	{
		public float hpOverride = -1;
		public float damageOverride = -1;
		public float defenseOverride = -1;

		public ref float SpawnType => ref Projectile.ai[0];
		public ref float projScale => ref Projectile.ai[1];

		public int DustCount;
		public CombatShrineDummy parent = null;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			Projectile.scale = projScale;

			if (Projectile.timeLeft == 70)
				Helpers.Helper.PlayPitched("ShadowSpawn", 0.4f, 1, Projectile.Center);

			if (Projectile.timeLeft == 30 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				int i = NPC.NewNPC(Projectile.GetSource_FromThis(), (int)Projectile.Center.X, (int)Projectile.Center.Y, (int)SpawnType);
				var spawnPacket = new ShadowSpawnPacket(i, hpOverride, damageOverride, defenseOverride, parent.identity, DustCount);
				spawnPacket.Send();
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ItemGlow").Value;
			Texture2D texRing = ModContent.Request<Texture2D>(AssetDirectory.GUI + "RingGlow").Value;

			float bright = Helpers.Helper.BezierEase(1 - (Projectile.timeLeft - 60) / 120f);

			if (Projectile.timeLeft < 20)
				bright = Projectile.timeLeft / 20f;

			float starScale = Helpers.Helper.BezierEase(1 - (Projectile.timeLeft - 90) / 30f);

			if (Projectile.timeLeft <= 90)
				starScale = 0.3f + Projectile.timeLeft / 90f * 0.7f;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Red * bright, Helpers.Helper.BezierEase(Projectile.timeLeft / 160f) * 6.28f, tex.Size() / 2, starScale * 0.3f * Projectile.scale, 0, 0);
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * bright, Helpers.Helper.BezierEase(Projectile.timeLeft / 160f) * 6.28f, tex.Size() / 2, starScale * 0.2f * Projectile.scale, 0, 0);

			float ringBright = 1;
			if (Projectile.timeLeft > 100)
				ringBright = 1 - (Projectile.timeLeft - 100) / 20f;

			float ringScale = 1 + (Projectile.timeLeft - 50) / 70f * 0.3f;

			if (Projectile.timeLeft <= 50)
				ringScale = Helpers.Helper.BezierEase((Projectile.timeLeft - 20) / 30f);

			spriteBatch.Draw(texRing, Projectile.Center - Main.screenPosition, null, Color.Red * ringBright * 0.8f, Projectile.timeLeft / 60f * 6.28f, texRing.Size() / 2, ringScale * 0.2f * Projectile.scale, 0, 0);
			spriteBatch.Draw(texRing, Projectile.Center - Main.screenPosition, null, Color.White * ringBright * 0.5f, Projectile.timeLeft / 60f * 6.28f, texRing.Size() / 2, ringScale * 0.195f * Projectile.scale, 0, 0);

			if (Projectile.timeLeft < 30)
			{
				Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
				spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, new Color(255, 50, 50) * (Projectile.timeLeft / 30f), 0, tex2.Size() / 2, (1 - Projectile.timeLeft / 30f) * 7 * Projectile.scale, 0, 0);
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 150, 150) * (Projectile.timeLeft / 30f), 0, tex.Size() / 2, (1 - Projectile.timeLeft / 30f) * 1 * Projectile.scale, 0, 0);

				if (Projectile.timeLeft > 15)
					spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 100, 100) * ((Projectile.timeLeft - 15) / 15f), 1.57f / 4, tex.Size() / 2, (1 - (Projectile.timeLeft - 15) / 15f) * 2 * Projectile.scale, 0, 0);
			}
		}
	}

	/// <summary>
	/// Specialized spawning packet for the combat shrine shadows that set their variables. Must be initiated by the server
	/// </summary>
	[Serializable]
	public class ShadowSpawnPacket : Module
	{
		readonly int npcId;
		readonly float hpOverride;
		readonly float damageOverride;
		readonly float defenseOverride;
		readonly int shrineDummyIdentity;
		readonly int dustCount;

		public ShadowSpawnPacket(int npcId, float hpOverride, float damageOverride, float defenseOverride, int shrineDummyIdentity, int dustCount)
		{
			this.npcId = npcId;
			this.hpOverride = hpOverride;
			this.damageOverride = damageOverride;
			this.defenseOverride = defenseOverride;
			this.shrineDummyIdentity = shrineDummyIdentity;
			this.dustCount = dustCount;
		}

		protected override void Receive()
		{
			NPC NPC = Main.npc[npcId];
			NPC.alpha = 255;
			NPC.GivenName = "Shadow";
			NPC.lavaImmune = true;
			NPC.trapImmune = true;
			NPC.HitSound = SoundID.NPCHit7;
			SoundStyle shadowDeath = new($"{nameof(StarlightRiver)}/Sounds/ShadowDeath") { MaxInstances = 3 };
			NPC.DeathSound = shadowDeath;

			if (NPC.TryGetGlobalNPC(out StarlightNPC starlightNPC)) // while this global NPC seems to never exist in time on mp clients, this particular bool only matters for the server
				starlightNPC.dontDropItems = true;

			if (hpOverride != -1)
			{
				NPC.lifeMax = (int)(NPC.lifeMax * hpOverride);
				NPC.life = (int)(NPC.life * hpOverride);
			}

			if (damageOverride != -1)
				NPC.damage = (int)(NPC.damage * damageOverride);

			if (defenseOverride != -1)
				NPC.defense = (int)(NPC.defense * defenseOverride);

			CombatShrineDummy shrineDummy = DummySystem.dummies.FirstOrDefault(n => n.active && n.identity == shrineDummyIdentity && n is CombatShrineDummy) as CombatShrineDummy;
			shrineDummy?.minions.Add(NPC);

			if (Main.netMode != NetmodeID.Server)
			{
				for (int k = 0; k < dustCount; k++)
				{
					Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(1.5f, 2), 0, new Color(255, 100, 100), 0.2f);
				}
			}
			else
			{
				NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI); //send data to ensure NPC fully exists on clients before this packet
				Send(runLocally: false); //forward packet to clients if this is a server
			}
		}
	}
}