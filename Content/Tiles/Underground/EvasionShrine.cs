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
	class EvasionShrine : DummyTile
	{
		public override int DummyType => ModContent.ProjectileType<EvasionShrineDummy>();

		public override bool Autoload(ref string name, ref string texture)
		{
			texture = "StarlightRiver/Assets/Tiles/Underground/EvasionShrine";
			return true;
		}

		public override void SetDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 3, 6, DustID.Stone, SoundID.Tink, false, new Color(100, 100, 100), false, false, "Mysterious Shrine");
		}

		public override bool NewRightClick(int i, int j)
		{
			var tile = (Tile)(Framing.GetTileSafely(i, j).Clone());

			if ((Dummy.modProjectile as EvasionShrineDummy).State == 0)
			{
				for (int x = 0; x < 3; x++)
					for (int y = 0; y < 6; y++)
					{
						int realX = x + i - tile.frameX / 18;
						int realY = y + j - tile.frameY / 18;

						Framing.GetTileSafely(realX, realY).frameX += 3 * 18;
					}

				(Dummy.modProjectile as EvasionShrineDummy).State = 1;
				return true;
			}

			return false;
		}
	}

	class EvasionShrineDummy : Dummy, IDrawAdditive
	{
		public int maxTime = 3600;

		public ref float Timer => ref projectile.ai[0];
		public ref float State => ref projectile.ai[1];

		public float Windup => Math.Min(1, Timer / 120f);

		public EvasionShrineDummy() : base(ModContent.TileType<EvasionShrine>(), 3 * 16, 6 * 16) { }

		public override void Update()
		{
			if (State == 0 && Parent.frameX > 3 * 18)
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
				(mod as StarlightRiver).useIntenseMusic = true;
				Dust.NewDustPerfect(projectile.Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(2), 0, new Color(50, 30, 205) * Windup, 0.2f);
				SpawnObstacles((int)Timer);
			}
		}

		public void SpawnObstacles(int timer)
		{

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
			}
		}

		private Color GetBeamColor(float time)
		{
			var sin = (0.5f + (float)Math.Sin(time * 2 + 1) * 0.5f);
			var sin2 = (0.5f + (float)Math.Sin(time) * 0.5f);
			return new Color(80 + (int)(50 * sin), 60, 255) * sin2 * Windup;
		}
	}
}
