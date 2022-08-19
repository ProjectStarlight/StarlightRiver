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

        public override void Load()
        {
            StarlightPlayer.CanUseItemEvent += PreventManaPotion;
            StarlightPlayer.OnHitNPCEvent += ManaLeechOnHit;
            StarlightPlayer.OnHitNPCWithProjEvent += ManaLeechOnHitProj;
        }

        public override void Unload()
        {
            StarlightPlayer.CanUseItemEvent -= PreventManaPotion;
            StarlightPlayer.OnHitNPCEvent -= ManaLeechOnHit;
            StarlightPlayer.OnHitNPCWithProjEvent -= ManaLeechOnHitProj;
        }

        private void ManaLeechOnHitProj(Player Player, Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (proj.DamageType == DamageClass.Magic && Equipped(Player) && Main.rand.NextFloat() < 0.25f)
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

        private void ManaLeechOnHit(Player player, Item Item, NPC target, int damage, float knockback, bool crit)
        {
            if (Item.DamageType == DamageClass.Magic && Equipped(player) && Main.rand.NextFloat() < 0.25f)
            {
                double decay = Math.Pow(1 * (1 - 0.02f), damage);
                decay = Math.Clamp(decay, 0.185f, 1);
                int manaAmount = (int)(damage * decay);

                player.ManaEffect(manaAmount);

                player.statMana += manaAmount;
                if (player.statMana > player.statManaMax2)
                    player.statMana = player.statManaMax2;

                NetMessage.SendData(MessageID.ManaEffect, -1, -1, null, player.whoAmI, manaAmount); // I think this is what im supposed to do for mana heal idk
            }
        }

        private bool PreventManaPotion(Player player, Item item)
        {
            if (Equipped(player) && item.healMana > 0)
                return false;

            return true;
        }

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Mana replenishing items cannot be used\nMagic attacks have a twenty-five percent chance to leech a large portion of damage as mana on hit");
        }
    }
}
