using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
    internal class Cherry : Ingredient
    {
        public Cherry() : base("Critical hits may cause explosions", 60, IngredientType.Side) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.White;

		public override void Load()
		{
            StarlightPlayer.ModifyHitNPCEvent += ChanceToBoom;
            StarlightPlayer.ModifyHitNPCWithProjEvent += ChanceToBoomProjectile;
		}

		private void ChanceToBoomProjectile(Player player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) //TODO: Custom projectile here?
		{
			if (crit && Active(player) && Main.rand.NextBool(10))
				Projectile.NewProjectile(player.GetSource_Buff(player.FindBuffIndex(ModContent.BuffType<Buffs.FoodBuff>())), target.Center, Vector2.Zero, ModContent.ProjectileType<Vitric.NeedlerExplosion>(), 50, 1, player.whoAmI);
		}

		private void ChanceToBoom(Player player, Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (crit && Active(player) && Main.rand.NextBool(10))
				Projectile.NewProjectile(player.GetSource_Buff(player.FindBuffIndex(ModContent.BuffType<Buffs.FoodBuff>())), target.Center, Vector2.Zero, ModContent.ProjectileType<Vitric.NeedlerExplosion>(), 50, 1, player.whoAmI);
		}
    }
}