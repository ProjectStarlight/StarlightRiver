using Terraria.ID;

namespace StarlightRiver.Content.Dusts
{
	public class BloodMetaballDust : ModDust
	{
		public override string Texture => AssetDirectory.Assets + "Invisible";

		public override void OnSpawn(Dust dust)
		{
			dust.noLight = true;
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;

			dust.customData ??= Main.rand.NextFloat(0.75f, 1.5f);

			if (dust.noGravity)
			{
				dust.velocity = new Vector2(0, -1f);
			}
			else
			{
				dust.velocity.Y += 0.2f;

				if (dust.position.X > 16 && dust.position.Y > 16)
				{
					Tile tile = Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16];

					if (tile.HasTile && tile.BlockType == BlockType.Solid && Main.tileSolid[tile.TileType])
					{
						dust.scale *= 1.03f;
						dust.velocity *= -0.5f;
					}
				}
			}

			dust.rotation = dust.velocity.ToRotation() + 1.57f;
			dust.scale *= 0.96f;

			if (dust.noGravity)
				dust.scale *= 0.96f;

			if (dust.scale < 0.05f)
				dust.active = false;

			return false;
		}
	}

	public class BloodMetaballDustLight : BloodMetaballDust { }
}
