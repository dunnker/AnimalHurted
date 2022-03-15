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

        public List<Type> TierTwoFoodPlus { get; }
        public List<Type> TierThreeFoodPlus { get; }
        public List<Type> TierFourFoodPlus { get; }
        public List<Type> TierFiveFoodPlus { get; }
        public List<Type> TierSixFoodPlus { get; }

        public List<Type> AllFood { get; set; }

        public List<Type> GetFoodListForRound(int round)
        {
            switch (round)
            {
                case int i when i >= 1 && i <= 2:
                    return TierOneFood;
                case int i when i >= 3 && i <= 4:
                    return FoodList.Instance.TierTwoFoodPlus;
                case int i when i >= 5 && i <= 6:
                    return FoodList.Instance.TierThreeFoodPlus;
                case int i when i >= 7 && i <= 8:
                    return FoodList.Instance.TierFourFoodPlus;
                case int i when i >= 9 && i <= 10:
                    return FoodList.Instance.TierFiveFoodPlus;
                case int i when i >= 11:
                    return FoodList.Instance.TierSixFoodPlus;
                default:
                    throw new Exception("Invalid round");
            }
        }

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

            TierTwoFoodPlus = new List<Type>();
            TierTwoFoodPlus.AddRange(TierOneFood);
            TierTwoFoodPlus.AddRange(TierTwoFood);

            TierThreeFoodPlus = new List<Type>();
            TierThreeFoodPlus.AddRange(TierTwoFoodPlus);
            TierThreeFoodPlus.AddRange(TierThreeFood);

            TierFourFoodPlus = new List<Type>();
            TierFourFoodPlus.AddRange(TierThreeFoodPlus);
            TierFourFoodPlus.AddRange(TierFourFood);

            TierFiveFoodPlus = new List<Type>();
            TierFiveFoodPlus.AddRange(TierFourFoodPlus);
            TierFiveFoodPlus.AddRange(TierFiveFood);

            TierSixFoodPlus = new List<Type>();
            TierSixFoodPlus.AddRange(TierFiveFoodPlus);
            TierSixFoodPlus.AddRange(TierSixFood);

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