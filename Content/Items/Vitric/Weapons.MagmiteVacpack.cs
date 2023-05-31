using System;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric
{
	public class MagmiteVacpack : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Blasts out Magmites that stick to enemies and increase summon tag damage");
		}

		public override void SetDefaults()
		{
			Item.width = 50;
			Item.height = 20;

			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 4, silver: 75);

			Item.damage = 28;
			Item.DamageType = DamageClass.Summon;
			Item.useTime = Item.useAnimation = 35;

			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shoot = ModContent.ProjectileType<MagmiteVacpackHoldout>();
			Item.shootSpeed = 17f;

			Item.noMelee = true;
			Item.autoReuse = true;

			Item.noUseGraphic = true;
			Item.channel = true;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-10f, 0f);
		}
	}

	//literally just for tank drawing lol
	internal class MagmiteVacpackPlayer : ModPlayer
	{
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			if (drawInfo.shadow != 0f)
				return;

			Texture2D tankTexture = ModContent.Request<Texture2D>(AssetDirectory.VitricItem + "MagmiteVacpack_Tank").Value;

			Player drawplayer = drawInfo.drawPlayer;

			Item heldItem = drawplayer.HeldItem;

			if (drawplayer.HeldItem.type == ModContent.ItemType<MagmiteVacpack>() && !drawplayer.frozen && !drawplayer.dead && (!drawplayer.wet || !heldItem.noWet) && drawplayer.wings <= 0)
			{
				SpriteEffects spriteEffects = drawplayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

				var drawPos = new Vector2((int)(drawplayer.position.X - Main.screenPosition.X + drawplayer.width / 2 - 9 * drawplayer.direction),
					(int)(drawplayer.position.Y - Main.screenPosition.Y + drawplayer.height / 2 + 2f * drawplayer.gravDir - 2f * drawplayer.gravDir));

				var tankData = new DrawData(tankTexture, drawPos, new Rectangle?(new Rectangle(0, 0, tankTexture.Width, tankTexture.Height)),
					drawInfo.colorArmorBody, drawplayer.bodyRotation, tankTexture.Size() / 2f, 1f, spriteEffects, 0);

				drawInfo.DrawDataCache.Add(tankData);
			}
		}
	}

	internal class MagmiteVacpackHoldout : ModProjectile
	{
		private int time;

		public ref float ShootDelay => ref Projectile.ai[0];
		public ref float MaxFramesTillShoot => ref Projectile.ai[1];

		public Player Owner => Main.player[Projectile.owner];

		public bool CanHold => Owner.channel && !Owner.CCed && !Owner.noItems;

		public override string Texture => AssetDirectory.VitricItem + "MagmiteVacpack";

		public override bool? CanDamage()
		{
			return false;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magmite Vacpack");
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 54;
			Projectile.height = 26;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			time++;
			ShootDelay++;

			Vector2 armPos = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
			armPos += Utils.SafeNormalize(Projectile.velocity, Owner.direction * Vector2.UnitX) * 15f;

			Vector2 barrelPos = armPos + Projectile.velocity * Projectile.width * 0.5f;
			barrelPos.Y -= 8;
			barrelPos.X += -8;

			if (MaxFramesTillShoot == 0f)
				MaxFramesTillShoot = Owner.HeldItem.useAnimation;

			if (!CanHold)
				Projectile.Kill();

			if (ShootDelay >= MaxFramesTillShoot)
			{
				Item heldItem = Owner.HeldItem;
				int damage = Projectile.damage;
				float shootSpeed = heldItem.shootSpeed;
				float knockBack = Owner.GetWeaponKnockback(heldItem, heldItem.knockBack);
				Vector2 shootVelocity = Utils.SafeNormalize(Projectile.velocity, Vector2.UnitY) * shootSpeed;

				if (Main.myPlayer == Projectile.owner)
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelPos, shootVelocity, ModContent.ProjectileType<MagmiteVacpackProjectile>(), damage, knockBack, Owner.whoAmI);

				for (int i = 0; i < 10; i++)
				{
					Gore.NewGore(Projectile.GetSource_FromThis(), barrelPos, (shootVelocity * Main.rand.NextFloat(0.15f, 0.25f)).RotatedByRandom(MathHelper.ToRadians(20f)),
						Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.8f, 0.95f));

					Dust.NewDustPerfect(barrelPos, ModContent.DustType<Dusts.FireDust2>(), (shootVelocity * Main.rand.NextFloat(0.25f, 0.35f)).RotatedByRandom(MathHelper.ToRadians(15f)), 0, Color.DarkOrange, Main.rand.NextFloat(0.5f, 0.75f));
				}

				SoundEngine.PlaySound(SoundID.Splash with { PitchRange = (-0.1f, 0.1f) } with { Volume = 0.8f }, Projectile.position);
				ShootDelay = 0;
			}

			Owner.ChangeDir(Projectile.direction);
			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;

			Projectile.timeLeft = 2;
			Projectile.rotation = Utils.ToRotation(Projectile.velocity);
			Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

			if (Projectile.spriteDirection == -1)
				Projectile.rotation += 3.1415927f;

			Projectile.position = armPos - Projectile.Size * 0.5f;

			Projectile.spriteDirection = Projectile.direction;

			if (Main.myPlayer == Projectile.owner)
			{
				float interpolant = Utils.GetLerpValue(5f, 25f, Projectile.Distance(Main.MouseWorld), true);

				Vector2 oldVelocity = Projectile.velocity;

				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Owner.DirectionTo(Main.MouseWorld), interpolant);

				if (Projectile.velocity != oldVelocity)
				{
					Projectile.netSpam = 0;
					Projectile.netUpdate = true;
				}
			}

			if (time < 4)
				return;

			if (Main.rand.NextBool(10))
			{
				Gore.NewGore(Projectile.GetSource_FromThis(), barrelPos, (Vector2.UnitY * Main.rand.NextFloat(0.4f, 0.5f)).RotatedByRandom(MathHelper.ToRadians(35f)), Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.6f, 0.7f));

				if (Main.rand.NextBool(2))
					Dust.NewDustPerfect(barrelPos, ModContent.DustType<Dusts.MagmaSmoke>(), (Vector2.UnitY * Main.rand.NextFloat(-3f, -2f)).RotatedByRandom(MathHelper.ToRadians(25f)), 100, Color.Black, Main.rand.NextFloat(0.7f, 0.9f));
			}

			if (Projectile.soundDelay == 0)
			{
				SoundEngine.PlaySound(SoundID.SplashWeak with { PitchRange = (-0.1f, 0.1f) }, Projectile.position);
				Projectile.soundDelay = 6;
			}
		}
	}

	internal class MagmiteVacpackProjectile : ModProjectile
	{
		internal int MaxBounces = 3;

		internal int enemyID;
		internal bool stuck = false;
		internal Vector2 offset = Vector2.Zero;

		internal Vector2 oldVelocity;

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bouncy Magmite");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;

			Projectile.Size = new Vector2(20);

			Projectile.timeLeft = 300;
			Projectile.penetrate = -1;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return Color.White;
		}

		public override bool PreAI()
		{
			if (stuck)
			{
				NPC target = Main.npc[enemyID];
				Projectile.position = target.position + offset;
			}

			return base.PreAI();
		}

		public override void AI()
		{
			if (stuck)
			{
				Projectile.scale -= 0.0025f;

				if (Main.npc[enemyID].active)
					Projectile.timeLeft = 2;
				else
					Projectile.Kill();

				if (Main.rand.NextBool(4))
					Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center + oldVelocity + new Vector2(-5f, -5f), Projectile.velocity * 0.1f, Mod.Find<ModGore>("MagmiteGore").Type).scale = Projectile.scale;

				if (Main.rand.NextBool(20))
					Dust.NewDustPerfect(Projectile.Center + oldVelocity, ModContent.DustType<Dusts.MagmaSmoke>(), (Vector2.UnitY * Main.rand.NextFloat(-3f, -2f)).RotatedByRandom(MathHelper.ToRadians(5f)), 100, Color.Black, Projectile.scale);
			}
			else
			{
				Projectile.rotation += 0.1f + Projectile.direction;

				Projectile.velocity.Y += 0.65f;

				if (Projectile.velocity.Y > 16f)
					Projectile.velocity.Y = 16f;

				if (Main.rand.NextBool(4))
					Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0.1f, Mod.Find<ModGore>("MagmiteGore").Type).scale = Main.rand.NextFloat(0.4f, 0.9f);
			}

			if (Projectile.scale < 0.3f)
				Projectile.Kill();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			MagmiteVacpackGlobalNPC globalNPC = target.GetGlobalNPC<MagmiteVacpackGlobalNPC>();

			if (globalNPC.magmiteAmount < 3)
			{
				stuck = true;
				Projectile.friendly = false;
				Projectile.tileCollide = false;
				enemyID = target.whoAmI;
				offset = Projectile.position - target.position;
				Projectile.netUpdate = true;

				globalNPC.magmiteAmount++;
				globalNPC.magmiteOwner = Projectile.owner;

				Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;

				oldVelocity = Projectile.velocity;
			}
			else
			{
				MaxBounces--;

				Projectile.velocity.X *= -1f;

				Projectile.scale -= 0.1f;

				for (int i = 0; i < 10; i++)
				{
					Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(0.1f, 0.2f), Mod.Find<ModGore>("MagmiteGore").Type);
				}

				SoundEngine.PlaySound(SoundID.SplashWeak, Projectile.position);
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			MaxBounces--;
			if (MaxBounces <= 0)
				Projectile.Kill();

			if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
				Projectile.velocity.X = -oldVelocity.X;

			if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
				Projectile.velocity.Y = -oldVelocity.Y;

			Projectile.scale -= 0.1f;

			for (int i = 0; i < 10; i++)
			{
				Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(4f, 5f), Mod.Find<ModGore>("MagmiteGore").Type, 1.35f);
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AmberBolt, 0, 0, 0, default, 0.5f);
			}

			SoundEngine.PlaySound(SoundID.SplashWeak, Projectile.position);
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (stuck)
				return true;

			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			var drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}

			return true;
		}

		public override void Kill(int timeLeft)
		{
			if (!stuck)
			{
				for (int i = 0; i < 15; i++)
				{
					Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.UnitY * Main.rand.NextFloat(-8, -1)).RotatedByRandom(0.5f), Mod.Find<ModGore>("MagmiteGore").Type).scale = Main.rand.NextFloat(0.5f, 0.8f);
				}
			}
			else
			{
				for (int i = 0; i < 15; i++)
				{
					Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center + oldVelocity, (Vector2.UnitY * Main.rand.NextFloat(-8, -1)).RotatedByRandom(0.3f), Mod.Find<ModGore>("MagmiteGore").Type).scale = Main.rand.NextFloat(0.5f, 0.8f);
				}

				MagmiteVacpackGlobalNPC globalNPC = Main.npc[enemyID].GetGlobalNPC<MagmiteVacpackGlobalNPC>();
				globalNPC.magmiteAmount--;
			}

			SoundEngine.PlaySound(SoundID.DD2_GoblinHurt, Projectile.Center);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			MagmiteVacpackGlobalNPC globalNPC = target.GetGlobalNPC<MagmiteVacpackGlobalNPC>();

			if (globalNPC.magmiteAmount >= 3)
				modifiers.SourceDamage *= 1.5f;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(stuck);
			writer.WritePackedVector2(offset);
			writer.Write(enemyID);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			stuck = reader.ReadBoolean();
			offset = reader.ReadPackedVector2();
			enemyID = reader.ReadInt32();
		}
	}

	internal class MagmiteVacpackGlobalNPC : GlobalNPC
	{
		public int magmiteAmount;
		public int magmiteOwner;

		public override bool InstancePerEntity => true;

		public override void ResetEffects(NPC npc)
		{
			magmiteAmount = Utils.Clamp(magmiteAmount, 0, 3);
		}

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (magmiteAmount > 0)
			{
				if (npc.lifeRegen > 0)
					npc.lifeRegen = 0;

				npc.lifeRegen -= 10 * magmiteAmount;
				if (damage < 1)
					damage = 1;
			}
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
		{
			Player player = Main.player[projectile.owner];

			bool IsSummoner = projectile.minion || projectile.DamageType == DamageClass.Summon || ProjectileID.Sets.MinionShot[projectile.type] == true;

			if (projectile.owner == magmiteOwner && projectile.friendly && IsSummoner && npc.whoAmI == player.MinionAttackTargetNPC && magmiteAmount > 0 && player.HasMinionAttackTargetNPC)
				modifiers.SourceDamage += magmiteAmount * 3;
		}
	}
}