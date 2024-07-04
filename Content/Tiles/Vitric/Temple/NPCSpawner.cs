using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.NPCs.Vitric.Gauntlet;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	internal class NPCSpawner : DummyTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override int DummyType => DummySystem.DummyType<NPCSpawnerDummy>();

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(2, 3, 0, SoundID.Tink, new Color(255, 255, 255));
		}
	}

	[SLRDebug]
	internal class NPCSpawnerItem : QuickTileItem
	{
		public NPCSpawnerItem() : base("NPC Spawner", "", "NPCSpawner", 1, AssetDirectory.VitricTile, false) { }
	}

	internal class NPCSpawnerDummy : Dummy
	{
		public bool spawnerActive;

		public float timer;
		public float spawned;

		public NPCSpawnerDummy() : base(ModContent.TileType<NPCSpawner>(), 32, 48) { }

		public override void Update()
		{
			if (spawned >= 600)
				spawnerActive = false;

			if (spawnerActive && timer < 60)
				timer++;

			if (!spawnerActive)
			{
				if (timer > 0)
					timer--;

				if (spawned > 0)
					spawned--;
			}

			if (Main.player.Any(n => Vector2.Distance(n.Center, Center) < 300 && !n.GetHandler().Unlocked<Dash>()) && (spawnerActive || spawned <= 0))
			{
				int nearby = Main.npc.Count(n => n.active && Vector2.Distance(n.Center, Center) < 600);

				if (nearby > 4)
				{
					spawned = 300;
					return;
				}

				spawnerActive = true;
				SpawnEnemies((int)Main.GameUpdateCount);
			}
			else
			{
				spawnerActive = false;
			}

			var color = Color.Lerp(new Color(255, 100, 40), new Color(255, 160, 100), timer / 60f);
			float glowOpacity = timer / 60f * 0.75f + (0.2f + (float)Math.Sin(3.14f + Main.GameUpdateCount / 60f * 6.28f) * 0.2f);

			Lighting.AddLight(Center, (color * glowOpacity).ToVector3());
		}

		public void SpawnEnemies(int time)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient && time % 60 == 0)
			{
				int monster = Main.rand.Next(3) switch
				{
					0 => ModContent.NPCType<GruntConstruct>(),
					1 => ModContent.NPCType<PelterConstruct>(),
					2 => ModContent.NPCType<ShieldConstruct>(),
					_ => ModContent.NPCType<GruntConstruct>(),
				};
				Projectile.NewProjectile(GetSource_FromThis(), Center, Vector2.UnitY.RotatedByRandom(1) * -5, ModContent.ProjectileType<ConstructSpawner>(), 1, 0, Main.myPlayer, monster);

				spawned += 150;
			}
		}

		public override void PostDraw(Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			Texture2D tex = Assets.Tiles.Vitric.NPCSpawnerGlow.Value;
			var frame = new Rectangle(0, (int)(1 + Helpers.Helper.SwoopEase(timer / 60f) * 18f) % 8 * 48, 22, 48);
			Vector2 pos = Center - Main.screenPosition + new Vector2(0, -12 * timer / 60f);

			var color = Color.Lerp(lightColor, Color.White, timer / 60f);

			spriteBatch.Draw(tex, pos, frame, color, 0, new Vector2(11, 24), 1, 0, 0);

			Texture2D glowTex = Assets.Keys.GlowAlpha.Value;
			var glowColor = new Color(255, 160, 100)
			{
				A = 0
			};

			float glowOpacity = timer / 60f * 0.5f + (float)Math.Sin(3.14f + Main.GameUpdateCount / 60f * 6.28f) * 0.5f;

			Main.spriteBatch.Draw(glowTex, pos, null, glowColor * glowOpacity, 0, glowTex.Size() / 2, 0.8f, 0, 0);
		}
	}
}