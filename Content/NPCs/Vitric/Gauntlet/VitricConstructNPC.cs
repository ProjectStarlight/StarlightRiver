using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
	public abstract class VitricConstructNPC : ModNPC
	{
		public bool ableToDoCombo = true;

		public int healingCounter = 0;//Counts down from 5 if the enemy isn't being healed

		public virtual string previewTexturePath => Texture + "_Preview";

		public virtual string previewTextureGlowmaskPath => Texture + "_Preview_Glow";

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
				barrierNPC.MaxBarrier = 120;
				barrierNPC.RechargeRate = 70;
			}
			else
			{
				barrierNPC.MaxBarrier = 0;
				barrierNPC.RechargeRate = 0;
			}
		}

		public virtual void SafeAI() { }
	}
}