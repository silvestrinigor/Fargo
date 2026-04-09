using Fargo.Sdk;

var sdk = new Engine();

await sdk.LogInAsync("localhost", "admin", "password123");

await sdk.LogOutAsync();

