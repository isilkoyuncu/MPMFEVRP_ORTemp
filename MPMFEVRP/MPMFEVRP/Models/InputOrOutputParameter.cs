using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Domains.AlgorithmDomain;

namespace MPMFEVRP.Models
{
    public enum UserInputObjectType { CheckBox, ComboBox, Slider, TextBox } //This enum identifies the input type on the form.

    public class InputOrOutputParameter
    {

        Object value;
        public Object Value { get { return value; } set { this.value = value; } }

        List<Object> possibleValues;
        public List<Object> PossibleValues { get { return possibleValues; } }

        List<String> valueDescriptions;
        public List<String> ValueDescriptions { get { return valueDescriptions; } }

        Object defaultValue;
        public Object DefaultValue { get { return defaultValue; } }

        UserInputObjectType userInputObjType;
        public UserInputObjectType UserInputObjType { get { return userInputObjType; } }

        ParameterID id;
        public ParameterID ID { get { return id; } }

        string description;
        public string Description { get { return description; } }

        /// <summary>
        /// Constructor for textBoxTypes.
        /// They don't have possible values or value descriptions basically.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="desc"></param>
        /// <param name="defVal"></param>
        /// <param name="typ"></param>
        public InputOrOutputParameter(ParameterID id, String desc, Object defVal)
        {
            this.id = id;
            this.description = desc;
            this.value = defVal;
            this.defaultValue = defVal;
            this.userInputObjType = UserInputObjectType.TextBox;

            this.possibleValues = new List<object>();
            this.valueDescriptions = new List<string>();
        }

        /// <summary>
        /// Constructor for general parameters.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="desc"></param>
        /// <param name="possibleVal"></param>
        /// <param name="defVal"></param>
        /// <param name="typ"></param>
        public InputOrOutputParameter(ParameterID id, String desc, List<Object> possibleVal, Object defVal, UserInputObjectType typ)
        {
            this.id = id;
            this.description = desc;
            this.value = defVal;
            this.possibleValues = possibleVal;
            this.defaultValue = defVal;
            this.userInputObjType = typ;

            this.valueDescriptions = new List<string>();

            foreach (object o in possibleVal)
            {
                if (o is Enum)
                {
                    this.valueDescriptions.Add(((Enum)o).GetDescription());
                }
                else
                {
                    this.valueDescriptions.Add(o.ToString());
                }
            }
        }

        /// <summary>
        /// Constructor for parameters with descriptions for each possible value.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="desc"></param>
        /// <param name="possibleVal"></param>
        /// <param name="valueDescs"></param>
        /// <param name="defVal"></param>
        /// <param name="typ"></param>
        public InputOrOutputParameter(ParameterID id, String desc, List<Object> possibleVal, List<String> valueDescs, Object defVal, UserInputObjectType typ) :
            this(id, desc, possibleVal, defVal, typ)
        {
            this.valueDescriptions = valueDescs;
        }

        public T GetValue<T>()
        {
            return (T)value;
        }

        public int GetIntValue()
        {
            if (userInputObjType == UserInputObjectType.TextBox)
            {
                return int.Parse(GetStringValue());
            }
            return GetValue<int>();
        }

        public double GetDoubleValue()
        {
            if (userInputObjType == UserInputObjectType.TextBox)
            {
                return double.Parse(GetStringValue());
            }
            return GetValue<double>();
        }

        public string GetStringValue()
        {
            return GetValue<string>();
        }

        public bool GetBoolValue()
        {
            return GetValue<bool>();
        }

    }
}
