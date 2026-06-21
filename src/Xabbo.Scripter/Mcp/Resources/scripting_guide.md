# Writing xabbo scripter scripts

Scripts are C# script files (.csx) made of top-level statements. Every public member of the
globals class 'G' is directly in scope, so you call the game API without any prefix.

## Essentials
- Calls are made directly: `Send(Out.Chat, "hello");`, `await Delay(500);`, `var room = Room;`.
- Most actions are async — use `await`. Long operations accept timeouts.
- `Ct` is the script's CancellationToken. Pass it to your own awaits and loops so the Stop
  button can cancel the script: `while (!Ct.IsCancellationRequested) { ... }`.
- A non-null value returned (or left as the last expression) is formatted into the log.
- Throwing an exception marks the script as faulted and prints the error + line to the log.

## Metadata directives (optional, as the first lines)
- `/// @name My Script`  sets the display name / tab title.
- `/// @group Utilities` groups the script in the script list.

## Discovering the API
- `list_api` returns every callable member signature (the full surface).
- `get_api` with a search term returns matching members with their documentation.
- `get_imports` lists the default usings and referenced assemblies available to scripts.

## Examples
```csharp
/// @name Wave
Wave();
```
```csharp
/// @name Greet room
foreach (var user in Users)
    Send(Out.Chat, $"Hello {user.Name}!");
```
```csharp
/// @name Walk loop
while (!Ct.IsCancellationRequested)
{
    await WalkTo(5, 5);
    await WalkTo(6, 6);
}
```
