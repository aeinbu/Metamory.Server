# Create the docker image

``` bash
cd /workspaces/Metamory.Server/Metamory.WebApi
dotnet publish --os linux --arch arm64 /t:PublishContainer -c Release
```

``` bash
dotnet dev-certs https -ep .aspnet/https/cert.pfx -p Pa$$w0rd
```
# Running the container
Preregquisites:
- Set up an authentication authority
    - https://auth0.com
    - set the environment variable Authentication__Schemes__Bearer__Authority to ...
    - can this be done with a local authority? dotnet user-jwts?
- Create a certificate for https called cert.pfx
    - mount the file location of the certificate at /https in the container
- mount a volume or file location at /data in the container


``` bash
docker run ...
-e ASPNETCORE_Kestrel__Certificates__Default__Password=pass
-e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/cert.pfx
-e Authentication__Schemes__Bearer__Authority=https://dev-se5kji3x7ce0r1mg.us.auth0.com/
-v <localpath>/https:/https
-v <localpath>/data:/data
-e NoAuth=<true|false>
...
```

__NOTE: When "NoAuth=true" is set, the container will not require authentication, but all URLs should use a query string for controlling the current role.__
- ?role=editor
- ?role=contributor
- ?role=reviewer
- or no querystring at all if you are just getting the current published content


#macos & linux:
```bash
openssl req \
    -x509 \
    -newkey rsa:4096 \
    -sha256 \
    -nodes \
    -days 3650 \
    -keyout cert.key \
    -out cert.crt \
&& \
openssl pkcs12 \
    -export \
    -inkey cert.key \
    -in cert.crt \
    -out cert.pfx \
&& \
rm cert.crt cert.key
```

#macos:
``` bash
sudo security add-trusted-cert -d -r trustAsRoot -k /Library/Keychains/System.keychain cert.pfx
```