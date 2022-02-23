using System;

namespace AutoPets
{
    public class CardCommand
    {
        int _index;
        Deck _deck;

        public Deck Deck { get { return _deck; } }

        public Card Card { get { return _deck[_index]; } }

        public int Index { get { return _index; } }

        public CardCommand(Card card)
        {
            // deck might be cleared later, and new card references added to the deck
            // so we don't keep a reference to the card, but only a reference to the deck 
            _index = card.Index;
            _deck = card.Deck;
        }

        public virtual CardCommand Execute()
        {
            return this;
        }

        public virtual CardCommand ExecuteAbility(CardCommandQueue queue)
        {
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

        public AttackCardCommand(Card card, int damage, Card opponentCard, int opponentDamage) : base(card)
        {
            _opponentDeck = opponentCard.Deck;
            _opponentIndex = opponentCard.Index;
            _damage = damage;
            _opponentDamage = opponentDamage;
        }

        public override CardCommand Execute()
        {
            Card.HitPoints -= _opponentDamage;
            OpponentCard.HitPoints -= _damage;

            Card.Deck.Player.Game.OnFightEvent();

            return this;
        }

        public override CardCommand ExecuteAbility(CardCommandQueue queue)
        {
            if (_opponentDamage > 0)
                Card.Ability.Hurt(queue, Card);
            if (_damage > 0)
                OpponentCard.Ability.Hurt(queue, OpponentCard);
            if (Card.HitPoints <= 0)
                queue.Enqueue(new FaintCardCommand(Card));
            if (OpponentCard.HitPoints <= 0)
                queue.Enqueue(new FaintCardCommand(OpponentCard));
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
            return this;
        }

        public override CardCommand ExecuteAbility(CardCommandQueue queue)
        {
            // Execute() is always called before ExecuteAbility() and the Card property is not a direct
            // reference, but a lookup based on Index. Once Card.Faint() is called, the Card instance
            // can no longer be looked up. So we stored Card in _faintedCard in Execute()
            _faintedCard.Ability.Fainted(queue, _faintedCard, Index);
            return this;
        }
    }

    public class HurtCardCommand : CardCommand
    {
        int _sourceIndex;
        Deck _sourceDeck;
        int _damage;
        bool _canFaint;

        public HurtCardCommand(Card card, int damage, Card sourceCard) : base(card)
        {
            _damage = damage;
            _sourceIndex = sourceCard.Index;
            _sourceDeck = sourceCard.Deck;
        }

        public override CardCommand Execute()
        {
            if (Card.HitPoints > 0)
                _canFaint = true;
            Card.Hurt(_damage, _sourceDeck[_sourceIndex]);
            return this;
        }

        public override CardCommand ExecuteAbility(CardCommandQueue queue)
        {
            Card.Ability.Hurt(queue, Card);
            // a card can be hurt multiple times in succession, e.g. from many mosquito attacks
            // and for each HurtCommand, we don't need to queue up multiple Faint commands
            // so using _canFaint to prevent multiple Faint commands being added to queue
            if (_canFaint && Card.HitPoints <= 0)
                queue.Enqueue(new FaintCardCommand(Card));
            return this;
        }
    }

    public class BuffCardCommand : CardCommand
    {
        int _sourceIndex; 
        int _hitPoints;
        int _attackPoints;

        public BuffCardCommand(Card card, int sourceIndex, int hitPoints, int attackPoints) : base(card)
        {
            _sourceIndex = sourceIndex;
            _hitPoints = hitPoints;
            _attackPoints = attackPoints;
        }

        public override CardCommand Execute()
        {
            Card.Buff(_sourceIndex, _hitPoints, _attackPoints);
            return this;
        }
    }

    public class SummonCardCommand : CardCommand
    {
        Ability _ability;
        int _atIndex;
        int _hitPoints;
        int _attackPoints;
        Card _summonedCard;

        public SummonCardCommand(Card card, int atIndex, Ability ability, int hitPoints, int attackPoints) : base(card)
        {
            _ability = ability;
            _atIndex = atIndex;
            _hitPoints = hitPoints;
            _attackPoints = attackPoints;
        }

        public override CardCommand Execute()
        {
            _summonedCard = new Card(Deck, _ability)
            {
                HitPoints = _hitPoints,
                AttackPoints = _attackPoints
            };
            _summonedCard.Summon(_atIndex);
            return this;
        }

        public override CardCommand ExecuteAbility(CardCommandQueue queue)
        {
            foreach (var c in Deck)
            {
                if (c != _summonedCard)
                    c.Ability.FriendSummoned(queue, c, _summonedCard);
            }
            return this;
        }
    }
}