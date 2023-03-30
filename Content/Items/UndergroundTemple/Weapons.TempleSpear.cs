using StarlightRiver.Content.Projectiles;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TempleSpear : ModItem
	{
		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Point of Light");
			Tooltip.SetDefault("Struck enemies glow\nSlain enemies leave behind a bright light");
		}

		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Melee;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 11;
			Item.crit = 10;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.knockBack = 2;
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ProjectileType<TempleSpearProjectile>();
			Item.shootSpeed = 1;
			Item.UseSound = SoundID.Item15;
		}
	}

	class TempleSpearProjectile : SpearProjectile
	{
		public TempleSpearProjectile() : base(30, 25, 100) { }

		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public override void PostAI()
		{
			//Dust effects
			var d = Dust.NewDustPerfect(Projectile.Center, 264, Projectile.velocity.RotatedBy(-0.5f));
			d.noGravity = true;
			d.color = new Color(255, 255, 200) * (Projectile.timeLeft / (30f * Main.player[Projectile.owner].GetTotalAttackSpeed(DamageClass.Melee)));
			d.scale = Projectile.timeLeft / (30f * Main.player[Projectile.owner].GetTotalAttackSpeed(DamageClass.Melee));

			d = Dust.NewDustPerfect(Projectile.Center, 264, Projectile.velocity.RotatedBy(0.5f));
			d.noGravity = true;
			d.color = new Color(255, 255, 200) * (Projectile.timeLeft / (30f * Main.player[Projectile.owner].GetTotalAttackSpeed(DamageClass.Melee)));
			d.scale = Projectile.timeLeft / (30f * Main.player[Projectile.owner].GetTotalAttackSpeed(DamageClass.Melee));
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			//inflicting debuff + light orbs on kill
			target.AddBuff(BuffType<Buffs.Illuminant>(), 600);
			if (damage >= target.life)
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, new Vector2(0, -1), ProjectileType<TempleSpearLight>(), 0, 0);
		}
	}

	class TempleSpearLight : ModProjectile
	{
		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = 3600;
		}

		public override void AI()
		{
			Projectile.velocity *= 0.99f;
			Lighting.AddLight(Projectile.Center, new Vector3(1, 1, 1) * Projectile.timeLeft / 3600f);
			var d = Dust.NewDustPerfect(Projectile.Center, 264);
			d.noGravity = true;
			d.color = new Color(255, 255, 200) * (Projectile.timeLeft / 3600f);
		}
	}
}