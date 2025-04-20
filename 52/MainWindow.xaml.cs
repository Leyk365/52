using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FoxAndChickensGame
{
    public partial class MainWindow : Window
    {
        private Dictionary<TextBlock, (int row, int col)> chickens = new();
        private Dictionary<TextBlock, (int row, int col)> foxes = new();
        private TextBlock selectedChicken = null;
        private TextBlock chickenCounterText = new TextBlock();
        private StackPanel menuPanel = new StackPanel();
        private Random random = new Random();

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

        private HashSet<(int, int)> winningCells = new()
        {
            (0,2), (0,3), (0,4),
            (1,2), (1,3), (1,4),
            (2,2), (2,3), (2,4)
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeMenu();
        }

        private void InitializeMenu()
        {
            GridMain.Children.Clear();
            menuPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Vertical,
                Background = new LinearGradientBrush(Colors.LightGoldenrodYellow, Colors.Wheat, 45)
            };


            var title = new TextBlock
            {
                Text = "🦊 Лисы и Куры 🐥",
                FontSize = 32,
                Margin = new Thickness(0, 0, 0, 40),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.SaddleBrown
            };


            var playButton = CreateMenuButton("Играть", Brushes.LightGreen, () => StartGame());


            var rulesButton = CreateMenuButton("Правила", Brushes.LightBlue, () => ShowRules());


            var exitButton = CreateMenuButton("Выйти", Brushes.LightCoral, () => Close());

            menuPanel.Children.Add(title);
            menuPanel.Children.Add(playButton);
            menuPanel.Children.Add(rulesButton);
            menuPanel.Children.Add(exitButton);

            Grid.SetRow(menuPanel, 0);
            Grid.SetColumn(menuPanel, 0);
            Grid.SetRowSpan(menuPanel, 7);
            Grid.SetColumnSpan(menuPanel, 7);
            GridMain.Children.Add(menuPanel);
        }

        private Button CreateMenuButton(string content, Brush background, Action action)
        {
            var button = new Button
            {
                Content = content,
                FontSize = 24,
                Width = 220,
                Height = 60,
                Margin = new Thickness(0, 0, 0, 20),
                Background = background,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.SaddleBrown,
                BorderThickness = new Thickness(2),
                Style = (Style)FindResource(ToolBar.ButtonStyleKey)
            };
            button.Click += (s, e) => action();
            return button;
        }

        private void ShowRules()
        {
            string rules = @"ПРАВИЛА ДЛЯ ИГРОКА:

🎯 Цель игры:
Провести 9 кур в ЗЕЛЕНУЮ зону (верхний квадрат).

🕹️ Управление:
1. Кликните на курицу (🐥), чтобы выбрать
2. Используйте стрелки:
   ↑ - вверх, ← - влево, → - вправо
   (Вниз ходить нельзя!)

⚠️ Поражение:
- Если лисы съедят 12+ кур

🐺 Поведение лис:
- Ходят после каждого вашего хода
- Обязаны есть при возможности
- Особенно опасны для кур в зеленой зоне

🏆 Советы:
1. Сначала двигайте нижних кур
2. Блокируйте лис другими курами
3. Не оставляйте кур поодиночке";

            var rulesWindow = new Window
            {
                Title = "Правила игры",
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = rules,
                        FontSize = 18,
                        Padding = new Thickness(25),
                        TextWrapping = TextWrapping.Wrap,
                        FontFamily = new FontFamily("Arial"),
                        Foreground = Brushes.DarkSlateGray
                    },
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Background = Brushes.FloralWhite
                },
                Width = 450,
                Height = 500,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = Brushes.LightGoldenrodYellow,
                FontWeight = FontWeights.Bold
            };

            rulesWindow.ShowDialog();
        }

        private void StartGame()
        {
            GridMain.Children.Remove(menuPanel);
            InitializeGame();
            this.KeyDown += Window_KeyDown;
        }

        private void InitializeGame()
        {
            GridMain.Children.Clear();
            chickens.Clear();
            foxes.Clear();


            foreach (var cell in validCells)
            {
                var border = new Border
                {
                    BorderBrush = Brushes.SaddleBrown,
                    BorderThickness = new Thickness(1),
                    Background = winningCells.Contains(cell)
                        ? new LinearGradientBrush(
                            Color.FromRgb(200, 255, 200),
                            Color.FromRgb(100, 255, 100),
                            new Point(0, 0), new Point(1, 1))
                        : new LinearGradientBrush(
                            Color.FromRgb(222, 184, 135),
                            Color.FromRgb(210, 180, 140),
                            new Point(0, 0), new Point(1, 1))
                };

                if (winningCells.Contains(cell))
                {
                    border.ToolTip = "Целевая зона - приведите сюда 9 кур";
                    border.BorderBrush = Brushes.Goldenrod;
                    border.BorderThickness = new Thickness(2);

                    // Добавляем иконку цели для наглядности
                    var targetIcon = new TextBlock
                    {
                        Text = "🎯",
                        FontSize = 14,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Opacity = 0.7
                    };
                    border.Child = targetIcon;
                }

                Grid.SetRow(border, cell.Item1);
                Grid.SetColumn(border, cell.Item2);
                GridMain.Children.Add(border);
            }

            // Счетчик кур
            var counterPanel = new Border
            {
                Background = new LinearGradientBrush(Colors.Wheat, Colors.BurlyWood, 90),
                BorderBrush = Brushes.SaddleBrown,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var counterStack = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            chickenCounterText = new TextBlock
            {
                Text = "20",
                FontSize = 26,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.SaddleBrown
            };

            var chickenIcon = new TextBlock
            {
                Text = "🐥",
                FontSize = 26
            };

            counterStack.Children.Add(chickenCounterText);
            counterStack.Children.Add(chickenIcon);
            counterPanel.Child = counterStack;

            Grid.SetRow(counterPanel, 0);
            Grid.SetColumn(counterPanel, 5);
            Grid.SetColumnSpan(counterPanel, 2);
            GridMain.Children.Add(counterPanel);


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

            UpdateChickenCounter();
        }

        private void AddChicken(int row, int col)
        {
            var chicken = new TextBlock
            {
                Text = "🐥",
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "Курица (кликните для выбора)",
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1)
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
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "Лиса (управляется компьютером)",
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1)
            };

            foxes[fox] = (row, col);
            Grid.SetRow(fox, row);
            Grid.SetColumn(fox, col);
            GridMain.Children.Add(fox);
        }

        private void UpdateChickenCounter()
        {
            chickenCounterText.Text = chickens.Count.ToString();

            if (chickens.Count <= 8)
            {
                ShowGameOverMessage();
                return;
            }

            CheckForWin();
        }

        private void ShowGameOverMessage()
        {
            MessageBox.Show("Лисы победили! Они съели слишком много кур.", "Поражение",
                MessageBoxButton.OK, MessageBoxImage.Information);
            InitializeMenu();
        }

        private void CheckForWin()
        {
            int chickensInWinningCells = chickens.Values.Count(pos => winningCells.Contains(pos));
            if (chickensInWinningCells >= 9)
            {
                MessageBox.Show("Поздравляем! Вы привели достаточное количество кур в безопасную зону!", "Победа",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                InitializeMenu();
            }
        }

        private void Chicken_Clicked(object sender, MouseButtonEventArgs e)
        {
            if (selectedChicken != null)
            {
                selectedChicken.FontSize = 28;
                (selectedChicken.RenderTransform as ScaleTransform).ScaleX = 1;
                (selectedChicken.RenderTransform as ScaleTransform).ScaleY = 1;
            }

            selectedChicken = sender as TextBlock;

            if (selectedChicken != null)
            {
                selectedChicken.FontSize = 34;
                var scale = selectedChicken.RenderTransform as ScaleTransform;
                scale.ScaleX = 1.2;
                scale.ScaleY = 1.2;
                selectedChicken.BringIntoView();
            }
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
                case Key.Down: return;
                default: return;
            }

            if (IsInsideGrid(newRow, newCol) && !IsOccupied(newRow, newCol))
            {
                chickens[selectedChicken] = (newRow, newCol);
                Grid.SetRow(selectedChicken, newRow);
                Grid.SetColumn(selectedChicken, newCol);


                selectedChicken.FontSize = 28;
                var scale = selectedChicken.RenderTransform as ScaleTransform;
                scale.ScaleX = 1;
                scale.ScaleY = 1;
                selectedChicken = null;


                Dispatcher.BeginInvoke(new Action(() =>
                {
                    FoxesMove();
                    UpdateChickenCounter();
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }

        private bool IsInsideGrid(int row, int col)
        {

            if (row < 0 || row > 6 || col < 0 || col > 6)
                return false;


            return validCells.Contains((row, col));
        }

        private bool IsOccupied(int row, int col)
        {
            return chickens.Values.Any(pos => pos == (row, col)) ||
                   foxes.Values.Any(pos => pos == (row, col));
        }

        private void FoxesMove()
        {
            if (foxes.Count == 0 || chickens.Count == 0)
                return;

            foreach (var foxEntry in foxes.ToList())
            {
                var fox = foxEntry.Key;
                var (foxRow, foxCol) = foxEntry.Value;


                foreach (var direction in new[] { "up", "down", "left", "right" })
                {
                    int dr = 0, dc = 0;
                    switch (direction)
                    {
                        case "up": dr = -1; break;
                        case "down": dr = 1; break;
                        case "left": dc = -1; break;
                        case "right": dc = 1; break;
                    }

                    int chickenRow = foxRow + dr;
                    int chickenCol = foxCol + dc;
                    int landRow = foxRow + 2 * dr;
                    int landCol = foxCol + 2 * dc;


                    if (chickens.ContainsValue((chickenRow, chickenCol)) &&
                        IsInsideGrid(landRow, landCol) &&
                        !IsOccupied(landRow, landCol))
                    {

                        var toRemove = chickens.First(c => c.Value == (chickenRow, chickenCol)).Key;
                        GridMain.Children.Remove(toRemove);
                        chickens.Remove(toRemove);

                        foxes[fox] = (landRow, landCol);
                        Grid.SetRow(fox, landRow);
                        Grid.SetColumn(fox, landCol);
                        return;
                    }
                }
            }


            SimpleFoxMove();
        }

        private void SimpleFoxMove()
        {
            var rand = new Random();
            foreach (var foxEntry in foxes.ToList())
            {
                var fox = foxEntry.Key;
                var (foxRow, foxCol) = foxEntry.Value;


                var directions = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) }
                    .OrderBy(x => rand.Next()).ToList();

                foreach (var (dr, dc) in directions)
                {
                    int newRow = foxRow + dr;
                    int newCol = foxCol + dc;

                    if (IsInsideGrid(newRow, newCol) && !IsOccupied(newRow, newCol))
                    {
                        foxes[fox] = (newRow, newCol);
                        Grid.SetRow(fox, newRow);
                        Grid.SetColumn(fox, newCol);
                        break;
                    }
                }
            }
        }

        private int DistanceToWinningZone(int row, int col)
        {
            return winningCells.Min(wc => Math.Abs(wc.Item1 - row) + Math.Abs(wc.Item2 - col));
        }

        private List<(int dr, int dc)> GetStrategicDirections(int foxRow, int foxCol)
        {
            var directions = new List<(int dr, int dc)>();


            var importantChickens = chickens.Values
                .Where(c => winningCells.Contains(c) || DistanceToWinningZone(c.row, c.col) <= 1)
                .ToList();

            if (importantChickens.Any())
            {

                var avgRow = (int)importantChickens.Average(c => c.row);
                var avgCol = (int)importantChickens.Average(c => c.col);

                int dr = avgRow.CompareTo(foxRow);
                int dc = avgCol.CompareTo(foxCol);


                if (dr != 0) directions.Add((dr, 0));
                if (dc != 0) directions.Add((0, dc));


                directions.Add((-1, 0));
                directions.Add((1, 0));
                directions.Add((0, -1));
                directions.Add((0, 1));
            }
            else
            {

                var nearestChicken = chickens.Values
                    .OrderBy(c => Math.Abs(c.row - foxRow) + Math.Abs(c.col - foxCol))
                    .FirstOrDefault();

                if (nearestChicken != default)
                {
                    int dr = nearestChicken.row.CompareTo(foxRow);
                    int dc = nearestChicken.col.CompareTo(foxCol);

                    if (dr != 0) directions.Add((dr, 0));
                    if (dc != 0) directions.Add((0, dc));
                }


                directions.Add((-1, 0));
                directions.Add((1, 0));
                directions.Add((0, -1));
                directions.Add((0, 1));
            }


            return directions.OrderBy(x => random.Next()).ToList();
        }
    }
}