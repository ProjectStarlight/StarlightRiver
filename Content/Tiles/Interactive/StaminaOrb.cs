using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using System;
using Terraria.ID;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Content.Tiles.Interactive
{
    internal class StaminaOrb : DummyTile
    {
        public override int DummyType => ProjectileType<StaminaOrbDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.InteractiveTile + name;
            return true;
        }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;

            drop = mod.ItemType("StaminaOrbItem");
            dustType = mod.DustType("Stamina");
            AddMapEntry(new Color(255, 186, 66));
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.236f / 1.1f;
            g = 0.144f / 1.1f;
            b = 0.071f / 1.1f;
        }
    }

    internal class StaminaOrbDummy : Dummy
    {
        public StaminaOrbDummy() : base(TileType<StaminaOrb>(), 16, 16) { }

        public override void Update()
        {
            if (projectile.localAI[0] > 0)
                projectile.localAI[0]--;
            else
            {
                float rot = Main.rand.NextFloat(0, 6.28f);
                Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Stamina>(), new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot)) * 0.4f, 0, default, 2f);
            }
        }

        public override void Collision(Player player)
        {
            AbilityHandler mp = player.GetHandler();

            mp.Stamina++;
            projectile.localAI[0] = 300;
            Main.PlaySound(SoundID.Item112, projectile.Center);
            CombatText.NewText(player.Hitbox, new Color(255, 170, 60), "+1");

            for (float k = 0; k <= 6.28; k += 0.1f)
                Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Stamina>(), new Vector2((float)Math.Cos(k), (float)Math.Sin(k)) * (Main.rand.Next(25) * 0.1f), 0, default, 3f);
        }
    }


}