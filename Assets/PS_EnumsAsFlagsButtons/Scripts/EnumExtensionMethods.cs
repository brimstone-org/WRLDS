using System;
using System.Collections.Generic;

public static class EnumExtension {

    public static bool HasFlag(this Enum @enum, Enum value) {
        if (@enum == null)
            return false;

        if (value == null)
            throw new ArgumentNullException("value");

        if (Enum.IsDefined(@enum.GetType(), value) == false) {
            throw new ArgumentException(string.Format("Enum type mismatch. The source enum is of type '{0}' compared to type '{1}'.", @enum.GetType(), value.GetType()));
        }

        ulong compareFlag = Convert.ToUInt64(value);
        return ((Convert.ToUInt64(@enum) & compareFlag) == compareFlag);
	}

	public static Enum[] GetAllFlags(this Enum @enum) {
		Type enumType = @enum.GetType();
		var enumValArray = Enum.GetValues(enumType);
		List<Enum> enumValList = new List<Enum>(enumValArray.Length);
		Enum e;

		for (int i = 0; i < enumValArray.Length; i++) {
			e = (Enum) enumValArray.GetValue(i);

			if (e != null && @enum.HasFlag(e))
				enumValList.Add(e);
		}

		return enumValList.ToArray();
	}

}