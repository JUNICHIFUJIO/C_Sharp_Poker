using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
   class AIPlayer : Player
   {
      /*
      // Testing mainframe
      public static void Main(string[] args)
      {
         Personality personality = new Personality(Wealth_Types.RICH, Betting_Types.EXTREME, Perception_Types.AWARE);
         AIPlayer player = new AIPlayer(personality, new Deck());

         player.AI_take_action(true, false, 20);

         Console.Read();
      }
      */

      // Constructor
      public AIPlayer(Personality personality, int starting_funds, int buy_in, int big_blind, int small_blind, int ante, Card[] starting_hand, Deck deck, int max_size, string name, bool is_male) : base(starting_funds, buy_in, big_blind, small_blind, ante, starting_hand, deck, max_size, name, is_male) {
         Wealth_Types actual_wealth_type = Wealth_Types.POOR;

         for (int i = (int)Wealth_Types.END - 1; i > 0; i--)
         {
            if (starting_funds > (int)(Wealth_Values)i)
            {
               actual_wealth_type = (Wealth_Types)i;
               break;
            }
         }

         AI_Ctor_Helper(personality);
      }

      public AIPlayer(Personality personality, Deck deck, string name, bool is_male) : this(personality, (int)(Personality.convert_wealth_type_to_value(personality.wealth_type)), Money.DEFAULT_BUY_IN, Money.DEFAULT_BIG_BLIND, Money.DEFAULT_SMALL_BLIND, Money.DEFAULT_ANTE, deck.draw(Hand.DEFAULT_MAX_SIZE), deck, Hand.DEFAULT_MAX_SIZE, name, is_male) {
         AI_Ctor_Helper(personality);
      }

      public AIPlayer(Personality personality, Deck deck) : this(personality, deck, pick_random_name(), true) { }

      static AIPlayer(){
         rand = new Random();
      }

      private void AI_Ctor_Helper(Personality personality){
         personality_p = personality;

         // decide if you'll bluff
         refresh_bluff();
      }

      static public string pick_random_name(bool is_male = true)
      {
         string[] male_names = {  "Rick",
                                   "Steve", 
                                   "Tom", 
                                   "Bill", 
                                   "Oliver", 
                                   "Josh", 
                                   "Ahmad", 
                                   "Kevin", 
                                   "Ben", 
                                   "Saitama", 
                                   "Jojo", 
                                   "Dio", 
                                   "Whammu", 
                                   "Kars", 
                                   "AC/DC", 
                                   "Dio, Stand Master", 
                                   "Genos", 
                                   "King",  
                                   "Robel", 
                                   "Thaddaeus", 
                                   "Garou", 
                                   "Kentarou", 
                                   "Goku", 
                                   "Frieza", 
                                   "Cell", 
                                   "Buu", 
                                   "Neko", 
                                   "Kuma", 
                                   "Teddy"};

         string[] female_names = {"Neko", 
                                    "Jessica", 
                                    "Isabella", 
                                    "Nicole", 
                                    "Quin An", 
                                    "Indigo", 
                                    "Kristi", 
                                    "Harriet", 
                                    "Grace", 
                                    "Ashley", 
                                    "Jennifer", 
                                    "Miki", 
                                    "Mariko", 
                                    "Nami", 
                                    "Robin", 
                                    "Nico Robin", 
                                    "Camie"};

         if (is_male)
         {
            int name_index = rand.Next(male_names.Length);
            return male_names[name_index];
         }
         else
         {
            int name_index = rand.Next(female_names.Length);
            return female_names[name_index];
         }
      }

      // Methods

      // bet
      // ante_up
      // end_turn
      // fold
      // pay_big_blind
      // pay_small_blind
      // pay_up
      // raise
      // should skip
      // status_refresh

      private int calculate_bet(bool first_turn, bool is_call, int call_amount, int actions_so_far_this_turn)
      {
         if(hand.cards.Length < 1
            || hand.cards[0] == null)
         {
            return 0;
         }

         Hand_Values hand_value = hand.best_hand_value;
         int personal_investment = money.current_bet;
         Card_Value high_card_value = hand.cards[0].value;

         int amount = personality.calculate_bet(
            bluff,                  // determines whether AI will bluff
            first_turn,             // whether a round of discards/betting has occurred
            is_call,                // whether someone has already bet
            n_folds,                // # of rounds ending in a fold -> pushes AI to bet or call bluffs
            call_amount,            // the current amount AI will have to match to stay in
            money.current_amount,   // money in possession (including what's been bet)
            available_funds,        // money allowed to bet
            personal_investment,    // money already placed up for bet
            hand_value,             // value of the hand
            high_card_value);       // determining card value of the hand

         // prevent from looping
         // also another factor determining if you should call instead of raise
         const double PERCENTAGE_GATE = .13;
         double loop_flag_percentage = 0;
         if (personal_investment == 0
            || call_amount == 0)
         {
            loop_flag_percentage = PERCENTAGE_GATE + 1; // 100 call_amount, 5000 starting funds; 2% flag_percentage
         }
         else
         {
            loop_flag_percentage = (double)call_amount / personal_investment; // 100 call_amount / 300 personal_investment = 30%
         }
         
         // determine if raise instead of call
         if (amount > call_amount)
         {
            if (loop_flag_percentage < PERCENTAGE_GATE)
            {
               // if raise, and in danger of having looped, just call instead of raise
               amount = call_amount;
            }
            else if(actions_so_far_this_turn > 1
               && first_turn)
            {
               amount = call_amount;
            }
            else if(actions_so_far_this_turn > 4
               && !first_turn)
            {
               amount = call_amount;
            }
         }

         // ERROR TESTING
         if (amount > 0
            && amount < 5)
         {
            amount = 5;
         }

         return amount;
      }

      // overwrites choose_action from Player
      new public Actions choose_action(int current_bet, int call_amount, bool already_bet = false)
      {
         Console.WriteLine("Invalid choose_action taken by AI class");
         return Actions.END;
      }

      // overwrites take_action from Player
      new public int take_action(Actions action, int current_bet)
      {
         Console.WriteLine("Invalid take_action taken by AI class");
         return 0;
      }

      public int AI_take_action(bool first_turn, bool is_call, int call_amount, int actions_so_far_this_turn)
      {
         int bet = calculate_bet(first_turn, is_call, call_amount, actions_so_far_this_turn);
         Actions action = Actions.END;

         // handle raises
         if (bet == 0)
         {
            action = Actions.FOLD;
            fold();
         }
         else if(bet == call_amount){
            action = Actions.CALL;
         }
         else if(bet > call_amount){ // determine that you want to raise
            if (is_call)
            {
               bet -= call_amount;
               if (bet == call_amount)
               {
                  action = Actions.CALL;
               }
               else
               {
                  action = Actions.RAISE;
               }
            }
            else
            {
               action = Actions.BET;
            }
         }
         else
         {
            Console.WriteLine("ERROR: returned bet from calculate_bet is non-zero, but less than previous bet; bet: {0}; call_amount: {1}", bet, call_amount);
            bet = call_amount;
            action = Actions.CALL;
            //action = Actions.END;
         }

         // bet the appropriate amount
         bet = money.bet(bet);
         if (action == Actions.RAISE)
         {
            money.bet(call_amount);
         }

         // adjust all_in appropriately
         if (available_funds <= 0)
         {
            all_in = true;
            if(action == Actions.RAISE
               || action == Actions.BET){
               if(bet <= call_amount){
                  action = Actions.CALL;
               }
            }
         }
         display_turn(action, bet, is_call);
         last_action = action;
         last_bet = bet;
         ++times_this_turn;

         return bet;
      }

      new public int discard()
      {
         int n_discarded = 0;
         Card[] initial_discard_hand;

         if (hand.best_hand_value == Hand_Values.HIGH_CARD
            && hand.cards[0].value > Card_Value.JACK)
         {
            initial_discard_hand = new Card[hand.the_rest.Length - 1];
            for (int i = 1; i < hand.the_rest.Length; i++)
            {
               initial_discard_hand[i - 1] = hand.the_rest[i];
            }
            
            //n_discarded = hand.discard(most_of_the_rest);
         }
         else
         {
            //n_discarded = hand.discard(hand.the_rest);
            initial_discard_hand = hand.the_rest;
         }

         // finalize the initial discard hand by deciding a random group of the ones to discard
         int n_to_discard = initial_discard_hand.Length;
         Card[] final_discard_hand;
         if (initial_discard_hand.Length > 3)
         {
            n_to_discard = rand.Next(initial_discard_hand.Length - 3) + 3;
            final_discard_hand = new Card[n_to_discard];
         }
         else
         {
            final_discard_hand = initial_discard_hand;
         }

         // fill final discard hand and discard it
         for (int i = n_to_discard - 1; i >= 0; i--)
         {
            final_discard_hand[i] = initial_discard_hand[i];
         }
         n_discarded = hand.discard(final_discard_hand);

         // draw discarded hands
         hand.draw(n_discarded);

         // reset tell
         tell = decide_tell();

         return n_discarded;
      }

      new public void draw_new_hand()
      {
         hand.draw_new_hand();
         tell = decide_tell();
      }

      new protected string decide_tell()
      {
         string tell_candidate = "";
         if (bluff
            && hand.best_hand_value < Hand_Values.END)
         {
            Hand_Values bluff_hand_value = (Hand_Values)(rand.Next((int)Hand_Values.END - (int)hand.best_hand_value - 1) + (int)hand.best_hand_value) + 1;
            tell_candidate = base.decide_tell(bluff_hand_value);
         }
         else
         {
            tell_candidate = base.decide_tell();
         }

         return tell_candidate;
      }

      public void display_turn(Actions action, int amount, bool is_call)
      {
         if (!is_call
            && action == Actions.CALL)
         {
            Console.Write("{0} {1}s", name, "Check");
         }
         else
         {
            Console.Write("{0} {1}s", name, action.ToString().ToLower());
         }
         // Jimmy fold; John raise; John bet;
         if (action == Actions.BET)
         {
            Console.WriteLine(" ${0}", amount);
         }
         else if (action == Actions.RAISE)
         {
            Console.WriteLine(" by ${0}", amount);
         }
         else
         {
            Console.WriteLine();
         }
      }

      public void update_tell_chance_modifier(Player[] players)
      {
         int[] tiers = new int[players.Length - 1];
         int tier_index = 0;
         for (int i = 0; i < players.Length; i++)
         {
            if (players[i] == null)
            {
               break;
            }
            else if (players[i].Equals(this))
            {
               continue;
            }
            else
            {
               tiers[tier_index++] = players[i].get_tell_tier();//players[i].tell_type, players[i].tell);
            }
         }

         personality.update_tell_chance_modifier(tiers, get_tell_tier());
      }

      new public void refresh_bluff()
      {
         if (hand == null)
         {
            return;
         }

         if (hand.best_hand_value < Hand_Values.TWO_PAIR)
         {
            if (rand.Next(9) > 6)
            {
               bluff = true;
            }
            else
            {
               bluff = false;
            }
         }

      }

      // Fields
      private Personality personality_p;
      private static Random rand;

      // Properties
      public Personality personality{
         get { return personality_p; }
         private set { personality_p = value; }
      }
   }
}
