using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Content.Items.BaseTypes;
using System;

namespace StarlightRiver.Content.CustomHooks
{
	class RelicItems : HookGroup
	{
		//gonna be really weird if anything does anything else here I think
		public override void Load()
		{
			IL.Terraria.Item.Prefix += ApplyTwice;
		}

		private void ApplyTwice(ILContext il) //this is horrid.
		{
			var c = new ILCursor(il);

			c.TryGotoNext(i => i.MatchStfld<Item>("crit"), i => i.MatchLdloc(2), i => i.MatchLdcI4(85));
			c.Index++;

			ILLabel label = il.DefineLabel(c.Next); //for when we need to skip later

			c.TryGotoPrev(i => i.MatchLdarg(0), i => i.MatchLdarg(0), i => i.MatchLdfld<Item>("damage"));

			c.Emit(OpCodes.Ldarg_0); //emits the values of the prefixes decided by vanilla, and the Item instance
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

		private bool ApplyTwiceBody(Item Item, float damage, float speed, float mana, float knockBack, float scale, float shootSpeed, int crit)
		{
			if (Item.GetGlobalItem<RelicItem>().isRelic)
			{
				Item.damage = (int)Math.Round(Item.damage * (damage + (damage - 1)));
				Item.useAnimation = (int)Math.Round(Item.useAnimation * (speed + (speed - 1)));
				Item.useTime = (int)Math.Round(Item.useTime * (speed + (speed - 1)));
				Item.reuseDelay = (int)Math.Round(Item.reuseDelay * (speed + (speed - 1)));
				Item.mana = (int)Math.Round(Item.mana * (mana + (mana - 1)));
				Item.knockBack *= knockBack + (knockBack - 1);
				Item.scale *= scale + (scale - 1);
				Item.shootSpeed *= shootSpeed + (shootSpeed - 1);
				Item.crit += crit * 2;

				return true;
			}

			return false;
		}
	}
}