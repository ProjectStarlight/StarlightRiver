using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core;
using StarlightRiver.Core.Systems;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
namespace StarlightRiver.Content.Items.Vitric
{
    public class CoachGunUpgrade : ModItem
    {
        private int cooldown = 0;

        public override void HoldItem(Player player) => cooldown--;
        public override bool AltFunctionUse(Player player) => true;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void Load()
        {
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricItem + "CoachGunUpgradeCasing");
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Magmatic Coach Gun");
            Tooltip.SetDefault("Press <right> to throw out an unstable crystal bomb\nExplodes shortly after, causing all other crystal bombs to also explode\n" +
                "Shoot to detonate it early, if detonated early enough, it will explode into a cone of crystal shards\n" +
                "If a crystal bomb is detonated by another crystal bomb, its damage is increased by 50%\n" +
                "Explosions and crystal shards increase targets Exposure by 35% and inflict Sweltered\n'Ultrakill style'");
        }

        public override void SetDefaults()
        {
            Item.damage = 40;
            Item.knockBack = 1f;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = Item.useAnimation = 30;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.width = Item.height = 32;
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 2, silver: 75);

            Item.shoot = ModContent.ProjectileType<CoachGunUpgradeBomb>();
            Item.shootSpeed = 18.5f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ModContent.ItemType<CoachGun>()).
                AddIngredient(ModContent.ItemType<MagmaCore>(), 3).
                AddIngredient(ItemID.HellstoneBar, 12).
                AddTile(TileID.Anvils).
                Register();
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-15, 0);
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useStyle = ItemUseStyleID.Swing;
                Item.noUseGraphic = true;

                Item.useTime = Item.useAnimation = 15;
                if (player.ownedProjectileCounts[ModContent.ProjectileType<CoachGunUpgradeBomb>()] >= 3 || cooldown > 0)
                    return false;
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.noUseGraphic = false;

                Item.useTime = Item.useAnimation = 30;
            }
            return base.CanUseItem(player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                velocity = velocity * 0.8f;
                type = ModContent.ProjectileType<CoachGunUpgradeBomb>();
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
                cooldown = 30;
            else
            {
                Vector2 offset = Vector2.Normalize(velocity) * 25f;

                for (int k = 0; k < 12; k++)
                {
                    Dust.NewDustPerfect(position + offset * 1.5f, ModContent.DustType<Dusts.Glow>(), (velocity.RotatedByRandom(0.35f)) * Main.rand.NextFloat(0.15f, 0.45f), 70, Color.DarkOrange, Main.rand.NextFloat(0.2f, 0.5f));
                }
                Vector2 smokeVelo = new Vector2(1, -0.05f * player.direction).RotatedBy(velocity.ToRotation());
                Dust.NewDustPerfect(position + offset * 1.5f, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + smokeVelo.RotatedByRandom(0.35f) * 4.5f, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));

                Helper.PlayPitched("Guns/PlinkLever", 0.45f, Main.rand.NextFloat(-0.1f, 0.1f), position);
                Helper.PlayPitched("Guns/RifleLight", 0.75f, Main.rand.NextFloat(-0.1f, 0.1f), position);
                Projectile.NewProjectileDirect(source, position, velocity * 1.4f, type, damage, knockback, player.whoAmI).GetGlobalProjectile<CoachGunUpgradeGlobalProj>().ShotFromGun = true;

                Gore.NewGore(source, player.Center + (offset / 2), new Vector2(player.direction * -1, -0.5f) * 2, Mod.Find<ModGore>("CoachGunUpgradeCasing").Type, 1f);
                return false;
            }
            return true;
        }
    }

    public class CoachGunUpgradeGlobalProj : GlobalProjectile //this is needed cause otherwise every projectile could break the crystal, only bullets fired from the gun should break the crystal
    {
        public override bool InstancePerEntity => true;

        public bool ShotFromGun = false;

        public IEntitySource entitySource;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            entitySource = source;
        }
    }

    internal class CoachGunUpgradeBomb : ModProjectile
    {
        float VeloMult;

        bool shrapnel = false;
        bool beenTouched = false; //kinda bad name but I could really think of a better one, basically if its been touched by a bullet or explosion, so that the explosion effects dont keep happening if the explosion is still intersecting the projectile, which made things like the projectile doing millions of damage
        public override string Texture => AssetDirectory.VitricBoss + "VitricBomb";

        public override void Load()
        {
            for (int i = 1; i < 5; i++)
            {
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricItem + Name + "_Gore" + i);
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystal Bomb");
            Main.projFrames[Projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = false;
            Projectile.timeLeft = 240;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f && Main.myPlayer == Projectile.owner)
            {
                float dist = Vector2.Distance(Projectile.Center, Main.MouseWorld);
                VeloMult = 0.95f + (dist / 15000);
                VeloMult = MathHelper.Clamp(VeloMult, 0.95f, 0.98f);
                Projectile.localAI[0] += 1f;
            }
            if (++Projectile.frameCounter >= 8 - (240 / Projectile.timeLeft))
            {
                Projectile.frameCounter = 0;

                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

            Projectile.velocity *= VeloMult;

            Projectile.rotation += 0.2f * (Projectile.velocity.X * 0.05f);

            var list = Main.projectile.Where(x => x.Distance(Projectile.Center) < 50f || x.Hitbox.Intersects(Projectile.Hitbox));
            foreach (var proj in list)
            {
                if ((proj.GetGlobalProjectile<CoachGunUpgradeGlobalProj>().ShotFromGun || proj.type == ModContent.ProjectileType<CoachGunUpgradeExplosion>()) && Projectile.timeLeft > 2 && proj.active && !beenTouched)
                {
                    if (proj.GetGlobalProjectile<CoachGunUpgradeGlobalProj>().ShotFromGun)
                    {
                        if (Projectile.timeLeft > 195)
                            ExplodeIntoShards(proj.velocity * 1.85f);

                        proj.penetrate--;
                        Projectile.Kill();
                    }

                    if (proj.type == ModContent.ProjectileType<CoachGunUpgradeExplosion>())
                    {
                        if (Projectile.localAI[0] == 0f)
                        {
                            Projectile.damage = (int)(Projectile.damage * 1.5f);
                            Projectile.localAI[0] = 1f;
                        }
                        Projectile.timeLeft = 10;
                    }
                    beenTouched = true;
                }
            }

            if (Main.rand.NextBool(23))
            {
                if (Main.rand.NextBool())
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.CrystalSparkle>(), 0, 0);
                else if (Main.rand.NextBool())
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.CrystalSparkle2>(), 0, 0);
                else
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.FireSparkle>(), 0, 0);
            }
        }

        public override void Kill(int timeLeft)
        {
            Helper.PlayPitched("GlassMiniboss/GlassSmash", 0.5f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.position);

            for (int i = 1; i < 5; i++)
            {
                Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0.5f, Mod.Find<ModGore>(Name + "_Gore" + i).Type).timeLeft = 90;
            }

            CameraSystem.Shake += 10;
            if (!shrapnel)
            {
                if (Main.myPlayer == Projectile.owner)
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CoachGunUpgradeExplosion>(), (int)(Projectile.damage * 1.35f), 0f, Projectile.owner, 75);


                if (Main.myPlayer == Projectile.owner)
                {
                    for (int k = 0; k < 5; k++)
                    {
                        Vector2 vel = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.4f, 0.5f);

                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Main.rand.NextVector2Circular(100, 100), vel, ModContent.ProjectileType<NeedlerEmber>(), 0, 0);
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(60, 60), ModContent.DustType<Dusts.Glow>(),
                        Main.rand.NextVector2Circular(5, 5), 0, new Color(255, 150, 50), 0.95f);

                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(60, 60), ModContent.DustType<Dusts.CoachGunDust>(),
                        Main.rand.NextVector2Circular(10, 10), 70 + Main.rand.Next(60), default, Main.rand.NextFloat(1.5f, 1.9f)).rotation = Main.rand.NextFloat(6.28f);

                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(60, 60), ModContent.DustType<Dusts.CoachGunDustTwo>(),
                        Main.rand.NextVector2Circular(10, 10), 80 + Main.rand.Next(40), default, Main.rand.NextFloat(1.5f, 1.9f)).rotation = Main.rand.NextFloat(6.28f);

                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(60, 60), ModContent.DustType<Dusts.CoachGunDustFour>()).scale = 0.9f;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            float offsetX = 20f;
            origin.X = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX);

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle, Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            //drawing glow, well not anymore :(
            /*Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.VitricItem + Name + "Glow").Value;

            sourceRectangle = tex.Frame(1, Main.projFrames[Projectile.type], frameY: Projectile.frame);
            origin = sourceRectangle.Size() / 2f;
            origin.X = Projectile.spriteDirection == 1 ? sourceRectangle.Width - 21f : 21f;
               
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, sourceRectangle, Color.White, Projectile.rotation, origin, 1, spriteEffects, 0);*/

            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Helper.PlayPitched("GlassMiniboss/GlassBounce", 0.15f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.position);

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X;

            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y;

            return false;
        }

        public void ExplodeIntoShards(Vector2 velocity)
        {
            shrapnel = true;
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = (velocity.RotatedByRandom(MathHelper.ToRadians(8f))) * Main.rand.NextFloat(0.8f, 1.1f);
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, vel * 1.45f, ModContent.ProjectileType<CoachGunUpgradeShards>(), (int)(Projectile.damage * 0.33f), 0.5f, Projectile.owner);
                }
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CoachGunUpgradeExplosion>(), (int)(Projectile.damage * 1.3f), 0f, Projectile.owner, 40, 1f);
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = (velocity.RotatedByRandom(MathHelper.ToRadians(5f))) * Main.rand.NextFloat(0.2f, 0.4f);
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.width, ModContent.DustType<VitricBombDust>(), vel.X, vel.Y, 0, default, Main.rand.NextFloat(0.7f, 0.9f)).noGravity = true;

                for (int d = 0; d < 2; d++)
                {
                    Vector2 velo = (velocity.RotatedByRandom(MathHelper.ToRadians(12f))) * Main.rand.NextFloat(0.25f, 0.45f);
                    Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), velo, 0, new Color(255, 150, 50), 0.85f);
                }

                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.CoachGunDustFour>(), (velocity.RotatedByRandom(MathHelper.ToRadians(10f))) * Main.rand.NextFloat(0.3f, 0.50f)).scale = 0.8f;
            }

            for (int i = 0; i < 8; i++)
            {
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<Dusts.Glow>(),
                    Main.rand.NextVector2Circular(4, 4), 0, new Color(255, 150, 50), 0.95f);

                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<Dusts.CoachGunDust>(),
                    Main.rand.NextVector2Circular(8, 8), 70 + Main.rand.Next(60), default, Main.rand.NextFloat(1.5f, 1.9f)).rotation = Main.rand.NextFloat(6.28f);

                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<Dusts.CoachGunDustTwo>(),
                    Main.rand.NextVector2Circular(8, 8), 80 + Main.rand.Next(40), default, Main.rand.NextFloat(1.5f, 1.9f)).rotation = Main.rand.NextFloat(6.28f);

                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<Dusts.CoachGunDustFour>()).scale = 0.9f;
            }

            for (int i = 0; i < 15; i++)
            {
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), ModContent.DustType<Dusts.Glow>(),
                    (velocity.RotatedByRandom(MathHelper.ToRadians(15f))) * Main.rand.NextFloat(0.2f, 0.5f), 0, new Color(255, 150, 50), 0.95f);

                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), ModContent.DustType<Dusts.CoachGunDust>(),
                    (velocity.RotatedByRandom(MathHelper.ToRadians(40f))) * Main.rand.NextFloat(0.7f, 1.8f), 70 + Main.rand.Next(60), default, Main.rand.NextFloat(1.5f, 1.9f)).rotation = Main.rand.NextFloat(6.28f);

                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), ModContent.DustType<Dusts.CoachGunDustTwo>(),
                    (velocity.RotatedByRandom(MathHelper.ToRadians(40f))) * Main.rand.NextFloat(0.7f, 1.8f), 80 + Main.rand.Next(40), default, Main.rand.NextFloat(1.5f, 1.9f)).rotation = Main.rand.NextFloat(6.28f);

                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), ModContent.DustType<Dusts.CoachGunDustFour>(),
                    (velocity.RotatedByRandom(MathHelper.ToRadians(35f))) * Main.rand.NextFloat(0.15f, 0.35f)).scale = 0.9f;
            }
        }
    }   

    public class VitricBombDust : ModDust
    {
        public override string Texture => AssetDirectory.Dust + Name;

        public override void OnSpawn(Dust dust)
        {
            dust.noLight = false;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += 0.12f;
            dust.scale *= 0.98f;
            if (dust.scale <= 0.2)
                dust.active = false;

            if (!dust.noGravity)
                dust.velocity.Y += 0.15f;

            return false;
        }
    }

    class SwelteredDeBuff : SmartBuff
    {
        public override string Texture => AssetDirectory.Debug;

        public SwelteredDeBuff() : base("Sweltered", "Damage taken increased by 35%", false) { }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<StarlightNPC>().DoT += 10;
            npc.GetGlobalNPC<ExposureNPC>().ExposureMultAll += 0.35f;
            Vector2 vel = new Vector2(0, -1).RotatedByRandom(0.5f) * 0.4f;
            if (Main.rand.NextBool(4))
                Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<CoachSmoke>(), vel.X, vel.Y, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));

            if (Main.rand.NextBool(2))
                Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<Dusts.Glow>(), 0, 0, 0,Color.DarkOrange, 0.6f);
        }
    }
}
