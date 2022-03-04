using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Dyes;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Geomancer
{
    public class EmeraldHeart : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Emerald Heart");
            Tooltip.SetDefault("You shouldn't see this");
        }

        public override void SetDefaults()
        {
            item.width = 24;
            item.height = 24;
            item.maxStack = 1;
        }

        public override bool ItemSpace(Player player) => true;
        public override bool OnPickup(Player player)
        {
            int healAmount = (int)MathHelper.Min(player.statLifeMax2 - player.statLife, 5);
            player.HealEffect(5);
            player.statLife += healAmount;

            Main.PlaySound(SoundID.Grab, (int)player.position.X, (int)player.position.Y);
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 100);
    }

    public class RubyDagger : ModProjectile
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoRuby";

        int counter;

        float speedMult;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ruby Dagger");
        }

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.Size = new Vector2(16, 16);
            projectile.penetrate = -1;
            projectile.timeLeft = 80;
        }

        public override void AI()
        {
            Vector2 direction = Main.npc[(int)projectile.ai[0]].Center - projectile.Center;
            direction.Normalize();

            projectile.rotation = direction.ToRotation() + 1.57f;

            counter++;

            float smallCounter = counter * 0.1f;
            if (smallCounter < 4f)
            {
                speedMult = (float)-Math.Sin(smallCounter);
                projectile.velocity = speedMult * direction * (float)Math.Sqrt(counter);
            }
            else if (projectile.velocity.Length() < 25)
                projectile.velocity *= 1.1f;
        }
    }
}