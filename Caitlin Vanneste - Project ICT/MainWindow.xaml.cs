using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TicTacToe.Logic;


namespace Caitlin_Vanneste___Project_ICT
{
	// Interaction logic for MainWindow.xaml
	public partial class MainWindow : Window
	{
		SerialPort _serialPort;
		byte[] _data;
		const int START_ADDRESS = 0;
		const int NUMBER_OF_DMX_BYTES = 512;
		DispatcherTimer _dispatcherTimer;

		private const string Player1Message = "Go Player 1!";
		private const string Player2Message = "Go Player 2!";
		private const string TiedGameMessage = "It's a tie!";
		private const string SymbolX = "X";
		private const string SymbolO = "O";

		// Manages the logic details of the game, separating those concerns from the game's UI.
		private GameLogic _ticTacToeLogic = null;
		private GameLogic TicTacToeLogic
		{
			get
			{
				if (_ticTacToeLogic == null)
				{
					_ticTacToeLogic = new GameLogic();
				}

				return _ticTacToeLogic;
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			cbxPortName.Items.Add("None");
			foreach (string s in SerialPort.GetPortNames())
				cbxPortName.Items.Add(s);

			_serialPort = new SerialPort();
			_serialPort.BaudRate = 250000;
			_serialPort.StopBits = StopBits.Two;

			_data = new byte[NUMBER_OF_DMX_BYTES];

			_dispatcherTimer = new DispatcherTimer();
			_dispatcherTimer.Interval = TimeSpan.FromSeconds(0.1);
			_dispatcherTimer.Tick += _dispatcherTimer_Tick;
			_dispatcherTimer.Start();

			// The primary UI that the user interacts with. Contains our game objects.
			// Player 1 always starts first and is "X" with red background.
			UpdateGameLabelForNextPlayer(Player1Message, Brushes.Red);
		}

		private void SendDmxData(byte[] data, SerialPort serialPort)
		{
			data[0] = 0;

			if (serialPort != null && serialPort.IsOpen)
			{
				serialPort.BreakState = true;
				Thread.Sleep(1);
				serialPort.BreakState = false;
				Thread.Sleep(1);

				serialPort.Write(data, 0, data.Length);
			}
		}

		private void _dispatcherTimer_Tick(object sender, EventArgs e)
		{
			SendDmxData(_data, _serialPort);
		}

		private void cbxPortName_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_serialPort != null)
			{
				if (_serialPort.IsOpen)
					_serialPort.Close();

				if (cbxPortName.SelectedItem.ToString() != "None")
				{
					_serialPort.PortName = cbxPortName.SelectedItem.ToString();
					_serialPort.Open();

				}
				else
				{

				}
			}
		}
		
		// The LED's on the DMX panel will go one and will form a red X on the panel.
		// The one's with 0 will go out, this is neccessary when player O is one and need to go out.
		private void PlayerX()
		{
			_data[START_ADDRESS + 1] = Convert.ToByte(255);
			_data[START_ADDRESS + 10] = Convert.ToByte(255);
			_data[START_ADDRESS + 16] = Convert.ToByte(255);
			_data[START_ADDRESS + 19] = Convert.ToByte(255);
			_data[START_ADDRESS + 28] = Convert.ToByte(255);
			_data[START_ADDRESS + 31] = Convert.ToByte(255);
			_data[START_ADDRESS + 37] = Convert.ToByte(255);
			_data[START_ADDRESS + 46] = Convert.ToByte(255);

			_data[START_ADDRESS + 6] = Convert.ToByte(0);
			_data[START_ADDRESS + 9] = Convert.ToByte(0);
			_data[START_ADDRESS + 15] = Convert.ToByte(0);
			_data[START_ADDRESS + 24] = Convert.ToByte(0);
			_data[START_ADDRESS + 27] = Convert.ToByte(0);
			_data[START_ADDRESS + 36] = Convert.ToByte(0);
			_data[START_ADDRESS + 42] = Convert.ToByte(0);
			_data[START_ADDRESS + 45] = Convert.ToByte(0);


		}

		// The LED's on the DMX panel will go one and will form a blue O on the panel.
		private void PlayerO()
		{
			_data[START_ADDRESS + 6] = Convert.ToByte(255);
			_data[START_ADDRESS + 9] = Convert.ToByte(255);
			_data[START_ADDRESS + 15] = Convert.ToByte(255);
			_data[START_ADDRESS + 24] = Convert.ToByte(255);
			_data[START_ADDRESS + 27] = Convert.ToByte(255);
			_data[START_ADDRESS + 36] = Convert.ToByte(255);
			_data[START_ADDRESS + 42] = Convert.ToByte(255);
			_data[START_ADDRESS + 45] = Convert.ToByte(255);

			_data[START_ADDRESS + 1] = Convert.ToByte(0);
			_data[START_ADDRESS + 10] = Convert.ToByte(0);
			_data[START_ADDRESS + 16] = Convert.ToByte(0);
			_data[START_ADDRESS + 19] = Convert.ToByte(0);
			_data[START_ADDRESS + 28] = Convert.ToByte(0);
			_data[START_ADDRESS + 31] = Convert.ToByte(0);
			_data[START_ADDRESS + 37] = Convert.ToByte(0);
			_data[START_ADDRESS + 46] = Convert.ToByte(0);
		}

		private void OnRectangleClick(object sender, MouseButtonEventArgs e)
		{
			if (TicTacToeLogic.GameOver)
			{
				return;
			}

			Rectangle clickedPiece = e.Source as Rectangle;
			int rectangleId = Convert.ToInt32(clickedPiece.Uid);

			if (TicTacToeLogic.IsValidMove(rectangleId))
			{
				UpdateGameUi(clickedPiece);
				UpdateGameLogicMap(rectangleId);
				TicTacToeLogic.MoveCounter++;
				if (TicTacToeLogic.CurrentPlayer == 1)
				{
					PlayerX();
				}
				else
				{
					PlayerO();
				}

				if (TicTacToeLogic.MoveCounter >= 5)
				{
					CheckForGameWinner();
					CheckForGameTie();
				}

				TicTacToeLogic.CurrentPlayer = TicTacToeLogic.IsPlayerOneTurn ? 1 : 2;
			}
		}

		// Reinitializes the game UI to the starting state (e.g. a new game).
				private void btn_Reset_Click(object sender, RoutedEventArgs e)
		{
			foreach (Rectangle gamePieces in grid_TicTacToe.Children.OfType<Rectangle>())
			{
				gamePieces.Fill = new SolidColorBrush(Colors.White);
			}

			TicTacToeLogic.ResetGame();
			UpdateGameLabelForNextPlayer(Player1Message, Brushes.Red);
		}

		// Examines the game board after a player has moved to determine, if there is a winner.
		// This code will halt the game if a winner exists; in that case the game must be reset via the 'Reset' button.
		private void CheckForGameWinner()
		{
			TicTacToeLogic.CheckForGameWinner();

			if (TicTacToeLogic.GameOver)
			{
				UpdateGameLabelForNextPlayer(string.Format("Player {0} has won!", TicTacToeLogic.CurrentPlayer), Brushes.DarkGoldenrod);
			}
		}

		// Ends the game if there is no winner after all 9 rectangles have been picked.
		private void CheckForGameTie()
		{
			if (TicTacToeLogic.MoveCounter >= 9 && !TicTacToeLogic.GameOver)
			{
				UpdateGameLabelForNextPlayer(TiedGameMessage, Brushes.Black);
				TicTacToeLogic.GameOver = true;
			}
		}

		// Updates the game UI based on the last player's actions.
		// name="clickedRectangle": The rectangle clicked by the user.
		private void UpdateGameUi(Rectangle clickedRectangle)
		{
			if (TicTacToeLogic.IsPlayerOneTurn)
			{
				UpdateRectangleFill(clickedRectangle, SymbolX, new SolidColorBrush(Colors.Red));
				UpdateGameLabelForNextPlayer(Player2Message, Brushes.Blue);
			}
			else
			{
				UpdateRectangleFill(clickedRectangle, SymbolO, new SolidColorBrush(Colors.Blue));
				UpdateGameLabelForNextPlayer(Player1Message, Brushes.Red);
			}
		}

		// Fills the rectangle that the user has clicked in the game.
		// name="clickedRectangle": The rectangle clicked by the user.
		// name="color": The color the rectangle will be filled with.
		private void UpdateRectangleFill(Rectangle clickedRectangle, string playerSymbol, Brush color)
		{
			if (clickedRectangle != null)
			{
				TextBlock tb = new TextBlock();
				tb.FontSize = 72;
				tb.Background = color;
				tb.Text = playerSymbol;
				BitmapCacheBrush bcb = new BitmapCacheBrush(tb);
				clickedRectangle.Fill = bcb;
			}
		}

		// Updates the game labels and color, indicating the turn of the next player.
		private void UpdateGameLabelForNextPlayer(string labelMessage, Brush color)
		{
			label_GameMessage.Content = labelMessage;
			label_GameMessage.Foreground = color;
		}

		// Updates the GameLogic's map with the index of the last rectangle picked.
		// The array index maps to the UID of the rectangle that was clicked.
		private void UpdateGameLogicMap(int arrayIndex)
		{
			int mapValue = TicTacToeLogic.CurrentPlayer;
			TicTacToeLogic.PickedRectangles[arrayIndex] = mapValue;
		}
		// When the window is closing all the led's will go out.
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SendDmxData(new byte[NUMBER_OF_DMX_BYTES], _serialPort);
			_serialPort.Dispose();
		}
	}
}
