using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{	
	const float mininumWidth = 60.0f;

    EnumFlagsAttribute flagAttribute;

    int enumLength;
	float labelWidth;

	int numBtnsPerRow;
	int numRows;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (property.propertyType != SerializedPropertyType.Enum)
            return 2f * EditorGUIUtility.singleLineHeight;

        SetDimensions(property, label);

		return numRows * EditorGUIUtility.singleLineHeight + numRows * EditorGUIUtility.standardVerticalSpacing;
	}

	void SetDimensions(SerializedProperty property, GUIContent label) {
        flagAttribute = attribute as EnumFlagsAttribute;
        
		enumLength = property.enumNames.Length;

        if (flagAttribute != null && flagAttribute.showLabel == false)
            labelWidth = GetIndentWidth();
        else 
		    labelWidth = (label == GUIContent.none ? GetIndentWidth() : EditorGUIUtility.labelWidth);

		float estimatedViewWidth = (EditorGUIUtility.currentViewWidth - (label == GUIContent.none ? 60f : EditorGUIUtility.labelWidth) - 25f);

		if (flagAttribute == null || flagAttribute.numBtnsPerRow < 0)
			numBtnsPerRow = Mathf.FloorToInt(estimatedViewWidth / mininumWidth);
		else
			numBtnsPerRow = flagAttribute.numBtnsPerRow;

		numRows = Mathf.CeilToInt((float)enumLength / (float)numBtnsPerRow);
	}

    float GetIndentWidth() {
        return EditorGUI.indentLevel * 15f;
    }

    // DRAW INSPECTOR

	public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label) {
        if (property == null)
            return;
        
        if (property.propertyType == SerializedPropertyType.Enum) { 
            DrawEnumButtons(pos, property, label);
        }
        else { 
            DrawErrorBox(pos, label);
        }
    }

    void DrawEnumButtons(Rect pos, SerializedProperty property, GUIContent label) {
        // Sets some dimensions that are used below
        SetDimensions(property, label);

        int targetValue = 0;
        bool[] buttonsPressed = new bool[enumLength];

        var enumType = GetEnumType(property);
        int[] enumValues = GetEnumValuesArray(enumType);

        if (enumValues == null)
            return;

        float buttonWidth = (pos.width - labelWidth) / Mathf.Min(numBtnsPerRow, enumLength);
        float posX, posY;
        int i;

        // In case of multi-editing with different values
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

        if (EditorGUI.showMixedValue)
            label.text = label.text + " (mixed)";

        EditorGUI.LabelField(new Rect(pos.x, pos.y, labelWidth, pos.height), label);

        // The change check ensures that no values are being overwritten during multi-selection
        EditorGUI.BeginChangeCheck();

        for (int row = 0; row < numRows; row++) {
            for (int btn = 0; btn < numBtnsPerRow; btn++) {
                i = btn + row * numBtnsPerRow;

                if (i >= enumLength)
                    break;

                // Check if we should display multiple values, show all buttons as unpressed
                if (EditorGUI.showMixedValue)
                    buttonsPressed[i] = false;
                // Check if the button is/was pressed 
                else if ((property.intValue & enumValues[i]) == enumValues[i])
                    buttonsPressed[i] = true;

                posX = pos.x + labelWidth + buttonWidth * btn;
                posY = pos.y + row * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                Rect buttonPos = new Rect(posX, posY, buttonWidth, EditorGUIUtility.singleLineHeight);

                buttonsPressed[i] = GUI.Toggle(buttonPos, buttonsPressed[i], GetButtonContent(property, i, enumValues[i]), EditorStyles.toolbarButton);

                if (buttonsPressed[i])
                    targetValue += enumValues[i];
            }
        }

        if (EditorGUI.EndChangeCheck())
            property.intValue = targetValue;
    }

    GUIContent GetButtonContent(SerializedProperty property, int i, int enumValue) {
        if (flagAttribute == null || flagAttribute.showTooltip)
            return new GUIContent(property.enumDisplayNames[i], property.enumDisplayNames[i] + " (" + enumValue + ")");
        else 
            return new GUIContent(property.enumDisplayNames[i]);
    }

    Type GetEnumType(SerializedProperty property) {
        if (fieldInfo.FieldType.IsArray)
            return fieldInfo.FieldType.GetElementType();
        else
            return fieldInfo.FieldType;
    }

    int[] GetEnumValuesArray(Type type) {
        if (!type.IsEnum)
            return null;

        var enumValues = Enum.GetValues(type);
        var enumArray = new int[enumValues.Length];

        for (int i = 0; i < enumValues.Length; i++)
            enumArray[i] = (int)enumValues.GetValue(i);

        return enumArray;
    }

    void DrawErrorBox(Rect pos, GUIContent label) {
        EditorGUI.LabelField(new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth, pos.height), label);
        EditorGUI.HelpBox(new Rect(pos.x + EditorGUIUtility.labelWidth, pos.y, pos.width - EditorGUIUtility.labelWidth, pos.height), 
            "EnumFlags attribute was applied to a field that is not of type Enum!", MessageType.Error);
    }

}