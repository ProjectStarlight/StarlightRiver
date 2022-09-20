using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class RuneStaff : ModItem
    {
        int charge = 0;

        public override string Texture => AssetDirectory.CaveTempleItem + Name;

        public override bool IsCloneable => true;

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ancient Rune Staff");
            Tooltip.SetDefault("Hold to summon up to 3 explosive runes\nReleasing will fire the runes towards your cursor\nContinuing to hold will detonate them for increased damage around you");
        }

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Magic;
            Item.mana = 10;
            Item.width = 32;
            Item.height = 32;
            Item.damage = 12;
            Item.crit = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.knockBack = 2;
            Item.rare = ItemRarityID.Green;
            Item.channel = true;
        }

        public override void HoldItem(Player player)
        {
            if (player.channel)
            {
                player.itemAnimation = player.itemAnimationMax - 1;

                if (charge % 30 == 0 && charge < 90)
                {
                    int index = charge / 30;
                    float rot = MathHelper.Pi / 3f * index - MathHelper.Pi / 3f;
                    var pos = player.Center + Vector2.UnitY.RotatedBy(rot) * -45;
                    int i = Projectile.NewProjectile(player.GetSource_ItemUse(Item), pos, Vector2.Zero, ProjectileType<RuneStaffProjectile>(), Item.damage, Item.knockBack, player.whoAmI, 0, charge);
                    Main.projectile[i].frame = index;

                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item8, player.Center);
                }
                charge++;
            }

            else charge = 0;
        }

		public override ModItem Clone(Item newEntity)
		{
			var item = base.Clone(newEntity);
            (item as RuneStaff).charge = charge;

            Main.NewText("Cloned new staff with a charge level of: " + (item as RuneStaff).charge);

            return item;
		}
	}

    class RuneStaffProjectile : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.CaveTempleItem + Name;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 600;
            Projectile.friendly = true;
        }

        public override bool? CanDamage() => Projectile.ai[0] == 1;

        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];
            int time = 600 - Projectile.timeLeft;

            //on release
            if (!Main.mouseLeft && Projectile.ai[0] == 0 && time >= 20)
            {
                Projectile.ai[0] = 1;
                Projectile.ai[1] = 0;
                Projectile.velocity = Vector2.Normalize(Projectile.Center - Main.MouseWorld) * -18;
                Projectile.tileCollide = true;
                Projectile.netUpdate = true;

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item82, Player.Center);
            }

            Projectile.ai[1]++;

            if (Projectile.ai[0] == 1)
                for (int k = 0; k < 2; k++)
                    Dust.NewDust(Projectile.position + Vector2.One * 4, 8, 8, DustType<Content.Dusts.Stamina>(), 0, 0, 0, default, 0.6f);
            else
            {
                Projectile.position += Player.velocity;

                //blow the runes if overcharged
                if (Projectile.ai[1] >= 120)
                    Projectile.timeLeft = 0;
            }
        }

        public override void Kill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<RuneStaffExplosion>(), Projectile.ai[0] == 0 ? 120 : 20, 2, Projectile.owner);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int time = 600 - Projectile.timeLeft;

            Texture2D tex = Request<Texture2D>(Texture).Value;
            float colorOff = time < 20 ? time / 20f : 1;
            Color color = new Color(255, colorOff, 1 - colorOff);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * 32, 32, 32), color, 0, Vector2.One * 16, colorOff / 2, 0, 0);

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            int time = 600 - Projectile.timeLeft;
            float colorOff = time < 20 ? time / 20f : 1;
            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;

            if (Projectile.ai[0] == 1)
            {
                if (Projectile.ai[1] < 15)
                {
                    Color color = new Color(255, 230, 100) * (1 - Projectile.ai[1] / 15f);
                    for (int k = 0; k < 3; k++)
                        spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, 0, tex.Size() / 2, Projectile.ai[1] / 7.5f + k / 12f, 0, 0);
                }

                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Color color = new Color(255, colorOff / 2, 1 - colorOff) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.5f;
                    if (k <= 4) color *= 1.2f;
                    float scale = colorOff / 2 * (Projectile.oldPos.Length - k) / Projectile.oldPos.Length * 1.4f;

                    spriteBatch.Draw(tex, Projectile.oldPos[k] + Projectile.Size / 2 - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
                }
            }
            else
            {
                Color color = new Color(255, colorOff / 2, 1 - colorOff);
                spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color * 0.6f, 0, tex.Size() / 2, colorOff / 2, 0, 0);
            }
        }
    }

    public class RuneStaffExplosion : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 128;
            Projectile.timeLeft = 2;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 100; k++)
                Dust.NewDustPerfect(Projectile.Center, DustType<Content.Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f) * 5);
        }
    }
}