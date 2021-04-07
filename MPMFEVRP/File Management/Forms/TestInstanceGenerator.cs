using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Instance_Generation.Other;
using Instance_Generation.Utility;
using Instance_Generation.Interfaces;
using Instance_Generation.FileReaders;
using Instance_Generation.FormSections;
using Instance_Generation.FileConverters;
using Instance_Generation.FileWriters;

namespace Instance_Generation.Forms
{
    public partial class TestInstanceGenerator : Form
    {
        string[] tiFilenames;
        IRawReader reader;
        FlexibleConverter fc;
        IWriter writer;

        int minSeed, maxSeed;
        DepotLocations depotLocation;
        int nCustomers;
        ServiceDurationDistributions serviceDurationDistribution;
        string customerDistribution;
        int nEVPremPayCustomers;
        double tMax;
        double travelSpeed;
        double l1kWhPerMinute, l2kWhPerMinute, l3kWhPerMinute;
        int xMax, yMax;
        int nISS, nISS_L1, nISS_L2, nISS_L3;
        int nESS, nESS_L1, nESS_L2, nESS_L3;
        List<Vehicle> list_EV;
        List<Vehicle> list_GDV;
        Vehicle selectedEV;
        Vehicle selectedGDV;
        ChargingLevels selectedDepotChargingLvl;
        BasePricingPolicy basePricingPol;
        double basePricingDollar;
        TripChargePolicy tripChargePol;
        double tripChargeDollar;
        double EVPrizeCoef;
        string EV_make, GDV_make;
        string filename;

        public TestInstanceGenerator()
        {
            InitializeComponent();

            foreach (KeyValuePair<string, IRawReader> entry in FileTypeConstants.InputFileTypes)
            {
                comboBox_FileType.Items.Add(entry.Key);
            }

        }
        private void TestInstanceGenerator_Load(object sender, EventArgs e)
        {
            list_EV = new List<Vehicle>();
            list_GDV = new List<Vehicle>();
            customerDistribution = comboBox_CustomerDistribution.Text.Substring(0, 1);
            nISS_L1 = int.Parse(textBox_nISS_L1.Text);
            nISS_L2 = int.Parse(textBox_nISS_L2.Text);
            nISS_L3 = int.Parse(textBox_nISS_L3.Text);
            Update_nISS();

            nESS_L1 = int.Parse(textBox_nESS_L1.Text);
            nESS_L2 = int.Parse(textBox_nESS_L2.Text);
            nESS_L3 = int.Parse(textBox_nESS_L3.Text);
            Update_nESS();

            l1kWhPerMinute = double.Parse(textBox_L1kwhPerMin.Text); //L1 charging speed
            l2kWhPerMinute = double.Parse(textBox_L2kwhPerMin.Text); //L2 charging speed
            l3kWhPerMinute = double.Parse(textBox_L3kwhPerMin.Text); //L3 charging speed

            foreach (DepotLocations dl in Enum.GetValues(typeof(DepotLocations)))
            {
                comboBox_DepotLocation.Items.Add(dl.ToString());
            }
            comboBox_DepotLocation.SelectedItem = comboBox_DepotLocation.Items[0];

            foreach (ServiceDurationDistributions sdd in Enum.GetValues(typeof(ServiceDurationDistributions)))
            {
                comboBox_ServiceDuration.Items.Add(sdd.ToString());
            }
            comboBox_ServiceDuration.SelectedItem = comboBox_ServiceDuration.Items[0];

            foreach (ChargingLevels chLvl in Enum.GetValues(typeof(ChargingLevels)))
            {
                comboBox_ChargingLevelAtDepot.Items.Add(chLvl.ToString());
            }
            comboBox_ChargingLevelAtDepot.SelectedIndex = 0;

            foreach (BasePricingPolicy bpp in Enum.GetValues(typeof(BasePricingPolicy)))
            {
                comboBox_BasePricingPolicy.Items.Add(bpp.ToString());
            }
            comboBox_BasePricingPolicy.SelectedIndex = 0;

            foreach (TripChargePolicy tcp in Enum.GetValues(typeof(TripChargePolicy)))
            {
                comboBox_TripChargePolicy.Items.Add(tcp.ToString());
            }
            comboBox_TripChargePolicy.SelectedIndex = 0;

            Vehicle veh;
            foreach (Vehicles v in Enum.GetValues(typeof(Vehicles)))
            {
                veh = new Vehicle(v);
                if (veh.Category == VehicleCategories.EV)
                {
                    list_EV.Add(veh);
                    comboBox_EV.Items.Add(veh.ID);
                }
                else
                {
                    list_GDV.Add(veh);
                    comboBox_GDV.Items.Add(veh.ID);
                }
            }
            comboBox_EV.SelectedItem = comboBox_EV.Items[1];
            comboBox_GDV.SelectedItem = comboBox_GDV.Items[1];
        }
        private void Button_SelectInputFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog1.InitialDirectory = System.Environment.CurrentDirectory;
            openFileDialog1.Multiselect = false;//TODO Change this to true after making sure we can process multiple files at once
            openFileDialog1.ShowDialog();
            tiFilenames = openFileDialog1.FileNames;
        }
        private void ComboBox_FileType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool fileNameSelected = (tiFilenames != null);
            switch (comboBox_FileType.Text)
            {
                case "CompletelyNew":
                    reader = null;
                    break;
                case "EMH_12":
                    if (fileNameSelected)
                        reader = new ErdoganMiller_Hooks12Reader(tiFilenames[0]);
                    else
                        throw new Exception("Filename expected before reader can be constructed");
                    break;
                case "Felipe_14":
                    if (fileNameSelected)
                        reader = new Felipe14Reader(tiFilenames[0]);
                    else
                        throw new Exception("Filename expected before reader can be constructed");
                    break;
                case "Goeke_15":
                    if (fileNameSelected)
                        reader = new GoekeSchneider15Reader(tiFilenames[0]);
                    else
                        throw new Exception("Filename expected before reader can be constructed");
                    break;
                case "Schneider_14":
                    if (fileNameSelected)
                        reader = new Schneider14Reader(tiFilenames[0]);
                    else
                        throw new Exception("Filename expected before reader can be constructed");
                    break;
                case "YavuzCapar_17":
                    if (fileNameSelected)
                        reader = new YavuzCapar17Reader(tiFilenames[0]);
                    else
                        throw new Exception("Filename expected before reader can be constructed");
                    break;
                default:
                    throw new Exception("combobox entry does not correspond to an IReader instance!");
            }
            if (reader != null)
            {
                reader.Read();
                //button_SelectInputFile.Enabled = false;
                //label_FileType.Enabled = false;
                //comboBox_FileType.Enabled = false;
                checkBox_NeedToShuffleCustomers.Checked = reader.needToShuffleCustomers();
                checkBoxDistanceMatrixReadFromFile.Checked = (reader.getDistanceMatrix() != null);
                label_Source.Text = StringOperations.SeparateFullFileName(tiFilenames[0])[1];//TODO When we allow selecting multiple files, this textbox will look awful, will have to do something
                //textBox_Seed.Enabled = false; //TODO if this stays here other than their positions all instances are going to have the same 
                FixOnFormInputFromFile(); 
                textBox_nCustomers.Text = reader.getNumCustomers().ToString();
                textBox_nESS.Text = reader.getNumESS().ToString();
                textBox_L3kwhPerMin.Text = reader.getESRechargingRate().ToString();
            }
            else
            {
                //textBox_nInstances.Enabled = true; // TODO Turn this on after making sure we can create multiple files at once
            }
        }
        void FixOnFormInputFromFile()
        {
            comboBox_DepotLocation.Text = "";
            textBox_nCustomers.Text = "";
            comboBox_CustomerDistribution.Text = "";
            comboBox_ServiceDuration.Text = "";
            textBox_XMax.Text = "";
            textBox_YMax.Text = "";
            textBox_TMax.Text = "";
            textBox_TravelSpeed.Text = "";
            groupBox_AsIsDataFromFile.Enabled = false;
        }
        private void TextBox_nISS_L1_TextChanged(object sender, EventArgs e)
        {
            nISS_L1 = int.Parse(textBox_nISS_L1.Text);
            Update_nISS();
        }
        private void TextBox_nISS_L2_TextChanged(object sender, EventArgs e)
        {
            nISS_L2 = int.Parse(textBox_nISS_L2.Text);
            Update_nISS();
        }
        private void TextBox_nISS_L3_TextChanged(object sender, EventArgs e)
        {
            nISS_L3 = int.Parse(textBox_nISS_L3.Text);
            Update_nISS();
        }
        private void TextBox_nESS_L1_TextChanged(object sender, EventArgs e)
        {
            nESS_L1 = int.Parse(textBox_nESS_L1.Text);
            Update_nESS();
        }

        private void TextBox_L3kwhPerMin_TextChanged(object sender, EventArgs e)
        {
            l3kWhPerMinute = double.Parse(textBox_L3kwhPerMin.Text); //L3 charging speed
        }
        private void TextBox_L2kwhPerMin_TextChanged(object sender, EventArgs e)
        {
            l2kWhPerMinute = double.Parse(textBox_L2kwhPerMin.Text); //L2 charging speed
        }
        private void TextBox_L1kwhPerMin_TextChanged(object sender, EventArgs e)
        {
            l1kWhPerMinute = double.Parse(textBox_L1kwhPerMin.Text); //L1 charging speed
        }

        private void TextBox_nESS_L2_TextChanged(object sender, EventArgs e)
        {
            nESS_L2 = int.Parse(textBox_nESS_L2.Text);
            Update_nESS();
        }
        private void TextBox_nESS_L3_TextChanged(object sender, EventArgs e)
        {
            nESS_L3 = int.Parse(textBox_nESS_L3.Text);
            Update_nESS();
        }
        private void ComboBox_BasePricingPolicy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_BasePricingPolicy.Items[comboBox_BasePricingPolicy.SelectedIndex].ToString() == BasePricingPolicy.Identical.ToString())
            {
                label_BasePriceDollar.Text = "$ Amount (per visit)";
                textBox_BasePriceDollar.Text = "50";
            }
            else
            {
                label_BasePriceDollar.Text = "$ Coefficient (per minute)";
                textBox_BasePriceDollar.Text = "1";
            }
        }
        private void ComboBox_TripChargePolicy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_TripChargePolicy.Items[comboBox_TripChargePolicy.SelectedIndex].ToString() == TripChargePolicy.None.ToString())
            {
                label_TripChargeDollar.Visible = false;
                textBox_TripChargeDollar.Text = "";
                textBox_TripChargeDollar.Visible = false;
            }
            else
            {
                if (comboBox_TripChargePolicy.Items[comboBox_TripChargePolicy.SelectedIndex].ToString() == TripChargePolicy.TwoTier.ToString())
                {
                    label_TripChargeDollar.Visible = true;
                    label_TripChargeDollar.Text = "Second tier increment ($)";
                    textBox_TripChargeDollar.Visible = true;
                    textBox_TripChargeDollar.Text = "10";
                }
                else//must be individualized
                {
                    label_TripChargeDollar.Visible = true;
                    label_TripChargeDollar.Text = "Per mile charge ($/mile)";
                    textBox_TripChargeDollar.Visible = true;
                    textBox_TripChargeDollar.Text = "0.50";
                }
            }
        }
        private void ComboBox_EV_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedEV = list_EV[comboBox_EV.SelectedIndex];
            textBox_EV_BatteryCapacity.Text = selectedEV.BatteryCapacity.ToString();
            textBox_EV_ConsumptionRate.Text = selectedEV.ConsumptionRate.ToString();
            textBox_EV_FixedCost.Text = selectedEV.FixedCost.ToString();
            textBox_EV_LoadCapacity.Text = selectedEV.LoadCapacity.ToString();
            textBox_EV_VariableCost.Text = selectedEV.VariableCostPerMile.ToString();
        }
        private void ComboBox_GDV_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedGDV = list_GDV[comboBox_GDV.SelectedIndex];
            textBox_GDV_BatteryCapacity.Text = selectedGDV.BatteryCapacity.ToString();
            textBox_GDV_ConsumptionRate.Text = selectedGDV.ConsumptionRate.ToString();
            textBox_GDV_FixedCost.Text = selectedGDV.FixedCost.ToString();
            textBox_GDV_LoadCapacity.Text = selectedGDV.LoadCapacity.ToString();
            textBox_GDV_VariableCost.Text = selectedGDV.VariableCostPerMile.ToString();
        }
        void Update_nISS()
        {
            nISS = nISS_L1 + nISS_L2 + nISS_L3;
            textBox_nISS.Text = nISS.ToString();
        }
        void Update_nESS()
        {
            nESS = nESS_L1 + nESS_L2 + nESS_L3;
            textBox_nESS.Text = nESS.ToString();
        }
        private void Button_UpdateFilename_Click(object sender, EventArgs e)
        {
            if (reader == null)
            {
                MessageBox.Show("Please select a file before you proceed!");
            }
            else
            {
                //Read Experiment Related Data from the form
                minSeed = int.Parse(textBox_Seed.Text);
                maxSeed = minSeed + int.Parse(textBox_nInstances.Text) - 1;

                //Take as is type data
                if (comboBox_FileType.Text == "CompletelyNew")
                {
                    //  ...from the form
                    string dl_str = comboBox_DepotLocation.Text; //depot location
                    depotLocation = DepotLocations.Center;
                    foreach (DepotLocations dl in Enum.GetValues(typeof(DepotLocations)))
                    {
                        if (dl_str == dl.ToString())
                        {
                            depotLocation = dl;
                            break;
                        }
                    }
                    nCustomers = int.Parse(textBox_nCustomers.Text); //number of customers
                    customerDistribution = comboBox_CustomerDistribution.Text.Substring(0, 1); //customer distribution
                    string sdd_str = comboBox_ServiceDuration.Text; //service duration
                    serviceDurationDistribution = ServiceDurationDistributions.f30;
                    foreach (ServiceDurationDistributions sdd in Enum.GetValues(typeof(ServiceDurationDistributions)))
                    {
                        if (sdd_str == sdd.ToString())
                        {
                            serviceDurationDistribution = sdd;
                            break;
                        }
                    }
                    xMax = int.Parse(textBox_XMax.Text); //max X-Y coordinates
                    yMax = int.Parse(textBox_YMax.Text);
                    tMax = double.Parse(textBox_TMax.Text); //workday length
                    travelSpeed = double.Parse(textBox_TravelSpeed.Text); //travel speed
                    l1kWhPerMinute = double.Parse(textBox_L1kwhPerMin.Text); //L1 charging speed
                    l2kWhPerMinute = double.Parse(textBox_L2kwhPerMin.Text); //L2 charging speed
                    l3kWhPerMinute = double.Parse(textBox_L3kwhPerMin.Text); //L3 charging speed
                }
                else
                {
                    //  ...from the input file
                    nCustomers = reader.getNumCustomers();
                }
                //Type, Gamma and Prize related data
                nEVPremPayCustomers = int.Parse(textBox_nEVPremiumPayingCustomers.Text); // # EV prem pay cust
                string dcl_str = comboBox_ChargingLevelAtDepot.Text; // charging level at depot
                foreach (ChargingLevels dcl in Enum.GetValues(typeof(ChargingLevels)))
                {
                    if (dcl_str == dcl.ToString())
                    {
                        selectedDepotChargingLvl = dcl;
                        break;
                    }
                }
                string bpp_str = comboBox_BasePricingPolicy.Text; // base pricing policy
                foreach (BasePricingPolicy bpp in Enum.GetValues(typeof(BasePricingPolicy)))
                {
                    if (bpp_str == bpp.ToString())
                    {
                        basePricingPol = bpp;
                        break;
                    }
                }
                if (textBox_BasePriceDollar.Text != "") // base price dollar
                    basePricingDollar = double.Parse(textBox_BasePriceDollar.Text);
                string tcp_str = comboBox_TripChargePolicy.Text; // trip chrge policy
                foreach (TripChargePolicy tcp in Enum.GetValues(typeof(TripChargePolicy)))
                {
                    if (tcp_str == tcp.ToString())
                    {
                        tripChargePol = tcp;
                        break;
                    }
                }
                if (textBox_TripChargeDollar.Text != "") // trip charge dollar
                    tripChargeDollar = double.Parse(textBox_TripChargeDollar.Text);
                if (textBox_EVPrizeCoef.Text != "") // EV prize coefficient
                    EVPrizeCoef = double.Parse(textBox_EVPrizeCoef.Text);

                //Now we need to verify the collected input and generate the file name
                if (VerifyUserInput())
                {
                    string filenamePrefix = textBox_FilenamePrefix.Text;
                    if (comboBox_FileType.Text == "CompletelyNew")
                    {
                        // TODO further update filename based on policies
                        filename = filenamePrefix + customerDistribution + textBox_nCustomers.Text + "_" + comboBox_ServiceDuration.Text + "_"
                            + nISS.ToString() + "(" + textBox_nEVPremiumPayingCustomers.Text + "," + nISS_L3.ToString() + "+" + nISS_L2.ToString() + "+" + nISS_L1.ToString() + ")_"
                            + textBox_nESS.Text + "(" + nESS_L3.ToString() + "+" + nESS_L2.ToString() + "+" + nESS_L1.ToString() + ")_"
                            + EV_make.Substring(0, 1) + textBox_EV_BatteryCapacity.Text + "_" + comboBox_DepotLocation.Text.Substring(0, 1);
                    }
                    else
                    {
                        string[] filenameSeparated = StringOperations.SeparateFullFileName(tiFilenames[0]);
                        filename = filenamePrefix + filenameSeparated[1] + "_"
                            + nISS.ToString() + "(" + textBox_nEVPremiumPayingCustomers.Text + "," + nISS_L3.ToString() + "+" + nISS_L2.ToString() + "+" + nISS_L1.ToString() + ")_"
                            + textBox_nESS.Text + "(" + nESS_L3.ToString() + "+" + nESS_L2.ToString() + "+" + nESS_L1.ToString() + ")_"
                            + EV_make.Substring(0, 1) + textBox_EV_BatteryCapacity.Text;
                    }
                    textBox_FullFilename.Text = filename + "_seed";
                    button_Create_n_Save.Enabled = true;
                }
            }
        }
        private void Button_Create_n_Save_Click(object sender, EventArgs e)
        {
            //Reading different sections of the form and creating their respective objects for flexible future use
            Experiment_RelatedData ExpData = new FormSections.Experiment_RelatedData(minSeed);
            CommonCoreData CCData = new FormSections.CommonCoreData(depotLocation,
            nCustomers,
            customerDistribution,
            serviceDurationDistribution,
            xMax, yMax,
            tMax,
            travelSpeed);
            TypeGammaPrize_RelatedData TGPData = new FormSections.TypeGammaPrize_RelatedData(nEVPremPayCustomers,
                nISS_L1, nISS_L2, nISS_L3,
                nESS_L1, nESS_L2, nESS_L3,
                l1kWhPerMinute,l2kWhPerMinute,l3kWhPerMinute,
                selectedDepotChargingLvl,
                basePricingPol, basePricingDollar,
                tripChargePol, tripChargeDollar,
                EVPrizeCoef);
            Vehicle_RelatedData VehData = new FormSections.Vehicle_RelatedData(selectedEV, selectedGDV);
            if (reader != null)
                if (reader.getInputFileType() == "Schneider_14")
                    VehData = new FormSections.Vehicle_RelatedData(reader.getVehicleRows()[0], reader.getVehicleRows()[1]);
                

            //The Flexible Converter is created to be our general contractor for all the remaining operations
            fc = new FlexibleConverter(ExpData, CCData, TGPData, VehData, reader);
            fc.Convert();
            writer = new KoyuncuYavuzFileWriter(filename, fc.NumberOfNodes, VehData, CCData,
                fc.NodeID, fc.NodeType, fc.X, fc.Y, fc.Demand, fc.TimeWindowStart, fc.TimeWindowEnd, fc.CustomerServiceDuration, fc.Gamma, fc.Prize, fc.TravelSpeed, fc.UseGeogPosition ,fc.Distance);
            writer.Write();
            //System.Windows.Forms.MessageBox.Show("Files were written successfully!");
        }
        bool VerifyUserInput()
        {
            if (customerDistribution != "U")
            {
                MessageBox.Show("No other case of Customer Distribution than 'Uniform' can be used at this time!"); // TODO You may want to define the clustered case for future use
            }
            if (nEVPremPayCustomers > nCustomers)
            {
                MessageBox.Show("nEVPremPayCustomers > nCustomers");
                return false;
            }
            // We decided not to impose this rule anymore!
            //if (nISS > nEVPremPayCustomers)
            //{
            //    MessageBox.Show("nISS > nEVPremPayCustomers");
            //    return false;
            //}
            if (reader != null)
                if (nESS != reader.getNumESS())
                {
                    MessageBox.Show("NESS != NESS from reader");
                }
            //Check vehicle compatibleness
            if (selectedEV != null && selectedGDV != null)
            {
                EV_make = selectedEV.ID.Substring(0, selectedEV.ID.IndexOf(" "));
                GDV_make = selectedGDV.ID.Substring(0, selectedGDV.ID.IndexOf(" ")); ;
                if (!EV_make.Equals(GDV_make))
                {
                    MessageBox.Show("Please choose comparable vehicles");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Please select a vehicle");
                return false;
            }
            //If none of the errors above are caught:
            return true;
        }
    }
}
