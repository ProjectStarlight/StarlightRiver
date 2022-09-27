//TODO:
//Make scroll of undoing more common artifact
//Make rune of undoing rarer
//Make scroll of undoing found in chests
//Make rune of undoing have an animation

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace StarlightRiver.Content.Items.Utility
{
	class RuneOfUndoing : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/Utility/" + Name;

		public static ParticleSystem ShardsSystem;

		private Vector2 drawPos = Vector2.Zero;

        public override void Load()
        {
			ShardsSystem = new ParticleSystem("StarlightRiver/Assets/Items/Utility/RuneOfUndoing", CursedAccessory.UpdateShards);
		}

        public override void Unload()
        {
			ShardsSystem = null;
		}

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Rune of Undoing");
			Tooltip.SetDefault("Place over an equipped cursed Item to take it off \nThis consumes the rune");
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 1;
			Item.rare = ItemRarityID.LightRed;
			Item.accessory = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Remove(tooltips.FirstOrDefault(n => n.Mod == "Terraria" && n.Name == "Equipable"));
		}

		public override bool CanEquipAccessory(Player Player, int slot, bool modded)
		{
			if (Player.armor[slot].ModItem is CursedAccessory && slot <= (Main.masterMode ? 9 : 8) + Player.extraAccessorySlots && Main.mouseLeft)
			{
				Main.mouseItem = Player.armor[slot].Clone();
				Player.armor[slot].TurnToAir();

				SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/Shadow2"));
				Helpers.Helper.TurnToShards(ShardsSystem, Item, drawPos);
			}

			return false;
		}

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
			drawPos = position + frame.Size() / 4;
		}
    }
}
