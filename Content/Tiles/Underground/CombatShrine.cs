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
		private List<NPC> slaves = new List<NPC>();

		public int maxWaves = 7;

		public ref float Timer => ref projectile.ai[0];
		public ref float State => ref projectile.ai[1];

		public CombatShrineDummy() : base(ModContent.TileType<CombatShrine>(), 3 * 16, 6 * 16) { }

		public override void Update()
		{
			if (State > 0)
			{
				Dust.NewDustPerfect(projectile.Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(2), 0, new Color(255, 40 + Main.rand.Next(50), 75), 0.2f);

				Timer++;

				if(State == maxWaves + 2)
				{
					if (Timer >= 128)
					{
						for (int k = 0; k < 30; k++)
							Dust.NewDustPerfect(projectile.Center + new Vector2(0, -32), ModContent.DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, new Color(255, 100, 100), 0.6f);

						Main.NewText("Ejaculatory Dysfunction!  Final time: " + Helpers.Helper.TicksToTime((int)Timer));
						State = 0;
					}
					return;
				}

				if (!slaves.Any(n => n.active))
				{
					SpawnWave();
					State++;
				}

				if(State == maxWaves + 2)	
					Timer = 0;
			}
		}

		private void SpawnWave()
		{
			for(int k = 0; k < 20; k++)
				Dust.NewDustPerfect(projectile.Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(5), 0, new Color(255, Main.rand.Next(50), 0), 0.5f);

			if (State == 1)
			{
				SpawnNPC(projectile.Center + Vector2.UnitX * 100, NPCID.BlueSlime, 50);
				SpawnNPC(projectile.Center + Vector2.UnitX * -100, NPCID.BlueSlime, 50);
			}

			if (State == 2)
			{
				SpawnNPC(projectile.Center + Vector2.UnitX * 100, NPCID.Skeleton, 50);
				SpawnNPC(projectile.Center + Vector2.UnitX * -100, NPCID.Skeleton, 50);
			}

			if (State >= 3 && State < maxWaves + 1)
			{
				SpawnNPC(projectile.Center + Vector2.UnitX * 50, NPCID.Skeleton, 50);
				SpawnNPC(projectile.Center + Vector2.UnitX * -50, NPCID.Skeleton, 50);
				SpawnNPC(projectile.Center + Vector2.UnitX * 80, NPCID.BlueSlime, 50);
				SpawnNPC(projectile.Center + Vector2.UnitX * -80, NPCID.BlueSlime, 50);

				SpawnNPC(projectile.Center + Vector2.UnitX * 100, NPCID.BlackSlime, 100);
				SpawnNPC(projectile.Center + Vector2.UnitX * -100, NPCID.BlackSlime, 100);
			}
		}

		private void SpawnNPC(Vector2 pos, int type, int dustAmount)
		{
			int i = NPC.NewNPC((int)pos.X, (int)pos.Y, type);
			var npc = Main.npc[i];
			npc.alpha = 255;
			npc.GetGlobalNPC<StarlightNPC>().dontDropItems = true;

			slaves.Add(npc);
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
			if (State > 0)
			{
				var tex = ModContent.GetTexture("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall");
				var origin = new Vector2(tex.Width / 2, tex.Height);
				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, 60), default, GetBeamColor(StarlightWorld.rottime), 0, origin, 3.5f, 0, 0);
				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(10, 60), default, GetBeamColor(StarlightWorld.rottime + 2) * 0.8f, 0, origin, 2.5f, 0, 0);
				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(-10, 60), default, GetBeamColor(StarlightWorld.rottime + 4) * 0.8f, 0, origin, 3.2f, 0, 0);

				float rad = -32;

				if (State >= maxWaves + 2)
					rad += Helpers.Helper.BezierEase(Timer / 128f) * 32;

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
			return new Color(255, (int)(50 * sin), 0) * sin2;
		}
	}
}
