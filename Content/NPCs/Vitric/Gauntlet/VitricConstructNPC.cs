using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core.Systems.BarrierSystem;
using System.IO;
using Terraria.GameContent.ItemDropRules;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
	public abstract class VitricConstructNPC : ModNPC
	{
		public bool partOfGauntlet = false;

		public bool ableToDoCombo = true;

		public int healingCounter = 0;//Counts down from 5 if the enemy isn't being healed

		public virtual string PreviewTexturePath => Texture + "_Preview";

		public virtual string PreviewTextureGlowmaskPath => Texture + "_Preview_Glow";

		public virtual Vector2 PreviewOffset => Vector2.Zero;

		public virtual void DrawHealingGlow(SpriteBatch spriteBatch) { }

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.ByCondition(new TempleCondition(), ItemType<TempleKey>(), 4));
		}

		public override void AI()
		{
			SafeAI();
			BarrierNPC barrierNPC = NPC.GetGlobalNPC<BarrierNPC>();

			if (healingCounter > 0)
			{
				healingCounter--;
				barrierNPC.maxBarrier = 120;
				barrierNPC.rechargeRate = 70;
			}
			else
			{
				barrierNPC.maxBarrier = 0;
				barrierNPC.rechargeRate = 0;
			}
		}

		public virtual void SafeAI() { }

		public sealed override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(partOfGauntlet);

			SafeSendExtraAI(writer);
		}

		public virtual void SafeSendExtraAI(BinaryWriter writer) { }

		public sealed override void ReceiveExtraAI(BinaryReader reader)
		{
			partOfGauntlet = reader.ReadBoolean();

			SafeReceiveExtraAI(reader);
		}

		public virtual void SafeReceiveExtraAI(BinaryReader reader) { }
	}
}