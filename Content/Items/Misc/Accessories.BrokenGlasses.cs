using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Helpers;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class BrokenGlasses : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;
		public BrokenGlasses() : base("Broken Glasses", "Damage over time effects are able to critically strike") { }

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += StarlightPlayer_OnHitNPCEvent;
			StarlightPlayer.OnHitNPCWithProjEvent += StarlightPlayer_OnHitNPCWithProjEvent;
		}

		private void StarlightPlayer_OnHitNPCWithProjEvent(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit)
		{
			if (Equipped(player))
				target.GetGlobalNPC<BrokenGlassesNPC>().lastPlayerHit = player.whoAmI;
		}

		private void StarlightPlayer_OnHitNPCEvent(Player player, Item Item, NPC target, int damage, float knockback, bool crit)
		{
			if (Equipped(player))
				target.GetGlobalNPC<BrokenGlassesNPC>().lastPlayerHit = player.whoAmI;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.White; //funny reference :)	
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Glass, 25);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	class BrokenGlassesNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public int lastPlayerHit;
	}

	class BrokenGlassesSystem : IOrderedLoadable
	{
		public float Priority => 1f;

		public void Load()
		{
			IL.Terraria.NPC.UpdateNPC_BuffApplyDOTs += InsertCrit;

		}

		public void Unload()
		{
			IL.Terraria.NPC.UpdateNPC_BuffApplyDOTs -= InsertCrit;
		}

		private static void InsertCrit(ILContext il)
		{
			ILCursor c = new(il);

			int critLocal = il.MakeLocalVariable<bool>(); //the local to store if we crit or not for this tick

			if (!c.TryGotoNext(MoveType.After, //move to after the vanilla combat text spawning
				i => i.MatchLdcI4(0),
				i => i.MatchLdcI4(1),
				i => i.MatchCall(typeof(CombatText).GetMethod(nameof(CombatText.NewText), new Type[] { typeof(Rectangle), typeof(Color), typeof(int), typeof(bool), typeof(bool) })),
				i => i.MatchPop()))
			{
				return;
			}

			ILLabel afterModded = il.DefineLabel(c.Next); //create a label at the next instruction -in vanilla- after this

			c.Emit(OpCodes.Ldloc, critLocal); //check if we should crit. This should come up when we reach this logic 'normally'
			c.Emit(OpCodes.Brfalse, afterModded); //if we dont want to do our logic because we didnt crit, skip over it and go back to the vanilla flow

			c.Emit(OpCodes.Ldloc, 0); //next we emit our logic for special combat text -before- that label. This should only run if we do crit
			c.Emit(OpCodes.Ldloc, 15);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate(DoCrit);

			c.Index -= 4;
			ILLabel afterVanillaBeforeModded = il.DefineLabel(c.Next); //generate a label to our modded logic that we jump to when appropriate

			//==============================================================================================================================================================================================================
			// !!! GOING UP !!! --------- This logic occurs before the previous logic in the vanilla IL! This is confusing to readers so im telling you now with ascii art to get your attention! --------- !!! GOING UP !!! 
			//==============================================================================================================================================================================================================

			if (!c.TryGotoPrev(  //now we go back up to before damage is calculated
				MoveType.Before,
				i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.npc))),
				i => i.MatchLdloc(15), //15 is the whoAmI of the npc
				i => i.MatchLdelemRef(),
				i => i.MatchLdfld(typeof(NPC).GetField(nameof(NPC.immortal)))))
			{
				return;
			}

			c.Index++;

			c.Emit(OpCodes.Ldloc, 15); //we roll if we should crit or not, and store that in a local
			c.Emit(OpCodes.Ldelem_Ref);
			c.EmitDelegate(Crit);
			c.Emit(OpCodes.Stloc, critLocal);

			c.Emit(OpCodes.Ldloc, critLocal); //then we check for a crit. If we did crit, we want to skip over the vanilla damage and combat text and go straight to our modded logic we emitter earlier
			c.Emit(OpCodes.Brtrue, afterVanillaBeforeModded);

			c.Emit(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.npc))); //this is here to satisfy the stack state
		}

		private static void DoCrit(int num, int whoAmI, NPC npc)
		{
			NPC realLifeNPC = Main.npc[whoAmI];
			if (!realLifeNPC.immortal)
				realLifeNPC.life -= num * 2;

			CombatText.NewText(new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height), Color.Yellow, num * 2 + "!", false, true);
		}

		private static bool Crit(NPC npc)
		{
			Player player = Main.player[npc.GetGlobalNPC<BrokenGlassesNPC>().lastPlayerHit];
			return Main.rand.NextFloat() < player.GetTotalCritChance(DamageClass.Generic) * 0.01f;
		}
	}
}
