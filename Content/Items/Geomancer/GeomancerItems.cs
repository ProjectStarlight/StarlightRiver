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
    public abstract class GeoGem : ModItem
    {

        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            SetName();
            Tooltip.SetDefault("You shouldn't see this");
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
        }

        public override bool ItemSpace(Player Player) => true;
        public override bool OnPickup(Player Player)
        {
            SetBonus(Player);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab, Player.position);
            Player.GetModPlayer<GeomancerPlayer>().timer = 1200;
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 100);

        protected virtual void SetName() { }

        protected virtual void SetBonus(Player Player) { }
    }

    public class GeoDiamond : GeoGem
    {
        const float rotation = 1.046f;
        protected override void SetName() => DisplayName.SetDefault("Diamond");

        protected override void SetBonus(Player Player)
        {
            GeomancerPlayer modPlayer = Player.GetModPlayer<GeomancerPlayer>();
            if (!modPlayer.DiamondStored && modPlayer.storedGem != StoredGem.All)
            {
                Projectile.NewProjectile(null, Player.Center, Vector2.Zero, ModContent.ProjectileType<GeoDiamondProj>(), 0, 0, Player.whoAmI, rotation);
                modPlayer.DiamondStored = true;
                modPlayer.storedGem = StoredGem.Diamond;
                modPlayer.ActivationCounter = 1f;
            }
        }
    }

    public class GeoRuby : GeoGem
    {
        const float rotation = 1.046f * 2;
        protected override void SetName() => DisplayName.SetDefault("Ruby");

        protected override void SetBonus(Player Player)
        {
            GeomancerPlayer modPlayer = Player.GetModPlayer<GeomancerPlayer>();
            if (!modPlayer.RubyStored && modPlayer.storedGem != StoredGem.All)
            {
                Projectile.NewProjectile(null, Player.Center, Vector2.Zero, ModContent.ProjectileType<GeoRubyProj>(), 0, 0, Player.whoAmI, rotation);
                modPlayer.RubyStored = true;
                modPlayer.storedGem = StoredGem.Ruby;
                modPlayer.ActivationCounter = 1f;
            }
        }
    }

    public class GeoEmerald : GeoGem
    {
        const float rotation = 1.046f * 3;
        protected override void SetName() => DisplayName.SetDefault("Emerald");

        protected override void SetBonus(Player Player)
        {
            int healAmount = (int)MathHelper.Min(Player.statLifeMax2 - Player.statLife, 20);
            Player.HealEffect(20);
            Player.statLife += healAmount;

            GeomancerPlayer modPlayer = Player.GetModPlayer<GeomancerPlayer>();
            if (!modPlayer.EmeraldStored && modPlayer.storedGem != StoredGem.All)
            {
                Projectile.NewProjectile(null, Player.Center, Vector2.Zero, ModContent.ProjectileType<GeoEmeraldProj>(), 0, 0, Player.whoAmI, rotation); 
                modPlayer.EmeraldStored = true;
                modPlayer.storedGem = StoredGem.Emerald;
                modPlayer.ActivationCounter = 1f;
            }
        }
    }

    public class GeoSapphire : GeoGem
    {
        const float rotation = 1.046f * 4;
        protected override void SetName() => DisplayName.SetDefault("Sapphire");

        protected override void SetBonus(Player Player)
        {
            int healAmount = (int)MathHelper.Min(Player.statManaMax2 - Player.statMana, 200);
            Player.ManaEffect(healAmount);
            Player.statMana += healAmount;

            GeomancerPlayer modPlayer = Player.GetModPlayer<GeomancerPlayer>();
            if (!modPlayer.SapphireStored && modPlayer.storedGem != StoredGem.All)
            {
                Projectile.NewProjectile(null, Player.Center, Vector2.Zero, ModContent.ProjectileType<GeoSapphireProj>(), 0, 0, Player.whoAmI, rotation);
                modPlayer.SapphireStored = true;
                modPlayer.storedGem = StoredGem.Sapphire;
                modPlayer.ActivationCounter = 1f;
            }
        }
    }

    public class GeoTopaz : GeoGem
    {
        const float rotation = 1.046f * 5;
        protected override void SetName() => DisplayName.SetDefault("Topaz");

        protected override void SetBonus(Player Player)
        {
            GeomancerPlayer modPlayer = Player.GetModPlayer<GeomancerPlayer>();
            if (!modPlayer.TopazStored && modPlayer.storedGem != StoredGem.All)
            {
                Projectile.NewProjectile(null, Player.Center, Vector2.Zero, ModContent.ProjectileType<GeoTopazProj>(), 0, 0, Player.whoAmI, rotation);
                modPlayer.TopazStored = true;
                modPlayer.storedGem = StoredGem.Topaz;
                modPlayer.ActivationCounter = 1f;
            }
        }
    }

    public class GeoAmethyst : GeoGem
    {
        const float rotation = 1.046f * 6;
        protected override void SetName() => DisplayName.SetDefault("Amethyst");

        protected override void SetBonus(Player Player)
        {
            GeomancerPlayer modPlayer = Player.GetModPlayer<GeomancerPlayer>();
            if (!modPlayer.AmethystStored && modPlayer.storedGem != StoredGem.All)
            {
                Projectile.NewProjectile(null, Player.Center, Vector2.Zero, ModContent.ProjectileType<GeoAmethystProj>(), 0, 0, Player.whoAmI, rotation);
                modPlayer.AmethystStored = true;
                modPlayer.storedGem = StoredGem.Amethyst;
                modPlayer.ActivationCounter = 1f;
            }
        }
    }
}