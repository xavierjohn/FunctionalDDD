using AuthorizationExample;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║           Authorization Example — Trellis                   ║");
Console.WriteLine("║  Same domain rules, two execution models                    ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.WriteLine();

DirectServiceExample.Run();

Console.WriteLine();

await MediatorExample.RunAsync();

Console.WriteLine();
Console.WriteLine("Both approaches enforce the same domain rules:");
Console.WriteLine("  ✓ Resource-based auth (owner check via IAuthorizeResource)");
Console.WriteLine("  ✓ Permission-based auth (static check via IAuthorize)");
Console.WriteLine("  ✓ Input validation (IValidate)");
Console.WriteLine();
Console.WriteLine("The difference: CQRS separates authorization from business logic.");
Console.WriteLine("Handlers contain zero auth code — pipeline behaviors handle it.");
Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();