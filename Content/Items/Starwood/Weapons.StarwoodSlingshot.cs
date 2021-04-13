using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.Audio;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Starwood
{
    public class StarwoodSlingshot : StarwoodItem
    {
        public override string Texture => AssetDirectory.StarwoodItem + Name;
        public StarwoodSlingshot() : base(ModContent.GetTexture(AssetDirectory.StarwoodItem + "StarwoodSlingshot_Alt")) { }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starwood Slingshot");
            Tooltip.SetDefault("Shoots fallen stars \nConsumes ammo every 50 shots");
        }

        public override void SetDefaults()
        {
            item.damage = 20;
            item.ranged = true;
            item.channel = true;
            item.width = 18;
            item.height = 34;
            item.useTime = 25;
            item.useAnimation = 25;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.knockBack = 4f;
            item.UseSound = SoundID.Item19;
            item.shoot = ModContent.ProjectileType<StarwoodSlingshotProj>();
            item.shootSpeed = 16f;
            item.noMelee = true;
            item.useAmmo = ItemID.FallenStar;
            item.noUseGraphic = true;
        }

        private int timesShot = 0;//Test: possible endless ammo
        public override bool ConsumeAmmo(Player player)
        {
            timesShot++;
            if (timesShot >= 50)
            {
                timesShot = 0;
                return true;
            }
            return false;
        }
    }

    public class StarwoodSlingshotProj : ModProjectile
	{
        public override string Texture => AssetDirectory.StarwoodItem + Name;
        public sealed override void SetDefaults()
		{
			projectile.hostile = false;
			projectile.magic = true;
			projectile.width = 30;
			projectile.height = 30;
			projectile.aiStyle = -1;
			projectile.friendly = false;
			projectile.penetrate = 1;
			projectile.tileCollide = false;
			projectile.alpha = 255;
			projectile.timeLeft = 9999;
		}
		
		const int minDamage = 15;
        const int maxDamage = 40;
        const int minVelocity = 4;
        const int maxVelocity = 25;
        const float chargeRate = 0.02f;

        //protected LegacySoundStyle soundtype = new LegacySoundStyle(soundId: SoundID.Item, style: 5).WithPitchVariance(0.2f);//????


        private bool empowered = false;
        private bool released = false;
        private bool fired = false;
        private float charge = 0;
        private Vector2 direction = Vector2.Zero;
        private Vector2 posToBe = Vector2.Zero;
        private Vector3 lightColor = new Vector3(0.4f, 0.2f, 0.1f);

        private float flickerTime = 0;

        public override void AI()
		{
            Lighting.AddLight(projectile.Center, lightColor);
			AdjustDirection();
			Player player = Main.player[projectile.owner];
			player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);
			player.heldProj = projectile.whoAmI;
			player.itemTime = 2;
			player.itemAnimation = 2;

            if (projectile.ai[0] == 0)
            {
                StarlightPlayer mp = Main.player[projectile.owner].GetModPlayer<StarlightPlayer>();
                if (mp.Empowered)
                    empowered = true;
                projectile.netUpdate = true;
                projectile.ai[0]++;
            }

			posToBe = player.Center + (direction * 40);
             Vector2 moveDirection = posToBe - projectile.Center;


            float speed = (float)Math.Sqrt(moveDirection.Length());

            if (speed > 0.05f)
            {
                moveDirection.Normalize();
                moveDirection *= speed;
                projectile.velocity = moveDirection;
            }
            else
                projectile.velocity = Vector2.Zero;


			if (player.channel && !released)
			{
                projectile.timeLeft = 15;
				if (charge < 1)
				{
					if ((charge + chargeRate) >= 1)
						Main.PlaySound(SoundID.MaxMana, (int)projectile.Center.X, (int)projectile.Center.Y, 1, 1, -0.25f);
					charge += chargeRate;
				}
			}
			else
			{
                if (!released)
                {
                    player.itemTime = 15;
                    player.itemAnimation = 15;
                    released = true;
                }
                if (!fired)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 dustVel = direction.RotatedBy(Main.rand.NextFloat(-0.2f,0.2f)) * Main.rand.NextFloat(0.8f, 2);
                        if (empowered)
                            Dust.NewDustPerfect(player.Center + (direction.RotatedBy(-0.06f) * 25), ModContent.DustType<BlueStamina>(), dustVel);
                        else
                            Dust.NewDustPerfect(player.Center + (direction.RotatedBy(-0.06f) * 25), ModContent.DustType<Stamina>(), dustVel);
                    }
                }
                if (projectile.timeLeft == 8)
                {
                    int proj = Projectile.NewProjectile(projectile.Center, direction * Helper.LerpFloat(minVelocity, maxVelocity, charge), ModContent.ProjectileType<StarwoodSlingshotStar>(), (int)Helper.LerpFloat(minDamage,maxDamage, charge), projectile.knockBack, projectile.owner);
                    Main.projectile[proj].frame = (int)(charge * 5) - 1;
                    if ((int)(charge * 5) == 0)
                        Main.projectile[proj].frame++;
                    fired = true;
                }
			}
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            SpriteEffects spriteEffect = SpriteEffects.None;
            Vector2 offset = Vector2.Zero;
            if (Main.player[projectile.owner].direction != 1)
                spriteEffect = SpriteEffects.FlipVertically;
            else
                offset = new Vector2(0, -6);
            Color color = lightColor;
            int frameHeight = (charge > 0.5f && !released) ? 30 : 0;
            if (empowered)
                frameHeight += 60;

            Rectangle frame = new Rectangle(0, frameHeight, 30, 30);
            Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], ((Main.player[projectile.owner].Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY)).PointAccur() + offset, frame, color, direction.ToRotation(), new Vector2(8, 10), projectile.scale, spriteEffect, 0);
            if (!fired)
            {
                for (float i = 0; i < charge; i += 0.2f)
                {
                    Vector2 offset2 = ((i * 6.28f) + direction.ToRotation()).ToRotationVector2();

                    if (charge > i + 0.33f || charge >= 1)
                        offset2 = Vector2.Zero;
                    else
                        offset2 *= Helper.LerpFloat(0, 7, (float)Math.Sqrt((0.33f - (charge - i)) * 3));

                    Texture2D fragmenttexture = mod.GetTexture(AssetDirectory.StarwoodItem.Remove(0, mod.Name.Length + 1) + "StarwoodSlingshotParts");
                    Rectangle frame2 = new Rectangle(0, (int)(i * 5 * 24), 22, 24);
                    if (empowered)
                        frame2.Y += fragmenttexture.Height / 2;

                    Main.spriteBatch.Draw(fragmenttexture, ((projectile.Center - Main.screenPosition) + offset2), frame2, new Color(255, 255, 255, 1 - ((0.33f - (charge - i)) * 5)), direction.ToRotation() + 1.57f, new Vector2(11,12), projectile.scale,SpriteEffects.None, 0);
                }
            }
            if (flickerTime < 16 && charge >= 1 && !fired)
            {
                flickerTime+= 0.5f;
                color = Color.White;
                float flickerTime2 = (float)(flickerTime / 20f);
                float alpha = 1.5f - (((flickerTime2 * flickerTime2) / 2) + (2f * flickerTime2));
                if (alpha < 0)
                    alpha = 0;

                Main.spriteBatch.Draw(ModContent.GetTexture(AssetDirectory.StarwoodItem + "StarwoodSlingshotStarWhite"), (projectile.Center - Main.screenPosition), null, color * alpha, direction.ToRotation() + 1.57f, new Vector2(11,12), projectile.scale,SpriteEffects.None, 0);
            }
            return false;
        }

		//helpers
		private void AdjustDirection(float deviation = 0f)
		{
			Player player = Main.player[projectile.owner];
			direction = (Main.MouseWorld - (player.Center - new Vector2(4, 4))) - new Vector2(0, Main.player[projectile.owner].gfxOffY);
			direction.Normalize();
			direction = direction.RotatedBy(deviation);
			player.itemRotation = direction.ToRotation();
			if (player.direction != 1)
				player.itemRotation -= 3.14f;
		}

        //Lerp float moved to helper.cs
	}

    public class StarwoodSlingshotStar : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.StarwoodItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shooting Star");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        const int EmpoweredDamageIncrease = 5;

        //These stats get scaled when empowered
        private float ScaleMult = 1.5f;
        private Vector3 lightColor = new Vector3(0.4f, 0.2f, 0.1f);
        private int dustType = ModContent.DustType<Stamina>(); //already implemented
        private bool empowered = false;


        private float rotationVar = 0;

        public override void SetDefaults()
        {
            projectile.width = 22;
            projectile.height = 24;
            projectile.friendly = true;
            projectile.penetrate = 2;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
            projectile.aiStyle = 1;
            Main.projFrames[projectile.type] = 10;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + 1.57f + rotationVar;
            rotationVar += 0.4f;
            StarlightPlayer mp = Main.player[projectile.owner].GetModPlayer<StarlightPlayer>();
            if (!empowered && mp.Empowered)
            {
                projectile.frame += 5;
                lightColor = new Vector3(0.05f, 0.1f, 0.2f);
                ScaleMult = 2f;
                dustType = ModContent.DustType<BlueStamina>();
                projectile.velocity *= 1.25f;//TODO: This could be on on the item's side like the staff does, thats generally the better way
                empowered = true;
            }

            if (projectile.timeLeft % 25 == 0)//delay between star sounds
                Main.PlaySound(SoundID.Item9, projectile.Center);

            Lighting.AddLight(projectile.Center, lightColor);
            projectile.velocity.X *= 0.995f;
            if (projectile.velocity.Y < 50)
                projectile.velocity.Y += 0.25f;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) 
        {
            if (empowered) 
                damage += EmpoweredDamageIncrease;
        }

        public override void Kill(int timeLeft)
        {
            Helpers.DustHelper.DrawStar(projectile.Center, dustType, pointAmount: 5, mainSize: 1.2f * ScaleMult, dustDensity: 1f, pointDepthMult: 0.3f, rotationAmount: projectile.rotation);
            Main.PlaySound(SoundID.Item10, projectile.Center);
            for (int k = 0; k < 35; k++)
                Dust.NewDustPerfect(projectile.Center, dustType, Vector2.One.RotatedByRandom(6.28f) * (Main.rand.NextFloat(0.25f, 1.2f) * ScaleMult), 0, default, 1.5f);

            if (empowered)
                for (int k = 0; k < 4; k++)
                    Projectile.NewProjectile(projectile.position, -projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(0.5f, 0.8f), ModContent.ProjectileType<StarwoodSlingshotFragment>(), projectile.damage / 2, projectile.knockBack, projectile.owner, Main.rand.Next(2));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);
            Vector2 drawOrigin = new Vector2(Main.projectileTexture[projectile.type].Width * 0.5f, projectile.height * 0.5f);
            for (int k = 0; k < projectile.oldPos.Length; k++)
            {
                Color color = projectile.GetAlpha(Color.White) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length * 0.5f);
                float scale = projectile.scale * (projectile.oldPos.Length - k) / projectile.oldPos.Length;

                spriteBatch.Draw(mod.GetTexture(AssetDirectory.StarwoodItem.Remove(0, mod.Name.Length + 1) + "StarwoodSlingshotGlowTrail"),
                projectile.oldPos[k] + drawOrigin - Main.screenPosition,
                new Rectangle(0, 24 * projectile.frame, 22, 24),
                color,
                projectile.oldRot[k],
                new Vector2(Main.projectileTexture[projectile.type].Width / 2, Main.projectileTexture[projectile.type].Height / 20),
                scale, default, default);
            }
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, new Rectangle(0, 24 * projectile.frame, 22, 24), Color.White, projectile.rotation, new Vector2(11, 12), projectile.scale, default, default);
            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (projectile.frame == 4 || projectile.frame == 9)
            {
                for (int k = 0; k < projectile.oldPos.Length; k++)
                {
                    Color color = (empowered ? new Color(200, 220, 255) * 0.35f : new Color(255, 255, 200) * 0.3f) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                    if (k <= 4) color *= 1.2f;
                    float scale = projectile.scale * (projectile.oldPos.Length - k) / projectile.oldPos.Length * 0.8f;
                    Texture2D tex = ModContent.GetTexture("StarlightRiver/Assets/Items/Starwood/Glow");

                    spriteBatch.Draw(tex, projectile.oldPos[k] + projectile.Size / 2 - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
                }
            }
        }
    }

    public class StarwoodSlingshotFragment : ModProjectile
    {
        public override string Texture => AssetDirectory.StarwoodItem + "StarwoodSlingshotFragment";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Star Fragment");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            projectile.timeLeft = 9;
            projectile.width = 12;
            projectile.height = 10;
            projectile.friendly = true;
            projectile.penetrate = 2;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
            projectile.aiStyle = -1;
            projectile.rotation = Main.rand.NextFloat(4f);
        }

        public override void AI() => projectile.rotation += 0.3f;

        public override void Kill(int timeLeft) 
        {
            for (int k = 0; k < 3; k++)
                Dust.NewDustPerfect(projectile.position, ModContent.DustType<StarFragment>(), projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.NextFloat(0.3f, 0.5f), 0, Color.White, 1.5f); 
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) 
        {
            Texture2D tex = ModContent.GetTexture(Texture);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, new Rectangle(0, projectile.ai[0] > 0 ? 10 : 0, 12, 10), Color.White, projectile.rotation, new Vector2(6, 5), projectile.scale, default, default);
            return false; 
        }
    }
}