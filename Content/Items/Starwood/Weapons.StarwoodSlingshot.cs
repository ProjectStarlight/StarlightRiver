using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Starwood
{
	public class StarwoodSlingshot : StarwoodItem
    {
        public override string Texture => AssetDirectory.StarwoodItem + Name;
        public StarwoodSlingshot() : base(ModContent.Request<Texture2D>(AssetDirectory.StarwoodItem + "StarwoodSlingshot_Alt").Value) { }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starwood Slingshot");
            Tooltip.SetDefault("Weaves together fallen stars \nConsumes ammo every 50 shots");
        }

        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.ranged = true;
            Item.channel = true;
            Item.width = 18;
            Item.height = 34;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item19;
            Item.shoot = ModContent.ProjectileType<StarwoodSlingshotProj>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.useAmmo = ItemID.FallenStar;
            Item.noUseGraphic = true;
        }

        private int timesShot = 0;//Test: possible endless ammo
        public override bool ConsumeAmmo(Player Player)
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
        const int minDamage = 15;
        const int maxDamage = 20;
        const int minVelocity = 4;
        const int maxVelocity = 25;
        const float chargeRate = 0.02f;

        private bool empowered = false;
        private bool released = false;
        private bool fired = false;
        private float charge = 0;
        private Vector2 direction = Vector2.Zero;
        private Vector2 posToBe = Vector2.Zero;
        private Vector3 lightColor = new Vector3(0.4f, 0.2f, 0.1f);

        private float flickerTime = 0;

        public override string Texture => AssetDirectory.StarwoodItem + Name;

        public sealed override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.magic = true;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.alpha = 255;
			Projectile.timeLeft = 9999;
		}
		
        public override void AI()
		{
            Lighting.AddLight(Projectile.Center, lightColor);
			AdjustDirection();
			Player Player = Main.player[Projectile.owner];
			Player.ChangeDir(Main.MouseWorld.X > Player.position.X ? 1 : -1);
			Player.heldProj = Projectile.whoAmI;
			Player.ItemTime = 2;
			Player.ItemAnimation = 2;

            if (Projectile.ai[0] == 0)
            {
                StarlightPlayer mp = Main.player[Projectile.owner].GetModPlayer<StarlightPlayer>();

                if (mp.empowered)
                    empowered = true;

                Projectile.netUpdate = true;
                Projectile.ai[0]++;
            }

			posToBe = Player.Center + (direction * 40);
             Vector2 moveDirection = posToBe - Projectile.Center;

            float speed = (float)Math.Sqrt(moveDirection.Length());

            if (speed > 0.05f)
            {
                moveDirection.Normalize();
                moveDirection *= speed;
                Projectile.velocity = moveDirection;
            }
            else
                Projectile.velocity = Vector2.Zero;

			if (Player.channel && !released)
			{
                Projectile.timeLeft = 15;

				if (charge < 1)
				{
					if ((charge + chargeRate) >= 1)
						Terraria.Audio.SoundEngine.PlaySound(SoundID.MaxMana, (int)Projectile.Center.X, (int)Projectile.Center.Y, 1, 1, -0.25f);

					charge += chargeRate;
				}
			}
			else
			{
                if (!released)
                {
                    Player.ItemTime = 15;
                    Player.ItemAnimation = 15;
                    released = true;
                }

                if (!fired)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 dustVel = direction.RotatedBy(Main.rand.NextFloat(-0.2f,0.2f)) * Main.rand.NextFloat(0.8f, 2);

                        if (empowered)
                            Dust.NewDustPerfect(Player.Center + (direction.RotatedBy(-0.06f) * 25), ModContent.DustType<BlueStamina>(), dustVel);
                        else
                            Dust.NewDustPerfect(Player.Center + (direction.RotatedBy(-0.06f) * 25), ModContent.DustType<Stamina>(), dustVel);
                    }
                }

                if (Projectile.timeLeft == 8)
                {
                    int proj = Projectile.NewProjectile(Projectile.Center, direction * Helper.LerpFloat(minVelocity, maxVelocity, charge), ModContent.ProjectileType<StarwoodSlingshotStar>(), (int)Helper.LerpFloat(minDamage,maxDamage, charge), Projectile.knockBack, Projectile.owner);
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

            if (Main.player[Projectile.owner].direction != 1)
                spriteEffect = SpriteEffects.FlipVertically;
            else
                offset = new Vector2(0, -6);

            Color color = lightColor;
            int frameHeight = (charge > 0.5f && !released) ? 30 : 0;

            if (empowered)
                frameHeight += 60;

            Rectangle frame = new Rectangle(0, frameHeight, 30, 30);
            Main.spriteBatch.Draw(Main.projectileTexture[Projectile.type], ((Main.player[Projectile.owner].Center - Main.screenPosition) + new Vector2(0, Main.player[Projectile.owner].gfxOffY)).PointAccur() + offset, frame, color, direction.ToRotation(), new Vector2(8, 10), Projectile.scale, spriteEffect, 0);
            
            if (!fired)
            {
                for (float i = 0; i < charge; i += 0.2f)
                {
                    Vector2 offset2 = ((i * 6.28f) + direction.ToRotation()).ToRotationVector2();

                    if (charge > i + 0.33f || charge >= 1)
                        offset2 = Vector2.Zero;
                    else
                        offset2 *= Helper.LerpFloat(0, 7, (float)Math.Sqrt((0.33f - (charge - i)) * 3));

                    Texture2D fragmenttexture = Mod.Request<Texture2D>(AssetDirectory.StarwoodItem.Remove(0, Mod.Name.Length + 1).Value + "StarwoodSlingshotParts");
                    Rectangle frame2 = new Rectangle(0, (int)(i * 5 * 24), 22, 24);
                    if (empowered)
                        frame2.Y += fragmenttexture.Height / 2;

                    Main.spriteBatch.Draw(fragmenttexture, ((Projectile.Center - Main.screenPosition) + offset2), frame2, new Color(255, 255, 255, 1 - ((0.33f - (charge - i)) * 5)), direction.ToRotation() + 1.57f, new Vector2(11,12), Projectile.scale,SpriteEffects.None, 0);
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

                Main.spriteBatch.Draw(ModContent.Request<Texture2D>(AssetDirectory.StarwoodItem + "StarwoodSlingshotStarWhite").Value, (Projectile.Center - Main.screenPosition), null, color * alpha, direction.ToRotation() + 1.57f, new Vector2(11,12), Projectile.scale,SpriteEffects.None, 0);
            }

            return false;
        }

		//helpers
		private void AdjustDirection(float deviation = 0f)
		{
			Player Player = Main.player[Projectile.owner];
			direction = (Main.MouseWorld - (Player.Center - new Vector2(4, 4))) - new Vector2(0, Main.player[Projectile.owner].gfxOffY);
			direction.Normalize();
			direction = direction.RotatedBy(deviation);
			Player.ItemRotation = direction.ToRotation();
			if (Player.direction != 1)
				Player.ItemRotation -= 3.14f;
		}
	}

    public class StarwoodSlingshotStar : ModProjectile, IDrawAdditive
    {
        const int EmpoweredDamageIncrease = 5;

        //These stats get scaled when empowered
        private float ScaleMult = 1.5f;
        private Vector3 lightColor = new Vector3(0.4f, 0.2f, 0.1f);
        private int dustType = ModContent.DustType<Stamina>(); //already implemented
        private bool empowered = false;
        private float rotationVar = 0;

        public override string Texture => AssetDirectory.StarwoodItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shooting Star");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.aiStyle = 1;
            Main.projFrames[Projectile.type] = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f + rotationVar;
            rotationVar += 0.4f;
            StarlightPlayer mp = Main.player[Projectile.owner].GetModPlayer<StarlightPlayer>();

            if (!empowered && mp.empowered)
            {
                Projectile.frame += 5;
                lightColor = new Vector3(0.05f, 0.1f, 0.2f);
                ScaleMult = 2f;
                dustType = ModContent.DustType<BlueStamina>();
                Projectile.velocity *= 1.25f;//TODO: This could be on on the Item's side like the staff does, thats generally the better way
                empowered = true;
            }

            if (Projectile.timeLeft % 25 == 0)//delay between star sounds
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);

            Lighting.AddLight(Projectile.Center, lightColor);
            Projectile.velocity.X *= 0.995f;

            if (Projectile.velocity.Y < 50)
                Projectile.velocity.Y += 0.25f;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) 
        {
            if (empowered) 
                damage += EmpoweredDamageIncrease;
        }

        public override void Kill(int timeLeft)
        {
            Helpers.DustHelper.DrawStar(Projectile.Center, dustType, pointAmount: 5, mainSize: 1.2f * ScaleMult, dustDensity: 1f, pointDepthMult: 0.3f, rotationAmount: Projectile.rotation);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);

            for (int k = 0; k < 35; k++)
            {
                Dust.NewDustPerfect(Projectile.Center, dustType, Vector2.One.RotatedByRandom(6.28f) * (Main.rand.NextFloat(0.25f, 1.2f) * ScaleMult), 0, default, 1.5f);
            }

            if (empowered)
                for (int k = 0; k < 4; k++)
                {
                    Projectile.NewProjectile(Projectile.position, -Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(0.5f, 0.8f), ModContent.ProjectileType<StarwoodSlingshotFragment>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, Main.rand.Next(2));
                }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(Main.projectileTexture[Projectile.type].Width * 0.5f, Projectile.height * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Color color = Projectile.GetAlpha(Color.White) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length * 0.5f);
                float scale = Projectile.scale * (Projectile.oldPos.Length - k) / Projectile.oldPos.Length;

                spriteBatch.Draw(Mod.Request<Texture2D>(AssetDirectory.StarwoodItem.Remove(0, Mod.Name.Length + 1).Value + "StarwoodSlingshotGlowTrail"),
                Projectile.oldPos[k] + drawOrigin - Main.screenPosition,
                new Rectangle(0, 24 * Projectile.frame, 22, 24),
                color,
                Projectile.oldRot[k],
                new Vector2(Main.projectileTexture[Projectile.type].Width / 2, Main.projectileTexture[Projectile.type].Height / 20),
                scale, default, default);
            }

            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, new Rectangle(0, 24 * Projectile.frame, 22, 24), Color.White, Projectile.rotation, new Vector2(11, 12), Projectile.scale, default, default);
            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (Projectile.frame == 4 || Projectile.frame == 9)
            {
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Color color = (empowered ? new Color(200, 220, 255) * 0.35f : new Color(255, 255, 200) * 0.3f) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                    if (k <= 4) 
                        color *= 1.2f;

                    float scale = Projectile.scale * (Projectile.oldPos.Length - k) / Projectile.oldPos.Length * 0.8f;
                    Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Starwood/Glow").Value;

                    spriteBatch.Draw(tex, Projectile.oldPos[k] + Projectile.Size / 2 - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.timeLeft = 9;
            Projectile.width = 12;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.aiStyle = -1;
            Projectile.rotation = Main.rand.NextFloat(4f);
        }

        public override void AI()
        {
            Projectile.rotation += 0.3f;
        }

        public override void Kill(int timeLeft) 
        {
            for (int k = 0; k < 3; k++)
                Dust.NewDustPerfect(Projectile.position, ModContent.DustType<StarFragment>(), Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.NextFloat(0.3f, 0.5f), 0, Color.White, 1.5f); 
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) 
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.ai[0] > 0 ? 10 : 0, 12, 10), Color.White, Projectile.rotation, new Vector2(6, 5), Projectile.scale, default, default);
            return false; 
        }
    }
}