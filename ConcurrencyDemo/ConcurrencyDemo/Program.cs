using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrencyDemo
{
    class Program
    {

        async Task DoSomethingAsync()
        {
            int val = 13;

            // Asynchronously wait 1 second.
            await Task.Delay(TimeSpan.FromSeconds(1));

            val *= 2;

            // Asynchronously wait 1 second.
            await Task.Delay(TimeSpan.FromSeconds(1));

            Trace.WriteLine(val);
        }

        private async Task<string> GetDataAsync()
        {
            // простой вызов асинхронного метода
            var result = await MyWebService.GetDataAsync();
            return result.ToString();
        }

        public ActionResult ActionAsync()
        {
            // DEADLOCK: это блокирует асинхронную задачу
            // которая ждёт, когда она сможет выполняться в этом контексте
            var data = GetDataAsync().Result;

            return View(data);
        }

        class ThreadSafe
        {
            static readonly object _locker = new object();
            static int _val1, _val2;

            static void Go()
            {
                lock (_locker)
                {
                    if (_val2 != 0) Console.WriteLine(_val1 / _val2);
                    _val2 = 0;
                }
            }
        }




        static void Main()
        {


            object locker1 = new object();
            object locker2 = new object();

            new Thread(() => {
                lock (locker1)
                {
                    Thread.Sleep(1000);
                    lock (locker2);      // Deadlock
                }
            }).Start();
            lock (locker2)
            {
                Thread.Sleep(1000);
                lock (locker1);         // Deadlock
            }



            FileStream fs = new FileStream(
                @"C:\windows\system32\autoexec.NT", 
                FileMode.Open, FileAccess.Read, 
                FileShare.Read, 1024, 
                FileOptions.Asynchronous);
            Byte[] data = new Byte[100];
            IAsyncResult ar = fs.BeginRead(data, 0, 
                data.Length, null, null);
            // другая работа
            Int32 bytesRead = fs.EndRead(ar);            
            fs.Close();







            var webClient = new WebClient();
            webClient.DownloadStringCompleted += 
                (_, e) =>
                {
                    // process result
                };
            webClient.DownloadStringAsync(
                new Uri("http://www.ya.ru"));




            Parallel.Invoke(
                () => new WebClient().DownloadFile("http://www.linqpad.net", "lp.html"),
                () => new WebClient().DownloadFile("http://www.jaoo.dk", "jaoo.html"));

            var wordsToTest = new List<string>();
            var dictionary = new HashSet<string>();

            var query = wordsToTest
              .AsParallel()
              .Select((word, index) => Tuple.Create((int)index, word))
              .Where(iword => !dictionary.Contains(iword.Item2))
              .OrderBy(iword => iword.Item1);







            var misspellings = new ConcurrentBag<Tuple<int, string>>();

            Parallel.ForEach(wordsToTest, (word, state, i) =>
            {
                if (!dictionary.Contains(word))
                    misspellings.Add(Tuple.Create((int)i, word));
            });








            //Task<string> task = Task.Factory.StartNew<string>
            //  (() => DownloadString("http://www.linqpad.net"));

            //RunSomeOtherMethod();

            //string result = task.Result;
        }

        static string DownloadString(string uri)
        {
            using (var wc = new System.Net.WebClient())
                return wc.DownloadString(uri);
        }



        //static void Main()
        //{
        //    Task.Factory.StartNew(Go);
        //}

        //static void Go()
        //{
        //    Console.WriteLine("Hello!");
        //}







        //static void Main()
        //{
        //    Thread t = new Thread(new ThreadStart(Go));

        //    t.Start();   // Run Go() on the new thread.
        //    Go();        // Simultaneously run Go() in the main thread.
        //}

        //static void Go()
        //{
        //    Console.WriteLine("hello!");
        //}










        static string[] _products = new string[] { "Lays", "Cheetos", "Pringles", "Estrella", "Русская картошка" };

        static Dictionary<string, int> _stockBalances = new Dictionary<string, int>();

        //static void Main(string[] args)
        //{
        //    foreach (var product in _products)
        //    {
        //        _stockBalances[product] = 0;
        //    }

        //    var worktime = TimeSpan.FromSeconds(2);
        //    Task t1 = Task.Run(() => DoWork(worktime));
        //    Task t2 = Task.Run(() => DoWork(worktime));
        //    Task t3 = Task.Run(() => DoWork(worktime));
        //    Task t4 = Task.Run(() => DoWork(worktime));

        //    Task.WaitAll(t1, t2, t3, t4);

        //    PrintLeftovers();
        //    Console.ReadKey();
        //}


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
