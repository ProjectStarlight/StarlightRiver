using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.ExposureSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
namespace StarlightRiver.Content.Items.Vitric
{
	public class CoachGunUpgrade : ModItem
	{
		public float shootRotation;
		public int shootDirection;

		public bool justAltUsed; // to prevent custom recoil anim

		private int cooldown = 0;

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void Load()
		{
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricItem + "CoachGunUpgradeCasing");
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magmatic Coach Gun");
			Tooltip.SetDefault("Press <right> to throw out crystal bombs, which explode in chain reactions \n" +
				"Firing at a bomb detonates it and inflicts {{BUFF:SwelteredDeBuff}}\n" +
				"'How does he manage to corenuke like that?'");
		}

		public override void SetDefaults()
		{
			Item.damage = 40;
			Item.knockBack = 1f;
			Item.DamageType = DamageClass.Ranged;
			Item.useTime = Item.useAnimation = 30;
			Item.autoReuse = true;
			Item.noMelee = true;

			Item.width = Item.height = 32;
			Item.useStyle = ItemUseStyleID.Shoot;

			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 2, silver: 75);

			Item.shoot = ModContent.ProjectileType<CoachGunUpgradeBomb>();
			Item.shootSpeed = 18.5f;
			Item.useAmmo = AmmoID.Bullet;
		}

		public override void AddRecipes()
		{
			CreateRecipe().
				AddIngredient(ModContent.ItemType<CoachGun>()).
				AddIngredient(ModContent.ItemType<MagmaCore>()).
				AddIngredient(ItemID.HellstoneBar, 12).
				AddTile(TileID.Anvils).
				Register();
		}

		public override void HoldItem(Player player)
		{
			cooldown--;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-15, 0);
		}

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				Item.useStyle = ItemUseStyleID.Swing;
				Item.noUseGraphic = true;

				Item.useTime = Item.useAnimation = 15;

				if (player.ownedProjectileCounts[ModContent.ProjectileType<CoachGunUpgradeBomb>()] >= 3 || cooldown > 0)
					return false;

				justAltUsed = true;
			}
			else
			{
				Item.useStyle = ItemUseStyleID.Shoot;
				Item.noUseGraphic = false;

				Item.useTime = Item.useAnimation = 30;
				justAltUsed = false;
			}

			shootRotation = (player.Center - Main.MouseWorld).ToRotation();
			shootDirection = (Main.MouseWorld.X < player.Center.X) ? -1 : 1;

			return base.CanUseItem(player);
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				velocity *= 0.8f;
				type = ModContent.ProjectileType<CoachGunUpgradeBomb>();
			}
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			if (justAltUsed)
				return;

			Vector2 itemPosition = CommonGunAnimations.SetGunUseStyle(player, Item, shootDirection, -6f, new Vector2(68f, 22f), new Vector2(-48f, 0f));

			float animProgress = 1f - player.itemTime / (float)player.itemTimeMax;

			if (animProgress >= 0.35f)
			{
				float lerper = (animProgress - 0.35f) / 0.65f;
				Dust.NewDustPerfect(itemPosition + new Vector2(55f, -2f * player.direction).RotatedBy(player.compositeFrontArm.rotation + 1.5707964f * player.gravDir), DustID.Wraith, Vector2.UnitY * -2f, (int)MathHelper.Lerp(210f, 200f, lerper), default, MathHelper.Lerp(0.75f, 1f, lerper)).noGravity = true;

				if (Main.rand.NextBool(20))
					Dust.NewDustPerfect(itemPosition + new Vector2(55f, -2f * player.direction).RotatedBy(player.compositeFrontArm.rotation + 1.5707964f * player.gravDir), ModContent.DustType<CoachGunUpgradeSmokeDust>(), Vector2.Zero, 150, new Color(255, 150, 50), 0.075f).noGravity = true;
			}
		}

		public override void UseItemFrame(Player player)
		{
			if (justAltUsed)
				return;

			CommonGunAnimations.SetGunUseItemFrame(player, shootDirection, shootRotation, -0.2f, true);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				cooldown = 30;
			}
			else
			{
				Vector2 barrelPos = position + new Vector2(60f, -6f * player.direction).RotatedBy(velocity.ToRotation());

				for (int i = 0; i < 8; i++)
				{
					Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), DustID.Torch, velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f), 0, default, 1.2f).noGravity = true;

					Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedEmber>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f), 0, new Color(255, 125, 0, 0), 0.15f).customData = -player.direction;

					Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedEmber>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f), 0, new Color(255, 80, 0, 0), 0.15f).customData = -player.direction;

					Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedGlow>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f), 0, new Color(255, 50, 0, 0), 0.35f).noGravity = true;
				}

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<CoachGunUpgradeSmokeDust>(), velocity * 0.025f, 75, new Color(255, 130, 0), 0.1f);

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<CoachGunUpgradeSmokeDust>(), velocity * 0.05f, 75, new Color(255, 130, 0), 0.15f);

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<CoachGunUpgradeSmokeDust>(), velocity * Main.rand.NextFloat(0.075f, 0.125f) - Vector2.UnitY * Main.rand.NextFloat(0.5f, 1.5f), 100, new Color(255, 150, 50), 0.125f).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<CoachGunUpgradeSmokeDust>(), velocity * Main.rand.NextFloat(0.075f, 0.125f) - Vector2.UnitY * Main.rand.NextFloat(0.5f, 2f), 100, new Color(255, 150, 50), 0.1f).noGravity = true;

				Dust.NewDustPerfect(barrelPos, ModContent.DustType<CoachGunUpgradeMuzzleFlashDust>(), Vector2.Zero, 0, default, 1f).rotation = velocity.ToRotation();

				Vector2 shellPos = player.Center + new Vector2(10f, -4f * player.direction).RotatedBy(velocity.ToRotation());

				Gore.NewGoreDirect(source, shellPos, new Vector2(player.direction * -1, -0.5f) * 2, Mod.Find<ModGore>("CoachGunUpgradeCasing").Type, 1f).timeLeft = 60;

				Dust.NewDustPerfect(shellPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<CoachGunUpgradeSmokeDust>(), new Vector2(player.direction * -1, -0.5f), 30, new Color(255, 130, 0), 0.05f);

				for (int i = 0; i < 4; i++)
				{
					Dust.NewDustPerfect(shellPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedEmber>(), new Vector2(player.direction * -1, -0.5f).RotatedByRandom(0.4f) * Main.rand.NextFloat(2f), 0, new Color(255, 125, 0, 0), 0.15f).customData = player.direction;

					Dust.NewDustPerfect(shellPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedEmber>(), new Vector2(player.direction * -1, -0.5f).RotatedByRandom(0.4f) * Main.rand.NextFloat(2f), 0, new Color(255, 80, 0, 0), 0.15f).customData = player.direction;

					Dust.NewDustPerfect(shellPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedGlow>(), new Vector2(player.direction * -1, -0.5f).RotatedByRandom(0.4f) * Main.rand.NextFloat(4f), 0, new Color(255, 150, 0, 0), 0.35f);
				}

				SoundHelper.PlayPitched("Guns/PlinkLever", 0.45f, Main.rand.NextFloat(-0.1f, 0.1f), position);
				SoundHelper.PlayPitched("Guns/RifleLight", 0.75f, Main.rand.NextFloat(-0.1f, 0.1f), position);
				Projectile.NewProjectileDirect(source, position, velocity * 1.4f, type, damage, knockback, player.whoAmI).GetGlobalProjectile<CoachGunUpgradeGlobalProj>().ShotFromGun = true;

				CameraSystem.shake += 1;
				Item.noUseGraphic = true;

				return false;
			}

			return true;
		}
	}

	public class CoachGunUpgradeGlobalProj : GlobalProjectile //this is needed cause otherwise every projectile could break the crystal, only bullets fired from the gun should break the crystal
	{
		public override bool InstancePerEntity => true;

		public bool ShotFromGun = false;

		public IEntitySource entitySource;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			entitySource = source;
		}
	}

	internal class CoachGunUpgradeBomb : ModProjectile
	{
		float VeloMult;

		bool shrapnel = false;
		bool beenTouched = false; //kinda bad name but I could really think of a better one, basically if its been touched by a bullet or explosion, so that the explosion effects dont keep happening if the explosion is still intersecting the projectile, which made things like the projectile doing millions of damage

		public override string Texture => AssetDirectory.VitricBoss + "VitricBomb";

		public override void Load()
		{
			for (int i = 1; i < 5; i++)
			{
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricItem + Name + "_Gore" + i);
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crystal Bomb");
			Main.projFrames[Projectile.type] = 8;
		}

		public override void SetDefaults()
		{
			Projectile.width = Projectile.height = 32;

			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = false;
			Projectile.timeLeft = 240;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0f && Main.myPlayer == Projectile.owner)
			{
				float dist = Vector2.Distance(Projectile.Center, Main.MouseWorld);
				VeloMult = 0.95f + dist / 15000;
				VeloMult = MathHelper.Clamp(VeloMult, 0.95f, 0.98f);
				Projectile.localAI[0] += 1f;
			}

			if (++Projectile.frameCounter >= 8 - 240 / Projectile.timeLeft)
			{
				Projectile.frameCounter = 0;

				if (++Projectile.frame >= Main.projFrames[Projectile.type])
					Projectile.frame = 0;
			}

			Projectile.velocity *= VeloMult;

			Projectile.rotation += 0.2f * (Projectile.velocity.X * 0.05f);

			IEnumerable<Projectile> list = Main.projectile.Where(x => x.Distance(Projectile.Center) < 50f || x.Hitbox.Intersects(Projectile.Hitbox));

			foreach (Projectile proj in list)
			{
				if ((proj.GetGlobalProjectile<CoachGunUpgradeGlobalProj>().ShotFromGun || proj.type == ModContent.ProjectileType<CoachGunUpgradeExplosion>()) && Projectile.timeLeft > 2 && proj.active && !beenTouched)
				{
					if (proj.GetGlobalProjectile<CoachGunUpgradeGlobalProj>().ShotFromGun)
					{
						if (Projectile.timeLeft > 195)
							ExplodeIntoShards(proj.velocity * 1.85f);

						proj.penetrate--;
						Projectile.Kill();
					}

					if (proj.type == ModContent.ProjectileType<CoachGunUpgradeExplosion>())
					{
						if (Projectile.localAI[0] == 0f)
						{
							Projectile.damage = (int)(Projectile.damage * 1.5f);
							Projectile.localAI[0] = 1f;
						}

						Projectile.timeLeft = 10;
					}

					beenTouched = true;
				}
			}

			if (Main.rand.NextBool(23))
			{
				if (Main.rand.NextBool())
					Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CrystalSparkle>(), 0, 0);
				else if (Main.rand.NextBool())
					Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CrystalSparkle2>(), 0, 0);
				else
					Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<FireSparkle>(), 0, 0);
			}
		}

		public override void OnKill(int timeLeft)
		{
			SoundHelper.PlayPitched("GlassMiniboss/GlassSmash", 0.5f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.position);

			for (int i = 1; i < 5; i++)
			{
				Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0.5f, Mod.Find<ModGore>(Name + "_Gore" + i).Type).timeLeft = 90;
			}

			CameraSystem.shake += 10;
			if (!shrapnel)
			{
				if (Main.myPlayer == Projectile.owner)
					Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CoachGunUpgradeExplosion>(), (int)(Projectile.damage * 1.35f), 0f, Projectile.owner, 75);

				if (Main.myPlayer == Projectile.owner)
				{
					for (int k = 0; k < 5; k++)
					{
						Vector2 vel = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.4f, 0.5f);
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Main.rand.NextVector2Circular(100, 100), vel, ModContent.ProjectileType<NeedlerEmber>(), 0, 0);
					}
				}

				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(60, 60), ModContent.DustType<Glow>(),
						Main.rand.NextVector2Circular(5, 5), 0, new Color(255, 150, 50), 0.95f);

					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(60, 60), ModContent.DustType<CoachGunDust>(),
						Main.rand.NextVector2Circular(10, 10), 70 + Main.rand.Next(60), default, Main.rand.NextFloat(1.5f, 1.9f)).rotation = Main.rand.NextFloat(6.28f);

					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(60, 60), ModContent.DustType<CoachGunDustTwo>(),
						Main.rand.NextVector2Circular(10, 10), 80 + Main.rand.Next(40), default, Main.rand.NextFloat(1.5f, 1.9f)).rotation = Main.rand.NextFloat(6.28f);

					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(60, 60), ModContent.DustType<CoachGunDustGlow>()).scale = 0.9f;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			Texture2D texture = Assets.Bosses.VitricBoss.VitricBomb.Value;

			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			int startY = frameHeight * Projectile.frame;

			var sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
			Vector2 origin = sourceRectangle.Size() / 2f;

			float offsetX = 20f;
			origin.X = Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX;

			Main.EntitySpriteDraw(texture,
				Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
				sourceRectangle, Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			SoundHelper.PlayPitched("GlassMiniboss/GlassBounce", 0.15f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.position);

			if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
				Projectile.velocity.X = -oldVelocity.X;

			if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
				Projectile.velocity.Y = -oldVelocity.Y;

			return false;
		}

		public void ExplodeIntoShards(Vector2 velocity)
		{
			shrapnel = true;

			if (Main.myPlayer == Projectile.owner)
			{
				for (int i = 0; i < 6; i++)
				{
					Vector2 vel = velocity.RotatedByRandom(MathHelper.ToRadians(8f)) * Main.rand.NextFloat(0.8f, 1.1f);
					Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, vel * 1.45f, ModContent.ProjectileType<CoachGunUpgradeShards>(), (int)(Projectile.damage * 0.33f), 0.5f, Projectile.owner);
				}

				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CoachGunUpgradeExplosion>(), (int)(Projectile.damage * 1.3f), 0f, Projectile.owner, 40, 1f);
			}

			for (int i = 0; i < 5; i++)
			{
				Vector2 vel = velocity.RotatedByRandom(MathHelper.ToRadians(5f)) * Main.rand.NextFloat(0.2f, 0.4f);
				Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.width, ModContent.DustType<VitricBombDust>(), vel.X, vel.Y, 0, default, Main.rand.NextFloat(0.7f, 0.9f)).noGravity = true;

				for (int d = 0; d < 2; d++)
				{
					Vector2 velo = velocity.RotatedByRandom(MathHelper.ToRadians(12f)) * Main.rand.NextFloat(0.25f, 0.45f);
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Glow>(), velo, 0, new Color(255, 150, 50), 0.85f);
				}

				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<CoachGunDust>(), velocity.RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(0.3f, 0.50f)).scale = 0.8f;
			}

			for (int i = 0; i < 8; i++)
			{
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<Glow>(),
					Main.rand.NextVector2Circular(4, 4), 0, new Color(255, 150, 50), 0.95f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDust>(),
					Main.rand.NextVector2Circular(8, 8), 70 + Main.rand.Next(60), default, Main.rand.NextFloat(1.5f, 1.9f)).rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustTwo>(),
					Main.rand.NextVector2Circular(8, 8), 80 + Main.rand.Next(40), default, Main.rand.NextFloat(1.5f, 1.9f)).rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustGlow>()).scale = 0.9f;
			}

			for (int i = 0; i < 15; i++)
			{
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), ModContent.DustType<Glow>(),
					velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.2f, 0.5f), 0, new Color(255, 150, 50), 0.95f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), ModContent.DustType<CoachGunDust>(),
					velocity.RotatedByRandom(MathHelper.ToRadians(40f)) * Main.rand.NextFloat(0.7f, 1.8f), 70 + Main.rand.Next(60), default, Main.rand.NextFloat(1.5f, 1.9f)).rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), ModContent.DustType<CoachGunDustTwo>(),
					velocity.RotatedByRandom(MathHelper.ToRadians(40f)) * Main.rand.NextFloat(0.7f, 1.8f), 80 + Main.rand.Next(40), default, Main.rand.NextFloat(1.5f, 1.9f)).rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), ModContent.DustType<CoachGunDustGlow>(),
					velocity.RotatedByRandom(MathHelper.ToRadians(35f)) * Main.rand.NextFloat(0.15f, 0.35f)).scale = 0.9f;
			}
		}
	}

	public class VitricBombDust : ModDust
	{
		public override string Texture => AssetDirectory.Dust + Name;

		public override void OnSpawn(Dust dust)
		{
			dust.noLight = false;
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.rotation += 0.12f;
			dust.scale *= 0.98f;
			if (dust.scale <= 0.2)
				dust.active = false;

			if (!dust.noGravity)
				dust.velocity.Y += 0.15f;

			return false;
		}
	}

	class SwelteredDeBuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public SwelteredDeBuff() : base("Sweltered", "Deals 10 damage per second\nDamage taken increased by 35%", true) { }

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.GetGlobalNPC<StarlightNPC>().DoT += 10;
			npc.GetGlobalNPC<ExposureNPC>().ExposureMultAll += 0.35f;
			Vector2 vel = new Vector2(0, -1).RotatedByRandom(0.5f) * 0.4f;

			if (Main.rand.NextBool(4))
				Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<CoachSmoke>(), vel.X, vel.Y, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));

			if (Main.rand.NextBool(2))
				Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<Dusts.Glow>(), 0, 0, 0, Color.DarkOrange, 0.6f);
		}
	}

	public class CoachGunUpgradeMuzzleFlashDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			dust.alpha += 20;
			dust.alpha = (int)(dust.alpha * 1.05f);

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = Assets.Items.Vitric.CoachGunUpgradeMuzzleFlashDust.Value;
			Texture2D texBlur = Assets.Items.Vitric.CoachGunUpgradeMuzzleFlashDust_Blur.Value;
			Texture2D texGlow = Assets.Items.Vitric.CoachGunUpgradeMuzzleFlashDust_Glow.Value;
			Texture2D bloomTex = Assets.Masks.GlowAlpha.Value;

			Main.spriteBatch.Draw(bloomTex, dust.position - Main.screenPosition, null, new Color(255, 75, 0, 0) * 0.25f * lerper, dust.rotation, bloomTex.Size() / 2f, dust.scale * 1.25f, 0f, 0f);

			Main.spriteBatch.Draw(texGlow, dust.position - Main.screenPosition, null, new Color(255, 75, 0, 0) * lerper, dust.rotation, texGlow.Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.White * lerper, dust.rotation, tex.Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.Draw(texBlur, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * 0.5f * lerper, dust.rotation, texBlur.Size() / 2f, dust.scale, 0f, 0f);

			return false;
		}
	}

	public class CoachGunUpgradeSmokeDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
			dust.customData = 1 + Main.rand.Next(3);
			dust.rotation = Main.rand.NextFloat(6.28f);
		}

		public override bool Update(Dust dust)
		{
			dust.position.Y -= 0.1f;
			if (dust.noGravity)
				dust.position.Y -= 0.5f;

			dust.position += dust.velocity;

			if (!dust.noGravity)
			{
				dust.velocity *= 0.99f;
			}
			else
			{
				dust.velocity *= 0.975f;
				dust.velocity.X *= 0.99f;
			}

			dust.rotation += dust.velocity.Length() * 0.01f;

			if (dust.noGravity)
				dust.alpha += 2;
			else
				dust.alpha += 5;

			dust.alpha = (int)(dust.alpha * 1.005f);

			if (!dust.noGravity)
				dust.scale *= 1.02f;
			else
				dust.scale *= 0.99f;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = Assets.SmokeTransparent_1.Value;
			if ((int)dust.customData == 2)
				tex = Assets.SmokeTransparent_2.Value;
			if ((int)dust.customData == 3)
				tex = Assets.SmokeTransparent_3.Value;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () => Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null,
				Color.Lerp(dust.color, Color.Black, 1f - lerper) * lerper, dust.rotation, tex.Size() / 2f, dust.scale, 0f, 0f));

			return false;
		}
	}
}