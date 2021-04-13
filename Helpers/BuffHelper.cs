using Terraria;
using Terraria.ID;


namespace StarlightRiver.Helpers
{
    public static partial class Helper
    {
        public static bool IsValidDebuff(Player player, int buffindex)
        {
            int bufftype = player.buffType[buffindex];
            bool vitalbuff = (bufftype == BuffID.PotionSickness || bufftype == BuffID.ManaSickness || bufftype == BuffID.ChaosState);
            return player.buffTime[buffindex] > 2 && Main.debuff[bufftype] && !Main.buffNoTimeDisplay[bufftype] && !Main.vanityPet[bufftype] && !vitalbuff;
        }
    }
}

