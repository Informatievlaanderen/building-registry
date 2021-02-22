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
