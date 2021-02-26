using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Data;

namespace Playground
{
    class Program
    {
        public static void Main(string[] args)
        {

            CreateAccounts(
                new Account { Id = 1, Name = "Jack", Balance = 1000 },
                new Account { Id = 2, Name = "Mike", Balance = 1000 },
                new Account { Id = 3, Name = "Paul", Balance = 1000 },
                new Account { Id = 4, Name = "Niky", Balance = 1000 },
                new Account { Id = 5, Name = "Jane", Balance = 1000 },
                new Account { Id = 6, Name = "Anthony", Balance = 1000 },
                new Account { Id = 7, Name = "Joe", Balance = 1000 },
                new Account { Id = 8, Name = "Smith", Balance = 1000 });

            var transferNumber = 10;
            var transferSize = 10;

            Console.WriteLine("READ COMMITED - WRITE LOCK");
            RunTestCase(1, 2, transferSize, IsolationLevel.ReadCommitted, true, transferNumber);
            PrintAccounts(1, 2);

            Console.WriteLine("READ COMMITED - NO WRITE LOCK");
            RunTestCase(3, 4, transferSize, IsolationLevel.ReadCommitted, false, transferNumber);
            PrintAccounts(3, 4);

            Console.WriteLine("REPEATABLE READ - WRITE LOCK");
            RunTestCase(5, 6, transferSize, IsolationLevel.RepeatableRead, true, transferNumber);
            PrintAccounts(5, 6);

            Console.WriteLine("REPEATABLE READ - NO WRITE LOCK");
            RunTestCase(7, 8, transferSize, IsolationLevel.RepeatableRead, false, transferNumber);
            PrintAccounts(7, 8);


            DeleteAccounts();

        }


        static void RunTestCase(int sourceAccountId, int destinationAccountId, int transferSize, IsolationLevel isolationLevel, bool writeLock, int transferNumber)
        {
            try
            {
                var tasks = new Task[transferNumber];
                for (var i = 0; i < transferNumber; i++)
                {
                    var task = Transfer(sourceAccountId, destinationAccountId, transferSize, isolationLevel, writeLock);
                    tasks[i] = task;
                }
                Task.WaitAll(tasks);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        static async Task Transfer(int sourceAccountId, int destinationAccountId, int transferSize, IsolationLevel isolationLevel, bool writeLock)
        {
            using (var db = new AppDbContext())
            {
                using(var transaction = db.Database.BeginTransaction(isolationLevel))
                {
                    string writeLockSuffix = "";
                    if (writeLock)
                        writeLockSuffix = "FOR UPDATE";

                    var accounts = await db.Accounts.FromSqlRaw(
                        $"SELECT * FROM accounts WHERE id = {sourceAccountId} OR id = {destinationAccountId} " + writeLockSuffix +"; ")
                        .ToListAsync();
                    if (accounts.Count == 2 && accounts.Any(a=>a.Id == sourceAccountId && a.Balance >= transferSize))
                    {

                        await db.Database.ExecuteSqlRawAsync(
                            $"UPDATE accounts SET balance = balance - {transferSize} WHERE id = {sourceAccountId}; "+
                            $"UPDATE accounts SET balance = balance + {transferSize} WHERE id = {destinationAccountId}; ");
                        
                    }

                    await transaction.CommitAsync();
                }
            }
        }

        static void CreateAccounts(params Account[] accounts)
        {
            using (var db = new AppDbContext())
            {
                db.AddRange(accounts);
                db.SaveChanges();
            }
        }

        static void PrintAccounts(int sourceAccountId, int destinationAccountId)
        {
            using (var db = new AppDbContext())
            {
                var accounts = db.Accounts.Where(a=>a.Id == sourceAccountId || a.Id == destinationAccountId).ToList();
                foreach (var account in accounts)
                {
                    Console.WriteLine("Id: " + account.Id + " || Name: " + account.Name + " || Balance: " + account.Balance);
                }
            }
        }

        static void DeleteAccounts()
        {
            using (var db = new AppDbContext())
            {
                var entityType = db.Model.FindEntityType(typeof(Account));
                var schema = entityType.GetSchema();
                var tableName = entityType.GetTableName();
                db.Database.ExecuteSqlRaw($"DELETE FROM {tableName}");
            }
        }

    }

}