using StarlightRiver.Content.Projectiles;
using Terraria.Audio;
using Terraria.DataStructures;
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
			Tooltip.SetDefault("Hold <left> to charge a powerful spear\nFires a laser when fully charged");
		}

		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Melee;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 11;
			Item.crit = 10;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 90;
			Item.useAnimation = 90;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.knockBack = 2f;
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ProjectileType<TempleSpearProjectile>();
			Item.shootSpeed = 1;
			Item.channel = true;
		}
	}

	class TempleSpearProjectile : ModProjectile
	{
		public int maxCharge;

		public bool stabbing;

		public Vector2 offset;

		public Vector2? OwnerMouse
		{
			get
			{
				if (Main.myPlayer == Projectile.owner)
					return Main.MouseWorld;

				return null;
			}
		}

		public ref float Timer => ref Projectile.ai[0];

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Point of Light");
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Melee;
		}

		public override void OnSpawn(IEntitySource source)
		{
			maxCharge = (int)(Owner.HeldItem.useAnimation * (1f / Owner.GetTotalAttackSpeed(DamageClass.Melee)));
			maxCharge = Utils.Clamp(maxCharge, 15, 90);
		}

		public override void AI()
		{
			if (Owner.HeldItem.ModItem is not TempleSpear)
			{
				Projectile.Kill();
				return;
			}

			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;

			Projectile.velocity = Vector2.Normalize(Projectile.velocity);

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (!Owner.channel && !stabbing && Timer >= 15)
			{
				stabbing = true;
				Timer = 0;
				Projectile.timeLeft = 25;
			}


			if (!stabbing)
			{
				Projectile.velocity = Owner.DirectionTo(OwnerMouse.Value);

				Projectile.timeLeft = 2;

				Timer++;
				
				if (Timer < 15) // Pullback
					offset = Vector2.Lerp(Vector2.Zero, new Vector2(-50, 0), Timer / 15f);
				else if (Timer < maxCharge + 15) //charging
				{
					offset = new Vector2(-50, 0);
				}
				else if (Timer == maxCharge + 15)
					SoundEngine.PlaySound(SoundID.MaxMana, Owner.Center);		
			}
			else
			{
				Timer++;

				if (Timer < 20)
					offset = Vector2.Lerp(new Vector2(-50, 0), new Vector2(100, 0), Timer / 20f);
			}

			Projectile.Center = Owner.MountedCenter + offset.RotatedBy(Projectile.rotation - MathHelper.PiOver2);

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.Pi);
			Owner.ChangeDir(Projectile.direction);
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
