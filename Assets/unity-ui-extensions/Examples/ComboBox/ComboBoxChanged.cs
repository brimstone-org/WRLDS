namespace UnityEngine.UI.Extensions.Examples
{
    public class ComboBoxChanged : MonoBehaviour
    {
        public void ComboBoxChangedEvent(string text)
        {

            global::Logger.Log("ComboBox changed [" + text + "]");
        }

        public void AutoCompleteComboBoxChangedEvent(string text)
        {

            global::Logger.Log("AutoCompleteComboBox changed [" + text + "]");
        }

        public void AutoCompleteComboBoxSelectionChangedEvent(string text, bool valid)
        {

            global::Logger.Log("AutoCompleteComboBox selection changed [" + text + "] and its validity was [" + valid + "]");
        }

        public void DropDownChangedEvent(int newValue)
        {

            global::Logger.Log("DropDown changed [" + newValue + "]");
        }
    }
}