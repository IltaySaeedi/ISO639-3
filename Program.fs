open System.Data.SQLite
open Vp.FSharp.Sql.Sqlite
open ISO639

let execNonQuery (connection: SQLiteConnection) (queries: (Async<option<string>>) list) =
    async {
        let! queriesString = 
            queries
            |> Async.Parallel
        if Array.forall (fun (query: string option) -> query.IsSome) queriesString then
            let commands = 
                queriesString
                |> Array.map 
                    (fun query -> 
                        SqliteCommand.text (Option.get query))
            let! rowsAffected =
                commands
                |> Array.map 
                    (fun command -> 
                        try 
                            SqliteCommand.executeNonQuery connection command
                        with
                            | exn ->
                                async {
                                    printfn "%s" exn.Message
                                    return -1})
                |> Async.Parallel
            return
                (Array.map (fun row -> sprintf "%i" row) rowsAffected)
        else
            return
                (queriesString
                |> Array.map
                    (fun query ->
                        match query with
                        | Some s ->
                            s.Split('\n')
                            |> Array.head
                        | None ->
                            "Invalid query"))
    }

let deleteFromTables() = 
    async {
        return 
            Some "DELETE FROM ISO_639_3;\n\
            DELETE FROM ISO_639_3_Macrolanguages;\n\
            DELETE FROM ISO_639_3_Names;\n\
            DELETE FROM ISO_639_3_Retirements;" }

let languagesToTable () =
    async {
        let! iso639 = ISO639Provider.AsyncLoad(ISO639)
        let languages = 
            seq { for row in iso639.Rows ->
                    toLanguage row }
        let numberOfRows = Seq.length languages
        match numberOfRows with
        | 0 -> 
            return None
        | 1 ->
            let query = 
                languages
                |> Seq.head
                |> Option.get
                |> (fun q -> Language.InsertString + q.ToString().Trim())
            return Some (query[0..query.Length - 2] + ";")
        | _ ->
            let head = 
                let text = 
                    (languages
                    |> Seq.head
                    |> Option.get).ToString().Trim()
                text[0..text.Length - 2] + ";"
            let query =
                (languages
                |> Seq.tail
                |> Seq.fold 
                    (fun acc lang ->
                        match lang with
                        | Some lang ->
                            let value = lang.ToString()
                            acc + value
                        | None ->
                            acc)
                    (Language.InsertString)) + head
            return Some query 
    }

let namesToTable() =
    async {
        let! iso639Names = ISO639NamesProvider.AsyncLoad(ISO639NameIndex)
        let names = 
            seq { for name in iso639Names.Rows ->
                    toLanguageName name }
        let numberOfRows = Seq.length names
        match numberOfRows with
        | 0 ->
            return None
        | 1 -> 
            let query = 
                names
                |> Seq.head
                |> (fun q -> Names.InsertString + q.ToString().Trim())
            return Some (query[0..query.Length - 2] + ";")
        | _ ->
            let head = 
                let text = (Seq.head names).ToString().Trim()
                text[0..text.Length - 2] + ";"
            let query =
                (names
                |> Seq.tail
                |> Seq.fold 
                    (fun acc name ->    
                        let value = name.ToString()
                        acc + value)
                    (Names.InsertString)) + head
            return Some query
    }

let macroLanguagesToTable () =
    async {
        let! iso639Macro = ISO639MacroProvider.AsyncLoad(ISO639Macrolanguages)
        let macroLanguages = 
            seq { for row in iso639Macro.Rows ->
                    toMacroLanguage row }
        let numberOfRows = Seq.length macroLanguages
        match numberOfRows with
        | 0 ->
            return None
        | 1 ->
            let query = 
                macroLanguages
                |> Seq.head
                |> Option.get
                |> (fun q -> MacroLanguages.InsertString + q.ToString().Trim())
            return Some (query[0..query.Length - 2] + ";")
        | _ ->
            let head = 
                let text = 
                    (macroLanguages
                    |> Seq.head
                    |> Option.get).ToString().Trim()
                text[0..text.Length - 2] + ";"
            let query =
                (macroLanguages
                |> Seq.tail
                |> Seq.fold 
                    (fun acc macroLang ->    
                        match macroLang with
                        | Some macroLang ->
                            let value = macroLang.ToString()
                            acc + value
                        | None ->
                            acc)
                    (MacroLanguages.InsertString)) + head
            return Some query 
    }

let retirementsToTable() =
    async {
        let! iso639Retirements = ISO639RetirementsProvider.AsyncLoad(ISO639Retirements)
        let retirements = 
            seq { for row in iso639Retirements.Rows ->
                    toRetirement row }
        let numberOfRows = Seq.length retirements
        match numberOfRows with
        | 0 ->
            return None
        | 1 ->
            let query = 
                retirements
                |> Seq.head
                |> Option.get
                |> (fun q -> Retirements.InsertString + q.ToString().Trim())
            return Some (query[0..query.Length - 2] + ";")
        | _ ->
            let head = 
                let text = 
                    (retirements
                    |> Seq.head
                    |> Option.get).ToString().Trim()
                text[0..text.Length - 2] + ";"
            let query =
                (retirements
                |> Seq.tail
                |> Seq.fold 
                    (fun acc retirement ->   
                        match retirement with
                        | Some retirement ->
                            let value = retirement.ToString()
                            acc + value
                        | None ->
                            acc) 
                    (Retirements.InsertString)) + head
            return Some query
    }

[<EntryPoint>]
let main args =
    let conn =
        new SQLiteConnection
            (ConnectionString = "Data Source=./ISO639.db;Version=3;")
    conn.Open()

    execNonQuery 
        conn 
        [ deleteFromTables() ]
    |> Async.RunSynchronously
    |> printfn "%A"

    execNonQuery 
        conn 
        [ languagesToTable()
        ; namesToTable()
        ; macroLanguagesToTable()
        ; retirementsToTable() ]
    |> Async.RunSynchronously
    |> printfn "%A"

    conn.Close()

    0