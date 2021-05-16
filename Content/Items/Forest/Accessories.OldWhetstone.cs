using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Forest
{
    class OldWhetstone : SmartAccessory
    {
        public override string Texture => AssetDirectory.ForestItem + Name;

        public OldWhetstone() : base("Old Whetstone", "+1 to all damage\n'why in tarnation are you sharpening your wand?!'") { }

        public override void SafeSetDefaults() => item.rare = ItemRarityID.Blue;

        public override bool Autoload(ref string name)
        {
            StarlightItem.ModifyWeaponDamageEvent += AddDamage;
            return base.Autoload(ref name);
        }

		private void AddDamage(Item item, Player player, ref float add, ref float mult, ref float flat)
		{
            if (Equipped(player))
                flat += 1;
		}
	}
}
