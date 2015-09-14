using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// each player has money
// each table has money
// big blind
// little blind
// ante
// pool / total money held
// current bet (can u call/raise?)
// remaining total

namespace Poker
{
   class Money
   {
      public const int DEFAULT_ORIGINAL_AMOUNT = 5000;
      public const int DEFAULT_ANTE = 20;
      public const int DEFAULT_BUY_IN = 100;
      public const int DEFAULT_SMALL_BLIND = 30;
      public const int DEFAULT_BIG_BLIND = 2*DEFAULT_SMALL_BLIND;

      // Test method
      /*
      public static void Main(string[] args)
      {
         Money player_money = new Money();
         Money table_money = new Money(0, DEFAULT_BUY_IN, DEFAULT_BIG_BLIND, DEFAULT_SMALL_BLIND, DEFAULT_ANTE);

         Console.WriteLine("Depositing 50 thousand.");
         player_money.deposit(50000);

         Console.Write("Player 1...");
         player_money.display();

         Console.WriteLine("Player 1 currently betting {0}", player_money.current_bet);
         Console.WriteLine("...wait, no, he raised his bet to 90000...");
         player_money.current_bet = 90000;

         Console.WriteLine("Extracting 90 thousand.");
         Console.WriteLine("${0} extracted...", player_money.withdraw(90000));

         Console.WriteLine("...but player won 40000.");
         player_money.win(40000);
         player_money.display();

         Console.Read();
      }
       * */


      // Constructors
      public Money(int original_amount, int buy_in, int big_blind, int small_blind, int ante)
      {
         this.original_amount = original_amount;
         this.buy_in = buy_in;
         this.big_blind = big_blind;
         this.small_blind = small_blind;
         this.ante = ante;
         current_amount = original_amount;
         current_bet = 0;
      }

      public Money()
         : this(DEFAULT_ORIGINAL_AMOUNT, DEFAULT_BUY_IN, DEFAULT_BIG_BLIND, DEFAULT_SMALL_BLIND, DEFAULT_ANTE) { }

      static Money()
      {
         rand = new Random();
      }

      // Methods
      // if returns false, no money left (all in)
      public int bet(int amount)
      {
         if (amount >= current_amount - current_bet)
         {
            current_bet = current_amount;
         }
         else
         {
            current_bet += amount;
         }
         return amount;
      }

      // deposit money
      public bool deposit(int amount)
      {
         if (amount > 0)
         {
            current_amount += amount;
            return true;
         }
         else
         {
            return false;
         }
      }

      public bool win(int amount)
      {
         if (deposit(amount))
         {
            current_bet = ante;
            return true;
         }
         else
         {
            return false;
         }
      }

      public int withdraw(int amount)
      {
         int money_withdrawn = 0;
         if (current_amount < amount)
         {
            money_withdrawn = current_amount;
            current_amount = 0;
         }
         else
         {
            current_amount -= amount;
            money_withdrawn = amount;
         }

         return money_withdrawn;
      }

      public void double_up_blinds()
      {
         int bb = big_blind;
         int sb = small_blind;
         small_blind *= 2;
         big_blind = bb * 2;
      }

      public void display()
      {
         Console.WriteLine("Currently holds: {0}.", current_amount);
      }

      public void refresh()
      {
         current_bet_p = 0;
      }

      // Fields
      private int original_amount_p;
      private int buy_in_p;
      private int big_blind_p;
      private int small_blind_p;
      private int ante_p;
      private int current_amount_p;
      private int current_bet_p;
      static private Random rand;

      // Properties
      public int original_amount
      {
         get { return original_amount_p; }
         private set { original_amount_p = value; }
      }
      public int buy_in
      {
         get { return buy_in_p; }
         set
         {
            if (value >= ante)
            {
               buy_in_p = value;
            }
            else
            {
               buy_in_p = ante;
            }
         }
      }
      public int big_blind {
         get { return big_blind_p; }
         set
         {
            if (value > small_blind)
            {
               big_blind_p = value;
            }
            else
            {
               big_blind_p = 2 * small_blind;
            }
         }
      }
      public int small_blind {
         get { return small_blind_p; }
         set
         {
            if (value > ante)
            {
               small_blind_p = value;

            }
            else
            {
               small_blind_p = ante;
            }
            big_blind = 2 * small_blind_p;
         }
      }
      public int ante {
         get { return ante_p; }
         set
         {
            if (value > 0)
            {
               ante_p = value;
            }
            else
            {
               ante_p = 10;
            }
         }
      }
      public int current_amount {
         get { return current_amount_p; }
         private set
         {
            current_amount_p = value;
         }
      }
      public int current_bet {
         get { return current_bet_p; }
         private set
         {
            if (value == 0)
            {
               current_bet_p = 0;
            }
            else if (value > ante)
            {
               current_bet_p = value;
            }
            else
            {
               current_bet_p = ante;
            }
         }
      }
      public int net_profit
      {
         get { return current_amount - original_amount; }
      }
   }
}
