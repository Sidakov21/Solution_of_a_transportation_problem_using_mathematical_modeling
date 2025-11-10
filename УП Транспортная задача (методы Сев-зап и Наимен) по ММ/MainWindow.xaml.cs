using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace УП_Транспортная_задача__методы_Сев_зап_и_Наимен__по_ММ
{
    public partial class MainWindow : Window
    {
        private int[,] costMatrix;
        private int[] supply;
        private int[] demand;
        private bool isBalanced;
        private bool hasFictitiousSupplier;
        private bool hasFictitiousConsumer;

        public class CostRow : INotifyPropertyChanged
        {
            public string Поставщик { get; set; }

            private List<int> _стоимости;
            public List<int> Стоимости
            {
                get => _стоимости;
                set { _стоимости = value; OnPropertyChanged(nameof(Стоимости)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class SupplyItem : INotifyPropertyChanged
        {
            private int _zapas;
            public int Zapas
            {
                get => _zapas;
                set
                {
                    _zapas = value;
                    OnPropertyChanged(nameof(Zapas));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class DemandItem : INotifyPropertyChanged
        {
            private int _potrebnost;
            public int Potrebnost
            {
                get => _potrebnost;
                set
                {
                    _potrebnost = value;
                    OnPropertyChanged(nameof(Potrebnost));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateMatrix_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int m = int.Parse(SuppliersCountBox.Text);
                int n = int.Parse(ConsumersCountBox.Text);

                // Проверка на положительные размеры
                if (m <= 0 || n <= 0)
                {
                    MessageBox.Show("Количество поставщиков и потребителей должно быть положительным числом!");
                    return;
                }

                // Создаем матрицу стоимостей
                var matrixRows = new System.Collections.ObjectModel.ObservableCollection<CostRow>();

                for (int i = 0; i < m; i++)
                {
                    var row = new CostRow
                    {
                        Поставщик = $"A{i + 1}",
                        Стоимости = Enumerable.Repeat(0, n).ToList()
                    };
                    matrixRows.Add(row);
                }

                CostMatrixGrid.ItemsSource = matrixRows;

                // Создание столбцов вручную
                CostMatrixGrid.Columns.Clear();
                CostMatrixGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Поставщик",
                    Binding = new System.Windows.Data.Binding("Поставщик"),
                    IsReadOnly = true
                });

                for (int j = 0; j < n; j++)
                {
                    CostMatrixGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = $"B{j + 1}",
                        Binding = new System.Windows.Data.Binding($"Стоимости[{j}]"),
                    });
                }

                // Запасы и потребности
                var supplyItems = new System.Collections.ObjectModel.ObservableCollection<SupplyItem>();
                var demandItems = new System.Collections.ObjectModel.ObservableCollection<DemandItem>();

                for (int i = 0; i < m; i++)
                    supplyItems.Add(new SupplyItem { Zapas = 0 });

                for (int j = 0; j < n; j++)
                    demandItems.Add(new DemandItem { Potrebnost = 0 });

                SupplyGrid.ItemsSource = supplyItems;
                DemandGrid.ItemsSource = demandItems;

                // Настройка AutoGenerateColumns для SupplyGrid и DemandGrid
                SupplyGrid.AutoGenerateColumns = true;
                DemandGrid.AutoGenerateColumns = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании матрицы: {ex.Message}");
            }
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int m = int.Parse(SuppliersCountBox.Text);
                int n = int.Parse(ConsumersCountBox.Text);

                // 1. Считываем и проверяем матрицу стоимостей
                costMatrix = new int[m, n];
                bool hasNegativeCosts = false;
                if (CostMatrixGrid.ItemsSource is System.Collections.ObjectModel.ObservableCollection<CostRow> costRows)
                {
                    for (int i = 0; i < m; i++)
                    {
                        var row = costRows[i];
                        for (int j = 0; j < n; j++)
                        {
                            int value = row.Стоимости[j];
                            if (value < 0)
                            {
                                hasNegativeCosts = true;
                                value = 0; // Заменяем отрицательные на 0 для продолжения расчета
                            }
                            costMatrix[i, j] = value;
                        }
                    }
                }

                if (hasNegativeCosts)
                {
                    MessageBox.Show("Обнаружены отрицательные стоимости! Они были заменены на 0.", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // 2. Считываем и проверяем запасы
                supply = new int[m];
                bool hasNegativeSupply = false;
                if (SupplyGrid.ItemsSource is System.Collections.ObjectModel.ObservableCollection<SupplyItem> supplyItems)
                {
                    for (int i = 0; i < m; i++)
                    {
                        int value = supplyItems[i].Zapas;
                        if (value < 0)
                        {
                            hasNegativeSupply = true;
                            value = 0; // Заменяем отрицательные на 0
                        }
                        supply[i] = value;
                    }
                }

                if (hasNegativeSupply)
                {
                    MessageBox.Show("Обнаружены отрицательные запасы! Они были заменены на 0.", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // 3. Считываем и проверяем потребности
                demand = new int[n];
                bool hasNegativeDemand = false;
                if (DemandGrid.ItemsSource is System.Collections.ObjectModel.ObservableCollection<DemandItem> demandItems)
                {
                    for (int j = 0; j < n; j++)
                    {
                        int value = demandItems[j].Potrebnost;
                        if (value < 0)
                        {
                            hasNegativeDemand = true;
                            value = 0; // Заменяем отрицательные на 0
                        }
                        demand[j] = value;
                    }
                }

                if (hasNegativeDemand)
                {
                    MessageBox.Show("Обнаружены отрицательные потребности! Они были заменены на 0.", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // Проверяем, что есть ненулевые запасы и потребности
                if (supply.Sum() == 0 || demand.Sum() == 0)
                {
                    MessageBox.Show("Сумма запасов или потребностей равна нулю! Невозможно решить задачу.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем, что данные считаны
                if (supply == null || demand == null || costMatrix == null)
                {
                    MessageBox.Show("Ошибка: данные не были правильно инициализированы");
                    return;
                }

                // Отображаем информацию о балансе
                DisplayBalanceInfo(supply, demand);

                // Балансируем задачу при необходимости
                int[,] balancedCostMatrix;
                int[] balancedSupply;
                int[] balancedDemand;

                BalanceProblem(out balancedCostMatrix, out balancedSupply, out balancedDemand);

                // Выбираем метод решения на основе выбора пользователя
                int[,] plan;
                string methodName;

                if (NorthWestRadio.IsChecked == true)
                {
                    plan = NorthWestCorner(balancedSupply, balancedDemand);
                    methodName = "Северо-Западный угол";
                }
                else
                {
                    plan = LeastCostMethod(balancedCostMatrix, balancedSupply, balancedDemand);
                    methodName = "Минимальный элемент";
                }

                // Отображаем результаты
                DisplayPlan(ResultGrid, plan, methodName, balancedSupply.Length, balancedDemand.Length);
                ResultTitle.Text = $"Опорный план ({methodName})";

                // Вычисляем стоимость только для реальных перевозок (без фиктивных)
                int totalCost = CalcTotalCost(plan, balancedCostMatrix, balancedSupply.Length, balancedDemand.Length);
                ResultCostLabel.Text = $"Общая стоимость: {totalCost}";

                // Добавим отладочную информацию
                Console.WriteLine($"Матрица стоимостей: {m}x{n}");
                Console.WriteLine($"Запасы: {string.Join(", ", supply)}");
                Console.WriteLine($"Потребности: {string.Join(", ", demand)}");
                Console.WriteLine($"Выбранный метод: {methodName}");
                Console.WriteLine($"Сумма плана: {SumPlan(plan)}");
                Console.WriteLine($"Сбалансированные запасы: {string.Join(", ", balancedSupply)}");
                Console.WriteLine($"Сбалансированные потребности: {string.Join(", ", balancedDemand)}");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете: {ex.Message}\n\nДетали: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Балансировка транспортной задачи
        /// </summary>
        private void BalanceProblem(out int[,] balancedCostMatrix, out int[] balancedSupply, out int[] balancedDemand)
        {
            int totalSupply = supply.Sum();
            int totalDemand = demand.Sum();

            isBalanced = totalSupply == totalDemand;

            if (isBalanced)
            {
                // Задача сбалансирована - используем исходные данные
                balancedCostMatrix = costMatrix;
                balancedSupply = supply;
                balancedDemand = demand;
                hasFictitiousSupplier = false;
                hasFictitiousConsumer = false;
                return;
            }

            if (totalSupply > totalDemand)
            {
                // Добавляем фиктивного потребителя
                int m = supply.Length;
                int n = demand.Length + 1;

                balancedCostMatrix = new int[m, n];
                balancedSupply = new int[m];
                balancedDemand = new int[n];

                // Копируем исходные данные
                for (int i = 0; i < m; i++)
                {
                    balancedSupply[i] = supply[i];
                    for (int j = 0; j < n - 1; j++)
                    {
                        balancedCostMatrix[i, j] = costMatrix[i, j];
                    }
                    // Стоимость до фиктивного потребителя = 0
                    balancedCostMatrix[i, n - 1] = 0;
                }

                for (int j = 0; j < n - 1; j++)
                {
                    balancedDemand[j] = demand[j];
                }
                // Потребность фиктивного потребителя = разность
                balancedDemand[n - 1] = totalSupply - totalDemand;

                hasFictitiousSupplier = false;
                hasFictitiousConsumer = true;
            }
            else
            {
                // Добавляем фиктивного поставщика
                int m = supply.Length + 1;
                int n = demand.Length;

                balancedCostMatrix = new int[m, n];
                balancedSupply = new int[m];
                balancedDemand = new int[n];

                // Копируем исходные данные
                for (int i = 0; i < m - 1; i++)
                {
                    balancedSupply[i] = supply[i];
                    for (int j = 0; j < n; j++)
                    {
                        balancedCostMatrix[i, j] = costMatrix[i, j];
                    }
                }

                for (int j = 0; j < n; j++)
                {
                    balancedDemand[j] = demand[j];
                }

                // Запас фиктивного поставщика = разность
                balancedSupply[m - 1] = totalDemand - totalSupply;
                // Стоимость от фиктивного поставщика = 0
                for (int j = 0; j < n; j++)
                {
                    balancedCostMatrix[m - 1, j] = 0;
                }

                hasFictitiousSupplier = true;
                hasFictitiousConsumer = false;
            }
        }

        private void DisplayPlan(DataGrid dataGrid, int[,] plan, string methodName, int rows, int cols)
        {
            try
            {
                // Создаем коллекцию для отображения
                var displayRows = new System.Collections.ObjectModel.ObservableCollection<dynamic>();

                for (int i = 0; i < rows; i++)
                {
                    dynamic row = new System.Dynamic.ExpandoObject();
                    var rowDict = row as IDictionary<string, object>;

                    // Помечаем фиктивного поставщика
                    if (hasFictitiousSupplier && i == rows - 1)
                    {
                        rowDict["Поставщик"] = $"Ф{i + 1} (фикт.)";
                    }
                    else
                    {
                        rowDict["Поставщик"] = $"A{i + 1}";
                    }

                    for (int j = 0; j < cols; j++)
                    {
                        string columnName;
                        // Помечаем фиктивного потребителя
                        if (hasFictitiousConsumer && j == cols - 1)
                        {
                            columnName = $"Ф{j + 1} (фикт.)";
                        }
                        else
                        {
                            columnName = $"B{j + 1}";
                        }

                        rowDict[columnName] = plan[i, j] == 0 ? "" : plan[i, j].ToString();
                    }
                    displayRows.Add(row);
                }

                dataGrid.ItemsSource = displayRows;

                // Настраиваем столбцы
                dataGrid.Columns.Clear();
                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Поставщик",
                    Binding = new System.Windows.Data.Binding("Поставщик"),
                    IsReadOnly = true,
                    Width = 120
                });

                for (int j = 0; j < cols; j++)
                {
                    string header;
                    if (hasFictitiousConsumer && j == cols - 1)
                    {
                        header = $"Ф{j + 1} (фикт.)";
                    }
                    else
                    {
                        header = $"B{j + 1}";
                    }

                    dataGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = header,
                        Binding = new System.Windows.Data.Binding($"{header}"),
                        IsReadOnly = true,
                        Width = 70
                    });
                }

                Console.WriteLine($"{methodName}: отображено {rows} строк, {cols} столбцов");
                Console.WriteLine($"Фиктивный поставщик: {hasFictitiousSupplier}, Фиктивный потребитель: {hasFictitiousConsumer}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении плана {methodName}: {ex.Message}");
            }
        }

        // Вспомогательный метод для отладки - подсчет суммы плана
        private int SumPlan(int[,] plan)
        {
            int sum = 0;
            for (int i = 0; i < plan.GetLength(0); i++)
                for (int j = 0; j < plan.GetLength(1); j++)
                    sum += plan[i, j];
            return sum;
        }

        private void DisplayBalanceInfo(int[] supply, int[] demand)
        {
            int sumA = supply.Sum();
            int sumB = demand.Sum();
            if (sumA == sumB)
            {
                BalanceLabel.Content = $"Задача сбалансирована ✅ (ΣA={sumA}, ΣB={sumB})";
                BalanceLabel.Foreground = Brushes.Green;
            }
            else if (sumA > sumB)
            {
                BalanceLabel.Content = $"Задача несбалансирована ❌ (ΣA={sumA} > ΣB={sumB}) - добавим фиктивного потребителя";
                BalanceLabel.Foreground = Brushes.Orange;
            }
            else
            {
                BalanceLabel.Content = $"Задача несбалансирована ❌ (ΣA={sumA} < ΣB={sumB}) - добавим фиктивного поставщика";
                BalanceLabel.Foreground = Brushes.Orange;
            }
        }

        private int[,] NorthWestCorner(int[] supplyArr, int[] demandArr)
        {
            int m = supplyArr.Length, n = demandArr.Length;
            int[,] plan = new int[m, n];
            int[] s = (int[])supplyArr.Clone(), d = (int[])demandArr.Clone();
            int i = 0, j = 0;

            while (i < m && j < n)
            {
                int x = Math.Min(s[i], d[j]);
                plan[i, j] = x;
                s[i] -= x; d[j] -= x;
                if (s[i] == 0 && d[j] == 0) { i++; j++; }
                else if (s[i] == 0) i++;
                else j++;
            }
            return plan;
        }

        private int[,] LeastCostMethod(int[,] cost, int[] supplyArr, int[] demandArr)
        {
            int m = supplyArr.Length, n = demandArr.Length;
            int[,] plan = new int[m, n];
            bool[,] used = new bool[m, n];
            int[] s = (int[])supplyArr.Clone(), d = (int[])demandArr.Clone();

            while (s.Sum() > 0 && d.Sum() > 0)
            {
                int minI = -1, minJ = -1, min = int.MaxValue;
                for (int i = 0; i < m; i++)
                    for (int j = 0; j < n; j++)
                        if (!used[i, j] && cost[i, j] < min)
                        { min = cost[i, j]; minI = i; minJ = j; }

                int x = Math.Min(s[minI], d[minJ]);
                plan[minI, minJ] = x;
                s[minI] -= x; d[minJ] -= x;

                if (s[minI] == 0) for (int j = 0; j < n; j++) used[minI, j] = true;
                if (d[minJ] == 0) for (int i = 0; i < m; i++) used[i, minJ] = true;
            }
            return plan;
        }

        private int CalcTotalCost(int[,] plan, int[,] cost, int rows, int cols)
        {
            int total = 0;
            for (int i = 0; i < rows; i++)
            {
                // Пропускаем фиктивного поставщика при расчете стоимости
                if (hasFictitiousSupplier && i == rows - 1) continue;

                for (int j = 0; j < cols; j++)
                {
                    // Пропускаем фиктивного потребителя при расчете стоимости
                    if (hasFictitiousConsumer && j == cols - 1) continue;

                    total += plan[i, j] * cost[i, j];
                }
            }
            return total;
        }
    }
}