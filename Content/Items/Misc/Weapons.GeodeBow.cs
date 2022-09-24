//TODO:
//Balance
//Sell price
//Rarity
//Better collision on crystals
//Visuals
//AOE effect
//Better arrow consumption

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
    class GeodeBow : ModItem
    {

        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geode Bow");
            Tooltip.SetDefault("Hit enemies to create crystal growths \nShoot these growths to deal massive damage");
        }

        public override void SetDefaults()
        {
            Item.damage = 44;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 16;
            Item.height = 64;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 1;
            Item.rare = ItemRarityID.Orange;
            Item.channel = true;
            Item.shoot = ProjectileType<GeodeBowProj>();
            Item.shootSpeed = 0f;
            Item.autoReuse = true;
            Item.useAmmo = AmmoID.Arrow;
            Item.useTurn = true;
            Item.channel = true;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<GeodeBowProj>());
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity / 4f, ProjectileType<GeodeBowProj>(), damage, knockback, player.whoAmI, type);
            return false;
        }
    }

    internal class GeodeBowProj : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        private int arrowType => (int)Projectile.ai[0];

        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geode Bow");
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
        }

        public override void AI()
        {
            owner.itemAnimation = owner.itemTime = 2;
            owner.direction = Math.Sign(owner.DirectionTo(Main.MouseWorld).X);
            Projectile.rotation = owner.DirectionTo(Main.MouseWorld).ToRotation();
            Projectile.velocity = Vector2.Zero;
            Projectile.Center = owner.Center;

            owner.itemRotation = Projectile.rotation;

            if (owner.direction != 1)
                owner.itemRotation -= 3.14f;

            owner.heldProj = Projectile.whoAmI;

            Projectile.frameCounter++;
            if (Projectile.frameCounter % 6 == 5)
            {
                Projectile.frame++;
                if (Projectile.frame == 4)
                    Shoot();
            }

            if (Projectile.frame >= 6)
            {
                if (owner.channel)
                {
                    Projectile.frame = 0;
                    Projectile.frameCounter = 0;
                }
                else
                    Projectile.active = false;
            }

            Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
            if (Projectile.frame == 3)
                stretch = Player.CompositeArmStretchAmount.None;
            else if (Projectile.frame == 2)
                stretch = Player.CompositeArmStretchAmount.Quarter;
            else if (Projectile.frame == 1)
                stretch = Player.CompositeArmStretchAmount.ThreeQuarters;
            else
                stretch = Player.CompositeArmStretchAmount.Full;
            owner.SetCompositeArmFront(true, stretch, Projectile.rotation - 1.57f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Rectangle frameBox = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frameBox, lightColor, Projectile.rotation, new Vector2(0, frameHeight / 2), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        private void Shoot()
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);
            Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.rotation.ToRotationVector2() * 20, arrowType, Projectile.damage, Projectile.knockBack, owner.whoAmI);
            proj.GetGlobalProjectile<GeodeBowGProj>().shotFromGeodeBow = true;
        }
    }

    internal class GeodeBowGrowth : ModProjectile
    {
        private Player owner => Main.player[Projectile.owner];

        public override string Texture => AssetDirectory.MiscItem + Name;

        public NPC target;

        public Vector2 offset;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geode Growth");
        }

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.timeLeft = 500;
            Projectile.scale = 0;
            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        public override void AI()
        {
            if (Projectile.scale < 1)
                Projectile.scale += 0.025f;

            Projectile.rotation = offset.ToRotation() + 2.35f;
            if (!target.active)
            {
                Projectile.active = false;
                return;
            }
            Projectile.Center = target.Center + offset;

            if (Main.projectile.Any(n => n.active && n.Hitbox.Intersects(Projectile.Hitbox) && n.GetGlobalProjectile<GeodeBowGProj>().shotFromGeodeBow))
                Shatter();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        private void Shatter()
        {
            Projectile.active = false;
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
        }
    }

    public class GeodeBowGProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool shotFromGeodeBow = false;

        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
        {
            if (!shotFromGeodeBow)
                return;
            Projectile proj = Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<GeodeBowGrowth>(), projectile.damage, projectile.knockBack, projectile.owner);
            var modProj = proj.ModProjectile as GeodeBowGrowth;

            Vector2 offset = (projectile.Center - target.Center);
            modProj.target = target;
            modProj.offset = offset + (Vector2.Normalize(offset) * 20);
        }
    }
}