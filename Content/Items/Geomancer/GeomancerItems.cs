using Terraria.ID;

namespace StarlightRiver.Content.Items.Geomancer
{
	public abstract class GeoGem : ModItem
	{
		public override string Texture => AssetDirectory.GeomancerItem + Name;

		protected abstract float Rotation { get; }

		protected abstract int ProjectileType { get; }

		protected abstract StoredGem GemType { get; }

		public override void SetStaticDefaults()
		{
			SetName();
			Tooltip.SetDefault("You shouldn't see this");
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.maxStack = 1;
		}

		public override bool ItemSpace(Player Player)
		{
			return true;
		}

		public override bool OnPickup(Player Player)
		{
			GeomancerPlayer modPlayer = Player.GetModPlayer<GeomancerPlayer>();
			if (!modPlayer.GetIsStored(GemType) && modPlayer.activeGem != StoredGem.All)
			{
				if (Player.whoAmI == Main.myPlayer)
					Projectile.NewProjectile(null, Player.Center, Vector2.Zero, ProjectileType, 0, 0, Player.whoAmI, Rotation);

				SetBonus(Player);
			}

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab, Player.position);
			return false;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return new Color(200, 200, 200, 100);
		}

		protected virtual void SetName() { }

		protected virtual void SetBonus(Player Player) { }
	}

	public class GeoDiamond : GeoGem
	{
		protected override float Rotation => 1.046f;

		protected override int ProjectileType => ModContent.ProjectileType<GeoDiamondProj>();

		protected override StoredGem GemType => StoredGem.Diamond;

		protected override void SetName()
		{
			DisplayName.SetDefault("Diamond");
		}
	}

	public class GeoRuby : GeoGem
	{
		protected override float Rotation => 1.046f * 2;

		protected override int ProjectileType => ModContent.ProjectileType<GeoRubyProj>();

		protected override StoredGem GemType => StoredGem.Ruby;

		protected override void SetName()
		{
			DisplayName.SetDefault("Ruby");
		}
	}

	public class GeoEmerald : GeoGem
	{
		protected override float Rotation => 1.046f * 3;

		protected override int ProjectileType => ModContent.ProjectileType<GeoEmeraldProj>();

		protected override StoredGem GemType => StoredGem.Emerald;

		protected override void SetName()
		{
			DisplayName.SetDefault("Emerald");
		}

		protected override void SetBonus(Player Player)
		{
			int healAmount = (int)MathHelper.Min(Player.statLifeMax2 - Player.statLife, 20);
			Player.HealEffect(20);
			Player.statLife += healAmount;
		}
	}

	public class GeoSapphire : GeoGem
	{
		protected override float Rotation => 1.046f * 4;

		protected override int ProjectileType => ModContent.ProjectileType<GeoSapphireProj>();

		protected override StoredGem GemType => StoredGem.Sapphire;

		protected override void SetName()
		{
			DisplayName.SetDefault("Sapphire");
		}

		protected override void SetBonus(Player Player)
		{
			int healAmount = (int)MathHelper.Min(Player.statManaMax2 - Player.statMana, 200);
			Player.ManaEffect(healAmount);
			Player.statMana += healAmount;
		}
	}

	public class GeoTopaz : GeoGem
	{
		protected override float Rotation => 1.046f * 5;

		protected override int ProjectileType => ModContent.ProjectileType<GeoTopazProj>();

		protected override StoredGem GemType => StoredGem.Topaz;

		protected override void SetName()
		{
			DisplayName.SetDefault("Topaz");
		}
	}

	public class GeoAmethyst : GeoGem
	{
		protected override float Rotation => 1.046f * 6;

		protected override int ProjectileType => ModContent.ProjectileType<GeoAmethystProj>();

		protected override StoredGem GemType => StoredGem.Amethyst;

		protected override void SetName()
		{
			DisplayName.SetDefault("Amethyst");
		}
	}
}