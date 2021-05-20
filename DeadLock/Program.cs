using System;
using System.Threading;

namespace DeadLock
{
    class Program
    {
        private static void DoTransactions(Thread t1, Thread t2)
        {
            t2.Start();
            t1.Start();
            
        }

        static void Main(string[] args)
        {
            Thread FirstTransactionThread = new Thread(DB.FirstTransaction);
            Thread SecondTransactionThread = new Thread(DB.SecondTransaction);
            DoTransactions(FirstTransactionThread, SecondTransactionThread);
            Console.ReadKey();
        }
    }
}
