using Microsoft.Xna.Framework.Graphics;
using NetEasy;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Loaders;
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

		public override string GetCustomKey()
		{
			return "A shrine - to which deity, you do not know, though it wields a blade. The statue's eyes seem to follow you, and strange runes dance across its pedestal.";
		}
	}

	[SLRDebug]
	class CombatShrineItem : QuickTileItem
	{
		public CombatShrineItem() : base("Combat shrine placer", "{{Debug}} item", "CombatShrine") { }
	}

	class CombatShrineDummy : CombatShrineBase
	{
		public override int MaxWaves => 6;

		public override int ShrineTileWidth => CombatShrine.COMBAT_SHRINE_TILE_WIDTH;
		public override int ShrineTileHeight => CombatShrine.COMBAT_SHRINE_TILE_HEIGHT;

		public CombatShrineDummy() : base(ModContent.TileType<CombatShrine>(), CombatShrine.COMBAT_SHRINE_TILE_WIDTH * 16, CombatShrine.COMBAT_SHRINE_TILE_HEIGHT * 16) { }

		protected override void SpawnWave()
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
	}

	class SpawnEgg : ModProjectile
	{
		public float hpOverride = -1;
		public float damageOverride = -1;
		public float defenseOverride = -1;

		public ref float SpawnType => ref Projectile.ai[0];
		public ref float projScale => ref Projectile.ai[1];

		public int DustCount;
		public CombatShrineBase parent = null;

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
				Helpers.SoundHelper.PlayPitched("ShadowSpawn", 0.4f, 1, Projectile.Center);

			if (Projectile.timeLeft == 30 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				int i = NPC.NewNPC(Projectile.GetSource_FromThis(), (int)Projectile.Center.X, (int)Projectile.Center.Y, (int)SpawnType);
				var spawnPacket = new ShadowSpawnPacket(i, hpOverride, damageOverride, defenseOverride, parent.identity, DustCount);
				spawnPacket.Send();
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Assets.GUI.ItemGlow.Value;
			Texture2D texRing = Assets.GUI.RingGlow.Value;

			float bright = Helpers.Eases.BezierEase(1 - (Projectile.timeLeft - 60) / 120f);

			if (Projectile.timeLeft < 20)
				bright = Projectile.timeLeft / 20f;

			float starScale = Helpers.Eases.BezierEase(1 - (Projectile.timeLeft - 90) / 30f);

			if (Projectile.timeLeft <= 90)
				starScale = 0.3f + Projectile.timeLeft / 90f * 0.7f;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 0, 0, 0) * bright, Helpers.Eases.BezierEase(Projectile.timeLeft / 160f) * 6.28f, tex.Size() / 2, starScale * 0.3f * Projectile.scale, 0, 0);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * bright, Helpers.Eases.BezierEase(Projectile.timeLeft / 160f) * 6.28f, tex.Size() / 2, starScale * 0.2f * Projectile.scale, 0, 0);

			float ringBright = 1;
			if (Projectile.timeLeft > 100)
				ringBright = 1 - (Projectile.timeLeft - 100) / 20f;

			float ringScale = 1 + (Projectile.timeLeft - 50) / 70f * 0.3f;

			if (Projectile.timeLeft <= 50)
				ringScale = Helpers.Eases.BezierEase((Projectile.timeLeft - 20) / 30f);

			Main.spriteBatch.Draw(texRing, Projectile.Center - Main.screenPosition, null, new Color(255, 0, 0, 0) * ringBright * 0.8f, Projectile.timeLeft / 60f * 6.28f, texRing.Size() / 2, ringScale * 0.2f * Projectile.scale, 0, 0);
			Main.spriteBatch.Draw(texRing, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * ringBright * 0.5f, Projectile.timeLeft / 60f * 6.28f, texRing.Size() / 2, ringScale * 0.195f * Projectile.scale, 0, 0);

			if (Projectile.timeLeft < 30)
			{
				Texture2D tex2 = Assets.Masks.GlowSoftAlpha.Value;
				Main.spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, new Color(255, 50, 50, 0) * (Projectile.timeLeft / 30f), 0, tex2.Size() / 2, (1 - Projectile.timeLeft / 30f) * 7 * Projectile.scale, 0, 0);
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 150, 150, 0) * (Projectile.timeLeft / 30f), 0, tex.Size() / 2, (1 - Projectile.timeLeft / 30f) * 1 * Projectile.scale, 0, 0);

				if (Projectile.timeLeft > 15)
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 100, 100, 0) * ((Projectile.timeLeft - 15) / 15f), 1.57f / 4, tex.Size() / 2, (1 - (Projectile.timeLeft - 15) / 15f) * 2 * Projectile.scale, 0, 0);
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

			CombatShrineBase shrineDummy = DummySystem.dummies.FirstOrDefault(n => n.active && n.identity == shrineDummyIdentity && n is CombatShrineBase) as CombatShrineBase;
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