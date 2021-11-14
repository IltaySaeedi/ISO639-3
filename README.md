# ISO639-3 Sqlite table creator

Create db using existing .sql file

`sqlite3 ISO639.db < ISO639_SQLITE.sql`

Then run the project using

`dotnet run`

## Make it offline

This project uses [tables](https://iso639-3.sil.org/code_tables/download_tables) of ISO 639-3 Registration Authority for type providers.

You should download UTF-8 tables and change the URI of type providers to make this project offline.

## Changing the name of db file

By doing that you should also change the connection string and name of the db in the proj file.