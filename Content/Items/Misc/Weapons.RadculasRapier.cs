using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class RadculasRapier : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override bool CanUseItem(Player player)
		{
			return player.ownedProjectileCounts[ModContent.ProjectileType<RadculasRapierSwungBlade>()] <= 0;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override void SetStaticDefaults	()
		{
			DisplayName.SetDefault("Radcula's Rapier");
			Tooltip.SetDefault("Rapidly stabs enemies, stealing the lifeforce of struck foes\n" +
				"Press <right> to teleport to the cursor, slashing all those in the way\nThe more enemies you strike with the dash, the lower the cooldown");
		}

		public override void SetDefaults()
		{
			Item.damage = 30;
			Item.DamageType = DamageClass.Melee;

			Item.useTime = 30;
			Item.useAnimation = 30; 

			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 3.5f;
			Item.shootSpeed = 5f;

			Item.shoot = ModContent.ProjectileType<RadculasRapierSwungBlade>();
			Item.noUseGraphic = true;
			Item.noMelee = true;

			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Orange;

			Item.channel = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false;
		}
	}

	class RadculasRapierSwungBlade : ModProjectile
	{
		public struct afterImageStruct
		{
			public Vector2 pos;
			public float rot;
			public int time;

			public afterImageStruct(Vector2 pos, float rot, int time)
			{
				this.pos = pos;
				this.rot = rot;
				this.time = time;
			}
		}

		public float pullBack = 0.8f;

		public int maxTimeLeft;

		public int amountStabs;

		public bool flurry;

		public bool fading;

		public bool playedSound;

		public Vector2 offset;

		public Vector2 stabVec;

		public List<NPC> hitNPCs = new();

		public List<afterImageStruct> afterImages = new(); // position of afterimage, rotation of afterimage, timer for afterimage

		public float progress => Projectile.timeLeft / (float)maxTimeLeft;

		public override string Texture => AssetDirectory.MiscItem + "RadculasRapier_Spear";

		public Player Owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Radculas Rapier");
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.hostile = false;

			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;

			Projectile.Size = new Vector2(16);

			Projectile.penetrate = -1;
			Projectile.ownerHitCheck = true;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.timeLeft = (int)(Owner.HeldItem.useAnimation * (1f / Owner.GetTotalAttackSpeed(DamageClass.Melee)));
			maxTimeLeft = Projectile.timeLeft;
		}

		public override void AI()
		{
			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = 2;

			Projectile.velocity = Vector2.Normalize(Projectile.velocity);

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (!Projectile.friendly)
				Projectile.friendly = true;

			int stabTimer = maxTimeLeft - Projectile.timeLeft;

			if ((amountStabs < 5 || Owner.channel) && !fading)
			{
				float totalTime = maxTimeLeft * 0.2f; //total amount of frames the stab lasts

				if (stabTimer < totalTime)
				{
					if (stabTimer == 1) //things are initialized here each stab, such as the random rotation, and the randomness of the length of the stab
					{
						Projectile.velocity = Owner.DirectionTo(Main.MouseWorld).RotatedByRandom(0.35f);
						stabVec = new Vector2(Main.rand.NextFloat(90, 150), 0);
						hitNPCs.Clear();
					}
					if (stabTimer < totalTime * 0.5f)
					{
						float lerper = stabTimer / (totalTime * 0.5f);

						offset = Vector2.Lerp(new Vector2(75, 0), stabVec, EaseBuilder.EaseCircularOut.Ease(lerper)).RotatedBy(Projectile.rotation - MathHelper.PiOver2);
					}
					else
					{
						if (!playedSound)
						{
							afterImages.Add(new afterImageStruct(Projectile.Center, Projectile.rotation, 10));
							Helper.PlayPitched("Magic/ShurikenThrow", 1f, Main.rand.NextFloat(-0.3f, 0.3f), Owner.Center);
							playedSound = true;
						}

						float lerper = (stabTimer - totalTime * 0.5f) / (float)(totalTime * 0.5f);

						offset = Vector2.Lerp(stabVec, new Vector2(75, 0), EaseBuilder.EaseCircularOut.Ease(lerper)).RotatedBy(Projectile.rotation - MathHelper.PiOver2);
					}

				}
				else
				{
					playedSound = false;
					amountStabs++;
					Projectile.timeLeft = maxTimeLeft;
				}
				if (Main.rand.NextBool(5))
					Dust.NewDustPerfect(Vector2.Lerp(Owner.Center, Projectile.Center, Main.rand.NextFloat()), ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.Zero, 0, new Color(255, 0, 0, 100), 0.45f);
			}
			else if (!fading)
			{
				Projectile.timeLeft = 5;
				fading = true;
			}

			for (int i = 0; i < afterImages.Count; i++)
			{
				afterImageStruct afterImage = afterImages[i];
				afterImages[i] = new afterImageStruct(afterImage.pos, afterImage.rot, afterImage.time - 1);

				if (afterImages[i].time <= 0)
					afterImages.RemoveAt(i);
			}


			Projectile.Center = Owner.MountedCenter + offset;

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.Pi);
			Owner.ChangeDir(Projectile.direction);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			hitNPCs.Add(target);
		    CameraSystem.shake += 1;

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Projectile.Center.DirectionTo(Owner.Center).RotatedByRandom(0.35f) * Main.rand.NextFloat(0.5f, 2f), 0, new Color(150, 0, 0, 100), 0.55f);
			}

			if (Helper.IsFleshy(target))
			{
				Helper.PlayPitched("Impale", 1, Main.rand.NextFloat(0.6f, 0.9f), Projectile.Center);

				for (int k = 0; k < 5; k++)
				{
					Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Projectile.Center.DirectionTo(Owner.Center).RotatedByRandom(0.35f) * Main.rand.NextFloat(2f, 8f), 0, default, 1.5f);

					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GraveBlood>(), Projectile.Center.DirectionTo(Owner.Center).RotatedByRandom(0.35f) * Main.rand.NextFloat(2f, 8f), 0, default, 1.5f);
				}
			}
			else
			{
				Helper.PlayPitched("Impacts/Clink", 1, Main.rand.NextFloat(0.1f, 0.3f), Projectile.Center);

				for (int k = 0; k < 5; k++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Projectile.Center.DirectionTo(Owner.Center).RotatedByRandom(0.35f) * Main.rand.NextFloat(2f, 8f), 0, new Color(255, Main.rand.Next(130, 255), 80), Main.rand.NextFloat(0.3f, 0.5f));
				}
			}
		}

		public override bool? CanHitNPC(NPC target)
		{
			return Projectile.friendly && !hitNPCs.Contains(target);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			Vector2 off = new Vector2(0, -20).RotatedBy(Projectile.rotation - MathHelper.PiOver2);

			float fade = 1f;
			if (Projectile.timeLeft <= 5 && fading)
				fade = Projectile.timeLeft / 5f;

			Color color = new Color(100, 0, 0, 0) * fade;

			for (int i = 0; i < afterImages.Count; i++)
			{
				afterImageStruct afterImage = afterImages[i];

				float opacity = MathHelper.Lerp(0.5f, 0f, 1f - afterImage.time / 10f);

				Main.spriteBatch.Draw(texGlow, afterImage.pos - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY) + new Vector2(-55, 0).RotatedBy(afterImage.rot - MathHelper.PiOver2), null, color * opacity, afterImage.rot, texGlow.Size() / 2f, Projectile.scale, 0f, 0f);

				Main.spriteBatch.Draw(texGlow, afterImage.pos - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY) + new Vector2(-55, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2), null, color * opacity, afterImage.rot, texGlow.Size() / 2f, Projectile.scale, 0f, 0f);

				Main.spriteBatch.Draw(tex, afterImage.pos - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY) + off, null, Color.White * 0.25f * opacity, afterImage.rot, Vector2.Zero, Projectile.scale, 0f, 0f);
			}

			Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY) + new Vector2(-55, 0).RotatedBy(Projectile.rotation - MathHelper.PiOver2), null, color, Projectile.rotation, texGlow.Size() / 2f, Projectile.scale, 0, 0);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY) + off, null, Color.White * fade, Projectile.rotation, Vector2.Zero, Projectile.scale, 0, 0);
			return false;
		}
	}
}