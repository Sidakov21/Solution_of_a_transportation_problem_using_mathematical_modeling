using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace УП_Транспортная_задача__методы_Сев_зап_и_Наимен__по_ММ
{
    public partial class MainWindow : Window
    {
        // ---- Класс для отображения исходной матрицы ----
        public class RowData
        {
            public string A { get; set; }
            public int B1 { get; set; }
            public int B2 { get; set; }
            public int B3 { get; set; }
            public int B4 { get; set; }
            public int B5 { get; set; }
            public int Запас { get; set; }
        }

        // ---- Класс для отображения опорных планов ----
        public class PlanRow
        {
            public string A { get; set; }
            public string B1 { get; set; }
            public string B2 { get; set; }
            public string B3 { get; set; }
            public string B4 { get; set; }
            public string B5 { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            LoadDataAndDisplay();
        }

        // ---- Основной метод загрузки данных ----
        private void LoadDataAndDisplay()
        {
            // Стоимость перевозок (матрица)
            int[,] D =
            {
                { 40, 19, 25, 25, 35 },
                { 49, 26, 27, 18, 38 },
                { 46, 27, 36, 40, 45 }
            };

            // Запасы (поставщики)
            int[] supply = { 230, 250, 170 };

            // Потребности (покупатели)
            int[] demand = { 140, 90, 160, 110, 150 };

            DisplayBalanceInfo(supply, demand);
            DisplayMatrix(D, supply);
            DisplayNorthWestPlan(D, supply, demand);
            DisplayLeastCostPlan(D, supply, demand);
        }

        // ---- Проверка сбалансированности задачи ----
        private void DisplayBalanceInfo(int[] supply, int[] demand)
        {
            int supplySum = supply.Sum();
            int demandSum = demand.Sum();

            if (supplySum == demandSum)
            {
                BalanceLabel.Content = $"Задача сбалансирована ✅ (Σa = {supplySum}, Σb = {demandSum})";
                BalanceLabel.Foreground = Brushes.Green;
            }
            else
            {
                BalanceLabel.Content = $"Задача несбалансирована ❌ (Σa = {supplySum}, Σb = {demandSum})";
                BalanceLabel.Foreground = Brushes.Red;
            }
        }

        // ---- Отображение исходной матрицы ----
        private void DisplayMatrix(int[,] D, int[] supply)
        {
            var rows = new List<RowData>();

            for (int i = 0; i < D.GetLength(0); i++)
            {
                rows.Add(new RowData
                {
                    A = $"A{i + 1}",
                    B1 = D[i, 0],
                    B2 = D[i, 1],
                    B3 = D[i, 2],
                    B4 = D[i, 3],
                    B5 = D[i, 4],
                    Запас = supply[i]
                });
            }

            DataGridMatrix.ItemsSource = rows;
        }

        // ---- Метод Северо-Западного угла ----
        private void DisplayNorthWestPlan(int[,] D, int[] supply, int[] demand)
        {
            int[,] plan = NorthWestCorner(supply, demand);
            DataGridNorthWest.ItemsSource = ConvertPlanToRows(plan);
        }

        private int[,] NorthWestCorner(int[] supplyArr, int[] demandArr)
        {
            int m = supplyArr.Length;
            int n = demandArr.Length;
            int[,] plan = new int[m, n];

            int[] supply = (int[])supplyArr.Clone();
            int[] demand = (int[])demandArr.Clone();

            int i = 0, j = 0;

            while (i < m && j < n)
            {
                int x = Math.Min(supply[i], demand[j]);
                plan[i, j] = x;
                supply[i] -= x;
                demand[j] -= x;

                if (supply[i] == 0 && demand[j] == 0)
                {
                    i++;
                    j++;
                }
                else if (supply[i] == 0)
                    i++;
                else
                    j++;
            }

            return plan;
        }

        // ---- Метод минимальных элементов ----
        private void DisplayLeastCostPlan(int[,] D, int[] supply, int[] demand)
        {
            int[,] plan = LeastCostMethod(D, supply, demand);
            DataGridLeastCost.ItemsSource = ConvertPlanToRows(plan);
        }

        private int[,] LeastCostMethod(int[,] cost, int[] supplyArr, int[] demandArr)
        {
            int m = supplyArr.Length;
            int n = demandArr.Length;
            int[,] plan = new int[m, n];
            bool[,] used = new bool[m, n];

            int[] supply = (int[])supplyArr.Clone();
            int[] demand = (int[])demandArr.Clone();

            while (supply.Sum() > 0 && demand.Sum() > 0)
            {
                int minI = -1, minJ = -1;
                int minCost = int.MaxValue;

                // Находим минимальную стоимость
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (!used[i, j] && cost[i, j] < minCost)
                        {
                            minCost = cost[i, j];
                            minI = i;
                            minJ = j;
                        }
                    }
                }

                if (minI == -1 || minJ == -1)
                    break;

                int x = Math.Min(supply[minI], demand[minJ]);
                plan[minI, minJ] = x;

                supply[minI] -= x;
                demand[minJ] -= x;

                // Отмечаем использованные строки/столбцы
                if (supply[minI] == 0)
                    for (int j = 0; j < n; j++) used[minI, j] = true;

                if (demand[minJ] == 0)
                    for (int i = 0; i < m; i++) used[i, minJ] = true;
            }

            return plan;
        }

        // ---- Вспомогательный метод: конвертация матрицы плана в строки ----
        private List<PlanRow> ConvertPlanToRows(int[,] plan)
        {
            var list = new List<PlanRow>();

            for (int i = 0; i < plan.GetLength(0); i++)
            {
                list.Add(new PlanRow
                {
                    A = $"A{i + 1}",
                    B1 = plan[i, 0] == 0 ? "" : plan[i, 0].ToString(),
                    B2 = plan[i, 1] == 0 ? "" : plan[i, 1].ToString(),
                    B3 = plan[i, 2] == 0 ? "" : plan[i, 2].ToString(),
                    B4 = plan[i, 3] == 0 ? "" : plan[i, 3].ToString(),
                    B5 = plan[i, 4] == 0 ? "" : plan[i, 4].ToString()
                });
            }

            return list;
        }
    }
}
