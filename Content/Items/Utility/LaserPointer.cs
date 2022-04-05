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
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Utility
{
	class LaserPointerLoader : IOrderedLoadable
	{
		public float Priority => 1;

		public void Load()
		{
            var Mod = StarlightRiver.Instance;

            Mod.AddItem("RedLaserPointer", new LaserPointer(new Color(255, 20, 20), ItemID.Ruby));
            Mod.AddItem("OrangeLaserPointer", new LaserPointer(new Color(255, 120, 20), ItemID.Amber));
            Mod.AddItem("YellowLaserPointer", new LaserPointer(new Color(255, 255, 20), ItemID.Topaz));
            Mod.AddItem("GreenLaserPointer", new LaserPointer(new Color(20, 255, 20), ItemID.Emerald));
            Mod.AddItem("BlueLaserPointer", new LaserPointer(new Color(20, 80, 255), ItemID.Sapphire));
            Mod.AddItem("PurpleLaserPointer", new LaserPointer(new Color(150, 20, 255), ItemID.Amethyst));
            Mod.AddItem("WhiteLaserPointer", new LaserPointer(Color.White, ItemID.Diamond));
            Mod.AddItem("PinkLaserPointer", new LaserPointer(new Color(255, 140, 180), ItemID.PinkGel));
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
			Item.width = 32;
			Item.height = 32;
			Item.rare = ItemRarityID.Green;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.channel = true;
            Item.shoot = ModContent.ProjectileType<LaserPointerProjectile>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
            if (!Main.projectile.Any(n => n.active && n.type == Item.shoot && n.owner == player.whoAmI))
            {
                var p = Projectile.NewProjectileDirect(player.Center, Vector2.Zero, ModContent.ProjectileType<LaserPointerProjectile>(), damage, knockback, player.whoAmI);
                (p.ModProjectile as LaserPointerProjectile).color = color;
            }

            return false;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
            var tex = ModContent.Request<Texture2D>(Texture + "Glow").Value;
            spriteBatch.Draw(tex, position, frame, color, 0, origin, scale, 0, 0);
		}

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
            var tex = ModContent.Request<Texture2D>(Texture + "Glow").Value;
            spriteBatch.Draw(tex, Item.position, null, color, 0, Vector2.Zero, scale, 0, 0);
        }

		public override void AddRecipes()
		{
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 5);
            recipe.AddIngredient(extraMaterial);
		}
	}

	class LaserPointerProjectile : ModProjectile, IDrawAdditive
    {
        public Vector2 endPoint;
        public float LaserRotation;
        public Color color;

        public Player Owner => Main.player[Projectile.owner];

        public override string Texture => "StarlightRiver/Assets/Items/Utility/LaserPointerProjectile";

        public override void SetDefaults()
        {
            Projectile.timeLeft = 2;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.Center = Owner.Center;
            LaserRotation = (Main.MouseWorld - Owner.Center).ToRotation();
            Owner.heldProj = Projectile.whoAmI;

            if(Owner.channel)
                Projectile.timeLeft = 2;

            for (int k = 0; k < 160; k++)
            {
                Vector2 posCheck = Projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * k * 8;

                if (Helper.PointInTile(posCheck) || k == 159)
                {
                    endPoint = posCheck;
                    break;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, Owner.Center - Main.screenPosition, null, lightColor, LaserRotation, new Vector2(0, ModContent.Request<Texture2D>(Texture).Value.Height - 3), 1, 0, 0);
            spriteBatch.Draw(ModContent.Request<Texture2D>(Texture + "Glow").Value, Owner.Center - Main.screenPosition, null, color, LaserRotation, new Vector2(0, ModContent.Request<Texture2D>(Texture + "Glow").Value.Height - 3), 1, 0, 0);
            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            var texBeam = ModContent.Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value;

            Vector2 origin = new Vector2(0, texBeam.Height / 2);

            float height = 10;
            int width = (int)(Projectile.Center - endPoint).Length() - 24;

            var pos = Projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(LaserRotation) * 24;
            var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 1.2f));
            spriteBatch.Draw(texBeam, target, null, color, LaserRotation, origin, 0, 0);

            for (int i = 0; i < width; i += 10)
                Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(LaserRotation) * i + Main.screenPosition, color.ToVector3() * height * 0.030f);         

            var impactTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;
            spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color, 0, impactTex.Size() / 2, 0.5f, 0, 0);
        }
    }
}
