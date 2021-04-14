using Microsoft.Xna.Framework;
using StarlightRiver.Codex;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Helpers;
using StarlightRiver.Content.GUI;

namespace StarlightRiver.Pickups
{
    internal class CodexPickup : AbilityPickup
    {
        public override string Texture => "StarlightRiver/Assets/GUI/Book1Closed";

        public override Color GlowColor => new Color(200, 130, 40);

        public override bool CanPickup(Player player) => player.GetModPlayer<CodexHandler>().CodexState == 0;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Starlight Codex");

        public override void Visuals()
        {
            float rot = Main.rand.NextFloat(6.28f);
            Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedBy(rot) * 20, DustType<Content.Dusts.Stamina>(), Vector2.One.RotatedBy(rot) * -1);

            Lighting.AddLight(npc.Center, new Vector3(1, 0.5f, 0));
        }

        public override void PickupVisuals(int timer)
        {
            if (timer == 1)
            {
                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Pickups/get")); //start the SFX
                Filters.Scene.Deactivate("Shockwave");
            }

            if (timer < 119)
            {
            }

            if (timer == 119)
            {
                string message = "Open the codex from your inventory to learn about the world.";

                UILoader.GetUIState<TextCard>().Display("Starlight Codex", message, null, 240);
                Helper.UnlockEntry<CodexEntry>(Main.LocalPlayer);
            }
        }

        public override void PickupEffects(Player player)
        {
            CodexHandler mp = player.GetModPlayer<CodexHandler>();
            mp.CodexState = 1;

            player.GetModPlayer<StarlightPlayer>().MaxPickupTimer = 120;
            player.AddBuff(BuffID.Featherfall, 130);
        }
    }

    public class CodexPickupTile : AbilityPickupTile
    {
        public override int PickupType => NPCType<CodexPickup>();
    }

    public class CodexTileItem : QuickTileItem
    {
        public CodexTileItem() : base("Starlight Codex", "Debug placer for ability pickup", TileType<CodexPickupTile>(), -1) { }

        public override string Texture => "StarlightRiver/Assets/GUI/Book1Closed";
    }
}