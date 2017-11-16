using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Dropbox.Api;

namespace InsertTask2.UI {
    [Activity]
    public class TaskDetailActivity :Activity {
        private string _identifier = null;
        private string _title = null;
        private string _accessToken = null;
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.TaskDetails);
            _identifier = Intent.GetStringExtra("identifier");
            _title = Intent.GetStringExtra("title");
            _accessToken = Intent.GetStringExtra("access_token");
            Title = _title;
            FindViewById<Android.Support.V4.Widget.SwipeRefreshLayout>(Resource.Id.taskDetailsSwiper).Refresh += TaskDetailActivity_Refresh;
            LoadText();
        }
        /*public override bool OnCreateOptionsMenu(IMenu menu) {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }*/
        private void TaskDetailActivity_Refresh(object sender, EventArgs e) {
            LoadText();
        }

        private void LoadText() {
            FindViewById<TextView>(Resource.Id.textContent).SetText(GetTasksContent(_accessToken), TextView.BufferType.Normal);
        }
        private string GetTasksContent(string accessToken) {
            var task = Task.Run(async () => {
                using (var dbx = new DropboxClient(accessToken)) {
                    var content = await dbx.Files.DownloadAsync("/Development/"+_identifier+".txt");
                    return await content.GetContentAsStringAsync();
                }
            });
            task.Wait();

            string result = null;
            bool succeed = task.Wait(10000);

            if (succeed && task.IsCompleted) {
                result = task.Result;
            }
            else {
                Toast.MakeText(this, "Error: Unable to get task details", ToastLength.Short).Show();
            }
            return result;
        }
    }
}