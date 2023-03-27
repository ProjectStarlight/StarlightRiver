using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric
{
	public class Needler : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needler");
			Tooltip.SetDefault("Stick spikes to enemies to build up heat \nOverheated enemies explode, dealing massive damage");
		}

		//TODO: Adjust rarity sellprice and balance
		public override void SetDefaults()
		{
			Item.damage = 8;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Orange;
			Item.shoot = ModContent.ProjectileType<NeedlerProj>();
			Item.shootSpeed = 14f;
			Item.autoReuse = true;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-10, 0);
		}

		public override bool? UseItem(Player Player)
		{
			Helper.PlayPitched("Guns/SMG2", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f), Player.position);
			return true;
		}

		//TODO: Add glowmask to weapon
		//TODO: Add holdoffset
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Helper.PlayPitched("Guns/SMG2", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));
			Vector2 direction = velocity;
			float itemRotation = Main.rand.NextFloat(-0.1f, 0.1f);
			direction = direction.RotatedBy(itemRotation * 2);
			Projectile.NewProjectile(source, position, direction, type, damage, knockback, player.whoAmI);

			direction = velocity.RotatedBy(itemRotation);

			for (int i = 0; i < 15; i++)
			{
				var dust = Dust.NewDustPerfect(position + direction * 3f, 6, direction.RotatedBy(Main.rand.NextFloat(-1, 1)) / 5f * Main.rand.NextFloat());
				dust.noGravity = true;
			}

			player.itemRotation = direction.ToRotation(); //TODO: Wrap properly when facing left

			if (player.direction != 1)
				player.itemRotation -= 3.14f;

			player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
			return false;
		}
	}

	public class NeedlerProj : ModProjectile
	{
		int enemyID;
		bool stuck = false;
		Vector2 offset = Vector2.Zero;

		float needleLerp = 0f;

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needle");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.aiStyle = 113;
			Projectile.width = Projectile.height = 20;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		private void findIfHit()
		{
			foreach (NPC NPC in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.life > 0 && n.Hitbox.Intersects(Projectile.Hitbox)))
			{
				OnHitNPC(NPC, 0, 0, false);
			}
		}

		//TODO: Move methods to top + method breaks
		//TODO: Turn needles into getnset
		public override bool PreAI()
		{
			if (stuck)
			{
				NPC target = Main.npc[enemyID];
				int needles = target.GetGlobalNPC<NeedlerNPC>().needles;

				if (Main.rand.NextBool(Math.Max((10 - needles) * 30 + 300, 50)))
					Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.4f, 0.8f));

				if (target.GetGlobalNPC<NeedlerNPC>().needleTimer == 1)
				{
					//Projectile.NewProjectile(Projectile.Center, Vector2.Zero, ModContent.ProjectileType<NeedlerExplosion>(), Projectile.damage * 3, Projectile.knockBack, Projectile.owner);
					Projectile.active = false;
				}

				if (needles > needleLerp)
				{
					if (needleLerp < 10)
					{
						needleLerp += 0.2f;

						if (needles > needleLerp + 3)
							needleLerp += 0.4f;
					}
				}
				else
				{
					needleLerp = needles;
				}

				var lightColor = Color.Lerp(Color.Orange, Color.Red, needleLerp / 20f);
				Lighting.AddLight(Projectile.Center, lightColor.R * needleLerp / 2000f, lightColor.G * needleLerp / 2000f, lightColor.B * needleLerp / 2000f);

				if (!target.active)
				{
					if (Projectile.timeLeft > 5)
						Projectile.timeLeft = 5;

					Projectile.velocity = Vector2.Zero;
				}
				else
				{
					Projectile.position = target.position + offset;
				}

				if (Projectile.timeLeft == 2)
					target.GetGlobalNPC<NeedlerNPC>().needles--;

				return false;
			}
			else
			{
				Projectile.rotation = Projectile.velocity.ToRotation();
			}

			return true;
		}

		public override void PostAI()
		{
			if (Main.myPlayer != Projectile.owner && !stuck)
				findIfHit();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!stuck && target.life > 0)
			{
				Projectile.penetrate++;
				target.GetGlobalNPC<NeedlerNPC>().needles++;
				target.GetGlobalNPC<NeedlerNPC>().needleDamage = Projectile.damage;
				target.GetGlobalNPC<NeedlerNPC>().needlePlayer = Projectile.owner;
				stuck = true;
				Projectile.friendly = false;
				Projectile.tileCollide = false;
				enemyID = target.whoAmI;
				offset = Projectile.position - target.position;
				offset -= Projectile.velocity;
				Projectile.timeLeft = 400;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			Color color;

			if (stuck)
				color = Helper.MoltenVitricGlow(100 - needleLerp * 10);
			else
				color = Helper.MoltenVitricGlow(100);

			ReLogic.Content.Asset<Texture2D> tex = TextureAssets.Projectile[Projectile.type];
			Rectangle glassFrame = tex.Frame(2, 1, 0);
			Rectangle hotFrame = tex.Frame(2, 1, 1);
			spriteBatch.Draw(tex.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), glassFrame, lightColor, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
			spriteBatch.Draw(tex.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), hotFrame, color * (needleLerp / 10f), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

			if (stuck)
			{
				tex = ModContent.Request<Texture2D>(AssetDirectory.VitricItem + "NeedlerBloom");
				color = Color.Lerp(Color.Orange, Color.Red, needleLerp / 20f);
				color.A = 0;
				spriteBatch.Draw(tex.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, color * 0.66f, Projectile.rotation, tex.Size() * 0.5f, (Projectile.scale * (needleLerp / 10f) + 0.25f) * new Vector2(1f, 1.25f), SpriteEffects.None, 0f);
			}

			return false;
		}
	}

	public class NeedlerExplosion : ModProjectile
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needle");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.width = Projectile.height = 200;
			Projectile.timeLeft = 20;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			for (int i = 0; i < 2; i++)
				Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center + Main.rand.NextVector2Circular(25, 25), Main.rand.NextFloat(3.14f, 6.28f).ToRotationVector2() * 7, Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.4f, 0.8f));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			crit = true;
			base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 180);

			if (target.GetGlobalNPC<NeedlerNPC>().needles >= 1 && target.GetGlobalNPC<NeedlerNPC>().needleTimer <= 0)
				target.GetGlobalNPC<NeedlerNPC>().needleTimer = 60;
		}
	}

	public class NeedlerEmber : ModProjectile
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needle");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.aiStyle = 1;
			Projectile.width = Projectile.height = 12;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.extraUpdates = 1;
			Projectile.alpha = 255;
		}

		public override void AI()
		{
			Projectile.scale *= 0.98f;

			if (Main.rand.NextBool(2))
			{
				var dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<NeedlerDustFastFade>(), Main.rand.NextVector2Circular(1.5f, 1.5f));
				dust.scale = 0.6f * Projectile.scale;
				dust.rotation = Main.rand.NextFloatDirection();
			}
		}
	}

	public class NeedlerNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public int needles = 0;
		public int needleTimer = 0;
		public int needleDamage = 0;
		public int needlePlayer = 0;

		public override void ResetEffects(NPC NPC)
		{
			needleTimer--;
			base.ResetEffects(NPC);
		}
		public override void AI(NPC NPC)
		{
			if (needles >= 8 && needleTimer <= 0)
			{
				needles++;
				needleTimer = 60;
				SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireCast"), NPC.Center);
			}

			if (needleTimer == 1)
			{
				SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireHit"), NPC.Center);

				if (needlePlayer == Main.myPlayer)
				{
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<NeedlerExplosion>(), (int)(needleDamage * Math.Sqrt(needles)), 0, needlePlayer);

					for (int i = 0; i < 5; i++)
					{
						Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 3), ModContent.ProjectileType<NeedlerEmber>(), 0, 0, needlePlayer).scale = Main.rand.NextFloat(0.85f, 1.15f);
					}

					CameraSystem.shake = 20;
				}

				for (int i = 0; i < 10; i++)
				{
					var dust = Dust.NewDustDirect(NPC.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<NeedlerDust>());
					dust.velocity = Main.rand.NextVector2Circular(10, 10);
					dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
					dust.alpha = 70 + Main.rand.Next(60);
					dust.rotation = Main.rand.NextFloat(6.28f);
				}

				for (int i = 0; i < 10; i++)
				{
					var dust = Dust.NewDustDirect(NPC.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<NeedlerDustSlowFade>());
					dust.velocity = Main.rand.NextVector2Circular(10, 10);
					dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
					dust.alpha = Main.rand.Next(80) + 40;
					dust.rotation = Main.rand.NextFloat(6.28f);

					Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<NeedlerDustGlow>()).scale = 0.9f;
				}

				needles = 0;
			}

			if (needleTimer > 30)
			{
				float angle = Main.rand.NextFloat(6.28f);
				var dust = Dust.NewDustPerfect(NPC.Center - new Vector2(15, 15) - angle.ToRotationVector2() * 70, ModContent.DustType<NeedlerDustGlowGrowing>());
				dust.scale = 0.05f;
				dust.velocity = angle.ToRotationVector2() * 0.2f;
			}

			base.AI(NPC);
		}
	}
}