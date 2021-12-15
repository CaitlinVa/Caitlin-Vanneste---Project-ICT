namespace TicTacToe.Logic
{
	// Handles the game logic for Tic Tac Toe and keeps track of player data.
	public class GameLogic
	{
		//Tracks the number of moves made by the players.
		public int MoveCounter { get; set; } = 0;

		// Keeps track of the current player.
		public int CurrentPlayer { get; set; } = 1;

		// Keeps track of whether the game is over (via won or draw).
		public bool GameOver { get; set; }

		// Determine the current player. Used by the UI and game logic.
		// Player 1 is red color and moves on even numbers.
		// Player 2 is blue color and moves on odd numbers.
		public bool IsPlayerOneTurn
		{
			get { return MoveCounter % 2 == 0; }
		}

		// Keeps track of the rectangles users have picked.
		// This array maps to the rectangles' UID in the XAML.
		// 0 is unclaimed; 1 = player 1; 2 = player 2.
		public int[] PickedRectangles { get; set; } = new int[9];

		// Constructor.
		public GameLogic() { }

		// Checks for a winning combination on the game board, assigning the result to the appropriate property.
		public void CheckForGameWinner()
		{
			GameOver = GameWinnerFound();
		}

		// Checks if the rectangle a player clicked on is "valid." (not claimed by either player)
		public bool IsValidMove(int clickedRectangleIndex)
		{
			if (PickedRectangles[clickedRectangleIndex] == 0)
			{
				return true;
			}

			return false;
		}

		// Checks the game rectangles for a winning combination of 3 consecutive rectangles horizontally, vertically, or diagonally.
		// There is likely a better way to check all 8 winning combinations but this is at least easily readable.
		private bool GameWinnerFound()
		{
			if (WinningCombinationFound(0, 1, 2)) return true;
			if (WinningCombinationFound(3, 4, 5)) return true;
			if (WinningCombinationFound(6, 7, 8)) return true;

			if (WinningCombinationFound(0, 3, 6)) return true;
			if (WinningCombinationFound(1, 4, 7)) return true;
			if (WinningCombinationFound(2, 5, 8)) return true;

			if (WinningCombinationFound(0, 4, 8)) return true;
			if (WinningCombinationFound(2, 4, 6)) return true;

			return false;
		}

		// Scans 3 provided rectangles for a winning combination. (3 in a row)
		private bool WinningCombinationFound(int left, int mid, int right)
		{
			return (
				PickedRectangles[left] == CurrentPlayer &&
				PickedRectangles[mid] == CurrentPlayer &&
				PickedRectangles[right] == CurrentPlayer
			);
		}

		// Resets game-tracking properties for a new game.
		public void ResetGame()
		{
			MoveCounter = 0;
			GameOver = false;
			CurrentPlayer = 1;
			PickedRectangles = new int[9];
		}
	}
}
