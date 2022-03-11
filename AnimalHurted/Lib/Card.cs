using System;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using System.IO;

namespace AnimalHurtedLib
{
    public class Card
    {
        readonly Deck _deck;
        Ability _ability;
        Ability _renderAbility;
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
            _renderAbility = ability;
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
			// not creating new instances of ability classes because of performance of Activator.CreateInstance
			// ability classes do keep state, but it's minimal, and if becomes an issue we can create an Init
			// method to invoke here
            _ability = card._ability;
            _renderAbility = card._renderAbility;
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

        public Ability Ability { get { return _ability; } set { _ability = value; } }
        
        public Ability RenderAbility { get { return _renderAbility; } set { _renderAbility = value; }  }

        public FoodAbility FoodAbility { get { return _foodAbility; } set { _foodAbility = value; } }

        public int HitPoints { get { return _hitPoints; } set { _hitPoints = value; } }

        public int AttackPoints { get { return _attackPoints; } set { _attackPoints = value; } }

        // additional hit points that have been acquired during the build but are reset
        // after a battle. For instance, from a cupcake
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

        public void SaveToStream(BinaryWriter writer)
        {
            // version number of this stream; used to support backward compatibility
            // if stream format changes later
            writer.Write(1);
            writer.Write(_index);
            writer.Write(_xp);
            writer.Write(_hitPoints);
            writer.Write(_attackPoints);
            writer.Write(_buildHitPoints);
            writer.Write(_buildAttackPoints);
            writer.Write(_ability != null ? _ability.GetType().Name : string.Empty);
            writer.Write(_renderAbility != null ? _renderAbility.GetType().Name : string.Empty);
            writer.Write(_foodAbility != null ? _foodAbility.GetType().Name : string.Empty);
        }

        public void LoadFromStream(BinaryReader reader)
        {
            int version = reader.ReadInt32();
            if (version != 1)
                throw new Exception("Invalid stream version");
            _index = reader.ReadInt32();
            _xp = reader.ReadInt32();
            _hitPoints = reader.ReadInt32();
            _attackPoints = reader.ReadInt32();
            _buildHitPoints = reader.ReadInt32();
            _buildAttackPoints = reader.ReadInt32();
            string abilityName = reader.ReadString();
            if (!string.IsNullOrEmpty(abilityName))
                _ability = Activator.CreateInstance(Type.GetType($"AnimalHurtedLib.{abilityName}, AnimalHurted, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")) as Ability;
            abilityName = reader.ReadString();
            if (!string.IsNullOrEmpty(abilityName))
                _renderAbility = Activator.CreateInstance(Type.GetType($"AnimalHurtedLib.{abilityName}, AnimalHurted, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")) as Ability;
            abilityName = reader.ReadString();
            if (!string.IsNullOrEmpty(abilityName))
                _foodAbility = Activator.CreateInstance(
                    Type.GetType($"AnimalHurtedLib.{abilityName}, AnimalHurted, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")) as FoodAbility;
        }

        public int GetDamage()
        {
            int damage = TotalAttackPoints;
            _foodAbility?.CalculatingDamage(this, ref damage);
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

        public void Attacking(CardCommandQueue queue)
        {
            _ability.BeforeAttack(queue, this);
        }

        public void Attack(ref int damage)
        {
            _foodAbility?.Hurting(this, ref damage);
            _hitPoints -= damage;
        }

        public void Attacked(CardCommandQueue queue, int damage, int opponentDamage, Card opponentCard = null)
        {
            var priorCard = _deck.LastOrDefault(c => c != null && c.Index < _index && c.TotalHitPoints > 0);
            priorCard?.Ability.FriendAheadAttacks(queue, priorCard);

            // invoke even if TotalHitPoints is <= 0, because, for example, Splash attack should still apply
            _foodAbility?.Attacking(queue, this, opponentCard);

            _ability.Attacked(queue, this, damage, opponentCard);

            if (opponentDamage > 0)
                Hurted(queue, true, opponentCard);
        }

        public void Summon(int atIndex)
        {
            if (_deck[atIndex] != null)
                throw new Exception(string.Format("A card already exists at {0}", atIndex));
            _deck.SetCard(this, atIndex);
        }

        public void Summoned(CardCommandQueue queue)
        {
            foreach (var c in _deck)
            {
                if (c != this && c.TotalHitPoints > 0)
                    c.Ability.FriendSummoned(queue, c, this);
            }
        }

        public void Faint()
        {
            _deck.Remove(_index);
        }

        public void Fainted(CardCommandQueue queue, int index, bool attacking, Card opponentCard = null)
        {
            // note the use of local param "index" and not "_index/Index"
            var priorCard = _deck.LastOrDefault(c => c.Index < index && c.TotalHitPoints > 0);
            priorCard?.Ability.FriendAheadFaints(queue, priorCard, index);
            _ability.Fainted(queue, this, index, attacking, opponentCard);
            _foodAbility?.Fainted(queue, this, index);
            foreach (var c in _deck)
            {
                if (c.TotalHitPoints > 0)
                    c.Ability.FriendFaints(queue, c, index);
            }
        }

        public void Buy(int index)
        {
            Summon(index);
        }

        public void Bought(CardCommandQueue queue)
        {
            _ability.Bought(queue, this);
            foreach (var c in _deck)
                if (c != this)
                {
                    c.Ability.FriendBought(queue, c, this);
                    c.Ability.FriendSummoned(queue, c, this);
                }
        }

        public void Sell()
        {
            _deck.Remove(_index);
            _deck.Player.Gold += Level;
        }

        public void Sold(CardCommandQueue queue, int index)
        {
            _ability.Sold(queue, this, index);
            foreach (var c in _deck)
                if (c != this)
                    c._ability.FriendSold(queue, c, this);
        }

        public void Hurt(int damage, Deck sourceDeck, int sourceIndex)
        {
            if (damage > 0)
            {
                _foodAbility?.Hurting(this, ref damage);
                _hitPoints -= damage;
            }
        }

        public void Hurted(CardCommandQueue queue, bool attacking, Card opponentCard = null)
        {
            if (opponentCard != null && TotalHitPoints <= 0 && opponentCard.TotalHitPoints > 0)
                opponentCard.Ability.Knockout(queue, opponentCard);
            if (TotalHitPoints > 0)
                _ability.Hurted(queue, this);
            if (TotalHitPoints <= 0)
                queue.Add(new FaintCardCommand(this, attacking, opponentCard).Execute());
        }

        public void Buff(int sourceIndex, int hitPoints, int attackPoints)
        {
            Debug.Assert(hitPoints >= 0 && attackPoints >= 0);
            _hitPoints += hitPoints;
            _attackPoints += attackPoints;
        }

        public void Eat(Food food)
        {
            food.Execute(this);
        }

        public void Ate(CardCommandQueue queue, Food food)
        {
            food.ExecuteAbility(queue, this);
            _ability.AteFood(queue, this);
            foreach (var card in _deck)
                if (card != this)
                    card._ability.FriendAteFood(queue, card, this);
        }
    
        public void GainXP(Card fromCard)
        {
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
            // if you level them up to create two level 2 ducks, then combine those two ducks to make one level 3 duck, 
            // you should still end up with a hp 6 duck

            // buffCard has hp that has accumulated due to gaining xp
            // so we're preserving that accumulated hp in the new merged card
            _hitPoints += buffCard._xp;
            _attackPoints += buffCard._xp;

            _xp = Math.Min(6, _xp + fromCard.XP);

            // Level is now the combined level

            //TODO play a sound when xp goes up

            // Note fromCard might be a ShopCard, so this will remove it from the shop
            // Also, removing this now before calling ability method because we don't want fromCard to be affected
            // e.g. see Fish ability
            fromCard.Deck.Remove(fromCard.Index);
        }

        public void GainedXP(CardCommandQueue queue, int oldLevel)
        {
            if (Level > oldLevel)
                _ability.LeveledUp(queue, this);
        }
    }
}
