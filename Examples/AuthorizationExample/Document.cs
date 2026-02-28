namespace AuthorizationExample;

/// <summary>
/// A simple document with an owner.
/// </summary>
public sealed record Document(string Id, string OwnerId, string Title, string Content, bool IsPublished = false);

/// <summary>
/// In-memory document store shared by both approaches.
/// </summary>
public sealed class DocumentStore
{
    private readonly Dictionary<string, Document> _documents = new();

    public void Add(Document doc) => _documents[doc.Id] = doc;
    public Document? Get(string id) => _documents.GetValueOrDefault(id);
    public void Update(Document doc) => _documents[doc.Id] = doc;
}
