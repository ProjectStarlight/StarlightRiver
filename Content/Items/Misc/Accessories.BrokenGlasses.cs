using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class BrokenGlasses : SmartAccessory
	{
		public override string Texture => AssetDirectory.Invisible;
		public BrokenGlasses() : base("Broken Glasses", "Damage over time effects are able to critically strike") { }

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += StarlightPlayer_OnHitNPCEvent;
			StarlightPlayer.OnHitNPCWithProjEvent += StarlightPlayer_OnHitNPCWithProjEvent;
		}

		private void StarlightPlayer_OnHitNPCWithProjEvent(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit)
		{
			if (Equipped(player))
				BrokenGlassesSystem.lastPlayerHit = player.whoAmI;
		}

		private void StarlightPlayer_OnHitNPCEvent(Player player, Item Item, NPC target, int damage, float knockback, bool crit)
		{
			if (Equipped(player))
				BrokenGlassesSystem.lastPlayerHit = player.whoAmI;
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

	class BrokenGlassesSystem : IOrderedLoadable
	{
		public float Priority => 1f;

		public static int lastPlayerHit;

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
			if (lastPlayerHit < 0)
				return;

			ILCursor c = new(il);

			ILLabel label = il.DefineLabel();

			int crit = il.MakeLocalVariable<bool>();

			c.EmitDelegate(Crit);
			c.Emit(OpCodes.Stloc, crit);

			int num = 0;

			c.TryGotoNext(MoveType.Before, x => x.MatchLdloca(out num));

			int whoAmI = 0;

			c.TryGotoNext(MoveType.Before, x => x.MatchStloc(out whoAmI));

			c.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(CombatText).GetMethod(nameof(CombatText.NewText))));

			c.MarkLabel(label); // crit label

			c.Emit(OpCodes.Ldloc, label);
			c.Emit(OpCodes.Brtrue_S, crit);

			c.Emit(OpCodes.Ldloc, num);
			c.Emit(OpCodes.Ldloc, whoAmI);
			c.Emit(OpCodes.Call, typeof(BrokenGlassesSystem).GetMethod(nameof(DoCrit), BindingFlags.NonPublic | BindingFlags.Static));
		}

		private static void DoCrit(int num, int whoAmI)
		{
			NPC npc = Main.npc[whoAmI];
			if (!npc.immortal)
			{
				npc.life -= num * 2;
			}

			CombatText.NewText(new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height), Color.Yellow, num * 2, false, true);
		}
		
		private static bool Crit()
		{
			return Main.rand.NextFloat() < Main.player[lastPlayerHit].GetTotalCritChance(DamageClass.Generic) * 0.01f;
		}
	}
}
