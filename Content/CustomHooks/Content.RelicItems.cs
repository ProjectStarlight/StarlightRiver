using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Content.Items.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
    class RelicItems : HookGroup
    {
        public override SafetyLevel Safety => SafetyLevel.Fragile; //gonna be really weird if anything does anything else here I think

        public override void Load()
        {
            IL.Terraria.Item.Prefix += ApplyTwice;
        }

        private void ApplyTwice(ILContext il) //this is horrid.
        {
            var c = new ILCursor(il);

            c.TryGotoNext(i => i.MatchStfld<Item>("crit"), i => i.MatchLdloc(2), i => i.MatchLdcI4(84));
            c.Index++;

            ILLabel label = il.DefineLabel(c.Next); //for when we need to skip later

            c.TryGotoPrev(i => i.MatchLdarg(0), i => i.MatchLdarg(0), i => i.MatchLdfld<Item>("damage"));

            c.Emit(OpCodes.Ldarg_0); //emits the values of the prefixes decided by vanilla, and the item instance
            c.Emit(OpCodes.Ldloc, 3);
            c.Emit(OpCodes.Ldloc, 5);
            c.Emit(OpCodes.Ldloc, 8);
            c.Emit(OpCodes.Ldloc, 4);
            c.Emit(OpCodes.Ldloc, 6);
            c.Emit(OpCodes.Ldloc, 7);
            c.Emit(OpCodes.Ldloc, 9);

            c.EmitDelegate<Func<Item, float, float, float, float, float, float, int, bool>>(ApplyTwiceBody); //funny!

            c.Emit(OpCodes.Brtrue, label); //skip vanilla stat setting if we do our own!
        }

        private bool ApplyTwiceBody(Item item, float damage, float speed, float mana, float knockBack, float scale, float shootSpeed, int crit)
        {
            if(item.GetGlobalItem<RelicItem>().isRelic)
            {
                item.damage = (int)Math.Round(item.damage * (damage + (damage - 1)));
                item.useAnimation = (int)Math.Round(item.useAnimation * (speed + (speed - 1)));
                item.useTime = (int)Math.Round(item.useTime * (speed + (speed - 1)));
                item.reuseDelay = (int)Math.Round(item.reuseDelay * (speed + (speed - 1)));
                item.mana = (int)Math.Round(item.mana * (mana + (mana - 1)));
                item.knockBack *= knockBack + (knockBack - 1);
                item.scale *= scale + (scale - 1);
                item.shootSpeed *= shootSpeed + (shootSpeed - 1);
                item.crit += crit * 2;

                return true;
            }

            return false;
        }
    }
}
