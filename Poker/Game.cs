using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StringManip;

// tracks players (who's in, out, folded, playing, waiting, etc.)
// tracks big blind/little blind/ante who owes what
// tracks overall pool
// uses the player order to skip people who are out/all in/folded
// tracks discard and deck piles
// manages deck and shuffling

namespace Poker
{
   class Game
   {
      // Testing mainframe
      static void Main(String[] args)
      {
         Game game = new Game();
         game.start_game();

         Console.ReadKey(true);
      }

      // Constructors
      public Game(int n_players, Deck deck, int starting_funds = Money.DEFAULT_ORIGINAL_AMOUNT, int buy_in = Money.DEFAULT_BUY_IN, int max_size = Hand.DEFAULT_MAX_SIZE, int big_blind = Money.DEFAULT_BIG_BLIND, int small_blind = Money.DEFAULT_SMALL_BLIND, int ante = Money.DEFAULT_ANTE)
      {
         if (n_players < 4)
         {
            n_players = 4;
         }
         else if (n_players > 6)
         {
            n_players = 6;
         }
         
         Random rand = new Random();
         this.deck = deck;
         this.deck.shuffle();

         players = new Player[n_players];
         player_queue = new Queue<Player>(n_players);
         string player_1_name = query_player_1_name();
         bool is_male_player = query_player_1_gender();

         // initialize the players
         // initialize the bank
         // empty out the queue
         // assign and shuffle the deck

         /*
         int starting_funds = Money.DEFAULT_ORIGINAL_AMOUNT;
         int buy_in = Money.DEFAULT_BUY_IN;
         int big_blind = Money.DEFAULT_BIG_BLIND;
         int small_blind = Money.DEFAULT_SMALL_BLIND;
         int ante = Money.DEFAULT_ANTE;
         int max_size = Hand.DEFAULT_MAX_SIZE;
         */

         bool[] AI_genders;
         string[] AI_names = choose_AI_names(player_1_name, out AI_genders);


         players[0] = new Player(starting_funds, buy_in, big_blind, small_blind, ante, deck.draw(5), this.deck, max_size, player_1_name, is_male_player);
         player_queue.Enqueue(players[0]);

         for (int i = 1; i < n_players; i++)
         {
            Wealth_Types wealth_type = (Wealth_Types)rand.Next((int)Wealth_Types.END);
            Betting_Types betting_type = (Betting_Types)rand.Next((int)Betting_Types.END);
            Perception_Types perception_type = (Perception_Types)rand.Next((int)Perception_Types.END);
            Personality personality = new Personality(wealth_type, betting_type, perception_type);
            
            players[i] = new AIPlayer(personality, this.deck, AI_names[i - 1], AI_genders[i - 1]);

            player_queue.Enqueue(players[i]);
         }

         // assign the bank's variables
         bank = new Money(0, buy_in, big_blind, small_blind, ante);
      }

      public Game() : this(4, new Deck()) { }

         private string query_player_1_name()
         {
            Console.WriteLine("What would you like to be called in the game?");
            string player_1_name = Console.ReadLine();

            while (player_1_name.Equals(""))
            {
               Console.WriteLine();
               Console.Write("Please enter your name: ");
               player_1_name = Console.ReadLine();
            }

            Console.WriteLine();

            return player_1_name;
         }

         private bool query_player_1_gender()
         {
            bool is_male = false;
            Console.WriteLine("Are you a man? Or a woman?");
            string player_1_gender = Console.ReadLine().ToLower();

            while(true)
            {
               if(player_1_gender.Equals("man")
                  || player_1_gender.Equals("boy")
                  || player_1_gender.Equals("guy")
                  || player_1_gender.Equals("male")
                  || player_1_gender.Equals("gentleman")){
                  is_male = true;
                  break;
               }
               else if(player_1_gender.Equals("woman")
                  || player_1_gender.Equals("girl")
                  || player_1_gender.Equals("gal")
                  || player_1_gender.Equals("female")
                  || player_1_gender.Equals("lady")){
                  is_male = false;
                  break;
               }
               else{
                  player_1_gender = "";
               }

               Console.WriteLine();
               Console.Write("Please enter your gender for the purpose of correct identification: ");
               player_1_gender = Console.ReadLine().ToLower();
            }

            Console.WriteLine();

            return is_male;
         }

         private string[] choose_AI_names(string[] PC_names, out bool[] is_male_indicators)
         {
            string[] AI_names = new string[players.Length];
            is_male_indicators = new bool[players.Length];

            int AI_index = 0;

            while (AI_index < players.Length)
            {
               is_male_indicators[AI_index] = rand.Next(2) == 0;
               string AI_name = AIPlayer.pick_random_name(is_male_indicators[AI_index]);
               bool taken = false;
            
               foreach (string name in PC_names)
               {
                  if (name == null)
                  {
                     continue;
                  }
                  else if (name.Equals(AI_name))
                  {
                     taken = true;
                     break;
                  }
               }
               foreach (string name in AI_names)
               {
                  if (name == null)
                  {
                     continue;
                  }
                  else if (name.Equals(AI_name)
                     || taken)
                  {
                     taken = true;
                     break;
                  }
               }

               if (!taken)
               {
                  AI_names[AI_index++] = AI_name;
               }
            }

            return AI_names;
         }

         private string[] choose_AI_names(string PC_name, out bool[] is_male_indicators)
         {
            string[] PC_names = new string[] { PC_name };
            return choose_AI_names(PC_names, out is_male_indicators);
         }

      static Game()
      {
         rand = new Random();
      }

      // Methods
      public void start_game(){
         intro();

         wait_for_acknowledgement();

         play_game();
      }

         private void intro()
         {
            Console.WriteLine("Welcome to Thomas Brown's Poker game!");
            Console.WriteLine();
            Console.Write("This is a rehashing of an old experiment I did ");
            Console.Write("for C++, but am now conceptually porting to C# to ");
            Console.Write("practice my raw understanding of the C# language.");
            Console.WriteLine();
            Console.WriteLine();


            Random rand = new Random();

            string[] locations = { "Across town and beyond the railroad tracks", "Down the street", "In the isolated city of Minewater", "Past the seedy red-light district", "Over the river and through the woods" };
            string[] official_tavern_names = { "The Caulky Beet", "The Tepid Hold", "Pa's Moonshine Shack", "Grandmother's Grave", "The Septic\'s Tank", "Beer and More", "The Beertles", "Crash", "That Drunkard's Place" };


            Console.Write("{0}, there's a shady, backwater tavern.", locations[rand.Next(locations.Length)]);
            Console.Write(" Though it officially goes by the name, \"{0}\", it is known to the locals as \"Sycophant's Grave\".", official_tavern_names[rand.Next(locations.Length)]);
            Console.Write(" Here, gamblers across the world come to drown their sorrows, search for a fresh beginning, or lose themselves in avarice.");
            Console.Write(" Once every year, there is an underground gambling tournament held to determine the best players around.");
            Console.Write(" As {0}, you've decided to take your chances in this seedy den of criminals and thieves to gamble your way to the top.", players[0].name);
            Console.Write(" You'll need luck, daring, and skill to make it to the top and win. Do you have what it takes?");
            Console.WriteLine();
            Console.WriteLine();
         }

         private void wait_for_acknowledgement()
         {
            // Wait for player acknowledgement
            Console.WriteLine("\t\t\tPress any key to continue...");
            Console.ReadKey(true);
            Console.WriteLine();
         }

      public void game_over(bool won)
      {
         if (player_queue.Peek().name.Equals(players[0]))
         {
            Console.WriteLine("Congratulations! You won ${0}", player_queue.Peek().money.net_profit);
         }
         else
         {
            Console.WriteLine("Game Over");
         }

         // reset held money for table, reset deck, reset graveyard, etc.
         bank.refresh();
         deck.reset();
      }

      public void play_game()
      {
         game_intro();
         reset_queue();

         while (!players[0].is_out // while player 1 (human player) isn't out
            && play_round()) // while still playing
         {
            reset_queue();
            draw_new_hand();
         }

         if (players[0].is_out)
         {
            game_over(false);
         }
         else
         {
            game_over(true);
         }
      }

         private void game_intro()
         {
            // introduce how the room looks and how you get there
            // introduce yourself sitting down
            // introduce other players
            // let the game begin!

            Random rand = new Random();

            string[] alert_lines = {"You are beckoned by a hostess to a side door",
                                      "You are motioned to by two large, brusque men to a small doorway", 
                                      "You stumble around in the hazy interior of the tavern until you find a door with a faded sign saying \"Keep Out!\"", 
                                      "You move towards the labeled VIP section", 
                                      "You order a \"Marjorie Special\", and with a knowing look, the bartender points you to a door",
                                      "You slip into the back unnoticed"};

            string[] location_lines = {"in a smokey den",
                                      "next to an old boxing ring",
                                      "in a modified guest room",
                                      "in an sparsely decorated room surrounded by one-way glass",
                                      "in a room straight from a sultan's palace",
                                      "in an auditorium in front of an old dance stage",
                                      "looking at a set of rickety, fold-out card tables"};

            // Describe transition to game table
            Console.WriteLine("{0}, and you soon find yourself {1}.", alert_lines[rand.Next(alert_lines.Length)], location_lines[rand.Next(location_lines.Length)]);
            Console.WriteLine();

            // Describe sitting down
            Console.Write("You find a table with your name on it, and sit down at one of the seats.");
            Console.Write(" Sitting down, you notice others around the room. Several of them approach and seat themselves at the table.");
            Console.WriteLine();
            Console.WriteLine();

            string[] sitting_lines = {"takes a seat",
                                     "grabs a chair",
                                     "sits down, slamming the table",
                                     "pulls out a cigar and lights it while sizing you up",
                                     "scoffs and sits down",
                                     "puts on some sunglasses and calmly sits",
                                     "sips a beer and takes a seat",
                                     "flashes an eery grin before sitting down",
                                     "confidently matches your gaze before sitting down",
                                     "takes a deep breath before taking a seat",
                                     "looks at the others before taking a seat",
                                     "nervously looks around and takes the nearest seat",
                                     "mutters something and slides into a chair"};

            // Describe others sitting down
            for (int i = 1; i < players.Length; i++)
            {
               Console.WriteLine("{0} {1}.", players[i].name, sitting_lines[rand.Next(sitting_lines.Length)]);
            }
            Console.WriteLine();

            Console.Write("A masked dealer emerges from the shadows and greets all players.");
            Console.Write(" He breaks open and expertly shuffles a new deck. Everyone grabs their cards.");
            Console.WriteLine(" The game begins!");
            Console.WriteLine();

            wait_for_acknowledgement();
         }

         // returns size of updated player queue
         // also used in play_round()
         private int reset_queue(bool ignore_folded = false, bool ignore_all_in = false)
         {
            player_queue.Clear();

            if (players.Length < 1)
            {
               return 0;
            }

            for (int i = 0; i < players.Length; i++)
            {
               if (players[i] == null)
               {
                  Console.WriteLine("ERROR: null player encountered in player set");
                  break;
               }
               else if (players[i].is_out)
               {
                  continue;
               }
               else if(players[i].folded
                  && ignore_folded){
                  continue;
               }
               else if (players[i].all_in
                  && ignore_all_in)
               {
                  continue;
               }
               else
               {
                  player_queue.Enqueue(players[i]);
               }
            }

            // update the starting_player_index
            // only applicable for big_blind/little blind stuff
            /*
            while (players[starting_player_index] != null
               && players[starting_player_index].is_out)
            {
               ++starting_player_index;
               starting_player_index %= players.Length;
            }

            // circle the queue around until you get to the starting player
            while (players[starting_player_index] != null
               && player_queue.Peek() != players[starting_player_index])
            {
               player_queue.Enqueue(player_queue.Dequeue());
            }
             * */

            return player_queue.Count;
         }

         private void draw_new_hand()
         {
            foreach (Player player in player_queue)
            {
               if (player is AIPlayer)
               {
                  AIPlayer AI = (AIPlayer)player;
                  AI.draw_new_hand();
               }
               else
               {
                  player.draw_new_hand();
               }
            }
         }

      public bool play_round()
      {
         // reset queue
         bool ignore_folded = true;
         bool ignore_all_in = true;
         reset_queue(ignore_folded, ignore_all_in);

         // collect ante from everyone
         collect_antes();
         // collect big and little blind from appropriate players
         // collect_blinds();

         // initialize method variables
         int turn = 1;
         int bet = bank.current_bet;
         bool is_call = false;

         // take first turn
         bet = play_turn(turn, ref is_call);

         Console.WriteLine();
         Console.WriteLine("First turn done.");
         Console.WriteLine();

         // take second turn
         if (player_queue.Count > 1)
         {
            process_discards();
            play_turn(++turn, ref is_call);
         }

         
         // wrap up the round
         display_hands();
         reset_queue(ignore_folded);
         Player[] winners = determine_winners();
         int divided_winnings = gather_divided_winnings(winners);
         end_round(winners, divided_winnings);

         return !is_game_over();
      }

         private void collect_antes()
         {
            foreach (Player player in player_queue)
            {
               bank.deposit(player.ante_up(bank.ante));
            }
            bank.bet(20);
         }

         private void process_discards()
         {
            foreach (Player player in player_queue)
            {
               if (player is AIPlayer)
               {
                  AIPlayer AI_Player = (AIPlayer)player;
                  int n_discarded = AI_Player.discard();
                  Console.WriteLine("{0} discarded {1} cards", AI_Player.name, n_discarded);
               }
               else
               {
                  player.discard();
               }
            }

            Console.WriteLine();
         }

         private void display_hands()
         {
            string[] round_end_lines = {"As silence fills the air",
                                       "As the last chips clatter to a halt",
                                       "Everyone stares at another player as they put down their hands",
                                       "The dealer coughs lightly",
                                       "Hazy figures approach the table to see the results of the round",
                                       "The dealer, with one short word, commands everyone to show their hand"};

            Console.WriteLine();
            Console.WriteLine("{0}, and everyone holds their breath as the results are declared...",
               round_end_lines[rand.Next(round_end_lines.Length)]);
            for (int i = 0; i < 3; i++)
            {
               for (int j = 0; j < i; j++)
               {
                  Console.Write("\t");
               }
                  Console.WriteLine("...");
            }

            Console.WriteLine();
            reset_queue();
            foreach (Player player in player_queue)
            {
               Console.WriteLine("{0} had a {1}...", player.name, Hand.get_winning_hand_string(player.hand)); // potentially organize so the highest hands are shown last
            }
            Console.WriteLine();
         }

         private Player[] determine_winners()
         {
            // guard conditions
            if (player_queue.Count < 1)
            {
               return new Player[0];
            }

            Player[] winners = new Player[player_queue.Count];
            int index = 0;

            Hand_Values best_hand_value = (Hand_Values)0;
            Card_Value best_card_value = (Card_Value)0;

            // get best hand/card combo
            foreach (Player player in player_queue)
            {
               if (player != null
                  && player.hand != null
                  && !player.folded
                  && !player.is_out)
               {
                  if (player.hand.best_hand_value > best_hand_value)
                  {
                     best_hand_value = player.hand.best_hand_value;
                     if (player.hand.cards[0] != null)
                     {
                        best_card_value = player.hand.cards[0].value;
                     }
                     else
                     {
                        Console.WriteLine("ERROR: Null card existing in hand when determining round winner.");
                     }
                  }
                  else if (player.hand.best_hand_value == best_hand_value)
                  {
                     if (player.hand.cards[0] != null
                        && player.hand.cards[0].value > best_card_value)
                     {
                        best_card_value = player.hand.cards[0].value;
                     }
                  }
               }
               else if (player == null
                  || player.hand == null)
               {
                  Console.WriteLine("ERROR: Null player or hand possessed by player when determining round winner.");
               }
            }

            foreach (Player player in player_queue)
            {
               if (player == null
                  || player.hand == null
                  || player.hand.cards[0] == null)
               {
                  Console.WriteLine("ERROR: Null exception when determining round winner.");
               }
               else if (!player.folded
                  && !player.is_out
                  && player.hand.best_hand_value == best_hand_value
                  && player.hand.cards[0].value == best_card_value)
               {
                  winners[index++] = player;
               }
            }

            // use tiebreaker to determine the best of the winners
            Hand[] hands = new Hand[index];
            for (int i = 0; i < index; i++)
            {
               hands[i] = winners[i].hand;
            }
            Card_Value tie_breaking_card = Hand.tie_breaker(hands);

            // use tie breaking card to figure out who the winner of winners is
            Player[] winners_of_winners = new Player[winners.Length];
            Card_Value best_tie_card_value = Hand.get_best_tie_card(winners[0].hand).value;
            // find the best tie card value
            for (int i = 0; i < winners.Length; i++)
            {
               if (winners[i] == null)
               {
                  break;
               }
               else
               {
                  Card best_tie_card = Hand.get_best_tie_card(winners[i].hand);
                  if (best_tie_card.value > best_tie_card_value)
                  {
                     best_tie_card_value = best_tie_card.value;
                  }
               }
            }
            // select the winners of the winners using the tie breaking card value
            index = 0;
            for (int i = 0; i < winners.Length; i++)
            {
               if (winners[i] == null)
               {
                  break;
               }
               else if (Hand.get_best_tie_card(winners[i].hand).value == best_tie_card_value)
               {
                  winners_of_winners[index++] = winners[i];
               }
            }

            // condense
            Player[] winners_no_nulls = new Player[index];
            for (int i = 0; i < index; i++)
            {
               winners_no_nulls[i] = winners_of_winners[i];
            }

            return winners_no_nulls;
         }

         private int gather_divided_winnings(Player[] winners)
         {
            int winnings = bank.withdraw(bank.current_amount);
            bank.refresh();
            winnings /= winners.Length;

            return winnings;
         }

         private void end_round(Player[] winners, int divided_winnings)
         {
            Console.WriteLine("\t\t888888888888888888888888888888888888");
            Console.WriteLine("\t\t888888888888888888888888888888888888");
            Console.WriteLine();

            for (int i = 0; i < players.Length; i++)
            {
               for (int j = 0; j < winners.Length; j++)
               {
                  if (winners[j] == null
                     && !players[i].is_out)
                  {
                     players[i].end_round(false, 0);
                  }
                  else if (players[i] == winners[j])
                  {
                     // display winning message
                     players[i].end_round(true, divided_winnings);
                  }
                  else if (!players[i].is_out)
                  {
                     players[i].end_round(false, 0);
                  }
               }
            }

            Console.WriteLine();
            Console.WriteLine("\t\t888888888888888888888888888888888888");
            Console.WriteLine("\t\t888888888888888888888888888888888888");
            Console.WriteLine();

            reset_queue();
         }

         private bool is_game_over()
         {
            int n_out = 0;
            for (int i = 0; i < players.Length; i++)
            {
               if (players[i] == null)
               {
                  Console.WriteLine("null player found when determining if game-end conditions are reached.");
                  break;
               }
               else if (players[i].is_out)
               {
                  ++n_out;
               }
            }

            return n_out >= players.Length - 1;
         }

      public int play_turn(int turn, ref bool is_call)
      {
         if(player_queue.Count <= 1){
            return 0;
         }

         Player original_player = player_queue.Peek();      // last player to bet or raise, or first player to go
         Player player = null;
         bool first_turn_taken = false;

         // continuously query players for their actions and corresponding bets until
         //    1) there is one player left
         //    2) the last player to bet or raise gets another turn
         //    3) all players check
         do
         {
            update_first_turn_taken(player, original_player, ref first_turn_taken);

            if (player_queue.Count == 1)
            {
               break;
            }

            // update player
            player = player_queue.Dequeue();

            // decide if player gets to go
            if (is_end_of_turn(player, original_player, first_turn_taken) == true)
            {
               break;
            }
            if (should_skip(player))
            {
               continue;
            }


            // take player action and get their bet
            Actions action = Actions.END;
            int player_bet;
            int call_amount = bank.current_bet - player.money.current_bet;

            if (player != null
               && player is AIPlayer)
            {
               action = AI_take_turn(player, ref is_call, out player_bet, turn);
            }
            else
            {
               action = human_take_turn(player, ref is_call, out player_bet);
            }

            update_bet_and_bank(action, player_bet, call_amount);

            // keep track of how many people still have to go (starts after the first bet)
            update_turn_loop_variables(action, player, ref original_player, ref first_turn_taken, ref is_call);
            enqueue(player);
         } while (player != null);

         end_turn();

         return bank.current_bet;
      }

         private bool is_end_of_turn(Player player, Player original_player, bool first_turn_taken){
            if(player != null
               && player == original_player
               && first_turn_taken
               && player.money.current_bet == bank.current_bet)
            {
               int original_queue_size = player_queue.Count;
               player_queue.Enqueue(player);

               // set the original player as the front of the queue
               for (int i = 0; i < original_queue_size; i++)
               {
                  player_queue.Enqueue(player_queue.Dequeue());
               }
               return true;
            }

            return false;
         }

         private void update_first_turn_taken(Player player, Player original_player, ref bool first_turn_taken)
         {
            if (player != null
               && player == original_player)
            {
               first_turn_taken = true;
            }
         }

         private bool should_skip(Player player)
         {
            if (player == null
               || player.should_skip())
            {
               return true;
            }
            else
            {
               return false;
            }
         }

         private Actions AI_take_turn(Player player, ref bool is_call, out int player_bet, int turn)
         {
            AIPlayer AI = (AIPlayer) player;
            int call_amount = bank.current_bet - AI.money.current_bet;
            player_bet = get_AI_bet(AI, is_call, call_amount, turn);
            return get_AI_action(AI, ref is_call, player_bet, call_amount);
         }

            private int get_AI_bet(AIPlayer AI, bool is_call, int call_amount, int turn)
            {
               AI.update_tell_chance_modifier(players);
               return AI.AI_take_action(turn == 1, is_call, call_amount, AI.times_this_turn);
            }

            private Actions get_AI_action(AIPlayer ai, ref bool is_call, int player_bet, int call_amount)
            {
               Actions action = Actions.END;

               if (player_bet == call_amount)
               {
                  action = Actions.CALL;
               }
               else if (player_bet == 0)
               {
                  action = Actions.FOLD;
               }
               else if (player_bet > 0)
               {
                  if (is_call)
                  {
                     action = Actions.RAISE;
                  }
                  else
                  {
                     action = Actions.BET;
                     is_call = true;
                  }
               }

               return action;
            }

         private Actions human_take_turn(Player player, ref bool is_call, out int player_bet)
         {
            Actions action = Actions.END;

            print_human_turn_output_separator();
            player.hand.display();

            // initialize while loop variables
            player_bet = -1;
            bool innocuous_action = true;

            while(player_bet < 0
               || innocuous_action)
            {
               action = Actions.END;
               while (action == Actions.END)
               {
                  action = player.choose_action(bank.current_bet, bank.current_amount, is_call);
               }
               Console.WriteLine();

               // update while loop variable
               if (action == Actions.CHECK_TELLS) // has to be handled from above the player's scope
               {
                  Player[] available_players = new Player[player_queue.Count];
                  int available_player_index = 0;
                  foreach (Player available_player in player_queue)
                  {
                     available_players[available_player_index++] = available_player;
                  }
                  player.check_tells(available_players);
               }
               else
               {
                  player_bet = player.take_action(action, bank.current_bet);
               }

               if (player_bet < 0)
               {
                  player_bet = -1;
                  action = Actions.END;
                  continue;
               }

               innocuous_action = action != Actions.BET 
                  && action != Actions.RAISE 
                  && action != Actions.FOLD 
                  && action != Actions.CALL;
            }

            print_human_turn_output_separator();

            return action;
         }
      
            private void print_human_turn_output_separator()
            {
               Console.WriteLine();
               Console.WriteLine("\t\t***********************************************");
               Console.WriteLine();
            }

         // returns current bank holdings
         // retrns -1 if there's an issue
         private int update_bet_and_bank(Actions action, int player_bet, int call_amount)
         {
            if (action == Actions.RAISE)
            {
               bank.deposit(call_amount);
            }
            if (action != Actions.CALL)
            {
               bank.deposit(player_bet); // to ensure that there is enough money for the bet to go through
               bank.bet(player_bet);
               bank.withdraw(player_bet);
            }
            if(bank.deposit(player_bet)){
               return bank.current_amount;
            }
            else
            {
               return -1;
            }
         }

         private void update_turn_loop_variables(Actions action, Player player, ref Player original_player, ref bool first_turn_taken, ref bool is_call)
         {
            if (action == Actions.FOLD)
            {
               player.fold();
               if (original_player == player)
               {
                  original_player = player_queue.Peek();
                  first_turn_taken = false;
               }
            }
            else if (action == Actions.BET)
            {
               is_call = true;
               original_player = player;
               first_turn_taken = false;
            }
            else if (action == Actions.RAISE)
            {
               if (!(original_player == player
                  && first_turn_taken))
               {
                  original_player = player;
                  first_turn_taken = false;
               }
            }
         }

         private bool enqueue(Player player)
         {
            if(!player.is_out
               && !player.folded
               && !player.all_in)
            {
               player_queue.Enqueue(player);
               return true;
            }
            else
            {
               return false;
            }
         }

         private void end_turn()
         {
            for (int i = 0; i < players.Length; i++)
            {
               players[i].end_turn();
            }
         }

      // Fields
      private Money bank;
      private Player[] players;
      private Queue<Player> player_queue;
      private Deck deck;
      private int starting_player_index;
      static private Random rand;
   }
}
