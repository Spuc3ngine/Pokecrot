using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Helpers;
using System;
using System.Threading;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using PokemonGo.RocketAPI.Logic.Utils;
using static PokemonGo.RocketAPI.Console.GUI;

namespace PokemonGo.RocketAPI.Console
{
    public partial class Pokemons : Form
    {
        public static string languagestr2;
        private static Client client;
        private static GetPlayerResponse profile;
        private static GetInventoryResponse inventory;
        private static IOrderedEnumerable<PokemonData> pokemons;
        public class taskResponse
        {
            public bool Status { get; set; }
            public string Message { get; set; }
            public taskResponse() { }
            public taskResponse(bool status, string message)
            {
                Status = status;
                Message = message;
            }
        }
        public Pokemons()
        {
            InitializeComponent();
            ClientSettings = new Settings();
        }
        
        public static ISettings ClientSettings;

        private void Pokemons_Load(object sender, EventArgs e)
        {
            reloadsecondstextbox.Text = "60";
            Execute();
            this.PokemonListView.ColumnClick += new ColumnClickEventHandler(PokemonListView_ColumnClick);
        }

        private async void Execute()
        {
            EnabledButton(false, "Reloading Pokemon list.");

            client = new Client(ClientSettings);
            client.setFailure(new PokemonGo.RocketAPI.Logic.ApiFailureStrat(client));

            try
            {
                await client.Login.DoLogin();

                profile = await client.Player.GetPlayer();
                inventory = await client.Inventory.GetInventory();
                pokemons =
                    inventory.InventoryDelta.InventoryItems
                    .Select(i => i.InventoryItemData?.PokemonData)
                        .Where(p => p != null && p?.PokemonId > 0)
                        .OrderByDescending(key => key.Cp);
                var families = inventory.InventoryDelta.InventoryItems
                    .Select(i => i.InventoryItemData?.Candy)
                    .Where(p => p != null && (int)p?.FamilyId > 0)
                    .OrderByDescending(p => (int)p.FamilyId);

                var imageSize = 50;

                var imageList = new ImageList { ImageSize = new Size(imageSize, imageSize) };
                PokemonListView.ShowItemToolTips = true;
                PokemonListView.SmallImageList = imageList;

                var templates = await client.Download.GetItemTemplates();
                var myPokemonSettings = templates.ItemTemplates.Select(i => i.PokemonSettings).Where(p => p != null && p?.FamilyId != PokemonFamilyId.FamilyUnset);
                var pokemonSettings = myPokemonSettings.ToList();

                var myPokemonFamilies = inventory.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Candy).Where(p => p != null && p?.FamilyId != PokemonFamilyId.FamilyUnset);
                var pokemonFamilies = myPokemonFamilies.ToArray();

                PokemonListView.DoubleBuffered(true);
                PokemonListView.View = View.Details;

                ColumnHeader columnheader;
                columnheader = new ColumnHeader();
                columnheader.Text = "Name";
                PokemonListView.Columns.Add(columnheader);
                columnheader = new ColumnHeader();
                columnheader.Text = "CP";
                PokemonListView.Columns.Add(columnheader);
                columnheader = new ColumnHeader();
                columnheader.Text = "IV A-D-S";
                PokemonListView.Columns.Add(columnheader);
                columnheader = new ColumnHeader();
                columnheader.Text = "LVL";
                PokemonListView.Columns.Add(columnheader);
                columnheader = new ColumnHeader();
                columnheader.Text = "Evolvable?";
                PokemonListView.Columns.Add(columnheader);
                columnheader = new ColumnHeader();
                columnheader.Text = "Height";
                PokemonListView.Columns.Add(columnheader);
                columnheader = new ColumnHeader();
                columnheader.Text = "Weight";
                PokemonListView.Columns.Add(columnheader);
                columnheader = new ColumnHeader();
                columnheader.Text = "HP";
                PokemonListView.Columns.Add(columnheader);
                columnheader = new ColumnHeader();
                columnheader.Text = "Attack";
                PokemonListView.Columns.Add(columnheader);
                columnheader = new ColumnHeader();
                columnheader.Text = "SpecialAttack (DPS)";
                PokemonListView.Columns.Add(columnheader);

                foreach (var pokemon in pokemons)
                {
                    Bitmap pokemonImage = null;
                    await Task.Run(() =>
                    {
                        pokemonImage = GetPokemonImage((int)pokemon.PokemonId);
                    });
                    imageList.Images.Add(pokemon.PokemonId.ToString(), pokemonImage);

                    PokemonListView.LargeImageList = imageList;
                    var listViewItem = new ListViewItem();
                    listViewItem.Tag = pokemon;



                    var currentCandy = families
                        .Where(i => (int)i.FamilyId <= (int)pokemon.PokemonId)
                        .Select(f => f.Candy_)
                        .First();
                    listViewItem.SubItems.Add(string.Format("{0}", pokemon.Cp));
                    listViewItem.SubItems.Add(string.Format("{0}% {1}-{2}-{3}", PokemonInfo.CalculatePokemonPerfection(pokemon).ToString("0.00"), pokemon.IndividualAttack, pokemon.IndividualDefense, pokemon.IndividualStamina));
                    listViewItem.SubItems.Add(string.Format("{0}", PokemonInfo.GetLevel(pokemon)));
                    listViewItem.ImageKey = pokemon.PokemonId.ToString();

                    listViewItem.Text = string.Format((pokemon.Favorite == 1) ? "{0} ★" : "{0}", StringUtils.getPokemonNameByLanguage(ClientSettings, (PokemonId)pokemon.PokemonId));

                    listViewItem.ToolTipText = new DateTime((long)pokemon.CreationTimeMs * 10000).AddYears(1769).ToString("dd/MM/yyyy HH:mm:ss");
                    if (pokemon.Nickname!="")
                        listViewItem.ToolTipText += "\nNickname: " + pokemon.Nickname;

                    var settings = pokemonSettings.Single(x => x.PokemonId == pokemon.PokemonId);
                    var familyCandy = pokemonFamilies.Single(x => settings.FamilyId == x.FamilyId);

                    if (settings.EvolutionIds.Count > 0 && familyCandy.Candy_ >= settings.CandyToEvolve)
                    {
                        listViewItem.SubItems.Add("Y (" + familyCandy.Candy_ + "/" + settings.CandyToEvolve + ")");
                        listViewItem.Checked = true;
                    }
                    else
                    {
                        if (settings.EvolutionIds.Count > 0)
                            listViewItem.SubItems.Add("N (" + familyCandy.Candy_ + "/" + settings.CandyToEvolve + ")");
                        else
                            listViewItem.SubItems.Add("N (" + familyCandy.Candy_ + "/Max)");
                    }
                    listViewItem.SubItems.Add(string.Format("{0}", Math.Round(pokemon.HeightM, 2)));
                    listViewItem.SubItems.Add(string.Format("{0}", Math.Round(pokemon.WeightKg, 2)));
                    listViewItem.SubItems.Add(string.Format("{0}/{1}", pokemon.Stamina, pokemon.StaminaMax));
                    listViewItem.SubItems.Add(string.Format("{0}", pokemon.Move1));
                    listViewItem.SubItems.Add(string.Format("{0} ({1})", pokemon.Move2, PokemonInfo.GetAttack(pokemon.Move2)));

                    PokemonListView.Items.Add(listViewItem);
                }
                PokemonListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                Text = "Pokemon List | User: " + profile.PlayerData.Username + " | Pokemons: " + pokemons.Count() + "/" + profile.PlayerData.MaxPokemonStorage;
                EnabledButton(true);

                statusTexbox.Text = string.Empty;
            }
            catch (Exception e)
            {
                Logger.ColoredConsoleWrite(ConsoleColor.Red, "Error reloading Pokemon list: " + e.Message);
                await Task.Delay(500); // Lets the API make a little pause, so we dont get blocked
                Execute();
            }
        }

        private void EnabledButton(bool enabled, string reason="")
        {
            statusTexbox.Text = reason;
            btnreload.Enabled = enabled;
            btnEvolve.Enabled = enabled;
            btnTransfer.Enabled = enabled;
            btnUpgrade.Enabled = enabled;
            btnFullPowerUp.Enabled = enabled;
            btnShowMap.Enabled = enabled;
            checkBoxreload.Enabled = enabled;
            reloadsecondstextbox.Enabled = enabled;
            PokemonListView.Enabled = enabled;
        }

        private static Bitmap GetPokemonImage(int pokemonId)
        {
            var Sprites = AppDomain.CurrentDomain.BaseDirectory + "Sprites\\";
            string location = Sprites + pokemonId + ".png";
            if (!Directory.Exists(Sprites))
                Directory.CreateDirectory(Sprites);
            bool err = false;
            Bitmap bitmapRemote = null;
            if (!File.Exists(location))
            {
                try {
                    ExtendedWebClient wc = new ExtendedWebClient();
                    wc.DownloadFile("http://data.pokecrot.com/pokemon/" + pokemonId + ".png", @location);
                } catch (Exception)
                {
                    // User fail picture
                    err = true;
                }
            }
            if (err)
            {
                PictureBox picbox = new PictureBox();
                picbox.Image = PokemonGo.RocketAPI.Console.Properties.Resources.error_sprite;
                bitmapRemote = (Bitmap)picbox.Image;
            }
            else
            {
                try
                {
                    PictureBox picbox = new PictureBox();
                    FileStream m = new FileStream(location, FileMode.Open);
                    picbox.Image = Image.FromStream(m);
                    bitmapRemote = (Bitmap)picbox.Image;
                    m.Close();
                } catch (Exception e)
                { 
                    PictureBox picbox = new PictureBox();
                    picbox.Image = PokemonGo.RocketAPI.Console.Properties.Resources.error_sprite;
                    bitmapRemote = (Bitmap)picbox.Image;
                }
            }
            return bitmapRemote;
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            PokemonListView.Clear();
            Execute();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (PokemonListView.FocusedItem.Bounds.Contains(e.Location) == true)
                {
                    if (PokemonListView.SelectedItems.Count > 1)
                    {
                        MessageBox.Show("You can only select 1 item for quick action!", "Selection to large", MessageBoxButtons.OK);
                        return;
                    }
                    contextMenuStrip1.Show(Cursor.Position);
                }
            }
        }

        private async void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var pokemon = (PokemonData)PokemonListView.SelectedItems[0].Tag;
            taskResponse resp = new taskResponse(false, string.Empty);

            if (MessageBox.Show(this, pokemon.PokemonId + " with " + pokemon.Cp + " CP thats " + Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon)) + "% perfect", "Are you sure you want to transfer?", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                resp = await transferPokemon(pokemon);
            }
            else
            {
                return;
            }
            if (resp.Status)
            {
                PokemonListView.Items.Remove(PokemonListView.SelectedItems[0]);
                Text = "Pokemon List | User: " + profile.PlayerData.Username + " | Pokemons: " + PokemonListView.Items.Count + "/" + profile.PlayerData.MaxPokemonStorage;
            }
            else
                MessageBox.Show(resp.Message + " transfer failed!", "Transfer Status", MessageBoxButtons.OK);
        }

        private ColumnHeader SortingColumn = null;

        private void PokemonListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ColumnHeader new_sorting_column = PokemonListView.Columns[e.Column];
            System.Windows.Forms.SortOrder sort_order;
            if (SortingColumn == null)
            {
                sort_order = SortOrder.Ascending;
            }
            else
            {
                if (new_sorting_column == SortingColumn)
                {
                    if (SortingColumn.Text.StartsWith("> "))
                    {
                        sort_order = SortOrder.Descending;
                    }
                    else
                    {
                        sort_order = SortOrder.Ascending;
                    }
                }
                else
                {
                    sort_order = SortOrder.Ascending;
                }
                SortingColumn.Text = SortingColumn.Text.Substring(2);
            }

            // Display the new sort order.
            SortingColumn = new_sorting_column;
            if (sort_order == SortOrder.Ascending)
            {
                SortingColumn.Text = "> " + SortingColumn.Text;
            }
            else
            {
                SortingColumn.Text = "< " + SortingColumn.Text;
            }

            // Create a comparer.
            PokemonListView.ListViewItemSorter = new ListViewComparer(e.Column, sort_order);

            // Sort.
            PokemonListView.Sort();
        }

        private async void btnEvolve_Click(object sender, EventArgs e)
        {
            EnabledButton(false, "Evolving...");
            var selectedItems = PokemonListView.SelectedItems;
            int evolved = 0;
            int total = selectedItems.Count;
            string failed = string.Empty;
            taskResponse resp = new taskResponse(false, string.Empty);

            foreach (ListViewItem selectedItem in selectedItems)
            {
                resp = await evolvePokemon((PokemonData)selectedItem.Tag);
                if (resp.Status)
                {
                    evolved++;
                    statusTexbox.Text = "Evolving..." + evolved;
                }
                else
                    failed += resp.Message + " ";
            }

            if (failed != string.Empty)
                MessageBox.Show("Succesfully evolved " + evolved + "/" + total + " Pokemons. Failed: " + failed, "Evolve status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Succesfully evolved " + evolved + "/" + total + " Pokemons.", "Evolve status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (evolved > 0)
            {
                PokemonListView.Clear();
                Execute();
            } else
            EnabledButton(true);
        }

        private async void btnTransfer_Click(object sender, EventArgs e)
        {
            EnabledButton(false, "Transfering...");
            var selectedItems = PokemonListView.SelectedItems;
            int transfered = 0;
            int total = selectedItems.Count;
            string failed = string.Empty;
            taskResponse resp = new taskResponse(false, string.Empty);

            foreach (ListViewItem selectedItem in selectedItems)
            {
                resp = await transferPokemon((PokemonData)selectedItem.Tag);
                if (resp.Status)
                {
                    PokemonListView.Items.Remove(selectedItem);
                    transfered++;
                    statusTexbox.Text = "Transfering..." + transfered;
                }
                else
                    failed += resp.Message + " ";

            }

            if (failed != string.Empty)
                MessageBox.Show("Succesfully transfered " + transfered + "/" + total + " Pokemons. Failed: " + failed, "Transfer status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Succesfully transfered " + transfered + "/" + total + " Pokemons.", "Transfer status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Text = "Pokemon List | User: " + profile.PlayerData.Username + " | Pokemons: " + PokemonListView.Items.Count + "/" + profile.PlayerData.MaxPokemonStorage;
            EnabledButton(true);
        }

        private async void btnUpgrade_Click(object sender, EventArgs e)
        {
            EnabledButton(false);
            var selectedItems = PokemonListView.SelectedItems;
            int powerdup = 0;
            int total = selectedItems.Count;
            string failed = string.Empty;
            taskResponse resp = new taskResponse(false, string.Empty);

            foreach (ListViewItem selectedItem in selectedItems)
            {
                resp = await PowerUp((PokemonData)selectedItem.Tag);
                if (resp.Status)
                    powerdup++;
                else
                    failed += resp.Message + " ";
            }
            if (failed != string.Empty)
                MessageBox.Show("Succesfully powered up " + powerdup + "/" + total + " Pokemons. Failed: " + failed, "Transfer status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Succesfully powered up " + powerdup + "/" + total + " Pokemons.", "Transfer status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (powerdup > 0)
            {
                PokemonListView.Clear();
                Execute();
            } else
                EnabledButton(true);
        }

        private static async Task<taskResponse> evolvePokemon(PokemonData pokemon)
        {
            taskResponse resp = new taskResponse(false, string.Empty);
            try
            {
                var evolvePokemonResponse = await client.Inventory.EvolvePokemon((ulong)pokemon.Id);

                if (evolvePokemonResponse.Result == EvolvePokemonResponse.Types.Result.Success)
                {
                    resp.Status = true;
                }
                else
                {
                    resp.Message = pokemon.PokemonId.ToString();
                }

                await RandomHelper.RandomDelay(1000, 2000);
            }
            catch (Exception e)
            {
                Logger.ColoredConsoleWrite(ConsoleColor.Red, "Error evolvePokemon: " + e.Message);
                await evolvePokemon(pokemon); }
            return resp;
        }

        private static async Task<taskResponse> transferPokemon(PokemonData pokemon)
        {
            taskResponse resp = new taskResponse(false, string.Empty);
            try
            {
                var transferPokemonResponse = await client.Inventory.TransferPokemon(pokemon.Id);

                if (transferPokemonResponse.Result == ReleasePokemonResponse.Types.Result.Success)
                {
                    resp.Status = true;
                }
                else
                {
                    resp.Message = pokemon.PokemonId.ToString();
                }
            }
            catch (Exception e)
            {
                Logger.ColoredConsoleWrite(ConsoleColor.Red, "Error transferPokemon: " + e.Message);
                await transferPokemon(pokemon);
            }
            return resp;
        }

        private static async Task<taskResponse> PowerUp(PokemonData pokemon)
        {
            taskResponse resp = new taskResponse(false, string.Empty);
            try
            {
                var evolvePokemonResponse = await client.Inventory.UpgradePokemon(pokemon.Id);

                if (evolvePokemonResponse.Result == UpgradePokemonResponse.Types.Result.Success)
                {
                    resp.Status = true;
                }
                else
                {
                    resp.Message = pokemon.PokemonId.ToString();
                }

                await RandomHelper.RandomDelay(1000, 2000);
            }
            catch (Exception e)
            {
                Logger.ColoredConsoleWrite(ConsoleColor.Red, "Error transferPokemon: " + e.Message);
                await PowerUp(pokemon);
            }
            return resp;
        }


        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (PokemonListView.SelectedItems[0].Checked)
                contextMenuStrip1.Items[2].Visible = true;
        }

        private void contextMenuStrip1_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            contextMenuStrip1.Items[2].Visible = false;
        }

        private async void evolveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var pokemon = (PokemonData)PokemonListView.SelectedItems[0].Tag;
            taskResponse resp = new taskResponse(false, string.Empty);

            if (MessageBox.Show(this, pokemon.PokemonId + " with " + pokemon.Cp + " CP thats " + Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon)) + "% perfect", "Are you sure you want to evolve?", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                resp = await evolvePokemon(pokemon);
            }
            else
            {
                return;
            }
            if (resp.Status)
            {
                PokemonListView.Clear();
                Execute();
            }
            else
                MessageBox.Show(resp.Message + " evolving failed!", "Evolve Status", MessageBoxButtons.OK);
        }

        private async void powerUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var pokemon = (PokemonData)PokemonListView.SelectedItems[0].Tag;
            taskResponse resp = new taskResponse(false, string.Empty);

            if (MessageBox.Show(this, pokemon.PokemonId + " with " + pokemon.Cp + " CP thats " + Math.Round(PokemonInfo.CalculatePokemonPerfection(pokemon)) + "% perfect", "Are you sure you want to power it up?", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                resp = await PowerUp(pokemon);
            }
            else
            {
                return;
            }
            if (resp.Status)
            {
                PokemonListView.Clear();
                Execute();
            }
            else
                MessageBox.Show(resp.Message + " powering up failed!", "PowerUp Status", MessageBoxButtons.OK);
        }

        private void checkboxReload_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxreload.Checked)
            {
                int def = 0;
                int interval;
                if (int.TryParse(reloadsecondstextbox.Text, out interval))
                {
                    def = interval;
                }
                if (def < 30 || def > 3600)
                {
                    MessageBox.Show("Interval has to be between 30 and 3600 seconds!");
                    reloadsecondstextbox.Text = "60";
                    checkBoxreload.Checked = false;
                }
                else
                {
                    reloadtimer.Interval = def * 1000;
                    reloadtimer.Start();
                }
            }
            else
            {
                reloadtimer.Stop();
            }

        }

        private void reloadtimer_Tick(object sender, EventArgs e)
        {
            PokemonListView.Clear();
            Execute();
        }

        private void reloadsecondstextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private async void btnFullPowerUp_Click(object sender, EventArgs e)
        {
            EnabledButton(false, "Powering up...");
            DialogResult result = MessageBox.Show("This process may take some time.", "FullPowerUp status", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result == DialogResult.OK)
            {
                var selectedItems = PokemonListView.SelectedItems;
                int powerdup = 0;
                int total = selectedItems.Count;
                string failed = string.Empty;

                taskResponse resp = new taskResponse(false, string.Empty);
                int i = 0;
                int powerUps = 0;
                while (i == 0)
                {
                    foreach (ListViewItem selectedItem in selectedItems)
                    {
                        resp = await PowerUp((PokemonData)selectedItem.Tag);
                        if (resp.Status)
                        {
                            powerdup++;
                        }
                        else
                            failed += resp.Message + " ";
                    }
                    if (failed != string.Empty)
                    {
                        if (powerUps > 0)
                        {
                            MessageBox.Show("Pokemon succesfully powered " + powerUps + " times up.", "FullPowerUp status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Pokemon not powered up. Not enough Stardust or Candy.", "FullPowerUp status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        i = 1;
                        EnabledButton(true);
                    }
                    else
                    {
                        powerUps++;
                        statusTexbox.Text = "Powering up..." + powerUps;
                    }
                }
                if (powerdup > 0 && i == 1)
                {
                    PokemonListView.Clear();
                    Execute();
                }
            }
            else
            {
                EnabledButton(true);
            }
        }

        private void btnShowMap_Click(object sender, EventArgs e)
        {
            new LocationSelect(true).Show();
        }

        private void lang_en_btn2_Click(object sender, EventArgs e)
        {
            lang_de_btn_2.Enabled = true;
            lang_spain_btn2.Enabled = true;
            lang_en_btn2.Enabled = false;
            lang_ptBR_btn2.Enabled = true;
            lang_tr_btn2.Enabled = true;
            languagestr2 = null;

            // Pokemon List GUI
            btnreload.Text = "Reload";
            btnEvolve.Text = "Evolve";
            checkBoxreload.Text = "Reload every";
            btnUpgrade.Text = "PowerUp";
            btnFullPowerUp.Text = "FULL-PowerUp";
            btnForceUnban.Text = "Force Unban";
            btnTransfer.Text = "Transfer";
        }

        private void lang_de_btn_2_Click(object sender, EventArgs e)
        {
            lang_en_btn2.Enabled = true;
            lang_spain_btn2.Enabled = true;
            lang_de_btn_2.Enabled = false;
            lang_ptBR_btn2.Enabled = true;
            lang_tr_btn2.Enabled = true;
            languagestr2 = "de";

            // Pokemon List GUI
            btnreload.Text = "Aktualisieren";
            btnEvolve.Text = "Entwickeln";
            checkBoxreload.Text = "Aktualisiere alle";
            btnUpgrade.Text = "PowerUp";
            btnFullPowerUp.Text = "FULL-PowerUp";
            btnForceUnban.Text = "Force Unban";
            btnTransfer.Text = "Versenden";
        }

        private void lang_spain_btn2_Click(object sender, EventArgs e)
        {
            lang_en_btn2.Enabled = true;
            lang_de_btn_2.Enabled = true;
            lang_spain_btn2.Enabled = false;
            lang_ptBR_btn2.Enabled = true;
            lang_tr_btn2.Enabled = true;
            languagestr2 = "spain";

            // Pokemon List GUI
            btnreload.Text = "Actualizar";
            btnEvolve.Text = "Evolucionar";
            checkBoxreload.Text = "Actualizar cada";
            btnUpgrade.Text = "Dar más poder";
            btnFullPowerUp.Text = "Dar más poder [TOTAL]";
            btnForceUnban.Text = "Force Unban";
            btnTransfer.Text = "Transferir";
        }

        private void lang_ptBR_btn2_Click(object sender, EventArgs e)
        {
            lang_en_btn2.Enabled = true;
            lang_de_btn_2.Enabled = true;
            lang_spain_btn2.Enabled = true;
            lang_ptBR_btn2.Enabled = false;
            lang_tr_btn2.Enabled = true;
            languagestr2 = "ptBR";

            // Pokemon List GUI
            btnreload.Text = "Recarregar";
            btnEvolve.Text = "Evoluir (selecionados)";
            checkBoxreload.Text = "Recarregar a cada";
            btnUpgrade.Text = "PowerUp (selecionados)";
            btnFullPowerUp.Text = "FULL-PowerUp (selecionados)";
            btnForceUnban.Text = "Force Unban";
            btnTransfer.Text = "Transferir (selecionados)";

        }

        private void lang_tr_btn2_Click(object sender, EventArgs e)
        {
            lang_de_btn_2.Enabled = true;
            lang_spain_btn2.Enabled = true;
            lang_en_btn2.Enabled = true;
            lang_ptBR_btn2.Enabled = true;
            lang_tr_btn2.Enabled = false;
            languagestr2 = "tr";

            // Pokemon List GUI
            btnreload.Text = "Yenile";
            btnEvolve.Text = "Geliştir";
            checkBoxreload.Text = "Yenile her";
            btnUpgrade.Text = "Güçlendir";
            btnFullPowerUp.Text = "TAM-Güçlendir";
            btnForceUnban.Text = "Banı Kaldırmaya Zorla";
            btnTransfer.Text = "Transfer";
        }

        private void btnForceUnban_Click(object sender, EventArgs e)    
        {
            Logic.Logic.failed_softban = 6;
            btnForceUnban.Enabled = false;

            Logger.ColoredConsoleWrite(ConsoleColor.Magenta, "Unbanning you at next Pokestop.");
            freezedenshit.Start();
        }

        private void freezedenshit_Tick(object sender, EventArgs e)
        {
            btnForceUnban.Enabled = true;
            freezedenshit.Stop();
        }
    }
    public static class ControlExtensions
    {
        public static void DoubleBuffered(this Control control, bool enable)
        {
            var doubleBufferPropertyInfo = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            doubleBufferPropertyInfo.SetValue(control, enable, null);
        }
    }
    // Compares two ListView items based on a selected column.
    public class ListViewComparer : System.Collections.IComparer
    {
        private int ColumnNumber;
        private SortOrder SortOrder;

        public ListViewComparer(int column_number, SortOrder sort_order)
        {
            ColumnNumber = column_number;
            SortOrder = sort_order;
        }

        // Compare two ListViewItems.
        public int Compare(object object_x, object object_y)
        {
            // Get the objects as ListViewItems.
            ListViewItem item_x = object_x as ListViewItem;
            ListViewItem item_y = object_y as ListViewItem;

            // Get the corresponding sub-item values.
            string string_x;
            if (item_x.SubItems.Count <= ColumnNumber)
            {
                string_x = "";
            }
            else
            {
                string_x = item_x.SubItems[ColumnNumber].Text;
            }

            string string_y;
            if (item_y.SubItems.Count <= ColumnNumber)
            {
                string_y = "";
            }
            else
            {
                string_y = item_y.SubItems[ColumnNumber].Text;
            }

            if (ColumnNumber == 2) //IV
            {
                string_x = string_x.Substring(0, string_x.IndexOf("%"));
                string_y = string_y.Substring(0, string_y.IndexOf("%"));

            }
            else if(ColumnNumber == 7) //HP
            {
                string_x = string_x.Substring(0, string_x.IndexOf("/"));
                string_y = string_y.Substring(0, string_y.IndexOf("/"));
            }

            // Compare them.
            int result;
            double double_x, double_y;
            if (double.TryParse(string_x, out double_x) &&
                double.TryParse(string_y, out double_y))
            {
                // Treat as a number.
                result = double_x.CompareTo(double_y);
            }
            else
            {
                DateTime date_x, date_y;
                if (DateTime.TryParse(string_x, out date_x) &&
                    DateTime.TryParse(string_y, out date_y))
                {
                    // Treat as a date.
                    result = date_x.CompareTo(date_y);
                }
                else
                {
                    // Treat as a string.
                    result = string_x.CompareTo(string_y);
                }
            }

            // Return the correct result depending on whether
            // we're sorting ascending or descending.
            if (SortOrder == SortOrder.Ascending)
            {
                return result;
            }
            else
            {
                return -result;
            }
        }
    }
}