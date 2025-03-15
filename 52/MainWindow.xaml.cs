using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxAndChickensGame
{
    public partial class MainWindow : Window
    {
        private Dictionary<TextBlock, (int row, int col)> chickens = new();
        private Dictionary<TextBlock, (int row, int col)> foxes = new();
        private TextBlock selectedChicken = null;

        private HashSet<(int, int)> validCells = new()
        {
            (0,2), (0,3), (0,4),
            (1,2), (1,3), (1,4),
            (2,0), (2,1), (2,2), (2,3), (2,4), (2,5), (2,6),
            (3,0), (3,1), (3,2), (3,3), (3,4), (3,5), (3,6),
            (4,0), (4,1), (4,2), (4,3), (4,4), (4,5), (4,6),
            (5,2), (5,3), (5,4),
            (6,2), (6,3), (6,4),
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            this.KeyDown += Window_KeyDown;
        }

        private void InitializeGame()
        {
            foreach (var cell in validCells)
            {
                var border = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Background = System.Windows.Media.Brushes.Beige
                };
                Grid.SetRow(border, cell.Item1);
                Grid.SetColumn(border, cell.Item2);
                GridMain.Children.Add(border);
            }

            AddFox(2, 2);
            AddFox(2, 4);

            int[,] chickenPositions =
            {
                {3,0}, {3,1}, {3,2}, {3,3}, {3,4}, {3,5}, {3,6},
                {4,0}, {4,1}, {4,2}, {4,3}, {4,4}, {4,5}, {4,6},
                {5,2}, {5,3}, {5,4},
                {6,2}, {6,3}, {6,4}
            };

            for (int i = 0; i < chickenPositions.GetLength(0); i++)
            {
                AddChicken(chickenPositions[i, 0], chickenPositions[i, 1]);
            }
        }

        private void AddChicken(int row, int col)
        {
            var chicken = new TextBlock
            {
                Text = "🐥",
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            chickens[chicken] = (row, col);
            Grid.SetRow(chicken, row);
            Grid.SetColumn(chicken, col);
            chicken.MouseLeftButtonDown += Chicken_Clicked;
            GridMain.Children.Add(chicken);
        }

        private void AddFox(int row, int col)
        {
            var fox = new TextBlock
            {
                Text = "🦊",
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            foxes[fox] = (row, col);
            Grid.SetRow(fox, row);
            Grid.SetColumn(fox, col);
            GridMain.Children.Add(fox);
        }

        private void Chicken_Clicked(object sender, MouseButtonEventArgs e)
        {
            if (selectedChicken != null)
                selectedChicken.FontSize = 24;

            selectedChicken = sender as TextBlock;

            if (selectedChicken != null)
                selectedChicken.FontSize = 32;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (selectedChicken == null) return;

            var (row, col) = chickens[selectedChicken];
            int newRow = row, newCol = col;

            switch (e.Key)
            {
                case Key.Up: newRow--; break;
                case Key.Left: newCol--; break;
                case Key.Right: newCol++; break;
                case Key.Down: return; // Курицы не ходят вниз
                default: return;
            }

            if (IsInsideGrid(newRow, newCol) && !IsOccupied(newRow, newCol))
            {
                chickens[selectedChicken] = (newRow, newCol);
                Grid.SetRow(selectedChicken, newRow);
                Grid.SetColumn(selectedChicken, newCol);

                selectedChicken.FontSize = 24;
                selectedChicken = null;

                FoxesMove(); // Ход лисы после игрока
            }
        }

        private bool IsInsideGrid(int row, int col) => validCells.Contains((row, col));

        private bool IsOccupied(int row, int col)
        {
            return chickens.Values.Any(pos => pos == (row, col)) ||
                   foxes.Values.Any(pos => pos == (row, col));
        }

        private void FoxesMove()
        {
            if (foxes.Count == 0 || chickens.Count == 0)
                return;

            var rand = new Random();
            var foxList = foxes.ToList();
            var foxEntry = foxList[rand.Next(foxList.Count)];
            var fox = foxEntry.Key;
            var (foxRow, foxCol) = foxEntry.Value;

            var nearestChicken = chickens.Values
                .OrderBy(c => Math.Abs(c.row - foxRow) + Math.Abs(c.col - foxCol))
                .First();

            var (chickRow, chickCol) = nearestChicken;
            int dRow = chickRow - foxRow;
            int dCol = chickCol - foxCol;

            int stepRow = dRow != 0 ? dRow / Math.Abs(dRow) : 0;
            int stepCol = dCol != 0 ? dCol / Math.Abs(dCol) : 0;

            // Попытка съесть курицу
            int midRow = foxRow + stepRow;
            int midCol = foxCol + stepCol;
            int landRow = foxRow + 2 * stepRow;
            int landCol = foxCol + 2 * stepCol;

            if (IsInsideGrid(landRow, landCol) &&
                chickens.Values.Contains((midRow, midCol)) &&
                !IsOccupied(landRow, landCol))
            {
                var toRemove = chickens.First(c => c.Value == (midRow, midCol)).Key;
                GridMain.Children.Remove(toRemove);
                chickens.Remove(toRemove);

                foxes[fox] = (landRow, landCol);
                Grid.SetRow(fox, landRow);
                Grid.SetColumn(fox, landCol);
                return;
            }

            // Попытка просто сдвинуться в доступное направление
            var directions = new List<(int dr, int dc)>
            {
                (stepRow, stepCol),
                (-1, 0), (1, 0), (0, -1), (0, 1)
            };

            foreach (var (dr, dc) in directions.OrderBy(_ => rand.Next()))
            {
                int newRow = foxRow + dr;
                int newCol = foxCol + dc;

                if (IsInsideGrid(newRow, newCol) && !IsOccupied(newRow, newCol))
                {
                    foxes[fox] = (newRow, newCol);
                    Grid.SetRow(fox, newRow);
                    Grid.SetColumn(fox, newCol);
                    return;
                }
            }
        }
    }
}
