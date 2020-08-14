using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Items.BossDrops.VitricBossDrops
{
    class VitricBossBow : ModItem, IGlowingItem
    {
        int charge = 0; 

        public override void SetDefaults()
        {
            item.damage = 18;
            item.ranged = true;
            item.width = 16;
            item.height = 64;
            item.useTime = 30;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.knockBack = 2;
            item.rare = ItemRarityID.Orange;
            item.channel = true;
            item.useAmmo = AmmoID.Arrow;
            item.useTurn = true;
        }

        public override void HoldItem(Player player)
        {
            if (player.channel)
            {
                if(charge < 60) charge++;
            }

            if (!player.channel)
            {
                if(charge > 15)
                {
                    Vector2 velocity = Vector2.Normalize(Main.MouseWorld - player.Center) * charge / 4f;
                    Projectile.NewProjectile(player.Center, velocity, ProjectileID.WoodenArrowFriendly, charge, charge / 30f, player.whoAmI);
                }

                charge = 0;
            }
        }

        public void DrawGlowmask(PlayerDrawInfo info)
        {
            Player player = info.drawPlayer;

            if (player.channel)
            {
                Vector2 off = Vector2.Normalize(Main.MouseWorld - player.Center) * 16;

                var data = new DrawData(GetTexture(Texture), player.Center + off - Main.screenPosition, null, Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16), off.ToRotation(), item.Size / 2, 1, 0, 0);
                Main.playerDrawData.Add(data);

                Texture2D tex = GetTexture("StarlightRiver/NPCs/Boss/VitricBoss/ConeTell");
                float rotOff = (1 - charge / 60f) * 0.3f;
                var data2 = new DrawData(tex, player.Center - Main.screenPosition, null, Color.White * (charge / 360f), off.ToRotation() + (float)Math.PI / 2 + rotOff, new Vector2(tex.Width / 2, tex.Height), 0.5f, 0, 0);
                var data3 = new DrawData(tex, player.Center - Main.screenPosition, null, Color.White * (charge / 360f), off.ToRotation() + (float)Math.PI / 2 - rotOff, new Vector2(tex.Width / 2, tex.Height), 0.5f, 0, 0);
                Main.playerDrawData.Add(data2);
                Main.playerDrawData.Add(data3);
            }
        }
    }
}
