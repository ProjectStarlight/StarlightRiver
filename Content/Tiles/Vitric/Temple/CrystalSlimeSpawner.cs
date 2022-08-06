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
using StarlightRiver.Content.NPCs.Vitric;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class CrystalSlimeSpawner : DummyTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override int DummyType => ModContent.ProjectileType<CrystalSlimeSpawnerDummy>();

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 5, 3, 0, SoundID.Tink, new Color(255, 255, 255));
		}
	}

	internal class CrystalSlimeSpawnerItem : QuickTileItem
	{
		public CrystalSlimeSpawnerItem() : base("NPC Spawner", "", "CrystalSlimeSpawner", 1, AssetDirectory.VitricTile, false) { }
	}

	internal class CrystalSlimeSpawnerDummy : Dummy
	{
		public bool active;

		public ref float timer => ref Projectile.ai[0];
		public ref float spawned => ref Projectile.ai[1];

		public CrystalSlimeSpawnerDummy() : base(ModContent.TileType<CrystalSlimeSpawner>(), 32, 48) { }

		public override void Update()
		{
			if (timer <= 0 && Main.player.Any(n => Vector2.Distance(n.Center, Projectile.Center) < 300))
			{
				active = true;
				timer = 1200;
			}

			if (timer > 0)
				timer--;

			if (active && !Main.npc.Any(n => n.active && n.type == ModContent.NPCType<CrystalSlime>()))
			{
				NPC.NewNPC(Projectile.GetSource_FromThis(), (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<CrystalSlime>(), 0, 0, 1);

				for(int k = 0; k < 20; k++)
				{
					Dust.NewDust(Projectile.Center, 20, 20, ModContent.DustType<Dusts.Cinder>());
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			var spriteBatch = Main.spriteBatch;

		}
	}
}
