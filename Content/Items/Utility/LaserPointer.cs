using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.Items.Utility
{
	class LaserPointerLoader : IOrderedLoadable
	{
		public float Priority => 1;

		public void Load()
		{
            var mod = StarlightRiver.Instance;

            mod.AddItem("RedLaserPointer", new LaserPointer(new Color(255, 20, 20), ItemID.Ruby));
            mod.AddItem("OrangeLaserPointer", new LaserPointer(new Color(255, 120, 20), ItemID.Amber));
            mod.AddItem("YellowLaserPointer", new LaserPointer(new Color(255, 255, 20), ItemID.Topaz));
            mod.AddItem("GreenLaserPointer", new LaserPointer(new Color(20, 255, 20), ItemID.Emerald));
            mod.AddItem("BlueLaserPointer", new LaserPointer(new Color(20, 80, 255), ItemID.Sapphire));
            mod.AddItem("PurpleLaserPointer", new LaserPointer(new Color(150, 20, 255), ItemID.Amethyst));
            mod.AddItem("WhiteLaserPointer", new LaserPointer(Color.White, ItemID.Diamond));
            mod.AddItem("PinkLaserPointer", new LaserPointer(new Color(255, 140, 180), ItemID.PinkGel));
        }

        public void Unload() { }
	}

	class LaserPointer : ModItem
	{
        private Color color;
        private int extraMaterial;

        public override string Texture => "StarlightRiver/Assets/Items/Utility/LaserPointer";

        public override bool CloneNewInstances => true;

        public LaserPointer(Color color, int extraMaterial)
		{
            this.color = color;
            this.extraMaterial = extraMaterial;
		}

		public override void SetDefaults()
		{
			item.width = 32;
			item.height = 32;
			item.rare = ItemRarityID.Green;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useTime = 5;
			item.useAnimation = 5;
			item.channel = true;
            item.shoot = ModContent.ProjectileType<LaserPointerProjectile>();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
            if (!Main.projectile.Any(n => n.active && n.type == item.shoot && n.owner == player.whoAmI))
            {
                var p = Projectile.NewProjectileDirect(player.Center, Vector2.Zero, ModContent.ProjectileType<LaserPointerProjectile>(), damage, knockBack, player.whoAmI);
                (p.modProjectile as LaserPointerProjectile).color = color;
            }

            return false;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
            var tex = ModContent.GetTexture(Texture + "Glow");
            spriteBatch.Draw(tex, position, frame, color, 0, origin, scale, 0, 0);
		}

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
            var tex = ModContent.GetTexture(Texture + "Glow");
            spriteBatch.Draw(tex, item.position, null, color, 0, Vector2.Zero, scale, 0, 0);
        }

		public override void AddRecipes()
		{
            ModRecipe r = new ModRecipe(mod);
            r.AddRecipeGroup(RecipeGroupID.IronBar, 5);
            r.AddIngredient(extraMaterial);
            r.SetResult(this);
            r.AddRecipe();
		}
	}

	class LaserPointerProjectile : ModProjectile, IDrawAdditive
    {
        public Vector2 endPoint;
        public float LaserRotation;
        public Color color;

        public Player Owner => Main.player[projectile.owner];

        public override string Texture => "StarlightRiver/Assets/Items/Utility/LaserPointerProjectile";

        public override void SetDefaults()
        {
            projectile.timeLeft = 2;
            projectile.width = 2;
            projectile.height = 2;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            projectile.Center = Owner.Center;
            LaserRotation = (Main.MouseWorld - Owner.Center).ToRotation();
            Owner.heldProj = projectile.whoAmI;

            if(Owner.channel)
                projectile.timeLeft = 2;

            for (int k = 0; k < 160; k++)
            {
                Vector2 posCheck = projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * k * 8;

                if (Helper.PointInTile(posCheck) || k == 159)
                {
                    endPoint = posCheck;
                    break;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(ModContent.GetTexture(Texture), Owner.Center - Main.screenPosition, null, lightColor, LaserRotation, new Vector2(0, ModContent.GetTexture(Texture).Height - 3), 1, 0, 0);
            spriteBatch.Draw(ModContent.GetTexture(Texture + "Glow"), Owner.Center - Main.screenPosition, null, color, LaserRotation, new Vector2(0, ModContent.GetTexture(Texture + "Glow").Height - 3), 1, 0, 0);
            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            var texBeam = ModContent.GetTexture(AssetDirectory.Assets + "GlowTrail");

            Vector2 origin = new Vector2(0, texBeam.Height / 2);

            float height = 10;
            int width = (int)(projectile.Center - endPoint).Length() - 24;

            var pos = projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(LaserRotation) * 24;
            var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 1.2f));
            spriteBatch.Draw(texBeam, target, null, color, LaserRotation, origin, 0, 0);

            for (int i = 0; i < width; i += 10)
                Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(LaserRotation) * i + Main.screenPosition, color.ToVector3() * height * 0.030f);         

            var impactTex = ModContent.GetTexture(AssetDirectory.Assets + "Keys/GlowSoft");
            spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color, 0, impactTex.Size() / 2, 0.5f, 0, 0);
        }
    }
}
