using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
   public enum Wealth_Values { POOR = 500, ALRIGHT = 5000, WELL_OFF = 20000, RICH = 100000, END = 0 };
   public enum Wealth_Types { POOR, ALRIGHT, WELL_OFF, RICH, END };
   // stingy == small bets/raises at most
   // reckless == large bets/raises lots of the time
   // calculated == makes good bets based off of what they have
   // all_in == if it has a decent hand and moderate/small amounts of money, will automatically go all in
   // extreme == all in or nothing
   // random == random decisions that purposefullyhas lots of feints to try and trick players
   public enum Betting_Types { STINGY, RECKLESS, CALCULATED, ALL_IN, EXTREME, RANDOM, END };

   // ERROR TESTING
   // unincorporated
   public enum Perception_Types { OBLIVIOUS, AWARE, PERCEPTIVE, END };

   // error testing
   // private enum Money_Tiers { ANTE = 20, THIRTY = 30, FIFTY = 50, EIGHTY = 80, ONE_HUNDRED = 100, TWO_HUNDRED = 200, FOUR_HUNDRED = 400, SIX_HUNDRED = 600, EIGHT_HUNDRED = 800, ONE_THOUSAND = 1000, ONE_THOUSAND_FIVE_HUNDRED = 1500, TWO_THOUSAND = 2000, THREE_THOUSAND = 3000, FOUR_THOUSAND = 4000, FIVE_THOUSAND = 5000 };

   sealed class Personality
   {
      /*
      // Testing mainframe
      public static void Main(string[] args)
      {
         Personality personality = new Personality();
         Random rand = new Random();

         int[] amounts = new int[4];

         int total_funds = 5000;
         int bet = 0;


         amounts[0] = personality.calculate_bet((bool)(rand.Next(2) == 0), (bool)(rand.Next(2) == 0), (bool)(rand.Next(2) == 0), bet = rand.Next(40) + 20, total_funds, total_funds - bet, 20, Hand_Values.HIGH_CARD, (Card_Value)rand.Next((int)Card_Value.KING));

         for(int i = 1; i < amounts.Length; i++){
            amounts[i] = personality.calculate_bet((bool)(rand.Next(2) == 0), (bool)(rand.Next(2) == 0), (bool)(rand.Next(2) == 0), bet = rand.Next(40) + 20, total_funds, total_funds - bet, 20, (Hand_Values)rand.Next((int)(Hand_Values.ROYAL_FLUSH)), (Card_Value)rand.Next((int)Card_Value.KING));
         }
         int amount_A = personality.calculate_bet(false, true, false, 2300, 3000, 1000, 2000, Hand_Values.HIGH_CARD, (Card_Value)rand.Next((int)Card_Value.KING));
         int amount_B = personality.calculate_bet(true, false, false, 1000, 2000, 500, 20, Hand_Values.ONE_PAIR, (Card_Value)rand.Next((int)Card_Value.KING));

         Console.WriteLine("For a normal person..");
         for (int i = 0; i < amounts.Length; i++)
         {
            Console.WriteLine("amount #{0}: {1}", i + 1, amounts[i]);
         }
         Console.WriteLine("A: {0}", amount_A);
         Console.WriteLine("B: {0}", amount_B);

         Personality rich_boy = new Personality(Wealth_Types.RICH);
         total_funds = 100000;
         bet = 0;

         for (int i = 0; i < amounts.Length; i++)
         {
            amounts[i] = rich_boy.calculate_bet((bool)(rand.Next(2) == 0), (bool)(rand.Next(2) == 0), (bool)(rand.Next(2) == 0), bet = rand.Next(40) + 20, total_funds, total_funds - bet, 20, (Hand_Values)rand.Next((int)(Hand_Values.ROYAL_FLUSH)), (Card_Value)rand.Next((int)Card_Value.KING));
         }
         amount_A = rich_boy.calculate_bet(false, true, false, 2300, 100000, 6700, 1000, Hand_Values.HIGH_CARD, (Card_Value)rand.Next((int)Card_Value.KING));
         amount_B = rich_boy.calculate_bet(true, false, false, 1000, 100000, 98980, 20, Hand_Values.ONE_PAIR, (Card_Value)rand.Next((int)Card_Value.KING));

         Console.WriteLine();
         Console.WriteLine();

         Console.WriteLine("For a rich person..");
         for (int i = 0; i < amounts.Length; i++)
         {
            Console.WriteLine("amount #{0}: {1}", i + 1, amounts[i]);
         }
         Console.WriteLine("A: {0}", amount_A);
         Console.WriteLine("B: {0}", amount_B);

         Console.Read();
      }
       * */
      
      // Constants
      private const int MAX_CHANCE = 1;
      private const int MIN_CHANCE = 0;

      private struct Method_Data
      {
         public bool bluff;
         public bool first_turn;
         public bool is_call;
         public int bet;
         public int total_money;
         public int available_funds;
         public int personal_investment;
         public double CALL_THRESHHOLD_MIN;
         public double CALL_THRESHHOLD_MAX;
         public Hand_Values hand_value;
      };

      // Constructors
      public Personality(Wealth_Types wealth_type = Wealth_Types.ALRIGHT, Betting_Types betting_type = Betting_Types.CALCULATED, Perception_Types perception_type = Perception_Types.AWARE){
         wealth_type_p = wealth_type;
         betting_type_p = betting_type;
         perception_type_p = perception_type;
         reset_tell_chance_modifier();
         chances = 0;
      }

      public Personality(int starting_funds, Betting_Types betting_type, Perception_Types perception_type)
         : this(Wealth_Types.ALRIGHT, betting_type, perception_type)
      {
         update_wealth_type(starting_funds);
      }

      static Personality()
      {
         rand = new Random();
      }

      // Methods
      public int calculate_bet(bool bluff, bool first_turn, bool is_call, int n_folds, int bet, int total_money, int available_funds, int personal_investment, Hand_Values hand_value, Card_Value high_card_value)
      {
         // calculate chances based off of checking other players cards/current_bets, and how much they just raised

         // based off of hand value and chances of winning,
         //    bet amount is based off of personality and remaining money
         // every chance level based off hand value is .09

         update_wealth_type(available_funds);
         modify_chances(hand_value, high_card_value);

         // fill out method data to pass around
         Method_Data data = new Method_Data();
         data.bluff = bluff;
         data.first_turn = first_turn;
         data.is_call = is_call;
         data.bet = bet;
         data.total_money = total_money;
         data.available_funds = available_funds;
         data.personal_investment = personal_investment;
         data.hand_value = hand_value;
         set_call_threshholds(ref data, n_folds, high_card_value);
         
         // pass off to personality determiner
         bet_method method_choice;
         switch (betting_type)
         {
            case (Betting_Types.STINGY):
               method_choice = stingy_bet;
               break;
            case(Betting_Types.RECKLESS):
               method_choice = reckless_bet;
               break;
            case(Betting_Types.CALCULATED):
               method_choice = calculated_bet;
               break;
            case(Betting_Types.ALL_IN):
               method_choice = all_in_bet;
               break;
            case(Betting_Types.EXTREME):
               method_choice = extreme_bet;
               break;
            case(Betting_Types.RANDOM):
               method_choice = random_bet;
               break;
            default:
               // Betting_Types.END chosen
               return 0;
         }

         double raw_amount = (double)method_choice(data);
         int amount = (int)raw_amount;
         
         // account for terribly inadequate hands returning a negative value
         if (raw_amount <= 0)
         {
            if (first_turn
               && data.bet < 40
               && data.bet < .1 * available_funds)
            {
               return data.bet;
            }
            else
            {
               return 0;
            }
         }

         // adjust for tricking opponents on the first turn
         if (first_turn
            && raw_amount > data.bet){
               if (data.bet == 0)
               {
                  amount = (int)raw_amount;
               }
               else if ( raw_amount > data.bet * (1 + (raw_amount - data.bet)/raw_amount) ){
                  amount = (int)(data.bet * (1 + (raw_amount - data.bet) / raw_amount));
               }
               else
               {
                  amount = (int)raw_amount;
               }

               if (amount < data.bet)
               {
                  amount = data.bet;
               }
            // amount /= 1.34;
         }
         else if (!first_turn
            && hand_value >= Hand_Values.TWO_PAIR)
         {
            amount = (int)(raw_amount * (2 + rand.NextDouble() / 2));//2.16);
         }
         else if (!first_turn)
         {
            amount = (int)(raw_amount * (1 + rand.NextDouble())); // 1.34);
         }

         // round off bet to make it look good if other bets have been rounded off
         if (round_off(bet) == bet)
         {
            int new_amount = round_off(amount);
            if(!(new_amount == 0
               && amount != 0))
            {
               if (new_amount >= data.bet)
               {
                  amount = new_amount;
               }
            }
         }

         return amount;
      }

      private int calculated_bet(Method_Data data)
      {
         // convert data items to normal item
         bool bluff = data.bluff;
         bool first_turn = data.first_turn;
         bool is_call = data.is_call;
         int bet = data.bet;
         int total_money = data.total_money;
         int available_funds = data.available_funds;
         int personal_investment = data.personal_investment;
         double CALL_THRESHHOLD_MIN = data.CALL_THRESHHOLD_MIN;
         double CALL_THRESHHOLD_MAX = data.CALL_THRESHHOLD_MAX;
         Hand_Values hand_value = data.hand_value;

         double amount; // return value in double form
         double investment = (double)personal_investment / total_money * 8; // 0 to 1
         double potential_loss = (double)bet / total_money * 4; // 0 to 1

         if (potential_loss == 0)
         {
            potential_loss = (double)personal_investment / total_money * 4;
         }

         // calculate wealth_factor
         double money_tier = 0;
         if (wealth_type == Wealth_Types.POOR)
         {
            money_tier = (int)Wealth_Values.POOR;
            money_tier *= 1.6;
         }
         else if (wealth_type == Wealth_Types.ALRIGHT)
         {
            money_tier = (int)Wealth_Values.ALRIGHT;
            money_tier /= 5;
         }
         else if (wealth_type == Wealth_Types.WELL_OFF)
         {
            money_tier = (int)Wealth_Values.WELL_OFF;
            money_tier /= 15;
         }
         else if (wealth_type == Wealth_Types.RICH)
         {
            money_tier = (int)Wealth_Values.RICH;
            money_tier /= 50;
         }

         int capper = 1;
         int bet_temp = bet;
         while (bet_temp > 0)
         {
            bet_temp /= 10;
            ++capper;
         }
         capper *= 3;

         double wealth_factor = 1.8 / (((int)wealth_type + 5) / (double)2 / ((int)wealth_type + 1));

         //double max_amount = potential_loss * chances * money_tier * investment * wealth_factor; // $ willing to bet
         double max_amount = .482 * Math.Sqrt(3 * (double)bet) + (0.0184 * (chances - 0.70) / .35) * money_tier + 5; // ERROR TESTING adjust bit in front of chances once chances gets fixed
         //max_amount /= capper;

         int[] hand_value_factors = { 1, 3, 6, 8, 9, 10, 11, 13, 15, 17, 20, 24, 30 };

         /*
         Console.WriteLine("Stats:");
         Console.WriteLine("\tfirst_turn: {0}", first_turn);
         Console.WriteLine("\tis_call: {0}", is_call);
         Console.WriteLine("\tbet: {0}", bet);
         Console.WriteLine("\ttotal_money: {0}", total_money);
         Console.WriteLine("\tavailable_funds: {0}", available_funds);
         Console.WriteLine("\t\tpotential_loss: {0}", potential_loss);
         Console.WriteLine("\t\tchances: {0}", chances);
         Console.WriteLine("\t\tmoney_tier: {0}", money_tier);
         Console.WriteLine("\t\tinvestment: {0}", investment);
         Console.WriteLine("\t\twealth_factor: {0}", wealth_factor);
         Console.WriteLine();
         Console.WriteLine("max_amount: {0}", max_amount);
         */

         //double max_amount = 1 / potential_loss / 10 * available_funds;
         // anywhere from 1/250 to 1/4

         // max raising of x10
         // max raising first turn of x3

         double adjusted_max = max_amount;

         if (bluff
            && hand_value == Hand_Values.HIGH_CARD)
         {
            adjusted_max *= hand_value_factors[rand.Next((int)Hand_Values.ROYAL_FLUSH - (int)Hand_Values.TWO_PAIR) + (int)Hand_Values.TWO_PAIR] / 3 + 1;
         }
         else
         {
            adjusted_max *= hand_value_factors[(int)hand_value] / 3 + 1;
         }

         // willingness to raise instead of call
         if(adjusted_max > bet
            && adjusted_max >= 1.6 * CALL_THRESHHOLD_MAX)
         {
            CALL_THRESHHOLD_MAX *= 2;
         }

         // Console.WriteLine(adjusted_max);

         if (bet < 0
            || adjusted_max < 0)
         {
            amount = 0;
         }
         else if (is_call)
         {
            if (adjusted_max >= CALL_THRESHHOLD_MIN
               && adjusted_max <= CALL_THRESHHOLD_MAX)
            {
               amount = bet;
            }
            else if (adjusted_max > CALL_THRESHHOLD_MAX)
            {
               amount = adjusted_max;
            }
            else
            {
               amount = 0;
            }
         }
         else
         {
            double random_factor = rand.NextDouble() * rand.NextDouble() * (rand.Next(8) + 1) % 1;
            while (random_factor < .85)
            {
               random_factor *= rand.Next(3) + 2;
               if (random_factor > 1)
               {
                  random_factor = 1;
               }
            }

            double recklessness_factor = ((int)wealth_type + 5) / (double)7;

            // Console.WriteLine("random_factor: {0}\nrecklessness: {1}", random_factor, recklessness_factor);

            amount = adjusted_max * random_factor * recklessness_factor;

            // Console.WriteLine("adjusted amount: {0}", amount);

            double investment_factor = 1;

            if (personal_investment > .9 * total_money)
            {
               investment_factor = 2000;
            }
            else if (personal_investment > .7 * total_money)
            {
               investment_factor = 3.5;
            }
            else if (personal_investment > .6 * total_money)
            {
               investment_factor = 2.5;
            }
            else if (personal_investment > .5 * total_money)
            {
               investment_factor = 2;
            }
            else
            {
               investment_factor = 1.1;
               investment_factor += ((double)personal_investment / total_money);
            }

            // Console.WriteLine("investment_factor: {0}", investment_factor);

            amount *= investment_factor;
         }

         if (amount < bet
            && amount != 0)
         {
            Console.WriteLine("final amount: {0}", amount);
         }

         return (int)amount;
      }

      private int stingy_bet(Method_Data data)
      {
         int amount = calculated_bet(data);
         const int BET_CAP = 40;

         if (amount > BET_CAP)
         {
            if (amount >= data.CALL_THRESHHOLD_MIN
               && amount <= data.CALL_THRESHHOLD_MAX)
            {
               amount = data.bet;
            }
            else if(amount > BET_CAP)
            {
               amount = BET_CAP;
            }
         }

         return amount;
      }

      private int reckless_bet(Method_Data data)
      {
         data.first_turn = false;

         double amount = (double)calculated_bet(data);

         if (amount < .4 * data.total_money)
         {
            amount *= 1.29;
         }

         return (int)amount;
      }

      private int all_in_bet(Method_Data data)
      {
         double amount = (double)calculated_bet(data);

         if (amount > .7 * data.available_funds)
         {
            return data.available_funds;
         }

         return (int)amount;
      }

      private int extreme_bet(Method_Data data)
      {
         double amount = (double)calculated_bet(data);

         if (data.hand_value < Hand_Values.ONE_PAIR
            && data.bet > .1 * data.available_funds)
         {
            return 0;
         }
         else if (amount > .7 * data.available_funds)
         {
            return data.available_funds;
         }
         else
         {
            return (int)amount;
         }
      }

      private int tricky_bet(Method_Data data)
      {
         data.bluff = true;
         return calculated_bet(data);
      }

      private int random_bet(Method_Data data){
         double choice = rand.NextDouble();
         bet_method method_choice;

         if (choice > .9)
         {
            method_choice = extreme_bet;
         }
         else if (choice > .8)
         {
            method_choice = all_in_bet;
         }
         else if (choice > .4)
         {
            method_choice = calculated_bet;
         }
         else if (choice > .3)
         {
            method_choice = tricky_bet;
         }
         else if (choice > .2)
         {
            method_choice = stingy_bet;
         }
         else
         {
            method_choice = reckless_bet;
         }

         return method_choice(data);
      }

      private void modify_chances(Hand_Values hand_value, Card_Value card_value)
      {
         // establish base chance off of hand value
         //chances = .09 * ((int)(hand_value) + 1);

         
         double[] chance_arr = new double[10];

         /*
         double total_cards = 52 * 51 * 50 * 49 * 48;
         const int N_SUITES = 4;
         const int N_STRAIGHTS = 10;
         const int N_IN_SUITE = 13;
         const int N_CARDS = 52;
         const int MAX = 5;
         double DENOM = combination(N_CARDS, MAX);
         double any_suites = Math.Pow(combination(N_SUITES, 1), MAX);
         
         double royal_flush_chance = (double)N_SUITES * combination(5, MAX) / DENOM;
         double royal_straight_chance = (double)any_suites * combination(5, MAX)
            - N_SUITES * combination(5, MAX) // exclude royal flush chance
            / DENOM;
         double straight_flush_chance = (double)N_SUITES * N_STRAIGHTS * combination(5, MAX)
            - N_SUITES * combination(5, MAX) // exclude royal flush chance
            / DENOM;
         double four_of_a_kind_chance = (double)N_IN_SUITE * combination(4, 4) * combination((N_IN_SUITE - 1) * N_SUITES, MAX) / DENOM;
         double full_house_chance = (double)N_IN_SUITE * combination(4, 3) * (N_IN_SUITE - 1) * combination(4, 2) / DENOM;
         double flush_chance = (double)N_SUITES * combination(N_IN_SUITE, MAX)
            - (double)N_SUITES * N_STRAIGHTS * combination(5, MAX) // exclude straight flushes and royal flush
            / DENOM;
         double straight_chance = (double)any_suites * N_STRAIGHTS * combination(5, MAX)
            - (double)N_SUITES * N_STRAIGHTS * combination(5, MAX) // exclude royal straight and straight flushes
            / DENOM;
         double three_of_a_kind_chance = (double)N_IN_SUITE * combination(4, 3) * combination((int)(N_SUITES * (N_IN_SUITE - 1)), 2)
            - (double)N_IN_SUITE * combination(4, 4) * combination ((N_IN_SUITE - 1) * N_SUITES, MAX) // exclude four of a kind
            - (double)N_IN_SUITE * combination(4, 3) * (N_IN_SUITE - 1) * combination(4, 2) // exclude full house
            / DENOM;
         double two_pair_chance = (double)N_IN_SUITE * combination(4, 2) * (N_IN_SUITE - 1) * combination(4, 2)
            - (double)N_IN_SUITE * combination(4, 3) * (N_IN_SUITE - 1) * combination(4, 2) // exclude full house chances
            / DENOM;
         double one_pair_chance = (double)N_IN_SUITE * combination(4, 2) / DENOM
            - full_house_chance - three_of_a_kind_chance - two_pair_chance - full_house_chance - four_of_a_kind_chance;
         double remaining_chance = 0;

         // fill chance_arr
         chance_arr[0] = one_pair_chance;
         chance_arr[1] = two_pair_chance;
         chance_arr[2] = three_of_a_kind_chance;
         chance_arr[3] = straight_chance;
         chance_arr[4] = flush_chance;
         chance_arr[5] = full_house_chance;
         chance_arr[6] = four_of_a_kind_chance;
         chance_arr[7] = straight_flush_chance;
         chance_arr[8] = royal_straight_chance;
         chance_arr[9] = royal_flush_chance;

         // calculate remaining_chance
         for(int i = 0; i < chance_arr.Length; i++){
            remaining_chance -= chance_arr[i];
            if((int)hand_value == i){
               break;
            }
         }

         // adjust remaining chances based off of card value
         if(hand_value == Hand_Values.HIGH_CARD){
            if((int)card_value == 0){
               ++card_value;
            }
            remaining_chance /= (int)card_value;
         }


         double royal_flush_chance = (double)N_SUITES * (5 * 4 * 3 * 2 * 1) / total_cards;
         double royal_straight_chance = (double)(Math.Pow(N_SUITES, (double)5) * (5 * 4 * 3 * 2 * 1) / total_cards;
         double straight_flush_chance = ((double)N_SUITES * (N_STRAIGHTS) * (5 * 4 * 3 * 2 * 1) - (N_SUITES * N_STRAIGHTS))/ total_cards; // account for royal flush chance at end
         double four_of_a_kind_chance = (double)N_IN_SUITE * (4 * 3 * 2 * 1 * (N_IN_SUITE - 1) * N_SUITES) / total_cards;
         double full_house_chance = (double)N_IN_SUITE * (52 * 3 * 2) * (double)(N_IN_SUITE - 1) * (49 * 3) / total_cards;
         double flush_chance = ((double)N_SUITES * (N_IN_SUITE * (N_IN_SUITE - 1) * (N_IN_SUITE - 2) * (N_IN_SUITE - 3) * (N_IN_SUITE - 4)) - (N_SUITES * N_STRAIGHTS)) / total_cards; // account for the straight flushes at the end
         double straight_chance = ((double)N_STRAIGHTS * Math.Pow(N_SUITES, (double)5) - N_SUITES * N_STRAIGHTS) * (5 * 4 * 3 * 2 * 1) / total_cards; // account for straight flushes (n_suites * n_straights)
         */

         double one_pair_chance = (double)3 / 51 * 48 / 50 * 47 / 49 * 46 / 48;
         chance_arr[0] = one_pair_chance;
         double two_pair_chance = (double)6 / 50 * 5 / 49 * 44 / 48;
         chance_arr[1] = two_pair_chance;
         double three_of_a_kind_chance = (double)3 / 51 * 2 / 50 * 48 / 49 * 47 / 48;
         chance_arr[2] = three_of_a_kind_chance;
         double flush_chance = (double)12 / 51 * 11 / 50 * 10 / 49 * 9 / 48;
         chance_arr[3] = flush_chance;
         double full_house_chance = (double)6 / 50 * 5 / 49 * 4 / 48;
         chance_arr[4] = full_house_chance;
         double royal_straight_chance = (double)20 / 52 * 16 / 51 * 12 / 50 * 8 / 49 * 4 / 48;
         double straight_chance = royal_straight_chance * 5;
         chance_arr[5] = straight_chance;
         chance_arr[6] = royal_straight_chance;
         double four_of_a_kind_chance = (double)3 / 51 * 2 / 50 * 1 / 49;
         chance_arr[7] = four_of_a_kind_chance;
         double straight_flush_chance = straight_chance / 13;
         chance_arr[8] = straight_flush_chance;
         double royal_flush_chance = (double)20 / 52 * 4 / 51 * 3 / 50 * 2 / 49 * 1 / 48;
         chance_arr[9] = royal_flush_chance;

         double chance_of_better_hand = 0;

         if(hand_value == Hand_Values.HIGH_CARD){
            for(int i = 0; i < chance_arr.Length; i++){
               chance_of_better_hand += (1 - (1 - chance_arr[i]) * (1 - chance_arr[i]));
            }
         }
         else{
            for(int i = chance_arr.Length - 1; i >= (int)hand_value; i--){
               chance_of_better_hand += (1 - (1 - chance_arr[i]) * (1 - chance_arr[i]));
            }
         }

         // adjust chances based off of how good the deciding card value of your hand is
         double card_value_modifier = 0;
         switch(hand_value){
            case(Hand_Values.HIGH_CARD):
               card_value_modifier = 0;
               break;
            case(Hand_Values.ONE_PAIR):
               card_value_modifier = (one_pair_chance - one_pair_chance / (int)Card_Value.KING * (int)card_value);
               break;
            case(Hand_Values.TWO_PAIR):
               card_value_modifier = (two_pair_chance - two_pair_chance / (int)Card_Value.KING * (int)card_value);
               break;
            case(Hand_Values.THREE_OF_A_KIND):
               card_value_modifier = (three_of_a_kind_chance - three_of_a_kind_chance / (int)Card_Value.KING * (int)card_value);
               break;
            case(Hand_Values.FLUSH):
               card_value_modifier = flush_chance;
               break;
            case(Hand_Values.FULL_HOUSE):
               card_value_modifier = (full_house_chance - full_house_chance/ (int)Card_Value.KING * (int)card_value);
               break;
            case(Hand_Values.STRAIGHT):
               card_value_modifier = (straight_chance - straight_chance / (int)Card_Value.KING * (int)card_value);
               break;
            case(Hand_Values.ROYAL_STRAIGHT):
               card_value_modifier = royal_straight_chance;
               break;
            case(Hand_Values.FOUR_OF_A_KIND):
               card_value_modifier = (four_of_a_kind_chance - four_of_a_kind_chance / (int)Card_Value.KING * (int)card_value);
               break;
            case(Hand_Values.ROYAL_FLUSH):
               card_value_modifier = royal_flush_chance;
               break;
            default:
               break;
         }

         // calculate and set chances
         if (hand_value == Hand_Values.HIGH_CARD)
         {
            chances = 1 - chance_of_better_hand;
            chances /= (int)Card_Value.KING;
            chances *= (int)card_value;
            return;
         }
         else{
            chances = 1 - chance_of_better_hand - card_value_modifier;
         }

         // modify estimated chance of winning by everyone's tells
         chances *= tell_chance_modifier;
         
         // adjust chances to be less than 1 if overdid it
         if (chances > MAX_CHANCE)
         {
            chances = MAX_CHANCE;
         }
      }

      public double update_tell_chance_modifier(int[] tiers, int my_tier)
      {
         tell_chance_modifier = 1;
         const int MAX_TIER = 3;
         const int MIN_TIER = 0;
         double[] tell_chance_modifiers = new double[MAX_TIER+1];
         // guard conditions
         if(my_tier > MAX_TIER
            || my_tier < MIN_TIER)
         {
            return tell_chance_modifier;
         }

         int index = 0;
         // fill tell_chance_modifiers
         for (int i = MIN_TIER; i < MAX_TIER; i++)
         {
            int tier_difference = my_tier - i;
            if(tier_difference > MAX_TIER - MIN_TIER - 1){
               // there's no way you'll lose to this guy
               tell_chance_modifiers[index++] = 1.6;
            } // most obvious tells
            else if(tier_difference < MIN_TIER - MAX_TIER + 1){
               // there's no way you'll win against this guy
               tell_chance_modifiers[index++] = .4;
            } // most obvious tells
            else{
               double deviation = tier_difference * .1;
               if (deviation < 0)
               {
                  deviation *= -1 * tier_difference;
               }
               else
               {
                  deviation *= tier_difference;
               }

               switch (perception_type)
               {
                  case(Perception_Types.OBLIVIOUS):
                     deviation = 0;
                     break;
                  case(Perception_Types.AWARE):
                     if(tier_difference < 2
                        || tier_difference > -2)
                     {
                        deviation = 0;
                     }
                     break;
               }

               tell_chance_modifiers[index++] = 1 + deviation;
            }
         }

         // modify the tell_chance appropriately
         for (int i = 0; i < tiers.Length; i++)
         {
            if (tiers[i] < 0
               || tiers[i] > tell_chance_modifiers.Length)
            {
               break;
            }
            else
            {
               tell_chance_modifier *= tell_chance_modifiers[tiers[i]];
            }
         }

         return tell_chance_modifier;
      }

      static public double combination(int k, int n)
      {
         int limit = k - n;
         if(limit < 1){
            limit = 1;
         }

         double sum = 1;

         while(k > limit){
            sum *= k--;
         }

         return sum;
      }

      private void update_wealth_type(int funds){
         Wealth_Values value = (Wealth_Values)funds;
         if (value >= Wealth_Values.RICH)
         {
            wealth_type_p = Wealth_Types.RICH;
         }
         else if (value >= Wealth_Values.WELL_OFF)
         {
            wealth_type_p = Wealth_Types.WELL_OFF;
         }
         else if (value >= Wealth_Values.ALRIGHT)
         {
            wealth_type_p = Wealth_Types.ALRIGHT;
         }
         else
         {
            wealth_type_p = Wealth_Types.POOR;
         }
      }

      private int round_off(int amount, int nearest_n = 5)
      {
         return (int)round_off((double)amount, nearest_n);
      }

      private double round_off(double amount, int nearest_n = 5)
      {
         int result = (int)amount / nearest_n * nearest_n;
         if ((int)amount % nearest_n >= (double)nearest_n / 2)
         {
            result += nearest_n;
         }

         return (double)result;
      }

      private void set_call_threshholds(ref Method_Data data, int n_folds, Card_Value high_card_value)
      {
         int bet = data.bet;
         Hand_Values hand_value = data.hand_value;

         if (bet == 0)
         {
            data.CALL_THRESHHOLD_MIN = Money.DEFAULT_ANTE - 4;
            data.CALL_THRESHHOLD_MAX = Money.DEFAULT_ANTE + 4;
         }
         else if (wealth_type == Wealth_Types.ALRIGHT)
         {
            data.CALL_THRESHHOLD_MIN = (double)bet * .85;
            data.CALL_THRESHHOLD_MAX = (double)bet * 1.15;
         }
         else if (wealth_type == Wealth_Types.WELL_OFF)
         {
            data.CALL_THRESHHOLD_MIN = (double)bet * .82;
            data.CALL_THRESHHOLD_MAX = (double)bet * 1.18;
         }
         else if (wealth_type == Wealth_Types.RICH)
         {
            data.CALL_THRESHHOLD_MIN = (double)bet * .73;
            data.CALL_THRESHHOLD_MAX = (double)bet * 1.27;
         }
         else
         {
            data.CALL_THRESHHOLD_MIN = (double)bet * .9;
            data.CALL_THRESHHOLD_MAX = (double)bet * 1.1;
         }

         // determines when the AI are going to call a player on bullcrap bluffs
         if (n_folds > 6 - (int)perception_type)
         {
            if (hand_value > Hand_Values.HIGH_CARD)
            {
               data.CALL_THRESHHOLD_MIN = (double)5;
            }
            else if (high_card_value > Card_Value.JACK)
            {
               data.CALL_THRESHHOLD_MIN = (double)25;
            }
         }
      }

      private double reset_tell_chance_modifier()
      {
         tell_chance_modifier_p = 1;
         return tell_chance_modifier;
      }

      static public Wealth_Values convert_wealth_type_to_value(Wealth_Types wealth_type)
      {
         if (wealth_type == Wealth_Types.POOR)
         {
            return Wealth_Values.POOR;
         }
         else if (wealth_type == Wealth_Types.ALRIGHT)
         {
            return Wealth_Values.ALRIGHT;
         }
         else if (wealth_type == Wealth_Types.WELL_OFF)
         {
            return Wealth_Values.WELL_OFF;
         }
         else if (wealth_type == Wealth_Types.RICH)
         {
            return Wealth_Values.RICH;
         }
         else
         {
            return Wealth_Values.END;
         }
      }

      // Fields
      private Wealth_Types wealth_type_p;
      private Betting_Types betting_type_p;
      private Perception_Types perception_type_p;
      private double chances_p;
      private double tell_chance_modifier_p;
      private delegate int bet_method(Method_Data data);
      static private Random rand;
      
      // Properties
      public Wealth_Types wealth_type
      {
         get { return wealth_type_p; }
         set { wealth_type_p = value; }
      }
      public Betting_Types betting_type
      {
         get { return betting_type_p; }
         set { betting_type_p = value; }
      }
      public Perception_Types perception_type
      {
         get { return perception_type_p; }
      }
      public double chances
      {
         get { return chances_p; }
         private set {
            if(value <= MAX_CHANCE
               && value >= MIN_CHANCE)
            {
               chances_p = value;
            }
         }
      }
      public double tell_chance_modifier
      {
         get { return tell_chance_modifier_p; }
         private set
         {
            if (value > 0)
            {
               tell_chance_modifier_p = value;
            }
         }
      }
   }
}
