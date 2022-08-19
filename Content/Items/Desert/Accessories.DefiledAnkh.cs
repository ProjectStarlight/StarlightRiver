using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ID;
using StarlightRiver.Content.Items.Potions;

namespace StarlightRiver.Content.Items.Desert
{
    public class DefiledAnkh : CursedAccessory
    {
        public override string Texture => AssetDirectory.DesertItem + Name;
        public DefiledAnkh() : base(ModContent.Request<Texture2D>(AssetDirectory.DesertItem + "DefiledAnkh").Value) { }

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Cursed : Your Barrier protects against twenty-five percent less damage\nYou cannot gain any debuff besides Potion, Barrier and Mana Sickness while Barrier is active" +
                "\n+40 max Barrier");
        }

        public override void SafeUpdateEquip(Player player)
        {
            var bp = player.GetModPlayer<BarrierPlayer>();
            bp.BarrierDamageReduction -= 0.25f;
            bp.MaxBarrier += 40;

            if (bp.Barrier > 0) // I couldnt figure out a way to do this besides this way.
            {
                for (int i = 0; i < Main.maxBuffTypes; i++) //vanilla debuffs
                {
                    int buffType = i;
                    if (buffType != BuffID.PotionSickness || buffType != BuffID.ManaSickness)
                        if (Main.debuff[buffType])
                            player.buffImmune[buffType] = true;
                }

                for (int i = Main.maxBuffTypes; i < BuffLoader.BuffCount; i++) //modded debuffs, everything after Main.maxBuffTypes is modded
                {
                    int buffType = i;
                    if (buffType != ModContent.BuffType<NoShieldPot>())
                        if (Main.debuff[buffType])
                            player.buffImmune[buffType] = true;
                }
            }
        }
    }
}
