using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
   class TextParser
   {
      // assumes that the string will be structured as follows
      // 10;Spades;mark
      // or
      // four;diamonds;mark

      public TextParser() { }

      public void get_card_info(string info, out Card_Info card_info){

         string[] parsed_elements = info.Split(';');
         string card_value_str = "";
         string card_suite_str = "";
         string mark = "";
         if (parsed_elements.Length > 0)
         {
            card_value_str = parsed_elements[0];
            if (parsed_elements.Length > 1)
            {
               card_suite_str = parsed_elements[1];
               if (parsed_elements.Length > 2)
               {
                  mark = parsed_elements[2].TrimStart();
               }
            }
         }

         card_info.suite = text_to_suite(card_suite_str);
         card_info.value = text_to_value(card_value_str);
         card_info.mark = mark;
      }

      public Card_Value text_to_value(string val_str)
      {
         if (val_str.Equals(""))
         {
            return Card_Value.END;
         }

         val_str = val_str.ToLower().Trim();
         
         // handle expected values
         if(val_str.Equals("joker")){
            return Card_Value.JOKER;
         }else if(val_str.Equals("ace")){
            return Card_Value.ACE;
         }else if(val_str.Equals("two")){
            return Card_Value.TWO;
         }else if(val_str.Equals("three")){
            return Card_Value.THREE;
         }else if(val_str.Equals("four")){
            return Card_Value.FOUR;
         }else if(val_str.Equals("five")){
            return Card_Value.FIVE;
         }else if(val_str.Equals("six")){
            return Card_Value.SIX;
         }else if(val_str.Equals("seven")){
            return Card_Value.SEVEN;
         }else if(val_str.Equals("eight")){
            return Card_Value.EIGHT;
         }else if(val_str.Equals("nine")){
            return Card_Value.NINE;
         }else if(val_str.Equals("ten")){
            return Card_Value.TEN;
         }else if(val_str.Equals("jack")){
            return Card_Value.JACK;
         }else if(val_str.Equals("queen")){
            return Card_Value.QUEEN;
         }else if(val_str.Equals("king")){
            return Card_Value.KING;
         }

         // unconventional spellings
         else if(val_str.Equals("1")
            || val_str.Equals("one")
            || val_str.Equals("won"))
         {
            return Card_Value.ACE;
         } else if(val_str.Equals("2")
            || val_str.Equals("too"))
         {
            return Card_Value.TWO;
         }
         else if (val_str.Equals("3"))
         {
            return Card_Value.THREE;
         } else if(val_str.Equals("4")
            || val_str.Equals("fore")
            || val_str.Equals("for"))
         {
            return Card_Value.FOUR;
         } else if(val_str.Equals("5")
            || val_str.Equals("faiv"))
         {
            return Card_Value.FIVE;
         } else if(val_str.Equals("6")
            || val_str.Equals("sex"))
         {
            return Card_Value.SIX;
         } else if(val_str.Equals("7")
            || val_str.Equals("sevan"))
         {
            return Card_Value.SEVEN;
         } else if(val_str.Equals("8")
            || val_str.Equals("ate"))
         {
            return Card_Value.EIGHT;
         }
         else if (val_str.Equals("9")
          || val_str.Equals("nein"))
         {
            return Card_Value.NINE;
         } else if(val_str.Equals("10")
            || val_str.Equals("tehn"))
         {
            return Card_Value.TEN;
         } else if(val_str.Equals("11")
            || val_str.Equals("eleven")
            || val_str.Equals("elevan"))
         {
            return Card_Value.JACK;
         } else if(val_str.Equals("12")
            || val_str.Equals("twelve"))
         {
            return Card_Value.QUEEN;
         } else if(val_str.Equals("13")
            || val_str.Equals("thirteen"))
         {
            return Card_Value.KING;
         }
         else
         {
            return Card_Value.END;
         }
      }

      public Card_Suite text_to_suite(string sui_str)
      {
         if (sui_str.Equals(""))
         {
            return Card_Suite.END;
         }

         sui_str = sui_str.ToLower().Trim();

         if (sui_str.Equals("spades"))
         {
            return Card_Suite.SPADES;
         }
         else if (sui_str.Equals("clubs"))
         {
            return Card_Suite.CLUBS;
         }
         else if (sui_str.Equals("diamonds"))
         {
            return Card_Suite.DIAMONDS;
         }
         else if (sui_str.Equals("hearts"))
         {
            return Card_Suite.HEARTS;
         }
         
         // unconventional spellings
         else if (sui_str.Equals("spade"))
         {
            return Card_Suite.SPADES;
         }
         else if (sui_str.Equals("club"))
         {
            return Card_Suite.CLUBS;
         }
         else if (sui_str.Equals("diamond"))
         {
            return Card_Suite.DIAMONDS;
         }
         else if (sui_str.Equals("heart"))
         {
            return Card_Suite.HEARTS;
         }
         else
         {
            return Card_Suite.END;
         }
      }

      public Card_Info[] extract_cards(string raw_input)
      {
         Card_Info[] info_arr = new Card_Info[10];
         int arr_index = 0;
         string[] raw_choices = raw_input.Split(new char[] { ',', ';' });
         foreach (string raw_choice in raw_choices)
         {
            string[] split_raw = raw_choice.Split(new char[] { ' ' });
            for (int i = 0; i < split_raw.Length; i++)
            {
               Card_Value val = text_to_value(split_raw[i]);
               if (val != Card_Value.END)
               {
                  // found the card value of the split
                  Card_Info card_info = new Card_Info();
                  card_info.value = val;
                  card_info.suite = text_to_suite(split_raw[i + 2]);

                  info_arr[arr_index++] = card_info;
               }
            }
         }

         return info_arr;
      }

   }
}
