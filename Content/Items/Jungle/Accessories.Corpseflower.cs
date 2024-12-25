using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Helpers;
using System;
using System.IO;
using System.Reflection;
using Terraria.ID;
using Terraria.Localization;
using static Humanizer.In;

namespace StarlightRiver.Content.Items.Jungle
{
	public class Corpseflower : CursedAccessory
	{
		public static int[] maxTimeLefts = new int[Main.maxCombatText];

		public override string Texture => AssetDirectory.JungleItem + Name;

		private static bool skipSendData = false;

		public override void Load()
		{
			StarlightPlayer.ModifyHitNPCEvent += ApplyDoTItems;
			StarlightPlayer.ModifyHitNPCWithProjEvent += ApplyDoTProjectiles;

			StarlightPlayer.OnHitNPCEvent += PostHitItems;
			StarlightPlayer.OnHitNPCWithProjEvent += PostHitProjectiles;

			On_CombatText.UpdateCombatText += CombatText_UpdateCombatText;
			IL_NPC.UpdateNPC_BuffApplyDOTs += ChangeDoTColor;

			On_NetMessage.SendData += SendDataOverrideHack;
		}

		public override void Unload()
		{
			On_CombatText.UpdateCombatText -= CombatText_UpdateCombatText;
			IL_NPC.UpdateNPC_BuffApplyDOTs -= ChangeDoTColor;
			On_NetMessage.SendData -= SendDataOverrideHack;
		}

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("All damage dealt is converted into stacks of {{BUFF:CorpseflowerBuff}}\nBase damage is decreased\nYou are unable to critically strike");

			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.NaturesGift;
			ItemID.Sets.ShimmerTransformToItem[ItemID.NaturesGift] = Type;
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(silver: 54);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.NaturesGift);
			recipe.AddIngredient(ItemID.ShadowScale, 5);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.NaturesGift);
			recipe.AddIngredient(ItemID.TissueSample, 5);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}

		#region IL
		private void ChangeDoTColor(ILContext il)
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
			if (buff is null || i < 0 || Main.dedServ) // WEIRD ass bug with ceiros that caused index out of bounds. Tested with other enemies, just happens with ceiros. This seems to fix it tho 
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
			{
				modifiers.HideCombatText();
				modifiers.DisableCrit();

				if (Main.netMode == NetmodeID.MultiplayerClient) //TODO: rework this too
					modifiers.Knockback *= 0; //not ideal but this is just since mp doesn't send regular hit packets so that it doesn't appear rubberbanding we prevent local knockback too
			}
		}

		private void ApplyDoTProjectiles(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Equipped(player))
			{
				modifiers.HideCombatText(); //it looks like these doesn't actually work possibly reach out to collaborators?
				modifiers.DisableCrit();

				if (Main.netMode == NetmodeID.MultiplayerClient) //TODO: rework this too
					modifiers.Knockback *= 0;//not ideal but this is just since mp doesn't send regular hit packets so that it doesn't appear rubberbanding we prevent local knockback too
			}
		}

		private void PostHitItems(Player player, Item Item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Equipped(player))
			{
				BuffInflictor.InflictStack<CorpseflowerBuff, CorpseflowerStack>(target, 600, new CorpseflowerStack() { duration = 600, damage = Utils.Clamp((int)(damageDone * 0.45f), 1, damageDone) });

				if (Main.myPlayer == player.whoAmI)
				{
					target.life += damageDone;
					skipSendData = true;
				}

				player.TryGetModPlayer(out StarlightPlayer sp);
				sp.SetHitPacketStatus(false);
			}
		}

		private void PostHitProjectiles(Player player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Equipped(player))
			{
				BuffInflictor.InflictStack<CorpseflowerBuff, CorpseflowerStack>(target, 600, new CorpseflowerStack() { duration = 600, damage = Utils.Clamp((int)(damageDone * 0.45f), 1, damageDone) });

				if (Main.myPlayer == player.whoAmI)
				{
					target.life += damageDone;
					skipSendData = true;
				}

				player.TryGetModPlayer(out StarlightPlayer sp);
				sp.SetHitPacketStatus(false);
			}
		}

		private void SendDataOverrideHack(On_NetMessage.orig_SendData orig, int msgType, int remoteClient = -1, int ignoreClient = -1, NetworkText text = null, int number = 0, float number2 = 0f, float number3 = 0f, float number4 = 0f, int number5 = 0, int number6 = 0, int number7 = 0)
		{
			//SYNC TODO: this could use a full rework for mp compatibility since this is vile
			if (msgType == MessageID.DamageNPC && Equipped(Main.LocalPlayer) && skipSendData)
			{
				skipSendData = false;
				return; //horrifying hack to avoid sending regular hitpackets while using corpseflower since those could accidentally kill the NPC on other clients / server
			}
			else
			{
				orig(msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
			}
		}
	}

	class CorpseflowerStack : BuffStack
	{
		public int damage;

		public override void SendCustomData(BinaryWriter writer)
		{
			writer.Write(damage);
		}

		public override void ReceiveCustomData(BinaryReader reader)
		{
			damage = reader.ReadInt32();
		}
	}

	class CorpseflowerBuff : StackableBuff<CorpseflowerStack>
	{
		public int totalDamage;
		public override string Name => "CorpseflowerBuff";

		public override string DisplayName => "Corpseflower Curse";

		public override string Texture => AssetDirectory.JungleItem + Name;

		public override bool Debuff => true;

		public override string Tooltip => "Deals damage over time based on the damage of the hit inflicting it";

		public override void SafeLoad()
		{
			StarlightNPC.UpdateLifeRegenEvent += StarlightNPC_UpdateLifeRegenEvent;
			StarlightNPC.ResetEffectsEvent += ResetDamage;
		}

		private void StarlightNPC_UpdateLifeRegenEvent(NPC npc, ref int damage)
		{
			InstancedBuff buff = GetInstance(npc);
			if (buff != null)
			{
				if (damage < (buff as CorpseflowerBuff).totalDamage * 0.1f)
					damage = (int)((buff as CorpseflowerBuff).totalDamage * 0.1f);
			}
		}

		private void ResetDamage(NPC npc)
		{

			InstancedBuff buff = GetInstance(npc);
			if (buff != null)
			{
				(buff as CorpseflowerBuff).totalDamage = 0;
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