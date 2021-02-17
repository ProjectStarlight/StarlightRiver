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

        public override bool NeedsSaving(Item item) => Enchantment != null;

        public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Enchantment?.DrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Enchantment is null) return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
            return Enchantment.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }

        public override string IsArmorSet(Item head, Item body, Item legs)
        {
            if (GetEnchant(head) != null && //so unenchanted items are unaffected
                GetEnchant(body) != null &&
                GetEnchant(legs) != null &&
                GetEnchant(head).Equals(GetEnchant(body)) && 
                GetEnchant(head).Equals(GetEnchant(legs)))

                return "CurrentEnchant";

            return "";
        }

        public override void UpdateArmorSet(Player player, string set)
        {
            var Enchantment = player.armor[0].GetGlobalItem<EnchantedArmorGlobalItem>().Enchantment;

            if(set == "CurrentEnchant" && Enchantment != null)
            {
                Enchantment.UpdateSet(player);
            }
        }

        public override TagCompound Save(Item item)
        {
            return new TagCompound()
            {
                ["EnchantGuid"] = Enchantment.Guid.ToString(),
                ["EnchantType"] = Enchantment.GetType().FullName,
                ["Enchantment"] = Enchantment.Save()
            };
        }

        public override void Load(Item item, TagCompound tag)
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

        public static ArmorEnchantment GetEnchant(Item item)
        {
            if (item.IsAir) 
                return null;


            var globalItem = item.GetGlobalItem<EnchantedArmorGlobalItem>();
            
            if (globalItem is null)
                return null;

            return globalItem.Enchantment;
        }
    }
}
