using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using System.Text;
using StarlightRiver.Core.Systems;
using System.IO;
using StarlightRiver.Content.Items.Vitric;
using System.Linq;

namespace StarlightRiver.Content.Items.Misc.SoilgunFiles
{   
    // This entire thing needs balancing, hopefully this code is better than before and hopefully all issues are fixed

    //some visuals here and there could be polished idk

    //god i spent too much time on this
    public class Soilgun : ModItem
    { 
        public List<int> ValidSoils => new List<int>() { ItemID.SandBlock, ItemID.EbonsandBlock, ItemID.PearlsandBlock, ItemID.CrimsandBlock, ItemID.DirtBlock, ItemID.SiltBlock,
            ItemID.SlushBlock, Mod.Find<ModItem>("VitricSandItem").Type, ItemID.MudBlock};
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<SoilgunHoldout>()] <= 0 && player.GetModPlayer<SoilgunPlayer>().HasSoilItem;
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soilgun");
            Tooltip.SetDefault("Hold <left> to charge up a volley of soil\nRelease to fire the soil at high velocities\nCan use many different types of soils\n'Soiled it! SOILED IT!'"); //idk im bad at tooltips;
        }

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 14;
            Item.width = 60;
            Item.height = 36;
            Item.useAnimation = Item.useTime = 55;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.rare = ItemRarityID.Blue;
            Item.shootSpeed = 16f;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.knockBack = 1f;
        }   

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            damage = damage + GetSoilDamage(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<SoilgunHoldout>(), damage, knockback, player.whoAmI, 0, player.GetModPlayer<SoilgunPlayer>().SoilItem.type);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ItemID.DirtRod).AddIngredient(ItemID.Sandgun).AddIngredient(ItemID.DirtBlock, 45).AddTile(TileID.WorkBenches).Register();
        }

        public override void UpdateInventory(Player player)
        {
            //TODO: make this prioritize top left -> bottom right, instead of bottom right -> top left, like actual ammo
            for (int i = 0; i < player.inventory.Length; i++)
            {
                Item item = player.inventory[i];

                if (ValidSoils.Contains(item.type))
                {
                    SoilgunPlayer modplayer = player.GetModPlayer<SoilgunPlayer>();

                    modplayer.SoilItem = item;
                    modplayer.HasSoilItem = true;
                }
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.LocalPlayer.GetModPlayer<SoilgunPlayer>().SoilItem == null)
                return;

            TooltipLine AmmoLine = new TooltipLine(StarlightRiver.Instance, "AmmoLineToolTip", $"Current Ammo: [i:{Main.LocalPlayer.GetModPlayer<SoilgunPlayer>().SoilItem.type}]{Main.LocalPlayer.GetModPlayer<SoilgunPlayer>().SoilItem.stack}");
            tooltips.Add(AmmoLine);
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Main.playerInventory || Main.LocalPlayer.GetModPlayer<SoilgunPlayer>().SoilItem == null)
                return;

            Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, $"{ Main.LocalPlayer.GetModPlayer<SoilgunPlayer>().SoilItem.stack}", position.X, position.Y + 15, Color.White, Color.Black, origin, (scale + 0.2f) * Main.UIScale);
        }

        //increases damage depending on type of soil, like ammos
        public int GetSoilDamage(Player player)
        {
            if (player.GetModPlayer<SoilgunPlayer>().SoilItem.type == Mod.Find<ModItem>("VitricSandItem").Type)
                return 8;
            switch (player.GetModPlayer<SoilgunPlayer>().SoilItem.type)
            {
                case ItemID.SandBlock:
                    return 2;
                case ItemID.CrimsandBlock:
                    return 4;
                case ItemID.EbonsandBlock:
                    return 4;
                case ItemID.PearlsandBlock:
                    return 15;
                case ItemID.DirtBlock:
                    return 2;
                case ItemID.SiltBlock:
                    return 3;
                case ItemID.SlushBlock:
                    return 3;
                case ItemID.MudBlock:
                    return 3;
            }
            return 0;
        }
    }

    class SoilgunPlayer : ModPlayer
    {
        internal Item SoilItem;
        internal bool HasSoilItem;

        public override void ResetEffects()
        {
            SoilItem = null;
            HasSoilItem = false;
        }
    }

    class SoilgunGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public TooltipLine infoTooltip2;
        public List<int> ValidSoils => new List<int>() { ItemID.SandBlock, ItemID.EbonsandBlock, ItemID.PearlsandBlock, ItemID.CrimsandBlock, ItemID.DirtBlock, ItemID.SiltBlock,
            ItemID.SlushBlock, Mod.Find<ModItem>("VitricSandItem").Type, ItemID.MudBlock};
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            //there could probably be a better color for the tooltips here, also keywords should be used for some, like Haunted
            if (!Main.LocalPlayer.HasItem(ModContent.ItemType<Soilgun>()))
                return;

            if (ValidSoils.Contains(item.type))
            {
                TooltipLine tooltip = new TooltipLine(Mod, "SoilgunAmmoTooltip", "This item can be used as ammo for the Soilgun");
                tooltips.Add(tooltip);
                tooltip.OverrideColor = new Color(202, 148, 115);
                if (item.type == Mod.Find<ModItem>("VitricSandItem").Type)
                {
                    TooltipLine infoTooltip = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of glassy sand, that cause crystals to grow out of enemies\nFor each crystal an enemy has, they take two damage per second, plus a base damage of 4, up to a maximum of ten crystals\nIf an enemy has had ten crystals on them for more than four seconds, all crystals become charged, exploding shorty after");
                    tooltips.Add(infoTooltip);
                    infoTooltip.OverrideColor = new Color(202, 148, 115);
                    return;
                }
                switch (item.type)
                {
                    case ItemID.SandBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of sand that split into many grains of sand upon death"); break;
                    case ItemID.CrimsandBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of Crimsand that steal life from hit enemies"); break;
                    case ItemID.EbonsandBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun it will fire out blocks of Ebonsand that apply stacks of Haunted to enemies\nHaunted does 1 DPS per haunted stack, maxing at 20\nWhen an enemy has 20 Haunted stacks, they will cause haunted apparitions to exorcise from them, getting rid of all their Haunted stacks"); break;
                    case ItemID.PearlsandBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of Pearlsand that home in on enemies"); break;
                    case ItemID.DirtBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of dirt"); break;
                    case ItemID.SiltBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of silt, that spawn coins upon hitting enemies"); break;
                    case ItemID.SlushBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of slush that cause hit enemies to have icicles impale them\nHitting and enemy with more than 10 icicles causes all icicles to shatter, causing large amounts of damage"); break;
                    case ItemID.MudBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of mud that bounce off tiles and enemies"); break;
                }
                tooltips.Add(infoTooltip2);
                infoTooltip2.OverrideColor = new Color(202, 148, 115);
            }
        }
    }

    class SoilgunGlobalNPC : GlobalNPC
    {
        public const int MAXHAUNTEDSTACKS = 20;

        public const int MAXVITRICSHARDS = 10;

        public int GlassAmount;

        public int GlassPlayerID;

        public int HauntedStacks;

        public int HauntedTimer;

        public int HauntedSoulDamage;

        public int HauntedSoulOwner;

        public int SpawnHauntedSoulTimer = 60;

        public int ShardAmount;

        public int ShardTimer;

        public int ShardOwner;

        public int HowLongShardHasBeenOnTarget;
        public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc)
        {
            if (HauntedTimer > 0)
            {
                HauntedTimer--;
                if (HauntedTimer <= 0)
                {
                    HauntedTimer = 0;
                    HauntedStacks = 0;
                }
            }
            HauntedStacks = Utils.Clamp(HauntedStacks, 0, MAXHAUNTEDSTACKS);

            if (ShardTimer > 0)
            {
                ShardTimer--;
                if (ShardTimer <= 0)
                {
                    ShardTimer = 0;
                    ShardAmount = 0;
                }
            }
            ShardAmount = Utils.Clamp(ShardAmount, 0, MAXVITRICSHARDS);
        }
        public override void AI(NPC npc)
        {
            if (GlassAmount > 0)
            {
                if (Main.rand.NextBool(20 - GlassAmount))
                    Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Ice, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1.1f));
            }

            if (HauntedTimer > 0)
            {
                float Rand = MathHelper.Clamp(20 - HauntedStacks, 1f, 20f);
                if (Main.rand.NextBool((int)Rand))
                {
                    Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Shadowflame, 0f, 0f, 25, default, Main.rand.NextFloat(0.75f, 1.2f));
                    if (Main.rand.NextBool(3))
                        Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Shadowflame, 0f, 0f, 0, default, Main.rand.NextFloat(0.1f, 1.5f));
                }

                if (HauntedStacks == MAXHAUNTEDSTACKS)
                {
                    if (SpawnHauntedSoulTimer == 60)
                        Helper.PlayPitched("ShadowSpawn", 1f, Main.rand.NextFloat(-0.1f, 0.1f), npc.position);


                    if (SpawnHauntedSoulTimer % 5 == 0)
                        CameraSystem.Shake += 1;

                    SpawnHauntedSoulTimer--;
                    if (SpawnHauntedSoulTimer <= 0)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            Dust.NewDustPerfect(npc.Top, DustID.Shadowflame, (Vector2.UnitY * Main.rand.NextFloat(-6f, -1)).RotatedByRandom(0.45f), 25, default, Main.rand.NextFloat(1.1f, 1.35f));
                        }
                        for (int i = 0; i < 2 + Main.rand.Next(2); i++)
                        {
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Top, (Vector2.UnitY * Main.rand.NextFloat(-8f, -5f)).RotatedByRandom(0.55f), ModContent.ProjectileType<HauntedSoul>(), HauntedSoulDamage, 1f, HauntedSoulOwner, npc.whoAmI);
                        }
                        CameraSystem.Shake += 5;
                        HauntedStacks = 0;
                        SpawnHauntedSoulTimer = 60;
                    }
                }                  
            }

            if (ShardAmount == MAXVITRICSHARDS)
                HowLongShardHasBeenOnTarget++;

            if (HowLongShardHasBeenOnTarget > 240 && ShardAmount == MAXVITRICSHARDS)
            {
                ShardTimer = 120;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];

                    if (proj.active && proj.owner == ShardOwner && proj.type == ModContent.ProjectileType<SoilgunVitricCrystals>() && proj.ModProjectile is SoilgunVitricCrystals crystal && crystal.enemyID == npc.whoAmI)
                    {
                        crystal.exploding = true;
                        proj.timeLeft = 120;
                    }
                }
                HowLongShardHasBeenOnTarget = 0;
                npc.AddBuff(BuffID.OnFire, 120);
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (HauntedStacks > 0)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                npc.lifeRegen -= HauntedStacks;
                if (damage < 1)
                    damage = 1;
            }

            if (ShardAmount > 0)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                npc.lifeRegen -= 4 + ShardAmount * 2;
                if (damage < 1)
                    damage = 1;
            }
        }
    }

    class SoilgunHoldout : ModProjectile
    {

        public bool CanShoot = true;

        public int MaxCharge;

        public int DrawWhiteTimer = 30;

        public ref float CurrentCharge => ref Projectile.ai[0];

        public ref float SoilType => ref Projectile.ai[1];

        public Player owner => Main.player[Projectile.owner];

        public bool CanHold => owner.channel && !owner.CCed && !owner.noItems;

        public override string Texture => AssetDirectory.MiscItem + "Soilgun";

        public override bool? CanDamage() => false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soilgun");
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = 68;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            CurrentCharge++;

            Vector2 armPos = owner.RotatedRelativePoint(owner.MountedCenter, true);

            Vector2 barrelPos = armPos + Projectile.velocity * Projectile.width * 0.5f;
            barrelPos.Y -= 8;

            if (MaxCharge == 0f)
                MaxCharge = owner.HeldItem.useAnimation;

            if (CurrentCharge >= MaxCharge)
                DrawWhiteTimer--;

            if (!CanHold)
            {
                if (CurrentCharge >= MaxCharge)
                {
                    ShootSoils(barrelPos);
                }
                else
                    Projectile.Kill();
            }

            if (CurrentCharge == MaxCharge)
            {
                //maybe better sound here
                SoundEngine.PlaySound(SoundID.MaxMana, Projectile.position);
                for (int i = 0; i < 9; i++)
                {
                    Dust.NewDust(barrelPos, 4, 8, DustID.Dirt, 0f, 0f, default, default, Main.rand.NextFloat(0.9f, 1.2f));
                }
            }

            owner.ChangeDir(Projectile.direction);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            if (CanShoot)
            {
                Projectile.timeLeft = 2;
                Projectile.rotation = Utils.ToRotation(Projectile.velocity);
                owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

                if (Projectile.spriteDirection == -1)
                    Projectile.rotation += 3.1415927f;
            }

            Projectile.position = armPos - Projectile.Size * 0.5f;

            Projectile.spriteDirection = Projectile.direction;

            if (Main.myPlayer == Projectile.owner)
            {
                float interpolant = Utils.GetLerpValue(5f, 25f, Projectile.Distance(Main.MouseWorld), true);

                Vector2 oldVelocity = Projectile.velocity;

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, owner.DirectionTo(Main.MouseWorld), interpolant);
                if (Projectile.velocity != oldVelocity)
                {
                    Projectile.netSpam = 0;
                    Projectile.netUpdate = true;
                }
            }

            if (CurrentCharge < 2)
                return;

            int DustFrequency = (int)(15 - (Utils.Clamp(CurrentCharge / 5, 0, 12)));
            if (Main.rand.NextBool(Utils.Clamp(DustFrequency, 1, 15)))
            {
                Dust dust = Dust.NewDustDirect(barrelPos, 2, 8, ChooseChargeDust(), 0f, 0f);
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.noGravity = false;
                if (Main.rand.NextBool(5))
                    Dust.NewDustDirect(barrelPos, 2, 8, ModContent.DustType<Dusts.Sand>(), 0, 0, 125, default, 0.5f);
            }
        }

        public override void PostDraw(Color lightColor)
        {
            if (CurrentCharge >= MaxCharge && DrawWhiteTimer > 0)
            {
                Texture2D texture = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "Soilgun_White").Value;
                SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                float progress = 1 - (DrawWhiteTimer / 30f);
                Color drawColor = Color.Lerp(Color.White, Color.Transparent, progress);

                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, texture.Size() / 2, 1f, spriteEffects, 0);
            }
        }

        public void ShootSoils(Vector2 position)
        {
            if (Main.myPlayer != Projectile.owner || !CanShoot)
                return;

            Item heldItem = owner.HeldItem;

            int damage = Projectile.damage;

            float shootSpeed = heldItem.shootSpeed;

            float knockBack = owner.GetWeaponKnockback(heldItem, heldItem.knockBack);

            Vector2 shootVelocity = Utils.SafeNormalize(Projectile.velocity, Vector2.UnitY) * shootSpeed;

            for (int i = 0; i < 4 + Main.rand.Next(3); i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, (shootVelocity.RotatedByRandom(MathHelper.ToRadians(18))) * Main.rand.NextFloat(0.9f, 1.1f), ChooseSoilType(), damage, knockBack, owner.whoAmI, SoilType);
            }
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVelocity = shootVelocity.RotatedByRandom(MathHelper.ToRadians(8)) * Main.rand.NextFloat(0.25f, 0.45f);

                Dust dust = Dust.NewDustDirect(position, 2, 8, DustID.Dirt, dustVelocity.X, dustVelocity.Y);
                dust.scale = Main.rand.NextFloat(1.1f, 1.55f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 7; i++)
            {
                Vector2 sandVelocity = (shootVelocity * Main.rand.NextFloat(0.12f, 0.20f)).RotatedByRandom(MathHelper.ToRadians(9f));

                Dust.NewDustDirect(position, 2, 8, ModContent.DustType<Dusts.Sand>(), sandVelocity.X, sandVelocity.Y, 140, default, 0.5f);

                Dust.NewDustDirect(position, 2, 8, ModContent.DustType<SandNoGravity>(), sandVelocity.X, sandVelocity.Y, 135, default, 0.65f);

                Dust.NewDustDirect(position, 2, 8, ModContent.DustType<SandNoGravity>(), sandVelocity.X, sandVelocity.Y, 135, default, 0.55f);
            }

            CameraSystem.Shake += 2;

            owner.reuseDelay = 15;

            Item item = owner.GetModPlayer<SoilgunPlayer>().SoilItem;
            item.stack--;
            if (item.stack <= 1)
                item.TurnToAir();
            //need a better sfx maybe
            Projectile.timeLeft = 30;
            SoundEngine.PlaySound(SoundID.Item61, Projectile.position);
            CanShoot = false;
        }

        public int ChooseChargeDust()
        {
            int VitricSand = Mod.Find<ModItem>("VitricSandItem").Type;
            if (SoilType == VitricSand)
            {
                return ModContent.DustType<VitricSandDust>();
            }
            switch (SoilType)
            {
                case ItemID.SandBlock: return DustID.Sand;
                case ItemID.CrimsandBlock: return DustID.CrimsonPlants;
                case ItemID.EbonsandBlock: return DustID.Ebonwood;
                case ItemID.PearlsandBlock: return DustID.Pearlsand;
                case ItemID.DirtBlock: return DustID.Dirt;
                case ItemID.SiltBlock: return DustID.Silt;
                case ItemID.SlushBlock: return DustID.Slush;
                case ItemID.MudBlock: return DustID.Mud;
            }
            return 0;
        }
        public int ChooseSoilType()
        {
            int VitricSand = Mod.Find<ModItem>("VitricSandItem").Type;
            if (SoilType == VitricSand)
            {
                return ModContent.ProjectileType<SoilgunVitricSandSoil>();
            }
            switch (SoilType)
            {
                case ItemID.SandBlock: return ModContent.ProjectileType<SoilgunSandSoil>();
                case ItemID.CrimsandBlock: return ModContent.ProjectileType<SoilgunCrimsandSoil>();
                case ItemID.EbonsandBlock: return ModContent.ProjectileType<SoilgunEbonsandSoil>();
                case ItemID.PearlsandBlock: return ModContent.ProjectileType<SoilgunPearlsandSoil>();
                case ItemID.DirtBlock: return ModContent.ProjectileType<SoilgunDirtSoil>();
                case ItemID.SiltBlock: return ModContent.ProjectileType<SoilgunSiltSoil>();
                case ItemID.SlushBlock: return ModContent.ProjectileType<SoilgunSlushSoil>();
                case ItemID.MudBlock: return ModContent.ProjectileType<SoilgunMudSoil>();
            }
            return 0;
        }
    }
}
