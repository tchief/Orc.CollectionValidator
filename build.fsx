#r @"tools\FAKE\tools\FakeLib.dll"

open System
open System.IO
open System.Linq
open System.Text
open System.Text.RegularExpressions
open Fake
open Fake.MSTest

// --------------------------------------------------------------------------------------
// Definitions

let srcDir  = @".\"
let deploymentDir  = @".\deployment\"
let nugetPath = @".\.nuget\nuget.exe"
let nugetAccessKey = ""
let version = "1.0.0.0"

let outputDir = @".\Orc.CollectionValidator\bin\"
let outputDebugDir = outputDir + "debug"
let outputReleaseDir = outputDir + "release"
let testDir = srcDir + @"tests\"
let testOutputDir = srcDir + @"Orc.CollectionValidator.Test\bin\"

let outputDebugFiles = !! (outputDebugDir + @"\**\*.*")
                            -- "*.vshost.exe"
let outputReleaseFiles = !! (outputReleaseDir + @"\**\*.*")
                            -- "*.vshost.exe"

let tests = srcDir + @"\**\*.Test.csproj" 
let allProjects = srcDir + @"\**\*.csproj" 

let testProjects  = !! tests
let otherProjects = !! allProjects
                        -- tests

// --------------------------------------------------------------------------------------
// Clean build results

Target "CleanPackagesDirectory" (fun _ ->
    CleanDirs [(deploymentDir + "packages");]
)

Target "DeleteOutputFiles" (fun _ ->
    !! (outputDebugDir + @"\**\*.*")
        ++ (outputReleaseDir + @"\**\*.*")
        ++ (testDir + @"\**\*.*")
        ++ (testOutputDir + @"\**\*.*")
        -- "\**\*.vshost.exe"
    |> DeleteFiles
)

Target "DeleteOutputDirectories" (fun _ ->
    CreateDir outputDir
    DirectoryInfo(outputDir).GetDirectories("*", SearchOption.AllDirectories)
    |> Array.filter (fun d -> not (d.GetFiles("*.vshost.exe", SearchOption.AllDirectories).Any()))
    |> Array.map (fun d -> d.FullName)
    |> DeleteDirs
)

// --------------------------------------------------------------------------------------
// Build projects

RestorePackages()

Target "UpdateAssemblyVersion" (fun _ ->
      let solutionAssemblyInfo = srcDir + "Orc.CollectionValidator\Properties\AssemblyInfo.cs"
      let pattern = Regex("Assembly(|File)Version(\w*)\(.*\)")
      let result = "Assembly$1Version$2(\"" + version + "\")"
      let content = File.ReadAllLines(solutionAssemblyInfo, Encoding.Unicode)
                    |> Array.map(fun line -> pattern.Replace(line, result, 1))
      File.WriteAllLines(solutionAssemblyInfo, content, Encoding.Unicode)
)

Target "BuildOtherProjects" (fun _ ->    
    otherProjects
      |> MSBuildRelease "" "Rebuild" 
      |> Log "Build Other Projects: "
)

Target "BuildTests" (fun _ ->    
    testProjects
      |> MSBuildDebug "" "Rebuild" 
      |> Log "Build Tests: "
)

// --------------------------------------------------------------------------------------
// Run tests

Target "RunTests" (fun _ ->
    ActivateFinalTarget "CloseMSTestRunner"
    CleanDir testDir
    CreateDir testDir

    !! (testOutputDir + @"\**\*.Test.dll") 
      |> MSTest (fun p ->
                  { p with
                     TimeOut = TimeSpan.FromMinutes 20.
                     ResultsDir = testDir})
)

FinalTarget "CloseMSTestRunner" (fun _ ->  
    ProcessHelper.killProcess "mstest.exe"
)

// --------------------------------------------------------------------------------------
// Build a NuGet package

Target "NuGet" (fun _ ->
    let nugetAccessPublishKey = getBuildParamOrDefault "nugetkey" nugetAccessKey
    let getOutputFile ext = sprintf @"%s\Orc.CollectionValidator.%s" outputReleaseDir ext
    let libraryFiles = !! (getOutputFile "dll")
                        ++ (getOutputFile "xml")
    
    let libraryDependencies =
                    ["FluentValidation", GetPackageVersion "./packages/" "FluentValidation"]

    let workingDeploymentDir = deploymentDir + @"packages\"
    let dllDeploymentDir = workingDeploymentDir + @"lib\NET40\"
    let getNupkgFile = sprintf "%s\Orc.CollectionValidator.%s.nupkg" dllDeploymentDir version
    let getNuspecFile = sprintf "%stemplates\Orc.CollectionValidator.nuspec" deploymentDir

    let preparePackage filesToPackage = 
        CreateDir workingDeploymentDir
        CreateDir dllDeploymentDir
        CopyFiles dllDeploymentDir filesToPackage

    let cleanPackage name = 
        MoveFile workingDeploymentDir getNupkgFile
        DeleteDir (workingDeploymentDir + "lib")

    let doPackage dependencies =   
        NuGet (fun p -> 
            {p with
                Version = version
                ToolPath = nugetPath
                OutputPath = dllDeploymentDir
                WorkingDir = workingDeploymentDir
                Dependencies = dependencies
                Publish = not (String.IsNullOrEmpty nugetAccessPublishKey)
                AccessKey = nugetAccessPublishKey })
                getNuspecFile
    
    let doAll files depenencies =
        preparePackage files
        doPackage depenencies
        cleanPackage ""

    doAll libraryFiles libraryDependencies
)

// --------------------------------------------------------------------------------------
// Combined targets

Target "Clean" DoNothing
"CleanPackagesDirectory" ==> "DeleteOutputFiles" ==> "DeleteOutputDirectories" ==> "Clean"

Target "Build" DoNothing
"UpdateAssemblyVersion" ==> "BuildOtherProjects" ==> "Build"

Target "Tests" DoNothing
"BuildTests" ==> "RunTests" ==> "Tests"

Target "All" DoNothing
"Clean" ==> "All"
"Build" ==> "All"
"Tests" ==> "All"

Target "Release" DoNothing
"All" ==> "Release"
"NuGet" ==> "Release"
 
RunTargetOrDefault "All"
