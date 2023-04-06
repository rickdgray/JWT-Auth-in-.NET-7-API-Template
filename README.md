# JWT Auth in .NET 7 API Template

This repo is an example of how to set up a basic API with authorization and authentication using identities.

## Identity Entities

One of the key mistakes to make early on is to not inheret your application's `DbContext` from `IdentityDbContext`. It is a difficult change to make after your application is already established and when you inevitably need to implement identities, you are forced to either make a difficult database migration or, the more likely route, you simply set up a second `DbContext` specifically for identity. Avoiding this complication is as simple as inhereting from the `IdentityDbContext` from the beginning. There's no reason you cannot extend the user or role models either; you are free to add to them for things like user preferences.

## Disabling Cookie Auth

Another point of contention you may see around the web is the issue of trying to force webapi to use JWT tokens _exclusively_. It is surprisingly difficult to convince the server to not automatically attach cookies on the responses. You may find some [examples](https://wildermuth.com/2018/04/10/Using-JwtBearer-Authentication-in-an-API-only-ASP-NET-Core-Project/) [here](https://www.c-sharpcorner.com/article/authentication-and-authorization-in-asp-net-5-with-jwt-and-swagger/) [and](https://stackoverflow.com/questions/53970230/dont-use-cookies-in-token-based-authentication-asp-net-core) [there](https://stackoverflow.com/questions/46323844/net-core-2-0-web-api-using-jwt-adding-identity-breaks-the-jwt-authentication) [and](https://www.reddit.com/r/dotnet/comments/8flq9r/net_core_why_does_identity_always_creates_cookies/) [everywhere](https://github.com/aspnet/Identity/issues/1376) that all mention one of three things: either use `.AddIdentity()` and just ignore the cookie, use `.AddIdentity()` and then subsequently delete the cookie on every request 😒, or use `.AddIdentityCore()` and do some extra boilerplate in the setup. If you are like me, then accepting the existence of the entirely useless cookie is not an option. So then what's the problem with `AddIdentityCore()`? The problem with it is that the [SignInManager](https://github.com/openiddict/openiddict-core/issues/578) is not properly added to the DI container and therefore is not available. Not only this, but the standard `PasswordSignInAsync()` method actually _relies_ on the cookie in the requests. So how then do you handle password checks? Manually? Give me a break.

OK, __here's__ the solution: Check out the source code for [`AddIdentity()`](https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Core/src/IdentityServiceCollectionExtensions.cs#L38), [`AddIdentityCore()`](https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/IdentityServiceCollectionExtensions.cs#L33), and the relatively newer method called [`AddRoles()`](https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/IdentityBuilder.cs#L185). If you compare, you will find that with `AddIdentityCore()` and `AddRoles()`, there is almost complete parity. All that is missing is a couple lines specifically for cookie usage, and these two:

```
builder.Services.AddHttpContextAccessor();
builder.Services.TryAddScoped<SignInManager<AppUser>>();
```

So, you use `AddIdentityCore()` and `AddRoles()` and then you subsequently inject the `SignInManager` as well as it's dependency, `HttpContextAccessor`.

Oh! But there's one more catch. The `PasswordSignInAsync()` depends on the cookie, remember? The solution is impossible to find but it is very simple: you use `CheckPasswordSignInAsync()` instead. It depends on fetching the user object before calling, but works all the same.

Now, __finally__, we have true JWT only authentication in our API.