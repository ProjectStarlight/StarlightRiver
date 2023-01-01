using System;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.ArmorEnchantment
{
	abstract class ArmorEnchantment
	{
		public Guid Guid;

		public virtual Color Color => Color.White;

		public virtual string Texture => AssetDirectory.Debug;

		public ArmorEnchantment()
		{
			Guid = Guid.Empty;
		}

		public ArmorEnchantment(Guid guid)
		{
			Guid = guid;
		}

		public virtual bool IsAvailable(Item head, Item chest, Item legs)
		{
			return false;
		}

		public virtual void UpdateSet(Player Player)
		{

		}

		public virtual void DrawInInventory(Item Item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{

		}

		public virtual bool PreDrawInInventory(Item Item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			return true;
		}

		public virtual TagCompound SaveData()
		{
			return new TagCompound()
			{

			};
		}

		public virtual void LoadData(TagCompound tag)
		{

		}

		public sealed override bool Equals(object obj)
		{
			if (obj is ArmorEnchantment)
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

		public static void EnchantArmor(Item Item, ArmorEnchantment enchant)
		{
			Item.GetGlobalItem<EnchantedArmorGlobalItem>().Enchantment = enchant;
		}

		public ArmorEnchantment MakeRealCopy() //this is dumb and there is a better way to do this probably but i really dont feel like figuring it out rn
		{
			Type type = GetType();
			object copy = MemberwiseClone();
			var realCopy = (ArmorEnchantment)Convert.ChangeType(copy, type);
			realCopy.Guid = Guid.NewGuid();

			return realCopy;
		}
	}
}
