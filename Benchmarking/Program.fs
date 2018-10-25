// Learn more about F# at http://fsharp.org

module Benchmarking

open System
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Processing
open SixLabors.ImageSharp.Advanced
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Jobs
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open SixLabors.Memory
open SixLabors.ImageSharp.PixelFormats
open BenchmarkDotNet.Exporters
open BenchmarkDotNet.Exporters.Csv
open BenchmarkDotNet.Columns
open BenchmarkDotNet.Reports
open SixLabors.ImageSharp.Formats.Png
open System.IO

// Much of this is basically copied/adapted from https://github.com/fsharp/fsharp/blob/master/tests/scripts/array-perf/array-perf.fs

type PerfConfig () =
    inherit ManualConfig ()
    do
        base.Add (Job.RyuJitX64.WithGcServer(true).WithGcForce(true).WithGcConcurrent(true))
        base.Add (MemoryDiagnoser.Default)
        base.Add (MarkdownExporter.GitHub)
        let ss = SummaryStyle.Default
        ss.PrintUnitsInHeader <- true
        ss.PrintUnitsInContent <- false
        base.Add(CsvExporter(CsvSeparator.Comma, ss))
        base.Add StatisticColumn.Min

[<Config (typeof<PerfConfig>)>]
type PerfBenchmark () =

    let mutable intensities = [||]
    let mutable imgWidth = 5
    let mutable imgHeight = 5
    let mutable out_img = new Image<Rgb24>(imgWidth, imgHeight)

    //[<Params (3, 5, 7, 9, 11)>]
    [<Params (3, 5, 7)>]
    //[<Params (3)>]
    member val public windowSize = 0 with get, set

    //[<Params ("very small", "small", "medium", "peppers_gray", "big", "very big")>]
    [<Params ("very small", "small", "medium", "peppers_gray")>]
    //[<Params ("very small")>]
    member val public filename = "" with get, set

    [<GlobalSetup>]
    member self.SetupData () =
        use img = Image.Load(@"..\Images\Inputs\" + self.filename + "_noisy.png")
        img.Mutate(fun x -> x.Grayscale() |> ignore)
        intensities <- img.GetPixelSpan().ToArray() |> Array.map (fun p -> p.R)
        imgWidth <- img.Width
        imgHeight <- img.Height
        img.Dispose()


    [<Benchmark(Baseline=true,Description="naive")>]
    member self.naive () =
        out_img <- Naive.medianFilter intensities imgWidth imgHeight self.windowSize

     [<Benchmark(Description="Braunl")>]
     member self.Braunl () =
         out_img <- Braunl.medianFilter intensities imgWidth imgHeight self.windowSize

     [<Benchmark(Description="cml")>]
     member self.cml () =
         out_img <- CML.medianFilter intensities imgWidth imgHeight self.windowSize


[<EntryPoint>]
let main argv =

    Configuration.Default.MemoryAllocator <- ArrayPoolMemoryAllocator.CreateWithModeratePooling()

    //let switch = BenchmarkSwitcher [|typeof<PerfBenchmark>|]

    //switch.Run argv |> ignore

    let base_filenames = ["very small"; "small"; "medium"; "peppers_gray"]
    let window_sizes = [3; 5; 7; 9; 11]
    let inputdir = @"..\Images\Inputs\"
    let outputdirbase = @"..\Images\Outputs\"

    for fn in base_filenames do
        (* use input_image = Image.Load(inputdir + fn + ".png")
        input_image.Mutate(fun x -> x.Grayscale() |> ignore)
        use out_file = new System.IO.FileStream(outputdirbase + @"\gray\" + fn + "_gray.png", FileMode.OpenOrCreate) *)
        let pngenc = PngEncoder()
        pngenc.ColorType <- PngColorType.Rgb
        (* input_image.Save(out_file, pngenc)
        let intensities = img.GetPixelSpan().ToArray() |> Array.map (fun p -> p.R)
        let imgWidth = input_image.Width
        let imgHeight = input_image.Height *)

        use input_image = Image.Load(inputdir + fn + "_noisy.png")
        input_image.Mutate(fun x -> x.Grayscale() |> ignore)
        let intensities = input_image.GetPixelSpan().ToArray() |> Array.map (fun p -> p.R)
        let imgWidth = input_image.Width
        let imgHeight = input_image.Height

        for ws in window_sizes do
            use naive_file = new System.IO.FileStream(outputdirbase + @"\Naive\" + fn + "_naive_" + string ws + ".png", FileMode.OpenOrCreate)
            use out_img = Naive.medianFilter intensities imgWidth imgHeight ws
            out_img.Save(naive_file, pngenc)
            naive_file.Dispose()
            out_img.Dispose()

            use braunl_file = new System.IO.FileStream(outputdirbase + @"\Braunl\" + fn + "_braunl_" + string ws + ".png", FileMode.OpenOrCreate)
            use out_img = Braunl.medianFilter intensities imgWidth imgHeight ws
            out_img.Save(braunl_file, pngenc)
            braunl_file.Dispose()
            out_img.Dispose()

            use cml_file = new System.IO.FileStream(outputdirbase + @"\CML\" + fn + "_cml_" + string ws + ".png", FileMode.OpenOrCreate)
            use out_img = CML.medianFilter intensities imgWidth imgHeight ws
            out_img.Save(cml_file, pngenc)
            cml_file.Dispose()
            out_img.Dispose()

        input_image.Dispose()


    0 // return an integer exit code
