using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
    public class Magebane : CursedAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public Magebane() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "Magebane").Value) {}

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Mana replenishing items cannot be used\nMagic attacks have a twenty-five percent chance to leech a large portion of damage as mana on hit");
        }

        public override void SafeUpdateEquip(Player Player)
        {
            Player.GetModPlayer<MagebaneModPlayer>().equipped = true;
        }
    }

    class MagebaneModPlayer : ModPlayer
    {
        public bool equipped;

        public override void ResetEffects()
        {
            equipped = false;
        }

        public override bool CanUseItem(Item item)
        {
            if (equipped && item.healMana > 0)
                return false;

            return base.CanUseItem(item);
        }
        //there arent any vanilla magic weapons that dont use projectiles but just in case a mod adds one
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
           if (item.DamageType == DamageClass.Magic && equipped && Main.rand.NextFloat() < 0.25f)
           {
                double decay = Math.Pow(1 * (1 - 0.02f), damage);
                decay = Math.Clamp(decay, 0.185f, 1);
                int manaAmount = (int)(damage * decay);

                Player.ManaEffect(manaAmount);

                Player.statMana += manaAmount;
                if (Player.statMana > Player.statManaMax2)
                    Player.statMana = Player.statManaMax2;

                NetMessage.SendData(MessageID.ManaEffect, -1, -1, null, Player.whoAmI, manaAmount); // I think this is what im supposed to do for mana heal idk
           }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (proj.DamageType == DamageClass.Magic && equipped && Main.rand.NextFloat() < 0.25f)
            {
                double decay = Math.Pow(1 * (1 - 0.02f), damage);
                decay = Math.Clamp(decay, 0.185f, 1);
                int manaAmount = (int)(damage * decay);

                Player.ManaEffect(manaAmount);

                Player.statMana += manaAmount;
                if (Player.statMana > Player.statManaMax2)
                    Player.statMana = Player.statManaMax2;

                NetMessage.SendData(MessageID.ManaEffect, -1, -1, null, Player.whoAmI, manaAmount);
            }
        }
    }
}
