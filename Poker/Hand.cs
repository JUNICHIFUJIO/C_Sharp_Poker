using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// # of cards in hand
// display cards in hand
// organize cards
// discard cards
// subsection cards
// draw cards

namespace Poker
{
   // Ciao churru (cat feed japanese) ERROR TESTING
   public enum Hand_Values { HIGH_CARD, ONE_PAIR, TWO_PAIR, THREE_OF_A_KIND, STRAIGHT, FLUSH, FULL_HOUSE, FOUR_OF_A_KIND, ROYAL_STRAIGHT, STRAIGHT_FLUSH, ROYAL_FLUSH, END};

   class Hand
   {
      /*
      public static void Main(string[] args)
      {
         // testing mainframe
         // test constructors
         Hand hand = new Hand();

         Card[] the_cards = new Card[] { new Card("13; diamonds", 0), new Card("2; club", 1), new Card("9; hearts", 2) };
         Deck the_deck = new Deck(the_cards);

         Hand hand_2 = new Hand(the_cards);
         Hand hand_3 = new Hand(the_deck, 52);

         // test methods
         Console.WriteLine("Displaying an empty hand:");
         hand.display();
         
         Console.WriteLine("Shuffling deck now.");
         hand.deck.shuffle();

         Console.WriteLine("Gathering cards for the hand:");
         hand.fill(); // fill hand
         hand.display(); // display now full hand

         // display best
         Console.WriteLine("Displaying the best of the hand:");
         for (int i = 0; i < hand.the_best.Length; i++)
         {
            if (hand.the_best[i] != null)
            {
               Console.Write("Best #{0}\t", i);
               hand.the_best[i].display();
            }
         }
         Console.WriteLine();

         // display the rest
         Console.WriteLine("Displaying the rest of the hand:");
         for (int i = 0; i < hand.the_rest.Length; i++)
         {
            if (hand.the_rest[i] != null)
            {
               Console.Write("Rest #{0}\t", i);
               hand.the_rest[i].display();
            }
         }
         Console.WriteLine();

         // organize the hand
         Console.WriteLine("manually reorganizing hand");
         hand.organize();
         hand.display();

         // discarding practice
         Console.WriteLine("Trying discard");
         hand.discard();

         Console.WriteLine("Your hand now consists of:");
         hand.display();

         Console.WriteLine("Refilling the hand:");
         hand.fill();
         hand.display();

         Console.WriteLine("This is what's in the deck's discard pile:");
         hand.deck.display_discard_pile();

         Console.Read();
      }
      */

      // Consts
      public const int DEFAULT_MAX_SIZE = 5;

      // Constructors
      public Hand(Hand other)
      {
         this.cards_p = other.cards;
         this.max_size_p = other.max_size;
         this.n_held_p = other.n_held;
         this.deck_p = other.deck;
         this.the_best_p = other.the_best;
         this.the_rest_p = other.the_rest;
         this.best_hand_value_p = other.best_hand_value;
      }

      public Hand(Card[] cards, Deck deck, int max_size)
      {
         cards_p = cards;
         max_size_p = max_size;
         n_held_p = 0;
         for (int i = 0; i < cards.Length; i++)
         {
            if (cards_p[i] == null)
            {
               break;
            }
            else
            {
               ++n_held_p;
            }
         }
         deck_p = deck;
         the_best_p = new Card[max_size_p];
         the_rest_p = new Card[0];
         best_hand_value_p = Hand_Values.END;

         organize();
      }

      public Hand(Deck deck, int max_size) : this(new Card[5], deck, max_size) { }

      public Hand(Card[] cards) : this(cards, new Deck(), cards.Length) { }

      public Hand() : this(new Card[5], new Deck(), 5) { }

      static Hand()
      {
         rand = new Random();
      }

      // Methods
      // display
      // discard
      // fill
      // organize_auto
      // subsection (based on getting stuff in a row, stuff of the same value, stff of the same suite)
      public void display()
      {
         Console.WriteLine("Cards held:");
         for (int i = 0; i < cards_p.Length; i++)
         {
            if (cards_p[i] != null)
            {
               Console.Write("\t{0}) ", i + 1);
               cards_p[i].display();
            }
         }

         Console.WriteLine();
      }

      public void draw(int n)
      {
         if(n > n_missing){
            n = n_missing;
         }
         else if (n < 0)
         {
            n = 0;
         }

         if (deck.size < n)
         {
            deck.reset();
            deck.shuffle();
         }

         Card[] cards_drawn = deck_p.draw(n);
         int hand_index = n_held;

         for (int i = 0; i < n; i++)
         {
            cards_p[hand_index++] = cards_drawn[i];
         }
         
         n_held_p = hand_index;

         organize();
      }

      public void draw_new_hand()
      {
         discard(cards_p);
         draw(max_size);
      }

      public void organize(){
         Card_Value straight_starting_value;
         Card_Suite flush_suite;
         
         best_hand_value_p = calculate_best_hand_value(out straight_starting_value, out flush_suite);

         bool straight = false;
         bool flush = false;

         switch(best_hand_value){
            case(Hand_Values.ROYAL_FLUSH):
            case(Hand_Values.STRAIGHT_FLUSH):
               flush = true;
               straight = true;
               break;
            case(Hand_Values.FLUSH):
               flush = true;
               break;
            case(Hand_Values.ROYAL_STRAIGHT):
            case(Hand_Values.STRAIGHT):
               straight = true;
               break;
            default:
               break;
         }

         fill_best(straight, flush, straight_starting_value, flush_suite);
         fill_rest();
         organize_hand();
      }

      public Hand_Values calculate_best_hand_value(out Card_Value straight_starting_value, out Card_Suite flush_suite){
         // have to make sure that the best hand value is updated
         // need to make sure the best part of the hand (the pair, two pairs, etc.) are at the front
         // have to distinguish between hand values

         straight_starting_value = Card_Value.END;
         flush_suite = Card_Suite.END;

         // initialize hand value tracking array
         bool[] is_hand_value = new bool[(int)Hand_Values.END];
         for(int i = 0; i < is_hand_value.Length; i++){
            is_hand_value[i] = false;
         }

         // check for the possibility of a flush
         Card_Suite[] flush_suites = check_for_flushes();
         is_hand_value[(int)Hand_Values.FLUSH] = flush_suites.Length > 0; // record if it's possible for a flush
         if(flush_suites.Length > 0){
            flush_suite = flush_suites[0];
         }

         // check for a normal straight
         is_hand_value[(int)Hand_Values.STRAIGHT] = check_for_straight(out straight_starting_value); // verify this will appropriately return TEN if royal straight present

         // check for a normal straight flush
         if(is_hand_value[(int)Hand_Values.FLUSH]
            && is_hand_value[(int)Hand_Values.STRAIGHT]){
            is_hand_value[(int)Hand_Values.STRAIGHT_FLUSH] = check_for_straight_flush(straight_starting_value, flush_suites);
            flush_suite = get_straight_flush_suite(straight_starting_value, flush_suites);
         }
         else{
            is_hand_value[(int)Hand_Values.STRAIGHT_FLUSH] = false;
         }

         // check for royal_straights/flushes
         if(is_hand_value[(int)Hand_Values.STRAIGHT]){
            is_hand_value[(int)Hand_Values.ROYAL_STRAIGHT] = check_for_royal_straight();
            if(is_hand_value[(int)Hand_Values.ROYAL_STRAIGHT]
               && is_hand_value[(int)Hand_Values.FLUSH]){
               is_hand_value[(int)Hand_Values.ROYAL_FLUSH] = check_for_royal_flush(flush_suites);
               flush_suite = get_royal_flush_suite(flush_suites);
            }
            else{
               is_hand_value[(int)Hand_Values.ROYAL_FLUSH] = false;
            }
         }

         // check for other non-flush things
         // check for multi-card things
         Hand_Values multi_card_hand_value = check_for_multi_card_hands();
         switch(multi_card_hand_value){
            case(Hand_Values.FULL_HOUSE):
               is_hand_value[(int)Hand_Values.FULL_HOUSE] = true;
               is_hand_value[(int)Hand_Values.THREE_OF_A_KIND] = true;
               is_hand_value[(int)Hand_Values.TWO_PAIR] = true;
               is_hand_value[(int)Hand_Values.ONE_PAIR] = true;
               is_hand_value[(int)Hand_Values.HIGH_CARD] = true;
               break;
            case(Hand_Values.TWO_PAIR):
               is_hand_value[(int)Hand_Values.TWO_PAIR] = true;
               is_hand_value[(int)Hand_Values.ONE_PAIR] = true;
               is_hand_value[(int)Hand_Values.HIGH_CARD] = true;
               break;
            case(Hand_Values.FOUR_OF_A_KIND):
               is_hand_value[(int)Hand_Values.FOUR_OF_A_KIND] = true;
               is_hand_value[(int)Hand_Values.THREE_OF_A_KIND] = true;
               is_hand_value[(int)Hand_Values.ONE_PAIR] = true;
               is_hand_value[(int)Hand_Values.HIGH_CARD] = true;
               break;
            case(Hand_Values.THREE_OF_A_KIND):
               is_hand_value[(int)Hand_Values.THREE_OF_A_KIND] = true;
               is_hand_value[(int)Hand_Values.ONE_PAIR] = true;
               is_hand_value[(int)Hand_Values.HIGH_CARD] = true;
               break;
            case(Hand_Values.ONE_PAIR):
               is_hand_value[(int)Hand_Values.ONE_PAIR] = true;
               is_hand_value[(int)Hand_Values.HIGH_CARD] = true;
               break;
            case(Hand_Values.HIGH_CARD):
               is_hand_value[(int)Hand_Values.HIGH_CARD] = true;
               break;
         }

         // find the best hand value
         for(int i = is_hand_value.Length - 1; i >= 0; i--){
            if(is_hand_value[i]){
               return (Hand_Values)i;
            }
         }

         return Hand_Values.HIGH_CARD;
      }

      private void organize_by_value(Card[] cards)
      {
         for (int i = 0; i < cards.Length; i++)
         {
            for (int j = i; j < cards.Length; j++)
            {
               if (cards[j] != null
                  && cards[i] != null
                  && cards[j].value > cards[i].value)
               {
                  Card temp = cards[i];
                  cards[i] = cards[j];
                  cards[j] = temp;
               }
            }
         }
      }

      private void organize_by_value(){
         organize_by_value(cards_p);
      }

      private Card_Suite[] check_for_flushes(int flush_length = 5){
         int[] n_in_suite = new int[(int)Card_Suite.END];
         for(int i = 0; i < n_in_suite.Length; i++){
            n_in_suite[i] = 0;
         }

         int n_flush_suites = 0;
         for(int i = 0; i < n_held; i++){
            int suite_index = (int)cards_p[i].suite;
            ++n_in_suite[suite_index];
            if(n_in_suite[suite_index] == flush_length){
               ++n_flush_suites;
            }
         }

         Card_Suite[] flush_suites = new Card_Suite[n_flush_suites];
         int flush_suite_index = 0;
         for(int i = 0; i < n_in_suite.Length; i++){
            if(n_in_suite[i] >= flush_length){
               flush_suites[flush_suite_index++] = (Card_Suite)i;
            }
         }

         return flush_suites;
      }

      // assumed to be organized by value
      private bool check_for_straight(out Card_Value starting_value){
         organize_by_value();
         const int STRAIGHT_LENGTH = 5;
         starting_value = Card_Value.END;

         if(n_held < STRAIGHT_LENGTH){
            return false;
         }

         Card_Value current_card_value = Card_Value.END;
         int n_in_a_row = 1;
         bool potential_royal_flush = false;
         // handle royal flush cases
         for(int i = 1; i < n_held; i++){
            current_card_value = cards_p[i].value;
            Card_Value previous_card_value = cards_p[i - 1].value;
            if(current_card_value == previous_card_value){
               continue;
            }
            else if(current_card_value == previous_card_value - 1){
               ++n_in_a_row;
               starting_value = current_card_value;
               if(starting_value == Card_Value.TEN
                  && n_in_a_row >= STRAIGHT_LENGTH - 1){
                  potential_royal_flush = true;
               }
            }
            else if(current_card_value == Card_Value.ACE){
               if(potential_royal_flush){
                  starting_value = Card_Value.TEN;
                  return true;
               }
            }
            else{
               n_in_a_row = 1;
               starting_value = current_card_value;
            }
         }

         if(n_in_a_row >= STRAIGHT_LENGTH){
            return true;
         }
         else{
            return false;
         }
      }

      private bool check_for_royal_straight(){
         const Card_Value required_starting_value = Card_Value.TEN;
         Card_Value starting_value;
         if(check_for_straight(out starting_value)){
            return starting_value == required_starting_value;
         }
         else{
            return false;
         }
      }

      private bool check_for_royal_flush(Card_Suite[] flush_suites){
         return get_royal_flush_suite(flush_suites) != null;
      }

      private Card_Suite get_royal_flush_suite(Card_Suite[] flush_suites){
         int[] royals_in_suite = new int[flush_suites.Length];
         // initialize royals_in_suite explicitly
         for(int i = 0; i < royals_in_suite.Length; i++){
            royals_in_suite[i] = 0;
         }

         for(int i = 0; i < n_held; i++){
            Card current_card = cards_p[i];

            if(current_card.value == Card_Value.ACE
               || (current_card.value >= Card_Value.TEN && current_card.value <= Card_Value.KING)){
               for(int royals_in_suite_index = 0; royals_in_suite_index < royals_in_suite.Length; royals_in_suite_index++){
                  if(current_card.suite == flush_suites[royals_in_suite_index]){
                     // if it's a royal, update the appropriate royals in suite number
                     ++royals_in_suite[royals_in_suite_index];
                  }
               }
            }
         }

         for(int i = 0; i < royals_in_suite.Length; i++){
            if(royals_in_suite[i] >= STRAIGHT_LENGTH){
               return flush_suites[i];
            }
         }

         return Card_Suite.END;
      }

      private bool check_for_straight_flush(Card_Value starting_value, Card_Suite[] flush_suites){
         if(check_for_royal_flush(flush_suites)){
            return true;
         }

         if(starting_value > (Card_Value)((int)Card_Value.END - 5)){
            return false;
         }

         return get_straight_flush_suite(starting_value, flush_suites) != Card_Suite.END;
      }

      private Card_Suite get_straight_flush_suite(Card_Value starting_value, Card_Suite[] flush_suites){
         Card_Suite royal_flush_suite = get_royal_flush_suite(flush_suites);
         if(royal_flush_suite != Card_Suite.END){
            return royal_flush_suite;
         }

         int[] n_in_suite = new int[flush_suites.Length];

         for(int i = 0; i < n_held; i++){
            Card current_card = cards_p[i];
            if(current_card.value < (Card_Value)((int)starting_value + 5)
               && current_card.value >= starting_value){
               // if it's the right value
               for(int suite_index = 0; suite_index < flush_suites.Length; suite_index++){
                  if(current_card.suite == flush_suites[suite_index]){
                     n_in_suite[suite_index]++;
                     if(n_in_suite[suite_index] >= STRAIGHT_LENGTH){
                        return flush_suites[suite_index];
                     }
                  }
               }
            }
         }

         return Card_Suite.END;
      }

      private Hand_Values check_for_multi_card_hands(){
         organize_by_value();

         Card_Value val1 = cards_p[0].value;
         int n_val_1 = 1;
         Card_Value val2 = (Card_Value)0;
         int n_val_2 = 0;

         // full house
         // two pair

         // four of a kind
         // three of a kind
         // one pair
         // high card
         for(int i = 1; i < n_held; i++){
            Card_Value current_val = cards_p[i].value;
            Card_Value previous_val = cards_p[i - 1].value;

            if(n_val_2 == 0
               && current_val != val1){
               val2 = current_val;
               n_val_2 = 1;
            }
            else if(current_val == previous_val){
               if(current_val == val1){
                  ++n_val_1;
               }
               else if(current_val == val2){
                  ++n_val_2;
               }
               else{
                  if(n_val_1 < 2){
                     val1 = current_val;
                     n_val_1 = 2;
                  }
                  else if(n_val_2 < 2){
                     val2 = current_val;
                     n_val_2 = 2;
                  }
               }
            }
         }

         if(n_val_1 + n_val_2 >= 5
            && n_val_1 > 1
            && n_val_2 > 1){
            return Hand_Values.FULL_HOUSE;
         }
         else if(n_val_1 > 1 && n_val_2 > 1){
            return Hand_Values.TWO_PAIR;
         }
         else if(n_val_1 > 3 || n_val_2 > 3){
            return Hand_Values.FOUR_OF_A_KIND;
         }
         else if(n_val_1 > 2 || n_val_2 > 2){
            return Hand_Values.THREE_OF_A_KIND;
         }
         else if(n_val_1 > 1 || n_val_2 > 1){
            return Hand_Values.ONE_PAIR;
         }
         else{
            return Hand_Values.HIGH_CARD;
         }
      }

      private Card[] get_multi_card_best(Hand_Values hand_value){
         Card_Value val1 = cards_p[0].value;
         int n_val_1 = 1;
         Card_Value val2 = (Card_Value)0;
         int n_val_2 = 0;

         // full house
         // two pair

         // four of a kind
         // three of a kind
         // one pair
         // high card
         for(int i = 1; i < n_held; i++){
            Card_Value current_val = cards_p[i].value;
            Card_Value previous_val = cards_p[i - 1].value;

            if(n_val_2 == 0){
               val2 = current_val;
               n_val_2 = 1;
            }
            else if(current_val == previous_val){
               if(current_val == val1){
                  ++n_val_1;
               }
               else if(current_val == val2){
                  ++n_val_2;
               }
               else{
                  if(n_val_1 < 2){
                     val1 = current_val;
                     n_val_1 = 2;
                  }
                  else if(n_val_2 < 2){
                     val2 = current_val;
                     n_val_2 = 2;
                  }
               }
            }
         }

         Card[] best = null;
         Card_Value primary_val = val1;
         Card_Value secondary_val = val2;
         if(n_val_2 > n_val_1){
            secondary_val = val1;
            primary_val = val2;
         }
         else if (n_val_1 == n_val_2
            && secondary_val > primary_val)
         {
            secondary_val = val1;
            primary_val = val2;
         }

         int primary_index;
         int secondary_index;

         switch(hand_value){
            case(Hand_Values.FULL_HOUSE):
               const int FULL_HOUSE_LENGTH = 5;
               best = new Card[FULL_HOUSE_LENGTH];
               primary_index = 0;
               secondary_index = 3;
               for(int i = 0; i < n_held; i++){
                  if(cards_p[i].value == primary_val){
                     best[primary_index++] = cards_p[i];
                  }
                  else if(cards_p[i].value == secondary_val){
                     best[secondary_index++] = cards_p[i];
                  }
               }
               break;
            case(Hand_Values.TWO_PAIR):
               const int TWO_PAIR_LENGTH = 4;
               best = new Card[TWO_PAIR_LENGTH];
               primary_index = 0;
               secondary_index = 2;
               for(int i = 0; i < n_held; i++){
                  if(cards_p[i].value == primary_val){
                     best[primary_index++] = cards_p[i];
                  }
                  else if(cards_p[i].value == secondary_val){
                     best[secondary_index++] = cards_p[i];
                  }
               }
               break;
            case(Hand_Values.FOUR_OF_A_KIND):
               best = new Card[4];
               primary_index = 0;
               for(int i = 0; i < n_held; i++){
                  if(cards_p[i].value == primary_val){
                     best[primary_index++] = cards_p[i];
                  }
               }
               break;
            case(Hand_Values.THREE_OF_A_KIND):
               best = new Card[3];
               primary_index = 0;
               for(int i = 0; i < n_held; i++){
                  if(cards_p[i].value == primary_val){
                     best[primary_index++] = cards_p[i];
                  }
               }
               break;
            case(Hand_Values.ONE_PAIR):
               best = new Card[2];
               primary_index = 0;
               for(int i = 0; i < n_held; i++){
                  if(cards_p[i].value == primary_val){
                     best[primary_index++] = cards_p[i];
                  }
               }
               break;
            case(Hand_Values.HIGH_CARD):
               best = new Card[1];
               Card_Value high_card_val = (Card_Value)0;
               for(int i = 0; i < n_held; i++){
                  if(cards_p[i].value > high_card_val){
                     high_card_val = cards_p[i].value;
                     best[0] = cards_p[i];
                  }
               }
               break;
         }

         return best;
      }

      private void fill_best(bool straight, bool flush, Card_Value straight_starting_value, Card_Suite flush_suite){
         if(straight && flush){
            if(best_hand_value_p == Hand_Values.ROYAL_FLUSH){
               the_best_p = get_royal_straight_best();
            }
            else{
               the_best_p = get_straight_flush_best(straight_starting_value);
            }
         }
         else if(straight){
            if(best_hand_value_p == Hand_Values.ROYAL_STRAIGHT){
               the_best_p = get_royal_straight_best();
            }
            else{
               the_best_p = get_straight_best(straight_starting_value);
            }
         }
         else if(flush){
            the_best_p = get_flush_best(flush_suite);
         }
         else{
            organize_by_value();
            the_best_p = get_multi_card_best(best_hand_value_p);
         }
      }

      private Card[] get_royal_straight_best(){
         if(n_held != 5){
            Console.WriteLine("Potential problem in get_royal_straight_best. not the expected # of cards");
         }

         Card[] best = new Card[5];
         for(int i = 0; i < n_held; i++){
            Card_Value current_val = cards_p[i].value;
            switch(current_val){
               case(Card_Value.ACE):
                  best[0] = cards_p[i];
                  break;
               case(Card_Value.KING):
                  best[1] = cards_p[i];
                  break;
               case(Card_Value.QUEEN):
                  best[2] = cards_p[i];
                  break;
               case(Card_Value.JACK):
                  best[3] = cards_p[i];
                  break;
               case(Card_Value.TEN):
                  best[4] = cards_p[i];
                  break;
               default:
                  continue;
            }
         }

         return best;
      }

      // assumes that only 5 cards are held
      private Card[] get_straight_flush_best(Card_Value starting_value){
         Card[] best = get_straight_best(starting_value);
         int[] n_of_suite = new int[5];

         for(int i = 0; i < n_held; i++){
            n_of_suite[(int)cards_p[i].suite]++;
         }

         Card_Suite most_suite = (Card_Suite)0;
         int highest_n_of_suite = 0;
         for(int i = 0; i < n_of_suite.Length; i++){
            if(n_of_suite[i] >= 5){
               return best;
            }
            else if(n_of_suite[i] > highest_n_of_suite){
               highest_n_of_suite = n_of_suite[i];
               most_suite = (Card_Suite)i;
            }
         }

         // replace incorrect cards
         for(int i = 0; i < n_held; i++){
            if(best[i].suite != most_suite){
               for(int j = 0; j < n_held; j++){
                  if(cards_p[j].suite == most_suite
                     && cards_p[j].value == best[i].value){
                     best[i] = cards_p[j];
                  }
               }
            }
         }

         return best;
      }

      private Card[] get_straight_best(Card_Value starting_value){
         Card[] best = new Card[5];

         for(int i = 0; i < n_held; i++){
            if(cards_p[i].value >= starting_value
               && cards_p[i].value < (Card_Value)((int)starting_value + 5)){
               best[4 - (int)cards_p[i].value + (int)starting_value] = cards_p[i];
            }
         }

         return best;
      }

      private Card[] get_flush_best(Card_Suite flush_suite){
         organize_by_value();

         Card[] best = new Card[5];
         int index = 0;

         for(int i = 0; i < n_held; i++){
            if(cards_p[i].suite == flush_suite){
               best[index++] = cards_p[i];
            }
         }

         return best;
      }

      private void fill_rest(){
         Card[] rest = new Card[max_size - the_best.Length];
         int rest_index = 0;

         for(int i = 0; i < n_held; i++){
            bool is_in_best = false;
            for(int best_index = 0; best_index < the_best.Length; best_index++){
               if(cards_p[i] == the_best[best_index]){
                  is_in_best = true;
                  break;
               }
            }

            if(!is_in_best){
               rest[rest_index++] = cards_p[i];
            }
         }

         the_rest_p = rest;
      }

      private void organize_hand(){
         cards_p = new Card[n_held];

         int index = 0;
         for(int i = 0; i < the_best.Length; i++){
            cards_p[index++] = the_best[i];
         }

         for(int i = 0; i < the_rest.Length; i++){
            cards_p[index++] = the_rest[i];
         }
      }





      /*
      
      public void organize()
      {
         if(n_held == 0
            || cards_p[0] == null)
         {
            return;
         }
         // organize by suite - check for flush or royal flush
         // organize by value - check for straight, straight flush, royal flush, royal straight, pairs, three of a kinds
         organize_by_suite();
         
         if(check_for_flush()){
            // hand is full and has flush --> check for straight
            organize_by_value(cards_p);
            
            Card_Value straight_start_val;
            bool straight = check_for_straight(out straight_start_val);

            if(straight){
               // done
               fill_best(true, true, cards_p[0].value, Card_Value.END, cards_p[0].suite);
            }
            else{
               fill_best(true, false, cards_p[0].value, Card_Value.END, cards_p[0].suite);
            }

            return;
         }
         else{
            bool flush = check_for_flush(true);
            bool potential_flush = (flush && n_held == max_size - 1) || (!flush && n_held == max_size && cards_p[0].suite == cards_p[max_size - 1].suite);
            organize_by_value(cards_p);
            Card_Value straight_start_val;
            bool straight = check_for_straight(out straight_start_val);

            if(straight){
               fill_best(false, true, straight_start_val, Card_Value.END, Card_Suite.END);
               return;
            }
            else{
               organize_unexemplary_hand();
            }
            
            if(best_hand_value_p == Hand_Values.HIGH_CARD
               && potential_flush){
               organize_by_suite();
               if (n_held > 1
                  && cards_p[1].suite != cards_p[0].suite)
               {
                  Card temp = cards_p[n_held - 1];
                  for (int i = 0; i < n_held; i++)
                  {
                     Card temp2 = cards_p[i];
                     cards_p[i] = temp;
                     temp = temp2;
                  }
               }
            }
         }
      }

      // if a straight or a flush doesn't exist in organize(), it calls this to find the non-straight or non-flush cards
      // that result in the best value
      private void organize_unexemplary_hand()
      {
         // four of a kind first
         // two pair
         // three of a kind
         // one pair
         // high card

         organize_by_value(cards_p);

         if (n_held == 0
            || cards_p[0] == null)
         {
            return;
         }

         Card_Value val_1 = Card_Value.END;
         Card_Value val_2 = Card_Value.END;
         bool val_1_found = false;
         bool val_2_found = false;

         for(int i = 1; i < n_held; i++){
            if(!val_1_found 
               && cards_p[i].value == cards_p[i-1].value){
               val_1 = cards_p[i].value;
               val_1_found = true;
            }
            else if(cards_p[i].value == cards_p[i-1].value
               && cards_p[i].value != val_1
               && val_1_found
               && !val_2_found){
               val_2 = cards_p[i].value;
               val_2_found = true;
               break;
            }
         }

         fill_best(false, false, val_1, val_2, Card_Suite.END);
      }

      private void fill_best(bool flush, bool straight, Card_Value val_1, Card_Value val_2, Card_Suite suite)
      {
         int best_arr_index = 0;
         int rest_arr_index = 0;
         Card[] temp_best = new Card[max_size];
         Card[] temp_rest = new Card[max_size];
         // handle flushes
         if(flush){
            for(int i = 0; i < n_held; i++){
               if(cards_p[i].suite == suite){
                  temp_best[best_arr_index++] = cards_p[i];
               }
               else{
                  temp_rest[rest_arr_index++] = cards_p[i];
               }
            }
            organize_by_value(temp_best);
            if(straight){
               if(the_best_p[0].value == (Card_Value)((int)Card_Value.KING - temp_best.Length)){
                  best_hand_value_p = Hand_Values.ROYAL_FLUSH;
               }
               best_hand_value_p = Hand_Values.STRAIGHT_FLUSH;
            }
            else
            {
               best_hand_value_p = Hand_Values.FLUSH;
            }
         }
         else if(straight){
            Card_Value value_tracker = val_1;
            organize_by_value(cards_p);
            for(int i = 0; i < n_held; i++){
               if(cards_p[i].value == value_tracker){
                  temp_best[best_arr_index++] = cards_p[i];
                  ++value_tracker;
               }
               else
               {
                  temp_rest[rest_arr_index++] = cards_p[i];
               }
            }
            if (the_best_p[0].value == (Card_Value)((int)Card_Value.KING - temp_best.Length))
            {
               best_hand_value_p = Hand_Values.ROYAL_STRAIGHT;
            }
            else
            {
               best_hand_value_p = Hand_Values.STRAIGHT;
            }
         }
         else{
            // handle everything else: four of a kind, full house, two pair, three of a kind, one pair, high card
            if (val_2 != Card_Value.END)
            {
               // handle full house, two pair
               // determine higher of the values
               Card_Value highest_val;
               if (val_2 > val_1)
               {
                  highest_val = val_2;
               }
               else
               {
                  highest_val = val_1;
               }

               int n_val_1 = 0;
               int n_val_2 = 0;
               for (int i = 0; i < n_held; i++)
               {
                  if (cards_p[i].value == val_1)
                  {
                     ++n_val_1;
                  }
                  else if (cards_p[i].value == val_2)
                  {
                     ++n_val_2;
                  }
               }

               Card_Value higher_value;
               Card_Value lower_value;
               if (n_val_2 > n_val_1)
               {
                  higher_value = val_2;
                  lower_value = val_1;
               }
               else
               {
                  higher_value = val_1;
                  lower_value = val_2;
               }

               for (int i = 0; i < n_held; i++)
               {
                  if (cards_p[i].value == higher_value)
                  {
                     temp_best[best_arr_index++] = cards_p[i];
                  }
                  else if(cards_p[i].value != lower_value)
                  {
                     temp_rest[rest_arr_index++] = cards_p[i];
                  }
               }
               for (int i = 0; i < n_held; i++)
               {
                  if (cards_p[i].value == lower_value)
                  {
                     temp_best[best_arr_index++] = cards_p[i];
                  }
               }

               if (n_val_1 == 3
                  || n_val_2 == 3)
               {
                  best_hand_value_p = Hand_Values.FULL_HOUSE;
               }
               else
               {
                  best_hand_value_p = Hand_Values.TWO_PAIR;
               }
            }

            // handle four of a kind, three of a kind, one pair, high card
            else
            {
               int counter = 0;
               for (int i = 0; i < n_held; i++)
               {
                  if (cards_p[i].value == val_1)
                  {
                     ++counter;
                     temp_best[best_arr_index++] = cards_p[i];
                  }
                  else
                  {
                     temp_rest[rest_arr_index++] = cards_p[i];
                  }
               }

               switch (counter)
               {
                  case 1:
                     best_hand_value_p = Hand_Values.HIGH_CARD;
                     break;
                  case 2:
                     best_hand_value_p = Hand_Values.ONE_PAIR;
                     break;
                  case 3:
                     best_hand_value_p = Hand_Values.THREE_OF_A_KIND;
                     break;
                  case 4:
                     best_hand_value_p = Hand_Values.FOUR_OF_A_KIND;
                     break;
                  default:
                     best_hand_value_p = Hand_Values.HIGH_CARD;
                     break;
               }
            }
         }



         // recreate best_p and fill it with temp_best's contents up to best_arr_index's index
         // also recreate rest_p
         the_best_p = new Card[best_arr_index];
         the_rest_p = new Card[rest_arr_index];

         for (int i = 0; i < best_arr_index; i++)
         {
            the_best_p[i] = temp_best[i];
         }
         for (int i = 0; i < rest_arr_index; i++)
         {
            the_rest_p[i] = temp_rest[i];
         }

         // reestablish the hand based off of the_best and the_rest
         int hand_index = 0;
         for (int i = 0; i < the_best_p.Length; i++)
         {
            cards_p[hand_index++] = the_best_p[i];
         }
         for (int i = 0; i < the_rest_p.Length; i++)
         {
            cards_p[hand_index++] = the_rest_p[i];
         }
      }

      private void fill_best_flush(bool straight, Card_Value val_1)

      // bubble sort by suite
      private void organize_by_suite()
      {
         for (int i = 0; i < n_held; i++)
         {
            for (int j = i; j < n_held; j++)
            {
               if (cards_p[i] != null
                  && cards_p[j] != null
                  && cards_p[i].suite < cards_p[j].suite)
               {
                  Card temp = cards_p[i];
                  cards_p[i] = cards_p[j];
                  cards_p[j] = temp;
               }
            }
         }
      }

      private bool check_for_flush(bool ignore_n_held)
      {
         Card_Suite flush_suite;
         if(n_held == 0
            || cards_p[0] == null){
            return false;
         }
         else if(n_held < max_size
            && !ignore_n_held)
         {
            return false;
         }
         else{
            flush_suite = cards_p[0].suite;
         }

         for (int i = 1; i < n_held; i++)
         {
            if (cards_p[i].suite != flush_suite)
            {
               return false;
            }
         }

         return true;
      }

      private bool check_for_flush()
      {
         return check_for_flush(false);
      }

      private bool check_for_straight(out Card_Value straight_start_val, bool ignore_n_held)
      {
         if(n_held == 0
            || cards_p.Length < 1)
         {
            straight_start_val = Card_Value.END;
            return false;
         }
         else
         {
            straight_start_val = cards_p[0].value;
         }

         // handle royal straight
         if (check_for_royal_straight())
         {
            straight_start_val = Card_Value.TEN;
            return true;
         }

         // handle normal straights
         int n_straight = 1;
         for (int i = 1; i < n_held; i++)
         {
            if (cards_p[i].value == cards_p[i - 1].value)
            {
               continue;
            }
            else if (cards_p[i].value == cards_p[i - 1].value - 1)
            {
               ++n_straight;
               straight_start_val = cards_p[i].value;
            }
            else
            {
               n_straight = 1;
               straight_start_val = cards_p[i].value;
            }
         }

         if (n_straight == 4 && ignore_n_held)
         {
            return true;
         }
         else if (n_straight >= 5)
         {
            return true;
         }
         else
         {
            return false;
         }
      }

      private bool check_for_royal_straight()
      {
         bool king_held = false;
         bool queen_held = false;
         bool jack_held = false;
         bool ten_held = false;
         bool ace_held = false;

         if (n_held < 5)
         {
            return false;
         }

         for (int i = 0; i < n_held; i++)
         {
            switch (cards_p[i].value)
            {
               case(Card_Value.ACE):
                  ace_held = true;
                  break;
               case(Card_Value.KING):
                  king_held = true;
                  break;
               case(Card_Value.QUEEN):
                  queen_held = true;
                  break;
               case(Card_Value.JACK):
                  jack_held = true;
                  break;
               case(Card_Value.TEN):
                  ten_held = true;
                  break;
               default:
                  continue;
            }
         }

         if(ace_held
            && king_held
            && queen_held
            && jack_held
            && ten_held)
         {
            return true;
         }
         else
         {
            return false;
         }
      }

      private bool check_for_straight(out Card_Value straight_start_val)
      {
         return check_for_straight(out straight_start_val, false);
      }

      */

      public int discard(Card_Info[] info_arr)
      {
         int n_discarded = 0;

         foreach (Card_Info info in info_arr)
         {
            for (int hand_index = 0; hand_index < n_held; hand_index++)
            {
               if (cards_p[hand_index].value == info.value
                  && cards_p[hand_index].suite == info.suite)
               {
                  deck_p.discard(cards_p[hand_index]);

                  if (hand_index == n_held)
                  {
                     cards_p[hand_index] = null;
                  }
                  else
                  {
                     cards_p[hand_index] = cards_p[n_held-1];
                     cards_p[n_held-1] = null;
                  }

                  --n_held_p;

                  ++n_discarded;
               }
            }
         }

         return n_discarded;
      }

      public int discard(Card[] cards)
      {
         int n_discarded = 0;
         
         for(int i = 0; i < cards.Length; i++){
            int card_index = 0;
            while (card_index < cards.Length)
            {
               Card card = cards[card_index];

               if (card == null)
               {
                  break;
               }

               for (int hand_index = 0; hand_index < n_held; hand_index++)
               {
                  if (cards_p[hand_index].value == card.value
                     && cards_p[hand_index].suite == card.suite)
                  {
                     deck_p.discard(cards_p[hand_index]);

                     if (hand_index == n_held)
                     {
                        cards_p[hand_index] = null;
                     }
                     else
                     {
                        cards_p[hand_index] = cards_p[n_held - 1];
                        cards_p[n_held - 1] = null;
                     }

                     --n_held_p;
                     ++n_discarded;
                     break;
                  }
               }
               ++card_index;
            }
         }

         return n_discarded;
      }

      public override string ToString()
      {
         if (n_held == 0)
         {
            return "Empty hand";
         }

         string to_string = cards_p[0].ToString();

         for (int i = 1; i < n_held; i++)
         {
            to_string += ", ";
            to_string += cards_p[i].ToString();
         }
         
         return to_string;
      }

      static public string get_winning_hand_string(Hand hand)
      {
         string winning_hand_string = "";

         if (hand.best_hand_value != Hand_Values.HIGH_CARD)
         {
            winning_hand_string = String.Format("{0}", StringManip.StringManip.proper_capitalization(hand.best_hand_value.ToString(), true));
         }
         switch (hand.best_hand_value)
         {
            case (Hand_Values.HIGH_CARD):
               winning_hand_string = String.Format("{0}", hand.cards[0].ToString());
               break;
            case (Hand_Values.FOUR_OF_A_KIND):
            case (Hand_Values.THREE_OF_A_KIND):
            case (Hand_Values.ONE_PAIR):
               winning_hand_string += String.Format(" of {0}s", StringManip.StringManip.proper_capitalization(hand.cards[0].value.ToString()), true);
               break;
            case (Hand_Values.FULL_HOUSE):
            case (Hand_Values.TWO_PAIR):
               string first_set_type = "pair";
               if (hand.best_hand_value == Hand_Values.FULL_HOUSE)
               {
                  first_set_type = "triplet";
               }
               winning_hand_string += String.Format("; a {0} of {1}s, and a pair of {2}s", first_set_type, StringManip.StringManip.proper_capitalization(hand.cards[0].value.ToString(), true), StringManip.StringManip.proper_capitalization(hand.cards[3].value.ToString(), true));
               break;
            case (Hand_Values.ROYAL_FLUSH):
            case (Hand_Values.FLUSH):
               winning_hand_string += String.Format(" of {0}", StringManip.StringManip.proper_capitalization(hand.cards[0].suite.ToString()));
               break;
            //case(Hand_Values.ROYAL_STRAIGHT):
            case (Hand_Values.STRAIGHT):
               winning_hand_string += String.Format(" ending in {0}s", StringManip.StringManip.proper_capitalization(hand.cards[0].value.ToString(), true));
               break;
            case (Hand_Values.STRAIGHT_FLUSH):
               winning_hand_string += String.Format(" ending in a {0}", hand.cards[0].ToString());
               break;
            default:
               break;
         }

         return winning_hand_string;
      }

      // returns the winning card_value out of the passed in hands
      static public Card_Value tie_breaker(Hand[] hands)
      {
         Hand_Values winning_hand_value = hands[0].best_hand_value;
         Card_Value winning_card_value = hands[0].cards[0].value;
         Card high_card = hands[0].cards[0];

         for (int i = 0; i < hands.Length; i++)
         {
            if (hands[i].best_hand_value < winning_hand_value)
            {
               continue;
            }
            else
            {
               // select high card
               /*
               switch (winning_hand_value)
               {
                  case(Hand_Values.HIGH_CARD):
                  case(Hand_Values.FLUSH):
                  case(Hand_Values.STRAIGHT):
                  case(Hand_Values.STRAIGHT_FLUSH):
                  case(Hand_Values.ROYAL_STRAIGHT):
                  case(Hand_Values.ROYAL_FLUSH):
                     high_card = hands[i].cards[0];
                     break;
                  case(Hand_Values.ONE_PAIR):
                  case(Hand_Values.TWO_PAIR):
                     high_card = hands[i].cards[2];
                     break;
                  case(Hand_Values.THREE_OF_A_KIND):
                  case(Hand_Values.FULL_HOUSE):
                     high_card = hands[i].cards[3];
                     break;
                  case(Hand_Values.FOUR_OF_A_KIND):
                     high_card = hands[i].cards[4];
                     break;
               }
               */

               high_card = get_best_tie_card(hands[i]);
               // guard condition
               if (high_card == null)
               {
                  return Card_Value.END;
               }
               
               if (high_card.value > winning_card_value)
               {
                  winning_card_value = high_card.value;
               }
            }
         }

         return winning_card_value;
      }

      static public Card get_best_tie_card(Hand hand)
      {
         if(hand == null
            || hand.n_held < 1)
         {
            return null;
         }

         Card high_card = hand.cards[0];
         Hand_Values winning_hand_value = hand.best_hand_value;

         // select high card
         switch (winning_hand_value)
         {
            case (Hand_Values.HIGH_CARD):
            case (Hand_Values.FLUSH):
            case (Hand_Values.STRAIGHT):
            case (Hand_Values.STRAIGHT_FLUSH):
            case (Hand_Values.ROYAL_STRAIGHT):
            case (Hand_Values.ROYAL_FLUSH):
               high_card = hand.cards[0];
               break;
            case (Hand_Values.ONE_PAIR):
            case (Hand_Values.TWO_PAIR):
               high_card = hand.cards[2];
               break;
            case (Hand_Values.THREE_OF_A_KIND):
            case (Hand_Values.FULL_HOUSE):
               high_card = hand.cards[3];
               break;
            case (Hand_Values.FOUR_OF_A_KIND):
               high_card = hand.cards[4];
               break;
         }

         return high_card;
      }

      // Fields
      private int max_size_p;
      private Card[] cards_p;
      private int n_held_p;
      const int STRAIGHT_LENGTH = 5;
      const int FLUSH_LENGTH = 5;
      private Deck deck_p;
      private Card[] the_best_p;
      private Card[] the_rest_p;
      private Hand_Values best_hand_value_p;
      private static Random rand;

      // Properties
      public int max_size
      {
         get { return max_size_p; }
      }
      public Card[] cards
      {
         get
         {
            Card[] cards_temp = new Card[n_held_p];
            for (int i = 0; i < n_held_p; i++)
            {
               cards_temp[i] = new Card(cards_p[i]);
            }

            return cards_temp;
         }
      }
      public int n_held
      {
         get { return n_held_p; }
      }
      public int n_missing{
         get { return max_size - n_held; }
      }
      public Deck deck
      {
         get { return deck_p; }
         set { deck_p = value; }
      }
      public Card[] the_best
      {
         get
         {
            Card[] best = new Card[the_best_p.Length];
            for (int i = 0; i < the_best_p.Length; i++)
            {
               best[i] = the_best_p[i];
            }
            return best;
         }
      }
      public Card[] the_rest
      {
         get
         {
            Card[] rest = new Card[the_rest_p.Length];
            for (int i = 0; i < the_rest_p.Length; i++)
            {
               rest[i] = the_rest_p[i];
            }
            return rest;
         }
      }
      public Hand_Values best_hand_value
      {
         get { return best_hand_value_p; }
      }
   }
}
