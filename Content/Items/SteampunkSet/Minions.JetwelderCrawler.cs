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

        private Player player => Main.player[projectile.owner];

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

		public override bool Autoload(ref string name)
		{
			StarlightRiver.Instance.AddGore(Texture + "_Gore1");
			StarlightRiver.Instance.AddGore(Texture + "_Gore2");
			StarlightRiver.Instance.AddGore(Texture + "_Gore3");
			StarlightRiver.Instance.AddGore(Texture + "_Gore4");
			StarlightRiver.Instance.AddGore(Texture + "_Gore5");
			StarlightRiver.Instance.AddGore(Texture + "_Gore6");
			StarlightRiver.Instance.AddGore(Texture + "_Gore7");
			return base.Autoload(ref name);
		}

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crawler");
            Main.projFrames[projectile.type] = 9;
            ProjectileID.Sets.Homing[projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.aiStyle = -1;
            projectile.width = 26;
            projectile.height = 26;
            projectile.friendly = false;
            projectile.tileCollide = false;
            projectile.hostile = false;
            projectile.minion = true;
            projectile.penetrate = -1;
            projectile.timeLeft = STARTTIMELEFT;
			shootDistance = Main.rand.Next(300, 500);
			windup = Main.rand.Next(40, 80);
			projectile.ignoreWater = true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);
            int frameHeight = tex.Height / Main.projFrames[projectile.type];
            Rectangle frame = new Rectangle(0, frameHeight * projectile.frame, tex.Width, frameHeight);

			SpriteEffects effects = flipVertical ? SpriteEffects.FlipVertically : SpriteEffects.None;
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, frame, lightColor, projectile.rotation % 6.28f, tex.Size() / new Vector2(2, 2 * Main.projFrames[projectile.type]), projectile.scale, effects, 0f);

			Texture2D gunTex = ModContent.GetTexture(Texture + "_Gun");
			Texture2D flashTex = ModContent.GetTexture(Texture + "_Gun_Flash");
			SpriteEffects gunEffects = flipGun ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			int gunFrameHeight = gunTex.Height / totalGunFrames;
			Rectangle gunFrameBox = new Rectangle(0, gunFrameHeight * gunFrame, gunTex.Width, gunFrameHeight);
			Vector2 gunOrigin = new Vector2(flipGun ? gunTex.Width - 22 : 22, 7);
			spriteBatch.Draw(gunTex, (projectile.Center - Main.screenPosition) + (GunOffset().RotatedBy(projectile.rotation)), gunFrameBox, lightColor, gunRotation - (flipGun ? 3.14f : 0), gunOrigin, projectile.scale, gunEffects, 0f);
			spriteBatch.Draw(flashTex, (projectile.Center - Main.screenPosition) + (GunOffset().RotatedBy(projectile.rotation)), gunFrameBox, Color.White, gunRotation - (flipGun ? 3.14f : 0), gunOrigin, projectile.scale, gunEffects, 0f);

			return false;
        }

        public override void AI()
        {
			NPC testtarget = Main.npc.Where(n => n.active && n.CanBeChasedBy(projectile, false) && Vector2.Distance(n.Center, projectile.Center) < 800 && ClearPath(n.Center,projectile.Center)).OrderBy(n => Vector2.Distance(n.Center, projectile.Center)).FirstOrDefault();

			int distance = 1000;
			if (testtarget != default)
			{
				gunRotationToBe = (Vector2.Zero - testtarget.DirectionTo(projectile.Center  +(GunOffset().RotatedBy(projectile.rotation)))).ToRotation();
				float rotDifference = ((((gunRotationToBe - gunRotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;
				gunRotation = MathHelper.Lerp(gunRotation, gunRotation + rotDifference, 0.2f);
				flipGun = gunRotation.ToRotationVector2().X < 0;

				distance = (int)(projectile.Center - testtarget.Center).Length();
			}
			firing = distance < (shootDistance + (firing ? 50 : 0)) && projectile.timeLeft < STARTTIMELEFT - windup;
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
				Gore.NewGore(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 2, projectile.height / 2), Main.rand.NextVector2Circular(5, 5), ModGore.GetGoreSlot(Texture + "_Gore" + i.ToString()), 1f);
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

			if (projectile.ai[0] == 0f)
			{
				moveDirection.Y = 1;
				moveDirection.X = Main.rand.NextBool() ? 1 : -1;
				if (moveDirection.X == -1)
					flipVertical = true;

				projectile.rotation = moveDirection.ToRotation();
				projectile.ai[0] = 1f;
			}

			if (projectile.ai[1] == 0f)
			{
				if (collideY)
					projectile.ai[0] = 2f;

				if (!collideY && projectile.ai[0] == 2f)
				{
					moveDirection.X = -moveDirection.X;
					projectile.ai[1] = 1f;
					projectile.ai[0] = 1f;
				}
				if (collideX)
				{
					moveDirection.Y = -moveDirection.Y;
					projectile.ai[1] = 1f;
				}
			}
			else
			{
				projectile.rotation -= (moveDirection.X * moveDirection.Y) * 0.13f;

				if (collideX)
					projectile.ai[0] = 2f;

				if (!collideX && projectile.ai[0] == 2f)
				{
					moveDirection.Y = -moveDirection.Y;
					projectile.ai[1] = 0f;
					projectile.ai[0] = 1f;
				}
				if (collideY)
				{
					moveDirection.X = -moveDirection.X;
					projectile.ai[1] = 0f;
				}
			}
			projectile.velocity = SPEED * moveDirection;
			projectile.velocity = Collide();
		}

		private Vector2 Collide() => Collision.noSlopeCollision(projectile.position, projectile.velocity, projectile.width, projectile.height, true, true);

		private void RotateCrawl()
		{
			float rotDifference = ((((projectile.velocity.ToRotation() - projectile.rotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;
			if (Math.Abs(rotDifference) < 0.15f)
			{
				projectile.rotation = projectile.velocity.ToRotation();
				return;
			}
			projectile.rotation += Math.Sign(rotDifference) * 0.2f;
		}

		private void Attack()
        {
			RotateCrawl();
			projectile.Center -= projectile.velocity;
			attackCounter++;
			if (attackCounter % 7 == 0 && attackCounter % 56 < 15)
            {
				Vector2 dir = gunRotation.ToRotationVector2();

				gunFrame = 0;
				projectile.frameCounter = 0;

				Vector2 pos = projectile.Center + (GunOffset().RotatedBy(projectile.rotation));
				Gore.NewGore(pos, new Vector2(Math.Sign(dir.X) * -1, -0.5f) * 2, ModGore.GetGoreSlot(AssetDirectory.MiscItem + "CoachGunCasing"), 1f);
				gunRotation -= Math.Sign(dir.X) * 0.3f;
				Projectile.NewProjectile(pos, dir.RotatedByRandom(0.1f) * 15, ProjectileID.Bullet, projectile.damage, projectile.knockBack, player.whoAmI);
            }
        }

		private void FindFrame()
        {
			projectile.frameCounter++;
			if (projectile.frameCounter % 3 == 0)
				projectile.frame++;

			if (projectile.frameCounter % 4 == 0 && gunFrame < totalGunFrames - 1)
				gunFrame++;
			projectile.frame %= Main.projFrames[projectile.type];
			if (firing)
				projectile.frame = 0;
		}

		private Vector2 GunOffset()
        {
			Vector2 ret = Vector2.Zero;
			ret.X = -3;
			switch (projectile.frame)
            {
				case 0:
					ret.Y = -14;
					break;
				case 1:
					ret.Y = -16;
					break;
				case 2:
					ret.Y = -16;
					break;
				case 3:
					ret.Y = -16;
					break;
				case 4:
					ret.Y = -14;
					break;
				case 5:
					ret.Y = -16;
					break;
				case 6:
					ret.Y = -16;
					break;
				case 7:
					ret.Y = -16;
					break;
				default:
					ret.Y = -16; 
					break;
			}
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
				if ((Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).active() && Main.tileSolid[Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).type]))
				{
					return false;
				}
			}
			return true;
		}
	}
}