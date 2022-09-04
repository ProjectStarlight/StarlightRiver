using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	[Autoload(false)]
	internal class PetalPickupTile : ModTile
	{
		public string type;
		public string name;

		public override string Name => name;

		public override string Texture => AssetDirectory.Invisible;

		public PetalPickupTile() { }

		public PetalPickupTile(string name, string type)
		{
			this.name = name;
			this.type = type;
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			var realType = Mod.Find<ModProjectile>(type).Type;

			if (!Main.projectile.Any(n => n.active && n.Center == new Vector2(i, j) * 16 && n.type == realType))
				Projectile.NewProjectile(null, new Vector2(i, j) * 16, Vector2.Zero, realType, 1, 0, Main.myPlayer, 1);
		}
	}

	[Autoload(false)]
	internal class PetalPickupTileItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		protected override bool CloneNewInstances => true;

		public PetalPickupTileItem() { }

		public PetalPickupTileItem(string name, string type) : base(name, name, "Debug item", type) { }
	}

	internal abstract class PetalPickup : ModProjectile
	{
		public int value;
		public int frameCount;
		public Vector2 frameSize;
		public string internalName;

		public override string Name => internalName;

		public override string Texture => AssetDirectory.OvergrowTile + Name;

		public bool TileSpawned => Projectile.ai[0] == 1;

		public PetalPickup(string name, int value, int frameCount, Vector2 frameSize)
		{
			internalName = name;
			this.value = value;
			this.frameCount = frameCount;
			this.frameSize = frameSize;
		}

		public override void Load()
		{
			Mod.AddContent(new PetalPickupTile(internalName + "Tile", internalName));
			Mod.AddContent(new PetalPickupTileItem(internalName + "TileItem", internalName + "Tile"));
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.timeLeft = 2;
			Projectile.hostile = true;
		}

		public override void AI()
		{
			Projectile.timeLeft = 2;

			if (Main.rand.NextBool(10))
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY * -0.2f,  0, Color.White, 0.5f);
		}

		public override bool CanHitPlayer(Player target)
		{
			if (TileSpawned)
				WorldGen.KillTile((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16);

			Projectile.timeLeft = 0;

			for (int k = 0; k < value * 8; k++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(value, value) * Main.rand.NextFloat(), 0, Color.White);
			}

			CombatText.NewText(Projectile.Hitbox, new Color(255, 255, 100), $"+{value}");

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var tex = ModContent.Request<Texture2D>(Texture).Value;
			var frameIndex = (int)(Main.GameUpdateCount * 0.1f + Projectile.position.X) % frameCount;
			var frame = new Rectangle(0, (int)frameSize.Y * frameIndex, (int)frameSize.X, (int)frameSize.Y);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, 0, frame.Size() / 2, 1, 0, 0);

			return false;
		}
	}

	internal class SinglePetalPickup : PetalPickup 
	{
		public SinglePetalPickup() : base("SinglePetalPickup", 1, 7, new Vector2(24, 18)) { }
	}

	internal class FivePetalPickup : PetalPickup
	{
		public FivePetalPickup() : base("FivePetalPickup", 5, 7, new Vector2(32, 28)) { }
	}
}
