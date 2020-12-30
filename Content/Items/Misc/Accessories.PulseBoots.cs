using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Misc
{
    [AutoloadEquip(EquipType.Shoes)]
    public class PulseBoots : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public PulseBoots() : base("Pulse Boots", "Rocket Power!") { }

        private bool doubleJumped = false;
        private bool releaseJump = false;
        private const int maxSpeed = 15;

        public override void SafeUpdateEquip(Player player)
        {
            void jumpSide(int side)
            {
                float velSide = player.velocity.X * side;
                if (velSide > 0 && velSide < maxSpeed)
                    player.velocity.X += (maxSpeed * side - player.velocity.X) / 2;
                else if (velSide < 0)
                    player.velocity.X = 6 * side;

                for (int y = 0; y < 10; y++)//placeholder dash dust
                    Dust.NewDust(player.BottomLeft + player.velocity, player.width, (int)player.velocity.Y, 6, 3 * -side, 0, 0, default, 2);
            }

            if (!player.controlJump && player.velocity.Y != 0)
                releaseJump = true;
            if (player.controlJump && player.velocity.Y != 0 && releaseJump && !doubleJumped)
            {
                doubleJumped = true;
                player.velocity.Y = -8; //base upward jump
                Main.PlaySound(SoundID.Item61, player.Center);

                for (float k = 0; k < 6.28f; k += 0.1f)
                {
                    float rand = Main.rand.NextFloat(-0.05f, 0.05f);
                    float x = (float)Math.Cos(k + rand) * 30;
                    float y = (float)Math.Sin(k + rand) * 10;
                    float rot = !player.controlLeft ? player.controlRight ? 1 : 0 : -1;

                    Dust.NewDustPerfect(player.Center + new Vector2(0, 16), DustType<Content.Dusts.Stamina>(), new Vector2(x, y).RotatedBy(rot) * 0.07f, 0, default, 1.6f);
                    Dust.NewDustPerfect(player.Center + new Vector2(0, 32), DustType<Content.Dusts.Stamina>(), new Vector2(x, y).RotatedBy(rot) * 0.09f, 0, default, 1.2f);
                    Dust.NewDustPerfect(player.Center + new Vector2(0, 48), DustType<Content.Dusts.Stamina>(), new Vector2(x, y).RotatedBy(rot) * 0.11f, 0, default, 0.8f);
                }
                Main.PlaySound(SoundID.DD2_BetsyFireballShot);

                if (player.controlLeft && player.controlRight || !player.controlLeft && !player.controlRight)
                {
                    player.velocity.Y += -2;//if neither or both, then slightly higher jump

                    for (int y = 0; y < 8; y++)//placeholder dash dust
                        Dust.NewDust(player.BottomLeft + player.velocity, player.width, (int)player.velocity.Y, 6, 0, 0, 0, default, 2);
                }
                else if (player.controlLeft)//-1
                    jumpSide(-1);
                else if (player.controlRight)//1
                    jumpSide(1);
            }
            if (player.velocity.Y == 0)
            {
                releaseJump = false;
                doubleJumped = false;
            }
        }
    }
}