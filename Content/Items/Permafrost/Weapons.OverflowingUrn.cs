using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using System.Linq;

namespace StarlightRiver.Content.Items.Permafrost
{
    class OverflowingUrn : ModItem
    {
        public int freezeTimer;
        public float animationTimer;

        public override string Texture => AssetDirectory.PermafrostItem + Name;

		public override void Load()
		{
            StarlightPlayer.PostDrawEvent += PostDrawIcon;
		}

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Overflowing Urn");
            Tooltip.SetDefault("Unleashes a torrent of chilling winds\nProlonged use will freeze you\nYou have greatly increased defense while frozen");
        }

        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.channel = true;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 6;
            Item.width = 2;
            Item.height = 34;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 0f;
            Item.crit = -4;
            Item.shoot = ModContent.ProjectileType<OverflowingUrnProj>();
            Item.shootSpeed = 15f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.FirstOrDefault(n => n.Name == "Damage").Text = tooltips.FirstOrDefault(n => n.Name == "Damage").Text.Replace("damage", "damage over time");
            tooltips.FirstOrDefault(n => n.Name == "CritChance").Text = "Cannot critically strike";
        }

        private void PostDrawIcon(Player Player, SpriteBatch spriteBatch)
        {
            if (Player.HeldItem.type == ModContent.ItemType<OverflowingUrn>())
			{
                var item = Player.HeldItem.ModItem as OverflowingUrn;
                var tex = ModContent.Request<Texture2D>(AssetDirectory.PermafrostItem + "UrnFreezeUnder").Value;
                var source = new Rectangle(32 * (int)(item.freezeTimer / 300f * 3), 0, 32, 46);

                if (item.animationTimer > 0)
                    spriteBatch.Draw(tex, Player.Center + Vector2.UnitY * (48 + Player.gfxOffY) - Main.screenPosition, source, Color.White * item.animationTimer, 0, new Vector2(16, 23), item.animationTimer, 0, 0);
            }         
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[Item.shoot] == 0)
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);

            if (player.channel && player.statMana > Item.mana)
                freezeTimer++;
            else if (freezeTimer > 0)
                freezeTimer--;

            if (freezeTimer >= 300)
            {
                player.AddBuff(ModContent.BuffType<UrnFreeze>(), 180);
                freezeTimer = 0;
            }

            if (freezeTimer > 0 && animationTimer < 1)
                animationTimer += 0.1f;
            else if (freezeTimer <= 0 && animationTimer > 0)
                animationTimer -= 0.1f;

            base.HoldItem(player);
        }
    }

    public class OverflowingUrnProj : ModProjectile
    {
        private bool windBlowing = false;

        private List<Vector2> cache;
        private Trail trail;

        private float currentRotation = 0f;
        private int attackCounter = 0;

        private int appearCounter = 0;
        private float hoverCounter = 0;
        private int capCounter = 0;
        private bool capLeaving;
        private float opacity = 0;

        private Microsoft.Xna.Framework.Audio.SoundEffectInstance sound;

        private Vector2 currentPoint => Projectile.Center - ((currentRotation).ToRotationVector2() * 500 * windStrength);
        private Vector2 controlPoint => Projectile.Center - ((Projectile.rotation + 1.57f).ToRotationVector2() * 350 * windStrength);
        private Vector2 controlPointSmall => Projectile.Center - ((Projectile.rotation + 1.57f).ToRotationVector2() * 50 * windStrength);
        private Vector2 controlPointMedium => Projectile.Center - ((Projectile.rotation + 1.57f - (currentRotation - (Projectile.rotation + 1.57f))).ToRotationVector2() * 150 * windStrength);

        private Player owner => Main.player[Projectile.owner];
        private float windStrength => MathHelper.Clamp((capCounter - 10) / 10f, 0, 1);

        public override string Texture => AssetDirectory.PermafrostItem + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Overflowing Urn");

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 40;
            Projectile.height = 58;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 20;
            Projectile.ignoreWater = true;
            Projectile.hide = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 2;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        public override void AI()
        {
            if (owner.HeldItem.type == ModContent.ItemType<OverflowingUrn>())
                Projectile.timeLeft = 20;

            float rotDifference = (((((Projectile.rotation + 1.57f) - currentRotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;

            currentRotation = MathHelper.Lerp(currentRotation, currentRotation + rotDifference, 0.15f);

            if (windStrength > 0)
            {
                if (!windBlowing)
                {
                    currentRotation = Projectile.rotation + 1.57f;
                    windBlowing = true;
                }
            }
            else
            {
                windBlowing = false;
            }

            capLeaving = false;
            if (owner.channel && owner.statMana > owner.HeldItem.mana)
            {
                if (capCounter < 20)
                {
                    capCounter++;
                    capLeaving = true;
                }
                else
                {
                    attackCounter++;

                    if (attackCounter % 180 == 0 || attackCounter == 1)
                    {
                        sound?.Stop(true);
                        sound = Terraria.Audio.SoundEngine.PlaySound(SoundID.BlizzardStrongLoop, Projectile.Center);
                        sound.Volume *= 4;
                    }
                        //Helper.PlayPitched("Effects/HeavyWhoosh", 0.5f, Main.rand.NextFloat(-0.5f, -0.2f), Projectile.Center);

                    if (attackCounter % 2 == 0)
                    {
                        float lerper = Main.rand.NextFloat();
                        var pos = Projectile.Center + ((Projectile.rotation - 1.57f).ToRotationVector2() * 20) + (Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(-8, 8, lerper));
                        var vel = Projectile.DirectionTo(Main.MouseWorld).RotatedBy(MathHelper.Lerp(0.4f, -0.4f, lerper)) * Main.rand.NextFloat(5, 40);
                        var d = Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.AuroraFast>(), vel, 0, Color.Lerp(Color.Cyan, Color.LightBlue, Main.rand.NextFloat()), Main.rand.NextFloat(0.4f, 0.6f));
                        d.customData = Main.rand.NextFloat(0.6f, 1.2f);
                        d.rotation = Main.rand.NextFloat(6.28f);
                    }

                    for(int k = 0; k < 20; k++)
					{
                        float lerper = Main.rand.NextFloat();
                        var pos = Projectile.Center + ((Projectile.rotation - 1.57f).ToRotationVector2() * 20) + (Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(-8, 8, lerper));
                        var vel = Projectile.DirectionTo(Main.MouseWorld).RotatedByRandom(0.3f) * Main.rand.NextFloat(8, 18);
                        Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Gas>(), vel, 0, new Color(255, 255, 255) * 0.1f, Main.rand.NextFloat(4.5f, 6.5f));
					}
                    
                }
            }
            else
            {
                if (capCounter > 0)
                    capCounter--;

                if (capCounter > 6)
                    capCounter--;
                else
                    attackCounter = 0;

                sound?.Stop(true);
            }

            hoverCounter += 0.03f;

            Projectile.velocity = Vector2.Zero;
            Projectile.Center = owner.Center - new Vector2(0, 50 + (5 * (float)Math.Sin(hoverCounter)));

            float rotDifference2 = ((((Projectile.DirectionTo(Main.MouseWorld).ToRotation() + 1.57f - Projectile.rotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;

            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, Projectile.rotation + rotDifference2, 0.15f);

            capCounter = (int)MathHelper.Clamp(capCounter, 0, 20);
            appearCounter++;
            Projectile.scale = opacity = MathHelper.Min(Projectile.timeLeft / 20f, appearCounter / 20f);
            owner.bodyFrame = new Rectangle(0, 56 * 5, 40, 56);

            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrails();
            }
        }

		public override bool? CanHitNPC(NPC target)
		{
            var rot = (Projectile.rotation + 1.57f * 3) % 6.28f - 3.14f;

            if(windBlowing && Helper.CheckConicalCollision(Projectile.Center, 500, rot, 0.3f, target.Hitbox))
			{
                target.AddBuff(ModContent.BuffType<Buffs.PrismaticDrown>(), 1);
                target.velocity += Vector2.Normalize(target.Center - Projectile.Center) * 0.4f * target.knockBackResist;
            }

            return false;
        }

		public override bool PreDraw(ref Color lightColor)
        {
            if (windStrength > 0)
                DrawWind();

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D topTex = ModContent.Request<Texture2D>(Texture + "_Top").Value;
            float capOpacity = owner.channel ? 1 - (capCounter / 20f) : 1 - MathHelper.Clamp(((capCounter - 6) / 14f), 0, 1);

            float rot = 0f;

            Vector2 rotationVector = (Projectile.rotation + 1.57f).ToRotationVector2();
            Vector2 capPos = (Projectile.Center - Main.screenPosition) + new Vector2(0, owner.gfxOffY) + (rotationVector * -(20 * (capLeaving ? EaseFunction.EaseCubicIn.Ease(1 - capOpacity) : EaseFunction.EaseCubicOut.Ease(1 - capOpacity))));
            Vector2 urnPos = Projectile.Center - Main.screenPosition + new Vector2(0, owner.gfxOffY);

            if (!capLeaving)
            {
                float shake = (float)Math.Sin(3.14f * Math.Clamp(capCounter / 6f, 0, 1)) * 3;
                rot = (float)Math.Sin(6.28f * Math.Clamp(capCounter / 6f, 0, 1)) * 0.03f;
                capPos += shake * rotationVector;
                urnPos += shake * rotationVector;
            }

            Main.spriteBatch.Draw(topTex, capPos, null, lightColor * opacity * capOpacity, Projectile.rotation + rot, new Vector2(topTex.Width / 2, (tex.Height / 2) + 10), Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, urnPos, null, lightColor * opacity, Projectile.rotation + rot, new Vector2(tex.Width / 2, tex.Height / 2), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        private void ManageCaches()
        {
            cache = new List<Vector2>();
            Vector2 goff = new Vector2(0, owner.gfxOffY) + ((Projectile.rotation - 1.57f).ToRotationVector2() * 12);
            BezierCurve curve = new BezierCurve(new Vector2[] { Projectile.Center + goff,  controlPointSmall + goff, controlPointMedium + goff, controlPoint + goff, currentPoint + goff });
            cache = curve.GetPoints(120);
            cache.Reverse();
        }

        private void ManageTrails()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 120, new TriangularTip(4), factor => 174 * windStrength, factor =>
            {
                return Lighting.GetColor(Projectile.Center.ToTileCoordinates());
            });
            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center;
        }

        private void DrawWind()
        {
            var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Gravedigger/GluttonyBG").Value;
            Main.spriteBatch.End();
            Effect effect1 = Filters.Scene["CycloneIce"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);


            effect1.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect1.Parameters["NoiseOffset"].SetValue(Vector2.One * Main.GameUpdateCount * 0.02f);
            effect1.Parameters["brightness"].SetValue(10);
            effect1.Parameters["MainScale"].SetValue(1.0f);
            effect1.Parameters["CenterPoint"].SetValue(new Vector2(0.5f, 1f));
            effect1.Parameters["TrailDirection"].SetValue(new Vector2(0, -1));
            effect1.Parameters["width"].SetValue(0.85f);
            effect1.Parameters["time"].SetValue(Main.GameUpdateCount * 0.15f);
            effect1.Parameters["distort"].SetValue(0.75f);
            effect1.Parameters["progMult"].SetValue(3.7f);
            effect1.Parameters["Resolution"].SetValue(tex.Size());
            effect1.Parameters["startColor"].SetValue(Color.Cyan.ToVector3());
            effect1.Parameters["endColor"].SetValue(Color.White.ToVector3());
            effect1.Parameters["sampleTexture"].SetValue(tex);
            effect1.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallDistort").Value);

            BlendState oldState = Main.graphics.GraphicsDevice.BlendState;
            Main.graphics.GraphicsDevice.BlendState = BlendState.Additive;
            trail?.Render(effect1);
            Main.graphics.GraphicsDevice.BlendState = oldState;

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    class UrnWindLine : ModDust
    {
        public override string Texture => AssetDirectory.VitricBoss + "RoarLine";

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * MathHelper.Min(1, dust.fadeIn / 20f);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 8, 128);

            dust.shader = new ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= new Vector2(4, 64).RotatedBy(dust.velocity.ToRotation() + 1.57f) * dust.scale;
                dust.customData = 1;
            }

            dust.rotation = dust.velocity.ToRotation() + 1.57f;
            dust.position += dust.velocity;

            dust.velocity *= 0.98f;
            dust.color *= 0.95f;

            dust.shader.UseColor(dust.color * MathHelper.Min(1, dust.fadeIn / 20f));
            dust.fadeIn += 2;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

            if (dust.fadeIn > 60)
                dust.active = false;
            return false;
        }
    }
}