using MPMFEVRP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MPMFEVRP.Domains.AlgorithmDomain;


namespace MPMFEVRP.Utils
{
    public class ParamUtil
    {
        public static void DrawParameters(Control container, Dictionary<ParameterID, InputOrOutputParameter> parameters, int stepY = 28, int padding = 5, int paddingTop = 5)
        {
            container.Controls.Clear();
            int startY = paddingTop, currentY = startY, startX = padding;
            foreach (var param in parameters)
            {
                var concreteParamLabel = new System.Windows.Forms.Label();
                concreteParamLabel.Size = new System.Drawing.Size(container.Width / 2 - (2 * padding), 13);
                concreteParamLabel.Location = new System.Drawing.Point(startX, currentY);
                concreteParamLabel.Name = param.Key + "_Label";
                concreteParamLabel.Text = param.Value.Description;
                container.Controls.Add(concreteParamLabel);

                switch (param.Value.UserInputObjType)
                {
                    case UserInputObjectType.CheckBox:
                        var concreteCheckBox = new CheckBox();
                        concreteCheckBox.Location = new System.Drawing.Point(3 * container.Width / 4 - 8, currentY);
                        concreteCheckBox.Name = param.Key + "_Val";
                        concreteCheckBox.Size = new System.Drawing.Size(15, 14);
                        concreteCheckBox.Checked = param.Value.GetBoolValue();
                        concreteCheckBox.Tag = param.Key;
                        concreteCheckBox.CheckedChanged += (s, e) => parameters[(ParameterID)((CheckBox)s).Tag].Value = ((CheckBox)s).Checked;
                        container.Controls.Add(concreteCheckBox);
                        break;
                    case UserInputObjectType.ComboBox:
                        var concreteComboBox = new ComboBox();
                        concreteComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
                        concreteComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                        concreteComboBox.Location = new System.Drawing.Point(container.Width / 2, currentY - 3);
                        concreteComboBox.Name = param.Key + "_Val";
                        concreteComboBox.Size = new System.Drawing.Size(container.Width / 2 - (2 * padding), 21);
                        // Put descriptions directly to the combobox.
                        // If there are descriptions, then it will be set otherwise, values will be set.
                        concreteComboBox.Items.AddRange(param.Value.ValueDescriptions.ToArray());

                        // Since there are no values in the combobox, selected item should be calculated first
                        object selected = null;
                        for (int i = 0; i < param.Value.PossibleValues.Count; i++)
                        {
                            object item = (object)(param.Value.ValueDescriptions[i]);
                            if (param.Value.GetValue<object>().Equals(param.Value.PossibleValues[i]))
                            {
                                selected = item;
                                break;
                            }
                        }
                        concreteComboBox.SelectedItem = selected;

                        concreteComboBox.Tag = param.Key;
                        concreteComboBox.SelectedValueChanged += (s, e) => parameters[(ParameterID)((ComboBox)s).Tag].Value = parameters[(ParameterID)((ComboBox)s).Tag].PossibleValues[((ComboBox)s).SelectedIndex];
                        container.Controls.Add(concreteComboBox);
                        break;
                    case UserInputObjectType.Slider:
                        var concreteSlider = new TrackBar();
                        concreteSlider.Location = new System.Drawing.Point(container.Width / 2, currentY - 7);
                        concreteSlider.Name = param.Key + "_Val";
                        concreteSlider.Tag = param.Key;
                        concreteSlider.Size = new System.Drawing.Size(container.Width / 2 - (2 * padding), 21);
                        concreteSlider.Minimum = (int)param.Value.PossibleValues[0];
                        concreteSlider.Maximum = (int)param.Value.PossibleValues[1];
                        concreteSlider.Value = (int)param.Value.GetIntValue();
                        concreteSlider.ValueChanged += (s, e) => parameters[(ParameterID)((TrackBar)s).Tag].Value = ((TrackBar)s).Value;
                        container.Controls.Add(concreteSlider);
                        break;
                    case UserInputObjectType.TextBox:
                        var concreteTextbox = new TextBox();
                        concreteTextbox.Location = new System.Drawing.Point(container.Width / 2, currentY - 3);
                        concreteTextbox.Name = param.Key + "_Val";
                        concreteTextbox.Size = new System.Drawing.Size(container.Width / 2 - (2 * padding), 20);
                        concreteTextbox.Text = param.Value.GetStringValue();
                        concreteTextbox.Tag = param.Key;
                        concreteTextbox.TextChanged += (s, e) => parameters[(ParameterID)((TextBox)s).Tag].Value = ((TextBox)s).Text;
                        container.Controls.Add(concreteTextbox);
                        break;
                }

                currentY += stepY;
            }
            if (container.Height < currentY + 20)
                container.Height = currentY + 20;
        }
    }
}
