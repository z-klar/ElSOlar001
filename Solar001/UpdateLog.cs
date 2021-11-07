using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Solar001
{
    class UpdateLog
    {
        private delegate void UpdateListbox(string msg);
        private ListBox lb;
        public UpdateLog(ListBox _lb)
        {
            lb = _lb;
        }
        // Any thread can call this method which indirectly updates the Listbox // using Control.Invoke.
        // The call is synchronous, so the callin g thread is blocked until the // // Listbox is update.
        // Consider Control.BeginInvoke for the asynchronous (non-blocking)solution.
        public void UpdateLB(string newMsg)
        {
            UpdateListbox ulb = new UpdateListbox(this.OnUpdate);
            lb.Invoke(ulb, new object[] { newMsg });
        }
        // Do not directly call this method.
        // This method is designed to use only as a delegate target that is invoke on the thread that
        // created the Listbox.
        private void OnUpdate(string msg)
        {
            lb.Items.Add(msg);
        }
    }
}
