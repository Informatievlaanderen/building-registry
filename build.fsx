#r "paket:
version 7.0.2
framework: net6.0
source https://api.nuget.org/v3/index.json

nuget Microsoft.Build 17.3.2
nuget Microsoft.Build.Framework 17.3.2
nuget Microsoft.Build.Tasks.Core 17.3.2
nuget Microsoft.Build.Utilities.Core 17.3.2

nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 6.0.6 //"

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

let buildSolution = buildSolution assemblyVersionNumber
let buildSource = build assemblyVersionNumber
let buildTest = buildTest assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let test = testSolution
let publishSource = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

supportedRuntimeIdentifiers <- [ "msil"; "linux-x64" ]

// Solution -----------------------------------------------------------------------

Target.create "Restore_Solution" (fun _ -> restore "BuildingRegistry")

Target.create "Build_Solution" (fun _ ->
  setVersions "SolutionInfo.cs"
  buildSolution "BuildingRegistry")

Target.create "Test_Solution" (fun _ ->
    [
        "test" @@ "BuildingRegistry.Tests"
    ] |> List.iter testWithDotNet
)

Target.create "Publish_Solution" (fun _ ->
  [
    "BuildingRegistry.Projector"
    "BuildingRegistry.Api.Legacy"
    "BuildingRegistry.Api.Oslo"
    "BuildingRegistry.Api.Extract"
    "BuildingRegistry.Api.CrabImport"
    "BuildingRegistry.Api.BackOffice"
    "BuildingRegistry.Api.BackOffice.Abstractions"
    "BuildingRegistry.Api.BackOffice.Handlers.Lambda"
    "BuildingRegistry.Consumer.Address"
    "BuildingRegistry.Consumer.Read.Parcel"
    "BuildingRegistry.Projections.Legacy"
    "BuildingRegistry.Projections.Extract"
    "BuildingRegistry.Projections.LastChangedList"
    "BuildingRegistry.Projections.Syndication"
    "BuildingRegistry.Projections.BackOffice"
    "BuildingRegistry.Migrator.Building"
    "BuildingRegistry.Producer"
    "BuildingRegistry.Producer.Snapshot.Oslo"
  ] |> List.iter publishSource)

Target.create "Pack_Solution" (fun _ ->
  [
    "BuildingRegistry.Api.Legacy"
    "BuildingRegistry.Api.Oslo"
    "BuildingRegistry.Api.Extract"
    "BuildingRegistry.Api.CrabImport"
    "BuildingRegistry.Api.BackOffice"
    "BuildingRegistry.Api.BackOffice.Abstractions"
  ] |> List.iter pack)

Target.create "Containerize_Projector" (fun _ -> containerize "BuildingRegistry.Projector" "projector")
Target.create "Containerize_ApiLegacy" (fun _ -> containerize "BuildingRegistry.Api.Legacy" "api-legacy")
Target.create "Containerize_ApiOslo" (fun _ -> containerize "BuildingRegistry.Api.Oslo" "api-oslo")
Target.create "Containerize_ApiExtract" (fun _ -> containerize "BuildingRegistry.Api.Extract" "api-extract")
Target.create "Containerize_ApiBackOffice" (fun _ -> containerize "BuildingRegistry.Api.BackOffice" "api-backoffice")
Target.create "Containerize_ApiCrabImport" (fun _ ->
  let dist = (buildDir @@ "BuildingRegistry.Api.CrabImport" @@ "linux")
  let source = "assets" @@ "sss"

  //Shell.copyFile dist (source @@ "SqlStreamStore.dll")
  //Shell.copyFile dist (source @@ "SqlStreamStore.MsSql.dll")

  containerize "BuildingRegistry.Api.CrabImport" "api-crab-import")
Target.create "Containerize_ProjectionsSyndication" (fun _ -> containerize "BuildingRegistry.Projections.Syndication" "projections-syndication")
Target.create "Containerize_ProjectionsBackOffice" (fun _ -> containerize "BuildingRegistry.Projections.BackOffice" "projections-backoffice")
Target.create "Containerize_ConsumerAddress" (fun _ -> containerize "BuildingRegistry.Consumer.Address" "consumer-address")
Target.create "Containerize_ConsumerParcel" (fun _ -> containerize "BuildingRegistry.Consumer.Read.Parcel" "consumer-read-parcel")
Target.create "Containerize_MigratorBuilding" (fun _ -> containerize "BuildingRegistry.Migrator.Building" "migrator-building")
Target.create "Containerize_Producer" (fun _ -> containerize "BuildingRegistry.Producer" "producer")
Target.create "Containerize_ProducerSnapshotOslo" (fun _ -> containerize "BuildingRegistry.Producer.Snapshot.Oslo" "producer-snapshot-oslo")

Target.create "SetAssemblyVersions" (fun _ -> setVersions "SolutionInfo.cs")
// --------------------------------------------------------------------------------

Target.create "Build" ignore
Target.create "Test" ignore
Target.create "Publish" ignore
Target.create "Pack" ignore
Target.create "Containerize" ignore

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
  // ==> "Containerize_Projector"
  // ==> "Containerize_ApiLegacy"
  // ==> "Containerize_ApiOslo"
  // ==> "Containerize_ApiExtract"
  // ==> "Containerize_ApiCrabImport"
  // ==> "Containerize_ApiBackOffice"
  // ==> "Containerize_ProjectionsSyndication"
  // ==> "Containerize_ProjectionsBackOffice"
  // ==> "Containerize_ConsumerAddress"
  // ==> "Containerize_MigratorBuilding"
  // ==> "Containerize_Producer"
  // ==> "Containerize_ProducerSnapshotOslo"
  ==> "Containerize"
// Possibly add more projects to containerize here

// By default we build & test
Target.runOrDefault "Test"
