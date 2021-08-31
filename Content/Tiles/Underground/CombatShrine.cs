using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StarlightRiver.Content.Buffs;


namespace StarlightRiver.Content.Tiles.Underground
{
	class CombatShrine : DummyTile
	{
		public override int DummyType => ModContent.ProjectileType<CombatShrineDummy>();

		public override bool Autoload(ref string name, ref string texture)
		{
			texture = "StarlightRiver/Assets/Tiles/Underground/CombatShrine";
			return true;
		}

		public override void SetDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 3, 6, DustID.Stone, SoundID.Tink, false, new Color(100, 100, 100), false, false, "Mysterious Shrine");
		}

		public override bool NewRightClick(int i, int j)
		{
			var tile = (Tile)(Framing.GetTileSafely(i, j).Clone());

			if ((Dummy.modProjectile as CombatShrineDummy).State == 0)
			{
				for (int x = 0; x < 3; x++)
					for (int y = 0; y < 6; y++)
					{
						int realX = x + i - tile.frameX / 18;
						int realY = y + j - tile.frameY / 18;

						Framing.GetTileSafely(realX, realY).frameX += 3 * 18;
					}

				(Dummy.modProjectile as CombatShrineDummy).State = 1;
				return true;
			}

			return false;
		}
	}

	class CombatShrineDummy : Dummy, IDrawAdditive
	{
		public List<NPC> slaves = new List<NPC>();

		public int maxWaves = 6;
		private int waveTime = 0;

		public ref float Timer => ref projectile.ai[0];
		public ref float State => ref projectile.ai[1];

		public float Windup => Math.Min(1, Timer / 120f);

		public CombatShrineDummy() : base(ModContent.TileType<CombatShrine>(), 3 * 16, 6 * 16) { }

		public override void Update()
		{
			if(State == 0 && Parent.frameX > 3 * 18)
			{
				for (int x = 0; x < 3; x++)
					for (int y = 0; y < 6; y++)
					{
						int realX = ParentX - 1 + x;
						int realY = ParentY - 3 + y;

						Framing.GetTileSafely(realX, realY).frameX -= 3 * 18;
					}
			}

			if (State != 0)
			{
				Dust.NewDustPerfect(projectile.Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(2), 0, new Color(255, 40 + Main.rand.Next(50), 75) * Windup, 0.2f);

				if (State == -1 || (!Main.player.Any(n => n.active && !n.dead && Vector2.Distance(n.Center, projectile.Center) < 500))) //"fail" conditions, no living players in radius or already failing
				{
					State = -1;

					if (Timer > 128)
						Timer = 128;

					Timer--;

					if (Timer <= 0)
					{
						State = 0;
						waveTime = 0;

						foreach (NPC npc in slaves)
							npc.active = false;

						slaves.Clear();
					}

					return;
				}

				Timer++;

				if(State == maxWaves + 2)
				{
					if (Timer - waveTime >= 128)
					{
						for (int k = 0; k < 30; k++)
							Dust.NewDustPerfect(projectile.Center + new Vector2(0, -32), ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, new Color(255, 100, 100), 0.6f);

						Main.NewText("Ejaculatory Dysfunction!  Final time: " + Helpers.Helper.TicksToTime((int)Timer));
						State = 0;

						Timer = 0;
						waveTime = 0;
					}

					return;
				}

				if (!slaves.Any(n => n.active) && Timer - waveTime > 181) //advance the wave
				{
					SpawnWave();
					waveTime = (int)Timer;
					State++;
				}
			}
		}

		private void SpawnWave()
		{
			for(int k = 0; k < 20; k++)
				Dust.NewDustPerfect(projectile.Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(5), 0, new Color(255, Main.rand.Next(50), 0), 0.5f);

			if (State == 1)
			{
				SpawnNPC(projectile.Center + new Vector2(130, 50), NPCID.RedSlime, 20);
				SpawnNPC(projectile.Center + new Vector2(-130, 50), NPCID.RedSlime, 20);
				SpawnNPC(projectile.Center + new Vector2(267, -40), NPCID.RedSlime, 20);
				SpawnNPC(projectile.Center + new Vector2(-267, -40), NPCID.RedSlime, 20);
			}

			if (State == 2)
			{
				SpawnNPC(projectile.Center + new Vector2(110, 50), NPCID.RedSlime, 20);
				SpawnNPC(projectile.Center + new Vector2(-110, 50), NPCID.RedSlime, 20);
				SpawnNPC(projectile.Center + new Vector2(240, 40), NPCID.Skeleton, 20);
				SpawnNPC(projectile.Center + new Vector2(-240, 40), NPCID.Skeleton, 20);
				SpawnNPC(projectile.Center + new Vector2(0, -150), NPCID.CaveBat, 20);
			}

			if (State == 3)
			{
				SpawnNPC(projectile.Center + new Vector2(130, 40), NPCID.SkeletonArcher, 20, hpOverride: 0.2f, damageOverride: 0.2f, defenseOverride: 0f);
				SpawnNPC(projectile.Center + new Vector2(-130, 40), NPCID.SkeletonArcher, 20, hpOverride: 0.2f, damageOverride: 0.2f, defenseOverride: 0f);
				SpawnNPC(projectile.Center + new Vector2(140, -140), NPCID.CaveBat, 20);
				SpawnNPC(projectile.Center + new Vector2(-140, -140), NPCID.CaveBat, 20);
			}

			if (State == 4)
			{
				SpawnNPC(projectile.Center + new Vector2(130, 50), NPCID.Skeleton, 20);
				SpawnNPC(projectile.Center + new Vector2(-130, 50), NPCID.Skeleton, 20);

				SpawnNPC(projectile.Center + new Vector2(267, -40), NPCID.SkeletonArcher, 20, hpOverride: 0.2f, damageOverride: 0.2f, defenseOverride: 0f);
				SpawnNPC(projectile.Center + new Vector2(-267, -40), NPCID.SkeletonArcher, 20, hpOverride: 0.2f, damageOverride: 0.2f, defenseOverride: 0f);

				SpawnNPC(projectile.Center + new Vector2(70, -140), NPCID.CaveBat, 20);
				SpawnNPC(projectile.Center + new Vector2(-70, -140), NPCID.CaveBat, 20);
			}

			if (State == 5)
			{
				SpawnNPC(projectile.Center + new Vector2(130, 50), NPCID.SkeletonArcher, 20, hpOverride: 0.2f, damageOverride: 0.2f, defenseOverride: 0f);
				SpawnNPC(projectile.Center + new Vector2(-130, 50), NPCID.SkeletonArcher, 20, hpOverride: 0.2f, damageOverride: 0.2f, defenseOverride: 0f);

				SpawnNPC(projectile.Center + new Vector2(120, -160), NPCID.CaveBat, 20);
				SpawnNPC(projectile.Center + new Vector2(-120, -160), NPCID.CaveBat, 20);

				SpawnNPC(projectile.Center + new Vector2(220, -110), NPCID.CaveBat, 20);
				SpawnNPC(projectile.Center + new Vector2(-220, -110), NPCID.CaveBat, 20);
			}

			if (State == 6)
			{
				SpawnNPC(projectile.Center + new Vector2(130, 50), NPCID.Skeleton, 20);
				SpawnNPC(projectile.Center + new Vector2(-130, 50), NPCID.Skeleton, 20);

				SpawnNPC(projectile.Center + new Vector2(267, -50), NPCID.Skeleton, 20);
				SpawnNPC(projectile.Center + new Vector2(-267, -50), NPCID.Skeleton, 20);

				SpawnNPC(projectile.Center + new Vector2(0, -170), NPCID.Demon, 40, hpOverride: 2f, scale: 1.5f);
			}
		}

		private void SpawnNPC(Vector2 pos, int type, int dustAmount, float hpOverride = -1, float damageOverride = -1, float defenseOverride = -1, float scale = 1)
		{
			int i = Projectile.NewProjectile(pos, Vector2.Zero, ModContent.ProjectileType<SpawnEgg>(), 0, 0, Main.myPlayer,type, dustAmount);
			(Main.projectile[i].modProjectile as SpawnEgg).parent = this;
			(Main.projectile[i].modProjectile as SpawnEgg).hpOverride = hpOverride;
			(Main.projectile[i].modProjectile as SpawnEgg).damageOverride = damageOverride;
			(Main.projectile[i].modProjectile as SpawnEgg).defenseOverride = defenseOverride;
			Main.projectile[i].scale = scale;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			for(int k = 0; k < slaves.Count; k++)
			{
				var target = slaves[k];

				if (!target.active)
					continue;
				
				if(Main.rand.Next(2) == 0)
					Dust.NewDustPerfect(target.position + new Vector2(Main.rand.Next(target.width), Main.rand.Next(target.height)), ModContent.DustType<Dusts.Shadow>(), new Vector2(0, -Main.rand.NextFloat()), 0, Color.Black, Main.rand.NextFloat());

				var effect = Terraria.Graphics.Effects.Filters.Scene["Whitewash"].GetShader().Shader;

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, effect);

				spriteBatch.Draw(Main.npcTexture[target.type], target.Center + Vector2.UnitX * 2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(Main.npcTexture[target.type], target.Center + Vector2.UnitX * -2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(Main.npcTexture[target.type], target.Center + Vector2.UnitY * 2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(Main.npcTexture[target.type], target.Center + Vector2.UnitY * -2 - Main.screenPosition, target.frame, Color.White, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default);

				spriteBatch.Draw(Main.npcTexture[target.type], target.Center - Main.screenPosition, target.frame, Color.Black, target.rotation, target.frame.Size() / 2, target.scale, target.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (State != 0)
			{
				var tex = ModContent.GetTexture("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall");
				var origin = new Vector2(tex.Width / 2, tex.Height);
				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, 60), default, GetBeamColor(StarlightWorld.rottime), 0, origin, 3.5f, 0, 0);
				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(10, 60), default, GetBeamColor(StarlightWorld.rottime + 2) * 0.8f, 0, origin, 2.5f, 0, 0);
				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(-10, 60), default, GetBeamColor(StarlightWorld.rottime + 4) * 0.8f, 0, origin, 3.2f, 0, 0);

				float rad = -32;

				if (State >= maxWaves + 2)
					rad += Helpers.Helper.BezierEase((Timer - waveTime) / 128f) * 32;

				for (int k = 0; k < Math.Min(State - 2, maxWaves - 1); k++)
				{
					var tex2 = ModContent.GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
					spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, -44) + Vector2.UnitX.RotatedBy(k / (float)(maxWaves - 2) * 3.14f) * rad, default, new Color(255, 100, 100), 0, tex.Size() / 2, 0.3f, 0, 0);
					spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition + new Vector2(0, -32) + Vector2.UnitX.RotatedBy(k / (float)(maxWaves - 2) * 3.14f) * rad, default, new Color(255, 100, 100), 0, tex2.Size() / 2, 0.3f, 0, 0);
					spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition + new Vector2(0, -32) + Vector2.UnitX.RotatedBy(k / (float)(maxWaves - 2) * 3.14f) * rad, default, Color.White, 0, tex2.Size() / 2, 0.1f, 0, 0);
				}
			}
		}

		private Color GetBeamColor(float time)
		{
			var sin = (0.5f + (float)Math.Sin(time * 2 + 1) * 0.5f);
			var sin2 = (0.5f + (float)Math.Sin(time) * 0.5f);
			return new Color(255, (int)(50 * sin), 0) * sin2 * Windup;
        }
    }

    class SpawnEgg : ModProjectile, IDrawAdditive
    {
		public float hpOverride = -1;
		public float damageOverride = -1;
		public float defenseOverride = -1;

		public ref float SpawnType => ref projectile.ai[0];
		public ref float DustCount => ref projectile.ai[1];

		public CombatShrineDummy parent = null;

		public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
			projectile.width = 32;
			projectile.height = 32;
			projectile.timeLeft = 120;
			projectile.tileCollide = false;
			projectile.friendly = true;
        }

		public override void AI()
		{
			if(projectile.timeLeft == 30)
			{
				int i = NPC.NewNPC((int)projectile.Center.X, (int)projectile.Center.Y, (int)SpawnType);
				var npc = Main.npc[i];
				npc.alpha = 255;
				npc.GetGlobalNPC<StarlightNPC>().dontDropItems = true;

				if (hpOverride != -1) { npc.lifeMax = (int)(npc.lifeMax * hpOverride); npc.life = (int)(npc.life * hpOverride); }
				if (damageOverride != -1) npc.damage = (int)(npc.damage * damageOverride);
				if (defenseOverride != -1) npc.defense = (int)(npc.defense * defenseOverride);

				Helpers.Helper.PlayPitched("Magic/Shadow2", 1.1f, 1, projectile.Center);

				for (int k = 0; k < DustCount; k++)
				{
					Dust.NewDustPerfect(projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(1.5f, 2), 0, new Color(255, 100, 100), 0.2f);
				}

				parent?.slaves.Add(npc);
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			var tex = ModContent.GetTexture(AssetDirectory.GUI + "ItemGlow");
			var texRing = ModContent.GetTexture(AssetDirectory.GUI + "RingGlow");

			var bright = Helpers.Helper.BezierEase(1 - (projectile.timeLeft - 60) / 120f);

			if (projectile.timeLeft < 20)
				bright = projectile.timeLeft / 20f;

			float starScale = Helpers.Helper.BezierEase(1 - (projectile.timeLeft - 90) / 30f);

			if (projectile.timeLeft <= 90)
				starScale = 0.3f + (projectile.timeLeft / 90f) * 0.7f;

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.Red * bright, Helpers.Helper.BezierEase(projectile.timeLeft / 160f) * 6.28f, tex.Size() / 2, starScale * 0.3f * projectile.scale, 0, 0);
			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White * bright, Helpers.Helper.BezierEase(projectile.timeLeft / 160f) * 6.28f, tex.Size() / 2, starScale * 0.2f * projectile.scale, 0, 0);

			float ringBright = 1;
			if (projectile.timeLeft > 100)
				ringBright = (1 - ((projectile.timeLeft - 100) / 20f));

			float ringScale = 1 + ((projectile.timeLeft - 50) / 70f) * 0.3f;

			if (projectile.timeLeft <= 50)
				ringScale = Helpers.Helper.BezierEase((projectile.timeLeft - 20) / 30f);

			spriteBatch.Draw(texRing, projectile.Center - Main.screenPosition, null, Color.Red * ringBright * 0.8f, projectile.timeLeft / 60f * 6.28f, texRing.Size() / 2, ringScale * 0.2f * projectile.scale, 0, 0);
			spriteBatch.Draw(texRing, projectile.Center - Main.screenPosition, null, Color.White * ringBright * 0.5f, projectile.timeLeft / 60f * 6.28f, texRing.Size() / 2, ringScale * 0.195f * projectile.scale, 0, 0);

			if (projectile.timeLeft < 30)
			{
				var tex2 = ModContent.GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
				spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, null, new Color(255, 50, 50) * (projectile.timeLeft / 30f), 0, tex2.Size() / 2, (1 - projectile.timeLeft / 30f) * 7 * projectile.scale, 0, 0);
				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(255, 150, 150) * (projectile.timeLeft / 30f), 0, tex.Size() / 2, (1 - projectile.timeLeft / 30f) * 1 * projectile.scale, 0, 0);

				if (projectile.timeLeft > 15)
					spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(255, 100, 100) * ((projectile.timeLeft - 15) / 15f), 1.57f / 4, tex.Size() / 2, (1 - (projectile.timeLeft - 15) / 15f) * 2 * projectile.scale, 0, 0);
			}
		}
	}

}
