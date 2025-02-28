using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace _52
{
    public partial class MainWindow : Window
    {
        private Dictionary<TextBlock, (int row, int col)> chickens = new Dictionary<TextBlock, (int, int)>();
        private TextBlock selectedChicken = null;

        public MainWindow()
        {
            InitializeComponent();
            InitializeChickens();
        }

        private void InitializeChickens()
        {
            // Находим все куры на поле
            foreach (var child in GridMain.Children)
            {
                if (child is TextBlock tb && tb.Text == "🐥")
                {
                    int row = Grid.GetRow(tb);
                    int col = Grid.GetColumn(tb);
                    chickens[tb] = (row, col);
                    tb.MouseLeftButtonDown += Chicken_Click;
                }
            }

            this.KeyDown += Window_KeyDown;
        }

        private void Chicken_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock chicken)
            {
                selectedChicken = chicken;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (selectedChicken == null) return;

            (int row, int col) = chickens[selectedChicken];
            int newRow = row, newCol = col;

            switch (e.Key)
            {
                case Key.W: newRow--; break;
                case Key.S: newRow++; break;
                case Key.A: newCol--; break;
                case Key.D: newCol++; break;
            }

            if (IsMoveValid(newRow, newCol))
            {
                Grid.SetRow(selectedChicken, newRow);
                Grid.SetColumn(selectedChicken, newCol);
                chickens[selectedChicken] = (newRow, newCol);
            }
        }

        private bool IsMoveValid(int row, int col)
        {
            // Проверяем, не выходит ли за границы
            if (row < 0 || row >= 7 || col < 0 || col >= 7) return false;

            // Проверяем, занято ли поле другой курицей или лисой
            foreach (var chicken in chickens.Values)
                if (chicken.row == row && chicken.col == col)
                    return false;

            foreach (var child in GridMain.Children)
                if (child is TextBlock tb && tb.Text == "🦊" && Grid.GetRow(tb) == row && Grid.GetColumn(tb) == col)
                    return false;

            return true;
        }
    }
}
