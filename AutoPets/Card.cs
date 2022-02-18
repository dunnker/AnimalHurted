using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AutoPets
{
    public enum CardState { Idle, Attacked, Hurt, Fainted, Summoned, Buffed, Bought, Sold, GainedXP, LeveledUp }

    public class Card
    {
        readonly Deck _deck;
        readonly Ability _ability;
        int _index;
        int _hitPoints;
        int _attackPoints;
        int _xp;
        CardState _state = CardState.Idle;

        public Card(Deck deck, Ability ability) : base()
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
        /// <param name="index"></param>
        /// <param name="card"></param>
        public Card(Deck deck, Card card)
        {
            _ability = card._ability;
            _deck = deck;
            _index = -1;
            _hitPoints = card._hitPoints;
            _attackPoints = card._attackPoints;
            _xp = card._xp;
		}

        public Deck Deck
        {
            get { return _deck; }
        }

        public int Index { get { return _index; } }

        public Ability Ability { get { return _ability; } }

        public int HitPoints
        {
            get { return _hitPoints; }
            set { _hitPoints = value; }
        }

        public int AttackPoints
        {
            get { return _attackPoints; }
            set { _attackPoints = value; }
        }

        public void FightOne(Card card)
        {
            Attack(card);
            card.Attack(this);

            int cardDamage = card.GetDamage();
            _hitPoints -= cardDamage;
            int damage = GetDamage();
            card._hitPoints -= damage;

            _deck.Player.Game.OnFightEvent();

            // not calling this.Hurt() since it will invoke Hurt event
            // users get a OnFightEvent instead of Hurt event
            // but we still need to call _ability.Hurt: 
            if (cardDamage > 0)
                _ability.Hurt(this);
            if (damage > 0)
                card._ability.Hurt(card);

            if (_hitPoints <= 0)
                Faint();
            if (card._hitPoints <= 0)
                card.Faint();
        }

        public int GetDamage()
        {
            return _attackPoints;
        }

        public int XP { get { return _xp; } }

        public int Level
        {
            get 
            {
                if (_xp < 3)
                    return 1;
                else if (_xp >= 3 && _xp < 6 )
                    return 2;
                else
                    return 3;
            }
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

        public CardState State { get { return _state; } }

        public void SetIndex(int index)
        {
            _index = index;
        }

        // Ability methods

        public void BattleStarted()
        {
            _ability.BattleStarted(this);
        }

        public void ResetState()
        {
            _state = CardState.Idle;
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

            _state = CardState.GainedXP;
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
                _state = CardState.LeveledUp;
                _ability.LeveledUp(this);
            }
        }

        public void Summon(int atIndex)
        {
            if (_deck[atIndex] != null)
                throw new Exception(string.Format("A card already exists at {0}", atIndex));
            _deck.SetCard(this, atIndex);
            _state = CardState.Summoned;
            _deck.Player.Game.OnCardSummonedEvent(this, atIndex);
            _ability.Summoned(this);
            foreach (var card in Deck)
            {
                if (card != this)
                    card._ability.FriendSummoned(card, this);
            }
        }

        public void Faint()
        {
            int saveIndex = _index;
            _deck.Remove(_index);
            _state = CardState.Fainted;
            _deck.Player.Game.OnCardFaintedEvent(this, saveIndex);
            _ability.Fainted(this, saveIndex);
        }

        public void Buy(int atIndex)
        {
            Summon(atIndex);
            _state = CardState.Bought;
            _ability.Bought(this);
        }

        public void Sell()
        {
            int saveIndex = _index;
            // remove first in case ability spawns something in place, see also Faint()
            _deck.Remove(_index);
            _deck.Player.Gold += Level;
            _state = CardState.Sold;
            _ability.Sold(this, saveIndex);
        }

        public void Attack(Card card)
        {
            _ability.BeforeAttack(this);
            _state = CardState.Attacked;
            _ability.Attacked(this);
        }

        public void Hurt(int damage, Card sourceCard)
        {
            if (damage > 0)
            {
                _state = CardState.Hurt;
                _hitPoints -= damage;
                _deck.Player.Game.OnCardHurtEvent(this, sourceCard);
                _ability.Hurt(this);
            }
        }

        public void Buff(int sourceIndex, int hitPoints, int attackPoints)
        {
            Debug.Assert(hitPoints >= 0 && attackPoints >= 0);
            _hitPoints += hitPoints;
            _attackPoints += attackPoints;
            _state = CardState.Buffed;
            _deck.Player.Game.OnCardBuffedEvent(this, sourceIndex);
            _ability.Buffed(this, sourceIndex, hitPoints, attackPoints); 
        }
    }
}
