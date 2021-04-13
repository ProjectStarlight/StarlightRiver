using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.WorldGeneration;
using Terraria.ModLoader;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
    public class Cheapskates : SmartAccessory, IChestItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public int Stack => 1;

        public ChestRegionFlags Regions => ChestRegionFlags.Ice;

        public Cheapskates() : base("Cheapskates", "Movement speed is doubled\nYou take 30% more damage") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.PreHurtEvent += PreHurtAccessory;

            return true;
        }

        public override void SafeUpdateEquip(Player player)
        {
            player.runAcceleration *= 2;
            player.maxRunSpeed *= 2;
        }

        private bool PreHurtAccessory(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (Equipped(player))
            {
                damage = (int)(damage * 1.3f);
            }

            return true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(ItemID.Wood, 50);
            recipe.AddIngredient(ItemID.Chain, 10);
            recipe.AddIngredient(ItemID.DemoniteBar, 20);
            recipe.AddTile(TileID.IceMachine);

            recipe.SetResult(this);

            recipe.AddRecipe();

            recipe = new ModRecipe(mod);

            recipe.AddIngredient(ItemID.Wood, 50);
            recipe.AddIngredient(ItemID.Chain, 10);
            recipe.AddIngredient(ItemID.CrimtaneBar, 20);
            recipe.AddTile(TileID.IceMachine);

            recipe.SetResult(this);

            recipe.AddRecipe();
        }
    }
}