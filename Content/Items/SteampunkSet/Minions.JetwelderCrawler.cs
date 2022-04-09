using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using System;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.SteampunkSet
{
    public class JetwelderCrawler : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + "JetwelderCrawler";

		private readonly int STARTTIMELEFT = 1200;

        private Player Player => Main.player[Projectile.owner];

		private Vector2 moveDirection;
		private Vector2 newVelocity = Vector2.Zero;

		private bool flipVertical = false;

		private int windup;
		private int shootDistance;

		private bool collideX;
		private bool collideY;

		private float gunRotation;
		private float gunRotationToBe;
		private bool flipGun = false;
		private int gunFrame = 3;
		private int totalGunFrames = 4;

		private bool firing = false;

		private int attackCounter = 0;
		private int SPEED => 3;

		public override void Load()
		{
			for (int k = 1; k <= 7; k++)
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore" + k);
		}

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crawler");
            Main.projFrames[Projectile.type] = 9;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = STARTTIMELEFT;
			shootDistance = Main.rand.Next(300, 500);
			windup = Main.rand.Next(40, 80);
			Projectile.ignoreWater = true;
        }

		public override bool? CanHitNPC(NPC target) => false;

		public override bool PreDraw(ref Color lightColor)
        {
			var spriteBatch = Main.spriteBatch;

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

			SpriteEffects effects = flipVertical ? SpriteEffects.FlipVertically : SpriteEffects.None;
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation % 6.28f, tex.Size() / new Vector2(2, 2 * Main.projFrames[Projectile.type]), Projectile.scale, effects, 0f);

			Texture2D gunTex = ModContent.Request<Texture2D>(Texture + "_Gun").Value;
			Texture2D flashTex = ModContent.Request<Texture2D>(Texture + "_Gun_Flash").Value;
			SpriteEffects gunEffects = flipGun ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			int gunFrameHeight = gunTex.Height / totalGunFrames;
			Rectangle gunFrameBox = new Rectangle(0, gunFrameHeight * gunFrame, gunTex.Width, gunFrameHeight);
			Vector2 gunOrigin = new Vector2(flipGun ? gunTex.Width - 22 : 22, 7);
			spriteBatch.Draw(gunTex, (Projectile.Center - Main.screenPosition) + (GunOffset().RotatedBy(Projectile.rotation)), gunFrameBox, lightColor, gunRotation - (flipGun ? 3.14f : 0), gunOrigin, Projectile.scale, gunEffects, 0f);
			spriteBatch.Draw(flashTex, (Projectile.Center - Main.screenPosition) + (GunOffset().RotatedBy(Projectile.rotation)), gunFrameBox, Color.White, gunRotation - (flipGun ? 3.14f : 0), gunOrigin, Projectile.scale, gunEffects, 0f);

			return false;
        }

        public override void AI()
        {
			NPC testtarget = Main.npc.Where(n => n.active && n.CanBeChasedBy(Projectile, false) && Vector2.Distance(n.Center, Projectile.Center) < 800 && ClearPath(n.Center,Projectile.Center)).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();

			int distance = 1000;
			if (testtarget != default)
			{
				gunRotationToBe = (Vector2.Zero - testtarget.DirectionTo(Projectile.Center  +(GunOffset().RotatedBy(Projectile.rotation)))).ToRotation();
				float rotDifference = ((((gunRotationToBe - gunRotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;
				gunRotation = MathHelper.Lerp(gunRotation, gunRotation + rotDifference, 0.2f);
				flipGun = gunRotation.ToRotationVector2().X < 0;

				distance = (int)(Projectile.Center - testtarget.Center).Length();
			}
			firing = distance < (shootDistance + (firing ? 50 : 0)) && Projectile.timeLeft < STARTTIMELEFT - windup;
			FindFrame();
			if (firing)
				Attack();
			else
			{
				Crawl();
			}
        }

		public override void Kill(int timeLeft)
		{
			for (int i = 1; i < 8; i++)
			{
				//Gore.NewGore(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2), Main.rand.NextVector2Circular(5, 5), Mod.Find<ModGore>(Texture + "_Gore" + i.ToString()).Type, 1f); //PORTTODO: Fix default gores
			}
		}

		private void Crawl()
		{
			attackCounter = 0;
			newVelocity = Collide();
			if (Math.Abs(newVelocity.X) < 0.5f)
				collideX = true;
			else
				collideX = false;
			if (Math.Abs(newVelocity.Y) < 0.5f)
				collideY = true;
			else
				collideY = false;

			RotateCrawl();

			if (Projectile.ai[0] == 0f)
			{
				moveDirection.Y = 1;
				moveDirection.X = Main.rand.NextBool() ? 1 : -1;
				if (moveDirection.X == -1)
					flipVertical = true;

				Projectile.rotation = moveDirection.ToRotation();
				Projectile.ai[0] = 1f;
			}

			if (Projectile.ai[1] == 0f)
			{
				if (collideY)
					Projectile.ai[0] = 2f;

				if (!collideY && Projectile.ai[0] == 2f)
				{
					moveDirection.X = -moveDirection.X;
					Projectile.ai[1] = 1f;
					Projectile.ai[0] = 1f;
				}
				if (collideX)
				{
					moveDirection.Y = -moveDirection.Y;
					Projectile.ai[1] = 1f;
				}
			}
			else
			{
				Projectile.rotation -= (moveDirection.X * moveDirection.Y) * 0.13f;

				if (collideX)
					Projectile.ai[0] = 2f;

				if (!collideX && Projectile.ai[0] == 2f)
				{
					moveDirection.Y = -moveDirection.Y;
					Projectile.ai[1] = 0f;
					Projectile.ai[0] = 1f;
				}
				if (collideY)
				{
					moveDirection.X = -moveDirection.X;
					Projectile.ai[1] = 0f;
				}
			}
			Projectile.velocity = SPEED * moveDirection;
			Projectile.velocity = Collide();
		}

		private Vector2 Collide() => Collision.noSlopeCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, true, true);

		private void RotateCrawl()
		{
			float rotDifference = ((((Projectile.velocity.ToRotation() - Projectile.rotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;
			if (Math.Abs(rotDifference) < 0.15f)
			{
				Projectile.rotation = Projectile.velocity.ToRotation();
				return;
			}
			Projectile.rotation += Math.Sign(rotDifference) * 0.2f;
		}

		private void Attack()
        {
			RotateCrawl();
			Projectile.Center -= Projectile.velocity;
			attackCounter++;
			if (attackCounter % 7 == 0 && attackCounter % 56 < 15)
            {
				Vector2 dir = gunRotation.ToRotationVector2();

				gunFrame = 0;
				Projectile.frameCounter = 0;

				Vector2 pos = Projectile.Center + (GunOffset().RotatedBy(Projectile.rotation));
				//Gore.NewGore(pos, new Vector2(Math.Sign(dir.X) * -1, -0.5f) * 2, Mod.Find<ModGore>(AssetDirectory.MiscItem + "CoachGunCasing").Type, 1f); //PORTTODO: Fix default gores
				gunRotation -= Math.Sign(dir.X) * 0.3f;
				Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), pos, dir.RotatedByRandom(0.1f) * 15, ProjectileID.Bullet, Projectile.damage, Projectile.knockBack, Player.whoAmI);
            }
        }

		private void FindFrame()
        {
			Projectile.frameCounter++;
			if (Projectile.frameCounter % 3 == 0)
				Projectile.frame++;

			if (Projectile.frameCounter % 4 == 0 && gunFrame < totalGunFrames - 1)
				gunFrame++;
			Projectile.frame %= Main.projFrames[Projectile.type];
			if (firing)
				Projectile.frame = 0;
		}

		private Vector2 GunOffset()
        {
			Vector2 ret = Vector2.Zero;
			ret.X = -3;
			ret.Y = -16;

			if (Projectile.frame == 0 || Projectile.frame == 4)
				ret.Y = -14;

			if (flipGun == flipVertical)
				ret.X *= -1;

			ret.Y *= (flipVertical ? -1 : 1);

			return ret;
        }

		private bool ClearPath(Vector2 point1, Vector2 point2)
		{
			Vector2 direction = point2 - point1;
			for (int i = 0; i < direction.Length(); i += 4)
			{
				Vector2 toLookAt = point1 + (Vector2.Normalize(direction) * i);

				if ((Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).HasTile && Main.tileSolid[Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).TileType]))
					return false;
			}
			return true;
		}
	}
}