using Fargo.Core.Entities.Abstracts;
using Fargo.Core.Enums;

namespace Fargo.Core.Entities
{
    public class SystemSetting : Entity
    {
        public bool? BoolValue { get; private init; }
        public Guid? GuidValue { get; private init; }
        public string? StringValue { get; private init; }
        public ESystemSettingValueType ValueType { get; private init; }

        private SystemSetting() { }

        public SystemSetting(Guid guidSettingValue)
        {
            GuidValue = guidSettingValue;
            ValueType = ESystemSettingValueType.Guid;
            EntityType = EEntityType.SystemSetting;
        }

        public SystemSetting(string stringSettingValue)
        {
            StringValue = stringSettingValue;
            ValueType = ESystemSettingValueType.String;
            EntityType = EEntityType.SystemSetting;
        }

        public SystemSetting(bool boolSettingValue)
        {
            BoolValue = boolSettingValue;
            ValueType = ESystemSettingValueType.Bool;
            EntityType = EEntityType.SystemSetting;
        }
    }
}
