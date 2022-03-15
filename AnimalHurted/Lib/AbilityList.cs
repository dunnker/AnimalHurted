using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalHurtedLib
{
    public class AbilityList
    {
        private static AbilityList _instance = null;

        public static AbilityList Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AbilityList();
                }
                return _instance;
            }
        }

        public List<Type> TierOneAbilities { get; }
        public List<Type> TierTwoAbilities { get; }
        public List<Type> TierThreeAbilities { get; }
        public List<Type> TierFourAbilities { get; }
        public List<Type> TierFiveAbilities { get; }
        public List<Type> TierSixAbilities { get; }

        public List<Type> TierTwoAbilitiesPlus { get; }
        public List<Type> TierThreeAbilitiesPlus { get; }
        public List<Type> TierFourAbilitiesPlus { get; }
        public List<Type> TierFiveAbilitiesPlus { get; }
        public List<Type> TierSixAbilitiesPlus { get; }

        public List<Type> AllAbilities { get; }

        public List<Type> GetAbilityListForRound(int round)
        {
            switch (round)
            {
                case int i when i >= 1 && i <= 2:
                    return TierOneAbilities;
                case int i when i >= 3 && i <= 4:
                    return TierTwoAbilitiesPlus;
                case int i when i >= 5 && i <= 6:
                    return TierThreeAbilitiesPlus;
                case int i when i >= 7 && i <= 8:
                    return TierFourAbilitiesPlus;
                case int i when i >= 9 && i <= 10:
                    return TierFiveAbilitiesPlus;
                case int i when i >= 11:
                    return TierSixAbilitiesPlus;
                default:
                    throw new Exception("Invalid round");
            }
        }

        private AbilityList()
        {
            TierOneAbilities = new List<Type>();
            TierTwoAbilities = new List<Type>();
            TierThreeAbilities = new List<Type>();
            TierFourAbilities = new List<Type>();
            TierFiveAbilities = new List<Type>();
            TierSixAbilities = new List<Type>();
            AllAbilities = new List<Type>();

            TierOneAbilities.Add(typeof(AntAbility));
            TierOneAbilities.Add(typeof(PigAbility));
            TierOneAbilities.Add(typeof(DuckAbility));
            TierOneAbilities.Add(typeof(OtterAbility));
            TierOneAbilities.Add(typeof(BeaverAbility));
            TierOneAbilities.Add(typeof(HorseAbility));
            TierOneAbilities.Add(typeof(FishAbility)); 
            TierOneAbilities.Add(typeof(CricketAbility));
            TierOneAbilities.Add(typeof(MosquitoAbility));

            TierTwoAbilities.Add(typeof(CrabAbility));    
            TierTwoAbilities.Add(typeof(DodoAbility));    
            TierTwoAbilities.Add(typeof(ElephantAbility));
            TierTwoAbilities.Add(typeof(FlamingoAbility));
            TierTwoAbilities.Add(typeof(HedgehogAbility)); 
            TierTwoAbilities.Add(typeof(PeacockAbility));  
            TierTwoAbilities.Add(typeof(RatAbility));      
            TierTwoAbilities.Add(typeof(ShrimpAbility));   
            TierTwoAbilities.Add(typeof(SpiderAbility));   
            TierTwoAbilities.Add(typeof(SwanAbility));     

            TierThreeAbilities.Add(typeof(BadgerAbility));
            TierThreeAbilities.Add(typeof(BlowfishAbility));
            TierThreeAbilities.Add(typeof(CamelAbility));
            TierThreeAbilities.Add(typeof(DogAbility));
            TierThreeAbilities.Add(typeof(GiraffeAbility));
            TierThreeAbilities.Add(typeof(KangarooAbility));
            TierThreeAbilities.Add(typeof(OxAbility));
            TierThreeAbilities.Add(typeof(RabbitAbility));
            TierThreeAbilities.Add(typeof(SheepAbility));
            TierThreeAbilities.Add(typeof(SnailAbility));
            TierThreeAbilities.Add(typeof(TurtleAbility));

            TierFourAbilities.Add(typeof(BisonAbility));
            TierFourAbilities.Add(typeof(DeerAbility));
            TierFourAbilities.Add(typeof(DolphinAbility));
            TierFourAbilities.Add(typeof(HippoAbility));
            TierFourAbilities.Add(typeof(ParrotAbility));
            TierFourAbilities.Add(typeof(PenguinAbility));
            TierFourAbilities.Add(typeof(RoosterAbility));
            TierFourAbilities.Add(typeof(SkunkAbility));
            TierFourAbilities.Add(typeof(SquirrelAbility));
            TierFourAbilities.Add(typeof(WhaleAbility));
            TierFourAbilities.Add(typeof(WormAbility));

            TierFiveAbilities.Add(typeof(CowAbility));
            TierFiveAbilities.Add(typeof(CrocodileAbility));
            TierFiveAbilities.Add(typeof(MonkeyAbility));
            TierFiveAbilities.Add(typeof(RhinoAbility));
            TierFiveAbilities.Add(typeof(ScorpionAbility));
            TierFiveAbilities.Add(typeof(SealAbility));
            TierFiveAbilities.Add(typeof(SharkAbility));
            TierFiveAbilities.Add(typeof(TurkeyAbility));

            TierSixAbilities.Add(typeof(BoarAbility));
            TierSixAbilities.Add(typeof(CatAbility));
            TierSixAbilities.Add(typeof(DragonAbility));
            TierSixAbilities.Add(typeof(FlyAbility));
            TierSixAbilities.Add(typeof(GorillaAbility));
            TierSixAbilities.Add(typeof(LeopardAbility));
            TierSixAbilities.Add(typeof(MammothAbility));
            TierSixAbilities.Add(typeof(SnakeAbility));
            TierSixAbilities.Add(typeof(TigerAbility));

            TierTwoAbilitiesPlus = new List<Type>();
            TierTwoAbilitiesPlus.AddRange(TierOneAbilities);
            TierTwoAbilitiesPlus.AddRange(TierTwoAbilities);

            TierThreeAbilitiesPlus = new List<Type>();
            TierThreeAbilitiesPlus.AddRange(TierTwoAbilitiesPlus);
            TierThreeAbilitiesPlus.AddRange(TierThreeAbilities);

            TierFourAbilitiesPlus = new List<Type>();
            TierFourAbilitiesPlus.AddRange(TierThreeAbilitiesPlus);
            TierFourAbilitiesPlus.AddRange(TierFourAbilities);

            TierFiveAbilitiesPlus = new List<Type>();
            TierFiveAbilitiesPlus.AddRange(TierFourAbilitiesPlus);
            TierFiveAbilitiesPlus.AddRange(TierFiveAbilities);

            TierSixAbilitiesPlus = new List<Type>();
            TierSixAbilitiesPlus.AddRange(TierFiveAbilitiesPlus);
            TierSixAbilitiesPlus.AddRange(TierSixAbilities);

            AllAbilities.AddRange(TierSixAbilitiesPlus);
            AllAbilities.Add(typeof(ZombieBeeAbility));
            AllAbilities.Add(typeof(ZombieBusAbility));
            AllAbilities.Add(typeof(ZombieChickAbility));
            AllAbilities.Add(typeof(ZombieCricketAbility));
            AllAbilities.Add(typeof(ZombieFlyAbility));
            AllAbilities.Add(typeof(ZombieRamAbility));
            AllAbilities.Add(typeof(DirtyRatAbility));
        }
    }
}
