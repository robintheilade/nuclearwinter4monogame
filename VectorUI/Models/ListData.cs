using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace VectorUI.Models
{
    //--------------------------------------------------------------------------
    public enum ListColumnType
    {
        Text,
        Image
    }

    //--------------------------------------------------------------------------
    public class ListColumn
    {
        //----------------------------------------------------------------------
        public ListColumn( ListColumnType _columnType, int _size )
        {
            ColumnType  = _columnType;
            Size        = _size;
        }

        //----------------------------------------------------------------------
        public ListColumnType       ColumnType;
        public int                  Size;
    }

    //--------------------------------------------------------------------------
    public class ListEntry
    {
        public bool                 Disabled;
        public string[]             Values;
        public object               UserData;
    }

    //--------------------------------------------------------------------------
    public class ListData
    {
        //----------------------------------------------------------------------
        public ListData()
        {
            Entries = new List<ListEntry>();
        }

        //----------------------------------------------------------------------
        public void AddEntry( string[] _values, object _userData )
        {
            var entry = new ListEntry();
            entry.Values = _values;
            entry.UserData = _userData;

            Entries.Add( entry );
        }

        //----------------------------------------------------------------------
        public ListColumn[]         Columns;
        public List<ListEntry>      Entries;
    }
}
