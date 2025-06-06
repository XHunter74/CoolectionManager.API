﻿using Moq;
using xhunter74.CollectionManager.Data.Mongo;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Data.Mongo.Repositories;
using MongoDB.Driver;
using System.Collections.Concurrent;

namespace CollectionManager.API.Test.Features;

internal class FakeMongoDbContext : IMongoDbContext
{
    private readonly FakeCollectionsRepository _repo;
    public FakeMongoDbContext()
    {
        // Mock IMongoDatabase and IClientSessionHandle for base constructor
        var mongoDatabaseMock = new Mock<IMongoDatabase>();
        var sessionMock = new Mock<IClientSessionHandle>();
        _repo = new FakeCollectionsRepository(mongoDatabaseMock.Object, sessionMock.Object);
        CollectionItems = _repo;
    }

    public CollectionsRepository CollectionItems { get; }
    public Task CommitAsync() => Task.CompletedTask;
    public void Dispose() { }

    public void AddItem(DynamicItemRecord item) => _repo.AddItem(item);
    public void RemoveItem(Guid id) => _repo.RemoveItem(id);
    public void Clear() => _repo.Clear();

    private class FakeCollectionsRepository : CollectionsRepository
    {
        private readonly ConcurrentDictionary<Guid, DynamicItemRecord> _items = new();
        public FakeCollectionsRepository(IMongoDatabase db, IClientSessionHandle session)
            : base(db, session, "Fake") { }

        public override Task<DynamicItemRecord> AddAsync(DynamicItemRecord entity, CancellationToken cancellationToken)
        {
            _items[entity.Id] = entity;
            return Task.FromResult(entity);
        }

        public override Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_items.TryRemove(id, out _));
        }

        public override Task<DynamicItemRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            _items.TryGetValue(id, out var item);
            return Task.FromResult(item);
        }

        public override Task<IEnumerable<DynamicItemRecord>> GetAllCollectionItemsAsync(Guid collectionId, CancellationToken cancellationToken)
        {
            var items = _items.Values.Where(i => i.CollectionId == collectionId).ToList();
            return Task.FromResult<IEnumerable<DynamicItemRecord>>(items);
        }

        public override Task<bool> UpdateAsync(Guid id, DynamicItemRecord entity, CancellationToken cancellationToken)
        {
            _items[id] = entity;
            return Task.FromResult(true);
        }

        public void AddItem(DynamicItemRecord item) => _items[item.Id] = item;
        public void RemoveItem(Guid id) => _items.TryRemove(id, out _);
        public void Clear() => _items.Clear();
    }
}
