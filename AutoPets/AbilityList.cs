using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPets
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

        public List<Ability> TierOneAbilities { get; set; }
        public List<Ability> TierTwoAbilities { get; set; }
        public List<Ability> TierThreeAbilities { get; set; }
        public List<Ability> TierFourAbilities { get; set; }
        public List<Ability> TierFiveAbilities { get; set; }
        public List<Ability> TierSixAbilities { get; set; }
        public List<Ability> AllAbilities { get; set; }

        // Tier 1
        public AntAbility AntAbility { get; }
        public PigAbility PigAbility { get; }
        public DuckAbility DuckAbility { get; }
        public OtterAbility OtterAbility { get; }
        public BeaverAbility BeaverAbility { get; }
        public HorseAbility HorseAbility { get; }
        public FishAbility FishAbility { get; }
        public CricketAbility CricketAbility { get; }
        public MosquitoAbility MosquitoAbility { get; }

        // Tier 2
        public CrabAbility CrabAbility { get; }
        public DodoAbility DodoAbility { get; }
        public ElephantAbility ElephantAbility { get; }
        public FlamingoAbility FlamingoAbility { get; }
        public HedgehogAbility HedgehogAbility { get; }
        public PeacockAbility PeacockAbility { get; }
        public RatAbility RatAbility { get; }
        public ShrimpAbility ShrimpAbility { get; }
        public SpiderAbility SpiderAbility { get; }
        public SwanAbility SwanAbility { get; }

        // Tier 3
        public BadgerAbility BadgerAbility { get; }
        public BlowfishAbility BlowfishAbility { get; }
        public CamelAbility CamelAbility { get; }
        public DogAbility DogAbility { get; }
        public GiraffeAbility GiraffeAbility { get; }
        public KangarooAbility KangarooAbility { get; }
        public OxAbility OxAbility { get; }
        public RabbitAbility RabbitAbility { get; }
        public SheepAbility SheepAbility { get; }
        public SnailAbility SnailAbility { get; }
        public TurtleAbility TurtleAbility { get; }

        // Tier 4
        public BisonAbility BisonAbility { get; }
        public DeerAbility DeerAbility { get; }
        public DolphinAbility DolphinAbility { get; }
        public HippoAbility HippoAbility { get; }
        public ParrotAbility ParrotAbility { get; }
        public PenguinAbility PenguinAbility { get; }
        public RoosterAbility RoosterAbility { get; }
        public SkunkAbility SkunkAbility { get; }
        public SquirrelAbility SquirrelAbility { get; }
        public WhaleAbility WhaleAbility { get; }
        public WormAbility WormAbility { get; }

        // Tier 5
        public CowAbility CowAbility { get; }
        public CrocodileAbility CrocodileAbility { get; }
        public MonkeyAbility MonkeyAbility { get; }
        public RhinoAbility RhinoAbility { get; }
        public ScorpionAbility ScorpionAbility { get; }
        public SealAbility SealAbility { get; }
        public SharkAbility SharkAbility { get; }
        public TurkeyAbility TurkeyAbility { get; }

        // Tier 6
        public BoarAbility BoarAbility { get; }
        public CatAbility CatAbility { get; }
        public DragonAbility DragonAbility { get; }
        public FlyAbility FlyAbility { get; }
        public GorillaAbility GorillaAbility { get; }
        public LeopardAbility LeopardAbility { get; }
        public MammothAbility MammothAbility { get; }
        public SnakeAbility SnakeAbility { get; }
        public TigerAbility TigerAbility { get; }

        public ZombieCricketAbility ZombieCricketAbility { get; }
        public DirtyRatAbility DirtyRatAbility { get; }
        public ZombieBeeAbility ZombieBeeAbility { get; } 
        public ZombieRamAbility ZombieRamAbility { get; } 
        public ZombieBusAbility ZombieBusAbility { get; } 

        private AbilityList()
        {
            AllAbilities = new List<Ability>();

            TierOneAbilities = new List<Ability>();
            TierTwoAbilities = new List<Ability>();
            TierThreeAbilities = new List<Ability>();
            TierFourAbilities = new List<Ability>();
            TierFiveAbilities = new List<Ability>();
            TierSixAbilities = new List<Ability>();

            AntAbility = new AntAbility();
            PigAbility = new PigAbility();
            DuckAbility = new DuckAbility();
            OtterAbility = new OtterAbility();
            BeaverAbility = new BeaverAbility();
            HorseAbility = new HorseAbility();
            FishAbility = new FishAbility();
            CricketAbility = new CricketAbility();
            MosquitoAbility = new MosquitoAbility();

            TierOneAbilities.Add(AntAbility);       //0
            TierOneAbilities.Add(PigAbility);       //1
            TierOneAbilities.Add(DuckAbility);      //2
            TierOneAbilities.Add(OtterAbility);     //3
            TierOneAbilities.Add(BeaverAbility);    //4
            TierOneAbilities.Add(HorseAbility);     //5
            TierOneAbilities.Add(FishAbility);      //6
            TierOneAbilities.Add(CricketAbility);   //7
            TierOneAbilities.Add(MosquitoAbility);  //8

            CrabAbility = new CrabAbility();
            DodoAbility = new DodoAbility();
            ElephantAbility = new ElephantAbility();
            FlamingoAbility = new FlamingoAbility();
            HedgehogAbility = new HedgehogAbility();
            PeacockAbility = new PeacockAbility();
            RatAbility = new RatAbility();
            ShrimpAbility = new ShrimpAbility();
            SpiderAbility = new SpiderAbility();
            SwanAbility = new SwanAbility();

            TierTwoAbilities.Add(CrabAbility);      //9
            TierTwoAbilities.Add(DodoAbility);      //10
            TierTwoAbilities.Add(ElephantAbility);  //11
            TierTwoAbilities.Add(FlamingoAbility);  //12
            TierTwoAbilities.Add(HedgehogAbility);  //13
            TierTwoAbilities.Add(PeacockAbility);   //14
            TierTwoAbilities.Add(RatAbility);       //15
            TierTwoAbilities.Add(ShrimpAbility);    //16
            TierTwoAbilities.Add(SpiderAbility);    //17
            TierTwoAbilities.Add(SwanAbility);      //18

            BadgerAbility = new BadgerAbility();
            BlowfishAbility = new BlowfishAbility();
            CamelAbility = new CamelAbility();
            DogAbility = new DogAbility();
            GiraffeAbility = new GiraffeAbility();
            KangarooAbility = new KangarooAbility();
            OxAbility = new OxAbility();
            RabbitAbility = new RabbitAbility();
            SheepAbility = new SheepAbility();
            SnailAbility = new SnailAbility();
            TurtleAbility = new TurtleAbility();

            TierThreeAbilities.Add(BadgerAbility);
            TierThreeAbilities.Add(BlowfishAbility);
            TierThreeAbilities.Add(CamelAbility);
            TierThreeAbilities.Add(DogAbility);
            TierThreeAbilities.Add(GiraffeAbility);
            TierThreeAbilities.Add(KangarooAbility);
            TierThreeAbilities.Add(OxAbility);
            TierThreeAbilities.Add(RabbitAbility);
            TierThreeAbilities.Add(SheepAbility);
            TierThreeAbilities.Add(SnailAbility);
            TierThreeAbilities.Add(TurtleAbility);

            BisonAbility = new BisonAbility();
            DeerAbility = new DeerAbility();
            DolphinAbility = new DolphinAbility();
            HippoAbility = new HippoAbility();
            ParrotAbility = new ParrotAbility();
            PenguinAbility = new PenguinAbility();
            RoosterAbility = new RoosterAbility();
            SkunkAbility = new SkunkAbility();
            SquirrelAbility = new SquirrelAbility();
            WhaleAbility = new WhaleAbility();
            WormAbility = new WormAbility();

            TierFourAbilities.Add(BisonAbility);
            TierFourAbilities.Add(DeerAbility);
            TierFourAbilities.Add(DolphinAbility);
            TierFourAbilities.Add(HippoAbility);
            TierFourAbilities.Add(ParrotAbility);
            TierFourAbilities.Add(PenguinAbility);
            TierFourAbilities.Add(RoosterAbility);
            TierFourAbilities.Add(SkunkAbility);
            TierFourAbilities.Add(SquirrelAbility);
            TierFourAbilities.Add(WhaleAbility);
            TierFourAbilities.Add(WormAbility);

            CowAbility = new CowAbility();
            CrocodileAbility = new CrocodileAbility();
            MonkeyAbility = new MonkeyAbility();
            RhinoAbility = new RhinoAbility();
            ScorpionAbility = new ScorpionAbility();
            SealAbility = new SealAbility();
            SharkAbility = new SharkAbility();
            TurkeyAbility = new TurkeyAbility();

            TierFiveAbilities.Add(CowAbility);
            TierFiveAbilities.Add(CrocodileAbility);
            TierFiveAbilities.Add(MonkeyAbility);
            TierFiveAbilities.Add(RhinoAbility);
            TierFiveAbilities.Add(ScorpionAbility);
            TierFiveAbilities.Add(SealAbility);
            TierFiveAbilities.Add(SharkAbility);
            TierFiveAbilities.Add(TurkeyAbility);

            BoarAbility = new BoarAbility();
            CatAbility = new CatAbility();
            DragonAbility = new DragonAbility();
            FlyAbility = new FlyAbility();
            GorillaAbility = new GorillaAbility();
            LeopardAbility = new LeopardAbility();
            MammothAbility = new MammothAbility();
            SnakeAbility = new SnakeAbility();
            TigerAbility = new TigerAbility();

            TierSixAbilities.Add(BoarAbility);
            TierSixAbilities.Add(CatAbility);
            TierSixAbilities.Add(DragonAbility);
            TierSixAbilities.Add(FlyAbility);
            TierSixAbilities.Add(GorillaAbility);
            TierSixAbilities.Add(LeopardAbility);
            TierSixAbilities.Add(MammothAbility);
            TierSixAbilities.Add(SnakeAbility);
            TierSixAbilities.Add(TigerAbility);

            AllAbilities.AddRange(TierOneAbilities);
            AllAbilities.AddRange(TierTwoAbilities);
            AllAbilities.AddRange(TierThreeAbilities);
            AllAbilities.AddRange(TierFourAbilities);
            AllAbilities.AddRange(TierFiveAbilities);
            AllAbilities.AddRange(TierSixAbilities);

            ZombieCricketAbility = new ZombieCricketAbility();
            AllAbilities.Add(ZombieCricketAbility);
            DirtyRatAbility = new DirtyRatAbility();
            AllAbilities.Add(DirtyRatAbility);
            ZombieBeeAbility = new ZombieBeeAbility();
            AllAbilities.Add(ZombieBeeAbility);
            ZombieRamAbility = new ZombieRamAbility();
            AllAbilities.Add(ZombieRamAbility);
            ZombieBusAbility = new ZombieBusAbility();
            AllAbilities.Add(ZombieBusAbility);
        }
    }
}
