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

namespace StarlightRiver.Content.Items.Misc
{
    public class Bladesaw : ModItem
    {
        private int swingDirection = 1;
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ProjectileType<BladesawSwungBlade>()] <= 0;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bladesaw");
            Tooltip.SetDefault("Slashes with a combo heavy swings\nSlows down on hit, tenderizing anything unlucky enough caught it the way");
        }

        public override void SetDefaults()
        {
            Item.damage = 24;
            Item.crit = 4;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = Item.useAnimation = 65;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4.5f;

            Item.shootSpeed = 5f;
            Item.shoot = ProjectileType<BladesawSwungBlade>();
            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.value = Item.sellPrice(0, 0, 20, 0);
            Item.rare = ItemRarityID.Blue;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingDirection *= -1;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, swingDirection);
            return false;
        }
    }

    class BladesawSwungBlade : ModProjectile
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        private Vector2 direction;

        private bool initialized;

        private float maxTimeLeft;

        private int oldTimeleft;

        private int hitAmount;

        public bool justHit = false;

        public short pauseTimer = -1;

        public float SwingDirection => Projectile.ai[0] * Math.Sign(direction.X);

        public Player Owner => Main.player[Projectile.owner];

        public override bool? CanHitNPC(NPC target)
        {
            return hitAmount <= 6 && 1 - (Projectile.timeLeft / maxTimeLeft) > 0.2f;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bladesaw");
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.Size = new Vector2(60);
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }

        public override bool PreAI()
        {
            return true;
        }

        public override void AI()
        {
            if (--pauseTimer > 0)
            {
                Projectile.timeLeft = oldTimeleft;
            }

            if (!initialized)
            {
                initialized = true;
                Projectile.timeLeft = Owner.HeldItem.useAnimation;
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

            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Type];
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            oldTimeleft = Projectile.timeLeft;
            pauseTimer = 5;
            hitAmount++;

            for (int i = 0; i < 5; i++)
            {
                Vector2 directionTo = target.DirectionTo(Owner.Center);
                if (!Helper.IsFleshy(target))
                    Dust.NewDustPerfect((target.Center + (directionTo * 10)) + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(), directionTo.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * -Main.rand.NextFloat(0.5f, 5f), 0, new Color(255, 230, 60) * 0.8f, 1.6f);
                else
                {
                    Helper.PlayPitched("Impacts/StabTiny", 0.8f, Main.rand.NextFloat(-0.3f, 0.3f), target.Center);

                    for (int j = 0; j < 2; j++)
                        Dust.NewDustPerfect(target.Center + (directionTo * 10), ModContent.DustType<Dusts.GraveBlood>(), directionTo.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * -Main.rand.NextFloat(0.5f, 5f));
                }
                Dust.NewDustPerfect(target.Center + (directionTo * 10), DustType<Dusts.BuzzsawSteam>(), Vector2.UnitY * -2f, 25, default, 0.5f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            SpriteEffects flip = 0;
            if (Owner.direction == -1)
                flip = SpriteEffects.FlipHorizontally;
            float rotation = Projectile.rotation + MathHelper.PiOver4 + (Owner.direction == -1 ? MathHelper.PiOver2 : 0f);
            Rectangle sourceRectangle = tex.Frame(1, Main.projFrames[Projectile.type], frameY: Projectile.frame);
            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 drawPos = Owner.Center + Projectile.rotation.ToRotationVector2() * 35f - Main.screenPosition;
            Main.spriteBatch.Draw(tex, drawPos, sourceRectangle, lightColor, rotation, origin, Projectile.scale, flip, 0f);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Owner.Center, Owner.Center + ((60 * Projectile.scale) * Projectile.rotation.ToRotationVector2()), 20, ref collisionPoint))
                return true;
            return false;
        }
    }
}
