using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.Hell;
using StarlightRiver.Content.NPCs.Vitric;
using StarlightRiver.Core;
using System.Linq;
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
            Item.damage = 120;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 36;
            Item.height = 38;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = 0;
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.shoot = ModContent.ProjectileType<MagmiteBottleProjectile>();
            Item.shootSpeed = 8.5f;
            Item.consumable = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ModContent.ItemType<MagmitePassiveItem>()).
                AddIngredient(ItemID.Bottle).
                Register();
        }
    }

    class MagmiteBottleProjectile : ModProjectile 
    {
        public override string Texture => AssetDirectory.VitricItem + "MagmiteBottle";

        public override Color? GetAlpha(Color lightColor) => Color.White;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 600;
            Projectile.friendly = true;
            Projectile.damage = 60;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.4f;
            Projectile.rotation += Projectile.velocity.X * 0.05f;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 60; k++)
                Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.UnitY * Main.rand.NextFloat(-16, -1)).RotatedByRandom(0.8f), Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(1.0f, 1.4f));

            for (int k = 0; k < 50; k++)
                Dust.NewDust(Projectile.position, 16, 16, 14);

            for (int x = -8; x < 8; x++)
            {
                for (int y = -8; y < 8; y++)
                {
                    Tile tile = Main.tile[(int)Projectile.Center.X / 16 + x, (int)Projectile.Center.Y / 16 + y];
                    if (tile.HasTile && Main.tileSolid[tile.TileType] && Helpers.Helper.IsEdgeTile((int)Projectile.Center.X / 16 + x, (int)Projectile.Center.Y / 16 + y))
                    {
                        Vector2 pos = new Vector2((int)Projectile.Center.X / 16 + x, (int)Projectile.Center.Y / 16 + y) * 16 + Vector2.One * 8;

                        if (!Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<MagmaSwordBurn>() && n.Center == pos))
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, Vector2.Zero, ModContent.ProjectileType<MagmaSwordBurn>(), 25, 0, Projectile.owner);
                        else Main.projectile.FirstOrDefault(n => n.active && n.type == ModContent.ProjectileType<MagmaSwordBurn>() && n.Center == pos).timeLeft = 180;
                    }
                }
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AmberBolt, 0, 0, 0, default, 0.5f);
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_GoblinHurt, Projectile.Center);
        }
    }

}
