using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalHurtedLib
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

        public List<Type> TierOneFood { get; set; }
        public List<Type> TierTwoFood { get; set; }
        public List<Type> TierThreeFood { get; set; }
        public List<Type> TierFourFood { get; set; }
        public List<Type> TierFiveFood { get; set; }
        public List<Type> TierSixFood { get; set; }
        public List<Type> AllFood { get; set; }

        private FoodList()
        {
            AllFood = new List<Type>();

            TierOneFood = new List<Type>();
            TierOneFood.Add(typeof(AppleFood));
            TierOneFood.Add(typeof(HoneyFood));

            TierTwoFood = new List<Type>();
            TierTwoFood.Add(typeof(CupcakeFood));
            TierTwoFood.Add(typeof(MeatBoneFood));
            TierTwoFood.Add(typeof(SleepingPillFood));

            TierThreeFood = new List<Type>();
            TierThreeFood.Add(typeof(GarlicFood));
            TierThreeFood.Add(typeof(SaladBowlFood));

            TierFourFood = new List<Type>();
            TierFourFood.Add(typeof(CannedFoodFood));
            TierFourFood.Add(typeof(PearFood));

            TierFiveFood = new List<Type>();
            TierFiveFood.Add(typeof(ChiliFood));
            TierFiveFood.Add(typeof(ChocolateFood));
            TierFiveFood.Add(typeof(SushiFood));

            TierSixFood = new List<Type>();
            TierSixFood.Add(typeof(MelonFood));
            TierSixFood.Add(typeof(MushroomFood));
            TierSixFood.Add(typeof(PizzaFood));
            TierSixFood.Add(typeof(SteakFood));

            AllFood.AddRange(TierOneFood);
            AllFood.AddRange(TierTwoFood);
            AllFood.AddRange(TierThreeFood);
            AllFood.AddRange(TierFourFood);
            AllFood.AddRange(TierFiveFood);
            AllFood.AddRange(TierSixFood);

            AllFood.Add(typeof(MilkFood));
        }
    }
}