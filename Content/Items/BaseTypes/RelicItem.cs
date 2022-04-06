using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace StarlightRiver.Content.Items.BaseTypes
{
	class RelicItem : GlobalItem
    {
        public bool isRelic = false;

        public bool doubled = false;

        public override bool InstancePerEntity => true;

		//public override bool CloneNewInstances => true; //PORTTODO: Double check that commenting this out doesn't fuck everything up

		//public override bool NeedsSaving(Item Item) => isRelic; //PORTTODO: Double check that commenting this out doesn't fuck everything up

		public Color RelicColor(int offset) => Color.Lerp(Color.Yellow, Color.LimeGreen, 0.5f + (float)(Math.Sin(Main.GameUpdateCount / 20f + offset)) / 2f);
        public Color RelicColorBad(int offset) => Color.Lerp(Color.Yellow, Color.OrangeRed, 0.5f + (float)(Math.Sin(Main.GameUpdateCount / 20f + offset)) / 2f);

        public override bool? PrefixChance(Item Item, int pre, UnifiedRandom rand)
        {
            if (isRelic)
            {
                if (pre == -3)
                    return false;

                if (pre == -1)
                    return true;
            }

            return base.PrefixChance(Item, pre, rand);
        }

        public override int ChoosePrefix(Item Item, UnifiedRandom rand)
        {
            if (isRelic)
            {
                int result = base.ChoosePrefix(Item, rand);
                return result != 0 ? result : ChoosePrefix(Item, rand);
            }

            return base.ChoosePrefix(Item, rand);
        }

        public override void UpdateAccessory(Item Item, Player Player, bool hideVisual) //re-add vanilla prefixes to double power. This is bad, but its not IL atleast :)
        {
			if (!isRelic)
			{ 
				base.UpdateAccessory(Item, Player, hideVisual);
				return;
				}

			if (Item.prefix == 62)
			{
				Player.statDefense++;
			}
			if (Item.prefix == 63)
			{
				Player.statDefense += 2;
			}
			if (Item.prefix == 64)
			{
				Player.statDefense += 3;
			}
			if (Item.prefix == 65)
			{
				Player.statDefense += 4;
			}
			if (Item.prefix == 66)
			{
				Player.statManaMax2 += 20;
			}
			if (Item.prefix == 67)
			{
				Player.GetCritChance(DamageClass.Melee) += 2;
				Player.GetCritChance(DamageClass.Ranged) += 2;
				Player.GetCritChance(DamageClass.Magic) += 2;
				Player.GetCritChance(DamageClass.Throwing) += 2;
			}
			if (Item.prefix == 68)
			{
				Player.GetCritChance(DamageClass.Melee) += 4;
				Player.GetCritChance(DamageClass.Ranged) += 4;
				Player.GetCritChance(DamageClass.Magic) += 4;
				Player.GetCritChance(DamageClass.Throwing) += 4;
			}
			if (Item.prefix == 69)
			{
				Player.GetDamage(DamageClass.Generic) += 0.01f; 
			}
			if (Item.prefix == 70)
			{
				Player.GetDamage(DamageClass.Generic) += 0.02f;
			}
			if (Item.prefix == 71)
			{
				Player.GetDamage(DamageClass.Generic) += 0.03f;
			}
			if (Item.prefix == 72)
			{
				Player.GetDamage(DamageClass.Generic) += 0.04f;
			}
			if (Item.prefix == 73)
			{
				Player.moveSpeed += 0.01f;
			}
			if (Item.prefix == 74)
			{
				Player.moveSpeed += 0.02f;
			}
			if (Item.prefix == 75)
			{
				Player.moveSpeed += 0.03f;
			}
			if (Item.prefix == 76)
			{
				Player.moveSpeed += 0.04f;
			}
			if (Item.prefix == 77)
			{
				Player.meleeSpeed += 0.01f;
			}
			if (Item.prefix == 78)
			{
				Player.meleeSpeed += 0.02f;
			}
			if (Item.prefix == 79)
			{
				Player.meleeSpeed += 0.03f;
			}
			if (Item.prefix == 80)
			{
				Player.meleeSpeed += 0.04f;
			}
		}

        public override void ModifyTooltips(Item Item, List<TooltipLine> tooltips)
        {
            if (isRelic)
            {
                for(int k = 0; k < tooltips.Count; k++)
                {
                    var line = tooltips[k];

                    if (line.Name == "ItemName")
                    {
                        line.text = line.text.Insert(0, "Twice ");
                        line.overrideColor = RelicColor(k);
                    }

					if (line.isModifier)
					{
						line.overrideColor = RelicColor(k);

						if (Item.accessory)
							line.text = DoubleIntValues(line.text);
					}

                    if (line.isModifierBad)
                        line.overrideColor = RelicColorBad(k);
                }

                var newLine = new TooltipLine(Mod, "relicLine", "Cannot be reforged");
                newLine.overrideColor = new Color(255, 180, 100);
                tooltips.Add(newLine);
            }
        }

		public string DoubleIntValues(string input)
        {
			for (int k = 0; k < input.Length; k++)
			{
				int result = 0;
				if (int.TryParse(input[k].ToString(), out result))
				{
					input = input.Remove(k, 1);
					input = input.Insert(k, (result * 2).ToString());
				}
			}

			return input;
        }

        public override void SaveData(Item Item, TagCompound tag)
        {
			tag.Add("isRelic", isRelic); //PORTTODO: Make sure this isn't fucked up
		}

        public override void LoadData(Item Item, TagCompound tag)
        {
            isRelic = tag.GetBool("isRelic");
        }
    }
}
