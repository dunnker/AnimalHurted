using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnimalHurtedLib
{
    public class CardCommand
    {
        int _index;
        Deck _deck;

        public Deck Deck { get { return _deck; } }

        public Card Card { get { return _deck[_index]; } }

        public int Index { get { return _index; } }

        public override string ToString()
        {
            return $"{_index} {GetType().ToString()}";
        }

        public CardCommand(Deck deck)
        {
            _deck = deck;
        }

        public CardCommand(Card card)
        {
            // deck might be cleared later, and new card references added to the deck
            // so we don't keep a reference to the card, but only a reference to the deck 
            _index = card.Index;
            _deck = card.Deck;
        }

        public virtual void CardMoving(Deck deck, int from, int to)
        {
            if (_deck == deck && _index == from)
                _index = to;
        }

        public virtual CardCommand Execute()
        {
            return this;
        }

        public virtual CardCommand ExecuteAbility(CardCommandQueue queue)
        {
            return this;
        }

		// UserEvent is for the UI logic to use to notify itself when the last command has executed and to know
		// when animations have stopped playing so the next set of command results can be animated
        public EventHandler UserEvent;
    }

    public class MoveCardsCommand : CardCommand
    {
        List<(int from, int to)> _indexes;

        public MoveCardsCommand(Deck deck, List<(int from, int to)> indexes) : base(deck)
        {
            _indexes = indexes;
        }

        public override CardCommand Execute()
        {
            foreach (var pair in _indexes)
                Deck.MoveCard(Deck[pair.from], pair.to);
            Deck.Player.Game.OnCardsMovedEvent(this);
            return this;
        }
    }

    public class AttackCardCommand : CardCommand
    {
        Deck _opponentDeck;
        int _opponentIndex;
        int _damage;
        int _opponentDamage;

        Card OpponentCard { get { return _opponentDeck[_opponentIndex]; } }

        public AttackCardCommand(Card card, Card opponentCard) : base(card)
        {
            _opponentDeck = opponentCard.Deck;
            _opponentIndex = opponentCard.Index;
        }

        public override void CardMoving(Deck deck, int from, int to)
        {
            base.CardMoving(deck, from, to);
            if (_opponentDeck == deck && _opponentIndex == from)
                _opponentIndex = to;
        }

        public override CardCommand Execute()
        {
            _damage = Card.GetDamage();
            _opponentDamage = OpponentCard.GetDamage();
            Card.Attack(ref _opponentDamage);
            OpponentCard.Attack(ref _damage);
            Card.Deck.Player.Game.OnAttackEvent(this);
            return this;
        }

        public override CardCommand ExecuteAbility(CardCommandQueue queue)
        {
            Card.Attacked(queue, _damage, _opponentDamage, OpponentCard);
            // OpponentCard might be null if Card dealt scorpion attack
            if (OpponentCard != null)
                OpponentCard.Attacked(queue, _opponentDamage, _damage, Card); // now Card could be null...
            return this;
        }
    }

    public class GainFoodAbilityCommand : CardCommand
    {
        FoodAbility _foodAbility;

        public GainFoodAbilityCommand(Card card, FoodAbility foodAbility) : base(card)
        {
            _foodAbility = foodAbility;
        }   

        public override CardCommand Execute()
        {
            Card.FoodAbility = _foodAbility;

            Deck.Player.Game.OnCardGainedFoodAbilityEvent(this);

            return this;
        }
    }

    public class FaintCardCommand : CardCommand
    {
        Card _faintedCard;

        public FaintCardCommand(Card card) : base(card)
        {

        }

        public override CardCommand Execute()
        {
            _faintedCard = Card;
            Card.Faint();
            Deck.Player.Game.OnCardFaintedEvent(this, Deck, Index);
            return this;
        }

        public override CardCommand ExecuteAbility(CardCommandQueue queue)
        {
            // Execute() is always called before ExecuteAbility() and the Card property is not a direct
            // reference, but a lookup based on Index. Once Card.Faint() is called, the Card instance
            // can no longer be looked up. So we stored Card in _faintedCard in Execute()
            _faintedCard.Fainted(queue, Index);
            return this;
        }
    }

    public class HurtCardCommand : CardCommand
    {
        int _sourceIndex;
        int _damage;
        Deck _sourceDeck;

        public Deck SourceDeck { get { return _sourceDeck; } }

        public int SourceIndex { get { return _sourceIndex; } }

        public HurtCardCommand(Card card, int damage, Deck sourceDeck, int sourceIndex) : base(card)
        {
            _damage = damage;
            _sourceIndex = sourceIndex;
            _sourceDeck = sourceDeck;
        }

        public override void CardMoving(Deck deck, int from, int to)
        {
            base.CardMoving(deck, from, to);
            if (_sourceDeck == deck && _sourceIndex == from)
                _sourceIndex = to;
        }

        public override CardCommand Execute()
        {
            Card.Hurt(_damage, _sourceDeck, _sourceIndex);
            Deck.Player.Game.OnCardHurtEvent(this, Card);
            return this;
        }

        public override CardCommand ExecuteAbility(CardCommandQueue queue)
        {
            if (Card != null)
            {
                Card opponentCard = null;
                if (Card.Deck.Player.Game.Fighting && 
                    _sourceDeck != Deck)
                    opponentCard = _sourceDeck[_sourceIndex];

                Card.Hurted(queue, opponentCard);
            }
            return this;
        }
    }

    public class BuffCardCommand : CardCommand
    {
        int _sourceIndex; 
        int _hitPoints;
        int _attackPoints;

        public int SourceIndex { get { return _sourceIndex; } }

        public BuffCardCommand(Card card, int sourceIndex, int hitPoints, int attackPoints) : base(card)
        {
            _sourceIndex = sourceIndex;
            _hitPoints = hitPoints;
            _attackPoints = attackPoints;
        }
        
        public override void CardMoving(Deck deck, int from, int to)
        {
            base.CardMoving(deck, from, to);
            if (Deck == deck && _sourceIndex == from)
                _sourceIndex = to;
        }

        public override CardCommand Execute()
        {
            Card.Buff(_sourceIndex, _hitPoints, _attackPoints);
            Deck.Player.Game.OnCardBuffedEvent(this);
            return this;
        }
    }

    public class SummonCardCommand : CardCommand
    {
        Type _abilityType;
        int _atIndex;
        int _hitPoints;
        int _attackPoints;
        int _level;
        Deck _atDeck;
        Card _summonedCard;
        Type _foodAbilityType;
        Type _renderAbilityType;

        public Card SummonedCard { get { return _summonedCard; } }

        public int AtIndex { get { return _atIndex; } }

        public SummonCardCommand(Card card, Deck atDeck, int atIndex, Type abilityType, int hitPoints, int attackPoints,
            int level = 1, Type foodAbilityType = null, Type renderAbilityType = null) : base(card)
        {
            _atDeck = atDeck;
            _abilityType = abilityType;
            _atIndex = atIndex;
            _hitPoints = hitPoints;
            _attackPoints = attackPoints;
            _level = level;
            _foodAbilityType = foodAbilityType;
            _renderAbilityType = renderAbilityType;
        }

        public override CardCommand Execute()
        {
            var ability = Activator.CreateInstance(_abilityType) as Ability;
            _summonedCard = new Card(_atDeck, ability)
            {
                HitPoints = _hitPoints,
                AttackPoints = _attackPoints
            };
            _summonedCard.XP = Card.GetXPFromLevel(_level);
            if (_foodAbilityType != null)
                _summonedCard.FoodAbility = Activator.CreateInstance(_foodAbilityType) as FoodAbility;
            if (_renderAbilityType != null)
                _summonedCard.RenderAbility = Activator.CreateInstance(_renderAbilityType) as Ability;
            _summonedCard.Summon(_atIndex);
            _atDeck.Player.Game.OnCardSummonedEvent(this);

            return this;
        }

        public override CardCommand ExecuteAbility(CardCommandQueue queue)
        {
            _summonedCard.Summoned(queue);
            return this;
        }
    }
}