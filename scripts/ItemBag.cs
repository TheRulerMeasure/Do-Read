using Godot;
using GDColl = Godot.Collections;
using System;

public class ItemBag : Node
{
    private Color _searchedColor = Colors.LightGray;
    private Color _unsearchedColor = Colors.White;
    private GDColl.Array<Area2D> items = new GDColl.Array<Area2D>();
    private int _itemIndex = 0;

    private void _OnFoundItem(Area2D area)
    {
        items.Add(area);
        _ItemLabel();
    }

    private void _OnAwayItem(Area2D area)
    {
        if (!items.Contains(area)) return;
        items.Remove(area);
        if (items.Count <= 0)
        {
            _ItemLabel(true);
            return;
        }
        if (_itemIndex >= items.Count)
        {
            _itemIndex = items.Count-1;
        }
        _ItemLabel();
    }

    private void _OnUseItem()
    {
        if (items.Count <= 0) return;
        items[_itemIndex].Call("UseItem");
        _ItemLabel();
    }

    private void _OnCycleItem()
    {
        if (items.Count <= 0) return;
        _itemIndex++;
        if (_itemIndex > items.Count-1) _itemIndex = 0;
        _ItemLabel();
    }

    private void _ItemLabel(bool clear=false)
    {
        var label = GetNode<Label>("../PlayerUI/Control/Label");
        if (clear)
        {
            label.Text = "";
            return;
        }
        var item = items[_itemIndex];
        var amount = items.Count > 1 ? "(" + items.Count + ")" : "";
        var name = item.Name;
        bool searched = false;
        if (item.Get("Searched") != null)
        {
            searched = (bool) item.Get("Searched");
        }
        if (searched)
        {
            label.Text = amount + name + " (Used)";
            label.Modulate = _searchedColor;
            return;
        }
        label.Text = amount + name;
        label.Modulate = _unsearchedColor;
    }
}
