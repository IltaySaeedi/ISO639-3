namespace ISO639

open FSharp.Data

[<AutoOpen>]
module internal Helpers =

    let valOrdefault (s: string option) =
        s
        |> Option.map (fun (s: string) -> s.Trim())
        |> fun s ->
            match s with
            | None | Some "" ->
                "Null"
            | Some s ->
                $"\"{s}\""

    let stringOrdefault (s: string) =
        if s.Trim() = "" then None 
        else Some s

    let stringOrNull (s: string) =
        if s.Trim() = "" then "Null"
        else $"\"{s}\""


type LanguageType =
    | Living
    | Extinct
    | Ancient
    | Historic
    | Constructed
    | Special
    member this.ToChar() =
        match this with
        | Living ->
            'L'
        | Extinct ->
            'E'
        | Ancient ->
            'A'
        | Historic ->
            'H'
        | Constructed ->
            'C'
        | Special ->
            'S'

type LanguageScope =
    | IndividualLanguages
    | MacroLanguages
    | CollectionsOfLanguages
    | Dialects
    | ReservedFoLocalUse
    | SpecialCodeElements
    member this.ToChar() =
        match this with
        | IndividualLanguages ->
            'I'
        | MacroLanguages ->
            'M'
        | CollectionsOfLanguages ->
            'C'
        | Dialects ->
            'D'
        | ReservedFoLocalUse ->
            'R'
        | SpecialCodeElements ->
            'S'

type Language =
    { Id: string
      ReferenceName: string
      LanguageType: LanguageType
      Scope: LanguageScope
      Part1: string option
      Part2B: string option
      Part2T: string option
      Comment: string option }

    override this.ToString() =
        $"(\"{this.Id}\", {valOrdefault this.Part2B}\
        , {valOrdefault this.Part2T}, {valOrdefault this.Part1}\
        , \"{this.Scope.ToChar()}\", \"{this.LanguageType.ToChar()}\"\
        , \"{this.ReferenceName}\", {valOrdefault this.Comment}),\n"

    static member InsertString =
        "INSERT INTO ISO_639_3 (Id, Part2B, Part2T, Part1, Scope, Type, Ref_Name, Comment)\n\
        VALUES "

type Names =
    { Id: string
    ; PrintName: string
    ; InvertedName: string }

    override this.ToString() = 
        $"(\"{this.Id}\", \"{this.PrintName}\", \"{this.InvertedName}\"),"

    static member InsertString =
        "INSERT INTO ISO_639_3_Names (Id, Print_Name, Inverted_Name)\n\
        VALUES "

type IndividualLanguageStatus =
    | Active
    | Retired
    member this.ToChar() =
        match this with
        | Active ->
            'A'
        | Retired ->
            'R'
type MacroLanguages =
    { MacroLanguageId: string
    ; IndividualLanguageId: string
    ; IndividualLanguageStatus : IndividualLanguageStatus }

    override this.ToString() = 
        $"(\"{this.MacroLanguageId}\", \"{this.IndividualLanguageId}\"\
        , \"{this.IndividualLanguageStatus.ToChar()}\"),"

    static member InsertString =
        "INSERT INTO ISO_639_3_Macrolanguages (M_Id, I_Id, I_Status)\n\
        VALUES "

type RetirementsReason =
    | Change of string
    | Duplicate of string
    | NonExistent
    | Split of string
    | Merge of string
    member this.ToChar() = 
        match this with
        | Change _->
            'C'
        | Duplicate _ ->
            'D'
        | NonExistent ->
            'N'
        | Split _ ->
            'S'
        | Merge _ ->
            'M'

type Retirements =
    { Id : string
    ; ReferenceName : string
    ; RetirementsReason : RetirementsReason
    ; Effective: System.DateTime }

    member private this.ChangeToAndReasonString() =
        match this.RetirementsReason with
        | Change s ->
            $"{stringOrNull s}, Null"
        | Duplicate s ->
            $"{stringOrNull s}, Null"
        | Merge s ->
            $"{stringOrNull s}, Null"
        | NonExistent ->
            "Null, Null"
        | Split s ->
            $"Null, {stringOrNull s}"
    
    member private this.EffectiveString() =
        sprintf "%s" (this.Effective.ToString("yyyy-MM-dd"))

    override this.ToString() =
        $"(\"{this.Id}\", \"{this.ReferenceName}\"\
        , \"{this.RetirementsReason.ToChar()}\", {this.ChangeToAndReasonString()}\
        , {this.EffectiveString()}),"

    static member InsertString =
        "INSERT INTO ISO_639_3_Retirements (Id, Ref_Name, Ret_Reason, Change_To, Ret_Remedy, Effective)\n\
        VALUES "

[<AutoOpen>]
module Functions =

    [<Literal>]
    let ISO639 =
        "https://iso639-3.sil.org/sites/iso639-3/files/downloads/\
        iso-639-3.tab"

    [<Literal>]
    let ISO639NameIndex = 
        "https://iso639-3.sil.org/sites/iso639-3/files/downloads/\
        iso-639-3_Name_Index.tab"

    [<Literal>]
    let ISO639Macrolanguages = 
        "https://iso639-3.sil.org/sites/iso639-3/files/downloads/\
        iso-639-3-macrolanguages.tab"

    [<Literal>]
    let ISO639Retirements = 
        "https://iso639-3.sil.org/sites/iso639-3/files/downloads/\
        iso-639-3_Retirements.tab"

    type ISO639Provider = CsvProvider<ISO639>

    type ISO639NamesProvider = CsvProvider<ISO639NameIndex>

    type ISO639MacroProvider = CsvProvider<ISO639Macrolanguages>

    type ISO639RetirementsProvider = CsvProvider<ISO639Retirements>
    
    let languageType =
        function
        | "L" -> Some Living
        | "E" -> Some Extinct
        | "C" -> Some Constructed
        | "A" -> Some Ancient
        | "H" -> Some Historic
        | "S" -> Some Special
        | _ -> None

    let languageScope =
        function
        | "I" -> Some IndividualLanguages
        | "M" -> Some MacroLanguages
        | "C" -> Some CollectionsOfLanguages
        | "D" -> Some Dialects
        | "R" -> Some ReservedFoLocalUse
        | "S" -> Some SpecialCodeElements
        | _ -> None

    let toLanguage (languageRow: ISO639Provider.Row) =
        let row = languageRow
        let languageType = languageType row.Language_Type
        let languageScope = languageScope row.Scope

        match languageType, languageScope with
        | Some langType, Some langScope ->
            Some
                { Id = row.Id
                  ReferenceName = row.Ref_Name
                  LanguageType = langType
                  Scope = langScope
                  Part1 = stringOrdefault row.Part1
                  Part2B = stringOrdefault row.Part2B
                  Part2T = stringOrdefault row.Part2T
                  Comment = stringOrdefault row.Comment }
        | _ -> None

    let toLanguageName (nameIndexRow: ISO639NamesProvider.Row) =
        let row = nameIndexRow
        {
            Id = row.Id
            PrintName = row.Print_Name
            InvertedName = row.Inverted_Name
        }
        
    let languageStatus = function
        | "A" ->
            Some Active
        | "R" ->
            Some Retired
        | _ ->
            None

    let toMacroLanguage (macroRow: ISO639MacroProvider.Row) =
        let row = macroRow
        match languageStatus row.I_Status with
        | Some status ->
            Some {
                MacroLanguageId = row.M_Id
                IndividualLanguageId = row.I_Id
                IndividualLanguageStatus = status
            }
        | _ ->
            None

    let toRetirement (retirementRow: ISO639RetirementsProvider.Row) = 
        let row = retirementRow
        match row.Ret_Reason with
        | "C" ->
            Some {
                Id = row.Id
                ReferenceName = row.Ref_Name
                RetirementsReason = Change row.Change_To
                Effective = row.Effective
            }
        | "D" ->
            Some {
                Id = row.Id
                ReferenceName = row.Ref_Name
                RetirementsReason = Duplicate row.Change_To
                Effective = row.Effective
            }
        | "M" ->
            Some {
                Id = row.Id
                ReferenceName = row.Ref_Name
                RetirementsReason = Merge row.Change_To
                Effective = row.Effective
            }
        | "N" ->
            Some {
                Id = row.Id
                ReferenceName = row.Ref_Name
                RetirementsReason = NonExistent
                Effective = row.Effective
            }
        | "S" ->
            Some {
                Id = row.Id
                ReferenceName = row.Ref_Name
                RetirementsReason = Split row.Ret_Reason
                Effective = row.Effective
            }
        | _ ->
            None