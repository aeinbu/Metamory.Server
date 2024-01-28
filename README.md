# Metamory.Server

## Run the server in a docker container
Instructions are given at [aeinbu/metamory on docker hub](https://hub.docker.com/repository/docker/aeinbu/metamory/general).

## Development setup

You can set the `connectionString` for the azure storage account in user-secrets, like this:
```console
dotnet user-secrets -p Metamory.WebApi set "AzureStorageRepositoryConfiguration:connectionString" "<you connectionstring here>"
```

THe connectionsstring will look like this
```
DefaultEndpointsProtocol=https;AccountName=<accountnamae here>;AccountKey=<accountkey here>

```


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

```json
{
  "https://metamory.server/roles": [
    "editor"
  ],
  "https://metamory.server/siteId": "first-site",
  "permissions": [
    "first-site:editor",
    "second-site:contributor"
  ]
}

```
|https://metamory.server/roles|
|-----------|
|Editor     |
|Contributor|
|Reviewer   |

|Claims|Value        |
|------|-------------|
|https://metamory.server/siteId|_the site id_|

To use a jwt token for testing, set the authentication header in the http request like this:
````
Authentication: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.ZMsp6M4XQs3STGSzT6YmaEfhaK9kXiXeQ7AJLHn57SY
````
Just remember to replace the dummy token used above :)

(In Postman, you will add this header to a request under the Authorization tab)

## License and Copyright
This project is open sourced under the MIT Licence. See [LICENSE.txt](./LICENSE.txt) for details.  
Copyright (c) 2016-2024 Arjan Einbu
