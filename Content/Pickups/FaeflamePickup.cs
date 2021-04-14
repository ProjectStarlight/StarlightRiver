using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faeflame;

namespace StarlightRiver.Pickups
{
    internal class FaeflamePickup : AbilityPickup
    {
        public override string Texture => "StarlightRiver/Assets/Abilities/Faeflame";
        public override Color GlowColor => new Color(255, 255, 130);

        public override bool CanPickup(Player player)
        {
            return !player.GetHandler().Unlocked<Wisp>();
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Faeflame");
        }

        public override void Visuals()
        {
            Dust.NewDustPerfect(npc.Center + new Vector2((float)Math.Cos(StarlightWorld.rottime), (float)Math.Sin(StarlightWorld.rottime)) * (float)Math.Sin(StarlightWorld.rottime * 2 + 1) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.Zero, 0, default, 0.65f);
            Dust.NewDustPerfect(npc.Center + new Vector2((float)Math.Cos(StarlightWorld.rottime + 2) / 2, (float)Math.Sin(StarlightWorld.rottime + 2)) * (float)Math.Sin(StarlightWorld.rottime * 2 + 4) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.Zero, 0, default, 0.65f);
            Dust.NewDustPerfect(npc.Center + new Vector2((float)Math.Cos(StarlightWorld.rottime + 4), (float)Math.Sin(StarlightWorld.rottime + 4) / 2) * (float)Math.Sin(StarlightWorld.rottime * 2 + 2) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.Zero, 0, default, 0.65f);

            Dust.NewDustPerfect(npc.Center + Vector2.One.RotateRandom(Math.PI) * (float)Math.Sin(StarlightWorld.rottime * 2 + 2) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.UnitY * -2, 0, default, 0.25f);
        }

        public override void PickupVisuals(int timer)
        {
            if (timer == 1)
            {
                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Pickups/get")); //start the SFX
                Filters.Scene.Deactivate("Shockwave");
            }
        }

        public override void PickupEffects(Player player)
        {
            AbilityHandler mp = player.GetHandler();
            mp.Unlock<Wisp>();

            player.GetModPlayer<StarlightPlayer>().MaxPickupTimer = 570;
            player.AddBuff(BuffID.Featherfall, 580);
        }
    }

    public class FaeflamePickupTile : AbilityPickupTile
    {
        public override int PickupType => NPCType<FaeflamePickup>();
    }

    public class FaeflameTileItem : QuickTileItem
    {
        public FaeflameTileItem() : base("Faeflame", "Debug placer for ability pickup", TileType<FaeflamePickupTile>(), -1) { }

        public override string Texture => AssetDirectory.Debug;
    }
}