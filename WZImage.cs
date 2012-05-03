﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace reWZ
{
    public class WZImage : WZObject
    {
        private bool _parsed;
        private bool _encrypted;
        private readonly Dictionary<String, WZObject> _backing;
        private readonly WZBinaryReader _r;

        public override WZObject this[string childName]
        {
            get
            {
                if(!_parsed) Parse();
                if (!_backing.ContainsKey(childName)) throw new KeyNotFoundException("No such child in WZImage.");
                return _backing[childName];
            }
        }

        internal WZImage(string name, WZObject parent, WZFile file, WZBinaryReader reader) : base(name, WZObjectType.Image, parent, file)
        {
            _r = reader;
            _backing = new Dictionary<string, WZObject>();
            Parse();
        }

        internal void Parse()
        {
            _r.Jump(0);
            if(_r.ReadByte() != 0x73) WZFile.Die("WZImage with invalid header (not beginning with 0x73!)");
            if (_r.PeekFor(() => _r.ReadWZString()) == "Property") _encrypted = true;
            else if (_r.PeekFor(() => _r.ReadWZString(false)) == "Property") _encrypted = false;
            else WZFile.Die("WZImage with invalid header (no Property string! check your WZVariant)");
            if(_r.ReadWZString(_encrypted) != "Property") WZFile.Die("Failed to deduce encryption of image.");
            if(_r.ReadUInt16() != 0) WZFile.Die("WZImage with invalid header (no zero UInt16!)");
            WZExtendedParser.ParsePropertyList(_r, this, File, _encrypted).ForEach(o => _backing.Add(o.Name, o));
            _parsed = true;
        }
    }
}
