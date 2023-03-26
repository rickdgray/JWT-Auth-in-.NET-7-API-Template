# Template API for SQL with Auth

This repo is an example of how to set up a basic API with authorization and authentication using identities.

One of the key mistakes to make early on is to not inheret your application's `DbContext` from `IdentityDbContext`. It is a difficult change to make after your application is already established and when you inevitably need to implement auth, you are forced to either make a difficult database migration or, the more likely route, you simply set up a second `DbContext` specifically for identity. Avoiding this complication is as simple as inhereting from the `IdentityDbContext` from the beginning. There's no reason you cannot extend the user or role models either; you are free to add to them for things like user preferences.