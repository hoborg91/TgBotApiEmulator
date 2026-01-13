using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TgBotApiEmulator.Contracts;
using TgBotApiEmulator.Storage.Contracts.Entities;

namespace TgBotApiEmulator.Storage;

internal sealed class TgBotSessionContext(DbContextOptions<TgBotSessionContext> options)
    : DbContext(options)
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new();

    public DbSet<Settings>? Settings { get; set; }

    public DbSet<TgSession>? TgSessions { get; set; }

    public DbSet<TgSessionRecord>? TgSessionRawRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TgSessionRecord>();//.ToTable("TgSessionRawRecord");
        modelBuilder.Entity<TgSessionBotActionRecord>();
        modelBuilder.Entity<TgSessionServerResponseRecord>();
        modelBuilder.Entity<TgSessionUpdateRecord>();

        modelBuilder
            .Entity<TgSessionRecord>()
            .HasDiscriminator<int>("Type")
            .HasValue<TgSessionBotActionRecord>(1)
            .HasValue<TgSessionServerResponseRecord>(2)
            .HasValue<TgSessionUpdateRecord>(3);

        modelBuilder
            .Entity<TgSessionBotActionRecord>()
            .Property(e => e.Headers)
            .HasConversion(
                v => JsonSerializer.Serialize(v, _jsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, _jsonSerializerOptions));

        modelBuilder.Entity<TgSessionRecord>().Property(e => e.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<TgSessionUpdateRecord>().Property(e => e.Offset);//.ValueGeneratedOnAdd();

        modelBuilder.Entity<TgSession>().Property(e => e.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<TgSession>().HasIndex(e => e.BotTokenHash).IsUnique();

        modelBuilder.Entity<Settings>().Property(e => e.Id).ValueGeneratedOnAdd();

        modelBuilder
            .HasDbFunction(() => StronglyTypedIdValue.InnerValue(default))
            .HasTranslation(args => new SqlFunctionExpression(
                functionName: "",
                arguments: args,
                nullable: false,
                argumentsPropagateNullability: [false],
                type: typeof(long),
                typeMapping: null
            ))
            .HasParameter("value")
            .HasStoreType("integer")
            ;
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<UpdateOffset>().HaveConversion<UpdateOffsetConverter>();
        configurationBuilder.Properties<TgSessionId>().HaveConversion<TgSessionIdConverter>();
        configurationBuilder.Properties<SettingsId>().HaveConversion<SettingsIdConverter>();
        configurationBuilder.Properties<TgSessionRecordId>().HaveConversion<TgSessionRecordIdConverter>();
    }

    private class UpdateOffsetConverter : ValueConverter<UpdateOffset, int>
    {
        public UpdateOffsetConverter()
            : base(v => v.Offset, v => new(v))
        {
        }
    }

    private class TgSessionIdConverter : ValueConverter<TgSessionId, long>
    {
        public TgSessionIdConverter()
            : base(v => v.Value, v => new(v))
        {
        }
    }

    private class SettingsIdConverter : ValueConverter<SettingsId, long>
    {
        public SettingsIdConverter()
            : base(v => v.Value, v => new(v))
        {
        }
    }

    private class TgSessionRecordIdConverter : ValueConverter<TgSessionRecordId, long>
    {
        public TgSessionRecordIdConverter()
            : base(v => v.Value, v => new(v))
        {
        }
    }
}
