# Changelog

## 2.0.0
### Added
- Added a maximum timespan for a restarted workflow
- Added workflow metrics and a sample dashboard for Dynatrace
- Added new configuration options to XRayClientSimulator 
- Added compression for serializing trace-details persisted in task states, improving performance and reducing I/O pressure
- Added semantic tests for mapping x-ray to OTLP
- Added basic tests for workflow functions

### Changes
 - Changed defaults in k8s manifests to better match a production ready scenario
 - Integrated purgehistory.yml in xrayconnector.yml
 - Upgraded XRayConnector project to .NET 10
 - Refactored Azure Functions framework using isolated-worker model and upgraded to latest versions of DTFx
 - Refactored AWS IAM using latest AWS SDK, offloading handling of temporal credentials to the SDK and using default credential chains.
 - Refactored workflow configurations using dependency injection
 - Changed handling of polling interval
 - Change various logs
 - Moved releasenotes into changelog.md

## 1.5.0
**IMPORTANT NOTE:** PLEASE UPDATE FROM PREVIOUS VERSIONS TO AVOID ISSUES IN HIGH LOAD SCENARIOS
### Added
- Add a load-simulator for XRayApi.
### Fixed
- Fixed a bug that can cause the workflow to unintentionally run away causing repeated executions.  

## 1.4.0
### Added
- Add new function endpoint and cronjob to purge database history

## 1.3.0
### Changes 
- Improve resilience 
    * Operate mssql as a statefulset
    * Improved logging of XRayCLient issues
  
  **BREAKING CHANGES** 
    * New YAML files for the mssql database. See updated instructions.  
    * Merged mssql-secrets.yml into connector-config.yml. Requires update of the xrayconnector.yml

## 1.2.0
### Added
- Update attribute mapping 
    * Incorporate xray segment error into span status 

## 1.1.0
### Added 
-  Add *TestSendSampleTrace* api to validate OTLP connectivity

## 1.0.0 
### Changes
- Switch to K8s deployment as the default option. 

  **BREAKING CHANGES** 
  * XRayConnector and XRayConnectorContainerized projects have been merged. To build the container image for K8s deployment, all references to *XRayConnectorContainerized* have been migrated to *XRayConnector*. Please also see the adapted instructions in **Deploy to K8s, Step 1**. 

## 0.11.0 
### Added 
- Add supoprt to automatically start the workflow
### Changes
  **BREAKING CHANGES**  
  - By default the workflow is now automatically started via a cronjob. Can be disabled via connector-config.yml, setting AutoStart to "False"

## 0.10.0 
### Added
- Added support for role assumption via AWS STS. 
- Added new config option to define polling interval in seconds.

## 0.9.0 
### Added
- Added a new project XRayConnectorContainerized +  manifest for k8s deployment

## 0.8.0 
### Added 
- Add mapping for SQS, SNS, DynamoDB and Links

## 0.6.0
### Added
- Add mappings for ApiGateway and Lambda

## 0.7.0
Initial release 
