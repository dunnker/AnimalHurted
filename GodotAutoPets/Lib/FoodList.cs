using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPets
{
    public class FoodList
    {
        private static FoodList _instance = null;

        public static FoodList Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FoodList();
                }
                return _instance;
            }
        }

        public List<Food> TierOneFood { get; set; }
        public List<Food> TierTwoFood { get; set; }
        public List<Food> TierThreeFood { get; set; }
        public List<Food> TierFourFood { get; set; }
        public List<Food> TierFiveFood { get; set; }
        public List<Food> TierSixFood { get; set; }
        public List<Food> AllFood { get; set; }

        // Tier 1
        public AppleFood AppleFood { get; }
        public HoneyFood HoneyFood { get; }
        // Tier 2
        public CupcakeFood CupcakeFood { get; }
        public MeatBoneFood MeatBoneFood { get; }
        public SleepingPillFood SleepingPillFood { get; }
        // Tier 3
        public GarlicFood GarlicFood { get; }
        public SaladBowlFood SaladBowlFood { get; }
        // Tier 4
        public CannedFoodFood CannedFoodFood { get; }
        public PearFood PearFood { get; }
        // Tier 5
        public ChiliFood ChiliFood { get; }
        public ChocolateFood ChocolateFood { get; }
        public SushiFood SushiFood { get; }
        // Tier 6
        public MelonFood MelonFood { get; }
        public MushroomFood MushroomFood { get; }
        public PizzaFood PizzaFood { get; }
        public SteakFood SteakFood { get; }

        public MilkFood MilkFood { get; }

        private FoodList()
        {
            AllFood = new List<Food>();

            AppleFood = new AppleFood();
            HoneyFood = new HoneyFood();

            TierOneFood = new List<Food>();
            TierOneFood.Add(AppleFood);
            TierOneFood.Add(HoneyFood);

            CupcakeFood = new CupcakeFood();
            MeatBoneFood = new MeatBoneFood();
            SleepingPillFood = new SleepingPillFood();
            
            TierTwoFood = new List<Food>();
            TierTwoFood.Add(CupcakeFood);
            TierTwoFood.Add(MeatBoneFood);
            TierTwoFood.Add(SleepingPillFood);

            GarlicFood = new GarlicFood();
            SaladBowlFood = new SaladBowlFood();

            TierThreeFood = new List<Food>();
            TierThreeFood.Add(GarlicFood);
            TierThreeFood.Add(SaladBowlFood);

            CannedFoodFood = new CannedFoodFood();
            PearFood = new PearFood();

            TierFourFood = new List<Food>();
            TierFourFood.Add(CannedFoodFood);
            TierFourFood.Add(PearFood);

            ChiliFood = new ChiliFood();
            ChocolateFood = new ChocolateFood();
            SushiFood = new SushiFood();

            TierFiveFood = new List<Food>();
            TierFiveFood.Add(ChiliFood);
            TierFiveFood.Add(ChocolateFood);
            TierFiveFood.Add(SushiFood);

            MelonFood = new MelonFood();
            MushroomFood = new MushroomFood();
            PizzaFood = new PizzaFood();
            SteakFood = new SteakFood();
            
            TierSixFood = new List<Food>();
            TierSixFood.Add(MelonFood);
            TierSixFood.Add(MushroomFood);
            TierSixFood.Add(PizzaFood);
            TierSixFood.Add(SteakFood);

            AllFood.AddRange(TierOneFood);
            AllFood.AddRange(TierTwoFood);
            AllFood.AddRange(TierThreeFood);
            AllFood.AddRange(TierFourFood);
            AllFood.AddRange(TierFiveFood);
            AllFood.AddRange(TierSixFood);

            AllFood.Add(MilkFood);
        }
    }
}