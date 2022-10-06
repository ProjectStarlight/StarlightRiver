using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Content.Items.Misc
{
	public class Cheapskates : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public Cheapskates() : base("Cheapskates", "Maximum movement speed is doubled\nTake up to 25% more damage while moving over default speed\nAcceleration is reduced by 75% when over default speed") { }

        public override void Load()
        {
            StarlightPlayer.PreHurtEvent += PreHurtAccessory;
            StarlightPlayer.PostUpdateRunSpeedsEvent += ModifyMovement;
        }

        public override void Unload()
		{
            StarlightPlayer.PreHurtEvent -= PreHurtAccessory;
            StarlightPlayer.PostUpdateRunSpeedsEvent -= ModifyMovement;
        }

        private bool PreHurtAccessory(Player Player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (Equipped(Player) && Math.Abs(Player.velocity.X) > 3f)
            {
                float damageMult = 1f + MathHelper.Lerp(0, 0.25f, (Math.Abs(Player.velocity.X) - 3f) / (Player.maxRunSpeed * 0.5f));
                damage = (int)(damage * damageMult);
            }
            return true;
        }

        private void ModifyMovement(Player Player)
        {
            if (Equipped(Player))
            {
                Player.maxRunSpeed = Player.maxRunSpeed * 2;
                if (Math.Abs(Player.velocity.X) > 3f)
                    Player.runAcceleration *= 0.25f;
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient(ItemID.Wood, 50);
            recipe.AddIngredient(ItemID.Chain, 10);
            recipe.AddIngredient(ItemID.DemoniteBar, 20);
            recipe.AddTile(TileID.IceMachine);
            recipe.Register();

            recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wood, 50);
            recipe.AddIngredient(ItemID.Chain, 10);
            recipe.AddIngredient(ItemID.CrimtaneBar, 20);
            recipe.AddTile(TileID.IceMachine);
            recipe.Register();
        }
    }
}