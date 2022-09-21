using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Archaeology;
using StarlightRiver.Content.Items.BuriedArtifacts;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Archaeology.BuriedArtifacts //using empty classes instead of interfaces so I can easily read them via Mod.GetContent in generation
{
    public abstract class OceanArtifact : Artifact { } 

    public abstract class LavaArtifact : Artifact { }

    public abstract class DesertArtifact : Artifact { }

    public abstract class UndergroundArtifact : Artifact { }
}