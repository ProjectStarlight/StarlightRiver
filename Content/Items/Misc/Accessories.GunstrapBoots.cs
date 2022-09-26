//TODO:
//Obtainment
//Sellprice
//Rarity
//Description
//Acount for grappling hooks
//Account for mounts
//Actual texture

using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using System;

namespace StarlightRiver.Content.Items.Misc
{
	public class GunstrapBoots : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public GunstrapBoots() : base("Gunstrap Boots", "[PH]") { }

        public Vector2 oldVelocity = Vector2.Zero;

        public override void SafeSetDefaults()
        {
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.LightRed;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override void SafeUpdateEquip(Player player)
        {
            if (oldVelocity.Y != 0 && player.velocity.Y != 0 && Math.Abs(oldVelocity.Y - player.velocity.Y) > 1 && player.controlJump)
                Fire(player);
            oldVelocity = player.velocity;
        }

        private void Fire(Player player)
        {
            if (player.PickAmmo(Item, out int projToShoot, out float speed, out int damage, out float knockBack, out int usedAmmoItemID))
            {
                Helpers.Helper.PlayPitched("Guns/Scrapshot", 0.4f, 0, player.Center);
                Core.Systems.CameraSystem.Shake += 2;
                for (int i = 0; i < 6; i++)
                    Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Center, Vector2.UnitY.RotatedByRandom(0.5f) * ((speed + 9) * Main.rand.NextFloat(0.85f,1.15f)), projToShoot, (int)(damage + (7 * player.bulletDamage.Multiplicative)), knockBack, player.whoAmI);
                
                Dust.NewDustPerfect(player.Bottom, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY.RotatedByRandom(0.2f) * 5, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));

                Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Bottom, Vector2.Zero, ModContent.ProjectileType<CoachGunMuzzleFlash>(), 0, 0, player.whoAmI, 1.57f);

                Gore.NewGore(player.GetSource_Accessory(Item), player.Bottom, new Vector2(Main.rand.NextBool() ? 1 : -1, -0.5f) * 2, Mod.Find<ModGore>("CoachGunCasing").Type, 1f);

                for (int k = 0; k < 15; k++)
                    Dust.NewDustPerfect(player.Bottom + new Vector2(Main.rand.Next(-10,10), 0), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY.RotatedByRandom(0.4f) * Main.rand.NextFloat(16), 125, new Color(150, 80, 40), Main.rand.NextFloat(0.5f, 0.9f));
            }
        }
    }
}