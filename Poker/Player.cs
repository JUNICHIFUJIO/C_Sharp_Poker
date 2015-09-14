using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StringManip;

// has money
// has a hand
// has turn order
// has big blind/little blind/ante responsibilities
// has a personality
// is in/out/folded/waiting/taking a turn (handled by game)

namespace Poker
{

   // Constants and Enums and Structs
   public enum Actions { FOLD, CALL, BET, RAISE, CHECK_HAND, CHECK_TELLS, CHECK_DISCARDS, END }
   

   class Player
   {
      /*
      // testing mainframe
      public static void Main(string[] args)
      {
         Deck deck = new Deck();
         deck.shuffle();
         Player player = new Player(deck);

         Console.WriteLine("player currently has ${0} and is betting ${1}", player.money.current_amount, player.money.current_bet);
         Console.WriteLine("Let's make player bet $3000...");
         player.bet(player.money.current_bet);
         Console.WriteLine();
         Console.WriteLine("Well done...the player is now betting ${0}", player.money.current_bet);

         Console.WriteLine("Let's raise the stakes...make him raise by another $3000...");
         player.raise(player.money.current_bet);
         Console.WriteLine();
         Console.WriteLine("Well done again...now let's check if we should skip him or not.");

         Console.WriteLine("It looks his skippable status is: {0}", player.should_skip());

         Console.WriteLine("Well now...let's refresh him.");
         player.money.refresh();
         player.status_refresh();
         Console.WriteLine("It looks like his skippable status is now: {0}", player.should_skip());

         Console.WriteLine("Let's make him fold to see how that affects his skippable status.");
         player.fold();
         Console.WriteLine("Status: {0};", player.should_skip());
         player.status_refresh();
         Console.WriteLine("After refreshing again: {0}", player.should_skip());

         Actions action = player.choose_action(player.money.current_bet);
         Console.WriteLine("You chose to {0}", action.ToString());
         player.take_action(action, player.money.current_bet);

         Console.Read();
      }
       * */


      protected enum Tell_Types { SCRATCH_EAR, BEND_CARD, RUB_NOSE, LOOK_AROUND, SHIFT_UNCOMFORTABLY, BITE_LIP, TAP_FINGER, CLEAR_THROAT, LEAN_BACK, COVER_MOUTH, END }

      // Constructors
      public Player(int starting_funds, int buy_in, int big_blind, int small_blind, int ante, Card[] starting_hand, Deck deck, int max_size, string name, bool is_male){
         money_p = new Money(starting_funds, buy_in, big_blind, small_blind, ante);
         hand_p = new Hand(starting_hand, deck, max_size);
         is_out_p = false;
         folded_p = false;
         all_in_p = false;
         refresh_bluff();
         name_p = name;
         is_male_p = is_male;
         n_times_this_turn = 0;
         tell_type_p = (Tell_Types)rand.Next((int)Tell_Types.END);
         tell_p = decide_tell();
         last_action_p = Actions.CALL;
         last_bet_p = 0;
      }


      public Player(Deck deck, string name, bool is_male) : this(Money.DEFAULT_ORIGINAL_AMOUNT, Money.DEFAULT_BUY_IN, Money.DEFAULT_BIG_BLIND, Money.DEFAULT_SMALL_BLIND, Money.DEFAULT_ANTE, deck.draw(5), deck, 5, name, is_male) { }

      public Player(Deck deck) : this(Money.DEFAULT_ORIGINAL_AMOUNT, Money.DEFAULT_BUY_IN, Money.DEFAULT_BIG_BLIND, Money.DEFAULT_SMALL_BLIND, Money.DEFAULT_ANTE, deck.draw(5), deck, 5, "Player_1", true) { }

      static Player()
      {
         rand = new Random();
      }

      // Methods
      // bet
      // raise
      // take_action
      // should_skip
      public Actions choose_action(int current_bet, int pool, bool already_bet = false)
      {
         Actions action = Actions.END;
         string[] action_strings = new string[(int)Actions.END];

         for(int i = 0; i < (int)Actions.END; i++){
            action_strings[i] = StringManip.StringManip.proper_capitalization(((Actions)i).ToString());
         }

         Console.WriteLine("The current bet is ${0},", current_bet);
         Console.WriteLine("\t${0} in the pool", pool);
         int to_call = current_bet - money.current_bet;
         if(to_call > available_funds){
            to_call = available_funds;
         }
         Console.WriteLine("\t\tand you would need to put in ${0} to call.", to_call);
         Console.WriteLine("\t\t(You have ${0} left to levy this turn)", available_funds);
         Console.WriteLine();
         Console.WriteLine("What would you like to do?");
         int adjustment = 0;
         for (int i = 0; i < (int)Actions.END; i++)
         {
            if (already_bet
               && action_strings[i].Equals("Bet"))
            {
               --adjustment;
            }
            else if(!already_bet
               && action_strings[i].Equals("Raise"))
            {
               --adjustment;
            }
            else if(!already_bet
               && action_strings[i].Equals("Call")){
               Console.WriteLine("\t{0}) {1}", i + 1 + adjustment, "Check");
            }
            else
            {
               Console.WriteLine("\t{0}) {1}", i + 1 + adjustment, action_strings[i]);
            }
         }
         Console.WriteLine();

         while(action == Actions.END){
            Console.Write("Choice: ");
            string choice = Console.ReadLine().ToLower();
            choice = StringManip.StringManip.proper_capitalization(choice);

            int choice_index = 0;

            if(!already_bet
               && choice.Equals("Check"))
            {
               return Actions.CALL;
            }

            for (; choice_index < (int)Actions.END; choice_index++)
            {
               if (choice.Equals(action_strings[choice_index]))
               {
                  if(already_bet
                     && action_strings[choice_index].Equals("Bet")){
                     continue;
                  }
                  else if(!already_bet
                     && action_strings[choice_index].Equals("Raise"))
                  {
                     continue;
                  }
                  break;
               }
            }

            action = (Actions)choice_index;

            if(action == Actions.END){
               Console.WriteLine("Invalid choice. Try again.");
            }
         }

         return action;
      }

      public int take_action(Actions action, int current_bet)
      {
         int money_bet = 0;

         switch (action)
         {
            case(Actions.FOLD):
               fold();
               break;
            case(Actions.CALL):
               return money.bet(current_bet - money.current_bet);
            case(Actions.BET):
               return bet(current_bet);
            case(Actions.RAISE):
               return raise(current_bet);
            case(Actions.CHECK_HAND):
               hand_p.display();
               break;
            case(Actions.CHECK_TELLS):
               // Console.WriteLine("Apologies, Check_Tells has not been implemented as of yet.");
               break;
            case(Actions.CHECK_DISCARDS):
               hand_p.deck.display_discard_pile();
               break;
            default:
               break;
         }

         ++times_this_turn;

         last_action = action;
         last_bet = money_bet;

         return money_bet;
      }

      public int pay_big_blind(int big_blind)
      {
         int amount_paid = 0;
         if (big_blind > 0)
         {
            amount_paid = money.bet(big_blind);
            if(amount_paid < big_blind){
               all_in = true;
            }
         }
         return amount_paid;
      }

      public int pay_small_blind(int small_blind)
      {
         int amount_paid = 0;
         if (small_blind > 0)
         {
            amount_paid = money.bet(small_blind);
            if(amount_paid < small_blind){
               all_in = true;
            }
         }
         return amount_paid;
      }

      public int ante_up(int ante){
         int amount_paid = 0;
         
         if(ante > 0){
            amount_paid = money.bet(ante);
            if(amount_paid < ante){
               all_in = true;
            }
         }

         return amount_paid;
      }

      public int bet(int current_bet)
      {
         int amount = -1;
         string amount_str = "filler";

         while (amount < 0)
         {
            Console.WriteLine("You can bet up to:\n\t${0}", available_funds);
            Console.WriteLine("\t(The ante was {0})", current_bet);
            Console.WriteLine("How much would you like to bet?");
            while (!StringManip.StringManip.is_valid_number(amount_str))
            {
               Console.WriteLine("Type \"back\" to go back.)");
               Console.WriteLine();
               Console.Write("\t");
               amount_str = Console.ReadLine();
               if (amount_str.ToLower().Equals("back"))
               {
                  Console.WriteLine();
                  return -1;
               }
               else if (!StringManip.StringManip.is_valid_number(amount_str))
               {
                  Console.WriteLine("Invalid amount.");
               }
            }
            amount = StringManip.StringManip.atoi(amount_str);

            if (amount < 0)
            {
               Console.WriteLine("That is not a valid amount to bet.");
            }
         }

         int amount_actual = money.bet(amount); // need to have original ante/big/small blinds as part of the initial bet? // ERROR TESTING
         if (money.current_bet == current_bet)
         {
            Console.WriteLine("You checked.");
         }
         else if (money.current_amount == money.current_bet)
         {
            Console.WriteLine("You went all in.");
         }
         else
         {
            Console.WriteLine("You bet ${0}.", amount_actual);
         }

         if (available_funds <= 0)
         {
            all_in = true;
         }
         else
         {
            Console.WriteLine("Remaining funds: {0}", available_funds);
         }
         Console.WriteLine();

         return amount_actual;
      }

      public int raise(int current_bet)
      {
         int amount = -1;
         string amount_str = "filler";
         int call_amount = current_bet - money.current_bet;

         while(amount < 0){
            Console.WriteLine("You can raise by up to:\n\t${0}", available_funds);
            Console.WriteLine("\t(The current bet is ${0}, and you would need to put in ${1} to call)", current_bet, call_amount);
            Console.WriteLine("How much would you like to raise by?");
            while(!StringManip.StringManip.is_valid_number(amount_str))
            {
               Console.WriteLine("Type \"back\" to go back.)");
               Console.WriteLine();
               Console.Write("\t");
               amount_str = Console.ReadLine();
               if (amount_str.ToLower().Equals("back"))
               {
                  Console.WriteLine();
                  return -1;
               }
               else if (!StringManip.StringManip.is_valid_number(amount_str))
               {
                  Console.WriteLine("Invalid amount.");
               }
            }
            amount = StringManip.StringManip.atoi(amount_str);

            if(amount < 0){
               Console.WriteLine("That is not a valid amount to raise by.");
            }
         }

         int amount_actual = money.bet(amount);
         money.bet(call_amount); // raise by required call amount

         if (money.current_bet == current_bet)
         {
            Console.WriteLine("You called.");
         }
         else if(money.current_amount == money.current_bet)
         {
            Console.WriteLine("You went all in.");
         }
         else if(money.current_bet > current_bet)
         {
            Console.WriteLine("You raised by ${0}, the current bet is now ${1}", amount_actual, money.current_bet);
         }
         else
         {
            Console.WriteLine("Erroneous return from \"bet()\" when attempting to raise.");
         }
         Console.WriteLine();

         // update all_in value
         if (available_funds <= 0)
         {
            all_in = true;
         }
         //else
         //{
         //   Console.WriteLine("available_funds: {0}", available_funds);
         //}

         return amount_actual;
      }

      public void fold()
      {
         folded = true;
         ++times_folded;
      }

      public void check_tells(Player[] other_players)
      {
         string[] misleading_tells = { "looks back at you",
                                     "plays with the chips a bit",
                                     "sighs heavily",
                                     "looks at the clock",
                                     "looks at the table",
                                     "plays with the cards a bit",
                                     "mutters something"};

         Console.WriteLine();
         Console.WriteLine("\t\t\t77777777777777777777");
         Console.WriteLine();

         Console.WriteLine("You glance around the table...");
         foreach (Player player in other_players)
         {
            if (rand.NextDouble() > .7)
            {
               Console.Write("{0} {1}.", player.name, misleading_tells[rand.Next(misleading_tells.Length)]);
            }
            else
            {
               Console.Write("{0} {1}.", player.name, player.tell);
            }
            Console.Write(" (");

            switch (player.last_action)
            {
               case(Actions.FOLD):
                  Console.Write("{0} folded.", player.name);
                  break;
               case(Actions.CALL):
                  Console.Write("{0} called.", player.name);
                  break;
               case(Actions.BET):
                  Console.Write("{0} bet ${1}.", player.name, player.last_bet);
                  break;
               case(Actions.RAISE):
                  Console.Write("{0} raised by ${1}.", player.name, player.last_bet);
                  break;
               default:
                  Console.Write("{0}'s last action was {1}.", player.name, player.last_action);
                  break;
            }

            Console.WriteLine(")");
         }
         Console.WriteLine();
         Console.WriteLine("\t\t\t77777777777777777777");
         Console.WriteLine();
      }

      public int discard()
      {
         int n_discarded = 0;

         int[] choices = new int[hand.max_size];
         string raw_input = query_discard_choices();

         if (raw_input.ToLower().Equals("all"))
         {
            n_discarded = hand.discard(hand.cards);
         }
         else
         {

            TextParser parser = new TextParser();
            Card_Info[] chosen_cards = parser.extract_cards(raw_input);
            n_discarded = hand.discard(chosen_cards);
         }
         hand.draw(n_discarded);
         tell_p = decide_tell();
         return n_discarded;
      }

      public void draw_new_hand()
      {
         hand.draw_new_hand();
         tell_p = decide_tell();
      }

      private string query_discard_choices()
      {
         Console.WriteLine();
         Console.Write("Please type the cards you would like to discard: (separate cards with \",\" or \";\")");
         Console.Write(" (Ex: 10 of diamonds, king of spades, \"all\" for discarding your entire hand)");
         Console.WriteLine();
         for (int i = 0; i < hand.n_held; i++)
         {
            Console.Write("\t{0}) ", i + 1);
            hand.cards[i].display();
         }
         Console.WriteLine();
         string raw_input = Console.ReadLine();

         return raw_input;
      }

      public bool should_skip()
      {
         return is_out || folded || all_in;
      }

      protected int pay_up(){
         int amount = money.withdraw(money.current_bet);
         money.refresh();
         return amount;
      }

      protected void take_winnings(int winnings){
         Console.Write("{0} won ${1} with a ", name, winnings);
         Console.Write(Hand.get_winning_hand_string(hand));
         Console.WriteLine("!");

         // appropriately set money
         money.win(winnings);
         money.refresh();
      }

      
      
      // returns true if player is out
      public bool end_turn()
      {
         bool original_folded = folded;
         status_refresh();
         folded = original_folded;
         return is_out;
      }

      // returns amount taken from player
      public int end_round(bool won, int winnings){
         if (is_out)
         {
            n_times_this_turn = 0;
            folded_p = false;
            all_in_p = false;
            times_folded = 0;
            return 0;
         }

         int amount = 0;
         if(!folded){ // reset times ending a round with a fold
            const int N_FOLD_DECREMENT = 2;
            if (n_folds < N_FOLD_DECREMENT)
            {
               n_folds = 0;
            }
            else
            {
               n_folds -= N_FOLD_DECREMENT;
            }
         }

         if(!won){
            amount = pay_up();
         }
         else{
            money.withdraw(money.current_bet); // subtract amount you put in the pot
            take_winnings(winnings);
            amount = -1 * winnings;
         }

         // refresh player's money held and current bet
         money.refresh();

         bool originally_out = is_out;
         n_times_this_turn = 0;
         refresh_bluff();
         last_action = Actions.CALL;
         last_bet = 0;

         // refresh/update player status
         status_refresh();

         if (!originally_out && is_out)
         {
            Console.WriteLine();
            Console.WriteLine("{0} is out of the game!", name);
            Console.WriteLine();
         }

         return amount;
      }

      public void status_refresh(){
         if (money.current_amount == 0)
         {
            is_out = true;
         }
         else
         {
            is_out = false;
         }

         if (!is_out)
         {
            folded = false;
            if (money.current_bet == money.current_amount)
            {
               all_in = true;
            }
            else
            {
               all_in = false;
            }
         }
      }

      override public string ToString()
      {
         return name;
      }

      protected string decide_tell(Hand_Values best_hand_value)
      {
         string[] tell_arr = get_tell_array(tell_type);

         int tier = 0;
         if (best_hand_value > Hand_Values.FULL_HOUSE)
         {
            tier = 3;
         }
         else if (best_hand_value > Hand_Values.ONE_PAIR)
         {
            tier = 2;
         }
         else if (best_hand_value > Hand_Values.HIGH_CARD)
         {
            tier = 1;
         }

         return tell_arr[tier];
      }

      protected string decide_tell()
      {
         return decide_tell(hand.best_hand_value);
      }

      protected string[] get_tell_array(Tell_Types tell_type)
      {
         const int n_tiers = 4;
         string[] tell_array = new string[n_tiers];
         tell_array[0] = "is doing nothing out of the ordinary";

         string possessive = "his";
         string referral = "he";
         if(!is_male){
            possessive = "her";
            referral = "she";
         }

         // set the other tiers
         switch (tell_type)
         {
            case (Tell_Types.SCRATCH_EAR):
               tell_array[1] = String.Format("taps {0} ear", possessive);
               tell_array[2] = String.Format("scratches {0} ear", possessive);
               tell_array[3] = String.Format("tugs lightly on {0} ear", possessive);
               break;
            case (Tell_Types.BEND_CARD):
               tell_array[1] = String.Format("is rubbing one of {0} cards with {0} thumb and forefinger", possessive);
               tell_array[2] = String.Format("is lightly tugging {0} cards", possessive);
               tell_array[3] = String.Format("is squeezing {0} card and bending it slightly", possessive);
               break;
            case (Tell_Types.RUB_NOSE):
               tell_array[1] = String.Format("sniffs and shifts a bit");
               tell_array[2] = String.Format("rubs {0} nose for a second", possessive);
               tell_array[3] = String.Format("is sniffing uncontrollably");
               break;
            case (Tell_Types.LOOK_AROUND):
               tell_array[1] = String.Format("glances around at the other players when they aren't looking");
               tell_array[2] = String.Format("darts quick glances around the table");
               tell_array[3] = String.Format("is intently watching your every move");
               break;
            case (Tell_Types.SHIFT_UNCOMFORTABLY):
               tell_array[1] = String.Format("sniffs and shifts a bit");
               tell_array[2] = String.Format("scoots back in {0} chair a bit", possessive);
               tell_array[3] = String.Format("shifts uncomfortably until {0} sees you looking", referral);
               break;
            case (Tell_Types.BITE_LIP):
               tell_array[1] = String.Format("makes a bit of an odd face");
               tell_array[2] = String.Format("is nibbling on {0} lip", possessive);
               tell_array[3] = String.Format("is chewing on {0} lip", possessive);
               break;
            case (Tell_Types.TAP_FINGER):
               tell_array[1] = String.Format("is tapping one of the cards {0}'s staring at", referral);
               tell_array[2] = String.Format("taps the table impatiently");
               tell_array[3] = String.Format("drums {0} fingers on the table regularly", possessive);
               break;
            case (Tell_Types.CLEAR_THROAT):
               tell_array[1] = String.Format("quietly clears {0} throat", possessive);
               tell_array[2] = String.Format("clears {0} throat", possessive);
               tell_array[3] = String.Format("sharply and suddenly clears {0} throat", possessive);
               break;
            case (Tell_Types.LEAN_BACK):
               tell_array[1] = String.Format("leans back slightly");
               tell_array[2] = String.Format("leans back");
               tell_array[3] = String.Format("leans back with a confident smile");
               break;
            case(Tell_Types.COVER_MOUTH):
               tell_array[1] = String.Format("touches {0} lower lip", possessive);
               tell_array[2] = String.Format("slightly covers {0} mouth in a thoughtful way", possessive);
               tell_array[3] = String.Format("covers {0} mouth intently", possessive);
               break;
            default:
               tell_array[1] = String.Format("is doing something slightly out of the ordinary");
               tell_array[2] = String.Format("is doing something pretty out of the ordinary");
               tell_array[3] = String.Format("is doing something really out of the ordinary");
               break;
         }

         return tell_array;
      }

      protected int get_tell_tier(Tell_Types tell_type, string tell)
      {
         string[] tell_arr = get_tell_array(tell_type);

         int tier = 0;

         for (int i = 0; i < tell_arr.Length; i++)
         {
            if (tell.Equals(tell_arr[i]))
            {
               tier = i;
               break;
            }
         }

         return tier;
      }

      public int get_tell_tier()
      {
         return get_tell_tier(tell_type, tell);
      }

      public void refresh_bluff()
      {
         // empty hold-over
         bluff_p = false;
      }

      // Fields
      private Money money_p;
      private Hand hand_p;
      private bool is_out_p;
      private bool folded_p;
      private bool all_in_p;
      private bool bluff_p;
      private int n_times_this_turn;
      private string name_p;
      private int times_folded;
      private string tell_p;
      private bool is_male_p;
      private int last_bet_p;
      private Actions last_action_p;
      private Tell_Types tell_type_p;
      static private Random rand;

      // Properties
      public Money money
      {
         get { return money_p; }
      }
      public Hand hand
      {
         get { return hand_p; }
      }
      public bool is_out{
         get{ return is_out_p; }
         protected set
         {
            if (money.current_amount == 0)
            {
               if (value == true)
               {
                  is_out_p = true;
               }
               else
               {
                  Console.WriteLine("invalid setting of is_out to false");
               }
            }
            else
            {
               if (value == false)
               {
                  is_out_p = false;
               }
               else
               {
                  Console.WriteLine("invalid setting of is_out to true");
               }
            }
         }
      }
      public bool folded
      {
         get { return folded_p; }
         protected set { folded_p = value; }
      }
      public bool all_in
      {
         get { return all_in_p; }
         protected set
         {
            if (money.current_amount == money.current_bet)
            {
               if (value == true)
               {
                  all_in_p = true;
               }
               else
               {
                  Console.WriteLine("Unable to set all_in to false because current_amount == current_bet");
               }
            }
            else
            {
               if (value == false)
               {
                  all_in_p = false;
               }
               else
               {
                  Console.WriteLine("Unable to set all_in to true because current_amount != current_bet");
               }
            }
         }
      }
      public int available_funds
      {
         get { return money.current_amount - money.current_bet; }
      }
      public string name
      {
         get { return name_p; }
      }
      public int n_folds
      {
         get { return times_folded; }
         protected set {
            if (value >= 0)
            {
               times_folded = value;
            }
         }
      }
      public int times_this_turn
      {
         get { return n_times_this_turn; }
         protected set
         {
            if (value == 0)
            {
               n_times_this_turn = 0;
            }
            else if (value > n_times_this_turn)
            {
               n_times_this_turn = value;
            }
         }
      }
      public bool bluff
      {
         get { return bluff_p; }
         protected set { bluff_p = value; }
      }
      public string tell
      {
         get { return tell_p; }
         protected set
         {
            if (!value.Equals(""))
            {
               tell_p = value;
            }
         }
      }
      public bool is_male
      {
         get { return is_male_p; }
      }
      protected Tell_Types tell_type{
         get { return tell_type_p; }
      }
      public Actions last_action
      {
         get { return last_action_p; }
         protected set
         {
            last_action_p = value;
         }
      }
      public int last_bet
      {
         get { return last_bet_p; }
         protected set
         {
            if (value >= 0)
            {
               last_bet_p = value;
            }
         }
      }
   }
}
