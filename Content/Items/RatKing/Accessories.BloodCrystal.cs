using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.RatKing
{
	class BloodCrystal : CursedAccessory
	{
		public override string Texture => AssetDirectory.RatKingItem + Name;

		public BloodCrystal() : base(ModContent.GetTexture(AssetDirectory.RatKingItem + "BloodCrystal")) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blood Crystal");
			Tooltip.SetDefault("Debuffs you inflict are inflicted onto you \nAll debuffs you inflict stack and last longer");
		}


        public override void SafeSetDefaults()
        {
			item.value = Item.sellPrice(0, 3, 0, 0);
        }
        public override void SafeUpdateEquip(Player player)
		{
			player.GetModPlayer<BloodCrystalPlayer>().equipped = true;
		}
	}

	class BloodCrystalComparer : IOrderedLoadable
    {
		public float Priority => 1.4f;

		public void Load()
        {
			StatusTrackingNPC.buffCompareEffects += buffCompareEffects;
		}


		public void Unload()
        {
			StatusTrackingNPC.buffCompareEffects -= buffCompareEffects;
		}

		private void buffCompareEffects(Player player, NPC npc, int[] storedBuffs, int[] buffType, int[]storedTimes, int[]buffTime)
        {
			if (player.GetModPlayer<BloodCrystalPlayer>().equipped)
			{
				for (int i = 0; i < 5; i++)
                {
					if (!storedBuffs.Contains(buffType[i]))
                    {
						player.AddBuff(buffType[i], buffTime[i] / 4);
						buffTime[i] = (int)(buffTime[i] * 1.25f);
					}
					else
                    {
						int storedIndex;
						for (storedIndex = 0; storedIndex < 5; storedIndex++)
						{
							if (storedBuffs[storedIndex] == buffType[i])
								break;
						}

						if (storedTimes[storedIndex] != buffTime[i] && storedTimes[storedIndex] - 1 != buffTime[i])
                        {
							buffTime[i] = storedTimes[storedIndex] + (int)(buffTime[i] * 0.25f);
							player.AddBuff(buffType[i], buffTime[i] / 4);
						}

					}
                }
			}
		}
	}

	public class BloodCrystalPlayer : ModPlayer
    {
		public bool equipped = false;

        public override void ResetEffects()
        {
			equipped = false;
        }
    }
}
