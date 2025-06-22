using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.ExposureSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Items.MechBoss
{
	internal class PlasmaCutter : ModItem
	{
		public override string Texture => AssetDirectory.MechBossItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Plasma Cutter");
			Tooltip.SetDefault("Drones follow your cursor while held\nUse to spend mana to make them fire lasers\n<right> to flip the drones around\ndeal lower damage but inflict {{BUFF:PlasmaCutterBuff}} while flipped");
		}

		public override void SetDefaults()
		{
			Item.damage = 40;
			Item.DamageType = DamageClass.Magic;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.noMelee = true;
			Item.knockBack = 1;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(0, 15, 0, 0);
			Item.useTurn = true;
			Item.channel = true;
			Item.mana = 3;
			Item.noUseGraphic = true;
			Item.autoReuse = true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.HallowedBar, 15);
			recipe.AddIngredient(ItemID.SoulofMight, 15);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}

		public override void HoldItem(Player player)
		{
			int cnt = player.ownedProjectileCounts[ModContent.ProjectileType<PlasmaCutterDrone>()];

			if (cnt != 2)
			{
				foreach (Projectile proj in Main.projectile.Where(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<PlasmaCutterDrone>()))
				{
					proj.active = false;
				}

				Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<PlasmaCutterDrone>(), Item.damage, Item.knockBack, player.whoAmI, 1);
				Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<PlasmaCutterDrone>(), Item.damage, Item.knockBack, player.whoAmI, -1);
			}
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool? UseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				foreach (Projectile proj in Main.projectile.Where(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<PlasmaCutterDrone>()))
				{
					if (proj.ModProjectile is PlasmaCutterDrone drone)
					{
						drone.State = drone.State == 1 ? 0 : 1;
						drone.animTimer = 20;
					}
				}
			}

			return true;
		}
	}

	internal class PlasmaCutterDrone : ModProjectile
	{
		public Projectile partner;
		public int animTimer;
		public Vector2 outLaserEnd;

		public ref float Direction => ref Projectile.ai[0];
		public ref float LaserSize => ref Projectile.ai[1];
		public ref float State => ref Projectile.ai[2];

		public override string Texture => AssetDirectory.MechBossItem + Name;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 2;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.timeLeft = 10;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 5;
			Projectile.DamageType = DamageClass.Magic;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (State == 0 && Direction == 1 && LaserSize > 0)
			{
				bool hit = Helpers.CollisionHelper.CheckLinearCollision(Projectile.Center, partner.Center, targetHitbox, out Vector2 inter);

				if (hit)
				{
					Dust.NewDustPerfect(inter, ModContent.DustType<Dusts.PixelSmokeColor>(), Vector2.UnitY * -Main.rand.NextFloat(2, 10), 100, Color.Gray, Main.rand.NextFloat(0.1f, 0.2f));

					for (int k = 0; k < 6; k++)
						Dust.NewDustPerfect(inter, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Vector2.UnitX.RotatedBy(k / 6f * 6.28f) * -Main.rand.NextFloat(2, 4), 0, new Color(150, 150, 255, 0), Main.rand.NextFloat(0.1f, 0.2f));
				}

				return hit;
			}

			if (State == 1 && LaserSize > 0)
			{
				return Helpers.CollisionHelper.CheckLinearCollision(Projectile.Center, outLaserEnd, targetHitbox, out Vector2 inter);
			}

			return false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (State == 1)
				modifiers.FinalDamage *= 0.2f;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (State == 1)
			{
				target.AddBuff(ModContent.BuffType<PlasmaCutterBuff>(), 300);
			}

			if (State == 0)
			{
				SoundHelper.PlayPitched("Magic/LightningShortest1", 0.5f, Main.rand.NextFloat(0.5f, 0.6f), target.Center);
			}
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (player.HeldItem.type != ModContent.ItemType<PlasmaCutter>())
				Projectile.active = false;

			if (partner is null || !partner.active || partner.type != Projectile.type)
			{
				partner = Main.projectile.FirstOrDefault(n => n.active && n.type == Projectile.type && n.owner == Projectile.owner && n.ModProjectile is PlasmaCutterDrone drone && drone.Direction != Direction);

				if (partner is null)
				{
					Projectile.active = false;
					return;
				}
			}

			// Select target ditance from the cursor
			int targetDist = 150;

			if (State == 1)
				targetDist = 50;

			if (Projectile.timeLeft > 2)
				return;

			if (Projectile.timeLeft <= 2)
				Projectile.timeLeft = 2;

			Vector2 target = Main.MouseWorld + Vector2.UnitX * targetDist * Direction;

			if (LaserSize <= 0)
				Projectile.Center += (target - Projectile.Center) * 0.09f;
			else
				Projectile.position.Y += (target.Y - Projectile.Center.Y) * 0.12f;

			if (LaserSize < 20)
			{
				float diff = Projectile.position.X - Projectile.oldPosition.X;
				Projectile.position.Y += diff * 0.1f * Direction * (1f - LaserSize / 20f);

				Projectile.position.Y += MathF.Sin(Main.GameUpdateCount * 0.01f * 6.28f + Direction) * targetDist / 100f * (1f - LaserSize / 20f);
			}

			Projectile.rotation = Projectile.Center.DirectionTo(partner.Center).ToRotation();
			partner.rotation = partner.Center.DirectionTo(Projectile.Center).ToRotation();

			if (player.channel && player.altFunctionUse != 2 && LaserSize < 20)
			{
				LaserSize++;
			}

			if (!player.channel && LaserSize > 0)
			{
				LaserSize -= 2;

				if (LaserSize < 0)
					LaserSize = 0;
			}

			Dust.NewDustPerfect(Projectile.Center + new Vector2(Direction * 12, 16), ModContent.DustType<Dusts.PixelatedGlow>(), Vector2.UnitY * Main.rand.NextFloat(0.5f, 1.8f), 0, new Color(255, Main.rand.Next(20, 255), 20, 0), Main.rand.NextFloat(0.2f));
			Dust.NewDustPerfect(Projectile.Center + new Vector2(Direction * 12, -16), ModContent.DustType<Dusts.PixelatedGlow>(), Vector2.UnitY * -Main.rand.NextFloat(0.5f, 1.8f), 0, new Color(255, Main.rand.Next(20, 255), 20, 0), Main.rand.NextFloat(0.2f));

			if (Main.rand.NextBool(4))
			{
				Dust.NewDustPerfect(Projectile.Center + new Vector2(Direction * 12, 16), DustID.Torch, Vector2.UnitY.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.5f, 1.8f), 0, new Color(255, Main.rand.Next(20, 255), 20, 0), 1);
				Dust.NewDustPerfect(Projectile.Center + new Vector2(Direction * 12, -16), DustID.Torch, Vector2.UnitY.RotatedByRandom(0.2f) * -Main.rand.NextFloat(0.5f, 1.8f), 0, new Color(255, Main.rand.Next(20, 255), 20, 0), 1);
			}

			// In mode
			if (State == 0)
			{
				if (LaserSize > 0)
				{
					var mid = Vector2.Lerp(Projectile.Center, partner.Center, 0.5f);
					Dust.NewDustPerfect(mid, ModContent.DustType<Dusts.PixelatedImpactLineDustGlow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), 0, new Color(100, 100, 255, 0), Main.rand.NextFloat(0.1f));

					Lighting.AddLight(mid, new Vector3(1.2f, 1.2f, 1.8f) * LaserSize / 20f);

					if (Main.GameUpdateCount % 10 == 0)
						SoundHelper.PlayPitched("Magic/LightningChargeShort", 0.5f, Main.rand.NextFloat(0.5f, 1f), mid);
				}
			}

			// Out mode
			if (State == 1)
			{
				// Scan laser for out mode
				if (LaserSize > 0)
				{
					CalculateLaserEnd();

					// We recalculate our partners end laser here too so the rotation for it isnt off,
					// since we dont control the update order of the projectiles we have to do this on each
					if (partner.ModProjectile is PlasmaCutterDrone drone)
						drone.CalculateLaserEnd();

					float dist = Vector2.Distance(Projectile.Center, outLaserEnd);

					for (int k = 0; k < dist; k += 32)
					{
						var pos = Vector2.Lerp(Projectile.Center, outLaserEnd, k / dist);
						Lighting.AddLight(pos, new Vector3(0.2f, 0.2f, 0.4f) * LaserSize / 20f);
					}

					Dust.NewDustPerfect(outLaserEnd, ModContent.DustType<Dusts.PixelatedImpactLineDustGlow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), 0, new Color(100, 100, 255, 0), Main.rand.NextFloat(0.1f));

					if (Main.GameUpdateCount % 10 == 0)
						SoundHelper.PlayPitched("Magic/LightningChargeShort", 0.15f, Main.rand.NextFloat(0.8f, 1f), Projectile.Center);
				}
			}

			if (animTimer > 0)
				animTimer--;
		}

		public void CalculateLaserEnd()
		{
			for (int k = 0; k < 150; k++)
			{
				Vector2 check = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 3.14f) * k * 8;
				if (Helpers.CollisionHelper.PointInTile(check))
				{
					outLaserEnd = check;
					break;
				}

				outLaserEnd = check;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			// Draw convergent laser
			if (LaserSize > 0 && Direction == 1 && State == 0)
			{
				ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
				{
					Texture2D laser = Assets.MagicPixel.Value;
					Vector2 mid = Vector2.Lerp(Projectile.Center, partner.Center, 0.5f) - Main.screenPosition;
					float dist = Vector2.Distance(Projectile.Center, partner.Center) - 23;

					float sizeProg = LaserSize / 20f;

					float pulse = 0.9f + MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;
					float pulse2 = 0.9f + MathF.Sin(Main.GameUpdateCount * 0.2f) * 0.1f;

					Rectangle target0 = new((int)mid.X, (int)mid.Y, (int)dist, (int)(LaserSize * 0.65f * pulse));
					Rectangle target1 = new((int)mid.X, (int)mid.Y, (int)dist, (int)(LaserSize * 0.4f * pulse));

					Main.spriteBatch.Draw(laser, target0, null, new Color(150, 150, 255) * sizeProg, Projectile.rotation, laser.Size() / 2f, 0, 0);
					Main.spriteBatch.Draw(laser, target1, null, Color.White * sizeProg, Projectile.rotation, laser.Size() / 2f, 0, 0);

					Texture2D glow = Assets.Masks.GlowAlpha.Value;
					Texture2D star = Assets.Masks.StarAlpha.Value;

					Main.spriteBatch.Draw(glow, mid, null, new Color(150, 150, 255, 0) * sizeProg, 0f, glow.Size() / 2f, 0.8f * sizeProg * pulse2, 0, 0);
					Main.spriteBatch.Draw(glow, mid, null, new Color(255, 255, 255, 0) * sizeProg, 0f, glow.Size() / 2f, 0.6f * sizeProg * pulse2, 0, 0);

					Main.spriteBatch.Draw(star, mid, null, new Color(150, 150, 255, 0) * sizeProg, 0f, star.Size() / 2f, 1.4f * sizeProg * pulse2, 0, 0);
					Main.spriteBatch.Draw(star, mid, null, new Color(255, 255, 255, 0) * sizeProg, 0f, star.Size() / 2f, 1.0f * sizeProg * pulse2, 0, 0);
				});
			}

			// Draw divergent laser
			if (LaserSize > 0 && State == 1)
			{
				ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
				{
					Texture2D laser = Assets.MagicPixel.Value;

					float sizeProg = LaserSize / 20f;

					float dist = Vector2.Distance(Projectile.Center, outLaserEnd);

					float pulse = 0.9f + MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;
					float pulse2 = 0.9f + MathF.Sin(Main.GameUpdateCount * 0.2f) * 0.1f;

					Main.spriteBatch.Draw(laser, Projectile.Center - Main.screenPosition, null, new Color(150, 150, 255) * sizeProg, Projectile.rotation - 3.14f, new Vector2(0, laser.Height / 2f), new Vector2(dist, LaserSize * 0.3f * pulse) / laser.Size(), 0, 0);
					Main.spriteBatch.Draw(laser, Projectile.Center - Main.screenPosition, null, Color.White * sizeProg, Projectile.rotation - 3.14f, new Vector2(0, laser.Height / 2f), new Vector2(dist, LaserSize * 0.15f * pulse) / laser.Size(), 0, 0);

					Texture2D glow = Assets.Masks.GlowAlpha.Value;
					Texture2D star = Assets.Masks.StarAlpha.Value;
					Vector2 mid = outLaserEnd - Main.screenPosition;

					Main.spriteBatch.Draw(glow, mid, null, new Color(150, 150, 255, 0) * sizeProg, 0f, glow.Size() / 2f, 0.4f * sizeProg * pulse2, 0, 0);
					Main.spriteBatch.Draw(glow, mid, null, new Color(255, 255, 255, 0) * sizeProg, 0f, glow.Size() / 2f, 0.2f * sizeProg * pulse2, 0, 0);

					Main.spriteBatch.Draw(star, mid, null, new Color(150, 150, 255, 0) * sizeProg, 0f, star.Size() / 2f, 0.6f * sizeProg * pulse2, 0, 0);
					Main.spriteBatch.Draw(star, mid, null, new Color(255, 255, 255, 0) * sizeProg, 0f, star.Size() / 2f, 0.4f * sizeProg * pulse2, 0, 0);
				});
			}

			Texture2D tex = Assets.Items.MechBoss.PlasmaCutterDrone.Value;

			int xFrame = 0;
			int yFrame = 0;

			if (State == 0)
			{
				if (LaserSize > 0)
				{
					yFrame = 1;
					xFrame = Main.GameUpdateCount % 10 <= 5 ? 0 : 1;
				}
				else if (animTimer > 0)
				{
					yFrame = 2;
					xFrame = (int)(animTimer / 20f * 3);
				}
				else
				{
					yFrame = 0;
					xFrame = 0;
				}
			}
			else if (State == 1)
			{
				if (LaserSize > 0)
				{
					yFrame = 4;
					xFrame = Main.GameUpdateCount % 10 <= 5 ? 0 : 1;
				}
				else if (animTimer > 0)
				{
					yFrame = 5;
					xFrame = (int)(animTimer / 20f * 3);
				}
				else
				{
					yFrame = 3;
					xFrame = 0;
				}
			}

			var frame = new Rectangle(46 * xFrame, 46 * yFrame, 46, 46);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, 3.14f + Projectile.rotation, frame.Size() / 2f, Projectile.scale, Direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0);

			return false;
		}
	}

	class PlasmaCutterBuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Buffs + "PlasmaCutterBuff";

		public PlasmaCutterBuff() : base("Plasma Destabilization", "Effected entities have 75% exposure to magic damage", true) { }

		public override void Update(NPC NPC, ref int buffIndex)
		{
			if (Main.rand.NextBool(4))
				Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<PixelatedEmber>(), 0, 0, 0, new Color(40, 40, 85, 0), Main.rand.NextFloat(0.15f));

			Dust.NewDustPerfect(NPC.Center, ModContent.DustType<PixelatedImpactLineDust>(), Vector2.UnitX.RotatedByRandom(6.28f), 0, new Color(20, 20, 80, 0), 0.1f);

			NPC.GetGlobalNPC<ExposureNPC>().ExposureMultMagic += 0.75f;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetModPlayer<ExposurePlayer>().exposureMult += 0.75f;
		}
	}
}
