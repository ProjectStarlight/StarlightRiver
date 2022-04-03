using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Palestone
{
	[AutoloadEquip(EquipType.Head)]
    public class PalestoneHead : ModItem
    {
        public override string Texture => AssetDirectory.PalestoneItem + "PalestoneHead";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palestone Helmet");
            Tooltip.SetDefault("5% increased building speed");
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 22;
            Item.value = Item.sellPrice(0, 0, 2, 0);
            Item.defense = 1;
        }

        public override void UpdateEquip(Player Player)
        {
            Player.tileSpeed += 5;
            Player.wallSpeed += 5;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class PalestoneChest : ModItem
    {
        public override string Texture => AssetDirectory.PalestoneItem + "PalestoneChest";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palestone Chestplate");
            Tooltip.SetDefault("1% increased melee speed");
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 22;
            Item.value = Item.sellPrice(0, 0, 1, 50);
            Item.defense = 2;
        }
        public override void UpdateEquip(Player Player)
        {
            Player.meleeSpeed += 1;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ItemType<PalestoneHead>() && body.type == ItemType<PalestoneChest>() && legs.type == ItemType<PalestoneLegs>();
        }
        public override void UpdateArmorSet(Player Player)
        {
            Player.setBonus = "10% increased mining and building speed";
            Player.pickSpeed += 10;
            Player.tileSpeed += 10;
            Player.wallSpeed += 10;
            //PalestonePlayer palestonePlayer = Player.GetModPlayer<PalestonePlayer>();
            //foreach (int i in palestonePlayer.tablets)
            //    if (i > 0)
            //        Player.endurance += 0.1f;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class PalestoneLegs : ModItem
    {
        public override string Texture => AssetDirectory.PalestoneItem + "PalestoneLegs";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palestone Leggings");
            Tooltip.SetDefault("Slightly increased movement speed");
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 20;
            Item.value = Item.sellPrice(0, 0, 1, 25);
            Item.defense = 1;
        }

        public override void UpdateEquip(Player Player)
        {
            Player.moveSpeed += 0.05f;
        }
    }

    /*
public class PalestonePlayer : ModPlayer
{
    public float counter = 0;
    public int[] tablets = new int[3];
    public override void OnHitNPC(Item Item, NPC target, int damage, float knockback, bool crit)
    {
        if (Item.melee && target.life <= 0)
            for (int i = 0; i < tablets.Length; i++)
                if (tablets[i] == 0)
                {
                    tablets[i] = 3;
                    break;
                }
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
    {
        if (proj.melee && target.life <= 0)
            for (int i = 0; i < tablets.Length; i++)
                if (tablets[i] == 0)
                {
                    tablets[i] = 3;
                    break;
                }
    }

    public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
    {
        for (int i = 0; i < tablets.Length; i++)
            if (tablets[i] > 0)
                tablets[i]--;
    }

    public override void ModifyDrawLayers(List<PlayerLayer> layers)
    {
        Action<PlayerDrawInfo> backTarget = s => DrawGlowmasks(s, false); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
        PlayerLayer backLayer = new PlayerLayer("PalestoneLayer", "Armor Glowmask", backTarget); //Instantiate a new instance of PlayerLayer to insert into the list
        layers.Insert(layers.IndexOf(layers.First()), backLayer); //Insert the layer at the appropriate index. 

        Action<PlayerDrawInfo> frontTarget = s => DrawGlowmasks(s, true); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
        PlayerLayer frontLayer = new PlayerLayer("PalestoneLayer", "Armor Glowmask", frontTarget); //Instantiate a new instance of PlayerLayer to insert into the list
        layers.Insert(layers.IndexOf(layers.Last()), frontLayer); //Insert the layer at the appropriate index. 

        float getTabletRotation(int currentTablet) => currentTablet / (tablets.FirstOrDefault(x => x == 0) + 1) * 6.28f + (float)Player.GetModPlayer<StarlightPlayer>().Timer % 120 / 120 * 6.28f;
        Vector2 getTabletPosition(int currentTablet)
        {
            float dist = 50;
            float rot = getTabletRotation(currentTablet);

            float posX = Player.Center.X + (float)(Math.Cos(rot) * dist);
            float posY = Player.Center.Y + (float)(Math.Sin(rot) * dist) / 2;
            return new Vector2(posX, posY);
        }
        void DrawGlowmasks(PlayerDrawInfo info, bool back)
        {
            for (int k = 0; k < tablets.Length; k++)
            {
                float rot = getTabletRotation(k);
                if ((back && rot % 6.28f < 3.14f || !back && rot % 6.28f >= 3.14f) && tablets[k] > 0)
                {
                    Vector2 pos = getTabletPosition(k);
                    Texture2D texture = Request<Texture2D>("StarlightRiver/Assets/Items/Palestone/Tablet").Value;
                    Main.playerDrawData.Add(new DrawData(
                        texture,
                        pos,  //position
                        new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)), //source
                        Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16), //color
                        0, //rotation
                        new Vector2(texture.Width / 2, texture.Height / 2), //origin
                        1f, //scale
                        SpriteEffects.None, 0));
                }
            }
        }
    }
}
*/

    /*
    internal class PalestoneArmorProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.PalestoneItem + "PalestoneArmorProjectile";
        private int MaxCharge = 300;
        private Vector2 LastLocation;
        public static int MaxTablets = 3;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Stone Tablet");
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item27);
        }

        public override bool CanDamage()
        {
            return false;
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            Projectile.localAI[0] = (float)reader.ReadDouble();
        }

        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            writer.Write((double)Projectile.localAI[0]);
        }

        public PalestoneArmorProjectile()
        {
            LastLocation = Vector2.One;
        }

        public override void AI()
        {
            NPC target = Main.npc[(int)Projectile.ai[1]];
            Player owner = Projectile.Owner();
            bool conditions = (owner != null && owner.active) && owner.Distance(Projectile.Center) < 300;//Grow or no?
            bool slamming = Projectile.ai[0] > MaxCharge;


            if (slamming)
            {
                Slam();
                return;
            }

            if (Helper.IsTargetValid(target) && Projectile.ai[0] >= 0)
            {
                LastLocation = target.position + new Vector2(target.width / 2, -96);
                Projectile.ai[0] += conditions ? 1 : -1;
            }
            else
            {
                Projectile.Kill();
            }

            Projectile.Center = LastLocation;

            void Slam()
            {
                Projectile.ai[0] += 1;
                //target.
            }

        }
    }*/
}