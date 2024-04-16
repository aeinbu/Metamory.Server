# Create the docker image


``` bash
cd /workspaces/Metamory.Server/Metamory.WebApi
dotnet publish --os linux --arch arm64 /t:PublishContainer -c Release -p ContainerImageTags='"latest;2024-04-09"'
dotnet publish --os linux --arch x64 /t:PublishContainer -c Release -p ContainerImageTags='"latest-x64;2024-04-09-x64"'

```

# Running the container
Preregquisites:
- Set up an authentication authority
    - //TODO:
    - https://auth0.com
    - set the environment variable Authentication__Schemes__Bearer__Authority to ...
    - can this be done with a local authority? dotnet user-jwts?
- Create a certificate for https called cert.pfx
    - mount the file location of the certificate at `/https` in the container
- Create a `accessControl.config.xml`
    - mount the file location of this file at `/authorization` in the container
- mount a volume or file location at `/data` in the container
- mount a volume or file location at `/data-protection-keys` in the container



# Run in docker
## Start Metamory server in demo mode
This example setup starts Metamory server without any authentication, authorization or ssl. It will create and use a docker volume named "metamory-demo-data". The base url is `http://localhost:5000/`

``` bash
docker run -d -p 5000:5000 -v metamory-demo-data:/data -v metamory-demo-data-protection-keys:/data-protection-keys` \
  -e NoAuth=true \
  --name metamory-demo-server aeinbu/metamory
```
__NOTE: When "NoAuth=true" is set, the container will not require authentication, but you should provide a role, either as a "role" http header or a "role" query string parameter. The following roles are supported.__
- role=editor
- role=contributor
- role=reviewer
- or don't supply role at all if you are just getting the current published content


## Start Metamory server in production mode
This setup starts Metamory server with authentication, authorization and ssl. The base url is `https://localhost:5001/`.
- You will need to create a certificate (`cert.pfx`) to use this setup.
- You will need to have an authentication authority to use this setup.
- You will need to create the `accessControl.config.xml` and place it in `<your-local-authorization-folder>` (See [this folder](https://github.com/aeinbu/Metamory.Server/tree/master/Metamory.WebApi) for samples)
- You'll need to replace `<your-local-pfx-folder>`, `<your-local-data-folder>`, `<your-local-authorization-folder>`, `<your-certificate-password>` and `<your-authentication-authority>`.

``` bash
docker run -d -p 5001:5001 -v <your-local-pfx-folder>:/https -v <your-local-data-folder>:/data -v <your-local-authorization-folder>:/authorization \
  -e CertificatePassword=<your-certificate-password> \
  -e Authentication__Schemes__Bearer__Authority=<your-authentication-authority> \
  --name metamory-server aeinbu/metamory
```



### Create the certificate

#macos & linux:
```bash
cd $HOME/https
openssl req -x509 -newkey rsa:4096 -sha256 -nodes -days 3650 -keyout cert.key -out cert.crt
openssl pkcs12 --export -inkey cert.key -in cert.crt -out cert.pfx
rm cert.crt cert.key
```
### Trust the certificate
#macos:
``` bash
sudo security add-trusted-cert -d -r trustAsRoot -k /Library/Keychains/System.keychain cert.pfx
```

## Provider settings
See:
- [FileProvider](./Metamory.Api/Providers/FileSystem/sample-provider-setting.json) for sample setup 
- [AzureStorageProvider](./Metamory.Api/Providers/AzureStorage/sample-provider-setting.json) for sample setup