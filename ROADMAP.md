# Roadmap

- stopwatch to logging, instead of x-time-taken header

- http caching

- AzureBlobStatusRepository using append-blobs

- support for amazon s3, postgres, ms sql, mongo and other data stores

- sitesettings.csv or .json pr. siteId
  - Disable whole site
    - On - Expected functionality
    - Off - Site is down 

- Certificates
- OAuth providers

sitesettings.json
```json
{
  "state": "On|Off"
}
```