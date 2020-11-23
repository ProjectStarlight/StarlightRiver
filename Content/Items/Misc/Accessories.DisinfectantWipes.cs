using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Dusts;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Items.Accessories.EarlyPreHardmode
{
    public class DisinfectantWipes : SmartAccessory
    {
        public DisinfectantWipes() : base("Disinfectant Wipes", "Melee crits reduce all debuff durations by 3 seconds\nNon-Melee attacks have a 50% chance to trigger on Crit\nhas a Short cooldown between activations") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.OnHitNPCEvent += OnHitNPCAccessory;
            StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;
            StarlightPlayer.ResetEffectsEvent += ResetEffectsAccessory;
            return true;
        }
        private void OnHit(Player player, NPC target, int damage, float knockback, bool crit, bool type)
        {
            if (Equipped(player) && crit && Main.rand.Next(0, 100) < (type ? 50 : 100))
            {
                DisinfectantWipes.CleanDebuff(player, type ? 1 : 0);
            }
        }
        private void OnHitNPCAccessory(Player player, Item item, NPC target, int damage, float knockback, bool crit)
        {
            OnHit(player, target, damage, knockback, crit, false);
        }
        private void OnHitNPCWithProjAccessory(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            OnHit(player, target, damage, knockback, crit, player.heldProj == proj.whoAmI ? false : true);
        }
        private void ResetEffectsAccessory(StarlightPlayer slp)
        {
            slp.DisinfectCooldown = (short)Math.Max(slp.DisinfectCooldown - 1, 0);
        }

        public static void MakeDusts(Rectangle rect, int dustcount = 5, Color color = default, int dust = DustID.Fire, float scale = 1f)
        {
            for (int k = 0; k <= dustcount; k++)
            {
                Dust.NewDustPerfect(new Vector2(rect.Left, rect.Top) + new Vector2(Main.rand.NextFloat(0, rect.Width), Main.rand.NextFloat(0, rect.Height)), dust, Vector2.Zero, 255, color, scale);
                for (int i = 0; i <= 3; i++)
                {
                    Vector2 velo = new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2));
                    Dust.NewDustPerfect(new Vector2(rect.Left, rect.Top) + new Vector2(Main.rand.NextFloat(0, rect.Width), Main.rand.NextFloat(0, rect.Height)), dust, velo, 100, color, scale * 0.5f);
                }
            }
            if (dustcount > 5)
                Main.PlaySound(SoundID.Item27);
        }

        public static void CleanDebuff(Player player, int type)
        {
            StarlightPlayer slp = player.GetModPlayer<StarlightPlayer>();
            short cooldown = slp.DisinfectCooldown;
            bool clean = false;
            List<int> buffs = new List<int>();

            for (int i = 0; i < Player.MaxBuffs; i += 1)
            {
                if (Helper.IsValidDebuff(player, i))
                {
                    if (type == 1)
                    {
                        buffs.Add(i);
                        clean = true;
                    }

                    if (type == 0 && slp.DisinfectCooldown < 1)
                    {
                        player.buffTime[i] = (int)MathHelper.Max(player.buffTime[i] - (60 * 3), 5);
                        clean = true;
                    }
                }
            }

            if (clean)
            {
                if (type == 1)
                {
                    if (buffs.Count > 0)
                    {
                        int buffid = Main.rand.Next(0, buffs.Count);
                        if (SanitizerSpray.SanitizeEnemies(player, player.buffType[buffid], 60 * 5, 300))
                        {
                            player.buffTime[buffid] = (int)MathHelper.Max(player.buffTime[buffid] - (60 * 5), 5);
                            MakeDusts(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), 2, Color.Blue, ModContent.DustType<Air>());
                        }
                    }
                }
                else
                {
                    slp.DisinfectCooldown = 20;
                    MakeDusts(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), 5, Color.White, ModContent.DustType<Air>());
                }
                Main.PlaySound(SoundID.Item, (int)player.Center.X, (int)player.Center.Y, 100, 0.65f, -Main.rand.NextFloat(0.35f, 0.75f));
            }

        }

    }
}