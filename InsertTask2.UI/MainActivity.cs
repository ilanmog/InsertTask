using Android.App;
using Android.Widget;
using Android.OS;
using InertTask2.Reader;
using System.Linq;
using System.Collections.Generic;
using static Android.Views.View;
using Android.Views;
using System;
using Java.Lang;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Dropbox.Api;
using Dropbox.CoreApi.Android.Session;
using Dropbox.CoreApi.Android;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Widget;
using Android.Runtime;
using System.Text.RegularExpressions;
using Android.Content;
using System.Text;
using SQLite;

namespace InsertTask2.UI {
    [Activity(Label = "InsertTask2.UI", MainLauncher = true)]
    public class MainActivity : Activity {
        private JavaList<IDictionary<string, object>> _adapterList;
        private IGenericTaskReader _reader = null;
        private ICollection<GenericTask> _data = null;
        private ListView _listView = null;
        string AppKey = "g3uhaafva4strsf";
        string AppSecret = "5o95o03qpidyk10";
        Dropbox.CoreApi.Android.DropboxApi _dropboxApi;
        private string _accessToken = null;
        private AndroidAuthSession _session = null;
        private const int TASKS_COUNTER = 1000;
        private bool _populateAfterAuthorize = false;
        private bool _startActivityAfterAuthorize = false;
        private bool _insertAfterAuthorize = false;
        private bool _doNothingAfterAuthorize = false;
        private GenericTask _taskToStartActivity = null;
        private IDBHandlerFactory _dbHandlerFactory = new DBHandlerFactory();
        private IDBHandler _dbHandler = null;
        private const string ACCESS_TOKEN_PROP = "access_token";
        private const int ACTIVITY_RESULT_INSERT_TASK = 1;
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            _dbHandler = _dbHandlerFactory.CreateDBHandler();
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            _listView = FindViewById<ListView>(Resource.Id.listTasksView);
            _listView.ItemClick += ListView_ItemClick;
            var swipeContainer = FindViewById<SwipeRefreshLayout>(Resource.Id.slSwipeContainer);
            swipeContainer.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
            swipeContainer.Refresh += SwipeContainer_Refresh;
            var searchView = FindViewById<SearchView>(Resource.Id.searchView);
            searchView.QueryTextSubmit += SearchView_QueryTextSubmit;
            searchView.Close += SearchView_Close;
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "Tasks";
            _dbHandler.Init();
            _data = _dbHandler.GetTasks();
            var accessTokenPropery = _dbHandler.GetSetting(ACCESS_TOKEN_PROP);
            if (accessTokenPropery != null) {
                _accessToken = accessTokenPropery.Value;
            }
            RenderItems(_listView, _data);
        }

        private void SearchView_Close(object sender, SearchView.CloseEventArgs e) {
            _data = _dbHandler.GetTasks();
            RenderItems(_listView, _data);
        }

        private void SearchView_QueryTextSubmit(object sender, SearchView.QueryTextSubmitEventArgs e) {
            _data = FindTasks(e.Query);
            RenderItems(_listView, _data);
        }

        private void StartDropboxAuthActivity() {
            AppKeyPair appKeys = new AppKeyPair(AppKey, AppSecret);
            _session = new AndroidAuthSession(appKeys);
            _dropboxApi = new DropboxApi(_session);
            _session.StartOAuth2Authentication(this);
        }
        protected override void OnResume() {
            base.OnResume();
            if (_dropboxApi != null && _session != null && (_populateAfterAuthorize || _startActivityAfterAuthorize || _insertAfterAuthorize || _doNothingAfterAuthorize)) {
                // After you allowed to link the app with Dropbox,
                // you need to finish the Authentication process
                var androidAuthSession = _dropboxApi.Session as AndroidAuthSession;
                //_session = androidAuthSession;
                if (!_session.AuthenticationSuccessful())
                    return;

                try {
                    // Call this method to finish the authentication process
                    // Will bind the user's access token to the session.
                    _session.FinishAuthentication();
                    _accessToken = _session.OAuth2AccessToken;
                    _dbHandler.MergeSettings(new InsertTaskSetting() {
                        Name = ACCESS_TOKEN_PROP,
                        Value = _accessToken
                    });
                    if (_populateAfterAuthorize) {
                        _populateAfterAuthorize = false;
                        RepopulateTasks();
                        FindViewById<SwipeRefreshLayout>(Resource.Id.slSwipeContainer).Refreshing = false;
                    }
                    else if (_startActivityAfterAuthorize) {
                        _startActivityAfterAuthorize = false;
                        StartDetailsActivity();
                    }
                    else if (_insertAfterAuthorize) {
                        _insertAfterAuthorize = false;
                        StartInsertTask();
                    }
                    else if (_doNothingAfterAuthorize) {
                        _doNothingAfterAuthorize = false;
                    }
                    // Save the Access Token somewhere

                    } catch (IllegalStateException ex) {
                    Toast.MakeText(this, ex.LocalizedMessage, ToastLength.Short).Show();
                }
            }
        }
        

        public override bool OnCreateOptionsMenu(IMenu menu) {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnMenuItemSelected(int featureId, IMenuItem item) {
            if (item.ItemId == Resource.Id.menu_edit) {
                if (_accessToken == null) {
                    _insertAfterAuthorize = true;
                    StartDropboxAuthActivity();
                } else {
                    StartInsertTask();
                }
            }
            else if (item.ItemId == Resource.Id.menu_redeem) {
                _doNothingAfterAuthorize = true;
                StartDropboxAuthActivity();
            }
            return base.OnMenuItemSelected(featureId, item);

        }

        private void StartInsertTask() {
            Android.Content.Intent intent = new Android.Content.Intent(this.BaseContext, typeof(InsertTaskActivity));
            intent.PutExtra("identifier", GetNewIdentifier());
            intent.PutExtra("access_token", _accessToken);

            StartActivityForResult(intent, ACTIVITY_RESULT_INSERT_TASK);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data) {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == ACTIVITY_RESULT_INSERT_TASK) {
                if (resultCode == Result.Ok) {
                    InsertTaskAfterActivityResult(data.GetStringExtra("identifier"), data.GetStringExtra("title"));
                } 
            }
        }

        private void InsertTaskAfterActivityResult(string identifier, string title) {
            var content = GetTasksContent(_accessToken);
            var index = content.IndexOf("Tasks\r\n");
            content = content.Insert(index + 7, "\t" + identifier + "::" + title + "\r\n");
            var task = Task.Run(async () => {
                using (var dbx = new DropboxClient(_accessToken)) {
                    var mem = new MemoryStream(Encoding.UTF8.GetBytes(content));
                    await dbx.Files.UploadAsync("/Development/Tasks.txt", Dropbox.Api.Files.WriteMode.Overwrite.Instance, false, null, false, mem);
                }
            });
            task.Wait();

            bool succeed = task.Wait(10000);

            if (succeed && task.IsCompleted) {
                var genericTask = new GenericTask() {
                    Category = "B",
                    Identifier = identifier,
                    IsCompleted = false,
                    IsOutside = false,
                    Title = title
                };
                _data.Add(genericTask);
                _dbHandler.InsertGenericTask(genericTask);
                Toast.MakeText(this, "Tasks updated", ToastLength.Short).Show();

                _adapterList.Add(new JavaDictionary<string, object> {
                            { "identifier", genericTask.Identifier },
                            { "title", genericTask.Title }
                        });
                RenderItems(_listView, _data);
            } else {
                Toast.MakeText(this, "Error: Unable to insert task", ToastLength.Short).Show();
            }
        }

        private string GetNewIdentifier() {
            HashSet<int> iterations = new HashSet<int>();

            foreach (var item in _data.Where(d => d.Identifier.Contains(".")).Select(d => int.Parse(d.Identifier.Split('.')[1])).OrderBy(c => c)) {

                iterations.Add(item);
            }
            int i = 1;
            for (; i < TASKS_COUNTER; i++) {
                if (!iterations.Contains(i)) {
                    break;
                }
            }
            return string.Format("B{0:MM}{0:dd}.{1:000}", DateTime.Today, i);

        }
        

        private void SwipeContainer_Refresh(object sender, EventArgs e) {
            if (_accessToken == null) {
                _populateAfterAuthorize = true;
                StartDropboxAuthActivity();
            }
            else {
                RepopulateTasks();
                ((SwipeRefreshLayout)sender).Refreshing = false;
            }
        }

        private void RepopulateTasks() {
            var tasksContent = GetTasksContent(_accessToken);
            var actualFiles = GetActualFiles(_accessToken);
            _reader = new GenericTaskReader(tasksContent, actualFiles);
            _data = _reader.GetTasks();
            _dbHandler.ClearTasks();

            foreach (var item in _data) {
                try {
                    _dbHandler.InsertGenericTask(item);
                }
                catch (SQLiteException e) {
                    Toast.MakeText(this, "Error: " + e.Message + ". item::" + item.Identifier, ToastLength.Long).Show();
                }
            }
            RenderItems(_listView, _data);
        }
        private string[] GetActualFiles(string accessToken) {
            var task = Task.Run(async () => {
                using (var dbx = new DropboxClient(accessToken)) {
                    var content = await dbx.Files.ListFolderAsync(new Dropbox.Api.Files.ListFolderArg("/Development"));
                    return content.Entries.Where(m => m.IsFile && m.IsDeleted == false).Select(m => m.Name).ToArray();
                }
            });
            task.Wait();

            string[] result = null;
            bool succeed = task.Wait(10000);

            if (succeed && task.IsCompleted) {
                result = task.Result;
            }
            else {
                Toast.MakeText(this, "Error: Unable to get task", ToastLength.Short).Show();
            }
            return result;
        }
        public string GetTasksContent(string accessToken) {
            var task = Task.Run(async () => {
                using (var dbx = new DropboxClient(accessToken)) {
                    var content = await dbx.Files.DownloadAsync("/Development/Tasks.txt");
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
                Toast.MakeText(this, "Error: Unable to get tasks", ToastLength.Short).Show();
            }
            return result;
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e) {
            var genericTask = _data.ElementAt(e.Position);
             _taskToStartActivity = genericTask;
            if (_accessToken == null) {
                _startActivityAfterAuthorize = true;
                StartDropboxAuthActivity();
            }
            else
                StartDetailsActivity();
        }

        private void StartDetailsActivity() {
            if (_taskToStartActivity.IsOutside) {
                Android.Content.Intent intent = new Android.Content.Intent(BaseContext, typeof(TaskDetailActivity));
                intent.PutExtra("identifier", _taskToStartActivity.Identifier);
                intent.PutExtra("title", _taskToStartActivity.Title);
                intent.PutExtra("access_token", _accessToken);
                _taskToStartActivity = null;
                StartActivity(intent);
            }
        }

        private ICollection<GenericTask> FindTasks(string searchTerm) {
            var value = searchTerm.ToLower();
            var tasks = _dbHandler.GetTasks();
            return tasks.Where(g =>
            g.Identifier.ToLower().Contains(value) ||
            g.Title.ToLower().Split(' ').Any(x => x.Contains(value))
            ).ToList();
        }


        private void RenderItems(ListView listView, IEnumerable<GenericTask> data) {
            _adapterList = GetAdapterList(data);
            SimpleAdapter adapter = new SimpleAdapter(this, _adapterList, Resource.Layout.GenericTaskItem, new string[] { "identifier", "title" }, new int[] { Resource.Id.taskIdentifier, Resource.Id.title });
            listView.Adapter = adapter;
        }

        private static JavaList<IDictionary<string, object>> GetAdapterList(IEnumerable<GenericTask> data) {
            var list = new JavaList<IDictionary<string, object>>();
            foreach (var item in data) {
                list.Add(new JavaDictionary<string, object> {
                    { "identifier", item.Identifier },
                    { "title", item.Title }
                });
            }

            return list;
        }
    }

}

