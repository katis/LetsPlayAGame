using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace LetsPlayAGame
{
    enum Player : byte {None, One, Two}

    enum Winner : byte { Nobody, Draw, PlayerOne, PlayerTwo }

    class GameViewModel : PropertyBase
    {
        private Player turn;
        public Player Turn {
            get { return turn; }
            set { SetProperty(ref this.turn, value); }
        }

        private int playerOnePoints;
        public int PlayerOnePoints
        {
            get { return playerOnePoints; }
            set { SetProperty(ref this.playerOnePoints, value); }
        }

        private int playerTwoPoints;
        public int PlayerTwoPoints
        {
            get { return playerTwoPoints; }
            set { SetProperty(ref this.playerTwoPoints, value); }
        }

        private readonly ICommand spaceClickCommand;
        public ICommand SpaceClickCommand { get { return spaceClickCommand; } }

        private ObservableCollection<ObservableCollection<Mark>> board;
        public ObservableCollection<ObservableCollection<Mark>> Board {
            get { return board; }
            private set { SetProperty(ref this.board, value); }
        }

        public readonly int WinCount;
        public readonly int Width;
        public readonly int Height;

        private int drawsInARow;

        public GameViewModel() : this(3, 3, 3) { }

        public GameViewModel(int winCount, int width, int height)
        {
            WinCount = winCount;
            Width = width;
            Height = height;
            drawsInARow = 0;

            Turn = Player.One;
            spaceClickCommand = new DelegateCommand<string>(s => MarkPlace(parseCoordinates(s)));

            Board = new ObservableCollection<ObservableCollection<Mark>>();

            for (var x = 0; x < Width; x++)
            {
                var col = new ObservableCollection<Mark>();
                for (var y = 0; y < Height; y++) col.Add(Mark.Empty);
                Board.Add(col);
            }
        }

        public void MarkPlace(Tuple<int, int> coords)
        {
            if (coords == null) return;
            if (Board[coords.Item1][coords.Item2] != Mark.Empty) return;

            Board[coords.Item1][coords.Item2] = turnMark();
            var winner = checkWinner(coords);
            if (winner != Winner.Nobody)
            {
                if (winner == Winner.PlayerOne)
                {
                    System.Windows.MessageBox.Show("YOU WIN.");
                    PlayerOnePoints++;
                    drawsInARow = 0;
                }
                else if (winner == Winner.PlayerTwo)
                {
                    System.Windows.MessageBox.Show("I WIN.");
                    PlayerTwoPoints++;
                    drawsInARow = 0;
                }
                else if (winner == Winner.Draw)
                {
                    drawsInARow++;
                    if (drawsInARow >= 3)
                    {
                        drawsInARow = 0;
                        System.Windows.MessageBox.Show("A STRANGE GAME.\nTHE ONLY WINNING MOVE IS\nNOT TO PLAY.\n\nHOW ABOUT A NICE GAME OF CHESS?");
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("STALEMATE.\nWANT TO PLAY AGAIN?");
                    }
                    PlayerOnePoints++;
                    PlayerTwoPoints++;
                }

                reset();
                return;
            }

            changeTurn();

            if (Turn == Player.Two) play();
        }

        /// <summary>
        /// Incredibly stupid random AI.
        /// </summary>
        private void play()
        {
            var spots = new List<Tuple<int, int>>();
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    if (Board[x][y] == Mark.Empty) spots.Add(Tuple.Create(x, y)); 
                }
            }

            Random r = new Random(new System.DateTime().Millisecond);
            var i = r.Next(spots.Count);
            MarkPlace(spots[i]);
        }

        private void reset()
        {
            Turn = Player.One;
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    Board[x][y] = Mark.Empty;
                }
            }
        }

        private Tuple<int, int> parseCoordinates(string coords)
        {
            var s = coords.Split(',');
            if (s.Length != 2) return null;
            return Tuple.Create(Convert.ToInt32(s[0]), Convert.ToInt32(s[1]));
        }

        private Mark turnMark()
        {
            if (Turn == Player.One) return Mark.Cross;
            else return Mark.Nought;
        }

        private void changeTurn()
        {
            if (Turn == Player.One) Turn = Player.Two;
            else Turn = Player.One;
        }

        private Winner checkWinner(Tuple<int, int> lastMove)
        {
            var coli = lastMove.Item1;
            var rowi = lastMove.Item2;

            var offset = WinCount - 1;
            var checkLen = WinCount * 2 - 1;

            // Check horizontally
            var winner = checkWinnerFromIndices(checkLen, x => Tuple.Create((coli + x) - offset, rowi));
            if (winner != Winner.Nobody) return winner;

            // Check vertically
            winner = checkWinnerFromIndices(checkLen, y => Tuple.Create(coli, rowi + y - offset));
            if (winner != Winner.Nobody) return winner;

            // Check diagonally right to left
            winner = checkWinnerFromIndices(checkLen, i => Tuple.Create(coli + i - offset, rowi + i - offset));
            if (winner != Winner.Nobody) return winner;

            // Check diagonally left to right
            winner = checkWinnerFromIndices(checkLen, i => Tuple.Create((coli - i) + offset, rowi + i - offset));

            if (winner == Winner.Nobody && isBoardFull()) return Winner.Draw;

            return winner;
        }

        /// <summary>
        /// Checks to see if a list created from the coordinates provided by the callback function
        /// has a winning combination of marks.
        /// </summary>
        /// <param name="length">Length for the list to be checked.</param>
        /// <param name="f">Callback that is provided with the current list index.
        /// The callback is called length times. Invalid board indices are handled as Mark.Empty </param>
        /// <returns>Winning player or Player.None if there is none.</returns>
        private Winner checkWinnerFromIndices(int length, Func<int, Tuple<int, int>> f)
        {
            var check = new List<Mark>(length);
            for (var i = 0; i < length; i++) check.Add(getMark(f(i)));
            return checkListWinner(check);
        }

        private Mark getMark(Tuple<int, int> coords)
        {
            return getMark(coords.Item1, coords.Item2);
        }

        private Mark getMark(int x, int y)
        {
            if (x < 0 || x >= Width) return Mark.Empty;
            if (y < 0 || y >= Height) return Mark.Empty;

            return Board[x][y];
        }

        private Winner checkListWinner(List<Mark> l)
        {
            var current = Mark.Empty;
            var inrow = 0;
            foreach (Mark mark in l)
            {
                if (mark == Mark.Empty)
                {
                    inrow = 0;
                    current = Mark.Empty;
                    continue;
                }
                if (mark == current)
                {
                    inrow++;
                }
                else
                {
                    inrow = 1;
                    current = mark;
                }
                if (inrow >= WinCount)
                {
                    if (current == Mark.Cross) return Winner.PlayerOne;
                    else return Winner.PlayerTwo;
                }
            }
            return Winner.Nobody;
        }

        private bool isBoardFull()
        {
            int empties = 0;
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    if (Board[x][y] == Mark.Empty) empties++;
                }
            }
            return empties == 0;
        }
    }
}
