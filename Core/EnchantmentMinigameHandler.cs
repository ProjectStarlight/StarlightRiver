using StarlightRiver.Content.ArmorEnchantment;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	class EnchantmentMinigameHandler : ModPlayer
    {
        int minigameLife;
        int minigameTimer;
        ArmorEnchantment activeEnchant;

        public void ResetMinigame(ArmorEnchantment enchant)
        {
            minigameLife = Player.statLifeMax >= 500 ? 4 : 3;
            minigameTimer = 0;
            activeEnchant = enchant;
        }

        public override void PostUpdate()
        {

        }

        public override void PreUpdateMovement()
        {            
            //Player.velocity *= 0.2f;
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            base.ModifyDrawLayers(layers);
        }
    }
}
