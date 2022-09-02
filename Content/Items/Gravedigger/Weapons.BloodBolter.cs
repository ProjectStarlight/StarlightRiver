using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace StarlightRiver.Content.Items.Gravedigger
{
	public class BloodBolter : ModItem
	{
		public override string Texture => AssetDirectory.GravediggerItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bloodbolter");
			Tooltip.SetDefault("Converts wooden arrows into bloodbolts \nBloodbolts impale dead fleshy enemies, exploding them on surfaces");
		}

		public override void SetDefaults()
		{
			Item.damage = 21;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 4;
			Item.useAnimation = 4;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.value = Item.sellPrice(0, 0, 20, 0);
			Item.crit = 7;
			Item.rare = ItemRarityID.Green;
			Item.shoot = 7;
			Item.shootSpeed = 12f;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
            Item.useAmmo = AmmoID.Arrow;
			Item.UseSound = SoundID.Item5;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<BloodBolterHeldProj>(), 0, 0, player.whoAmI, velocity.ToRotation());

			if (type == ProjectileID.WoodenArrowFriendly)
			{
				velocity *= 1.5f;
				type = ModContent.ProjectileType<BloodBolt>();
			}

			Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback,player.whoAmI);
			return false;
        }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<LivingBlood>(), 12);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class BloodBolterHeldProj : ModProjectile
	{

		public override string Texture => AssetDirectory.GravediggerItem + Name;

		private Player Owner => Main.player[Projectile.owner];

		ref float OriginalRotation => ref Projectile.ai[0];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blood bolter");
			Main.projFrames[Projectile.type] = 7;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(32, 32);
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = Owner.Center;
			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = Owner.itemAnimation = 2;

			Vector2 direction = OriginalRotation.ToRotationVector2();
			Owner.direction = Math.Sign(direction.X);
			Projectile.rotation = OriginalRotation;

			int frameTicker = 3;

			if (Projectile.frame == Main.projFrames[Projectile.type] - 1)
				frameTicker = 30;

			Projectile.frameCounter++;

			if (Projectile.frameCounter >= frameTicker)
			{
				Projectile.frame++;
				Projectile.frameCounter = 0;
			}

			if (Projectile.frame >= Main.projFrames[Projectile.type])
				Projectile.active = false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

			Vector2 origin = new Vector2(10, frameHeight * 0.5f);
			float rotation = Projectile.rotation;
			SpriteEffects effects = SpriteEffects.None;

			if (Owner.direction == -1)
			{
				origin = new Vector2(tex.Width - origin.X, origin.Y);
				rotation += 3.14f;
				effects = SpriteEffects.FlipHorizontally;
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, rotation, origin, Projectile.scale, effects, 0f);
			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, frame, Color.White, rotation, origin, Projectile.scale, effects, 0f);
			return false;
		}
	}

	internal class BloodBolt : ModProjectile
	{
		public override string Texture => AssetDirectory.GravediggerItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blood Bolt");
		}
		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 60;
			Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
			Projectile.rotation = Projectile.velocity.ToRotation();

			if (!Projectile.friendly)
				Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Main.rand.NextVector2Circular(2, 2) + (Projectile.velocity / 2), 0, default, 1.25f);
        }

        public override bool PreDraw(ref Color lightColor)
		{
			var spriteBatch = Main.spriteBatch;
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) //ModifyHitNPC so it runs before enemies prehurt method
		{
			BloodBolterGNPC GNPC = target.GetGlobalNPC<BloodBolterGNPC>();
			GNPC.hitFromBolter = true;
			GNPC.boltOffset = target.Center - Projectile.Center;
			GNPC.bolt = Projectile;

			GoreDestroyerNPC goreDestroyerNPC = target.GetGlobalNPC<GoreDestroyerNPC>();
			goreDestroyerNPC.destroyGore = true;
		}

        public override void Kill(int timeLeft)
        {
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
            for (int i = 0; i < 12; i++)
            {
				Vector2 dir = Vector2.Normalize(-Projectile.velocity).RotatedByRandom(0.8f);
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), DustID.Blood, dir * Main.rand.NextFloat(7), 0, default, 1.25f);
            }
        }
    }

	internal class BloodBolterExplosion : ModProjectile
	{
		public override string Texture => AssetDirectory.Assets + "Invisible";

		public float radiusMult = 1f;

		public float Progress => 1 - (Projectile.timeLeft / 10f);

		private float Radius => 150 * (float)(Math.Sqrt(Progress)) * radiusMult;

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 10;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blood Bolter");
		}

		public override bool PreDraw(ref Color lightColor) => false;

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius;

			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
				return true;

			return false;
		}
	}

	public class BloodBolterGNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public bool hitFromBolter = false;

		public Projectile bolt = default;
		public Vector2 boltOffset = Vector2.Zero;

		public bool markedForDeath = false;

		public int deathCounter = 0;

		public override void ResetEffects(NPC npc)
		{
			hitFromBolter = false;
		}

		public override void PostAI(NPC npc)
		{
			if (markedForDeath && bolt != default)
			{
				npc.Center = bolt.Center + boltOffset + new Vector2(3, 3);
				npc.velocity = bolt.velocity;

				deathCounter++;

				if (!bolt.active || ((npc.collideX || npc.collideY) && deathCounter > 2))
				{
					bolt.active = false;
					npc.Kill();
					SpawnBlood(npc, bolt);
				}
			}
		}

		public override bool PreKill(NPC npc)
		{
			if (hitFromBolter)
				return false;

			return base.PreKill(npc);
		}

		public override bool CheckDead(NPC npc)
		{
			if (hitFromBolter && !markedForDeath && npc.knockBackResist != 0 && Helper.IsFleshy(npc))
			{
				Helper.PlayPitched("Impale", 0.5f, Main.rand.NextFloat(-0.1f, 0.1f), npc.Center);
				npc.life = 1;
				markedForDeath = true;
				npc.immortal = true;
				npc.noTileCollide = false;
				npc.noGravity = true;

				npc.width -= 6;
				npc.height -= 6;

				if (bolt != default)
				{
					bolt.friendly = false;
					bolt.penetrate++;
				}
				return false;
			}
			return base.CheckDead(npc);
		}

		private static void SpawnBlood(NPC npc, Projectile projectile)
		{
			Helper.PlayPitched("Impacts/GoreHeavy", 0.5f, Main.rand.NextFloat(-0.1f, 0.1f), npc.Center);
			Core.Systems.CameraSystem.Shake += 8;
			Vector2 direction = -Vector2.Normalize(projectile.velocity);

			for (int i = 0; i < 16; i++)
			{
				Dust.NewDustPerfect(npc.Center - projectile.velocity + Main.rand.NextVector2Circular(npc.width / 2, npc.height / 2), DustID.Blood, Main.rand.NextVector2Circular(5, 5), 0, default, 1.4f);
				Dust.NewDustPerfect(npc.Center - projectile.velocity + Main.rand.NextVector2Circular(npc.width / 2, npc.height / 2), DustID.Blood, direction.RotatedBy(Main.rand.NextFloat(-0.9f, 0.9f)) * Main.rand.NextFloat(3, 8), 0, default, 2.1f);
				Dust.NewDustPerfect(npc.Center - projectile.velocity + Main.rand.NextVector2Circular(npc.width / 2, npc.height / 2), ModContent.DustType<BloodMetaballDust>(), direction.RotatedBy(Main.rand.NextFloat(-0.9f, 0.9f)) * Main.rand.NextFloat(3, 8), 0, default, 0.3f);
				Dust.NewDustPerfect(npc.Center - projectile.velocity + Main.rand.NextVector2Circular(npc.width / 2, npc.height / 2), ModContent.DustType<BloodMetaballDustLight>(), direction.RotatedBy(Main.rand.NextFloat(-0.9f, 0.9f)) * Main.rand.NextFloat(3, 8), 0, default, 0.3f);
			}

			for (int i = 0; i < 8; i++)
			{
				Dust.NewDustPerfect(npc.Center - projectile.velocity + Main.rand.NextVector2Circular(npc.width / 2, npc.height / 2), ModContent.DustType<SmokeDustColor>(), Main.rand.NextVector2Circular(3, 3), 0, Color.DarkRed, Main.rand.NextFloat(1, 1.5f));
			}

			Projectile.NewProjectile(new EntitySource_HitEffect(npc), npc.Center, Vector2.Zero, ModContent.ProjectileType<BloodBolterExplosion>(), (int)(projectile.damage * 1.6f), projectile.knockBack, projectile.owner);
		}
	}
}