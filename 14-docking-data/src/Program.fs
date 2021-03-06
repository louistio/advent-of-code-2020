open System.IO
open System

type Instruction =
    | SetMask of list<char>
    | SetMem of int64 * int64

let ParseInstructions (lines: string list) =
    lines
    |> List.map (fun line ->
        if line.StartsWith("ma") then
            let mask = line.Split(" = ").[1]
            let maskChars = mask |> Seq.toList
            SetMask maskChars
        else
            let split = line.Split("] = ")
            let address = split.[0].Split("[").[1] |> int64
            let value = int64 split.[1]

            SetMem (address, value)
    )

let SetMemoryPartOne memory mask (address: int64, value: int64) =
    let binaryValue = Convert.ToString(value, 2).PadLeft(36, '0')

    let maskedBinaryValue =
        binaryValue
        |> Seq.indexed
        |> Seq.map (fun (i, originalChar) ->
            match mask |> List.tryItem i with
            | Some maskChar when maskChar <> 'X' -> maskChar
            | _ -> originalChar
        )
        |> Seq.toArray
        |> String

    let newValue = Convert.ToInt64(maskedBinaryValue, 2)

    memory |> Map.change address (fun _ -> Some newValue)

let ExecuteInstructions instructions setMemory =
    let rec executeInstructions instructions i mask memory =
        match instructions |> List.tryItem i with
        | Some instruction ->
            let executeNextInstruction = executeInstructions instructions (i+1)

            match instruction with
            | SetMask value -> executeNextInstruction value memory
            | SetMem (address, value) ->
                let newMemory = setMemory memory mask (address, value)

                executeNextInstruction mask newMemory
        | None -> memory

    executeInstructions instructions 0 [] Map.empty


let SolvePuzzle lines =
    let instructions = ParseInstructions lines

    let memory = ExecuteInstructions instructions SetMemoryPartOne

    memory
    |> Map.toSeq
    |> Seq.sumBy (fun (_, value) -> value)


let SetMemoryPartTwo memory mask (address: int64, value: int64) =
    let binaryAddress = Convert.ToString(address, 2).PadLeft(36, '0')

    let maskedBinaryAddress =
        binaryAddress
        |> Seq.indexed
        |> Seq.map (fun (i, originalChar) ->
            match mask |> List.tryItem i with
            | Some maskChar when maskChar <> '0' -> maskChar
            | _ -> originalChar
        )
        |> Seq.toArray
        |> String

    let rec generatePossibleAddresses (line: string) =
        match line |> Seq.tryFindIndex ((=) 'X') with
        | Some i ->
            let lineWith0 = line.Remove(i, 1).Insert(i, "0")
            let lineWith1 = line.Remove(i, 1).Insert(i, "1")

            List.append (generatePossibleAddresses lineWith0) (generatePossibleAddresses lineWith1)
        | None -> [line]

    let allAddresses = generatePossibleAddresses maskedBinaryAddress

    allAddresses
    |> List.fold (fun mem matchingAddress -> mem |> Map.change (int64 (Convert.ToUInt64(matchingAddress, 2))) (fun _ -> Some (uint64 value))) memory


let SolvePuzzlePartTwo lines =
    let instructions = ParseInstructions lines

    let memory = ExecuteInstructions instructions SetMemoryPartTwo

    memory
    |> Map.toSeq
    |> Seq.sumBy (fun (_, value) -> value)

[<EntryPoint>]
let main argv =

    let lines =
        File.ReadAllLinesAsync(argv.[0])
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> List.ofArray

    printfn "Answer for part one is %d" (SolvePuzzle lines)
    printfn "Answer for part two is %d" (SolvePuzzlePartTwo lines)

    0
