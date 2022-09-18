using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Food
{
	internal class Meal : ModItem
    {
        public List<Item> Ingredients { get; set; } = new List<Item>();
        public int Fullness { get; set; }
        public float FullnessMult { get; set; } = 1;
        public float WellFedMult { get; set; } = 1;

        public override string Texture => AssetDirectory.FoodItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Meal");
            Tooltip.SetDefault("Rich food that provides these buffs:");
        }

        public override void SetDefaults()
        {
            Item.consumable = true;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.width = 32;
            Item.height = 32;
        }

        public override bool CanUseItem(Player player)
        {
            FoodBuffHandler mp = player.GetModPlayer<FoodBuffHandler>();

            if (player.HasBuff(BuffType<Full>())) return false;

            if (Ingredients.Count > 0)
            {
                foreach (Item Item in Ingredients)
                    mp.Consumed.Add(Item.Clone());

                player.AddBuff(BuffType<FoodBuff>(), (int)(Fullness * FullnessMult));
                player.AddBuff(BuffType<Full>(), (int)(Fullness * 1.5f * WellFedMult));               
            }
            else 
                Main.NewText("Bad food! Please report me to the Mod devs.", Color.Red);

            Item.stack--;
            return true;
        }

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
            if (Ingredients.Any(n => (n.ModItem as Ingredient).ThisType == IngredientType.Bonus))
            {
                int type = Ingredients.FirstOrDefault(n => (n.ModItem as Ingredient).ThisType == IngredientType.Bonus).type;
                var tex = TextureAssets.Item[type].Value;

                float thisScale = tex.Width > tex.Height ? 32f / tex.Width : 32f / tex.Height;

                spriteBatch.Draw(tex, position, null, Color.White, 0, tex.Size() / 2, thisScale, 0, 0);

                return false;
            }

            return true;
        }

		public override void OnConsumeItem(Player player)
		{
            FoodBuffHandler mp = player.GetModPlayer<FoodBuffHandler>();
            mp.Consumed.ForEach(n => (n.ModItem as Ingredient).OnUseEffects(player, mp.Multiplier));
        }

		public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string sidesName = "";

            if (Ingredients.Any(n => (n.ModItem as Ingredient).ThisType == IngredientType.Side))
            {
                List<Item> sides = Ingredients.FindAll(n => (n.ModItem as Ingredient).ThisType == IngredientType.Side);
                sidesName += " with " + sides[0].Name;

                if (sides.Count == 2) 
                    sidesName += " and " + sides[1].Name;
            }

            string mainName = "";

            if (Ingredients.Any(n => (n.ModItem as Ingredient).ThisType == IngredientType.Main)) 
                mainName = Ingredients.FirstOrDefault(n => (n.ModItem as Ingredient).ThisType == IngredientType.Main).Name;

            string fullName = mainName + sidesName;

            if (Ingredients.Any(n => (n.ModItem as Ingredient).ThisType == IngredientType.Bonus))
                fullName = Ingredients.FirstOrDefault(n => (n.ModItem as Ingredient).ThisType == IngredientType.Bonus).Name;

            tooltips.FirstOrDefault(n => n.Name == "ItemName" && n.Mod == "Terraria").Text = fullName; 

            foreach (Item Item in Ingredients.Where(n => n.ModItem is Ingredient))
            {
                TooltipLine line = new TooltipLine(Mod, "StarlightRiver: Ingredient", (Item.ModItem as Ingredient).ItemTooltip)
                {
                    OverrideColor = (Item.ModItem as Ingredient).GetColor()
                };

                tooltips.Add(line);
            }

            TooltipLine durationLine = new TooltipLine(Mod, "StarlightRiver: Duration", (int)(Fullness * FullnessMult) / 60 + " seconds duration") { OverrideColor = new Color(110, 235, 255) };
            tooltips.Add(durationLine);

            TooltipLine cooldownLine = new TooltipLine(Mod, "StarlightRiver: Cooldown", (int)(Fullness * 1.5f * WellFedMult) / 60 + " seconds fullness") { OverrideColor = new Color(255, 170, 120) };
            tooltips.Add(cooldownLine);
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("Items", Ingredients);
            tag.Add("Fullness", Fullness);
            tag.Add("FullnessMult", FullnessMult);
            tag.Add("WellFedMult", WellFedMult);
        }

        public override void LoadData(TagCompound tag)
        {
            Ingredients = (List<Item>)tag.GetList<Item>("Items");
            Fullness = tag.GetInt("Fullness");
            FullnessMult = tag.GetInt("FullnessMult");
            WellFedMult = tag.GetInt("WellFedMult");
        }
    }
}