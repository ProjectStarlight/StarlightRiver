using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.ChestLootSystem;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ObjectData;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Misc
{
    public class Sling : MultiAmmoWeapon
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public override List<AmmoStruct> ValidAmmos => new() 
        {
            new AmmoStruct(ItemID.Seed, ModContent.ProjectileType<SlingSeedProjectile>(), shootspeed: -5f), //seed projectile has 1 extra update, so shootspeed is decreased to compensate, effective shootspeed is around 20
            new AmmoStruct(ItemID.StoneBlock, ModContent.ProjectileType<SlingStoneProjectile>(), 5, -4f, 2.5f),
            new AmmoStruct(ItemID.Mushroom, ModContent.ProjectileType<SlingMushroomProjectile>(), -2)
        };

        public override bool SafeCanUseItem(Player player)
        {
            if (!hasAmmo)
            {
                Item.noUseGraphic = false;
                player.itemTime = 0;
                return false;
            }
            Item.noUseGraphic = true;
            player.itemTime = 2;
            if (currentAmmoStruct.ammoID == ItemID.Seed)
                Item.useAnimation = Item.useTime = 50;

            if (currentAmmoStruct.ammoID == ItemID.Mushroom)
                Item.useAnimation = Item.useTime = 60;

            if (currentAmmoStruct.ammoID == ItemID.StoneBlock)
                Item.useAnimation = Item.useTime = 75;
            return player.ownedProjectileCounts[ModContent.ProjectileType<SlingHeldProjectile>()] <= 0;
        }
        public override bool CanConsumeAmmo(Item ammo, Player player) => false;
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Sling seeds, stones, and mushrooms at your enemies\n" +
                "Seeds have increase velocity\n" +
                "Stones have less velocity but deal more damage and knockback\n" +
                "Mushrooms deal less damage but inflict Poisoned\n" +
                "75% chance to not consume ammo");
        }

        public override void SafeSetDefaults()
        {
            Item.knockBack = 3.5f;
            Item.damage = 18;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 55;
            Item.useAnimation = 55;
            Item.noMelee = true;

            Item.UseSound = new Terraria.Audio.SoundStyle("StarlightRiver/Sounds/Effects/Sling_Pullback") with {Volume = 0.65f, Pitch = 0.15f};
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.holdStyle = ItemHoldStyleID.HoldHeavy;

            Item.value = Item.sellPrice(silver: 25);
            Item.rare = ItemRarityID.White;

            Item.shootSpeed = 15.5f;
            Item.width = Item.height = 20;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }

        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.itemTime > 1)
                return;

            if (Main.MouseWorld.X > player.Center.X)
                player.ChangeDir(1);
            else
                player.ChangeDir(-1);
            Vector2 itemPosition = player.MountedCenter + new Vector2(4f * player.direction, -1f * player.gravDir);
            float rotation = itemPosition.DirectionTo(Main.MouseWorld).ToRotation();
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rotation - (player.direction == 1 ? MathHelper.ToRadians(60f) : MathHelper.ToRadians(120f)));
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation - (player.direction == 1 ? MathHelper.ToRadians(120f) : MathHelper.ToRadians(60f)));
            Vector2 itemSize = new Vector2(22);
            Vector2 itemOrigin = new Vector2(-6f, 0f);
            HoldStyleAdjustments(player, rotation, itemPosition, itemSize, itemOrigin, true, false, true);
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation += player.DirectionTo(Main.MouseWorld).SafeNormalize(player.direction * Vector2.UnitX) * -9f;
        }

        public override void UseAnimation(Player player)
        {
            Item.noUseGraphic = true;
            if (Main.myPlayer == player.whoAmI)
            {
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.Center.DirectionTo(Main.MouseWorld).ToRotation() - MathHelper.ToRadians(60f));
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, player.Center.DirectionTo(Main.MouseWorld).ToRotation() - MathHelper.ToRadians(30f));
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, ModContent.ProjectileType<SlingHeldProjectile>(), damage, knockback, player.whoAmI,
                currentAmmoStruct.projectileID, Item.shootSpeed + currentAmmoStruct.ShootSpeed);
            return false;
        }

        public void HoldStyleAdjustments(Player player, float desiredRotation, Vector2 desiredPosition, Vector2 spriteSize, Vector2? rotationOriginFromCenter = null, bool noSandstorm = false, bool flipAngle = false, bool stepDisplace = true)
        {
            if (noSandstorm)
                player.sandStorm = false;

            if (rotationOriginFromCenter == null)
                rotationOriginFromCenter = new Vector2?(Vector2.Zero);

            Vector2 origin = rotationOriginFromCenter.Value;
            origin.X *= player.direction;
            origin.Y *= player.gravDir;
            player.itemRotation = desiredRotation;

            if (flipAngle)
                player.itemRotation *= player.direction;
            else if (player.direction < 0)
                player.itemRotation += 3.1415927f;

            Vector2 consistentAnchor = player.itemRotation.ToRotationVector2() * (spriteSize.X / -2f - 10f) * player.direction - origin.RotatedBy(player.itemRotation, default(Vector2));
            Vector2 offsetAgain = spriteSize * -0.5f;
            Vector2 finalPosition = desiredPosition + offsetAgain + consistentAnchor;
            if (stepDisplace)
            {
                int frame = player.bodyFrame.Y / player.bodyFrame.Height;
                if ((frame > 6 && frame < 10) || (frame > 13 && frame < 17))
                    finalPosition -= Vector2.UnitY * 2f;
            }
            player.itemLocation = finalPosition;
        }
    }

    class SlingGlobalTile : GlobalTile //probably bad method of doing this
    {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Vector2 worldPosition = new Vector2(i, j).ToWorldCoordinates();
            Player nearestPlayer = Main.player[Player.FindClosest(worldPosition, 16, 16)];
            if (nearestPlayer.HasItem(ModContent.ItemType<Sling>()))
                if ((type == TileID.Plants || type == TileID.Plants2 || type == TileID.PottedPlants1 || type == TileID.PottedPlants2) && Main.rand.NextFloat() < 0.75f)
                    Item.NewItem(new EntitySource_TileBreak(i, j), worldPosition, 1, 1, ItemID.Seed, Main.rand.Next(1, 4));
        }
    }

    class SlingHeldProjectile : ModProjectile
    {
        private int ticksPerFrame;

        private bool initialized;

        private bool shotProjectile;

        private ref float ProjectileToShoot => ref Projectile.ai[0];

        private ref float shootSpeed => ref Projectile.ai[1];
        private Player Owner => Main.player[Projectile.owner];
        public override bool? CanDamage() => false;
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sling");
            Main.projFrames[Type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = 68;
            Projectile.height = 22;

            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 2;
            Projectile.alpha = 255;
        }

        public override bool PreAI()
        {
            return true;
        }

        public override void AI()
        {
            if (!initialized)
            {
                initialized = true;
                Projectile.timeLeft = (int)(Owner.HeldItem.useAnimation * Owner.GetTotalAttackSpeed(DamageClass.Ranged));
                ticksPerFrame = (int)System.Math.Round((float)(Projectile.timeLeft / Main.projFrames[Type]));
            }

            if (Main.myPlayer == Projectile.owner)
            {
                float interpolant = Utils.GetLerpValue(5f, 25f, Projectile.Distance(Main.MouseWorld), true);

                Vector2 oldVelocity = Projectile.velocity;

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Owner.DirectionTo(Main.MouseWorld), interpolant);
                if (Projectile.velocity != oldVelocity)
                {
                    Projectile.netSpam = 0;
                    Projectile.netUpdate = true;
                }
            }
            Projectile.alpha -= 50;
            Vector2 pos = Owner.RotatedRelativePoint(Owner.MountedCenter + new Vector2(0, -7f * Owner.gravDir), true);
            pos += Projectile.velocity.SafeNormalize(Owner.direction * Vector2.UnitX) * 1f;
            if (Main.myPlayer == Owner.whoAmI && Main.MouseWorld.Y > Owner.Center.Y)
                pos.X += 6 * Owner.direction;
            Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
            if (Projectile.frame == 2)
                stretch = Player.CompositeArmStretchAmount.ThreeQuarters;
            if (Projectile.frame == 3)
                stretch = Player.CompositeArmStretchAmount.Quarter;
            if (Projectile.frame == 4)
                stretch = Player.CompositeArmStretchAmount.None;
            Owner.SetCompositeArmFront(true, stretch, Projectile.rotation - MathHelper.ToRadians(120f) * Projectile.direction);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(60f) * Projectile.direction);
            Projectile.rotation = Utils.ToRotation(Projectile.velocity);
            Projectile.position = pos - Projectile.Size * 0.5f;
            Projectile.spriteDirection = Projectile.direction;
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;
            if (Projectile.spriteDirection == -1)
                Projectile.rotation += 3.1415927f;

            if (Projectile.frame == 4 && !shotProjectile)
            {
                ShootSlingProjectile();
                shotProjectile = true;
            }

            if (++Projectile.frameCounter >= ticksPerFrame)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.timeLeft = 1;
                    Owner.itemTime = 0;
                    Owner.HeldItem.noUseGraphic = false;
                    return;
                }
            }
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);
        }

        public void ShootSlingProjectile()
        {
            CameraSystem.Shake += 2;
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 pos = Owner.RotatedRelativePoint(Owner.MountedCenter + new Vector2(-4f * Owner.direction, -5f * Owner.gravDir), true);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, Projectile.DirectionTo(Main.MouseWorld) * shootSpeed, (int)ProjectileToShoot, Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
            Helper.PlayPitched("Effects/HeavyWhooshShort", 0.6f, 0.15f, Projectile.position);
            if (Owner.HeldItem.ModItem is Sling sling)
            {
                int type = sling.currentAmmoStruct.projectileID;
                bool dontConsumeAmmo = false;

                if (Owner.magicQuiver && sling.ammoItem.ammo == AmmoID.Arrow && Main.rand.NextBool(5))
                    dontConsumeAmmo = true;
                if (Owner.ammoBox && Main.rand.NextBool(5))
                    dontConsumeAmmo = true;
                if (Owner.ammoPotion && Main.rand.NextBool(5))
                    dontConsumeAmmo = true;
                if (Owner.ammoCost80 && Main.rand.NextBool(5))
                    dontConsumeAmmo = true;
                if (Owner.ammoCost75 && Main.rand.NextBool(4))
                    dontConsumeAmmo = true;
                if (type == 85 && Owner.itemAnimation < Owner.itemAnimationMax - 6)
                    dontConsumeAmmo = true;
                if ((type == 145 || type == 146 || (type == 147 || type == 148) || type == 149) && Owner.itemAnimation < Owner.itemAnimationMax - 5)
                    dontConsumeAmmo = true;

                if (!dontConsumeAmmo)
                {
                    if (Main.rand.NextFloat() < 0.25f) //consume ammo 25% of the time
                    {
                        if (sling.ammoItem.ModItem != null)
                            sling.ammoItem.ModItem.OnConsumedAsAmmo(Owner.HeldItem, Owner);

                        sling.OnConsumeAmmo(sling.ammoItem, Owner);

                        sling.ammoItem.stack--;
                        if (sling.ammoItem.stack <= 0)
                            sling.ammoItem.TurnToAir();
                    }
                }
            }
        }
    }

    class SlingMushroomProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Mushroom;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shroom");
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;

             Projectile.width = Projectile.height = 22;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.25f;
            if (Projectile.velocity.Y > 0)
            {
                if (Projectile.velocity.Y < 12f)
                    Projectile.velocity.Y *= 1.075f;
                else
                    Projectile.velocity.Y *= 1.035f;
            }
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadItem(ItemID.Mushroom);
            Texture2D tex = TextureAssets.Item[ItemID.Mushroom].Value;
            Vector2 drawOrigin = tex.Size() / 2f;
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(tex, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Poisoned, Main.rand.Next(new int[] { 180, 240, 300 }));
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.MushroomDust>()).scale = Main.rand.NextFloat(0.8f, 1.2f);
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Grass with { Pitch = -0.1f, Volume = 1.25f }, Projectile.Center);
        }
    }

    class SlingStoneProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.StoneBlock;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Stone");
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;

            Projectile.width = Projectile.height = 16;
        }
        public override void AI()
        {
            Projectile.rotation += (0.35f * (Projectile.velocity.X * 0.15f)) * Projectile.direction;
            Projectile.velocity.Y += 0.2f;
            if (Projectile.velocity.Y > 0)
            {
                if (Projectile.velocity.Y < 13f)
                    Projectile.velocity.Y *= 1.085f;
                else
                    Projectile.velocity.Y *= 1.04f;
            }
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = tex.Size() / 2f;
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(tex, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale * MathHelper.Lerp(1, 0.5f, k / (float)Projectile.oldPos.Length), SpriteEffects.None, 0);
            }
            return true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            CameraSystem.Shake += 1;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Stone).scale = Main.rand.NextFloat(0.75f, 1.2f);
            }
            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with { Volume = 0.8f, Pitch = -0.1f }, Projectile.position);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            return true;
        }
    }

    class SlingSeedProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.IvyItem + "SeedProjectile"; //vanilla seed looks bad

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Seed");
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.timeLeft = 480;
            Projectile.penetrate = 1;

            Projectile.width = 8;
            Projectile.height = 12;
            Projectile.extraUpdates = 1;
            Projectile.aiStyle = 1;
            AIType = ProjectileID.Bullet;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);
            Projectile.velocity.Y += 0.05f;
            if (Projectile.velocity.Y > 0)
                Projectile.velocity.Y *= 1.035f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = tex.Size() / 2f;
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(tex, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale * MathHelper.Lerp(1, 0.25f, k / (float)Projectile.oldPos.Length), SpriteEffects.None, 0);
            }
            return true;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt).scale = Main.rand.NextFloat(0.45f, 0.7f);
            }
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }
}
