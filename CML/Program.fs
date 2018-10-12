﻿// Learn more about F# at http://fsharp.org

module CML

open Hopac
open Hopac.Extensions
open Hopac.Infixes
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Processing
open SixLabors.ImageSharp.Advanced
open SixLabors.Memory
open SixLabors.ImageSharp.Formats.Png

type 'a Pix = {
    intensity: 'a
    index: int
    neighbours: 'a option list
    chan: 'a option Ch
}

type 'a Choice =
    | Give
    | Take of ('a option * int)

let listDisplacements ws =
    let ub = (ws - 1) / 2
    let lb = -ub
    let mutable disps = List.empty
    for i in lb..ub do
        for j in lb..ub do
            if i = 0 && j = 0 then
                ()
            else
                disps <- (i, j) :: disps
    disps


let findIndex width x y =
    x + width * y

let findCoords width index =
    (index % width, index / width)

let makeNeighboursIndexList' pix coordFinder indexFinder windowSize =
    let x,y = coordFinder pix.index
    listDisplacements windowSize |>
    List.map (fun (dx, dy) ->
        indexFinder (x + dx) (y + dy)
    )

let takeIntensity pixels neighbourIndex =
    if neighbourIndex < 0 || neighbourIndex >= (Array.length pixels) then
        Alt.always (Take(None, neighbourIndex))
    else
        Ch.take pixels.[neighbourIndex].chan
            ^-> (fun i -> Take (i, neighbourIndex))


let inline findListMedian l =
    List.sort l
    |> (fun m -> m.[m.Length / 2])

let createGive pix =
    pix.chan *<- Some(pix.intensity)
                ^->. Give

let createTake pixels neighbour =
    takeIntensity pixels neighbour

let buildAlts (pixels: 'a Pix []) pix neighbour =
    let give = pix.chan *<- Some(pix.intensity)
                ^->. Give
    let take = takeIntensity pixels neighbour
    [give; take]

let buildAlts' (pixels: 'a Pix []) pix neighbours =
   let give = pix.chan *<- Some(pix.intensity)
               ^->. Give
   let takes = List.map (takeIntensity pixels) neighbours
   (give, takes)

let makeRgb24 intensity = Rgb24(intensity, intensity, intensity)

(* let runPixel coordFinder indexFinder pixels barrier windowSize (outputArray: Rgb24 []) pix =
    let neighboursIndexList = makeNeighboursIndexList' pix coordFinder indexFinder windowSize
    let ba = buildAlts pixels pix
    let alts = ba (List.head neighboursIndexList)
    job {
        do! Job.iterateServer ((List.tail neighboursIndexList), pix, alts) <| fun (neighbours, p, alts) ->
                Alt.choose alts |> Alt.afterFun (fun x ->
                                                    match x with
                                                    | Give -> (neighbours, p, alts)
                                                    | Take(n,i) ->
                                                        if List.isEmpty neighbours then
                                                            let median = List.choose id p.neighbours |> findListMedian
                                                            outputArray.[p.index] <- median |> makeRgb24
                                                            Latch.decrement barrier |> run
                                                            ([], p, [pix.chan *<- Some(pix.intensity) ^->. Give])
                                                        else
                                                            let newAlts = ba (List.head neighbours)
                                                            ((List.tail neighbours), {p with neighbours = n :: p.neighbours}, newAlts)

                )
        return pix
    } *)

let runPixel coordFinder indexFinder pixels barrier windowSize (outputArray: Rgb24 []) pix =
    let neighboursIndexList = makeNeighboursIndexList' pix coordFinder indexFinder windowSize
    let ba = buildAlts' pixels pix
    let (give, takes) = ba neighboursIndexList
    //let alts = [give; (List.head takes)]
    job {
        do! Job.iterateServer ((List.tail neighboursIndexList), pix, give, takes) <| fun (neighbours, p, give, takes) ->
                Alt.choose [give; (List.head takes)] |> Alt.afterFun (fun x ->
                                                    match x with
                                                    | Give -> (neighbours, p, give, takes)
                                                    | Take(n,i) ->
                                                        if List.isEmpty neighbours then
                                                            let median = List.choose id p.neighbours |> findListMedian
                                                            outputArray.[p.index] <- median |> makeRgb24
                                                            Latch.decrement barrier |> run
                                                            ([], p, give, List.singleton (Alt.never ()))
                                                        else
                                                            //let newAlts = ba (List.head neighbours)
                                                            ((List.tail neighbours), {p with neighbours = n :: p.neighbours}, give, (List.tail takes))

                )
        return pix
    }

let storeMedians (arr: Rgb24 []) oachan = job {
    let! (index, median) = Ch.take oachan
    arr.[index] <- makeRgb24 median
}

let medianFilter intensities width height windowSize =
    let pixelCount = width * height
    let fc = findCoords width
    let fi = findIndex width
    let barrier = Hopac.Latch pixelCount
    let outputArray = Array.zeroCreate pixelCount
    let pixels = Array.Parallel.mapi (fun i x -> {intensity = x; index = i; neighbours = [Some(x)]; chan = Ch ();}) intensities
    let runpix = runPixel fc fi pixels barrier windowSize outputArray

    Seq.Con.mapJob runpix pixels |> run |> ignore
    job {do! (Latch.await barrier)} |> run
    Image.LoadPixelData(outputArray, width, height)


let run input calc =
    let inputList = Seq.toList input
    let rec subrun inp acc =
        match inp with
        | [] -> (acc, "Done")
        | (x :: xs) ->
            let res = calc x
            match res with
            | Some(y) -> subrun xs (acc + y)
            | None -> (acc, "Error")
    subrun inputList 0


[<EntryPoint>]
let main argv =
    let filename = argv.[0]
    let numIterations = int argv.[1]
    let windowSize = int argv.[2]

    Configuration.Default.MemoryAllocator <- ArrayPoolMemoryAllocator.CreateWithModeratePooling()

    use img: Image<Rgb24> = Image.Load(@"..\Images\Inputs\" + filename)

    img.Mutate(fun x -> x.Grayscale() |> ignore)

    let mutable out_img = new Image<Rgb24>(img.Width, img.Height)

    let timer = System.Diagnostics.Stopwatch ()

    for _ in 1..numIterations do

        System.GC.Collect()

        timer.Start ()

        let imageWidth = img.Width
        let imageHeight = img.Height
        let pixelCount = imageWidth * imageHeight
        let intensities = img.GetPixelSpan().ToArray() |> Array.Parallel.map (fun p -> p.R)
        out_img <- medianFilter intensities img.Width img.Height windowSize
        timer.Stop ()

    use out_file = new System.IO.FileStream(@"..\Images\Outputs\cml_" + System.IO.Path.GetFileNameWithoutExtension(filename) +
                    "_" + string windowSize +  ".png", System.IO.FileMode.OpenOrCreate)

    let pngenc = PngEncoder()
    pngenc.ColorType <- PngColorType.Rgb
    out_img.Save(out_file, pngenc)

    let totalTimeTaken = timer.Elapsed.TotalSeconds
    printfn "Total time was %f" totalTimeTaken
    printfn "Average time was %f" (totalTimeTaken / (float numIterations))
    0 // return an integer exit code
