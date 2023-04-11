using LiteDB;
using Microsoft.Data.Sqlite;

namespace Xunkong.Desktop.Helpers;

internal class DatabaseProvider
{


    private static readonly string _sqlitePath;

    private static readonly string _genshinDataDbPath;

    private static readonly string _docDbPath;

    private static readonly string _sqliteConnectionString;

    private static readonly string _genshinDataDbConnectionString;

    private static readonly string _docDbConnectionString;

    private static bool _initialized;


    public static string SqlitePath => _sqlitePath;
    public static string GenshinDataDbPath => _genshinDataDbPath;

    public static int DatabaseVersion => UpdateSqls.Count;


    private static List<string> UpdateSqls = new() { TableStructure_v1, TableStructure_v2, TableStructure_v3, TableStructure_v4, TableStructure_v5, TableStructure_v6 };



    static DatabaseProvider()
    {
        Directory.CreateDirectory(Path.Combine(XunkongEnvironment.UserDataPath, "Database"));
        _genshinDataDbPath = Path.Combine(XunkongEnvironment.UserDataPath, @"Database\GenshinData.db");
        _docDbPath = Path.Combine(XunkongEnvironment.UserDataPath, @"Database\Document.db");
        _sqlitePath = Path.Combine(XunkongEnvironment.UserDataPath, @"Database\XunkongData.db");
        _sqliteConnectionString = $"Data Source={_sqlitePath};";
        _genshinDataDbConnectionString = $"Filename={_genshinDataDbPath};Connection=shared;";
        _docDbConnectionString = $"Filename={_docDbPath};Connection=shared;";

        SqlMapper.AddTypeHandler(new DapperSqlMapper.DateTimeOffsetHandler());
        SqlMapper.AddTypeHandler(new DapperSqlMapper.TravelNotesPrimogemsMonthGroupStatsListHandler());
        SqlMapper.AddTypeHandler(new DapperSqlMapper.StringListHandler());
    }




    private static void Initialize()
    {
        using var dapper = new SqliteConnection(_sqliteConnectionString);
        var version = dapper.QueryFirstOrDefault<int>("PRAGMA USER_VERSION;");
        if (version < UpdateSqls.Count)
        {
            foreach (var sql in UpdateSqls.Skip(version))
            {
                dapper.Execute(sql);
            }
        }
        _initialized = true;
    }



    public static SqliteConnection CreateConnection()
    {
        if (!_initialized)
        {
            Initialize();
        }
        var dapper = new SqliteConnection(_sqliteConnectionString);
        dapper.Open();
        return dapper;
    }




    public static LiteDatabase CreateGenshinDataDb()
    {
        return new LiteDatabase(_genshinDataDbConnectionString);
    }



    public static LiteDatabase CreateDocDb()
    {
        return new LiteDatabase(_docDbConnectionString);
    }





    private const string TableStructure_Init = """
        CREATE TABLE IF NOT EXISTS DatabaseVersion
        (
            Key   TEXT NOT NULL PRIMARY KEY,
            Value TEXT
        );
        """;


    private const string TableStructure_v1 = """
        PRAGMA JOURNAL_MODE = WAL;

        BEGIN TRANSACTION;
        
        CREATE TABLE IF NOT EXISTS Setting
        (
            Key   TEXT NOT NULL PRIMARY KEY,
            Value TEXT
        );
        
        CREATE TABLE IF NOT EXISTS DailySignInHistory
        (
            Id   INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Uid  INTEGER NOT NULL,
            Date TEXT    NOT NULL,
            Time TEXT    NOT NULL
        );
        
        CREATE TABLE IF NOT EXISTS GenshinRoleInfo
        (
            Uid        INTEGER NOT NULL PRIMARY KEY,
            GameBiz    TEXT,
            Region     INTEGER NOT NULL,
            Nickname   TEXT,
            Level      INTEGER NOT NULL,
            IsChosen   INTEGER NOT NULL,
            RegionName TEXT,
            IsOfficial INTEGER NOT NULL,
            Cookie     TEXT
        );
        
        CREATE TABLE IF NOT EXISTS HoyolabUserInfo
        (
            Uid       INTEGER NOT NULL PRIMARY KEY,
            Nickname  TEXT,
            Introduce TEXT,
            Avatar    TEXT,
            Gender    INTEGER NOT NULL,
            AvatarUrl TEXT,
            Pendant   TEXT,
            Cookie    TEXT
        );
        
        CREATE TABLE IF NOT EXISTS SpiralAbyssInfo
        (
            Id               INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Uid              INTEGER NOT NULL,
            ScheduleId       INTEGER NOT NULL,
            StartTime        TEXT    NOT NULL,
            EndTime          TEXT    NOT NULL,
            TotalBattleCount INTEGER NOT NULL,
            TotalWinCount    INTEGER NOT NULL,
            MaxFloor         TEXT,
            TotalStar        INTEGER NOT NULL,
            Value            TEXT    NOT NULL
        );
        
        CREATE INDEX IF NOT EXISTS IX_SpiralAbyssInfo_MaxFloor ON SpiralAbyssInfo (MaxFloor);
        CREATE INDEX IF NOT EXISTS IX_SpiralAbyssInfo_ScheduleId ON SpiralAbyssInfo (ScheduleId);
        CREATE INDEX IF NOT EXISTS IX_SpiralAbyssInfo_TotalBattleCount ON SpiralAbyssInfo (TotalBattleCount);
        CREATE INDEX IF NOT EXISTS IX_SpiralAbyssInfo_TotalStar ON SpiralAbyssInfo (TotalStar);
        CREATE INDEX IF NOT EXISTS IX_SpiralAbyssInfo_TotalWinCount ON SpiralAbyssInfo (TotalWinCount);
        CREATE UNIQUE INDEX IF NOT EXISTS IX_SpiralAbyssInfo_Uid_ScheduleId ON SpiralAbyssInfo (Uid, ScheduleId);
        
        CREATE TABLE IF NOT EXISTS TravelNotesAwardItem
        (
            Id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Uid        INTEGER NOT NULL,
            Year       INTEGER NOT NULL,
            Month      INTEGER NOT NULL,
            Type       INTEGER NOT NULL,
            ActionId   INTEGER NOT NULL,
            ActionName TEXT,
            Time       TEXT    NOT NULL,
            Number     INTEGER NOT NULL
        );
        
        CREATE INDEX IF NOT EXISTS IX_TravelNotesAwardItem_Uid_Year_Month ON TravelNotesAwardItem (Uid, Year, Month);
        CREATE INDEX IF NOT EXISTS IX_TravelNotesAwardItem_Type ON TravelNotesAwardItem (Type);
        CREATE INDEX IF NOT EXISTS IX_TravelNotesAwardItem_Time ON TravelNotesAwardItem (Time);
        
        CREATE TABLE IF NOT EXISTS TravelNotesMonthData
        (
            Id                    INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Uid                   INTEGER NOT NULL,
            Year                  INTEGER NOT NULL,
            Month                 INTEGER NOT NULL,
            CurrentPrimogems      INTEGER NOT NULL,
            CurrentMora           INTEGER NOT NULL,
            LastPrimogems         INTEGER NOT NULL,
            LastMora              INTEGER NOT NULL,
            CurrentPrimogemsLevel INTEGER NOT NULL,
            PrimogemsChangeRate   INTEGER NOT NULL,
            MoraChangeRate        INTEGER NOT NULL,
            PrimogemsGroupBy      TEXT    NOT NULL
        );
        
        CREATE UNIQUE INDEX IF NOT EXISTS IX_TravelNotesMonthData_Uid_Year_Month ON TravelNotesMonthData (Uid, Year, Month);
        
        CREATE TABLE IF NOT EXISTS WebToolItem
        (
            Id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Title      TEXT,
            Icon       TEXT,
            "Order"    INTEGER NOT NULL,
            Url        TEXT    NOT NULL,
            JavaScript TEXT
        );
        
        CREATE TABLE IF NOT EXISTS WishlogItem
        (
            Uid       INTEGER NOT NULL,
            Id        INTEGER NOT NULL,
            WishType  INTEGER NOT NULL,
            Time      TEXT    NOT NULL,
            Name      TEXT    NOT NULL,
            Language  TEXT    NOT NULL,
            ItemType  TEXT    NOT NULL,
            RankType  INTEGER NOT NULL,
            QueryType INTEGER NOT NULL,
            PRIMARY KEY (Uid, Id)
        );

        CREATE TABLE IF NOT EXISTS WishlogUrl
        (
            Uid      INTEGER NOT NULL PRIMARY KEY,
            Url      TEXT    NOT NULL,
            DateTime TEXT    NOT NULL
        );
        
        PRAGMA USER_VERSION = 1;
        
        COMMIT TRANSACTION;
        """;


    private const string TableStructure_v2 = """
        BEGIN TRANSACTION;
        CREATE TABLE IF NOT EXISTS AchievementData
        (
            Uid            INTEGER NOT NULL,
            Id             INTEGER NOT NULL,
            Current        INTEGER NOT NULL,
            Status         INTEGER NOT NULL,
            FinishedTime   TEXT,
            Comment        TEXT,
            LastUpdateTime TEXT,
            PRIMARY KEY (Uid, Id)
        );
        PRAGMA USER_VERSION = 2;
        COMMIT TRANSACTION;
        """;


    private const string TableStructure_v3 = """
        BEGIN TRANSACTION;
        DROP TABLE IF EXISTS DatabaseVersion;

        CREATE TABLE IF NOT EXISTS OperationHistory
        (
            Id        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Time      TEXT    NOT NULL,
            Operation TEXT    NOT NULL,
            Key       TEXT,
            Value     TEXT
        );
        CREATE INDEX IF NOT EXISTS IX_OperationHistory_Time ON OperationHistory (Time);
        CREATE INDEX IF NOT EXISTS IX_OperationHistory_Operation ON OperationHistory (Operation);
        CREATE INDEX IF NOT EXISTS IX_OperationHistory_Key ON OperationHistory (Key);

        PRAGMA USER_VERSION = 3;
        COMMIT TRANSACTION;
        """;


    private const string TableStructure_v4 = """
        BEGIN TRANSACTION;

        CREATE TABLE IF NOT EXISTS GameAccount
        (
            SHA256 TEXT    NOT NULL PRIMARY KEY,
            Name   TEXT    NOT NULL,
            Server INTEGER NOT NULL,
            Value  BLOB    NOT NULL,
            Time   TEXT    NOT NULL
        );
        CREATE UNIQUE INDEX IF NOT EXISTS IX_GameAccount_Name_Server ON GameAccount (Name, Server);

        CREATE INDEX IF NOT EXISTS IX_TravelNotesAwardItem_ActionName ON TravelNotesAwardItem (ActionName);

        PRAGMA USER_VERSION = 4;
        COMMIT TRANSACTION;
        """;


    private const string TableStructure_v5 = """
        BEGIN TRANSACTION;

        CREATE TABLE IF NOT EXISTS CharacterInfo
        (
            Uid   INTEGER NOT NULL,
            Id    INTEGER NOT NULL,
            Value TEXT    NOT NULL,
            Time  TEXT    NOT NULL,
            PRIMARY KEY (Uid, Id)
        );

        PRAGMA USER_VERSION = 5;
        COMMIT TRANSACTION;
        """;


    private const string TableStructure_v6 = """
        BEGIN TRANSACTION;

        CREATE TABLE IF NOT EXISTS DailyNoteInfo
        (
            Id    INTEGER PRIMARY KEY AUTOINCREMENT,
            Uid   INTEGER NOT NULL,
            Time  TEXT    NOT NULL,
            Value TEXT
        );
        CREATE INDEX IF NOT EXISTS IX_DailyNoteInfo_Uid ON DailyNoteInfo (Uid);
        CREATE INDEX IF NOT EXISTS IX_DailyNoteInfo_Time ON DailyNoteInfo (Time);

        CREATE TABLE IF NOT EXISTS WallpaperInfo
        (
            Id          INTEGER NOT NULL PRIMARY KEY,
            Enable      INTEGER NOT NULL DEFAULT 0,
            Title       TEXT,
            Author      TEXT,
            Description TEXT,
            FileName    TEXT,
            Tags        TEXT,
            Url         TEXT,
            Source      TEXT,
            Rating      REAL    NOT NULL DEFAULT -1,
            RatingCount INTEGER NOT NULL DEFAULT 0
        );
        CREATE INDEX IF NOT EXISTS IX_WallpaperInfo_Author ON WallpaperInfo (Author);
        CREATE INDEX IF NOT EXISTS IX_WallpaperInfo_Tags ON WallpaperInfo (Tags);
        CREATE INDEX IF NOT EXISTS IX_WallpaperInfo_Rating ON WallpaperInfo (Rating);

        CREATE TABLE IF NOT EXISTS WallpaperHistory
        (
            Id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Time        TEXT    NOT NULL,
            WallpaperId INTEGER NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_WallpaperHistory_Time ON WallpaperHistory (Time);
        CREATE INDEX IF NOT EXISTS IX_WallpaperHistory_WallpaperId ON WallpaperHistory (WallpaperId);

        CREATE TABLE IF NOT EXISTS WallpaperRating
        (
            WallpaperId INTEGER NOT NULL PRIMARY KEY,
            Time        TEXT    NOT NULL,
            Rating      INTEGER NOT NULL,
            Uploaded    INTEGER NOT NULL DEFAULT 0
        );
        CREATE INDEX IF NOT EXISTS IX_WallpaperRating_Time ON WallpaperRating (Time);
        CREATE INDEX IF NOT EXISTS IX_WallpaperRating_Uploaded ON WallpaperRating (Uploaded);

        INSERT OR REPLACE INTO WallpaperInfo (Id, Enable, Title, Author, Description, FileName, Tags, Url, Source)
        SELECT JSON_EXTRACT(Value, '$.Id'),
               JSON_EXTRACT(Value, '$.Enable'),
               JSON_EXTRACT(Value, '$.Title'),
               JSON_EXTRACT(Value, '$.Author'),
               JSON_EXTRACT(Value, '$.Description'),
               JSON_EXTRACT(Value, '$.FileName'),
               JSON_EXTRACT(Value, '$.Tags'),
               JSON_EXTRACT(Value, '$.Url'),
               JSON_EXTRACT(Value, '$.Source')
        FROM OperationHistory WHERE (Operation = 'OpenWallpaper' OR Operation = 'SaveWallpaper') AND Value IS NOT NULL;

        UPDATE OperationHistory SET Key=JSON_EXTRACT(Value, '$.Id'), Value=NULL
        WHERE (Operation = 'OpenWallpaper' OR Operation = 'SaveWallpaper') AND Value IS NOT NULL;

        PRAGMA USER_VERSION = 6;
        COMMIT TRANSACTION;
        """;

}


