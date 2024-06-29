using System;

using Solana.Unity.Rpc.Models;
using World.Program;

namespace Solana.Unity.Bolt.Examples
{
    public class BoltTest
    {
        public static void Main(string[] args)
        {
            var initializeComponentAccounts = new InitializeComponentAccounts() { };
            var createSessionIx = WorldProgram.InitializeComponent(initializeComponentAccounts);

            var transaction = new Transaction();
            transaction.Add(createSessionIx);
            Console.WriteLine("Transaction : " + transaction);
        }
    }
}