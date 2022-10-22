using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Misc.SoilgunFiles;
using StarlightRiver.Core;
using StarlightRiver.Core.Systems;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    public class Earthduster : MultiAmmoWeapon
    {
        public override List<AmmoStruct> ValidAmmos => new List<AmmoStruct>()
        {
            new AmmoStruct(ItemID.SandBlock, ModContent.ProjectileType<SoilgunSandSoil>(), 2),
            new AmmoStruct(ItemID.EbonsandBlock, ModContent.ProjectileType<SoilgunEbonsandSoil>(), 4),
            new AmmoStruct(ItemID.PearlsandBlock, ModContent.ProjectileType<SoilgunPearlsandSoil>(), 15),
            new AmmoStruct(ItemID.CrimsandBlock, ModContent.ProjectileType<SoilgunCrimsandSoil>(), 4),
            new AmmoStruct(ItemID.DirtBlock, ModContent.ProjectileType<SoilgunDirtSoil>(), 2),
            new AmmoStruct(ItemID.SiltBlock, ModContent.ProjectileType<SoilgunSiltSoil>(), 3),
            new AmmoStruct(ItemID.SlushBlock, ModContent.ProjectileType<SoilgunSlushSoil>(), 3),
            new AmmoStruct(Mod.Find<ModItem>("VitricSandItem").Type, ModContent.ProjectileType<SoilgunVitricSandSoil>(), 8),
            new AmmoStruct(ItemID.MudBlock, ModContent.ProjectileType<SoilgunMudSoil>(), 3),
        };

        public override bool CanConsumeAmmo(Item ammo, Player player) => false;
        public override bool SafeCanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<EarthdusterHoldout>()] <= 0;
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Hold <left> to fire a rapid stream of soil\nCan use many different types of soils");
        }

        public override void SafeSetDefaults()
        {
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 25;
            Item.width = 70;
            Item.height = 40;
            Item.useAnimation = Item.useTime = 8;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.rare = ItemRarityID.Orange;
            Item.shootSpeed = 21f;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.knockBack = 3f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile proj = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<EarthdusterHoldout>(), damage, knockback, player.whoAmI);

            if (proj.ModProjectile is EarthdusterHoldout earthduster)
                earthduster.SoilType = type;
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ModContent.ItemType<Soilgun>()).
                AddIngredient(ItemID.CrimtaneBar, 12).
                AddIngredient(ItemID.Bone, 50).
                AddTile(TileID.Anvils).
                Register();

            CreateRecipe().
                AddIngredient(ModContent.ItemType<Earthduster>()).
                AddIngredient(ItemID.CrimtaneBar, 12).
                AddIngredient(ItemID.Bone, 50).
                AddTile(TileID.Anvils).
                Register();
        }
    }

    class EarthdusterHoldout : ModProjectile
    {
        public const int MAXSHOTS = 100;

        public const int MAXCHARGEDELAY = 30;

        public int SoilType;

        public int shots;

        public float rotTimer;

        public bool charged;

        public bool draw; //only draw two ticks after spawning

        public bool reloading;

        public Vector2 mouse;

        public ref float ShootDelay => ref Projectile.ai[0];

        public ref float MaxShootDelay => ref Projectile.ai[1];

        public Player owner => Main.player[Projectile.owner];

        public bool CanHold => owner.channel && !owner.CCed && !owner.noItems;

        public override string Texture => AssetDirectory.MiscItem + Name;

        public override bool? CanDamage() => false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Earthduster");
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.width = 72;
            Projectile.height = 62;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Vector2 armPos = owner.RotatedRelativePoint(owner.MountedCenter, true);
            armPos += Utils.SafeNormalize(Projectile.velocity, Vector2.UnitX) * 20f;
            armPos += Vector2.UnitY.RotatedBy(Projectile.velocity.ToRotation()) * -10f * owner.direction;
            Vector2 barrelPos = armPos + (Projectile.velocity * 25f);

            if (MaxShootDelay == 0f)
                MaxShootDelay = CombinedHooks.TotalUseTime(owner.HeldItem.useTime, owner, owner.HeldItem);

            if (rotTimer > 0)
                rotTimer--;

            if (!CanHold && !reloading)
            {
                Projectile.Kill();
                return;
            }

            if (shots < MAXSHOTS && !reloading)
            {
                ShootDelay++;

                if (ShootDelay < MAXCHARGEDELAY && !charged)
                {
                    if (ShootDelay > 2)
                    {
                        draw = true;
                        for (int i = 0; i < 3; i++)
                        {
                            float lerper = MathHelper.Lerp(50f, 1f, ShootDelay / MAXCHARGEDELAY);
                            Vector2 pos = barrelPos + Main.rand.NextVector2CircularEdge(lerper * 0.5f, lerper).RotatedBy(Projectile.rotation);
                            Dust.NewDustPerfect(pos, DustID.Dirt, pos.DirectionTo(barrelPos)).noGravity = true;
                        }
                    }
                }
                else if (ShootDelay < (MAXCHARGEDELAY * 1.5f) && !charged)
                {
                    if (ShootDelay == MAXCHARGEDELAY)
                    {
                        SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.5f}, owner.Center);
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), armPos, Projectile.velocity * 1.5f, ModContent.ProjectileType<EarthdusterRing>(), 0, 0f, owner.whoAmI, 15f);

                            (proj.ModProjectile as EarthdusterRing).trailColorOutline = GetRingColor();

                            (proj.ModProjectile as EarthdusterRing).trailColor = GetRingInsideColor();
                        }
                    }
                }
                else if (ShootDelay >= MaxShootDelay)
                {
                    if (!charged)
                        charged = true;

                    ShootSoils(barrelPos);
                    shots++;
                    ShootDelay = 0;
                }

                if (draw)
                {
                    Vector2 dirtPos = armPos + new Vector2(-30, MathHelper.Lerp(-20f, -10f, shots / (float)MAXSHOTS) * owner.direction).RotatedBy(Projectile.rotation);
                    if (Main.rand.NextBool(20))
                        Dust.NewDustDirect(dirtPos, 20, 5, DustID.Dirt).alpha = 50;
                }
            }
            else
            {
                if (!reloading)
                {
                    if (Main.myPlayer == Projectile.owner)
                        mouse = owner.DirectionTo(Main.MouseWorld);

                    ShootDelay = 0;
                    reloading = true;
                }

                if (++ShootDelay < 60)
                {
                    float progress = EaseBuilder.EaseCircularInOut.Ease(ShootDelay / 60f);
                    Projectile.velocity = Vector2.One.RotatedBy(mouse.ToRotation() + MathHelper.ToRadians(MathHelper.Lerp(0f, 360f, progress)) - MathHelper.PiOver4);

                    shots = (int)MathHelper.Lerp(MAXSHOTS, 0, progress);
                }
                else if (ShootDelay < 70)
                {
                    armPos -= Utils.SafeNormalize(Projectile.velocity, Vector2.UnitX) * MathHelper.Lerp(0f, 5f, (ShootDelay - 60) / 10f);
                }
                else if (ShootDelay < 80)
                {
                    armPos -= Utils.SafeNormalize(Projectile.velocity, Vector2.UnitX) * MathHelper.Lerp(5f, -5f, (ShootDelay - 70) / 10f);
                }
                else
                {
                    armPos -= Utils.SafeNormalize(Projectile.velocity, Vector2.UnitX) * -5f;
                    if (ShootDelay > 90)
                        Projectile.Kill();
                }
            }

            if (!reloading)
                owner.ChangeDir(Projectile.direction);

            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            Projectile.timeLeft = 2;
            Projectile.rotation = Utils.ToRotation(Projectile.velocity) - (Projectile.direction == -1 ? -MathHelper.ToRadians(rotTimer) : MathHelper.ToRadians(rotTimer));
            owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

            Projectile.position = armPos - Projectile.Size * 0.5f;

            Projectile.spriteDirection = Projectile.direction;

            if (Main.myPlayer == Projectile.owner && !reloading)
            {
                float interpolant = Utils.GetLerpValue(1f, 5f, Projectile.Distance(Main.MouseWorld), true);

                Vector2 oldVelocity = Projectile.velocity;

                Projectile.velocity = Vector2.One.RotatedBy(Vector2.Lerp(Projectile.velocity, owner.DirectionTo(Main.MouseWorld), interpolant).ToRotation() - MathHelper.PiOver4);
                if (Projectile.velocity != oldVelocity)
                {
                    Projectile.netSpam = 0;
                    Projectile.netUpdate = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!draw)
                return false;

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D dirtTex = ModContent.Request<Texture2D>(Texture + "_Dirt").Value;

            Vector2 offset = Vector2.Lerp(Vector2.Zero, Vector2.UnitY.RotatedBy(Projectile.rotation + (owner.direction == -1 ? MathHelper.Pi : 0f)) * 13f, shots / (float)MAXSHOTS);
            Main.spriteBatch.Draw(dirtTex, (Projectile.Center + offset) - Main.screenPosition, null, lightColor, Projectile.rotation, dirtTex.Size() / 2f, Projectile.scale, owner.direction == -1 ? SpriteEffects.FlipVertically : 0f, 0f);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, owner.direction == -1 ? SpriteEffects.FlipVertically : 0f, 0f);
            return false;
        }

        public Color GetRingColor()
        {
            switch (SoilType) //switch cant use non constants without this wacko work around
            {
                case var dirt when dirt == ModContent.ProjectileType<SoilgunDirtSoil>(): return new Color(81, 47, 27);
            }
            return Color.White;
        }

        public Color GetRingInsideColor()
        {
            switch (SoilType) //switch cant use non constants without this wacko work around
            {
                case var dirt when dirt == ModContent.ProjectileType<SoilgunDirtSoil>(): return new Color(105, 67, 44);
            }
            return Color.White;
        }

        public void ShootSoils(Vector2 position)
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            Item heldItem = owner.HeldItem;

            if (owner.HeldItem.ModItem is Earthduster gun && gun.ammoItem is null)
            {
                Projectile.Kill();
                return;
            }

            int damage = Projectile.damage;

            float shootSpeed = heldItem.shootSpeed;

            float knockBack = owner.GetWeaponKnockback(heldItem, heldItem.knockBack);

            Vector2 shootVelocity = Utils.SafeNormalize(Projectile.velocity, Vector2.UnitY) * shootSpeed;

            if (Main.myPlayer == Projectile.owner)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, (shootVelocity.RotatedByRandom(MathHelper.ToRadians(5))) * Main.rand.NextFloat(0.9f, 1.1f), SoilType, damage, knockBack, owner.whoAmI);

            for (float k = 0; k < 50; k++)
            {
                float rads = 6.28f * (k / 50f);
                float x = (float)Math.Cos(rads) * 50;
                float y = (float)Math.Sin(rads) * 25;

                Dust.NewDustPerfect(position, DustID.Dirt, new Vector2(x, y).RotatedBy(Projectile.rotation + MathHelper.PiOver2) * 0.055f + (Vector2.UnitX.RotatedBy(Projectile.rotation) * 5f), 0, default, 0.85f).noGravity = true;
            }

            shots++;
            SoundEngine.PlaySound(SoundID.Item11, Projectile.position);

            CameraSystem.Shake += 1;

            rotTimer += 8;

            if (owner.HeldItem.ModItem is Earthduster earthduster)
            {
                int type = earthduster.currentAmmoStruct.projectileID; // this code is still bad
                bool dontConsumeAmmo = false;

                if (owner.magicQuiver && earthduster.ammoItem.ammo == AmmoID.Arrow && Main.rand.NextBool(5))
                    dontConsumeAmmo = true;
                if (owner.ammoBox && Main.rand.NextBool(5))
                    dontConsumeAmmo = true;
                if (owner.ammoPotion && Main.rand.NextBool(5))
                    dontConsumeAmmo = true;
                if (owner.ammoCost80 && Main.rand.NextBool(5))
                    dontConsumeAmmo = true;
                if (owner.ammoCost75 && Main.rand.NextBool(4))
                    dontConsumeAmmo = true;
                if (type == 85 && owner.itemAnimation < owner.itemAnimationMax - 6)
                    dontConsumeAmmo = true;
                if ((type == 145 || type == 146 || (type == 147 || type == 148) || type == 149) && owner.itemAnimation < owner.itemAnimationMax - 5)
                    dontConsumeAmmo = true;

                if (!dontConsumeAmmo)
                {
                    if (earthduster.ammoItem.ModItem != null)
                        earthduster.ammoItem.ModItem.OnConsumedAsAmmo(owner.HeldItem, owner);

                    earthduster.OnConsumeAmmo(earthduster.ammoItem, owner);

                    earthduster.ammoItem.stack--;
                    if (earthduster.ammoItem.stack <= 0)
                        earthduster.ammoItem.TurnToAir();
                }
            }
        }
    }
    public class EarthdusterRing : ModProjectile, IDrawPrimitive
    {
        public override string Texture => AssetDirectory.Assets + "Invisible";

        public Color trailColor = Color.White;

        public Color trailColorOutline = Color.Black;

        private List<Vector2> cache;

        private Trail trail;
        private Trail trail2;

        public int timeLeftStart = 50;
        private float Progress => 1 - (Projectile.timeLeft / (float)timeLeftStart);

        private float Radius => Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = timeLeftStart;
            Projectile.extraUpdates = 1;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ring");
        }

        public override void AI()
        {
            Projectile.velocity *= 0.965f;
            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;

        private void ManageCaches()
        {
            cache = new List<Vector2>();
            float radius = Radius;
            for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
            {
                double rad = (i / 32f) * 6.28f;
                Vector2 offset = new Vector2((float)Math.Sin(rad) * 0.3f, (float)Math.Cos(rad));
                offset *= radius;
                offset = offset.RotatedBy(Projectile.velocity.ToRotation());
                cache.Add(Projectile.Center + offset);
            }

            while (cache.Count > 33)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 28 * (1 - Progress), factor =>
            {
                return trailColorOutline;
            });

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 10 * (1 - Progress), factor =>
            {
                return trailColor;
            });
            float nextplace = 33f / 32f;
            Vector2 offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
            offset *= Radius;

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + offset;

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = Projectile.Center + offset;
        }

        public void DrawPrimitives()
        {
            Effect effect = Terraria.Graphics.Effects.Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
            effect.Parameters["alpha"].SetValue(1);

            trail?.Render(effect);
            trail2?.Render(effect);
        }
    }
}
