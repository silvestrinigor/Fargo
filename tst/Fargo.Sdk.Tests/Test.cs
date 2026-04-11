using Fargo.Sdk;

var engine = new Engine();

await engine.LogInAsync("https://localhost:7563", "admin", "HJLBaQLIcinDp6KrqRjjgQ@");

var article = await engine.Articles.CreateAsync("test article");

await article.UpdateAsync(a => a.Name = "other name");

//await engine.LogOutAsync();
