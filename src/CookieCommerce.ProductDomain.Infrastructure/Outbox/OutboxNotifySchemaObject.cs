using System.Data.Common;
using Weasel.Core;
using Weasel.Postgresql;

namespace CookieCommerce.ProductDomain.Infrastructure.Outbox;

/// <summary>
/// Marten schema object that installs PostgreSQL trigger for outbox NOTIFY functionality.
/// Creates a trigger that sends a notification whenever a new OutboxMessage is inserted.
/// 
/// PostgreSQL Trigger Documentation:
/// The trigger function 'notify_outbox()' is called after each INSERT on the OutboxMessage table.
/// It sends a notification on the 'outbox_channel' channel with the new message ID as payload.
/// This allows the OutboxBackgroundPublisher to react immediately to new messages.
/// </summary>
public class OutboxNotifySchemaObject : ISchemaObject
{
    public DbObjectName Identifier { get; } = new DbObjectName("public", "outbox_notify_setup");

    public void WriteCreateStatement(Migrator migrator, TextWriter writer)
    {
        // Create the notify function
        writer.WriteLine(@"
CREATE OR REPLACE FUNCTION notify_outbox()
RETURNS trigger AS $$
BEGIN
    -- Send notification with the new message ID
    PERFORM pg_notify('outbox_channel', NEW.id::text);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
");

        // Create the trigger on the OutboxMessage table
        // Marten stores documents in tables named mt_doc_{typename}
        writer.WriteLine(@"
DROP TRIGGER IF EXISTS outbox_notify ON mt_doc_outboxmessage;
CREATE TRIGGER outbox_notify
    AFTER INSERT ON mt_doc_outboxmessage
    FOR EACH ROW
    EXECUTE FUNCTION notify_outbox();
");
    }

    public void WriteDropStatement(Migrator migrator, TextWriter writer)
    {
        writer.WriteLine("DROP TRIGGER IF EXISTS outbox_notify ON mt_doc_outboxmessage;");
        writer.WriteLine("DROP FUNCTION IF EXISTS notify_outbox();");
    }

    public void ConfigureQueryCommand(Weasel.Core.DbCommandBuilder builder)
    {
        // Not needed for this schema object
    }

    public IEnumerable<DbObjectName> AllNames()
    {
        yield return Identifier;
    }

    public Task<ISchemaObjectDelta> CreateDeltaAsync(DbDataReader reader, CancellationToken ct)
    {
        // Always return a delta that will recreate the object
        return Task.FromResult<ISchemaObjectDelta>(new SchemaObjectDelta(this));
    }

    private class SchemaObjectDelta : ISchemaObjectDelta
    {
        private readonly OutboxNotifySchemaObject _parent;

        public SchemaObjectDelta(OutboxNotifySchemaObject parent)
        {
            _parent = parent;
        }

        public ISchemaObject SchemaObject => _parent;
        
        public SchemaPatchDifference Difference => SchemaPatchDifference.Update;

        public void WriteUpdate(Migrator rules, TextWriter writer)
        {
            _parent.WriteDropStatement(rules, writer);
            _parent.WriteCreateStatement(rules, writer);
        }

        public void WriteRollback(Migrator rules, TextWriter writer)
        {
            _parent.WriteDropStatement(rules, writer);
        }

        public void WriteRestorationOfPreviousState(Migrator rules, TextWriter writer)
        {
            _parent.WriteDropStatement(rules, writer);
        }
    }
}
