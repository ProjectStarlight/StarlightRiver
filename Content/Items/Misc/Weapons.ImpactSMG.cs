using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class ImpactSMG : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Impact SMG");

			Tooltip.SetDefault("Hold <left> to rapidly fire high impact bullets, heating up over time\n" +
				"Release to boomerang the gun\n" +
				"Boomeranging while its not overheated grants you a stacking buff to the Impact SMG's damage\n" +
				"Boomeranging while its overheated causes it to consume all stacks for a deadly explosion");
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 2f;
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ModContent.ProjectileType<ImpactSMGHoldout>();
			Item.shootSpeed = 1f;
			Item.noUseGraphic = true;
			Item.channel = true;
			Item.useAmmo = AmmoID.Bullet;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile.NewProjectile(source, position, velocity, Item.shoot, damage, knockback, player.whoAmI);
			return false;
		}

		public override bool CanUseItem(Player Player)
		{
			return Player.ownedProjectileCounts[ModContent.ProjectileType<ImpactSMGHoldout>()] <= 0;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 8);
			recipe.AddIngredient(ItemID.IllegalGunParts);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class ImpactSMGPlayer : ModPlayer //TODO: Rework into stackable buff
	{
		public int stacks;

		public int flashTimer;

		public override void ResetEffects()
		{
			if (flashTimer > 0)
				flashTimer--;

			stacks = Utils.Clamp(stacks, 0, 5);
		}
	}

	public class ImpactSMGBuff : ModBuff
	{
		public override string Texture => AssetDirectory.MiscItem + Name + "_Back";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Heated Up!");
			Description.SetDefault("");
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.GetModPlayer<ImpactSMGPlayer>().stacks > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}

		public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
		{
			rare = ItemRarityID.Red;
			tip = "The Impact SMG deals " + Main.LocalPlayer.GetModPlayer<ImpactSMGPlayer>().stacks * 5 + "% " + "increased damage";
		}

		public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
		{
			Texture2D backTex = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + Name + "_Back").Value;
			Texture2D frontTex = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + Name + "_Front").Value;
			Texture2D frontTexGlow = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + Name + "_Front_Glow").Value;

			ImpactSMGPlayer mp = Main.LocalPlayer.GetModPlayer<ImpactSMGPlayer>();

			float progress = mp.stacks / 5f;

			var color = Color.Lerp(Color.Transparent, new Color(255, 50, 50, 0), progress);

			spriteBatch.Draw(backTex, drawParams.Position, drawParams.SourceRectangle, drawParams.DrawColor, 0f, default, 1f, 0f, 0f);

			spriteBatch.Draw(frontTexGlow, drawParams.Position, drawParams.SourceRectangle, color, 0f, default, 1f, 0f, 0f);
			spriteBatch.Draw(frontTex, drawParams.Position, frontTex.Frame(verticalFrames: 5, frameY: mp.stacks - 1), drawParams.DrawColor, 0f, default, 1f, 0f, 0f);

			return false;
		}
	}

	public class ImpactSMGHoldout : ModProjectile
	{
		public int shootDelay;
		public int shots;
		public int heat;

		public int flashTimer;
		public int throwTimer;
		public int explodingTimer;

		public int catchDelay;

		public bool hasHit;

		public bool flashed;

		public bool draw; //only draw two ticks after spawning

		public bool thrown;
		public bool throwing;

		public bool exploding;
		public bool exploded;

		public Vector2 mousePos;
		public Vector2 mouseWorld;

		public bool updateVelo = true;

		public bool CanHold => Owner.channel && !Owner.CCed && !Owner.noItems;

		public bool Overheated => shots >= 30;

		public ref float ShootDelay => ref Projectile.ai[0];

		public ref float MaxShootDelay => ref Projectile.ai[1];

		private Player Owner => Main.player[Projectile.owner];

		public ImpactSMG Holdout => Owner.HeldItem.ModItem as ImpactSMG;

		public override string Texture => AssetDirectory.MiscItem + "ImpactSMG";

		public override bool? CanDamage()
		{
			return thrown;
		}

		public override void Load()
		{
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Shell");
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Impact SMG");
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Ranged;

			Projectile.width = 52;
			Projectile.height = 34;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			Vector2 armPos = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
			armPos += Utils.SafeNormalize(Projectile.velocity, Vector2.UnitX) * 15f;

			if (MaxShootDelay == 0f)
				MaxShootDelay = CombinedHooks.TotalUseTime(Owner.HeldItem.useTime, Owner, Owner.HeldItem);

			if (Overheated)
				MaxShootDelay = CombinedHooks.TotalUseTime(Owner.HeldItem.useTime, Owner, Owner.HeldItem) * 1.2f;

			if (flashTimer > 0)
				flashTimer--;

			if (catchDelay > 0)
				catchDelay--;

			if (!CanHold)
			{
				if (shots < 10)
				{
					Projectile.Kill();
					return;
				}

				if (Overheated)
					ExplodingAI();
				else
					BoomerangAI();

				throwing = true;
			}
			else if (!throwing)
			{
				shootDelay++;

				if (shootDelay > 2)
					draw = true;

				if (shootDelay % (int)MaxShootDelay == 0)
					ShootBullet(armPos);

				Owner.ChangeDir(Projectile.direction);

				Owner.heldProj = Projectile.whoAmI;
				Owner.itemTime = 2;
				Owner.itemAnimation = 2;

				Projectile.timeLeft = 2;
				Projectile.rotation = Utils.ToRotation(Projectile.velocity);
				Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

				Projectile.position = armPos - Projectile.Size * 0.5f;

				Projectile.spriteDirection = Projectile.direction;

				if (Main.myPlayer == Projectile.owner && updateVelo)
				{
					updateVelo = false;
					float interpolant = Utils.GetLerpValue(1f, 5f, Projectile.Distance(Main.MouseWorld), true);
					Vector2 oldVelocity = Projectile.velocity;

					Projectile.velocity = Vector2.One.RotatedBy(Vector2.Lerp(Projectile.velocity, Owner.DirectionTo(Main.MouseWorld), interpolant).ToRotation() - MathHelper.PiOver4).RotatedByRandom(0.1f);

					if (Projectile.velocity != oldVelocity)
					{
						Projectile.netSpam = 0;
						Projectile.netUpdate = true;
					}
				}

				if (Overheated && !flashed)
				{
					SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
					flashTimer = 20;
					flashed = true;
				}
			}
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			width = 12;
			height = 12;

			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (thrown)
			{
				if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
					Projectile.velocity.X = -oldVelocity.X;

				if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
					Projectile.velocity.Y = -oldVelocity.Y;

				Projectile.velocity.Y -= 5f;

				Projectile.velocity *= 0.45f;

				for (int k = 0; k < 10; k++)
				{
					Dust.NewDustPerfect(Projectile.Center + new Vector2(0f, 40f), ModContent.DustType<Dusts.BuzzSpark>(), Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(-0.4f, -0.1f), 0, new Color(255, 255, 60) * 0.8f, 1.1f);
				}

				Helper.PlayPitched("Impacts/Clink", 0.50f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.position);

				catchDelay = 15;

				if (!exploding)
					shootDelay = 15;
			}

			return false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Owner.GetModPlayer<ImpactSMGPlayer>().stacks > 0)
				modifiers.SourceDamage *= 1f + Owner.GetModPlayer<ImpactSMGPlayer>().stacks * 0.05f;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Overheated && !exploding && !exploded)
			{
				Helper.PlayPitched("Magic/FireCast", 0.75f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);
				ShootDelay = 0;
				Projectile.friendly = false;
				Projectile.velocity *= -0.25f;
				Projectile.velocity.Y -= 3.5f;
				exploding = true;
				Projectile.tileCollide = true;
			}
			else if (!Overheated && thrown)
			{
				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowFastDecelerate>(), Projectile.velocity.RotatedByRandom(0.45f) * Main.rand.NextFloat(0.5f), 0, new Color(190, 50, 50), 0.35f);
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowFastDecelerate>(), Projectile.velocity.RotatedByRandom(0.55f) * Main.rand.NextFloat(0.5f), 0, new Color(255, 40, 40), 0.4f);
				}

				if (ShootDelay < 25)
				{
					Projectile.velocity.Y -= 1.5f;
					Projectile.velocity *= -0.45f;
					ShootDelay = 25;
				}

				hasHit = true;
			}
		}

		public override void Kill(int timeLeft)
		{
			if (!Overheated)
			{
				if (hasHit)
				{
					ImpactSMGPlayer mp = Owner.GetModPlayer<ImpactSMGPlayer>();

					if (mp.stacks < 5)
						mp.stacks++;

					if (!Owner.HasBuff<ImpactSMGBuff>())
						Owner.AddBuff(ModContent.BuffType<ImpactSMGBuff>(), 120);

					mp.flashTimer = 15;

					for (int i = 0; i < 15; i++)
					{
						Dust.NewDustPerfect(Main.GetPlayerArmPosition(Projectile) + Main.rand.NextVector2Circular(10f, 10f), ModContent.DustType<GlowFastDecelerate>(), Vector2.Zero, 0, new Color(255, 50, 50), 0.4f);
					}
				}
				else
				{
					Owner.GetModPlayer<ImpactSMGPlayer>().stacks = 0;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!draw)
				return false;

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D whiteTex = ModContent.Request<Texture2D>(Texture + "_White").Value;
			Texture2D flareTex = ModContent.Request<Texture2D>(Texture + "Flare").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Texture2D glowMap = ModContent.Request<Texture2D>(Texture + "_GlowMap").Value;
			Texture2D bloomMap = ModContent.Request<Texture2D>(Texture + "_BloomMap").Value;

			SpriteEffects flip = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : 0f;

			if (Owner.GetModPlayer<ImpactSMGPlayer>().stacks > 0)
			{
				float progress = Owner.GetModPlayer<ImpactSMGPlayer>().stacks / 5f;
				Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(180, 0, 0, 0), progress), Projectile.rotation, bloomTex.Size() / 2f, 0.65f, 0f, 0f);
				Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(180, 0, 0, 0), progress), Projectile.rotation, glowTex.Size() / 2f, Projectile.scale, flip, 0f);
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, flip, 0f);

			if (shots > 0)
			{
				Main.spriteBatch.Draw(glowMap, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(255, 0, 0, 0), shots / 30f) * 0.5f, Projectile.rotation, glowMap.Size() / 2f, Projectile.scale, flip, 0f);
				Main.spriteBatch.Draw(glowMap, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(185, 50, 50, 0), shots / 30f) * 0.5f, Projectile.rotation, glowMap.Size() / 2f, Projectile.scale, flip, 0f);

				Main.spriteBatch.Draw(bloomMap, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(255, 0, 0, 0), shots / 30f) * 0.5f, Projectile.rotation, bloomMap.Size() / 2f, Projectile.scale, flip, 0f);
				Main.spriteBatch.Draw(bloomMap, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(185, 50, 50, 0), shots / 30f) * 0.5f, Projectile.rotation, bloomMap.Size() / 2f, Projectile.scale, flip, 0f);
				//Main.spriteBatch.Draw(bloomTex, Projectile.Center + new Vector2(Projectile.direction == 1 ? -14 : 0, -10 * Projectile.spriteDirection).RotatedBy(Projectile.rotation) - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(255, 100, 50, 0), shots / 30f) * 0.5f, Projectile.rotation, bloomTex.Size() / 2f, 0.35f, 0f, 0f);
				//Main.spriteBatch.Draw(bloomTex, Projectile.Center + new Vector2(Projectile.direction == 1 ? -14 : 0, -10 * Projectile.spriteDirection).RotatedBy(Projectile.rotation) - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(180, 50, 50, 0), shots / 30f) * 0.5f, Projectile.rotation, bloomTex.Size() / 2f, 0.35f, 0f, 0f);
			}

			if (flashed)
			{
				Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(180, 50, 50, 0), flashTimer / 20f), Projectile.rotation, glowTex.Size() / 2f, Projectile.scale, flip, 0f);
				Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, new Color(180, 50, 50, 0), flashTimer / 20f), Projectile.rotation, bloomTex.Size() / 2f, 0.85f, 0f, 0f);
			}

			if (explodingTimer > 0)
			{
				float progress = explodingTimer / 45f;

				if (!exploded)
				{
					for (int i = 0; i < 2; i++)
					{
						Main.spriteBatch.Draw(flareTex, Projectile.Center - Main.screenPosition, null, new Color(255, 50, 50, 0), MathHelper.Lerp(0f, 6f, progress), flareTex.Size() / 2f, MathHelper.Lerp(0.1f, 0.025f, progress), 0f, 0f);
					}
				}

				Color overlayColor;

				if (progress < 0.5f)
					overlayColor = Color.Lerp(new Color(0, 0, 0, 0), new Color(255, 50, 50, 0) * 0.5f, progress * 2);
				else
					overlayColor = Color.Lerp(new Color(255, 50, 50, 0) * 0.5f, Color.White, (progress - 0.5f) * 2);

				Main.spriteBatch.Draw(whiteTex, Projectile.Center - Main.screenPosition, null, overlayColor, Projectile.rotation, whiteTex.Size() / 2f, Projectile.scale, flip, 0);

				progress *= progress;
				Color glowColor;

				if (progress < 0.5f)
					glowColor = Color.Lerp(new Color(0, 0, 0, 0), new Color(255, 50, 50, 0), progress * 2);
				else
					glowColor = Color.Lerp(new Color(255, 50, 50, 0), new Color(180, 50, 50, 0), (progress - 0.5f) * 2);
				glowColor.A = 0;

				Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, glowColor * 0.75f, Projectile.rotation, glowTex.Size() / 2f, Projectile.scale, flip, 0);

				Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, glowColor, 0f, bloomTex.Size() / 2f, 0.55f, 0, 0);
			}

			return false;
		}

		private void ShootBullet(Vector2 armPos)
		{
			shots++;

			Vector2 pos = armPos + Projectile.rotation.ToRotationVector2() * 20f + Vector2.UnitY.RotatedBy(Projectile.velocity.ToRotation()) * -10f * Owner.direction;

			Owner.PickAmmo(Owner.HeldItem, out int projID, out float shootSpeed, out int damage, out float knockBack, out int ammoID);

			if (Main.myPlayer == Projectile.owner)
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(25f, 25f + shootSpeed), ModContent.ProjectileType<ImpactSMGShot>(), (int)(Projectile.damage * (Overheated ? 1.2f : 1f)), Projectile.knockBack, Projectile.owner);

			Gore.NewGoreDirect(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2(Projectile.direction == 1 ? -14 : 0, -10 * Projectile.spriteDirection).RotatedBy(Projectile.rotation), -Projectile.rotation.ToRotationVector2() * 3f + Vector2.UnitY * -2f, Mod.Find<ModGore>("ImpactSMG_Shell").Type).timeLeft = 1;

			updateVelo = true;

			Helper.PlayPitched("Guns/RifleLight", 0.35f, Main.rand.NextFloat(-0.1f, 0.1f), pos);

			if (shots >= 25)
			{
				Helper.PlayPitched("Guns/dry_fire", 0.7f, Main.rand.NextFloat(-0.1f, 0.1f), pos);

				if (Main.rand.NextBool(3))
					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(1f, 5), 0, new Color(100, 55, 50) * 0.5f, Main.rand.NextFloat(0.35f, 0.5f));
			}

			for (int i = 0; i < 3; i++)
			{
				Dust.NewDustPerfect(pos, ModContent.DustType<GlowFastDecelerate>(), Projectile.rotation.ToRotationVector2().RotatedByRandom(0.25f) * 5f, 0, new Color(190, 40, 40), 0.25f);
				Dust.NewDustPerfect(pos, ModContent.DustType<GlowFastDecelerate>(), Projectile.rotation.ToRotationVector2().RotatedByRandom(0.35f) * 7f, 0, new Color(255, 40, 40), 0.3f);
			}

			if (!Owner.HasAmmo(Owner.HeldItem))
				Projectile.Kill();
		}

		private void ClampVelocity()
		{
			Vector2 playerCenter = Owner.Center;
			Vector2 pos = Projectile.Center;

			float betweenX = Owner.Center.X - pos.X;
			float betweenY = Owner.Center.Y - pos.Y;

			float distance = (float)Math.Sqrt(betweenX * betweenX + betweenY * betweenY);
			float speed = 15f;
			float adjust = 0.95f;

			if (distance > 3000f)
				Projectile.Kill();

			distance = speed / distance;
			betweenX *= distance;
			betweenY *= distance;

			if (Projectile.velocity.X < betweenX)
			{
				Projectile.velocity.X += adjust;

				if (Projectile.velocity.X < 0f && betweenX > 0f)
					Projectile.velocity.X += adjust;
			}
			else if (Projectile.velocity.X > betweenX)
			{
				Projectile.velocity.X -= adjust;

				if (Projectile.velocity.X > 0f && betweenX < 0f)
					Projectile.velocity.X -= adjust;
			}

			if (Projectile.velocity.Y < betweenY)
			{
				Projectile.velocity.Y += adjust;

				if (Projectile.velocity.Y < 0f && betweenY > 0f)
					Projectile.velocity.Y += adjust;
			}
			else if (Projectile.velocity.Y > betweenY)
			{
				Projectile.velocity.Y -= adjust;

				if (Projectile.velocity.Y > 0f && betweenY < 0f)
					Projectile.velocity.Y -= adjust;
			}

			if (Vector2.Distance(Projectile.Center, Owner.Center) < 20f)
				Projectile.Kill();
		}

		private void BoomerangAI()
		{
			if (Main.myPlayer == Projectile.owner && mousePos == Vector2.Zero)
				mousePos = Owner.DirectionTo(Main.MouseWorld);

			Vector2 armPos = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
			armPos += Utils.SafeNormalize(Projectile.velocity, Vector2.UnitX) * 15f;

			if (!thrown)
			{
				throwTimer++;

				if (Main.myPlayer == Owner.whoAmI)
					Owner.ChangeDir(Main.MouseWorld.X < Owner.Center.X ? -1 : 1);

				Owner.heldProj = Projectile.whoAmI;
				Owner.itemTime = 2;
				Owner.itemAnimation = 2;

				Projectile.timeLeft = 2;
				Projectile.rotation = Utils.ToRotation(Projectile.velocity);
				Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

				Projectile.position = armPos - Projectile.Size * 0.5f;

				Projectile.spriteDirection = Owner.direction;
			}

			if (throwTimer < 20)
			{
				float progress = EaseFunction.EaseCircularInOut.Ease(throwTimer / 20f);
				Projectile.velocity = Vector2.One.RotatedBy(mousePos.ToRotation() + MathHelper.Lerp(0f, -1.85f, progress) * Owner.direction - MathHelper.PiOver4);
			}
			else if (throwTimer < 30)
			{
				float progress = EaseFunction.EaseCircularInOut.Ease((throwTimer - 20f) / 10f);
				Projectile.velocity = Vector2.One.RotatedBy(mousePos.ToRotation() + MathHelper.Lerp(-1.85f, 0.35f, progress) * Owner.direction - MathHelper.PiOver4);
			}
			else
			{
				if (!thrown && Main.myPlayer == Owner.whoAmI)
				{
					Projectile.friendly = true;
					SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
					Projectile.velocity = Owner.DirectionTo(Main.MouseWorld) * 20f;
					Projectile.timeLeft = 2400;
					Projectile.ignoreWater = false;
					Projectile.width = Projectile.height = 32;
					Projectile.tileCollide = true;
					ShootDelay = 0;
					thrown = true;
				}

				if (++ShootDelay >= 25)
				{
					if (Projectile.tileCollide)
						Projectile.tileCollide = false;

					Owner.heldProj = Projectile.whoAmI;

					ClampVelocity();
				}

				Owner.ChangeDir(Projectile.Center.X < Owner.Center.X ? -1 : 1);
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.Center.DirectionTo(Projectile.Center).ToRotation() - MathHelper.PiOver2);
				Projectile.rotation += 0.3f * (Projectile.velocity.Length() * 0.07f);

				if (Projectile.soundDelay == 0)
				{
					Projectile.soundDelay = 8;
					SoundEngine.PlaySound(SoundID.Item7, Projectile.position);
				}
			}
		}

		private void ExplodingAI()
		{
			if (Main.myPlayer == Projectile.owner && mousePos == Vector2.Zero)
				mousePos = Owner.DirectionTo(Main.MouseWorld);

			Vector2 armPos = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
			armPos += Utils.SafeNormalize(Projectile.velocity, Vector2.UnitX) * 15f;

			if (!thrown)
			{
				throwTimer++;

				if (Main.myPlayer == Owner.whoAmI)
					Owner.ChangeDir(Main.MouseWorld.X < Owner.Center.X ? -1 : 1);

				Owner.heldProj = Projectile.whoAmI;
				Owner.itemTime = 2;
				Owner.itemAnimation = 2;

				Projectile.timeLeft = 2;
				Projectile.rotation = Utils.ToRotation(Projectile.velocity);
				Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

				Projectile.position = armPos - Projectile.Size * 0.5f;

				Projectile.spriteDirection = Owner.direction;
			}

			if (throwTimer < 20)
			{
				float progress = EaseFunction.EaseCircularInOut.Ease(throwTimer / 20f);
				Projectile.velocity = Vector2.One.RotatedBy(mousePos.ToRotation() + MathHelper.Lerp(0f, -1.85f, progress) * Owner.direction - MathHelper.PiOver4);
			}
			else if (throwTimer < 30)
			{
				float progress = EaseFunction.EaseCircularInOut.Ease((throwTimer - 20f) / 10f);
				Projectile.velocity = Vector2.One.RotatedBy(mousePos.ToRotation() + MathHelper.Lerp(-1.85f, 0.35f, progress) * Owner.direction - MathHelper.PiOver4);
			}
			else
			{
				if (!thrown && Main.myPlayer == Owner.whoAmI)
				{
					Projectile.friendly = true;
					SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
					Projectile.velocity = Owner.DirectionTo(Main.MouseWorld) * 20f;
					Projectile.timeLeft = 2400;
					Projectile.ignoreWater = false;
					Projectile.width = Projectile.height = 32;
					ShootDelay = 0;
					Projectile.tileCollide = true;
					thrown = true;
				}

				if (exploding && !exploded)
				{
					Projectile.velocity.Y += 0.1f;
					Projectile.velocity *= 0.915f;
					Projectile.rotation += Projectile.velocity.Length() * 0.075f;

					for (int i = 0; i < 2; i++)
					{
						float radius = MathHelper.Lerp(65f, 0f, 1f - Projectile.timeLeft / 45f);
						Vector2 circle = Main.rand.NextVector2CircularEdge(radius, radius);
						Dust.NewDustPerfect(Projectile.Center + circle, ModContent.DustType<Glow>(), Vector2.Zero, 0, new Color(255, 50, 50), Main.rand.NextFloat(0.3f, 0.4f));
					}

					if (++explodingTimer >= 45)
					{
						ImpactSMGPlayer mp = Owner.GetModPlayer<ImpactSMGPlayer>();
						Helper.PlayPitched("Magic/FireHit", 0.75f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);
						CameraSystem.shake += 10 + mp.stacks;

						for (int i = 0; i < 20 + mp.stacks * 3; i++)
						{
							var dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<ImpactSMGDustTwo>());
							dust.velocity = Main.rand.NextVector2Circular(7 + mp.stacks, 7 + mp.stacks);
							dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
							dust.alpha = Main.rand.Next(80) + 40;
							dust.rotation = Main.rand.NextFloat(6.28f);

							var dust2 = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<ImpactSMGDust>());
							dust2.velocity = Main.rand.NextVector2Circular(7 + mp.stacks, 7 + mp.stacks);
							dust2.scale = Main.rand.NextFloat(1.5f, 1.9f);
							dust2.alpha = 70 + Main.rand.Next(60);
							dust2.rotation = Main.rand.NextFloat(6.28f);

							Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowFastDecelerate>(), Main.rand.NextVector2Circular(10f, 10f), 0, new Color(255, 50, 50), 0.35f);
							Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Glow>(), Main.rand.NextVector2CircularEdge(5f, 5f), 0, new Color(255, 50, 50), 0.65f);
						}

						if (Main.myPlayer == Projectile.owner)
							Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ImpactSMGExplosion>(), (int)(Projectile.damage * (2 + mp.stacks * 0.1f)), 3f, Projectile.owner, 65 + Owner.GetModPlayer<ImpactSMGPlayer>().stacks * 10);

						exploding = false;
						exploded = true;
						ShootDelay = 0;
						Projectile.velocity = Projectile.DirectionTo(Owner.Center) * 15f;
						Projectile.velocity.Y += -3f;
						Projectile.friendly = true;
						mp.stacks = 0;
					}
				}
				else if (++ShootDelay >= 25)
				{
					if (Projectile.tileCollide)
						Projectile.tileCollide = false;

					Owner.heldProj = Projectile.whoAmI;

					ClampVelocity();
				}

				if (!exploding)
				{
					if (Projectile.soundDelay == 0)
					{
						Projectile.soundDelay = 8;
						SoundEngine.PlaySound(SoundID.Item7, Projectile.position);
					}

					Projectile.rotation += 0.3f * (Projectile.velocity.Length() * 0.07f);
				}

				Owner.ChangeDir(Projectile.Center.X < Owner.Center.X ? -1 : 1);
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.Center.DirectionTo(Projectile.Center).ToRotation() - MathHelper.PiOver2);

				if (exploded && explodingTimer > 0)
					explodingTimer--;
			}
		}
	}

	public class ImpactSMGShot : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;

			Projectile.timeLeft = 80;
			Projectile.extraUpdates = 4;

			Projectile.penetrate = 2;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Impact SMG Shot");
		}

		public override void AI()
		{
			Projectile.velocity *= 0.93f;

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			Player Owner = Main.player[Projectile.owner];

			if (Owner.GetModPlayer<ImpactSMGPlayer>().stacks > 0)
				modifiers.SourceDamage *= 1f + Owner.GetModPlayer<ImpactSMGPlayer>().stacks * 0.05f;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.friendly = false;

			for (int i = 0; i < 2; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowFastDecelerate>(), -Projectile.velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(), 0, new Color(190, 40, 40), 0.25f);
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowFastDecelerate>(), -Projectile.velocity.RotatedByRandom(0.35f) * Main.rand.NextFloat(), 0, new Color(255, 40, 40), 0.3f);
			}

			Projectile.velocity *= 0f;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.friendly = false;

			for (int i = 0; i < 2; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowFastDecelerate>(), -Projectile.velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(), 0, new Color(190, 40, 40), 0.25f);
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowFastDecelerate>(), -Projectile.velocity.RotatedByRandom(0.35f) * Main.rand.NextFloat(), 0, new Color(255, 40, 40), 0.3f);
			}

			Projectile.velocity *= 0f;

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();
			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 10; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 10)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(160), factor => factor * 3.5f, factor => new Color(255, 255, 255) * factor.X * (Projectile.timeLeft / 80f));

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(160), factor => factor * 6f, factor => new Color(190, 40, 40) * factor.X * (Projectile.timeLeft / 80f));

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

			trail2?.Render(effect);
			trail?.Render(effect);
		}
	}

	class ImpactSMGExplosion : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;

		private float Progress => 1 - Projectile.timeLeft / 25f;
		private float Radius => Projectile.ai[0] * (float)Math.Sqrt(Math.Sqrt(Progress));

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 25;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 25;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Explosion");
		}

		public override void AI()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			for (int k = 0; k < 6; k++)
			{
				float rot = Main.rand.NextFloat(0, 6.28f);

				Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * Radius, ModContent.DustType<GlowFastDecelerate>(),
					Vector2.One.RotatedBy(rot) * 0.5f, 0, Main.rand.NextBool() ? new Color(180, 50, 50) : new Color(255, 50, 50), Main.rand.NextFloat(0.35f, 0.4f));
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius + 20;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line);
		}

		private void ManageCaches()
		{
			if (cache is null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 40; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			for (int k = 0; k < 40; k++)
			{
				cache[k] = Projectile.Center + Vector2.One.RotatedBy(k / 19f * 6.28f) * Radius;
			}

			while (cache.Count > 40)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 25 * (1 - Progress), factor => new Color(180, 50, 50));

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 20 * (1 - Progress), factor => new Color(255, 50, 50));

			trail.Positions = cache.ToArray();
			trail.NextPosition = cache[39];

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = cache[39];
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.03f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value);

			trail?.Render(effect);
			trail2?.Render(effect);

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "FireTrail").Value);

			trail2?.Render(effect);
		}
	}
}