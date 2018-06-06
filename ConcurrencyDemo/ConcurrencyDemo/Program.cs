using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrencyDemo
{
    class Program
    {
        static string[] _products = new string[] { "Lays", "Cheetos", "Pringles", "Estrella", "Русская картошка" };

        static Dictionary<string, int> _stockBalances = new Dictionary<string, int>();

        static void Main(string[] args)
        {
            foreach (var product in _products)
            {
                _stockBalances[product] = 0;
            }

            var worktime = TimeSpan.FromSeconds(2);
            Task t1 = Task.Run(() => DoWork(worktime));
            Task t2 = Task.Run(() => DoWork(worktime));
            Task t3 = Task.Run(() => DoWork(worktime));
            Task t4 = Task.Run(() => DoWork(worktime));

            Task.WaitAll(t1, t2, t3, t4);

            PrintLeftovers();
            Console.ReadKey();
        }


        static void BuyProduct(string productName, int amount)
        {
            _stockBalances[productName] += amount;
        }

        static bool TrySellProduct(string productName)
        {
            if (_stockBalances[productName] > 0)
            {
                _stockBalances[productName]--;
                return true;
            }
            return false;
        }

        static void DoWork(TimeSpan workTime)
        {
            var rand = new Random(Thread.CurrentThread.ManagedThreadId);
            var start = DateTime.Now;
            while (DateTime.Now - start < workTime)
            {
                Thread.Sleep(rand.Next(100));
                bool buy = (rand.Next(6) == 0);
                string product = _products[rand.Next(_products.Length)];
                if (buy)
                {
                    int amount = rand.Next(9) + 1;
                    BuyProduct(product, amount);
                    DisplayPurchase(product, amount);
                }
                else
                {
                    var success = TrySellProduct(product);
                    DisplaySaleAttempt(success, product);
                }
            }
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} finished.");
        }

        private static void DisplaySaleAttempt(bool success, string product)
        {
            if (success) Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} sold {product}");
            else Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} couldn't sell {product}");
        }

        private static void DisplayPurchase(string product, int amount)
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} purchased {amount} items of {product}");
        }

        private static void PrintLeftovers()
        {
            Console.WriteLine();
            Console.WriteLine();
            foreach (var productBalance in _stockBalances)
            {
                Console.WriteLine($"{productBalance.Key}: {productBalance.Value}");
            }
        }
    }
}
