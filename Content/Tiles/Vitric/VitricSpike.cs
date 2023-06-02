using Terraria.ID;
using StarlightRiver.Content.Abilities;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricSpike : ModTile, IHintable
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			this.QuickSet(200, DustType<Dusts.GlassGravity>(), SoundID.Item27, new Color(95, 162, 138), -1, false, false, "");
			TileID.Sets.TouchDamageImmediate[Type] = 5;//vanilla contact damage
			TileID.Sets.DrawsWalls[Type] = true;
			Main.tileMerge[Type][Mod.Find<ModTile>("VitricSand").Type] = true;
			Main.tileMerge[Type][TileType<VitricGiantCrystal>()] = true;
			Main.tileMerge[Type][TileType<VitricMediumCrystal>()] = true;
			Main.tileMerge[Type][TileType<VitricLargeCrystal>()] = true;
			Main.tileMerge[Type][TileType<VitricSmallCrystal>()] = true;
		}

		public override bool IsTileDangerous(int i, int j, Player player)
		{
			return true;
		}

		public static void CollideWithSpikes(Entity entity, out int damage)
		{
			if (entity is NPC && ((entity as NPC).dontTakeDamage || (entity as NPC).immortal))
			{
				damage = 0;
				return;
			}

			damage = 0;
			System.Collections.Generic.List<Point> points = new();
			Collision.GetEntityEdgeTiles(points, entity);

			foreach (Point p in points)
			{
				if (!WorldGen.InWorld(p.X, p.Y))
					continue;

				// If any edge tiles are spikes, collide
				Tile tile = Framing.GetTileSafely(p);
				var vector = new Vector2();

				if (tile.HasTile && tile.TileType == TileType<VitricSpike>())
				{
					// ech, necessary
					if (p.X * 16 + 16 <= entity.TopLeft.X)
						vector.X += 1;
					if (p.X * 16 >= entity.TopRight.X)
						vector.X -= 1;
					if (p.Y * 16 + 16 <= entity.TopLeft.Y)
						vector.Y += 1;
					if (p.Y * 16 >= entity.TopRight.Y)
						vector.Y -= 1;
					// Damage
					damage = 25;
				}

				if (vector != default)
				{
					vector.Normalize();
					vector *= 6;
					entity.velocity.X = vector.X == 0 ? entity.velocity.X : vector.X;
					entity.velocity.Y = vector.Y == 0 ? entity.velocity.Y : vector.Y;
				}
			}
		}
		public string GetHint()
		{
			return "Dangerous.";
		}
	}

	class VitricSpikeItem : QuickTileItem
	{
		public VitricSpikeItem() : base("Vitric Spikes", "Ouch!", "VitricSpike", 0, AssetDirectory.VitricTile) { }
	}
}