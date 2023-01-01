using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Dungeon
{
	public class SkullBuster : ModItem
	{
		private int cooldown = 0;

		public override string Texture => AssetDirectory.DungeonItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Skullbuster");
			Tooltip.SetDefault("Right click to throw 4 skullbombs \nRelease right click to shoot them all in quick succession");
		}

		public override void SetDefaults()
		{
			Item.damage = 45;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 35;
			Item.useAnimation = 35;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.shoot = ModContent.ProjectileType<SkullBusterProj>();
			Item.shootSpeed = 12f;
			Item.useAmmo = AmmoID.Bullet;
			Item.autoReuse = true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<CoachGun>(), 1);
			recipe.AddIngredient(ItemID.Bone, 10);
			recipe.AddTile(TileID.Anvils);
		}

		public override void HoldItem(Player Player)
		{
			cooldown--;
		}

		public override bool CanUseItem(Player Player)
		{
			if (Player.altFunctionUse == 2)
			{
				Item.useStyle = ItemUseStyleID.Swing;
				Item.noUseGraphic = true;

				Item.useTime = 15;
				Item.useAnimation = 15;

				if (cooldown > 0)
					return false;
			}
			else
			{
				Item.useTime = 35;
				Item.useAnimation = 35;
				Item.useStyle = ItemUseStyleID.Shoot;
				Item.noUseGraphic = false;
			}

			return base.CanUseItem(Player);
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-6, 0);
		}

		public override bool AltFunctionUse(Player Player)
		{
			return true;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				Vector2 dir = Vector2.Normalize(velocity) * 9;
				velocity = dir;
				type = ModContent.ProjectileType<SkullBomb>();
			}
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				for (int i = 0; i < 4; i++)
				{
					var bomb = Projectile.NewProjectileDirect(source, position, velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.8f, 1.3f), type, damage, knockback, player.whoAmI);
					(bomb.ModProjectile as SkullBomb).crosshairSin = -i * 0.15f;
				}

				var proj = Projectile.NewProjectileDirect(source, position, Vector2.Zero, ModContent.ProjectileType<SkullBusterProj>(), damage, knockback, player.whoAmI);
				(proj.ModProjectile as SkullBusterProj).baseItem = Item;
				cooldown = 130;
			}
			else
			{
				float rot = velocity.ToRotation();
				float spread = 0.4f;

				Vector2 offset = new Vector2(1, -0.1f * player.direction).RotatedBy(rot);

				for (int k = 0; k < 15; k++)
				{
					Vector2 direction = offset.RotatedByRandom(spread);

					Dust.NewDustPerfect(position + offset * 43, ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(8), 125, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
				}

				Helper.PlayPitched("Guns/PlinkLever", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f), position);
				Helper.PlayPitched("Guns/RifleLight", 0.7f, Main.rand.NextFloat(-0.1f, 0.1f), position);
				Dust.NewDustPerfect(player.Center + offset * 43, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + offset.RotatedByRandom(spread) * 5, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));

				var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center + offset * 43, velocity * 2, type, damage, knockback, player.whoAmI);

				Projectile.NewProjectile(player.GetSource_ItemUse(Item), position + offset * 43, Vector2.Zero, ModContent.ProjectileType<CoachGunMuzzleFlash>(), 0, 0, player.whoAmI, rot);
			}

			return false;
		}
	}

	public class SkullBusterReload : ModProjectile
	{
		public override string Texture => AssetDirectory.DungeonItem + "SkullBusterReload";

		private Player owner => Main.player[Projectile.owner];

		private Vector2 direction = Vector2.One;

		private int frameCounter = 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Skull Buster");
			Main.projFrames[Projectile.type] = 22;
		}

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.damage = 0;
			Projectile.height = 2;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.timeLeft = 5;
			Projectile.friendly = false;
		}

		public override void AI()
		{
			Projectile.Center = owner.Center;
			Projectile.timeLeft = 2;

			direction = owner.DirectionTo(Main.MouseWorld);

			owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f);
			owner.heldProj = Projectile.whoAmI;
			owner.itemTime = owner.itemAnimation = 3;

			if (direction.X > 0)
				owner.direction = 1;
			else
				owner.direction = -1;

			frameCounter++;
			if (frameCounter % 4 == 0)
			{
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type])
					Projectile.active = false;

				if (Projectile.frame == 1)
				{
					for (int i = 0; i < 4; i++)
					{
						Vector2 casingOffset = new Vector2(1, -1 * owner.direction).RotatedBy(direction.ToRotation() - 0.6f);
						Gore.NewGore(Projectile.GetSource_FromThis(), owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f) + casingOffset * 6, new Vector2(owner.direction * -1, -0.5f) * 2, Mod.Find<ModGore>("CoachGunCasing").Type, 1f);
					}
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (frameCounter < 2)
				return false;

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			var frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

			var startOrigin = new Vector2(30, 66);
			var midOrigin = new Vector2(12, 50);
			var endOrigin = new Vector2(4, 60);

			Vector2 origin;
			if (Projectile.frame < 18)
				origin = Vector2.Lerp(startOrigin, midOrigin, Projectile.frame / 18f);
			else
				origin = Vector2.Lerp(midOrigin, endOrigin, EaseFunction.EaseQuadOut.Ease((Projectile.frame - 18) / 4f));

			SpriteEffects effects = SpriteEffects.None;
			float rot = direction.ToRotation();

			if (owner.direction != 1)
			{
				effects = SpriteEffects.FlipHorizontally;
				origin.X = tex.Width - origin.X;
				rot -= 3.14f;
			}

			Main.spriteBatch.Draw(tex, owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f) - Main.screenPosition, frame, lightColor, rot, origin, Projectile.scale, effects, 0f);

			return false;
		}
	}

	public class SkullBusterProj : ModProjectile
	{
		public Item baseItem = default;

		private bool releasingSmoke = false;

		private bool released = false;

		private int shootTimer = 0;

		private Vector2 direction = Vector2.One;

		private readonly List<Projectile> shotBombs = new();

		public override string Texture => AssetDirectory.DungeonItem + "SkullBuster";

		private Player Owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Skull Buster");
		}

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.damage = 0;
			Projectile.height = 2;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.timeLeft = 5;
			Projectile.friendly = false;
		}

		public override void AI()
		{
			Projectile.Center = Owner.Center;
			Owner.heldProj = Projectile.whoAmI;

			if (Main.mouseRight && !released || Owner.itemTime > 3)
			{
				Projectile.timeLeft = 30;
				if (Owner.itemTime < 3)
					Owner.itemTime = Owner.itemAnimation = 3;
			}
			else
			{
				released = true;

				if (shootTimer % 4 == 0)
				{
					Projectile targetBomb = Main.projectile.Where(x => x.active && x.owner == Owner.whoAmI && x.type == ModContent.ProjectileType<SkullBomb>() && !shotBombs.Contains(x)).OrderBy(n => n.Distance(Owner.Center)).FirstOrDefault();

					if (targetBomb != default)
					{
						Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f);
						float spread = 0.4f;
						Projectile.timeLeft = 30;

						direction = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f).DirectionTo(targetBomb.Center) * 20;

						if (direction.X > 0)
							Owner.direction = 1;
						else
							Owner.direction = -1;

						Vector2 offset = new Vector2(1, -0.3f * Owner.direction).RotatedBy(direction.ToRotation());

						Vector2 position = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f);

						int ammoType = -1;
						for (int i = 0; i < Owner.inventory.Length; ++i) //Consume ammo here so it's used when shot rather than when clicked
						{
							if (Owner.inventory[i].ammo == AmmoID.Bullet)
							{
								if (Owner.inventory[i].consumable && VanillaAmmoConsumption(Owner, Owner.inventory[i].ammo)) //Do not consume ammo if possible
								{
									Owner.inventory[i].stack--;

									if (Owner.inventory[i].stack <= 0)
										Owner.inventory[i].TurnToAir();
								}

								ammoType = Owner.inventory[i].shoot;
								break;
							}
						}

						if (ammoType != -1)
						{
							var proj = Projectile.NewProjectileDirect(Projectile.GetSource_ItemUse_WithPotentialAmmo(baseItem, AmmoID.Bullet), position + offset * 43, direction, ModContent.ProjectileType<SkullBusterBullet>(), 0, Projectile.knockBack, Owner.whoAmI);
							(proj.ModProjectile as SkullBusterBullet).target = targetBomb.whoAmI;
							shotBombs.Add(targetBomb);

							Vector2 gunTip = position + offset * 46;

							for (int k = 0; k < 15; k++)
							{
								Vector2 direction = offset.RotatedByRandom(spread);

								Dust.NewDustPerfect(gunTip, ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(8), 125, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
							}

							Helper.PlayPitched("Guns/PlinkLever", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f), position);
							Helper.PlayPitched("Guns/RifleLight", 0.7f, Main.rand.NextFloat(-0.1f, 0.1f), position);
							//Dust.NewDustPerfect(gunTip, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + offset.RotatedByRandom(spread) * 5, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));

							Projectile.NewProjectile(Projectile.GetSource_FromThis(), gunTip, Vector2.Zero, ModContent.ProjectileType<CoachGunMuzzleFlash>(), 0, 0, Owner.whoAmI, direction.ToRotation());
						}
					}
					else
					{
						if (shootTimer == 0)
							Projectile.active = false;
						else
							releasingSmoke = true;
					}
				}

				if (releasingSmoke)
				{
					Vector2 offset = new Vector2(1, -0.3f * Owner.direction).RotatedBy(direction.ToRotation());
					Vector2 position = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f);
					Vector2 gunTip = position + offset * 46;
					Dust.NewDustPerfect(gunTip, ModContent.DustType<Smoke>(), 4 * offset, 0, new Color(60, 55, 50) * 0.5f * (Projectile.timeLeft / 30f), 0.15f);
				}

				if (direction.X > 0)
					Owner.direction = 1;
				else
					Owner.direction = -1;
				bool facingRight = Owner.direction == 1;
				Owner.itemTime = Owner.itemAnimation = 2;
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f);
				shootTimer++;
			}
		}

		public override void Kill(int timeLeft)
		{
			Helper.PlayPitched("Guns/RevolvingReload", 0.6f, 0, Owner.Center);
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center, Vector2.Zero, ModContent.ProjectileType<SkullBusterReload>(), 0, 0, Owner.whoAmI);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (released)
			{
				float rot = direction.ToRotation();
				Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
				var origin = new Vector2(10, tex.Height * 0.75f);
				SpriteEffects effects = SpriteEffects.None;
				if (Owner.direction != 1)
				{
					rot += 3.14f;
					effects = SpriteEffects.FlipHorizontally;
					origin.X = tex.Width - origin.X;
				}

				Main.spriteBatch.Draw(tex, Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f) - Main.screenPosition, null, lightColor, rot, origin, Projectile.scale, effects, 0f);
			}

			return false;
		}

		public static bool VanillaAmmoConsumption(Player p, int ammo)
		{
			float chance = 0;

			static float CombineChances(float p1, float p2)
			{
				return p1 + p2 - p1 * p2;
			}

			if (p.ammoBox) //1/5 chance to reduce
				chance = 0.2f;

			if (p.ammoCost75) //1/4 chance to reduce
				chance = CombineChances(chance, 0.25f);

			if (p.ammoCost80) //1/5 chance to reduce
				chance = CombineChances(chance, 0.2f);

			if (p.ammoPotion) //1/5 chance to reduce
				chance = CombineChances(chance, 0.2f);

			if (ammo == AmmoID.Arrow && p.magicQuiver) //1/5 chance to reduce for arrows only
				chance = CombineChances(chance, 0.2f);

			if (p.armor[0].type == ItemID.ChlorophyteHelmet) //1/5 chance to reduce -- seems unique??
				chance = CombineChances(chance, 0.2f);

			return Main.rand.NextFloat(1f) > chance;
		}
	}

	public class SkullBomb : ModProjectile
	{
		private bool shot = false;

		private float crosshairRotation = 0f;

		public float crosshairSin = 0f;

		public override string Texture => AssetDirectory.DungeonItem + Name;

		private Player Owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Skull Bomb");
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Shuriken);
			Projectile.width = 18;
			Projectile.damage = 0;
			Projectile.height = 18;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.timeLeft = 150;
			Projectile.aiStyle = 14;
			Projectile.friendly = false;
		}

		public override void AI()
		{
			if (crosshairSin < 1f)
				crosshairSin += 0.025f;

			crosshairSin = MathHelper.Min(crosshairSin, 1);
			crosshairRotation += 0.05f * crosshairSin;
			float progress = 1 - Projectile.timeLeft / 150f;
			for (int i = 0; i < 2; i++)
			{
				var sparks = Dust.NewDustPerfect(Projectile.Center + (Projectile.rotation - 1.57f).ToRotationVector2() * 12, ModContent.DustType<CoachGunSparks>(), (Projectile.rotation + Main.rand.NextFloat(-0.6f, 0.6f)).ToRotationVector2() * Main.rand.NextFloat(0.4f, 1.2f));
				sparks.fadeIn = progress * 45;
			}

			var Hitbox = new Rectangle((int)Projectile.Center.X - 50, (int)Projectile.Center.Y - 50, 100, 100);
			IEnumerable<Projectile> list = Main.projectile.Where(x => x.Hitbox.Intersects(Hitbox));
			foreach (Projectile proj in list)
			{
				if (proj.type == ModContent.ProjectileType<SkullBusterBullet>() && (proj.ModProjectile as SkullBusterBullet).target == Projectile.whoAmI && Projectile.timeLeft > 2 && proj.active && proj.velocity.Length() > 1)
				{
					shot = true;
					Projectile.timeLeft = 2;
					proj.velocity = Vector2.Zero;
				}
			}
		}

		public override void Kill(int timeLeft)
		{
			CameraSystem.shake += 3;

			for (int i = 0; i < 3; i++)
			{
				var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(7, 7), ProjectileID.Bone, Projectile.damage / 2, Projectile.knockBack, Owner.whoAmI);
				proj.friendly = true;
				proj.hostile = false;
				proj.scale = 0.75f;
			}

			for (int i = 0; i < 10; i++)
			{
				var dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<SkullbusterDust>());
				dust.velocity = Main.rand.NextVector2Circular(5, 5);
				dust.scale = Main.rand.NextFloat(1f, 1.5f);
				dust.alpha = Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}

			for (int i = 0; i < 10; i++)
			{
				var dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<SkullbusterDustShrinking>());
				dust.velocity = Main.rand.NextVector2Circular(5, 5);
				dust.scale = Main.rand.NextFloat(1f, 1.5f);
				dust.alpha = Main.rand.Next(80);
				dust.rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<SkullbusterDustGlow>()).scale = 0.9f;
			}

			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SkullbusterSkull>(), Projectile.damage, 0, Owner.whoAmI);
			for (int i = 0; i < 10; i++)
			{
				Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
				var dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16) + vel * Main.rand.Next(70), 0, 0, ModContent.DustType<SkullbusterDustFastSlowdown>());
				dust.velocity = vel * Main.rand.Next(5);
				dust.scale = Main.rand.NextFloat(0.25f, 0.5f);
				dust.alpha = Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}

			for (int i = 0; i < 15; i++)
			{
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<Content.Dusts.WaterBubble>(), Main.rand.NextVector2Circular(2, 2), 0, new Color(160, 180, 255), Main.rand.NextFloat(0.6f, 0.8f));
			}

			for (int i = 0; i < 13; i++)
			{
				Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
				Dust.NewDustPerfect(Projectile.Center + new Vector2(0, 30) + vel * 5, ModContent.DustType<Dusts.BuzzSpark>(), vel * Main.rand.NextFloat(2, 10), 0, Color.Aqua, 1.5f);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D whiteTex = ModContent.Request<Texture2D>(Texture + "_White").Value;
			Texture2D crosshairTex = ModContent.Request<Texture2D>(Texture + "_Crosshair").Value;

			float progress = 1 - Projectile.timeLeft / 150f;
			Color overlayColor = Color.White;
			if (progress < 0.5f)
				overlayColor = Color.Lerp(new Color(0, 0, 0, 0), Color.Gray * 0.5f, progress * 2);
			else
				overlayColor = Color.Lerp(Color.Gray * 0.5f, Color.White, (progress - 0.5f) * 2);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(whiteTex, Projectile.Center - Main.screenPosition, null, overlayColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			if (crosshairSin > 0)
			{
				for (int i = 0; i < 4; i++)
				{
					float rot = i / 4f * 6.28f;

					float ease = EaseFunction.EaseQuinticIn.Ease(crosshairSin);
					Vector2 origin = crosshairTex.Size() * (1.75f + 0.25f * (float)Math.Cos(ease * 3.14f));
					Main.spriteBatch.Draw(crosshairTex, Projectile.Center - Main.screenPosition, null, Color.Red * crosshairSin, crosshairRotation + rot, origin, 1, SpriteEffects.None, 0f);
				}
			}

			return false;
		}
	}

	internal class SkullbusterSkull : ModProjectile, IDrawAdditive
	{
		private float fadeIn = 0;

		private float fadeOut = 1;

		private int skullNumber = 1;

		public override string Texture => AssetDirectory.DungeonItem + "SkullBusterSkull";

		private float Radius => 100 * fadeIn;

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 50;
			skullNumber = Main.rand.Next(1, 4);
			Projectile.rotation = Main.rand.NextFloat(-0.2f, 0.2f);
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Skullbomb");
		}

		public override void AI()
		{
			if (fadeIn < 1)
			{
				fadeIn += 0.1f;
			}
			else
			{
				Projectile.friendly = false;
				fadeOut -= 0.05f;
			}

			if (fadeOut <= 0)
				Projectile.active = false;
			//ManageCaches();
			//ManageTrail();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius;

			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
				return true;

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public void DrawAdditive(SpriteBatch sb)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture + skullNumber.ToString()).Value;
			float opacity = fadeOut;
			float scale = fadeIn + 0.25f * (1 - fadeOut);
			sb.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Aqua * opacity, Projectile.rotation, tex.Size() / 2, scale, SpriteEffects.None, 0f);
		}
	}

	public class SkullBusterBullet : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public int target = -1;

		public override string Texture => AssetDirectory.BreacherItem + "ExplosiveFlare";

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 100;
			Projectile.extraUpdates = 4;
			Projectile.alpha = 255;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bullet");
			Main.projFrames[Projectile.type] = 2;
		}

		public override void AI()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 15; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 15)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(40 * 4), factor => factor * 3, factor => new Color(255, 170, 80) * factor.X * (Projectile.timeLeft / 100f));

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

			trail?.Render(effect);
		}
	}
}