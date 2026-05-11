---
package: Trellis.Analyzers (applied form)
namespaces: [Trellis, Trellis.Analyzers]
types: [TRLS001, TRLS003, TRLS010, TRLS016, TRLS017, TRLS018, TRLS019]
related_docs: [trellis-api-analyzers.md, trellis-api-cookbook.md]
version: v3
last_verified: 2026-05-10
audience: [llm]
---
# Trellis Anti-Pattern → Fix Gallery

> A condensed atlas mapping each common Trellis analyzer trigger to its idiomatic fix. **Read this file alongside `trellis-api-cookbook.md` whenever you are touching a Trellis pipeline.** Each section's WRONG/FIX pair captures the canonical control-flow shape the analyzer expects — preserve that shape and adapt identifiers, types, and error values to your caller. The snippets are pattern examples, not drop-in replacements.

This file is the canonical reference for analyzer-triggered anti-patterns. It used to live as Recipe 11 in `trellis-api-cookbook.md` and was extracted so that:

1. It can be loaded independently when you are debugging an analyzer warning.
2. The reference list in `copilot-instructions.md` can name it directly, so AI sessions are more likely to load it.
3. The cookbook's Patterns Index can route by symptom into this file when the symptom is "I am getting `TRLSxxx`."

The analyzer rules themselves are documented in `trellis-api-analyzers.md` (severity, when they fire, suppression guidance). This file is the *applied* form — the snippets you adapt.

## TRLS001 — Result return value not handled

```csharp
// WRONG — Result<T> dropped on the floor
PlaceOrder(cmd);                                   // TRLS001

// FIX 1 — propagate up the ROP chain (preferred when the caller is itself in a Result pipeline).
return PlaceOrder(cmd).Map(_ => Unit.Value);

// FIX 2 — terminal side-effect via Switch (void-returning; for fire-and-forget log/metric).
PlaceOrder(cmd).Switch(
    onSuccess: _       => logger.LogInformation("Order placed."),
    onFailure: failure => logger.LogWarning("Order rejected: {Code}", failure.Code));

// FIX 3 — terminal projection via Match (both branches return a value; use when the
// caller needs an int/IActionResult/string back, not just a side effect).
int statusCode = PlaceOrder(cmd).Match(
    onSuccess: _       => 200,
    onFailure: failure => 422);
```

> Don't throw from inside `Match` / `Switch` to "handle" failure — it defeats the point of `Result<T>`. Use `Switch` for void side-effects and propagate the `Result` up the chain instead. (Note: TRLS010 only fires inside chain methods like `Bind`/`Map`/`Tap`/`Ensure` — not `Match` or `Switch` — so the analyzer won't catch this; it's a Result-discipline guideline, not an analyzer rule.)

## TRLS003 — Unsafe `Maybe.Value`

```csharp
// WRONG
string city = customer.Email.Value;                // TRLS003

// FIX 1 — guard
if (customer.Email.HasValue) { var v = customer.Email.Value; }

// FIX 2 — convert to Result
Result<EmailAddress> r = customer.Email.ToResult(new Error.NotFound(ResourceRef.For("Email", customer.Id)));
```

## TRLS010 — Throwing in a Result chain

```csharp
// WRONG
.Bind(o => throw new InvalidOperationException("bad"))   // TRLS010

// FIX
.Bind(o => Result.Fail<Order>(new Error.Conflict(ResourceRef.For<Order>(o.Id), "invalid_state")))
```

## TRLS016 — `HasIndex` on a `Maybe<T>` property

```csharp
// WRONG
b.HasIndex(c => c.Email);                          // TRLS016 — silently no-op

// FIX
b.HasTrellisIndex(c => new { c.Email });
```

## TRLS017 — Wrong attribute namespace on a value object

```csharp
// WRONG — System.ComponentModel.DataAnnotations
[System.ComponentModel.DataAnnotations.StringLength(10)]    // TRLS017 — generator ignores it
public sealed partial class CurrencyCode : RequiredString<CurrencyCode>;

// FIX
[Trellis.StringLength(10)]
public sealed partial class CurrencyCode : RequiredString<CurrencyCode>;
```

## TRLS018 — Unsafe `Result<T>` deconstruction

```csharp
// WRONG
var (ok, value, err) = result;
SendEmail(value);                                  // TRLS018 — value is default on failure

// FIX
var (ok, value, err) = result;
if (!ok) return err.ToHttpResponse();
SendEmail(value);                                  // gated by !ok early-return
```

## TRLS019 — `default(Result)` / `default(Maybe<T>)`

```csharp
// WRONG
return default;                                    // TRLS019 — typed FAILURE, not success
return default(Maybe<Email>);                      // TRLS019 — equivalent to .None but obscure

// FIX
return Result.Ok();
return Maybe<Email>.None;
```
