using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;

namespace gamebot
{
	class gamebot
	{
		static void Main(string[] args) => new gamebot().Start();

		private static DiscordClient _client = new DiscordClient();

		public static string prefix = "g!"; // Sets custom bot prefix

		List<TicTacToe> TTTGames = new List<TicTacToe>();

		private void SaveState()
			{
			    List<JSON.TicTacToeStruct> ttts = new List<JSON.TicTacToeStruct>();
			    foreach (TicTacToe t in TTTGames)
			    {
				ttts.Add(t.ToStruct());
			    }
			    Save.Saves(ttts.ToArray(), "ttt.json");

			    // Console.WriteLine("[Debug] TicTacToe: Saved!");
			}
		
		public void Start()
		{
			_client.Log.Message += (s, e) => Console.WriteLine($"[{e.Severity}] {e.Source}: {e.Message}");

			_client.MessageReceived += async (s, e) =>
			{
				try
				{
					if (!e.User.IsBot)
					{
						string msg = e.Message.RawText;
						string rawcmd = "no-cmd"; // Filler command
						if (msg.StartsWith(prefix)) // Check if message starts with prefix
							rawcmd = msg.Replace(prefix, ""); // Set rawcmd to full command (cmd + arguments)
						string cmd = rawcmd.Split(' ')[0]; // Grab just the command

						string[] par = msg.Split(' ').Skip(1).ToArray(); // Grabs the arguments used in the command

						//						string parContents = null;
						//						foreach (string arg in par)
						//							parContents += arg + " ";
						//						Console.WriteLine(parContents);

						if (cmd == "help") // help command code
						{
							string greet = "Heya! I'm gamebot, a bot for automating various games. Here's a list of games/commands you can use:";
							string help = "`g!help` - displays info about the bot & bot commands";
							string info = "`g!info` - displays extra info about the bot";
							string ttt = "`g!ttt` - displays help about tic-tac-toe";

							await e.Channel.SendMessage($"{greet}\n\n{help}\n{info}\n{ttt}");
						}
						else if (cmd == "info") // info command
						{
							string contributors = "`Noahkiq` and `Technochips`";
							string message = $"Heya! I'm gamebot, a bot for automating various games. I've been created by {contributors}. You can report issues, make suggestions, or examine my source code at <https://github.com/Noahkiq/gamebot/>.";
							await e.Channel.SendMessage(message);
						}
						else if (cmd == "ttt") // tictactoe command code
						{
							string helpNew = $"Type `{prefix}{cmd} new <mention>` to invite someone to play Tic Tac Toe.";
							string helpPlay = $"Type `{prefix}{cmd} play <X> <Y>` to place a cross or a circle in a game.";
							string helpCancel = $"Type `{prefix}{cmd} cancel` to cancel your current game in this channel.";
							if (par.Length == 1) // checks if only one command argument was supplied
							{
								if (par[0] == "new")
									await e.Channel.SendMessage(helpNew); // outputs the 'helpNew' string if the argument was 'new'
								else if (par[0] == "play")
									await e.Channel.SendMessage(helpPlay); // same as above comment, but with 'helpPlay' string and 'play' arg
								else if (par[0] == "cancel")
								{
									int i = TicTacToe.SearchPlayer(TTTGames.ToArray(), e.User, e.Channel); // searches for a game with command runner and channel
									if (i != -1) //checks if it actually finds a player
									{
										TTTGames.RemoveAt(i); // deletes game at 'i', which will be the current game if found
										await e.Channel.SendMessage($"The game has successfully been cancelled.");
									}
									else
										await e.Channel.SendMessage($"You are currently not in a game in this channel.");
									SaveState();
								}
								else if (par[0] == "save")
								{
									SaveState();
									await e.Channel.SendMessage("Saved!");
								}
								else
									await e.Channel.SendMessage($"{helpNew}\n{helpPlay}\n{helpCancel}");
							}
							else if (par.Length == 2 || par.Length == 4) // checks if two or four arguments were supplied
							{
								if (par[0] == "play")
									await e.Channel.SendMessage(helpPlay); // too few requirements were supplied so help is shown
								else if (par[0] == "cancel")
									await e.Channel.SendMessage(helpCancel);
								else if (par[0] == "new")
								{
									User[] mentioned = e.Message.MentionedUsers.ToArray();
									if (mentioned.Length != 1 || mentioned[0] == null)
										await e.Channel.SendMessage(helpNew); // too few (or many) users were mentioned, help is show
									else
									{
										var i = TicTacToe.SearchPlayer(TTTGames.ToArray(), e.User, e.Channel); //search the user
										var j = TicTacToe.SearchPlayer(TTTGames.ToArray(), mentioned[0], e.Channel); //search the mentioned user
										if (i == -1 && j == -1) //if it doesn't find anything
										{
											if (mentioned[0].IsBot)
												await e.Channel.SendMessage($"You cannot play against another bot!");
											// else if (mentioned[0].Status.Value == UserStatus.Offline)
											//     await e.Channel.SendMessage($"You cannot play against an offline/invisible user!");
											else if (mentioned[0] == e.User)
												await e.Channel.SendMessage($"You cannot play a game with yourself!");
											else
											{
												if (par.Length == 2)
												{
													TTTGames.Add(new TicTacToe(e.User, mentioned[0], e.Channel)); // a new TTT game is added to 'TTTGames' with the command runner, opponent, and channel
													await e.Channel.SendMessage("A new game has started!");
												}
												else if (par.Length == 4)
												{
													bool validInts = true;

													try
													{
														int.Parse(par[2]);
														int.Parse(par[3]);
													}
													catch
													{
														validInts = false;
													}

													if (validInts)
													{
														if (int.Parse(par[2]) < 3 || int.Parse(par[3]) < 3)
														{
															await e.Channel.SendMessage("Board must be atleast 3x3.");
														}
														else if (int.Parse(par[2]) > 9 || int.Parse(par[3]) > 9)
														{
															await e.Channel.SendMessage("Board must be atmost 9x9.");
														}
														else
														{
															TTTGames.Add(new TicTacToe(e.User, mentioned[0], e.Channel, int.Parse(par[2]), int.Parse(par[3]))); // a new TTT game is added to 'TTTGames' with the command runner, opponent, channel, and board size
															await e.Channel.SendMessage($"A new game has started with a board size of {int.Parse(par[2])} x {int.Parse(par[3])}!");
														}
													}
													else
														await e.Channel.SendMessage($"**Error:** Invalid integers were supplied for the board size.");
												}
											}
										}
										else if (i != -1) //if it has found the user
											await e.Channel.SendMessage("You are already in a game in this channel."); //the user cannot play two game in a channel
										else if (j != -1) //if it has found the mentioned user
											await e.Channel.SendMessage("They are already in a game in this channel."); //the user cannot play with another user playing another game	
									}
									SaveState();
								}
								else
									await e.Channel.SendMessage($"{helpNew}\n{helpPlay}\n{helpCancel}"); // send default help message if no valid commands were detected

							}
							else if (par.Length == 3)
							{
								if (par[0] == "new")
									await e.Channel.SendMessage(helpNew); // too many requirements were supplied so help is shown
								else if (par[0] == "cancel")
									await e.Channel.SendMessage(helpCancel);
								else if (par[0] == "play")
								{
									int i = TicTacToe.SearchPlayer(TTTGames.ToArray(), e.User, e.Channel);
									if (i != -1) //checks if it actually finds a player
									{
										bool? isc = TTTGames[i].TakeTurn(e.User, int.Parse(par[1]), int.Parse(par[2])); //check the turn
										if (isc == true) //if the turn was successful
										{
											await e.Channel.SendMessage(TTTGames[i].DrawGame()); //write down the game
											var c = TTTGames[i].CheckGame(); //check if someone wins
											if (c == TicTacToe.GameStat.CircleWin || c == TicTacToe.GameStat.CrossWin) //if someone wins
											{
												await e.Channel.SendMessage($"Congratulation, <@{e.User.Id}>, you won!");
												TTTGames.RemoveAt(i); //delete the game
											}
											else if (c == TicTacToe.GameStat.Tie) //if there is a tie
											{
												await e.Channel.SendMessage("You are both stuck, there is a tie. The game has ended.");
												TTTGames.RemoveAt(i); //delete the game
											}
											//otherwise well the game continues
										}
										else if (isc == false)
											await e.Channel.SendMessage("It's not your turn."); //the user cannot play if it's not his turn
										else
											await e.Channel.SendMessage("You can't place a shape onto another shape."); //the user cannot cheat by replacing a shape
										SaveState();
									}
									else
										await e.Channel.SendMessage("You are currently not in a game in this channel."); //the user cannot play if he's not playing
								}
								else
									await e.Channel.SendMessage($"{helpNew}\n{helpPlay}\n{helpCancel}"); // invalid arguments given, help displayed
							}
							else
								await e.Channel.SendMessage($"{helpNew}\n{helpPlay}\n{helpCancel}");
						}
						//						else if (cmd == "hangman") // hangman command code
						//						{
						//							if (par.Length == 1) // checks if only one command argument was supplied
						//							{
						//								if (par[0] == "new") {
						//									await e.Channel.SendMessage("Setting up game..."); // outputs string
						//								}
						//							}
						//						}
						else if (cmd == "crash")
						{
							throw new Exception("Manual crash tester.");
						}
					}
				}
				catch (Exception ex)
				{
					await e.Channel.SendMessage("**Error:** A unexcepted error happened.\nIf you think this bug should be fixed, go here: <https://github.com/Noahkiq/gamebot/issues>");
					if (ex.ToString().Length < 2000)
						await e.Channel.SendMessage($"```\n{ex}```");
					Console.WriteLine(ex);
				}
			};
			string token = File.ReadAllText("bot-token.txt");
			_client.ExecuteAndWait(async () =>
				{
					await _client.Connect(token, TokenType.Bot);
				});
			_client.Ready += (s, e) =>
			{
				if (File.Exists(Save.path + "ttt.json"))
				{
					Console.WriteLine("[Info] TicTacToe: Games file found. Loading.");
					JSON.TicTacToeStruct[] gamej = Save.Load<JSON.TicTacToeStruct[]>("ttt.json");

					foreach (JSON.TicTacToeStruct j in gamej)
					{
					TTTGames.Add(TicTacToe.ToClass(j, _client));
					}

					if (gamej.Length == 1)
						Console.WriteLine("[Info] TicTacToe: Loaded " + gamej.Length.ToString() + " game from file!");
					else
						Console.WriteLine("[Info] TicTacToe: Loaded " + gamej.Length.ToString() + " games from file!");
				}
			};
		}
	}
}
