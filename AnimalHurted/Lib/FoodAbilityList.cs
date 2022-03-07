using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalHurtedLib
{
    public class FoodAbilityList
    {
        private static FoodAbilityList _instance = null;

        public static FoodAbilityList Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FoodAbilityList();
                }
                return _instance;
            }
        }

        public List<Type> AllAbilities { get; set; }

        private FoodAbilityList()
        {
            AllAbilities = new List<Type>();
            AllAbilities.Add(typeof(HoneyBeeAbility));
            AllAbilities.Add(typeof(BoneAttackAbility));
            AllAbilities.Add(typeof(GarlicArmorAbility));
            AllAbilities.Add(typeof(MelonArmorAbility));
            AllAbilities.Add(typeof(SplashAttackAbility));
            AllAbilities.Add(typeof(CoconutShieldAbility));
            AllAbilities.Add(typeof(SteakAttackAbility));
            AllAbilities.Add(typeof(ExtraLifeAbility));
        }
    }
}