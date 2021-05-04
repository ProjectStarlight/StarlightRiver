using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;

using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.Items.Misc
{
    class MonkSpade : ModItem
    {
        public float bonusChance = 0;

        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetDefaults()
        {
            item.melee = true;
            item.width = 32;
            item.height = 32;
            item.damage = 20;
            item.useTime = 40;
            item.useAnimation = 40;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.knockBack = 2.5f;
            item.autoReuse = true;

            item.UseSound = mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/GlassMinibossSword");

            item.shoot = ProjectileType<MonkSpadeProjectile>();
            item.shootSpeed = 1;
        }
    }

    class MonkSpadeProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.timeLeft = 40;
            projectile.penetrate = -1;
            projectile.friendly = true;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            var player = Main.player[projectile.owner];

            var center = player.Center + Vector2.UnitY * player.gfxOffY;

            if(projectile.timeLeft == 40)
                projectile.rotation = (player.Center - Main.MouseWorld).ToRotation();

            player.heldProj = projectile.whoAmI;

            projectile.extraUpdates = projectile.timeLeft > 15 ? 2 : 0;

            projectile.Center = center + Vector2.UnitX.RotatedBy(projectile.rotation + (projectile.timeLeft - 20f) / 20f * 0.3f * -player.direction) * (float)Math.Sin(projectile.timeLeft / 40f * Math.PI) * -100;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var player = Main.player[projectile.owner];

            float fade =
                projectile.timeLeft > 35 ? (40 - projectile.timeLeft) / 5f :
                projectile.timeLeft < 5 ? projectile.timeLeft / 5f : 
                1;

            var tex = GetTexture(Texture);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, lightColor * fade, (projectile.Center - player.Center).ToRotation() + (float)Math.PI - (float)Math.PI / 4, new Vector2(8, 8), 1, 0, 0);

            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            var ownerSpade = Main.player[projectile.owner].HeldItem.modItem as MonkSpade;

            if (projectile.type == ProjectileType<MonkSpadeProjectile>() && ownerSpade != null)
            {
                if (target.life <= 0)
                {
                    if (Main.rand.NextFloat() < ownerSpade.bonusChance)
                    {
                        Item.NewItem(target.Center, ItemID.Heart);
                        ownerSpade.bonusChance = 0;
                    }
                    else
                        ownerSpade.bonusChance += 0.1f;
                }
                else
                    ownerSpade.bonusChance += 0.05f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            projectile.friendly = false; //cant deal damage through walls
            return false;
        }
    }
}
