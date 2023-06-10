using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.NPCs.Vitric.Gauntlet;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	internal class NPCSpawner : DummyTile, IHintable
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override int DummyType => ModContent.ProjectileType<NPCSpawnerDummy>();

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(2, 3, 0, SoundID.Tink, new Color(255, 255, 255));
		}
		public string GetHint()
		{
			return "Dangerous.";
		}
	}

	[SLRDebug]
	internal class NPCSpawnerItem : QuickTileItem
	{
		public NPCSpawnerItem() : base("NPC Spawner", "", "NPCSpawner", 1, AssetDirectory.VitricTile, false) { }
	}

	internal class NPCSpawnerDummy : Dummy
	{
		public bool active;

		public ref float Timer => ref Projectile.ai[0];
		public ref float Spawned => ref Projectile.ai[1];

		public NPCSpawnerDummy() : base(ModContent.TileType<NPCSpawner>(), 32, 48) { }

		public override void Update()
		{
			if (Spawned >= 600)
				active = false;

			if (active && Timer < 60)
				Timer++;

			if (!active)
			{
				if (Timer > 0)
					Timer--;

				if (Spawned > 0)
					Spawned--;
			}

			if (Main.player.Any(n => Vector2.Distance(n.Center, Projectile.Center) < 300) && (active || Spawned <= 0))
			{
				int nearby = Main.npc.Count(n => n.active && Vector2.Distance(n.Center, Projectile.Center) < 600);

				if (nearby > 4)
				{
					Spawned = 300;
					return;
				}

				active = true;
				SpawnEnemies((int)Main.GameUpdateCount);
			}
			else
			{
				active = false;
			}

			var color = Color.Lerp(new Color(255, 100, 40), new Color(255, 160, 100), Timer / 60f);
			float glowOpacity = Timer / 60f * 0.75f + (0.2f + (float)Math.Sin(3.14f + Main.GameUpdateCount / 60f * 6.28f) * 0.2f);

			Lighting.AddLight(Projectile.Center, (color * glowOpacity).ToVector3());
		}

		public void SpawnEnemies(int time)
		{
			if (time % 60 == 0)
			{
				int monster = Main.rand.Next(3) switch
				{
					0 => ModContent.NPCType<GruntConstruct>(),
					1 => ModContent.NPCType<PelterConstruct>(),
					2 => ModContent.NPCType<ShieldConstruct>(),
					_ => ModContent.NPCType<GruntConstruct>(),
				};
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitY.RotatedByRandom(1) * -5, ModContent.ProjectileType<ConstructSpawner>(), 1, 0, Main.myPlayer, monster);

				Spawned += 150;
			}
		}

		public override void PostDraw(Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "NPCSpawnerGlow").Value;
			var frame = new Rectangle(0, (int)(1 + Helpers.Helper.SwoopEase(Timer / 60f) * 18f) % 8 * 48, 22, 48);
			Vector2 pos = Projectile.Center - Main.screenPosition + new Vector2(0, -12 * Timer / 60f);

			var color = Color.Lerp(lightColor, Color.White, Timer / 60f);

			spriteBatch.Draw(tex, pos, frame, color, 0, new Vector2(11, 24), 1, 0, 0);

			Texture2D glowTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;
			var glowColor = new Color(255, 160, 100)
			{
				A = 0
			};

			float glowOpacity = Timer / 60f * 0.5f + (float)Math.Sin(3.14f + Main.GameUpdateCount / 60f * 6.28f) * 0.5f;

			Main.spriteBatch.Draw(glowTex, pos, null, glowColor * glowOpacity, 0, glowTex.Size() / 2, 0.8f, 0, 0);
		}
	}
}