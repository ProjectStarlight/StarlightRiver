using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    class Guillotine : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public Guillotine() : base("Golden Guillotine", "Critical strikes gain power as your foes lose health\nExecutes normal enemies on low health") { }

        public override void Load()
        {
            StarlightPlayer.ModifyHitNPCEvent += ModifyCrit;
            StarlightPlayer.ModifyHitNPCWithProjEvent += ModifyCritProj;
            return base.Autoload(ref name);
        }

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.Orange;
        }

        private void ModifyCritProj(Player Player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (Equipped(Main.player[proj.owner]))
            {
                float multiplier = 2 + CritMultiPlayer.GetMultiplier(proj);

                if (crit)
                    damage += (int)((damage / multiplier) * (1.5f - (target.life / target.lifeMax) / 2));

                if (!target.boss && (target.life / target.lifeMax) < 0.1f && (damage * multiplier * 1.5f) > target.life)
                    Execute(target, proj.owner);
            }
        }

        private void ModifyCrit(Player Player, Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (Equipped(Player))
            {
                float multiplier = 2 + CritMultiPlayer.GetMultiplier(Item);

                if (crit)
                    damage += (int)((damage / multiplier) * (1.5f - (target.life / target.lifeMax) / 2));

                if (!target.boss && (target.life / target.lifeMax) < 0.1f && (damage * multiplier * 1.5f) > target.life)
                    Execute(target, Player.whoAmI);
            }
        }

        private void Execute(NPC NPC, int owner)
        {
            int flesh = 1;
            if (Helpers.Helper.IsFleshy(NPC))
                flesh = 0;

            if (Main.myPlayer == owner)
            {
                Projectile.NewProjectile(NPC.Center, Vector2.Zero, ModContent.ProjectileType<GuillotineVFX>(), 0, 0, Main.myPlayer, NPC.whoAmI, flesh);
                NPC.StrikeNPC(9999, 0f, 1, false, true, false);// kill NPC
            }
        }
    }

    class GuillotineVFX : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public ref float HitNpcIndex => ref Projectile.ai[0];

        // 0 for fleshy, anything else for not fleshy
        public ref float WasFleshy => ref Projectile.ai[1];

        private bool alreadyHit = false;

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.timeLeft = 45;
        }

        public override void AI()
        {
            if (!alreadyHit)
            {
                alreadyHit = true;

                CombatText.NewText(Projectile.Hitbox, new Color(255, 230, 100), "Ouch!", true);

                NPC NPC = Main.npc[(int)HitNpcIndex];
                NPC.StrikeNPCNoInteraction(9999, 1, 0, false, true);
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);

                if (WasFleshy == 0)
                {
                    Helpers.Helper.PlayPitched("Impacts/GoreHeavy", 1, 0, Projectile.Center);

                    for (int k = 0; k < 200; k++)
                    {
                        Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), 0, default, 2);
                    }
                }
                else
                    Helpers.Helper.PlayPitched("ChainHit", 1, -0.5f, Projectile.Center);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var tex = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "Guillotine").Value;
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * (Projectile.timeLeft / 45f), 0, tex.Size() / 2f, (1 - (Projectile.timeLeft / 45f)) * 3f, 0, 0);

            return false;
        }

    }
}
