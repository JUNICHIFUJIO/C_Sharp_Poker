using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Each card has a suite, a number, and a unique ID?, and a value attached to it
// Cards may also have a mark or wear or tear or something to tip off cheaters

// public
//    characteristics (string showing marks)
//    get_suite
//    get_number
//    get_value
//    get_ID / set_ID (setting is for shuffling, when the index in the array is the ID of the card)
//    is_discarded
//    display function

// private
//    suite
//    number
//    ID
//    value
//    mark
//    discarded or not

namespace Poker
{
   public enum Card_Suite {SPADES, CLUBS, DIAMONDS, HEARTS, END};

   public enum Card_Value {JOKER, ACE, TWO, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, TEN, JACK, QUEEN, KING, END, ACE_HIGH};

   public struct Card_Info{
      public Card_Suite suite;
      public Card_Value value;
      public string mark;
   };

   class Card
   {
      // Test mainframe
      /*
      public static void Main(string[] args)
      {
         uint card_id = 0;
         Card card = new Card("8; diamonds; what is this garbage?", card_id++);
         Card card2 = new Card("13; club; what IS this??", card_id++);
         Card card3 = new Card("what; WHAT; huh?", card_id++);

         card.display();
         card2.display();
         card3.display();

         Console.Read();
      }
       * */

      public Card(Card other)
      {
         this.suite_p = other.suite_p;
         this.value_p = other.value_p;
         this.ID_p = other.ID_p;
         this.mark_p = other.mark_p;
         this.discarded_p = other.discarded_p;
         this.unavailable_p = other.unavailable_p;
      }

      public Card(Card_Info info, uint ID){
         suite_p = info.suite;
         value_p = info.value;
         ID_p = ID;
         mark_p = info.mark;
         discarded_p = false;
      }

      public Card(string card_info, uint ID){
         Card_Info info = interpret(card_info);
         suite_p = info.suite;
         value_p = info.value;
         ID_p = ID;
         mark_p = info.mark;
         discarded_p = false;
      }

      public Card() { 
         suite_p = Card_Suite.END;
         value_p = Card_Value.END;
         ID_p = 0;
         mark_p = "";
         discarded_p = false;
      }

      public void display()
      {
         display(suite, value);
      }

      public void discard()
      {
         discarded = true;
      }

      public bool Equals(Card other)
      {
         return other.value == value
            && other.suite == suite
            && other.mark.Equals(mark)
            && other.unavailable == unavailable
            && other.discarded == discarded
            && other.ID == ID;
      }

      /// <summary>
      /// Converts out the value and suite of the card to a string.
      /// </summary>
      /// <returns></returns>
      override public string ToString()
      {
         string card_str = "";

         card_str += value.ToString();
         if (value != Card_Value.JOKER)
         {
            card_str += " of ";
            card_str += suite.ToString();
         }

         return StringManip.StringManip.proper_capitalization(card_str, true);
      }

      // Static Methods
      static public Card_Info interpret(string info){
         Card_Info card_info;
         TextParser parser = new TextParser();
         
         parser.get_card_info(info, out card_info);

         return card_info;
      }

      static public void display(Card_Info info)
      {
         display(info.suite, info.value);
      }

      static public void get_mark(Card_Info info)
      {
         if (info.mark.Equals(""))
         {
            Console.WriteLine("It is a plain card.");
         }
         else
         {
            Console.WriteLine(info.mark);
         }
      }

      static private void display(Card_Suite suite, Card_Value value)
      {
         string val_str = value.ToString().ToLower();
         val_str = val_str.ElementAt(0).ToString().ToUpper() + val_str.Substring(1);
         string sui_str = suite.ToString().ToLower();
         sui_str = sui_str.ElementAt(0).ToString().ToUpper() + sui_str.Substring(1);

         // guard conditions
         if (value == Card_Value.JOKER)
         {
            Console.WriteLine("{0}", val_str);
         }
         else if (suite == Card_Suite.END || value == Card_Value.END)
         {
            Console.WriteLine("Invalid card passed");
         }
         else
         {
            Console.WriteLine("{0} of {1}", val_str, sui_str);
         }
      }

      // Fields
      private Card_Suite suite_p;
      private Card_Value value_p;
      private uint ID_p;
      private string mark_p;
      private bool discarded_p;
      private bool unavailable_p;

      // Properties
      public Card_Suite suite
      {
         get { return suite_p; }
      }
      public Card_Value value
      {
         get { return value_p; }
      }
      public uint ID
      {
         get { return ID_p; }
         set { ID_p = value; }
      }
      public string mark
      {
         get
         {
            if (mark_p.Equals(""))
            {
               return "It is a plain card.";
            }
            else
            {
               return mark_p;
            }
         }
         set { mark_p = value; }
      }
      public bool discarded
      {
         get { return discarded_p; }
         set
         {
            discarded_p = value;
            if (value)
            {
               unavailable_p = true;
            }
         }
      }
      public bool unavailable
      {
         get { return unavailable_p; }
         set { unavailable_p = value; }
      }
   }
}
