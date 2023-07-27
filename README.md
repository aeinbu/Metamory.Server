# Metamory.Server

## Development setup

### Set up ASP.NET Core SSL certificate for development.
At the command prompt, run:
```console
dotnet dev-certs https --trust
```

### To run the server
At the command prompt, run:
```console
dotnet run --project Metamory.WebApi
```

## Roles and claims used in the jwt-token

|Roles      |
|-----------|
|Editor     |
|Contributor|
|Reviewer   |

|Claims|Value        |
|------|-------------|
|SiteId|_the site id_|

To use a jwt token for testing, set the authentication header in the http request like this:
````
Authentication: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.ZMsp6M4XQs3STGSzT6YmaEfhaK9kXiXeQ7AJLHn57SY
````
Just remember to replace the dummy token used above :)

(In Postman, you will add this header to a request under the Authorization tab)

### jwt tokens for some users when testing in your local development environment


|Persona|Role                                              |JWT Bearer token|
|-------|--------------------------------------------------|----------------|
|alice  |Editor of _first-site_                            |eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFsaWNlIiwic3ViIjoiYWxpY2UiLCJqdGkiOiI5MWEwYmRjZSIsInJvbGUiOiJlZGl0b3IiLCJzaXRlSWQiOiJmaXJzdC1zaXRlIiwiYXVkIjpbImh0dHA6Ly9sb2NhbGhvc3Q6NDkzNDIiLCJodHRwczovL2xvY2FsaG9zdDo0NDM3MCIsImh0dHBzOi8vbG9jYWxob3N0OjUwMDEiLCJodHRwOi8vbG9jYWxob3N0OjUwMDAiXSwibmJmIjoxNjkwNDcwNjY3LCJleHAiOjE2OTg0MTk0NjcsImlhdCI6MTY5MDQ3MDY2NywiaXNzIjoiZG90bmV0LXVzZXItand0cyJ9.rn55WPoc_nWesrIpIZmTh19JEIRYPeDnnsjAWVLh9BA|
|robert |Editor of _second-site_                           |eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFsaWNlIiwic3ViIjoiYWxpY2UiLCJqdGkiOiI5MWEwYmRjZSIsInJvbGUiOiJlZGl0b3IiLCJzaXRlSWQiOiJmaXJzdC1zaXRlIiwiYXVkIjpbImh0dHA6Ly9sb2NhbGhvc3Q6NDkzNDIiLCJodHRwczovL2xvY2FsaG9zdDo0NDM3MCIsImh0dHBzOi8vbG9jYWxob3N0OjUwMDEiLCJodHRwOi8vbG9jYWxob3N0OjUwMDAiXSwibmJmIjoxNjkwNDcwNjY3LCJleHAiOjE2OTg0MTk0NjcsImlhdCI6MTY5MDQ3MDY2NywiaXNzIjoiZG90bmV0LXVzZXItand0cyJ9.rn55WPoc_nWesrIpIZmTh19JEIRYPeDnnsjAWVLh9BA|
|olivia |Contributor to both _first-site_ and _second-site_|eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFsaWNlIiwic3ViIjoiYWxpY2UiLCJqdGkiOiI5MWEwYmRjZSIsInJvbGUiOiJlZGl0b3IiLCJzaXRlSWQiOiJmaXJzdC1zaXRlIiwiYXVkIjpbImh0dHA6Ly9sb2NhbGhvc3Q6NDkzNDIiLCJodHRwczovL2xvY2FsaG9zdDo0NDM3MCIsImh0dHBzOi8vbG9jYWxob3N0OjUwMDEiLCJodHRwOi8vbG9jYWxob3N0OjUwMDAiXSwibmJmIjoxNjkwNDcwNjY3LCJleHAiOjE2OTg0MTk0NjcsImlhdCI6MTY5MDQ3MDY2NywiaXNzIjoiZG90bmV0LXVzZXItand0cyJ9.rn55WPoc_nWesrIpIZmTh19JEIRYPeDnnsjAWVLh9BA|
|jack   |Reviewer for _first-site_                         |eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImphY2siLCJzdWIiOiJqYWNrIiwianRpIjoiOTYxMjk5MjAiLCJyb2xlIjoicmV2aWV3ZXIiLCJzaXRlSWQiOiJmaXJzdC1zaXRlIiwiYXVkIjpbImh0dHA6Ly9sb2NhbGhvc3Q6NDkzNDIiLCJodHRwczovL2xvY2FsaG9zdDo0NDM3MCIsImh0dHBzOi8vbG9jYWxob3N0OjUwMDEiLCJodHRwOi8vbG9jYWxob3N0OjUwMDAiXSwibmJmIjoxNjkwNDcwNzE3LCJleHAiOjE2OTg0MTk1MTcsImlhdCI6MTY5MDQ3MDcxOCwiaXNzIjoiZG90bmV0LXVzZXItand0cyJ9.fNJL09DI5SW-4eKelFEuwgaI0Ju9CIJ-uYClwm-7CE4|

__If you open this in a development container, the below scripts have already been run when the container was created.__  
__If NOT, you can run the scripts below:__

Script to set up these pre-made personas into your development environments user-jwts store:
```console
dotnet user-secrets set Authentication:Schemes:Bearer:SigningKeys:0:Value PX674ghWi1nmjWJLcId0pbLPaZBRVZnBm6a4XyOZQ14=
cat artifacts/user-jwts.json > ~/.microsoft/usersecrets/c190885f-9527-40ab-b353-3b8539e29001/user-jwts.json
```
This is done because the `dotnet user-jwts create` tool isn't able to create tokens that have multiple claims with the same name, as Alice has (and which jwt allows for).  
Alice's token was thus hand-crafted using http://jwt.io and the base64 encoded secret key: PX674ghWi1nmjWJLcId0pbLPaZBRVZnBm6a4XyOZQ14=

You can create more users as shown in this example:
```console
dotnet user-jwts create --name john --role reviewer --claim siteId=second-site
```

## License and Copyright
This project is open sourced under the MIT Licence. See [LICENSE.txt](./LICENSE.txt) for details.  
Copyright (c) 2016-2023 Arjan Einbu
