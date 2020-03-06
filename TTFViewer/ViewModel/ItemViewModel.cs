using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using TTFViewer.Model;

namespace TTFViewer.ViewModel
{
    public class CollapsedTree
    {
        public bool Collapsed;
        public List<CollapsedTree> Children;
    }


    public abstract class ItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        protected static readonly ItemViewModel Dummy = new MessageViewModel("Dummy");

        List<CollapsedTree> CollapsedTree;
        public bool IsExpanded
        {
            get { return CollapsedTree == null; }
            set
            {
                if (IsLeaf())
                    return;
                if (value != (CollapsedTree == null))
                {
                    if (value)
                    {
                        LoadFromCollapsedTree();
                    }
                    else
                    {
                        SaveToCollapsedTree();
                    }
                    RaisePropertyChanged("IsExpanded");
                }
            }
        }


        public abstract List<ItemViewModel> Children { get; protected set; }
        public abstract string Text { get; }
        public abstract bool IsValid { get; }
        public abstract string Description { get; }


        protected abstract TTFItemModel GetModel();
        protected abstract Type GetValueType();


        protected ItemViewModel()
        {
            CollapsedTree = new List<CollapsedTree>();
        }


        protected static TTFItemModel GetModel(ItemViewModel ivm)
        {
            return ivm.GetModel();
        }


        protected Type GetValueType(ItemViewModel ivm)
        {
            return ivm.GetValueType();
        }

        bool IsLeaf()
        {
            bool result = false;
            if (GetModel().Parent == null)
                return false;
            Type valueType = GetValueType();
            if (valueType == null || valueType.IsSubclassOf(typeof(ILoadable)))
                result = true;

            return result;
        }


        void SaveToCollapsedTree()
        {
            if (Children != null)
            {
                var collapsedTree = new List<CollapsedTree>();

                foreach (ItemViewModel tnv in Children)
                {
                    bool collapsed = !tnv.IsExpanded;
                    if (!collapsed)
                        tnv.SaveToCollapsedTree();

                    var item = new CollapsedTree()
                    {
                        Collapsed = collapsed,
                        Children = tnv.CollapsedTree
                    };
                    collapsedTree.Add(item);
                }

                CollapsedTree = collapsedTree;
                Children = null; // reload Children
            }
        }


        void LoadFromCollapsedTree()
        {
            var collapsedTree = CollapsedTree;
            CollapsedTree = null;
            Children = null; // reload Children

            if (collapsedTree != null && collapsedTree.Count > 0)
            {
                var children = Children;
                for (int i = 0; i < children.Count; i++)
                {
                    var ct = collapsedTree[i];
                    children[i].CollapsedTree = ct.Children;
                    if (!ct.Collapsed)
                        children[i].IsExpanded = true;
                }
            }
        }
    }


    //-----------------------------------------------


    public class TTFItemViewModel : ItemViewModel
    {
        List<ItemViewModel> _Children;
        public override List<ItemViewModel> Children
        {
            get
            {
                if (_Children == null)
                {
                    TTFItemModel model = Model;
                    if (model.Parent == null)   // Root
                    {
                        if (IsExpanded)
                            _Children = ItemViewModelHelper.CreateModelChildren(this, model, 0);
                        else
                            _Children = new List<ItemViewModel>() { Dummy };
                    }
                    else
                    {
                        if (IsExpanded)
                            _Children = ItemViewModelHelper.CreateFieldChildren(this, model.Value);
                        else
                            _Children = new List<ItemViewModel>() { Dummy };
                    }
                }
                return _Children;
            }

            protected set
            {
                if (value != _Children)
                {
                    _Children = value;
                    RaisePropertyChanged("Children");
                }
            }
        }


        public override string Text
        {
            get
            {
                Type valueType = GetValueType(this);
                if (valueType != null)
                {
                    string name = TTFOfficialName.GetValue(valueType);
                    if (!string.IsNullOrEmpty(name))
                        return string.Format("\"{0}\"", name);
                    return valueType.Name;
                }
                return null;
            }
        }

        public override bool IsValid { get => true; }

        public override string Description
        {
            get => string.Format(" (FilePosition=0x{0:X8})", Model.FilePosition);
        }


        TTFItemModel Model;


        public TTFItemViewModel(TTFItemModel model)
        {
            Model = model;
        }

        protected override TTFItemModel GetModel()
        {
            return Model;
        }

        protected override Type GetValueType()
        {
            return Model?.ValueType;
        }
    }


    public class AssociationViewModel : ItemViewModel
    {
        List<ItemViewModel> _Children;
        public override List<ItemViewModel> Children
        {
            get
            {
                if (_Children == null)
                {
                    _Children = ItemViewModelHelper.CreateFieldChildren(this, Value);
                }
                return _Children;
            }
            protected set
            {
                if (value != _Children)
                {
                    _Children = value;
                    RaisePropertyChanged("Children");
                }
            }
        }

        public override string Text
        {
            get
            {
                object value = Value;
                if (value != null)
                {
                    Type valueType = value.GetType();
                    string name = TTFOfficialName.GetValue(valueType);
                    if (!string.IsNullOrEmpty(name))
                        return string.Format("\"{0}\"", name);
                    return name;
                }
                return null;
            }
        }

        public override bool IsValid { get => true; }

        public override string Description
        {
            get
            {
                if (Value is TableRecord tr)
                    return string.Format(" (Tag='{0}')", tr.tableTag.ToString());
                return null;
            }
        }


        TTFItemModel Model;
        object Value;


        public AssociationViewModel(TTFItemModel model, object value)
        {
            Model = model;
            Value = value;
        }


        protected override TTFItemModel GetModel()
        {
            return Model;
        }


        protected override Type GetValueType()
        {
            return Value?.GetType();
        }
    }


    public class FieldViewModel : ItemViewModel
    {
        List<ItemViewModel> _Children;
        public override List<ItemViewModel> Children
        {
            get
            {
                if (_Children == null)
                {
                    if (Value is Offset32 o32)
                    {
                        if (IsExpanded)
                            _Children = ItemViewModelHelper.CreateModelChildren(this, GetModel(), o32);
                        else
                            _Children = new List<ItemViewModel>() { Dummy };
                    }
                    else if (Value is Offset16 o16)
                    {
                        if (IsExpanded)
                            _Children = ItemViewModelHelper.CreateOffset16Children(o16, GetModel());
                        else
                            _Children = new List<ItemViewModel>() { Dummy };
                    }
                    else
                        _Children = ItemViewModelHelper.CreateFieldChildren(this, Value);
                }
                return _Children;
            }
            protected set
            {
                if (value != _Children)
                {
                    _Children = value;
                    RaisePropertyChanged("Children");
                }
            }
        }

        public override string Text
        {
            get
            {
                ItemContent ic = GetItemContent();
                if (ic != null && ic.Text != null)
                {
                    string text = ic.Text(this);
                    if (!string.IsNullOrEmpty(text))
                        return string.Format("{0} {1}={2}", FieldType.Name, FieldName, ic.Text(this));
                    else
                        return string.Format("{0} {1}", FieldType.Name, FieldName);
                }
                return ItemContentHelper.DefaultText(FieldType, FieldName, Value?.GetType(), Value);
            }
        }

        public override bool IsValid
        {
            get
            {
                ItemContent ic = GetItemContent();
                return ic == null || ic.IsValid == null || ic.IsValid(this);
            }
        }

        public override string Description
        {
            get
            {
                ItemContent ic = GetItemContent();
                if (ic != null && ic.Description != null)
                {
                    string descr = ic.Description(this);
                    if (!string.IsNullOrEmpty(descr))
                        return string.Format(" ({0})", ic.Description(this));
                }
                return null;
            }
        }

        public ItemViewModel Parent { get; }
        public object Value { get; }


        Type FieldType;
        string FieldName;

        public FieldViewModel(Type fieldType, string fieldName, object fieldValue, ItemViewModel parent)
        {
            Parent = parent;
            Value = fieldValue;

            FieldType = fieldType;
            FieldName = fieldName;
        }


        public double GetRelative()
        {
            double result = double.NaN;
            if (TTFItemModelHelper.CreateFontTable(GetModel().Parent, "head") is head head)
            {
                if (Value is int16 i16)
                    result = (double)i16 / (double)head.unitsPerEm;
                else if (Value is int16 u16)
                    result = (double)u16 / (double)head.unitsPerEm;
            }
            return result;
        }


        protected override TTFItemModel GetModel()
        {
            return GetModel(Parent);
        }


        protected override Type GetValueType()
        {
            return Value?.GetType();
        }


        ItemContent GetItemContent()
        {
            Type parentValueType = GetValueType(Parent);
            ItemContentTable table = TTFContentManager.Select(parentValueType);
            if (table != null)
            {
                string baseName = ItemViewModelHelper.FieldBaseName(FieldName);
                return table.Select(baseName); 
            }
            return null;
        }
    }


    public class MessageViewModel : ItemViewModel
    {
        public override List<ItemViewModel> Children
        {
            get => null;
            protected set {}
        }

        public override string Text { get => null; }

        public override bool IsValid { get => true; }

        public override string Description { get; }


        public MessageViewModel(string message)
        {
            Description = message;
        }

        protected override TTFItemModel GetModel()
        {
            return null;
        }

        protected override Type GetValueType()
        {
            return null;
        }
    }
}
