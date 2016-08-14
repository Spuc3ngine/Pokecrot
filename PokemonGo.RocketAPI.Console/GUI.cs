using PokemonGo.RocketAPI.Logic.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

using PokemonGo.RocketAPI.Logic.Translation;
using POGOProtos.Enums;
using System.Threading;

namespace PokemonGo.RocketAPI.Console
{
    public partial class GUI : Form
    {
        
        public static NumberStyles cords = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        public static int[] evolveBlacklist = {
            3, 6, 9, 12, 15, 18, 20, 22, 24, 26, 28, 31, 34, 36, 38, 40, 42, 45, 47, 49, 51, 53, 55, 57, 59, 62, 65, 68, 71, 73, 76, 78, 80, 82, 83, 85, 87, 89, 91, 94, 95, 97, 99, 101, 103, 105, 106, 107, 108, 110, 112, 113, 114, 115, 117, 119, 121, 122, 123, 124, 125, 126, 127, 128, 130, 131, 132, 134, 135, 136, 137, 139, 141, 142, 143, 144, 145, 146, 149, 150, 151
        };
        public static Dictionary<string, string> gerEng = new Dictionary<string, string>();

        public GUI()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Globals.acc = comboBox1.SelectedIndex == 0 ? Enums.AuthType.Google : Enums.AuthType.Ptc;
            if (comboBox1.SelectedIndex == 0)
            {
                label2.Text = "E-Mail:";
                //    textBox1.Hide();
                //    label2.Hide();
                //    textBox2.Hide();
                //    label3.Hide();
            }
            else
            {
                label2.Text = TranslationHandler.GetString("username", "Username :");
                /*if (languagestr == null)
                {
                    label2.Text = "Username:";
                }
                else if (languagestr == "de")
                {
                    label2.Text = "Benutzername:";
                }
                else if (languagestr == "spain")
                {
                    label2.Text = "Usuario:";
                }
                else if (languagestr == "ptBR")
                {
                    label2.Text = "Usuário:";
                }
                else if (languagestr == "tr")
                {
                    label2.Text = "KullaniciAdi:";
                }*/
                //    textBox1.Show();
                //    label2.Show();
                //    textBox2.Show();
                //    label3.Show();
            }
        }

        public static ISettings _clientSettings;

        static string devicePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Device");
        static string deviceinfo = Path.Combine(devicePath, "DeviceInfo.txt");

        private void GUI_Load(object sender, EventArgs e)
        {
            MessageBox.Show("Pokemon GO permanent ban storm is coming");
            MessageBox.Show("USE POKECROT AT YOUR OWN RISK");
            _clientSettings = new Settings();
            // Create missing Files
            Directory.CreateDirectory(Program.path);
            Directory.CreateDirectory(Program.path_translation);
            Directory.CreateDirectory(devicePath);

            if (!File.Exists(deviceinfo))
            {
                var f = File.Create(deviceinfo);
                f.Close();
                File.WriteAllLines(deviceinfo, new string[] { "galaxy6", " " });
            } else
            {
                // Try to read the device name
                string[] arrLine = File.ReadAllLines(deviceinfo);
                try
                {
                    if (arrLine[0] != null)
                    {
                        comboBox2.Text = arrLine[0];
                    }
                } catch (Exception) { 
                }
            } 

            List<string> b = new List<string>();
            b.Add("de.json");
            b.Add("france.json");
            b.Add("italian.json");
            b.Add("ptBR.json");
            b.Add("ru.json");
            b.Add("spain.json");
            b.Add("tr.json");

            foreach (var l in b)
            {
                Extract("PokemonGo.RocketAPI.Console", Program.path_translation, "Lang", l); 
            }

            TranslationHandler.Init();

            // Version Infoooo
            groupBox9.Text = "Your Version: " + Assembly.GetExecutingAssembly().GetName().Version + " | Newest: " + Program.getNewestVersion();
            if (Program.getNewestVersion() > Assembly.GetExecutingAssembly().GetName().Version)
            {
                DialogResult dialogResult = MessageBox.Show("There is an Update on Pokecrot. do you want to redownload it ?", "Newest Version: " + Program.getNewestVersion(), MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    Process.Start("https://pokecrot.com/");
                }
                else if (dialogResult == DialogResult.No)
                {
                    //nothing   
                }
            }

            comboBox1.DisplayMember = "Text";
            var types = new[] {
                new { Text = "Google"},
                new { Text = "Pokemon Trainer Club"},
            };
            comboBox1.DataSource = types;

            //textBox1.Hide();
            //label2.Hide();
            //textBox2.Hide();
            //label3.Hide();

            var pokeIDS = new Dictionary<string, int>();
            var evolveIDS = new Dictionary<string, int>();
            int i = 1;
            int ev = 1;
            foreach (PokemonId pokemon in Enum.GetValues(typeof(PokemonId)))
            {
                if (pokemon.ToString() != "Missingno")
                {
                    pokeIDS[pokemon.ToString()] = i;
                    gerEng[StringUtils.getPokemonNameGer(pokemon)] = pokemon.ToString();
                    if (checkBox8.Checked)
                    {
                        checkedListBox1.Items.Add(StringUtils.getPokemonNameGer(pokemon));
                        checkedListBox2.Items.Add(StringUtils.getPokemonNameGer(pokemon));
                        if (!(evolveBlacklist.Contains(i)))
                        {
                            checkedListBox3.Items.Add(StringUtils.getPokemonNameGer(pokemon));
                            evolveIDS[pokemon.ToString()] = ev;
                            ev++;
                        }
                    }
                    else
                    {
                        checkedListBox1.Items.Add(pokemon.ToString());
                        checkedListBox2.Items.Add(pokemon.ToString());
                        if (!(evolveBlacklist.Contains(i)))
                        {
                            checkedListBox3.Items.Add(pokemon.ToString());
                            evolveIDS[pokemon.ToString()] = ev;
                            ev++;
                        }
                    }
                    i++;
                }
            }

            if (File.Exists(Program.account))
            {
                string[] lines = File.ReadAllLines(@Program.account);
                i = 1;
                int tb = 1;
                foreach (string line in lines)
                {
                    switch (i)
                    {
                        case 1:
                            if (line == "Google")
                                comboBox1.SelectedIndex = 0;
                            else
                                comboBox1.SelectedIndex = 1;
                            break;
                        case 9:
                            checkBox1.Checked = bool.Parse(line);
                            break;
                        case 10:
                            checkBox2.Checked = bool.Parse(line);
                            break;
                        case 12:
                            checkBox3.Checked = bool.Parse(line);
                            break;
                        case 14:
                            textBox18.Text = line;
                            break;
                        case 15:
                            textBox19.Text = line;
                            break;
                        case 16:
                            textBox20.Text = line;
                            break;
                        case 17:
                            //if (line == "1")
                            //{
                            //    Globals.navigation_option = 1;
                            //    checkBox8.Checked = true;
                            //    checkBox7.Checked = false;
                            //} else
                            //{
                            //    Globals.navigation_option = 2;
                            //    checkBox7.Checked = true;
                            //    checkBox8.Checked = false;
                            //}
                            break;
                        case 18:
                            checkBox7.Checked = bool.Parse(line);
                            break;
                        case 19:
                            checkBox8.Checked = bool.Parse(line);
                            break;
                        case 20:
                            checkBox9.Checked = bool.Parse(line);
                            break;
                        case 21:
                            textBox24.Text = line;
                            break;
                        case 22:
                            checkBox10.Checked = bool.Parse(line);
                            break;
                        case 23:
                            checkBox11.Checked = bool.Parse(line);
                            break;
                        case 24:
                            checkBox12.Checked = bool.Parse(line);
                            break;
                        case 25:
                            chkAutoIncubate.Checked = bool.Parse(line);
                            chkAutoIncubate_CheckedChanged(null, EventArgs.Empty);
                            break;
                        case 26:
                            chkUseBasicIncubators.Checked = bool.Parse(line);
                            break;
                        case 27:
                            checkBox15.Checked = bool.Parse(line);
                            break;
                        case 28:
                            //langSelected = line;
                            break;
                        case 29:
                            checkBox16.Checked = bool.Parse(line);
                            break;
                        case 30:
                            textBox26.Text = line;
                            break;
                        case 31:
                            checkBox17.Checked = bool.Parse(line);
                            break;
                        default:
                            TextBox temp = (TextBox)Controls.Find("textBox" + tb, true).FirstOrDefault();
                            temp.Text = line;
                            tb++;
                            break;
                    }
                    i++;
                }
            }
            else
            {
                textBox3.Text = "40,764883";
                textBox4.Text = "-73,972967";
                textBox5.Text = "10";
                textBox6.Text = "5";
                textBox7.Text = "5000";
                textBox8.Text = "3";
                textBox9.Text = "999";
                textBox20.Text = "5000";
            }

            if (File.Exists(Program.items))
            {
                string[] lines = File.ReadAllLines(@Program.items);
                i = 10;
                foreach (string line in lines)
                {
                    if (i == 18)
                    {
                        i = 22;
                    }
                    else if (i == 23)
                    {
                        i = 21;
                    }
                    else if (i == 22)
                    {
                        i = 23;
                    }
                    TextBox temp = (TextBox)Controls.Find("textBox" + i, true).FirstOrDefault();
                    temp.Text = line;
                    i++;
                }
            }
            else
            {
                textBox10.Text = "20";
                textBox11.Text = "50";
                textBox12.Text = "100";
                textBox13.Text = "20";
                textBox14.Text = "0";
                textBox15.Text = "0";
                textBox16.Text = "50";
                textBox17.Text = "75";
                textBox22.Text = "200";
                textBox21.Text = "100";
                textBox23.Text = "20";
                textBox24.Text = "90";
            }

            if (File.Exists(Program.keep))
            {
                string[] lines = File.ReadAllLines(@Program.keep);
                foreach (string line in lines)
                {
                    if (line != string.Empty)
                        if (checkBox8.Checked)
                            checkedListBox1.SetItemChecked(pokeIDS[gerEng[line]] - 1, true);
                        else
                            checkedListBox1.SetItemChecked(pokeIDS[line] - 1, true);
                }
            }

            if (File.Exists(Program.ignore))
            {
                string[] lines = File.ReadAllLines(@Program.ignore);
                foreach (string line in lines)
                {
                    if (line != string.Empty)
                        if (checkBox8.Checked)
                            checkedListBox2.SetItemChecked(pokeIDS[gerEng[line]] - 1, true);
                        else
                            checkedListBox2.SetItemChecked(pokeIDS[line] - 1, true);
                }
            }

            if (File.Exists(Program.lastcords))
            {
                try
                {
                    var latlngFromFile = File.ReadAllText(Program.lastcords);
                    var latlng = latlngFromFile.Split(':');
                    double latitude, longitude;
                    double.TryParse(latlng[0], out latitude);
                    double.TryParse(latlng[1], out longitude);
                    Globals.latitute = latitude;
                    Globals.longitude = longitude;
                }
                catch
                {

                }
            }

            if (File.Exists(Program.evolve))
            {
                string[] lines = File.ReadAllLines(@Program.evolve);
                foreach (string line in lines)
                {
                    if (line != string.Empty)
                        if (checkBox8.Checked)
                            checkedListBox3.SetItemChecked(evolveIDS[gerEng[line]] - 1, true);
                        else
                            checkedListBox3.SetItemChecked(evolveIDS[line] - 1, true);
                }
            }
            // Load Proxy Settings
            if (_clientSettings.UseProxyHost != string.Empty) 
            prxyIP.Text = _clientSettings.UseProxyHost;

            if (_clientSettings.UseProxyPort != 0)
            prxyPort.Text = "" + _clientSettings.UseProxyPort;
    
            if (_clientSettings.UseProxyUsername != string.Empty)
            prxyUser.Text = _clientSettings.UseProxyUsername;

            if (_clientSettings.UseProxyPassword != string.Empty)
            prxyPass.Text = "" + _clientSettings.UseProxyPassword;

            if (prxyIP.Text != "HTTPS Proxy IP")
                _clientSettings.UseProxyVerified = true;
            else
                _clientSettings.UseProxyVerified = false;

            if (prxyUser.Text != "Proxy Username")
                _clientSettings.UseProxyAuthentication = true;
            else
                _clientSettings.UseProxyAuthentication = false;

            // Placeholder event add
            prxyIP.GotFocus += new EventHandler(prxy_GotFocus);
            prxyPort.GotFocus += new EventHandler(prxy_GotFocus);
            prxyUser.GotFocus += new EventHandler(prxy_GotFocus);
            prxyPass.GotFocus += new EventHandler(prxy_GotFocus); 
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.') && (e.KeyChar != '-'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '-') && ((sender as TextBox).Text.IndexOf('-') > -1))
            {
                e.Handled = true;
            }
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void textBox7_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == string.Empty)
            {
                textBox1.BackColor = Color.Red;
                return;
            }
            else
                Globals.username = textBox1.Text;
            if (textBox2.Text == string.Empty)
            {
                textBox2.BackColor = Color.Red;
                return;
            }
            else
                Globals.password = textBox2.Text;

            if (textBox3.Text == string.Empty)
            {
                textBox3.BackColor = Color.Red;
                return;
            }
            else
                Globals.latitute = double.Parse(textBox3.Text.Replace(',', '.'), cords, NumberFormatInfo.InvariantInfo);

            if (textBox4.Text == string.Empty)
            {
                textBox4.BackColor = Color.Red;
                return;
            }
            else
                Globals.longitude = double.Parse(textBox4.Text.Replace(',', '.'), cords, NumberFormatInfo.InvariantInfo);

            if (textBox5.Text == string.Empty)
            {
                textBox5.BackColor = Color.Red;
                return;
            }
            else
                Globals.altitude = double.Parse(textBox5.Text.Replace(',', '.'), cords, NumberFormatInfo.InvariantInfo);

            if (textBox6.Text == string.Empty)
            {
                textBox6.BackColor = Color.Red;
                return;
            }
            else
                Globals.speed = double.Parse(textBox6.Text.Replace(',', '.'), cords, NumberFormatInfo.InvariantInfo);

            if (textBox7.Text == string.Empty)
            {
                textBox7.BackColor = Color.Red;
                return;
            }
            else
                Globals.radius = int.Parse(textBox7.Text);

            if (textBox8.Text == string.Empty)
            {
                textBox8.BackColor = Color.Red;
                return;
            }
            else
                Globals.duplicate = int.Parse(textBox8.Text);

            if (textBox9.Text == string.Empty)
            {
                textBox9.BackColor = Color.Red;
                return;
            }
            else
                Globals.maxCp = int.Parse(textBox9.Text);

            Globals.transfer = checkBox2.Checked;
            Globals.defLoc = checkBox1.Checked;
            Globals.evolve = checkBox3.Checked;

            if (textBox10.Text == string.Empty)
            {
                textBox10.BackColor = Color.Red;
                return;
            }
            else
                Globals.pokeball = int.Parse(textBox10.Text);

            if (textBox11.Text == string.Empty)
            {
                textBox11.BackColor = Color.Red;
                return;
            }
            else
                Globals.greatball = int.Parse(textBox11.Text);

            if (textBox12.Text == string.Empty)
            {
                textBox12.BackColor = Color.Red;
                return;
            }
            else
                Globals.ultraball = int.Parse(textBox12.Text);

            if (textBox13.Text == string.Empty)
            {
                textBox13.BackColor = Color.Red;
                return;
            }
            else
                Globals.revive = int.Parse(textBox13.Text);

            if (textBox14.Text == string.Empty)
            {
                textBox14.BackColor = Color.Red;
                return;
            }
            else
                Globals.potion = int.Parse(textBox14.Text);

            if (textBox15.Text == string.Empty)
            {
                textBox15.BackColor = Color.Red;
                return;
            }
            else
                Globals.superpotion = int.Parse(textBox15.Text);

            if (textBox16.Text == string.Empty)
            {
                textBox16.BackColor = Color.Red;
                return;
            }
            else
                Globals.hyperpotion = int.Parse(textBox16.Text);

            if (textBox17.Text == string.Empty)
            {
                textBox17.BackColor = Color.Red;
                return;
            }
            else
                Globals.berry = int.Parse(textBox17.Text);

            if (textBox18.Text != string.Empty)
                Globals.telAPI = textBox18.Text;

            if (textBox19.Text != string.Empty)
                Globals.telName = textBox19.Text;

            if (textBox20.Text == string.Empty)
            {
                textBox20.BackColor = Color.Red;
                return;
            }
            else
                Globals.telDelay = int.Parse(textBox20.Text);

            if (textBox21.Text == string.Empty)
            {
                textBox21.BackColor = Color.Red;
            }
            else
            {
                Globals.toppotion = int.Parse(textBox21.Text);
            }

            if (textBox22.Text == string.Empty)
            {
                textBox22.BackColor = Color.Red;
            }
            else
            {
                Globals.masterball = int.Parse(textBox22.Text);
            }

            if (textBox23.Text == string.Empty)
            {
                textBox23.BackColor = Color.Red;
            }
            else
            {
                Globals.toprevive = int.Parse(textBox23.Text);
            }

            if (textBox24.Text == string.Empty)
            {
                textBox24.BackColor = Color.Red;
            }
            else
            {
                Globals.ivmaxpercent = int.Parse(textBox24.Text);
            }

            if (textBox26.Text == string.Empty)
            {
                textBox26.BackColor = Color.Red;
            } else {
                int x = int.Parse(textBox26.Text);
                decimal c = ((decimal)x / 100);
                Globals.razzberry_chance = Convert.ToDouble(c);
            }

            Globals.gerNames = checkBox8.Checked;
            Globals.useincense = checkBox9.Checked;
            Globals.pokeList = checkBox10.Checked;
            Globals.keepPokemonsThatCanEvolve = checkBox11.Checked;
            //Globals.pokevision = checkBox12.Checked;
            Globals.useLuckyEggIfNotRunning = checkBox12.Checked;
            Globals.userazzberry = checkBox16.Checked;
            Globals.TransferFirstLowIV = checkBox15.Checked;
            Globals.settingsLanguage = langSelected;
            Globals.sleepatpokemons = checkBox17.Checked;

            foreach (string pokemon in checkedListBox1.CheckedItems)
            {
                if (checkBox8.Checked)
                    Globals.noTransfer.Add((PokemonId)Enum.Parse(typeof(PokemonId), gerEng[pokemon]));
                else
                    Globals.noTransfer.Add((PokemonId)Enum.Parse(typeof(PokemonId), pokemon));
            }
            foreach (string pokemon in checkedListBox2.CheckedItems)
            {
                if (checkBox8.Checked)
                    Globals.noCatch.Add((PokemonId)Enum.Parse(typeof(PokemonId), gerEng[pokemon]));
                else
                    Globals.noCatch.Add((PokemonId)Enum.Parse(typeof(PokemonId), pokemon));
            }
            foreach (string pokemon in checkedListBox3.CheckedItems)
            {
                if (checkBox8.Checked)
                    Globals.doEvolve.Add((PokemonId)Enum.Parse(typeof(PokemonId), gerEng[pokemon]));
                else
                    Globals.doEvolve.Add((PokemonId)Enum.Parse(typeof(PokemonId), pokemon));
            }
            
            string[] accFile = {
                    Globals.acc.ToString(),
                    Globals.username,
                    Globals.password,
                    Globals.latitute.ToString(),
                    Globals.longitude.ToString(),
                    Globals.altitude.ToString(),
                    Globals.speed.ToString(),
                    Globals.radius.ToString(),
                    Globals.defLoc.ToString(),
                    Globals.transfer.ToString(),
                    Globals.duplicate.ToString(),
                    Globals.evolve.ToString(),
                    Globals.maxCp.ToString(),
                    Globals.telAPI,
                    Globals.telName,
                    Globals.telDelay.ToString(),
                    Globals.navigation_option.ToString(),
                    Globals.useluckyegg.ToString(),
                    Globals.gerNames.ToString(),
                    Globals.useincense.ToString(),
                    Globals.ivmaxpercent.ToString(),
                    Globals.pokeList.ToString(),
                    Globals.keepPokemonsThatCanEvolve.ToString(),
                    Globals.useLuckyEggIfNotRunning.ToString(),
                    Globals.autoIncubate.ToString(),
                    Globals.useBasicIncubators.ToString(),
                    Globals.TransferFirstLowIV.ToString(),
                    Globals.settingsLanguage,
                    Globals.userazzberry.ToString(),
                    Convert.ToInt16(Globals.razzberry_chance * 100).ToString(),
                    Globals.sleepatpokemons.ToString()
            };
            File.WriteAllLines(@Program.account, accFile);

            string[] itemsFile = {
                    Globals.pokeball.ToString(),
                    Globals.greatball.ToString(),
                    Globals.ultraball.ToString(),
                    Globals.revive.ToString(),
                    Globals.potion.ToString(),
                    Globals.superpotion.ToString(),
                    Globals.hyperpotion.ToString(),
                    Globals.berry.ToString(),
                    Globals.masterball.ToString(),
                    Globals.toppotion.ToString(),
                    Globals.toprevive.ToString()
            };
            File.WriteAllLines(@Program.items, itemsFile);

            string[] temp = new string[200];
            int i = 0;
            foreach (PokemonId pokemon in Globals.noTransfer)
            {
                if (checkBox8.Checked)
                    temp.SetValue(StringUtils.getPokemonNameGer(pokemon), i);
                else
                    temp.SetValue(pokemon.ToString(), i);
                i++;
            }
            string[] noTransFile = temp.Where(x => !String.IsNullOrEmpty(x)).ToArray();
            File.WriteAllLines(Program.@keep, noTransFile);

            i = 0;
            Array.Clear(temp, 0, temp.Length);
            foreach (PokemonId pokemon in Globals.noCatch)
            {
                if (checkBox8.Checked)
                    temp.SetValue(StringUtils.getPokemonNameGer(pokemon), i);
                else
                    temp.SetValue(pokemon.ToString(), i);
                i++;
            }
            string[] noCatchFile = temp.Where(x => !String.IsNullOrEmpty(x)).ToArray();
            File.WriteAllLines(@Program.ignore, noCatchFile);

            Array.Clear(temp, 0, temp.Length);
            i = 0;
            foreach (PokemonId pokemon in Globals.doEvolve)
            {
                if (checkBox8.Checked)
                    temp.SetValue(StringUtils.getPokemonNameGer(pokemon), i);
                else
                    temp.SetValue(pokemon.ToString(), i);
                i++;
            }
            string[] EvolveFile = temp.Where(x => !String.IsNullOrEmpty(x)).ToArray();
            File.WriteAllLines(@Program.evolve, EvolveFile);

            ActiveForm.Dispose();
        }

        #region CheckedChanged Events

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            int i = 0;
            while (i < checkedListBox1.Items.Count)
            {
                checkedListBox1.SetItemChecked(i, checkBox4.Checked);
                i++;
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            int i = 0;
            while (i < checkedListBox2.Items.Count)
            {
                checkedListBox2.SetItemChecked(i, checkBox5.Checked);
                i++;
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            int i = 0;
            while (i < checkedListBox3.Items.Count)
            {
                checkedListBox3.SetItemChecked(i, checkBox6.Checked);
                i++;
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            Globals.useluckyegg = checkBox7.Checked;
        }

        private void checkBox16_CheckedChanged(object sender, EventArgs e)
        {
            Globals.userazzberry = checkBox16.Checked;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            while (checkedListBox1.Items.Count > 0)
            {
                checkedListBox1.Items.RemoveAt(0);
                checkedListBox2.Items.RemoveAt(0);
                if (checkedListBox3.Items.Count > 0)
                {
                    checkedListBox3.Items.RemoveAt(0);
                }
            }
            int i = 1;
            foreach (PokemonId pokemon in Enum.GetValues(typeof(PokemonId)))
            {
                if (pokemon.ToString() != "Missingno")
                {
                    if (checkBox8.Checked)
                    {
                        checkedListBox1.Items.Add(StringUtils.getPokemonNameGer(pokemon));
                        checkedListBox2.Items.Add(StringUtils.getPokemonNameGer(pokemon));
                        if (!(evolveBlacklist.Contains(i)))
                        {
                            checkedListBox3.Items.Add(StringUtils.getPokemonNameGer(pokemon));
                        }
                    }
                    else
                    {
                        checkedListBox1.Items.Add(pokemon.ToString());
                        checkedListBox2.Items.Add(pokemon.ToString());
                        if (!(evolveBlacklist.Contains(i)))
                        {
                            checkedListBox3.Items.Add(pokemon.ToString());
                        }
                    }
                    i++;
                }
            }
        }

        private void checkBox15_CheckedChanged(object sender, EventArgs e)
        {
            Globals.TransferFirstLowIV = checkBox15.Checked;
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            Globals.keepPokemonsThatCanEvolve = checkBox11.Checked;
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            Globals.useLuckyEggIfNotRunning = checkBox12.Checked;
        }

        private void chkAutoIncubate_CheckedChanged(object sender, EventArgs e)
        {
            Globals.autoIncubate = chkAutoIncubate.Checked;
            chkUseBasicIncubators.Enabled = chkAutoIncubate.Checked;
        }

        private void chkUseBasicIncubators_CheckedChanged(object sender, EventArgs e)
        {
            Globals.useBasicIncubators = chkUseBasicIncubators.Checked;
        }

        #endregion CheckedChanged Events

        private void GUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                DisplayLocationSelector();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Logger.Write(ex.Message);
            }
        }

        private void DisplayLocationSelector()
        {
            LocationSelect locationSelector = new LocationSelect(false);
            locationSelector.ShowDialog();
            textBox3.Text = Globals.latitute.ToString(CultureInfo.InvariantCulture);
            textBox4.Text = Globals.longitude.ToString(CultureInfo.InvariantCulture);
            textBox5.Text = Globals.altitude.ToString(CultureInfo.InvariantCulture);
        }

        private void TextBoxes_Items_TextChanged(object sender, EventArgs e)
        {
            int itemSumme = 0;

            if (textBox10.Text != string.Empty && textBox11.Text != string.Empty && textBox12.Text != string.Empty && textBox13.Text != string.Empty && textBox14.Text != string.Empty && textBox15.Text != string.Empty && textBox16.Text != string.Empty && textBox17.Text != string.Empty && textBox22.Text != string.Empty && textBox21.Text != string.Empty && textBox23.Text != string.Empty)
            {
                itemSumme = Convert.ToInt16(textBox10.Text) +
                            Convert.ToInt16(textBox11.Text) +
                            Convert.ToInt16(textBox12.Text) +
                            Convert.ToInt16(textBox13.Text) +
                            Convert.ToInt16(textBox14.Text) +
                            Convert.ToInt16(textBox15.Text) +
                            Convert.ToInt16(textBox16.Text) +
                            Convert.ToInt16(textBox17.Text) +
                            Convert.ToInt16(textBox22.Text) +
                            Convert.ToInt16(textBox23.Text) +
                            Convert.ToInt16(textBox21.Text);
            }

            textBox25.Text = Convert.ToString(itemSumme);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.pokecrot.com/dl/#donate");
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Process.Start("https://pokecrot.com/");
        }

        private string langSelected = "en";
        private void load_lang()
        {
            //TranslationHandler.getString("username", "Username :");

            Globals.acc = comboBox1.SelectedIndex == 0 ? Enums.AuthType.Google : Enums.AuthType.Ptc;
            if (comboBox1.SelectedIndex == 0)
            {
                label2.Text = "E-Mail:";
            }
            else
            {
                label2.Text = TranslationHandler.GetString("username", "Username :");
            }

            label1.Text = TranslationHandler.GetString("accountType", "Account Type:");
            //label2.Text = TranslationHandler.getString("username", "Username:");
            label3.Text = TranslationHandler.GetString("password", "Password:");
            groupBox2.Text = TranslationHandler.GetString("locationSettings", "Location Settings");
            label7.Text = TranslationHandler.GetString("speed", "Speed:");
            label9.Text = TranslationHandler.GetString("moveRadius", "Move Radius:");
            label10.Text = TranslationHandler.GetString("meters", "meters");
            checkBox1.Text = TranslationHandler.GetString("startFromDefaultLocation", "Start from default location");
            groupBox3.Text = TranslationHandler.GetString("botSettings", "Bot Settings");
            checkBox2.Text = TranslationHandler.GetString("autoTransferDoublePokemon", "Auto transfer double Pokemons");
            label11.Text = TranslationHandler.GetString("maxDupPokemon", "Max. duplicate Pokemons");
            label12.Text = TranslationHandler.GetString("maxCPtransfer", "Max. CP to transfer:");
            label28.Text = TranslationHandler.GetString("maxIVtransfer", "Max. IV to transfer:");
            groupBox8.Text = TranslationHandler.GetString("telegramSettings", "Telegram Settings");
            label13.Text = TranslationHandler.GetString("maxPokeballs", "Max. Pokeballs:");
            label14.Text = TranslationHandler.GetString("maxGreatballs", "Max. GreatBalls:");
            label15.Text = TranslationHandler.GetString("maxUltraballs", "Max. UltraBalls:");
            label26.Text = TranslationHandler.GetString("maxMasterballs", "Max. MasterBalls:");
            label16.Text = TranslationHandler.GetString("maxRevives", "Max. Revives:");
            label27.Text = TranslationHandler.GetString("maxTopRevives", "Max. TopRevives:");
            label17.Text = TranslationHandler.GetString("maxPotions", "Max. Potions:");
            label18.Text = TranslationHandler.GetString("maxSuperpotions", "Max. SuperPotions:");
            label19.Text = TranslationHandler.GetString("maxHyperpotions", "Max. HyperPotions:");
            label25.Text = TranslationHandler.GetString("maxToppotions", "Max. TopPotions:");
            label20.Text = TranslationHandler.GetString("maxRazzberrys", "Max. RazzBerrys:");
            label31.Text = TranslationHandler.GetString("totalCount", "Total Count:");
            groupBox5.Text = TranslationHandler.GetString("pokemonNotToTransfer", "Pokemons - Not to transfer");
            checkBox4.Text = TranslationHandler.GetString("selectAll", "Select all");
            groupBox6.Text = TranslationHandler.GetString("pokemonNotToCatch", "Pokemons - Not to catch");
            checkBox5.Text = TranslationHandler.GetString("selectAll", "Select all");
            groupBox7.Text = TranslationHandler.GetString("pokemonNotToEvolve", "Pokemons - To envolve");
            checkBox6.Text = TranslationHandler.GetString("selectAll", "Select all");
            button1.Text = TranslationHandler.GetString("saveConfig", "Save Configuration / Start Bot");
            groupBox10.Text = TranslationHandler.GetString("otherSettings", "Other Settings");
            checkBox7.Text = TranslationHandler.GetString("useLuckyeggAtEvolve", "Use LuckyEgg at Evolve");
            checkBox16.Text = TranslationHandler.GetString("useRazzBerry", "Use RazzBerry");
            checkBox8.Text = TranslationHandler.GetString("germanPokemonNames", "German Pokemon names");
            checkBox9.Text = TranslationHandler.GetString("useIncese", "Use Incense every 30min");
            checkBox3.Text = TranslationHandler.GetString("evolvePokemonIfEnoughCandy", "Evolve Pokemons if enough candy");
            checkBox10.Text = TranslationHandler.GetString("enablePokemonListGUI", "Enable Pokemon List GUI");
            checkBox11.Text = TranslationHandler.GetString("keepPokemonWhichCanBeEvolved", "Keep Pokemons which can be evolved");
            chkAutoIncubate.Text = TranslationHandler.GetString("autoIncubate", "Auto incubate");
            chkUseBasicIncubators.Text = TranslationHandler.GetString("useBasicIncubators", "Use basic incubators");
        }
         
        private void languages_btn_Click(object sender, EventArgs e)
        {
            var clicked = (Button)sender;
            Button[] languageButtons = {
                lang_en_btn,
                lang_spain_btn,
                lang_de_btn,
                lang_ptBR_btn,
                lang_tr_btn,
                lang_ru_btn,
                lang_france_btn

                // add the new languages' buttons here
            };

            if (clicked != null)
            {
                // I have used the tag field of the button to save the language key
                langSelected = (string)clicked.Tag; 
                if (!string.IsNullOrWhiteSpace(langSelected))
                {
                    if (langSelected == "en")
                    {
                        TranslationHandler.SelectLangauge(null);
                    }
                    else
                    {
                        TranslationHandler.SelectLangauge(langSelected);
                    }
                    load_lang();
                }
                else
                {
                    throw new MissingFieldException("Every language buttons needs to have as Tag field the language key");
                }
            }
        }

        public static void Extract(string nameSpace, string outDir, string internalFilePath, string resourceName)
        {
            Assembly ass = Assembly.GetCallingAssembly();

            //Logger.ColoredConsoleWrite(ConsoleColor.Red, ass.GetName().ToString());

            using (var s = ass.GetManifestResourceStream(nameSpace + "." + (internalFilePath == string.Empty ? string.Empty : internalFilePath + ".") + resourceName))
            using (BinaryReader r = new BinaryReader(s))
            using (FileStream fs = new FileStream(outDir + "\\" + resourceName, FileMode.OpenOrCreate))
            using (BinaryWriter w = new BinaryWriter(fs))
                w.Write(r.ReadBytes((int)s.Length));
        }
        // Code cleanup we can do later
        public class ExtendedWebClient : WebClient
        {

            private int timeout;
            public int Timeout
            {
                get
                {
                    return timeout;
                }
                set
                {
                    timeout = value;
                }
            }
            public ExtendedWebClient()
            {
                this.timeout = 2000;//In Milli seconds 
            }
            protected override WebRequest GetWebRequest(Uri address)
            {
                var objWebRequest = base.GetWebRequest(address);
                objWebRequest.Timeout = this.timeout;
                return objWebRequest;
            }
        }
        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox13.Checked)
            {
                button1.Enabled = false;
                prxyIP.Enabled = true;
                prxyPort.Enabled = true;
                UserSettings.Default.UseProxyVerified = true;
            }
            else
            {
                button1.Enabled = true;
                prxyIP.Enabled = false;
                prxyPort.Enabled = false;
                UserSettings.Default.UseProxyVerified = false;
            }

        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox14.Checked)
            {
                prxyUser.Enabled = true;
                prxyPass.Enabled = true;
            }
            else
            {
                prxyUser.Enabled = false;
                prxyPass.Enabled = false;
            }

        }
        
        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private void checkPrxy_Click(object sender, EventArgs e)
        {
            string proxyip = prxyIP.Text;
            int port = Convert.ToInt32(prxyPort.Text);
            /*//if (string.IsNullOrWhiteSpace(prxyIP.Text) || string.IsNullOrWhiteSpace(prxyPort.Text) || string.IsNullOrWhiteSpace(prxyUser.Text) || string.IsNullOrWhiteSpace(prxyPass.Text))
               // return;

            
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            HttpWebRequest proxyrequest = (HttpWebRequest)WebRequest.Create("https://www.nianticlabs.com/pokemongo/error");
            WebProxy myProxy = new WebProxy(proxyip, 80);
            
            if (checkBox14.Checked)
            {
                myProxy.Credentials = new NetworkCredential(prxyUser.Text, prxyUser.Text);
            }

            prxyStatus.Text = "Verifiying proxy status....";
            myProxy.BypassProxyOnLocal = false;
            proxyrequest.Proxy = myProxy;
            proxyrequest.Method = "GET";

            try
            {
                HttpWebResponse response = (HttpWebResponse)proxyrequest.GetResponse();

                MessageBox.Show("Status du opfa: " + response.StatusCode.ToString());

                /*if (response.StatusCode.ToString() == "OK")
                {
                    prxyStatus.Text = "Proxy working :)";
                    _clientSettings.UseProxyVerified = true;
                    _clientSettings.UseProxyHost = prxyIP.Text;
                    _clientSettings.UseProxyPort = port;
                    if (checkBox14.Checked)
                    {
                        _clientSettings.UseProxyUsername = prxyUser.Text;
                        _clientSettings.UseProxyPassword = prxyPass.Text;
                    }
                    button1.Enabled = true;
                    return;
                }



            }
            catch (WebException ex)
            {
                prxyStatus.Text = "Failed to resolve proxy....";
                MessageBox.Show("WebException: " + ex.Message);
                return;
            }
            catch (Exception ex)
            {
                prxyStatus.Text = "Proxy not working...";
                //MessageBox.Show("Please show this to Devs: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }*/

            _clientSettings.UseProxyHost = prxyIP.Text;
            _clientSettings.UseProxyPort = port;
            
            if(checkBox14.Checked)
            {
                _clientSettings.UseProxyUsername = prxyUser.Text;
                _clientSettings.UseProxyPassword = prxyPass.Text;
                _clientSettings.UseProxyAuthentication = true;
            }
            _clientSettings.UseProxyVerified = true;
            button1.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _clientSettings.UseProxyHost = string.Empty;
            _clientSettings.UseProxyPort = 0;
            _clientSettings.UseProxyUsername = string.Empty;
            _clientSettings.UseProxyVerified = false;
            _clientSettings.UseProxyAuthentication = false;
        }

        private void prxy_GotFocus(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text == "HTTPS Proxy IP")
            {
                tb.Text = "";
            } else if (tb.Text == "HTTPS Proxy Port")
            {
                tb.Text = "";
            } else if (tb.Text == "Proxy Username")
            {
                tb.Text = "";
            } else if (tb.Text == "Proxy Password")
            {
                tb.Text = "";
            }
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://proxylist.hidemyass.com/search-1297445#listable");
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try {
                string[] arrLine = File.ReadAllLines(deviceinfo);
                arrLine[0] = comboBox2.SelectedItem.ToString();
                File.WriteAllLines(deviceinfo, arrLine);
            } catch (IndexOutOfRangeException)
            {
                File.WriteAllLines(deviceinfo, new string[] { comboBox2.SelectedItem.ToString(), " " });
            }
        }

        private void checkBox17_CheckedChanged(object sender, EventArgs e)
        {
            Globals.sleepatpokemons = checkBox17.Checked;
        }
    }
}
