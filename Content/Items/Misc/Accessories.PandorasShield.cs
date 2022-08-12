using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Potions;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    public class PandorasShield : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public PandorasShield() : base("Pandora's Shield","Grazes grant a portion of the projectiles' damage as barrier"){}

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 1);
        }
        public override void SafeUpdateEquip(Player Player)
        {
            var gp = Player.GetModPlayer<GrazePlayer>();
            gp.doGrazeLogic = true;
            
            if (gp.justGrazed)
            {
                float scale = 1f;
                if (Main.expertMode)
                    scale = 2f;
                if (Main.masterMode)
                    scale = 3f;

                int amount = (int)(gp.lastGrazeDamage * scale);
                Player.AddBuff(ModContent.BuffType<ShieldDegenReduction>(), 180 + (amount * 2));
                Player.GetModPlayer<BarrierPlayer>().Barrier += amount;
                CombatText.NewText(Player.Hitbox, new Color(150, 255, 255), amount);
            }
        }
    }
}
