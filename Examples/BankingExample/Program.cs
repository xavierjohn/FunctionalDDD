using BankingExample;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Run all banking examples
await BankingExamples.RunExamplesAsync();

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();