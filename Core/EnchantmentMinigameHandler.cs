using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using StarlightRiver.Content.ArmorEnchantment;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Core
{
    class EnchantmentMinigameHandler : ModPlayer
    {
        int minigameLife;
        int minigameTimer;
        ArmorEnchantment activeEnchant;

        public void ResetMinigame(ArmorEnchantment enchant)
        {
            minigameLife = player.statLifeMax >= 500 ? 4 : 3;
            minigameTimer = 0;
            activeEnchant = enchant;
        }

        public override void PostUpdate()
        {

        }

        public override void PreUpdateMovement()
        {            
            //player.velocity *= 0.2f;
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            base.ModifyDrawLayers(layers);
        }
    }
}
