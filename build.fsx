#r "paket:
version 5.241.6
framework: netstandard20
source https://api.nuget.org/v3/index.json
nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 4.1.0 //"

#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.FileSystemOperators
open ``Build-generic``

let product = "Basisregisters Vlaanderen"
let copyright = "Copyright (c) Vlaamse overheid"
let company = "Vlaamse overheid"

let dockerRepository = "building-registry"
let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "%s")

let build = buildSolution assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let test = testSolution
let publish = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

supportedRuntimeIdentifiers <- [ "linux-x64" ]

// Solution -----------------------------------------------------------------------

Target.create "Restore_Solution" (fun _ -> restore "BuildingRegistry")

Target.create "Build_Solution" (fun _ ->
  setVersions "SolutionInfo.cs"
  build "BuildingRegistry")

Target.create "Test_Solution" (fun _ -> test "BuildingRegistry")

Target.create "Publish_Solution" (fun _ ->
  [
    "BuildingRegistry.Projector"
    "BuildingRegistry.Api.Legacy"
    "BuildingRegistry.Api.Extract"
    "BuildingRegistry.Api.CrabImport"
    "BuildingRegistry.Projections.Legacy"
    "BuildingRegistry.Projections.Extract"
    "BuildingRegistry.Projections.LastChangedList"
    "BuildingRegistry.Projections.Syndication"
  ] |> List.iter publish)

Target.create "Pack_Solution" (fun _ ->
  [
    "BuildingRegistry.Projector"
    "BuildingRegistry.Api.Legacy"
    "BuildingRegistry.Api.Extract"
    "BuildingRegistry.Api.CrabImport"
  ] |> List.iter pack)

Target.create "Containerize_Projector" (fun _ -> containerize "BuildingRegistry.Projector" "projector")
Target.create "PushContainer_Projector" (fun _ -> push "projector")

Target.create "Containerize_ApiLegacy" (fun _ -> containerize "BuildingRegistry.Api.Legacy" "api-legacy")
Target.create "PushContainer_ApiLegacy" (fun _ -> push "api-legacy")

Target.create "Containerize_ApiExtract" (fun _ -> containerize "BuildingRegistry.Api.Extract" "api-extract")
Target.create "PushContainer_ApiExtract" (fun _ -> push "api-extract")

Target.create "Containerize_ApiCrabImport" (fun _ ->
  let dist = (buildDir @@ "BuildingRegistry.Api.CrabImport" @@ "linux")
  let source = "assets" @@ "sss"

  //Shell.copyFile dist (source @@ "SqlStreamStore.dll")
  //Shell.copyFile dist (source @@ "SqlStreamStore.MsSql.dll")

  containerize "BuildingRegistry.Api.CrabImport" "api-crab-import")

Target.create "PushContainer_ApiCrabImport" (fun _ -> push "api-crab-import")

Target.create "Containerize_ProjectionsLegacy" (fun _ -> containerize "BuildingRegistry.Projections.Legacy" "projections-legacy")
Target.create "PushContainer_ProjectionsLegacy" (fun _ -> push "projections-legacy")

Target.create "Containerize_ProjectionsExtract" (fun _ -> containerize "BuildingRegistry.Projections.Extract" "projections-extract")
Target.create "PushContainer_ProjectionsExtract" (fun _ -> push "projections-extract")

Target.create "Containerize_ProjectionsSyndication" (fun _ -> containerize "BuildingRegistry.Projections.Syndication" "projections-syndication")
Target.create "PushContainer_ProjectionsSyndication" (fun _ -> push "projections-syndication")

// --------------------------------------------------------------------------------

Target.create "Build" ignore
Target.create "Test" ignore
Target.create "Publish" ignore
Target.create "Pack" ignore
Target.create "Containerize" ignore
Target.create "Push" ignore

"NpmInstall"
  ==> "DotNetCli"
  ==> "Clean"
  ==> "Restore_Solution"
  ==> "Build_Solution"
  ==> "Build"

"Build"
  ==> "Test_Solution"
  ==> "Test"

"Test"
  ==> "Publish_Solution"
  ==> "Publish"

"Publish"
  ==> "Pack_Solution"
  ==> "Pack"

"Pack"
  ==> "Containerize_Projector"
  ==> "Containerize_ApiLegacy"
  ==> "Containerize_ApiExtract"
  ==> "Containerize_ApiCrabImport"
  ==> "Containerize_ProjectionsSyndication"
  ==> "Containerize"
// Possibly add more projects to containerize here

"Containerize"
  ==> "DockerLogin"
  ==> "PushContainer_Projector"
  ==> "PushContainer_ApiLegacy"
  ==> "PushContainer_ApiExtract"
  ==> "PushContainer_ApiCrabImport"
  ==> "PushContainer_ProjectionsSyndication"
  ==> "Push"
// Possibly add more projects to push here

// By default we build & test
Target.runOrDefault "Test"
