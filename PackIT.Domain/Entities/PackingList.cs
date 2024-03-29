﻿using PackIT.Domain.Events;
using PackIT.Domain.Exceptions;
using PackIT.Domain.ValueObjects;
using PackIT.Shared.Abstractions.Domain;

namespace PackIT.Domain.Entities;

public class PackingList : AggregateRoot<PackingListId>
{
    private PackingListName _name;
    private Localization _localization;

    private readonly LinkedList<PackingItem> _items = new();

    public PackingList()
    {
        
    }
    
    internal PackingList(PackingListId id, PackingListName name, Localization localization)
    {
        Id = id;
        _name = name;
        _localization = localization;
    }

    public void AddItem(PackingItem item)
    {
        var alreadyExists = _items.Any(x => x.Name == item.Name);

        //TODO: can be reconstructed to generate some response to client with exception message or putted to custom validator.
        if (alreadyExists)
            throw new PackingItemAlreadyExistsException(_name, item.Name);

        _items.AddLast(item);
        AddEvent(new PackingItemAdded(this, item));
    }

    public void AddItems(IEnumerable<PackingItem> items)
    {
        foreach (var item in items)
        {
            AddItem(item);
        }
    }

    public void PackItem(string itemName)
    {
        var item = GetItem(itemName);

        var packedItem = item with {IsPacked = true};

        _items.Find(item).Value = packedItem;
        AddEvent(new PackingItemPacked(this, item));
    }

    public void RemoveItem(string itemName)
    {
        var item = GetItem(itemName);

        _items.Remove(item);
        AddEvent(new PackingItemRemoved(this, item));
    }

    private PackingItem GetItem(string itemName)
    {
        var item = _items.SingleOrDefault(x => x.Name == itemName);

        if (item is null)
            throw new PackingItemNotFoundException(itemName);

        return item;
    }
}