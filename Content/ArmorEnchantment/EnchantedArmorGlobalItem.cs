using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.ArmorEnchantment
{
	class EnchantedArmorGlobalItem : GlobalItem
    {
        public ArmorEnchantment Enchantment { get; set; }

        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

        public override bool NeedsSaving(Item Item) => Enchantment != null;

        public override void PostDrawInInventory(Item Item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
        {
            Enchantment?.DrawInInventory(Item, spriteBatch, position, frame, drawColor, ItemColor, origin, scale);
        }

        public override bool PreDrawInInventory(Item Item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
        {
            if (Enchantment is null) return base.PreDrawInInventory(Item, spriteBatch, position, frame, drawColor, ItemColor, origin, scale);
            return Enchantment.PreDrawInInventory(Item, spriteBatch, position, frame, drawColor, ItemColor, origin, scale);
        }

        public override string IsArmorSet(Item head, Item body, Item legs)
        {
            if (GetEnchant(head) != null && //so unenchanted Items are unaffected
                GetEnchant(body) != null &&
                GetEnchant(legs) != null &&
                GetEnchant(head).Equals(GetEnchant(body)) && 
                GetEnchant(head).Equals(GetEnchant(legs)))

                return "CurrentEnchant";

            return "";
        }

        public override void UpdateArmorSet(Player Player, string set)
        {
            var Enchantment = Player.armor[0].GetGlobalItem<EnchantedArmorGlobalItem>().Enchantment;

            if(set == "CurrentEnchant" && Enchantment != null)
            {
                Enchantment.UpdateSet(Player);
            }
        }

        public override TagCompound Save(Item Item)
        {
            return new TagCompound()
            {
                ["EnchantGuid"] = Enchantment.Guid.ToString(),
                ["EnchantType"] = Enchantment.GetType().FullName,
                ["Enchantment"] = Enchantment.Save()
            };
        }

        public override void Load(Item Item, TagCompound tag)
        {
            var guid = Guid.ParseExact(tag.GetString("EnchantGuid"), "D");
            var type = tag.GetString("EnchantType");

            if (guid != null && type != null)
            {
                var a = Assembly.GetAssembly(this.GetType()).GetType(type);

                Enchantment = (ArmorEnchantment)Activator.CreateInstance
                    (
                        a,
                        new object[] { guid }
                    );

                Enchantment.Load(tag.GetCompound("Enchantment"));
            }
        }

        public static ArmorEnchantment GetEnchant(Item Item)
        {
            if (Item.IsAir) 
                return null;


            var globalItem = Item.GetGlobalItem<EnchantedArmorGlobalItem>();
            
            if (globalItem is null)
                return null;

            return globalItem.Enchantment;
        }
    }
}
