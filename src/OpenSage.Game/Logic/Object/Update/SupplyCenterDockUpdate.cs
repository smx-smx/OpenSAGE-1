﻿using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SupplyCenterDockUpdateModuleData : DockUpdateModuleData
    {
        internal static SupplyCenterDockUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SupplyCenterDockUpdateModuleData> FieldParseTable = DockUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<SupplyCenterDockUpdateModuleData>
            {
                { "GrantTemporaryStealth", (parser, x) => x.GrantTemporaryStealth = parser.ParseInteger() },
                { "BonusScience", (parser, x) => x.BonusScience = parser.ParseString() },
                { "BonusScienceMultiplier", (parser, x) => x.BonusScienceMultiplier = parser.ParsePercentage() },
            });

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int GrantTemporaryStealth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string BonusScience { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float BonusScienceMultiplier { get; private set; }
    }
}
