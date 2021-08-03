using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	public class Ironheart : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public override bool Autoload(ref string name)
        {
            StarlightPlayer.OnHitNPCEvent += OnHit;
            return base.Autoload(ref name);
        }
        public Ironheart() : base("Ironheart", "NaN") { }

        private void OnHit(Player player, Item item, NPC target, int damage, float knockback, bool crit)
        {
            if(Equipped(player))
                player.GetModPlayer<StarlightPlayer>().SetIronHeart(damage);
        }
    }

    public class IronheartBuff : ModBuff
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.MiscItem + name;
            return true;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault("Ironheart");
            //Description.SetDefault("TODO: desc");
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            StarlightPlayer mp = player.GetModPlayer<StarlightPlayer>();

            float level;

            if (mp.ironheartTimer < 1)
            {
                mp.ironheartTimer += 0.01f;
                level = mp.ironheartLevel;
                player.GetModPlayer<ShieldPlayer>().DontDrainOvershield = true;
            }
            else
            {
                mp.ironheartTimer *= 1.02f;
                level = (mp.ironheartLevel + 1) - mp.ironheartTimer;
                player.GetModPlayer<ShieldPlayer>().OvershieldDrainRate = (int)(2.2f * mp.ironheartTimer);
            }

            //Main.NewText(level + " | " + mp.ironheartTimer);
            Main.NewText(level);
            if (level < 0.001f)
            {
                player.ClearBuff(Type);
                mp.ResetIronHeart();
            }

            player.statDefense += (int)level;

            player.buffTime[buffIndex] = (int)level * 60;//visual time value
        }
    }
}

namespace StarlightRiver.Core
{
	public partial class StarlightPlayer : ModPlayer
    {
        public const int IronheartMaxLevel = 15;
        public const int IronheartMaxDamage = 75; 

        public int ironheartLevel = 0;
        public float ironheartTimer = 0;

        public void SetIronHeart(int damage)
        {
            int buffType = ModContent.BuffType<IronheartBuff>();

            if (!player.HasBuff(buffType))
                ResetIronHeart();

            int level = Math.Min(damage, IronheartMaxDamage) / 15;

            if (level > 0 && ironheartLevel < IronheartMaxLevel)//if level was increased
            {
                player.GetModPlayer<ShieldPlayer>().Shield += ((ironheartLevel += level) > IronheartMaxLevel ? 
                    level - (ironheartLevel - IronheartMaxLevel) : level) * 2;

                ironheartLevel = ironheartLevel > IronheartMaxLevel ? IronheartMaxLevel : ironheartLevel;//caps value
                player.AddBuff(buffType, 1);
            }
        }

        public void ResetIronHeart()
        {
            ironheartLevel = 0;
            ironheartTimer = 0;
        }
    }
}