using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.Hell;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
    class MagmiteBottle : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Magmite in a Bottle");
            Tooltip.SetDefault("Why would you do this to him?!");
        }

        public override void SetDefaults()
        {
            item.damage = 120;
            item.ranged = true;
            item.width = 36;
            item.height = 38;
            item.useTime = 18;
            item.useAnimation = 18;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.value = 0;
            item.rare = ItemRarityID.Orange;
            item.UseSound = SoundID.Item1;
            item.autoReuse = false;
            item.useTurn = true;
            item.shoot = ModContent.ProjectileType<MagmiteBottleProjectile>();
            item.shootSpeed = 8.5f;
            item.consumable = true;
        }
    }

    class MagmiteBottleProjectile : ModProjectile 
    {
        public override string Texture => AssetDirectory.VitricItem + "MagmiteBottle";

        public override Color? GetAlpha(Color lightColor) => Color.White;

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.timeLeft = 600;
            projectile.friendly = true;
            projectile.damage = 50;
        }

        public override void AI()
        {
            projectile.velocity.Y += 0.4f;
            projectile.rotation += projectile.velocity.X * 0.05f;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 60; k++)
                Gore.NewGoreDirect(projectile.Center, (Vector2.UnitY * Main.rand.NextFloat(-16, -1)).RotatedByRandom(0.8f), ModGore.GetGoreSlot("StarlightRiver/Assets/NPCs/Vitric/MagmiteGore"), Main.rand.NextFloat(1.0f, 1.4f));

            for (int k = 0; k < 50; k++)
                Dust.NewDust(projectile.position, 16, 16, 14);

            for (int x = -8; x < 8; x++)
            {
                for (int y = -8; y < 8; y++)
                {
                    Tile tile = Main.tile[(int)projectile.Center.X / 16 + x, (int)projectile.Center.Y / 16 + y];
                    if (tile.active() && Main.tileSolid[tile.type] && Helpers.Helper.IsEdgeTile((int)projectile.Center.X / 16 + x, (int)projectile.Center.Y / 16 + y))
                    {
                        Vector2 pos = new Vector2((int)projectile.Center.X / 16 + x, (int)projectile.Center.Y / 16 + y) * 16 + Vector2.One * 8;
                        if (!Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<MagmaSwordBurn>() && n.Center == pos))
                            Projectile.NewProjectile(pos, Vector2.Zero, ModContent.ProjectileType<MagmaSwordBurn>(), 25, 0, projectile.owner);
                        else Main.projectile.FirstOrDefault(n => n.active && n.type == ModContent.ProjectileType<MagmaSwordBurn>() && n.Center == pos).timeLeft = 180;
                    }
                }
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.AmberBolt, 0, 0, 0, default, 0.5f);
            }

            Main.PlaySound(SoundID.Shatter, projectile.Center);
            Main.PlaySound(SoundID.DD2_GoblinHurt, projectile.Center);
            //Main.PlaySound(SoundID.Drown, projectile.Center);
        }
    }

}
