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

namespace StarlightRiver.Content.Items.Permafrost
{
    class OverflowingUrn : ModItem
    {
        public override string Texture => AssetDirectory.PermafrostItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Overflowing Urn");
            Tooltip.SetDefault("egshels update this lol");
        }

        public override void SetDefaults()
        {
            Item.damage = 15;
            Item.channel = true;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 10;
            Item.width = 2;
            Item.height = 34;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 0f;
            Item.shoot = ModContent.ProjectileType<OverflowingUrnProj>();
            Item.shootSpeed = 15f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[Item.shoot] == 0)
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
            base.HoldItem(player);
        }
    }
    public class OverflowingUrnProj : ModProjectile
    {
        public override string Texture => AssetDirectory.PermafrostItem + Name;

        private Player owner => Main.player[Projectile.owner];


        private int attackCounter = 0;

        private int appearCounter = 0;
        private float hoverCounter = 0;
        private int capCounter = 0;
        private bool capLeaving;
        private float opacity = 0;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Steamsaw");

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 40;
            Projectile.height = 58;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 20;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        public override void AI()
        {
            if (owner.HeldItem.type == ModContent.ItemType<OverflowingUrn>())
            {
                Projectile.timeLeft = 20;
            }
            capLeaving = false;
            if (owner.channel)
            {
                if (capCounter < 20)
                {
                    capCounter++;
                    capLeaving = true;
                }
                else
                {
                    attackCounter++;
                    if (attackCounter % 2 == 0)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + ((Projectile.rotation - 1.57f).ToRotationVector2() * 20), Projectile.DirectionTo(Main.MouseWorld).RotatedByRandom(0.3f) * Main.rand.NextFloat(5, 10), ModContent.ProjectileType<UrnWind>(), Projectile.damage, Projectile.knockBack, owner.whoAmI);

                        float lerper = Main.rand.NextFloat();
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + ((Projectile.rotation - 1.57f).ToRotationVector2() * 20) + (Projectile.rotation.ToRotationVector2() * MathHelper.Lerp(-8,8,lerper)), ModContent.DustType<UrnWindLine>(), Projectile.DirectionTo(Main.MouseWorld).RotatedBy(MathHelper.Lerp(0.6f,-0.6f, lerper)) * Main.rand.NextFloat(5, 10), 0, Color.Lerp(Color.Cyan, Color.LightBlue, Main.rand.NextFloat()), Main.rand.NextFloat(0.4f, 0.6f));
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
            }
            hoverCounter += 0.03f;

            Projectile.velocity = Vector2.Zero;
            Projectile.Center = owner.Center - new Vector2(0, 50 + (5 * (float)Math.Sin(hoverCounter)));

            float rotDifference = ((((Projectile.DirectionTo(Main.MouseWorld).ToRotation() + 1.57f - Projectile.rotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;

            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, Projectile.rotation + rotDifference, 0.15f);

            capCounter = (int)MathHelper.Clamp(capCounter, 0, 20);
            appearCounter++;
            Projectile.scale = opacity = MathHelper.Min(Projectile.timeLeft / 20f, appearCounter / 20f);
            owner.bodyFrame = new Rectangle(0, 56 * 5, 40, 56);
        }

        public override bool PreDraw(ref Color lightColor)
        {
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

        private void DrawWind()
        {
            var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Gravedigger/GluttonyBG").Value;
            float prog = MathHelper.Clamp((capCounter - 10) / 10f, 0, 1);
            var effect1 = Filters.Scene["CycloneIce"].GetShader().Shader;
            effect1.Parameters["NoiseOffset"].SetValue(Vector2.One * Main.GameUpdateCount * 0.02f);
            effect1.Parameters["brightness"].SetValue(10);
            effect1.Parameters["MainScale"].SetValue(1.0f);
            effect1.Parameters["CenterPoint"].SetValue(new Vector2(0.5f, 1f));
            effect1.Parameters["TrailDirection"].SetValue(new Vector2(0, -1));
            effect1.Parameters["width"].SetValue(0.85f);
            effect1.Parameters["distort"].SetValue(0.75f);
            effect1.Parameters["Resolution"].SetValue(tex.Size());
            effect1.Parameters["startColor"].SetValue(Color.Cyan.ToVector3());
            effect1.Parameters["endColor"].SetValue(Color.White.ToVector3());

            effect1.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallDistort").Value);

            //Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Black * prog * 0.8f, Projectile.rotation, new Vector2(tex.Width / 2, tex.Height), prog * 0.55f * new Vector2(1, 1.5f), 0, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect1, Main.GameViewMatrix.ZoomMatrix);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(220, 50, 90) * prog * 0.7f, Projectile.rotation, new Vector2(tex.Width / 2, tex.Height), prog * 0.55f * new Vector2(1,1.5f), 0, 0);
            //spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
        }
    }
    public class UrnWind : ModProjectile
    {
        public override string Texture => AssetDirectory.Dust + "NeedlerDust";

        private Player owner => Main.player[Projectile.owner];

        public Color color = Color.Cyan;


        public override void SetStaticDefaults() => DisplayName.SetDefault("Urn Wind");

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 50;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(6.28f);
            Projectile.scale = Main.rand.NextFloat(0.85f, 1.15f);
            color = Color.Lerp(Color.LightBlue, Color.Cyan, Main.rand.NextFloat());
        }

        public override void AI()
        {
            if (Projectile.extraUpdates == 0)
                Projectile.velocity *= 0.99f;
            Projectile.alpha = (int)MathHelper.Lerp(100, 255, 1 - (Projectile.timeLeft / 50f));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor.MultiplyRGB(color) * (1 - (Projectile.alpha / 255f)), Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.extraUpdates = 2;
            Projectile.friendly = false;
            Projectile.velocity = oldVelocity / 3;
            return false;
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