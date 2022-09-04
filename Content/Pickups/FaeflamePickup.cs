using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Pickups
{
	internal class FaeflamePickup : AbilityPickup
    {
        public override string Texture => "StarlightRiver/Assets/Abilities/Faeflame";
        public override Color GlowColor => new Color(255, 255, 130);

        public Color[] playerImageData;
        public float oldLerper = 0;

        public override bool CanPickup(Player Player)
        {
            return !Player.GetHandler().Unlocked<Whip>();
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Faewhip");
        }

        public override void Visuals()
        {
            Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Cos(StarlightWorld.rottime), (float)Math.Sin(StarlightWorld.rottime)) * (float)Math.Sin(StarlightWorld.rottime * 2 + 1) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.Zero, 0, default, 0.65f);
            Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Cos(StarlightWorld.rottime + 2) / 2, (float)Math.Sin(StarlightWorld.rottime + 2)) * (float)Math.Sin(StarlightWorld.rottime * 2 + 4) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.Zero, 0, default, 0.65f);
            Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Cos(StarlightWorld.rottime + 4), (float)Math.Sin(StarlightWorld.rottime + 4) / 2) * (float)Math.Sin(StarlightWorld.rottime * 2 + 2) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.Zero, 0, default, 0.65f);

            Dust.NewDustPerfect(NPC.Center + Vector2.One.RotateRandom(Math.PI) * (float)Math.Sin(StarlightWorld.rottime * 2 + 2) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.UnitY * -2, 0, default, 0.25f);
        }

        public override void PickupVisuals(int timer)
        {
            Player Player = Main.LocalPlayer;

            if (timer == 1)
            {
                Terraria.Audio.SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Pickups/get")); //start the SFX
                Filters.Scene.Deactivate("Shockwave");
            }

            if (!Main.dedServ)
            {
                RenderTarget2D target = PlayerTarget.Target;
                if (timer == 1)
                {
                    playerImageData = new Color[target.Width * target.Height];
                    target.GetData(playerImageData);
                }
                if (timer > 1)
                {
                    float lerper = timer / 570f;

                    for (int x = (int)(oldLerper * PlayerTarget.sheetSquareX); x < lerper * PlayerTarget.sheetSquareX; x += 2)
                    {
                        for (int y = 0; y < PlayerTarget.sheetSquareY; y+= 2)
                        {
                            Color pixel = playerImageData[y * target.Width + x];
                            if (pixel.A > 0 && Main.rand.NextBool(5)) //TODO: Fix with zoom
                            {
                                Vector2 worldPosition = PlayerTarget.getPlayerTargetPosition(Player.whoAmI) + Main.screenPosition + new Vector2(x, y);
                                Vector2 direction = worldPosition.DirectionTo(Player.Center);

                                Dust dust = Dust.NewDustPerfect(worldPosition + Main.rand.NextVector2Circular(40,40), ModContent.DustType<Dusts.FaewhipMetaballDust>(), direction, 0, Color.Transparent, 1);
                                dust.customData = worldPosition;
                            }
                        }
                    }
                    oldLerper = lerper;
                }
                if (timer == 569)
                    foreach (Dust dust in Main.dust)
                    {
                        if (dust.type == ModContent.DustType<Dusts.FaewhipMetaballDust>())
                            dust.active = false;
                    }
            }
        }

        public override void PickupEffects(Player player)
        {
            AbilityHandler mp = player.GetHandler();
            mp.Unlock<Whip>();

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
        public FaeflameTileItem() : base("Faeflame", "Debug placer for ability pickup", "FaeflamePickupTile", -1) { }

        public override string Texture => "StarlightRiver/Assets/Abilities/Faeflame";
    }
}