# Roadmap

- Seperate server for managing vs. serving content?
- Same server, separate ports for managing vs. serving content?
  - Posssibility to run one or both in the same prosess?


- http caching

- AzureBlobStatusRepository using append-blobs

- support for amazon s3, postgres, ms sql, mongo and other data stores

- sitesettings.csv or .json pr. siteId
  - Disable whole site
    - On - Expected functionality
    - Off - Site is down
    - Readonly - Site is read-only

- Certificates
- OAuth providers

sitesettings.json
```json
{
  "state": "On|Off"
}
```