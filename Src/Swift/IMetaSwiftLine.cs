using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biz
{
    public enum SwiftLineType
    {
        Default, Multi, CodeHidden, LableHidden
    }

    public enum SwiftLineIn
    {
        Both, DraftOnly, MessageOnly,
    }

    public enum SwiftLineCondition
    {
        None, IfThen, HideWhen, Case
    }

    public enum ConditionOP
    {
        None, StringEq, StringNotEq, StringEmpty, NumberGreater, NumberLess, NumberEq, NumberGreaterOrEq, NumberLessOrEq, StringNotEmpty
    }

    public interface IMetaSwiftLine
    {
        [Key]
        int ID { get; set; }
        int MetaID { get; set; }
        string Code { get; set; }
        string Name { get; set; }
        [Column("Value")]
        string MetaValue { get; set; }
        int Ordinal { get; set; }
        string UsedIn { get; set; }
        SwiftLineType Type { get; set; }
        SwiftLineIn LineIn { get; set; }
        string MultiCodes { get; set; }

        SwiftLineCondition Condition { get; set; }
        string IfCheck { get; set; }
        ConditionOP IfOperator { get; set; }
        string IfValue { get; set; }
        string IfThen { get; set; }
        string IfElse { get; set; }

        string HideCheck { get; set; }
        ConditionOP HideOperator { get; set; }
        string HideValue { get; set; }

        string CaseCheck { get; set; }
        string CaseFrom { get; set; }
        string CaseTo { get; set; }
        string CaseOperators { get; set; }

        int FixLength { get; set; }
        int RowLength { get; set; }
        int MaxRows { get; set; }

        bool CanOverride { get; set; }
        bool CanOverrideInRelay { get; set; }
        string Hint { get; set; }
    }
}
