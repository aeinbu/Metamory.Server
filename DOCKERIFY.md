# How to create the container
```bash
cd Metamory.WebApi
dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer -c Release
```

# What needs to be configured:

## Common

## Local file system Provider
- data directory
- external TCP port
- authentication authority

```bash
docker run -dit -v ~/metamory-data:/data -p 5001:5001 -e "Authentication.Schemes.Bearer.Authority=..." --name metamory-server metamory:latest /bin/bash
```

docker-compose.yml
```yml
metamory:
  image: ...
  volumes:
   - ~/metamory-data:data
  ports:
   - 5001:5001
  environment:
   - Authentication.Schemes.Bearer.Authority: "..."
```

## Azure Storage Provider
- azure storage account name
- connectionString