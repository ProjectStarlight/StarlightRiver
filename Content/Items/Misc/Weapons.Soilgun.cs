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

namespace StarlightRiver.Content.Items.Misc
{   
    // This entire thing needs balancing, also this code is probably bad and convulted but

    //some visuals here and there could be polished idk

    //god i spent too much time on this
    class Soilgun : ModItem
    { 
        public List<int> ValidSoils => new List<int>() { ItemID.SandBlock, ItemID.EbonsandBlock, ItemID.PearlsandBlock, ItemID.CrimsandBlock, ItemID.DirtBlock, ItemID.SiltBlock, ItemID.SlushBlock, Mod.Find<ModItem>("VitricSandItem").Type};
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
        //TODO make this look not bad
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
        public List<int> ValidSoils => new List<int>() { ItemID.SandBlock, ItemID.EbonsandBlock, ItemID.PearlsandBlock, ItemID.CrimsandBlock, ItemID.DirtBlock, ItemID.SiltBlock, ItemID.SlushBlock, Mod.Find<ModItem>("VitricSandItem").Type };
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            //there could probably be a better color for the tooltips here, also keywords should be used for some, like Haunted
            if (!Main.LocalPlayer.HasItem(ModContent.ItemType<Soilgun>()))
                return;

            if (ValidSoils.Contains(item.type))
            {
                TooltipLine tooltip = new TooltipLine(Mod, "SoilgunAmmoTooltip", "This item can be used as ammo for the Soilgun");
                tooltips.Add(tooltip);
                tooltip.OverrideColor = new Color(114, 81, 56);
                if (item.type == Mod.Find<ModItem>("VitricSandItem").Type)
                {
                    TooltipLine infoTooltip = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out volatile blocks of sand that stick to enemies and explode");
                    tooltips.Add(infoTooltip);
                    infoTooltip.OverrideColor = new Color(114, 81, 56);
                    return;
                }
                switch (item.type)
                {
                    case ItemID.SandBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of sand that split into many grains of sand upon death"); break;
                    case ItemID.CrimsandBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of Crimsand that steal life from hit enemies"); break;
                    case ItemID.EbonsandBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun it apply stacks of Haunted to enemies\nHaunted does 1 DPS per haunted stack, maxing at 20\nWhen an enemy has 20 Haunted stacks, they will cause haunted apparitions to exorcise from them, getting rid of all their Haunted stacks"); break;
                    case ItemID.PearlsandBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of Pearlsand that home in on enemies"); break;
                    case ItemID.DirtBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of dirt"); break;
                    case ItemID.SiltBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of silt, that spawn coins upon hitting enemies"); break;
                    case ItemID.SlushBlock: infoTooltip2 = new TooltipLine(Mod, "AmmoInfoTooltip", "When used with the Soilgun, it will fire out blocks of slush that cause hit enemies to have icicles impale them\nHitting and enemy with more than 15 icicles causes all icicles to shatter, causing large amounts of damage"); break;
                }
                tooltips.Add(infoTooltip2);
                infoTooltip2.OverrideColor = new Color(114, 81, 56);
            }
        }
    }

    class SoilgunGlobalNPC : GlobalNPC
    {
        //its actually ice but whateva
        public const int MaxHauntedStacks = 20;

        public int GlassAMT;

        public int GlassPlayerID;

        public int HauntedStacks;

        public int HauntedTimer;

        public int HauntedSoulDamage;

        public int HauntedSoulOwner;

        public int SpawnHauntedSoulTimer = 60;
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
            HauntedStacks = Utils.Clamp(HauntedStacks, 0, MaxHauntedStacks);
        }
        public override void AI(NPC npc)
        {
            if (GlassAMT > 0)
            {
                if (Main.rand.NextBool(20 - GlassAMT))
                    Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Ice, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1.1f));
            }
            if (HauntedTimer > 0)
            {
                float Rand = MathHelper.Clamp(20 - HauntedStacks, 1f, 20f);
                if (Main.rand.NextBool((int)Rand))
                    Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Shadowflame, 0f, 0f, 25, default, Main.rand.NextFloat(0.75f, 1.2f));

                if (HauntedStacks == MaxHauntedStacks)
                {
                    if (SpawnHauntedSoulTimer == 60)
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/ShadowSpawn").WithPitchOffset(Main.rand.NextFloat(-0.1f, 0.1f)));

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
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (HauntedStacks > 0)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= HauntedStacks;
                if (damage < 1)
                {
                    damage = 1;
                }
            }
        }
    }

    class SoilgunHoldout : ModProjectile
    {
        public bool FullyChargedEffects = false;

        public bool CanShoot = false;
        public Player owner => Main.player[Projectile.owner];
        public int MaxCharge;
        public ref float CurrentCharge => ref Projectile.ai[0];
        public ref float SoilType => ref Projectile.ai[1];
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

            if (MaxCharge == 0f)
            {
                MaxCharge = owner.HeldItem.useAnimation;
            }

            if (!CanHold)
            {
                if (CurrentCharge >= MaxCharge)
                    ShootThings(barrelPos);

                Projectile.Kill();
            }
            if (CurrentCharge == MaxCharge)
            {
                //maybe better sound here
                SoundEngine.PlaySound(SoundID.MaxMana, Projectile.position);
                CombatText.NewText(owner.getRect(), new Color(151, 107, 75), "Loaded!", true, true);
                for (int i = 0; i < 9; i++)
                {
                    Dust.NewDust(barrelPos, 4, 8, DustID.Dirt, 0f, 0f, default, default, Main.rand.NextFloat(0.9f, 1.2f));
                }
            }

            owner.ChangeDir(Projectile.direction);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);
            Projectile.timeLeft = 2;
            Projectile.position = armPos - Projectile.Size * 0.5f;
            Projectile.rotation = Utils.ToRotation(Projectile.velocity);

            if (Projectile.spriteDirection == -1)
                Projectile.rotation += 3.1415927f;

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
                Dust dust = Dust.NewDustDirect(barrelPos, 2, 8, DustID.Dirt, 0f, 0f);
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.noGravity = false;
                if (Main.rand.NextBool(5))
                    Dust.NewDustDirect(barrelPos, 2, 8, ModContent.DustType<Dusts.Sand>(), 0, 0, 125, default, 0.5f);
            }
        }

        public void ShootThings(Vector2 position)
        {
            if (Main.myPlayer != Projectile.owner)
                return;
            Item heldItem = owner.HeldItem;
            int damage = Projectile.damage;
            float shootSpeed = heldItem.shootSpeed;
            float knockBack = owner.GetWeaponKnockback(heldItem, heldItem.knockBack);
            Vector2 shootVelocity = Utils.SafeNormalize(Projectile.velocity, Vector2.UnitY) * shootSpeed;
            for (int i = 0; i < 4 + Main.rand.Next(3); i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, (shootVelocity.RotatedByRandom(MathHelper.ToRadians(18))) * Main.rand.NextFloat(0.9f, 1.1f), ModContent.ProjectileType<SoilGunSoilProjs>(), damage, knockBack, owner.whoAmI, SoilType);
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
            {
                item.TurnToAir();
            }
            //need a better sfx
            SoundEngine.PlaySound(SoundID.Item61, Projectile.position);
        }
    }

    class SandNoGravity : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "Sand";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.scale *= 6;
            dust.frame = new Rectangle(0, 0, 10, 10);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {
            dust.color = Lighting.GetColor((int)(dust.position.X / 16), (int)(dust.position.Y / 16)).MultiplyRGB(Color.White) * 0.2f * (dust.alpha / 255f);
            dust.position += dust.velocity;
            dust.scale *= 0.9745f;
            dust.velocity *= 0.97f;
            dust.rotation += 0.1f;

            if (dust.scale <= 0.2f)
                dust.active = false;
            return false;
        }
    }

    class SoilGunSoilProjs : ModProjectile, IDrawPrimitive
    {
        //this class is a projectile that is all soil projectiles I guess idk

        //sticky code from breacher i know copying bad but
        public override string Texture => AssetDirectory.MiscItem + "Soilgun";

        private List<Vector2> cache;
        private Trail trail;

        private int EnemyType;

        private bool stuck;

        private bool foundTarget;

        private Vector2 offset = Vector2.Zero;
        public float AmmoType => Projectile.ai[0];

        public ref float Time => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soil");
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.Size = new Vector2(12);
            Projectile.friendly = true;
            Projectile.timeLeft = 240;
        }

        public override bool PreAI()
        {
            int VitricSand = Mod.Find<ModItem>("VitricSandItem").Type;
            if (AmmoType == VitricSand)
            {
                if (stuck)
                {
                    NPC target = Main.npc[EnemyType];
                    Projectile.position = target.position + offset;
                }
                Projectile.penetrate = -1;
            }
            return true;
        }

        public override void AI()
        {
            Time++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.timeLeft < 230 && !foundTarget)
            {
                Projectile.velocity.Y += 0.96f;
                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;
            }
            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }
            #region soil-specific ai
            int VitricSand = Mod.Find<ModItem>("VitricSandItem").Type;
            if (AmmoType == VitricSand)
            {
                if (stuck)
                {
                    if (!Main.npc[EnemyType].active)
                        Projectile.Kill();
                    int decreasing = 0;
                    for (int i = 0; i < Time / 2; i += 10)
                    {
                        decreasing += 3;
                    }
                    if (Projectile.timeLeft < 120 && Main.rand.NextBool(5))
                    {
                        float angle = Main.rand.NextFloat(6.28f);
                        Dust dust = Dust.NewDustPerfect((Projectile.Center - new Vector2(15, 15)) - (angle.ToRotationVector2() * (60 - decreasing)), ModContent.DustType<Dusts.NeedlerDustFive>());
                        dust.scale = 0.05f;
                        dust.velocity = angle.ToRotationVector2() * (Time < 60? 0.08f : 0.15f);
                    }
                    if (Projectile.timeLeft == 60)
                    {
                        SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireCast"), Projectile.position);
                    }
                }
                else if (Main.rand.NextBool(5) && !stuck)
                {
                    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.GlassGravity>()).scale = Main.rand.NextFloat(0.8f, 1.1f);
                }
            }
            switch (AmmoType)
            {
                case ItemID.SandBlock:
                    if (Main.rand.NextBool(8))
                    {
                        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0f, 0f, 35, default, Main.rand.NextFloat(0.8f, 1.2f));
                        dust.noGravity = true;
                    }                   
                    break;
                case ItemID.CrimsandBlock:
                    if (Main.rand.NextBool(10))
                    {
                        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CrimsonPlants, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.25f));
                        dust.noGravity = true;
                    }
                    break;
                case ItemID.EbonsandBlock:
                    if (Main.rand.NextBool(8))
                    {
                        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ebonwood, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1.15f));
                        dust.noGravity = true;
                        if (Main.rand.NextBool(2))
                        {
                            Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.2f));
                            dust2.noGravity = true;
                        }
                    }
                    break;
                case ItemID.PearlsandBlock:
                    Vector2 npcCenter = Projectile.Center;
                    NPC npc = Projectile.FindTargetWithinRange(1500f);

                    if (npc != null && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1) && !npc.dontTakeDamage && !npc.immortal)
                    {
                        npcCenter = npc.Center;
                        foundTarget = true;
                    }
                    if (foundTarget)
                    {
                        float speed = Main.player[Projectile.owner].HeldItem.shootSpeed;
                        Vector2 velo = Utils.SafeNormalize(npcCenter - Projectile.Center, Vector2.UnitY);
                        Projectile.velocity = (Projectile.velocity * 20f + velo * speed) / (21f);
                    }

                    if (Main.rand.NextBool(5))
                    {
                        Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Pearlsand, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1.1f)).noGravity = true;
                        if (Main.rand.NextBool(2))
                            Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.MoonstoneShimmer>(), Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(0.1f, 0.2f), 25, new Color(0.3f, 0.2f, 0.3f, 0f), Main.rand.NextFloat(0.2f, 0.3f));
                    }


                    break;
                case ItemID.DirtBlock:
                    if (Main.rand.NextBool(10))
                    {
                        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.25f));
                        dust.noGravity = true;
                        if (Main.rand.NextBool(3))
                        {
                            Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<SandNoGravity>(), 0f, 0f, 120, default, Main.rand.NextFloat(0.7f, 1.1f));
                        }
                    }
                    break;
                case ItemID.SiltBlock:
                    break;
                case ItemID.SlushBlock:
                    break;
            }
            #endregion soil-specific ai
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //this predraw code is kinda bad examplemod boilerplate but it works
            Main.instance.LoadItem((int)AmmoType);
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Item[(int)AmmoType].Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);


            Vector2 origin = sourceRectangle.Size() / 2f;

            float offsetX = 0f;
            origin.X = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX);

            Color drawColor = Projectile.GetAlpha(lightColor);

            Main.EntitySpriteDraw(texture,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle, AmmoType == ItemID.PearlsandBlock ? Color.White : drawColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            int VitricSand = Mod.Find<ModItem>("VitricSandItem").Type;
            if (AmmoType == VitricSand)
            {
                float progress = 1 - (Projectile.timeLeft / 240f);
                Color explodingColor = Color.Lerp(Color.Transparent, Color.Lerp(Color.Orange, Color.Red, progress) * 0.5f, progress);
                Texture2D WhiteVitricSandTex = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "SoilgunVitricSandWhite").Value;
                Main.EntitySpriteDraw(WhiteVitricSandTex,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                sourceRectangle, explodingColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            }

            return false;
        }

        public override void Kill(int timeLeft)
        {
            int VitricSand = Mod.Find<ModItem>("VitricSandItem").Type;
            if (AmmoType == VitricSand)
            {
                SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireHit"), Projectile.Center);
                if (!stuck)
                {
                    CameraSystem.Shake += 2;
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SoilgunExplosion>(), 5 + (int)(Projectile.damage * 0.25f), 2.5f, Projectile.owner, 55);
                        for (int i = 0; i < 2; i++)
                        {
                            Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 3), ModContent.ProjectileType<NeedlerEmber>(), 0, 0, Projectile.owner);
                        }
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 velocity = Main.rand.NextVector2Circular(7, 7);
                        Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<Dusts.NeedlerDust>(), velocity.X, velocity.Y, 75 + Main.rand.Next(65), default, Main.rand.NextFloat(1.1f, 1.5f));
                        dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

                        Vector2 velocity2 = Main.rand.NextVector2Circular(7, 7);
                        Dust dust2 = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<Dusts.NeedlerDustTwo>(), velocity2.X, velocity2.Y, 45 + Main.rand.Next(85), default, Main.rand.NextFloat(1.1f, 1.5f));
                        dust2.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

                        Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), ModContent.DustType<Dusts.NeedlerDustFour>()).scale = 0.75f;
                    }
                }
                else
                {
                    CameraSystem.Shake += 4;
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SoilgunExplosion>(), 5 + (int)(Projectile.damage * 0.5f), 2.5f, Projectile.owner, 95);
                        for (int i = 0; i < 6; i++)
                        {
                            Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 3), ModContent.ProjectileType<NeedlerEmber>(), 0, 0, Projectile.owner);
                        }
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 velocity = Main.rand.NextVector2Circular(9, 9);
                        Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<Dusts.NeedlerDust>(), velocity.X, velocity.Y, 70 + Main.rand.Next(60), default, Main.rand.NextFloat(1.3f, 1.7f));
                        dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

                        Vector2 velocity2 = Main.rand.NextVector2Circular(9, 9);
                        Dust dust2 = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<Dusts.NeedlerDustTwo>(), velocity2.X, velocity2.Y, 40 + Main.rand.Next(80), default, Main.rand.NextFloat(1.3f, 1.7f));
                        dust2.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

                        Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<Dusts.NeedlerDustFour>()).scale = 0.85f;
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, stuck ? ModContent.DustType<MoltenGlassGravity>() : ModContent.DustType<Dusts.GlassGravity>(), 0f, 0f).scale = Main.rand.NextFloat(0.7f, 1.1f);
                }
                return;
            }

            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

            switch (AmmoType)
            {
                case ItemID.SandBlock:
                    for (int i = 0; i < 12; i++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.1f));

                        Dust.NewDustPerfect(Projectile.Center, DustID.Sand, (Vector2.UnitY * Main.rand.NextFloat(-3, -1)).RotatedByRandom(0.35f), 35, default, Main.rand.NextFloat(0.8f, 1.1f));
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        if (Main.myPlayer == Projectile.owner)
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.UnitY * Main.rand.NextFloat(-6.5f, -1)).RotatedByRandom(0.45f), ModContent.ProjectileType<SoilgunSandGrain>(), (int)(Projectile.damage * 0.33f), 0f, Projectile.owner);
                    }
                    break;
                case ItemID.CrimsandBlock:
                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CrimsonPlants, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1f));
                    }
                    break;
                case ItemID.EbonsandBlock:
                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ebonwood, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1f));
                    }
                    break;
                case ItemID.PearlsandBlock:
                    for (int i = 0; i < 10; i++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Pearlsand, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1f));
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.MoonstoneShimmer>(), Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(0.3f, 0.4f), 25, new Color(0.3f, 0.2f, 0.3f, 0f), Main.rand.NextFloat(0.3f, 0.4f));
                    }
                    break;
                case ItemID.DirtBlock:
                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, 0f, 0f, 15, default, Main.rand.NextFloat(0.8f, 1f));
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.Sand>(), 0f, 0f, 140, default, Main.rand.NextFloat(0.8f, 1.1f));
                    }
                    break;
                case ItemID.SiltBlock:
                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Silt, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1f));
                    }
                    break;
                case ItemID.SlushBlock:
                    for (int i = 0; i < 15; i++)
                    {
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, 0f, 0f, 15, default, Main.rand.NextFloat(0.8f, 1f));
                    }
                    SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
                    break;
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            SoilgunGlobalNPC globalNPC = target.GetGlobalNPC<SoilgunGlobalNPC>();
            int VitricSand = Mod.Find<ModItem>("VitricSandItem").Type;
            if (AmmoType == VitricSand)
            {
                if (!stuck && target.life > 0)
                {
                    stuck = true;
                    Projectile.friendly = false;
                    Projectile.tileCollide = false;
                    EnemyType = target.whoAmI;
                    offset = Projectile.position - target.position;
                    offset -= Projectile.velocity;
                    Projectile.netUpdate = true;
                }
                return;
            }
            switch (AmmoType)
            {
                case ItemID.SandBlock:
                    break;
                case ItemID.CrimsandBlock:
                    if (Main.rand.NextBool(3) && Main.player[Projectile.owner].statLife < Main.player[Projectile.owner].statLifeMax2)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.LifeDrain, (Projectile.DirectionTo(Main.player[Projectile.owner].Center) * Main.rand.NextFloat(8f, 12f)).RotatedByRandom(MathHelper.ToRadians(5f)), 50, default, Main.rand.NextFloat(0.75f, 1f));
                            dust.noGravity = true;
                        }
                        if (Main.myPlayer == Projectile.owner && !target.SpawnedFromStatue && target.lifeMax > 5)
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.DirectionTo(Main.player[Projectile.owner].Center), ModContent.ProjectileType<SoilgunLifeSteal>(), 0, 0f, Projectile.owner, 2 + (int)(damage * 0.1f));
                    }
                    break;
                case ItemID.EbonsandBlock:
                    globalNPC.HauntedSoulDamage = damage * 3;
                    globalNPC.HauntedStacks++;
                    globalNPC.HauntedTimer = 420;
                    globalNPC.HauntedSoulOwner = Projectile.owner;
                    break;
                case ItemID.PearlsandBlock:
                    break;
                case ItemID.DirtBlock:
                    break;
                case ItemID.SiltBlock:
                    for (int i = 0; i < 12; i++)
                    {
                        Dust.NewDustPerfect(Projectile.Center, Main.rand.Next(new int[] {DustID.CopperCoin, DustID.SilverCoin, DustID.GoldCoin, DustID.PlatinumCoin}), (Vector2.UnitY * Main.rand.NextFloat(-4, -1)).RotatedByRandom(0.25f), 35, default, Main.rand.NextFloat(1f, 1.3f));
                    }
                    for (int i = 0; i < 1 + Main.rand.Next(2); i++)
                    {
                        if (Main.myPlayer == Projectile.owner)
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.UnitY * Main.rand.NextFloat(-9f, -1)).RotatedByRandom(0.35f), ModContent.ProjectileType<SoilgunCoinsProjectile>(), (int)(Projectile.damage * 0.66f), 1f, Projectile.owner);
                    }
                    break;
                case ItemID.SlushBlock:
                    globalNPC.GlassPlayerID = Projectile.owner;
                    globalNPC.GlassAMT++;
                    if (Main.myPlayer == Projectile.owner)
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SoilgunIcicleProj>(), (int)(Projectile.damage * 0.65f), 0f, Projectile.owner, target.whoAmI);
                    if (globalNPC.GlassAMT > 15)
                    {
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            Projectile proj = Main.projectile[i];
                            if (proj.type == ModContent.ProjectileType<SoilgunIcicleProj>() && proj.active && proj.ai[0] == target.whoAmI)
                            {
                                proj.ai[1] = 1f;
                                proj.Kill();
                            }    
                        }
                        globalNPC.GlassAMT = 0;
                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath.WithVolumeScale(3f), Projectile.position);
                        CameraSystem.Shake += 5;
                        for (int i = 0; i < 5; i++)
                        {
                            Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5, 5), ModContent.DustType<Dusts.Mist>(), Main.rand.NextVector2Circular(1, 1), 0, Color.LightBlue, Main.rand.NextFloat(0.8f, 1.1f));
                        }
                    }
                    break;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(stuck);
            writer.WritePackedVector2(offset);
            writer.Write(EnemyType);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            stuck = reader.ReadBoolean();
            offset = reader.ReadPackedVector2();
            EnemyType = reader.ReadInt32();
        }
        public Color TrailColor(int SoilType)
        {
            //switch statement didnt like mod.find
            int VitricSand = Mod.Find<ModItem>("VitricSandItem").Type;
            if (AmmoType == VitricSand)
            {
                float progress = 1 - (Projectile.timeLeft / 240f);
                return Color.Lerp(new Color(86, 57, 47), Color.Lerp(Color.Orange, Color.Red, progress), progress);
            }
            switch (SoilType) 
            {
                case ItemID.SandBlock:
                    return new Color(58, 49, 18);
                case ItemID.CrimsandBlock:
                    return new Color(39, 17, 14);
                case ItemID.EbonsandBlock:
                    return new Color(26, 18, 31);
                case ItemID.PearlsandBlock:
                    return new Color(87, 77, 106);
                case ItemID.DirtBlock:
                    return new Color(30, 19, 12);
                case ItemID.SiltBlock:
                    return new Color(22, 24, 32);
                case ItemID.SlushBlock:
                    return new Color(27, 40, 51);
            }
            return Color.White;
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 13; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 13)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(4), factor => 7, factor =>
            {
                return TrailColor((int)AmmoType) * 0.8f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.position + Projectile.velocity;
        }
        public void DrawPrimitives()
        {
            if (stuck)
                return;
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "SoilgunMuddyTrail").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            trail?.Render(effect);
        }
    }

    //kinda just turned magmite gore into projectile cause I think it would be good for grains of sand
    class SoilgunSandGrain : ModProjectile
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sand Grain");
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.width = Projectile.height = 10;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 240;
            Projectile.penetrate = -1;
        }

        public override Color? GetAlpha(Color lightColor) => lightColor * (Projectile.scale < 0.5f ? Projectile.scale * 2 : 1);
        public override void AI()
        {
            if (Projectile.wet)
                Projectile.Kill();

            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.scale *= 0.99f;

            if (Projectile.scale < 0.1f)
                Projectile.Kill();

            if (Projectile.velocity.Y == 0)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = 0;
                Projectile.frame = 1;
            }

            if (Projectile.frame == 0)
            {
                Projectile.velocity.Y += 0.5f;
                if (Projectile.velocity.Y > 16f)
                {
                    Projectile.velocity.Y = 16f;
                }
            }
            if (Projectile.frame == 1)
            {
                Projectile.position.Y += 0.02f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.position += oldVelocity;
            Projectile.velocity *= 0f;
            return false;
        }
    }

    class SoilgunLifeSteal : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;
        public ref float LifeStealAMT => ref Projectile.ai[0];
        public override string Texture => AssetDirectory.Invisible;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lifesteal Orb");
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 4;
            Projectile.timeLeft = 120;

            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            float homingSpeed = player.lifeMagnet ? 20f : 16f;
            Vector2 playerVector = player.Center - Projectile.Center;
            if (Projectile.Hitbox.Intersects(player.Hitbox))
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    player.HealEffect((int)LifeStealAMT, false);
                    player.statLife += (int)LifeStealAMT;
                    if (player.statLife > player.statLifeMax2)
                    {
                        player.statLife = player.statLifeMax2;
                    }
                    NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, Projectile.owner, (float)LifeStealAMT, 0f, 0f, 0, 0, 0);
                }
                Projectile.Kill();
            }
            Vector2 velo = Utils.SafeNormalize(playerVector, Vector2.UnitY);
            Projectile.velocity = (Projectile.velocity * 20f + velo * homingSpeed) / 21f;

            if (!player.active)
            {
                Projectile.Kill();
            }

            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.LifeDrain, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.1f));
                dust.noGravity = true;
            }

            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 13; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 13)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(4), factor => 4, factor =>
            {
                return Color.Red * 0.8f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.position + Projectile.velocity;
        }
        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/LightningTrail").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/ShadowTrail").Value);

            trail?.Render(effect);
        }
    }
    internal class SoilgunExplosion : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        private float Progress => 1 - (Projectile.timeLeft / 5f);

        private float Radius => Projectile.ai[0] * (float)Math.Sqrt(Math.Sqrt(Progress));

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosion");
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
            line.Normalize();
            line *= Radius;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
            {
                return true;
            }
            return false;
        }
    }

    internal class SoilgunIcicleProj : ModProjectile
    {
        public Vector2 pos;
        public ref float TargetWhoAmI => ref Projectile.ai[0];
        public override string Texture => AssetDirectory.MiscItem + "SoilgunIcicles";
        public override bool? CanDamage() => false;

        public override void Load()
        {
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore1");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore2");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore3");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore4");
        }
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            DisplayName.SetDefault("Icicle");
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.frame = Main.rand.Next(4);
            Projectile.height = Projectile.width = 12;
        }

        public override void AI()
        {
            NPC npc = Main.npc[(int)TargetWhoAmI];
            if (Projectile.localAI[0] == 0f)
            {
                pos = new Vector2(Main.rand.Next(-npc.width / 2, npc.width / 2), Main.rand.Next(-npc.height / 2, npc.height / 2));
                Projectile.localAI[0] = 1f;
            }
            Projectile.Center = npc.Center + pos;
            if (Projectile.timeLeft < 50)
                Projectile.alpha += 5;

            if (!npc.active)
            {
                Projectile.ai[1] = 1f;
                Projectile.Kill();
            }
        }

        public override void Kill(int timeLeft)
        {
            NPC npc = Main.npc[(int)TargetWhoAmI];
            SoilgunGlobalNPC globalNPC = npc.GetGlobalNPC<SoilgunGlobalNPC>();
            if (Projectile.ai[1] == 1f)
            {
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1.1f));
                }
                for (int i = 0; i < 2; i++)
                {
                    int GoreType = Mod.Find<ModGore>("SoilgunIcicles_Gore" + Main.rand.Next(1, 5).ToString()).Type;
                    Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(1, 1), GoreType).timeLeft = 60;
                }
                if (Main.myPlayer == Projectile.owner)
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SoilgunExplosion>(), (int)(Projectile.damage * 0.33f), 0f, Projectile.owner, 35);
            }
            if (globalNPC.GlassAMT > 0)
                globalNPC.GlassAMT--;
        }
    }

    internal class SoilgunCoinsProjectile : ModProjectile
    {
        public bool foundTarget;

        public bool HasCharged;
        public override bool? CanDamage() => Projectile.timeLeft < 60;
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Coin");
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8; 
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 12;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.frame = Main.rand.Next(4);
            Projectile.timeLeft = 120;
            if (Projectile.frame < 2)
                Projectile.penetrate = 2;
            else
                Projectile.penetrate = 1;
        }

        public override void AI()
        {
            NPC npc = Projectile.FindTargetWithinRange(450f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (npc != null && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1) && !npc.dontTakeDamage && !npc.immortal && !foundTarget && Projectile.timeLeft < 60)
            {
                foundTarget = true;
                if (!HasCharged)
                {
                    const int Repeats = 35;
                    for (int i = 0; i < Repeats; ++i)
                    {
                        float angle2 = 6.2831855f * (float)i / (float)Repeats;
                        Dust dust3 = Dust.NewDustPerfect(Projectile.Center, ChooseDustType(), null, 0, default(Color), 1.1f);
                        dust3.velocity = Utils.ToRotationVector2(angle2) * 2.5f;
                        dust3.noGravity = true;
                    }
                    Projectile.velocity = Vector2.Normalize(npc.Center - Projectile.Center) * 18f;
                    HasCharged = true;
                }
            }
            if (!foundTarget)
            {
                Projectile.velocity.Y += 0.05f;
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Coins, Projectile.position);
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ChooseDustType(), 0f, 0f).scale = Main.rand.NextFloat(0.8f, 1.1f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            int FrameHeight = texture.Height / Main.projFrames[Projectile.type];

            Rectangle frameRect = new Rectangle(0, FrameHeight * Projectile.frame, texture.Width, FrameHeight);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, frameRect, color, Projectile.oldRot[k], drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            switch (Projectile.frame)
            {
                case 0: damage = (int)(damage * 1.3f); break; // platinum coin do more dmg then gold, gold more than silver, etc
                case 1: damage = (int)(damage * 1.2f); break;
                case 2: damage = (int)(damage * 1.1f); break;
            }
        }

        public int ChooseDustType()
        {
            switch (Projectile.frame)
            {
                case 0: return DustID.PlatinumCoin;
                case 1: return DustID.GoldCoin;
                case 2: return DustID.SilverCoin;
                case 3: return DustID.CopperCoin;
            }
            return 0;
        }
    }

    public class MoltenGlassGravity : ModDust
    {
        //idk why but the sprite exists with no actual dust so
        public override string Texture => AssetDirectory.Dust + "GlassMolten";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity.Y += 0.15f;
            dust.rotation += 0.1f;
            dust.scale *= 0.98f;
            if (dust.scale <= 0.2)
                dust.active = false;
            return false;
        }
    }
    
    //maybe a sprite for this
    public class HauntedSoul : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        public bool foundTarget;
        public override string Texture => AssetDirectory.Invisible;

        public ref float NPCWhoSpawnedThis => ref Projectile.ai[0];

        public override bool? CanDamage() => Projectile.timeLeft < 330;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Soul");
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 360;
            Projectile.friendly = true;
            Projectile.width = Projectile.height = 16;
        }

        public override void AI()
        {
            Vector2 npcCenter = Projectile.Center;
            NPC npc = Projectile.FindTargetWithinRange(2000f);

            if (npc != null && !npc.dontTakeDamage && !npc.immortal && Projectile.timeLeft < 330)
            {
                npcCenter = npc.Center;
                foundTarget = true;
            }
            if (foundTarget)
            {
                float speed = 10f;
                Vector2 velo = Utils.SafeNormalize(npcCenter - Projectile.Center, Vector2.UnitY);
                Projectile.velocity = (Projectile.velocity * 20f + velo * speed) / (21f);
            }

            if (Main.rand.NextBool(3))
                Dust.NewDustDirect(Projectile.position, 1, 1, DustID.Shadowflame, 0f, 0f, 25, default, 0.9f);

            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/ShadowDeath").WithPitchOffset(Main.rand.NextFloat(-0.1f, 0.1f)));

            const int Repeats = 45;
            for (int i = 0; i < Repeats; ++i)
            {
                float angle2 = 6.2831855f * (float)i / (float)Repeats;
                Dust dust3 = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, null, 0, default(Color), 1.1f);
                dust3.velocity = Utils.ToRotationVector2(angle2) * 2.25f;
                dust3.noGravity = true;
            }
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 13; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 13)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(4), factor => 16, factor =>
            {
                return new Color(52, 21, 141) * 0.8f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.position + Projectile.velocity;
        }
        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/ShadowTrail").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            trail?.Render(effect);
        }
    }
}
