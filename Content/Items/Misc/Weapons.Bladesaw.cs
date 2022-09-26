using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent;
using StarlightRiver.Core.Systems;
using Terraria.GameContent.Achievements;

namespace StarlightRiver.Content.Items.Misc
{
    public class Bladesaw : ModItem
    {
        private int swingDirection = 1;
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ProjectileType<BladesawSwungBlade>()] <= 0;

        public override bool AltFunctionUse(Player player) => true;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bladesaw");
            Tooltip.SetDefault("Slashes with a combo heavy swings\nSlows down on hit, shredding anything unlucky enough to be caught in the way\n" +
                "Striking enemies inflicts them with stacks of Shredded, and heats up the blade... er saw\nThe bladesaw does more damage depending on how heated it is, to a maximum of 18%\n" +
                "<right> to cause the blade to slice through trees\n'What are you, some kind of Chainsaw Man?'");

            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 11;
            Item.crit = 6;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 50;
            Item.useAnimation = 50;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4.5f;
            Item.shootSpeed = 5f;
            Item.shoot = ProjectileType<BladesawSwungBlade>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item23;

            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingDirection *= -1;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, swingDirection, player.altFunctionUse == 2 ? 1f : 0f);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.CopperBar, 8).
                AddRecipeGroup(RecipeGroupID.IronBar, 12).
                AddTile(TileID.Anvils).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.TinBar, 8).
                AddRecipeGroup(RecipeGroupID.IronBar, 12).
                AddTile(TileID.Anvils).
                Register();
        }
    }

    class ShreddedNPC : GlobalNPC
    {
        public const int MAXSHREDDEDSTACKS = 5;
        public int ShreddedStacks;
        public int ShreddedTimer;

        public override bool InstancePerEntity => true;
        public override void ResetEffects(NPC npc)
        {
            ShreddedStacks = Utils.Clamp(ShreddedStacks, 0, MAXSHREDDEDSTACKS);
            if (--ShreddedTimer == 1)
                ShreddedStacks = 0;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            if (ShreddedStacks > 0)
                damage = (int)Main.CalculateDamageNPCsTake(damage, npc.defense - (2 *  ShreddedStacks));
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (ShreddedStacks > 0)
                damage = (int)Main.CalculateDamageNPCsTake(damage, npc.defense - (2 * ShreddedStacks));
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (ShreddedStacks <= 0)
                return;

            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            npc.lifeRegen -= 2 * ShreddedStacks;

            if (damage < 1)
                damage = 1;
        }

        public override void AI(NPC npc)
        {
            if (ShreddedStacks > 0 && Main.rand.NextBool(7 - ShreddedStacks))
            {
                Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood).scale = 1.4f;
                if (Main.rand.NextBool())
                    Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<Dusts.GraveBlood>()).scale = 1.2f;
            }
        }
    }

    class BladesawSwungBlade : ModProjectile
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        private List<float> oldRotation = new List<float>();

        private Vector2 direction;

        private bool initialized;

        private float maxTimeLeft;

        private int oldTimeleft;

        private int hitAmount;

        public bool justHit = false;

        public int pauseTimer;

        public float SwingDirection => Projectile.ai[0] * Math.Sign(direction.X);

        public Player Owner => Main.player[Projectile.owner];

        public override bool? CanHitNPC(NPC target) => !target.friendly && hitAmount <= 6 && 1 - (Projectile.timeLeft / maxTimeLeft) > 0.2f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bladesaw");
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.Size = new Vector2(60);
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 7;
        }

        public override void AI()
        {
            if (--pauseTimer > 0)
                Projectile.timeLeft = oldTimeleft;

            if (!initialized)
            {
                initialized = true;
                float attackSpeed = Owner.GetTotalAttackSpeed(DamageClass.Melee) - 1f;
                Projectile.timeLeft = (int)(Owner.HeldItem.useAnimation * (1f - attackSpeed));
                maxTimeLeft = Projectile.timeLeft;
                direction = Projectile.velocity;
                direction.Normalize();
                Projectile.rotation = Utils.ToRotation(direction);
                Projectile.netUpdate = true;
            }

            Projectile.Center = Owner.Center + direction * 45;

            if (pauseTimer <= 0)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Lerp(2f * SwingDirection, -2f * SwingDirection, EaseBuilder.EaseCircularInOut.Ease(1 - (Projectile.timeLeft / maxTimeLeft)));

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            if (pauseTimer <= 0)
                Projectile.scale = 1f + (float)Math.Sin(EaseBuilder.EaseCircularInOut.Ease(1 - (Projectile.timeLeft / maxTimeLeft)) * MathHelper.Pi) * 0.4f * 0.4f;

            Owner.heldProj = Projectile.whoAmI;

            if (Main.myPlayer == Owner.whoAmI)
                Owner.direction = Main.MouseWorld.X > Owner.Center.X ? 1 : -1;

            if (!(Owner.HeldItem.ModItem is Bladesaw)) //since this doesnt set player.itemTime we have to manually check if the player tries to switch to another weapon. Not setting itemTime creates that smooth transition between swings
                Projectile.Kill();

            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Type];
            }

            oldRotation.Add(Projectile.rotation);

            if (oldRotation.Count > 10)
                oldRotation.RemoveAt(0);

            if (Projectile.soundDelay <= 0)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item22, Projectile.Center);
                Projectile.soundDelay = 30;
            }

            if (Projectile.ai[1] == 1f)
                for (int i = 0; i < 30; i++)
                {
                    Point bladePoint = (Owner.Center + (((2 * i) * Projectile.scale) * Projectile.rotation.ToRotationVector2())).ToTileCoordinates();
                    BreakTrees(bladePoint.X, bladePoint.Y, Owner.Center + (((2 * i) * Projectile.scale) * Projectile.rotation.ToRotationVector2()));
                }

            if (hitAmount > 2 && Projectile.timeLeft > 15)
                if (Main.rand.NextBool(12 - hitAmount))
                {
                    Vector2 bladeLine = Vector2.Lerp(Owner.Center + ((10 * Projectile.scale) * Projectile.rotation.ToRotationVector2()), Owner.Center + ((60 * Projectile.scale) * Projectile.rotation.ToRotationVector2()), Main.rand.NextFloat()); //randomly lerps between start of blade and end of it
                    Dust.NewDustPerfect(bladeLine, ModContent.DustType<Dusts.GlowFastDecelerate>(), null, 25, new Color(255, 96, 0), Main.rand.NextFloat(0.4f, 0.5f));
                }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            oldTimeleft = Projectile.timeLeft;
            pauseTimer = 7;
            hitAmount++;
            for (int i = 0; i < (target.type == NPCID.Bunny ? 35 : 5); i++)
            {
                Vector2 directionTo = target.DirectionTo(Owner.Center);
                if (!Helper.IsFleshy(target))
                    Dust.NewDustPerfect((target.Center + (directionTo * 10)) + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(), directionTo.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * -Main.rand.NextFloat(0.5f, 5f), 0, new Color(255, 230, 60) * 0.8f, 1.6f);
                else
                {
                    for (int j = 0; j < 6; j++)
                    {
                        Dust.NewDustPerfect(target.Center + (directionTo * 10), DustID.Blood, directionTo.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * -Main.rand.NextFloat(0.5f, 5f), 0, default, 1.3f);
                        Dust.NewDustPerfect(target.Center + (directionTo * 10), DustID.Blood, directionTo.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * -Main.rand.NextFloat(0.5f, 5f), 0, default, 0.7f);
                    }
                    Dust.NewDustPerfect(target.Center + (directionTo * 5), ModContent.DustType<Dusts.GraveBlood>(), directionTo.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.NextFloat(3f, 6f), 0, default, 1.1f);
                    Dust.NewDustPerfect(target.Center + (directionTo * 10), ModContent.DustType<Dusts.GraveBlood>(), Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * -Main.rand.NextFloat(3f, 5f), 0, default, 1.1f);
                }
                Dust.NewDustPerfect(target.Center + (directionTo * 10), DustType<Dusts.BuzzsawSteam>(), Vector2.UnitY * -2f, 25, default, 0.5f);
            }
            Helper.PlayPitched("Impacts/StabTiny", 1.2f, Main.rand.NextFloat(-0.3f, 0.3f), target.Center);
            CameraSystem.Shake += 2;

            var globalNPC = target.GetGlobalNPC<ShreddedNPC>();
            globalNPC.ShreddedTimer = 300;
            globalNPC.ShreddedStacks++;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage = (int)(damage * (1f + (0.03f * hitAmount)));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D bladeTex = ModContent.Request<Texture2D>(Texture + "_Blade").Value;
            Texture2D chainTex = ModContent.Request<Texture2D>(Texture + "_Chain").Value;
            Texture2D chainGlowTex = ModContent.Request<Texture2D>(Texture + "_ChainGlow").Value;
            SpriteEffects flip = Owner.direction == -1 ? SpriteEffects.FlipHorizontally : 0;
            float rotation = Projectile.rotation + MathHelper.PiOver4 + (Owner.direction == -1 ? MathHelper.PiOver2 : 0f);
            Rectangle sourceRectangle = tex.Frame(1, Main.projFrames[Projectile.type], frameY: Projectile.frame);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 drawPos = Owner.Center + Projectile.rotation.ToRotationVector2() * 35f - Main.screenPosition + Main.rand.NextVector2Circular(1.5f, 1.5f);
            Color heatColor = Color.Lerp(Color.Transparent, new Color(255, 96, 0), (hitAmount / 6f) * 0.65f);
            if (Projectile.timeLeft < 20)
                heatColor = Color.Lerp(heatColor, Color.Transparent, 1f - (Projectile.timeLeft / 20f));

            for (int k = 10; k > 0; k--)
            {
                float progress = 1 - (float)(((float)(10 - k) / (float)10));
                Color color = Color.Lerp(lightColor, new Color(255, 96, 0), (hitAmount / 6f)) * EaseFunction.EaseQuarticOut.Ease(progress) * 0.1f;
                if (Projectile.timeLeft < 20)
                    color = Color.Lerp(color, Color.Transparent, 1f - (Projectile.timeLeft / 20f));
                color.A = 0;
                if (k > 0 && k < oldRotation.Count)
                    Main.spriteBatch.Draw(bladeTex, drawPos, sourceRectangle, color, oldRotation[k] + MathHelper.PiOver4 + (Owner.direction == -1 ? MathHelper.PiOver2 : 0f), origin, Projectile.scale, flip, 0f);
            }
            Main.spriteBatch.Draw(tex, drawPos, sourceRectangle, lightColor, rotation, origin, Projectile.scale, flip, 0f);
            Main.spriteBatch.Draw(chainTex, drawPos, sourceRectangle, heatColor, rotation, origin, Projectile.scale, flip, 0f);
            heatColor.A = 0;
            Main.spriteBatch.Draw(chainGlowTex, drawPos, sourceRectangle, heatColor, rotation, origin, Projectile.scale, flip, 0f);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Owner.Center, Owner.Center + ((60 * Projectile.scale) * Projectile.rotation.ToRotationVector2()), 20, ref collisionPoint))
                return true;
            return false;
        }
        
        public override bool? CanCutTiles()
        {
            return false;
        }

        public void BreakTrees(int i, int j, Vector2 visualPos)
        {
            Tile tileAtPosition = Framing.GetTileSafely(i, j);
            if (!tileAtPosition.HasTile || !Main.tileAxe[(int)(tileAtPosition.TileType)])
                return;
            float swingCompletion = 1f - (Projectile.timeLeft / maxTimeLeft);

            if (swingCompletion < 0.45f || swingCompletion > 0.6f)  
                return;

            if (!WorldGen.CanKillTile(i, j) || pauseTimer > 0)
                return;
            
            Owner.PickTile(i, j, 40);
            oldTimeleft = Projectile.timeLeft;
            pauseTimer = 6;
            for (int d = 0; d < 2; d++)
            {
                Dust.NewDustPerfect(visualPos, DustType<Dusts.BuzzsawSteam>(), Vector2.UnitY * -2f, 25, default, 0.5f);
                Dust.NewDustPerfect(visualPos + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(),
                    Vector2.UnitX.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * -Main.rand.NextFloat(0.5f, 5f), 0, new Color(255, 230, 60) * 0.8f, 1.6f);
            }
        }
    }
}