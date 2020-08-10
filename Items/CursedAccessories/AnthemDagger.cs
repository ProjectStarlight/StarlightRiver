using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.CursedAccessories
{
    internal class AnthemDagger : CursedAccessory
    {
        public AnthemDagger() : base(GetTexture("StarlightRiver/Items/CursedAccessories/AnthemDaggerGlow")) { }

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Consume mana to absorb damage\n90% Reduced defense");
            DisplayName.SetDefault("Anthem Dagger");
        }

        public override void SafeUpdateEquip(Player player)
        {
            player.statDefense /= 10;
            player.manaFlower = false;
        }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.PreHurtEvent += PreHurtDagger;
            return true;
        }

        private bool PreHurtDagger(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (Equipped(player))
            {
                if (player.statMana > damage)
                {
                    player.statMana -= damage;
                    player.ManaEffect(damage);
                    damage = 0;
                    player.manaRegenDelay = 0;
                    player.statLife += 1;
                    playSound = false;
                    genGore = false;
                    Main.PlaySound(SoundID.MaxMana);
                }
                else if (player.statMana > 0)
                {
                    player.ManaEffect(player.statMana);
                    damage -= player.statMana;
                    player.statMana = 0;
                    player.manaRegenDelay = 0;
                    Main.PlaySound(SoundID.MaxMana);
                }
            }
            return true;
        }
    }
}