using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Exists to avoid forking + updating the used library
namespace LiveSplit.Terraria {
    public class ItemSelectorForm : Form {
        private TextBox txtSearch;
        private ListBox lstItems;
        private Button btnAdd;

        public List<string> SelectedItemNames { get; private set; }

        private List<string> allItemNames;

        public ItemSelectorForm() {
            SelectedItemNames = new List<string>();

            InitializeComponent();
            LoadItems();

            this.ShowInTaskbar = false;
            this.TopMost = true;
        }

        private void InitializeComponent() {
            this.Text = "Add Item Split";
            this.Size = new Size(300, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            var lblSearch = new Label { Text = "Search:", Location = new Point(10, 10), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(10, 30), Width = 260 };
            txtSearch.TextChanged += (s, e) => FilterList();

            lstItems = new ListBox {
                Location = new Point(10, 80),
                Width = 260,
                Height = 280,
                IntegralHeight = false,
                SelectionMode = SelectionMode.MultiExtended
            };
            lstItems.DoubleClick += (s, e) => AddSelectionAndClose();

            btnAdd = new Button { Text = "Add", Location = new Point(110, 370) };
            btnAdd.Click += (s, e) => AddSelectionAndClose();

            var btnCancel = new Button { Text = "Cancel", Location = new Point(190, 370), DialogResult = DialogResult.Cancel };

            this.Controls.AddRange(new Control[] { lblSearch, txtSearch, lstItems, btnAdd, btnCancel });
            this.AcceptButton = btnAdd;
            this.CancelButton = btnCancel;
        }

        private void LoadItems() {
            allItemNames = Enum.GetNames(typeof(EItems)).OrderBy(name => name).ToList();
            FilterList();
        }

        private void FilterList() {
            lstItems.BeginUpdate();
            lstItems.Items.Clear();

            string searchText = txtSearch.Text.ToLowerInvariant();

            var filteredItems = allItemNames.Where(name =>
                name.ToLowerInvariant().Contains(searchText)
            );

            lstItems.Items.AddRange(filteredItems.ToArray());
            lstItems.EndUpdate();
        }

        private void AddSelectionAndClose() {
            if(lstItems.SelectedItems.Count > 0) {
                SelectedItemNames = lstItems.SelectedItems.Cast<string>().ToList();
                this.DialogResult = DialogResult.OK;
                this.Close();
            } else {
                MessageBox.Show("Please select one or more items from the list.", "No Item(s) Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}