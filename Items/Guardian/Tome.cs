using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Guardian
{
    internal class Tome : ModItem
    {
        public int Radius { get; set; }
        public int ProjectileType { get; set; }
        public int HealthCost { get; set; }

        public Tome(int projType, int rad, int hpcost)
        {
            Radius = rad;
            ProjectileType = projType;
            HealthCost = hpcost;
        }

        public virtual void EffectTooltip(List<TooltipLine> tooltips)
        {
        }

        public sealed override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            EffectTooltip(tooltips);
            tooltips.Insert(2, new TooltipLine(mod, "Radius", Radius + " effect radius"));
            tooltips.Insert(3, new TooltipLine(mod, "HealthCost", "uses " + HealthCost + " Life"));
        }

        public virtual void SafeSetDefaults()
        {
        }

        public sealed override void SetDefaults()
        {
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.EatingUsing;
            item.useTime = 15;
            item.useAnimation = 15;
        }

        public sealed override bool CanUseItem(Player player)
        {
            if (player.statLife > HealthCost)
            {
                player.statLife -= HealthCost;
                CombatText.NewText(player.Hitbox, Color.Red, HealthCost);
                return true;
            }
            return false;
        }

        public virtual bool SafeUseItem(Player player)
        {
            return true;
        }

        public sealed override bool UseItem(Player player)
        {
            StarlightPlayer mp = player.GetModPlayer<StarlightPlayer>();
            Projectile.NewProjectile(Main.MouseWorld, Vector2.Zero, ProjectileType, 0, 0, player.whoAmI, mp.GuardBuff, Radius + mp.GuardRad);
            return true;
        }
    }

    public class TomeProjectile : ModProjectile
    {
        public virtual void BoostPlayer(Player player)
        {
        }

        public virtual void SafeAI()
        {
        }

        public override void AI()
        {
            foreach (Player player in Main.player.Where(player => Helper.CheckCircularCollision(projectile.Center, (int)(projectile.ai[1] * 1.3f), player.Hitbox)))
            {
                BoostPlayer(player);
            }
            for (int k = 0; k <= projectile.ai[1]; k += 4)
            {
                Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * projectile.ai[1], DustType<Dusts.Starlight>(), Vector2.Zero, 0, default, 0.8f);
            }
            SafeAI();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = GetTexture(projectile.modProjectile.Texture);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), Color.White, 0, tex.Size() / 2, 1, 0, 0);

            foreach (Player player in Main.player.Where(player => Helper.CheckCircularCollision(projectile.Center, (int)(projectile.ai[1] * 1.3f), player.Hitbox)))
            {
                Rectangle target = new Rectangle((int)(player.Center.X - 8 - Main.screenPosition.X), (int)(player.Center.Y - 48 - Main.screenPosition.Y), 16, 16);
                spriteBatch.Draw(tex, target, tex.Frame(), Color.White * (0.5f + (float)Math.Sin(StarlightWorld.rottime * 2) * 0.5f), 0, Vector2.Zero, 0, 0);
            }
            return false;
        }
    }
}