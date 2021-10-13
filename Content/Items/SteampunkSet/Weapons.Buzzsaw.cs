using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.SteampunkSet
{
	public class Buzzsaw : ModItem
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public override Vector2? HoldoutOffset() => new Vector2(-15, 0);

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Steamsaw");
			Tooltip.SetDefault("Strike enemies to build up pressure\nRelease to vent the pressure, launching the sawblade\n'The right tool for the wrong job'");
		}

		public override void SetDefaults()
		{
			item.damage = 30;
			item.width = 65;
			item.height = 21;
			item.useTime = 65;
			item.useAnimation = 65;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.noMelee = true;
			item.knockBack = 1.5f;
			item.value = Item.sellPrice(0, 1, 0, 0);
			item.rare = 3;
			item.autoReuse = false;
			item.shoot = ModContent.ProjectileType<BuzzsawProj>();
			item.shootSpeed = 20f;
			item.melee = true;
			item.channel = true;
			item.noUseGraphic = true;
			//item.UseSound = SoundID.DD2_SkyDragonsFuryShot;
		}
	}

	public class BuzzsawProj : ModProjectile
	{
		private const int OFFSET = 30;
		private const int MAXCHARGE = 20;

		public Vector2 direction = Vector2.Zero;

		private int counter;
		private float bladeRotation;
		private int charge;
		private bool released = false;

		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public override void SetStaticDefaults() => DisplayName.SetDefault("Steamsaw");

		public override void SetDefaults()
		{
			projectile.hostile = false;
			projectile.melee = true;
			projectile.width = 32;
			projectile.height = 32;
			projectile.aiStyle = -1;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.timeLeft = 999999;
			projectile.ignoreWater = true;
			projectile.alpha = 255;
			Main.projFrames[projectile.type] = 5;
		}

		public override void AI()
		{
			Player player = Main.player[projectile.owner];

			projectile.velocity = Vector2.Zero;
			projectile.timeLeft = 2;
			player.itemTime = 5; // Set item time to 2 frames while we are used
			player.itemAnimation = 5; // Set item animation time to 2 frames while we are used

			if (player.direction != 1)
				player.itemRotation -= 3.14f;

			float shake = 0;

			if (player.channel && !released)
			{
				bladeRotation += 1.2f;
				player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);
				shake = MathHelper.Lerp(0.04f, 0.15f, (float)charge / (float)MAXCHARGE);
				direction = Main.MouseWorld - (player.MountedCenter);
				direction.Normalize();
				counter++;
				projectile.frame = ((counter / 5) % 2) + 2;

				if (counter % 30 == 1)
					Main.PlaySound(2, projectile.Center, 22); //Chainsaw sound

				ReleaseSteam(player);
			}
			else
			{
				projectile.friendly = false;
				projectile.frame = 5;

				if (!released)
					LaunchSaw(player);
				else if (player.ownedProjectileCounts[ModContent.ProjectileType<BuzzsawProj2>()] == 0)
					projectile.active = false;
			}

			projectile.Center = player.MountedCenter + (direction * OFFSET * Main.rand.NextFloat(1 - shake, 1 + shake));
			projectile.velocity = Vector2.Zero;
			player.itemRotation = direction.ToRotation();

			if (player.direction != 1)
				player.itemRotation -= 3.14f;

			player.itemRotation = MathHelper.WrapAngle(player.itemRotation);

			player.heldProj = projectile.whoAmI;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (charge < MAXCHARGE)
				charge++;

			for (int i = 0; i < 2; i++)
			{
				if (!Helper.IsFleshy(target))
				{
					for (int k = 0; k < 10; k++)
					{
						Dust.NewDustPerfect((projectile.Center + (direction * 10)) + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.Next(3, 20), 0, new Color(255, 255, 60) * 0.8f, 1.6f);
					}

					Dust.NewDustPerfect((projectile.Center + (direction * 10)), ModContent.DustType<Dusts.Glow>(), direction.RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f) - 1.57f) * Main.rand.Next(3, 10), 0, new Color(150, 80, 30), 0.2f);
				}
				else
				{
					for (int j = 0; j < 15; j++)
					{
						Dust.NewDustPerfect(projectile.Center + (direction * 15), DustID.Blood, direction.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * Main.rand.NextFloat(0f, 6f), 0, default, 1.5f);
						Dust.NewDustPerfect(projectile.Center + (direction * 15), DustID.Blood, direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.NextFloat(0f, 3f), 0, default, 0.8f);
					}
				}
			}

			hitDirection = Math.Sign(direction.X);
			base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) //extremely messy code I ripped from a weapon i made for spirit :trollge:
		{
			Player player = Main.player[projectile.owner];
			Texture2D texture = Main.projectileTexture[projectile.type];
			Texture2D texture2 = ModContent.GetTexture(Texture + "_Blade");
			int height1 = texture.Height;
			int height2 = texture2.Height / Main.projFrames[projectile.type];
			int y2 = height2 * projectile.frame;
			Vector2 origin = new Vector2((float)texture.Width / 2f, (float)height1 / 2f);
			Vector2 position = (projectile.position - (0.5f * (direction * OFFSET)) + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition).Floor();

			if (!released)
				spriteBatch.Draw(texture2, projectile.Center - Main.screenPosition, new Rectangle(0, y2, texture2.Width, height2), lightColor, bladeRotation, new Vector2(15, 15), projectile.scale, SpriteEffects.None, 0.0f);

			if (player.direction == 1)
			{
				SpriteEffects effects1 = SpriteEffects.None;
				spriteBatch.Draw(texture, position, null, lightColor, direction.ToRotation(), origin, projectile.scale, effects1, 0.0f);

			}
			else
			{
				SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
				spriteBatch.Draw(texture, position, null, lightColor, direction.ToRotation() - 3.14f, origin, projectile.scale, effects1, 0.0f);
			}

			return false;
		}

		private void LaunchSaw(Player player)
		{
			released = true;
			float speed = MathHelper.Lerp(8f, 14f, charge / (float)MAXCHARGE);
			float damageMult = MathHelper.Lerp(0.75f, 2f, charge / (float)MAXCHARGE);
			Projectile.NewProjectile(projectile.Center, direction * speed, ModContent.ProjectileType<BuzzsawProj2>(), (int)(projectile.damage * damageMult), projectile.knockBack, projectile.owner);
		}

		private void ReleaseSteam(Player player)
        {
			float alphaMult = MathHelper.Lerp(0.75f, 3f, charge / (float)MAXCHARGE);
			Dust.NewDustPerfect(Vector2.Lerp(projectile.Center, player.Center, 0.75f), ModContent.DustType<Dusts.BuzzsawSteam>(), new Vector2(0.2f, -Main.rand.NextFloat(0.7f, 1.6f)), (int)(Main.rand.Next(15) * alphaMult), Color.White, Main.rand.NextFloat(0.2f, 0.5f));
		}
	}
	public class BuzzsawProj2 : ModProjectile
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		private float rotationCounter;

		private int counter;

		private Vector2 oldVel;

		public int pauseTimer = -1;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Buzzsaw");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			projectile.width = 30;
			projectile.height = 30;
			projectile.aiStyle = 3;
			projectile.friendly = false;
			projectile.melee = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 700;
			Main.projFrames[projectile.type] = 2;
			projectile.extraUpdates = 1;
		}

		public override bool PreAI()
		{
			if (--pauseTimer > 0)
			{
				if (projectile.velocity != Vector2.Zero)
					oldVel = projectile.velocity;

				projectile.velocity = Vector2.Zero;
				return false;
			}

			if (pauseTimer == 0)
				projectile.velocity = oldVel;

			return true;
		}

		public override void AI()
		{
			if (counter == 0)
				Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<PhantomBuzzsaw>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.whoAmI);

			counter++;
			projectile.frameCounter += 1;
			projectile.frame = (projectile.frameCounter / 5) % 2;
			rotationCounter += 0.6f;
			projectile.rotation = rotationCounter;
		}

	}
	public class PhantomBuzzsaw : ModProjectile
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		private Projectile parent => Main.projectile[(int)projectile.ai[0]];

		private Player player => Main.player[projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Buzzsaw");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			projectile.width = 30;
			projectile.height = 30;
			projectile.friendly = true;
			projectile.melee = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 700;
			Main.projFrames[projectile.type] = 2;
			projectile.extraUpdates = 1;
			projectile.hide = true;
		}

        public override void AI()
        {
			projectile.Center = parent.Center;
			projectile.velocity = parent.velocity;

			if (!parent.active)
				projectile.active = false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {

			Vector2 direction = target.Center - projectile.Center;
			direction.Normalize();
			for (int i = 0; i < 2; i++)
			{

				if (!Helper.IsFleshy(target))
					Dust.NewDustPerfect((projectile.Center + (direction * 10)) + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f) + 1.57f) * Main.rand.Next(15, 20), 0, new Color(255, 230, 60) * 0.8f, 1.6f);
				else
				{
					Helper.PlayPitched("Impacts/StabTiny", 0.8f, Main.rand.NextFloat(-0.3f, 0.3f), target.Center);

					for (int j = 0; j < 2; j++)
						Dust.NewDustPerfect(projectile.Center + (direction * 15), ModContent.DustType<GraveBlood>(), direction.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * Main.rand.NextFloat(0.5f, 5f));
				}

			}
			if (Helper.IsFleshy(target))
            {
				int bloodID = ModContent.ProjectileType<BuzzsawBlood1>();
				int spriteDirection = Math.Sign(direction.X);

				Projectile proj = Projectile.NewProjectileDirect(target.Center, Vector2.Zero, bloodID, 0, 0, projectile.owner);
				proj.spriteDirection = -spriteDirection;
			}

			player.GetModPlayer<StarlightPlayer>().Shake += 6;
			target.immune[projectile.owner] = 20;

			if (parent.modProjectile is BuzzsawProj2 modProj)
				modProj.pauseTimer = 16;
		}
    }

	public class BuzzsawBlood1 : ModProjectile
    {
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Buzzsaw");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			projectile.width = 90;
			projectile.height = 90;
			projectile.friendly = false;
			projectile.melee = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 700;
			projectile.rotation = Main.rand.NextFloat(0.78f);
			SetFrames();
		}

        public override void AI()
        {
			if (projectile.ai[0]++ == 0)
				projectile.position -= new Vector2(-projectile.spriteDirection * 20, 28).RotatedBy(projectile.rotation);
			projectile.velocity = Vector2.Zero;
			projectile.frameCounter++;
			if (projectile.frameCounter > 4)
            {
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= Main.projFrames[projectile.type])
					projectile.active = false;
            }
        }
        protected virtual void SetFrames()
        {
			Main.projFrames[projectile.type] = 3;
		}
	}
}