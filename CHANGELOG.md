## [1.14.13](https://github.com/informatievlaanderen/building-registry/compare/v1.14.12...v1.14.13) (2019-12-18)


### Bug Fixes

* set the correct building geometry when importing multiple ([cf77f6a](https://github.com/informatievlaanderen/building-registry/commit/cf77f6a))

## [1.14.12](https://github.com/informatievlaanderen/building-registry/compare/v1.14.11...v1.14.12) (2019-12-17)


### Bug Fixes

* add versieid as string to WMS tables ([d5888da](https://github.com/informatievlaanderen/building-registry/commit/d5888da))

## [1.14.11](https://github.com/informatievlaanderen/building-registry/compare/v1.14.10...v1.14.11) (2019-12-17)


### Bug Fixes

* handle multipolygons in api ([67f052a](https://github.com/informatievlaanderen/building-registry/commit/67f052a))


### Performance Improvements

* add index on building for wms buildingunits ([667628b](https://github.com/informatievlaanderen/building-registry/commit/667628b))

## [1.14.10](https://github.com/informatievlaanderen/building-registry/compare/v1.14.9...v1.14.10) (2019-12-17)


### Bug Fixes

* correct unit projections when removing building ([9fa2cac](https://github.com/informatievlaanderen/building-registry/commit/9fa2cac))

## [1.14.9](https://github.com/informatievlaanderen/building-registry/compare/v1.14.8...v1.14.9) (2019-12-16)


### Bug Fixes

* cast version to correct offset string ([05c53a7](https://github.com/informatievlaanderen/building-registry/commit/05c53a7))

## [1.14.8](https://github.com/informatievlaanderen/building-registry/compare/v1.14.7...v1.14.8) (2019-12-15)


### Bug Fixes

* upgrade packages to extend import api timeout idempotency ([ceee046](https://github.com/informatievlaanderen/building-registry/commit/ceee046))

## [1.14.7](https://github.com/informatievlaanderen/building-registry/compare/v1.14.6...v1.14.7) (2019-12-13)


### Bug Fixes

* change version to string in wms views ([7dba307](https://github.com/informatievlaanderen/building-registry/commit/7dba307))
* drop views in correct order for down ([ba44c43](https://github.com/informatievlaanderen/building-registry/commit/ba44c43))
* filter out buildings without geometry ([ba40068](https://github.com/informatievlaanderen/building-registry/commit/ba40068))
* rework migrations to cleanly add WMS views ([cccea09](https://github.com/informatievlaanderen/building-registry/commit/cccea09))

## [1.14.6](https://github.com/informatievlaanderen/building-registry/compare/v1.14.5...v1.14.6) (2019-12-12)


### Performance Improvements

* add index on buildingunitdetail projection ([13ad305](https://github.com/informatievlaanderen/building-registry/commit/13ad305))

## [1.14.5](https://github.com/informatievlaanderen/building-registry/compare/v1.14.4...v1.14.5) (2019-12-10)


### Bug Fixes

* correct buildingunit namespace ([bc80c1b](https://github.com/informatievlaanderen/building-registry/commit/bc80c1b))

## [1.14.4](https://github.com/informatievlaanderen/building-registry/compare/v1.14.3...v1.14.4) (2019-12-10)


### Bug Fixes

* projection duplicate id check now in correct table ([c2c9450](https://github.com/informatievlaanderen/building-registry/commit/c2c9450))

## [1.14.3](https://github.com/informatievlaanderen/building-registry/compare/v1.14.2...v1.14.3) (2019-12-10)


### Bug Fixes

* duplicate and removed persistent id's can happen more than once ([cd4653f](https://github.com/informatievlaanderen/building-registry/commit/cd4653f))
* load spatial types in importer ([108c328](https://github.com/informatievlaanderen/building-registry/commit/108c328))
* multipolygons will not appear in wms and extract ([5313099](https://github.com/informatievlaanderen/building-registry/commit/5313099))
* projections getting units of building in memory before editing them ([d478243](https://github.com/informatievlaanderen/building-registry/commit/d478243))

## [1.14.2](https://github.com/informatievlaanderen/building-registry/compare/v1.14.1...v1.14.2) (2019-12-04)


### Bug Fixes

* add view to count list items building/buidingunit GR-852 ([4fcc9a7](https://github.com/informatievlaanderen/building-registry/commit/4fcc9a7))
* remove count from building sync GR-852 ([34570ad](https://github.com/informatievlaanderen/building-registry/commit/34570ad))

## [1.14.1](https://github.com/informatievlaanderen/building-registry/compare/v1.14.0...v1.14.1) (2019-11-29)

# [1.14.0](https://github.com/informatievlaanderen/building-registry/compare/v1.13.14...v1.14.0) (2019-11-20)


### Features

* add projections concerning removed/duplicated persistent local ids ([f389e5f](https://github.com/informatievlaanderen/building-registry/commit/f389e5f))

## [1.13.14](https://github.com/informatievlaanderen/building-registry/compare/v1.13.13...v1.13.14) (2019-11-20)


### Bug Fixes

* building import with geometry collections determine polygon ([c2bf0c9](https://github.com/informatievlaanderen/building-registry/commit/c2bf0c9))
* correct import bugs + add test case ([e7df518](https://github.com/informatievlaanderen/building-registry/commit/e7df518))
* correct importing multipolygon + history of unit attributes ([a704492](https://github.com/informatievlaanderen/building-registry/commit/a704492))
* correct retire building with units ([1714230](https://github.com/informatievlaanderen/building-registry/commit/1714230))
* dont log unnneeded warning GR-884 ([1912947](https://github.com/informatievlaanderen/building-registry/commit/1912947))
* extra fixes import building cra(b)(p) ([3736460](https://github.com/informatievlaanderen/building-registry/commit/3736460))
* function/point duplicate properties clean up ([ce802d9](https://github.com/informatievlaanderen/building-registry/commit/ce802d9))
* handle f'ed up legacy units + unusual crab behavior ([15d17d7](https://github.com/informatievlaanderen/building-registry/commit/15d17d7))
* import bug persistent id's odd case [#955842](https://github.com/informatievlaanderen/building-registry/issues/955842) ([8a67633](https://github.com/informatievlaanderen/building-registry/commit/8a67633))
* order units by id for building detail ([8371bf4](https://github.com/informatievlaanderen/building-registry/commit/8371bf4))
* projection when retire building also delete addresses with units ([f81c04f](https://github.com/informatievlaanderen/building-registry/commit/f81c04f))
* readdressing bug ([ffe6b54](https://github.com/informatievlaanderen/building-registry/commit/ffe6b54))
* rename oslo id to persistent local id in testclient ([9417de4](https://github.com/informatievlaanderen/building-registry/commit/9417de4))
* sometimes a new index is duplicate old oslo id's ([cbc2805](https://github.com/informatievlaanderen/building-registry/commit/cbc2805))

## [1.13.13](https://github.com/informatievlaanderen/building-registry/compare/v1.13.12...v1.13.13) (2019-10-25)


### Bug Fixes

* add setter for geometry to support docs ([bf27d64](https://github.com/informatievlaanderen/building-registry/commit/bf27d64))
* update some docs ([ddf9e4f](https://github.com/informatievlaanderen/building-registry/commit/ddf9e4f))

## [1.13.12](https://github.com/informatievlaanderen/building-registry/compare/v1.13.11...v1.13.12) (2019-10-25)


### Bug Fixes

* map point and function in response ([203c879](https://github.com/informatievlaanderen/building-registry/commit/203c879))

## [1.13.11](https://github.com/informatievlaanderen/building-registry/compare/v1.13.10...v1.13.11) (2019-10-24)


### Bug Fixes

* use proper gebouweenheidId ([42713dd](https://github.com/informatievlaanderen/building-registry/commit/42713dd))

## [1.13.10](https://github.com/informatievlaanderen/building-registry/compare/v1.13.9...v1.13.10) (2019-10-24)


### Bug Fixes

* push to correct repo ([8301bf1](https://github.com/informatievlaanderen/building-registry/commit/8301bf1))
* upgrade grar common ([686b16b](https://github.com/informatievlaanderen/building-registry/commit/686b16b))

## [1.13.9](https://github.com/informatievlaanderen/building-registry/compare/v1.13.8...v1.13.9) (2019-10-10)


### Bug Fixes

* correct import bugs + add test case ([7e7809b](https://github.com/informatievlaanderen/building-registry/commit/7e7809b))

## [1.13.8](https://github.com/informatievlaanderen/building-registry/compare/v1.13.7...v1.13.8) (2019-10-02)


### Bug Fixes

* sometimes a new index is duplicate old oslo id's ([a2f54fb](https://github.com/informatievlaanderen/building-registry/commit/a2f54fb))

## [1.13.7](https://github.com/informatievlaanderen/building-registry/compare/v1.13.6...v1.13.7) (2019-10-02)


### Bug Fixes

* import bug persistent id's odd case [#955842](https://github.com/informatievlaanderen/building-registry/issues/955842) ([af7aae7](https://github.com/informatievlaanderen/building-registry/commit/af7aae7))

## [1.13.6](https://github.com/informatievlaanderen/building-registry/compare/v1.13.5...v1.13.6) (2019-10-01)


### Bug Fixes

* when more than one duplicate unit is detected then create new id's ([9c1570e](https://github.com/informatievlaanderen/building-registry/commit/9c1570e))

## [1.13.5](https://github.com/informatievlaanderen/building-registry/compare/v1.13.4...v1.13.5) (2019-09-30)


### Bug Fixes

* check removed before completeness GR-900 ([7260c19](https://github.com/informatievlaanderen/building-registry/commit/7260c19))

## [1.13.4](https://github.com/informatievlaanderen/building-registry/compare/v1.13.3...v1.13.4) (2019-09-27)


### Bug Fixes

* deduplication when last index was duplicate ([85d09cf](https://github.com/informatievlaanderen/building-registry/commit/85d09cf))

## [1.13.3](https://github.com/informatievlaanderen/building-registry/compare/v1.13.2...v1.13.3) (2019-09-27)


### Bug Fixes

* readdressed housenumber for subaddress now retrieves id ([96fae1f](https://github.com/informatievlaanderen/building-registry/commit/96fae1f))

## [1.13.2](https://github.com/informatievlaanderen/building-registry/compare/v1.13.1...v1.13.2) (2019-09-26)


### Bug Fixes

* update legacy package ([fc1d1ce](https://github.com/informatievlaanderen/building-registry/commit/fc1d1ce))

## [1.13.1](https://github.com/informatievlaanderen/building-registry/compare/v1.13.0...v1.13.1) (2019-09-26)


### Bug Fixes

* update asset to fix importer ([db246c1](https://github.com/informatievlaanderen/building-registry/commit/db246c1))

# [1.13.0](https://github.com/informatievlaanderen/building-registry/compare/v1.12.4...v1.13.0) (2019-09-26)


### Features

* upgrade projector and resume projections on startup ([6a1d919](https://github.com/informatievlaanderen/building-registry/commit/6a1d919))

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
