using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using StarlightRiver.Content.Archaeology;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
    class ArchaeologistsMap : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Archaeologist's Map");
            Tooltip.SetDefault("Reveals the location of one nearby artifact");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.value = Item.sellPrice(0, 0, 10, 0);
            Item.rare = ItemRarityID.Blue;
            Item.noUseGraphic = false;
            Item.consumable = true;
            Item.maxStack = 999;
            Item.UseSound = SoundID.Item8;
        }

        public override bool? UseItem(Player player)
        {
            return RevealArtifact(player);
        }

        private bool RevealArtifact(Player player)
        {
            List<Artifact> artifacts = new List<Artifact>();
            foreach (var item in TileEntity.ByID)
            {
                if (item.Value is Artifact artifact && !artifact.displayedOnMap && artifact.CanBeRevealed())
                    artifacts.Add(artifact);
            }

            if (artifacts.Count == 0)
                return false;

            Artifact nearestArtifact = artifacts.OrderBy(n => n.WorldPosition.Distance(player.Center)).FirstOrDefault();
            nearestArtifact.displayedOnMap = true;
            (ModContent.GetInstance<ArchaeologyMapLayer>()).CalculateDrawables();
            return true;
        }
    }
}