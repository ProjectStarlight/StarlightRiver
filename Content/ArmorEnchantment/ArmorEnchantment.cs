using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;

namespace StarlightRiver.Content.ArmorEnchantment
{
    abstract class ArmorEnchantment
    {
        public Guid Guid;

        public virtual Color Color => Color.White;

        public virtual string Texture => AssetDirectory.Debug;

        public ArmorEnchantment() => Guid = Guid.Empty;

        public ArmorEnchantment(Guid guid)
        {
            Guid = guid;
        }

        public virtual bool IsAvailable(Item head, Item chest, Item legs)
        {
            return false;
        }

        public virtual void UpdateSet(Player player)
        {

        }

        public virtual void DrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {

        }

        public virtual bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            return true;
        }

        public virtual TagCompound Save()
        {
            return new TagCompound()
            {

            };
        }

        public virtual void Load(TagCompound tag)
        {

        }

        public sealed override bool Equals(object obj)
        {
            if(obj is ArmorEnchantment)
            {
                var a = obj as ArmorEnchantment;

                if (a.Guid == Guid) 
                    return true;
            }

            return false;
        }

        public static void EnchantArmor(Item head, Item chest, Item legs, ArmorEnchantment enchant)
        {
            EnchantArmor(head, enchant);
            EnchantArmor(chest, enchant);
            EnchantArmor(legs, enchant);
        }

        public static void EnchantArmor(Item item, ArmorEnchantment enchant)
        {
            item.GetGlobalItem<EnchantedArmorGlobalItem>().Enchantment = enchant;
        }

        public ArmorEnchantment MakeRealCopy() //this is dumb and there is a better way to do this probably but i really dont feel like figuring it out rn
        {
            var type = GetType();
            var copy = MemberwiseClone();
            ArmorEnchantment realCopy = (ArmorEnchantment)Convert.ChangeType(copy, type);
            realCopy.Guid = Guid.NewGuid();

            return realCopy;
        }
    }
}
