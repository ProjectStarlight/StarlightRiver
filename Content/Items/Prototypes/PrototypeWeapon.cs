using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Items.Prototypes
{
    internal enum BreakType : int
    {
        MaxUses = 0, //the weapon can only be used as many times as durability it has
        Time = 1, //the weapon can only be used for as many ticks as durability it has
        MaxDamage = 2 //the weapon can only be used before it has delt the amount of damage as durability it has
    }

    internal class PrototypeWeapon : ModItem
    {
        public int Durability { get; set; } //how many on the appropriate degradation factor the prototype can withstand before breaking
        public int MaxDurability { get; set; } //the maximum durability, for the purpose of calculting durability bars
        public BreakType Breaktype { get; set; } //the restriction for the weapons usage, see above enumerator

        public PrototypeWeapon(int durability, BreakType breaktype)
        {
            MaxDurability = durability;
            Durability = durability;
            Breaktype = breaktype;
        }

        public override bool CloneNewInstances => true; //allows dynamic tooltips and for multiple of the same prototype to coexist with different durabilities

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string text = "";
            text += Breaktype != BreakType.Time ? "" + Durability : Helper.TicksToTime(Durability); //adds the timer or counter
            switch (Breaktype) //adds the flavor text for the appropriate breaking method
            {
                case (BreakType.MaxUses): text += " Uses Left"; break;
                case (BreakType.Time): text += " Left"; break;
                case (BreakType.MaxDamage): text += " Damage Left"; break;
            }
            TooltipLine line = new TooltipLine(mod, "PrototypeInfo", text)
            {
                overrideColor = new Color(255, 200, 100)
            };
            tooltips.Add(line);
        }

        public virtual bool SafeUseItem(Player player)
        {
            return true;
        } //allows on-use effects without breaking the prototype behavior

        public sealed override bool UseItem(Player player) //reduces durability on use if the breaking method of the prototype is limited uses
        {
            if (Breaktype == BreakType.MaxUses) Durability--;
            if (Durability == 0) BreakItem(player.Center); //destroys the item on it's last use
            return SafeUseItem(player);
        }

        public virtual void SafeUpdateInventory(Player player)
        {
        } //allows in-inventory effects without breaking the prototype behavior

        public sealed override void UpdateInventory(Player player) //reduces durability every tick in the player's inventory if of the appropriate breaking method
        {
            if (Breaktype == BreakType.Time) Durability--;
            if (Durability == 0) BreakItem(player.Center); //destroys the item when time is up, if in the inventory
            SafeUpdateInventory(player);
        }

        public sealed override void PostUpdate() //reduces durability every tick in the world if of the appropriate breaking method
        {
            if (Breaktype == BreakType.Time) Durability--;
            if (Durability == 0) BreakItem(item.Center); //destroys the item when time is up, if on the ground
        }

        public void BreakItem(Vector2 spawnpos)
        {
            Main.NewText("Your " + item.Name + " broke...", new Color(255, 200, 100));
            Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode, spawnpos);
            for (int k = 0; k <= 40; k++)
            {
                Dust.NewDustPerfect(spawnpos, DustType<Content.Dusts.Stone>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5));
                Dust.NewDustPerfect(spawnpos, 133, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10));
            }
            item.TurnToAir();
        }
    }
}