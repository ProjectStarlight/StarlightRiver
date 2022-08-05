using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using StarlightRiver.Content.NPCs.Vitric.Gauntlet;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class NPCSpawner : DummyTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override int DummyType => ModContent.ProjectileType<NPCSpawnerDummy>();

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 2, 3, 0, SoundID.Tink, new Color(255, 255, 255));
		}
	}

	internal class NPCSpawnerItem : QuickTileItem
	{
		public NPCSpawnerItem() : base("NPC Spawner", "", "NPCSpawner", 1, AssetDirectory.VitricTile, false) { }
	}

	internal class NPCSpawnerDummy : Dummy
	{
		public bool active;

		public ref float timer => ref Projectile.ai[0];
		public ref float spawned => ref Projectile.ai[1];

		public NPCSpawnerDummy() : base(ModContent.TileType<NPCSpawner>(), 32, 48) { }

		public override void Update()
		{
			if (spawned >= 600)
				active = false;

			if (active && timer < 60)
				timer++;

			if (!active)
			{
				if (timer > 0)
					timer--;

				if (spawned > 0)
					spawned--;
			}

			if (Main.player.Any(n => Vector2.Distance(n.Center, Projectile.Center) < 600) && (active || spawned <= 0))
			{
				var nearby = Main.npc.Count(n => n.active && Vector2.Distance(n.Center, Projectile.Center) < 600);

				if (nearby > 4)
				{
					spawned = 300;
					return;
				}

				active = true;
				SpawnEnemies((int)Main.GameUpdateCount);
			}
			else
				active = false;

			Color color = Color.Lerp(new Color(255, 100, 40), new Color(255, 160, 100), timer / 60f);
			var glowOpacity = (timer / 60f) * 0.75f + (0.2f + (float)(Math.Sin(3.14f + Main.GameUpdateCount / 60f * 6.28f)) * 0.2f);

			Lighting.AddLight(Projectile.Center, (color * glowOpacity).ToVector3());
		}

		public void SpawnEnemies(int time)
		{
			if (time % 60 == 0)
			{
				int monster;
				switch(Main.rand.Next(3))
				{
					case 0: monster = ModContent.NPCType<GruntConstruct>(); break;
					case 1: monster = ModContent.NPCType<PelterConstruct>(); break;
					case 2: monster = ModContent.NPCType<ShieldConstruct>(); break;
					default: monster = ModContent.NPCType<GruntConstruct>(); break;
				}

				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitY.RotatedByRandom(1) * -5, ModContent.ProjectileType<GauntletSpawner>(), 1, 0, Main.myPlayer, monster);

				spawned += 150;
			}
		}

		public override void PostDraw(Color lightColor)
		{
			var spriteBatch = Main.spriteBatch;
			var tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "NPCSpawnerGlow").Value;
			var frame = new Rectangle(0, (int)(1 + Helpers.Helper.SwoopEase(timer / 60f) * 18f) % 8 * 48, 22, 48);
			var pos = Projectile.Center - Main.screenPosition + new Vector2(0, -12 * timer / 60f);

			Color color = Color.Lerp(lightColor, Color.White, timer / 60f);

			spriteBatch.Draw(tex, pos, frame, color, 0, new Vector2(11, 24), 1, 0, 0);

			var glowTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;
			var glowColor = new Color(255, 160, 100);
			glowColor.A = 0;

			var glowOpacity = (timer / 60f) * 0.5f + (float)(Math.Sin(3.14f + Main.GameUpdateCount / 60f * 6.28f)) * 0.5f;

			Main.spriteBatch.Draw(glowTex, pos, null, glowColor * glowOpacity, 0, glowTex.Size() / 2, 0.8f, 0, 0);
		}

	}
}
