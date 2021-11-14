CREATE TABLE ISO_639_3
(
    Id char(3) NOT NULL,
    -- The three-letter 639-3 identifier
    Part2B char(3) NULL,
    -- Equivalent 639-2 identifier of the bibliogra
    -- code set, if there is one
    Part2T char(3) NULL,
    -- Equivalent 639-2 identifier of the terminolo
    -- set, if there is one
    Part1 char(2) NULL,
    -- Equivalent 639-1 identifier, if there is one
    Scope char(1) NOT NULL,
    -- I(ndividual), M(acrolanguage), S(pecial)
    Type char(1) NOT NULL,
    -- A(ncient), C(onstructed),  
    -- E(xtinct), H(istorical), L(iving), S(pecial)
    Ref_Name varchar(150) NOT NULL,
    -- Reference language name 
    Comment varchar(150) NULL
);

CREATE TABLE ISO_639_3_Names
(
    Id char(3) NOT NULL,
    -- The three-letter 639-3 identifier
    Print_Name varchar(75) NOT NULL,
    -- One of the names associated with this identifier 
    Inverted_Name varchar(75) NOT NULL
    -- The inverted form of this Print_Name form   
);

CREATE TABLE ISO_639_3_Macrolanguages
(
    M_Id char(3) NOT NULL,
    -- The identifier for a macrolanguage
    I_Id char(3) NOT NULL,
    -- The identifier for an individual language
    I_Status char(1) NOT NULL
    -- that is a member of the macrolanguage
    -- A (active) or R (retired) indicating the
    -- status of the individual code element
);

CREATE TABLE ISO_639_3_Retirements
(
    Id char(3) NOT NULL,
    -- The three-letter 639-3 identifier
    Ref_Name varchar(150) NOT NULL,
    -- reference name of language
    Ret_Reason char(1) NOT NULL,
    -- code for retirement: C (change), D (duplicate),
    -- N (non-existent), S (split), M (merge)
    Change_To char(3) NULL,
    -- in the cases of C, D, and M, the identifier 
    -- to which all instances of this Id should be changed
    Ret_Remedy varchar(300) NULL,
    -- The instructions for updating an instance
    -- of the retired (split) identifier
    Effective date NOT NULL
    -- The date the retirement became effective
);