using StarlightRiver.Core;

namespace StarlightRiver.Packets
{
    //[Serializable]
    //public class StartAbility : Module
    //{
    //    public StartAbility(int fromWho, Ability ability)
    //    {
    //        this.fromWho = fromWho;
    //        abTypeName = ability.GetType().FullName;
    //    }

    //    private readonly int fromWho;
    //    private readonly string abTypeName;

    //    protected override void Receive()
    //    {
    //        if (Main.netMode == Terraria.ID.NetmodeID.Server)
    //        {
    //            Send(-1, fromWho, true);
    //            return;
    //        }

    //        Player player = Main.player[fromWho];
    //        AbilityHandler handler = player.GetHandler();
    //        DragonHandler dragon = player.GetModPlayer<DragonHandler>();
    //        Type abType = Type.GetType(abTypeName);
    //        Ability ab = handler.Abilities.Single(a => a.GetType() == abType);

    //        //if the player: has enough stamina  && unlocked && not on CD     && Has no other abilities active
    //        if (ab.Available && handler.Stamina >= ab.ActivationCost && !ab.Locked && ab.Cooldown == 0 && !handler.Abilities.Any(a => a.Active))
    //        {
    //            handler.Stamina -= ab.ActivationCost; //Consume the stamina
    //                                                  //if (dragon.DragonMounted) OnCastDragon(); //Do what the ability should do when it starts

    //            ab.Activate();
    //        }
    //    }
    //}
}