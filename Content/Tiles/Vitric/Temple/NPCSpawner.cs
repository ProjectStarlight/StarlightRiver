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

		public NPCSpawnerDummy() : base(ModContent.TileType<NPCSpawner>(), 32, 48) { }

		public override void Update()
		{
			if (active && timer < 60)
				timer++;

			if (!active && timer > 0)
				timer--;

			if (Main.player.Any(n => Vector2.Distance(n.Center, Projectile.Center) < 400))
			{
				active = true;
				SpawnEnemies((int)timer);
			}
			else
				active = false;
		}

		public void SpawnEnemies(int time)
		{

		}

		public override void PostDraw(Color lightColor)
		{
			var spriteBatch = Main.spriteBatch;
			var tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "NPCSpawnerGlow").Value;
			var frame = new Rectangle(0, (int)(1 + Helpers.Helper.SwoopEase(timer / 60f) * 18f) % 8 * 48, 22, 48);

			Color color = Color.Lerp(lightColor, Color.White, timer / 60f);

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, -12 * timer / 60f), frame, color, 0, new Vector2(11, 24), 1, 0, 0);
		}

	}
}
