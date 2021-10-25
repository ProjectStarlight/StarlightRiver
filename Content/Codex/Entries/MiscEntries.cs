using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Core;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Codex.Entries
{
	internal class StaminaEntry : CodexEntry
    {
        public StaminaEntry()
        {
            Category = Categories.Misc;
            Title = "Stamina";
            Body = "All of your abilities utilize stamina, a resource represented by the orange diamonds next to your mana bar. Your stamina will replenish itself over time. You can gain additional stamina by unlocking new abilities and having certain equipment.";
            Hint = "Unlock an ability...";
            Image = GetTexture("StarlightRiver/Assets/GUI/Stamina");
            Icon = GetTexture("StarlightRiver/Assets/GUI/Stamina");
        }
    }

    internal class StaminaShardEntry : CodexEntry
    {
        public StaminaShardEntry()
        {
            Category = Categories.Misc;
            Title = "Stamina Vessels";
            Body = "Shards of these intricate metal devices seem to be scattered around the world everywhere. By combining 3 of them, a completed stamina vessel can be made, allowing it's holder to store an extra unit of stamina. Unfortunately, the strange material and intricate tooling of these fragments makes it nigh impossible to re-create them on your own.";
            Hint = "Find a stamina vessel shard...";
            Image = GetTexture("StarlightRiver/Assets/GUI/StaminaEmpty");
            Icon = GetTexture("StarlightRiver/Assets/Abilities/Stamina1");
        }
    }

    internal class InfusionEntry : CodexEntry
    {
        public InfusionEntry()
        {
            Category = Categories.Misc;
            Title = "Infusions";
            Body = "Infusions are powerful magical relics which can augment a user's abilities or change how they work altogether. Two main types of infusions exist, Generic Infusions and Ability Infusions. NEWBLOCK " +
                "Generic Infusions typically provide a general boost that applied to all of the player's abilities, or have small enough effects that they do not warrant the ability-type restriction of Ability Infusions. While you can equip as many Generic Infusions as you have slots, you cannot equip duplicates. NEWBLOCK " +
                "Ability Infusions are specific to one of your abilities, and greatly augment them or change how they function alltogether. While powerful, these infusions are generally harder to come by than Generic Infusions. You cannot equip two Ability Infusions for the same ability, and you cannot equip duplicates.";
            Hint = "Find a mysterious altar...";
            Image = GetTexture("StarlightRiver/Assets/GUI/StaminaEmpty");
            Icon = Image;
        }
    }

    internal class BarrierEntry : CodexEntry
    {
        public BarrierEntry()
        {
            Category = Categories.Misc;
            Title = "Barrier";
            Body = "Barrier acts as a way to wrap your body in magical energies, protecting it from physical harm. While not normally attainable, adventerers can wear magical accessories or armors that allow them to keep and accumulate barrier on themselves, or quaff potions which grant a brief period of fleeting barrier. NEWBLOCK " +
                "While powerful, barrier is not capable of fully absorbing damage, normal barriers will only protect from about 75% of damage, while also taking the full brunt of the attack to itself. Barrier will also not protect it's user against all forms of bodily harm, ailments that directly affect it's bearer such as fire and poisions or magics which directly drain one's body such as chaos state will cause full harm, through any amount of barrier NEWBLOCK " +
                "It should also be noted that barrier cannot accumulate on a being in pain, one must wait around 3 seconds after being harmed for their barrier to begin to recharge without the aid of potions or equipment to expediate the effect.";
            Hint = "Use equipment or potions which grant barrier...";
            Image = GetTexture(AssetDirectory.GUI + "ShieldHeartOver");
            Icon = Image;
        }
    }
}