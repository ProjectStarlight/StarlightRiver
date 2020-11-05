#if DEBUG
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Abilities;
using StarlightRiver.GUI;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Debug
{
    public class DebugPlacer1 : QuickTileItem
    {
        public override string Texture => "StarlightRiver/Items/Debug/DebugPotion";
        public DebugPlacer1() : base("Debug Placer 1", "Suck my huge dragon dong", TileType<Tiles.Overgrow.BossPit>(), 0) { }
    }

    public class DebugPlacer3 : QuickTileItem
    {
        public override string Texture => "StarlightRiver/Items/Debug/DebugPotion";
        public DebugPlacer3() : base("Debug Placer 3", "Suck my huge dragon dong", TileType<Tiles.VerletBanner>(), 0) { }
    }

    public class DebugPlacer4 : QuickTileItem
    {
        public override string Texture => "StarlightRiver/Items/Debug/DebugPotion";
        public DebugPlacer4() : base("Debug Placer 4", "Suck my huge dragon dong", TileType<Tiles.Overgrow.BossAltar>(), 0) { }
    }

    public class DebugPotion : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 64;
            item.height = 64;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 10;
            item.useTime = 10;
            item.rare = ItemRarityID.Green;
            item.autoReuse = true;
        }

        public override string Texture => "StarlightRiver/MarioCumming";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Potion of Debugging");
            Tooltip.SetDefault("Effects vary");
        }

        public override bool UseItem(Player player)
        {
            //NPC.NewNPC((StarlightWorld.VitricBiome.X - 10) * 16, (StarlightWorld.VitricBiome.Center.Y + 12) * 16, NPCType<NPCs.Miniboss.Glassweaver.GlassweaverWaiting>());
            //StarlightWorld.knownRecipies.Clear();

            //StarlightWorld.AshHellGen(new Terraria.World.Generation.GenerationProgress());

            StarlightWorld.FlipFlag(WorldFlags.SquidBossDowned);
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine line = new TooltipLine(mod, "Titties", GetHashCode().ToString());
            tooltips.Add(line);
        }

        public override void HoldItem(Player player)
        {
            //player.GetHandler().StaminaMaxBonus = 10;
            Main.time += 60;
        }

        Effect theEffect;

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            return true; 

            if (theEffect is null) theEffect = Main.dedServ ? null : Filters.Scene["IceCrystal"].GetShader().Shader;
            if (theEffect is null) return true;

            GraphicsDevice graphics = Main.instance.GraphicsDevice;
            VertexPositionColorTexture[] verticies = new VertexPositionColorTexture[6];

            verticies[0] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
            verticies[1] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.Red, new Vector2(1, 1));
            verticies[2] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));

            verticies[3] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
            verticies[4] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.Red, new Vector2(0, 0));
            verticies[5] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));

            VertexBuffer buffer = new VertexBuffer(graphics, typeof(VertexPositionColorTexture), 6, BufferUsage.WriteOnly);
            buffer.SetData(verticies);

            graphics.SetVertexBuffer(buffer);
            graphics.RasterizerState = new RasterizerState() { CullMode = CullMode.None };

            theEffect.Parameters["resolution"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            theEffect.Parameters["center"].SetValue(new Vector2(0.5f, 0.5f));
            theEffect.Parameters["zoom"].SetValue(Main.GameViewMatrix.ZoomMatrix);
            theEffect.Parameters["sprite"].SetValue(GetTexture("StarlightRiver/sprite"));
            theEffect.Parameters["behind"].SetValue(Main.screenTarget);
            theEffect.Parameters["volumeMap"].SetValue(GetTexture("StarlightRiver/volume"));
            theEffect.Parameters["refractMap"].SetValue(GetTexture("StarlightRiver/refract"));

            foreach (EffectPass pass in theEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }

            return true;
        }
    }

    public class DebugPotion2 : ModItem
    {
        public override void SetDefaults()
        {
            item.width = 64;
            item.height = 64;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 2;
            item.useTime = 2;
            item.rare = ItemRarityID.Green;
            item.noUseGraphic = true;
            item.createTile = TileType<NPCs.Vitric.WalkingSentinelTile>();
        }

        public override string Texture => "StarlightRiver/MarioCumming";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Potion of Debugging 2");
            Tooltip.SetDefault("Effects varyy");
        }

        public override bool UseItem(Player player)
        {
            return true;
        }

        public override void HoldItem(Player player)
        {

        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {

        }
    }
}
#endif