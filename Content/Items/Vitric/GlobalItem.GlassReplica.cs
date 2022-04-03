using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Vitric
{
	class GlassReplica : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

        public override bool NeedsSaving(Item Item) => isReplica;

        public bool isReplica;
        private bool firstTime = true;

        public override TagCompound Save(Item Item)
        {
            return new TagCompound
            {
                ["isReplica"] = isReplica
            };
        }

        public override void Load(Item Item, TagCompound tag)
        {
            isReplica = tag.GetBool("isReplica");
            firstTime = false;
        }

        public override bool PreDrawInInventory(Item Item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
        {
            if (isReplica)
            {
                spriteBatch.End();
                spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, Filters.Scene["VitricReplicaItem"].GetShader().Shader, Main.UIScaleMatrix);
                Filters.Scene["VitricReplicaItem"].GetShader().Shader.Parameters["uTime"].SetValue(StarlightWorld.rottime);
            }

            return true;
        }

        public override void PostDrawInInventory(Item Item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
        {
            if (isReplica)
            {
                spriteBatch.End();
                spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);
            }
        }

        public override bool PreDrawInWorld(Item Item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (isReplica)
            {
                if (firstTime)
                {
                    spriteBatch.End();
                    spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                    var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/RiftCrafting/Glow1").Value;
                    float scale1 = Main.PopupTexture[Item.type].Size().Length() / tex.Size().Length();
                    var color = new Color(180, 240, 255);

                    spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, color * 0.5f, StarlightWorld.rottime, tex.Size() / 2, 2 * scale1, 0, 0);
                    spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, color * 0.3f, -StarlightWorld.rottime, tex.Size() / 2, 2.5f * scale1, 0, 0);
                    spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, color * 0.8f, StarlightWorld.rottime * 2, tex.Size() / 2, 1.2f * scale1, 0, 0);
                }

                spriteBatch.End();
                spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, Filters.Scene["VitricReplicaItem"].GetShader().Shader, Main.GameViewMatrix.ZoomMatrix);
                Filters.Scene["VitricReplicaItem"].GetShader().Shader.Parameters["uTime"].SetValue(StarlightWorld.rottime);
            }

            return true;
        }

        public override void PostDrawInWorld(Item Item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (isReplica)
            {
                spriteBatch.End();
                spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            }
        }

        public override void ModifyTooltips(Item Item, List<TooltipLine> tooltips)
        {
            if (isReplica)
            {
                tooltips.FirstOrDefault(n => n.Name == "ItemName" && n.Mod == "Terraria").text = "Replica " + Item.Name;
            }
        }

        public override bool OnPickup(Item Item, Player Player)
        {
            firstTime = false;
            return true;
        }
    }
}
