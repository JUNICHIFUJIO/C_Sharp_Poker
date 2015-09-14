using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// consists of multiple cards (default of 52)
// contains multiple suites, multiple values, some jokers, etc.

// can be shuffled
// can be split
// can be checked for how many remain
// can be checked for how many are in the discard pile
// can peek at top number of cards
// can check what cards remain in the deck

// public
//    shuffle
//    split
//    check #
//    check discard #
//    peek
//    

// private
//    holding array for cards before shuffling
//    array of cards
//    order of cards
//    array of discarded cards
//    # of suites
//    range of values
//    face cards allowed
//    create deck from notepad of cards wanted, can be parsed

namespace Poker
{
   class Deck 
   {
      // Test mainframe
      /*
      public static void Main(string[] args)
      {
         Deck deck = new Deck();

         for (int i = 0; i < deck.size; i++)
         {
            Console.Write("Card #{0}:\t", i);
            deck.deck[i].display();
         }

         Console.WriteLine("Now we are going to try shuffling...");
         deck.shuffle();

         for (int i = 0; i < deck.size; i++)
         {
            Console.Write("Card #{0}:\t", deck.deck[i].ID);
            deck.deck[i].display();
         }
         Console.WriteLine();

         Console.WriteLine("Would you like to try shuffling again? Type quit to quit.");
         string retry = Console.ReadLine();
         while (!retry.ToLower().Equals("quit"))
         {
            Console.WriteLine();
            Console.WriteLine("Shuffling again...");
            deck.shuffle();

            for (int i = 0; i < deck.size; i++)
            {
               Console.Write("Card #{0}:\t", deck.deck[i].ID);
               deck.deck[i].display();
            }
            Console.WriteLine();

            Console.WriteLine("Would you like to try shuffling again? Type quit to quit.");
            retry = Console.ReadLine();
         }
      }
       * */

      // Constructors

      /*
       * public Deck(File text_file){
       *    foreach line in file
       *    ArrayList<string> lines;
       *    lines.add(line);
       *    
       *    decks = new Card[lines.size]
       *    
       *    for(int i = 0; i < lines.size; i++){
       *       deck[i] = new Card(line);
       */

      public Deck(Card[] cards)
      {
         deck = cards;
         n_cards = cards.Length;
         top_index = 0;
         if (n_cards > 0)
         {
            top_card = cards[0];
         }
         discard_pile = new Stack<Card>();
      }

      public Deck() : this(standard_deck) { }

      static Deck()
      {
         rand = new Random();

         // construct the standard deck
         standard_deck = new Card[52];
         uint ID = 0;

         for (int i = 0; i < (int)Card_Suite.END; i++)
         {
            for (int j = 1; j < (int)Card_Value.END; j++)
            {
               Card_Info info = new Card_Info();
               info.suite = (Card_Suite)i;
               info.value = (Card_Value)j;
               info.mark = "A fresh playing card with no markings.";
               standard_deck[i * ((int)Card_Value.END - 1) + j - 1] = new Card(info, ID);
               ++ID;
            }
         }
      }

      // Methods
      public void shuffle()
      {
         for (int times = 0; times < rand.Next(3) + 1; times++)
         {
            int n_shuffles = rand.Next(14) + 7;
            int card_step = rand.Next(4) + 4;

            for (int i = 0; i < n_shuffles; i++)
            {
               shuffle_recursive(card_step);
               int pivot = rand.Next((n_cards / 2) - 1) + 1;
               bool reverse_order = card_step % 2 == 1;
               //flip(rand.Next((n_cards / 2) - 1) + 1, (card_step % 2 == 1));
               flip(pivot, reverse_order);
               split(rand.Next(2) + 1);
            }

            for (uint i = 0; i < n_cards; i++)
            {
               deck[i].ID = i;
            }
         }
      }

      private void shuffle_recursive(int card_step){
         if (card_step <= 0)
         {
            return;
         }

         int index = top_index;
         
         // swap cards in the card_step
         while(index < size){
            int index1 = index;
            Card card1 = deck[index];
            index += card_step;
            if(index >= n_cards){
               break;
            }
            int index2 = index;
            Card card2 = deck[index];

            // swap cards
            deck[index1] = card2;
            deck[index2] = card1;

            index += card_step;
         }

         shuffle_recursive(card_step - 1);
      }

      private void flip(int pivot_index)
      {
         flip(pivot_index, false);
      }

      private void flip(int pivot_index, bool reverse_order)
      {
         if (pivot_index > size / 2)
         {
            pivot_index = size / 2;
         }

         if (!reverse_order)
         {

            for (int i = 0; i < pivot_index; i++)
            {
               int index1 = top_index + i;
               int index2 = 2 * pivot_index - i - 1 + top_index;
               Card card1 = deck[index1];
               Card card2 = deck[index2];
               deck[index1] = card2;
               deck[index2] = card1;
            }
         }
         else
         {
            int index1 = deck.Length - 1;
            int index2 = deck.Length - 2 * pivot_index;

            for (int i = 0; i < pivot_index; i++)
            {
               Card card1 = deck[index1];
               Card card2 = deck[index2];
               deck[index1--] = card2;
               deck[index2++] = card1;
            }
         }
      }

      // split a deck into multiple stacks and then put them back together in reverse stack order, starting from the middle (outermost cards will remain unchanged)
      public void split(int n_stacks)
      {
         if (n_stacks < 1)
         {
            n_stacks = 1;
         }

         int cards_per_stack = size/n_stacks + size%n_stacks;
         Card[][] stacks = new Card[n_stacks][];

         int stack_length = size/n_stacks;
         int index_left = size / 2 - 1;
         if (index_left < 0)
         {
            return;
         }
         int index_right = index_left + stack_length;

         // handle odd numbered n_stacks to adjust indices to skip the middle stack
         if(n_stacks %2 == 1){
            int adjustment_steps = 0;
            while(adjustment_steps < stack_length){
               ++index_right;
               ++adjustment_steps;
               if(adjustment_steps >= stack_length){
                  break;
               }
               --index_left;
               ++adjustment_steps;
            }
         }

         // go through deck, swapping cards for each stack as you branch out from the middle
         for(int i = 0; i < n_stacks/2; i++){
            for(int j = 0; j < stack_length; j++){
               if(index_left < 0 || index_right >= n_cards){
                  break;
               }
               Card left_card = deck[index_left--];
               Card right_card = deck[index_right--];
            }

            index_right += 2*stack_length;
         }
      }

      public Card[] draw(int n)
      {
         // guard condition
         if (n > size + discard_pile.Count)
         {
            n = size + discard_pile.Count;
         }
         if (n < 0)
         {
            n = 0;
         }

         // requires gathering discarded cards and reshuffling
         if(size < n){
            reset();
         }
         Card[] cards = new Card[n];

         for (int i = 0; i < n; i++)
         {
            cards[i] = deck[top_index + i];
            deck[top_index + i].unavailable = true;
         }

         top_index += n;
         if (top_index >= n_cards)
         {
            top_card = null;
         }
         else
         {
            top_card = deck[top_index];
         }

         return cards;
      }

      public Card[] peek(int n)
      {
         int cards_remaining = n_cards - top_index - 1;
         if (cards_remaining < n)
         {
            n = cards_remaining;
         }
         Card[] cards = new Card[n];

         for (int i = 0; i < n; i++)
         {
            cards[i] = new Card(deck[top_index + i]);
         }

         return cards;
      }

      public void discard(Card card)
      {
         if(deck[card.ID].value == card.value
            && deck[card.ID].suite == card.suite)
         {
            deck[card.ID].discard();
            discard_pile.Push(deck[card.ID]);
         }

         // handle reassigning top_card and top_card_index
         if (top_card == deck[card.ID]
            || top_index == card.ID)
         {
            top_index++;
            if (top_index >= n_cards)
            {
               top_card = null;
            }
            else
            {
               top_card = deck[top_index];
            }
         }
      }

      public void discard(uint ID)
      {
         deck[ID].discard();
         discard_pile.Push(deck[ID]);

         // handle reassigning top_card and top_card_index
         if (top_card == deck[ID]
            || top_index == ID)
         {
            top_index++;
            if (top_index >= n_cards)
            {
               top_card = null;
            }
            else
            {
               top_card = deck[top_index];
            }
         }
      }

      public void reset()
      {
         int currently_in_use = 0;
         //System.Collections.Generic.HashSet<Card> cards_in_use_set = new System.Collections.Generic.HashSet<Card>();
         //Card[] cards_in_use = new Card[top_index - discard_pile.Count];
         Card[] new_deck = new Card[n_cards];
         int unused_index = top_index - discard_pile.Count;
         int index = unused_index + size;

         for (int i = 0; i < n_cards; i++)
         {
            if (deck[i].discarded == true)
            {
               deck[i].discarded = false;
               deck[i].unavailable = false;
               deck[i].ID = (uint)index;
               new_deck[index++] = deck[i];
            }
            else if (deck[i].unavailable)
            {
               //cards_in_use_set.Add(deck[i]);
               deck[i].ID = (uint)currently_in_use;
               new_deck[currently_in_use++] = deck[i];
            }
            else
            {
               deck[i].ID = (uint)unused_index;
               new_deck[unused_index++] = deck[i];
            }
         }

         deck = new_deck;

         top_index = currently_in_use;
         top_card = deck[currently_in_use];
         discard_pile.Clear();
      }

      public void display_discard_pile()
      {
         if (discard_pile.Count == 0)
         {
            Console.WriteLine("Discard pile is empty.");
         }
         else
         {
            int discard_index = 0;
            foreach (Card card in discard_pile)
            {
               Console.Write("\tDiscard #{0}:\t", discard_index++);
               card.display();
            }
         }
      }

      // Fields
      private int n_cards;
      private Card[] deck;
      private Card top_card; // pointer to the top of the deck;
      private int top_index;
      static private Card[] standard_deck;
      private Stack<Card> discard_pile;
      static private Random rand;

      // Properties
      public int max_size
      {
         get { return n_cards; }
      }
      public int size
      {
         get { return n_cards - top_index; }
      }
      public Card top{
         get { return top_card; }
      }

      /*
      // Private classes
      static private class Deck_Iterator : IEnumerator<Card> {
         private Card card_iterator;
         private Deck deck;
         public Deck_Iterator(Deck deck){
            this.deck = deck;
            card_iterator = deck.top;
         }

         public Deck_Iterator() : this(new Deck(new Card[1])){}

         // Methods
         public Card get(){
            return card_iterator;
         }

         public void next(){
            if(card_iterator.ID < deck.n_cards-1){
               card_iterator = deck.deck[card_iterator.ID + 1];
            }
         }

         public void previous(){
            if(card_iterator.ID > deck.top.ID){
               card_iterator = deck.deck[card_iterator.ID - 1];
            }
         }

         public void begin(){
            card_iterator = deck.top;
         }

         public void end(){
            card_iterator = deck.deck[deck.n_cards - 1];
         }
      }
       * */
   }

}
