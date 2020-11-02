using System.Collections.Generic;
using System.Windows.Forms;

namespace KEUtils.MultichoiceListDialog {

    public partial class MultiChoiceListDialog : Form {
        private List<string> itemList;
        public List<string> SelectedList { get; set; }
        public SelectionMode SelectionMode {
            get {
                return listBox.SelectionMode;
            }
            set {
                listBox.SelectionMode = value;
            }
        }
        public int SelectedIndex {
            get {
                return listBox.SelectedIndex;
            }
            set {
                listBox.SelectedIndex = value;
            }
        }
        public string Prompt {
            get {
                return labelMsg.Text;
            }
            set {
                labelMsg.Text = value;
            }
        }

        public MultiChoiceListDialog(List<string> newList) {
            this.itemList = new List<string>();
            foreach (string item in newList) {
                if (item != null) this.itemList.Add(item);
            }
            this.itemList = newList;

            InitializeComponent();
            populateList();
        }

        private void populateList() {
            listBox.DataSource = null;
            List<string> items = new List<string>();
            foreach (string name in itemList) {
                items.Add(name);
            }
            listBox.DataSource = items;
        }

        private void onOkClick(object sender, System.EventArgs e) {
            SelectedList = new List<string>();
            foreach (object item in listBox.SelectedItems) {
                SelectedList.Add(item.ToString());
            }
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void onCancelClick(object sender, System.EventArgs e) {
            SelectedList = null;
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
