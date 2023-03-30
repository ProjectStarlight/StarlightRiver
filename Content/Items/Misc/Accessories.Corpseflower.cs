using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Helpers;
using System;

namespace StarlightRiver.Content.Items.Misc
{
	public class Corpseflower : CursedAccessory
	{
		public int[] maxTimeLefts = new int[Main.maxCombatText];

		public override string Texture => AssetDirectory.MiscItem + Name;

		public Corpseflower() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "Corpseflower").Value) { }

		public override void Load()
		{
			StarlightPlayer.ModifyHitNPCEvent += ApplyDoTItems;
			StarlightPlayer.ModifyHitNPCWithProjEvent += ApplyDoTProjectiles;

			StarlightPlayer.OnHitNPCEvent += PostHitItems;
			StarlightPlayer.OnHitNPCWithProjEvent += PostHitProjectiles;

			On_CombatText.UpdateCombatText += CombatText_UpdateCombatText;
			Terraria.IL_NPC.UpdateNPC_BuffApplyDOTs += ChangeDoTColor;
		}

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("All damage dealt is converted into damage over time\nDamage is decreased by 66%\nYou are unable to critically strike while equipped");
		}

		#region IL
		private void ChangeDoTColor(MonoMod.Cil.ILContext il)
		{
			var c = new ILCursor(il);

			int indexLocal = il.MakeLocalVariable<int>();

			if (!c.TryGotoNext(MoveType.After, //move to after the vanilla combat text spawning
				i => i.MatchLdcI4(0),
				i => i.MatchLdcI4(1),
				i => i.MatchCall(typeof(CombatText).GetMethod(nameof(CombatText.NewText), new Type[] { typeof(Rectangle), typeof(Color), typeof(int), typeof(bool), typeof(bool) }))))
			{
				return;
			}

			c.Emit(OpCodes.Stloc, indexLocal); //store the index returned by CombatText.NewText

			c.Emit(OpCodes.Ldloc, indexLocal); //load the index to use as our first parameter in ApplyDoTColor
			c.Emit(OpCodes.Ldloc, 18); //18 is the whoAmI of the npc. Second parameter for our delegate.
			c.EmitDelegate(ApplyDoTColor);

			c.Emit(OpCodes.Ldloc, indexLocal); // push the indexLocal to the top of the stack to satisfy the stack state, since the pop call expects the return value of the CombatText call

			if (!c.TryGotoNext(MoveType.After, // Move after the SECOND vanilla combat text call.
				i => i.MatchLdcI4(0),
				i => i.MatchLdcI4(1),
				i => i.MatchCall(typeof(CombatText).GetMethod(nameof(CombatText.NewText), new Type[] { typeof(Rectangle), typeof(Color), typeof(int), typeof(bool), typeof(bool) }))))
			{
				return;
			}

			//same code as before
			c.Emit(OpCodes.Stloc, indexLocal); //store the index returned by CombatText.NewText

			c.Emit(OpCodes.Ldloc, indexLocal); //load the index to use as our first parameter in ApplyDoTColor
			c.Emit(OpCodes.Ldloc, 19); //19 is the whoAmI of the npc. Second parameter for our delegate.
			c.EmitDelegate(ApplyDoTColor);

			c.Emit(OpCodes.Ldloc, indexLocal); // push the indexLocal to the top of the stack to satisfy the stack state, since the pop call expects the return value of the CombatText call
		}

		private void ApplyDoTColor(int i, int whoAmI)
		{
			CorpseflowerBuff buff = InstancedBuffNPC.GetInstance<CorpseflowerBuff>(Main.npc[whoAmI]);
			if (buff is null || i < 0) // WEIRD ass bug with ceiros that caused index out of bounds. Tested with other enemies, just happens with ceiros. This seems to fix it tho 
				return;

			maxTimeLefts[i] = Main.combatText[i].lifeTime;
		}

		#endregion IL

		private void CombatText_UpdateCombatText(On_CombatText.orig_UpdateCombatText orig)
		{
			orig();

			for (int i = 0; i < Main.maxCombatText; i++)
			{
				CombatText text = Main.combatText[i];
				if (maxTimeLefts[i] > 0)
				{
					if (text.active)
					{
						text.color = Color.Lerp(Color.Purple, Color.LimeGreen, 1f - text.lifeTime / (float)maxTimeLefts[i]);
					}
					else
					{
						maxTimeLefts[i] = 0;
					}
				}
			}
		}

		private void ApplyDoTItems(Player player, Item Item, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Equipped(player))
				modifiers.HideCombatText();
		}

		private void ApplyDoTProjectiles(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Equipped(player))
				modifiers.HideCombatText();
		}

		private void PostHitItems(Player player, Item Item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Equipped(player))
			{
				BuffInflictor.InflictStack<CorpseflowerBuff, CorpseflowerStack>(target, 600, new CorpseflowerStack() { duration = 600, damage = (int)(damageDone * 0.33f) });
				target.life += damageDone;
			}
		}

		private void PostHitProjectiles(Player player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Equipped(player))
			{
				BuffInflictor.InflictStack<CorpseflowerBuff, CorpseflowerStack>(target, 600, new CorpseflowerStack() { duration = 600, damage = (int)(damageDone * 0.33f) });
				target.life += damageDone;
			}
		}
	}

	class CorpseflowerStack : BuffStack
	{
		public int damage;
	}

	class CorpseflowerBuff : StackableBuff<CorpseflowerStack>
	{
		public int totalDamage;
		public override string Name => "CorpseflowerBuff";

		public override string DisplayName => "Corpseflowered";

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override bool Debuff => true;

		public override string Tooltip => "You have been cursed by the corpeflower"; //idk man

		public override void Load()
		{
			StarlightNPC.UpdateLifeRegenEvent += StarlightNPC_UpdateLifeRegenEvent;
			StarlightNPC.ResetEffectsEvent += ResetDamage;
		}

		private void StarlightNPC_UpdateLifeRegenEvent(NPC npc, ref int damage)
		{
			if (AnyInflicted(npc))
			{
				if (damage < (GetInstance(npc) as CorpseflowerBuff).totalDamage * 0.33f)
					damage = (int)((GetInstance(npc) as CorpseflowerBuff).totalDamage * 0.33f);
			}
		}

		private void ResetDamage(NPC NPC)
		{
			if (AnyInflicted(NPC))
			{
				(GetInstance(NPC) as CorpseflowerBuff).totalDamage = 0;
			}
		}

		public override CorpseflowerStack GenerateDefaultStackTyped(int duration)
		{
			return new CorpseflowerStack()
			{
				duration = duration,
				damage = 1
			};
		}

		public override void PerStackEffectsNPC(NPC npc, CorpseflowerStack stack)
		{
			npc.lifeRegen -= stack.damage;
			totalDamage += stack.damage;
		}
	}
}