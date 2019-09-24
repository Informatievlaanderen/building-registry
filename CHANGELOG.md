## [1.12.4](https://github.com/informatievlaanderen/building-registry/compare/v1.12.3...v1.12.4) (2019-09-24)


### Bug Fixes

* importing OsloId's broke when id should be removed ([03b53bc](https://github.com/informatievlaanderen/building-registry/commit/03b53bc))

## [1.12.3](https://github.com/informatievlaanderen/building-registry/compare/v1.12.2...v1.12.3) (2019-09-23)


### Bug Fixes

* filter out building(units) without a persistent id ([20dd8db](https://github.com/informatievlaanderen/building-registry/commit/20dd8db))

## [1.12.2](https://github.com/informatievlaanderen/building-registry/compare/v1.12.1...v1.12.2) (2019-09-23)


### Bug Fixes

* tweak logging ([9e01824](https://github.com/informatievlaanderen/building-registry/commit/9e01824))
* tweak logging ([1c4df98](https://github.com/informatievlaanderen/building-registry/commit/1c4df98))

## [1.12.1](https://github.com/informatievlaanderen/building-registry/compare/v1.12.0...v1.12.1) (2019-09-23)


### Bug Fixes

* use ioptions for examples ([e83adde](https://github.com/informatievlaanderen/building-registry/commit/e83adde))

# [1.12.0](https://github.com/informatievlaanderen/building-registry/compare/v1.11.0...v1.12.0) (2019-09-20)


### Features

* remove EF.NTS ([b0db5f3](https://github.com/informatievlaanderen/building-registry/commit/b0db5f3))

# [1.11.0](https://github.com/informatievlaanderen/building-registry/compare/v1.10.3...v1.11.0) (2019-09-19)


### Features

* upgrade shaperon and NTS packages ([61a638f](https://github.com/informatievlaanderen/building-registry/commit/61a638f))

## [1.10.3](https://github.com/informatievlaanderen/building-registry/compare/v1.10.2...v1.10.3) (2019-09-17)


### Bug Fixes

* upgrade api for error headers ([6b23b79](https://github.com/informatievlaanderen/building-registry/commit/6b23b79))

## [1.10.2](https://github.com/informatievlaanderen/building-registry/compare/v1.10.1...v1.10.2) (2019-09-17)


### Bug Fixes

* use generic dbtraceconnection ([fc007d0](https://github.com/informatievlaanderen/building-registry/commit/fc007d0))

## [1.10.1](https://github.com/informatievlaanderen/building-registry/compare/v1.10.0...v1.10.1) (2019-09-13)


### Bug Fixes

* update redis lastchangedlist to log time of lasterror ([c98f19b](https://github.com/informatievlaanderen/building-registry/commit/c98f19b))

# [1.10.0](https://github.com/informatievlaanderen/building-registry/compare/v1.9.10...v1.10.0) (2019-09-12)


### Features

* keep track of how many times lastchanged has errored ([ac91c96](https://github.com/informatievlaanderen/building-registry/commit/ac91c96))

## [1.9.10](https://github.com/informatievlaanderen/building-registry/compare/v1.9.9...v1.9.10) (2019-09-06)


### Bug Fixes

* add tracing to legacycontext and use correct extractcontext ([727a779](https://github.com/informatievlaanderen/building-registry/commit/727a779))

## [1.9.9](https://github.com/informatievlaanderen/building-registry/compare/v1.9.8...v1.9.9) (2019-09-05)


### Bug Fixes

* initial jira version ([85e642e](https://github.com/informatievlaanderen/building-registry/commit/85e642e))

## [1.9.8](https://github.com/informatievlaanderen/building-registry/compare/v1.9.7...v1.9.8) (2019-09-05)


### Bug Fixes

* report correct version number ([25b3d35](https://github.com/informatievlaanderen/building-registry/commit/25b3d35))

## [1.9.7](https://github.com/informatievlaanderen/building-registry/compare/v1.9.6...v1.9.7) (2019-09-03)


### Bug Fixes

* fix projection schemas, problemdetails and fix build ([40fffcd](https://github.com/informatievlaanderen/building-registry/commit/40fffcd))
* update problemdetails for xml response GR-829 ([434422f](https://github.com/informatievlaanderen/building-registry/commit/434422f))
* use longer timeout for migrations ([5d1e4ef](https://github.com/informatievlaanderen/building-registry/commit/5d1e4ef))

## [1.9.6](https://github.com/informatievlaanderen/building-registry/compare/v1.9.5...v1.9.6) (2019-09-02)


### Bug Fixes

* do not log to console writeline ([4c9b236](https://github.com/informatievlaanderen/building-registry/commit/4c9b236))

## [1.9.5](https://github.com/informatievlaanderen/building-registry/compare/v1.9.4...v1.9.5) (2019-09-02)


### Bug Fixes

* properly report errors ([49b9656](https://github.com/informatievlaanderen/building-registry/commit/49b9656))

## [1.9.4](https://github.com/informatievlaanderen/building-registry/compare/v1.9.3...v1.9.4) (2019-08-27)


### Bug Fixes

* make datadog tracing check more for nulls ([f5b8848](https://github.com/informatievlaanderen/building-registry/commit/f5b8848))

## [1.9.3](https://github.com/informatievlaanderen/building-registry/compare/v1.9.2...v1.9.3) (2019-08-27)


### Bug Fixes

* use new desiredstate columns for projections ([28f7b0f](https://github.com/informatievlaanderen/building-registry/commit/28f7b0f))

## [1.9.2](https://github.com/informatievlaanderen/building-registry/compare/v1.9.1...v1.9.2) (2019-08-26)


### Bug Fixes

* use fixed datadog tracing ([e16c352](https://github.com/informatievlaanderen/building-registry/commit/e16c352))

## [1.9.1](https://github.com/informatievlaanderen/building-registry/compare/v1.9.0...v1.9.1) (2019-08-26)


### Bug Fixes

* fix swagger ([f81c57e](https://github.com/informatievlaanderen/building-registry/commit/f81c57e))

# [1.9.0](https://github.com/informatievlaanderen/building-registry/compare/v1.8.0...v1.9.0) (2019-08-26)


### Features

* bump to .net 2.2.6 ([6468bdf](https://github.com/informatievlaanderen/building-registry/commit/6468bdf))

# [1.8.0](https://github.com/informatievlaanderen/building-registry/compare/v1.7.0...v1.8.0) (2019-08-22)


### Features

* extract datavlaanderen namespace to settings ([abacc91](https://github.com/informatievlaanderen/building-registry/commit/abacc91))

# [1.7.0](https://github.com/informatievlaanderen/building-registry/compare/v1.6.0...v1.7.0) (2019-08-19)


### Features

* add wait for user input to importer ([c1385db](https://github.com/informatievlaanderen/building-registry/commit/c1385db))

# [1.6.0](https://github.com/informatievlaanderen/building-registry/compare/v1.5.0...v1.6.0) (2019-08-16)


### Features

* change NTS types to ByteArray in legacy to prevent memory leak ([455fa3a](https://github.com/informatievlaanderen/building-registry/commit/455fa3a))

# [1.5.0](https://github.com/informatievlaanderen/building-registry/compare/v1.4.5...v1.5.0) (2019-08-13)


### Features

* add missing event handlers where nothing was expected [#9](https://github.com/informatievlaanderen/building-registry/issues/9) ([a77aabe](https://github.com/informatievlaanderen/building-registry/commit/a77aabe))

## [1.4.5](https://github.com/informatievlaanderen/building-registry/compare/v1.4.4...v1.4.5) (2019-08-09)


### Bug Fixes

* fix container id in logging ([7c4c947](https://github.com/informatievlaanderen/building-registry/commit/7c4c947))

## [1.4.4](https://github.com/informatievlaanderen/building-registry/compare/v1.4.3...v1.4.4) (2019-07-17)


### Bug Fixes

* use serilog compact ([3a70557](https://github.com/informatievlaanderen/building-registry/commit/3a70557))

## [1.4.3](https://github.com/informatievlaanderen/building-registry/compare/v1.4.2...v1.4.3) (2019-07-17)


### Bug Fixes

* do not hardcode logging to console ([aecff22](https://github.com/informatievlaanderen/building-registry/commit/aecff22))
* do not hardcode logging to console ([7721371](https://github.com/informatievlaanderen/building-registry/commit/7721371))

## [1.4.2](https://github.com/informatievlaanderen/building-registry/compare/v1.4.1...v1.4.2) (2019-07-17)


### Bug Fixes

* push syndications to production ([f181e52](https://github.com/informatievlaanderen/building-registry/commit/f181e52))

## [1.4.1](https://github.com/informatievlaanderen/building-registry/compare/v1.4.0...v1.4.1) (2019-07-16)


### Bug Fixes

* minor style change to force build ([7473cd0](https://github.com/informatievlaanderen/building-registry/commit/7473cd0))

# [1.4.0](https://github.com/informatievlaanderen/building-registry/compare/v1.3.0...v1.4.0) (2019-07-15)


### Features

* prepare for deploy ([2fb1aed](https://github.com/informatievlaanderen/building-registry/commit/2fb1aed))
* prepare for deploy init.sh ([5d6bca3](https://github.com/informatievlaanderen/building-registry/commit/5d6bca3))

# [1.3.0](https://github.com/informatievlaanderen/building-registry/compare/v1.2.2...v1.3.0) (2019-07-11)


### Bug Fixes

* extract now treats removed buildings/units correctly ([2298b83](https://github.com/informatievlaanderen/building-registry/commit/2298b83))


### Features

* rename OsloId to PersistentLocalId ([434f2fd](https://github.com/informatievlaanderen/building-registry/commit/434f2fd))

## [1.2.2](https://github.com/informatievlaanderen/building-registry/compare/v1.2.1...v1.2.2) (2019-07-10)


### Bug Fixes

* don't persist processed keys ([e62c0b8](https://github.com/informatievlaanderen/building-registry/commit/e62c0b8))
* explicitly move all parameters to factory and pass no importoptions ([28a2e96](https://github.com/informatievlaanderen/building-registry/commit/28a2e96))
* generate sequence in parallel now works correctly ([2aa4aa6](https://github.com/informatievlaanderen/building-registry/commit/2aa4aa6))

## [1.2.1](https://github.com/informatievlaanderen/building-registry/compare/v1.2.0...v1.2.1) (2019-07-08)


### Bug Fixes

* log listing on port in output ([a5fa390](https://github.com/informatievlaanderen/building-registry/commit/a5fa390))

# [1.2.0](https://github.com/informatievlaanderen/building-registry/compare/v1.1.0...v1.2.0) (2019-06-26)


### Bug Fixes

* build test client ([a95893e](https://github.com/informatievlaanderen/building-registry/commit/a95893e))


### Features

* upgrade packages ([5f3dc89](https://github.com/informatievlaanderen/building-registry/commit/5f3dc89))

# [1.1.0](https://github.com/informatievlaanderen/building-registry/compare/v1.0.2...v1.1.0) (2019-06-20)


### Bug Fixes

* completion of building was not registered in all cases ([2788eef](https://github.com/informatievlaanderen/building-registry/commit/2788eef))


### Features

* add assets for importer ([a1898c5](https://github.com/informatievlaanderen/building-registry/commit/a1898c5))
* change sync response to be consistent with detail response ([521ac95](https://github.com/informatievlaanderen/building-registry/commit/521ac95))
* upgrade packages and fix api calls ([6b4abf1](https://github.com/informatievlaanderen/building-registry/commit/6b4abf1))

## [1.0.2](https://github.com/informatievlaanderen/building-registry/compare/v1.0.1...v1.0.2) (2019-06-17)

## [1.0.1](https://github.com/informatievlaanderen/building-registry/compare/v1.0.0...v1.0.1) (2019-06-17)


### Bug Fixes

* push projector package ([0b34f1a](https://github.com/informatievlaanderen/building-registry/commit/0b34f1a))

# 1.0.0 (2019-06-17)


### Bug Fixes

* fix plan -> reason changes ([89421bf](https://github.com/informatievlaanderen/building-registry/commit/89421bf))


### Features

* add dependencies ([e462640](https://github.com/informatievlaanderen/building-registry/commit/e462640))
* initial commit of registry ([e937a1d](https://github.com/informatievlaanderen/building-registry/commit/e937a1d))
* open source with EUPL-1.2 license as 'agentschap Informatie Vlaanderen ([aca8f83](https://github.com/informatievlaanderen/building-registry/commit/aca8f83))
