## [4.24.7](https://github.com/informatievlaanderen/building-registry/compare/v4.24.6...v4.24.7) (2025-01-29)


### Bug Fixes

* **wfswms:** remove buildingsv2 ([0a11d6c](https://github.com/informatievlaanderen/building-registry/commit/0a11d6c818212118b3d32118ed07874a59a8df1e))

## [4.24.6](https://github.com/informatievlaanderen/building-registry/compare/v4.24.5...v4.24.6) (2025-01-20)

## [4.24.5](https://github.com/informatievlaanderen/building-registry/compare/v4.24.4...v4.24.5) (2025-01-13)


### Bug Fixes

* **snapshot:** reenable reproducers ([6511334](https://github.com/informatievlaanderen/building-registry/commit/6511334c7f5f603e977aea66f4b801b6644f9a14))

## [4.24.4](https://github.com/informatievlaanderen/building-registry/compare/v4.24.3...v4.24.4) (2025-01-13)

## [4.24.3](https://github.com/informatievlaanderen/building-registry/compare/v4.24.2...v4.24.3) (2025-01-13)


### Bug Fixes

* **snapshot:** cleanup csproj ([71b8f0c](https://github.com/informatievlaanderen/building-registry/commit/71b8f0c1d4397634bee6c957e24325dcf9f5da58))

## [4.24.2](https://github.com/informatievlaanderen/building-registry/compare/v4.24.1...v4.24.2) (2025-01-10)


### Bug Fixes

* **snapshot:** disable reproducers ([09edc32](https://github.com/informatievlaanderen/building-registry/commit/09edc321f9cb2dda8bc89d003f94f678a74d66fb))

## [4.24.1](https://github.com/informatievlaanderen/building-registry/compare/v4.24.0...v4.24.1) (2025-01-10)

# [4.24.0](https://github.com/informatievlaanderen/building-registry/compare/v4.23.0...v4.24.0) (2025-01-07)


### Features

* **api:** add docs to feed GAWR-5321 ([4e48ab5](https://github.com/informatievlaanderen/building-registry/commit/4e48ab5c51b0296a2fd2992af6be55e8f59a9214))

# [4.23.0](https://github.com/informatievlaanderen/building-registry/compare/v4.22.1...v4.23.0) (2025-01-02)


### Features

* add cache introspection ([a1e92c2](https://github.com/informatievlaanderen/building-registry/commit/a1e92c240426781cb51e861036bfff04efbfedc4))

## [4.22.1](https://github.com/informatievlaanderen/building-registry/compare/v4.22.0...v4.22.1) (2024-12-05)


### Bug Fixes

* don't use CancellationToken for kafka message handler ([f6aa187](https://github.com/informatievlaanderen/building-registry/commit/f6aa1874dc6784da24f7c51bcd03a37791617295))

# [4.22.0](https://github.com/informatievlaanderen/building-registry/compare/v4.21.0...v4.22.0) (2024-12-04)


### Features

* configure consumer offset without deploy ([21b9669](https://github.com/informatievlaanderen/building-registry/commit/21b966946614b6fc19b902bbbecc52c13c083e36))
* switch wms/wfs views V3 GAWR-6685 ([892c5c1](https://github.com/informatievlaanderen/building-registry/commit/892c5c179f2356149aede5c32472c4d1ddafb90a))

# [4.21.0](https://github.com/informatievlaanderen/building-registry/compare/v4.20.0...v4.21.0) (2024-11-29)


### Features

* add snapshot on request ([4ffb970](https://github.com/informatievlaanderen/building-registry/commit/4ffb97024f31add59cb85d74a6c292580c5afd42))

# [4.20.0](https://github.com/informatievlaanderen/building-registry/compare/v4.19.1...v4.20.0) (2024-11-27)


### Bug Fixes

* add event-handlers + tests ([093c0ff](https://github.com/informatievlaanderen/building-registry/commit/093c0ff0fd8a0088b2ed5aaa9255ba6d247045b7))


### Features

* add buildingV3 for wms/wfs rebuild GAWR-6685 ([8f0cc9f](https://github.com/informatievlaanderen/building-registry/commit/8f0cc9f84a2f87d812e177f629d558ce91e46a9c))
* remove V1 tables of wms/wfs GAWR-6684 ([ceb4ba5](https://github.com/informatievlaanderen/building-registry/commit/ceb4ba5ac01551c137c99502a12426163e91c65a))

## [4.19.1](https://github.com/informatievlaanderen/building-registry/compare/v4.19.0...v4.19.1) (2024-11-19)


### Bug Fixes

* **docs:** hide legacy event markedasmigrated ([ca6c41f](https://github.com/informatievlaanderen/building-registry/commit/ca6c41f20fbae359e2746e6f179bc5731e8aed9a))

# [4.19.0](https://github.com/informatievlaanderen/building-registry/compare/v4.18.2...v4.19.0) (2024-11-19)


### Features

* bump event-handling + hide events ([aaa2600](https://github.com/informatievlaanderen/building-registry/commit/aaa2600c6e18ecd0931f5edc5f8dc69f9c4ba147))

## [4.18.2](https://github.com/informatievlaanderen/building-registry/compare/v4.18.1...v4.18.2) (2024-10-30)


### Bug Fixes

* **api:** parcelmatching now uses robustoverlay instead of intersects ([635c643](https://github.com/informatievlaanderen/building-registry/commit/635c64389766bad5f1165da700665527db1ff101))

## [4.18.1](https://github.com/informatievlaanderen/building-registry/compare/v4.18.0...v4.18.1) (2024-10-29)


### Bug Fixes

* snapshot command idempotency ([b48f525](https://github.com/informatievlaanderen/building-registry/commit/b48f5259743344e83cdfcbc8b349b113778957e0))

# [4.18.0](https://github.com/informatievlaanderen/building-registry/compare/v4.17.1...v4.18.0) (2024-10-29)


### Features

* add create building oslo snapshots ([a014af2](https://github.com/informatievlaanderen/building-registry/commit/a014af2349fdec0fb791920915b22fb28b9f3055))

## [4.17.1](https://github.com/informatievlaanderen/building-registry/compare/v4.17.0...v4.17.1) (2024-10-23)


### Bug Fixes

* **producer:** add logging + catch gone when reproducing ([f8bc7c6](https://github.com/informatievlaanderen/building-registry/commit/f8bc7c63b5227aef00da561595895a4b4a3e984c))

# [4.17.0](https://github.com/informatievlaanderen/building-registry/compare/v4.16.10...v4.17.0) (2024-10-21)


### Features

* add snapshot reproducer ([dc9be4d](https://github.com/informatievlaanderen/building-registry/commit/dc9be4d1aaa7d2b3c5a32af2efbeb8fb240cac1e))

## [4.16.10](https://github.com/informatievlaanderen/building-registry/compare/v4.16.9...v4.16.10) (2024-10-21)


### Bug Fixes

* building snapshot producer buildingunit status  events ([e515e97](https://github.com/informatievlaanderen/building-registry/commit/e515e9764ca510051732b3dc1b42326fbbdd4151))

## [4.16.9](https://github.com/informatievlaanderen/building-registry/compare/v4.16.8...v4.16.9) (2024-10-18)

## [4.16.8](https://github.com/informatievlaanderen/building-registry/compare/v4.16.7...v4.16.8) (2024-09-30)


### Bug Fixes

* keep trying to acquire lock in cache invalidator ([bc6a423](https://github.com/informatievlaanderen/building-registry/commit/bc6a423152352db6665663b174bf5cdd73e093e7))

## [4.16.7](https://github.com/informatievlaanderen/building-registry/compare/v4.16.6...v4.16.7) (2024-09-27)


### Bug Fixes

* batches in RedisCacheInvalidateService ([9c0114f](https://github.com/informatievlaanderen/building-registry/commit/9c0114f33fd636fd6c7f3d7d1130e3f0a52d9d93))

## [4.16.6](https://github.com/informatievlaanderen/building-registry/compare/v4.16.5...v4.16.6) (2024-09-27)


### Bug Fixes

* add logging to cache invalidator ([1af3e49](https://github.com/informatievlaanderen/building-registry/commit/1af3e498f4f16ac67206fd9514e8bfbebc45129b))

## [4.16.5](https://github.com/informatievlaanderen/building-registry/compare/v4.16.4...v4.16.5) (2024-09-27)


### Bug Fixes

* ci deploy lambda ([46e3806](https://github.com/informatievlaanderen/building-registry/commit/46e3806cbc0772fc3edfd7cbd91341a20789201e))
* retrieve RedisCacheKeyFormats ([b85cd53](https://github.com/informatievlaanderen/building-registry/commit/b85cd53a5209cbadb45e6ea329042557e02c27db))

## [4.16.4](https://github.com/informatievlaanderen/building-registry/compare/v4.16.3...v4.16.4) (2024-09-23)


### Bug Fixes

* style to trigger bump ([0c23087](https://github.com/informatievlaanderen/building-registry/commit/0c230876eb6814a0472d4871cd9064802aba558d))

## [4.16.3](https://github.com/informatievlaanderen/building-registry/compare/v4.16.2...v4.16.3) (2024-09-23)


### Bug Fixes

* style to trigger bump ([2c6a9b3](https://github.com/informatievlaanderen/building-registry/commit/2c6a9b3a0a14d6c9600abe83cf541f3dab4b0a23))

## [4.16.2](https://github.com/informatievlaanderen/building-registry/compare/v4.16.1...v4.16.2) (2024-09-23)


### Bug Fixes

* ci to bump + set cache invalidator as task ([d3d6cb1](https://github.com/informatievlaanderen/building-registry/commit/d3d6cb17a5ef632ee089caee4f346f83a642972b))

## [4.16.1](https://github.com/informatievlaanderen/building-registry/compare/v4.16.0...v4.16.1) (2024-09-19)


### Bug Fixes

* **cache:** building cachekey ([ba81de3](https://github.com/informatievlaanderen/building-registry/commit/ba81de3d8b3964e5781c1beef80c71bc678276a1))

# [4.16.0](https://github.com/informatievlaanderen/building-registry/compare/v4.15.1...v4.16.0) (2024-09-19)


### Bug Fixes

* revert remove measured buildings temporarily ([51867dd](https://github.com/informatievlaanderen/building-registry/commit/51867dda42e029c1d5b39b13c56b6f722900c67b))


### Features

* add building cache + invalidation by parcel ([d895aec](https://github.com/informatievlaanderen/building-registry/commit/d895aecaa0de43034b477c13c386984a55be5ecf))

## [4.15.1](https://github.com/informatievlaanderen/building-registry/compare/v4.15.0...v4.15.1) (2024-09-16)


### Bug Fixes

* remove measured buildings temporarily ([9dcea49](https://github.com/informatievlaanderen/building-registry/commit/9dcea490f3372f28f7dc18fd7a08d3429a9cde56))

# [4.15.0](https://github.com/informatievlaanderen/building-registry/compare/v4.14.1...v4.15.0) (2024-09-11)


### Features

* **integration:** remove buildingunit from version projection when removed ([9e7c94e](https://github.com/informatievlaanderen/building-registry/commit/9e7c94e86cc3828d0f3fe50f18a6eafb5d20655e))

## [4.14.1](https://github.com/informatievlaanderen/building-registry/compare/v4.14.0...v4.14.1) (2024-09-06)


### Bug Fixes

* **integration:** correct readdress projection ([6460075](https://github.com/informatievlaanderen/building-registry/commit/6460075e08a918b5c5e4e471ede127d5b54ecf7c))

# [4.14.0](https://github.com/informatievlaanderen/building-registry/compare/v4.13.0...v4.14.0) (2024-08-28)


### Features

* **consumer:** add offset as projection state to read consumer ([#1259](https://github.com/informatievlaanderen/building-registry/issues/1259)) ([376c624](https://github.com/informatievlaanderen/building-registry/commit/376c624e367961f77d4e137480fa63135c9c43d5))

# [4.13.0](https://github.com/informatievlaanderen/building-registry/compare/v4.12.7...v4.13.0) (2024-08-22)


### Features

* cleanup legacy api ([3a8084f](https://github.com/informatievlaanderen/building-registry/commit/3a8084fff55f6f992a6e8e65260e11df4ea62d52))

## [4.12.7](https://github.com/informatievlaanderen/building-registry/compare/v4.12.6...v4.12.7) (2024-08-22)


### Bug Fixes

* **integration:** idempotent link needs to check deleted state GAWR-6562 ([b83e830](https://github.com/informatievlaanderen/building-registry/commit/b83e830f333651190804bfcf70372b332b849adf))

## [4.12.6](https://github.com/informatievlaanderen/building-registry/compare/v4.12.5...v4.12.6) (2024-08-20)


### Bug Fixes

* currently realized common units need to be retired when notrealizing building GAWR-3426 ([4142c6d](https://github.com/informatievlaanderen/building-registry/commit/4142c6d586653269c8014f5ef6fef6b80244a087))

## [4.12.5](https://github.com/informatievlaanderen/building-registry/compare/v4.12.4...v4.12.5) (2024-08-19)


### Performance Improvements

* **integration:** add position composite indexes ([306cbda](https://github.com/informatievlaanderen/building-registry/commit/306cbdad096820ff60f004323ea8d0e74669c6df))

## [4.12.4](https://github.com/informatievlaanderen/building-registry/compare/v4.12.3...v4.12.4) (2024-08-19)


### Performance Improvements

* **integration:** try using split query for versions ([7856795](https://github.com/informatievlaanderen/building-registry/commit/78567951deb6579194dfb2c5e8806d765c94dc0a))

## [4.12.3](https://github.com/informatievlaanderen/building-registry/compare/v4.12.2...v4.12.3) (2024-08-19)


### Bug Fixes

* add longer timeout for integration db ([7e67d07](https://github.com/informatievlaanderen/building-registry/commit/7e67d07733b799081dd7f3b755e1a2e95a952674))
* revert "fix: add index for BuildingVersion.BuildingId" ([133b4cc](https://github.com/informatievlaanderen/building-registry/commit/133b4cc89d9f7f6a5ea13a13ec27c30b8c7e51fb))
* revert "fix: add migration" ([0e3190e](https://github.com/informatievlaanderen/building-registry/commit/0e3190ed51803b7f4ae4aa92da80e923fdf0bdee))

## [4.12.2](https://github.com/informatievlaanderen/building-registry/compare/v4.12.1...v4.12.2) (2024-08-06)


### Bug Fixes

* add index for BuildingVersion.BuildingId ([8f7971a](https://github.com/informatievlaanderen/building-registry/commit/8f7971a8465c5669256ea20bba41031435aee147))
* add migration ([d1a5ab7](https://github.com/informatievlaanderen/building-registry/commit/d1a5ab71df260ce5335786cf3149987462a66c89))

## [4.12.1](https://github.com/informatievlaanderen/building-registry/compare/v4.12.0...v4.12.1) (2024-07-30)


### Bug Fixes

* new address can be null for rejected or retired address because of municipality merger ([ac8e90c](https://github.com/informatievlaanderen/building-registry/commit/ac8e90cd27251642cc658a3f06f730bb6b648ae1))

# [4.12.0](https://github.com/informatievlaanderen/building-registry/compare/v4.11.0...v4.12.0) (2024-07-29)


### Features

* add snapshot status endpoint ([8055d1f](https://github.com/informatievlaanderen/building-registry/commit/8055d1f27dfa9e2445b366839328af67537ba1d5))
* blacklist ovocodes ([#1249](https://github.com/informatievlaanderen/building-registry/issues/1249)) ([b71d013](https://github.com/informatievlaanderen/building-registry/commit/b71d0135255cee00879fe5e40544bc9db53f6ff4))
* replace building unit address because of municipality merger ([bdb2507](https://github.com/informatievlaanderen/building-registry/commit/bdb2507d29db2c5c1cebe70c2b5535cf447c1a2d))

# [4.11.0](https://github.com/informatievlaanderen/building-registry/compare/v4.10.4...v4.11.0) (2024-07-22)


### Features

* correct removal building unit GAWR-4186 ([0cc9a96](https://github.com/informatievlaanderen/building-registry/commit/0cc9a9665ebf3345fb6fea3e4ddf212075f2e9e4))

## [4.10.4](https://github.com/informatievlaanderen/building-registry/compare/v4.10.3...v4.10.4) (2024-07-17)


### Bug Fixes

* add unused common building units in snapshot ([2cdc5f8](https://github.com/informatievlaanderen/building-registry/commit/2cdc5f84f49c1649f3c6e751ad87cff9089ce955))

## [4.10.3](https://github.com/informatievlaanderen/building-registry/compare/v4.10.2...v4.10.3) (2024-07-11)


### Bug Fixes

* **integration:** insert invalid pers id for addressid not found ([b6854bf](https://github.com/informatievlaanderen/building-registry/commit/b6854bf42c289fa8a72d39ff00d2e0350673f16b))

## [4.10.2](https://github.com/informatievlaanderen/building-registry/compare/v4.10.1...v4.10.2) (2024-07-05)

## [4.10.1](https://github.com/informatievlaanderen/building-registry/compare/v4.10.0...v4.10.1) (2024-06-28)


### Bug Fixes

* conditionally save changes ([95af855](https://github.com/informatievlaanderen/building-registry/commit/95af8557d0dbcce4a6a3fba33b3f5051fef154ae))

# [4.10.0](https://github.com/informatievlaanderen/building-registry/compare/v4.9.11...v4.10.0) (2024-06-28)


### Bug Fixes

* don't use user transaction, but use ef savechanges instead ([fdcbefd](https://github.com/informatievlaanderen/building-registry/commit/fdcbefd3600589a819de25ee9a549d7cd5b6c93a))


### Features

* add syndication to oslo api ([b03a7f3](https://github.com/informatievlaanderen/building-registry/commit/b03a7f31a1bd9ab08804cb6a9011df5ca941375f))

## [4.9.11](https://github.com/informatievlaanderen/building-registry/compare/v4.9.10...v4.9.11) (2024-06-27)


### Bug Fixes

* **integration:** add more efficient method retrievingpersistentlocalid ([c42cd8d](https://github.com/informatievlaanderen/building-registry/commit/c42cd8da9ef16723d2ba2e788c64697b45660aef))

## [4.9.10](https://github.com/informatievlaanderen/building-registry/compare/v4.9.9...v4.9.10) (2024-06-26)


### Bug Fixes

* correct previous fix for limit on sync ([5768306](https://github.com/informatievlaanderen/building-registry/commit/5768306bf7afdd461e1ca70860446c275041b66b))

## [4.9.9](https://github.com/informatievlaanderen/building-registry/compare/v4.9.8...v4.9.9) (2024-06-26)


### Bug Fixes

* limit sync response to max 100 items ([9764617](https://github.com/informatievlaanderen/building-registry/commit/9764617a14cf0af31ea46e4e22d7c9e39675a406))

## [4.9.8](https://github.com/informatievlaanderen/building-registry/compare/v4.9.7...v4.9.8) (2024-06-24)


### Bug Fixes

* EnsureCommonBuildingUnit when common building unit is retired ([adfc5fa](https://github.com/informatievlaanderen/building-registry/commit/adfc5fad88cb16f9170fba9a38f128ab9c826bf7))

## [4.9.7](https://github.com/informatievlaanderen/building-registry/compare/v4.9.6...v4.9.7) (2024-06-19)


### Bug Fixes

* **ci:** new deploy lambda tst+stg ([c88e309](https://github.com/informatievlaanderen/building-registry/commit/c88e309c21b128e9d623e351fa18f85af3bbe1ba))

## [4.9.6](https://github.com/informatievlaanderen/building-registry/compare/v4.9.5...v4.9.6) (2024-06-17)


### Bug Fixes

* include navigation properties when loading building version ([e30897e](https://github.com/informatievlaanderen/building-registry/commit/e30897e7d817e7aa0d2c6e5d7c3edfad9cc494f3))

## [4.9.5](https://github.com/informatievlaanderen/building-registry/compare/v4.9.4...v4.9.5) (2024-06-14)


### Bug Fixes

* check if entity is marked as deleted ([f666f67](https://github.com/informatievlaanderen/building-registry/commit/f666f67f78af1b9975577307cdd511fbb9c794ab))

## [4.9.4](https://github.com/informatievlaanderen/building-registry/compare/v4.9.3...v4.9.4) (2024-06-12)


### Bug Fixes

* add missing spatial index to existing migration ([d86494e](https://github.com/informatievlaanderen/building-registry/commit/d86494e922ffbb808621f8458cd7b16b0ad4e081))

## [4.9.3](https://github.com/informatievlaanderen/building-registry/compare/v4.9.2...v4.9.3) (2024-06-12)

## [4.9.2](https://github.com/informatievlaanderen/building-registry/compare/v4.9.1...v4.9.2) (2024-06-06)


### Bug Fixes

* add buildingwasmigrated to backoffice projections ([3041dfb](https://github.com/informatievlaanderen/building-registry/commit/3041dfb0ea3f48e631711864bba6538fbb873283))
* keep event provenance timestamp for new commands ([fbcfbbf](https://github.com/informatievlaanderen/building-registry/commit/fbcfbbf12c0acc08045a17c6fb42546e4213669a))
* use a buffer to calculate if building overlaps ([bf96a0e](https://github.com/informatievlaanderen/building-registry/commit/bf96a0e8a02bc04aa80e24d51b8f139af380b9e2))

## [4.9.1](https://github.com/informatievlaanderen/building-registry/compare/v4.9.0...v4.9.1) (2024-05-29)


### Bug Fixes

* remove all duplicate address links when detaching ([#1224](https://github.com/informatievlaanderen/building-registry/issues/1224)) ([b7f009d](https://github.com/informatievlaanderen/building-registry/commit/b7f009dcd40ee3618dfd8552af81e7066cd8c91c))

# [4.9.0](https://github.com/informatievlaanderen/building-registry/compare/v4.8.0...v4.9.0) (2024-05-29)


### Bug Fixes

* don't use db transaction in the backoffice projections ([b1d97cd](https://github.com/informatievlaanderen/building-registry/commit/b1d97cd037fb6026aee1a9a1fd128e83e446d063))


### Features

* add integration version projections from migration ([63f0577](https://github.com/informatievlaanderen/building-registry/commit/63f057743338f7f89b47b6390027e51e96c40222))

# [4.8.0](https://github.com/informatievlaanderen/building-registry/compare/v4.7.1...v4.8.0) (2024-05-28)


### Bug Fixes

* project BuildingUnitWasMovedIntoBuilding in wms ([#1219](https://github.com/informatievlaanderen/building-registry/issues/1219)) ([85422d2](https://github.com/informatievlaanderen/building-registry/commit/85422d220141322677833e0d0131911d1056f197))


### Features

* consume AddressRemovalWasCorrected + ParcelAddressesWereReaddressed ([d711301](https://github.com/informatievlaanderen/building-registry/commit/d711301308d18c68c9b37ae15d726f9ea25eb097))
* create oslo snapshots ([f95f934](https://github.com/informatievlaanderen/building-registry/commit/f95f9345d40966428b0cd6bbf218c5e16b58dd88))

## [4.7.1](https://github.com/informatievlaanderen/building-registry/compare/v4.7.0...v4.7.1) (2024-05-27)


### Bug Fixes

* drop ListCountViews ([5b2009e](https://github.com/informatievlaanderen/building-registry/commit/5b2009ea827453b4fc69747bb71256ec02715415))

# [4.7.0](https://github.com/informatievlaanderen/building-registry/compare/v4.6.0...v4.7.0) (2024-05-27)


### Features

* delete syndication project ([9a712cf](https://github.com/informatievlaanderen/building-registry/commit/9a712cf8762c21c7bea9487aaf547d69cd1ea223))

# [4.6.0](https://github.com/informatievlaanderen/building-registry/compare/v4.5.1...v4.6.0) (2024-05-23)


### Bug Fixes

* **ci:** add newstg pipeline ([ceeda26](https://github.com/informatievlaanderen/building-registry/commit/ceeda26db1363770e1a47aeff7139449037c9b5a))


### Features

* GAWR-6428 add readdress streetname ([8729c1c](https://github.com/informatievlaanderen/building-registry/commit/8729c1cf4d0806247a03579e09b005a41d1ca6cb))

## [4.5.1](https://github.com/informatievlaanderen/building-registry/compare/v4.5.0...v4.5.1) (2024-05-10)


### Bug Fixes

* get hash from unused common units ([4617b92](https://github.com/informatievlaanderen/building-registry/commit/4617b92b66d7304dda52904af80c2e07ec7cb3bf))

# [4.5.0](https://github.com/informatievlaanderen/building-registry/compare/v4.4.0...v4.5.0) (2024-05-10)


### Features

* allow unused common units to remove or detach address ([6ed8c05](https://github.com/informatievlaanderen/building-registry/commit/6ed8c0581548ce7e2b4802bbcbc55c79136cac64))

# [4.4.0](https://github.com/informatievlaanderen/building-registry/compare/v4.3.4...v4.4.0) (2024-04-29)


### Features

* add combined index isremoved and status on integration projections ([6d8696a](https://github.com/informatievlaanderen/building-registry/commit/6d8696afae8f58013c6737907135718ce405bfc0))

## [4.3.4](https://github.com/informatievlaanderen/building-registry/compare/v4.3.3...v4.3.4) (2024-04-29)


### Bug Fixes

* registry IBuildingGeometries implementation for Consumer.Address ([7784566](https://github.com/informatievlaanderen/building-registry/commit/77845669ac73c6965fcb8b12998459e7d691b183))

## [4.3.3](https://github.com/informatievlaanderen/building-registry/compare/v4.3.2...v4.3.3) (2024-04-25)


### Bug Fixes

* only validate on active, overlapping buildings ([a80a7f8](https://github.com/informatievlaanderen/building-registry/commit/a80a7f81cd0688f09bd080521823bf0cee3fc97a))

## [4.3.2](https://github.com/informatievlaanderen/building-registry/compare/v4.3.1...v4.3.2) (2024-04-23)


### Bug Fixes

* geometries context persistent local id name ([0ea828e](https://github.com/informatievlaanderen/building-registry/commit/0ea828e43b374b12840be0c28026333edd484ba4))

## [4.3.1](https://github.com/informatievlaanderen/building-registry/compare/v4.3.0...v4.3.1) (2024-04-23)


### Bug Fixes

* check overlap must be CCW oriented ([9278432](https://github.com/informatievlaanderen/building-registry/commit/9278432a5b9a45e9836aa905c6dcd5b3c61a9c78))

# [4.3.0](https://github.com/informatievlaanderen/building-registry/compare/v4.2.2...v4.3.0) (2024-04-23)


### Features

* add deviation to buildingunit WFS GAWR-5319 ([d9f2936](https://github.com/informatievlaanderen/building-registry/commit/d9f2936a07c9a69a096803b14a6a7439927c5a39))
* realize building validate overlapping building geometry GAWR-4815 ([aea4197](https://github.com/informatievlaanderen/building-registry/commit/aea4197eadb731c451de95f6744e72ca3ab563a5))

## [4.2.2](https://github.com/informatievlaanderen/building-registry/compare/v4.2.1...v4.2.2) (2024-04-16)


### Bug Fixes

* contract movebuildingunit ([d3189f1](https://github.com/informatievlaanderen/building-registry/commit/d3189f1e20b006ff47d597ed55dceb7cf726a9c8))

## [4.2.1](https://github.com/informatievlaanderen/building-registry/compare/v4.2.0...v4.2.1) (2024-04-15)


### Bug Fixes

* to trigger build add default setting in projector ([831745f](https://github.com/informatievlaanderen/building-registry/commit/831745f2a9b833f42319b1641979546ec0e92413))

# [4.2.0](https://github.com/informatievlaanderen/building-registry/compare/v4.1.10...v4.2.0) (2024-04-11)


### Features

* move BuildingUnit GAWR-5487 ([86295f9](https://github.com/informatievlaanderen/building-registry/commit/86295f96807d47f1c827c2e471c17e817d08bbbe))

## [4.1.10](https://github.com/informatievlaanderen/building-registry/compare/v4.1.9...v4.1.10) (2024-04-09)

## [4.1.9](https://github.com/informatievlaanderen/building-registry/compare/v4.1.8...v4.1.9) (2024-03-27)


### Bug Fixes

* consumeraddresscontext get addresses ([5995753](https://github.com/informatievlaanderen/building-registry/commit/5995753c13412e528bb4ee047d688d2e3da99d41))

## [4.1.8](https://github.com/informatievlaanderen/building-registry/compare/v4.1.7...v4.1.8) (2024-03-27)


### Bug Fixes

* retrieve addresses ([cce0a08](https://github.com/informatievlaanderen/building-registry/commit/cce0a0865d95bb7103fd1287d9bd7d76f79fbd2e))

## [4.1.7](https://github.com/informatievlaanderen/building-registry/compare/v4.1.6...v4.1.7) (2024-03-27)


### Bug Fixes

* lambda parcel config ([3565186](https://github.com/informatievlaanderen/building-registry/commit/35651867692ebbdbf517e66058acfb3365f31067))

## [4.1.6](https://github.com/informatievlaanderen/building-registry/compare/v4.1.5...v4.1.6) (2024-03-27)


### Bug Fixes

* add logging for realizeunplannedbuildingunit ([2783f12](https://github.com/informatievlaanderen/building-registry/commit/2783f12d91f5cd719b10913c8c59402ebc048083))
* mainbuildingtype realizeunplannedbuildingunit ([836e789](https://github.com/informatievlaanderen/building-registry/commit/836e7897a21ce7c5d7fcd8ab6ce6c5ff47e64f36))

## [4.1.5](https://github.com/informatievlaanderen/building-registry/compare/v4.1.4...v4.1.5) (2024-03-26)


### Bug Fixes

* ano organisation ([6989117](https://github.com/informatievlaanderen/building-registry/commit/69891172ebe702a3b4067372f00c96b8f538fa78))

## [4.1.4](https://github.com/informatievlaanderen/building-registry/compare/v4.1.3...v4.1.4) (2024-03-26)


### Bug Fixes

* registration of httpclient in lambda handlers ([cfa22bc](https://github.com/informatievlaanderen/building-registry/commit/cfa22bc85f0bbcedbc41cc52cfd4b6d0d9a7be76))

## [4.1.3](https://github.com/informatievlaanderen/building-registry/compare/v4.1.2...v4.1.3) (2024-03-26)


### Bug Fixes

* set message group id ([003ad1d](https://github.com/informatievlaanderen/building-registry/commit/003ad1dc1c605c49abddd8d988b852e48804ff31))

## [4.1.2](https://github.com/informatievlaanderen/building-registry/compare/v4.1.1...v4.1.2) (2024-03-25)


### Bug Fixes

* style to trigger build ([009ca32](https://github.com/informatievlaanderen/building-registry/commit/009ca329b3049c9928fe0167dd8ff054c4833310))

## [4.1.1](https://github.com/informatievlaanderen/building-registry/compare/v4.1.0...v4.1.1) (2024-03-25)


### Bug Fixes

* style to trigger build ([6b1a205](https://github.com/informatievlaanderen/building-registry/commit/6b1a205a42d62641dd7429d5ff9f71f38fb8982b))

# [4.1.0](https://github.com/informatievlaanderen/building-registry/compare/v4.0.0...v4.1.0) (2024-03-22)


### Bug Fixes

* sqs registration in lambda ([a24f926](https://github.com/informatievlaanderen/building-registry/commit/a24f92673ad7102f6d1ca65b6e16f52652f9879b))


### Features

* add idempotencekey ksql ([6edfa12](https://github.com/informatievlaanderen/building-registry/commit/6edfa1295422780a1acb4b7e5b888dc49fb91526))

# [4.0.0](https://github.com/informatievlaanderen/building-registry/compare/v3.108.0...v4.0.0) (2024-03-21)


### Features

* move to dotnet 8.0.2 ([8887980](https://github.com/informatievlaanderen/building-registry/commit/888798044e55886ce587832191a835dbe2f0d68f))


### BREAKING CHANGES

* move to dotnet 8.0.2

# [3.108.0](https://github.com/informatievlaanderen/building-registry/compare/v3.107.0...v3.108.0) (2024-03-20)


### Bug Fixes

* selftouching polygon ([e2d42ea](https://github.com/informatievlaanderen/building-registry/commit/e2d42ea6b51c6e6a76b2e03c92746288afc786a8))


### Features

* add toggles for anoapi + auto buildingunit creation ([4d0c6aa](https://github.com/informatievlaanderen/building-registry/commit/4d0c6aad9834c6d5897fcb61cb07f740cc1df806))

# [3.107.0](https://github.com/informatievlaanderen/building-registry/compare/v3.106.5...v3.107.0) (2024-03-18)


### Features

* notify outlined realized building GAWR-2327 ([246a485](https://github.com/informatievlaanderen/building-registry/commit/246a485eded375f2967db6ee861262993fa2f148))

## [3.106.5](https://github.com/informatievlaanderen/building-registry/compare/v3.106.4...v3.106.5) (2024-03-18)


### Bug Fixes

* add count to buildingunit address version ([cdb685c](https://github.com/informatievlaanderen/building-registry/commit/cdb685cb4047a37f111fcbba54d19e7b0a96bfde))

## [3.106.4](https://github.com/informatievlaanderen/building-registry/compare/v3.106.3...v3.106.4) (2024-03-15)


### Bug Fixes

* catching up size integration ([6944511](https://github.com/informatievlaanderen/building-registry/commit/6944511a7250351ef76351484179c2f7e1e0fef4))

## [3.106.3](https://github.com/informatievlaanderen/building-registry/compare/v3.106.2...v3.106.3) (2024-03-07)


### Bug Fixes

* building version readdress date as date ([8c476eb](https://github.com/informatievlaanderen/building-registry/commit/8c476eb5739cacb2bed563dadbfbdafe18d37387))
* column name readdress old date ([3cad671](https://github.com/informatievlaanderen/building-registry/commit/3cad671e6bcfc75f2815e8a6f521ee483a204299))

## [3.106.2](https://github.com/informatievlaanderen/building-registry/compare/v3.106.1...v3.106.2) (2024-03-06)


### Bug Fixes

* bump fix download artifact ([f518330](https://github.com/informatievlaanderen/building-registry/commit/f51833010da474a55b99479e2816fb92e152bf98))

## [3.106.1](https://github.com/informatievlaanderen/building-registry/compare/v3.106.0...v3.106.1) (2024-03-06)


### Bug Fixes

* include building unit versions in integration projections ([9f4af8f](https://github.com/informatievlaanderen/building-registry/commit/9f4af8fdf4aaa41bec1e82209bbe597b80780f2d))


### Performance Improvements

* eager load building unit versions in syndication projections ([9a5af97](https://github.com/informatievlaanderen/building-registry/commit/9a5af97226971625d8888b2ec6b4c8bd7b998be8))

# [3.106.0](https://github.com/informatievlaanderen/building-registry/compare/v3.105.2...v3.106.0) (2024-03-05)


### Features

* realize unplanned building unit GAWR-2809 ([f63a293](https://github.com/informatievlaanderen/building-registry/commit/f63a2938db467558645795e152eeedb908d15ffb))

## [3.105.2](https://github.com/informatievlaanderen/building-registry/compare/v3.105.1...v3.105.2) (2024-03-05)


### Bug Fixes

* add null check when assigning ID in integration version ([bf1aecb](https://github.com/informatievlaanderen/building-registry/commit/bf1aecbeedeef7099b576fc25a9758a295d93a44))

## [3.105.1](https://github.com/informatievlaanderen/building-registry/compare/v3.105.0...v3.105.1) (2024-03-05)


### Bug Fixes

* persistentlocalid finder ([e4d4642](https://github.com/informatievlaanderen/building-registry/commit/e4d464273def17cadd860d3ea66f382e2f8872cd))

# [3.105.0](https://github.com/informatievlaanderen/building-registry/compare/v3.104.0...v3.105.0) (2024-03-05)


### Bug Fixes

* addressrepository integration db ([4447dbb](https://github.com/informatievlaanderen/building-registry/commit/4447dbb103c6fe6c552bfcf4258921e734cfca61))


### Features

* add type to latest versions integration ([6d61778](https://github.com/informatievlaanderen/building-registry/commit/6d617785047f983a1ab6f5c66a17e4b7deb6515b))

# [3.104.0](https://github.com/informatievlaanderen/building-registry/compare/v3.103.0...v3.104.0) (2024-02-20)


### Features

* add ignore data check when setting offset ([d6db01c](https://github.com/informatievlaanderen/building-registry/commit/d6db01c36a390e185413ff72940d8381d25a2fbe))

# [3.103.0](https://github.com/informatievlaanderen/building-registry/compare/v3.102.0...v3.103.0) (2024-02-20)


### Features

* shutdown backoffice projections service when projection is stopped ([7c83c95](https://github.com/informatievlaanderen/building-registry/commit/7c83c95f9e0cd8b9bfda05541e8e78808819c307))

# [3.102.0](https://github.com/informatievlaanderen/building-registry/compare/v3.101.1...v3.102.0) (2024-02-16)


### Features

* add building polygon projection ESRI format ([28ad353](https://github.com/informatievlaanderen/building-registry/commit/28ad353153cada964049e2d97313bf5b46ededbb))

## [3.101.1](https://github.com/informatievlaanderen/building-registry/compare/v3.101.0...v3.101.1) (2024-02-13)


### Bug Fixes

* remove startup ([83fbcfe](https://github.com/informatievlaanderen/building-registry/commit/83fbcfe899a2cecff775481459b85b16b9ba6e27))

# [3.101.0](https://github.com/informatievlaanderen/building-registry/compare/v3.100.6...v3.101.0) (2024-02-13)


### Features

* add lastchangedlist console ([60c318b](https://github.com/informatievlaanderen/building-registry/commit/60c318b4cc2e2efc0fae25c17cbd6affb5765154))
* add number of records to status cache ([cefce1a](https://github.com/informatievlaanderen/building-registry/commit/cefce1a91faef127566955b38e1fd21cb529fa32))

## [3.100.6](https://github.com/informatievlaanderen/building-registry/compare/v3.100.5...v3.100.6) (2024-02-13)


### Bug Fixes

* bump projection-handling + projector ([a42d750](https://github.com/informatievlaanderen/building-registry/commit/a42d7501c505436203dc699438d27f9cfa6c9ad1))

## [3.100.5](https://github.com/informatievlaanderen/building-registry/compare/v3.100.4...v3.100.5) (2024-02-12)


### Bug Fixes

* try projector fix ([7933da0](https://github.com/informatievlaanderen/building-registry/commit/7933da0e6c0b82ff40548ad38733318a21966d4e))

## [3.100.4](https://github.com/informatievlaanderen/building-registry/compare/v3.100.3...v3.100.4) (2024-02-08)


### Bug Fixes

* uri producer projections response ([a4cc994](https://github.com/informatievlaanderen/building-registry/commit/a4cc9947ad65ce3a63643098ab2c99fc7e6cb443))

## [3.100.3](https://github.com/informatievlaanderen/building-registry/compare/v3.100.2...v3.100.3) (2024-02-08)

## [3.100.2](https://github.com/informatievlaanderen/building-registry/compare/v3.100.1...v3.100.2) (2024-02-08)


### Bug Fixes

* json output producer projection status ([adaf256](https://github.com/informatievlaanderen/building-registry/commit/adaf256e192f4c128be2aa10a2e72974cefc6f9b))

## [3.100.1](https://github.com/informatievlaanderen/building-registry/compare/v3.100.0...v3.100.1) (2024-02-08)


### Bug Fixes

* producer port ([9531462](https://github.com/informatievlaanderen/building-registry/commit/9531462c8f94baf75b3524bf165349f39a1f8980))
* remove listen ip port ([772f7c3](https://github.com/informatievlaanderen/building-registry/commit/772f7c3653892b15fba99b4148e8041e2fe9236a))

# [3.100.0](https://github.com/informatievlaanderen/building-registry/compare/v3.99.1...v3.100.0) (2024-02-07)


### Bug Fixes

* bump projector + projectionhandling ([ca8e8b9](https://github.com/informatievlaanderen/building-registry/commit/ca8e8b9543e2f9cd155ba7f4bddecfc90f08ba4b))


### Features

* add backoffice projections status ([b608ce3](https://github.com/informatievlaanderen/building-registry/commit/b608ce34250e22212a5418768cb7f1d6a68914da))
* add integration database version projections ([d528f5d](https://github.com/informatievlaanderen/building-registry/commit/d528f5d29ceb130e198164eef54b26d4ff009799))
* disable merge building with all dependencies GAWR-5417 ([93fa2fb](https://github.com/informatievlaanderen/building-registry/commit/93fa2fb945e3e60351b34f9c2d20131923103b27))

## [3.99.1](https://github.com/informatievlaanderen/building-registry/compare/v3.99.0...v3.99.1) (2024-02-06)


### Bug Fixes

* bump projectionhandling ([554122c](https://github.com/informatievlaanderen/building-registry/commit/554122c3197c9b84ea1f4e72317d0bb674836e2d))

# [3.99.0](https://github.com/informatievlaanderen/building-registry/compare/v3.98.1...v3.99.0) (2024-02-02)


### Bug Fixes

* add objectid to idempotencekey snapshot oslo ([78a9a15](https://github.com/informatievlaanderen/building-registry/commit/78a9a155e113b6d5bc29e9febe18328ee6c3f428))


### Features

* add cachevalidator lastchangedlist GAWR-5407 ([3037a11](https://github.com/informatievlaanderen/building-registry/commit/3037a1179343276727680530f4a9d1f4db3a44ae))

## [3.98.1](https://github.com/informatievlaanderen/building-registry/compare/v3.98.0...v3.98.1) (2024-01-25)


### Bug Fixes

* push images to test ci + bump ([190a65e](https://github.com/informatievlaanderen/building-registry/commit/190a65e2d2d3033b847e8cd603eba0cf9f74b417))

# [3.98.0](https://github.com/informatievlaanderen/building-registry/compare/v3.97.1...v3.98.0) (2024-01-25)


### Bug Fixes

* add new test ci + bump ([464766b](https://github.com/informatievlaanderen/building-registry/commit/464766bbd8fb0c9eb35e3b92e00348be348794a4))
* Integration database Oslo column values ([0dd5c58](https://github.com/informatievlaanderen/building-registry/commit/0dd5c58b5d5c721fd7dd1067e2a23e97079d7b3a))
* rename PuriId to Puri ([f782fde](https://github.com/informatievlaanderen/building-registry/commit/f782fdebf09bd36596b2d4a536e2f32e39c1f8cc))


### Features

* add integration database latest item projections ([832381a](https://github.com/informatievlaanderen/building-registry/commit/832381af25cb9dcefca74d8834beb743727f3b14))

## [3.97.1](https://github.com/informatievlaanderen/building-registry/compare/v3.97.0...v3.97.1) (2024-01-09)


### Bug Fixes

* command provenance should be set after idempotency check ([63aeceb](https://github.com/informatievlaanderen/building-registry/commit/63aecebac0f2fad80c8cf237e463b5b99ee5d8eb))
* event timestamp ([167f0d7](https://github.com/informatievlaanderen/building-registry/commit/167f0d77a1794c857c20dffba46f3b01bbdf3713))

# [3.97.0](https://github.com/informatievlaanderen/building-registry/compare/v3.96.3...v3.97.0) (2023-12-04)


### Bug Fixes

* multiple common building units in BuildingWasMigrated ([c97c617](https://github.com/informatievlaanderen/building-registry/commit/c97c617420836c2e73d46b6679729277cd59180c))


### Features

* add integrationdb ksql scripts ([2517d4c](https://github.com/informatievlaanderen/building-registry/commit/2517d4c2589baade4aa24bb277e9d179f4d0ca7d))

## [3.96.3](https://github.com/informatievlaanderen/building-registry/compare/v3.96.2...v3.96.3) (2023-11-22)


### Bug Fixes

* sysgeometry projecitons ([a3f6143](https://github.com/informatievlaanderen/building-registry/commit/a3f6143ee3407cadbb220b0474967e4156d65603))

## [3.96.2](https://github.com/informatievlaanderen/building-registry/compare/v3.96.1...v3.96.2) (2023-11-10)


### Bug Fixes

* allow cors for producer ([4a5ad24](https://github.com/informatievlaanderen/building-registry/commit/4a5ad24f49df1983bf8e2fe9cf010d77909cc0a1))

## [3.96.1](https://github.com/informatievlaanderen/building-registry/compare/v3.96.0...v3.96.1) (2023-11-10)

# [3.96.0](https://github.com/informatievlaanderen/building-registry/compare/v3.95.0...v3.96.0) (2023-11-08)


### Features

* switch wms views GAWR-4041 ([1a2beba](https://github.com/informatievlaanderen/building-registry/commit/1a2bebaa25270eb52ccb391fce6f48fce8cd991d))

# [3.95.0](https://github.com/informatievlaanderen/building-registry/compare/v3.94.0...v3.95.0) (2023-10-19)


### Features

* add building matching ([6602555](https://github.com/informatievlaanderen/building-registry/commit/66025550d119c26caee15ba7a6c91e6699de6846))

# [3.94.0](https://github.com/informatievlaanderen/building-registry/compare/v3.93.3...v3.94.0) (2023-10-16)


### Features

* add sysgeometry to buildingv2 projections ([1e5a4ce](https://github.com/informatievlaanderen/building-registry/commit/1e5a4cee506458d2e136a8c727dcc83fafcedc83))

## [3.93.3](https://github.com/informatievlaanderen/building-registry/compare/v3.93.2...v3.93.3) (2023-10-13)


### Bug Fixes

* drop & recreate index for alter column ([7ea1805](https://github.com/informatievlaanderen/building-registry/commit/7ea1805c8356a1526e03b426e435b1358c1fbd69))

## [3.93.2](https://github.com/informatievlaanderen/building-registry/compare/v3.93.1...v3.93.2) (2023-10-13)


### Bug Fixes

* legacy filters ([8a39302](https://github.com/informatievlaanderen/building-registry/commit/8a39302ae40508fe0149769fec9db85f0f5c7019))

## [3.93.1](https://github.com/informatievlaanderen/building-registry/compare/v3.93.0...v3.93.1) (2023-10-12)


### Bug Fixes

* include persistentlocalid in command hash ([928cc1a](https://github.com/informatievlaanderen/building-registry/commit/928cc1a2e9b17277957f9499c99d9f9146582303))

# [3.93.0](https://github.com/informatievlaanderen/building-registry/compare/v3.92.2...v3.93.0) (2023-10-12)


### Features

* add WFS views ([c72334e](https://github.com/informatievlaanderen/building-registry/commit/c72334e6b588c54995cb84cfa720bffb964be81a))

## [3.92.2](https://github.com/informatievlaanderen/building-registry/compare/v3.92.1...v3.92.2) (2023-10-11)


### Performance Improvements

* add indexes on legacy projections ([cf38821](https://github.com/informatievlaanderen/building-registry/commit/cf38821e43341b9edb7fa221dd08a702615fcc6e))
* only select required columns in oslo ([40fe6e2](https://github.com/informatievlaanderen/building-registry/commit/40fe6e23bf91489390ce2632f00a69c5eb89cb25))

## [3.92.1](https://github.com/informatievlaanderen/building-registry/compare/v3.92.0...v3.92.1) (2023-10-11)


### Bug Fixes

* filter on string instead of enum on detailv2 list ([17a94e0](https://github.com/informatievlaanderen/building-registry/commit/17a94e021d845b81fe7b51a137cbbcde93e28c0d))

# [3.92.0](https://github.com/informatievlaanderen/building-registry/compare/v3.91.1...v3.92.0) (2023-10-06)


### Bug Fixes

* gebouweenheidfunctie filter should be in Dutch ([af15d15](https://github.com/informatievlaanderen/building-registry/commit/af15d15135c5a4faa1c8d08170f621399e79925c))
* grb buildingunit stream ([8892dd2](https://github.com/informatievlaanderen/building-registry/commit/8892dd2c4f817601a7be41cb9f6bd777c7944bae))


### Features

* gebouweenheidfunctie filter ([d7d1eff](https://github.com/informatievlaanderen/building-registry/commit/d7d1effc7dd2b354fa53d3d74b7fc5535b03971d))

## [3.91.1](https://github.com/informatievlaanderen/building-registry/compare/v3.91.0...v3.91.1) (2023-10-04)


### Bug Fixes

* GebouwDetailGebouweenheid Status should be enum ([5a5337e](https://github.com/informatievlaanderen/building-registry/commit/5a5337e13e93817c2677c413efe3bda0b3d6ab52))

# [3.91.0](https://github.com/informatievlaanderen/building-registry/compare/v3.90.1...v3.91.0) (2023-10-04)


### Features

* add building filter on list building units ([2233832](https://github.com/informatievlaanderen/building-registry/commit/2233832a15ee21275eec1810aeaca130b7100243))

## [3.90.1](https://github.com/informatievlaanderen/building-registry/compare/v3.90.0...v3.90.1) (2023-10-04)


### Bug Fixes

* add snapshot verifier release ([406cdec](https://github.com/informatievlaanderen/building-registry/commit/406cdec1379c7e9f415380b24f2e28a04664e93c))
* build.fsx typo ([6dd437c](https://github.com/informatievlaanderen/building-registry/commit/6dd437cb0749462f81aeb63a5bb29130a125a584))
* status to building bu detail in read api ([e4e89a3](https://github.com/informatievlaanderen/building-registry/commit/e4e89a3e57eb9d8bc2773d8e64aceea2b48a5131))

# [3.90.0](https://github.com/informatievlaanderen/building-registry/compare/v3.89.2...v3.90.0) (2023-10-03)


### Features

* add status to building bu detail in read api GAWR-672 ([f1a25ff](https://github.com/informatievlaanderen/building-registry/commit/f1a25ff33bde0a05192f29de3db00b520cbf54dc))

## [3.89.2](https://github.com/informatievlaanderen/building-registry/compare/v3.89.1...v3.89.2) (2023-09-29)


### Bug Fixes

* set migrator bigbuilding threshold to 27000 ([11f005e](https://github.com/informatievlaanderen/building-registry/commit/11f005edba840ed5936f3b433a7c692251800cb4))

## [3.89.1](https://github.com/informatievlaanderen/building-registry/compare/v3.89.0...v3.89.1) (2023-09-18)


### Bug Fixes

* only match realized parcels when parcelMatching ([1cde393](https://github.com/informatievlaanderen/building-registry/commit/1cde39388613e7c65a0ce3b126f62c9271b70a12))

# [3.89.0](https://github.com/informatievlaanderen/building-registry/compare/v3.88.0...v3.89.0) (2023-09-15)


### Features

* add consumer parcel matching to legacy api ([74b884c](https://github.com/informatievlaanderen/building-registry/commit/74b884c2e9b4923f5f030f5b483a227a67437b62))
* add parcel matching from consumer ([2107089](https://github.com/informatievlaanderen/building-registry/commit/2107089c4fb4f95a990c97410883efdc61d1bde3))

# [3.88.0](https://github.com/informatievlaanderen/building-registry/compare/v3.87.0...v3.88.0) (2023-09-14)


### Bug Fixes

* remove sequences from snapshot verifier ([c785b0b](https://github.com/informatievlaanderen/building-registry/commit/c785b0bb9edf3e413f5d690bba2dd85d40391b97))
* snapshot verifier compare building units by persistent local id ([b800b23](https://github.com/informatievlaanderen/building-registry/commit/b800b23d2c4cb3e93b9a1a38cc34289daa9306c1))


### Features

* add snapshot verifier ([e03ef30](https://github.com/informatievlaanderen/building-registry/commit/e03ef303197a76f6f2f7883d60096be37977367c))
* consume parcel geometry & addresses ([df8ee24](https://github.com/informatievlaanderen/building-registry/commit/df8ee24a6a20eb336f2bfb2ea4aca42556224ac2))
* don't cache legacy GAWR-5195 ([a641286](https://github.com/informatievlaanderen/building-registry/commit/a6412867cb972b65fafd40b2ffdf16a416fa6001))

# [3.87.0](https://github.com/informatievlaanderen/building-registry/compare/v3.86.3...v3.87.0) (2023-09-05)


### Features

* add building unit v2 events to last changed list projection ([5a4a0d6](https://github.com/informatievlaanderen/building-registry/commit/5a4a0d6959d4c5ab31d0a7802234d86e9753aad7))

## [3.86.3](https://github.com/informatievlaanderen/building-registry/compare/v3.86.2...v3.86.3) (2023-09-04)


### Bug Fixes

* add missing parcel projections ([9d2df25](https://github.com/informatievlaanderen/building-registry/commit/9d2df25d9fc5c055185fad2b3ced0941a1208808))
* bump lambda ([e63daf4](https://github.com/informatievlaanderen/building-registry/commit/e63daf4c42c3ea57e2efa19888d2c31807b37211))
* bump lambda packages ([73d95e6](https://github.com/informatievlaanderen/building-registry/commit/73d95e63fda0285aa44be42490fa9f273c7fe0cc))

## [3.86.2](https://github.com/informatievlaanderen/building-registry/compare/v3.86.1...v3.86.2) (2023-08-30)


### Bug Fixes

* **migrator:** complete BuildingUnits when not removed ([1d1bf74](https://github.com/informatievlaanderen/building-registry/commit/1d1bf746a20d6be1a013e08158d3095eb248530e))

## [3.86.1](https://github.com/informatievlaanderen/building-registry/compare/v3.86.0...v3.86.1) (2023-08-30)


### Bug Fixes

* **migrator:** don't migrate address not in list ([e4cefb6](https://github.com/informatievlaanderen/building-registry/commit/e4cefb64273c1e7db42d0c3d91e72875cdf26d57))

# [3.86.0](https://github.com/informatievlaanderen/building-registry/compare/v3.85.1...v3.86.0) (2023-08-29)


### Bug Fixes

* **migrator:** allow skip of certain buildings if they're incomplete ([620faec](https://github.com/informatievlaanderen/building-registry/commit/620faec688225c67debd4e7a07e93c39d5430abb))


### Features

* add spatial indexes wfs ([d2b9c70](https://github.com/informatievlaanderen/building-registry/commit/d2b9c7039b604265e92d8c3ae36e72bbc997fc40))

## [3.85.1](https://github.com/informatievlaanderen/building-registry/compare/v3.85.0...v3.85.1) (2023-08-21)


### Bug Fixes

* **migration:** retired outlined buildings should be notRealized ([ef398e8](https://github.com/informatievlaanderen/building-registry/commit/ef398e89bdc7cdd94d6c8942db9654fd9991f96d))

# [3.85.0](https://github.com/informatievlaanderen/building-registry/compare/v3.84.2...v3.85.0) (2023-08-18)


### Bug Fixes

* call cancel on lambda cancellationToken after 5 minutes ([2d28d3d](https://github.com/informatievlaanderen/building-registry/commit/2d28d3dde3b0e63bf9a7cd451ee1da93dbf656c9))
* error codes ([bf6bc48](https://github.com/informatievlaanderen/building-registry/commit/bf6bc482757039c6753f59cd812ec69b1a1c43ed))


### Features

* add auth merge building ([957ce37](https://github.com/informatievlaanderen/building-registry/commit/957ce37796400e52dc606e4b92ec6920be08a8bb))

## [3.84.2](https://github.com/informatievlaanderen/building-registry/compare/v3.84.1...v3.84.2) (2023-08-14)


### Bug Fixes

* change measure building validation rule for notrealized building ([2a82c28](https://github.com/informatievlaanderen/building-registry/commit/2a82c284b2783c2a5467e5405746cc3af11d4dfc))
* errormessage ([0837bce](https://github.com/informatievlaanderen/building-registry/commit/0837bce4e81d7266c610aab79e62d2df0d827a16))

## [3.84.1](https://github.com/informatievlaanderen/building-registry/compare/v3.84.0...v3.84.1) (2023-08-11)


### Bug Fixes

* style to trigger build ([4d17544](https://github.com/informatievlaanderen/building-registry/commit/4d175447c6591392cf9dd9abc9bc2201a0ffc340))

# [3.84.0](https://github.com/informatievlaanderen/building-registry/compare/v3.83.5...v3.84.0) (2023-08-08)


### Bug Fixes

* change how lambda is deployed in test ([ce24544](https://github.com/informatievlaanderen/building-registry/commit/ce245448dba550e5755fd5fc7134dc09ce82c152))


### Features

* add BUILDINGUNIT_SNAPSHOT_OSLO_STREAM_FLATTEN.ksql ([4e1d2cc](https://github.com/informatievlaanderen/building-registry/commit/4e1d2cc4bb1f192202ddb737afaf1f6ff76659ba))

## [3.83.5](https://github.com/informatievlaanderen/building-registry/compare/v3.83.4...v3.83.5) (2023-07-28)


### Bug Fixes

* use buildingUnitPLID instead of buildingPLID in buildingUnit snapshot methods ([b1c99e5](https://github.com/informatievlaanderen/building-registry/commit/b1c99e5dd41290b21c7e2b5b2424a9ffcc474721))

## [3.83.4](https://github.com/informatievlaanderen/building-registry/compare/v3.83.3...v3.83.4) (2023-07-25)


### Bug Fixes

* errorcode when correcting/changing building geometry ([2474b5e](https://github.com/informatievlaanderen/building-registry/commit/2474b5eb9faaae97fe9bed5e9e185d742b2665d8))

## [3.83.3](https://github.com/informatievlaanderen/building-registry/compare/v3.83.2...v3.83.3) (2023-07-24)


### Bug Fixes

* change event descriptions ([957cbb4](https://github.com/informatievlaanderen/building-registry/commit/957cbb488176790db0fcc6d993cee029bfc65f40))

## [3.83.2](https://github.com/informatievlaanderen/building-registry/compare/v3.83.1...v3.83.2) (2023-07-14)


### Bug Fixes

* BuildingGeometryWasImportedFromGrb event description ([b64b245](https://github.com/informatievlaanderen/building-registry/commit/b64b24595b8b47a276654953fca11db6371c9ce9))
* don't project BuildingGeometryWasImportedFromGrb in syndication ([58900ee](https://github.com/informatievlaanderen/building-registry/commit/58900ee213d2bc808ff0c677e80376b7645c4e5a))
* importer legacy ([5c19b30](https://github.com/informatievlaanderen/building-registry/commit/5c19b30eebb5703bac57fde73520a35aa167cfef))
* the BuildingGeometryWasImportedFromGrb event should not be visible in sync or docs ([7b7d6d4](https://github.com/informatievlaanderen/building-registry/commit/7b7d6d4a841ee252cc092bfc3e1f475fff279f22))

## [3.83.1](https://github.com/informatievlaanderen/building-registry/compare/v3.83.0...v3.83.1) (2023-07-03)


### Bug Fixes

* GmlHelpers ToExtendedWkbGeometry method ([3835849](https://github.com/informatievlaanderen/building-registry/commit/3835849821daa1ba988c52b44943423dc2814cf0))

# [3.83.0](https://github.com/informatievlaanderen/building-registry/compare/v3.82.0...v3.83.0) (2023-06-30)


### Bug Fixes

* remove metadata from requests ([b6f06a7](https://github.com/informatievlaanderen/building-registry/commit/b6f06a784a207b74489a82a29aed48a026c2b391))


### Features

* add projections merge building ([f32d843](https://github.com/informatievlaanderen/building-registry/commit/f32d843e69d9c066d271f2b191b19ca9eacc7898))
* merge buildings backoffice ([10f7798](https://github.com/informatievlaanderen/building-registry/commit/10f779847291a9295772d9a1077333b4bed6d167))
* set lastevent on bu (correct)removal ([79705b9](https://github.com/informatievlaanderen/building-registry/commit/79705b9c66bd5b255a979a9a52553fa46e63615d))

# [3.82.0](https://github.com/informatievlaanderen/building-registry/compare/v3.81.5...v3.82.0) (2023-06-28)


### Features

* add MergeBuilding api contract ([70ddf16](https://github.com/informatievlaanderen/building-registry/commit/70ddf16a6126576c7b501e85ba6fdaf0ca4c233c))
* add new building by merger ([d8d8a0b](https://github.com/informatievlaanderen/building-registry/commit/d8d8a0bcfefea500267ca289b590b7de53765734))
* mark building as merged ([8444594](https://github.com/informatievlaanderen/building-registry/commit/8444594636aa8cc1e387a1beec0f8a9149deda98))

## [3.81.5](https://github.com/informatievlaanderen/building-registry/compare/v3.81.4...v3.81.5) (2023-06-21)


### Bug Fixes

* naming producers migration ([622cfc8](https://github.com/informatievlaanderen/building-registry/commit/622cfc8df94a533c0a0716cc01bddd28787f20b4))

## [3.81.4](https://github.com/informatievlaanderen/building-registry/compare/v3.81.3...v3.81.4) (2023-06-16)


### Bug Fixes

* consume parcels idempotent ([2e0f451](https://github.com/informatievlaanderen/building-registry/commit/2e0f451d61c1844047d336ba05f0ea3fb140a697))
* consume parcels idempotent the right way ([306421b](https://github.com/informatievlaanderen/building-registry/commit/306421bbf0bce75231aafa03e4fc4c8266b3e4a3))

## [3.81.3](https://github.com/informatievlaanderen/building-registry/compare/v3.81.2...v3.81.3) (2023-06-15)


### Bug Fixes

* registration of IConsumer in parcel consumer ([6d66fdf](https://github.com/informatievlaanderen/building-registry/commit/6d66fdf53e2391648a85692cae125059581fa54f))

## [3.81.2](https://github.com/informatievlaanderen/building-registry/compare/v3.81.1...v3.81.2) (2023-06-15)


### Bug Fixes

* name of consumer parcel service in release ([fa9d163](https://github.com/informatievlaanderen/building-registry/commit/fa9d163bc678ff6425086c7c451bb703acf798c4))

## [3.81.1](https://github.com/informatievlaanderen/building-registry/compare/v3.81.0...v3.81.1) (2023-06-15)


### Bug Fixes

* use AddressRegistry addressDetail Legacy projections for migrator ([e655dba](https://github.com/informatievlaanderen/building-registry/commit/e655dba9ada6c9392a55e8ffd24b3034d8809ea9))


### Performance Improvements

* migrate projections in batch ([1697416](https://github.com/informatievlaanderen/building-registry/commit/1697416c492b1d4e164adfa32a1ae86f326c104b))

# [3.81.0](https://github.com/informatievlaanderen/building-registry/compare/v3.80.3...v3.81.0) (2023-06-14)


### Bug Fixes

* fallbacks for idempotent relation add ([3f03b2c](https://github.com/informatievlaanderen/building-registry/commit/3f03b2caf3c00e246d864459eaaff7a0d861ac3d))
* producer naming building ([9784329](https://github.com/informatievlaanderen/building-registry/commit/9784329493edbc8172682c78084c4018c8717f37))
* producer naming buildingunit ([be6f410](https://github.com/informatievlaanderen/building-registry/commit/be6f410ae125bd07c5473c1635550b335e835a03))
* register consumer parcel module in legacy and oslo api ([9aec638](https://github.com/informatievlaanderen/building-registry/commit/9aec63840a1968488124144cf779448cb45a41e9))


### Features

* consume parcel topic ([b91e5c5](https://github.com/informatievlaanderen/building-registry/commit/b91e5c5ce98ae9226014cd59176cf3ff70176cac))
* replace syndication parcel items by kafka consumed parcel items ([e8f988c](https://github.com/informatievlaanderen/building-registry/commit/e8f988c2b34426f5a307d47da6164a2b382474f3))

## [3.80.3](https://github.com/informatievlaanderen/building-registry/compare/v3.80.2...v3.80.3) (2023-06-13)


### Bug Fixes

* big building migration threshold to 30k ([18394cb](https://github.com/informatievlaanderen/building-registry/commit/18394cb417c3a7a03f525cbba312730eef20ac9f))

## [3.80.2](https://github.com/informatievlaanderen/building-registry/compare/v3.80.1...v3.80.2) (2023-06-12)


### Performance Improvements

* set migrator catchuppagesize to increase performance ([172b1c9](https://github.com/informatievlaanderen/building-registry/commit/172b1c9add4128fdf82c1021bf60f1fdd0e220fb))

## [3.80.1](https://github.com/informatievlaanderen/building-registry/compare/v3.80.0...v3.80.1) (2023-06-12)


### Bug Fixes

* set projection migrator saveinterval to 10000 ([2b4c22a](https://github.com/informatievlaanderen/building-registry/commit/2b4c22a5c2ef827796e1903c11dff3820685123f))

# [3.80.0](https://github.com/informatievlaanderen/building-registry/compare/v3.79.1...v3.80.0) (2023-06-12)


### Bug Fixes

* revert add elastic apm ([d192a72](https://github.com/informatievlaanderen/building-registry/commit/d192a72dfec6cb84453a211f973b993f8daf4c13))
* set bigbuilding threshold to 50000 for building migrator ([bb99639](https://github.com/informatievlaanderen/building-registry/commit/bb99639a8442383c2e3315e9a990386ba5705f3a))


### Features

* add controller consumer ([e7e70c5](https://github.com/informatievlaanderen/building-registry/commit/e7e70c5d070b3a603fb509c8d0d1ce7c2075c484))

## [3.79.1](https://github.com/informatievlaanderen/building-registry/compare/v3.79.0...v3.79.1) (2023-06-08)


### Bug Fixes

* big building migration threshold to 5000 & pagesize 6 ([795598f](https://github.com/informatievlaanderen/building-registry/commit/795598f2d107b57da75ec995e4ae4731a235fab0))

# [3.79.0](https://github.com/informatievlaanderen/building-registry/compare/v3.78.15...v3.79.0) (2023-06-08)


### Bug Fixes

* check if big building wasn't already migrated with the small buildings ([0b80e3a](https://github.com/informatievlaanderen/building-registry/commit/0b80e3aef041e8c830627779cfb724171f5556d0))


### Features

* add elastic apm ([49b66a7](https://github.com/informatievlaanderen/building-registry/commit/49b66a7d412129b8e327667e9d670fb8e87c7809))

## [3.78.15](https://github.com/informatievlaanderen/building-registry/compare/v3.78.14...v3.78.15) (2023-06-06)


### Bug Fixes

* split migrator into small/big building migration ([fa382ec](https://github.com/informatievlaanderen/building-registry/commit/fa382ec9720d3302e71f3d11a1c64533a9ec4d6e))

## [3.78.14](https://github.com/informatievlaanderen/building-registry/compare/v3.78.13...v3.78.14) (2023-06-05)


### Bug Fixes

* add registration iaddcommonbuildingunit to migrator ([2e3f0d9](https://github.com/informatievlaanderen/building-registry/commit/2e3f0d9f9423f68a1040c053ceff97cf0183f618))

## [3.78.13](https://github.com/informatievlaanderen/building-registry/compare/v3.78.12...v3.78.13) (2023-06-02)


### Bug Fixes

* run task with network configuration ([7425e87](https://github.com/informatievlaanderen/building-registry/commit/7425e87db4a2819317d6a70632749993fcbbd8b7))

## [3.78.12](https://github.com/informatievlaanderen/building-registry/compare/v3.78.11...v3.78.12) (2023-06-02)


### Bug Fixes

* run task instead of start task + retry with network configuration ([37d7c0f](https://github.com/informatievlaanderen/building-registry/commit/37d7c0f13998c89a597f1871da62dcfceee0df1f))

## [3.78.11](https://github.com/informatievlaanderen/building-registry/compare/v3.78.10...v3.78.11) (2023-06-01)


### Bug Fixes

* to trigger build ci/cd new prd ([ae1463d](https://github.com/informatievlaanderen/building-registry/commit/ae1463db877b2d40f496d35afc0f867c803fbd03))

## [3.78.10](https://github.com/informatievlaanderen/building-registry/compare/v3.78.9...v3.78.10) (2023-06-01)


### Bug Fixes

* zip job results dbf file ([4a15918](https://github.com/informatievlaanderen/building-registry/commit/4a15918d3a22568dfd9865652e69f65655cf009f))

## [3.78.9](https://github.com/informatievlaanderen/building-registry/compare/v3.78.8...v3.78.9) (2023-06-01)


### Bug Fixes

* add db schema to delete job records sql statement ([199642d](https://github.com/informatievlaanderen/building-registry/commit/199642dcfa0b4f1c73f4330c185151257f8dc17e))
* get consumerItems before processing migrator ([7b6c4c8](https://github.com/informatievlaanderen/building-registry/commit/7b6c4c8a2e3069446eba37523a669c4c314ee86d))

## [3.78.8](https://github.com/informatievlaanderen/building-registry/compare/v3.78.7...v3.78.8) (2023-05-31)


### Bug Fixes

* set grb results stream position to 0 before uploading ([32d460f](https://github.com/informatievlaanderen/building-registry/commit/32d460f8315769116d796c9d9f3df0fa6834a6b9))

## [3.78.7](https://github.com/informatievlaanderen/building-registry/compare/v3.78.6...v3.78.7) (2023-05-31)


### Bug Fixes

* add db schema to query ([2da7cc0](https://github.com/informatievlaanderen/building-registry/commit/2da7cc058d1c68c3b5510a7527f6093b64d7b4ca))
* don't use parallelization in jobrecordmonitor ([d83c6d6](https://github.com/informatievlaanderen/building-registry/commit/d83c6d6879a1734f80a576c43e3aaa8204131888))
* job results should use public api url ([0453459](https://github.com/informatievlaanderen/building-registry/commit/0453459639e788bd1b905db3c735a1d0b8959e1e))

## [3.78.6](https://github.com/informatievlaanderen/building-registry/compare/v3.78.5...v3.78.6) (2023-05-31)


### Bug Fixes

* error code null in errorWarningEvaluator ([d3afea1](https://github.com/informatievlaanderen/building-registry/commit/d3afea16fb56b2a352a99dfc058f546e17009afd))


### Performance Improvements

* run migrator in parallel ([17cca58](https://github.com/informatievlaanderen/building-registry/commit/17cca581bf42c3a252bab39939461d8222d7c89d))

## [3.78.5](https://github.com/informatievlaanderen/building-registry/compare/v3.78.4...v3.78.5) (2023-05-30)


### Bug Fixes

* BackOfficeApiResult IsSuccess evaluation ([b383118](https://github.com/informatievlaanderen/building-registry/commit/b383118b305d42242476057b7870f63a252728ba))

## [3.78.4](https://github.com/informatievlaanderen/building-registry/compare/v3.78.3...v3.78.4) (2023-05-30)


### Bug Fixes

* register grbApiOptions + routes backoffice api ([fa1738b](https://github.com/informatievlaanderen/building-registry/commit/fa1738bbe459caaeaf69396dcc3b351ed352cd9b))

## [3.78.3](https://github.com/informatievlaanderen/building-registry/compare/v3.78.2...v3.78.3) (2023-05-26)


### Bug Fixes

* support binary file types in s3 getblobasync ([604f3e3](https://github.com/informatievlaanderen/building-registry/commit/604f3e32b270af1bcf110871802a92f296c8ce7d))

## [3.78.2](https://github.com/informatievlaanderen/building-registry/compare/v3.78.1...v3.78.2) (2023-05-26)


### Bug Fixes

* complete ticket when grb job is cancelled ([7f1eaa9](https://github.com/informatievlaanderen/building-registry/commit/7f1eaa979913010d848c2ac9b5d053229fbb4fed))

## [3.78.1](https://github.com/informatievlaanderen/building-registry/compare/v3.78.0...v3.78.1) (2023-05-25)


### Bug Fixes

* already an open DataReader associated with this Connection + migration tablename ([be86c25](https://github.com/informatievlaanderen/building-registry/commit/be86c257ab85222bd8e8cffdaeeb884dd8024e8a))
* register AmazonS3Client with configured region ([50d58e1](https://github.com/informatievlaanderen/building-registry/commit/50d58e19c392444450e84141936d3e9a10603b2b))

# [3.78.0](https://github.com/informatievlaanderen/building-registry/compare/v3.77.0...v3.78.0) (2023-05-24)


### Bug Fixes

* change name status page bu projections ([ebe2d7e](https://github.com/informatievlaanderen/building-registry/commit/ebe2d7e93c42810988dd23e3ff08616e2602dbbd))


### Features

* add grb realeases ([57512b3](https://github.com/informatievlaanderen/building-registry/commit/57512b315d778b2b74fcfcc00a6968c4363159b4))

# [3.77.0](https://github.com/informatievlaanderen/building-registry/compare/v3.76.0...v3.77.0) (2023-05-23)


### Bug Fixes

* grb response remove unused metadata ([8f0c355](https://github.com/informatievlaanderen/building-registry/commit/8f0c355ccd8e4fc4192a70928090e50bb000682a))


### Features

* add grb jobrecord status ErrorResolved ([f4f0e62](https://github.com/informatievlaanderen/building-registry/commit/f4f0e6230ef650aeda50672e3d65ca14224cbeb9))
* upload grb job result + archive grb job records ([ee1f705](https://github.com/informatievlaanderen/building-registry/commit/ee1f70505cbac5f4b164a36318b707d050673411))

# [3.76.0](https://github.com/informatievlaanderen/building-registry/compare/v3.75.0...v3.76.0) (2023-05-17)


### Bug Fixes

* buildingunitaddresslinkextract types GAWR-4855 ([6bf481f](https://github.com/informatievlaanderen/building-registry/commit/6bf481fe8e6ac133350edbfe76b7dabd07514548))


### Features

* add job processor GAWR-3120 ([e1bfa34](https://github.com/informatievlaanderen/building-registry/commit/e1bfa34c10a7e05f097adf67a7c463a6482c9be1))

# [3.75.0](https://github.com/informatievlaanderen/building-registry/compare/v3.74.1...v3.75.0) (2023-05-10)


### Bug Fixes

* produce oslo snapshot based upon timestamp and etag ([3fd0e0a](https://github.com/informatievlaanderen/building-registry/commit/3fd0e0a6f940afd30c673b77aebea758ea46dc81))


### Features

* initial job processor ([43eae4f](https://github.com/informatievlaanderen/building-registry/commit/43eae4f38282486e4a0b1a01448cec7700eee332))
* upload processor grb ([0e8274c](https://github.com/informatievlaanderen/building-registry/commit/0e8274c98bdc18b57afa5ef06d37036a54e59110))

## [3.74.1](https://github.com/informatievlaanderen/building-registry/compare/v3.74.0...v3.74.1) (2023-05-04)


### Bug Fixes

* change docs event readdress ([2a9d52a](https://github.com/informatievlaanderen/building-registry/commit/2a9d52a6aef6f33897c66be1a5f938c40043df34))

# [3.74.0](https://github.com/informatievlaanderen/building-registry/compare/v3.73.0...v3.74.0) (2023-05-03)


### Bug Fixes

* building unit status error code when correcting realization ([a002cc6](https://github.com/informatievlaanderen/building-registry/commit/a002cc691d5b54105fae856d995d6fdf0ad8dc5b))
* RealizeAndMeasureUnplannedBuildingSqsHandler handles correct request ([f51591c](https://github.com/informatievlaanderen/building-registry/commit/f51591ce11696631d17983a25bbaa4b39dfe604b))


### Features

* changeBuildingMeasurement ([0c98421](https://github.com/informatievlaanderen/building-registry/commit/0c98421e26803b68a0da794268e1a62908406b64))

# [3.73.0](https://github.com/informatievlaanderen/building-registry/compare/v3.72.0...v3.73.0) (2023-05-02)


### Bug Fixes

* don't produce BuildingGeometryWasImportedFromGrb to Oslo topic ([ed09788](https://github.com/informatievlaanderen/building-registry/commit/ed0978870c352db3fae217fb21fb8feba1e56f95))


### Features

* add GRB cancel job endpoint ([4f3f7b2](https://github.com/informatievlaanderen/building-registry/commit/4f3f7b2567794e1ec5bfbecbc0e2e22c9e2a3c6e))
* correct building measurement api ([aade06f](https://github.com/informatievlaanderen/building-registry/commit/aade06f10d282ff02ad82121f43e3dfe897c9971))
* correct building measurement domain ([064adc4](https://github.com/informatievlaanderen/building-registry/commit/064adc470da7a2b3e28fbbee343f8174b6ea6b36))
* correcting building measurement projections ([769fdff](https://github.com/informatievlaanderen/building-registry/commit/769fdff9b65aac86683a6161a42aceb00cce0f17))
* grb api add GetResult ([f726c15](https://github.com/informatievlaanderen/building-registry/commit/f726c1586f305870bcf2aa1ad2bae982df4a235a))
* measure building api + handlers ([d2b5f39](https://github.com/informatievlaanderen/building-registry/commit/d2b5f3947a0ed32ee8545a7789aa7145d7d380cb))
* measure building domain ([e48e3e2](https://github.com/informatievlaanderen/building-registry/commit/e48e3e2c22aba5c7074f5bf3549af5945b62a751))
* measure building projections ([671c207](https://github.com/informatievlaanderen/building-registry/commit/671c2074d75976942c372e81c8dec43dcf4e2146))

# [3.72.0](https://github.com/informatievlaanderen/building-registry/compare/v3.71.0...v3.72.0) (2023-04-27)


### Features

* add GRB abstractions ([a3b49dd](https://github.com/informatievlaanderen/building-registry/commit/a3b49dd02ec5368a78baf8a3112bef001b6b16f0))
* demolish building ([0534e67](https://github.com/informatievlaanderen/building-registry/commit/0534e679b5ce32f14bf5ed45dd28d9cd66db75d3))
* realize and measure unplanned building ([dd51c31](https://github.com/informatievlaanderen/building-registry/commit/dd51c313ef0b6daad39c427a402ba97217399549))
* refactor producer to share v2 + demolishBuilding projections ([cd60570](https://github.com/informatievlaanderen/building-registry/commit/cd605700fb2ecc5abfd8d6a930b918169249d63d))
* save job when creating presignedurl ([d242eee](https://github.com/informatievlaanderen/building-registry/commit/d242eee89e8b7bc58370e64f883af5583105f0c9))

# [3.71.0](https://github.com/informatievlaanderen/building-registry/compare/v3.70.4...v3.71.0) (2023-04-18)


### Features

* consume new readdress events ([36910e3](https://github.com/informatievlaanderen/building-registry/commit/36910e321e8676cd697c6dac5ad7c99f4a4954b2))

## [3.70.4](https://github.com/informatievlaanderen/building-registry/compare/v3.70.3...v3.70.4) (2023-04-13)

## [3.70.3](https://github.com/informatievlaanderen/building-registry/compare/v3.70.2...v3.70.3) (2023-04-12)


### Bug Fixes

* only expose edit contract GebouweenheidFunctie NietGekend ([e3aabc4](https://github.com/informatievlaanderen/building-registry/commit/e3aabc4a058dfc7f6677ea92b4c858fe01493ae5))

## [3.70.2](https://github.com/informatievlaanderen/building-registry/compare/v3.70.1...v3.70.2) (2023-04-11)


### Bug Fixes

* remove removed building buildingunits from extract ([2e6fc30](https://github.com/informatievlaanderen/building-registry/commit/2e6fc30b4a9abd7294b0d0a16a3ee25e4638de00))

## [3.70.1](https://github.com/informatievlaanderen/building-registry/compare/v3.70.0...v3.70.1) (2023-04-11)


### Bug Fixes

* fix flaky test WithActiveBuildingUnit_ThenCommonBuildingUnitWasNotAdded ([f72396d](https://github.com/informatievlaanderen/building-registry/commit/f72396d81821c193be60163f6411bf2fcc179ce7))
* remove addressPersistentLocalIds from buildingUnitRemovalWasCorrected event ([374b22d](https://github.com/informatievlaanderen/building-registry/commit/374b22d786524e4cee8188e3bede99ab45600dbc))

# [3.70.0](https://github.com/informatievlaanderen/building-registry/compare/v3.69.2...v3.70.0) (2023-04-11)


### Features

* consume readdress event ([91e03f2](https://github.com/informatievlaanderen/building-registry/commit/91e03f26c42565c9d0935ddb829a3e358ecf1d21))

## [3.69.2](https://github.com/informatievlaanderen/building-registry/compare/v3.69.1...v3.69.2) (2023-04-05)


### Bug Fixes

* extract addresslink filename ([9ff98bc](https://github.com/informatievlaanderen/building-registry/commit/9ff98bcb35d5eaf209747444761d4c617155dc1b))

## [3.69.1](https://github.com/informatievlaanderen/building-registry/compare/v3.69.0...v3.69.1) (2023-04-04)


### Bug Fixes

* extract addresslink migration ([f463b2f](https://github.com/informatievlaanderen/building-registry/commit/f463b2f32f998180abb1efbd2399b52c05ef248b))

# [3.69.0](https://github.com/informatievlaanderen/building-registry/compare/v3.68.2...v3.69.0) (2023-04-04)


### Bug Fixes

* run containers as non-root ([8997cac](https://github.com/informatievlaanderen/building-registry/commit/8997cac8e7e7a7f816e8a95575e791d94e55797e))


### Features

* add buildingunit-address links extract ([2afe36a](https://github.com/informatievlaanderen/building-registry/commit/2afe36a40d9faab9e6501a1bdd42c2dd4026cef3))

## [3.68.2](https://github.com/informatievlaanderen/building-registry/compare/v3.68.1...v3.68.2) (2023-03-31)


### Bug Fixes

* correct errorcode when building is removed ([f7397df](https://github.com/informatievlaanderen/building-registry/commit/f7397df476faabe0d8172a65d9fab0a9f7b0b193))

## [3.68.1](https://github.com/informatievlaanderen/building-registry/compare/v3.68.0...v3.68.1) (2023-03-31)


### Bug Fixes

* wms views v2 migration ([b4b14b3](https://github.com/informatievlaanderen/building-registry/commit/b4b14b31b98b7954e754e8b4eded5c4bd1cedba4))

# [3.68.0](https://github.com/informatievlaanderen/building-registry/compare/v3.67.1...v3.68.0) (2023-03-31)


### Features

* add wms views v2 ([7a0b247](https://github.com/informatievlaanderen/building-registry/commit/7a0b2470071a67b6473f8fdc88a0f184ae831da0))

## [3.67.1](https://github.com/informatievlaanderen/building-registry/compare/v3.67.0...v3.67.1) (2023-03-30)


### Bug Fixes

* wfs grb in oslo api ([13b985f](https://github.com/informatievlaanderen/building-registry/commit/13b985f4e68ba01a690e71ec64b515bd047eb403))

# [3.67.0](https://github.com/informatievlaanderen/building-registry/compare/v3.66.0...v3.67.0) (2023-03-29)


### Features

* add new grbwfs namespace and make backwards compatible ([d1359d6](https://github.com/informatievlaanderen/building-registry/commit/d1359d68c74955fd4a4e6dd85b63ce0d2bda045e))
* add new grbwfs namespace and make backwards compatible ([1d9ded9](https://github.com/informatievlaanderen/building-registry/commit/1d9ded9a1942f24f9a96a3a724eda71464095018))

# [3.66.0](https://github.com/informatievlaanderen/building-registry/compare/v3.65.1...v3.66.0) (2023-03-29)


### Bug Fixes

* enable snapshotting + add tests ([e022525](https://github.com/informatievlaanderen/building-registry/commit/e022525b789da8a6b42ffadf2c8b5afee4de07f8))


### Features

* Add prefix to grb file ([9587db8](https://github.com/informatievlaanderen/building-registry/commit/9587db85fcb5ca93cc5680ea1d7b286ad1ac1f04))

## [3.65.1](https://github.com/informatievlaanderen/building-registry/compare/v3.65.0...v3.65.1) (2023-03-17)


### Bug Fixes

* grb upload remove content-type condition ([af11336](https://github.com/informatievlaanderen/building-registry/commit/af113367bb07b05e2566fc3a202db64946ef0931))

# [3.65.0](https://github.com/informatievlaanderen/building-registry/compare/v3.64.3...v3.65.0) (2023-03-17)


### Features

* add authorization for grb upload ([f75fb27](https://github.com/informatievlaanderen/building-registry/commit/f75fb271ee15c9ef7811a130dd0a22f83d78026d))
* grb upload should have content type zip ([7f8d2f6](https://github.com/informatievlaanderen/building-registry/commit/7f8d2f68814059fd510d817906e913662c71d70d))

## [3.64.3](https://github.com/informatievlaanderen/building-registry/compare/v3.64.2...v3.64.3) (2023-03-17)


### Bug Fixes

* api grb contract + flaky tests ([9c342a1](https://github.com/informatievlaanderen/building-registry/commit/9c342a191d1cede4871ab57dc081c72e4b2bf754))
* buildingunit ifmatch validator exception ([a467a0d](https://github.com/informatievlaanderen/building-registry/commit/a467a0d462528b4d12587b4c1ab541f5cb8ded0a))

## [3.64.2](https://github.com/informatievlaanderen/building-registry/compare/v3.64.1...v3.64.2) (2023-03-16)


### Bug Fixes

* api grb presigned response ([37ae862](https://github.com/informatievlaanderen/building-registry/commit/37ae862b9cb03ad136b0df274e65dd2949b14c74))

## [3.64.1](https://github.com/informatievlaanderen/building-registry/compare/v3.64.0...v3.64.1) (2023-03-16)


### Bug Fixes

* correct aws config ([73d2a1e](https://github.com/informatievlaanderen/building-registry/commit/73d2a1eb917b7f7ecae9e72499575391e372411d))

# [3.64.0](https://github.com/informatievlaanderen/building-registry/compare/v3.63.0...v3.64.0) (2023-03-16)


### Features

* make bucket options configurable in grb api ([3e69fa2](https://github.com/informatievlaanderen/building-registry/commit/3e69fa2e58bbe88c582e9b0c4376d87063218153))

# [3.63.0](https://github.com/informatievlaanderen/building-registry/compare/v3.62.3...v3.63.0) (2023-03-16)


### Features

* add grb api ([86d6ac2](https://github.com/informatievlaanderen/building-registry/commit/86d6ac2820c1b57496f55677fef902cae471f502))

## [3.62.3](https://github.com/informatievlaanderen/building-registry/compare/v3.62.2...v3.62.3) (2023-03-14)


### Bug Fixes

* consumer address missing SequenceModule registration ([394bcf6](https://github.com/informatievlaanderen/building-registry/commit/394bcf61c8ad566a7503330d949dc600235d5891))

## [3.62.2](https://github.com/informatievlaanderen/building-registry/compare/v3.62.1...v3.62.2) (2023-03-13)


### Bug Fixes

* ksl geolocation GEBOUWEENHEDEN_OBJECTIDS ([d8fa16e](https://github.com/informatievlaanderen/building-registry/commit/d8fa16e73b43c189fe74ea656a5b2a198cee4f80))

## [3.62.1](https://github.com/informatievlaanderen/building-registry/compare/v3.62.0...v3.62.1) (2023-03-07)


### Bug Fixes

* make producer reliable ([fab5228](https://github.com/informatievlaanderen/building-registry/commit/fab5228240e361c6294cb6a0108b9305a0c3cf48))

# [3.62.0](https://github.com/informatievlaanderen/building-registry/compare/v3.61.0...v3.62.0) (2023-03-06)


### Features

* detach address when address is removed because streetname was removed ([4c2784d](https://github.com/informatievlaanderen/building-registry/commit/4c2784df306f07328f0bc9631942648e101202ee))

# [3.61.0](https://github.com/informatievlaanderen/building-registry/compare/v3.60.2...v3.61.0) (2023-03-01)


### Bug Fixes

* bump mediatr ([d62c103](https://github.com/informatievlaanderen/building-registry/commit/d62c10395b103799295cceb41dc8d67392b4c003))
* no merge group for ksql ([cbf5ffc](https://github.com/informatievlaanderen/building-registry/commit/cbf5ffc499a8bbbfd52acc458a3f430d330ba603))
* remove ServiceFactory block ([3fe9677](https://github.com/informatievlaanderen/building-registry/commit/3fe96779d170e98da1df1a6ac9e70b603d7d2a8e))


### Features

* add v2 examples ([#937](https://github.com/informatievlaanderen/building-registry/issues/937)) ([477ddbd](https://github.com/informatievlaanderen/building-registry/commit/477ddbd65fb94de94e718550e9b4d0dd608821c3))

## [3.60.2](https://github.com/informatievlaanderen/building-registry/compare/v3.60.1...v3.60.2) (2023-02-27)


### Bug Fixes

* add missing detach address events in backoffice projections ([d13139a](https://github.com/informatievlaanderen/building-registry/commit/d13139ab91a339d5d448f8004f31b2c8d0846af7))
* bump grar common to 18.1.1 ([0854d80](https://github.com/informatievlaanderen/building-registry/commit/0854d8023f0800ff3ac691c1c183756733b18a9e))
* missing mediatr dependency ([c76c0db](https://github.com/informatievlaanderen/building-registry/commit/c76c0db22f76a546a1096db0551dc0972d9c402b))

## [3.60.1](https://github.com/informatievlaanderen/building-registry/compare/v3.60.0...v3.60.1) (2023-02-23)


### Bug Fixes

* use new ci flow ([31f264d](https://github.com/informatievlaanderen/building-registry/commit/31f264d20f69f379d4fef36d961037f964c48d29))

# [3.60.0](https://github.com/informatievlaanderen/building-registry/compare/v3.59.4...v3.60.0) (2023-02-21)


### Features

* consume streetname rejected address events ([5add190](https://github.com/informatievlaanderen/building-registry/commit/5add1903ce262468671abd83ea4ed3f4cc9ab9fa))

## [3.59.4](https://github.com/informatievlaanderen/building-registry/compare/v3.59.3...v3.59.4) (2023-02-20)


### Bug Fixes

* clean up references + idempotency plan commands ([7f6b5d7](https://github.com/informatievlaanderen/building-registry/commit/7f6b5d72d614f51e3869034149632d5269538592))
* connectionstring cleanup ([3349910](https://github.com/informatievlaanderen/building-registry/commit/3349910f7c3adba5d062f83085f679846b92e379))
* consumer should commit if message is already processed ([f7352ba](https://github.com/informatievlaanderen/building-registry/commit/f7352ba7400c1d1b067db9af6a8804cd6aad65dc))
* dont mark building as deleted when building unit was deleted ([39fd5f8](https://github.com/informatievlaanderen/building-registry/commit/39fd5f88d2528ebd3ca9e330a994d46b641c2799))
* idempotency on planning building and building unit ([b431ecb](https://github.com/informatievlaanderen/building-registry/commit/b431ecbb53a74a2aa9ed4abd11f4a9a78c38b05a))
* refactor legacy/oslo ([ddb50c7](https://github.com/informatievlaanderen/building-registry/commit/ddb50c74f2fc056dc96e2837c80f63d25c4e97dc))
* registrations ([d30045d](https://github.com/informatievlaanderen/building-registry/commit/d30045d70539946ed38ab8431ec52f4f33edf452))
* use merge queue ([e7e8751](https://github.com/informatievlaanderen/building-registry/commit/e7e875122304e291ab9f5466e5960a6b769752c3))

## [3.59.3](https://github.com/informatievlaanderen/building-registry/compare/v3.59.2...v3.59.3) (2023-02-14)


### Bug Fixes

* number ksql ([bc81652](https://github.com/informatievlaanderen/building-registry/commit/bc8165290dce7ed3132eba3734afcf17238b5ecd))

## [3.59.2](https://github.com/informatievlaanderen/building-registry/compare/v3.59.1...v3.59.2) (2023-02-13)


### Bug Fixes

* event jsondata should contain provenance ([441d7b8](https://github.com/informatievlaanderen/building-registry/commit/441d7b8b54e339b90b301b30febbcd1d7ba1284d))
* use correct date in provenance for consumer command handling ([4ecba34](https://github.com/informatievlaanderen/building-registry/commit/4ecba34a9651c53469e3dc5dff892ed837d29006))

## [3.59.1](https://github.com/informatievlaanderen/building-registry/compare/v3.59.0...v3.59.1) (2023-01-26)


### Bug Fixes

* remove unnecessary ?? null ([a307410](https://github.com/informatievlaanderen/building-registry/commit/a3074105948992773d72bf83fc9fdcd7aae1eb0e))
* use return value of Metadata.Add ([f2f5d44](https://github.com/informatievlaanderen/building-registry/commit/f2f5d44410a4447d37a5b0f340b1d94daed24e8e))

# [3.59.0](https://github.com/informatievlaanderen/building-registry/compare/v3.58.1...v3.59.0) (2023-01-23)


### Bug Fixes

* add decentraleBijwerker auth attr to remove endpoints ([5ccb1e3](https://github.com/informatievlaanderen/building-registry/commit/5ccb1e339c8b18fae9a32a0b9da357cbe87a1eb6))
* change encoding docker file ([36f221a](https://github.com/informatievlaanderen/building-registry/commit/36f221ac9f49fa03b956f7cb11f9933c05ee73b1))


### Features

* add acm idm auth to endpoints ([8212a69](https://github.com/informatievlaanderen/building-registry/commit/8212a69ebfe1874b8594b2308e63df689d6f780f))

## [3.58.1](https://github.com/informatievlaanderen/building-registry/compare/v3.58.0...v3.58.1) (2023-01-13)


### Bug Fixes

* add backoffice projections to cd ([77b14dd](https://github.com/informatievlaanderen/building-registry/commit/77b14dd00c55f7330d74c9b61a0b945931a08a79))

# [3.58.0](https://github.com/informatievlaanderen/building-registry/compare/v3.57.0...v3.58.0) (2023-01-13)


### Bug Fixes

* clean snapshot producers from incompatible events ([e5810e4](https://github.com/informatievlaanderen/building-registry/commit/e5810e4929176e84307789b0c33b4bf52abb94c2))


### Features

* add correct bu deregularization ([696b64d](https://github.com/informatievlaanderen/building-registry/commit/696b64db9264cde066cf4c40edf9f0953bfdca19))

# [3.57.0](https://github.com/informatievlaanderen/building-registry/compare/v3.56.0...v3.57.0) (2023-01-12)


### Features

* update Be.Vlaanderen.Basisregisters.Api to 19.0.1 ([0b37006](https://github.com/informatievlaanderen/building-registry/commit/0b3700674fff7f73bafb4d6fce3e597e63d15351))

# [3.56.0](https://github.com/informatievlaanderen/building-registry/compare/v3.55.0...v3.56.0) (2023-01-12)


### Bug Fixes

* catch bu not found in detach controller ([af89a5e](https://github.com/informatievlaanderen/building-registry/commit/af89a5ed0dbbc92de2bcf3fa8c27f303ff17b6ff))


### Features

* correct BU regularization backoffice ([e8ffa05](https://github.com/informatievlaanderen/building-registry/commit/e8ffa05eccb62eacdee5ec3559f27ef0ded62db5))
* correct bu regularization domain ([b456ac5](https://github.com/informatievlaanderen/building-registry/commit/b456ac5b7541c1fc5a8ddd95c29c567ea3c68367))

# [3.55.0](https://github.com/informatievlaanderen/building-registry/compare/v3.54.0...v3.55.0) (2023-01-10)


### Features

* detach address on remove building and bu ([5edfb39](https://github.com/informatievlaanderen/building-registry/commit/5edfb39b66375306b9ab25d000cfb279dde1531d))
* detach address when not realing building or building unit ([25f1697](https://github.com/informatievlaanderen/building-registry/commit/25f16977555691762aa77f78f48fbf21fc1132fe))
* remove buildingunit address relation from backoffice context ([75813a9](https://github.com/informatievlaanderen/building-registry/commit/75813a9324f0f159e0c59eec2985448ffdaf1a7a))

# [3.54.0](https://github.com/informatievlaanderen/building-registry/compare/v3.53.1...v3.54.0) (2023-01-09)


### Bug Fixes

* correct default offset address consumer ([4b37ef8](https://github.com/informatievlaanderen/building-registry/commit/4b37ef82ded7c2065f3cdcced5421bf4e25c4b2e))


### Features

* add ability to offset to address consumer ([d66a08c](https://github.com/informatievlaanderen/building-registry/commit/d66a08c4db1b9b38af288cbe1ef5bb7051bb47e4))
* detach address when retiring building unit ([fa39732](https://github.com/informatievlaanderen/building-registry/commit/fa397322142220a340e57af3ef78cae2fdc5f130))

## [3.53.1](https://github.com/informatievlaanderen/building-registry/compare/v3.53.0...v3.53.1) (2023-01-06)


### Bug Fixes

* add consumeraddress to appsettings ([2af3c2a](https://github.com/informatievlaanderen/building-registry/commit/2af3c2a3bb20bd529253763f0a8edd73c1ad7430))

# [3.53.0](https://github.com/informatievlaanderen/building-registry/compare/v3.52.1...v3.53.0) (2023-01-05)


### Bug Fixes

* sqs action names ([4fa7884](https://github.com/informatievlaanderen/building-registry/commit/4fa7884997d6d59d5ccc6571ee56ff2b3ff6f7dd))


### Features

* handle detach address in back office projection ([4540322](https://github.com/informatievlaanderen/building-registry/commit/45403221c657c778abd3f182436af56da5f7ef1e))

## [3.52.1](https://github.com/informatievlaanderen/building-registry/compare/v3.52.0...v3.52.1) (2023-01-05)


### Bug Fixes

* misisng IAdresses registration in Lambda ([ae46b29](https://github.com/informatievlaanderen/building-registry/commit/ae46b2952550ced20d9e6218a7d578eb3509eb05))

# [3.52.0](https://github.com/informatievlaanderen/building-registry/compare/v3.51.1...v3.52.0) (2023-01-03)


### Bug Fixes

* boo boo ([7e54da3](https://github.com/informatievlaanderen/building-registry/commit/7e54da3c2b9efdf08f305f93a40d73e8272715f8))
* semantic release ([8db9b69](https://github.com/informatievlaanderen/building-registry/commit/8db9b69e2fd9f7492f282eb1a7fca73bbb3b63de))
* spelling of backoffice ([54e338d](https://github.com/informatievlaanderen/building-registry/commit/54e338dd0b6368390c2f1bd8bd46bd36dd5dd253))
* trigger ([51414eb](https://github.com/informatievlaanderen/building-registry/commit/51414ebe15d9c2fa41344cfe082af5082132511f))


### Features

* add backoffice projection ([0c951d6](https://github.com/informatievlaanderen/building-registry/commit/0c951d6120ea0a2144dcbc268d3d156af437cbf8))
* add producer ([#848](https://github.com/informatievlaanderen/building-registry/issues/848)) ([387a432](https://github.com/informatievlaanderen/building-registry/commit/387a432f4564c92b94db56ef71781177d31443f4))
* add snapshot producer ([8f69a2a](https://github.com/informatievlaanderen/building-registry/commit/8f69a2aac8f9bbd6f10b2868c8cd4052a353b242))
* remove building ([bda5950](https://github.com/informatievlaanderen/building-registry/commit/bda595014c51c2f6d385de8b6874099d22e7cd26))

## [3.51.1](https://github.com/informatievlaanderen/building-registry/compare/v3.51.0...v3.51.1) (2022-12-22)

# [3.51.0](https://github.com/informatievlaanderen/building-registry/compare/v3.50.0...v3.51.0) (2022-12-22)


### Features

* address commandhandling consumer ([c869fcf](https://github.com/informatievlaanderen/building-registry/commit/c869fcfa74dd2ca17b19f8e603392d515dba3411))

# [3.50.0](https://github.com/informatievlaanderen/building-registry/compare/v3.49.0...v3.50.0) (2022-12-20)


### Features

* add attach/detach address projections ([7e2ff70](https://github.com/informatievlaanderen/building-registry/commit/7e2ff708d62816577f4a9ab568e02b5ed32e6599))
* detach addres from building unit backoffice api ([0e131ca](https://github.com/informatievlaanderen/building-registry/commit/0e131cad43a5330fa1f1dddf6b8b7231fe107f4b))
* detach address from building unit ([84a26b6](https://github.com/informatievlaanderen/building-registry/commit/84a26b6bce7dd4591711cee963fc1fa1ebb3ac27))


### Performance Improvements

* add indexes building extract ([4bcc059](https://github.com/informatievlaanderen/building-registry/commit/4bcc05967d968bcd326236ce564b05fb10f13f4f))

# [3.49.0](https://github.com/informatievlaanderen/building-registry/compare/v3.48.11...v3.49.0) (2022-12-20)


### Bug Fixes

* correct release.yml ([0d19efd](https://github.com/informatievlaanderen/building-registry/commit/0d19efdb89d953e407ad675a24d6ef9264c014b2))
* description attach address ([2534072](https://github.com/informatievlaanderen/building-registry/commit/253407256d30261c757385d232da467920dccab4))
* determine start position migration ([8bec901](https://github.com/informatievlaanderen/building-registry/commit/8bec90147e1b03ac0b62969d7ba3f380d0ea8510))
* remove old workflows ([dbb4b06](https://github.com/informatievlaanderen/building-registry/commit/dbb4b067d17ca87ca7724e6343424b33bb3cd591))


### Features

* attach address to building unit ([8c9a407](https://github.com/informatievlaanderen/building-registry/commit/8c9a40786ac5cdbda18c65b2151cc60930b8cbde))


### Performance Improvements

* add indexes for boundinbox extract GAWR-4162 ([b7cfe47](https://github.com/informatievlaanderen/building-registry/commit/b7cfe4765455ab783656c7cb87c43e45ad50363b))

## [3.48.11](https://github.com/informatievlaanderen/building-registry/compare/v3.48.10...v3.48.11) (2022-12-04)


### Bug Fixes

* remove projections-legacy & projections-extract ([181a56c](https://github.com/informatievlaanderen/building-registry/commit/181a56cc33dbface8945fbf1d2e483d014fdd653))

## [3.48.10](https://github.com/informatievlaanderen/building-registry/compare/v3.48.9...v3.48.10) (2022-12-04)


### Bug Fixes

* fix lambda source folder ([7beb80e](https://github.com/informatievlaanderen/building-registry/commit/7beb80e6f36a8b11b9a1963f3ce73fe8b577672b))

## [3.48.9](https://github.com/informatievlaanderen/building-registry/compare/v3.48.8...v3.48.9) (2022-12-04)


### Bug Fixes

* remove unused identifier ([8f84446](https://github.com/informatievlaanderen/building-registry/commit/8f844467bc4e429fdd382ea8d980b1321bc58595))

## [3.48.8](https://github.com/informatievlaanderen/building-registry/compare/v3.48.7...v3.48.8) (2022-12-04)


### Bug Fixes

* remove projections-legacy & projections-extract ([6816c91](https://github.com/informatievlaanderen/building-registry/commit/6816c91943f7d1f37ac652af38c0da9799bcfd22))

## [3.48.7](https://github.com/informatievlaanderen/building-registry/compare/v3.48.6...v3.48.7) (2022-12-04)


### Bug Fixes

* add docker images ([d3de23c](https://github.com/informatievlaanderen/building-registry/commit/d3de23ceb8ecf30b3865eb995657e3a5cd3a8106))

## [3.48.6](https://github.com/informatievlaanderen/building-registry/compare/v3.48.5...v3.48.6) (2022-12-03)


### Bug Fixes

* change workflow ([d89d966](https://github.com/informatievlaanderen/building-registry/commit/d89d966e3ddbf71e81a567f6dec203cc6da729ec))

## [3.48.5](https://github.com/informatievlaanderen/building-registry/compare/v3.48.4...v3.48.5) (2022-11-25)


### Bug Fixes

* clear dbcontext in address consumer when primary key violation ([7774051](https://github.com/informatievlaanderen/building-registry/commit/7774051e751b28f086ec9337aad65fb219cfe347))

## [3.48.4](https://github.com/informatievlaanderen/building-registry/compare/v3.48.3...v3.48.4) (2022-11-25)


### Bug Fixes

* catch sql duplicate key exception in address consumer ([2d922d3](https://github.com/informatievlaanderen/building-registry/commit/2d922d3db039afae95d3e4549247bed018723c6b))
* how we catch dbUpdateException in address consumer ([#817](https://github.com/informatievlaanderen/building-registry/issues/817)) ([a7a88c4](https://github.com/informatievlaanderen/building-registry/commit/a7a88c4b095e83716e1d735ea7be2b5ff1c7008f))

## [3.48.3](https://github.com/informatievlaanderen/building-registry/compare/v3.48.2...v3.48.3) (2022-11-22)


### Bug Fixes

* consume AddressWasCorrectedFromRetiredToCurrent ([1448a97](https://github.com/informatievlaanderen/building-registry/commit/1448a971de41a5dd4f3ab966836aafbf65d88390))

## [3.48.2](https://github.com/informatievlaanderen/building-registry/compare/v3.48.1...v3.48.2) (2022-11-18)


### Bug Fixes

* extract v1 now has deviation value ([943994c](https://github.com/informatievlaanderen/building-registry/commit/943994c1ed62a91171ff66e56a508184aeb9352a))

## [3.48.1](https://github.com/informatievlaanderen/building-registry/compare/v3.48.0...v3.48.1) (2022-11-18)


### Bug Fixes

* change type deviation in extract GAWR-4038 ([74f57e6](https://github.com/informatievlaanderen/building-registry/commit/74f57e65d8932388f07d6f2df24dc161226e30b7))
* correct docs GAWR-4036 ([1e4f698](https://github.com/informatievlaanderen/building-registry/commit/1e4f698c1dd2e2fb140817681e6576bf06ccb28e))

# [3.48.0](https://github.com/informatievlaanderen/building-registry/compare/v3.47.0...v3.48.0) (2022-11-17)


### Bug Fixes

* rename AfwijkingWerdVastgesteld to AfwijkingVastgesteld ([e175621](https://github.com/informatievlaanderen/building-registry/commit/e175621bcc5c0a75680dfc24b911939ac91efe8e))
* subtitle regulate deregulate bu ([945fb13](https://github.com/informatievlaanderen/building-registry/commit/945fb13fae58a1b3516815a6b5dec3cc21539d24))


### Features

* add HasDeviation to building unit syndication response ([dddea49](https://github.com/informatievlaanderen/building-registry/commit/dddea49466c8a33e977cd2aa638454a41986d70f))

# [3.47.0](https://github.com/informatievlaanderen/building-registry/compare/v3.46.0...v3.47.0) (2022-11-15)


### Bug Fixes

* error code and messages ([e48d2ae](https://github.com/informatievlaanderen/building-registry/commit/e48d2ae87430628898cac9dbcd2a4dcdfeeda4f0))
* error code removed bu ([52238cc](https://github.com/informatievlaanderen/building-registry/commit/52238cc513e1996e1877cdf273feb621c39241fc))
* include AddressPersistentLocalIds in BuildingUnitRemovalWasCorrected ([56d2b7c](https://github.com/informatievlaanderen/building-registry/commit/56d2b7ca4468eee987f81f1945c3c37ac06f7ac3))


### Features

* add building unit HasDeviation to api responses ([12d0592](https://github.com/informatievlaanderen/building-registry/commit/12d0592d6807511bf4baab1d407ddaee52304240))
* add building unit HasDeviation to projections - EF Migrations ([eea4956](https://github.com/informatievlaanderen/building-registry/commit/eea4956c9e0b8263a40d79f7092846fb0fe0e4ba))
* set building unit HasDeviation in projections ([507bb10](https://github.com/informatievlaanderen/building-registry/commit/507bb1089f440641eae7892e462e6d44dfb95ee2))

# [3.46.0](https://github.com/informatievlaanderen/building-registry/compare/v3.45.0...v3.46.0) (2022-11-14)


### Bug Fixes

* BuildingOutlineWasChanged projections ([f75ee63](https://github.com/informatievlaanderen/building-registry/commit/f75ee6356d5df284c2e4b337834037f7d7390857))


### Features

* add regularize bu backoffice ([e70c63a](https://github.com/informatievlaanderen/building-registry/commit/e70c63a47d7f264e48d9cb2b0a98b82d1ac320e8))
* BuildingUnitRemovalWasCorrected projections ([460e08a](https://github.com/informatievlaanderen/building-registry/commit/460e08a018bf693b62833ba4c9338e9edb1b5eb2))
* deregulate building unit backoffice ([7d94116](https://github.com/informatievlaanderen/building-registry/commit/7d94116f6ba3d9fec9945acc065e377be6c6f2d0))
* deregulate building unit domain ([87d3c87](https://github.com/informatievlaanderen/building-registry/commit/87d3c87e031fde17c21673789585bf9fc610323d))
* regularize bu domain ([dbd5200](https://github.com/informatievlaanderen/building-registry/commit/dbd52000cd6a40723500a18f8cb27b95044b4477))

# [3.45.0](https://github.com/informatievlaanderen/building-registry/compare/v3.44.0...v3.45.0) (2022-11-14)


### Features

* correct common BU removal on plan BU domain ([cf286b2](https://github.com/informatievlaanderen/building-registry/commit/cf286b26d2e9fd039a3463342f7f241824e4a23f))

# [3.44.0](https://github.com/informatievlaanderen/building-registry/compare/v3.43.0...v3.44.0) (2022-11-09)


### Bug Fixes

* points must form a closed linestring ([4fbf478](https://github.com/informatievlaanderen/building-registry/commit/4fbf478ee88d9734fdad53ef95f789db14ce783c))
* update hash in legacy buildingUnitDetailV2 for BuildingUnitPositionWasCorrected ([8649f99](https://github.com/informatievlaanderen/building-registry/commit/8649f99c1048e7c607714cb751b892a6c93e65f5))
* validation error code for CorrectBuildingUnitRetirement building… ([#790](https://github.com/informatievlaanderen/building-registry/issues/790)) ([cb383b7](https://github.com/informatievlaanderen/building-registry/commit/cb383b72ae65e0e5d35c60922a6ea4d0aff5561b))


### Features

* remove common BU when last not removed BU ([#791](https://github.com/informatievlaanderen/building-registry/issues/791)) ([50557ed](https://github.com/informatievlaanderen/building-registry/commit/50557ed4828e38edc0127d44ede1ed9e9e5296a1))

# [3.43.0](https://github.com/informatievlaanderen/building-registry/compare/v3.42.0...v3.43.0) (2022-11-08)


### Bug Fixes

* error message missing punctuation mark ([f2fe9e1](https://github.com/informatievlaanderen/building-registry/commit/f2fe9e1a3fa3649d816f3015c9dce1de3d2c52ae))
* punctuation mark tests ([6359166](https://github.com/informatievlaanderen/building-registry/commit/635916641487b1fbea5c533d973e89e9e201400c))


### Features

* building status validation for retire buildingunit ([e06df11](https://github.com/informatievlaanderen/building-registry/commit/e06df117629fc3646a5f02f5aa522e75a9a33619))
* split position changes into separate event for CorrectedFromRetiredToRealized & WasCorrectedFromNotRealizedToPlanned ([53e511f](https://github.com/informatievlaanderen/building-registry/commit/53e511f5b4a8ecae6d11a28686bc97d2244edd24))

# [3.42.0](https://github.com/informatievlaanderen/building-registry/compare/v3.41.0...v3.42.0) (2022-11-08)


### Bug Fixes

* correct buildingunit retirement common unit logic ([ccd0e76](https://github.com/informatievlaanderen/building-registry/commit/ccd0e7624e5742eaeb23d6a34030c31ecdda3014))


### Features

* add remove building unit domain ([ec45ee4](https://github.com/informatievlaanderen/building-registry/commit/ec45ee4ecbb363ab394e572db5046d2a5bd1e480))

# [3.41.0](https://github.com/informatievlaanderen/building-registry/compare/v3.40.3...v3.41.0) (2022-11-07)


### Bug Fixes

* syndication of address id type GAWR-3952 ([33cd6e1](https://github.com/informatievlaanderen/building-registry/commit/33cd6e1888e148447ab81e2c2fdd7db93e9501b4))


### Features

* validate building status when changing building outline ([865b11e](https://github.com/informatievlaanderen/building-registry/commit/865b11ed625d4d8f6ac09cd6cffced548b224ded))

## [3.40.3](https://github.com/informatievlaanderen/building-registry/compare/v3.40.2...v3.40.3) (2022-11-07)


### Bug Fixes

* wms buildingunit assign id when already removed GAWR-3945 ([01f4f0e](https://github.com/informatievlaanderen/building-registry/commit/01f4f0ebaeef7512a166f2bbe5ba3c7632a36860))

## [3.40.2](https://github.com/informatievlaanderen/building-registry/compare/v3.40.1...v3.40.2) (2022-11-04)


### Bug Fixes

* add example request correct position bu ([4d9e221](https://github.com/informatievlaanderen/building-registry/commit/4d9e2216a335fe79e6f07fbe673a87dcd0f6347c))
* change error messages building invalid status on correct bu position ([8f1ca81](https://github.com/informatievlaanderen/building-registry/commit/8f1ca811b54b1c6ec253659fd78a692f4321f33e))

## [3.40.1](https://github.com/informatievlaanderen/building-registry/compare/v3.40.0...v3.40.1) (2022-11-03)


### Bug Fixes

* correct workflow ([1dc73d6](https://github.com/informatievlaanderen/building-registry/commit/1dc73d6b3c5143d54c5bb4bfc079b7b6edf89403))
* flaky tests ([6823f3b](https://github.com/informatievlaanderen/building-registry/commit/6823f3b084874e75a57d7e80df48994a449743c2))
* retire bu route ([30510be](https://github.com/informatievlaanderen/building-registry/commit/30510beaeeb733d29f75e2fcde1d5d7597f70854))
* title event BuildingOutlineWasChanged ([b761732](https://github.com/informatievlaanderen/building-registry/commit/b761732c41815e46e7b82ddd37a520e8f7549b8a))

# [3.40.0](https://github.com/informatievlaanderen/building-registry/compare/v3.39.2...v3.40.0) (2022-11-02)


### Bug Fixes

* add nuget to dependabot ([4109f3f](https://github.com/informatievlaanderen/building-registry/commit/4109f3fbe597a1241a883336d88c4fe6d526d5dc))
* enable pr's & coverage ([3919b7d](https://github.com/informatievlaanderen/building-registry/commit/3919b7dafa5b3686a5af8f5d67bc469ca470561c))
* make IHasBackOfficeRequest covariant ([194da89](https://github.com/informatievlaanderen/building-registry/commit/194da89d4967894efc62adf11c4775e6fbd19872))
* unit test common buildingunit ([b7d999d](https://github.com/informatievlaanderen/building-registry/commit/b7d999df6820b5c1e9c0ef75c3b6dd01c8e5ce89))
* use VBR_SONAR_TOKEN ([2bb162d](https://github.com/informatievlaanderen/building-registry/commit/2bb162d8e01b2328dcb1c9ae39dbc905fa800cc7))
* use VBR_SONAR_TOKEN ([f7d8078](https://github.com/informatievlaanderen/building-registry/commit/f7d8078141d249d58007a72446e58240dee308cd))


### Features

* add correct position building unit domain ([af83878](https://github.com/informatievlaanderen/building-registry/commit/af838788def28ec713188c9f808e3f0b2494938f))
* change geometry outlined building - backoffice ([cda5f72](https://github.com/informatievlaanderen/building-registry/commit/cda5f7228db14fcfdaa44f0821dde845b046fa49))
* change geomettry outlined building domain ([321d51a](https://github.com/informatievlaanderen/building-registry/commit/321d51ac680b4d6d1d1f65bac805611446665e7c))
* common buildingunit logic when retiring a buildingunit ([e6ca54b](https://github.com/informatievlaanderen/building-registry/commit/e6ca54bf9242c88ea3e5514ab86997b213d54b21))
* correct building unit position on BuildingUnitWasCorrectedFromNotRealizedToPlanned and BuildingUnitWasCorrectedFromRetiredToRealized ([4af463a](https://github.com/informatievlaanderen/building-registry/commit/4af463af776f4e60fc70b7acdaa07535db420b1e))
* correct buildingunit retirement ([9850056](https://github.com/informatievlaanderen/building-registry/commit/98500561c91d50f0f1b60e67b7a7686e94803a23))
* correct position buildingunit backofffice ([76cfae7](https://github.com/informatievlaanderen/building-registry/commit/76cfae7eb732cc081ddfdd470feab904eb027af9))
* correct position buildingunit backofffice ([845c23a](https://github.com/informatievlaanderen/building-registry/commit/845c23abb1810ce151a84806be36895c009c6dbe))
* properly implement HandleAggregateIdIsNotFoundException ([e178a60](https://github.com/informatievlaanderen/building-registry/commit/e178a607ff2e9ff9fe0025db15f25ef72e7dcc62))
* retire buildingunit ([5bb6b8e](https://github.com/informatievlaanderen/building-registry/commit/5bb6b8eb1323973463536eec9b29a8893dda707f))

## [3.39.2](https://github.com/informatievlaanderen/building-registry/compare/v3.39.1...v3.39.2) (2022-10-27)


### Bug Fixes

* correct building unit correct realization error message ([dfc43f9](https://github.com/informatievlaanderen/building-registry/commit/dfc43f9ab77017d1cf73451b72f32c3e40d5ef7e))
* override InnerMapDomainException ([cce031c](https://github.com/informatievlaanderen/building-registry/commit/cce031ccb547dc900de328456a6f0d3a89fc84bd))
* use BasisRegisters.Sqs package ([2dcb7ef](https://github.com/informatievlaanderen/building-registry/commit/2dcb7efb5d7396a827e005feb82cec07faeb7342))

## [3.39.1](https://github.com/informatievlaanderen/building-registry/compare/v3.39.0...v3.39.1) (2022-10-26)


### Bug Fixes

* common building unit validations ([5cf254d](https://github.com/informatievlaanderen/building-registry/commit/5cf254d46049bf9af8818426666bacc177e97d3e))
* error code common building unit ([d6e2843](https://github.com/informatievlaanderen/building-registry/commit/d6e284397cc988e41220425f239b4c268ca7a83f))

# [3.39.0](https://github.com/informatievlaanderen/building-registry/compare/v3.38.1...v3.39.0) (2022-10-26)


### Features

* add BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized ([3c21148](https://github.com/informatievlaanderen/building-registry/commit/3c211482f5311a1e7b87db80524038ef3efaf30b))
* add BuildingUnitWasRealizedBecauseBuildingWasRealized ([05ad8db](https://github.com/informatievlaanderen/building-registry/commit/05ad8db63ba219b58c323df35ffb2cfd98b9b6c6))

## [3.38.1](https://github.com/informatievlaanderen/building-registry/compare/v3.38.0...v3.38.1) (2022-10-26)

# [3.38.0](https://github.com/informatievlaanderen/building-registry/compare/v3.37.3...v3.38.0) (2022-10-25)


### Bug Fixes

* add common building unit to backoffice context ([437c100](https://github.com/informatievlaanderen/building-registry/commit/437c100dce72cb1236f96d2c082c21646378c9ac))
* prevent half consumption of kafka GAWR-3879 ([70b475b](https://github.com/informatievlaanderen/building-registry/commit/70b475b4c10ea765aaec1ae5deb058792dc57e30))


### Features

* add building validation for correcting not realize building unit ([52a90c8](https://github.com/informatievlaanderen/building-registry/commit/52a90c869cfcf8b5aecfd9814fe08efb7f6e27a4))
* common building unit add or status changes ([691e9c4](https://github.com/informatievlaanderen/building-registry/commit/691e9c4d459c497a019dff393424f3bfee234c67))
* notrealize or retire common building unit ([f16e90c](https://github.com/informatievlaanderen/building-registry/commit/f16e90cd49f03239b0c1b7a48c92ec0afb1dcf73))

## [3.37.3](https://github.com/informatievlaanderen/building-registry/compare/v3.37.2...v3.37.3) (2022-10-20)


### Bug Fixes

* error code CorrectUnderConstruction GAWR-3856 ([9c33b45](https://github.com/informatievlaanderen/building-registry/commit/9c33b457652afb7111dfb6d20bc7e5bc9d7b4086))

## [3.37.2](https://github.com/informatievlaanderen/building-registry/compare/v3.37.1...v3.37.2) (2022-10-19)


### Bug Fixes

* error code for invalid BuildingGeometryMethod ([af00816](https://github.com/informatievlaanderen/building-registry/commit/af0081667b31f7e148e5d62a1e56c2783889435d))
* make lambda request base classes abstract ([3d0086e](https://github.com/informatievlaanderen/building-registry/commit/3d0086e68b398c8e17d80ad3ee32a71f356a271f))
* map to correct sqs request in api ([efd18fc](https://github.com/informatievlaanderen/building-registry/commit/efd18fc04255d0992e9eb99970b85e2c1918d6b8))

## [3.37.1](https://github.com/informatievlaanderen/building-registry/compare/v3.37.0...v3.37.1) (2022-10-18)


### Bug Fixes

* event description ([e3b5bd2](https://github.com/informatievlaanderen/building-registry/commit/e3b5bd21a85b727ba7d8daa72e8ed66765c23468))

# [3.37.0](https://github.com/informatievlaanderen/building-registry/compare/v3.36.0...v3.37.0) (2022-10-18)


### Features

* correct notrealize building unit ([7917b42](https://github.com/informatievlaanderen/building-registry/commit/7917b426fd14e17a09612b107ebc09e6df10d772))
* correct notrealize buildingunit backoffice + projections ([a96ce94](https://github.com/informatievlaanderen/building-registry/commit/a96ce94f2a0d50f9fec67735eef40511587f22d2))
* correct realize building unit ([db17531](https://github.com/informatievlaanderen/building-registry/commit/db175312ac2c80a8534a4835e52dc7525a3bd833))
* correct realize building unit backoffice ([d80ad91](https://github.com/informatievlaanderen/building-registry/commit/d80ad9126270267e8e52ab89306e86424826a89a))

# [3.36.0](https://github.com/informatievlaanderen/building-registry/compare/v3.35.0...v3.36.0) (2022-10-17)


### Features

* consume address status changes ([3ef7a6d](https://github.com/informatievlaanderen/building-registry/commit/3ef7a6d47d1f23d726352b9ca73f50c44bee8b8f))
* correct building place under construction ([13a3958](https://github.com/informatievlaanderen/building-registry/commit/13a39584d4c55783fe87cc4ccb91a24d8a6bdf52))
* correct not-realization building ([970595a](https://github.com/informatievlaanderen/building-registry/commit/970595ad93e2e9c9879623baecc1d7b4691bbe06))

# [3.35.0](https://github.com/informatievlaanderen/building-registry/compare/v3.34.3...v3.35.0) (2022-10-14)


### Features

* correct building realization backoffice + projections ([01c491a](https://github.com/informatievlaanderen/building-registry/commit/01c491ac26f911918ce554451c261d65325a2e9b))
* correct realize of building in domain ([d9a9c6a](https://github.com/informatievlaanderen/building-registry/commit/d9a9c6aeaf7cccc7a53688abbb13832ccad31f63))
* throw when sqs request is not mapped to lambda request ([172e133](https://github.com/informatievlaanderen/building-registry/commit/172e1337c83dae4b61c36d38ffc3efa8c727ae48))

## [3.34.3](https://github.com/informatievlaanderen/building-registry/compare/v3.34.2...v3.34.3) (2022-10-13)


### Bug Fixes

* plan buildingunit in no existing building gives 400 ([e14d98b](https://github.com/informatievlaanderen/building-registry/commit/e14d98b33b75d3f6aa00bf06d85dee84b76b0104))

## [3.34.2](https://github.com/informatievlaanderen/building-registry/compare/v3.34.1...v3.34.2) (2022-10-13)


### Bug Fixes

* verify if building exists by trying to read single message from stream ([15a7608](https://github.com/informatievlaanderen/building-registry/commit/15a7608670aa37df85aa20c1d3a7ba065cbc27e7))

## [3.34.1](https://github.com/informatievlaanderen/building-registry/compare/v3.34.0...v3.34.1) (2022-10-12)


### Bug Fixes

* use getstream metadata to check stream exists ([2ca75d9](https://github.com/informatievlaanderen/building-registry/commit/2ca75d92b7dd257cbfa2adb4beba20e540e46eed))

# [3.34.0](https://github.com/informatievlaanderen/building-registry/compare/v3.33.6...v3.34.0) (2022-10-12)


### Bug Fixes

* change workflow ([f197cdb](https://github.com/informatievlaanderen/building-registry/commit/f197cdb2795d6b0a621b4dae987baf25da6f7636))


### Features

* add existing building check in api before sending to handler ([fc916e9](https://github.com/informatievlaanderen/building-registry/commit/fc916e93e4eddc61b87c4a5b9913fa59ad0627a2))

## [3.33.6](https://github.com/informatievlaanderen/building-registry/compare/v3.33.5...v3.33.6) (2022-10-11)


### Bug Fixes

* confirm to serializable pattern ([95ee4e7](https://github.com/informatievlaanderen/building-registry/commit/95ee4e77253aac59a9b441ad257992546ddc5002))
* don't throw general exceptions ([54a8135](https://github.com/informatievlaanderen/building-registry/commit/54a8135a919f256a7d14b18576c08115ffafb238))
* public field to property ([51186a6](https://github.com/informatievlaanderen/building-registry/commit/51186a6732dac0558fbd6966697cf0b100978fcd))
* remove unused variable ([5913c4f](https://github.com/informatievlaanderen/building-registry/commit/5913c4ffbe11d4bd1932ec912703b4f80d9ab152))
* rename parameter ([0c5953b](https://github.com/informatievlaanderen/building-registry/commit/0c5953b429027699d890d7ae391737c06af17731))

## [3.33.5](https://github.com/informatievlaanderen/building-registry/compare/v3.33.4...v3.33.5) (2022-10-05)


### Bug Fixes

* xml serialization syndication ([f37df73](https://github.com/informatievlaanderen/building-registry/commit/f37df73c845474a7220d7c522d3676b9b2c59b89))

## [3.33.4](https://github.com/informatievlaanderen/building-registry/compare/v3.33.3...v3.33.4) (2022-10-04)


### Bug Fixes

* lambda container ticketing registration ([d07c1d5](https://github.com/informatievlaanderen/building-registry/commit/d07c1d5e5edd31477437c7d45200744b36981e16))

## [3.33.3](https://github.com/informatievlaanderen/building-registry/compare/v3.33.2...v3.33.3) (2022-10-04)


### Bug Fixes

* style to trigger build ([32edc7b](https://github.com/informatievlaanderen/building-registry/commit/32edc7bb3384737ee8845b3c67992f90bcf9ae55))

## [3.33.2](https://github.com/informatievlaanderen/building-registry/compare/v3.33.1...v3.33.2) (2022-10-04)


### Bug Fixes

* remove unused reference ([d28f33a](https://github.com/informatievlaanderen/building-registry/commit/d28f33a952c179892ed67ced4313fa2f67bd346e))

## [3.33.1](https://github.com/informatievlaanderen/building-registry/compare/v3.33.0...v3.33.1) (2022-09-28)


### Bug Fixes

* style to trigger build ([9a4b963](https://github.com/informatievlaanderen/building-registry/commit/9a4b9637d03416fb90c2e8d30c96a6db98019d85))

# [3.33.0](https://github.com/informatievlaanderen/building-registry/compare/v3.32.7...v3.33.0) (2022-09-28)


### Bug Fixes

* fix format strings ([aa167f2](https://github.com/informatievlaanderen/building-registry/commit/aa167f2a3f5027e06ecc548495911d56e0dba3ba))
* implement serializable correctly ([6e34a88](https://github.com/informatievlaanderen/building-registry/commit/6e34a88513400bea4e7730862410ca8bc8710155))
* remove extra braces ([07db5cd](https://github.com/informatievlaanderen/building-registry/commit/07db5cdb03d9dbdf909240773526028fe1fde964))


### Features

* refactor to async ([3699178](https://github.com/informatievlaanderen/building-registry/commit/36991788fc8d657bb2029b137b2f1d257d1b037b))
* refactor to async ([4f5c617](https://github.com/informatievlaanderen/building-registry/commit/4f5c617e147ac78658f4ddf14fc3a161cee2e934))

## [3.32.7](https://github.com/informatievlaanderen/building-registry/compare/v3.32.6...v3.32.7) (2022-09-19)


### Bug Fixes

* change order between building and building unit events ([3046769](https://github.com/informatievlaanderen/building-registry/commit/304676908875cc4108ca3ab52b4ac051ea756cb6))

## [3.32.6](https://github.com/informatievlaanderen/building-registry/compare/v3.32.5...v3.32.6) (2022-09-14)


### Bug Fixes

* translate common building unit in extract ([7a6a291](https://github.com/informatievlaanderen/building-registry/commit/7a6a29143ceeb89eb50c850451750ec12d0664a8))

## [3.32.5](https://github.com/informatievlaanderen/building-registry/compare/v3.32.4...v3.32.5) (2022-09-09)


### Bug Fixes

* fix s3 lambda destination ([8ffadc5](https://github.com/informatievlaanderen/building-registry/commit/8ffadc5e78a2a5f99e43281e0b69514f720b75c5))

## [3.32.4](https://github.com/informatievlaanderen/building-registry/compare/v3.32.3...v3.32.4) (2022-09-09)


### Bug Fixes

* event documentation ([548013c](https://github.com/informatievlaanderen/building-registry/commit/548013cf3e586b63cc8810c1c2c8ac3a503c287a))

## [3.32.3](https://github.com/informatievlaanderen/building-registry/compare/v3.32.2...v3.32.3) (2022-08-30)


### Bug Fixes

* include etag in legacy and oslo detail responses ([9a55148](https://github.com/informatievlaanderen/building-registry/commit/9a55148d2cd283b0421d85a7f7154c6e3ecc3252))

## [3.32.2](https://github.com/informatievlaanderen/building-registry/compare/v3.32.1...v3.32.2) (2022-08-29)


### Bug Fixes

* migrate building units using centroidwithinarea ([22c7804](https://github.com/informatievlaanderen/building-registry/commit/22c780486423ab1352d0c61614e9429b13aab742))

## [3.32.1](https://github.com/informatievlaanderen/building-registry/compare/v3.32.0...v3.32.1) (2022-08-24)


### Bug Fixes

* bump snapshotting ([24eeb83](https://github.com/informatievlaanderen/building-registry/commit/24eeb830254cff821e90f25ac6483f8abf589e27))

# [3.32.0](https://github.com/informatievlaanderen/building-registry/compare/v3.31.3...v3.32.0) (2022-08-23)


### Features

* check for duplicate persistentLocalId when adding buildingUnit ([f7ddb6b](https://github.com/informatievlaanderen/building-registry/commit/f7ddb6b3bea67602e46a0b644a347dd87142484a))

## [3.31.3](https://github.com/informatievlaanderen/building-registry/compare/v3.31.2...v3.31.3) (2022-08-17)


### Bug Fixes

* review prs ([378bf99](https://github.com/informatievlaanderen/building-registry/commit/378bf992ca5432e77a511eb5b9cbbfdf34eb830e)), closes [#640](https://github.com/informatievlaanderen/building-registry/issues/640) [#682](https://github.com/informatievlaanderen/building-registry/issues/682)

## [3.31.2](https://github.com/informatievlaanderen/building-registry/compare/v3.31.1...v3.31.2) (2022-08-16)

## [3.31.1](https://github.com/informatievlaanderen/building-registry/compare/v3.31.0...v3.31.1) (2022-08-16)


### Bug Fixes

* automatic common building unit (review) ([e9865b7](https://github.com/informatievlaanderen/building-registry/commit/e9865b7ba73da0bc5288064724689239092a3e94))
* Make namespace import commands backwards compatible ([a96d3c7](https://github.com/informatievlaanderen/building-registry/commit/a96d3c756dbbf4b7c7d8190ce1d974718edba980))

# [3.31.0](https://github.com/informatievlaanderen/building-registry/compare/v3.30.13...v3.31.0) (2022-08-12)


### Features

* remove redundant check on PositionGeometryMethod ([061d4cd](https://github.com/informatievlaanderen/building-registry/commit/061d4cd4d089c286d6ceea3a72d1c54450f7c07d))

## [3.30.13](https://github.com/informatievlaanderen/building-registry/compare/v3.30.12...v3.30.13) (2022-08-11)


### Bug Fixes

* remove wrongly added file Q ([6cf7f3e](https://github.com/informatievlaanderen/building-registry/commit/6cf7f3e1e3369a8c2e6e9a158db78cadd2ece333))

## [3.30.12](https://github.com/informatievlaanderen/building-registry/compare/v3.30.11...v3.30.12) (2022-08-10)


### Bug Fixes

* remove sync tag from BuildingWasMarkedAsMigrated ([c030471](https://github.com/informatievlaanderen/building-registry/commit/c0304719c4db406adf2d25e428538877ed64eb9d))

## [3.30.11](https://github.com/informatievlaanderen/building-registry/compare/v3.30.10...v3.30.11) (2022-08-03)


### Bug Fixes

* add migration script for v2 listcount views ([d43b0ca](https://github.com/informatievlaanderen/building-registry/commit/d43b0ca66d021b68e89b654ff45c20bb955cf662))
* test ([2a8dff4](https://github.com/informatievlaanderen/building-registry/commit/2a8dff43136126581b5f2e095c315f24a6454520))
* validation error when aggregatenotfound on plan buildingunit ([4193d8f](https://github.com/informatievlaanderen/building-registry/commit/4193d8fd40d53374ec12426352545186887151ce))

## [3.30.10](https://github.com/informatievlaanderen/building-registry/compare/v3.30.9...v3.30.10) (2022-08-02)


### Bug Fixes

* aggregateNotFound Exception produces apiException ([87b301d](https://github.com/informatievlaanderen/building-registry/commit/87b301d69ff61e0d57cd610615333a78bf0ab232))

## [3.30.9](https://github.com/informatievlaanderen/building-registry/compare/v3.30.8...v3.30.9) (2022-08-02)


### Bug Fixes

* legacy buildingunitV2 get handler to return addressId's ([c247e4a](https://github.com/informatievlaanderen/building-registry/commit/c247e4a20650cd0efd5ac93ab0238adb575cb969))

## [3.30.8](https://github.com/informatievlaanderen/building-registry/compare/v3.30.7...v3.30.8) (2022-08-01)


### Bug Fixes

* get buildingunit oslo handler ([0a8aa01](https://github.com/informatievlaanderen/building-registry/commit/0a8aa01d36a816e1004d079868d26a2421f40c4c))

## [3.30.7](https://github.com/informatievlaanderen/building-registry/compare/v3.30.6...v3.30.7) (2022-08-01)


### Bug Fixes

* add gebouwId to validation error ([20f9220](https://github.com/informatievlaanderen/building-registry/commit/20f92200cd139df975d734add21a8b98ff69f97c))

## [3.30.6](https://github.com/informatievlaanderen/building-registry/compare/v3.30.5...v3.30.6) (2022-07-29)


### Bug Fixes

* building id not found on plan building unit ([c025aa7](https://github.com/informatievlaanderen/building-registry/commit/c025aa71a7c6118b8064573fc526a2ebaf54d16d))
* buildingcontroller CrabGebouwen endpoint ([c4ff072](https://github.com/informatievlaanderen/building-registry/commit/c4ff0723d286e23839bff449d119eb84927f2bbc))

## [3.30.5](https://github.com/informatievlaanderen/building-registry/compare/v3.30.4...v3.30.5) (2022-07-29)


### Bug Fixes

* error code and message ([c484a9d](https://github.com/informatievlaanderen/building-registry/commit/c484a9d401014d609e65dd43285d228bd24a3653))

## [3.30.4](https://github.com/informatievlaanderen/building-registry/compare/v3.30.3...v3.30.4) (2022-07-28)


### Bug Fixes

* override Equals & comparison operators ([551fbaf](https://github.com/informatievlaanderen/building-registry/commit/551fbaf5dd5a68bb37094b441b27352288932cff))
* remove unused private fields ([91e9be2](https://github.com/informatievlaanderen/building-registry/commit/91e9be203bcf646a747c0b283190fe92809db704))

## [3.30.3](https://github.com/informatievlaanderen/building-registry/compare/v3.30.2...v3.30.3) (2022-07-28)


### Bug Fixes

* collapse if stmts ([c1e552d](https://github.com/informatievlaanderen/building-registry/commit/c1e552d504b766eacfe29488ca7c29f83913c253))
* correct param name ([a7544db](https://github.com/informatievlaanderen/building-registry/commit/a7544db68399cad3fbda49157fa81617724adb5f))
* don't nest ternary clauses ([59123c5](https://github.com/informatievlaanderen/building-registry/commit/59123c5996cd8fb671230216cb73c33d08c5e19c))
* fix field access ([a1ebf71](https://github.com/informatievlaanderen/building-registry/commit/a1ebf719100d2369ce2c2eec5ca126780f636503))
* remove new Guid() ([7482ff6](https://github.com/informatievlaanderen/building-registry/commit/7482ff6ba40e235f86e1ee8abeb87062e473e00e))
* remove null check ([4e7a106](https://github.com/informatievlaanderen/building-registry/commit/4e7a1060f31c83214d1261dff190327dcf368e8c))
* remove unnecessary casts ([50eaa64](https://github.com/informatievlaanderen/building-registry/commit/50eaa645cd22330630c71eabf5848eb21861519c))
* remove unused params ([3b23117](https://github.com/informatievlaanderen/building-registry/commit/3b23117f01305c138d56f0730cecbb0211c75f34))
* remove unused variables ([6cc7912](https://github.com/informatievlaanderen/building-registry/commit/6cc7912cb496652a7176c084742b1e985f7d6bf1))
* remove useless assignment ([cb6d64e](https://github.com/informatievlaanderen/building-registry/commit/cb6d64ece2b2d493d9a0f2d8cc031c2d58c0f193))
* seal EnvelopePartialRecord ([83054f8](https://github.com/informatievlaanderen/building-registry/commit/83054f8985e1b4f4e6e3a534a59d2487a3fadc5e))
* seal private non-derived types ([f733b4f](https://github.com/informatievlaanderen/building-registry/commit/f733b4f82a3be77c753d7ee73834fb52ad7bab62))
* simplify loops with linq ([1becf07](https://github.com/informatievlaanderen/building-registry/commit/1becf07be1e09679019db100efac44ca90c9c7a6))

## [3.30.2](https://github.com/informatievlaanderen/building-registry/compare/v3.30.1...v3.30.2) (2022-07-27)


### Bug Fixes

* buildingSyndication include BuildingUnitV2 table ([eaefb09](https://github.com/informatievlaanderen/building-registry/commit/eaefb099ee676f3f9f145381420035a0a400e80d))
* bump dependencies ([fa6525c](https://github.com/informatievlaanderen/building-registry/commit/fa6525c1d6c30f21017c98a731d6d9a1f0cd868c))
* fix empty methods ([bafd57d](https://github.com/informatievlaanderen/building-registry/commit/bafd57d80204797b042791ae7e9ca93b58c7f2e2))
* fix sonar fields visibility ([6e89072](https://github.com/informatievlaanderen/building-registry/commit/6e89072f2eb235013dfc442a5805562726cb6231))
* fix sonar general exceptions ([ad9ec82](https://github.com/informatievlaanderen/building-registry/commit/ad9ec82d749f21f193e8838893bc3b12316a0072))
* fix sonar parameter names ([bb5b5cd](https://github.com/informatievlaanderen/building-registry/commit/bb5b5cdf77537f1159c3a9c5acdbdbe55499597f))
* fix sonar protected ctor or static ([66fab11](https://github.com/informatievlaanderen/building-registry/commit/66fab1110384319f6e4a16e548c6b5a82e430c92))
* remove unused private members ([3655682](https://github.com/informatievlaanderen/building-registry/commit/3655682606e4b6599470b3a3ffa6ce4e2d94f1a1))
* Serializable exceptions ([6ff2893](https://github.com/informatievlaanderen/building-registry/commit/6ff28936608265c554c5afae4bd320b77deb45fd))

## [3.30.1](https://github.com/informatievlaanderen/building-registry/compare/v3.30.0...v3.30.1) (2022-07-27)


### Bug Fixes

* conform exceptions to serializable spec ([1d82017](https://github.com/informatievlaanderen/building-registry/commit/1d820173054eeea011dd9144696bd963a0796bae))
* fix sonar bugs ([a333fb4](https://github.com/informatievlaanderen/building-registry/commit/a333fb4563d852eb56939b28cf564c0b2dd0e2ee))

# [3.30.0](https://github.com/informatievlaanderen/building-registry/compare/v3.29.3...v3.30.0) (2022-07-27)


### Features

* not realize building ([9e5d411](https://github.com/informatievlaanderen/building-registry/commit/9e5d4113bf7f5b8cb1542ecafa7d2fb5b8044ec5))

## [3.29.3](https://github.com/informatievlaanderen/building-registry/compare/v3.29.2...v3.29.3) (2022-07-27)


### Bug Fixes

* fix lambda.zip location ([d221f65](https://github.com/informatievlaanderen/building-registry/commit/d221f65ef932c6a911584c4f06b2706b0e3eb46d))

## [3.29.2](https://github.com/informatievlaanderen/building-registry/compare/v3.29.1...v3.29.2) (2022-07-27)


### Bug Fixes

* finalize lambda deployment ([9aec4a5](https://github.com/informatievlaanderen/building-registry/commit/9aec4a5d308735b5e074de50908fa6439389a510))

## [3.29.1](https://github.com/informatievlaanderen/building-registry/compare/v3.29.0...v3.29.1) (2022-07-26)


### Bug Fixes

* copy lambda.zip to S3 bucket ([4add199](https://github.com/informatievlaanderen/building-registry/commit/4add199ed8f7f09362b0fbfd30b983450547fc1b))

# [3.29.0](https://github.com/informatievlaanderen/building-registry/compare/v3.28.2...v3.29.0) (2022-07-26)


### Features

* building unit not realize ([95dcebb](https://github.com/informatievlaanderen/building-registry/commit/95dcebb5774465b158974cd86bd1f93fa7d60536))


### Reverts

* Revert "fix: resolve sonar bugs" ([485995a](https://github.com/informatievlaanderen/building-registry/commit/485995a89c5b40d11dda8c7cfe76413a834c26c6))

## [3.28.2](https://github.com/informatievlaanderen/building-registry/compare/v3.28.1...v3.28.2) (2022-07-25)


### Bug Fixes

* update release.yml ([eb1d70d](https://github.com/informatievlaanderen/building-registry/commit/eb1d70d05f11b17aeb3fea12d89cb2628736acf7))

## [3.28.1](https://github.com/informatievlaanderen/building-registry/compare/v3.28.0...v3.28.1) (2022-07-25)


### Bug Fixes

* objectId for BuildingUnitResponse in Legacy GetHandler ([a9995a7](https://github.com/informatievlaanderen/building-registry/commit/a9995a7cfa248eb2a73ef99897b190164782a587))

# [3.28.0](https://github.com/informatievlaanderen/building-registry/compare/v3.27.3...v3.28.0) (2022-07-25)


### Bug Fixes

* build tests ([8d709c2](https://github.com/informatievlaanderen/building-registry/commit/8d709c21c1672fc0e562275b44ff1b9538ff3251))
* buildingUnitRealizedV2 event docs ([49a2e20](https://github.com/informatievlaanderen/building-registry/commit/49a2e205b59288bf7111c117048d10cfadef4036))


### Features

* plan building unit validation position ([5522fdd](https://github.com/informatievlaanderen/building-registry/commit/5522fddc05c048f5eb7b3284ad8a8cba4b01286e))

## [3.27.3](https://github.com/informatievlaanderen/building-registry/compare/v3.27.2...v3.27.3) (2022-07-25)


### Bug Fixes

* add build.yml & release.yml ([f518406](https://github.com/informatievlaanderen/building-registry/commit/f5184062d981e94705f6a7902ac31394966f5e66))

## [3.27.2](https://github.com/informatievlaanderen/building-registry/compare/v3.27.1...v3.27.2) (2022-07-23)


### Bug Fixes

* fix build (S3 access) ([1020c50](https://github.com/informatievlaanderen/building-registry/commit/1020c50ea1e56d793ee08659dff34e0036409332))
* rename ci to tests3 ([b85a1e7](https://github.com/informatievlaanderen/building-registry/commit/b85a1e7f0814ee0b5d2806ef9dbc44e3c2297a46))
* run tests3 manually ([18a199e](https://github.com/informatievlaanderen/building-registry/commit/18a199ef0c2e6fae318bad8a903f9c953d7226cf))

## [3.27.1](https://github.com/informatievlaanderen/building-registry/compare/v3.27.0...v3.27.1) (2022-07-22)


### Bug Fixes

* deploy lambda functions ([d172e29](https://github.com/informatievlaanderen/building-registry/commit/d172e29a8cceebc284ca0637145ea50309adeff8))
* deploy lambda functions ([0c9fda1](https://github.com/informatievlaanderen/building-registry/commit/0c9fda18a98b2cb2199f2d23b5d501b327e56c81))

# [3.27.0](https://github.com/informatievlaanderen/building-registry/compare/v3.26.1...v3.27.0) (2022-07-22)


### Bug Fixes

* add linux rid to lambda project ([ba5f2a6](https://github.com/informatievlaanderen/building-registry/commit/ba5f2a6c53dc9e4ea2ec911c4086865f10010978))
* add sqs & lambda to build ([3aa7f2c](https://github.com/informatievlaanderen/building-registry/commit/3aa7f2c366ec64f7e9511bffd4fb862d01515330))
* realize non existing building unit should return 404 ([a51ce03](https://github.com/informatievlaanderen/building-registry/commit/a51ce0327fdab0bd035e2d95eda1b184bf9fb9f5))


### Features

* automatic common building unit ([#637](https://github.com/informatievlaanderen/building-registry/issues/637)) ([c103ab1](https://github.com/informatievlaanderen/building-registry/commit/c103ab1bf978f4ab911ae8e2ecacadfac67f8116))
* realize building unit val4 ([bbc5bdb](https://github.com/informatievlaanderen/building-registry/commit/bbc5bdb412bfe1896a82a8770bc4bae4b69a6853))
* refactoring tryGetBuildingIdForBuildingUnit ([ab313df](https://github.com/informatievlaanderen/building-registry/commit/ab313dfa8f72c9336e07b64a7de35635911f9d7e))

## [3.26.1](https://github.com/informatievlaanderen/building-registry/compare/v3.26.0...v3.26.1) (2022-07-22)


### Bug Fixes

* turn off lambda processing ([598dbdc](https://github.com/informatievlaanderen/building-registry/commit/598dbdc7e0e9159dcd179f7914049f8c867ef560))

# [3.26.0](https://github.com/informatievlaanderen/building-registry/compare/v3.25.0...v3.26.0) (2022-07-18)


### Features

* plan building unit validition building istatus ([732bc1d](https://github.com/informatievlaanderen/building-registry/commit/732bc1d8de6baad90b05f80f3bdf62cc0e32dc08))

# [3.25.0](https://github.com/informatievlaanderen/building-registry/compare/v3.24.7...v3.25.0) (2022-07-15)


### Features

* add RealizeBecauseBuildingWasRealized on buildingunit ([ba3e9d4](https://github.com/informatievlaanderen/building-registry/commit/ba3e9d48227d4c8235be9c2a73f0182ac0c3d116))

## [3.24.7](https://github.com/informatievlaanderen/building-registry/compare/v3.24.6...v3.24.7) (2022-07-15)


### Bug Fixes

* add ticketing url ([0d01fc7](https://github.com/informatievlaanderen/building-registry/commit/0d01fc704bcfe02ff15d4f122e47ccbb634ad739))
* bump TicketingService.Abstractions dependency ([9ab31e1](https://github.com/informatievlaanderen/building-registry/commit/9ab31e1729d5d35bcd3324b7fb42ec8bc8d2d6fa))
* remove reference from csproj file ([8a3444d](https://github.com/informatievlaanderen/building-registry/commit/8a3444d0f931b85df7bdc20f1c0413f9b7a7e600))

## [3.24.6](https://github.com/informatievlaanderen/building-registry/compare/v3.24.5...v3.24.6) (2022-07-14)


### Bug Fixes

* use ticketing http proxy ([e361ef3](https://github.com/informatievlaanderen/building-registry/commit/e361ef3b7d4232720ef1123ba87758ac5a3a3992))

## [3.24.5](https://github.com/informatievlaanderen/building-registry/compare/v3.24.4...v3.24.5) (2022-07-13)


### Bug Fixes

* use ticketing system abstractions ([d2fb0ac](https://github.com/informatievlaanderen/building-registry/commit/d2fb0ac946e89740c356c0ba0460101e9e1818f2))

## [3.24.4](https://github.com/informatievlaanderen/building-registry/compare/v3.24.3...v3.24.4) (2022-07-12)

## [3.24.3](https://github.com/informatievlaanderen/building-registry/compare/v3.24.2...v3.24.3) (2022-07-11)


### Bug Fixes

* add assemblyinfo files ([eb619b3](https://github.com/informatievlaanderen/building-registry/commit/eb619b3e1689c9f0eeda57f0d7ca5936bb9eea1d))

## [3.24.2](https://github.com/informatievlaanderen/building-registry/compare/v3.24.1...v3.24.2) (2022-07-08)


### Bug Fixes

* use SqsQueueOptions ([f868500](https://github.com/informatievlaanderen/building-registry/commit/f86850001f41c546f299672a8a938b4d403d86a1))

## [3.24.1](https://github.com/informatievlaanderen/building-registry/compare/v3.24.0...v3.24.1) (2022-07-08)


### Bug Fixes

* lanbda sends result to ticket ([a5b4025](https://github.com/informatievlaanderen/building-registry/commit/a5b4025ea5401e1da992921441e1d2a5b728d229))

# [3.24.0](https://github.com/informatievlaanderen/building-registry/compare/v3.23.2...v3.24.0) (2022-07-07)


### Features

* remove deviatedBuildingUnitWasPlanned ([5dc0043](https://github.com/informatievlaanderen/building-registry/commit/5dc004386503588c8a24aced2ed48762a5942975))

## [3.23.2](https://github.com/informatievlaanderen/building-registry/compare/v3.23.1...v3.23.2) (2022-07-06)


### Bug Fixes

* fix projections hash & version according to buildingunit events ([a3d2f47](https://github.com/informatievlaanderen/building-registry/commit/a3d2f479c65778f72b3fbe7616ecc16a1ffd3754))

## [3.23.1](https://github.com/informatievlaanderen/building-registry/compare/v3.23.0...v3.23.1) (2022-07-06)


### Bug Fixes

* building aggregate root set hash on building unit command ([d325ca8](https://github.com/informatievlaanderen/building-registry/commit/d325ca8ebe3d79b2a8b5e8b1eabfb9b813215f51))

# [3.23.0](https://github.com/informatievlaanderen/building-registry/compare/v3.22.1...v3.23.0) (2022-07-06)


### Features

* realize buildingunit val3 ([fc3be8d](https://github.com/informatievlaanderen/building-registry/commit/fc3be8d63058a8e5ea88e4f8d86ca2e3b4a5b39d))
* take snapshot ([af99745](https://github.com/informatievlaanderen/building-registry/commit/af99745484779f12c438c0b261dd9f7c413791bf))

## [3.22.1](https://github.com/informatievlaanderen/building-registry/compare/v3.22.0...v3.22.1) (2022-07-05)


### Bug Fixes

* use ticketing abstractions ([99adea6](https://github.com/informatievlaanderen/building-registry/commit/99adea6f8d9fe4dbff275a604f2c0bdc3f888687))

# [3.22.0](https://github.com/informatievlaanderen/building-registry/compare/v3.21.1...v3.22.0) (2022-07-05)


### Features

* update buildingunit projections ([f70f625](https://github.com/informatievlaanderen/building-registry/commit/f70f6250b940a43210b2ccece155cc5cb2a0403a))

## [3.21.1](https://github.com/informatievlaanderen/building-registry/compare/v3.21.0...v3.21.1) (2022-07-03)


### Bug Fixes

* add retry per stream with backoff ([01b6130](https://github.com/informatievlaanderen/building-registry/commit/01b61301af50a1f54b9fcfc45aa8955d6019f021))

# [3.21.0](https://github.com/informatievlaanderen/building-registry/compare/v3.20.1...v3.21.0) (2022-07-01)


### Bug Fixes

* build ([7a6557a](https://github.com/informatievlaanderen/building-registry/commit/7a6557a8103cbdc7874707a45c128007535dbef1))


### Features

* add ticketing system ([969f592](https://github.com/informatievlaanderen/building-registry/commit/969f592ac812beaae1f5118aa4fda052c375a0ac))

## [3.20.1](https://github.com/informatievlaanderen/building-registry/compare/v3.20.0...v3.20.1) (2022-07-01)

# [3.20.0](https://github.com/informatievlaanderen/building-registry/compare/v3.19.1...v3.20.0) (2022-07-01)


### Features

* implement sqs + lambda for buildingunit plan & realize ([84c79f2](https://github.com/informatievlaanderen/building-registry/commit/84c79f2bfc27a42718592e2e007cc0d45d4fc4c5))

## [3.19.1](https://github.com/informatievlaanderen/building-registry/compare/v3.19.0...v3.19.1) (2022-06-30)


### Bug Fixes

* pr review ([183b537](https://github.com/informatievlaanderen/building-registry/commit/183b537953eea21f71c5f43e5aea65b9d22249ce))

# [3.19.0](https://github.com/informatievlaanderen/building-registry/compare/v3.18.5...v3.19.0) (2022-06-30)


### Features

* buildingunit plan & realize controllers ([c131dad](https://github.com/informatievlaanderen/building-registry/commit/c131dad9dc7f0eac525134f39996ce8a6ef0768d))

## [3.18.5](https://github.com/informatievlaanderen/building-registry/compare/v3.18.4...v3.18.5) (2022-06-30)


### Bug Fixes

* add LABEL to Dockerfile (for easier DataDog filtering) ([3e2d239](https://github.com/informatievlaanderen/building-registry/commit/3e2d23989a107467a367c5cc97651a2d2ba7bee3))

## [3.18.4](https://github.com/informatievlaanderen/building-registry/compare/v3.18.3...v3.18.4) (2022-06-29)

## [3.18.3](https://github.com/informatievlaanderen/building-registry/compare/v3.18.2...v3.18.3) (2022-06-29)


### Bug Fixes

* add message group id through persistentLocalId ([83c4cfb](https://github.com/informatievlaanderen/building-registry/commit/83c4cfbea369b61db68322beb1088100a811939f))

## [3.18.2](https://github.com/informatievlaanderen/building-registry/compare/v3.18.1...v3.18.2) (2022-06-28)


### Bug Fixes

* correct method of WMS ([ac47cc6](https://github.com/informatievlaanderen/building-registry/commit/ac47cc6e9a4618cd9afc71866000109f882b2478))

## [3.18.1](https://github.com/informatievlaanderen/building-registry/compare/v3.18.0...v3.18.1) (2022-06-28)


### Bug Fixes

* combine lambda projects ([8838e17](https://github.com/informatievlaanderen/building-registry/commit/8838e17662971962d7e335d4e0a33c901604b296))

# [3.18.0](https://github.com/informatievlaanderen/building-registry/compare/v3.17.2...v3.18.0) (2022-06-27)


### Bug Fixes

* add BuildingUnit Sqs components ([61d15ea](https://github.com/informatievlaanderen/building-registry/commit/61d15eaafaa1c215d561d3049fafbccc18e5a919))


### Features

* plan & realize building unit aggregate + tests ([0ff678d](https://github.com/informatievlaanderen/building-registry/commit/0ff678dd87d999ad95dc93652cc4b65404c88fb4))

## [3.17.2](https://github.com/informatievlaanderen/building-registry/compare/v3.17.1...v3.17.2) (2022-06-24)


### Bug Fixes

* one queue per api ([5f241a8](https://github.com/informatievlaanderen/building-registry/commit/5f241a8e2a3d56250b0564de3fb139594fbc89aa))

## [3.17.1](https://github.com/informatievlaanderen/building-registry/compare/v3.17.0...v3.17.1) (2022-06-24)


### Bug Fixes

* add order to planbuildingunit request ([977a12a](https://github.com/informatievlaanderen/building-registry/commit/977a12a5df05e11ef5e967edec7652f476efa5c3))

# [3.17.0](https://github.com/informatievlaanderen/building-registry/compare/v3.16.1...v3.17.0) (2022-06-24)


### Bug Fixes

* finish migrator ([9cf46b3](https://github.com/informatievlaanderen/building-registry/commit/9cf46b3a3c3f711cbc5365795a460bbe954bf08c))


### Features

* add planbuildingunit contract ([b9503d9](https://github.com/informatievlaanderen/building-registry/commit/b9503d9e59545335e722a6011fb8965773d2f39b))

## [3.16.1](https://github.com/informatievlaanderen/building-registry/compare/v3.16.0...v3.16.1) (2022-06-24)


### Bug Fixes

* add hexa notation docs extended wkb event info ([9fd531b](https://github.com/informatievlaanderen/building-registry/commit/9fd531b69c5ebd0605c83f2198eb4468dff97e01))
* correct errorcodes realize/underconstruction ([74c8fde](https://github.com/informatievlaanderen/building-registry/commit/74c8fde68abf08bba7c67e1a41d270c6184a20cf))

# [3.16.0](https://github.com/informatievlaanderen/building-registry/compare/v3.15.0...v3.16.0) (2022-06-23)


### Features

* add backoffice api lambda ([78c0a2d](https://github.com/informatievlaanderen/building-registry/commit/78c0a2df500d70b0a5cd3d91f720f1d1c966dfd7))

# [3.15.0](https://github.com/informatievlaanderen/building-registry/compare/v3.14.0...v3.15.0) (2022-06-23)


### Bug Fixes

* use documentation for invalid polygon validation ([42ea36c](https://github.com/informatievlaanderen/building-registry/commit/42ea36c3e44749904a78217247b012727d556633))


### Features

* implement backoffice sqs handlers ([ecfa267](https://github.com/informatievlaanderen/building-registry/commit/ecfa2679ac86962053601859e9774c463a0e7337))

# [3.14.0](https://github.com/informatievlaanderen/building-registry/compare/v3.13.3...v3.14.0) (2022-06-22)


### Bug Fixes

* correct sync feed handler ([a67ed0e](https://github.com/informatievlaanderen/building-registry/commit/a67ed0ed6179afd5a7477736048668b7391caa70))


### Features

* add backoffice handlers sqs project (to be implemented) ([e6da22a](https://github.com/informatievlaanderen/building-registry/commit/e6da22a4251fa9059def9ff2d9dfa0d153f1aaf6))
* realize & construct validations ([26b1045](https://github.com/informatievlaanderen/building-registry/commit/26b10457de74d8a7590af652f1a5830bb96bd286))

## [3.13.3](https://github.com/informatievlaanderen/building-registry/compare/v3.13.2...v3.13.3) (2022-06-22)


### Bug Fixes

* inject SqsOptions ([c184ee9](https://github.com/informatievlaanderen/building-registry/commit/c184ee9ace492ae3401541b9e9e7c41cdf1763a6))

## [3.13.2](https://github.com/informatievlaanderen/building-registry/compare/v3.13.1...v3.13.2) (2022-06-22)


### Bug Fixes

* extract with no building units won't crash ([8f92fe3](https://github.com/informatievlaanderen/building-registry/commit/8f92fe3298922635cfc14b915799dd232dba7051))

## [3.13.1](https://github.com/informatievlaanderen/building-registry/compare/v3.13.0...v3.13.1) (2022-06-21)


### Bug Fixes

* correct v2 event descriptions ([ace2f31](https://github.com/informatievlaanderen/building-registry/commit/ace2f31fdaa5990558813d9c352cc49a1d7f9406))
* simplify SqsOptions creation ([76c81a3](https://github.com/informatievlaanderen/building-registry/commit/76c81a3db1d1782d31a69d20e35fcf1937fdb997))

# [3.13.0](https://github.com/informatievlaanderen/building-registry/compare/v3.12.1...v3.13.0) (2022-06-21)


### Bug Fixes

* register MessageHandler ([5dff391](https://github.com/informatievlaanderen/building-registry/commit/5dff391414859956308ffd7de153ac2b2e8fbc41))
* remove CrabImport.json ([f4b632b](https://github.com/informatievlaanderen/building-registry/commit/f4b632b76e28f7e0ce5d194bcda8c5a797181a0f))


### Features

* add CrabImport lambda ([0a56522](https://github.com/informatievlaanderen/building-registry/commit/0a565220c255b47f9634bc74d22e3b4bafc764c7))
* add CrabImport lambda ([a56b332](https://github.com/informatievlaanderen/building-registry/commit/a56b332121a0982068a7aaa2d9747a815aab7099))
* add lambda ([#582](https://github.com/informatievlaanderen/building-registry/issues/582)) ([b03ba84](https://github.com/informatievlaanderen/building-registry/commit/b03ba84db9fd4086618c370c1a73ef26b2ebc43c))

## [3.12.1](https://github.com/informatievlaanderen/building-registry/compare/v3.12.0...v3.12.1) (2022-06-20)


### Bug Fixes

* etag for construct & realize ([1a5e44b](https://github.com/informatievlaanderen/building-registry/commit/1a5e44b253754d51ecdd47468d92a407bab0ddd3))

# [3.12.0](https://github.com/informatievlaanderen/building-registry/compare/v3.11.1...v3.12.0) (2022-06-20)


### Bug Fixes

* after testing ([1a99a98](https://github.com/informatievlaanderen/building-registry/commit/1a99a98e8647f4213bf24d9019feda7d946afece))
* typo `realiseren` ([c8c07da](https://github.com/informatievlaanderen/building-registry/commit/c8c07dad70cabe294d78cc2b1df8163332129165))


### Features

* add underConstruction & realize projections ([30635b5](https://github.com/informatievlaanderen/building-registry/commit/30635b5c7b3dba8aca9e035959d1814e3f2856d0))

## [3.11.1](https://github.com/informatievlaanderen/building-registry/compare/v3.11.0...v3.11.1) (2022-06-20)


### Bug Fixes

* add projection toggle in settings ([04ee9b1](https://github.com/informatievlaanderen/building-registry/commit/04ee9b130c92e86516331e22c1f77237db5ff4fe))

# [3.11.0](https://github.com/informatievlaanderen/building-registry/compare/v3.10.2...v3.11.0) (2022-06-20)


### Features

* add underConstruction & realize ([76cda71](https://github.com/informatievlaanderen/building-registry/commit/76cda71a156d6f117bff2634093a7cd9c3a6efba))

## [3.10.2](https://github.com/informatievlaanderen/building-registry/compare/v3.10.1...v3.10.2) (2022-06-15)


### Bug Fixes

* consumer connectionString in migrator ([7f40b04](https://github.com/informatievlaanderen/building-registry/commit/7f40b04e90ad54ccca5e622157a01298b3e2fe3f))

## [3.10.1](https://github.com/informatievlaanderen/building-registry/compare/v3.10.0...v3.10.1) (2022-06-15)


### Bug Fixes

* backoffice consumer connectionstring ([759b73c](https://github.com/informatievlaanderen/building-registry/commit/759b73cd232336ad90570bcc191092fea4271360))

# [3.10.0](https://github.com/informatievlaanderen/building-registry/compare/v3.9.4...v3.10.0) (2022-06-14)


### Features

* toggle projections ([267fe84](https://github.com/informatievlaanderen/building-registry/commit/267fe84843dfc9029c1ba3ffb5207341e5fccf94))

## [3.9.4](https://github.com/informatievlaanderen/building-registry/compare/v3.9.3...v3.9.4) (2022-06-13)


### Bug Fixes

* correct nupkg dependency abstractions ([b7a8c98](https://github.com/informatievlaanderen/building-registry/commit/b7a8c982077843b381ce4de1667f84ec660ac281))

## [3.9.3](https://github.com/informatievlaanderen/building-registry/compare/v3.9.2...v3.9.3) (2022-06-13)


### Bug Fixes

* remove double clustered index ([0d9a6c4](https://github.com/informatievlaanderen/building-registry/commit/0d9a6c4b74362b7e4cef5808a7e1343f2021207b))

## [3.9.2](https://github.com/informatievlaanderen/building-registry/compare/v3.9.1...v3.9.2) (2022-06-13)


### Bug Fixes

* correct syndication event dto ([1081cdd](https://github.com/informatievlaanderen/building-registry/commit/1081cddad0ca435ce7c04be5f4caa323ff4e1687))

## [3.9.1](https://github.com/informatievlaanderen/building-registry/compare/v3.9.0...v3.9.1) (2022-06-13)


### Bug Fixes

* move examples to abstractions ([9f7c432](https://github.com/informatievlaanderen/building-registry/commit/9f7c43244c4c39a3d687a32cc08343aa3efd8d11))

# [3.9.0](https://github.com/informatievlaanderen/building-registry/compare/v3.8.2...v3.9.0) (2022-06-13)


### Bug Fixes

* activate address consumer in migrator ([5437bed](https://github.com/informatievlaanderen/building-registry/commit/5437bed9eb04489f8d9b32ac445dd2680747ba98))
* move MediatR contracts to Abstractions ([5647d32](https://github.com/informatievlaanderen/building-registry/commit/5647d323fa087dd7415e129ea34b81728e7388f4))
* remove aws sdk dep + move backoffice context ([d917d2f](https://github.com/informatievlaanderen/building-registry/commit/d917d2f4092c7eb69698869597f77230964185de))


### Features

* add plan building ([bfab949](https://github.com/informatievlaanderen/building-registry/commit/bfab9499ac19b1c499cd9cf9d68ca482518cdd9d))

## [3.8.2](https://github.com/informatievlaanderen/building-registry/compare/v3.8.1...v3.8.2) (2022-06-10)


### Bug Fixes

* correct containerize migrator ([069d98a](https://github.com/informatievlaanderen/building-registry/commit/069d98ad537d2b72089c5e849d4abd6683f972e3))

## [3.8.1](https://github.com/informatievlaanderen/building-registry/compare/v3.8.0...v3.8.1) (2022-06-10)


### Bug Fixes

* correct push to test docker images ([02f48aa](https://github.com/informatievlaanderen/building-registry/commit/02f48aaac475489afc3cb16bb50ed073e5c7bca8))

# [3.8.0](https://github.com/informatievlaanderen/building-registry/compare/v3.7.1...v3.8.0) (2022-06-10)


### Features

* add migrator backoffice to pipeline ([2095061](https://github.com/informatievlaanderen/building-registry/commit/2095061da0811cf3e000f983e086c339aa4f0cbe))

## [3.7.1](https://github.com/informatievlaanderen/building-registry/compare/v3.7.0...v3.7.1) (2022-06-10)


### Bug Fixes

* remove ParallelForEachAsync ([91b6aa4](https://github.com/informatievlaanderen/building-registry/commit/91b6aa40a733fcacc723960953bf118b11fe8b30))

# [3.7.0](https://github.com/informatievlaanderen/building-registry/compare/v3.6.0...v3.7.0) (2022-06-10)


### Bug Fixes

* add experimental SQS handler for CrabImport (not used) ([#560](https://github.com/informatievlaanderen/building-registry/issues/560)) ([3751ac5](https://github.com/informatievlaanderen/building-registry/commit/3751ac5befea8621ab00166612c27bd68f84a99a))
* consumer ([197e19c](https://github.com/informatievlaanderen/building-registry/commit/197e19c8fdd7a82f93d3c7a945d9d85c19ecd216))
* consumer address resolve context ([7528b8d](https://github.com/informatievlaanderen/building-registry/commit/7528b8d36849d308d5b4d43af510bc2115fa9051))
* correct build ([6b05a02](https://github.com/informatievlaanderen/building-registry/commit/6b05a02b5cf463a9ea13d4e92bfe0a6b521c9195))
* migrator build ([4624c07](https://github.com/informatievlaanderen/building-registry/commit/4624c078f20c6eb493a3c07a333f4dab6b1d220e))


### Features

* add migrator ([6b276b7](https://github.com/informatievlaanderen/building-registry/commit/6b276b79ef54f6e3129a29aec212b9e88c1a96b3))

# [3.6.0](https://github.com/informatievlaanderen/building-registry/compare/v3.5.0...v3.6.0) (2022-06-08)


### Features

* add projector toggle V2 ([5a247b6](https://github.com/informatievlaanderen/building-registry/commit/5a247b638848e9ffa58578788ad4ebf35416118f))

# [3.5.0](https://github.com/informatievlaanderen/building-registry/compare/v3.4.1...v3.5.0) (2022-06-07)


### Features

* add WMS and WFS v2 projections ([70eda33](https://github.com/informatievlaanderen/building-registry/commit/70eda3350f7a26828949545340bc7d9826fbeec4))

## [3.4.1](https://github.com/informatievlaanderen/building-registry/compare/v3.4.0...v3.4.1) (2022-06-07)


### Bug Fixes

* cleanup handlers ([10fc026](https://github.com/informatievlaanderen/building-registry/commit/10fc026d12ed7e882229b626c75f12002b8dbec7))

# [3.4.0](https://github.com/informatievlaanderen/building-registry/compare/v3.3.2...v3.4.0) (2022-06-07)


### Bug Fixes

* correct consumer schema ([3c704ab](https://github.com/informatievlaanderen/building-registry/commit/3c704ab30967a753499850a5fb89c69f59dd8aa8))


### Features

* add extract projections v2 ([d246cd3](https://github.com/informatievlaanderen/building-registry/commit/d246cd37da9d14963a65b5ec6781b119f3a9ea83))
* add legacy v2 projections ([7d87407](https://github.com/informatievlaanderen/building-registry/commit/7d87407ba5231a570cf4feed41e9d1f1ff04353e))

## [3.3.2](https://github.com/informatievlaanderen/building-registry/compare/v3.3.1...v3.3.2) (2022-06-03)


### Bug Fixes

* add MediatR handlers for Api.Oslo BuildingUnitController ([0c39d42](https://github.com/informatievlaanderen/building-registry/commit/0c39d4217ecb0c896ca970c29e6220cf66dfdf24))

## [3.3.1](https://github.com/informatievlaanderen/building-registry/compare/v3.3.0...v3.3.1) (2022-06-03)


### Bug Fixes

* correct paket template for abstractions ([a58a581](https://github.com/informatievlaanderen/building-registry/commit/a58a581ef78eaaea976fb4f683651a92b49e8889))

# [3.3.0](https://github.com/informatievlaanderen/building-registry/compare/v3.2.0...v3.3.0) (2022-06-03)


### Bug Fixes

* build handlers ([3c4fab1](https://github.com/informatievlaanderen/building-registry/commit/3c4fab1adf4e56e771a329e606f232adfc5823e9))


### Features

* add MediatR handlers for all API projects ([b64da5c](https://github.com/informatievlaanderen/building-registry/commit/b64da5c271d1aaaa145b7aed9fa9cba046026252))

# [3.2.0](https://github.com/informatievlaanderen/building-registry/compare/v3.1.2...v3.2.0) (2022-06-02)


### Bug Fixes

* correct usings ([625d30d](https://github.com/informatievlaanderen/building-registry/commit/625d30dc2e97eae276129eb6986ad1b160d9b5c4))


### Features

* new aggregate + migration command & event ([63c05e3](https://github.com/informatievlaanderen/building-registry/commit/63c05e32bbf05198105c040bb057475f85ada809))

## [3.1.2](https://github.com/informatievlaanderen/building-registry/compare/v3.1.1...v3.1.2) (2022-06-01)


### Bug Fixes

* add MediatR handlers for BuildingRegistry.Api.Oslo ([8aa40a6](https://github.com/informatievlaanderen/building-registry/commit/8aa40a6208671eab93cc514fe2b5cd31ba1bb279))

## [3.1.1](https://github.com/informatievlaanderen/building-registry/compare/v3.1.0...v3.1.1) (2022-05-31)


### Bug Fixes

* add MediatR handlers for BuildingRegistry.Api.CrabImport ([5a9bfa5](https://github.com/informatievlaanderen/building-registry/commit/5a9bfa54e5ac17f4033064874f04b6c4b31637a8))

# [3.1.0](https://github.com/informatievlaanderen/building-registry/compare/v3.0.2...v3.1.0) (2022-05-31)


### Bug Fixes

* change csproj to reflect .net6 ([292b248](https://github.com/informatievlaanderen/building-registry/commit/292b248e2c3b63376b32565279cf19e65829e630))


### Features

* add consumer address ([d739b4c](https://github.com/informatievlaanderen/building-registry/commit/d739b4c84e9a3db3703eb1a8062fdd2b600a9858))

## [3.0.2](https://github.com/informatievlaanderen/building-registry/compare/v3.0.1...v3.0.2) (2022-04-29)


### Bug Fixes

* redirect sonar to /dev/null ([52e8550](https://github.com/informatievlaanderen/building-registry/commit/52e8550b2ce7adf3d0fe4d18bcea046f5b8b2a28))
* run sonar end when release version != none ([36a3441](https://github.com/informatievlaanderen/building-registry/commit/36a3441f78ddae3e546fd56e52193d7d4e9d8f5b))

## [3.0.2](https://github.com/informatievlaanderen/building-registry/compare/v3.0.1...v3.0.2) (2022-04-27)


### Bug Fixes

* redirect sonar to /dev/null ([52e8550](https://github.com/informatievlaanderen/building-registry/commit/52e8550b2ce7adf3d0fe4d18bcea046f5b8b2a28))

## [3.0.1](https://github.com/informatievlaanderen/building-registry/compare/v3.0.0...v3.0.1) (2022-04-04)


### Bug Fixes

* set oslo context type to string GAWR-2931 ([dcbc732](https://github.com/informatievlaanderen/building-registry/commit/dcbc732bddb4401049193232645a7a218e2df340))

# [3.0.0](https://github.com/informatievlaanderen/building-registry/compare/v2.33.5...v3.0.0) (2022-03-30)


### Features

* move to dotnet 6.0.3 ([#525](https://github.com/informatievlaanderen/building-registry/issues/525)) ([849550f](https://github.com/informatievlaanderen/building-registry/commit/849550f40497f4369ee1628d30e1c0c27c58b553))
* update api to 17.0.0 ([#517](https://github.com/informatievlaanderen/building-registry/issues/517)) ([6e6596c](https://github.com/informatievlaanderen/building-registry/commit/6e6596cb9048ae91125b5d340e3499fec29ccad5))


### BREAKING CHANGES

* move to dotnet 6.0.3

## [2.33.5](https://github.com/informatievlaanderen/building-registry/compare/v2.33.4...v2.33.5) (2022-02-10)


### Bug Fixes

* update Api dependency to fix exception handler ([0193669](https://github.com/informatievlaanderen/building-registry/commit/01936698cc6401d00f7a8d2099e57185a68d6fee))

## [2.33.4](https://github.com/informatievlaanderen/building-registry/compare/v2.33.3...v2.33.4) (2022-01-21)


### Bug Fixes

* correctly resume projections async ([3032352](https://github.com/informatievlaanderen/building-registry/commit/30323523df1f9bca45b4c8bfdd0ba92f872f3ecf))

## [2.33.3](https://github.com/informatievlaanderen/building-registry/compare/v2.33.2...v2.33.3) (2022-01-18)


### Bug Fixes

* GML polygon values GAWR-2614 + docs GAWR-2616 ([672d843](https://github.com/informatievlaanderen/building-registry/commit/672d843b2f1252aa0653eba0ea5e75519c284573))

## [2.33.2](https://github.com/informatievlaanderen/building-registry/compare/v2.33.1...v2.33.2) (2022-01-18)

## [2.33.1](https://github.com/informatievlaanderen/building-registry/compare/v2.33.0...v2.33.1) (2022-01-17)


### Bug Fixes

* gML afronden 2 cijfers na komma ([69e10d8](https://github.com/informatievlaanderen/building-registry/commit/69e10d8f286e041bc4861b9facba33779c8fb575))

# [2.33.0](https://github.com/informatievlaanderen/building-registry/compare/v2.32.5...v2.33.0) (2022-01-04)


### Features

* GAWR-2519 add buildingPolygoon property with gml 3.2 xml string ([1617297](https://github.com/informatievlaanderen/building-registry/commit/1617297ee6a26c4d5824a423a6757c8a49ca6e9f))

## [2.32.5](https://github.com/informatievlaanderen/building-registry/compare/v2.32.4...v2.32.5) (2021-12-21)


### Bug Fixes

* gawr-2514 api docs ([11c576b](https://github.com/informatievlaanderen/building-registry/commit/11c576b5b8dbfbc3a8f942bd564ad6cdc740dd4f))

## [2.32.4](https://github.com/informatievlaanderen/building-registry/compare/v2.32.3...v2.32.4) (2021-12-21)


### Bug Fixes

* gawr-2506 till gawr-2509 api docs oslo endpoints ([540a4c0](https://github.com/informatievlaanderen/building-registry/commit/540a4c0918a6bfab964c16d88f7cfdc9e8529257))

## [2.32.3](https://github.com/informatievlaanderen/building-registry/compare/v2.32.2...v2.32.3) (2021-12-20)


### Bug Fixes

* add filter to datamigration ([a9c8b95](https://github.com/informatievlaanderen/building-registry/commit/a9c8b957f72a5a07dafd3b1a7219438bc709bb1f))

## [2.32.2](https://github.com/informatievlaanderen/building-registry/compare/v2.32.1...v2.32.2) (2021-12-16)


### Bug Fixes

* use async startup of projections to fix hanging migrations ([7c8db16](https://github.com/informatievlaanderen/building-registry/commit/7c8db16aefc64b1a4d5148f2044c43bd3859f99a))

## [2.32.1](https://github.com/informatievlaanderen/building-registry/compare/v2.32.0...v2.32.1) (2021-12-13)


### Bug Fixes

* add gml property to BuildingUnitOsloResponse ([0b49a62](https://github.com/informatievlaanderen/building-registry/commit/0b49a62e686113db2c6198a57c87b801c666af3b))

# [2.32.0](https://github.com/informatievlaanderen/building-registry/compare/v2.31.0...v2.32.0) (2021-12-13)


### Features

* add oslo api  ([a4e04d6](https://github.com/informatievlaanderen/building-registry/commit/a4e04d6c8c4bb7df00ae12a48b358e12e05148d9))

# [2.31.0](https://github.com/informatievlaanderen/building-registry/compare/v2.30.17...v2.31.0) (2021-12-13)


### Features

* buildingunit add projection handler ([ec2cd70](https://github.com/informatievlaanderen/building-registry/commit/ec2cd703058a75985f1554eded1a3b26f37246ca))

## [2.30.17](https://github.com/informatievlaanderen/building-registry/compare/v2.30.16...v2.30.17) (2021-11-18)


### Bug Fixes

* remove . in docs GAWR-2202 ([74b9d9a](https://github.com/informatievlaanderen/building-registry/commit/74b9d9a04fb623c6b45e7eab960d8544dfddaa75))

## [2.30.16](https://github.com/informatievlaanderen/building-registry/compare/v2.30.15...v2.30.16) (2021-11-08)


### Bug Fixes

* gawr-2202   another docu fix ([4158d37](https://github.com/informatievlaanderen/building-registry/commit/4158d3793c16ddb854481d23f80e5d3f2982a7f4))

## [2.30.15](https://github.com/informatievlaanderen/building-registry/compare/v2.30.14...v2.30.15) (2021-11-05)


### Bug Fixes

* pakket bump ([430d274](https://github.com/informatievlaanderen/building-registry/commit/430d27416ac9941582591fd79bf35ada6432069d))

## [2.30.14](https://github.com/informatievlaanderen/building-registry/compare/v2.30.13...v2.30.14) (2021-11-02)


### Bug Fixes

* gawr-2202 IdentificatorTerreinObject docu changed ([4445582](https://github.com/informatievlaanderen/building-registry/commit/444558291ced76e72153110bd0fadbbfe4b72862))

## [2.30.13](https://github.com/informatievlaanderen/building-registry/compare/v2.30.12...v2.30.13) (2021-10-27)


### Bug Fixes

* remove default accesskey/secret ([bda4833](https://github.com/informatievlaanderen/building-registry/commit/bda483305e323f69c58a32347a8239ae9e8fbe28))

## [2.30.12](https://github.com/informatievlaanderen/building-registry/compare/v2.30.11...v2.30.12) (2021-10-25)


### Bug Fixes

* paket bump ([53bda81](https://github.com/informatievlaanderen/building-registry/commit/53bda8190ea6e48501c77f57fddaf09c5b26622a))

## [2.30.11](https://github.com/informatievlaanderen/building-registry/compare/v2.30.10...v2.30.11) (2021-10-21)


### Bug Fixes

* gawr-2202 api doc changes ([3dcdc85](https://github.com/informatievlaanderen/building-registry/commit/3dcdc85101425d7d40f53e885551e1dfa5aca888))

## [2.30.10](https://github.com/informatievlaanderen/building-registry/compare/v2.30.9...v2.30.10) (2021-10-20)


### Bug Fixes

* bump projection-handling ([53970d1](https://github.com/informatievlaanderen/building-registry/commit/53970d1c80eb45ad393eb83e84e26b629e8654de))

## [2.30.9](https://github.com/informatievlaanderen/building-registry/compare/v2.30.8...v2.30.9) (2021-10-08)


### Bug Fixes

* docs CRAB remove `-` GAWR-632 ([0283b32](https://github.com/informatievlaanderen/building-registry/commit/0283b32950e73614b537491742b74a1dacc7eb2b))

## [2.30.8](https://github.com/informatievlaanderen/building-registry/compare/v2.30.7...v2.30.8) (2021-10-06)


### Bug Fixes

* add Test to ECR ([c4110ad](https://github.com/informatievlaanderen/building-registry/commit/c4110adeb467702145ea96eb9a80d9ab8eac9b43))
* added paket files ([58a4a95](https://github.com/informatievlaanderen/building-registry/commit/58a4a952b94bac19a3a7942d34db6074812e3216))
* gawr-612 add id with stringformat to url ([ce4ae8e](https://github.com/informatievlaanderen/building-registry/commit/ce4ae8ec5a4337f679a8c6553030a4273416053d))
* gawr-615 versionid datetimeoffset +2 ([fd004f4](https://github.com/informatievlaanderen/building-registry/commit/fd004f4304770404df0d2c951c4cbc20ea0797b6))
* GAWR-615 versionid localtime in docs ([a0cc294](https://github.com/informatievlaanderen/building-registry/commit/a0cc294438dec25778a0cd968f905a6da954628e))
* gawr-652 docfix real building uri ([495e48f](https://github.com/informatievlaanderen/building-registry/commit/495e48fa6b7bfee582653640859c8faaa24e02f6))

## [2.30.7](https://github.com/informatievlaanderen/building-registry/compare/v2.30.6...v2.30.7) (2021-10-04)


### Bug Fixes

* correct addressid for linked readdresses in syndication GAWR-2128 ([854da6a](https://github.com/informatievlaanderen/building-registry/commit/854da6ac5dc891a0796ddd4e03675ec6f0cc90b0))

## [2.30.6](https://github.com/informatievlaanderen/building-registry/compare/v2.30.5...v2.30.6) (2021-10-01)


### Bug Fixes

* update packages ([2bb9df2](https://github.com/informatievlaanderen/building-registry/commit/2bb9df28554bcca01f0ab4e540829b5fec1e2abc))

## [2.30.5](https://github.com/informatievlaanderen/building-registry/compare/v2.30.4...v2.30.5) (2021-09-29)


### Bug Fixes

* gawr-627 api documentation ([af5f71e](https://github.com/informatievlaanderen/building-registry/commit/af5f71ec9768abeeb8eb69f7353969b5ba242796))
* gawr-652 docfix real building uri ([2e55cc0](https://github.com/informatievlaanderen/building-registry/commit/2e55cc08ae6a471f439a4f274ac53b39db0ffa24))

## [2.30.4](https://github.com/informatievlaanderen/building-registry/compare/v2.30.3...v2.30.4) (2021-09-24)


### Bug Fixes

* gawr-730 api documentation ([7a4a1df](https://github.com/informatievlaanderen/building-registry/commit/7a4a1df2408899357b70ad8b2898957141aae49a))

## [2.30.3](https://github.com/informatievlaanderen/building-registry/compare/v2.30.2...v2.30.3) (2021-09-22)


### Bug Fixes

* gawr-611 fix exception detail ([a52a763](https://github.com/informatievlaanderen/building-registry/commit/a52a763f1335a515accefc75e9f65435e7462f24))

## [2.30.2](https://github.com/informatievlaanderen/building-registry/compare/v2.30.1...v2.30.2) (2021-09-20)


### Bug Fixes

* update package ([5aa7e60](https://github.com/informatievlaanderen/building-registry/commit/5aa7e6054e3ea290118618e1cbc3fa288bf9b8d9))

## [2.30.1](https://github.com/informatievlaanderen/building-registry/compare/v2.30.0...v2.30.1) (2021-08-26)


### Bug Fixes

* update grar-common dependencies GRAR-2060 ([3ab923a](https://github.com/informatievlaanderen/building-registry/commit/3ab923a51d91423a5897d35bc29288a3bcf021a5))

# [2.30.0](https://github.com/informatievlaanderen/building-registry/compare/v2.29.8...v2.30.0) (2021-08-25)


### Features

* add metadata file with latest event id to building and buildingunit extract GRAR-2060 ([5e2d8b2](https://github.com/informatievlaanderen/building-registry/commit/5e2d8b27bf4e4bff2f5edf43164cfc5921c98c73))

## [2.29.8](https://github.com/informatievlaanderen/building-registry/compare/v2.29.7...v2.29.8) (2021-06-25)


### Bug Fixes

* added unique constraint to the persistentlocalid ([77eee7d](https://github.com/informatievlaanderen/building-registry/commit/77eee7d33abe983a174202708e9e0ed095e3f958))

## [2.29.7](https://github.com/informatievlaanderen/building-registry/compare/v2.29.6...v2.29.7) (2021-06-25)


### Bug Fixes

* update aws DistributedMutex package ([df8acbc](https://github.com/informatievlaanderen/building-registry/commit/df8acbc7491e45d295e84839dac47a9fa8ec89a3))

## [2.29.6](https://github.com/informatievlaanderen/building-registry/compare/v2.29.5...v2.29.6) (2021-06-17)


### Bug Fixes

*  update nuget package ([e3e7d65](https://github.com/informatievlaanderen/building-registry/commit/e3e7d652cf9f7553940071518531ec9f2c790dad))

## [2.29.5](https://github.com/informatievlaanderen/building-registry/compare/v2.29.4...v2.29.5) (2021-05-31)


### Bug Fixes

* update api ([7e44651](https://github.com/informatievlaanderen/building-registry/commit/7e446517392214a84af6b6def914a2242d24eb85))

## [2.29.4](https://github.com/informatievlaanderen/building-registry/compare/v2.29.3...v2.29.4) (2021-05-31)


### Bug Fixes

* update api ([7ab5bc6](https://github.com/informatievlaanderen/building-registry/commit/7ab5bc6767201457f951b835b634b3cb921e954f))

## [2.29.3](https://github.com/informatievlaanderen/building-registry/compare/v2.29.2...v2.29.3) (2021-05-31)


### Bug Fixes

* move to 5.0.6 ([1e7702c](https://github.com/informatievlaanderen/building-registry/commit/1e7702cd862236d14664af2fdf5ace2d16f8d212))

## [2.29.2](https://github.com/informatievlaanderen/building-registry/compare/v2.29.1...v2.29.2) (2021-05-11)


### Bug Fixes

* correct the author of the syndication feed ([277da69](https://github.com/informatievlaanderen/building-registry/commit/277da69d5e352af50b8cd62fe245bf1f38a1154c))

## [2.29.1](https://github.com/informatievlaanderen/building-registry/compare/v2.29.0...v2.29.1) (2021-05-10)


### Bug Fixes

* correct provenance for persistentlocalid ([7608947](https://github.com/informatievlaanderen/building-registry/commit/7608947ce5f0802ac81f540655310fc9faf09368))

# [2.29.0](https://github.com/informatievlaanderen/building-registry/compare/v2.28.4...v2.29.0) (2021-05-04)


### Features

* bump packages ([856d054](https://github.com/informatievlaanderen/building-registry/commit/856d054197b85af89b07a6d1abbe7faa7db11194))

## [2.28.4](https://github.com/informatievlaanderen/building-registry/compare/v2.28.3...v2.28.4) (2021-04-26)


### Bug Fixes

* rename cache status endpoint in projector ([432f7fe](https://github.com/informatievlaanderen/building-registry/commit/432f7fe1128f454a33815bc140e43290853dce31))

## [2.28.3](https://github.com/informatievlaanderen/building-registry/compare/v2.28.2...v2.28.3) (2021-04-01)


### Bug Fixes

* bump projection-handling & projector ([c47aeb7](https://github.com/informatievlaanderen/building-registry/commit/c47aeb7efa30f4836d783464ae6cfb715f82f609))
* update docs projections ([a5e7045](https://github.com/informatievlaanderen/building-registry/commit/a5e70458d1431854d6d4bb3711f14382d94f8853))

## [2.28.2](https://github.com/informatievlaanderen/building-registry/compare/v2.28.1...v2.28.2) (2021-03-31)


### Bug Fixes

* change buildingcrabidprojections 'ConnectedProjectionName' and 'ConnectedProjectionDescription' ([93686b0](https://github.com/informatievlaanderen/building-registry/commit/93686b070f4903e5609299817c63756973ebfb5b))

## [2.28.1](https://github.com/informatievlaanderen/building-registry/compare/v2.28.0...v2.28.1) (2021-03-22)


### Bug Fixes

* remove ridingwolf, collaboration ended ([895fade](https://github.com/informatievlaanderen/building-registry/commit/895fade3b1f9c32d0be7814ae6e3fbe313f30287))

# [2.28.0](https://github.com/informatievlaanderen/building-registry/compare/v2.27.3...v2.28.0) (2021-03-11)


### Bug Fixes

* update projector dependency GRAR-1876 ([385af8a](https://github.com/informatievlaanderen/building-registry/commit/385af8ac3a3b6e4269d631e185abb49ff5773c1a))


### Features

* add projection attributes GRAR-1876 ([19df3e2](https://github.com/informatievlaanderen/building-registry/commit/19df3e298f70e78d46d5c0575059ea7149dbf3d9))

## [2.27.3](https://github.com/informatievlaanderen/building-registry/compare/v2.27.2...v2.27.3) (2021-03-10)


### Bug Fixes

* use isolation extract archive for extracts ([fc6836f](https://github.com/informatievlaanderen/building-registry/commit/fc6836f82f9e9c90fc5bb10152ed9869da4b905c))

## [2.27.2](https://github.com/informatievlaanderen/building-registry/compare/v2.27.1...v2.27.2) (2021-03-08)


### Bug Fixes

* disabled removed/duplictated persistentId projections GRAR-1876 ([d4dcc57](https://github.com/informatievlaanderen/building-registry/commit/d4dcc57a48624d3fafb3b49353f877194a3ca532))

## [2.27.1](https://github.com/informatievlaanderen/building-registry/compare/v2.27.0...v2.27.1) (2021-03-06)


### Bug Fixes

* disable retry strategy in extract ([53a3f89](https://github.com/informatievlaanderen/building-registry/commit/53a3f89e208f5e08ba75f418b9fa2cfc040de7d6))

# [2.27.0](https://github.com/informatievlaanderen/building-registry/compare/v2.26.5...v2.27.0) (2021-03-05)


### Features

* add transaction isolation snapshot to extract GRAR-1796 ([ff99f88](https://github.com/informatievlaanderen/building-registry/commit/ff99f886ce3c840ce28e5cada1d10d0dc5a65c8a))

## [2.26.5](https://github.com/informatievlaanderen/building-registry/compare/v2.26.4...v2.26.5) (2021-02-22)


### Bug Fixes

* add query splitting behavior for sync query ([4572825](https://github.com/informatievlaanderen/building-registry/commit/4572825df9983c75437a76be495fc39149cffbe6))

## [2.26.4](https://github.com/informatievlaanderen/building-registry/compare/v2.26.3...v2.26.4) (2021-02-22)


### Bug Fixes

* correct possible duplicate addressid's in sync buildingunit ([69a417d](https://github.com/informatievlaanderen/building-registry/commit/69a417d1cad3789d87a5e814082c5b9738114f8c))


### Performance Improvements

* sync only include when object is requested ([d9d0629](https://github.com/informatievlaanderen/building-registry/commit/d9d0629c848521b06fb4ad78fd50222658b20b30))

## [2.26.3](https://github.com/informatievlaanderen/building-registry/compare/v2.26.2...v2.26.3) (2021-02-15)


### Bug Fixes

* register problem details helper for projector GRAR-1814 ([f7467f5](https://github.com/informatievlaanderen/building-registry/commit/f7467f556a2a15016b614d5b0745921f7eecbb5d))

## [2.26.2](https://github.com/informatievlaanderen/building-registry/compare/v2.26.1...v2.26.2) (2021-02-11)


### Bug Fixes

* update api with use of problemdetailshelper GRAR-1814 ([56a05ae](https://github.com/informatievlaanderen/building-registry/commit/56a05ae6c7e6eb3d319e4d7502cbe4048eef20f6))

## [2.26.1](https://github.com/informatievlaanderen/building-registry/compare/v2.26.0...v2.26.1) (2021-02-02)


### Bug Fixes

* move to 5.0.2 ([c6bf43c](https://github.com/informatievlaanderen/building-registry/commit/c6bf43c7615c0fb97e2bf52d27f932fbed61bdfc))

# [2.26.0](https://github.com/informatievlaanderen/building-registry/compare/v2.25.6...v2.26.0) (2021-01-30)


### Features

* add sync tags to events ([8bc7177](https://github.com/informatievlaanderen/building-registry/commit/8bc71775e147651b343228ddde9f300ca956e0be))

## [2.25.6](https://github.com/informatievlaanderen/building-registry/compare/v2.25.5...v2.25.6) (2021-01-29)


### Bug Fixes

* remove sync alternate links ([45068f5](https://github.com/informatievlaanderen/building-registry/commit/45068f56ae07ed234f8034a4d24cfac5296ac8b8))
* update basisregisters api dependency GRAR-170 ([50d31c2](https://github.com/informatievlaanderen/building-registry/commit/50d31c27469f15275d328b79d4a87f9e0c7cf430))

## [2.25.5](https://github.com/informatievlaanderen/building-registry/compare/v2.25.4...v2.25.5) (2021-01-27)


### Bug Fixes

* bump basisregisters.api version ([6c79b0b](https://github.com/informatievlaanderen/building-registry/commit/6c79b0b6210d12d9d4057e1c69fbaa2f09f33a2d))

## [2.25.4](https://github.com/informatievlaanderen/building-registry/compare/v2.25.3...v2.25.4) (2021-01-21)


### Bug Fixes

* add handler for GRB WFS exceptions GRAR-170 ([1f265d3](https://github.com/informatievlaanderen/building-registry/commit/1f265d361e78a334ac1389c85b013fe13336022f))
* invert catch to all except GrbWfsExceptions GRAR-170 ([6b642a1](https://github.com/informatievlaanderen/building-registry/commit/6b642a19ce40e9a29db78ca03a56db54ba2e9279))
* update error handling for wfs requests GRAR-170 ([ee3bc07](https://github.com/informatievlaanderen/building-registry/commit/ee3bc07384b4585c0b7f3b2ff719eaa95c2c0e94))
* updated the GRB WFS exception message GRAR-170 ([288b875](https://github.com/informatievlaanderen/building-registry/commit/288b87588958ee570f8cd3c647a7fe1823766a39))

## [2.25.3](https://github.com/informatievlaanderen/building-registry/compare/v2.25.2...v2.25.3) (2021-01-21)


### Bug Fixes

* correct usage of address in BuildingUnitWasAddedToRetiredBuilding GRAR-1767 ([e956f04](https://github.com/informatievlaanderen/building-registry/commit/e956f0470e565a1ee72d899660276ed6e9e871ba))

## [2.25.2](https://github.com/informatievlaanderen/building-registry/compare/v2.25.1...v2.25.2) (2021-01-19)


### Bug Fixes

* event deserialization some ctor parameters were incorrectly named GRAR-1759 ([10f8e80](https://github.com/informatievlaanderen/building-registry/commit/10f8e80ce1d91ec57958aa9aead2d55524f44cf1))

## [2.25.1](https://github.com/informatievlaanderen/building-registry/compare/v2.25.0...v2.25.1) (2021-01-18)


### Bug Fixes

* localdate serialization in sync eventxml GRAR-1754 ([38463d4](https://github.com/informatievlaanderen/building-registry/commit/38463d444397e9a7d246b6098cc8d8b8b23c6c9e))

# [2.25.0](https://github.com/informatievlaanderen/building-registry/compare/v2.24.8...v2.25.0) (2021-01-12)


### Features

* add syndication status to projector api GRAR-1567 ([6522f55](https://github.com/informatievlaanderen/building-registry/commit/6522f55408efb95b9c02caecf003532a1279e924))

## [2.24.8](https://github.com/informatievlaanderen/building-registry/compare/v2.24.7...v2.24.8) (2021-01-11)


### Bug Fixes

* bump version, force build ([4d80cb1](https://github.com/informatievlaanderen/building-registry/commit/4d80cb146480ffdfd0f9fc6ea39f01a10a528fe0))
* ewkb property names to be consistent with address GRAR-1716 GRAR-1717 ([74d59c1](https://github.com/informatievlaanderen/building-registry/commit/74d59c1283714f2e21cb8f463b6af3df08cfb780))

## [2.24.7](https://github.com/informatievlaanderen/building-registry/compare/v2.24.6...v2.24.7) (2021-01-07)


### Bug Fixes

* improve cache status page GRAR-1734 ([ae89aa9](https://github.com/informatievlaanderen/building-registry/commit/ae89aa9cab92b357b31b29e6d6c2e6762034e72b))

## [2.24.6](https://github.com/informatievlaanderen/building-registry/compare/v2.24.5...v2.24.6) (2021-01-07)


### Bug Fixes

* update deps ([d858603](https://github.com/informatievlaanderen/building-registry/commit/d8586030c6cecc0fedf2790682147add6d8b1dab))

## [2.24.5](https://github.com/informatievlaanderen/building-registry/compare/v2.24.4...v2.24.5) (2020-12-28)


### Bug Fixes

* update basisregisters api dependency ([62f67a5](https://github.com/informatievlaanderen/building-registry/commit/62f67a5dc524c2853cad654525842277af94f55d))

## [2.24.4](https://github.com/informatievlaanderen/building-registry/compare/v2.24.3...v2.24.4) (2020-12-21)


### Bug Fixes

* move to 5.0.1 ([83ea95f](https://github.com/informatievlaanderen/building-registry/commit/83ea95f7d7090ccc6af69c4b7c305a7063a84332))

## [2.24.3](https://github.com/informatievlaanderen/building-registry/compare/v2.24.2...v2.24.3) (2020-12-16)


### Bug Fixes

* correct nullable objectid in sync api GRAR-1710 ([1b46392](https://github.com/informatievlaanderen/building-registry/commit/1b46392bf64d13720e4bfb9f901fc691dffcb7be))

## [2.24.2](https://github.com/informatievlaanderen/building-registry/compare/v2.24.1...v2.24.2) (2020-12-14)


### Bug Fixes

* sync objectid can be null GRAR-1710 ([d88f552](https://github.com/informatievlaanderen/building-registry/commit/d88f55241fd6125f76001505f4d038e83df27961))

## [2.24.1](https://github.com/informatievlaanderen/building-registry/compare/v2.24.0...v2.24.1) (2020-12-14)


### Bug Fixes

* rename adressen to adressenids in sync ([031780c](https://github.com/informatievlaanderen/building-registry/commit/031780ce92af4f766def89130d153d9c88ac7813))

# [2.24.0](https://github.com/informatievlaanderen/building-registry/compare/v2.23.8...v2.24.0) (2020-12-11)


### Features

* add sync projection catchup size to config ([6fa400c](https://github.com/informatievlaanderen/building-registry/commit/6fa400c58fb225e4a900e9a085d090a16fb8da73))

## [2.23.8](https://github.com/informatievlaanderen/building-registry/compare/v2.23.7...v2.23.8) (2020-12-09)


### Bug Fixes

* correct name of addresses in sync GRAR-1670 ([5672bf3](https://github.com/informatievlaanderen/building-registry/commit/5672bf34d6370a962fa48c9beefff19746725ac0))

## [2.23.7](https://github.com/informatievlaanderen/building-registry/compare/v2.23.6...v2.23.7) (2020-12-08)


### Bug Fixes

* correct unretire multiple times by previously readdressed unit ([082dfec](https://github.com/informatievlaanderen/building-registry/commit/082dfec2acf8f4971b67ef544d5f49dfa15fa215))

## [2.23.6](https://github.com/informatievlaanderen/building-registry/compare/v2.23.5...v2.23.6) (2020-12-03)


### Bug Fixes

* remove iscomplete from parcel sync GRAR-1652 ([a659a30](https://github.com/informatievlaanderen/building-registry/commit/a659a30679892988c73e6c6b30a324fa4d2272d1))

## [2.23.5](https://github.com/informatievlaanderen/building-registry/compare/v2.23.4...v2.23.5) (2020-11-19)


### Bug Fixes

* remove set-env usage in gh-actions ([22ebb66](https://github.com/informatievlaanderen/building-registry/commit/22ebb665e63f54318251b0e5354d501414836a75))
* update references for event property descriptions ([83cabde](https://github.com/informatievlaanderen/building-registry/commit/83cabdec93f861d82f0b30da101b3d879aba111e))

## [2.23.4](https://github.com/informatievlaanderen/building-registry/compare/v2.23.3...v2.23.4) (2020-11-16)


### Bug Fixes

* handle ParcelWasRecovered event in syndication ([63e07d0](https://github.com/informatievlaanderen/building-registry/commit/63e07d027cebd1fa5b303cf41fc9858639cb1d66))

## [2.23.3](https://github.com/informatievlaanderen/building-registry/compare/v2.23.2...v2.23.3) (2020-11-13)


### Bug Fixes

* display sync response example as correct xml GRAR-1599 ([ddce248](https://github.com/informatievlaanderen/building-registry/commit/ddce2488ddcbfeac87cd88e24528c448b4e42cc3))
* upgrade basisregisters.api GRAR-1599 ([bd1c94b](https://github.com/informatievlaanderen/building-registry/commit/bd1c94b9c823ff7dee39bb48d496b56e9b8bb458))
* use production url for sync examples ([104b72a](https://github.com/informatievlaanderen/building-registry/commit/104b72a90e065d11ccc115ae209c80e533719bc8))

## [2.23.2](https://github.com/informatievlaanderen/building-registry/compare/v2.23.1...v2.23.2) (2020-11-12)


### Bug Fixes

* use event name instead of type for sync xml serialization ([2192499](https://github.com/informatievlaanderen/building-registry/commit/2192499d2287203240069c25aecdc676e6614cb8))

## [2.23.1](https://github.com/informatievlaanderen/building-registry/compare/v2.23.0...v2.23.1) (2020-11-06)


### Bug Fixes

* logging ([3407eb6](https://github.com/informatievlaanderen/building-registry/commit/3407eb6b1d98de87002782664e707ddb8aeed2cf))
* logging ([c3dca67](https://github.com/informatievlaanderen/building-registry/commit/c3dca67cafc5e97021ecb232402db0dde6c503e6))
* logging ([a5a3983](https://github.com/informatievlaanderen/building-registry/commit/a5a398323e5339ec6dc5c2a720ef50073fe09d1b))
* logging ([fa014ff](https://github.com/informatievlaanderen/building-registry/commit/fa014ffa7d7c3b6e29d8a5e8cc8549c4fd755904))
* logging ([65fd450](https://github.com/informatievlaanderen/building-registry/commit/65fd450f6f7724a0c5e03a6a404e84bf2a75c9a6))

# [2.23.0](https://github.com/informatievlaanderen/building-registry/compare/v2.22.0...v2.23.0) (2020-10-27)


### Features

* add error message for syndication projections ([0a40aa6](https://github.com/informatievlaanderen/building-registry/commit/0a40aa679c06909577bb4f289585aacba15af720))

# [2.22.0](https://github.com/informatievlaanderen/building-registry/compare/v2.21.0...v2.22.0) (2020-10-27)


### Features

* update projector with gap detection and extended status api ([b3a3e49](https://github.com/informatievlaanderen/building-registry/commit/b3a3e49dc5b96fff02211afc86f2c0505b725eac))

# [2.21.0](https://github.com/informatievlaanderen/building-registry/compare/v2.20.1...v2.21.0) (2020-10-16)


### Features

* add cache status to projector api ([f366e57](https://github.com/informatievlaanderen/building-registry/commit/f366e57f11ad19d0943e79884227508c958cccc1))

## [2.20.1](https://github.com/informatievlaanderen/building-registry/compare/v2.20.0...v2.20.1) (2020-10-13)


### Bug Fixes

* remove offset and add from to next uri GRAR-1422 ([a127395](https://github.com/informatievlaanderen/building-registry/commit/a127395477e8e4aaebe3e8eafb6ecf076178d75d))

# [2.20.0](https://github.com/informatievlaanderen/building-registry/compare/v2.19.1...v2.20.0) (2020-10-07)


### Bug Fixes

* add geometry check selftouchingring to WFS and make valid GRAR-1577 ([b34506d](https://github.com/informatievlaanderen/building-registry/commit/b34506dfc60ba315d6962a90550d742e31a903b9))


### Features

* add GrbPolygon for projections ([84775f3](https://github.com/informatievlaanderen/building-registry/commit/84775f3fe5ecc84754913c12cd76de5e518aa127))

## [2.19.1](https://github.com/informatievlaanderen/building-registry/compare/v2.19.0...v2.19.1) (2020-10-05)


### Bug Fixes

* run projection using the feedprojector GRAR-1562 ([7b7a9fa](https://github.com/informatievlaanderen/building-registry/commit/7b7a9fac228aff7c146cc5859b9ac852e771dc59))

# [2.19.0](https://github.com/informatievlaanderen/building-registry/compare/v2.18.10...v2.19.0) (2020-09-22)


### Bug Fixes

* add missing configuration items GRAR-1533 ([055209c](https://github.com/informatievlaanderen/building-registry/commit/055209c6a533ebc96ec34b1b3c1783b4efe5354b))
* move to 3.1.8 ([18a5d36](https://github.com/informatievlaanderen/building-registry/commit/18a5d36e75603f3cdb4ad068f9a0a6961c8bdf65))


### Features

* add import status endpoint GRAR-1400 ([8c1b51d](https://github.com/informatievlaanderen/building-registry/commit/8c1b51dda1c6ebea6baa9f59e8c70243ace46c21))

## [2.18.10](https://github.com/informatievlaanderen/building-registry/compare/v2.18.9...v2.18.10) (2020-09-15)


### Bug Fixes

* correct applying provenance on readdress event GRAR-1548 ([f2e9d15](https://github.com/informatievlaanderen/building-registry/commit/f2e9d1537556a611bccd0c44aa20670b64f120eb))
* correct syndication projection handling readdressing provenance ([d297ddc](https://github.com/informatievlaanderen/building-registry/commit/d297ddcf355db62ef9f5380d64ad189ec7bc6d0c))

## [2.18.9](https://github.com/informatievlaanderen/building-registry/compare/v2.18.8...v2.18.9) (2020-09-14)


### Bug Fixes

* add provenance for readdressing commands GRAR-1548 ([c4967a5](https://github.com/informatievlaanderen/building-registry/commit/c4967a5a76508fe87c5f1b946975997fb23280db))

## [2.18.8](https://github.com/informatievlaanderen/building-registry/compare/v2.18.7...v2.18.8) (2020-09-11)


### Bug Fixes

* remove Modification from xml GRAR-1529 ([f6a0702](https://github.com/informatievlaanderen/building-registry/commit/f6a07022f2bd8abaa65f3a80a13dc30c18a4f5b1))

## [2.18.7](https://github.com/informatievlaanderen/building-registry/compare/v2.18.6...v2.18.7) (2020-09-11)


### Bug Fixes

* remove paging response header in sync ([e1715bc](https://github.com/informatievlaanderen/building-registry/commit/e1715bc566225bfee4e98f8ab34e296bf4e8f347))

## [2.18.6](https://github.com/informatievlaanderen/building-registry/compare/v2.18.5...v2.18.6) (2020-09-10)


### Bug Fixes

* add sync with correct timestamp configuration GRAR-1483 ([efcf26f](https://github.com/informatievlaanderen/building-registry/commit/efcf26f8e58fe63e580143f4baa126a33e20d9cc))

## [2.18.5](https://github.com/informatievlaanderen/building-registry/compare/v2.18.4...v2.18.5) (2020-09-10)


### Bug Fixes

* add provenance when assigning localpersistentids GRAR-1532 ([55e714f](https://github.com/informatievlaanderen/building-registry/commit/55e714f3e2666201ff782fd6b2b50bace21c8ab3))

## [2.18.4](https://github.com/informatievlaanderen/building-registry/compare/v2.18.3...v2.18.4) (2020-09-04)


### Bug Fixes

* buildingunit position event was not projected to sync GRAR-1543 ([397ab10](https://github.com/informatievlaanderen/building-registry/commit/397ab10b1587ca25c40353e2f7a5223fff6d8dd3))

## [2.18.3](https://github.com/informatievlaanderen/building-registry/compare/v2.18.2...v2.18.3) (2020-09-03)


### Bug Fixes

* sync null organisation defaults to Unknown ([9bc1714](https://github.com/informatievlaanderen/building-registry/commit/9bc1714c672026fbf7805702366ebf3cef7508e3))

## [2.18.2](https://github.com/informatievlaanderen/building-registry/compare/v2.18.1...v2.18.2) (2020-09-02)


### Bug Fixes

* upgrade common to fix sync author ([acac5de](https://github.com/informatievlaanderen/building-registry/commit/acac5de4c1b00836d2a81ca62c9435eb3704828e))

## [2.18.1](https://github.com/informatievlaanderen/building-registry/compare/v2.18.0...v2.18.1) (2020-07-19)


### Bug Fixes

* move to 3.1.6 ([c25f3b0](https://github.com/informatievlaanderen/building-registry/commit/c25f3b023fee0ac3fb83f498864aeba7cad94676))

# [2.18.0](https://github.com/informatievlaanderen/building-registry/compare/v2.17.3...v2.18.0) (2020-07-14)


### Features

* add timestamp to sync provenance GRAR-1451 ([90be3ec](https://github.com/informatievlaanderen/building-registry/commit/90be3ecff7804471120272b6c613e92aaac00149))

## [2.17.3](https://github.com/informatievlaanderen/building-registry/compare/v2.17.2...v2.17.3) (2020-07-13)


### Bug Fixes

* update dependencies ([bc4cdc7](https://github.com/informatievlaanderen/building-registry/commit/bc4cdc70c80bbe8a2dacbb803959c1d5467ad1a6))
* use typed embed value GRAR-1465 ([4bedb3c](https://github.com/informatievlaanderen/building-registry/commit/4bedb3cd50086d3783781f94708d3d6cb468aa9b))

## [2.17.2](https://github.com/informatievlaanderen/building-registry/compare/v2.17.1...v2.17.2) (2020-07-10)


### Bug Fixes

* correct author, links entry atom feed + example GRAR-1443 GRAR-1447 ([b37ebf2](https://github.com/informatievlaanderen/building-registry/commit/b37ebf2dcb39f2402280933a14ecb29a7b3f969e))

## [2.17.1](https://github.com/informatievlaanderen/building-registry/compare/v2.17.0...v2.17.1) (2020-07-10)


### Bug Fixes

* enums were not correctly serialized in syndication event GRAR-1490 ([ddf54e8](https://github.com/informatievlaanderen/building-registry/commit/ddf54e8ecb8eeb63ecd1780837bb163f2d9d14d7))

# [2.17.0](https://github.com/informatievlaanderen/building-registry/compare/v2.16.12...v2.17.0) (2020-07-09)


### Features

* add WFS building/building unit GRAR-275 ([8482ddb](https://github.com/informatievlaanderen/building-registry/commit/8482ddb4849498a3aaca3f120b878aa418f12e12))

## [2.16.12](https://github.com/informatievlaanderen/building-registry/compare/v2.16.11...v2.16.12) (2020-06-30)


### Bug Fixes

* correct response object descriptions GRAR-1386 ([aac8338](https://github.com/informatievlaanderen/building-registry/commit/aac8338d113b5c69905899f4664001b8355edb64))
* correct response object descriptions GRAR-1387 ([6753850](https://github.com/informatievlaanderen/building-registry/commit/6753850b50ef4230cab6ea2c7c2b79b52898be38))

## [2.16.11](https://github.com/informatievlaanderen/building-registry/compare/v2.16.10...v2.16.11) (2020-06-29)


### Bug Fixes

* make CRAB naming consistent GRAR-1386 ([5f6165c](https://github.com/informatievlaanderen/building-registry/commit/5f6165cd8b96fd4cf496f8a39a0e38304cf15b1c))

## [2.16.10](https://github.com/informatievlaanderen/building-registry/compare/v2.16.9...v2.16.10) (2020-06-29)


### Bug Fixes

* add readdressing logic to fix housenr test + fixgrar1359 command id ([59c1bdc](https://github.com/informatievlaanderen/building-registry/commit/59c1bdcd9c46bf5e5b5d3a5786f3407f17c386ed))

## [2.16.9](https://github.com/informatievlaanderen/building-registry/compare/v2.16.8...v2.16.9) (2020-06-24)


### Bug Fixes

* force build ([cc7e1de](https://github.com/informatievlaanderen/building-registry/commit/cc7e1de2eaa3415376d7b66243d64227ae657c03))

## [2.16.8](https://github.com/informatievlaanderen/building-registry/compare/v2.16.7...v2.16.8) (2020-06-24)


### Bug Fixes

* add extra comments to docs + nullability for oidn GRAR-1383 ([daec2d5](https://github.com/informatievlaanderen/building-registry/commit/daec2d536ccfa4d59a2d5c9f3246e540947d6a2f))

## [2.16.7](https://github.com/informatievlaanderen/building-registry/compare/v2.16.6...v2.16.7) (2020-06-23)


### Bug Fixes

* correct response crabgebouwen GRAR-1381 ([d6fa913](https://github.com/informatievlaanderen/building-registry/commit/d6fa913983abe4e7d9fc208c66be550de0e71719))

## [2.16.6](https://github.com/informatievlaanderen/building-registry/compare/v2.16.5...v2.16.6) (2020-06-23)


### Bug Fixes

* configure baseurls for all problemdetails GRAR-1357 ([c138fda](https://github.com/informatievlaanderen/building-registry/commit/c138fdaabc13c8235b806a55e5fccd1f591f349e))

## [2.16.5](https://github.com/informatievlaanderen/building-registry/compare/v2.16.4...v2.16.5) (2020-06-22)


### Bug Fixes

* configure baseurls for all problemdetails GRAR-1358 GRAR-1357 ([190c501](https://github.com/informatievlaanderen/building-registry/commit/190c5015c940d39e552974927752d69a6f8815cb))

## [2.16.4](https://github.com/informatievlaanderen/building-registry/compare/v2.16.3...v2.16.4) (2020-06-22)


### Bug Fixes

* correct reactivated housenumbers and added processed archives ([879937c](https://github.com/informatievlaanderen/building-registry/commit/879937c351fbeba733aa88238c635ddb402c4150))

## [2.16.3](https://github.com/informatievlaanderen/building-registry/compare/v2.16.2...v2.16.3) (2020-06-22)


### Bug Fixes

* add identificator to crab endpoint response ([f9604bf](https://github.com/informatievlaanderen/building-registry/commit/f9604bff0b811a42ac181b4be291ab39ff2a4b4d))

## [2.16.2](https://github.com/informatievlaanderen/building-registry/compare/v2.16.1...v2.16.2) (2020-06-19)


### Bug Fixes

* add identificator to references response ([c227584](https://github.com/informatievlaanderen/building-registry/commit/c227584e60f6a48f4bada076b7894008208455a2))
* correct generating command id for grar-1359 ([dbfdadc](https://github.com/informatievlaanderen/building-registry/commit/dbfdadca0d12def3c0b7fae5f5f0bf69de370f89))
* move to 3.1.5 ([e33362b](https://github.com/informatievlaanderen/building-registry/commit/e33362b9954480e748f3501bbee4c32b4c9b000e))
* move to 3.1.5 ([1b37983](https://github.com/informatievlaanderen/building-registry/commit/1b3798336d2d7ce9ae238930cacb69bb8106cc43))

## [2.16.1](https://github.com/informatievlaanderen/building-registry/compare/v2.16.0...v2.16.1) (2020-06-17)


### Bug Fixes

* provenance for fix-1359 is now added ([6f77c13](https://github.com/informatievlaanderen/building-registry/commit/6f77c137dacc4386882eac6a3585d676a3e5e2ac))

# [2.16.0](https://github.com/informatievlaanderen/building-registry/compare/v2.15.0...v2.16.0) (2020-06-17)


### Features

* add building references endpoint ([4f8d152](https://github.com/informatievlaanderen/building-registry/commit/4f8d152deb5271f3379656cc4f56d242f076d124))

# [2.15.0](https://github.com/informatievlaanderen/building-registry/compare/v2.14.5...v2.15.0) (2020-06-17)


### Features

* add crab mapping api endpoint ([c5d767b](https://github.com/informatievlaanderen/building-registry/commit/c5d767b03b0cb2096a6bd28bd0fa411fc7530f65))
* add crabgebouwen endpoint to map crab/grb to grar GRAR-1369 ([706c465](https://github.com/informatievlaanderen/building-registry/commit/706c4658bb99786370e4c2e2b145c5e0ca0ccec6))

## [2.14.5](https://github.com/informatievlaanderen/building-registry/compare/v2.14.4...v2.14.5) (2020-06-17)


### Bug Fixes

* add retry logic for WMS projections GRAR-1284 ([f519c05](https://github.com/informatievlaanderen/building-registry/commit/f519c0574de0da33191d7105c5c9bdddd624cd74))

## [2.14.4](https://github.com/informatievlaanderen/building-registry/compare/v2.14.3...v2.14.4) (2020-06-11)


### Bug Fixes

* force release ([0b80d01](https://github.com/informatievlaanderen/building-registry/commit/0b80d01d57df238766177d9fdbae66637f51e14a))

## [2.14.3](https://github.com/informatievlaanderen/building-registry/compare/v2.14.2...v2.14.3) (2020-06-10)


### Bug Fixes

* update grar extract GRAR-1330 ([c8ab641](https://github.com/informatievlaanderen/building-registry/commit/c8ab64179bf1e6bf972aca13537b1118d757f52b))

## [2.14.2](https://github.com/informatievlaanderen/building-registry/compare/v2.14.1...v2.14.2) (2020-06-09)


### Bug Fixes

* reactivate housenumber didn't reactivate all subaddresses GRAR-1359 ([a951618](https://github.com/informatievlaanderen/building-registry/commit/a951618c8951e4f10269d9cf31634076e009d93f))

## [2.14.1](https://github.com/informatievlaanderen/building-registry/compare/v2.14.0...v2.14.1) (2020-06-08)


### Bug Fixes

* build msil version for public api ([24a8dbf](https://github.com/informatievlaanderen/building-registry/commit/24a8dbf1392b1ed6835300bf2de958f84efa0418))

# [2.14.0](https://github.com/informatievlaanderen/building-registry/compare/v2.13.0...v2.14.0) (2020-06-08)


### Features

* add status to api list buildings GRAR-33 ([a8ecf7c](https://github.com/informatievlaanderen/building-registry/commit/a8ecf7c6214298fd5cf8a3c12c4042d7b4ddf8a8))

# [2.13.0](https://github.com/informatievlaanderen/building-registry/compare/v2.12.3...v2.13.0) (2020-06-08)


### Features

* add status to api list buildingunits GRAR-25 ([230f827](https://github.com/informatievlaanderen/building-registry/commit/230f8275334ab379baa9a99f31742a1bcd993e53))

## [2.12.3](https://github.com/informatievlaanderen/building-registry/compare/v2.12.2...v2.12.3) (2020-05-30)


### Bug Fixes

* test client can now compile ([be39f79](https://github.com/informatievlaanderen/building-registry/commit/be39f79cc7cfcab15e5083557e30b280965792b2))
* when updating building relation with previously coupled units ([59182e5](https://github.com/informatievlaanderen/building-registry/commit/59182e5298c25eacb74edc04b42fb229207357f2))

## [2.12.2](https://github.com/informatievlaanderen/building-registry/compare/v2.12.1...v2.12.2) (2020-05-29)


### Bug Fixes

* update dependencies GRAR-752 ([a293311](https://github.com/informatievlaanderen/building-registry/commit/a293311f9c49774481f151def1cdf13889dcbe2c))

## [2.12.1](https://github.com/informatievlaanderen/building-registry/compare/v2.12.0...v2.12.1) (2020-05-27)


### Bug Fixes

* extend commandtimeout for sync projection ([e598d31](https://github.com/informatievlaanderen/building-registry/commit/e598d31e72c0f390e7ec7650d78248bc0e66abe8))

# [2.12.0](https://github.com/informatievlaanderen/building-registry/compare/v2.11.3...v2.12.0) (2020-05-22)


### Bug Fixes

* only run ci on master repo ([1a2ef5e](https://github.com/informatievlaanderen/building-registry/commit/1a2ef5e644474091690ebd3e17aa8c3b1132db4c))


### Features

* add prj file to extract GRAR-356 ([2966069](https://github.com/informatievlaanderen/building-registry/commit/296606957095e832f74011bf17f1366d3124e0ac))

## [2.11.3](https://github.com/informatievlaanderen/building-registry/compare/v2.11.2...v2.11.3) (2020-05-20)


### Bug Fixes

* fall back on default sss ([3d4cc86](https://github.com/informatievlaanderen/building-registry/commit/3d4cc86c843b6452f1a0ab41583d15012f587108))

## [2.11.2](https://github.com/informatievlaanderen/building-registry/compare/v2.11.1...v2.11.2) (2020-05-20)


### Bug Fixes

* add build badge ([b88981f](https://github.com/informatievlaanderen/building-registry/commit/b88981fbdc9b78b3627f401f623b3eef96cbb609))
* force build by updating readme ([77d724f](https://github.com/informatievlaanderen/building-registry/commit/77d724fe3f1a7e2360e6b56704c12bfb9548579c))

## [2.11.1](https://github.com/informatievlaanderen/building-registry/compare/v2.11.0...v2.11.1) (2020-05-19)


### Bug Fixes

* move to 3.1.4 and gh actions ([ae18959](https://github.com/informatievlaanderen/building-registry/commit/ae189593ccb146ace998932448b294ea0a9d73d9))

# [2.11.0](https://github.com/informatievlaanderen/building-registry/compare/v2.10.13...v2.11.0) (2020-05-01)


### Features

* add projection to map crab id to persistent local id ([2be9f9e](https://github.com/informatievlaanderen/building-registry/commit/2be9f9e))

## [2.10.13](https://github.com/informatievlaanderen/building-registry/compare/v2.10.12...v2.10.13) (2020-04-30)


### Bug Fixes

* update packages and add filebasedproxy for debugging purposes ([#109](https://github.com/informatievlaanderen/building-registry/issues/109)) ([2e163d5](https://github.com/informatievlaanderen/building-registry/commit/2e163d5))

## [2.10.12](https://github.com/informatievlaanderen/building-registry/compare/v2.10.11...v2.10.12) (2020-04-29)


### Bug Fixes

* add logo and licence info to nuget ([7e52b93](https://github.com/informatievlaanderen/building-registry/commit/7e52b93))

## [2.10.11](https://github.com/informatievlaanderen/building-registry/compare/v2.10.10...v2.10.11) (2020-04-28)


### Bug Fixes

* update grar dependencies GRAR-412 ([5f1450a](https://github.com/informatievlaanderen/building-registry/commit/5f1450a))

## [2.10.10](https://github.com/informatievlaanderen/building-registry/compare/v2.10.9...v2.10.10) (2020-04-27)


### Bug Fixes

* add hacked sss 1.1.3 with 1h timeout ([64e42d3](https://github.com/informatievlaanderen/building-registry/commit/64e42d3))
* use new sss for now ([16a7edd](https://github.com/informatievlaanderen/building-registry/commit/16a7edd))

## [2.10.9](https://github.com/informatievlaanderen/building-registry/compare/v2.10.8...v2.10.9) (2020-04-20)


### Bug Fixes

* api buildingunit return 404 incomplete building ([a554213](https://github.com/informatievlaanderen/building-registry/commit/a554213))

## [2.10.8](https://github.com/informatievlaanderen/building-registry/compare/v2.10.7...v2.10.8) (2020-04-20)


### Bug Fixes

* exclude parcel when intersection calcularion fails GRAR-1189 ([3f296ce](https://github.com/informatievlaanderen/building-registry/commit/3f296ce))

## [2.10.7](https://github.com/informatievlaanderen/building-registry/compare/v2.10.6...v2.10.7) (2020-04-16)


### Bug Fixes

* building should not be cached ([1452b01](https://github.com/informatievlaanderen/building-registry/commit/1452b01))

## [2.10.6](https://github.com/informatievlaanderen/building-registry/compare/v2.10.5...v2.10.6) (2020-04-16)


### Bug Fixes

* remove unique constraint from persistentlocalid GRAR-1189 ([adcdfc2](https://github.com/informatievlaanderen/building-registry/commit/adcdfc2))

## [2.10.5](https://github.com/informatievlaanderen/building-registry/compare/v2.10.4...v2.10.5) (2020-04-14)


### Bug Fixes

* update packages ([89c3e85](https://github.com/informatievlaanderen/building-registry/commit/89c3e85))

## [2.10.4](https://github.com/informatievlaanderen/building-registry/compare/v2.10.3...v2.10.4) (2020-04-10)


### Bug Fixes

* update grar common packages ([26cee99](https://github.com/informatievlaanderen/building-registry/commit/26cee99))

## [2.10.3](https://github.com/informatievlaanderen/building-registry/compare/v2.10.2...v2.10.3) (2020-04-10)


### Bug Fixes

* update packages for import batch timestamps ([026a6db](https://github.com/informatievlaanderen/building-registry/commit/026a6db))

## [2.10.2](https://github.com/informatievlaanderen/building-registry/compare/v2.10.1...v2.10.2) (2020-04-06)


### Bug Fixes

* import new building via update with older linked data ([32ac1df](https://github.com/informatievlaanderen/building-registry/commit/32ac1df))

## [2.10.1](https://github.com/informatievlaanderen/building-registry/compare/v2.10.0...v2.10.1) (2020-04-06)


### Bug Fixes

* change importer to lazy call so db is not called every update ([e29310b](https://github.com/informatievlaanderen/building-registry/commit/e29310b))

# [2.10.0](https://github.com/informatievlaanderen/building-registry/compare/v2.9.1...v2.10.0) (2020-04-03)


### Features

* upgrade projection handling to include errmessage lastchangedlist ([50ca3aa](https://github.com/informatievlaanderen/building-registry/commit/50ca3aa))

## [2.9.1](https://github.com/informatievlaanderen/building-registry/compare/v2.9.0...v2.9.1) (2020-03-27)


### Bug Fixes

* set sync feed dates to belgian timezone ([454e12e](https://github.com/informatievlaanderen/building-registry/commit/454e12e))

# [2.9.0](https://github.com/informatievlaanderen/building-registry/compare/v2.8.1...v2.9.0) (2020-03-25)


### Features

* add status filter on lists ([d436b09](https://github.com/informatievlaanderen/building-registry/commit/d436b09))

## [2.8.1](https://github.com/informatievlaanderen/building-registry/compare/v2.8.0...v2.8.1) (2020-03-23)


### Bug Fixes

* correct versie id type change in syndication ([e2a7658](https://github.com/informatievlaanderen/building-registry/commit/e2a7658))
* update grar common to fix versie id type ([fefd263](https://github.com/informatievlaanderen/building-registry/commit/fefd263))

# [2.8.0](https://github.com/informatievlaanderen/building-registry/compare/v2.7.0...v2.8.0) (2020-03-20)


### Features

* send mail when importer crashes ([92f972e](https://github.com/informatievlaanderen/building-registry/commit/92f972e))

# [2.7.0](https://github.com/informatievlaanderen/building-registry/compare/v2.6.2...v2.7.0) (2020-03-18)


### Features

* upgrade importer to netcore3 ([c911088](https://github.com/informatievlaanderen/building-registry/commit/c911088))

## [2.6.2](https://github.com/informatievlaanderen/building-registry/compare/v2.6.1...v2.6.2) (2020-03-12)


### Bug Fixes

* extract building unit gets updated correctly for building events ([8be16f6](https://github.com/informatievlaanderen/building-registry/commit/8be16f6))

## [2.6.1](https://github.com/informatievlaanderen/building-registry/compare/v2.6.0...v2.6.1) (2020-03-11)


### Bug Fixes

* count building and units now counts correctly ([7a386bb](https://github.com/informatievlaanderen/building-registry/commit/7a386bb))

# [2.6.0](https://github.com/informatievlaanderen/building-registry/compare/v2.5.13...v2.6.0) (2020-03-10)


### Features

* add totaal aantal endpoints ([3a00be9](https://github.com/informatievlaanderen/building-registry/commit/3a00be9))

## [2.5.13](https://github.com/informatievlaanderen/building-registry/compare/v2.5.12...v2.5.13) (2020-03-10)


### Performance Improvements

* don't track ef entities in legacy syndication ([7a2fe1d](https://github.com/informatievlaanderen/building-registry/commit/7a2fe1d))

## [2.5.12](https://github.com/informatievlaanderen/building-registry/compare/v2.5.11...v2.5.12) (2020-03-06)


### Bug Fixes

* correct building sync projection convert localdate to instant ([951d6f0](https://github.com/informatievlaanderen/building-registry/commit/951d6f0))

## [2.5.11](https://github.com/informatievlaanderen/building-registry/compare/v2.5.10...v2.5.11) (2020-03-05)


### Bug Fixes

* correct convert datetime to instant for sync projection ([45df361](https://github.com/informatievlaanderen/building-registry/commit/45df361))

## [2.5.10](https://github.com/informatievlaanderen/building-registry/compare/v2.5.9...v2.5.10) (2020-03-05)


### Bug Fixes

* correct provenance ([411157d](https://github.com/informatievlaanderen/building-registry/commit/411157d))

## [2.5.9](https://github.com/informatievlaanderen/building-registry/compare/v2.5.8...v2.5.9) (2020-03-05)


### Bug Fixes

* update grar common to fix provenance ([8cbed0c](https://github.com/informatievlaanderen/building-registry/commit/8cbed0c))

## [2.5.8](https://github.com/informatievlaanderen/building-registry/compare/v2.5.7...v2.5.8) (2020-03-04)


### Bug Fixes

* bump netcore dockerfiles ([7ad59e4](https://github.com/informatievlaanderen/building-registry/commit/7ad59e4))

## [2.5.7](https://github.com/informatievlaanderen/building-registry/compare/v2.5.6...v2.5.7) (2020-03-03)


### Bug Fixes

* bump netcore to 3.1.2 ([bb6e9c0](https://github.com/informatievlaanderen/building-registry/commit/bb6e9c0))
* remove unique index from buildingunit ([b2fbd78](https://github.com/informatievlaanderen/building-registry/commit/b2fbd78))
* update dockerid detection ([2094510](https://github.com/informatievlaanderen/building-registry/commit/2094510))

## [2.5.6](https://github.com/informatievlaanderen/building-registry/compare/v2.5.5...v2.5.6) (2020-02-27)


### Bug Fixes

* update json serialization dependencies ([c7deb1e](https://github.com/informatievlaanderen/building-registry/commit/c7deb1e))

## [2.5.5](https://github.com/informatievlaanderen/building-registry/compare/v2.5.4...v2.5.5) (2020-02-24)


### Bug Fixes

* update projection handling & sync migrator ([0360c85](https://github.com/informatievlaanderen/building-registry/commit/0360c85))

## [2.5.4](https://github.com/informatievlaanderen/building-registry/compare/v2.5.3...v2.5.4) (2020-02-21)


### Performance Improvements

* increase performance by removing count from lists ([bc5d938](https://github.com/informatievlaanderen/building-registry/commit/bc5d938))

## [2.5.3](https://github.com/informatievlaanderen/building-registry/compare/v2.5.2...v2.5.3) (2020-02-21)


### Bug Fixes

* force build ([7995dfe](https://github.com/informatievlaanderen/building-registry/commit/7995dfe))

## [2.5.2](https://github.com/informatievlaanderen/building-registry/compare/v2.5.1...v2.5.2) (2020-02-21)


### Performance Improvements

* add index on buildingunit address ([1a8a2ee](https://github.com/informatievlaanderen/building-registry/commit/1a8a2ee))

## [2.5.1](https://github.com/informatievlaanderen/building-registry/compare/v2.5.0...v2.5.1) (2020-02-20)


### Bug Fixes

* revert experiment ([113dc30](https://github.com/informatievlaanderen/building-registry/commit/113dc30))

# [2.5.0](https://github.com/informatievlaanderen/building-registry/compare/v2.4.15...v2.5.0) (2020-02-20)


### Features

* set count -1 ([e2961c9](https://github.com/informatievlaanderen/building-registry/commit/e2961c9))

## [2.4.15](https://github.com/informatievlaanderen/building-registry/compare/v2.4.14...v2.4.15) (2020-02-20)


### Bug Fixes

* add index on synced address persistent id ([6c05490](https://github.com/informatievlaanderen/building-registry/commit/6c05490))

## [2.4.14](https://github.com/informatievlaanderen/building-registry/compare/v2.4.13...v2.4.14) (2020-02-20)


### Bug Fixes

* update grar common ([1ecec78](https://github.com/informatievlaanderen/building-registry/commit/1ecec78))

## [2.4.13](https://github.com/informatievlaanderen/building-registry/compare/v2.4.12...v2.4.13) (2020-02-19)


### Bug Fixes

* order by is now in api's + added clustered indexes ([cdb7dd6](https://github.com/informatievlaanderen/building-registry/commit/cdb7dd6))

## [2.4.12](https://github.com/informatievlaanderen/building-registry/compare/v2.4.11...v2.4.12) (2020-02-18)


### Bug Fixes

* projections extract buildingunit when building incomplete ([3cfd92b](https://github.com/informatievlaanderen/building-registry/commit/3cfd92b))

## [2.4.11](https://github.com/informatievlaanderen/building-registry/compare/v2.4.10...v2.4.11) (2020-02-18)


### Bug Fixes

* check for not null buildingunits on extract ([271e0df](https://github.com/informatievlaanderen/building-registry/commit/271e0df))

## [2.4.10](https://github.com/informatievlaanderen/building-registry/compare/v2.4.9...v2.4.10) (2020-02-17)


### Bug Fixes

* update packages to fix json order ([8a5c2c6](https://github.com/informatievlaanderen/building-registry/commit/8a5c2c6))

## [2.4.9](https://github.com/informatievlaanderen/building-registry/compare/v2.4.8...v2.4.9) (2020-02-17)


### Bug Fixes

* upgrade Grar common libs ([901e3c4](https://github.com/informatievlaanderen/building-registry/commit/901e3c4))

## [2.4.8](https://github.com/informatievlaanderen/building-registry/compare/v2.4.7...v2.4.8) (2020-02-14)


### Bug Fixes

* add indexes on lists ([7d25d0e](https://github.com/informatievlaanderen/building-registry/commit/7d25d0e))

## [2.4.7](https://github.com/informatievlaanderen/building-registry/compare/v2.4.6...v2.4.7) (2020-02-11)


### Bug Fixes

* correct status projections added buildingunit to retired building ([709a727](https://github.com/informatievlaanderen/building-registry/commit/709a727))

## [2.4.6](https://github.com/informatievlaanderen/building-registry/compare/v2.4.5...v2.4.6) (2020-02-11)


### Bug Fixes

* set interior ring to null when no rings present ([c8e5381](https://github.com/informatievlaanderen/building-registry/commit/c8e5381))

## [2.4.5](https://github.com/informatievlaanderen/building-registry/compare/v2.4.4...v2.4.5) (2020-02-10)


### Bug Fixes

* json default values for nullable fields ([05bf81d](https://github.com/informatievlaanderen/building-registry/commit/05bf81d))

## [2.4.4](https://github.com/informatievlaanderen/building-registry/compare/v2.4.3...v2.4.4) (2020-02-04)


### Bug Fixes

* instanceuri for error example show correctly ([726aa3b](https://github.com/informatievlaanderen/building-registry/commit/726aa3b))

## [2.4.3](https://github.com/informatievlaanderen/building-registry/compare/v2.4.2...v2.4.3) (2020-02-03)


### Bug Fixes

* add type to problemdetails ([8cbb4d4](https://github.com/informatievlaanderen/building-registry/commit/8cbb4d4))

## [2.4.2](https://github.com/informatievlaanderen/building-registry/compare/v2.4.1...v2.4.2) (2020-02-03)


### Bug Fixes

* specify non nullable responses ([fd3504a](https://github.com/informatievlaanderen/building-registry/commit/fd3504a))

## [2.4.1](https://github.com/informatievlaanderen/building-registry/compare/v2.4.0...v2.4.1) (2020-02-03)


### Bug Fixes

* add nullable shapecontent for building ([e166e53](https://github.com/informatievlaanderen/building-registry/commit/e166e53))
* load of collection not needed in creating new entity - syndication ([066af64](https://github.com/informatievlaanderen/building-registry/commit/066af64))

# [2.4.0](https://github.com/informatievlaanderen/building-registry/compare/v2.3.10...v2.4.0) (2020-02-01)


### Features

* upgrade netcoreapp31 and dependencies ([adddb39](https://github.com/informatievlaanderen/building-registry/commit/adddb39))

## [2.3.10](https://github.com/informatievlaanderen/building-registry/compare/v2.3.9...v2.3.10) (2020-01-31)


### Bug Fixes

* unit completeness based on building now stored seperatly ([2b73426](https://github.com/informatievlaanderen/building-registry/commit/2b73426))

## [2.3.9](https://github.com/informatievlaanderen/building-registry/compare/v2.3.8...v2.3.9) (2020-01-31)


### Performance Improvements

* add index on IsComplete filter for views WMS ([ef170a2](https://github.com/informatievlaanderen/building-registry/commit/ef170a2))

## [2.3.8](https://github.com/informatievlaanderen/building-registry/compare/v2.3.7...v2.3.8) (2020-01-31)


### Performance Improvements

* add indexes on wms on status to improve performance ([151db36](https://github.com/informatievlaanderen/building-registry/commit/151db36))

## [2.3.7](https://github.com/informatievlaanderen/building-registry/compare/v2.3.6...v2.3.7) (2020-01-30)


### Bug Fixes

* add migration for extract changes ([c620036](https://github.com/informatievlaanderen/building-registry/commit/c620036))

## [2.3.6](https://github.com/informatievlaanderen/building-registry/compare/v2.3.5...v2.3.6) (2020-01-30)


### Bug Fixes

* xml (gml) coordinates are now rounded on 11 digits ([d9e5e5d](https://github.com/informatievlaanderen/building-registry/commit/d9e5e5d))

## [2.3.5](https://github.com/informatievlaanderen/building-registry/compare/v2.3.4...v2.3.5) (2020-01-29)


### Bug Fixes

* update grar packages ([f22ec21](https://github.com/informatievlaanderen/building-registry/commit/f22ec21))

## [2.3.4](https://github.com/informatievlaanderen/building-registry/compare/v2.3.3...v2.3.4) (2020-01-29)


### Bug Fixes

* correct unit version and status from building events ([c7ef3b6](https://github.com/informatievlaanderen/building-registry/commit/c7ef3b6))

## [2.3.3](https://github.com/informatievlaanderen/building-registry/compare/v2.3.2...v2.3.3) (2020-01-29)


### Bug Fixes

* correct SRID for WMS projection ([3354390](https://github.com/informatievlaanderen/building-registry/commit/3354390))

## [2.3.2](https://github.com/informatievlaanderen/building-registry/compare/v2.3.1...v2.3.2) (2020-01-24)


### Bug Fixes

* add syndication to api references ([e8684c9](https://github.com/informatievlaanderen/building-registry/commit/e8684c9))

## [2.3.1](https://github.com/informatievlaanderen/building-registry/compare/v2.3.0...v2.3.1) (2020-01-23)


### Bug Fixes

* syndication distributedlock runs async ([0cbf41a](https://github.com/informatievlaanderen/building-registry/commit/0cbf41a))

# [2.3.0](https://github.com/informatievlaanderen/building-registry/compare/v2.2.0...v2.3.0) (2020-01-23)


### Features

* upgrade projectionhandling ([0878d49](https://github.com/informatievlaanderen/building-registry/commit/0878d49))

# [2.2.0](https://github.com/informatievlaanderen/building-registry/compare/v2.1.4...v2.2.0) (2020-01-23)


### Features

* use distributed lock for syndication ([379590d](https://github.com/informatievlaanderen/building-registry/commit/379590d))

## [2.1.4](https://github.com/informatievlaanderen/building-registry/compare/v2.1.3...v2.1.4) (2020-01-17)


### Bug Fixes

* get api's working again ([4e4ea42](https://github.com/informatievlaanderen/building-registry/commit/4e4ea42))

## [2.1.3](https://github.com/informatievlaanderen/building-registry/compare/v2.1.2...v2.1.3) (2020-01-15)


### Bug Fixes

* building unit addresses are now ordered ([3ec94fd](https://github.com/informatievlaanderen/building-registry/commit/3ec94fd))

## [2.1.2](https://github.com/informatievlaanderen/building-registry/compare/v2.1.1...v2.1.2) (2020-01-10)


### Bug Fixes

* map crab geometry method surveyed to outlined ([c4ecc25](https://github.com/informatievlaanderen/building-registry/commit/c4ecc25))
* tests to accomodate geometry changes ([a20f402](https://github.com/informatievlaanderen/building-registry/commit/a20f402))

## [2.1.1](https://github.com/informatievlaanderen/building-registry/compare/v2.1.0...v2.1.1) (2020-01-03)


### Bug Fixes

* increase wms timeout ([0d7f985](https://github.com/informatievlaanderen/building-registry/commit/0d7f985))

# [2.1.0](https://github.com/informatievlaanderen/building-registry/compare/v2.0.0...v2.1.0) (2020-01-03)


### Features

* allow only one projector instance ([5cfe37e](https://github.com/informatievlaanderen/building-registry/commit/5cfe37e))

# [2.0.0](https://github.com/informatievlaanderen/building-registry/compare/v1.14.14...v2.0.0) (2019-12-26)


### Code Refactoring

* upgrade to netcoreapp31 ([6886b06](https://github.com/informatievlaanderen/building-registry/commit/6886b06))


### BREAKING CHANGES

* Upgrade to .NET Core 3.1

## [1.14.14](https://github.com/informatievlaanderen/building-registry/compare/v1.14.13...v1.14.14) (2019-12-18)


### Performance Improvements

* increase timeout for wms projections ([a1f100d](https://github.com/informatievlaanderen/building-registry/commit/a1f100d))

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
