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

            Card.Deck.Player.Game.OnAttackEvent();

            return this;
        }

        public override CardCommand ExecuteAbility(CardCommandQueue queue)
        {
            if (_opponentDamage > 0)
                Card.Ability.Hurt(queue, Card);
            if (_damage > 0)
                OpponentCard.Ability.Hurt(queue, OpponentCard);
            if (Card.TotalHitPoints <= 0)
                queue.Add(new FaintCardCommand(Card));
            if (OpponentCard.TotalHitPoints <= 0)
                queue.Add(new FaintCardCommand(OpponentCard));
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
            if (_faintedCard.FoodAbility != null)
                _faintedCard.FoodAbility.Fainted(queue, _faintedCard, Index);
            return this;
        }
    }

    public class HurtCardCommand : CardCommand
    {
        int _sourceIndex;
        int _damage;
        Deck _sourceDeck;
        int _saveHitPoints;

        public HurtCardCommand(Card card, int damage, Deck sourceDeck, int sourceIndex) : base(card)
        {
            _damage = damage;
            _sourceIndex = sourceIndex;
            _sourceDeck = sourceDeck;
        }

        public override CardCommand Execute()
        {
            _saveHitPoints = Card.TotalHitPoints;
            Card.Hurt(_damage, _sourceDeck, _sourceIndex);
            return this;
        }

        public override CardCommand ExecuteAbility(CardCommandQueue queue)
        {
            Card.Ability.Hurt(queue, Card);

            // Prevent queuing up more than one FaintCommand by checking _saveHitPoints.
            // HurtCommand's can queue multiple times for one card.
            // An example of this would be a bunch of mosquito's vs one duck. If the mosquito
            // ability were allowed to damage the duck over and over, then we would be 
            // queuing up multiple FaintCommand's for the duck. So the mosquito ability is
            // coded to Hurt the duck just once to cause it to faint. (see GetRandomCard, checks HitPoints >= 0)
            // However, the other mosquito ability methods will be hurting the same duck!
            // So it seems we can't avoid multiple HurtCommands from queuing up, so at least
            // we avoid queuing up more than one FaintCommand:
            if (_saveHitPoints > 0 && Card.TotalHitPoints <= 0)
                queue.Add(new FaintCardCommand(Card));
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
        int _level;
        Deck _atDeck;
        Card _summonedCard;

        public SummonCardCommand(Card card, Deck atDeck, int atIndex, Ability ability, int hitPoints, int attackPoints,
            int level = 1) : base(card)
        {
            _atDeck = atDeck;
            _ability = ability;
            _atIndex = atIndex;
            _hitPoints = hitPoints;
            _attackPoints = attackPoints;
            _level = level;
        }

        public override CardCommand Execute()
        {
            // normally it wouldn't be necessary to find an empty slot because
            // an ability would check this before creating the Summon command, however
            // food abilities can't know if a pet ability summoned something. For example
            // a cricket with a honey ability will try and summon both a cricket and a bee

            int summonIndex = -1;
            // find the first empty slot on or after _atIndex
            for (int i = _atIndex; i < _atDeck.Size; i++)
                if (_atDeck[i] == null)
                {
                    summonIndex = i;
                    break;
                }

            //TODO: it's easy enough to move pets, but what happens is the subsequent
            // call to ExecuteAbility will queue up additional commands that will be 
            // associated with a card whose index may change later if MakeRoomAt is called 
            // a test case is when several horses are buffing a honey cricket at the end of the deck.
            // the horses will end up buffing the summoned bee instead of the summoned cricket

            // push pets if we couldn't find room after _atIndex
            //if (summonIndex == -1 && _atDeck.MakeRoomAt(_atIndex))
            //    summonIndex = _atIndex;

            if (summonIndex != -1)
            {
                _summonedCard = new Card(_atDeck, _ability)
                {
                    HitPoints = _hitPoints,
                    AttackPoints = _attackPoints
                };
                _summonedCard.XP = Card.GetXPFromLevel(_level);
                _summonedCard.Summon(summonIndex);
            }
            return this;
        }

        public override CardCommand ExecuteAbility(CardCommandQueue queue)
        {
            if (_summonedCard != null)
                foreach (var c in _atDeck)
                {
                    if (c != _summonedCard)
                        c.Ability.FriendSummoned(queue, c, _summonedCard);
                }
            return this;
        }
    }
}