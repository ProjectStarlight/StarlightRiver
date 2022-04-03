using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	internal class Sandscript : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sandscript");
            Tooltip.SetDefault("Manifests a blade of sand\n`This lost tablet contains a fragment of the Epic of Yeremy\n...The writing is sorta ameteurish for an ancient relic`");
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 43;
            Item.useTime = 43;
            Item.shootSpeed = 1f;
            Item.knockBack = 7f;
            Item.damage = 12;
            Item.shoot = ProjectileType<SandSlash>();
            Item.rare = ItemRarityID.Blue;
            Item.noMelee = true;
            Item.magic = true;
            Item.mana = 10;

            Item.UseSound = SoundID.Item45;
        }

        public override bool Shoot(Player Player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            int i = Projectile.NewProjectile(Player.Center, Vector2.Normalize(Main.MouseWorld - Player.Center) * 25, type, damage, knockBack, Player.whoAmI);
            Main.projectile[i].rotation = (Main.MouseWorld - Player.Center).ToRotation();
            return false;
        }

		public override void AddRecipes()
		{
            var r = new LearnableRecipe("SandScripts");
            r.AddIngredient(ItemID.Sandstone, 10);
            r.AddIngredient(ItemID.Topaz);
            r.AddTile(TileID.Anvils);
            r.SetResult(this);
            r.AddRecipe();
		}
	}

    internal class SandSlash : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 45;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            if (Projectile.ai[0] == 30) Projectile.knockBack *= 0;

            Vector2 relativeRot = new Vector2
            {
                X = (float)Math.Cos(Projectile.ai[0] / 60 * 6.28f) * 3f,
                Y = (float)Math.Sin(Projectile.ai[0] / 60 * 6.28f) * 10f
            };
            Projectile.velocity = relativeRot.RotatedBy(Projectile.rotation - 1.57f);

            Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Stamina>(), Projectile.velocity * Main.rand.NextFloat(0.2f, 1.1f), 0, default, 1f);
            Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Sand>(), Projectile.velocity * Main.rand.NextFloat(0.8f, 1.2f), 140, default, 0.7f);

            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.2f, 0));
        }
    }
}