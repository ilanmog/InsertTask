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
using static Android.Widget.TextView;
using System.IO;

namespace InsertTask2.UI {
    [Activity(Label = "Insert Task")]
    public class InsertTaskActivity : Activity {
        private string _access_token = null;
        private string _identifier = null;
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.InsertTask);
            _access_token = Intent.GetStringExtra("access_token");
            _identifier = Intent.GetStringExtra("identifier");
            FindViewById<TextView>(Resource.Id.txtIdentifier).SetText(_identifier, BufferType.Spannable);
            FindViewById<Button>(Resource.Id.btnInsert).Click += InsertTask_Click;
        }

        private void InsertTask_Click(object sender, EventArgs e) {
            
            var txtTitle = FindViewById<TextView>(Resource.Id.editTitle);
            Android.Content.Intent intent = new Intent();
            intent.PutExtra("title", txtTitle.Text);
            intent.PutExtra("identifier", _identifier);
            SetResult(Result.Ok, intent);
            Finish();

        }
    }
}