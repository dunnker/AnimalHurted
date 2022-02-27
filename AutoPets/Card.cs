using System;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace AutoPets
{
    public class Card
    {
        readonly Deck _deck;
        Ability _ability;
        FoodAbility _foodAbility;
        int _index;
        int _hitPoints;
        int _attackPoints;
        int _xp;
        int _buildHitPoints;
        int _buildAttackPoints;
        
        public Card(Deck deck)
        {
            _deck = deck;
            _index = -1;
            _xp = 1;
        }

        public Card(Deck deck, Ability ability)
        {
            _deck = deck;
            _index = -1;
            _xp = 1;
            _ability = ability;
            _hitPoints = _ability.DefaultHP;
            _attackPoints = _ability.DefaultAttack;
        }

        /// <summary>
        /// Clone an existing card. For example, to create a battle card from a build card. Or to create a build card from a shop card.
        /// </summary>
        /// <param name="deck"></param>
        /// <param name="card"></param>
        public Card(Deck deck, Card card)
        {
            _ability = card._ability;
            _foodAbility = card._foodAbility;
            _deck = deck;
            _index = -1;
            _hitPoints = card._hitPoints;
            _attackPoints = card._attackPoints;
            _buildHitPoints = card._buildHitPoints;
            _buildAttackPoints = card._buildAttackPoints;
            _xp = card._xp;
		}

        public Deck Deck
        {
            get { return _deck; }
        }

        public int Index { get { return _index; } }

        public Ability Ability { get { return _ability; } }

        public FoodAbility FoodAbility { get { return _foodAbility; } set { _foodAbility = value; } }

        public int HitPoints { get { return _hitPoints; } set { _hitPoints = value; } }

        public int AttackPoints { get { return _attackPoints; } set { _attackPoints = value; } }

        // additional hit points that have been acquired during the build but are reset
        // after a battle. For instance, from a cupkake
        public int BuildHitPoints { get { return _buildHitPoints; } set { _buildHitPoints = value; } }

        // see comments on BuildHitPoints
        public int BuildAttackPoints { get { return _buildAttackPoints; } set { _buildAttackPoints = value; } }

        public int TotalHitPoints { get { return _buildHitPoints + _hitPoints; } }
        public int TotalAttackPoints { get { return _buildAttackPoints + _attackPoints; } }

        public int XP { get { return _xp; } set { _xp = value; } }

        public int Level
        {
            get 
            {
                // 1,2 = 1
                // 3,4,5 = 2
                // 6+ = 3
                return (_xp / 3) + 1;
            }
        }

        public static int GetXPFromLevel(int level)
        {
            // xp starts at 1, so ensure it's at least 1
            return Math.Max(1, (level - 1) * 3);
        }

        public int XPRemainder
        {
            get
            {
                if (_xp < 3)
                    return _xp - 1;
                else if (_xp >= 3 && _xp < 6)
                {
                    Math.DivRem(_xp, 3, out int result);
                    return result;
                }
                else
                    return 0;
            }
        }

        public void SaveToStream(StreamWriter writer)
        {
            // version number of this stream; used to support backward compatibility
            // if stream format changes later
            writer.WriteLine(1);
            writer.WriteLine(_index);
            writer.WriteLine(_xp);
            writer.WriteLine(_hitPoints);
            writer.WriteLine(_attackPoints);
            writer.WriteLine(_buildHitPoints);
            writer.WriteLine(_buildAttackPoints);
            if (_ability == null)
                writer.WriteLine(string.Empty);
            else
                writer.WriteLine(_ability.GetType().Name);
            if (_foodAbility == null)
                writer.WriteLine(string.Empty);
            else
                writer.WriteLine(_foodAbility.GetType().Name);
        }

        public void LoadFromStream(StreamReader reader)
        {
            int version = Int32.Parse(reader.ReadLine());
            if (version != 1)
                throw new Exception("Invalid stream version");
            _index = Int32.Parse(reader.ReadLine());
            _xp = Int32.Parse(reader.ReadLine());
            _hitPoints = Int32.Parse(reader.ReadLine());
            _attackPoints = Int32.Parse(reader.ReadLine());
            _buildHitPoints = Int32.Parse(reader.ReadLine());
            _buildAttackPoints = Int32.Parse(reader.ReadLine());
            string abilityName = reader.ReadLine();
            _ability = AbilityList.Instance.AllAbilities.FirstOrDefault((a) => a.GetType().Name == abilityName );
            string foodAbilityName = reader.ReadLine();
            if (!string.IsNullOrEmpty(foodAbilityName))
                _foodAbility = Activator.CreateInstance(
                    Type.GetType($"AutoPets.{foodAbilityName}, AutoPets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")) as FoodAbility;
        }

        public void Attack(CardCommandQueue queue, Card card)
        {
            _ability.BeforeAttack(queue, this);
            card._ability.BeforeAttack(queue, card);

            int opponentDamage = card.GetDamage();
            int damage = GetDamage();

            queue.Add(new AttackCardCommand(this, damage, card, opponentDamage));
        }

        public int GetDamage()
        {
            int damage = TotalAttackPoints;
            _foodAbility?.Attacking(this, ref damage);
            return damage;
        }

        public void SetIndex(int index)
        {
            _index = index;
        }

        public void NewRound()
        {
            _buildAttackPoints = 0;
            _buildHitPoints = 0;
            _ability.RoundStarted(this);
        }

        public void BuildEnded(CardCommandQueue queue)
        {
            _ability.RoundEnded(queue, this);
        }

        public void GainXP(Card fromCard)
        {
            //TODO: what if already at Level 3?
            //TODO: merge food

            // take the higher hitpoints from the two merging cards
            var baseCard = this;
            var buffCard = fromCard;
            //TODO: check only hitpoints?
            if (fromCard._hitPoints > baseCard._hitPoints)
            {
                baseCard = fromCard;
                buffCard = this;
            }

            _hitPoints = baseCard._hitPoints;
            _attackPoints = baseCard._attackPoints;

            // a test case to consider is leveling up 6 ducks (each are hp 1)
            // if you level them up serially to create one level 3 duck, it will be a hp 6 duck
            // if you level them up to create two level 2 ducks, then combine those two ducks to make one level 3 duck, you should still end up with a hp 6 duck

            // buffCard has hp that has accumulated due to gaining xp
            // so we're preserving that accumulated hp in the new merged card
            _hitPoints += buffCard._xp;
            _attackPoints += buffCard._xp;

            int oldLevel = Level;
            int oldXP = _xp;
            _xp += fromCard.XP; //TODO if we put a cap on the amount of xp accumulated, then we need to use that accumulated value in the hitpoints calculation above

            // Level is now the combined level

            //TODO ability method for gaining xp for reporting to UI

            // Note fromCard might be a ShopCard, so this will remove it from the shop
            // Also, removing this now before calling ability method because we don't want fromCard to be affected
            // e.g. see Fish ability
            fromCard.Deck.Remove(fromCard.Index);

            if (Level > oldLevel)
            {
                _ability.LeveledUp(this);
            }
        }

        public void Summon(int atIndex)
        {
            if (_deck[atIndex] != null)
                throw new Exception(string.Format("A card already exists at {0}", atIndex));
            _deck.SetCard(this, atIndex);
            _deck.Player.Game.OnCardSummonedEvent(this, atIndex);
        }

        public void Faint()
        {
            int saveIndex = _index;
            _deck.Remove(_index);
            _deck.Player.Game.OnCardFaintedEvent(this, saveIndex);
        }

        public void Buy(int atIndex)
        {
            Summon(atIndex);
            _ability.Bought(this);
            foreach (var card in _deck)
                if (card != this)
                    card._ability.FriendBought(card, this);
        }

        public void Sell()
        {
            int saveIndex = _index;
            // remove first in case ability spawns something in place, see also Faint()
            _deck.Remove(_index);
            _deck.Player.Gold += Level;
            _ability.Sold(this, saveIndex);
            foreach (var c in _deck)
                if (c != this)
                    c._ability.FriendSold(c, this);
        }

        public void Hurt(int damage, Deck sourceDeck, int sourceIndex)
        {
            if (damage > 0)
            {
                if (_foodAbility != null)
                    _foodAbility.Hurting(this, ref damage);
                _hitPoints -= damage;
                _deck.Player.Game.OnCardHurtEvent(this, sourceDeck, sourceIndex);
            }
        }

        public void Buff(int sourceIndex, int hitPoints, int attackPoints)
        {
            Debug.Assert(hitPoints >= 0 && attackPoints >= 0);
            _hitPoints += hitPoints;
            _attackPoints += attackPoints;
            _deck.Player.Game.OnCardBuffedEvent(this, sourceIndex);
        }

        public void Eat(CardCommandQueue queue, Food food)
        {
            food.Execute(queue, this);
            _ability.AteFood(this);
            foreach (var card in _deck)
                if (card != this)
                    card._ability.FriendAteFood(card, this);
        }
    }
}
