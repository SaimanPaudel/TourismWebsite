using System.Data.Entity.Migrations;

public partial class EnsureUniqueEmails : DbMigration
{
    public override void Up()
    {
        // 1) normalize emails: trim + lowercase
        Sql(@"
UPDATE dbo.Users
SET Email = LOWER(LTRIM(RTRIM(Email)))
WHERE Email IS NOT NULL;
");

        // 2) make duplicates unique (keep newest per email; older ones get .UserId appended)
        Sql(@"
WITH d AS (
  SELECT UserId, Email,
         ROW_NUMBER() OVER (PARTITION BY Email ORDER BY UserId DESC) AS rn
  FROM dbo.Users
  WHERE Email IS NOT NULL
)
UPDATE u
   SET Email = u.Email + '.' + CAST(u.UserId AS NVARCHAR(20))
FROM dbo.Users u
JOIN d ON d.UserId = u.UserId
WHERE d.rn > 1;
");

        // 3) add unique index on Email (case-insensitive collation will respect the normalized lowercasing above)
        CreateIndex("dbo.Users", "Email", unique: true, name: "UX_Users_Email");
    }

    public override void Down()
    {
        DropIndex("dbo.Users", "UX_Users_Email");
        // Can't safely revert the dedup renames
    }
}
