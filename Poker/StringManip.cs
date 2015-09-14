using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// To handle manipulation of strings, for example, converting a string into an integer or a double or a float

namespace StringManip
{
   class StringManip
   {
      public StringManip() { }

      static public int atoi(string str){
         if (!is_valid_number(str))
         {
            return 0;
         }

         if (str.Length == 0)
         {
            return 0;
         }

         bool negative = false;
         int result = 0;
         int i = 0;

         if (str[0] == '-')
         {
            negative = true;
            ++i;
         }

         for (; i < str.Length; i++)
         {
            if (str[i] == ',')
            {
               continue;
            }
            if (str[i] == '.')
            {
               break;
            }
            result *= 10;
            result += str[i] - '0';
         }

         if (i < str.Length - 1 && str[i] == '.')
         {
            if((str[i+1] - '0') >= 5 ){
               result += 1;
            }
         }

         if (negative)
         {
            result *= -1;
         }

         return result;
      }

      static public double To_Double(string str){
         // if (!is_valid_number(str))
         //{
         //   return 0;
         //}

         double result = 0;

         int integer_part = atoi(str);

         int i = 0;
         while (i < str.Length && str[i] != '.')
         {
            ++i;
         }

         // check if the number is a decimal
         if (str[i] != '.')
         {
            // kick out here if end of string was reached
            return (double)integer_part;
         }
         else
         {
            // compute the decimal part of the double, and attach it to the integer part.
            // if the decimal part is greater than .499999..., subtract one from the integer part
            string substr = str.Substring(i);
            int decimal_places = substr.Length - 1;

            // adjust integer part if necessary
            if (substr[1] >= '5')
            {
               integer_part--;
            }

            // get decimal part
            double decimal_part = 0;
            for (int j = 1; j < substr.Length; j++)
            {
               decimal_part *= 10;
               decimal_part += substr[j] - '0';
            }

            // calculate result
            for(int shifts = 0; shifts < decimal_places; shifts++){
               decimal_part /= 10;
            }
            result = integer_part + decimal_part;

            return result;
         }
      }

      static public bool is_valid_number(string str)
      {
         if (str.Length == 0)
         {
            return false;
         }
         bool decimal_reached = false;
         int comma_index = 0;
         bool is_valid = true;
         int index = 0;

         while (index < str.Length)
         {
            if (str[index] == '.')
            {
               if (decimal_reached)
               {
                  return false;
               }
               else
               {
                  decimal_reached = true;
               }
               
            }
            if (str[index] == '-' && index != 0)
            {
               return false;
            }
            if (str[index] == ',')
            {
               if (comma_index == 0)
               {
                  comma_index = index;
               }
               else if (index - comma_index != 3)
               {
                  return false;
               }
            }
            if(str[index] < '0'
               || str[index] > '9')
            {
               return false;
            }
            index++;
         }

         return is_valid;
      }

      static public string proper_capitalization(string str, bool all_words_first_char_capitalized = false)
      {
         // guard conditions
         if (str.Equals(""))
         {
            return "";
         }

         string result = "";
         char[] separators = new char[] { ' ', '_' };
         string[] str_elements = str.Split(separators);

         string[] fillers = new String[] { "the", "of", "and", "is", "an", "a"};

         // skip all empty starting strings passed in
         int first_word_index = 0;
         while (str_elements[first_word_index].Equals(""))
         {
            ++first_word_index;
         }

         // special treatment of the first word
         if (!str_elements[first_word_index].Equals(""))
         {
            string first_word = str_elements[first_word_index].ToLower();
            if (first_word.Length > 1)
            {
               first_word = first_word.ElementAt(0).ToString().ToUpper() + first_word.Substring(1);
            }
            result += first_word;
         }
         

         // analyze the rest of the elements of the passed in string
         for (int i = first_word_index + 1; i < str_elements.Length; i++)
         {
            if (!str_elements[i].Equals(""))
            {
               string str_i = str_elements[i].ToLower();

               // determine if the word being analyzed is a filler word
               bool is_filler = false;
               foreach (string filler in fillers)
               {
                  if (str_i.Equals(filler))
                  {
                     is_filler = true;
                     break;
                  }
               }

               // capitalize the first letter if not a filler word
               if (all_words_first_char_capitalized
                  && !is_filler
                  && str_i.Length > 1)
               {
                  str_i = str_i.ElementAt(0).ToString().ToUpper() + str_i.Substring(1);
               }

               // add the manipulated string element to the string
               result += " ";
               result += str_i;
            }
         }

         return result;
      }
   }
}
