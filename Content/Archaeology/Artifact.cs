using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System;

namespace StarlightRiver.Content.Archaeology
{
    //TODO: Manually load generic artifacts once manual loading for tile entities is supported
    public abstract class Artifact : ModTileEntity
    {
        /// <summary>
        /// Whether or not the artifact is displayed on the map
        /// </summary>
        public bool displayedOnMap = false;

        /// <summary>
        /// Whether or not the artifact can be revealed by the archaeologist's map. Set to false if the artifact has a special reveal condition
        /// </summary>
        public virtual bool CanBeRevealed() => true; 

        public virtual string TexturePath => AssetDirectory.Archaeology + Name;

        /// <summary>
        /// Texture path the artifact uses on the map when revealed
        /// </summary>
        public virtual string MapTexturePath => AssetDirectory.Archaeology + "DigMarker";

        /// <summary>
        /// Size of the artifact. In world coordinates, not tile coordinates
        /// </summary>
        public virtual Vector2 Size { get; set; }

        /// <summary>
        /// The dust the artifact creates.
        /// </summary>
        public virtual int SparkleDust { get; set; }

        /// <summary>
        /// The rate at which sparkles spawn. Increase for lower spawnrate.
        /// </summary>
        public virtual int SparkleRate { get; set; }

        /// <summary>
        /// The color of the glowy effect when the artifact is excavated
        /// </summary>
        public virtual Color BeamColor { get; set; }

        /// <summary>
        /// The item the artifact drops
        /// </summary>
        public virtual int ItemType { get; set; }

        /// <summary>
        /// Pretty self explanatory. Higher = higher spawnrate
        /// </summary>
        public virtual float SpawnChance { get; set; } 

        public Vector2 WorldPosition => Position.ToVector2() * 16;

        /// <summary>
        /// Override if you want to check at these specific coordinates whether the artifact can generate
        /// </summary>
        public virtual bool CanGenerate(int i, int j) => true; 

        public virtual void Draw(SpriteBatch spriteBatch) => GenericDraw(spriteBatch);

        public override void Update()
        {
            CheckOpen();
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(displayedOnMap)] = displayedOnMap;
        }

        public override void LoadData(TagCompound tag)
        {
            try
            {
                displayedOnMap = tag.GetBool(nameof(displayedOnMap));
            }
            catch (Exception e)
            {
                StarlightRiver.Instance.Logger.Debug("handled error loading Artifacts: " + e);
            }
        }

        public override bool IsTileValidForEntity(int x, int y)
        {
            return true;
        }

        public bool IsOnScreen()
        {
            return Helper.OnScreen(new Rectangle((int)WorldPosition.X - (int)Main.screenPosition.X, (int)WorldPosition.Y - (int)Main.screenPosition.Y, (int)Size.X, (int)Size.Y));
        }

        public void CreateSparkles() 
        {
            Vector2 pos = WorldPosition + (Size * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat()));

            Color lightColor = Lighting.GetColor((pos / 16).ToPoint());
            if (lightColor == Color.Black)
                return;

            float sparkleMult = MathHelper.Max(lightColor.R, MathHelper.Max(lightColor.G, lightColor.B)) / 255f;

            if (sparkleMult == 0) //incase for whatever reason the Color.Black check wasn't enough
                return;

            int modifiedSparkleRate = (int)(SparkleRate / sparkleMult); //spawns sparkles relative to light level
            if (Main.rand.NextBool(modifiedSparkleRate))
                    Dust.NewDustPerfect(WorldPosition + (Size * new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat())), SparkleDust, Vector2.Zero);
        }

        public void GenericDraw(SpriteBatch spriteBatch) //I have no idea why but the drawing is offset by -192 on each axis by default, so I had to correct it
        {
            Texture2D tex = ModContent.Request<Texture2D>(TexturePath).Value;

            Vector2 offScreen = new Vector2(Main.offScreenRange);
            if (Main.drawToScreen)
            {
                offScreen = Vector2.Zero;
            }

            spriteBatch.Draw(tex, (WorldPosition - Main.screenPosition) + offScreen, null, Lighting.GetColor(Position.ToPoint()), 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
        }

        public void CheckOpen()
        {
            for (int i = 0; i < Size.X / 16; i++)
                for (int j = 0; j < Size.Y / 16; j++)
                {
                    Tile tile = Main.tile[i + Position.X, j + Position.Y];
                    if (tile.HasTile)
                        return;
                }

            Kill(Position.X, Position.Y);
            (ModContent.GetInstance<ArchaeologyMapLayer>()).CalculateDrawables();

            Projectile proj = Projectile.NewProjectileDirect(new EntitySource_Misc("Artifact"), WorldPosition, new Vector2(0, -0.5f), ModContent.ProjectileType<ArtifactItemProj>(), 0, 0);
            ArtifactItemProj modProj = proj.ModProjectile as ArtifactItemProj;
            modProj.itemTexture = TexturePath;
            modProj.glowColor = BeamColor;
            modProj.itemType = ItemType;
            modProj.size = Size;
            modProj.sparkleType = SparkleDust;
        }
    }
}